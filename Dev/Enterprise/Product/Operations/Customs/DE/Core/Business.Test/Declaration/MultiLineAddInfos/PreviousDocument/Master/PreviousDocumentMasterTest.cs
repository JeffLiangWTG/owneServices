using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(PreviousDocumentMaster))]
	class PreviousDocumentMasterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCSI_ReferenceNumber2MaxLength()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			AssertEquals(17, master.CSI_ReferenceNumber2Info.MaxLength);

			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			AssertEquals(35, master.CSI_ReferenceNumber2Info.MaxLength);
		}

		public void TestAuthorizationNumberMaxLength()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			AssertEquals(35, master.AuthorizationNumberInfo.MaxLength);
		}

		public void TestSubType()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			AssertEquals("Default value should be false", false, master.SimplifiedGrantAuthorizationFlag);
		}

		public void TestAuthorizationNumberReadOnlyAndBlankWhenSubTypeIsTrue_ATAV()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			master.SimplifiedGrantAuthorizationFlag = false;
			master.AuthorizationNumber = "Test";
			master.SimplifiedGrantAuthorizationFlag = true;
			CombineAssertions("When SimplifiedGrantAuthorizationFlag is true...", () =>
			{
				AssertEquals("Authorization Number should be cleared", ZString.Empty, master.AuthorizationNumber);
				AssertEquals("", true, master.AuthorizationNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_Procedure_ThatOnlyRequiresACode()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._OHNE;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			AssertEquals(PreviousProcedureList.Codes._OHNE, previousDocument.CSI_Procedure);
		}

		public void TestCSI_Procedure_ThatAllowsMultipleWithDefaultsStatus()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._ATA;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("Default from the Header Code", PreviousProcedureList.Codes._ATA, previousDocument.CSI_Procedure);
				AssertEquals("Default status for a new document", YesNoList.Codes.Yes, previousDocument.CSI_Status);
				var additonalPreviousDocument = previousDocuments.AddNew();
				AssertEquals("Additonal Document defaults from the Header Code", PreviousProcedureList.Codes._ATA, previousDocument.CSI_Procedure);
				AssertEquals("Status default also occurs on additional", YesNoList.Codes.Yes, previousDocument.CSI_Status);
			});
		}

		public void TestCSI_Procedure_FromMultipleToSingleToBlank()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Multiple Previous Documents", 2, previousDocuments.Count);
				master.CSI_Procedure = PreviousProcedureList.Codes._OHNE;
				AssertEquals("Single Document Code", 1, previousDocuments.Count);
				master.CSI_Procedure = ZString.Empty;
				AssertEquals("No code collection empty", 0, previousDocuments.Count);
			});
		}

		public void TestCSI_Procedure_Cancellation()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._A;
			declaration.OnPreviousDocumentMasterCSI_ProcedureAboutToChange += canceledFunction;
			master.CSI_Procedure = PreviousProcedureList.Codes._V;

			CombineAssertions(() =>
			{
				AssertEquals("Should not have set the value, as the change was canceled.", PreviousProcedureList.Codes._A, master.CSI_Procedure);
				declaration.OnPreviousDocumentMasterCSI_ProcedureAboutToChange -= canceledFunction;
				declaration.OnPreviousDocumentMasterCSI_ProcedureAboutToChange += notCanceledFunction;
				master.CSI_Procedure = PreviousProcedureList.Codes._V;
				AssertEquals("Should have set the value, as the change was not canceled.", PreviousProcedureList.Codes._V, master.CSI_Procedure);
			});
			declaration.OnPreviousDocumentMasterCSI_ProcedureAboutToChange -= notCanceledFunction;

			void canceledFunction(object sender, CancelEventArgs args) => args.Cancel = true;
			void notCanceledFunction(object sender, CancelEventArgs args) => args.Cancel = false;
		}

		public void TestCSI_ReferenceNumber2()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			master.CSI_ReferenceNumber2 = "DOCLOCREF";
			var prevDocument1 = previousDocuments.AddNew();
			var prevDocument2 = previousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Previous document 1 linked to Invoice Header Header Value", "DOCLOCREF", prevDocument1.CSI_ReferenceNumber2);
				AssertEquals("Previous document 2 linked to Invoice Header Header Value", "DOCLOCREF", prevDocument2.CSI_ReferenceNumber2);

				master.CSI_ReferenceNumber2 = "DOCLOCREF2";
				AssertEquals("Reference Number update from header for document 1", "DOCLOCREF2", prevDocument1.CSI_ReferenceNumber2);
				AssertEquals("Reference Number update from header for document 2", "DOCLOCREF2", prevDocument2.CSI_ReferenceNumber2);

				master.CSI_Procedure = PreviousProcedureList.Codes._OHNE;
				var prevDocument3 = previousDocuments.AddNew();
				AssertEquals("procedure code has changed no default", ZString.Empty, prevDocument3.CSI_ReferenceNumber2);
			});
		}

		public void TestAuthorizationNumber_DefaultFromOrgForCode_ATAV()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEFAULT1");
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			AssertEquals("DEFAULT1", instruction.GetOnlyPreviousDocument().AuthorizationNumber);
		}

		public void TestAuthorizationNumber_DefaultFromOrgForCode_ATAV_EXP()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEFAULT1");
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			AssertEquals("DEFAULT1", instruction.GetOnlyPreviousDocument().AuthorizationNumber);
		}

		public void TestPreviousDocumentMaster_AuthorizationNumber()
		{
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			master.AuthorizationNumber = "DOCAUTHNU";
			var prevDocument1 = previousDocuments.AddNew();
			var prevDocument2 = previousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Previous document 1 linked to Invoice Header Header Value", "DOCAUTHNU", prevDocument1.AuthorizationNumber);
				AssertEquals("Previous document 2 linked to Invoice Header Header Value", "DOCAUTHNU", prevDocument2.AuthorizationNumber);

				master.AuthorizationNumber = "DOCAUTHNU2";
				AssertEquals("Authorization Number update from header for document 1", "DOCAUTHNU2", prevDocument1.AuthorizationNumber);
				AssertEquals("Authorization Number update from header for document 2", "DOCAUTHNU2", prevDocument2.AuthorizationNumber);

				master.CSI_Procedure = PreviousProcedureList.Codes._OHNE;
				var prevDocument3 = previousDocuments.AddNew();
				AssertEquals("procedure code has changed no default", ZString.Empty, prevDocument3.AuthorizationNumber);
			});
		}

		public void TestAuthorizationNumber_DefaultFromAuthorizationForCode_ATZL()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEFAULT2");
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			AssertEquals("DEFAULT2", instruction.GetOnlyPreviousDocument().AuthorizationNumber);
		}

		protected override BusinessObject GetNewBusinessObject() => master;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction = declaration.CustomsEntryInstructions.AddNew();
			master = instruction.PreviousDocumentMaster;
			previousDocuments = instruction.PreviousDocuments;
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
		PreviousDocumentMaster master;
		PreviousDocumentCollection previousDocuments;
	}
}
