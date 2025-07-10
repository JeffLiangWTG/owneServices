using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class DFOUserControlTestCase : TestCaseWithFactory
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
				invoiceline.CA_DFOInd = "Y";
				var requirments = invoiceline.PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.DFO);
				var program1 = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == DFOPGADepartmentCodes.Codes.ABI);
				program1.Indicator = "Y";
				var program2 = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == DFOPGADepartmentCodes.Codes.AIS);
				program2.Indicator = "N";
				ZTabPage tabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.DFO && p.TabVisible);
				AssertNotNull("Tab not null", tabPage);
				var userControl = tabPage.FindSingleOrDefault<DFOUserControl>();
				var tabpage1 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == DFOPGADepartmentCodes.Descriptions.ABI && p.TabVisible);
				var tabpage2 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == DFOPGADepartmentCodes.Descriptions.AIS && p.TabVisible);
				AssertNotNull("Tab not null", tabpage1);
				AssertNull("Tab null", tabpage2);
			}
		}

		public void TestDefaultDataSourceBindingMemberAttributeAndDefaultBindingPropertyAttribute()
		{
			var defaultDataSourceBindingMember = typeof(DFOUserControl).GetCustomAttributes(typeof(DefaultDataSourceBindingMemberAttribute), false).First();
			AssertNotNull(defaultDataSourceBindingMember);
			AssertNull(((DefaultDataSourceBindingMemberAttribute)defaultDataSourceBindingMember).DefaultBindingMember);

			var defaultBindingProperty = typeof(DFOUserControl).GetCustomAttributes(typeof(DefaultBindingPropertyAttribute), false).First();
			AssertNotNull(defaultBindingProperty);
			AssertEquals("DFOPGAHeader", ((DefaultBindingPropertyAttribute)defaultBindingProperty).Name);
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
