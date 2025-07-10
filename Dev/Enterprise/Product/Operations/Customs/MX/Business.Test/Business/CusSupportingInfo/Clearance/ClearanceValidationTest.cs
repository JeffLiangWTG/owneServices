using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MX.Business.Testing
{
	public class ClearanceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			clearance.CSI_Code = "XXXX";
			AssertHasMessageError(clearance.CSI_CodeInfo, "Patent allows numeric characters.");
			clearance.CSI_Code = "1234";
			AssertNoMessageError(clearance.CSI_CodeInfo, "Patent allows numeric characters.");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			clearance.CSI_ReferenceNumber = "XXXXXXX";
			AssertHasMessageError(clearance.CSI_ReferenceNumberInfo, "Entry Number allows numeric characters.");
			clearance.CSI_ReferenceNumber = "1234567";
			AssertNoMessageError(clearance.CSI_ReferenceNumberInfo, "Entry Number allows numeric characters.");
		}

		public void TestCheckCSI_CustomsOffice()
		{
			clearance.CSI_CustomsOffice = "XXX";
			AssertHasMessageError(clearance.CSI_CustomsOfficeInfo, "Customs Area allows numeric characters.");
			clearance.CSI_CustomsOffice = "123";
			AssertNoMessageError(clearance.CSI_CustomsOfficeInfo, "Customs Area allows numeric characters.");
		}

		public void TestCheckCSI_Tariff()
		{
			clearance.CSI_Tariff = "XXXXXXXX";
			AssertHasMessageError(clearance.CSI_TariffInfo, "Tariff allows numeric characters.");
			clearance.CSI_Tariff = "12345678";
			AssertNoMessageError(clearance.CSI_TariffInfo, "Tariff allows numeric characters.");
		}

		public void TestCheckCSI_UnitOfQuantity()
		{
			clearance.CSI_UnitOfQuantity = "XX";
			AssertHasMessageError(clearance.CSI_UnitOfQuantityInfo, "UQ allows numeric characters.");
			clearance.CSI_UnitOfQuantity = "12";
			AssertNoMessageError(clearance.CSI_UnitOfQuantityInfo, "UQ allows numeric characters.");
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			clearance.CSI_ReferenceNumber2 = "XX";
			AssertHasMessageError(clearance.CSI_ReferenceNumber2Info, "Validation Year allows numeric characters.");
			clearance.CSI_ReferenceNumber2 = "12";
			AssertNoMessageError(clearance.CSI_ReferenceNumber2Info, "Validation Year allows numeric characters.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			clearance = entryInstruction.Clearances.AddNew();
		}

		Clearance clearance;
	}
}
