using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCProductTypeCollection : EXDOCRefCodeCollection
	{
		public NEXDOCProductTypeCollection(IEXDOCRefCodeTypeProvider typeProvider)
			: base(typeProvider, "NPRD")
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
	}
}
