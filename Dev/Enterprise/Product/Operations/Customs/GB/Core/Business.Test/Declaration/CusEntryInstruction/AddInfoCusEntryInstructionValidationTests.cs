using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class AddInfoCusEntryInstructionValidationTests : TestCaseWithFactory
	{
		public void TestCEI_SplitReference()
		{
			var msgError = "Split Reference must be blank or 2 numeric digits.";
			var dec = Factory.New<JobDeclaration>();

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;

			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_SplitReference = "";
			AssertNoMessageErrorContaining(cei.CEI_SplitReferenceInfo, msgError);

			cei.CEI_SplitReference = "01";
			AssertNoMessageErrorContaining(cei.CEI_SplitReferenceInfo, msgError);

			cei.CEI_SplitReference = "1A";
			AssertHasMessageErrorContaining(cei.CEI_SplitReferenceInfo, msgError);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.ZG_Gateway = "";

			cei.CEI_SplitReference = "ZZ";
			AssertNoMessageErrorContaining(cei.CEI_SplitReferenceInfo, msgError);
		}
	}
}
