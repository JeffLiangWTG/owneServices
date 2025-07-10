using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryCustomsBillsFor5ULController))]
	sealed class EntryCustomsBillsFor5ULControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.KR.EntryCustomsBillsFor5UL;

		public override void TestEditForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestViewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestNewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert("Not Implemented", true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var payer = Factory.New<OrgHeader>();
			payer.OH_Category = "BUS";
			payer.OH_Code = "RK Payer";
			payer.OH_FullName = "RK Payer Test";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryReleaseDate = new ZDateTime(2024, 08, 01);

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "1234522123450X";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = payer.PK;
			statement.B2_StatementType = "D";
			statement.B2_PaymentStatus = "PYC";
			statement.B2_StatementNumber = "1111111111111111111";
			statement.B2_PaymentAuthorizationDate = new ZDateTime(2024, 09, 01);
			statement.B2_PrintDate = new ZDateTime(2024, 10, 01);
			statement.B2_ProcessDate = new ZDateTime(2024, 11, 01);
			statement.B2_DueDate = new ZDateTime(2024, 12, 01);

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = entryNum.CE_EntryNum;
			statementLine.B3_AssociatedEntry = "2222222222222222222";
			statementLine.B3_CustomsFeesTotal = 1234500m;

			Factory.Save();

			var query = new ZQuery(KREntryCustomsBillsViewSchema.KEB_ImportEntryNum, entryNum.CE_EntryNum);
			query.AddToFilter(KREntryCustomsBillsViewSchema.KEB_CustomsDisbursementBillNumber, statement.B2_StatementNumber);
			return Factory.LoadTop1<KREntryCustomsBillsView>(query);
		}
	}
}
