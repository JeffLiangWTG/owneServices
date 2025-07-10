using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACFIAMiscCodes))]
	sealed class CACFIAMiscCodesTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CACFIAMiscCodes(Factory.New<ZZRefCusCodeListCombined>());
		}

		public void TestHumanReadableName()
		{
			var fiaMiscCodes = CreateFIAMiscCodes(Factory, "1234");
			AssertEquals("HumanReadableName", "Miscellaneous Code: '1234'", fiaMiscCodes.HumanReadableName);
		}

		ZZRefCusCodeListCombined CreateRefCusCodeListForCACFIAMiscCodes(BusinessObjectFactory factory, ZString code, string description = "")
		{
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			cusCodeList.ZZD_CodeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CFIAMiscCodes;
			cusCodeList.ZZD_Code = code;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);

			return cusCodeList;
		}

		public CACFIAMiscCodes CreateFIAMiscCodes(BusinessObjectFactory factory, ZString code, string description = "")
		{
			return new CACFIAMiscCodes(CreateRefCusCodeListForCACFIAMiscCodes(factory, code, description));
		}
	}
}
