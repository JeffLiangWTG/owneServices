using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class HCUserControlTestCase : TestCaseWithFactory
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
				invoiceline.CA_HCInd = "Y";
				var requirments = invoiceline.PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.HC);
				var program1 = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == HCPGADepartmentCodes.Codes.API);
				program1.Indicator = "Y";
				var program2 = requirments.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == HCPGADepartmentCodes.Codes.BBC);
				program2.Indicator = "N";
				var tabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.HC && p.TabVisible);
				AssertNotNull("Tab not null", tabPage);
				var userControl = tabPage.FindSingleOrDefault<HCUserControl>();
				var tabpage1 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == HCPGADepartmentCodes.Descriptions.API && p.TabVisible);
				var tabpage2 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == HCPGADepartmentCodes.Descriptions.BBC && p.TabVisible);
				AssertNotNull("Tab not null", tabpage1);
				AssertNull("Tab null", tabpage2);
			}
		}

		public void TestSubTabUserControl()
		{
			SetInvoiceData();
			using (var form = new ZForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				control.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoiceline);
				invoiceline.CA_HCInd = "Y";
				var requirment = invoiceline.PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.HC);
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.API, HCPGADepartmentCodes.Descriptions.API).FindSingleOrDefault<ActivePHIngredientsUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.BBC, HCPGADepartmentCodes.Descriptions.BBC).FindSingleOrDefault<BloodComponentUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.CPR, HCPGADepartmentCodes.Descriptions.CPR).FindSingleOrDefault<ConsumerProductUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.CTO, HCPGADepartmentCodes.Descriptions.CTO).FindSingleOrDefault<CellsTissuesAndOrgansUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.DSE, HCPGADepartmentCodes.Descriptions.DSE).FindSingleOrDefault<DonorSemenUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.HDR, HCPGADepartmentCodes.Descriptions.HDR).FindSingleOrDefault<HumanDrugsUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.MDE, HCPGADepartmentCodes.Descriptions.MDE).FindSingleOrDefault<MedicalDevicesUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.NHP, HCPGADepartmentCodes.Descriptions.NHP).FindSingleOrDefault<NaturalHealthProductsUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.OCS, HCPGADepartmentCodes.Descriptions.OCS).FindSingleOrDefault<OfficeOfControlledSubstancesUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.PES, HCPGADepartmentCodes.Descriptions.PES).FindSingleOrDefault<PesticideUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.RED, HCPGADepartmentCodes.Descriptions.RED).FindSingleOrDefault<RadiationEmittingDevicesUserControl>());
				AssertNotNull(AssertSubTabUserControl(control, requirment, HCPGADepartmentCodes.Codes.VET, HCPGADepartmentCodes.Descriptions.VET).FindSingleOrDefault<VetDrugUserControl>());
			}
		}

		ZTabPage AssertSubTabUserControl(CAImportInvoiceLineUserControl control, PGARequirement requirment, ZString programCode, ZString programDescription)
		{
			var program1 = requirment.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == programCode);
			program1.Indicator = "Y";
			var tabPage = control.LineDetailTabControl.FindSingleOrDefault<ZTabPage>(p => p.Text == PGACodes.Codes.HC && p.TabVisible);
			var userControl = tabPage.FindSingleOrDefault<HCUserControl>();
			var tabpage1 = userControl.FindSingleOrDefault<ZTabPage>(p => p.Text == programDescription && p.TabVisible);
			return tabpage1;
		}

		public void TestDefaultDataSourceBindingMemberAttributeAndDefaultBindingPropertyAttribute()
		{
			var defaultDataSourceBindingMember = typeof(HCUserControl).GetCustomAttributes(typeof(DefaultDataSourceBindingMemberAttribute), false).First();
			AssertNotNull(defaultDataSourceBindingMember);
			AssertNull(((DefaultDataSourceBindingMemberAttribute)defaultDataSourceBindingMember).DefaultBindingMember);

			var defaultBindingProperty = typeof(HCUserControl).GetCustomAttributes(typeof(DefaultBindingPropertyAttribute), false).First();
			AssertNotNull(defaultBindingProperty);
			AssertEquals("HCPGAHeader", ((DefaultBindingPropertyAttribute)defaultBindingProperty).Name);
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
