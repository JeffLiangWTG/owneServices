using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DRWBCKMessageBuilderTest : CMRMessageBuilderAbstractTest
	{
		public void TestOriginalDRWBCKMessage()
		{
			var expectedMessage = CMRDrawbackMessageTestData.DRWBCKOriginal;
			var builder = new DRWBCKMessageBuilder(declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			var result = builder.MessageText
				.Replace("<<MSGNO PLACEHOLDER>>", "1")
				.Replace("<<SENDERS REFERENCE PLACE HOLDER>>", "B00001142")
				.Replace("<<OWNER REFERENCE PLACE HOLDER>>", "OWNERS REF")
				.Replace("<<AGENT REFERENCE PLACE HOLDER>>", "TESTER DRAWBACK");
			AssertEquals("MessageString", expectedMessage, result);
		}

		protected override CMRMessageBuilder GetMessageBuilderToTest() => new DRWBCKMessageBuilder(declaration);

		protected override void SetUp()
		{
			base.SetUp();

			var brokerAccount = Factory.New<AccBankAccount>();
			brokerAccount.AB_BSB = "242200";
			brokerAccount.AB_AccountNum = "323232";
			brokerAccount.AB_BankAccountName = "EDI CUSTOMS BROKERS";
			Env.Registry.SetCustomsPaymentBankAccountForCurrentCompany(brokerAccount.PK.ToGuid());

			GlbStaff.CurrentUser.GS_FullName = "FRED";
			GlbStaff.CurrentUser.GS_WorkPhone = "07 3268 2903";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AA33HF";

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001142";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.DeclarationNumber = "AAACFLYXE";
			declaration.JE_OwnerRef = "OWNERS REF";
			declaration.JE_AgentsReference = "TESTER DRAWBACK";
			declaration.DrawbackHeaderAssesmentMethod = "A";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			var claimant = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = claimant.PK;
			claimant.OH_FullName = "FRED BLOGGS";
			claimant.LocalBusinessRegNo = "66015286036";
			var jobComInvHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var jobComInvLine = jobComInvHeader.JobComInvoiceLines.AddNew();
			jobComInvLine.JI_Description = "TEST PRODUCT 1";
			jobComInvLine.JI_Tariff = "7318.22.00 99";
			jobComInvLine.JI_CustomsUnitQty = "NO";
			jobComInvLine.JI_CustomsQuantity = 300m;
			jobComInvLine.AddInfo.ZA_DCV_Hidden = 3952.57m;
			jobComInvLine.AddInfo.ZA_DDN_Hidden = "AAACEFCKA";
			jobComInvLine.AddInfo.ZA_DDL_Hidden = 1;
			jobComInvLine.AddInfo.ZA_DAM_Hidden = "A";
			jobComInvLine.AddInfo.ZA_EDN_Hidden = "AAACFL64K";
			jobComInvLine.AddInfo.ZA_DTR_Hidden = 5m;
			jobComInvLine.AddInfo.ZA_DDT_Hidden = 197.62m;
		}
		JobDeclaration declaration;
	}
}
