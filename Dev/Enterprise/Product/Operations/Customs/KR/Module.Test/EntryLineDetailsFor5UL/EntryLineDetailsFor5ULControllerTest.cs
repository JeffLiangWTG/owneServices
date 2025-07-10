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
	[TestedType(typeof(EntryLineDetailsFor5ULController))]
	sealed class EntryLineDetailsFor5ULControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.KR.EntryLineDetailsFor5UL;

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
			var orgHeaderPayer = Factory.New<OrgHeader>();
			orgHeaderPayer.OH_Category = "BUS";
			orgHeaderPayer.OH_Code = "Payer RK3";
			orgHeaderPayer.OH_FullName = "RK Payer TestData3";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_DutyPayer = orgHeaderPayer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "1234522123452M";
			entryNum.CE_IssueDate = new ZDateTime("2022-01-03");

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "0102399000";
			entryLine.CL_CustomsValue = 100m;
			entryLine.CL_Description = "STAINLESS STEEL";
			entryLine.CL_ValueForVAT = 200m;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();

			var query = new ZQuery(KREntryLineDetailsViewSchema.KEL_LineNumber, entryLine.CL_LineNumber);
			query.AddToFilter(KREntryLineDetailsViewSchema.KEL_EntryNum, entryNum.CE_EntryNum);
			return Factory.LoadTop1<KREntryLineDetailsView>(query);
		}
	}
}
