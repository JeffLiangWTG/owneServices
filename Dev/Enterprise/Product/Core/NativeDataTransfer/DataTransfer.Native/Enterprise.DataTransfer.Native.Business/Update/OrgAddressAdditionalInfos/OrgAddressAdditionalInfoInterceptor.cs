using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgAddressAdditionalInfos
{
	public class OrgAddressAdditionalInfoInterceptor : BaseInterceptor
	{
		readonly BusinessObjectFactory factory;

		public OrgAddressAdditionalInfoInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices) : base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}

		public override void Invoke(IEntitySet entitySet)
		{
			UpdateAdditionalInfo(entitySet.Root);
			Function(entitySet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void CheckEntityValid(IEntity address)
		{
			var allOrgAddressAdditionalInfoNodes = GetAllOrgAddressAdditionalInfoNodes(address);
			if (allOrgAddressAdditionalInfoNodes != null)
			{
				if (allOrgAddressAdditionalInfoNodes.Any(x => x.HasProperty("AdditionalInfo") && string.IsNullOrWhiteSpace(x.GetPropertyOrBlankString("AdditionalInfo"))))
				{
					throw new NativeXMLUserVisibleException("AdditionalInfo under OrgAddressAdditionalInfo should not be empty.");
				}

				if (allOrgAddressAdditionalInfoNodes.Any(x => GetAllOrgTranslatedAddressAdditionalInfoNodes(x) != null && GetAllOrgTranslatedAddressAdditionalInfoNodes(x).Any(y => y.HasProperty("AdditionalInfo") && string.IsNullOrWhiteSpace(y.GetPropertyOrBlankString("AdditionalInfo")))))
				{
					throw new NativeXMLUserVisibleException("AdditionalInfo under OrgTranslatedAddressAdditionalInfo should not be empty.");
				}

				if (allOrgAddressAdditionalInfoNodes.Count(x => x.GetPropertyOrBlankString("IsPrimary").Equals("true", StringComparison.InvariantCultureIgnoreCase)) > 1)
				{
					var primaryAdditionalInfos = allOrgAddressAdditionalInfoNodes.Where(t => t.GetPropertyOrBlankString("IsPrimary").Equals("true", StringComparison.InvariantCultureIgnoreCase)).Select(t => t.GetPropertyOrBlankString("AdditionalInfo"));
					throw new NativeXMLUserVisibleException($"There should only be 1 primary OrgAddressAdditionalInfo under same OrgAddressAdditionalInfoCollection. But these AdditionalInfo are all set to primary: {string.Join(", ", primaryAdditionalInfos)}");
				}

				foreach (var orgAddressAdditionalInfoNode in allOrgAddressAdditionalInfoNodes)
				{
					var allOrgTranslatedAddressAdditionalInfoNodes = GetAllOrgTranslatedAddressAdditionalInfoNodes(orgAddressAdditionalInfoNode);
					if (allOrgTranslatedAddressAdditionalInfoNodes != null)
					{
						var duplicateLanguages = allOrgTranslatedAddressAdditionalInfoNodes.GroupBy(t => t.GetPropertyOrBlankString("Language").Trim()).Where(t => t.Count() > 1).Select(t => t.Key);

						if (duplicateLanguages.Any())
						{
							throw new NativeXMLUserVisibleException($"Language under same OrgTranslatedAddressAdditionalInfo should not be identical. Duplicates are {string.Join(", ", duplicateLanguages)}");
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void UpdateAdditionalInfo(IEntity root)
		{
			var addresses = root.ChildrenCollection.Where(x => x.EntityName == OrgAddressSchema.Constants.TableName).ToList();

			foreach (var address in addresses)
			{
				CheckEntityValid(address);
			}

			foreach (var address in addresses)
			{
				var allOrgAddressAdditionalInfoNodes = GetAllOrgAddressAdditionalInfoNodes(address);
				var oldAdditionalAddressInformation = address.GetPropertyOrBlankString("AdditionalAddressInformation");

				if (!string.IsNullOrWhiteSpace(oldAdditionalAddressInformation))
				{
					var newAdditionalInfoToBeSetToTrue = allOrgAddressAdditionalInfoNodes.FirstOrDefault(a => a.GetPropertyOrBlankString("AdditionalInfo").Equals(oldAdditionalAddressInformation, StringComparison.InvariantCultureIgnoreCase));
					if (newAdditionalInfoToBeSetToTrue != null)
					{
						allOrgAddressAdditionalInfoNodes.Except(newAdditionalInfoToBeSetToTrue).ForEach(a => a["IsPrimary"] = "false");
						if (!newAdditionalInfoToBeSetToTrue.GetPropertyOrBlankString("IsPrimary").Equals("true", StringComparison.InvariantCultureIgnoreCase))
						{
							newAdditionalInfoToBeSetToTrue["IsPrimary"] = "true";
						}

						allOrgAddressAdditionalInfoNodes.Except(newAdditionalInfoToBeSetToTrue)
							.Where(a => a.GetPropertyOrBlankString("AdditionalInfo").Equals(oldAdditionalAddressInformation, StringComparison.InvariantCultureIgnoreCase))
							.ForEach(a => address.ChildrenCollection.Remove(a));
					}
					else
					{
						allOrgAddressAdditionalInfoNodes.ForEach(a => a["IsPrimary"] = "false");
						var additionalInfoEntity = new Entity(address.Definition.Children.First(d => d.EntityName == OrgAddressAdditionalInfoSchema.Constants.TableName), sessionServices)
						{
							Action = EntityAction.MERGE,
							Parent = address,
							["AdditionalInfo"] = oldAdditionalAddressInformation,
							["IsPrimary"] = "true"
						};
						address.ChildrenCollection.Add(additionalInfoEntity);
					}
				}
				else
				{
					var primaryAdditionalInfo = allOrgAddressAdditionalInfoNodes.FirstOrDefault(additionalInfo => additionalInfo.GetPropertyOrBlankString("IsPrimary").Equals("true", StringComparison.InvariantCultureIgnoreCase));
					if (primaryAdditionalInfo != null)
					{
						var primaryAdditionalInfoValue = primaryAdditionalInfo.GetPropertyOrBlankString("AdditionalInfo");
						address["AdditionalAddressInformation"] = primaryAdditionalInfoValue;
					}
					else
					{
						var primaryAdditionalInfoInDb = GetPrimaryAdditionalInfoInDb(address);
						if (!string.IsNullOrWhiteSpace(primaryAdditionalInfoInDb?.OAI_AdditionalInfo))
						{
							address["AdditionalAddressInformation"] = primaryAdditionalInfoInDb.OAI_AdditionalInfo;
						}
					}
				}

				UpdateTranslatedAdditionalInfo(address);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void UpdateTranslatedAdditionalInfo(IEntity address)
		{
			var validLanguageList = new List<string>();
			var additionalInfoEntity = address.Children?.Where(x => x.EntityName == OrgAddressAdditionalInfoSchema.Constants.TableName).FirstOrDefault(x => x.GetPropertyOrBlankString("IsPrimary").Equals("true", StringComparison.InvariantCultureIgnoreCase));
			var orgTranslatedAddressAdditionalInfos = additionalInfoEntity?.Children?.Where(x => x.EntityName == OrgTranslatedAddressAdditionalInfoSchema.Constants.TableName).ToList();

			var orgTranslatedAddresses = address.ChildrenCollection.Where(x => x.EntityName == OrgTranslatedAddressSchema.Constants.TableName).ToList();
			foreach (var orgTranslatedAddress in orgTranslatedAddresses)
			{
				var language = orgTranslatedAddress.GetPropertyOrBlankString("Language");
				validLanguageList.Add(language);

				if (!string.IsNullOrWhiteSpace(language))
				{
					var orgTranslatedAddressAdditionalAddress = orgTranslatedAddress.GetPropertyOrBlankString("AdditionalAddressInformation");

					if (!string.IsNullOrWhiteSpace(orgTranslatedAddressAdditionalAddress))
					{
						var orgTranslatedAddressAdditionalInfoWithSameLanguage = orgTranslatedAddressAdditionalInfos?.FirstOrDefault(
								o => o.GetPropertyOrBlankString("Language").Equals(language, StringComparison.InvariantCultureIgnoreCase));

						if (orgTranslatedAddressAdditionalInfoWithSameLanguage != null)
						{
							orgTranslatedAddressAdditionalInfoWithSameLanguage["AdditionalInfo"] = orgTranslatedAddressAdditionalAddress;
						}
						else if (additionalInfoEntity != null)
						{
							var translatedInfoEntity = new Entity(additionalInfoEntity.Definition.Children.First(d => d.EntityName == OrgTranslatedAddressAdditionalInfoSchema.Constants.TableName), sessionServices)
							{
								Action = EntityAction.MERGE,
								Parent = additionalInfoEntity,
								["Language"] = language,
								["AdditionalInfo"] = orgTranslatedAddressAdditionalAddress
							};

							additionalInfoEntity.ChildrenCollection.Add(translatedInfoEntity);
						}
					}
					else
					{
						var orgTranslatedAddressAdditionalInfoWithSameLanguage = orgTranslatedAddressAdditionalInfos?.FirstOrDefault(
							o => o.GetPropertyOrBlankString("Language").Equals(language, StringComparison.InvariantCultureIgnoreCase));

						if (orgTranslatedAddressAdditionalInfoWithSameLanguage == null)
						{
							var primaryTranslatedAdditionalInfoInDb = GetPrimaryTranslatedAdditionalInfoInDb(address, language)?.OTI_AdditionalInfo;
							if (!string.IsNullOrWhiteSpace(primaryTranslatedAdditionalInfoInDb))
							{
								orgTranslatedAddress["AdditionalAddressInformation"] = primaryTranslatedAdditionalInfoInDb;
							}
						}
						else
						{
							orgTranslatedAddress["AdditionalAddressInformation"] = orgTranslatedAddressAdditionalInfoWithSameLanguage["AdditionalInfo"];
						}
					}
				}
			}

			var invalidOrgTranslatedAddressAdditionalInfos = orgTranslatedAddressAdditionalInfos?.Where(o => !validLanguageList.Contains(o.GetPropertyOrBlankString("Language")));
			invalidOrgTranslatedAddressAdditionalInfos.ForEach(i => additionalInfoEntity?.ChildrenCollection.Remove(i));
		}

		OrgAddressAdditionalInfo GetPrimaryAdditionalInfoInDb(IEntity address)
		{
			var query = new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, address.InternalPK);
			query.AddToFilter(OrgAddressAdditionalInfoSchema.OAI_IsPrimary, true);

			return factory.LoadTop1<OrgAddressAdditionalInfo>(query);
		}

		OrgTranslatedAddressAdditionalInfo GetPrimaryTranslatedAdditionalInfoInDb(IEntity address, string translatedLanguage)
		{
			var query = new ZDBOnlyQuery(typeof(OrgTranslatedAddressAdditionalInfo));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgAddressAdditionalInfo), OrgAddressAdditionalInfoSchema.PK);
			subQuery.AddToFilter(OrgAddressAdditionalInfoSchema.PK, address.InternalPK)
				.AddToFilter(OrgAddressAdditionalInfoSchema.OAI_IsPrimary, true);
			query.AddToFilter(OrgTranslatedAddressAdditionalInfoSchema.OTI_Language, translatedLanguage);
			query.AddSubQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, subQuery, JoinCondition.And);

			return factory.LoadTop1<OrgTranslatedAddressAdditionalInfo>(query);
		}

		List<IEntity> GetAllOrgAddressAdditionalInfoNodes(IEntity address)
		{
			return address?.Children?.Where(x => x.EntityName == OrgAddressAdditionalInfoSchema.Constants.TableName).ToList();
		}

		List<IEntity> GetAllOrgTranslatedAddressAdditionalInfoNodes(IEntity addressAdditionalInfo)
		{
			return addressAdditionalInfo?.Children?.Where(x => x.EntityName == OrgTranslatedAddressAdditionalInfoSchema.Constants.TableName).ToList();
		}
	}
}
