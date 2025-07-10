using System.Collections;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaTransportDocumentInfoLookups : Customs.Business.CusSupportingInfoLookups
	{
		public AsycudaTransportDocumentInfoLookups(AsycudaTransportDocumentInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList
			=> GetCodeListCore();

		ICollection GetCodeListCore()
			=> (string)Parent.CSI_SubType switch
			{
				ILAdditionalInfoSubTypeList.Codes.TransportDocument => Factory.GetCachedValue("IL.Business.CodeDescriptionPairLists.TransportDocsTypeList", () => new TransportDocsTypeList()),
				_ => base.CodeList,
			};

		public CodeDescriptionPairList ConditionList => Factory.GetCachedValue<ILBillConditionList>();
	}
}
