using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[CodeProperty(LicenceCompanySchema.Constants.LC_CompanyCode), DescriptionProperty("CodeDescription")]
	public class LicenceCompany : AutoLicenceCompany
	{
		public LicenceCompany(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			LC_IsGSTCashBasis = false;
			LC_IsWHTCashBasis = true;   // All WHT is cash basis only
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			InvoiceDeliveries.DeleteAll();
			PriceHeaders.DeleteAll();
			var selfBillingToDelete = ReadonlySelfBilling;
			if (selfBillingToDelete != null && !selfBillingToDelete.IsDeleted)
			{
				selfBillingToDelete.Delete();
			}
			base.Delete();
			foreach (ClientLicenceFee fee in Fees.ToArray())
			{
				if (fee.L8_LD.IsEmpty)
				{
					fee.Delete();
				}
				else
				{
					fee.L8_LC = ZGuid.Empty;
				}
			}
			LicDatabases.RemoveAll();
		}

		#endregion

		#region Header

		public new EDIOrgHeader Header
		{
			get { return Factory.Load<EDIOrgHeader>(LC_OH); }
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (value && !IsSettingHasChangesSuspended && LC_OH.IsValid)
				{
					Factory.Load<EDIOrgHeader>(LC_OH)?.MarkForSaving();
				}
			}
		}

		#endregion

		#region Properties

		#region CompanyId

		public ZString CompanyId
		{
			get { return LC_CompanyNumber != 0 ? Base27Encoding.Encode(LC_CompanyNumber) : ""; }
		}

		public ZPropertyInfo CompanyIdInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyId)); }
		}

		#endregion

		#region LC_IsReciprocal

		protected bool LC_IsReciprocal_ReadOnly
		{
			get { return !EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed; }
		}

		#endregion

		#region LC_CompanyCountry

		public override ZString LC_CompanyCountry
		{
			get { return base.LC_CompanyCountry; }
			set
			{
				base.LC_CompanyCountry = value;
				if (!LC_CompanyCountry.IsEmpty && Country.IsSupportedForLicenceBuilder(LC_CompanyCountry))
				{
					LC_IsReciprocal = Country.IsReciprocal(LC_CompanyCountry);
					LC_IsGSTCashBasis = RefCountry.IsGSTCashBasis(LC_CompanyCountry);
				}
			}
		}

		#endregion

		public ZString PricelistRegion
		{
			get { return EDIDataRegistry.Instance.PricelistCountryRegions.Value.GetDescriptionFromCode(LC_CompanyCountry); }
		}

		#region LC_IsGSTRegistered

		protected bool LC_IsGSTRegistered_ReadOnly
		{
			get { return !EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed; }
		}

		#endregion

		#region GSTRegistered Is Different To Default

		public bool GSTRegisteredDifferentToDefault
		{
			get { return LC_IsGSTRegistered != Header.UNLOCO.Country.IsGSTRegistered; }
		}

		#endregion

		#region LC_IsGSTCashBasis

		protected bool LC_IsGSTCashBasis_ReadOnly
		{
			get { return !EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed; }
		}

		#endregion

		#region LC_IsWHTCashBasis

		protected bool LC_IsWHTCashBasis_ReadOnly
		{
			get { return !EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed; }
		}

		#endregion

		#region Address

		public ZGuid AddressPK
		{
			get { return fAddressPK; }
			set
			{
				SetNonPersistentPropertyValue(AddressPKInfo, ref fAddressPK, value);
			}
		}

		ZGuid fAddressPK;

		public ZPropertyInfo AddressPKInfo
		{
			get { return GetZPropertyInfo(nameof(AddressPK)); }
		}

		#endregion

		#region ExchangeRateText

		[MaxLength(100)]
		public ZString ExchangeRateText
		{
			get
			{
				return LC_IsReciprocal ? "1 Unit of Foreign is equal to X Unit(s) of Local" :
					"1 Unit of Local is equal to X Unit(s) of Foreign";
			}
		}

		public ZPropertyInfo ExchangeRateTextInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRateText)); }
		}

		#endregion

		#region LC_IsWHTRegistered

		public override ZBool LC_IsWHTRegistered
		{
			get { return base.LC_IsWHTRegistered; }
			set
			{
				base.LC_IsWHTRegistered = value;
				if (Header != null && Header.CompanyData.OB_ARWHTApplicable != value)
				{
					Header.CompanyData.OB_ARWHTApplicable = value;
				}
			}
		}

		protected bool LC_IsWHTRegistered_ReadOnly
		{
			get { return !EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed; }
		}

		#endregion

		#endregion

		public ZString CodeDescription
		{
			get { return "[" + LC_CompanyCountry + "] " + Header.OH_FullName; }
		}

		#region Saving

		public override void OnSaving()
		{
			SetCompanyNumberIfRequired();
			base.OnSaving();
			LogChanges();
		}

		void LogChanges()
		{
			if (LC_IsReciprocalInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.CurrentCulture, "Exchange Reciprocal Rate Status from {0} to {1}",
					(ZBool)LC_IsReciprocalInfo.OriginalValue ? "Yes" : "No", LC_IsReciprocal ? "Yes" : "No"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			if (LC_IsGSTRegisteredInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.CurrentCulture, "GST Registered Status from {0} to {1}",
					(ZBool)LC_IsGSTRegisteredInfo.OriginalValue ? "Yes" : "No", LC_IsGSTRegistered ? "Yes" : "No"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			if (LC_IsGSTCashBasisInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.CurrentCulture, "GST Cash Basis Status from {0} to {1}",
					(ZBool)LC_IsGSTCashBasisInfo.OriginalValue ? "Yes" : "No", LC_IsGSTCashBasis ? "Yes" : "No"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			if (LC_IsWHTRegisteredInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.CurrentCulture, "WHT Registered Status from {0} to {1}",
					(ZBool)LC_IsWHTRegisteredInfo.OriginalValue ? "Yes" : "No", LC_IsWHTRegistered ? "Yes" : "No"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			if (LC_IsWHTCashBasisInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.CurrentCulture, "WHT Cash Basis Status from {0} to {1}",
					(ZBool)LC_IsWHTCashBasisInfo.OriginalValue ? "Yes" : "No", LC_IsWHTCashBasis ? "Yes" : "No"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					LC_CompanyNumber = 0;
				}
			}
		}

		public void SetCompanyNumberIfRequired()
		{
			if (!IsInDatabase && LC_CompanyNumber == 0)
			{
				var fountain = Modules.ClientNumberFountainRegistration.GetInstance().LicenceCompanyNumber;
				LC_CompanyNumber = (int)fountain.GetNext(Factory);
			}
		}

		#endregion

		#region Clone

		protected override System.Collections.Generic.IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new string[] { Schema.LC_CompanyNumber };
		}

		#endregion

		#region Related Business Objects

		#region Licence Enterprise

		[RelatedBusinessObject("LicEnterprise")]
		public override ZGuid LC_LE
		{
			get { return base.LC_LE; }
			set { base.LC_LE = value; }
		}

		public LicenceEnterprise LicEnterprise
		{
			get { return Factory.Load<LicenceEnterprise>(LC_LE); }
		}

		#endregion

		#region Licence Databases

		[ChildEditable(true)]
		public LicenceCompanyLicenceDatabaseCollection LicDatabases
		{
			get
			{
				if (fLicDatabases == null)
				{
					fLicDatabases = new LicenceCompanyLicenceDatabaseCollection(this);
					fLicDatabases.Load();
					fLicDatabases.ActiveCountChanged += new EventHandler(LicDatabases_ActiveCountChanged);
					RegisterEditableChildObject(fLicDatabases);
				}

				return fLicDatabases;
			}
		}

		void LicDatabases_ActiveCountChanged(object sender, EventArgs e)
		{
			InactiveDatabaseCountInfo.RefreshBinding();
			licHeadersForAllDatabases?.Load();
		}

		LicenceCompanyLicenceDatabaseCollection fLicDatabases;

		#endregion

		#region Active or Inactive Databases

		/// <summary>
		/// This Licence Database collection changes dynamically to:
		///   - show all Licence Databases
		///   - show only active Licence Databases
		/// Based on the IncludeInactiveDatabases property.
		/// </summary>
		ActiveOrAllLicenceDatabaseCollection fActiveOrAllLicenceDatabases;
		public ActiveOrAllLicenceDatabaseCollection ActiveOrAllLicDatabases
		{
			get
			{
				if (fActiveOrAllLicenceDatabases == null)
				{
					fActiveOrAllLicenceDatabases = new ActiveOrAllLicenceDatabaseCollection(LicDatabases);
				}
				return fActiveOrAllLicenceDatabases;
			}
		}

		public LicenceHeader GetHeader(LicenceDatabase licDatabase)
		{
			return (LicenceHeader)LicDatabases.GetRelationshipBusinessObject(licDatabase);
		}

		public bool IsActive(LicenceDatabase licDatabase)
		{
			bool result = false;
			if (licDatabase.LD_IsActive)
			{
				LicenceHeader licHeader = GetHeader(licDatabase);
				result = !(licHeader != null) || (bool)licHeader.LA_IsActive;
			}
			return result;
		}

		#endregion

		#region IncludeInactiveDatabases

		public ZBool IncludeInactiveDatabases
		{
			get { return ActiveOrAllLicDatabases.IncludeInactiveDatabases; }
			set
			{
				ActiveOrAllLicDatabases.IncludeInactiveDatabases = value;
			}
		}

		public ZInt InactiveDatabaseCount
		{
			get { return LicDatabases.Cast<LicenceDatabase>().Count(s => !IsActive(s)); }
		}

		public ZPropertyInfo InactiveDatabaseCountInfo
		{
			get { return GetZPropertyInfo(nameof(InactiveDatabaseCount)); }
		}

		#endregion

		#region Licence Headers

		public LicenceHeader DummyLicHeaderForBinding => null;

		public LicenceHeaderCollection LicHeadersForAllDatabases
		{
			get
			{
				if (licHeadersForAllDatabases == null)
				{
					var localLicHeadersForAllDatabases = new LicenceHeaderCollection(Factory, new ZQuery(LicenceHeaderSchema.LA_LC, PK));
					localLicHeadersForAllDatabases.Load();
					licHeadersForAllDatabases = localLicHeadersForAllDatabases;
				}
				return licHeadersForAllDatabases;
			}
		}
		LicenceHeaderCollection licHeadersForAllDatabases;

		#endregion

		#region CompanyAddresses

		[BusinessObjectTestExclude]
		public OrgAddressDependentCollection CompanyAddresses
		{
			get
			{
				if (fCompanyAddresses == null)
				{
					fCompanyAddresses = new OrgAddressDependentCollection(Header);
					ZDBOnlyQuery addressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
					ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
					addressSubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgConstants.AddressType.Office);
					addressQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
					fCompanyAddresses.Load(addressQuery);
					fCompanyAddresses.IsManagedForDataRefresh = true;
				}
				return fCompanyAddresses;
			}
		}

		OrgAddressDependentCollection fCompanyAddresses;

		#endregion

		#region Licence3rdPartySoftware

		[ChildEditable(true)]
		public Licence3rdPartySoftwareDependentCollection Licence3rdPartySoftware
		{
			get
			{
				if (fLicence3rdPartySoftware == null)
				{
					fLicence3rdPartySoftware = new Licence3rdPartySoftwareDependentCollection(this);
					fLicence3rdPartySoftware.Load();
					RegisterEditableChildObject(fLicence3rdPartySoftware);
					if (!EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed)
					{
						fLicence3rdPartySoftware.SetReadOnlyIncludingChildren(true);
					}
				}
				return fLicence3rdPartySoftware;
			}
		}
		Licence3rdPartySoftwareDependentCollection fLicence3rdPartySoftware;

		#endregion

		#region CompanyContacts

		public OrgContactDependentCollection CompanyContacts
		{
			get
			{
				if (fCompanyContacts == null)
				{
					if (Header != null)
					{
						fCompanyContacts = new OrgContactDependentCollection(Header, new BusinessObjectFactory(), true);

						foreach (OrgContact contact in Header.Contacts)
						{
							fCompanyContacts.AddFromDatabase(contact.PK);
						}
					}
					else
					{
						fCompanyContacts = new OrgContactDependentCollection(Factory);
					}
				}

				return fCompanyContacts;
			}
		}

		OrgContactDependentCollection fCompanyContacts;

		#endregion

		#region ClientLicenceBilling

		/// <summary>
		/// Billing record to use for bills to this company.
		/// Used for branch, currency and tax.
		/// </summary>
		public ClientLicenceBilling SelfBilling
		{
			get { return GetSelfBilling(true); }
		}

		public ClientLicenceBilling ReadonlySelfBilling
		{
			get { return GetSelfBilling(false); }
		}

		ClientLicenceBilling selfBilling;

		ClientLicenceBilling GetSelfBilling(bool create)
		{
			if (selfBilling == null || (!IsDeleted && selfBilling.IsDeleted))
			{
				selfBilling = Factory.LoadTop1<ClientLicenceBilling>(new ZQuery(ClientLicenceBillingSchema.L4_LC, this.PK));
				if (selfBilling == null && create)
				{
					selfBilling = CreateSelfBilling();
				}
				if (selfBilling != null)
				{
					RegisterEditableChildObject(selfBilling);
					RegisterListChangedCalledRefreshBinding(selfBilling);
				}
			}
			return selfBilling;
		}

		ClientLicenceBilling CreateSelfBilling()
		{
			ClientLicenceBilling result = Factory.New<ClientLicenceBilling>();
			result.L4_LC = this.PK;
			return result;
		}

		#endregion

		#region InvoiceDelivery

		[ChildEditable]
		public ClientInvoiceDeliveryCollection InvoiceDeliveries
		{
			get
			{
				if (invoiceDeliveries == null)
				{
					invoiceDeliveries = new ClientInvoiceDeliveryCollection(this);
					RegisterEditableChildObject(invoiceDeliveries);
				}

				return invoiceDeliveries;
			}
		}
		ClientInvoiceDeliveryCollection invoiceDeliveries;

		public ZString InvoiceToText
		{
			get
			{
				ZString result = "";

				if (InvoiceDeliveries.Count > 0)
				{
					result = string.Join(", ", InvoiceDeliveries.Cast<ClientInvoiceDelivery>().Select(x => x.InvoiceToText.ToString()).Distinct().OrderBy(s => s).ToArray());
				}

				return result;
			}
		}

		public ZString InvoiceTaxCode
		{
			get
			{
				const string DefaultText = "Default";
				string result = string.Join(", ", InvoiceDeliveries.Cast<ClientInvoiceDelivery>()
					.Where(x => x.L9_IsBilled)
					.Select(x => x.TaxId != null ? x.TaxId.AT_Code.ToString() : DefaultText)
					.Distinct()
					.OrderBy(x => x)
					.ToArray());

				return result != DefaultText ? result : string.Empty;
			}
		}

		public ZString InvoiceCurrency
		{
			get
			{
				return string.Join(", ", InvoiceDeliveries.Cast<ClientInvoiceDelivery>()
					.Where(x => x.L9_IsBilled)
					.Select(x => x.L9_RX_NKInvoiceCurrency.ToString())
					.Distinct()
					.OrderBy(x => x)
					.ToArray());
			}
		}

		public ZString InvoiceBranch
		{
			get
			{
				return string.Join(", ", InvoiceDeliveries.Cast<ClientInvoiceDelivery>()
					.Where(x => x.L9_IsBilled && x.InvoicingBranch != null)
					.Select(x => x.InvoicingBranch.GB_Code.ToString())
					.Distinct()
					.OrderBy(x => x)
					.ToArray());
			}
		}

		#endregion

		public static LicenceCompany Load(BusinessObjectFactory factory, string enterpriseCode, string companyCode, string serverCode)
		{
			// select * from dbo.LicenceCompany
			// where LC_CompanyCode = 'ccc'
			// and lc_pk in (select LA_LC from dbo.LicenceHeader where LA_LD in (select LD_PK from dbo.LicenceDatabase where LD_ServerCode = 'sss'))
			// and lc_le in (select le_pk from dbo.LicenceEnterprise where LE_EnterpriseCode = 'eee')

			ZDBOnlyQuery companyQuery = new ZDBOnlyQuery(typeof(LicenceCompany));
			companyQuery.AddToFilter(LicenceCompanySchema.LC_CompanyCode, companyCode);

			ZDBOnlySubQuery enterpriseQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
			enterpriseQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);

			ZDBOnlySubQuery databaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			databaseQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);

			ZDBOnlySubQuery headerQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LC);
			headerQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, databaseQuery, JoinCondition.And);

			companyQuery.AddSubQuery(headerQuery, JoinCondition.And);
			companyQuery.AddSubQuery(enterpriseQuery, JoinCondition.And);

			return factory.LoadTop1<LicenceCompany>(companyQuery);
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ClearPriceHeadersNotifications();
			base.RunPreSaveValidationCore();
			PriceHeaders.ValidateStandardPricesExist();
			PriceHeaders.CheckNoDuplicateSettings();
		}

		void ClearPriceHeadersNotifications()
		{
			foreach (var header in PriceHeaders)
			{
				header.ClearRowNotifications();
			}
		}

		#endregion

		#region PriceHeaders

		[ChildEditable]
		public ClientLicencePriceHeaderCollection PriceHeaders
		{
			get
			{
				if (priceHeaders == null)
				{
					priceHeaders = new ClientLicencePriceHeaderCollection(this);
					RegisterEditableChildObject(priceHeaders);
					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						priceHeaders.SetReadOnlyIncludingChildren(true);
					}
				}

				return priceHeaders;
			}
		}
		ClientLicencePriceHeaderCollection priceHeaders;

		public static LicenceCompany StandardPricesCompany
		{
			get { return standardPricesCompany ?? (standardPricesCompany = GetStandardPricesCompany()); }
		}

		public static LicenceCompany GetStandardPricesCompany(BusinessObjectFactory factory)
		{
			var licHeader = LicenceHeader.LoadFromLicenceCode(factory, EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.Value);
			return licHeader != null ? licHeader.Company : null;
		}

		[ThreadStatic]
		static LicenceCompany standardPricesCompany;

		static LicenceCompany GetStandardPricesCompany()
		{
			return GetStandardPricesCompany(new BusinessObjectFactory());
		}

		public static ClientLicencePriceHeaderCollection GetStandardPriceHeaders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("StdPriceHeaderVersions", () =>
			{
				var stdCompany = LicenceCompany.GetStandardPricesCompany(factory);
				return stdCompany != null ? new ClientLicencePriceHeaderCollection(stdCompany) : null;
			});
		}

		internal static void ClearStandardPricesCompanyCache()
		{
			standardPricesCompany = null;
		}

		public ClientLicencePriceHeader PriceHeaderForDate(ZDateTime date, ZString systemCode)
		{
			ZQuery query = new ZQuery(ClientLicencePriceHeaderSchema.L6_LC, PK);
			query.AddToFilter(ClientLicencePriceHeaderSchema.L6_ValidFrom, SQLComparisonOperator.LessThanOrEqualTo, date);
			query.AddToFilter(ClientLicencePriceHeaderSchema.L6_SystemCode, systemCode);
			query.OrderBy = ClientLicencePriceHeaderSchema.Constants.L6_ValidFrom + " DESC";
			ClientLicencePriceHeader result = Factory.LoadTop1<ClientLicencePriceHeader>(query);

			// Note: not checking on ValidTo in the query above since a pricelist with a later ValidFrom automatically
			// expires all earlier pricelists.
			// All we need to check is if the latest ValidFrom pricelist has expired.
			if (result != null && result.L6_ValidTo.IsValid && result.L6_ValidTo < date)
			{
				result = null;
			}

			return result;
		}

		public ClientLicencePriceHeader OnDemandPriceHeaderForDate(ZDateTime date)
		{
			return PriceHeaderForDate(date, BillingConstants.BillingSystem.ODM);
		}

		public ClientLicencePriceHeader OneTimePriceHeaderForDate(ZDateTime date)
		{
			return PriceHeaderForDate(date, BillingConstants.BillingSystem.Maintenance);
		}

		public ClientLicencePriceHeader CreatePriceList(bool isStandard)
		{
			ClientLicencePriceHeader result = Factory.New<ClientLicencePriceHeader>();

			if (PK == GetStandardPricesCompany(Factory)?.PK)
			{
				isStandard = false;
			}
			result.L6_IsStandard = isStandard;
			result.L6_RN_NKCountry = LC_CompanyCountry;
			//Note: Not setting currency so as to force user to check for the case of price currency different from licence currency

			if (LicDatabases.Count > 0)
			{
				var databases = LicDatabases.ToArray<LicenceDatabase>();
				LicenceDatabase db = databases.FirstOrDefault(d => d.LD_IsActive && d.LD_LicenceType == DatabaseTypes.Codes.Production) ?? databases.First();

				var licHeader = GetHeader(db);
				result.L6_SystemCode = licHeader.IsOnDemandModuleTypeAllowed ? BillingConstants.BillingSystem.ODM : BillingConstants.BillingSystem.Maintenance;
			}

			PriceHeaders.Add(result);

			return result;
		}

		public void QuickAddTransactionalPriceList()
		{
			ClientLicencePriceHeader priceHeader = OnDemandPriceHeaderForDate(ZDateTime.Now);

			if (priceHeader == null || priceHeader.L6_IsStandard)
			{
				priceHeader = CreatePriceList(false);
				priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			}

			AddPriceItem(priceHeader, BillingConstants.BillingSystem.ImporterSecurityFiling);
			AddPriceItem(priceHeader, BillingConstants.BillingSystem.eBACCA);
			AddPriceItem(priceHeader, BillingConstants.BillingSystem.DeniedPartyScreening);
			AddPriceItem(priceHeader, BillingConstants.BillingSystem.ExDocs);
			AddPriceItem(priceHeader, BillingConstants.BillingSystem.DistanceCalculatorGeneric);
			AddPriceItem(priceHeader, BillingConstants.BillingSystem.DistanceCalculatorPcMiler);
			AddPriceItem(priceHeader, BillingConstants.BillingSystem.S8Cargo);
		}

		public ClientLicencePriceHeader QuickAddEHubPriceList()
		{
			ClientLicencePriceHeader result = Factory.New<ClientLicencePriceHeader>();
			result.L6_IsStandard = false;
			result.L6_UseStandardDiscount = false;
			result.L6_RN_NKCountry = LC_CompanyCountry;
			result.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;
			result.L6_DiscountCode = GetDefaultEHubDiscount();
			PriceHeaders.Add(result);
			return result;
		}

		string GetDefaultEHubDiscount()
		{
			string sql = "select top 1 L6_DiscountCode from dbo.ClientLicencePriceHeader where L6_SystemCode = 'HUB' and L6_DiscountCode != '' order by L6_ValidFrom desc";
			using (var cmd = CargoWise.Data.Db.Connection.Command(sql))
			{
				return cmd.ExecuteScalar().ToString();
			}
		}

		public void QuickAddOtherPriceList()
		{
			ClientLicencePriceHeader priceHeader = PriceHeaderForDate(ZDateTime.Now, BillingConstants.PriceHeaderType.Other);

			if (priceHeader == null || priceHeader.L6_IsStandard)
			{
				priceHeader = CreatePriceList(false);
				priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			}

			string[] chargeItemCodes = {
										   "W1C", "H1C", "S1C",
										   "W2C", "H2C", "S2C",
										   "W3C", "H3C", "S3C",
										   "W4C", "H4C", "S4C",
										   "W5C", "H5C", "S5C",
										   "W6C", "H6C", "S6C",
										   "WAC", "HAC", "SAC",
										   "WXC", "HXC", "SXC",
										   "WUC", "HUC", "SUC",
										   "WEC", "HEC",
										   "WRC", "HRC",
										   "WAP", "HAP", "SAP",
										   "WAN", "HAN", "SAN"
									   };

			string[] remitItemCodes = {
										  "WXP", "HXP", "SXP",
										  "WEP", "HEP",
										  "WRP", "HRP"
									  };

			short order = 1;

			foreach (var code in chargeItemCodes)
			{
				AddPriceItem(priceHeader, code, GetAirPriceItemDescription(code, false), order++);
			}

			foreach (var code in remitItemCodes)
			{
				AddPriceItem(priceHeader, code, GetAirPriceItemDescription(code, true), order++);
			}
		}

		ZString GetAirPriceItemDescription(ZString subCode, bool isRemitUsage)
		{
			if (subCode.Length != 3)
			{
				return "";
			}

			string fullDescription = "";

			var prefix = subCode.SubstringSafe(0, 1);
			var provider = subCode.SubstringSafe(1, 1);
			bool isTraxonService = provider == "E" || provider == "R";

			if (prefix == "W") { fullDescription += isRemitUsage || isTraxonService ? "FWB" : "FWB (net)"; }
			else if (prefix == "H") { fullDescription += isRemitUsage || isTraxonService ? "FHL" : "FHL (net)"; }
			else if (prefix == "S") { fullDescription += "FSU"; }

			if (provider == "1") { fullDescription += " - BT"; }
			if (provider == "2") { fullDescription += " - Delta"; }
			if (provider == "3") { fullDescription += " - CCSJ"; }
			if (provider == "4") { fullDescription += " - CCN"; }
			if (provider == "5") { fullDescription += " - Descartes"; }
			if (provider == "6") { fullDescription += " - GLSHK"; }
			if (provider == "A") { fullDescription += " - Traxon"; }
			if (provider == "X") { fullDescription += " - Traxon (CX/LY/AI/5X/US)"; }
			if (provider == "U") { fullDescription += " - Other Provider"; }
			if (provider == "E") { fullDescription += " - Traxon (EDP Service)"; }
			if (provider == "R") { fullDescription += " - Traxon (RCF Service)"; }

			if (subCode.SubstringSafe(2, 1) == "N") { fullDescription += " (Non-Chargeable)"; }
			else if (isRemitUsage) { fullDescription = "Remit: " + fullDescription; }

			return fullDescription;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void AddPriceItem(ClientLicencePriceHeader priceHeader, string moduleCode, string description = "", short order = 0)
		{
			ClientLicencePriceItem priceItem = priceHeader.Items.FindByCode(moduleCode);
			if (priceItem == null)
			{
				priceItem = priceHeader.Items.AddNew();
				priceItem.L7_Code = moduleCode;
				if (!string.IsNullOrEmpty(description))
				{
					priceItem.L7_Description = description;
				}
				else
				{
					priceItem.L7_Description = LicenceModuleList.Instance.GetDescriptionFromCode(moduleCode);
					if (priceItem.L7_Description.IsEmpty)
					{
						priceItem.L7_Description = BillingConstants.BillingSystemList.GetDescriptionFromCode(moduleCode);
					}
				}
				priceItem.L7_Order = order;

				priceItem.L7_FeeType = BillingConstants.FeeType.Transactional;
			}
		}

		public ClientLicencePriceHeader CopyPriceList(ClientLicencePriceHeader original)
		{
			ClientLicencePriceHeader result = null;
			ClientLicencePriceItemCollection items = original != null ? original.LocalOrStandardItems : null;
			if (items != null && items.Count > 0)
			{
				result = PriceHeaders.AddNew();
				result.CopyPersistentValuesFrom(original, new BusinessObjectCloneArgs(new string[] { ClientLicencePriceHeaderSchema.Constants.L6_LC, ClientLicencePriceHeaderSchema.Constants.L6_IsStandard }));
				result.L6_LC = PK;
				result.L6_IsStandard = false;
				foreach (ClientLicencePriceItem item in items)
				{
					ClientLicencePriceItem newItem = result.Items.AddNew();
					newItem.CopyPersistentValuesFrom(item, new BusinessObjectCloneArgs(new string[] { ClientLicencePriceItemSchema.Constants.L7_L6 }));

					foreach (EdiPriceItemRate rate in item.CurrencyRates)
					{
						var newRate = newItem.CurrencyRates.AddNew();
						newRate.CopyPersistentValuesFrom(rate, new BusinessObjectCloneArgs(new string[] { EdiPriceItemRateSchema.Constants.PIR_L7 }));
					}
				}
			}
			return result;
		}

		#endregion

		#region Deposit Balances

		[ChildEditable(true)]
		public DepositBalanceCollection DepositBalances
		{
			get
			{
				if (depositBalances == null)
				{
					depositBalances = new DepositBalanceCollection(this);
					depositBalances.Load();
					depositBalances.HasChanges = false;
					RegisterEditableChildObject(depositBalances);
				}

				return depositBalances;
			}
		}
		DepositBalanceCollection depositBalances;

		[ChildEditable()]
		[BusinessObjectTestExclude]
		public EdiDepositAdjustCollection DepositAdjustments
		{
			get
			{
				CreateDepositAdjustmentsIfNeeded(false);
				return depositAdjustments;
			}
		}

		internal bool CreateDepositAdjustmentsIfNeeded(bool load)
		{
			if (depositAdjustments == null)
			{
				depositAdjustments = new EdiDepositAdjustCollection(this.Header);
				RegisterEditableChildObject(depositAdjustments);
				if (load)
				{
					depositAdjustments.RefreshFromDb();
				}
				return true;
			}

			return false;
		}
		EdiDepositAdjustCollection depositAdjustments;

		public ZDecimal MonthlyUsageDepositBalance
		{
			get { return GetDepositBalanceAmount(EDIDataRegistry.Instance.OdplDepositChargeCode.Value); }
		}

		public ZString MonthlyUsageDepositCurrency
		{
			get { return GetDepositBalanceCurrency(EDIDataRegistry.Instance.OdplDepositChargeCode.Value); }
		}

		public ZBool IsMonthlyUsageDepositValid
		{
			get
			{
				var balance = FindDepositBalance(EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				return balance != null ? balance.IsValid : ZBool.True;
			}
		}

		internal void SetMonthlyUsageDepositBalanceForTest(ZDecimal amount, string currencyCode, bool isValid = true)
		{
			var balance = FindDepositBalance(EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
			balance.Amount = amount;
			balance.CurrencyCode = currencyCode;
			balance.IsValid = isValid;
		}

		public const string OldOdplDepositChargeCode = "ODPL";

		DepositBalance FindDepositBalance(string chargeCode)
		{
			return DepositBalances.Cast<DepositBalance>().FirstOrDefault(x => x.ChargeCode == chargeCode);
		}

		public ZDecimal GetDepositBalanceAmount(string chargeCode)
		{
			var balance = FindDepositBalance(chargeCode);
			return balance != null ? balance.Amount : ZDecimal.Zero;
		}

		public ZString GetDepositBalanceCurrency(string chargeCode)
		{
			var balance = FindDepositBalance(chargeCode);
			return balance != null ? balance.CurrencyCode : ZString.Empty;
		}

		#endregion

		#region Fees

		[ChildEditable]
		public ClientLicenceFeeCollection Fees
		{
			get
			{
				if (fees == null)
				{
					fees = new ClientLicenceFeeCollection(this);
					RegisterEditableChildObject(fees);

					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						fees.SetReadOnlyIncludingChildren(true);
					}
				}
				return fees;
			}
		}
		ClientLicenceFeeCollection fees;

		#endregion

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString log = string.Format(CultureInfo.CurrentCulture, "Company {0}{1}", LC_CompanyCode, ((LC_CompanyCodeInfo.HasChanges && !LC_CompanyCodeInfo.OriginalValue.IsEmpty) ? "(" + LC_CompanyCodeInfo.OriginalValue + ") -" : " -"));
				log = LogHelper.AddChangeLog(log, "Currency", LC_RX_NKCurrencyInfo);
				return log.EndsWith(" -", StringComparison.Ordinal) ? log.Left(log.Length - 2) : log;
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList objects = new ArrayList();
				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.AddRange(LicDatabases);

				foreach (LicenceDatabase licDB in LicDatabases)
				{
					var licHeader = GetHeader(licDB);
					objects.Add(licHeader);
					objects.AddRange(licHeader.BusinessObjectsWithRelatedEvents);
					objects.AddRange(licDB.BusinessObjectsWithRelatedEvents);
				}

				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			LC_CompanyCountry = Enterprise.Core.Constants.CountryCodes.Australia;
		}

#endif
		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceModify.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}

