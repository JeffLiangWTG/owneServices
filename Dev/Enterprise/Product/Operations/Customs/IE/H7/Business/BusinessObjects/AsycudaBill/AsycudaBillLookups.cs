using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AsycudaBillLookups : EU.H7.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		protected override ICollection CountryList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
			CountryCodes.Ireland,
			UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI008,
			ZDateTime.Today);

		public CodeDescriptionPairList SubStyleList => Factory.GetCachedValue<SubStyleCodeList>();

		public CodeDescriptionPairList BillStatusList => Factory.GetCachedValue<AISEntryStatusList>();

		public CodeDescriptionPairList ImporterIdentificationTypeList => Factory.GetCachedValue<ImporterIdentificationTypes>();

		public new CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<LogicalStatusList>();
	}
}
