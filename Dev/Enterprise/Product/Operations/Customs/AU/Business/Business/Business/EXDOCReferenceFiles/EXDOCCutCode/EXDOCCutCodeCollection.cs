using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCCutCodeCollection : EXDOCRefCodeCollection
	{
		public EXDOCCutCodeCollection(IEXDOCRefCodeTypeProvider typeProvider)
			: base(typeProvider, "CUTC")
		{
		}

		protected override ZString GetCodeTypeCore()
		{
			var singleCharCommodityCode = (ZString)EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(typeProvider?.Type);
			if (!singleCharCommodityCode.IsEmpty)
			{
				return baseCodeType + singleCharCommodityCode;
			}

			return null;
		}

		public const string AttributeIsBeefVeal = "IsBeefVeal";
		public const string AttributeIsChemicalLean = "IsChemicalLean";
		public const string AttributeBoneInIndicator = "BoneInIndicator";
	}
}
