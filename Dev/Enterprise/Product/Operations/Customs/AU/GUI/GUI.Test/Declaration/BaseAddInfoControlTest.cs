using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	abstract class BaseAddInfoControlTest : TestCaseWithFactory
	{
		public void TestReadOnlyWhenSetBeforeBinding()
		{
			using (ZChildForm testForm = new ZChildForm(invoiceLine))
			{
				testControl = ControlToTest;
				testControl.BindTo = "AddInfo+" + SchemaColumnToBindTo;
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();
				AssertEquals("Control ReadOnly", false, testControl.ReadOnly);
			}
		}

		public abstract void TestBindingWithFlattenedHierarchy();

		public abstract void TestBindingWithNormalProperty();

		protected abstract BaseAddInfoControl ControlToTest { get; }

		protected abstract ZString SchemaColumnToBindTo { get; }

		protected abstract ZPropertyInfo PropertyInfo { get; }

		protected BaseAddInfoControl testControl;
		protected ZButton button;
		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}

		protected void ChangeFocusToInvokeBinding()
		{
			button.Focus();
			Application.DoEvents();
		}
	}
}
