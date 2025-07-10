using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Services.ServiceHost;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Newtonsoft.Json;
using WTG.Foundation.Http;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;
using static Enterprise.Core.Constants.CustomerService;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;

namespace Enterprise.Client.EDI.IncidentManager.Service
{
	public class EdiIncidentRequestService : IIncidentRequestService
	{
		const string Contact = "Contact";
		const string Email = "Email";
		const string Organization = "Organization";

		public IEnumerable<Tuple<string, string>> GetProductList(string language, Guid contactPk)
		{
			var isInternal = IsInternalContact(contactPk);
			var products = GetLicencedProducts(contactPk);

			var productList = GetOrCreateProductList(language, isInternal).List;
			var result = isInternal || HasNoLicencedProductOrWTAOnly(products.List) //return a full product list for the service provider which doesn't have any licenses/products or have only WTA licence.
				? productList
				: productList.Where(i => products.List.Any(t => t.Item1.Equals(i.Item1)));

			var additionalProducts = GetAdditionalProductsForContact(contactPk);

			return result.Union(additionalProducts);
		}

		#region Default Lists

		public IEnumerable<Tuple<string, string>> GetFullProductList()
		{
			var productList = GetOrCreateProductList(string.Empty, isInternal: true).List;
			return productList;
		}

		public IEnumerable<Tuple<string, string, string, string>> GetFullModuleListByProduct()
		{
			var productCriticalityModuleCombinations = new List<Tuple<string, string, string, string>>();
			var cw1MenuSectionModuleList = GetModuleList(ProductTypes.Codes.Enterprise, string.Empty, excludeExternal: false, excludeDisabled: true);
			var cw1Cr8ModuleList = GetModuleList(ProductTypes.Codes.Enterprise, CriticalityCodes.CR8_ComplianceRequirement, excludeExternal: false, excludeDisabled: true);
			var cw1Cr9ModuleList = GetModuleList(ProductTypes.Codes.Enterprise, CriticalityCodes.CR9_CustomerServiceRequest, excludeExternal: false, excludeDisabled: true);

			var cw1FullModuleList = new CodeDescriptionPairList();
			cw1FullModuleList.AddRange(cw1MenuSectionModuleList);
			cw1FullModuleList.AddRange(cw1Cr8ModuleList);
			cw1FullModuleList.AddRange(cw1Cr9ModuleList);

			foreach (CodeDescriptionPair module in cw1MenuSectionModuleList)
			{
				productCriticalityModuleCombinations.Add(new Tuple<string, string, string, string>(ProductTypes.Codes.Enterprise, string.Empty, module.Code, module.Description));
			}

			foreach (CodeDescriptionPair module in cw1Cr8ModuleList)
			{
				productCriticalityModuleCombinations.Add(new Tuple<string, string, string, string>(ProductTypes.Codes.Enterprise, CriticalityCodes.CR8_ComplianceRequirement, module.Code, module.Description));
			}

			foreach (CodeDescriptionPair module in cw1Cr9ModuleList)
			{
				productCriticalityModuleCombinations.Add(new Tuple<string, string, string, string>(ProductTypes.Codes.Enterprise, CriticalityCodes.CR9_CustomerServiceRequest, module.Code, module.Description));
			}

			var builder = new SupportIncidentModuleListBuilder();
			var products = new ProductTypes(includeCargoWiseOne: false, includeInternal: true);
			foreach (CodeDescriptionPair product in products)
			{
				if (product.Code != ProductTypes.Codes.Enterprise)
				{
					var moduleList = builder.BuildWithCriticality(Enterprise.CustomerService.Business.ModuleListType.Unspecified, product.Code, string.Empty, false, true);
					foreach (var module in moduleList)
					{
						productCriticalityModuleCombinations.Add(new Tuple<string, string, string, string>(product.Code, module.Item1, module.Item2, module.Item3));
					}
				}
			}

			var distinctProductModules = productCriticalityModuleCombinations
				.GroupBy(t => new { t.Item1, t.Item2, t.Item3 })
				.Select(group => group.First())
				.OrderBy(x => x.Item4);

			return distinctProductModules;
		}

		#endregion

		public IEnumerable<Tuple<string, string>> GetModuleList(string product, string criticality, string language, string status, Guid contactPk)
		{
			var isContactInternal = IsInternalContact(contactPk);

			if (ShouldGetWebSecurityRestrictedModules(isContactInternal, product, language))
			{
				return GetWebSecurityRestrictedModules(contactPk, product, criticality);
			}
			else
			{
				var shouldShowInternalItem = (!string.IsNullOrWhiteSpace(status) && !new[] { LegacyStatusCodes.New, IncidentApprovalLogSubscriber.StatusApprovalRequested }.Any(x => x.Equals(status, StringComparison.OrdinalIgnoreCase)))
											|| isContactInternal;
				var key = new ModuleListKey(shouldShowInternalItem, language, product, criticality);

				return GetOrAddRecent(ModuleListCache, key, CreateModuleList).List.Distinct();
			}
		}

		IEnumerable<Tuple<string, string>> GetWebSecurityRestrictedModules(Guid contactPK, string product, string criticality)
		{
			var additional = (criticality == "CR8" || criticality == "CR9") ? ListWithCreatedTime.Empty : GetAdditionalProductsModulesForContactCache(contactPK);
			var modules = additional.List.Where(i => i.Item1 == product).Select(m =>
			{
				var description = string.Empty;
				foreach (var entry in MappingCollection.Collection)
				{
					if (entry.ProductMapping == product && entry.ModuleMapping == m.Item2)
					{
						description = entry.ModuleMappingDescription;
						break;
					}
				}

				return new Tuple<string, string>(m.Item2, description);
			});

			return modules;
		}

		bool ShouldGetWebSecurityRestrictedModules(bool isContactInternal, string product, string language)
		{
			return !isContactInternal && MappingCollection.Collection.HasProduct(product) && IsProductInternal(product, language);
		}

		public IEnumerable<Tuple<string, string>> GetServiceTypeList(string product, string criticality, string module, string sourceModuleId)
		{
			var result = new CodeDescriptionPairList();

			if (!string.IsNullOrEmpty(product) && !string.IsNullOrEmpty(criticality) && !string.IsNullOrEmpty(module))
			{
				var registryProductCollection = GetOrCreateServiceTypeList(criticality);
				var sytemProducts = registryProductCollection?.List.Cast<ServiceTypeProduct>().FirstOrDefault(p => p.Code.EqualsIgnoringCase(product));

				if (sytemProducts?.ServiceTypeModuleMappings.Count > 0)
				{
					var productArea = IncidentDetailsHelper.FindProductArea(product, criticality, module, sourceModuleId);

					var productAreaModuleMapping = sytemProducts.ServiceTypeModuleMappings.Cast<ServiceTypeProductAreaModuleMapping>().FirstOrDefault(p =>
																											p.ProductArea.EqualsIgnoringCase(productArea) &&
																											p.ModuleCode.EqualsIgnoringCase(module));

					if (productAreaModuleMapping != null)
					{
						foreach (var item in productAreaModuleMapping.ServiceTypeMappings.Cast<ServiceTypeModuleMapping>())
						{
							result.Add(new CodeDescriptionPair((string)item.Code, (string)item.Description));
						}
					}
				}
			}

			return result.Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
		}

		public void NotificationWhenUnsubscribe(Guid jobPK, string userType, ICollection<Guid> userPKs, string email)
		{
			var factory = new BusinessObjectFactory();
			var supportIncident = factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, jobPK));
			if (supportIncident == null)
			{
				return;
			}

			var unsubscribedParticipants = supportIncident.EConversation.ExistingConversation.UnsubscribedParticipants;

			if (userType == Email && !string.IsNullOrEmpty(email))
			{
				var emailParticipant = new JobConversationParticipant.EmailConversationParticipant(email);
				unsubscribedParticipants.Add(emailParticipant);
			}

			if ((userType == Contact || userType == Organization) && userPKs != null && userPKs.Count != 0)
			{
				var tableCode = userType == Contact ? "OC" : "OH";
				foreach (var pk in userPKs)
				{
					var participant = (IConversationParticipant)factory.Load(tableCode, pk);
					unsubscribedParticipants.Add(participant);
				}
			}

			if (unsubscribedParticipants != null && unsubscribedParticipants.Count != 0)
			{
				supportIncident.SendUnsubscribedParticipantsNotifications();
			}
		}

		public Dictionary<string, string> GetDocumentUrls(Guid contactPk, ICollection<string> documentIds)
		{
			if (documentIds.IsNullOrEmpty())
			{
				return new Dictionary<string, string>();
			}

			var factory = new BusinessObjectFactory();
			var contact = factory.Load<OrgContact>(contactPk);

			if (contact == null)
			{
				return new Dictionary<string, string>();
			}

			var (userAccount, billingOrg) = WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(contact.Header, contact);
			if (userAccount == null)
			{
				return new Dictionary<string, string>();
			}

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var accessToken = accessControl.CreateLimitedToken(AccessTokenTypes.WiseTechAcademyAccessTokenType, new AccessTokenInfo("", contact.PK.ToGuid(), OrgContactSchema.Constants.Prefix), TimeSpan.FromMinutes(5), maxUses: 1);

			var tokenRequestInfo = new WTASSORequestInfo
			{
				Token = accessToken,
				TenantId = userAccount.Database.LD_TenantID,
				ContactEmail = contact.OC_Email,
				ContactName = contact.ContactNameWithoutNumberSuffix,
				ContactKey = contact.PK.ToString(),
				OrganisationKey = contact.OC_OH.ToString(),
				OrganisationName = contact.Header.OH_FullName,
				PersonIDs = string.Join(",", contact.Person.PersonIDs.Select(x => x.ToString())),
				IsTesting = true,
			};

			var httpClient = GetHttpClient();

			var jsonContent = JsonConvert.SerializeObject(tokenRequestInfo);

			var tokenResponse = httpClient.PostAsync(EDIDataRegistry.Instance.WiseTechAcademyTokenEndpointUrl.Value.TrimEnd('/'), new StringContent(jsonContent, Encoding.UTF8, "application/json"));

			if (!tokenResponse.Result.IsSuccessStatusCode)
			{
				return new Dictionary<string, string>();
			}

			var jwtTokenResponse = tokenResponse.Result.Content.ReadAsStringAsync().Result;
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtTokenResponse);

			var documentIdsJson = JsonConvert.SerializeObject(documentIds);
			var documentUrlResponse = httpClient.PostAsync(EDIDataRegistry.Instance.WiseTechAcademyDocumentUrlsEndpointUrl.Value.TrimEnd('/'), new StringContent(documentIdsJson, Encoding.UTF8, "application/json"));

			if (!documentUrlResponse.Result.IsSuccessStatusCode)
			{
				return new Dictionary<string, string>();
			}

			var documentUrlsJson = documentUrlResponse.Result.Content.ReadAsStringAsync().Result;
			var documentUrls = JsonConvert.DeserializeObject<Dictionary<string, string>>(documentUrlsJson);

			return documentUrls;
		}

		HttpClient GetHttpClient()
		{
			var httpClientFactory = ObjectFactory.Get<IHttpClientFactory>();
			var httpClient = httpClientFactory
				.CreateNew(
					new HttpClientHandlerWithDiagnostics(new CookieContainer()),
					TimeSpan.FromSeconds(60));

			return httpClient;
		}

		bool IsProductInternal(string product, string language)
		{
			if (ProductTypes.ProductsWithENTLicense.Contains(product) || ProductTypes.IsEnterpriseFamily(product))
			{
				return false;
			}

			ListWithCreatedTime list;

			GetOrCreateProductList(language, true);
			if (InternalProductListCache.TryGetValue(language, out list))
			{
				return list.List.Any(t => t.Item1 == product);
			}

			return false;
		}

		bool HasNoLicencedProductOrWTAOnly(IEnumerable<Tuple<string, string>> productsList)
		{
			var list = productsList.ToList();
			return !productsList.Any() || (list.Count == 1 && list[0].Item1 == ProductTypes.Codes.WiseTechAcademy);
		}

		[Immutable]
		sealed class ModuleListKey : Tuple<bool, string, string, string>
		{
			public ModuleListKey(bool isInternal, string language, string product, string criticality)
				: base(isInternal, language, product, ConvertCriticalityToCategory(criticality))
			{
			}

			static string ConvertCriticalityToCategory(string criticality)
			{
				if (criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement ||
					criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
				{
					return criticality;
				}
				else
				{
					// For CR1..7
					return string.Empty;
				}
			}

			public bool IsInternal => Item1;
			public string Language => Item2;
			public string Product => Item3;
			public string CriticalityCategory => Item4;
		}

		[Immutable]
		sealed class ListWithCreatedTime
		{
			public ListWithCreatedTime(ReadOnlyCollection<Tuple<string, string>> list)
			{
				Created = ZDateTime.UtcNow;
				List = list;
			}

			[SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			const int MaxAgeMinutes = 15;
			public bool IsTooOld => (ZDateTime.UtcNow - Created).TotalMinutes >= MaxAgeMinutes;

			public readonly ReadOnlyCollection<Tuple<string, string>> List;
			public readonly ZDateTime Created;

			public static ListWithCreatedTime Empty
			{
				get
				{
					return new ListWithCreatedTime(Array.AsReadOnly(Enumerable.Empty<string>().ToArray().Select(x => Tuple.Create(string.Empty, string.Empty)).ToArray()));
				}
			}
		}

		[ThreadStatic]
		static ConcurrentDictionary<ModuleListKey, ListWithCreatedTime> moduleListCache;
		static ConcurrentDictionary<ModuleListKey, ListWithCreatedTime> ModuleListCache
		{
			get { return moduleListCache ?? (moduleListCache = new ConcurrentDictionary<ModuleListKey, ListWithCreatedTime>()); }
		}

		[ThreadStatic]
		static ConcurrentDictionary<string, ListWithCreatedTime> internalProductListCache;
		static ConcurrentDictionary<string, ListWithCreatedTime> InternalProductListCache
		{
			get { return internalProductListCache ?? (internalProductListCache = new ConcurrentDictionary<string, ListWithCreatedTime>()); }
		}

		[ThreadStatic]
		static ConcurrentDictionary<string, ListWithCreatedTime> externalProductListCache;
		static ConcurrentDictionary<string, ListWithCreatedTime> ExternalProductListCache
		{
			get { return externalProductListCache ?? (externalProductListCache = new ConcurrentDictionary<string, ListWithCreatedTime>()); }
		}

		[ThreadStatic]
		static ConcurrentDictionary<Guid, bool> isInternalContactCache;
		static ConcurrentDictionary<Guid, bool> IsInternalContactCache
		{
			get { return isInternalContactCache ?? (isInternalContactCache = new ConcurrentDictionary<Guid, bool>()); }
		}

		[ThreadStatic]
		static ConcurrentDictionary<Guid, ListWithCreatedTime> licencedProductsContactCache;
		static ConcurrentDictionary<Guid, ListWithCreatedTime> LicencedProductsContactCache
		{
			get { return licencedProductsContactCache ?? (licencedProductsContactCache = new ConcurrentDictionary<Guid, ListWithCreatedTime>()); }
		}

		[ThreadStatic]
		static ConcurrentDictionary<Guid, ListWithCreatedTime> additionalProductsModulesForContactCache;
		static ConcurrentDictionary<Guid, ListWithCreatedTime> AdditionalProductsModulesForContactCache
		{
			get { return additionalProductsModulesForContactCache ?? (additionalProductsModulesForContactCache = new ConcurrentDictionary<Guid, ListWithCreatedTime>()); }
		}

		ListWithCreatedTime CreateModuleList(ModuleListKey key)
		{
			using (Res.TemporarilySwitchLanguage(key.Language))
			{
				var codes = key.IsInternal
					? GetModuleList(key.Product, key.CriticalityCategory, false, true)
					: GetExternalModuleList(key.Product, key.CriticalityCategory);

				return new ListWithCreatedTime(Array.AsReadOnly(codes.Cast<ICodeDescription>()
					.Select(x => Tuple.Create(x.Code, x.Description))
					.ToArray())); // evaluate the enumerable and hence the multilingual description before switching back the language
			}
		}

		CodeDescriptionPairList GetExternalModuleList(string product, string criticality)
		{
			CodeDescriptionPairList list;

			if (!string.IsNullOrEmpty(product))
			{
				list = GetModuleList(product, criticality, true, true);
			}
			else
			{
				list = GetExternalFullModuleList();
			}

			return list;
		}

		static CodeDescriptionPairList GetModuleList(string product, string criticality, bool excludeExternal = false, bool excludeDisabled = true)
		{
			CustomerService.Business.ModuleListType moduleListType;
			if (criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement)
			{
				moduleListType = Enterprise.CustomerService.Business.ModuleListType.Cr8;
			}
			else if (criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
			{
				moduleListType = Enterprise.CustomerService.Business.ModuleListType.Cr9;
			}
			else if (!string.IsNullOrEmpty(product))
			{
				moduleListType = Enterprise.CustomerService.Business.ModuleListType.MenuSection;
			}
			else
			{
				moduleListType = Enterprise.CustomerService.Business.ModuleListType.Unspecified;
			}

			var builder = new SupportIncidentModuleListBuilder();
			return builder.Build(moduleListType, product, "", excludeExternal, excludeDisabled);
		}

		static CodeDescriptionPairList GetExternalFullModuleList()
		{
			var result = new CodeDescriptionPairList();

			var cw1MenuSectionModuleList = GetModuleList(ProductTypes.Codes.Enterprise, string.Empty, true, true);
			var cw1Cr8ModuleList = GetModuleList(ProductTypes.Codes.Enterprise, Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, true, true);
			var cw1Cr9ModuleList = GetModuleList(ProductTypes.Codes.Enterprise, Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, true, true);

			var cw1FullModuleList = new CodeDescriptionPairList();
			cw1FullModuleList.AddRange(cw1MenuSectionModuleList);
			cw1FullModuleList.AddRange(cw1Cr8ModuleList);
			cw1FullModuleList.AddRange(cw1Cr9ModuleList);

			foreach (CodeDescriptionPair module in cw1FullModuleList)
			{
				result.AddPair(module.Code, FormattableString.Invariant($"[{ProductTypes.Descriptions.EnterpriseCW1}] {module.Description}"));
			}

			var builder = new SupportIncidentModuleListBuilder();
			var products = new ProductTypes(includeCargoWiseOne: false, includeInternal: false);
			foreach (CodeDescriptionPair product in products)
			{
				if (product.Code != ProductTypes.Codes.Enterprise)
				{
					var moduleList = builder.Build(Enterprise.CustomerService.Business.ModuleListType.Unspecified, product.Code, string.Empty, true, true);
					foreach (CodeDescriptionPair module in moduleList)
					{
						result.AddPair(module.Code, FormattableString.Invariant($"[{product.Description}] {module.Description}"));
					}
				}
			}

			result.SortByDescription();

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Doesnt matter if we need a new value in the cache.")]
		static ListWithCreatedTime GetOrAddRecent<T>(ConcurrentDictionary<T, ListWithCreatedTime> cache, T key, Func<T, ListWithCreatedTime> valueFactory)
		{
			var result = cache.GetOrAdd(key, valueFactory);
			if (result.IsTooOld)
			{
				cache.TryRemove(key, out var x);
				result = cache.GetOrAdd(key, valueFactory);
			}
			return result;
		}

		ListWithCreatedTime GetOrCreateProductList(string language, bool isInternal)
		{
			return isInternal
				? GetOrAddRecent(InternalProductListCache, language, CreateInternalProductList)
				: GetOrAddRecent(ExternalProductListCache, language, CreateExternalProductList);
		}

		ListWithCreatedTime CreateExternalProductList(string language)
		{
			return CreateProductList(language, false);
		}

		ListWithCreatedTime CreateInternalProductList(string language)
		{
			return CreateProductList(language, true);
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "language", Justification = "Ignoring language until product list is translatable")]
		ListWithCreatedTime CreateProductList(string language, bool isInternal)
		{
			return new ListWithCreatedTime(Array.AsReadOnly(new ProductTypes(includeCargoWiseOne: false, includeInternal: isInternal)
				.Cast<ICodeDescription>()
				.Select(x => Tuple.Create(x.Code, x.Description))
				.ToArray()));
		}

		bool IsInternalContact(Guid contactPk)
		{
			return IsInternalContactCache.GetOrAdd(contactPk, IsInternalContactCore);
		}

		ListWithCreatedTime GetLicencedProducts(Guid contactPk)
		{
			return GetOrAddRecent(LicencedProductsContactCache, contactPk, GetListOfOrgLicencedProducts);
		}

		IEnumerable<Tuple<string, string>> GetAdditionalProductsForContact(Guid contactPk)
		{
			var productsModules = GetAdditionalProductsModulesForContactCache(contactPk);
			return productsModules.List.Select
				(
					i =>
					{
						string description = string.Empty;
						foreach (var entry in MappingCollection.Collection)
						{
							if (entry.ProductMapping == i.Item1)
							{
								description = entry.ProductMappingDescription;
								break;
							}
						}
						return Tuple.Create(i.Item1, description);
					}
				).Distinct();
		}

		ListWithCreatedTime GetAdditionalProductsModulesForContactCache(Guid contactPk)
		{
			return GetOrAddRecent(AdditionalProductsModulesForContactCache, contactPk, GetAdditionalProductsAndModulesForContact);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		bool IsInternalContactCore(Guid contactPk)
		{
			const string sql = "select LE_IsInternal from dbo.LicenceEnterprise join dbo.LicenceCompany on LC_LE = LE_PK join dbo.OrgContact on OC_OH = LC_OH where OC_PK = @contactPk";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@contactPk", contactPk, OrgContactSchema.PK);
				var result = cmd.ExecuteScalar();
				return (result is bool) && (bool)result;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ListWithCreatedTime GetListOfOrgLicencedProducts(Guid contactPk)
		{
			const string sql = @"
select distinct LD_Product from dbo.LicenceEnterprise 
join dbo.LicenceCompany on LC_LE = LE_PK
join dbo.LicenceHeader on LA_LC = LC_PK
join dbo.LicenceDatabase on LA_LD = LD_PK and LD_IsActive = 1
join dbo.OrgContact on OC_OH = LC_OH where OC_PK = @contactPk
union
select LD_Product from dbo.LicenceDatabase where LD_OH_WebAccessOrg in
(
	select OC_OH from dbo.OrgContact where OC_PK = @contactPk
)";

			var products = new List<string>();
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@contactPk", contactPk, OrgContactSchema.PK);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var newProduct = reader.GetString(0);
						if (!products.Contains(newProduct))
						{
							products.Add(newProduct);
						}
					}
				}

				if (products.Contains(ProductTypes.Codes.CargoWiseOne) || products.Contains(ProductTypes.Codes.CargoWiseNext))
				{
					products.Add(ProductTypes.Codes.Enterprise);
				}

				if (products.Contains(ProductTypes.Codes.Enterprise))
				{
					foreach (var product in ProductTypes.ProductsWithENTLicense)
					{
						if (!products.Contains(product))
						{
							products.Add(product);
						}
					}
				}

				return new ListWithCreatedTime(Array.AsReadOnly(products.ToArray()
				.Select(x => Tuple.Create(x, string.Empty))
				.ToArray()));
			}
		}

		[ThreadStatic]
		static WebSecurityMappingCollectionWithTime mappingCollection;

		static WebSecurityMappingCollectionWithTime MappingCollection
		{
			get
			{
				if (mappingCollection == null || mappingCollection.IsTooOld)
				{
					mappingCollection = new WebSecurityMappingCollectionWithTime(EDIDataRegistry.Instance.WebSecurityProductModuleMappings.Value);
				}

				return mappingCollection;
			}
		}

		internal class WebSecurityMappingCollectionWithTime
		{
			internal WebSecurityMappingCollectionWithTime(WebSecurityMappingCollection collection)
			{
				Created = ZDateTime.UtcNow;
				this.collection = collection;
			}

			readonly ZDateTime Created;

			public bool IsTooOld
			{
				get
				{
					var span = ZDateTime.UtcNow - Created;
					return span.TotalMinutes >= 15 || span.TotalMinutes < 0;
				}
			}

			readonly WebSecurityMappingCollection collection;
			public WebSecurityMappingCollection Collection => collection;
		}

		ListWithCreatedTime GetAdditionalProductsAndModulesForContact(Guid contactPk)
		{
			if (IsInternalContact(contactPk))
			{
				return new ListWithCreatedTime(Array.AsReadOnly(Enumerable.Empty<string>().ToArray().Select(x => Tuple.Create(string.Empty, string.Empty)).ToArray()));
			}

			if (MappingCollection.Collection == null || MappingCollection.Collection.Count == 0)
			{
				return ListWithCreatedTime.Empty;
			}

			var list = new List<Tuple<string, string>>();
			var securities = EDIWebSecurityRightsList.New();

			OrgContact contact = new BusinessObjectFactory().Load<OrgContact>(contactPk);

			if (contact == null)
			{
				return ListWithCreatedTime.Empty;
			}

			return new ListWithCreatedTime(Array.AsReadOnly(MappingCollection.Collection.ToArray()
				.Cast<WebSecurityMapping>()
				.Where(i =>
				{
					var securityRight = securities.FirstOrDefault(right => right.Code == i.WebSecurity);
					if (securityRight == null)
					{
						return false;
					}

					return OrgContactWebUser.IsRightGrantedWithoutCache(securityRight, contact);
				})
				.Select(x => Tuple.Create((string)x.ProductMapping, (string)x.ModuleMapping))
				.ToArray()));
		}

		#region Service Type Cache

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "ConcurrentDictionary is thread safe")]
		static ConcurrentDictionary<string, ServiceTypeListWithCreatedTime> serviceTypeListCache;
		static ConcurrentDictionary<string, ServiceTypeListWithCreatedTime> ServiceTypeListCache
		{
			get { return serviceTypeListCache ?? (serviceTypeListCache = new ConcurrentDictionary<string, ServiceTypeListWithCreatedTime>()); }
		}

		ServiceTypeListWithCreatedTime GetOrCreateServiceTypeList(string criticality)
		{
			if (string.IsNullOrEmpty(criticality))
			{
				return null;
			}

			return ServiceTypeListCache.AddOrUpdate(criticality, CreateServiceTypeList, UpdateServiceTypeList);
		}

		ServiceTypeListWithCreatedTime CreateServiceTypeList(string criticality)
		{
			ServiceTypeListWithCreatedTime registryProductCollection;
			if (criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement)
			{
				registryProductCollection = CreateServiceTypeProductList(EDIDataRegistry.Instance.ServiceTypeCr8Mappings.Value);
			}
			else if (criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
			{
				registryProductCollection = CreateServiceTypeProductList(EDIDataRegistry.Instance.ServiceTypeCr9Mappings.Value);
			}
			else
			{
				registryProductCollection = CreateServiceTypeProductList(EDIDataRegistry.Instance.ServiceTypeMappings.Value);
			}
			return registryProductCollection;
		}

		ServiceTypeListWithCreatedTime UpdateServiceTypeList(string criticality, ServiceTypeListWithCreatedTime existingValue)
		{
			if (existingValue == null || existingValue.IsTooOld)
			{
				return CreateServiceTypeList(criticality);
			}
			else
			{
				return existingValue;
			}
		}

		ServiceTypeListWithCreatedTime CreateServiceTypeProductList(SystemProductCollection registryData)
		{
			return new ServiceTypeListWithCreatedTime(Array.AsReadOnly(registryData
				.Cast<SystemProduct>()
				.Select(x => new ServiceTypeProduct(x.Code, x.ServiceTypeModuleMappings))
				.ToArray()));
		}

		sealed class ServiceTypeProduct
		{
			public ServiceTypeProduct(ZString code, ServiceTypeProductAreaModuleMappingCollection serviceTypeModuleMappings)
			{
				Code = code;
				ServiceTypeModuleMappings = serviceTypeModuleMappings;
			}

			public readonly ZString Code;
			public readonly ServiceTypeProductAreaModuleMappingCollection ServiceTypeModuleMappings;
		}

		sealed class ServiceTypeListWithCreatedTime
		{
			public ServiceTypeListWithCreatedTime(ReadOnlyCollection<ServiceTypeProduct> list)
			{
				Created = ZDateTime.UtcNow;
				List = list;
			}

			[SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			const int MaxAgeMinutes = 15;
			public bool IsTooOld => (ZDateTime.UtcNow - Created).TotalMinutes >= MaxAgeMinutes;

			public readonly ReadOnlyCollection<ServiceTypeProduct> List;
			public readonly ZDateTime Created;
		}

		#endregion

		internal static void ClearCacheForTest()
		{
			IsInternalContactCache.Clear();
			ModuleListCache.Clear();
			InternalProductListCache.Clear();
			ExternalProductListCache.Clear();
			LicencedProductsContactCache.Clear();
			AdditionalProductsModulesForContactCache.Clear();
			ServiceTypeListCache.Clear();
			mappingCollection = null;
		}
	}
}
