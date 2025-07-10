using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using CodeList = Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class MessageSendingEntryLineObjectLookups : ZLookups
	{
		public MessageSendingEntryLineObjectLookups(AutoMessageSendingEntryLineObject parent) : base(parent)
		{
		}
		public CodeDescriptionPairList PostClearanceYNCodeList => Factory.GetCachedValue<CodeList.YesNoList>();

		public CodeDescriptionPairList SpecificUseProductTypeList => Factory.GetCachedValue<SpecificUseProductTypeList>();

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public ConsigneeCollection ConsigneeAddress
		{
			get
			{
				if (consigneeAddress == null)
				{
					consigneeAddress = new ConsigneeCollection(Factory);
				}

				return consigneeAddress;
			}
		}
		ConsigneeCollection consigneeAddress;

		public CodeDescriptionPairList DutyReductionClassificationList => Factory.GetCachedValue<ImportDutyReductionClassificationList>();

		public CodeDescriptionPairList ReductionRateRegulationList => Factory.GetCachedValue<ImportReductionRateRegulationList>();

		public ICollection GoodsDestination => new RefCountryCollection(Factory);

		public TariffViewCollection SecondaryPreferences => TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DutyReductionExemption, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection InstallmentCodes => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.InstallmentCode, ZDateTime.Today);

		public CodeDescriptionPairList NetWeightUnitList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public ICollection CountryOfOrigins => new RefCountryCollection(Factory);

		public UntranslatableCodeDescriptionPairList PreferenceCodeList
		{
			get
			{
				var codes = new UntranslatableCodeDescriptionPairList(nameof(PreferenceCodeList));
				codes.AddPair(Parent.Preference, Parent.PreferenceDescription);
				return codes;
			}
		}
		public CodeDescriptionPairList COOProductTypeList => Factory.GetCachedValue<CertificateOfOriginIssuedCodeList>();

		public CodeDescriptionPairList ThirdCountryAdditionalInvoiceIssuedYNCodeList => Factory.GetCachedValue<CodeList.YesNoList>();

		public CodeDescriptionPairList COOSupportingDocTypeList => Factory.GetCachedValue<CountryOfOriginSupportingDocTypeCodeList>();

		public CodeDescriptionPairList COOIssuerTypeList => Factory.GetCachedValue<CertifiticateOfOriginIssuedTypeList>();

		new MessageSendingEntryLineObject Parent => (MessageSendingEntryLineObject)base.Parent;
	}
}

