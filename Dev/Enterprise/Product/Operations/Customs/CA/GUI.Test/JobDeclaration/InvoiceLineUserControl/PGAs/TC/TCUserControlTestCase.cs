using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class TCUserControlTestCase : TestCaseWithFactory
	{
		public void TestSubTabIsVisible()
		{
			this.SetInvoiceData();
			using (var form = new ZForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				control.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoiceline);
				invoiceline.CA_TCInd = "Y";
				var requirments = invoiceline.PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.TC);
				var program1 = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == TCPGADepartmentCodes.Codes.TPR);
				program1.Indicator = "Y";
				var program2 = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == TCPGADepartmentCodes.Codes.VPR);
				program2.Indicator = "N";
				ZTabPage tabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.TC && p.TabVisible);
				AssertNotNull("Tab not null", tabPage);
				var userControl = tabPage.FindSingleOrDefault<TCUserControl>();
				var tabpage1 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == TCPGADepartmentCodes.Descriptions.TPR && p.TabVisible);
				var tabpage2 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == TCPGADepartmentCodes.Descriptions.VPR && p.TabVisible);
				AssertNotNull("Tab not null", tabpage1);
				AssertNull("Tab null", tabpage2);
			}
		}

		public void TestDefaultDataSourceBindingMemberAttributeAndDefaultBindingPropertyAttribute()
		{
			var defaultDataSourceBindingMember = typeof(TCUserControl).GetCustomAttributes(typeof(DefaultDataSourceBindingMemberAttribute), false).First();
			AssertNotNull(defaultDataSourceBindingMember);
			AssertNull(((DefaultDataSourceBindingMemberAttribute)defaultDataSourceBindingMember).DefaultBindingMember);

			var defaultBindingProperty = typeof(TCUserControl).GetCustomAttributes(typeof(DefaultBindingPropertyAttribute), false).First();
			AssertNotNull(defaultBindingProperty);
			AssertEquals("TCPGAHeader", ((DefaultBindingPropertyAttribute)defaultBindingProperty).Name);
		}

		void SetInvoiceData()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			invoiceline = invoice.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceline;
	}
}
