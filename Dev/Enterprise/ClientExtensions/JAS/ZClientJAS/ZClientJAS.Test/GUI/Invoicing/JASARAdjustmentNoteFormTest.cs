using System.Windows.Forms;
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
	[TestedType(typeof(JASARAdjustmentNoteForm))]
	internal class JASARAdjustmentNoteFormTest : ARAdjustmentNoteFormTest
	{
		public void TestConstructor()
		{
			var adjustmentNote = GetJASInvoiceWithValidTestData();
			adjustmentNote.FillWithValidTestData();
			Factory.Save();
			using (var form = new JASARAdjustmentNoteFormForTest(adjustmentNote))
			{
				AssertNotNull("JXC Export menu item should exist", form.ActionsMenuItem.MenuItems.FindByText("Export JXC Financial Message"));
			}
		}

		#region IJXCExportForm
		public void TestValidateAll()
		{
			var adjustmentNote = GetJASInvoiceWithValidTestData();
			var line = (InvoicingLineBase)adjustmentNote.Lines.AddNew();
			using (adjustmentNote.SuspendValidationTesting())
			using (line.SuspendValidationTesting())
			using (var form = new JASARAdjustmentNoteFormForTest(adjustmentNote))
			{
				line.AL_Desc = "";
				adjustmentNote.AH_GB = ZGuid.Empty;
				adjustmentNote.ClearAllNotifications();
				line.ClearAllNotifications();
				Assert("Pre-condition", !adjustmentNote.HasNotifications());
				Assert("Pre-condition", !line.HasNotifications());
				form.Show();
				((IJXCExportForm)form).ValidateAll();
				Assert("Should have notifications now", adjustmentNote.HasNotifications());
				Assert("Should have notifications now", line.HasNotifications());
			}
		}

		public void TestBusinessEntity()
		{
			var adjustmentNote = GetJASInvoiceWithValidTestData();
			using (var form = new JASARAdjustmentNoteFormForTest(adjustmentNote))
			{
				var formAsInterface = (IJXCExportForm)form;
				AssertEquals("Should be the ZWinForm.BusinessEntity", adjustmentNote, formAsInterface.BusinessEntity);
			}
		}

		#endregion
		#region Implementation
		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return new JASARAdjustmentNoteForm((JASARAdjustmentNote)invoice)
			{ ControllerID = Enterprise.ZArchitecture.Modules.ControllerIDs.ARAdjustmentNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null) => GetJASInvoiceWithValidTestData(fillTestData: fillTestData, factory: factory);
		JASARAdjustmentNote GetJASInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var adjustmentNote = factory != null ? factory.New<JASARAdjustmentNote>() : Factory.New<JASARAdjustmentNote>();
			if (fillTestData)
			{
				adjustmentNote.FillWithValidTestData();
			}

			return adjustmentNote;
		}

		#region class JASARAdjustmentNoteFormForTest
		class JASARAdjustmentNoteFormForTest : JASARAdjustmentNoteForm
		{
			public JASARAdjustmentNoteFormForTest(JASARAdjustmentNote businessEntity) : base(businessEntity)
			{
			}

			public new MenuItem ActionsMenuItem
			{
				get
				{
					return base.ActionsMenuItem;
				}
			}
		}
		#endregion
		#endregion
	}
}
