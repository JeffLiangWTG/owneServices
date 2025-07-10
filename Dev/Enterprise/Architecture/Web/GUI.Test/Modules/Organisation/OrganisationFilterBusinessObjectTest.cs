using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(OrganisationFilterBusinessObject))]
	public class OrganisationFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		#region TestFilter

		public virtual void TestFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Org1";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Org2";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "Org3";
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_FullName = "Org4";
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();
			org5.OH_FullName = "Org5";
			OrgHeader org6 = Factory.NewWithValidTestData<OrgHeader>();
			org6.OH_FullName = "Org6";

			OrgHeader relatedConsign = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignee = ZBool.True;
			relatedConsign.BuyerLinks.AddNew(org1);
			org2.OH_IsConsignor = ZBool.True;
			relatedConsign.SupplierLinks.AddNew(org2);

			relatedConsign.BuyerLinks.AddNew(org3);
			relatedConsign.SupplierLinks.AddNew(org4);

			org5.OH_IsConsignee = ZBool.True;
			org6.OH_IsConsignor = ZBool.True;

			Factory.Save();

			OrgHeaderCollection matchingOrgs = new OrgHeaderCollection(Factory);

			FilterObject.OH_RelatedConsign = relatedConsign.PK;
			FilterObject.OH_IsConsignor = ZBool.True;
			FilterObject.OH_IsConsignee = ZBool.True;
			matchingOrgs.Load(FilterObject.Filter);

			TestFilterInternal(matchingOrgs, relatedConsign, org1, org2, org3, org4, org5, org6);
		}

		protected virtual void TestFilterInternal(OrgHeaderCollection matchedOrgs, OrgHeader relatedConsign,
			OrgHeader linkedBuyingConsignee, OrgHeader linkedSupplyingConsignor,
			OrgHeader linkedBuyer, OrgHeader linkedSupplier,
			OrgHeader consignee, OrgHeader consignor)
		{
			AssertEquals("Collection", 3, matchedOrgs.Count);
			AssertCollectionContains("RelatedConsign should always be included", relatedConsign, matchedOrgs);
			AssertCollectionContains("LinkedBuyingConsignee should be included - Consignee which Buys from RelatedConsign", linkedBuyingConsignee, matchedOrgs);
			AssertCollectionContains("LinkedSupplyingConsignor should be included - Consignor which Supplies to RelatedConsign", linkedSupplyingConsignor, matchedOrgs);
			AssertCollectionNotContains("LinkedSupplier should not be included - Not a Consignee/Consignor", linkedSupplier, matchedOrgs);
			AssertCollectionNotContains("LinkedBuyer should not be included - Not  a Consignee/Consignor", linkedBuyer, matchedOrgs);
			AssertCollectionNotContains("Consignee should not be included - No Supplier/Buyer link", consignee, matchedOrgs);
			AssertCollectionNotContains("Consignor should not be included - No Supplier/Buyer link", consignor, matchedOrgs);
		}

		#endregion

		#region TestDetailsFilterControlFilterWithStartsWith

		public void TestDetailsFilterControlFilterWithStartsWith()
		{
			// Note: This is to ensure that all filters are tested. If this fails, then a filter type has been added or removed and the test needs to be changed accordingly.
			AssertEquals("OH_DetailsFilter_List.Count", 14, FilterObject.OH_DetailsFilter_List.Count);

			FilterObject.ActiveStatus = OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients;
			//FilterObject.OH_IsActive = false;
			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Code;
			FilterObject.OH_Details = "AA";
			ZString expectedFilter = "OH_Code like 'AA%'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.FullName;
			expectedFilter = "OH_FullName like 'AA%'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Address;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Address1 like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.City;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_City like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.State;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_State like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.PostCode;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_PostCode like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Phone;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Phone like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Mobile;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Mobile like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Fax;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Fax like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Email;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Email like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Web;
			expectedFilter = "OH_PK IN (SELECT PU_OH FROM dbo.OrgWebURL WHERE PU_URL like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.All;
			expectedFilter = "OH_Code like 'AA%' or OH_FullName like 'AA%' or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Address1 like 'AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_City like 'AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_State like 'AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_PostCode like 'AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Phone like 'AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Mobile like 'AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Fax like 'AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Email like 'AA%') or OH_PK IN (SELECT PU_OH FROM dbo.OrgWebURL WHERE PU_URL like 'AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Common;
			expectedFilter = "OH_Code like 'AA%' or OH_FullName like 'AA%'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = "None";
			expectedFilter = "";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());
		}
		#endregion

		#region TestDetailsFilterControlFilterWithContains

		public void TestDetailsFilterControlFilterWithContains()
		{
			// Note: This is to ensure that all filters are tested. If this fails, then a filter type has been added or removed and the test needs to be changed accordingly.
			AssertEquals("OH_DetailsFilter_List.Count", 14, FilterObject.OH_DetailsFilter_List.Count);

			FilterObject.ActiveStatus = OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients;
			//				FilterObject.OH_IsActive = false;
			FilterObject.OH_Calc_StartsWith = false;
			FilterObject.OH_Calc_Contains = true;
			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Code;
			FilterObject.OH_Details = "AA";
			ZString expectedFilter = "OH_Code like '%AA%'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.FullName;
			expectedFilter = "OH_FullName like '%AA%'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Address;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Address1 like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.City;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_City like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.State;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_State like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.PostCode;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_PostCode like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Phone;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Phone like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Mobile;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Mobile like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Fax;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Fax like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Email;
			expectedFilter = "OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Email like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Web;
			expectedFilter = "OH_PK IN (SELECT PU_OH FROM dbo.OrgWebURL WHERE PU_URL like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.All;
			expectedFilter = "OH_Code like '%AA%' or OH_FullName like '%AA%' or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Address1 like '%AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_City like '%AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_State like '%AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_PostCode like '%AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Phone like '%AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Mobile like '%AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Fax like '%AA%') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Email like '%AA%') or OH_PK IN (SELECT PU_OH FROM dbo.OrgWebURL WHERE PU_URL like '%AA%')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO);

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Common;
			expectedFilter = "OH_Code like '%AA%' or OH_FullName like '%AA%'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO);

			FilterObject.OH_DetailsFilter = "None";
			expectedFilter = "";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO);
		}

		public void TestDetailsFilterControlFilter_MaxLength()
		{
			// Note: This is to ensure that all filters are tested. If this fails, then a filter type has been added or removed and the test needs to be changed accordingly.
			AssertEquals("OH_DetailsFilter_List.Count", 14, FilterObject.OH_DetailsFilter_List.Count);

			FilterObject.ActiveStatus = OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients;
			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Code;
			var expectedCode = new string('A', OrgHeaderSchema.OH_Code.MaxLength);
			FilterObject.OH_Details = expectedCode + "A";
			var expectedFilter = $"OH_Code = '{expectedCode}'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.FullName;
			var expectedFullName = new string('A', OrgHeaderSchema.OH_FullName.MaxLength);
			FilterObject.OH_Details = expectedFullName + "A";
			expectedFilter = $"OH_FullName = '{expectedFullName}'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Address;
			var expectedAddress1 = new string('A', OrgAddressSchema.OA_Address1.MaxLength);
			FilterObject.OH_Details = expectedAddress1 + "A";
			expectedFilter = $"OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Address1 = '{expectedAddress1}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.City;
			var expectedCity = new string('A', OrgAddressSchema.OA_City.MaxLength);
			FilterObject.OH_Details = expectedCity + "A";
			expectedFilter = $"OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_City = '{expectedCity}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.State;
			var expectedState = new string('A', OrgAddressSchema.OA_State.MaxLength);
			FilterObject.OH_Details = expectedState + "A";
			expectedFilter = $"OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_State = '{expectedState}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.PostCode;
			var expectedPostCode = new string('A', OrgAddressSchema.OA_PostCode.MaxLength);
			FilterObject.OH_Details = expectedPostCode + "A";
			expectedFilter = $"OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_PostCode = '{expectedPostCode}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Phone;
			var expectedPhone = new string('A', OrgAddressSchema.OA_Phone.MaxLength);
			FilterObject.OH_Details = expectedPhone + "A";
			expectedFilter = $"OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Phone = '{expectedPhone}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Mobile;
			var expectedMobile = new string('A', OrgAddressSchema.OA_Mobile.MaxLength);
			FilterObject.OH_Details = expectedMobile + "A";
			expectedFilter = $"OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Mobile = '{expectedMobile}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Fax;
			var expectedFax = new string('A', OrgAddressSchema.OA_Fax.MaxLength);
			FilterObject.OH_Details = expectedFax + "A";
			expectedFilter = $"OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Fax = '{expectedFax}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgAddress.Email;
			var expectedEmail = new string('A', OrgAddressSchema.OA_Email.MaxLength);
			FilterObject.OH_Details = expectedEmail + "A";
			expectedFilter = $"OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Email = '{expectedEmail}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Web;
			var expectedWebURL = new string('A', OrgWebURLSchema.PU_URL.MaxLength);
			FilterObject.OH_Details = expectedWebURL + "A";
			expectedFilter = $"OH_PK IN (SELECT PU_OH FROM dbo.OrgWebURL WHERE PU_URL = '{expectedWebURL}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO.Trim());

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.All;
			FilterObject.OH_Details = new string('A', GetLargestMaxLength() + 1);
			expectedFilter = $"OH_Code = '{expectedCode}' or OH_FullName = '{expectedFullName}' or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Address1 = '{expectedAddress1}') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_City = '{expectedCity}') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_State = '{expectedState}') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_PostCode = '{expectedPostCode}') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Phone = '{expectedPhone}') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Mobile = '{expectedMobile}') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Fax = '{expectedFax}') or OH_PK IN (SELECT OA_OH FROM dbo.OrgAddress WHERE OA_Email = '{expectedEmail}') or OH_PK IN (SELECT PU_OH FROM dbo.OrgWebURL WHERE PU_URL = '{expectedWebURL}')";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO);

			FilterObject.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.Common;
			expectedFilter = $"OH_Code = '{expectedCode}' or OH_FullName = '{expectedFullName}'";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO);

			FilterObject.OH_DetailsFilter = "None";
			expectedFilter = "";
			AssertEquals("OrgDetailsGroupBoxFilter", expectedFilter, FilterObject.DetailsFilterControlFilterInternal.LiteralTextADO);

			int GetLargestMaxLength()
			{
				var maxLengths = new[]
				{
						OrgHeaderSchema.OH_Code.MaxLength,
						OrgHeaderSchema.OH_FullName.MaxLength,
						OrgAddressSchema.OA_Address1.MaxLength,
						OrgAddressSchema.OA_City.MaxLength,
						OrgAddressSchema.OA_State.MaxLength,
						OrgAddressSchema.OA_PostCode.MaxLength,
						OrgAddressSchema.OA_Phone.MaxLength,
						OrgAddressSchema.OA_Mobile.MaxLength,
						OrgAddressSchema.OA_Fax.MaxLength,
						OrgAddressSchema.OA_Email.MaxLength,
						OrgWebURLSchema.PU_URL.MaxLength,
					};

				return maxLengths.Max();
			}
		}
		#endregion

		#region TestOrgDetailsGroupBoxFilter

		public void TestOrgDetailsGroupBoxFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "BBBB";

			OrgHeader relatedConsign = Factory.NewWithValidTestData<OrgHeader>();
			relatedConsign.OH_FullName = "AAAA";

			org1.OH_IsConsignee = ZBool.True;
			relatedConsign.BuyerLinks.AddNew(org1);

			Factory.Save();

			OrgHeaderCollection matchedOrgs = new OrgHeaderCollection(Factory);

			OrganisationFilterBusinessObject filterBizO = FilterBusinessObjectFactory.New<OrganisationFilterBusinessObject>();
			filterBizO.OH_RelatedConsign = relatedConsign.PK;
			filterBizO.OH_IsConsignor = ZBool.True;
			filterBizO.OH_IsConsignee = ZBool.True;
			filterBizO.OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.FullName;
			filterBizO.OH_Details = "B";
			matchedOrgs.Load(filterBizO.Filter);

			AssertCollectionContains("Organisation should be included if it matches search criteria", org1, matchedOrgs);
			AssertCollectionNotContains("RelatedConsign should not be included if it does not match search criteria", relatedConsign, matchedOrgs);

			org1.OH_IsActive = false;
			Factory.Save();
			matchedOrgs.Load(filterBizO.Filter);
			AssertCollectionNotContains("Organisation should not be included if it is inactive", org1, matchedOrgs);
		}

		#endregion

		#region InitialiseFilterBusinessObject

		[ExpectNoExceptions()]
		public void TestInitialiseFilterBusinessObject()
		{
			BusinessObjectFactory busFactory = new BusinessObjectFactory();
			FilterObject.SetExternalDefaults(new ConsigneeForWebCollection(busFactory).FilterBusinessObjectDefaults);
			FilterObject.SetExternalDefaults(new ConsignorForWebCollection(busFactory).FilterBusinessObjectDefaults);
		}

		#endregion

		#region Implementation

		protected OrganisationFilterBusinessObject FilterObject
		{
			get
			{
				if (fFilterObject == null)
				{
					fFilterObject = (OrganisationFilterBusinessObject)GetNewBusinessObject();
				}
				return fFilterObject;
			}
		}
		OrganisationFilterBusinessObject fFilterObject;

		#endregion
	}
}
