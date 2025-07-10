using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAHouse))]
	sealed class CusSCAHouseTest : EnterpriseBusinessObjectTestCase
	{
		class CusSCAHouseForTesting : CusSCAHouse
		{
			public CusSCAHouseForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void ResetCustomBusinessObjectForTesting() => ResetCustomBusinessObject();
		}

		[TestedType(typeof(CusSCAHouse))]
		sealed class CustomFieldsProviderTest : TestICustomFieldProvider
		{
		}

		public void TestWorkflowRelatedProperties()
		{
			var house = Factory.New<CusSCAHouse>();
			Assert("Should support workflow.", house.SupportsWorkflow);
			AssertEquals(typeof(CusSCAHouseProcessTaskCollection), ((IWorkflowProvider)house).WorkflowItems.GetType());
		}

		public void TestWorkflowTemplateIsApplied()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode;
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			Factory.Save();

			var house = Factory.New<CusSCAHouse>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			house.CA_CB = oceanBill.PK;
			Factory.Save();
			AssertEquals(1, ((IWorkflowProvider)house).WorkflowItems.Triggers.Count);
			AssertEquals("trigger", ((IWorkflowProvider)house).WorkflowItems.Triggers[0].P9_Description);
		}

		public void TestIWorkflowTriggerEventSourceMembers()
		{
			var house = Factory.New<CusSCAHouse>();
			AssertEquals("JobHeaderCompany", GlbCompany.CurrentCompany.PK, ((IWorkflowTriggerEventSource)house).JobHeaderCompany.PK);
			AssertEquals("ParentWorkflowProviders", 0, ((IWorkflowTriggerEventSource)house).ParentWorkflowProviders.Count);

			var shipment = Factory.New<ForwardingShipment>();
			house.CA_JS = shipment.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			AssertEquals("JobHeaderCompany", job.Company, ((IWorkflowTriggerEventSource)house).JobHeaderCompany);
			AssertEquals("ParentWorkflowProviders", 1, ((IWorkflowTriggerEventSource)house).ParentWorkflowProviders.Count);
			AssertEquals("ParentWorkflowProviders", shipment, ((IWorkflowTriggerEventSource)house).ParentWorkflowProviders[0]);

			var oceanBill = Factory.New<CusSCAOceanBill>();
			house.CA_CB = oceanBill.PK;
			AssertEquals("JobHeaderCompany", oceanBill.Branch.Company, ((IWorkflowTriggerEventSource)house).JobHeaderCompany);
		}

		public void TestCustomBusinessObject()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode;
			template1.P0_Name = "TEST 1";
			template1.P0_Description = "TEST 1 DESC";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "stringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = template1.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "intField";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			Factory.Save();

			var housebill = Factory.New<CusSCAHouseForTesting>();
			var resetCount = 0;
			housebill.OnResetCustomBusinessObject = () => resetCount++;
			ICustomFieldProvider customFieldProvider = housebill;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			AssertEquals("resetCount", 0, resetCount);
			housebill.ResetCustomBusinessObjectForTesting();
			AssertEquals("resetCount", 1, resetCount);
			var oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			customField2.Delete();
			AssertEquals("resetCount", 1, resetCount);
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			housebill.ResetCustomBusinessObjectForTesting();
			AssertEquals("resetCount", 2, resetCount);
			oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("STRINGFIELD", properties[0]);
			if (ErrorReporter.LastKeyReported == "CUS-MAWB-WorkflowProviderTypeUnknownCountry")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestImporterABNAndImporterIdentifier()
		{
			var house = Factory.New<CusSCAHouse>();
			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "Test 1";
			consignee1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
			consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");

			house.CA_OA_ConsigneeAddress = consignee1.MainAddress.PK;
			AssertEquals("", house.CA_ConsigneeBusinessNumber);
			AssertEquals("12345678901", house.CA_ConsigneeIdentifier);

			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "Test 2";
			consignee2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");

			house.CA_OA_ConsigneeAddress = consignee2.MainAddress.PK;
			AssertEquals("12345678901/123", house.CA_ConsigneeBusinessNumber);
			AssertEquals("", house.CA_ConsigneeIdentifier);

			var consignee3 = Factory.New<OrgHeader>();
			consignee3.OH_Code = "Test 3";
			consignee3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");

			house.CA_OA_ConsigneeAddress = consignee3.MainAddress.PK;
			AssertEquals("", house.CA_ConsigneeBusinessNumber);
			AssertEquals("12345678901", house.CA_ConsigneeIdentifier);

			var consignee4 = Factory.New<OrgHeader>();
			consignee4.OH_Code = "Test 4";
			consignee4.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");

			house.CA_OA_ConsigneeAddress = consignee4.MainAddress.PK;
			AssertEquals("12345678901", house.CA_ConsigneeBusinessNumber);
			AssertEquals("", house.CA_ConsigneeIdentifier);
		}

		public void TestCA_ConsigneeIdentifierAndCA_ConsigneeBusinessNumberReadOnlyWhenUnmatched()
		{
			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			var house = Factory.New<CusSCAHouse>();
			house.CA_OA_ConsigneeAddress = unmatchedOrg.MainAddress.PK;
			Assert(!house.CA_ConsigneeIdentifierInfo.ReadOnly);
			Assert(!house.CA_ConsigneeBusinessNumberInfo.ReadOnly);

			var org = Factory.New<OrgHeader>();
			house.CA_OA_ConsigneeAddress = org.MainAddress.PK;
			Assert(house.CA_ConsigneeIdentifierInfo.ReadOnly);
			Assert(house.CA_ConsigneeBusinessNumberInfo.ReadOnly);
		}

		public void TestCA_ConsignorIdentifierAndCA_VendorIdentifierReadOnlyWhenUnmatched()
		{
			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			var house = Factory.New<CusSCAHouse>();
			house.CA_OA_ConsignorAddress = unmatchedOrg.MainAddress.PK;
			Assert(!house.CA_ConsignorIdentifierInfo.ReadOnly);
			Assert(!house.CA_VendorIdentifierInfo.ReadOnly);

			var org = Factory.New<OrgHeader>();
			house.CA_OA_ConsignorAddress = org.MainAddress.PK;
			Assert(house.CA_ConsignorIdentifierInfo.ReadOnly);
			Assert(house.CA_VendorIdentifierInfo.ReadOnly);
		}

		public void TestVendorIdentifierAndConsignorIdentifier()
		{
			var house = Factory.New<CusSCAHouse>();
			AssertEquals("", house.CA_ConsignorIdentifier);
			AssertEquals("", house.CA_VendorIdentifier);

			var consignor1 = Factory.New<OrgHeader>();
			consignor1.OH_Code = "Test 1";
			house.CA_OA_ConsignorAddress = consignor1.MainAddress.PK;
			AssertEquals("", house.CA_ConsignorIdentifier);
			AssertEquals("", house.CA_VendorIdentifier);

			var consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_Code = "Test 2";
			consignor2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			consignor2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123");
			house.CA_OA_ConsignorAddress = consignor2.MainAddress.PK;
			AssertEquals("12345678901", house.CA_ConsignorIdentifier);
			AssertEquals("123", house.CA_VendorIdentifier);

			house.CA_VendorIdentifier = "0123 456 78 90/001";
			AssertEquals("01234567890001", house.CA_VendorIdentifier);
		}

		public void TestSavingWithFactoryOnDifferentDbConnection()
		{
			using (DbConnection connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();

				try
				{
					BusinessObjectFactory factoryOnExtraConnection = new BusinessObjectFactory(connection);
					CusSCAHouse house = factoryOnExtraConnection.NewWithValidTestData<CusSCAHouse>();
					Assert(house.CA_BGMReference.IsEmpty);

					BusinessObjectFactory.SaveTogether(factoryOnExtraConnection);
					Assert(!house.CA_BGMReference.IsEmpty);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestConsigneeAddressAsASingleLine()
		{
			CusSCAHouse house = Factory.New<CusSCAHouse>();
			house.CA_ConsigneeAddress1 = "26 Myrtle";
			house.CA_ConsigneeAddress2 = "Street";
			house.CA_ConsigneeSuburb = "Prospect";
			house.CA_ConsigneeState = "NSW";
			house.CA_ConsigneePostcode = "2149";

			AssertEquals("26 Myrtle Street Prospect NSW 2149", house.ConsigneeAddressAsASingleLine);

			house.CA_ConsigneeAddress2 = "";

			AssertEquals("26 Myrtle Prospect NSW 2149", house.ConsigneeAddressAsASingleLine);
		}

		public void TestConsigneeAddressDefaults()
		{
			CusSCAHouse house = Factory.New<CusSCAHouse>();

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "Cuckoo Sqkr";

			OrgAddress deliveryAddress1 = Factory.New<OrgAddress>();
			deliveryAddress1.OA_Address1 = "1 DEL ST";
			deliveryAddress1.OA_Address2 = "LINE 2";
			deliveryAddress1.OA_City = "DELIVERYVILLE";
			deliveryAddress1.OA_State = "NSW";
			deliveryAddress1.OA_PostCode = "2222";
			deliveryAddress1.OA_Phone = "1234";
			deliveryAddress1.OA_Fax = "555";
			deliveryAddress1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			deliveryAddress1.AddressCapability.SetIsNotMainAddress(OrgAddressType.Delivery.Code);
			consignee.Addresses.Add(deliveryAddress1);

			OrgAddress deliveryAddress2 = Factory.New<OrgAddress>();
			deliveryAddress2.OA_Address1 = "2 DEL ST";
			deliveryAddress2.OA_Address2 = "LINE 22";
			deliveryAddress2.OA_City = "DELIVERYVILLE2";
			deliveryAddress2.OA_State = "NSW2";
			deliveryAddress2.OA_PostCode = "3333";
			deliveryAddress2.OA_Phone = "4321";
			deliveryAddress2.OA_Fax = "666";
			deliveryAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			deliveryAddress2.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery.Code);
			consignee.Addresses.Add(deliveryAddress2);

			OrgAddress padAddress = Factory.New<OrgAddress>();
			padAddress.OA_Address1 = "1 PAD ST";
			padAddress.OA_Address2 = "LINE 2";
			padAddress.OA_City = "PADSTOW";
			padAddress.OA_State = "NSW";
			padAddress.OA_PostCode = "1111";
			padAddress.OA_Phone = "5555";
			padAddress.OA_Fax = "777";
			padAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code);
			padAddress.AddressCapability.SetIsNotMainAddress(OrgAddressType.PickupAndDelivery.Code);
			consignee.Addresses.Add(padAddress);

			house.CA_OA_ConsigneeAddress = deliveryAddress2.PK;

			AssertEquals(deliveryAddress2.OA_Address1, house.CA_ConsigneeAddress1);
			AssertEquals(deliveryAddress2.OA_Address2, house.CA_ConsigneeAddress2);
			AssertEquals(deliveryAddress2.OA_City + " " + deliveryAddress2.OA_State, house.CA_ConsigneeSuburb);
			AssertEquals(deliveryAddress2.OA_PostCode, house.CA_ConsigneePostcode);
			AssertEquals(deliveryAddress2.OA_Phone, house.CA_ConsigneePhone);
			AssertEquals(deliveryAddress2.OA_Fax, house.CA_ConsigneeFax);

			house.CA_OA_ConsigneeAddress = ZGuid.Empty;

			deliveryAddress2.AddressCapability.SetIsNotMainAddress(OrgAddressType.Delivery.Code);
			padAddress.AddressCapability.SetIsMainAddress(OrgAddressType.PickupAndDelivery.Code);

			house.CA_OA_ConsigneeAddress = padAddress.PK;

			AssertEquals(padAddress.OA_Address1, house.CA_ConsigneeAddress1);
			AssertEquals(padAddress.OA_Address2, house.CA_ConsigneeAddress2);
			AssertEquals(padAddress.OA_City + " " + padAddress.OA_State, house.CA_ConsigneeSuburb);
			AssertEquals(padAddress.OA_PostCode, house.CA_ConsigneePostcode);
			AssertEquals(padAddress.OA_Phone, house.CA_ConsigneePhone);
			AssertEquals(padAddress.OA_Fax, house.CA_ConsigneeFax);
		}

		public void TestConsignorAddressDefaultsFromHeader()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CKS";
			consignor.OH_FullName = "Cuckoo Sqkr";
			var mainAddress = consignor.MainAddress;
			mainAddress.OA_Address1 = "1 DEL ST";
			mainAddress.OA_Address2 = "LINE 2";
			mainAddress.OA_City = "DELIVERYVILLE";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_PostCode = "2222";
			mainAddress.OA_Phone = "1234";

			var house = Factory.New<CusSCAHouse>();
			house.CA_OA_ConsignorAddress = consignor.MainAddress.PK;

			AssertEquals("Cuckoo Sqkr", house.CA_ConsignorName);
			AssertEquals("1 DEL ST", house.CA_ConsignorAddress1);
			AssertEquals("LINE 2", house.CA_ConsignorAddress2);
			AssertEquals("DELIVERYVILLE NSW", house.CA_ConsignorSuburb);
			AssertEquals("2222", house.CA_ConsignorPostcode);
			AssertEquals("1234", house.CA_ConsignorPhone);
		}

		public void TestConsignorAddressDefaultsFromAddress()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CKS";
			consignor.OH_FullName = "Cuckoo Sqkr";
			var mainAddress = consignor.MainAddress;
			mainAddress.OA_Address1 = "1 DEL ST";
			mainAddress.OA_Address2 = "LINE 2";
			mainAddress.OA_City = "DELIVERYVILLE";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_PostCode = "2222";
			mainAddress.OA_Phone = "1234";

			var house = Factory.New<CusSCAHouse>();
			house.CA_OA_ConsignorAddress = mainAddress.PK;

			AssertEquals("Cuckoo Sqkr", house.CA_ConsignorName);
			AssertEquals("1 DEL ST", house.CA_ConsignorAddress1);
			AssertEquals("LINE 2", house.CA_ConsignorAddress2);
			AssertEquals("DELIVERYVILLE NSW", house.CA_ConsignorSuburb);
			AssertEquals("2222", house.CA_ConsignorPostcode);
			AssertEquals("1234", house.CA_ConsignorPhone);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			return oceanBill.HouseBills.AddNew();
		}

		const string ContainerNumber1 = "GFDS000011";
		const string ContainerNumber2 = "GFDS000022";

		public void TestContainers()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = ContainerNumber1;
			CusSCAContainer container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = ContainerNumber2;

			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();

			AssertEquals("PreCondition", 0, houseBill.Pivot.Count);
			// Adds two rows - Container and a pivot
			CusSCAPivot houseContainerPivot1 = houseBill.Pivot.AddNew();
			houseContainerPivot1.CV_AssociatedContainer = ContainerNumber1;
			AssertEquals(2, oceanBill.Containers.Count);
			AssertEquals(1, houseBill.Pivot.Count);
			CusSCAPivot houseContainerPivot2 = houseBill.Pivot.AddNew();
			houseContainerPivot2.CV_AssociatedContainer = ContainerNumber2;
			AssertEquals(2, houseBill.Pivot.Count);
			AssertEquals(2, oceanBill.Containers.Count);
			CusSCAPivot houseContainerPivot3 = houseBill.Pivot.AddNew();
			houseContainerPivot3.CV_AssociatedContainer = ContainerNumber1;
			AssertEquals(3, houseBill.Pivot.Count);
			AssertEquals(2, oceanBill.Containers.Count);
			AssertEquals("Housebill Pivot should have message errors as the same container is referenced twice", true, houseContainerPivot3.HasMessageErrors);
		}

		public void TestOceanBill()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			AssertEquals(oceanBill, houseBill.OceanBill);
		}

		public void TestOceanBillViaDirectCreationOfHouseBill()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.CA_CB = oceanBill.PK;
			AssertEquals(oceanBill, houseBill.OceanBill);
		}

		public void TestOceanBillReturnsNullWhenAccessedPriorToSetting()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			CusSCAOceanBill oceanBill = houseBill.OceanBill;
			AssertNull(oceanBill);
		}

		public void TestMakeOceanBillAnEditableChild()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill1 = oceanBill.HouseBills.AddNew();
			CusSCAHouse houseBill2 = oceanBill.HouseBills.AddNew();
			CusSCAContainer container1 = oceanBill.Containers.AddNew();

			AssertEquals("Precondition: oceanBill.Containers is registered as an editable child", true, oceanBill.IsRegisteredEditableChildObject(oceanBill.Containers));
			AssertEquals("Precondition: oceanBill.HouseBills is registered as an editable child", true, oceanBill.IsRegisteredEditableChildObject(oceanBill.HouseBills));
			AssertEquals("Precondition: oceanBill is not registered as an editable child of houseBill1", false, houseBill1.IsRegisteredEditableChildObject(oceanBill));
			AssertEquals("Precondition: oceanBill is not registered as an editable child of houseBill2", false, houseBill2.IsRegisteredEditableChildObject(oceanBill));

			houseBill2.MakeOceanBillAnEditableChild();

			AssertEquals("OceanBill is registered as an editable child of houseBill2", true, houseBill2.IsRegisteredEditableChildObject(oceanBill));
			AssertEquals("OceanBill is not registered as an editable child of houseBill1", false, houseBill1.IsRegisteredEditableChildObject(oceanBill));
			AssertEquals("HouseBills is not registered as an editable child of OceanBill", false, oceanBill.IsRegisteredEditableChildObject(oceanBill.HouseBills));
			AssertEquals("Containers is not registered as an editable child of OceanBill", false, oceanBill.IsRegisteredEditableChildObject(oceanBill.Containers));

			container1.CN_ContainerNumber = "CN1";
			Assert("Container changes do not update HasChanges on OceanBill", !oceanBill.HasChanges);
			Assert("Container changes do not update HasChanges on HouseBill2", !houseBill2.HasChanges);

			houseBill1.CA_HouseBill = "HB1";
			Assert("HouseBill1 changes do not update HasChanges on OceanBill", !oceanBill.HasChanges);
			Assert("HouseBill1 changes do not update HasChanges on HouseBill2", !houseBill2.HasChanges);

			oceanBill.CB_OceanBill = "OB1";
			Assert("OceanBill changes update HasChanges on OceanBill", oceanBill.HasChanges);
			Assert("OceanBill changes update HasChanges on HouseBill2", houseBill2.HasChanges);
		}

		public void TestIsAnEditableChildOfOceanBill()
		{
			CusSCAOceanBill oceanBill1 = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill1 = oceanBill1.HouseBills.AddNew();

			CusSCAOceanBill oceanBill2 = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container2 = oceanBill2.Containers.AddNew();

			AssertEquals("OceanBill1.HouseBills is registered as an editable child", true, oceanBill1.IsRegisteredEditableChildObject(oceanBill1.HouseBills));
			AssertEquals("OceanBill1.Containers is registered as an editable child", true, oceanBill1.IsRegisteredEditableChildObject(oceanBill1.Containers));
			AssertEquals("OceanBill2.HouseBills is registered as an editable child", true, oceanBill2.IsRegisteredEditableChildObject(oceanBill2.HouseBills));
			AssertEquals("OceanBill2.Containers is registered as an editable child", true, oceanBill2.IsRegisteredEditableChildObject(oceanBill2.Containers));
			Assert(!oceanBill1.HasChanges);
			Assert(!oceanBill2.HasChanges);

			houseBill1.CA_HouseBill = "HB1";
			Assert("HouseBill1 changes update HasChanges on OceanBill", oceanBill1.HasChanges);

			container2.CN_ContainerNumber = "CN2";
			Assert("Container changes update HasChanges on OceanBill2", oceanBill2.HasChanges);
		}

		public void TestCA_OH_NotifyAndOverride()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			OrgHeader notify = GetOrgHeaderNotify();
			notify.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			houseBill.CA_OH_Notify = ZGuid.Empty;
			AssertEquals("CA_NotifyName ReadOnly", false, houseBill.CA_NotifyNameInfo.ReadOnly);
			AssertEquals("CA_NotifyAddress1 ReadOnly", false, houseBill.CA_NotifyAddress1Info.ReadOnly);
			AssertEquals("CA_NotifyAddress2 ReadOnly", false, houseBill.CA_NotifyAddress2Info.ReadOnly);
			AssertEquals("CA_NotifySuburb ReadOnly", false, houseBill.CA_NotifySuburbInfo.ReadOnly);
			AssertEquals("CA_NotifyPostCode ReadOnly", false, houseBill.CA_NotifyPostcodeInfo.ReadOnly);
			AssertEquals("CA_NotifyPhone ReadOnly", false, houseBill.CA_NotifyPhoneInfo.ReadOnly);
			AssertEquals("CA_NotifyFax ReadOnly", false, houseBill.CA_NotifyFaxInfo.ReadOnly);
			AssertEquals("CA_RN_NKNotifyCountryCode ReadOnly", false, houseBill.CA_RN_NKNotifyCountryCodeInfo.ReadOnly);

			houseBill.CA_NotifyName = OverrideName;
			houseBill.CA_NotifyAddress1 = OverrideAddress1;
			houseBill.CA_NotifyAddress2 = OverrideAddress2;
			houseBill.CA_NotifySuburb = OverrideAddress3;
			houseBill.CA_NotifyPostcode = OverridePostCode;
			houseBill.CA_NotifyPhone = OverridePhone;
			houseBill.CA_NotifyFax = OverrideFax;
			houseBill.CA_RN_NKNotifyCountryCode = "SG";

			houseBill.CA_OH_Notify = notify.PK;
			AssertEquals("Notify Name", notify.OH_FullName.SubstringSafe(0, 35), houseBill.CA_NotifyName);
			AssertEquals("Notify Address1", notify.MainAddress.OA_Address1, houseBill.CA_NotifyAddress1);
			AssertEquals("Notify Address2", notify.MainAddress.OA_Address2, houseBill.CA_NotifyAddress2);
			AssertEquals("Notify Address3", notify.MainAddress.OA_City + " " + notify.MainAddress.OA_State, houseBill.CA_NotifySuburb);
			AssertEquals("Notify PostCode", notify.MainAddress.OA_PostCode, houseBill.CA_NotifyPostcode);
			AssertEquals("Notify Phone", notify.MainAddress.OA_Phone, houseBill.CA_NotifyPhone);
			AssertEquals("Notify Fax", notify.MainAddress.OA_Fax, houseBill.CA_NotifyFax);
			AssertEquals("Notify Country Code", "AU", houseBill.CA_RN_NKNotifyCountryCode);

			AssertEquals("CA_NotifyName ReadOnly", true, houseBill.CA_NotifyNameInfo.ReadOnly);
			AssertEquals("CA_NotifyAddress1 ReadOnly", true, houseBill.CA_NotifyAddress1Info.ReadOnly);
			AssertEquals("CA_NotifyAddress2 ReadOnly", true, houseBill.CA_NotifyAddress2Info.ReadOnly);
			AssertEquals("CA_NotifySuburbInfo ReadOnly", true, houseBill.CA_NotifySuburbInfo.ReadOnly);
			AssertEquals("CA_NotifyPostCode ReadOnly", true, houseBill.CA_NotifyPostcodeInfo.ReadOnly);
			AssertEquals("CA_NotifyPhone ReadOnly", true, houseBill.CA_NotifyPhoneInfo.ReadOnly);
			AssertEquals("CA_NotifyFax ReadOnly", true, houseBill.CA_NotifyFaxInfo.ReadOnly);
			AssertEquals("CA_RN_NKNotifyCountryCode ReadOnly", true, houseBill.CA_RN_NKNotifyCountryCodeInfo.ReadOnly);

			houseBill.CA_OH_Notify = ZGuid.Empty;
			AssertEquals("CA_NotifyName ReadOnly", false, houseBill.CA_NotifyNameInfo.ReadOnly);
			AssertEquals("CA_NotifyAddress1 ReadOnly", false, houseBill.CA_NotifyAddress1Info.ReadOnly);
			AssertEquals("CA_NotifyAddress2 ReadOnly", false, houseBill.CA_NotifyAddress2Info.ReadOnly);
			AssertEquals("CA_NotifySuburb ReadOnly", false, houseBill.CA_NotifySuburbInfo.ReadOnly);
			AssertEquals("CA_NotifyPostCode ReadOnly", false, houseBill.CA_NotifyPostcodeInfo.ReadOnly);
			AssertEquals("CA_NotifyPhone ReadOnly", false, houseBill.CA_NotifyPhoneInfo.ReadOnly);
			AssertEquals("CA_NotifyFax ReadOnly", false, houseBill.CA_NotifyFaxInfo.ReadOnly);
			AssertEquals("CA_RN_NKNotifyCountryCode ReadOnly", false, houseBill.CA_RN_NKNotifyCountryCodeInfo.ReadOnly);

			AssertEquals("HouseBill Notify Name", NotifyName.SubstringSafe(0, 35).Trim(' '), houseBill.CA_NotifyName.Trim(' '));
			AssertEquals("HouseBill Notify Address1", notify.MainAddress.OA_Address1, houseBill.CA_NotifyAddress1);
			AssertEquals("HouseBill Notify Address2", notify.MainAddress.OA_Address2, houseBill.CA_NotifyAddress2);
			AssertEquals("HouseBill Notify Suburb", notify.MainAddress.OA_City + " " + notify.MainAddress.OA_State, houseBill.CA_NotifySuburb);
			AssertEquals("HouseBill Notify PostCode", notify.MainAddress.OA_PostCode, houseBill.CA_NotifyPostcode);
			AssertEquals("HouseBill Notify Phone", notify.MainAddress.OA_Phone, houseBill.CA_NotifyPhone);
			AssertEquals("HouseBill Notify Fax", notify.MainAddress.OA_Fax, houseBill.CA_NotifyFax);
			AssertEquals("Notify Country Code", "AU", houseBill.CA_RN_NKNotifyCountryCode);
		}

		[ExpectNoExceptions()]
		public void TestHouseBillValidationDoesNotCauseStackOverflow()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.RunPreSaveValidation();
			houseBill.Validation.ValidateCA_HouseBill();
		}

		public void TestReadOnly()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			AssertEquals("House ReadOnly", false, houseBill.ReadOnly);
			AssertEquals("Pivot ReadOnly", false, houseBill.Pivot.ReadOnly);

			houseBill.ReadOnly = true;
			AssertEquals("House ReadOnly", true, houseBill.ReadOnly);
			AssertEquals("Pivot ReadOnly", true, houseBill.Pivot.All(pivot => pivot.ReadOnly));
		}

		public void TestSynchronisedFieldsAreReadOnlyWhenOverrideFreightDefaultsIsDisabled()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			AssertEquals(true, oceanBill.OverrideFreightDefaults);

			AssertEquals(false, houseBill.CA_HouseBillInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_RL_NK_PortOfDestinationInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_RL_NK_PortOfOriginInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_IsMasterHouseInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_MasterHouseBillInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_PrepaidCollectOtherInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_RN_NKGoodsOriginInfo.ReadOnly);

			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			AssertEquals(false, oceanBill.OverrideFreightDefaults);

			AssertEquals(true, houseBill.CA_HouseBillInfo.ReadOnly);
			AssertEquals(true, houseBill.CA_RL_NK_PortOfDestinationInfo.ReadOnly);
			AssertEquals(true, houseBill.CA_RL_NK_PortOfOriginInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_IsMasterHouseInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_MasterHouseBillInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_PrepaidCollectOtherInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_RN_NKGoodsOriginInfo.ReadOnly);

			oceanBill.OverrideFreightDefaults = true;

			AssertEquals(false, houseBill.CA_HouseBillInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_RL_NK_PortOfDestinationInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_RL_NK_PortOfOriginInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_IsMasterHouseInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_MasterHouseBillInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_PrepaidCollectOtherInfo.ReadOnly);
			AssertEquals(false, houseBill.CA_RN_NKGoodsOriginInfo.ReadOnly);
		}

		public void TestUnderBondValidation()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.Validation.RunPreUnderbondValidation();
			Assert("Pre-Condition, Container Underbond From/To are not set", houseBill.CA_MoveUnderbondFromInfo.HasMessageErrors() && houseBill.CA_MoveUnderbondToInfo.HasMessageErrors());
			houseBill.CA_MoveUnderbondFrom = "34DT";
			houseBill.Validation.RunPreUnderbondValidation();
			Assert("Container Underbond From is set, To is not set, but From is set incorecctly. Should both be in error", houseBill.CA_MoveUnderbondFromInfo.HasMessageErrors() && houseBill.CA_MoveUnderbondToInfo.HasMessageErrors());
			houseBill.CA_MoveUnderbondTo = "87E";
			houseBill.Validation.RunPreUnderbondValidation();
			Assert("Container Underbond From is set, To is set, but From/To are set incorecctly. Should both be in error", houseBill.CA_MoveUnderbondFromInfo.HasMessageErrors() && houseBill.CA_MoveUnderbondToInfo.HasMessageErrors());
			houseBill.CA_MoveUnderbondFrom = "S039D";
			houseBill.Validation.RunPreUnderbondValidation();
			Assert("Container Underbond From is set correctly, To is set, but is set incorecctly. To should be in error", !houseBill.CA_MoveUnderbondFromInfo.HasMessageErrors() && houseBill.CA_MoveUnderbondToInfo.HasMessageErrors());
			houseBill.CA_MoveUnderbondTo = "FA76C";
			houseBill.Validation.RunPreUnderbondValidation();
			Assert("Container Underbond From is set correctly, To is set correctly. No errors", !houseBill.CA_MoveUnderbondFromInfo.HasMessageErrors() && !houseBill.CA_MoveUnderbondToInfo.HasMessageErrors());
		}

		public void TestConsignorNameIsTrimmedto35inLength()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			OrgHeader consignor = GetOrgHeaderConsignor();

			houseBill.CA_OA_ConsignorAddress = consignor.MainAddress.PK;
			AssertEquals("Consignor Name should be trimmed to 35 as that is the limit for the messaging when entered as an organisation", ConsignorName.SubstringSafe(0, 35), houseBill.CA_ConsignorName);
		}

		public void TestConsigneeNameIsTrimmedto35inLength()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			OrgHeader consignee = GetOrgHeaderConsignee();

			houseBill.CA_OA_ConsigneeAddress = consignee.MainAddress.PK;
			AssertEquals("Consignee Name should be trimmed to 35 as that is the limit for the messaging when entered as an organisation", ConsigneeName.SubstringSafe(0, 35), houseBill.CA_ConsigneeName);
		}

		public void TestNotifyPartyNameIsTrimmedto35inLength()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			OrgHeader notify = GetOrgHeaderNotify();

			houseBill.CA_OH_Notify = notify.PK;
			AssertEquals("Consignee Name should be trimmed to 35 as that is the limit for the messaging when entered as an organisation", NotifyName.SubstringSafe(0, 35), houseBill.CA_NotifyName);
		}

		public void TestConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "BIG BUSINESS INC.";
			consignee.MainAddress.OA_Address1 = "LEVEL 48 BIG BUILDING";

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_OA_ConsigneeAddress = consignee.MainAddress.PK;

			AssertEquals(consignee.PK, houseBill.Consignee.PK);
		}

		public void TestConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "BIG BUSINESS INC.";
			consignor.MainAddress.OA_Address1 = "LEVEL 48 BIG BUILDING";

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_OA_ConsignorAddress = consignor.MainAddress.PK;

			AssertEquals(consignor.PK, houseBill.Consignor.PK);
		}

		public void TestCA_OA_ConsignorAddressAndOverride()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			OrgHeader consignor = GetOrgHeaderConsignor();
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";

			houseBill.CA_OA_ConsignorAddress = ZGuid.Empty;
			AssertEquals("CA_ConsignorName ReadOnly", false, houseBill.CA_ConsignorNameInfo.ReadOnly);
			AssertEquals("CA_ConsingorAddress1 ReadOnly", false, houseBill.CA_ConsignorAddress1Info.ReadOnly);
			AssertEquals("CA_ConsingorAddress2 ReadOnly", false, houseBill.CA_ConsignorAddress2Info.ReadOnly);
			AssertEquals("CA_ConsignorSuburbInfo ReadOnly", false, houseBill.CA_ConsignorSuburbInfo.ReadOnly);
			AssertEquals("CA_ConsingorPostCode ReadOnly", false, houseBill.CA_ConsignorPostcodeInfo.ReadOnly);
			AssertEquals("CA_RN_NKConsignorCountryCode ReadOnly", false, houseBill.CA_RN_NKConsignorCountryCodeInfo.ReadOnly);

			houseBill.CA_ConsignorName = OverrideName;
			houseBill.CA_ConsignorAddress1 = OverrideAddress1;
			houseBill.CA_ConsignorAddress2 = OverrideAddress2;
			houseBill.CA_ConsignorSuburb = OverrideAddress3;
			houseBill.CA_ConsignorPostcode = OverridePostCode;
			houseBill.CA_RN_NKConsignorCountryCode = "NZ";

			houseBill.CA_OA_ConsignorAddress = consignor.MainAddress.PK;
			AssertEquals("Consignor", consignor, houseBill.Consignor);
			AssertEquals("Consignor Name", consignor.OH_FullName.SubstringSafe(0, 35), houseBill.CA_ConsignorName);
			AssertEquals("Consignor Address1", consignor.MainAddress.OA_Address1, houseBill.CA_ConsignorAddress1);
			AssertEquals("Consignor Address2", consignor.MainAddress.OA_Address2, houseBill.CA_ConsignorAddress2);
			AssertEquals("Consignor Address3", consignor.MainAddress.OA_City + " " + consignor.MainAddress.OA_State, houseBill.CA_ConsignorSuburb.Trim());
			AssertEquals("Consignor PostCode", consignor.MainAddress.OA_PostCode, houseBill.CA_ConsignorPostcode);
			AssertEquals("Consignor Country Code", "US", houseBill.CA_RN_NKConsignorCountryCode);

			AssertEquals("CA_ConsignorName ReadOnly", true, houseBill.CA_ConsignorNameInfo.ReadOnly);
			AssertEquals("CA_ConsingorAddress1 ReadOnly", true, houseBill.CA_ConsignorAddress1Info.ReadOnly);
			AssertEquals("CA_ConsingorAddress2 ReadOnly", true, houseBill.CA_ConsignorAddress2Info.ReadOnly);
			AssertEquals("CA_ConsignorSuburb ReadOnly", true, houseBill.CA_ConsignorSuburbInfo.ReadOnly);
			AssertEquals("CA_ConsingorPostCode ReadOnly", true, houseBill.CA_ConsignorPostcodeInfo.ReadOnly);
			AssertEquals("CA_RN_NKConsingorCountryCode ReadOnly", true, houseBill.CA_RN_NKConsignorCountryCodeInfo.ReadOnly);

			houseBill.CA_OA_ConsignorAddress = ZGuid.Empty;
			AssertEquals("CA_ConsignorName ReadOnly", false, houseBill.CA_ConsignorNameInfo.ReadOnly);
			AssertEquals("CA_ConsingorAddress1 ReadOnly", false, houseBill.CA_ConsignorAddress1Info.ReadOnly);
			AssertEquals("CA_ConsingorAddress2 ReadOnly", false, houseBill.CA_ConsignorAddress2Info.ReadOnly);
			AssertEquals("CA_ConsignorSuburb ReadOnly", false, houseBill.CA_ConsignorSuburbInfo.ReadOnly);
			AssertEquals("CA_ConsingorPostCode ReadOnly", false, houseBill.CA_ConsignorPostcodeInfo.ReadOnly);
			AssertEquals("CA_RN_NKConsingorCountryCode ReadOnly", false, houseBill.CA_RN_NKConsignorCountryCodeInfo.ReadOnly);

			AssertEquals("Consignor", null, houseBill.Consignor);
			AssertEquals("HouseBill Consignor Name", ConsignorName.SubstringSafe(0, 35).Trim(' '), houseBill.CA_ConsignorName.Trim(' '));
			AssertEquals("HouseBill Consignor Address1", consignor.MainAddress.OA_Address1, houseBill.CA_ConsignorAddress1);
			AssertEquals("HouseBill Consignor Address2", consignor.MainAddress.OA_Address2, houseBill.CA_ConsignorAddress2);
			AssertEquals("HouseBill Consignor Address3", consignor.MainAddress.OA_City + " " + consignor.MainAddress.OA_State, houseBill.CA_ConsignorSuburb.Trim());
			AssertEquals("HouseBill Consignor PostCode", consignor.MainAddress.OA_PostCode, houseBill.CA_ConsignorPostcode);
			AssertEquals("Consignor Country Code", "US", houseBill.CA_RN_NKConsignorCountryCode);
		}

		public void TestCA_OA_ConsigneeAndOverride()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			OrgHeader consignee = GetOrgHeaderConsignee();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			houseBill.CA_OA_ConsigneeAddress = ZGuid.Empty;
			AssertEquals("CA_ConsigneeName ReadOnly", false, houseBill.CA_ConsigneeNameInfo.ReadOnly);
			AssertEquals("CA_ConsigneeAddress1 ReadOnly", false, houseBill.CA_ConsigneeAddress1Info.ReadOnly);
			AssertEquals("CA_ConsigneeAddress2 ReadOnly", false, houseBill.CA_ConsigneeAddress2Info.ReadOnly);
			AssertEquals("CA_ConsigneeSuburb ReadOnly", false, houseBill.CA_ConsigneeSuburbInfo.ReadOnly);
			AssertEquals("CA_ConsigneePostCode ReadOnly", false, houseBill.CA_ConsigneePostcodeInfo.ReadOnly);
			AssertEquals("CA_ConsigneePhone ReadOnly", false, houseBill.CA_ConsigneePhoneInfo.ReadOnly);
			AssertEquals("CA_ConsigneeFax ReadOnly", false, houseBill.CA_ConsigneeFaxInfo.ReadOnly);
			AssertEquals("CA_RN_NKConsigneeCountryCode", false, houseBill.CA_RN_NKConsigneeCountryCodeInfo.ReadOnly);

			houseBill.CA_ConsigneeName = OverrideName;
			houseBill.CA_ConsigneeAddress1 = OverrideAddress1;
			houseBill.CA_ConsigneeAddress2 = OverrideAddress2;
			houseBill.CA_ConsigneeSuburb = OverrideAddress3;
			houseBill.CA_ConsigneePostcode = OverridePostCode;
			houseBill.CA_ConsigneePhone = OverridePhone;
			houseBill.CA_ConsigneeFax = OverrideFax;
			houseBill.CA_RN_NKConsigneeCountryCode = "NZ";

			houseBill.CA_OA_ConsigneeAddress = consignee.MainAddress.PK;
			AssertEquals("Consignee", consignee, houseBill.Consignee);
			AssertEquals("Consignee Name", consignee.OH_FullName.SubstringSafe(0, 35), houseBill.CA_ConsigneeName);
			AssertEquals("Consignee Address1", consignee.MainAddress.OA_Address1, houseBill.CA_ConsigneeAddress1);
			AssertEquals("Consignee Address2", consignee.MainAddress.OA_Address2, houseBill.CA_ConsigneeAddress2);
			AssertEquals("Consignee Address3", consignee.MainAddress.OA_City + " " + consignee.MainAddress.OA_State, houseBill.CA_ConsigneeSuburb);
			AssertEquals("Consignee PostCode", consignee.MainAddress.OA_PostCode, houseBill.CA_ConsigneePostcode);
			AssertEquals("Consignee Phone", consignee.MainAddress.OA_Phone, houseBill.CA_ConsigneePhone);
			AssertEquals("Consignee Fax", consignee.MainAddress.OA_Fax, houseBill.CA_ConsigneeFax);
			AssertEquals("Consignee Country Code", "AU", houseBill.CA_RN_NKConsigneeCountryCode);

			AssertEquals("CA_ConsigneeName ReadOnly", true, houseBill.CA_ConsigneeNameInfo.ReadOnly);
			AssertEquals("CA_ConsigneeAddress1 ReadOnly", true, houseBill.CA_ConsigneeAddress1Info.ReadOnly);
			AssertEquals("CA_ConsigneeAddress2 ReadOnly", true, houseBill.CA_ConsigneeAddress2Info.ReadOnly);
			AssertEquals("CA_ConsigneeSuburbInfo ReadOnly", true, houseBill.CA_ConsigneeSuburbInfo.ReadOnly);
			AssertEquals("CA_ConsigneePostCode ReadOnly", true, houseBill.CA_ConsigneePostcodeInfo.ReadOnly);
			AssertEquals("CA_ConsigneePhone ReadOnly", true, houseBill.CA_ConsigneePhoneInfo.ReadOnly);
			AssertEquals("CA_ConsigneeFax ReadOnly", true, houseBill.CA_ConsigneeFaxInfo.ReadOnly);
			AssertEquals("CA_RN_NKConsigneeCountryCode", true, houseBill.CA_RN_NKConsigneeCountryCodeInfo.ReadOnly);

			houseBill.CA_OA_ConsigneeAddress = ZGuid.Empty;
			AssertEquals("CA_ConsigneeName ReadOnly", false, houseBill.CA_ConsigneeNameInfo.ReadOnly);
			AssertEquals("CA_ConsigneeAddress1 ReadOnly", false, houseBill.CA_ConsigneeAddress1Info.ReadOnly);
			AssertEquals("CA_ConsigneeAddress2 ReadOnly", false, houseBill.CA_ConsigneeAddress2Info.ReadOnly);
			AssertEquals("CA_ConsigneeSuburb ReadOnly", false, houseBill.CA_ConsigneeSuburbInfo.ReadOnly);
			AssertEquals("CA_ConsigneePostCode ReadOnly", false, houseBill.CA_ConsigneePostcodeInfo.ReadOnly);
			AssertEquals("CA_ConsigneePhone ReadOnly", false, houseBill.CA_ConsigneePhoneInfo.ReadOnly);
			AssertEquals("CA_ConsigneeFax ReadOnly", false, houseBill.CA_ConsigneeFaxInfo.ReadOnly);
			AssertEquals("CA_RN_NKConsigneeCountryCode", false, houseBill.CA_RN_NKConsigneeCountryCodeInfo.ReadOnly);

			AssertEquals("Consignee", null, houseBill.Consignee);
			AssertEquals("HouseBill Consignee Name", ConsigneeName.SubstringSafe(0, 35).Trim(' '), houseBill.CA_ConsigneeName.Trim(' '));
			AssertEquals("HouseBill Consignee Address1", consignee.MainAddress.OA_Address1, houseBill.CA_ConsigneeAddress1);
			AssertEquals("HouseBill Consignee Address2", consignee.MainAddress.OA_Address2, houseBill.CA_ConsigneeAddress2);
			AssertEquals("HouseBill Consignee Address3", consignee.MainAddress.OA_City + " " + consignee.MainAddress.OA_State, houseBill.CA_ConsigneeSuburb);
			AssertEquals("HouseBill Consignee PostCode", consignee.MainAddress.OA_PostCode, houseBill.CA_ConsigneePostcode);
			AssertEquals("HouseBill Consignee Phone", consignee.MainAddress.OA_Phone, houseBill.CA_ConsigneePhone);
			AssertEquals("HouseBill Consignee Fax", consignee.MainAddress.OA_Fax, houseBill.CA_ConsigneeFax);
			AssertEquals("Consignee Country Code", "AU", houseBill.CA_RN_NKConsigneeCountryCode);
		}

		public void TestIDocManagerSupportIncudingRelatedObjectsMembers()
		{
			var house = Factory.New<CusSCAHouse>();
			var underbond = house.AllUnderbonds.AddNew();
			var supporter = house as IDocManagerSupportIncudingRelatedObjects;
			AssertEquals("Self reference", house, supporter.SelfReference);
			AssertEquals(1, supporter.GetRelatedBusinessObjects().Count());
			Assert(supporter.GetRelatedBusinessObjects().Contains(underbond));
		}

		public void TestDocManagerInfo()
		{
			var house = Factory.New<CusSCAHouse>();
			DocManagerInfo docManagerInfo = ((IDocManagerSupport)house).DocManagerInfo;
			AssertType(typeof(DocManagerIncludingRelatedObjectsInfo), docManagerInfo);
			AssertEquals("SCH", docManagerInfo.DocManagerCode);
		}

		public void TestGetAllPossibleCollectionProvidersShouldNotHaveNulls()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.Pivot.AddNew();

			AssertEquals("Should not contain any nulls", true, ((ICusUnderbondUnionCollectionParent)houseBill).GetAllPossibleCollectionProviders().All(x => x != null));
		}

		public void TestGetHVLVStatusMapping()
		{
			var houseBill = Factory.New<CusSCAHouse>();
			AssertEquals("HVLV Status = HLD", "HLD", GetHVLVStatusMappingForTest(houseBill, "MIX"));
			AssertEquals("HVLV Status = HLD", "HLD", GetHVLVStatusMappingForTest(houseBill, "HLD"));
			AssertEquals("HVLV Status = HLD", "HLD", GetHVLVStatusMappingForTest(houseBill, "ACZ"));
			AssertEquals("HVLV Status = HLD", "HLD", GetHVLVStatusMappingForTest(houseBill, "SUB"));
			AssertEquals("HVLV Status = HLD", "HLD", GetHVLVStatusMappingForTest(houseBill, "TRT"));
			AssertEquals("HVLV Status = HLD", "HLD", GetHVLVStatusMappingForTest(houseBill, "DCL"));
			AssertEquals("HVLV Status = CLR", "CLR", GetHVLVStatusMappingForTest(houseBill, "CLR"));
			AssertEquals("HVLV Status = CLR", "CLR", GetHVLVStatusMappingForTest(houseBill, "CLH"));
			AssertEquals("HVLV Status = GAR", "GAR", GetHVLVStatusMappingForTest(houseBill, "AQZ"));
			AssertEquals("HVLV Status = GAR", "GAR", GetHVLVStatusMappingForTest(houseBill, "CCL"));
			AssertEquals("HVLV Status = TRS", "TRS", GetHVLVStatusMappingForTest(houseBill, "TRS"));
			AssertEquals("HVLV Status = TRS", "TRS", GetHVLVStatusMappingForTest(houseBill, "TRH"));
			AssertEquals("HVLV Status is empty", "", GetHVLVStatusMappingForTest(houseBill, "ZZZ"));
		}

		string GetHVLVStatusMappingForTest(CusSCAHouse houseBill, ZString status)
		{
			var getHVLVStatusMappingMethodInfo = typeof(CusSCAHouse).GetMethod("GetHVLVStatusMapping", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
			return getHVLVStatusMappingMethodInfo.Invoke(houseBill, new object[] { status }).ToString();
		}

		public void TestGetReason()
		{
			const string CARSTHeldMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1GAG D03A D06F:1+8'
DTM+9:20051110003152681932:ZZZ'
DTM+132:20051110:102'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:TEST'
TDT +20+006++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+8553P::95'
NAD+MR+FGH939C::95'
NAD+UD+83003926181::95'
RFF+ABO:321/PRD1::1'
RFF+MWB:08145322174'
RFF+HWB:V0014102928'
UNT+34+000001'
";

			AssertReason("Has Reason", "CONDITIONAL RELEASE TEST", CARSTHeldMessage);

			const string CARSTHeldMessage2 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1GAG D03A D06F:1+8'
DTM+9:20051110003152681932:ZZZ'
DTM+132:20051110:102'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
TDT +20+006++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+8553P::95'
NAD+MR+FGH939C::95'
NAD+UD+83003926181::95'
RFF+ABO:321/PRD1::1'
RFF+MWB:08145322174'
RFF+HWB:V0014102928'
UNT+34+000001'
";

			AssertReason("No Reason", "", CARSTHeldMessage2);
		}

		void AssertReason(ZString message, ZString expectedReason, ZString carstHeldMessage)
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			var container = oceanBill.Containers.AddNew();
			var pivot = houseBill.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			pivot.CV_CargoStatus = "CCL";

			var ediMessage = Factory.New<CMRCARSTMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = carstHeldMessage.Replace("\r\n", "");
			Factory.Save();
			pivot.Messages.AddFromDatabase(ediMessage.PK);
			Factory.Save();

			var getReason = typeof(CusSCAHouse).GetMethod("GetReason", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

			AssertEquals(message, expectedReason, getReason.Invoke(houseBill, System.Array.Empty<object>()));
		}

		public void TestMessagesForBinding()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			house.Messages.AddNew();
			var pivot1 = house.Pivot.AddNew();
			pivot1.Messages.AddNew();
			var pivot2 = house.Pivot.AddNew();
			pivot2.Messages.AddNew();
			AssertEquals(3, house.MessagesForBinding.Count);
			house.Messages.AddNew();
			AssertEquals(4, house.MessagesForBinding.Count);
			house.Messages.RemoveAll();
			AssertEquals(2, house.MessagesForBinding.Count);
		}

		public void TestShipmentWithMultipleCusSCAHouse()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			oceanBill.CB_OceanBill = "OC1";
			var cusSCAHouse = oceanBill.HouseBills.AddNew();
			cusSCAHouse.CA_JS = shipment.PK;
			Factory.Save();
			AssertNullOrEmpty("No Last Message Reported", ErrorReporter.LastMessageReported);

			cusSCAHouse.CA_AuthenticationCode = "AA";
			Factory.Save();
			AssertNullOrEmpty("No Last Message Reported", ErrorReporter.LastMessageReported);

			var cusSCAHouse2 = oceanBill.HouseBills.AddNew();
			cusSCAHouse2.CA_JS = shipment.PK;

			Factory.Save();
			AssertContains("Last Message Reported", "ShipmentNumber=" + shipment.JS_UniqueConsignRef, ErrorReporter.LastMessageReported);
			AssertContains("Last Message Reported", "OceanBillNumber=OC1", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var query = new ZDBOnlyQuery(typeof(CusSCAHouse));
			query.AddToFilter(CusSCAHouseSchema.CA_CB, oceanBill.PK);
			query.AddToFilter(CusSCAHouseSchema.CA_JS, shipment.PK);
			var existingHouseBills = (CusSCAHouse[])consol.Factory.Load(typeof(CusSCAHouse), query);
			AssertEquals(existingHouseBills.Length, 2);
			AssertEquals(existingHouseBills[0].PK, cusSCAHouse.PK);
			AssertEquals(existingHouseBills[1].PK, cusSCAHouse2.PK);
		}

		public void TestHouseBillsAfterUniversalShipmentImportSaved()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			oceanBill.CB_OceanBill = "OC1";

			// simulate shipment added by UMI Service Task
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB001";
			consol.Factory.Save();
			AssertEquals("OceanBill has no HouseBill", 0, oceanBill.HouseBills.Count);

			var user1Factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var user1Consol = user1Factory.Load<ForwardingConsol>(consol.PK);
			var user1SeaCargoSynchroniser = new CMRSeaCargoSynchroniser(user1Consol);
			var user1OceanBill = user1SeaCargoSynchroniser.OceanBill;
			var user1HouseBill = user1OceanBill.HouseBills[0];
			AssertEquals("New HouseBill from Shipment", shipment.PK, user1HouseBill.CA_JS);

			var user2Factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var user2Consol = user2Factory.Load<ForwardingConsol>(consol.PK);
			var user2SeaCargoSynchroniser = new CMRSeaCargoSynchroniser(user2Consol);
			var user2OceanBill = user2SeaCargoSynchroniser.OceanBill;
			var user2HouseBill = user2OceanBill.HouseBills[0];
			AssertEquals("New HouseBill from Shipment", shipment.PK, user2HouseBill.CA_JS);

			user1Factory.Save();
			user2Factory.Save();

			oceanBill.HouseBills.Reload(true);
			AssertEquals("OceanBill has just one HouseBill", 1, oceanBill.HouseBills.Count);
		}

		public void TestCA_ShipmentStatus()
		{
			var house = Factory.New<CusSCAHouse>();
			house.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			house.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			AssertEquals(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, house.CA_ShipmentStatus);
			house.Pivot[0].CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals(CMRConsolidatedCargoStatuses.Codes.SeePackingDetails, house.CA_ShipmentStatus);
		}

		public void TestDefaultFromShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB1";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB2";
			var packing = shipment.OuterPackLines.AddNew();
			packing.SetContainer(container.PK);
			packing.JL_PackageCount = 10;
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = oceanBill.HouseBills.AddNew();
			house.CA_JS = shipment.PK;
			house.DefaultFromShipment();
			AssertEquals("HB1", house.CA_HouseBill);
			AssertEquals(10, house.Pivot[0].CV_PackageCount);
			AssertNull("Synchronising one shipment does not affect other shipments", shipment2.AUCusSCAHouse);
		}

		public void TestAddHLSShipmentMessagingEventIfRequired()
		{
			var house = Factory.New<CusSCAHouse>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			house.CA_JS = shipment.PK;
			var logs = new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady);
			AssertEquals(0, logs.Count);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(1, logs.Count);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(1, logs.Count);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals(2, logs.Count);
		}

		public void TestDoNotAddHVLShipmentMessagingEvent_ForHLSShipment()
		{
			var house = Factory.New<CusSCAHouse>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			house.CA_JS = shipment.PK;

			var logs = new LogsForNominatedEvent(shipment.GetLogs(), Events.HVLVReady);
			AssertEquals("Precondition: No HLR logs on the HVLV shipment", 0, logs.Count);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("There should be no HLR event log for HVLV shipment if the HouseBill's message status changes", 0, logs.Count);
			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(0, logs.Count);
			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals(0, logs.Count);
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		#region Implementation

		const string OverrideName = "Override Name";
		const string OverrideAddress1 = "Override Address 1";
		const string OverrideAddress2 = "Override Address 2";
		const string OverrideAddress3 = "Override Address 3";
		const string OverridePostCode = "Override1";
		const string OverridePhone = "54321";
		const string OverrideFax = "12345";

		static readonly ZString ConsignorName = "COR_Consignor,_This is longer_then_35_Characters";
		static readonly ZString ConsigneeName = "CEE Consignee, This is longer_then_35_Characters";
		static readonly ZString NotifyName = "NOT Notify, This is longer_then_35_Characters";

		OrgHeader GetOrgHeaderConsignor()
		{
			OrgHeader result = GetDefaultOrgHeader(ConsignorName);
			result.OH_IsConsignor = true;
			return result;
		}

		OrgHeader GetOrgHeaderConsignee()
		{
			OrgHeader result = GetDefaultOrgHeader(ConsigneeName);
			result.OH_IsConsignee = true;
			return result;
		}

		OrgHeader GetOrgHeaderNotify()
		{
			OrgHeader result = GetDefaultOrgHeader(NotifyName);
			return result;
		}

		OrgHeader GetDefaultOrgHeader(string iD)
		{
			string prefix = iD;
			if (prefix.Length > 3)
			{
				prefix = prefix.Substring(0, 3);
			}
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = iD;
			result.MainAddress.OA_Address1 = prefix + "Address Line 1";
			result.MainAddress.OA_Address2 = prefix + "Address Line 2";
			result.MainAddress.OA_City = prefix + "City";
			result.MainAddress.OA_State = "A State";
			result.MainAddress.OA_Phone = prefix + "12345";
			result.MainAddress.OA_Fax = prefix + "54321";
			return result;
		}

		#endregion

	}
}
