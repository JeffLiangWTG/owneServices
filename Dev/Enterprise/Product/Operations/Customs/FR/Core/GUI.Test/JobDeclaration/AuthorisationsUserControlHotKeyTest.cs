#if !WINZOR
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.GUI.NCTS;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class AuthorisationsUserControlHotKeyTest : TestCaseWithFactory
	{
		public void TestF5KeyDownOnEntryInstructionAuthorisationShowsDialog()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.First();
			entryInstruction.CusAuthorizationUsages.AddNew();
			Env.Security.FlagAuthorisationAdHoc.IsAllowed = true;
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionAuthorisationsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetAuthorizationAsActiveControl();
				control.SetAuthorizationAsActiveControl();
				EmulateF5KeyDown(control);
				AssertType<CusAuthorisationForm>("A temporary authorisation form should show.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestF5KeyDownOnInvoiceLineAuthorisationShowsDialog()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var authorisationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
			Env.Security.FlagAuthorisationAdHoc.IsAllowed = true;
			using (var form = new ZForm(declaration))
			using (var control = new InvoiceLineAuthorisationsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetAuthorizationAsActiveControl();
				control.SetAuthorizationAsActiveControl();
				EmulateF5KeyDown(control);
				AssertType<CusAuthorisationForm>("A temporary authorisation form should show.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestF3KeyDownOnAuthorisedLocationCodeLeadsToFilteredAuthorisationsPhase4()
		{
			var authorisationHolder = Factory.New<OrgHeader>();
			authorisationHolder.OH_Code = "SJCORP";

			var authorisationHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345";
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			authorisationHeader.CPH_OH_PermitHolder = authorisationHolder.PK;

			var authorisationHolder2 = Factory.New<OrgHeader>();
			authorisationHolder2.OH_Code = "BNCORP";

			var authorisationHeader2 = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisationHeader2.CPH_Number = "12345";
			authorisationHeader2.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			authorisationHeader2.CPH_OH_PermitHolder = authorisationHolder2.PK;

			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Consignor.E2_OA_Address = authorisationHolder.MainAddress.PK;
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "12345";

			using (var form = new NctsMovementForm(nctsHeader))
			{
				form.Show();

				var codeFindBox = (ZCodeFindBox)form.Controls.Find("AuthorisedLocationCodeFindBox", true).First();

				KeySender.PostKeyDown(codeFindBox.CodeBox, codeFindBox.CodeBox.Handle, Keys.F3);
				Application.DoEvents();

				AssertType<CusAuthorisationForm>("A temporary authorisation form should show, with authorisation matching both queried authorisation holder and authorisation number.", ZFormModaliser.LastFormShownForTest);
				AssertEquals("Edit Authorization", ZFormModaliser.LastFormShownForTest.Text);
			}
		}

		static void EmulateF5KeyDown(EntryInstructionAuthorisationsUserControlForTest control)
		{
			KeySender.PostKeyDown(control.AuthorisationsGrid, control.AuthorisationsGrid.Handle, Keys.F5);
			Application.DoEvents();
		}

		static void EmulateF5KeyDown(InvoiceLineAuthorisationsUserControlForTest control)
		{
			KeySender.PostKeyDown(control.AuthorisationsGrid, control.AuthorisationsGrid.Handle, Keys.F5);
			Application.DoEvents();
		}

		public class InvoiceLineAuthorisationsUserControlForTest : InvoiceLineAuthorisationsUserControl
		{
			public new ZGrid AuthorisationsGrid => base.AuthorisationsGrid;

			public void SetAuthorizationAsActiveControl()
			{
				ActiveControl = AuthorisationsGrid.Controls.OfType<TextBox>().ToArray().First(x => x.Name == "AGC_CPH_Authorization");
			}
		}

		public class EntryInstructionAuthorisationsUserControlForTest : EntryInstructionAuthorisationsUserControl
		{
			public new ZGrid AuthorisationsGrid => base.AuthorisationsGrid;

			public void SetAuthorizationAsActiveControl()
			{
				ActiveControl = AuthorisationsGrid.Controls.OfType<TextBox>().ToArray().First(x => x.Name == "AGC_CPH_Authorization");
			}
		}
	}
}
#endif
