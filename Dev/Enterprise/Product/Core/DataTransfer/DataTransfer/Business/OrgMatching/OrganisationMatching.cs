using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Business
{
	public class OrganisationMatching : IOrganisationMatching
	{
		public OrganisationMatching(BusinessObjectFactoryProvider factoryProvider, XmlInterchange interchange, INotifications notifications)
		{
			FactoryProvider = factoryProvider;
			Interchange = interchange;
			Notifications = notifications;
		}
		protected readonly BusinessObjectFactoryProvider FactoryProvider;
		protected readonly XmlInterchange Interchange;
		protected readonly INotifications Notifications;
		readonly SystemDefinedOrganisation registryOrganisation = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;

		#region Matching

		public BusinessObjectFactory Factory
		{
			get { return FactoryProvider.Current; }
		}

		public OrgMatchingResult Match(IValueObject orgValue, OrganisationTypes orgTypes)
		{
			return Match(orgValue, orgTypes, CreateTemporaryOrUnmatchOrgIfNoMatchFound);
		}

		public virtual OrgMatchingResult Match(IValueObject matchingCriteria, OrganisationTypes orgTypes, bool createTempOrgIfNoMatchFound)
		{
			if (matchingCriteria is Organisation)
			{
				return MatchOrganisation((Organisation)matchingCriteria, orgTypes, createTempOrgIfNoMatchFound);
			}

			return new OrgMatchingResult(null, false, false, null);
		}

		OrgMatchingResult MatchOrganisation(Organisation matchingCriteria, OrganisationTypes orgTypes, bool createTempOrgIfNoMatchFound)
		{
			CodeDescriptionPairList additionalInfo = new CodeDescriptionPairList();
			OrgHeader matchedOrg;

			var eDICode = matchingCriteria.EDICode;
			var foreignCode = matchingCriteria.OwnerCode;
			var temporaryOrganisation = CreateTemporaryOrganisationForMatching(matchingCriteria, orgTypes, OrgAddressSorter);

			var mappingOrg = GetMappingOrg(matchingCriteria);
			if (mappingOrg != null)
			{
				additionalInfo.Add(new CodeDescriptionPair(Res.GetString("1f4cf76a-39c2-48a9-b871-201e93957b83", "Mapping Organization"), mappingOrg.OH_Code));
			}
			string matcher;
			if (foreignCode.IsEmpty || mappingOrg == null)
			{
				if (!string.IsNullOrEmpty(eDICode))
				{
					additionalInfo.AddPair(Res.GetString("86b122d2-2711-43d6-91c4-1d761baee04f", "Matching by {0} Code", Core.Constants.ProductName), eDICode);
				}
				matchedOrg = MatchByLocalCode(eDICode, temporaryOrganisation, out matcher);
			}
			else
			{
				if (!string.IsNullOrEmpty(foreignCode))
				{
					additionalInfo.AddPair(Res.GetString("dd9d23b1-86cd-4474-a0eb-304e3e6ec1c4", "Matching by Foreign code"), foreignCode);
				}
				matchedOrg = MatchByForeignCode(foreignCode, mappingOrg, temporaryOrganisation, out matcher);
			}

			additionalInfo.AddPair(Res.GetString("87d77f35-95ec-4651-9525-2ec71b1c7fd3", "Using"), matcher);

			temporaryOrganisation.Delete();

			OrgMatchingResult result;

			var foundMatch = matchedOrg != null;
			additionalInfo.AddPair(Res.GetString("68b5ed9f-8316-4285-bffb-64dd2fa35dfa", "Found match"), foundMatch ? bool.TrueString : bool.FalseString);

			if (foundMatch)
			{
				return new OrgMatchingResult(matchedOrg, foundMatch, false, additionalInfo);
			}

			additionalInfo.AddPair(Res.GetString("e92a50ec-d489-496f-9905-a88a3bf96a54", "Should create Temp. Organization"), createTempOrgIfNoMatchFound ? bool.TrueString : bool.FalseString);
			if (!createTempOrgIfNoMatchFound)
			{
				return new OrgMatchingResult(null, foundMatch, false, additionalInfo);
			}

			OrgHeader orgHeader = null;

			bool criteriaHasNameOrCode = !temporaryOrganisation.OH_Code.IsEmpty || !temporaryOrganisation.OH_FullName.IsEmpty;
			additionalInfo.AddPair(Res.GetString("1ee98032-fd81-4330-8f53-1d0a0f7b41c5", "Criteria has Name/Code"), criteriaHasNameOrCode ? bool.TrueString : bool.FalseString);
			if (criteriaHasNameOrCode)
			{
				orgHeader = CreateTemporaryOrgHeader(matchingCriteria, orgTypes, OrgAddressSorter);
			}

			additionalInfo.AddPair(Res.GetString("6dba1352-79f8-4dec-8301-5ecad592ffb3", "Temp. Organization created"), orgHeader != null ? bool.TrueString : bool.FalseString);
			if (orgHeader != null)
			{
				result = new OrgMatchingResult(orgHeader, foundMatch, false, additionalInfo);
			}
			else
			{
				result = new OrgMatchingResult(null, foundMatch, true, additionalInfo);
			}

			return result;
		}

		protected virtual bool CreateTemporaryOrUnmatchOrgIfNoMatchFound
		{
			get { return true; }
		}

		protected OrgHeader Match(IOrgHeaderForMatching temporaryOrganisation)
		{
			var matcher = new OrgSimilarMatcher(Factory);
			return matcher.GetMatchingOrganisation(temporaryOrganisation);
		}

		protected OrgHeader Match(ZString orgCode, IOrgHeaderForMatching temporaryOrganisation)
		{
			string matcher;
			return MatchByLocalCode(orgCode, temporaryOrganisation, out matcher);
		}

		protected OrgHeader MatchForeignCodeOrMatchOnTempAndRegisterForeignCode(ZString foreignOrgCode, OrgHeader mappingOrg, IOrgHeaderForMatching temporaryOrganisation)
		{
			string matcher;
			return MatchByForeignCode(foreignOrgCode, mappingOrg, temporaryOrganisation, out matcher);
		}

		#region Implementation

		OrgHeader MatchByLocalCode(ZString orgCode, IOrgHeaderForMatching temporaryOrganisation, out string usedMatcher)
		{
			var localCodeMatcher = new OrgLocalCodeMatcher(Factory);
			var similarityMatcher = new OrgSimilarMatcher(Factory);
			var defaulValueMatcher = new OrgDefaultValueMatcher(Factory);
			var result = localCodeMatcher.Match(orgCode, temporaryOrganisation);
			if (result != null)
			{
				usedMatcher = Res.GetString("9f37f4a6-3f3c-4188-893a-23f74490c09d", "{0} Code Matcher", Core.Constants.ProductName);
			}
			else
			{
				result = similarityMatcher.GetMatchingOrganisation(temporaryOrganisation);
				if (result != null)
				{
					usedMatcher = SimilarityMatcherText;
				}
				else
				{
					result = defaulValueMatcher.Match();
					usedMatcher = DefaultMatcherText;
				}
			}
			return result;
		}

		OrgHeader MatchByForeignCode(ZString foreignOrgCode, OrgHeader mappingOrg, IOrgHeaderForMatching temporaryOrganisation, out string usedMatcher)
		{
			var foreignCodeMatcher = new OrgForeignCodeMatcher(Factory);
			var similarityMatcher = new OrgSimilarMatcher(Factory);
			var defaulValueMatcher = new OrgDefaultValueMatcher(Factory);

			var result = foreignCodeMatcher.Match(foreignOrgCode, mappingOrg);
			if (result != null)
			{
				usedMatcher = Res.GetString("548f9530-faaf-4139-93b7-c6ac16fc04e1", "Foreign Code Matcher");
			}
			else
			{
				result = similarityMatcher.GetMatchingOrganisation(temporaryOrganisation);
				if (result != null)
				{
					usedMatcher = SimilarityMatcherText;
					RegistryForeignCode(foreignOrgCode, mappingOrg, result);
				}
				else
				{
					usedMatcher = DefaultMatcherText;
					result = defaulValueMatcher.Match();
				}
			}
			return result;
		}

		static string SimilarityMatcherText
		{
			get { return Res.GetString("c428d07f-f05f-499a-ae9a-73268cac587a", "Similarity Matcher"); }
		}
		static string DefaultMatcherText
		{
			get { return Res.GetString("aaf1127e-a52c-43d0-80a6-b2b928a5bfdd", "Default Matcher"); }
		}

		void RegistryForeignCode(ZString foreignOrgCode, OrgHeader mappingOrg, IOrgHeaderForMatching result)
		{
			var codeMappingCreater = new EDICodeMappingCreater();
			if (result.PK != registryOrganisation.Organisation)
			{
				codeMappingCreater.CreateEDICodeMapping(mappingOrg, foreignOrgCode, result);
			}
		}

		OrgHeader GetMappingOrg(Organisation orgValue)
		{
			var mappingOrg = (OrgHeader)Factory.Load(typeof(OrgHeader), GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var isMatchingTheMappingOrg = Interchange.InterchangeInfo.EDIOrganisation == orgValue;

			if (!Interchange.InterchangeInfo.IsSpecified || isMatchingTheMappingOrg || !Interchange.InterchangeInfo.EDIOrganisation.IsSpecified)
			{
				return mappingOrg;
			}

			var context = new ValueObjectImportContext(FactoryProvider, Interchange, new NotificationBuffer());
			var orgHeader = context.FindOrganisation(Interchange.InterchangeInfo.EDIOrganisation, null, OrganisationTypes.None);

			if (orgHeader != null && orgHeader.PK != registryOrganisation.Organisation)
			{
				mappingOrg = orgHeader;
			}
			return mappingOrg;
		}

		IOrgHeaderForMatching CreateTemporaryOrganisationForMatching(Organisation orgValue, OrganisationTypes orgTypes, IOrgAddressSorter sorter)
		{
			var interchangeForCreatingTempOrg = new XmlInterchange();
			interchangeForCreatingTempOrg.ImportEDICode = Interchange.ImportEDICode;

			var context = new ValueObjectImportContext(FactoryProvider, interchangeForCreatingTempOrg, Notifications);
			var result = new OrgHeaderForMatching(context.Factory);
			new TempOrganisationValueObjectDataAdapterForOrgMatching(orgTypes, sorter).ImportFromValueObjectForMatching(result, orgValue, context);
			return result;
		}

		OrgHeader CreateTemporaryOrgHeader(Organisation orgValue, OrganisationTypes orgTypes, IOrgAddressSorter sorter)
		{
			var result = (OrgHeader)Factory.New(typeof(OrgHeader));
			result.OH_IsTempAccount = true;

			var interchangeForCreatingTempOrg = new XmlInterchange();
			interchangeForCreatingTempOrg.ImportEDICode = Interchange.ImportEDICode;

			var context = new ValueObjectImportContext(FactoryProvider, interchangeForCreatingTempOrg, Notifications);
			new OrganisationValueObjectDataAdapterForOrgMatching(orgTypes, sorter).ImportFromValueObject(result, orgValue, context);
			int dummy = result.PatternMatchesForThisOrg.Count; // to launch pattern matches generation
			return result;
		}

		#endregion

		#endregion

		#region Finding Organisations

		public ZGuid FindOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			IOrgHeaderForMatching orgHeader = FindOrganisation(value, sourceObject, orgType);
			return orgHeader == null ? ZGuid.Empty : orgHeader.PK;
		}

		public OrgHeader FindOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationSubType = Enum.GetName(typeof(OrganisationTypes), orgType) };
			return FindOrganisation(value, sourceObject, orgType, unmatchOrgRecordCriteria);
		}

		public OrgHeader FindOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			var organisation = value as Organisation;
			if (organisation != null)
			{
				return FindOrCreateTempOrganisation(organisation, sourceObject, orgType, false, unmatchOrgRecordCriteria);
			}
			return null;
		}

		public ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationSubType = Enum.GetName(typeof(OrganisationTypes), orgType) };
			return FindOrCreateTempOrganisationPK(value, sourceObject, orgType, unmatchOrgRecordCriteria);
		}

		public ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			IOrgHeaderForMatching org = null;

			if (value is Organisation)
			{
				org = FindOrCreateTempOrganisation(value, sourceObject, orgType, unmatchOrgRecordCriteria);
			}

			return (org == null) ? ZGuid.Empty : org.PK;
		}

		public IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationSubType = Enum.GetName(typeof(OrganisationTypes), orgType) };
			return FindOrCreateTempOrganisation(value, sourceObject, orgType, unmatchOrgRecordCriteria);
		}

		public IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			var organisation = value as Organisation;
			if (organisation != null)
			{
				return FindOrCreateTempOrganisation(organisation, sourceObject, orgType, true, unmatchOrgRecordCriteria);
			}
			return null;
		}

		public IOrgAddressSorter OrgAddressSorter { get; set; }

		#region Implementation

		OrgHeader FindOrCreateTempOrganisation(Organisation orgValue, BusinessObject sourceObject, OrganisationTypes orgType, bool createTempIfNotFound, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			OrgHeader result = null;
			if (orgValue == null || !orgValue.IsSpecified)
			{
				return result;
			}
			var matchingResult = Match(orgValue, orgType, createTempIfNotFound);

			if (matchingResult.MatchFound)
			{
				result = matchingResult.Match as OrgHeader;

				var iNotification = new OrganisationMatchedNotification(orgValue, result, matchingResult.AdditionalInfo);
				var notificationBuffer = Notifications as NotificationBuffer;
				if (notificationBuffer != null)
				{
					var msg = iNotification.Message;
					if (msg.Length > 0 && !notificationBuffer.Events.Contains(msg))
					{
						Notifications.Notify(iNotification);
					}
				}
				else
				{
					Notifications.Notify(iNotification);
				}

				if (result.PK == registryOrganisation.Organisation)
				{
					if (sourceObject == null)
					{
						return result;
					}

					var unmatchOrg = ConvertOrgUnmatchDetail(orgType, orgValue, unmatchOrgRecordCriteria);
					new UnmatchNoteCreator(Factory).Create(EntityInfo.New(sourceObject), unmatchOrg);
				}
			}
			else
			{
				if (matchingResult.TempOrgCouldNotBeCreated)
				{
					NotifyCouldNotMatchOrCreateTemporaryOrg(orgValue);
				}
				else if (matchingResult.Match != null)
				{
					result = matchingResult.Match as OrgHeader;
					PopulateMainAddress1WithDummyDataIfEmpty(result);
					Notifications.Notify(new OrganisationUnmatchedNotification(result, matchingResult.AdditionalInfo));
					Notifications.Notify(new BusinessObjectCreatedOrUpdatedNotification(result));
				}
			}
			return result;
		}

		internal static string UnmatchedOrgNoteDescriptionText
		{
			get { return Res.GetString("4e7a93b4-2ca4-46f5-8bb4-82bd34d909ea", "Organization matching failed to find one or more organizations during data import. Details of the unmatched organizations are shown below:"); }
		}

		UnmatchOrgRecord ConvertOrgUnmatchDetail(OrganisationTypes orgType, AutoOrganisation orgValue, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			var unmatchOrg = new UnmatchOrgRecord();
			unmatchOrg.OrganisationType = orgType.ToString();
			unmatchOrg.OwnerCode = orgValue.OwnerCode;
			unmatchOrg.EDICode = orgValue.EDICode;
			unmatchOrg.OrganisationName = orgValue.OrganisationDetails.Name;
			unmatchOrg.OrganisationSubType = unmatchOrgRecordCriteria.OrganisationSubType;
			unmatchOrg.DocAddressType = unmatchOrgRecordCriteria.DocAddressType;

			if (orgValue.OrganisationDetails.Addresses.Count > 0)
			{
				Xsd.OrgAddress addressValue = orgValue.OrganisationDetails.Addresses[0];
				unmatchOrg.AddressLine1 = addressValue.AddressLine1;
				unmatchOrg.AddressLine2 = addressValue.AddressLine2;
				unmatchOrg.PostCode = addressValue.PostCode;
				unmatchOrg.City = addressValue.CityOrSuburb;
				unmatchOrg.StateOrProvince = addressValue.StateOrProvince;
				unmatchOrg.Country = addressValue.LocationSpecified ? addressValue.Location.Value.SubstringSafe(0, 2) : orgValue.OrganisationDetails.Location.Value.SubstringSafe(0, 2);
			}

			return unmatchOrg;
		}

		void PopulateMainAddress1WithDummyDataIfEmpty(OrgHeader organisation)
		{
			if (!organisation.MainAddress.OA_Address1.IsEmpty)
			{
				return;
			}

			organisation.MainAddress.OA_Address1 = organisation.OH_Code;
		}

		void NotifyCouldNotMatchOrCreateTemporaryOrg(AutoOrganisation orgValue)
		{
			var finalNotificationString = "";
			if (orgValue.OwnerCode.IsEmpty)
			{
				finalNotificationString = Res.GetString("E2115352-1696-4d7b-BAA6-418C7E18B7E4", "No match found and could not create temporary organization for {0} Code='{1}'", Core.Constants.ProductName, orgValue.EDICode);
			}
			else
			{
				finalNotificationString = Res.GetString("A97AC4FE-290C-4e0d-BDE8-473CC61EA232", "No match found and could not create temporary organization for Owner Code='{0}'", orgValue.OwnerCode);
			}
			AddNotificationCouldNotMatchOrCreateTemporaryOrg(finalNotificationString);
		}

		void AddNotificationCouldNotMatchOrCreateTemporaryOrg(string finalNotificationString)
		{
			var notificationBuffer = Notifications as NotificationBuffer;

			if (notificationBuffer != null)
			{
				if (!notificationBuffer.Events.ContainsNotificationContaining(finalNotificationString))
				{
					Notifications.Notify(new WarningNotification(WarningType.Warning, finalNotificationString));
				}
			}
		}

		#endregion

		#endregion
	}
}
