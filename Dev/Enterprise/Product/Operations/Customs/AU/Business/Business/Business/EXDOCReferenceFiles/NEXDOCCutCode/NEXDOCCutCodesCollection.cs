using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCCutCodesCollection : EXDOCRefCodeCollection
	{
		public NEXDOCCutCodesCollection(IEXDOCRefCodeTypeProvider typeProvider)
			: base(typeProvider, "NCUTC")
		{
			UpdateFilterDefaults();
		}

		protected void UpdateFilterDefaults()
		{
			var filterDefault1 = new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.Description, "Property", ZString.Empty);
			FilterBusinessObjectDefaults.Add(filterDefault1);
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)"NCUTC", false));
		}
	}
}
