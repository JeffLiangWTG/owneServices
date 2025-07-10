using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCSupplementaryCodeCollection : EXDOCRefCodeCollection
	{
		public EXDOCSupplementaryCodeCollection(IEXDOCRefCodeTypeProvider typeProvider)
			: base(typeProvider, "SUPP")
		{
			UpdateFilterDefaults();
		}

		protected void UpdateFilterDefaults()
		{
			var filterDefault = new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", GetAttributeName);  // identifier
			FilterBusinessObjectDefaults.Add(filterDefault);
		}

		IZType GetAttributeName()
		{
			return (ZString)EXDOCCommodityCodeAttributes.GetAttributeNameFromThreeCharCode(typeProvider?.Type);
		}
	}
}
