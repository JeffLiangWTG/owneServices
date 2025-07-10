using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DrawbackJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestCheckJE_MergeBy()
		{
			declaration.JE_MergeBy = ZString.Empty;
			AssertNoErrors(declaration.JE_MergeByInfo);
		}

		public void TestOwnerRefValidation()
		{
			declaration.JE_OwnerRef = "";
			AssertHasMessageErrors("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = "qwertyuiopasdfgh";
			AssertNoWarnings("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = "1234567890";
			AssertNoWarnings("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = ".,-()=+:'/";
			AssertNoWarnings("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = "!@#$%^&*";
			AssertHasWarnings("Owner Reference", declaration.JE_OwnerRefInfo);
		}

		public void TestCheckJE_AmberStatement()
		{
			DrawbackJobDeclarationValidation declarationValidation = (DrawbackJobDeclarationValidation)GetNewValidationProvider(declaration);

			declaration.JE_AmberStatement = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_AmberStatementInfo);

			declaration.JE_AmberStatement = "Foo";
			AssertNoMessageErrors(declaration.JE_AmberStatementInfo);

			declaration.DrawbackHeaderAmberReasonCode = JobDeclaration.DrawbackAmberReasonTypes.Declaration;
			declaration.JE_AmberStatement = ZString.Empty;
			AssertHasMessageErrors(declaration.JE_AmberStatementInfo);

			declaration.JE_AmberStatement = "Foo";
			AssertNoMessageErrors(declaration.JE_AmberStatementInfo);

			declaration.JE_AmberStatement = ZString.Replicate('1', 2560);
			AssertNoErrors("Amber Statement", declaration.JE_AmberStatementInfo);

			StmNote amberStatementNote = ((StmNoteCollection)declaration.Notes.GetAllNotes())[0];
			amberStatementNote.ST_NoteDataAsText = ZString.Replicate('1', 2561);
			declarationValidation.ValidateJE_AmberStatement();
			AssertHasErrors("Amber Statement", declaration.JE_AmberStatementInfo);

			declaration.JE_AmberStatement = ZString.Replicate('1', 2560);
			AssertNoErrors("Amber Statement", declaration.JE_AmberStatementInfo);
		}

		public override void TestMergeByForExport()
		{
			Assert("Drawbacks does not merge", true);
		}

		public void TestPaymentMethod()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.DrawbackClaimant;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "Drawback Claimant's Bank details are not complete. Please do the following.\r\nPress F3 in the Drawback Claimant field in Declaration tab and go to Consignee on Organisation form. Fill in BSB and Account Number there.");

			importer.MiscServ.OM_IMEFTBankBSB = "123";
			importer.MiscServ.OM_IMEFTBankAccount = "123456789";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.DrawbackClaimant;
			AssertEquals("No message error", false, declaration.JE_PaymentMethodInfo.HasMessageErrors());

			AccBankAccount brokerAccount = Factory.New<AccBankAccount>();
			brokerAccount.AB_AccountNum = "123456";
			brokerAccount.AB_BSB = "";

			Env.Registry.SetCustomsPaymentBankAccountForCurrentCompany(brokerAccount.PK.ToGuid());
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "Broker's Bank details are not complete. Please do the following.\r\nGo to Registry -> Customs -> Australia -> Payment Bank Account and select a bank account and fill in BSB and Account Number.");

			brokerAccount.AB_BSB = "123";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertEquals("No message error", false, declaration.JE_PaymentMethodInfo.HasMessageErrors());
		}

		#region Implementation
		protected override JobDeclarationValidation GetNewValidationProvider(JobDeclaration jobDeclaration)
		{
			return new DrawbackJobDeclarationValidation(jobDeclaration);
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}
		#endregion
	}
}
