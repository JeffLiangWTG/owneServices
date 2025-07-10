using System.Collections;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GVMS
{
	public abstract class GvmsItemReferenceLookups : CusSupportingInfoLookups
	{
		public GvmsItemReferenceLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList => Factory.GetCachedValue($"{GetType().FullName}.CodeList", () =>
		{
			var codePairList = Factory.GetCachedValue<GVMSCustomsReference>();
			var result = new CodeDescriptionPairList();
			foreach (var code in ValidCodeList)
			{
				result.AddPair(code, codePairList.GetDescriptionFromCode(code));
			}
			return result;
		});

		public CodeDescriptionPairList YesNoList => new YesNoList();

		protected abstract IEnumerable<string> ValidCodeList { get; }
	}
}
