using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class CountryAutoCompleteHelperTest : AutoCompleteHelperTest
	{
		protected override AutoCompleteHelper GetHelper()
		{
			return new CountryAutoCompleteHelper(Factory);
		}
		protected override BusinessObject GetBusinessObject()
		{
			var refCountry = base.GetBusinessObject();
			refCountry[KeyColumn] = "Z" + i++.ToString();
			return refCountry;
		}
		int i;
	}
}
