using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocBranch))]
	public class DocBranchTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocBranch.New(Branch, Factory)
			};
		}

		public void TestIBranchInterface()
		{
			var branchWrapper = (IBranch)DocBranch.New(GlbBranch.CurrentBranch, Factory);
			AssertEquals(GlbBranch.CurrentBranch.PK, branchWrapper.PK);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, branchWrapper.CompanyPK);
		}

		public void TestMailToAddress()
		{
			ZBool printBranchAddress = ZBool.False;
			if (Branch.PK.IsValid)
			{
				printBranchAddress = AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.GetValueWithoutFallback(Guid.Empty, Branch.PK.ToGuid(), Guid.Empty);
			}

			if (printBranchAddress)
			{
				Branch.GB_OH_OrgProxy = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
				BranchWrapper = DocBranch.New(Branch, Factory);
				AssertEquals("Branch Org Proxy Address", BranchWrapper.Organisation.ARAddress.PostalAddress, BranchWrapper.MailToAddress.PostalAddress);
			}
			else
			{
				Branch.GB_GC = GlbCompany.CurrentCompany.PK;
				DocCompany company = DocCompany.New(Branch.Company, Factory);
				BranchWrapper = DocBranch.New(Branch, Factory);
				AssertEquals("Company Org Proxy Address", company.Organisation.ARAddress.PostalAddress, BranchWrapper.MailToAddress.PostalAddress);
			}
		}

		public void TestAddress1()
		{
			AssertEquals("Branch address", "", BranchWrapper.Address1);

			ZString address1 = new ZString("Branch address 1");
			Branch.GB_Address1 = address1;
			AssertEquals("Branch Address", address1, BranchWrapper.Address1);
		}

		public void TestAddress2()
		{
			AssertEquals("Branch address", "", BranchWrapper.Address2);

			ZString address2 = new ZString("Branch address 2");
			Branch.GB_Address2 = address2;
			AssertEquals("Branch Address", address2, BranchWrapper.Address2);
		}

		public void TestBranchName()
		{
			AssertEquals("Branch Name", "", BranchWrapper.BranchName);

			ZString branchName = new ZString("Branch Name");
			Branch.GB_BranchName = branchName;
			AssertEquals("Branch Name", branchName, BranchWrapper.BranchName);
		}

		public void TestCity()
		{
			AssertEquals("Branch City", "", BranchWrapper.City);

			ZString city = new ZString("Branch City");
			Branch.GB_City = city;
			AssertEquals("Branch City", city, BranchWrapper.City);
		}

		public void TestCode()
		{
			AssertEquals("Branch Code", "", BranchWrapper.Code);

			ZString code = new ZString("CCC");
			Branch.GB_Code = code;
			AssertEquals("Branch Code", code, BranchWrapper.Code);
		}

		public void TestEmail()
		{
			AssertEquals("Branch Email", "", BranchWrapper.Email);

			ZString email = new ZString("Branch Email");
			Branch.GB_Email = email;
			AssertEquals("Branch Email", email, BranchWrapper.Email);
		}

		public void TestFax()
		{
			AssertEquals("Branch Fax", "", BranchWrapper.Fax);

			Branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Branch.GB_Fax = new ZString("0203030303");
			AssertEquals("Branch Fax", "+61 2 0303 0303", BranchWrapper.Fax);
		}

		public void TestBranch()
		{
			AssertEquals("Should be CurrentBranch", GlbBranch.CurrentBranch.PK, BranchWrapper.Branch);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			Branch.GB_GC = company.PK;
			AssertEquals(Branch.PK, BranchWrapper.Branch);
		}

		public void TestBranchCompany()
		{
			AssertEquals("Should be CurrentCompany", GlbCompany.CurrentCompany.PK, BranchWrapper.BranchCompany);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			Branch.GB_GC = company.PK;
			AssertEquals(company.PK, BranchWrapper.BranchCompany);
		}

		public void TestLogo()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(3, 3));
			AssertEquals("Should return Company Logo", new Size(3, 3), BranchWrapper.Logo.Size);
			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(1, 1));
			AssertEquals("Should not return InvoiceAndStatementLogo", new Size(3, 3), BranchWrapper.Logo.Size);
		}

		public void TestGetDepartmentBranchLogoHierarhy()
		{
			Guid departmentPk = Guid.NewGuid();

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, BranchWrapper.Branch.ToGuid(), Guid.Empty, new Bitmap(2, 1));
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, BranchWrapper.Branch.ToGuid(), departmentPk, new Bitmap(3, 3));

			AssertEquals("Should return branch-level logo", new Size(2, 1), BranchWrapper.Logo.Size);
			AssertEquals("Should return branch-level logo", new Size(2, 1), BranchWrapper.GetDepartmentBranchLogo(Guid.Empty).Size);
			AssertEquals("Should return branch-level logo", new Size(2, 1), BranchWrapper.GetDepartmentBranchLogo(Guid.NewGuid()).Size);
			AssertEquals("Should return department-level logo", new Size(3, 3), BranchWrapper.GetDepartmentBranchLogo(departmentPk).Size);
		}

		public void TestInvoiceAndStatementLogo()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(3, 3));
			AssertEquals("Should return Company Logo", new Size(3, 3), BranchWrapper.GetInvoiceAndStatementLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Bitmap(4, 4));
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), new Bitmap(5, 5));
			AssertEquals("Should return Company Logo(with Department)", new Size(5, 5), BranchWrapper.GetInvoiceAndStatementLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(2, 1));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(2, 1), BranchWrapper.GetInvoiceAndStatementLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Env.CurrentDepartment.PK, new Bitmap(3, 3));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(3, 3), BranchWrapper.GetInvoiceAndStatementLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, new Bitmap(2, 2));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(2, 2), BranchWrapper.GetInvoiceAndStatementLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);
		}

		public void TestARInvoiceLogo()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			//UseThisBranchLetterheadOnARInvoice: CurrentBranch
			DocBranch branchW = DocBranch.New(GlbBranch.CurrentBranch, Factory);
			AccountingConfigurationRegistry.Instance.UseThisBranchLetterheadOnARInvoice.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid());

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(3, 3));
			AssertEquals("Should return Company Logo", new Size(3, 3), branchW.GetARInvoiceLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Bitmap(4, 4));
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), new Bitmap(5, 5));
			AssertEquals("Should return Company Logo(without Department)", new Size(4, 4), branchW.GetARInvoiceLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(2, 1));
			AssertEquals("Should return InvoiceAndStatementLogo", new Size(2, 1), branchW.GetARInvoiceLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			//UseThisBranchLetterheadOnARInvoice: not Current Branch
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			AccountingConfigurationRegistry.Instance.UseThisBranchLetterheadOnARInvoice.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, branch.PK.ToGuid());

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, new Bitmap(4, 4));
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), new Bitmap(5, 5));
			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, new Bitmap(6, 6));
			AssertEquals("Should return new branch's InvoiceAndStatementLogo", new Size(6, 6), branchW.GetARInvoiceLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, null);
			AssertEquals("Should return new branch's CompanyLogo(without Department)", new Size(4, 4), branchW.GetARInvoiceLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, null);
			AssertNull(branchW.GetARInvoiceLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()));

			//UseThisBranchLetterheadOnARInvoice: not exist Branch
			AccountingConfigurationRegistry.Instance.UseThisBranchLetterheadOnARInvoice.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, Guid.NewGuid());
			AssertEquals("Should have no Exception and return InvoceAndStatementLogo", new Size(2, 1), branchW.GetARInvoiceLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);

			SystemDataRegistry.Instance.InvoceAndStatementLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertEquals("Should have no Exception and return CompanyLogo(with Department)", new Size(5, 5), branchW.GetARInvoiceLogo(GlbDepartment.CurrentDepartment.PK.ToGuid()).Size);
		}

		public void TestPhone()
		{
			AssertEquals("Branch Phone", "", BranchWrapper.Phone);
			Branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Branch.GB_Phone = new ZString("0201010101");
			AssertEquals("Branch Phone", "+61 2 0101 0101", BranchWrapper.Phone);
		}

		public void TestPostCode()
		{
			AssertEquals("Branch PostCode", "", BranchWrapper.PostCode);

			ZString postCode = new ZString("PostCode");
			Branch.GB_PostCode = postCode;
			AssertEquals("Branch PostCode", postCode, BranchWrapper.PostCode);
		}

		public void TestState()
		{
			AssertEquals("Branch State", "", BranchWrapper.State);

			ZString state = new ZString("Branch State");
			Branch.GB_State = state;
			AssertEquals("Branch State", state, BranchWrapper.State);
		}

		public void TestWebAddress()
		{
			AssertEquals("Branch WebAddress", "", BranchWrapper.WebAddress);

			ZString webAddress = new ZString("Branch WebAddress");
			Branch.GB_WebAddress = webAddress;
			AssertEquals("Branch WebAddress", webAddress, BranchWrapper.WebAddress);
		}

		public void TestIsActive()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());
			Branch.GB_GC = company.PK;
			Branch.GB_IsActive = ZBool.False;
			Assert("Branch Is not active", !BranchWrapper.IsActive);

			Branch.GB_IsActive = ZBool.True;
			Assert("Branch is active", BranchWrapper.IsActive);
		}

		public void TestToString()
		{
			AssertEquals("ToString", "", BranchWrapper.ToString());

			ZString code = new ZString("CCC");
			Branch.GB_Code = code;
			AssertEquals("ToString", code, BranchWrapper.ToString());
		}

		public void TestCompany()
		{
			AssertNull("Null company when branch company is not set", BranchWrapper.Company);

			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());
			Branch.GB_GC = company.PK;
			AssertNotNull("Not null company when branch company is set", BranchWrapper.Company);
			AssertEquals("Company is a DocCompany", typeof(DocCompany), BranchWrapper.Company.GetType());
		}

		public void TestCountry()
		{
			AssertNull("Null country when branch company is not set", BranchWrapper.Country);

			Branch.GB_RL_NKHomePort = "AUBNE";
			AssertNotNull("Not null country when branch country is set", BranchWrapper.Country);
			AssertEquals("Should return Australia as the country", Core.Constants.CountryCodes.Australia, BranchWrapper.Country.Code);
			AssertEquals("Country is a DocCountry", typeof(DocCountry), BranchWrapper.Country.GetType());
		}

		public void TestOrganisation()
		{
			AssertNull("Null organisation when branch organisation is not set", BranchWrapper.Organisation);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Branch.GB_OH_OrgProxy = org.PK;
			AssertNotNull("Not null organisation when branch organisation is set", BranchWrapper.Organisation);
			AssertEquals("Organisation is a DocOrganisation", typeof(DocOrganisation), BranchWrapper.Organisation.GetType());
		}

		public void TestOrganisationWithCompanyFallBack_ReturnsBranchOrganisation_WhenBranchHasOrganisation()
		{
			var company = Factory.New<GlbCompany>();
			Branch.GB_GC = company.PK;
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			org1.OH_Code = "OrgA";
			org2.OH_Code = "OrgB";
			Branch.GB_OH_OrgProxy = org1.PK;
			company.GC_OH_OrgProxy = org2.PK;

			AssertNotNull("Precondition : Branch has an OrgProxy", Branch.OrgProxy);
			AssertNotNull("Precondition : Company has an OrgProxy", Branch.Company.OrgProxy);

			AssertEquals("OrgA", BranchWrapper.OrganisationWithCompanyFallBack.Code);
			AssertNotEquals(BranchWrapper.OrganisationWithCompanyFallBack.Code, company.OrgProxy.OH_Code);
		}

		public void TestOrganisationWithCompanyFallBack_ReturnsCompanyOrganisation_WhenBranchHasNoOrganisation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OrgA";
			var company = Factory.New<GlbCompany>();
			Branch.GB_GC = company.PK;
			company.GC_OH_OrgProxy = org.PK;
			AssertNull("Precondition : Branch has no OrgProxy", Branch.OrgProxy);
			AssertNotNull("Precondition : Company has an OrgProxy", Branch.Company.OrgProxy);

			AssertEquals("OrgA", BranchWrapper.OrganisationWithCompanyFallBack.Code);
		}

		public void TestLoco()
		{
			AssertNull("Null Loco when branch Loco is not set", BranchWrapper.Loco);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Branch.GB_RL_NKHomePort = uNLOCO.RL_Code;
			AssertNotNull("Not null Loco when branch Loco is set", BranchWrapper.Loco);
			AssertEquals("Loco is a DocUNLOCO", typeof(DocUNLOCO), BranchWrapper.Loco.GetType());
		}

		#region Implementation

		GlbBranch Branch;
		DocBranch BranchWrapper;

		protected override void SetUp()
		{
			Branch = Factory.New<GlbBranch>();
			BranchWrapper = DocBranch.New(Branch, Factory);
			AssertNotNull("PreCondition: Valid DocBranch", BranchWrapper);

			base.SetUp();
		}

		#endregion
	}
}
