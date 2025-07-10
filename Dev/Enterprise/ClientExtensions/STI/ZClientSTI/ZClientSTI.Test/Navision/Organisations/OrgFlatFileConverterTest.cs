using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class OrgFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestExport()
		{
			OrgFlatFileConverterTestClass converter = new OrgFlatFileConverterTestClass();
			FlatFileDataRowCollection dataRows = converter.MapExport(CreateOrgXSD());
			AssertEquals("Should be one row in the collection", 1, dataRows.Count);
			OrgFlatFileDataRow row = (OrgFlatFileDataRow)dataRows[0];
			AssertEquals("Org Code", "EAGLEDAT", row.OrgCode);
			AssertEquals("Org Name", "Eagle Datamation International", row.OrgName);
			AssertEquals("Search Name", "Eagle Datamation International", row.SearchName);
			AssertEquals("Address1", "Level 3", row.Address);
			AssertEquals("Address2", "184 Bourke Road", row.Address2);
			AssertEquals("City", "Alexandria", row.City);
			AssertEquals("Phone", "+61290251100", row.PhoneNumber);
			AssertEquals("Country Code", "AU", row.CountryCode);
			AssertEquals("Fax", "61 2 9025 1199", row.FaxNumber);
			AssertEquals("Post Code", "2015", row.PostCode);
			AssertEquals("County/State", "NSW", row.County);
			AssertEquals("Email", "support@edi.com.au", row.Email);
			AssertEquals("Home Page", "http://www.edi.com.au", row.HomePage);
			AssertEquals("Credit Limit", 100000m, row.CreditLimit);
			AssertEquals("Customer Posting Group", "ASC", row.CustomerPostingGroup);
			AssertEquals("Payment Terms Code", "INV30", row.PaymentTermsCode);
			AssertEquals("Sales Person Code", "Ezy Rep", row.SalesPersonCode);
			AssertEquals("Shipment Method", Core.Constants.IncoTerms.CostInsuranceAndFreight, row.ShipmentMethodCode);
			AssertEquals("Blocked", "", row.Blocked);
			AssertEquals("Bill To Customer Number", "NYKLIN", row.BillToCustomerNumber);
			AssertEquals("GST Business Posting Group", "No GST", row.GSTBusinessPostingGroup);
			AssertEquals("ABN", "41 065 894 724", row.ABN);
			AssertEquals("Global Dimension 1 Code", "SYD", row.GlobalDimension1Code);
		}

		#region Create Business Objects & XSDs
		OrgHeader CreateOrgBusinessObject()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org.OH_FullName = "Eagle Datamation International";
			org.MainAddress.OA_Address1 = "Level 3";
			org.MainAddress.OA_Address2 = "184 Bourke Road";
			org.MainAddress.OA_City = "Alexandria";
			org.MainAddress.OA_Phone = "61 2 9025 1100";
			org.MiscServ.OM_ARCreditLimit = 100000m;
			org.MiscServ.OM_OJ_ARDebtorGroup = Factory.LoadFromUniqueKey(typeof(OrgDebtorGroup), OrgDebtorGroupSchema.OJ_Code, (ZString)"ASC").PK;
			org.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 30;
			org.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "EZY";
			staff.GS_FullName = "Ezy Rep";
			staff.GS_LoginName = "ezyr";
			OrgStaffAssignments staffAssignment = org.StaffAssignments.AddNew();
			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			staffAssignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			org.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			org.OH_RL_NKClosestPort = "AUSYD";
			org.ARSettlementGroupPK = Factory.LoadFromUniqueKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, (ZString)"NYKLIN").PK;
			org.MainAddress.OA_Fax = "61 2 9025 1199";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_Email = "support@edi.com.au";
			org.MainWebURL.PU_URL = "http://www.edi.com.au";
			org.CompanyData.SetARTaxApplicable(ZBool.False);
			org.PrimaryRegistrationNumber.Number = "41 065 894 724";
			org.OH_Code = "EAGLEDAT";
			Factory.Save();
			return org;
		}

		Xsd.Organisation CreateOrgXSD()
		{
			OrgHeader org = CreateOrgBusinessObject();
			OrganisationValueObjectDataAdapter orgDataAdapter = new OrganisationValueObjectDataAdapter();
			return orgDataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));
		}

		#endregion
		#region Test Class
		class OrgFlatFileConverterTestClass : OrgFlatFileConverter
		{
			public OrgFlatFileConverterTestClass() : base(new NotificationBuffer(), new BusinessObjectFactory())
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
		#endregion
	}
}
