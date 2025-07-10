using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	class NRCanUserControlTestCase : TestCaseWithFactory
	{
		public void TestSubTabIsVisible()
		{
			SetInvoiceData();
			using (var form = new ZForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				control.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoiceline);
				invoiceline.CA_NRCanInd = "Y";
				var requirments = invoiceline.PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.NRCan);
				var program1 = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == NRCanPGADepartmentCodes.Codes.EEF);
				program1.Indicator = "Y";
				var program2 = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == NRCanPGADepartmentCodes.Codes.EXP);
				program2.Indicator = "N";
				var tabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.NRCan && p.TabVisible);
				AssertNotNull("Tab not null", tabPage);
				var userControl = tabPage.FindSingleOrDefault<NRCanUserControl>();
				var tabpage1 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == NRCanPGADepartmentCodes.Descriptions.EEF && p.TabVisible);
				var tabpage2 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == NRCanPGADepartmentCodes.Descriptions.EXP && p.TabVisible);
				AssertNotNull("Tab not null", tabpage1);
				AssertNull("Tab null", tabpage2);
			}
		}

		public void TestDefaultDataSourceBindingMemberAttributeAndDefaultBindingPropertyAttribute()
		{
			var defaultDataSourceBindingMember = typeof(NRCanUserControl).GetCustomAttributes(typeof(DefaultDataSourceBindingMemberAttribute), false).First();
			AssertNotNull(defaultDataSourceBindingMember);
			AssertNull(((DefaultDataSourceBindingMemberAttribute)defaultDataSourceBindingMember).DefaultBindingMember);

			var defaultBindingProperty = typeof(NRCanUserControl).GetCustomAttributes(typeof(DefaultBindingPropertyAttribute), false).First();
			AssertNotNull(defaultBindingProperty);
			AssertEquals("NRCanPGAHeader", ((DefaultBindingPropertyAttribute)defaultBindingProperty).Name);
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
