using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Contract))]
	sealed class ContractTest : CusCodeDataTest<Contract>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.Contract, contract.CY_Type);
		}

		public void TestValidation()
		{
			AssertType<ContractValidation>(contract.Validation);
		}

		public void TestCY_Data()
		{
			AssertEquals(40, contract.CY_DataInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return contract;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<Contract>();

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			contract = (Contract)GetNewBusinessObject();
			contract.Parent = invoice;
			Factory.Save();
		}
		Contract contract;
	}
}
