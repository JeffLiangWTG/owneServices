using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryDetailsFor5GWController))]
	sealed class EntryDetailsFor5GWControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.KR.ImportEntryDetails;

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
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Category = "BUS";
			orgHeader.OH_Code = "RK3";
			orgHeader.OH_FullName = "RK TestData3";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = orgHeader.PK;
			invoice.JZ_Weight = 300;
			invoice.JZ_NoOfPacks = 300;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "1234522123452X";
			entryNum.CE_IssueDate = new CargoWise.Types.ZDateTime("2022-01-03");

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 300;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();

			return Factory.LoadTop1<KREntryHeaderDetailsView>(new ZQuery(KREntryHeaderDetailsViewSchema.KEH_JE_PK, declaration.PK));
		}
	}
}
