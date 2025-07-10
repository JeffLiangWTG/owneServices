using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CompletionCustomsOffice))]
	public class CompletionCustomsOfficeTest : CusCodeDataTest<CompletionCustomsOffice>
	{
		[ExpectNoExceptions]
		public void TestHumanReadableName()
		{
			NUnit.Framework.Assert.That(office.HumanReadableName, Is.EqualTo("Customs Office").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCY_Description()
		{
			office.CY_Data = "DE000001";
			NUnit.Framework.Assert.That(office.CY_OfficeDescription, Is.EqualTo("DE000001 DESC").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			NUnit.Framework.Assert.That(office.CY_Type, Is.EqualTo(CusCodeDataTypeList.Codes.CompletionCustomsOffice).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(office.CY_Code, Is.EqualTo(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			NUnit.Framework.Assert.That(office.Lookups, Is.TypeOf<CompletionCustomsOfficeLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(office.Validation, Is.TypeOf<CompletionCustomsOfficeValidation>());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.CreateInwardProcessingInstruction().CompletionCustomsOffices.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "DE000001 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var instruction = Factory.CreateInwardProcessingInstruction();
			office = instruction.CompletionCustomsOffices.AddNew();
		}

		CompletionCustomsOffice office;
	}
}
