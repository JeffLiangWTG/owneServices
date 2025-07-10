using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCompany))]
	public class DocCompanyTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocCompany.New(Company, Factory)
			};
		}

		public void TestAddress1()
		{
			ZString address1 = new ZString("Company address 1");
			Company.GC_Address1 = address1;
			AssertEquals("Company address 1", address1, CompanyWrapper.Address1);
		}

		public void TestAddress2()
		{
			ZString address2 = new ZString("Company Address2");
			Company.GC_Address2 = address2;
			AssertEquals("Company address 2", address2, CompanyWrapper.Address2);
		}

		public void TestBusinessRegNo()
		{
			ZString businessRegNo = new ZString("BusinessRegNo");
			Company.GC_BusinessRegNo = businessRegNo;
			AssertEquals("Company BusinessRegNo", businessRegNo, CompanyWrapper.BusinessRegNo);
		}

		public void TestBusinessRegNo2()
		{
			ZString businessRegNo2 = new ZString("BusinessRegNo2");
			Company.GC_BusinessRegNo2 = businessRegNo2;
			AssertEquals("Company BusinessRegNo2", businessRegNo2, CompanyWrapper.BusinessRegNo2);
		}

		public void TestCity()
		{
			ZString city = new ZString("Company City");
			Company.GC_City = city;
			AssertEquals("Company City", city, CompanyWrapper.City);
		}

		public void TestCode()
		{
			ZString code = new ZString("CCC");
			Company.GC_Code = code;
			AssertEquals("Company Code", code, CompanyWrapper.Code);
		}

		public void TestCustomsRegistrationNo()
		{
			ZString customsRegistrationNo = new ZString("CustomsRegNo");
			Company.GC_CustomsRegistrationNo = customsRegistrationNo;
			AssertEquals("Company CustomsRegistrationNo", customsRegistrationNo, CompanyWrapper.CustomsRegistrationNo);
		}

		public void TestEmail()
		{
			ZString email = new ZString("Company Email");
			Company.GC_Email = email;
			AssertEquals("Company Email", email, CompanyWrapper.Email);
		}

		public void TestFax()
		{
			Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Company.GC_Fax = new ZString("0204040404");
			AssertEquals("Company Fax", "+61 2 0404 0404", CompanyWrapper.Fax);
		}

		public void TestName()
		{
			ZString name = new ZString("Company Name");
			Company.GC_Name = name;
			AssertEquals("Company Name", name, CompanyWrapper.Name);
		}

		public void TestPhone()
		{
			Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Company.GC_Phone = new ZString("0205050505");
			AssertEquals("Company Phone", "+61 2 0505 0505", CompanyWrapper.Phone);
		}

		public void TestPostCode()
		{
			ZString postCode = new ZString("PostCode");
			Company.GC_PostCode = postCode;
			AssertEquals("Company PostCode", postCode, CompanyWrapper.PostCode);
		}

		public void TestState()
		{
			ZString state = new ZString("Company State");
			Company.GC_State = state;
			AssertEquals("Company State", state, CompanyWrapper.State);
		}

		public void TestWebAddress()
		{
			ZString webAddress = new ZString("Company WebAddress");
			Company.GC_WebAddress = webAddress;
			AssertEquals("Company WebAddress", webAddress, CompanyWrapper.WebAddress);
		}

		public void TestNoOfAccountingPeriods()
		{
			ZInt noOfAccountingPeriods = new ZInt(5);
			Company.GC_NoOfAccountingPeriods = noOfAccountingPeriods;
			AssertEquals("Company NoOfAccountingPeriods", noOfAccountingPeriods, CompanyWrapper.NoOfAccountingPeriods);
		}

		public void TestIsActive()
		{
			Company.GC_IsActive = ZBool.False;
			Assert("Company Is not active", !CompanyWrapper.IsActive);

			Company.GC_IsActive = ZBool.True;
			Assert("Company is active", CompanyWrapper.IsActive);
		}

		public void TestIsGSTCashBasis()
		{
			Company.GC_IsGSTCashBasis = ZBool.False;
			Assert("Company Is not GST Cash Basis", !CompanyWrapper.IsGSTCashBasis);

			Company.GC_IsGSTCashBasis = ZBool.True;
			Assert("Company is GST Cash Basis", CompanyWrapper.IsGSTCashBasis);
		}

		public void TestIsGSTRegistered()
		{
			Company.GC_IsGSTRegistered = ZBool.False;
			Assert("Company Is not GST Registered", !CompanyWrapper.IsGSTRegistered);

			Company.GC_IsGSTRegistered = ZBool.True;
			Assert("Company IsGSTRegistered", CompanyWrapper.IsGSTRegistered);
		}

		public void TestIsReciprocal()
		{
			Company.GC_IsReciprocal = ZBool.False;
			Assert("Company Is not Reciprocal", !CompanyWrapper.IsReciprocal);

			Company.GC_IsReciprocal = ZBool.True;
			Assert("Company IsReciprocal", CompanyWrapper.IsReciprocal);
		}

		public void TestIsWHTCashBasis()
		{
			Company.GC_IsWHTCashBasis = ZBool.False;
			Assert("Company Is not WHT Cash Basis", !CompanyWrapper.IsWHTCashBasis);

			Company.GC_IsWHTCashBasis = ZBool.True;
			Assert("Company IsWHTCashBasis", CompanyWrapper.IsWHTCashBasis);
		}

		public void TestIsWHTRegistered()
		{
			Company.GC_IsWHTRegistered = ZBool.False;
			Assert("Company Is not WHT Registered", !CompanyWrapper.IsWHTRegistered);

			Company.GC_IsWHTRegistered = ZBool.True;
			Assert("Company IsWHTRegistered", CompanyWrapper.IsWHTRegistered);
		}

		public void TestOrganisation()
		{
			AssertNotNull("Not null organisation when company organisation is set", CompanyWrapper.Organisation);
			AssertEquals("Organisation is a DocOrganisation", typeof(DocOrganisation), CompanyWrapper.Organisation.GetType());

			Company.GC_OH_OrgProxy = ZGuid.Empty;
			AssertNull("Null organisation when company organisation is not set", CompanyWrapper.Organisation);
		}

		public void TestStartDate()
		{
			Company.GC_StartDate = ZDateTime.Empty;
			AssertEquals("Company start date should be empty", ZDateTime.Empty, CompanyWrapper.StartDate);

			ZDateTime startDate = new ZDateTime(2004, 01, 01);
			Company.GC_StartDate = startDate;
			AssertEquals("Company start date", startDate, CompanyWrapper.StartDate);
		}

		public void TestCountry()
		{
			Company.GC_RN_NKCountryCode = ZString.Empty;
			AssertNull("Country should be empty with no country set", CompanyWrapper.Country);

			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			Company.GC_RN_NKCountryCode = country.Code;
			AssertNotNull("Country should not be empty with country set", CompanyWrapper.Country);
			AssertEquals("Country is a DocCountry", typeof(DocCountry), CompanyWrapper.Country.GetType());
		}

		public void TestToString()
		{
			Company.GC_Name = "";
			AssertEquals("ToString should be empty", "", CompanyWrapper.ToString());
			ZString name = new ZString("Company Name");
			Company.GC_Name = name;
			AssertEquals("Company Name", name, CompanyWrapper.ToString());
		}

		#region Implementation

		GlbCompany Company;
		DocCompany CompanyWrapper;

		protected override void SetUp()
		{
			Company = Factory.LoadTop1<GlbCompany>(new ZQuery() { OrderBy = ZArchitecture.Schema.GlbCompanySchema.Constants.GC_Code, });
			CompanyWrapper = DocCompany.New(Company, Factory);
			AssertNotNull("PreCondition: Valid DocCompany", CompanyWrapper);

			base.SetUp();
		}

		#endregion
	}
}
