//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoQuarantineExDocHeaderLookups
//
//    This class should be used for overriding collections in AutoQuarantineExDocHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocHeaderLookups : AutoQuarantineExDocHeaderLookups
	{
		public QuarantineExDocHeaderLookups(AutoQuarantineExDocHeader parent)
			: base(parent)
		{
			this.parent = (QuarantineExDocHeader)parent;
		}

		public CodeDescriptionPairList ProduceType
		{
			get
			{
				var isOtherActive = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
				return Factory.GetCachedValue("QuarantineExDocHeader_ProduceType" + isOtherActive, () =>
				{
					var result = new EXDOCCommodityCodes();
					if (!isOtherActive)
					{
						result.RemoveCode(EXDOCCommodityCodes.Codes.OtherGoods);
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList TemperatureUnit => Factory.GetCachedValue<EXDOCTemperatureUnitCodes>();

		public CodeDescriptionPairList CertificatePrintCode => Factory.GetCachedValue<EXDOCCertificatePrintCodes>();

		public CodeDescriptionPairList ProductUseIndicatorList
		{
			get
			{
				if (parent.IsNEXDOCSActive)
				{
					return Factory.GetCachedValue<NEXDOCProductUseIndicatorCodes>();
				}
				else
				{
					return Factory.GetCachedValue<EXDOCProductUseIndicatorCodes>();
				}
			}
		}

		public const string NPRTR = "NPRTR";
		public const string CommodityTypeCode = "CommodityTypeCode";

		public EXDOCAqisPlaceCollection AqisPlaces
		{
			get
			{
				EXDOCAqisPlaceCollection aqisPlaces = null;
				if (parent.IsNEXDOCSActive)
				{
					aqisPlaces = new EXDOCAqisPlaceCollection(parent, NPRTR);
					aqisPlaces.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)NPRTR));
					aqisPlaces.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", (ZString)Core.Constants.CountryCodes.Australia));
					aqisPlaces.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", ZDateTime.Today));
					aqisPlaces.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", (ZString)CommodityTypeCode));
					aqisPlaces.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", GetNexdocAttributeValue()));
				}
				else
				{
					aqisPlaces = new EXDOCAqisPlaceCollection(parent);
				}
				aqisPlaces.SetOverrideNotificationWhenAdditionalFilterNotMet("When produce type is meat, only Quarantine Regions can be selected");
				return aqisPlaces;
			}
		}

		public IZType GetNexdocAttributeValue()
		{
			var result = ZString.Empty;
			if (!parent.QH_ProduceType.IsEmpty)
			{
				result = parent.QH_ProduceType.Substring(0, 1);
			}
			return result;
		}

		public CodeDescriptionPairList LocationWithAqisPlace => Factory.GetCachedValue("QuarantineExDocHeader_LocationWithAqisPlace", GetLocationWithAqisPlace);

		public CodeDescriptionPairList GetLocationWithAqisPlace()
		{
			CodeDescriptionPairList result = new EXDOCCodeOrganisation();
			result.AddPair(AqisPlaceCode, AqisPlaceCode);
			return result;
		}

		public CodeDescriptionPairList Location => Factory.GetCachedValue<EXDOCCodeOrganisation>();

		public OrgHeaderCollectionWithSpecificRegistrationCodes Establishment
		{
			get
			{
				var result = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber);
				result.SetOverrideNotificationWhenAdditionalFilterNotMet(OrgHeaderCollectionWithSpecificRegistrationCodes.OrgDoesNotHaveRegNo("(ESN) EXDOC Establishment Number"));
				return result;
			}
		}

		public OrgHeaderCollectionWithSpecificRegistrationCodes EDIUser
		{
			get
			{
				var result = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, parent.UserIdentifierCusCode);

				var message = parent.IsNEXDOCSActive ? "(NEI) NEXDOCS External ID" : "(EEU) EXDOC Edi User";
				result.SetOverrideNotificationWhenAdditionalFilterNotMet(OrgHeaderCollectionWithSpecificRegistrationCodes.OrgDoesNotHaveRegNo(message));

				return result;
			}
		}

		public OrgHeaderCollectionWithSpecificRegistrationCodes ExporterNumber
		{
			get
			{
				var result = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, parent.ExporterNumberCusCode);

				var message = parent.IsNEXDOCSActive ? "(NEN) NEXDOCS Exporter Number" : "(EEN) EXDOC Exporter Number";
				result.SetOverrideNotificationWhenAdditionalFilterNotMet(OrgHeaderCollectionWithSpecificRegistrationCodes.OrgDoesNotHaveRegNo(message));

				return result;
			}
		}

		public CodeDescriptionPairList ComplianceCodes => !parent.IsNEXDOCSActive
			? Factory.GetCachedValue<EXDOCComplianceStatusCodes>()
			: Factory.GetCachedValue(string.Concat(nameof(QuarantineExDocHeaderLookups), nameof(ComplianceCodes)), () =>
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(EXDOCComplianceStatusCodes.Codes.Completed, EXDOCComplianceStatusCodes.Descriptions.Completed);
				result.AddPair(EXDOCComplianceStatusCodes.Codes.Final, EXDOCComplianceStatusCodes.Descriptions.Final);
				result.AddPair(EXDOCComplianceStatusCodes.Codes.CertificateReady, EXDOCComplianceStatusCodes.Descriptions.CertificateReady);
				result.AddPair(EXDOCComplianceStatusCodes.Codes.Initial, EXDOCComplianceStatusCodes.Descriptions.Initial);
				result.AddPair(EXDOCComplianceStatusCodes.Codes.Order, EXDOCComplianceStatusCodes.Descriptions.Order);

				return result;
			});

		public CodeDescriptionPairList DeclarationOfCompliance => Factory.GetCachedValue<EXDOCYesNoEmpty>();

		public CodeDescriptionPairList TrueAndCompleteIndicatorList => Factory.GetCachedValue<EXDOCYesNoEmpty>();

		public CodeDescriptionPairList ImportedProduct => Factory.GetCachedValue<EXDOCYesNoEmpty>();

		public EXDOCApprovedCertifierCollection EXDOCApprovedCertifiers
		{
			get
			{
				if (approvedCertifiers == null)
				{
					approvedCertifiers = new EXDOCApprovedCertifierCollection(parent);
					approvedCertifiers.Load();
				}
				return approvedCertifiers;
			}
		}
		EXDOCApprovedCertifierCollection approvedCertifiers;

		public CodeDescriptionPairList EXDOCAverageAgeOfAnimalsList => Factory.GetCachedValue<EXDOCAverageAgeOfAnimalsCodes>();

		public CodeDescriptionPairList EXDOCTransitLocationTypeList => Factory.GetCachedValue<EXDOCTransitLocationTypeCodes>();

		readonly QuarantineExDocHeader parent;
		public const string AqisPlaceCode = "AQIS PLACE";
	}
}
