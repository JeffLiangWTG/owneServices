using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocEstablishmentAndTimeLookups : AutoQuarantineExDocEstablishmentAndTimeLookups
	{
		public QuarantineExDocEstablishmentAndTimeLookups(AutoQuarantineExDocEstablishmentAndTime parent)
			: base(parent)
		{
		}

		protected new QuarantineExDocEstablishmentAndTime Parent => (QuarantineExDocEstablishmentAndTime)base.Parent;
		protected QuarantineExDocHeader QuarantineExDocHeader => Parent.QuarantineExDocHeader;

		public CodeDescriptionPairList ProcessingType
		{
			get
			{
				var isNEXDOCSActive = QuarantineExDocHeader != null && QuarantineExDocHeader.IsNEXDOCSActive;
				var isProduceTypeFish = Parent.QuarantineExDocLine != null && Parent.QuarantineExDocLine.QL_ProduceType == EXDOCCommodityCodes.Codes.Fish;
				var key = string.Format(CultureInfo.CurrentCulture, "QuarantineExDocEstablishmentAndTimeLookups|{0}|{1}", isNEXDOCSActive, isProduceTypeFish);

				return Factory.GetCachedValue(key, () => GetProcessingTypes(isNEXDOCSActive, isProduceTypeFish));
			}
		}

		CodeDescriptionPairList GetProcessingTypes(bool isNEXDOCSActive, bool isProduceTypeFish)
		{
			var result = new EXDOCProcessTypeCodes();
			if (!isNEXDOCSActive)
			{
				result.RemoveNEXDOCTypes();
			}
			if (isProduceTypeFish)
			{
				result.AddRange(new EXDOCProcessTypeCodesFish());
			}
			result.Sort();
			return result;
		}

		public virtual CodeDescriptionPairList EstablishmentIndicatorList => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList EstablishmentPostedStatusList => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList TreatmentCode => new CodeDescriptionPairList();

		public OrgHeaderCollectionWithSpecificRegistrationCodes AuthorisationEstablishment
		{
			get
			{
				return Factory.GetCachedValue("QuarantineExDocEstablishmentAndTimeLookups|AuthorisationEstablishment", () =>
				{
					var result = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber);
					result.SetOverrideNotificationWhenAdditionalFilterNotMet(OrgHeaderCollectionWithSpecificRegistrationCodes.OrgDoesNotHaveRegNo("(ESN) EXDOC Establishment Number"));
					return result;
				});
			}
		}

		public override JobDocAddressCollection Addresses
		{
			get
			{
				var result = new JobDocAddressCollection(Factory);
				var declaration = Parent?.JobDeclaration;
				if (declaration != null)
				{
					var addresses = declaration.Shipment?.DocAddresses ?? declaration.DocAddresses;
					result.AddRange(addresses.FindDocAddressesByType(DocAddressType.AQISProcessingEstablishment).Where(address => !address.IsEmpty));
				}
				return result;
			}
		}

		public virtual CodeDescriptionPairList TreatmentDurationUQ => Factory.GetCachedValue<EXDOCTreatmentDurationCodeList>();

		public virtual CodeDescriptionPairList TreatmentTemperatureUQ => Factory.GetCachedValue<EXDOCTemperatureUnitCodes>();

		public virtual CodeDescriptionPairList TreatmentConcentrationUQ => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TreatmentConcentration, ZDateTime.Today);
	}
}
