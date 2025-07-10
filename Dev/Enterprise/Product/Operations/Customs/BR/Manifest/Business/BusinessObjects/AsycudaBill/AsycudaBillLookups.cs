using System.Collections;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList DocumentTypeList => Factory.GetCachedValue<BRBillDocumentTypeList>();

		public override ICollection Locations => Parent.Header.AMA_ManifestType != BRManifestTypes.Codes.MER
					? base.Locations
					: AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Parent.Header.Factory, CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DischargePortTerminalOperator);

		public CodeDescriptionPairList FRTModes => Factory.GetCachedValue<FRTModeList>();
	}
}
