using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business
{
	public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCached(ref typeCodeDescriptionPairList, () =>
		{
			var codeList = new GBGuaranteeTypeList();
			codeList.RemoveCode(EUGuaranteeTypeList.Codes.TST);
			return codeList;
		});
		CachedProperty<CodeDescriptionPairList> typeCodeDescriptionPairList;
	}
}
