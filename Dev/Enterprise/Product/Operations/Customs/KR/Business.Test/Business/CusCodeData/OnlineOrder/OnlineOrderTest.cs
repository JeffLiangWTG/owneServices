using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(OnlineOrder))]
	sealed class OnlineOrderTest : CusCodeDataTest<OnlineOrder>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.OnlineOrder, onlineOrder.CY_Type);
		}

		public void TestValidation()
		{
			AssertType<OnlineOrderValidation>(onlineOrder.Validation);
		}

		public void TestCY_Data()
		{
			onlineOrder.CY_Data = "12345678";
			AssertEquals("12345678", onlineOrder.CY_Data);
			AssertEquals(35, onlineOrder.CY_DataInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return onlineOrder;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<OnlineOrder>();

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			onlineOrder = entryInstruction.OnlineOrders.AddNew();
			Factory.Save();
		}
		OnlineOrder onlineOrder;
	}
}
