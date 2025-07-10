using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI.Testing
{
	[TestedType(typeof(JASARCreditNoteForm))]
	class JASARCreditNoteFormTest : ARCreditNoteFormTest
	{
		public void TestConstructor()
		{
			var creditNote = GetJASInvoiceWithValidTestData();
			creditNote.FillWithValidTestData();
			Factory.Save();
			using (var form = new JASARCreditNoteFormForTest(creditNote))
			{
				AssertNotNull("JXC Export menu item should exist", form.ActionsMenuItem.MenuItems.FindByText("Export JXC Financial Message"));
			}
		}

		#region IJXCExportForm
		public void TestValidateAll()
		{
			var creditNote = GetJASInvoiceWithValidTestData();
			var line = (InvoicingLineBase)creditNote.Lines.AddNew();
			using (creditNote.SuspendValidationTesting())
			using (line.SuspendValidationTesting())
			using (var form = new JASARCreditNoteFormForTest(creditNote))
			{
				line.AL_Desc = "";
				creditNote.AH_GB = ZGuid.Empty;
				creditNote.ClearAllNotifications();
				line.ClearAllNotifications();
				Assert("Pre-condition", !creditNote.HasNotifications());
				Assert("Pre-condition", !line.HasNotifications());
				form.Show();
				((IJXCExportForm)form).ValidateAll();
				Assert("Should have notifications now", creditNote.HasNotifications());
				Assert("Should have notifications now", line.HasNotifications());
			}
		}

		public void TestBusinessEntity()
		{
			var creditNote = GetJASInvoiceWithValidTestData();
			using (var form = new JASARCreditNoteFormForTest(creditNote))
			{
				var formAsInterface = (IJXCExportForm)form;
				AssertEquals("Should be the ZWinForm.BusinessEntity", creditNote, formAsInterface.BusinessEntity);
			}
		}

		#endregion
		#region Implementation
		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return new JASARCreditNoteForm((JASARCreditNote)invoice)
			{ ControllerID = Enterprise.ZArchitecture.Modules.ControllerIDs.ARCreditNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null) => GetJASInvoiceWithValidTestData(fillTestData: fillTestData, factory: factory);
		JASARCreditNote GetJASInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var creditNote = factory != null ? factory.New<JASARCreditNote>() : Factory.New<JASARCreditNote>();
			if (fillTestData)
			{
				creditNote.FillWithValidTestData();
			}

			return creditNote;
		}
		#endregion
	}
}
