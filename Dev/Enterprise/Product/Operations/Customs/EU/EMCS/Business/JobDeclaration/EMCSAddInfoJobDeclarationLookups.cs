using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoJobDeclarationLookups : EUEMCSAddInfoLookups
	{
		public EMCSAddInfoJobDeclarationLookups(EMCSAddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected new EMCSAddInfoJobDeclaration Parent => (EMCSAddInfoJobDeclaration)base.Parent;

		protected EMCSJobDeclaration Declaration => (EMCSJobDeclaration)Parent.Parent;

		public virtual CodeDescriptionPairList DeferredSubmissionList => Factory.GetCachedValue<EMCSDeferredSubmissionList>();

		public virtual CodeDescriptionPairList GuarantorTypeList => RefCusCodeListTypes.GetCachedList(Factory
			, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN
			, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes
			, ZDateTime.Today);

		public CodeDescriptionPairList TransportArrangementList => Factory.GetCachedValue<EMCSTransportArrangementList>();

		public virtual CodeDescriptionPairList OriginTypeList => Factory.GetCachedValue<EMCSOriginTypeList>();

		public CodeDescriptionPairList SubmissionTypeList => Factory.GetCachedValue<EMCSSubmissionTypeList>();

		public CodeDescriptionPairList EuropeanUnionCountryList => Factory.GetEuropeanUnionCountryList();
	}
}
