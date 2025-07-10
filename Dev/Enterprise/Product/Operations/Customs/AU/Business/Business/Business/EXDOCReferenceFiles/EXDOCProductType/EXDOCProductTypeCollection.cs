using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCProductTypeCollection : EXDOCRefCodeCollection
	{
		public EXDOCProductTypeCollection(IEXDOCRefCodeTypeProvider typeProvider)
			: base(typeProvider, "PROD")
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
