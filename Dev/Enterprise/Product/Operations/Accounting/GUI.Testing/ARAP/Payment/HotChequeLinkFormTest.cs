using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(HotChequeLinkForm))]
	public class HotChequeLinkFormTest : ZFormBasherTest
	{
		#region Test Class

		public class MockHotChequeLinkForm : HotChequeLinkForm
		{
			public MockHotChequeLinkForm(HotChequeLink chequeLink) : base(chequeLink)
			{
			}

			public ZDisplayGrid HotChequeGrid_Exposed
			{
				get { return HotChequesGrid; }
			}

			public void HandleDoubleClick_Exposed(object sender, EventArgs e)
			{
				HandleDoubleClick(sender, e);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			HotCheques = new AccHotChequeCollection(Factory);
		}

		protected override Form GetFormToBashCore()
		{
			return new HotChequeLinkForm(new HotChequeLink(HotCheques));
		}

		AccHotCheque GetHotCheque(OrgHeader org, AccChequeBook chequeBook)
		{
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_OH = org.PK;
			hotCheque.AQ_AK = chequeBook.PK;
			return hotCheque;
		}

		AccChequeBook ChequeBook;
		AccHotChequeCollection HotCheques;
		OrgHeader Org;

		#endregion

		#region HandleDoubleClick Test

		public void TestHandleDoubleClick()
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			ChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 100;

			AccHotCheque hotCheque = GetHotCheque(Org, ChequeBook);
			hotCheque.AQ_Cancelled = false;
			hotCheque.AQ_ChequeNumber = "1";

			AccHotCheque hotCheque2 = GetHotCheque(Org, ChequeBook);
			hotCheque2.AQ_Cancelled = false;
			hotCheque2.AQ_ChequeNumber = "2";

			HotCheques.Add(hotCheque);
			HotCheques.Add(hotCheque2);

			using (HotChequeLinkForm form = (HotChequeLinkForm)GetFormToBashCore())
			{
				form.Show();
				form.HotChequeLink.HotCheques.Sort(AccHotChequeSchema.AQ_ChequeNumber.Name, ListSortDirection.Descending);
				AssertEquals("There should be 2 hot cheques in the collection", 2, form.HotChequeLink.HotCheques.Count);
				form.HotChequesGrid_ForTestOnly.ListManager.Position = 1;
				form.HandleDoubleClick_ForTestOnly(this, EventArgs.Empty);
				AssertEquals("SelectedHotCheque should be HotCheque", hotCheque.PK, form.SelectedHotCheque.PK);
				Assert("Form should close", !form.Visible);
			}
		}

		#endregion

		#region SelectButton Test

		public void TestSelectButton_Click()
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			ChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 100;

			AccHotCheque hotCheque = GetHotCheque(Org, ChequeBook);
			hotCheque.AQ_Amount = 10m;
			hotCheque.AQ_ChequeNumber = "000030";

			AccHotCheque hotCheque2 = GetHotCheque(Org, ChequeBook);
			hotCheque2.AQ_Amount = 20m;
			hotCheque2.AQ_ChequeNumber = "000040";

			HotCheques.Add(hotCheque);
			HotCheques.Add(hotCheque2);

			using (HotChequeLinkForm form = (HotChequeLinkForm)GetFormToBashCore())
			{
				form.Show();
				HotCheques.Sort(AccHotChequeSchema.AQ_ChequeNumber.Name, ListSortDirection.Descending);
				form.HotChequesGrid_ForTestOnly.ListManager.Position = 1;
				form.SelectButton_Click_ForTestOnly(form, EventArgs.Empty);
				AssertEquals("SelectedHotCheque should be set", hotCheque, form.FSelectedHotCheque_ForTestOnly);
				Assert("Form should be closed and not visible", !form.Visible);
			}
		}

		#endregion

		#region EnterKeyPressed Test

		public void TestEnterKeyPressed()
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			ChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 100;

			AccHotCheque hotCheque = GetHotCheque(Org, ChequeBook);
			hotCheque.AQ_ChequeNumber = "000090";
			AccHotCheque hotCheque2 = GetHotCheque(Org, ChequeBook);
			hotCheque2.AQ_ChequeNumber = "000091";

			HotCheques.Add(hotCheque);
			HotCheques.Add(hotCheque2);

			using (HotChequeLinkForm form = (HotChequeLinkForm)GetFormToBashCore())
			{
				form.Show();
				HotCheques.Sort(AccHotChequeSchema.AQ_ChequeNumber.Name, ListSortDirection.Descending);

				form.HotChequesGrid_ForTestOnly.ListManager.Position = 1;
				TestKeyStrokeHelper.SendKeyToControl(form, Keys.Enter, true);
				Assert("Form should close", !form.Visible);
				AssertEquals("HotCheque should be selected", hotCheque, form.SelectedHotCheque);
			}
		}

		#endregion

		#region DoDisplayModeBrowse Test

		public void TestDoDisplayModeBrowse()
		{
			using (HotChequeLinkForm form = (HotChequeLinkForm)GetFormToBashCore())
			{
				form.SelectButton_ForTestOnly.Enabled = false;
				form.SelectButton_ForTestOnly.ReadOnly = true;
				form.SelectButton_ForTestOnly.Text = "Test";
				ZFormStrategy.DoDisplayModeBrowse(form);
				Assert("Select button should be enabled", form.SelectButton_ForTestOnly.Enabled);
				Assert("Select button should not be read only", !form.SelectButton_ForTestOnly.ReadOnly);
				AssertEquals("Select button should read 'select'", "&Select", form.SelectButton_ForTestOnly.Text);
			}
		}

		#endregion
	}
}
