using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(RefLocalLanguageCollection))]
	sealed class RefLocalLanguageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefLocalLanguageCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var bizO = Factory.New<RefLocalLanguage>();
			bizO.RA_Code = "EN";
			bizO.RA_RN_NKCountryCode = "KN";
			bizO.RA_IsSystem = true;
			bizO.RA_Description = "Something";
			return bizO;
		}
	}
}
