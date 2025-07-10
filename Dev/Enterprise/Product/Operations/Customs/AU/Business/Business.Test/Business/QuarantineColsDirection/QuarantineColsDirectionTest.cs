using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineColsDirection))]
	sealed class QuarantineColsDirectionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestColsHeader()
		{
			var colsDirection = Factory.New<QuarantineColsDirection>();
			AssertNull(colsDirection.ColsHeader);

			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsDirection.QCD_QCH_ColsHeader = colsHeader.PK;
			AssertSame(colsHeader, colsDirection.ColsHeader);
		}

		public void TestContainer()
		{
			var colsDirection = Factory.New<QuarantineColsDirection>();
			AssertNull(colsDirection.Container);

			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			var container = Factory.New<CusContainer>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			quarantineColsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			colsDirection.QCD_QCH_ColsHeader = quarantineColsHeader.PK;

			colsDirection.QCD_CO_Container = container.PK;
			AssertSame(container, colsDirection.Container);
		}

		public void TestContainerReadOnly()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			var container = Factory.New<CusContainer>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			quarantineColsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;

			var colsDirection = quarantineColsHeader.Directions.AddNew();
			AssertEquals("QCD_CO_Container is not readonly when its own Entry Line has not been entered and no other existing Entry Line",
				false, colsDirection.QCD_CO_Container_ReadOnly);

			var quarantineColsHeader2 = Factory.New<QuarantineColsHeader>();
			quarantineColsHeader2.QCH_CH_CusEntryHeader = cusEntryHeader.PK;

			CusEntryLine existingEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			existingEntryLine.CL_LineNumber = 123;

			var existingColsDirection = quarantineColsHeader2.Directions.AddNew();
			existingColsDirection.QCD_CL_CusEntryLine = existingEntryLine.PK;

			colsDirection = quarantineColsHeader2.Directions.AddNew();
			AssertEquals("QCD_CO_Container is readonly when its own Entry Line has not been entered but there is other existing Entry Line",
				true, colsDirection.QCD_CO_Container_ReadOnly);

			var quarantineColsHeader3 = Factory.New<QuarantineColsHeader>();
			quarantineColsHeader3.QCH_CH_CusEntryHeader = cusEntryHeader.PK;

			CusEntryLine cusEntryLineSelf = cusEntryHeader.AllEntryLines.AddNew();
			cusEntryLineSelf.CL_LineNumber = 567;

			colsDirection = quarantineColsHeader3.Directions.AddNew();
			colsDirection.QCD_CL_CusEntryLine = cusEntryLineSelf.PK;
			AssertEquals("QCD_CO_Container is readonly when its own Entry Line has been entered",
				true, colsDirection.QCD_CO_Container_ReadOnly);
		}

		public void TestCusEntryLine()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			var colsDirection = Factory.New<QuarantineColsDirection>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			quarantineColsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			colsDirection.QCD_QCH_ColsHeader = quarantineColsHeader.PK;

			AssertNull(colsDirection.CusEntryLine);

			var cusEntryLine = Factory.New<CusEntryLine>();
			colsDirection.QCD_CL_CusEntryLine = cusEntryLine.PK;
			AssertSame(cusEntryLine, colsDirection.CusEntryLine);
		}

		public void TestCusEntryLineReadOnly()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			quarantineColsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;

			var colsDirection = quarantineColsHeader.Directions.AddNew();
			AssertEquals("QCD_CL_CusEntryLine is not readonly when its own Container Number has not been entered and no other existing Container Number",
				false, colsDirection.QCD_CL_CusEntryLine_ReadOnly);

			var quarantineColsHeader2 = Factory.New<QuarantineColsHeader>();
			quarantineColsHeader2.QCH_CH_CusEntryHeader = cusEntryHeader.PK;

			var existingContainer = Factory.New<CusContainer>();
			existingContainer.CO_ContainerNumber = "ABCD1234560";

			var existingColsDirection = quarantineColsHeader2.Directions.AddNew();
			existingColsDirection.QCD_CO_Container = existingContainer.PK;

			colsDirection = quarantineColsHeader2.Directions.AddNew();
			AssertEquals("QCD_CL_CusEntryLine is readonly when its own Container Number has not been entered but there is other existing Container Number",
				true, colsDirection.QCD_CL_CusEntryLine_ReadOnly);

			var quarantineColsHeader3 = Factory.New<QuarantineColsHeader>();
			quarantineColsHeader3.QCH_CH_CusEntryHeader = cusEntryHeader.PK;

			existingContainer = Factory.New<CusContainer>();
			existingContainer.CO_ContainerNumber = "EFGH3456780";

			colsDirection = quarantineColsHeader3.Directions.AddNew();
			colsDirection.QCD_CO_Container = existingContainer.PK;
			AssertEquals("QCD_CL_CusEntryLine is readonly when its own Container Number has been entered",
				true, colsDirection.QCD_CL_CusEntryLine_ReadOnly);
		}

		public void TestDirection()
		{
			var colsDirection = Factory.New<QuarantineColsDirection>();
			AssertNullOrEmpty(colsDirection.QCD_Direction);

			var direction = "DummyDirection";
			colsDirection.QCD_Direction = direction;
			AssertEquals(direction, colsDirection.QCD_Direction);
		}

		public void TestTreatmentType()
		{
			AssertEntity<QuarantineColsDirection>()
				.HasProperty(x => x.QCD_TreatmentType)
				.WithCaption("Treatment Type")
				.WithList("Lookups.TreatmentTypeList");

			var colsDirection = Factory.New<QuarantineColsDirection>();
			AssertNullOrEmpty(colsDirection.QCD_TreatmentType);

			var treatmentType = "DummyTreatmentType";
			colsDirection.QCD_TreatmentType = treatmentType;
			AssertEquals(treatmentType, colsDirection.QCD_TreatmentType);
		}

		public void TestAAAddress() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var colsDirection = colsHeader.Directions.AddNew();
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			colsHeader.DeliveryOrUnpack.OrganisationPK = deliveryOrg.PK;
			var deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Miscellaneous, false);
			deliveryAddress.Address1 = "Address";
			deliveryAddress.CompanyName = "Company";
			var aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "ABC12", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			colsHeader.QCH_ApprovedArrangementRefNum = "ABC12";

			var aaAddress = colsDirection.AAAddress;
			AssertEquals("New address is created", false, aaAddress.IsInDatabase);
			AssertEquals("Creating address doesn't change HasChanges", false, colsDirection.HasChanges);
			AssertEquals("E2_ParentID", colsDirection.PK, aaAddress.E2_ParentID);
			AssertEquals("E2_ParentTableCode", colsDirection.TablePrefix, aaAddress.E2_ParentTableCode);
			AssertEquals("E2_AddressType", "AAA", aaAddress.E2_AddressType);
			AssertEquals("E2_RN_NKCountryCode", "AU", aaAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_GovRegNumType", "AAN", aaAddress.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", "ABC12", aaAddress.E2_GovRegNum);
			AssertEquals("E2_CompanyName", "Company", aaAddress.E2_CompanyName);
			AssertEquals("E2_Address1", "Address", aaAddress.E2_Address1);
			AssertEquals("E2_AddressOverride", true, aaAddress.E2_AddressOverride);
			aaAddress.E2_GovRegNum = "XYZ";
			AssertEquals("Updating address sets HasChanges=true", true, colsDirection.HasChanges);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedColsDirection = newFactory.Load<QuarantineColsDirection>(colsDirection.PK);
			var loadedAddress = loadedColsDirection.AAAddress;
			AssertEquals("Address loaded from DB", aaAddress.PK, loadedAddress.PK);
			AssertEquals("Loading address doesn't change HasChanges", false, loadedColsDirection.HasChanges);
			AssertEquals("Loaded address E2_AddressType", "AAA", loadedAddress.E2_AddressType);
			AssertEquals("Loaded address E2_RN_NKCountryCode", "AU", loadedAddress.E2_RN_NKCountryCode);
			AssertEquals("Loaded address E2_GovRegNumType", "AAN", loadedAddress.E2_GovRegNumType);
			AssertEquals("Loaded address E2_GovRegNumType", "XYZ", loadedAddress.E2_GovRegNum);
		});

		public void TestDeleteDirectionDeletesAAAddress() => CombineAssertions(() =>
		{
			var colsDirection1 = (QuarantineColsDirection)GetNewBusinessObject();
			var address1 = colsDirection1.AAAddress;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedColsDirection = newFactory.Load<QuarantineColsDirection>(colsDirection1.PK);
			var loadedAddress = newFactory.Load<JobDocAddress>(address1.PK);
			AssertEquals("Before loadedColsDirection is deleted, loadedAddress is not deleted", false, loadedAddress.IsDeleted);
			loadedColsDirection.Delete();
			AssertEquals("After loadedColsDirection is deleted, loadedAddress is also deleted", true, loadedAddress.IsDeleted);

			var colsDirection2 = (QuarantineColsDirection)GetNewBusinessObject();
			var address2 = colsDirection2.AAAddress;
			AssertEquals("Before colsDirection2 is deleted, address2 is not deleted", false, address2.IsDeleted);
			colsDirection2.Delete();
			AssertEquals("After colsDirection2 is deleted, address2 is also deleted", true, address2.IsDeleted);
		});

		public void TestAAId() => CombineAssertions(() =>
		{
			AssertEntity<QuarantineColsDirection>()
				.HasProperty(x => x.AAId)
				.WithCaption("(AA) ID")
				.WithList("Lookups.AAIDList");

			var colsDirection = (QuarantineColsDirection)GetNewBusinessObject();
			AssertEquals("AAId initially empty", ZString.Empty, colsDirection.AAId);
			AssertEquals("AAId is editable", false, colsDirection.AAIdInfo.ReadOnly);
			AssertEquals("HasChanges initially false", false, colsDirection.HasChanges);
			colsDirection.AAId = "XYZ";
			AssertEquals("Updating AAId updates HasChanges to true", true, colsDirection.HasChanges);
			AssertEquals("AAId bound to AAAddress.E2_GovRegNum", "XYZ", colsDirection.AAAddress.E2_GovRegNum);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedColsDirection = newFactory.Load<QuarantineColsDirection>(colsDirection.PK);
			AssertEquals("Loaded colsDirection AAId", "XYZ", loadedColsDirection.AAId);
		});

		public void TestAALocation_Caption()
		{
			var colsDirection = Factory.New<QuarantineColsDirection>();
			AssertEquals("(AA) Location", DataBoundResourceStrings.GetDataForProperty(colsDirection.AALocationInfo).Caption);
		}

		public void TestAALocation() => CombineAssertions(() =>
		{
			var colsDirection = (QuarantineColsDirection)GetNewBusinessObject();
			AssertEquals("AALocation initially empty", ZString.Empty, colsDirection.AALocation);
			AssertEquals("AALocation is editable", false, colsDirection.AALocationInfo.ReadOnly);
			AssertEquals("HasChanges initially false", false, colsDirection.HasChanges);
			colsDirection.AALocation = "XYZ";
			AssertEquals("Updating AALocation updates HasChanges to true", true, colsDirection.HasChanges);
			AssertEquals("AALocation bound to AAAddress.E2_Address1", "XYZ", colsDirection.AAAddress.E2_Address1);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedColsDirection = newFactory.Load<QuarantineColsDirection>(colsDirection.PK);
			AssertEquals("Loaded colsDirection AALocation", "XYZ", loadedColsDirection.AALocation);
		});

		public void TestDefaultAALocationAndAAName() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var colsDirection = colsHeader.Directions.AddNew();
			var deliveryOrg = Factory.New<OrgHeader>();
			colsHeader.DeliveryOrUnpack.OrganisationPK = deliveryOrg.PK;

			var deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Miscellaneous, false);
			deliveryAddress.Address1 = "Address1";
			deliveryAddress.CompanyName = "Company1";
			var aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			colsDirection.AAId = ZString.Empty;
			colsDirection.AAId = "CODE1";
			AssertEquals("AALocation defaults to a premises address which is not Delivery or PickupAndDelivery address and is not checked as Main", "Address1", colsDirection.AALocation);
			AssertEquals("AAName defaults to the company name of a premises address which is not Delivery or PickupAndDelivery address and is not checked as Main", "Company1", colsDirection.AAName);

			var cacheKey = $"QuarantineColsDirection|AddressesWithAANCode|{deliveryOrg.PK}";
			Factory.ClearCachedValue<Dictionary<ZString, OrgAddress>>(cacheKey);
			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Miscellaneous, true);
			deliveryAddress.Address1 = "Address2";
			deliveryAddress.CompanyName = "Company2";
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			colsDirection.AAId = ZString.Empty;
			colsDirection.AAId = "CODE1";
			AssertEquals("AALocation defaults to a premises address which is not Delivery or PickupAndDelivery address and is checked as Main", "Address2", colsDirection.AALocation);
			AssertEquals("AAName defaults to the company name of a premises address which is not Delivery or PickupAndDelivery address and is checked as Main", "Company2", colsDirection.AAName);

			Factory.ClearCachedValue<Dictionary<ZString, OrgAddress>>(cacheKey);
			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Delivery, false);
			deliveryAddress.Address1 = "Address3";
			deliveryAddress.CompanyName = "Company3";
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			colsDirection.AAId = ZString.Empty;
			colsDirection.AAId = "CODE1";
			AssertEquals("AALocation defaults to Delivery address which is not checked as Main", "Address3", colsDirection.AALocation);
			AssertEquals("AAName defaults to the company name of Delivery address which is not checked as Main", "Company3", colsDirection.AAName);

			Factory.ClearCachedValue<Dictionary<ZString, OrgAddress>>(cacheKey);
			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.PickupAndDelivery, false);
			deliveryAddress.Address1 = "Address4";
			deliveryAddress.CompanyName = "Company4";
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			colsDirection.AAId = ZString.Empty;
			colsDirection.AAId = "CODE1";
			AssertEquals("AALocation defaults to PickupAndDelivery address which is not checked as Main", "Address4", colsDirection.AALocation);
			AssertEquals("AAName defaults to the company name of PickupAndDelivery address which is not checked as Main", "Company4", colsDirection.AAName);

			Factory.ClearCachedValue<Dictionary<ZString, OrgAddress>>(cacheKey);
			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Delivery, true);
			deliveryAddress.Address1 = "Address5";
			deliveryAddress.CompanyName = "Company5";
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			colsDirection.AAId = ZString.Empty;
			colsDirection.AAId = "CODE1";
			AssertEquals("AALocation defaults to Delivery address and is checked as Main", "Address5", colsDirection.AALocation);
			AssertEquals("AAName defaults to the company name of Delivery address and is checked as Main", "Company5", colsDirection.AAName);

			Factory.ClearCachedValue<Dictionary<ZString, OrgAddress>>(cacheKey);
			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			deliveryAddress.Address1 = "Address6";
			deliveryAddress.CompanyName = "Company6";
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			colsDirection.AAId = ZString.Empty;
			colsDirection.AAId = "CODE1";
			AssertEquals("AALocation defaults to PickupAndDelivery address and is checked as Main", "Address6", colsDirection.AALocation);
			AssertEquals("AAName defaults to the company name of PickupAndDelivery address and is checked as Main", "Company6", colsDirection.AAName);
		});

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var colsDirection = colsHeader.Directions.AddNew();
			return colsDirection;
		}
	}
}
