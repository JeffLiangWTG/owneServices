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
	[TestedType(typeof(EntryDetailsFor5SGController))]
	sealed class EntryDetailsFor5SGControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.KR.EntryDetailsFor5SG;

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

			var orgHeaderImporter = Factory.New<OrgHeader>();
			orgHeaderImporter.OH_Category = "BUS";
			orgHeaderImporter.OH_Code = "Importer RK3";
			orgHeaderImporter.OH_FullName = "RK Importer TestData3";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_DutyPayer = orgHeaderPayer.PK;
			declaration.JE_OH_Importer = orgHeaderImporter.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ProvAdditionalRate = 98;
			invoice.JZ_ProvAdditionalAmount = 180;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_TotalPaid = 200;
			entry.CH_EntryReleaseDate = ZDateTime.Today;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "1234522123452M";
			entryNum.CE_IssueDate = new ZDateTime("2022-01-03");

			entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "934";
			entryNum.CE_ExpiryDate = ZDateTime.Today;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 300;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();

			return Factory.LoadTop1<KREntryHeaderDetailsView>(new ZQuery(KREntryHeaderDetailsViewSchema.KEH_JE_PK, declaration.PK));
		}
	}
}
