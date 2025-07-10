using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(VehicleNumber))]
	sealed class VehicleNumberTest : CusCodeDataTest<VehicleNumber>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.VehicleNumber, vehicleNumber.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.VehicleNumber, vehicleNumber.CY_Code);
		}

		public void TestValidation()
		{
			AssertType<VehicleNumberValidation>(vehicleNumber.Validation);
		}

		public void TestCY_Data()
		{
			vehicleNumber.CY_Data = "VIN1234567890";
			AssertEquals("VIN1234567890", vehicleNumber.CY_Data);
			AssertEquals(20, vehicleNumber.CY_DataInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return vehicleNumber;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<VehicleNumber>();

		protected override void SetUp()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			vehicleNumber = invoiceLine.VehicleNumbers.AddNew();
			Factory.Save();
		}
		VehicleNumber vehicleNumber;
	}
}
