using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class OnlineOrderValidationTest : CusCodeDataValidationTest
	{
		public void TestOnlineOrderNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var onlineOrder = entryInstruction.OnlineOrders.AddNew();
			onlineOrder.CY_Order = 1;
			onlineOrder.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(onlineOrder.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

			onlineOrder.CY_Data = "22222";
			AssertNoMessageErrorContaining(onlineOrder.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
