using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineColsDirectionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAAIdList() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var colsDirection = colsHeader.Directions.AddNew();
			var deliveryOrg = Factory.New<OrgHeader>();
			var address1 = deliveryOrg.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			address1.Address1 = "First Address";
			var address2 = deliveryOrg.Addresses.AddNew(OrgAddressType.Miscellaneous, true);
			address2.Address1 = "Second Address";
			var address3 = deliveryOrg.Addresses.AddNew(OrgAddressType.Delivery, true);
			address3.Address1 = "Third Address";
			var address4 = deliveryOrg.Addresses.AddNew(OrgAddressType.Pickup, true);
			address4.Address1 = "Fourth Address";
			colsHeader.DeliveryOrUnpack.OrganisationPK = deliveryOrg.PK;
			var code1 = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			code1.OK_OA_PremisesAddress = address1.PK;
			var code2 = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			code2.OK_OA_PremisesAddress = address2.PK;
			var code3 = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE2", Core.Constants.CountryCodes.Australia);
			code3.OK_OA_PremisesAddress = address3.PK;
			var code4 = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE3", Core.Constants.CountryCodes.Australia);
			code4.OK_OA_PremisesAddress = address4.PK;

			var list = colsDirection.Lookups.AAIDList;
			AssertContainsExactElementsInExactOrder("List is sorted and no duplicate codes", "CODE1, CODE2, CODE3", list.CodesAsString);
			AssertEquals("Description for CODE1", "First Address", list.GetDescriptionFromCode("CODE1"));
			AssertEquals("Description for CODE2", "Third Address", list.GetDescriptionFromCode("CODE2"));
			AssertEquals("Description for CODE3", "Fourth Address", list.GetDescriptionFromCode("CODE3"));
			AssertSame("Cached", list, colsDirection.Lookups.AAIDList);
		});

		public void TestTreatmentTypeList() => CombineAssertions(() =>
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType("COLTT", "COLS - Direction Treatment Type", "AU");
			helper.CreateNewOrGetExistingCusCodeList("AU", "COLTT", "CODE1", "DESC1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("AU", "COLTT", "CODE2", "DESC2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("AU", "COLTT", "CODE3", "DESC3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var colsDirection = Factory.New<QuarantineColsDirection>();
			var list = (ZZRefCusCodeListCombinedCollection)colsDirection.Lookups.TreatmentTypeList;
			list.Load();
			AssertContainsExactElementsInExactOrder("List is sorted", "CODE1, CODE2, CODE3", $"{list[0].ZZD_Code}, {list[1].ZZD_Code}, {list[2].ZZD_Code}");
			AssertSame("Cached", list, colsDirection.Lookups.TreatmentTypeList);
		});

		public void TestDirectionsList() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("COLRT", "COLS - Direction Request Type", "AU");
			_ = helper.CreateNewOrGetExistingCusCodeList("AU", "COLRT", "5", "Export from Australia (incl. transhipment)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList("AU", "COLRT", "1", "Release on Documents", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList("AU", "COLRT", "3", "Inspection of Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var colsDirection = Factory.New<QuarantineColsDirection>();
			var list = colsDirection.Lookups.DirectionsList;
			AssertContainsExactElementsInExactOrder("List is sorted", "Export from Australia (incl. transhipment), Inspection of Goods, Release on Documents", list.CodesAsString);
			AssertSame("Cached", list, colsDirection.Lookups.DirectionsList);
		});

		public void TestContainersList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			var quarantineColsDirection = Factory.New<QuarantineColsDirection>();

			cusEntryHeader.CH_JE = jobDeclaration.PK;
			quarantineColsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			quarantineColsDirection.QCD_QCH_ColsHeader = quarantineColsHeader.PK;

			var list = quarantineColsDirection.Lookups.ContainersList;
			AssertEquals("DirectionsGridContainerList Count is 0 when no container is configured", 0, list.Count());

			var container0 = jobDeclaration.CusContainers.AddNew();
			var container1 = jobDeclaration.CusContainers.AddNew();
			var container2 = jobDeclaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "EFGH5678902";
			container1.CO_ContainerNumber = "ABCD1234560";
			container2.CO_ContainerNumber = "XYZA3456785";

			Factory.Save();
			list = quarantineColsDirection.Lookups.ContainersList;
			AssertEquals("DirectionsGridContainerList Count is correct after containers are configured", 3, list.Count());

			AssertEquals("DirectionsGridContainerList Container Numbers are correct after sorted.",
				"ABCD1234560, EFGH5678902, XYZA3456785",
				string.Join(", ", list.Select(e => e.CO_ContainerNumber)));

			AssertSame("Accessing the list twice should get the same lis since cache is applied", list, quarantineColsDirection.Lookups.ContainersList);
		}

		public void TestDirectionsGridEntryLineList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			var quarantineColsDirection = Factory.New<QuarantineColsDirection>();

			cusEntryHeader.CH_JE = jobDeclaration.PK;
			quarantineColsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			quarantineColsDirection.QCD_QCH_ColsHeader = quarantineColsHeader.PK;

			var list = quarantineColsDirection.Lookups.EntryLinesList;
			AssertEquals("DirectionsGridEntryLineList Count is 0 when no Entry Line is configured", 0, list.Count());

			var cusEntryLine0 = cusEntryHeader.AllEntryLines.AddNew();
			var cusEntryLine1 = cusEntryHeader.AllEntryLines.AddNew();
			var cusEntryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine0.CL_LineNumber = 32767;
			cusEntryLine1.CL_LineNumber = 12;
			cusEntryLine2.CL_LineNumber = 45;

			Factory.Save();
			list = quarantineColsDirection.Lookups.EntryLinesList;
			AssertEquals("DirectionsGridEntryLineList Count is correct after Entry Lines are configured", 3, list.Count());

			AssertEquals("DirectionsGridEntryLineList Entry Lines are correct after sorted.",
				"12, 45, 32767",
				string.Join(", ", list.Select(e => e.CL_LineNumber)));

			AssertSame("Accessing the list twice should get the same lis since cache is applied", list, quarantineColsDirection.Lookups.EntryLinesList);
		}
	}
}
