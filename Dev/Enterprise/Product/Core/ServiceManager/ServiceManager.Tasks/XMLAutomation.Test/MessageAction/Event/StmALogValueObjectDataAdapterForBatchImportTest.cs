using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[TestedType(typeof(StmALogValueObjectDataAdapterForBatchImportTestClass))]
	sealed class StmALogValueObjectDataAdapterForBatchImportTest : StmALogValueObjectDataAdapterTest
	{
		public void TestImportWithLinking()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Xsd.Event value = new Xsd.Event() { DateTime = ZDateTime.Today };
			Xsd.EventReferenceKey refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.ConsigneeCode;
			refKey.Value = org.OH_Code;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			value.Code = "ZZ~";
			StmALog log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNull("StmALog", log);
			AssertContains("Unknown code (Event code 'ZZ~')", context.LastNotificationMessage);

			value.Code = Events.Delivered.Code;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNull("StmALog", log);
			AssertContains("Event 'DLV' - Referenced record number is not specified", context.LastNotificationMessage);

			refKey.ReferenceKeyName = Xsd.ReferenceType.ShipmentJobNumber;
			refKey.Value = "ABIRVALG";
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNull("StmALog", log);
			AssertContains("Event 'DLV' - Unable to find referenced record 'ABIRVALG'", context.LastNotificationMessage);

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			refKey.Value = shipment.JS_UniqueConsignRef;

			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(shipment.PK, log.SL_Parent);

			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			refKey.ReferenceKeyName = Xsd.ReferenceType.DeclarationJobNumber;
			refKey.Value = declaration.JE_DeclarationReference;

			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(declaration.PK, log.SL_Parent);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			refKey.ReferenceKeyName = Xsd.ReferenceType.ConsolNumber;
			refKey.Value = consol.JK_UniqueConsignRef;

			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(consol.PK, log.SL_Parent);

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "asdffwefrqewrf";
			refKey.ReferenceKeyName = Xsd.ReferenceType.MasterBill;
			refKey.Value = consol.JK_MasterBillNum;

			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(consol.PK, log.SL_Parent);

			Order order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "asdkjayhsdkjahsd";
			order1.JD_OrderNumberSplit = 0;
			Order order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "asdkjayhsdkjahsd";
			order2.JD_OrderNumberSplit = 2;
			Order order3 = Factory.NewWithValidTestData<Order>();
			order3.JD_OrderNumber = "asdkjayhsdkjahsd";
			order3.JD_OrderNumberSplit = 1;
			value.Code = Events.ContainerPack.Code;
			refKey.ReferenceKeyName = Xsd.ReferenceType.OrderNumber;
			refKey.Value = order1.JD_OrderNumber;

			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(order2.PK, log.SL_Parent);

			OrgHeader matchingOrg = Factory.Load<OrgHeader>(context.Converter.MappingOrgPK);
			OrgPatternMatchOverride eventCodeOverride = matchingOrg.CreatePatternMatchOverrideForTest();
			eventCodeOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.EventCode;
			eventCodeOverride.OO_LocalCode = Events.ContainerPack.Code;
			eventCodeOverride.OO_ForeignCode = "Y12";
			//Factory.Save();
			value.Code = "Y12";
			StmALog log2 = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log2);
			AssertEquals(order2.PK, log2.SL_Parent);
			AssertEquals(log, log2);

			value.DateTime = ZDateTime.Now.AddSeconds(-10);
			log2 = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log2);
			AssertEquals(order2.PK, log2.SL_Parent);
			AssertNotEquals(log, log2);

			Order order4 = Factory.NewWithValidTestData<Order>();
			order4.JD_OrderNumber = "AAAAA";
			order4.JD_OrderNumberSplit = 1;
			value.Code = Events.ContainerManifestExported.Code;
			refKey.ReferenceKeyName = Xsd.ReferenceType.OrderNumber;
			refKey.Value = order4.JD_OrderNumberAndSplit;

			StmALog log3 = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log3);
			AssertEquals(order4.PK, log3.SL_Parent);

			Order order5 = Factory.NewWithValidTestData<Order>();
			order5.JD_OrderNumber = "AAAAA-";
			order5.JD_OrderNumberSplit = 0;
			value.Code = Events.ContainerManifestExported.Code;
			refKey.ReferenceKeyName = Xsd.ReferenceType.OrderNumber;
			refKey.Value = order5.JD_OrderNumberAndSplit;

			StmALog log4 = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log4);
			AssertEquals(order5.PK, log4.SL_Parent);

			Order order6 = Factory.NewWithValidTestData<Order>();
			order6.JD_OrderNumber = "AAAAA-10";
			order6.JD_OrderNumberSplit = 0;
			value.Code = Events.ContainerManifestExported.Code;
			refKey.ReferenceKeyName = Xsd.ReferenceType.OrderNumber;
			refKey.Value = order6.JD_OrderNumberAndSplit;

			StmALog log5 = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log5);
			AssertEquals(order6.PK, log5.SL_Parent);

			Order order7 = Factory.NewWithValidTestData<Order>();
			order7.JD_OrderNumber = "AAAAA@-1";
			order7.JD_OrderNumberSplit = 2;
			value.Code = Events.ContainerManifestExported.Code;
			refKey.ReferenceKeyName = Xsd.ReferenceType.OrderNumber;
			refKey.Value = order7.JD_OrderNumberAndSplit;

			StmALog log6 = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log6);
			AssertEquals(order7.PK, log6.SL_Parent);
		}

		public void TestGetConsolByUniqueRefLoadsForwardingConsol()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Xsd.Event value = new Xsd.Event();
			Xsd.EventReferenceKey refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.ConsigneeCode;
			refKey.Value = org.OH_Code;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			value.Code = Events.WorkflowTriggerEventCode;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			refKey.ReferenceKeyName = Xsd.ReferenceType.ConsolNumber;
			refKey.Value = consol.JK_UniqueConsignRef;

			Factory.Save();

			var log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			Assert(log.Master is IWorkflowProvider);
			AssertEquals(log.Master.GetType(), consol.GetType());
		}

		public void TestLogProcessedWhenDeleted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			DataAdapter.TestParent = shipment;

			var incomingDate = new ZDateTime(2020, 10, 29, 11, 58, 47);
			var indbDate = new ZDateTime(2020, 10, 28);

			var log1 = Factory.New<StmALog>();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_SE_NKEvent = Events.PickupCartageAdvised.Code;
				log1.SL_IsEstimate = false;
				log1.SL_Parent = shipment.PK;
				log1.SL_Table = "JobShipment";
				log1.SL_EventTime = indbDate;
			}
			Factory.Save();

			Xsd.Event eventValue = new Xsd.Event();
			eventValue.DateTime = incomingDate;
			eventValue.Code = Events.PickupCartageAdvised.Code;

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.EventReferenceKey refKey = eventValue.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.ShipmentJobNumber;
			refKey.Value = shipment.JS_UniqueConsignRef;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(eventValue, new ValueObjectImportContext(Factory, notify)));
		}

		public void TestGetConsolByMasterBillLoadsForwardingConsol()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Xsd.Event value = new Xsd.Event();
			Xsd.EventReferenceKey refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.ConsigneeCode;
			refKey.Value = org.OH_Code;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			value.Code = Events.WorkflowTriggerEventCode;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "SDSD55589";
			refKey.ReferenceKeyName = Xsd.ReferenceType.MasterBill;
			refKey.Value = consol.JK_MasterBillNum;

			Factory.Save();

			var log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			Assert(log.Master is IWorkflowProvider);
			AssertEquals(log.Master.GetType(), consol.GetType());
		}

		public void TestImportCusEntryNumbers()
		{
			DummyCustomsEntryNumsParent parentWithSupport = Factory.New<DummyCustomsEntryNumsParent>();

			Xsd.Event value = new Xsd.Event();
			value.Code = Events.Delivered.Code;
			Xsd.EventReferenceKey refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.TariffLookup;
			refKey.Value = parentWithSupport.PK.ToString();

			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.CustomsEntryNumber;
			refKey.ReferenceKeyType = "CAN";
			refKey.Value = "111";
			refKey.ReferenceKeyCountry = Constants.CountryCodes.Australia;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			DataAdapter.TestParent = parentWithSupport;
			StmALog log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(parentWithSupport.PK, log.SL_Parent);
			AssertEquals("CAN", parentWithSupport.NumberType);
			AssertEquals("111", parentWithSupport.Value);
			AssertEquals(Constants.CountryCodes.Australia, parentWithSupport.CountryCode);

			parentWithSupport = Factory.New<DummyCustomsEntryNumsParent>();
			value = new Xsd.Event();
			value.Code = Events.Delivered.Code;
			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.TariffLookup;
			refKey.Value = parentWithSupport.PK.ToString();

			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.CustomsEntryNumber;
			refKey.ReferenceKeyType = "CAN";
			refKey.Value = "111";

			DataAdapter.TestParent = parentWithSupport;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(parentWithSupport.PK, log.SL_Parent);
			AssertEquals("", parentWithSupport.NumberType);
			AssertEquals("", parentWithSupport.Value);
			AssertEquals("", parentWithSupport.CountryCode);
			AssertContains("'Country/Region' of customs entry number with value '111' must be provided", context.LastNotificationMessage);

			DummyEnterpriseBusinessObject parent = Factory.New<DummyEnterpriseBusinessObject>();
			value = new Xsd.Event();
			value.Code = Events.Delivered.Code;
			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.TariffLookup;
			refKey.Value = parent.PK.ToString();

			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.CustomsEntryNumber;
			refKey.ReferenceKeyType = "CAN";
			refKey.Value = "111";
			refKey.ReferenceKeyCountry = Constants.CountryCodes.Australia;

			DataAdapter.TestParent = parent;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(parent.PK, log.SL_Parent);
			AssertContains("No support for customs entry numbers", context.LastNotificationMessage);

			parentWithSupport = Factory.New<DummyCustomsEntryNumsParent>();
			value = new Xsd.Event();
			value.Code = Events.Delivered.Code;
			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.TariffLookup;
			refKey.Value = parentWithSupport.PK.ToString();

			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.CustomsEntryNumber;
			refKey.ReferenceKeyType = "PMT";
			refKey.Value = "Japan Permit6";
			refKey.ReferenceKeyCountry = Constants.CountryCodes.Japan;
			ZDateTime currentDateTime = ZDateTime.Now;
			refKey.ReferenceKeyDateTime = currentDateTime;

			DataAdapter.TestParent = parentWithSupport;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(parentWithSupport.PK, log.SL_Parent);
			AssertEquals("PMT", parentWithSupport.NumberType);
			AssertEquals("Japan Permit6", parentWithSupport.Value);
			AssertEquals(currentDateTime, parentWithSupport.DateTime);
			AssertEquals(Constants.CountryCodes.Japan, parentWithSupport.CountryCode);
		}

		public void TestImportAttitionalRefNumbers()
		{
			DummyAdditionalNumsParent parentWithSupport = Factory.New<DummyAdditionalNumsParent>();

			Xsd.Event value = new Xsd.Event();
			value.Code = Events.Delivered.Code;
			Xsd.EventReferenceKey refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.TariffLookup;
			refKey.Value = parentWithSupport.PK.ToString();

			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.AdditionalReferenceNumber;
			refKey.ReferenceKeyType = "CAN";
			refKey.Value = "111";
			refKey.ReferenceKeyCountry = Constants.CountryCodes.Australia;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			DataAdapter.TestParent = parentWithSupport;
			StmALog log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(parentWithSupport.PK, log.SL_Parent);
			AssertEquals("CAN", parentWithSupport.NumberType);
			AssertEquals("111", parentWithSupport.Value);
			AssertEquals(Constants.CountryCodes.Australia, parentWithSupport.CountryCode);

			parentWithSupport = Factory.New<DummyAdditionalNumsParent>();
			value = new Xsd.Event();
			value.Code = Events.Delivered.Code;
			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.TariffLookup;
			refKey.Value = parentWithSupport.PK.ToString();

			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.AdditionalReferenceNumber;
			refKey.ReferenceKeyType = "CAN";
			refKey.Value = "111";

			DataAdapter.TestParent = parentWithSupport;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(parentWithSupport.PK, log.SL_Parent);
			AssertEquals("", parentWithSupport.NumberType);
			AssertEquals("", parentWithSupport.Value);
			AssertEquals("", parentWithSupport.CountryCode);
			AssertContains("'Country/Region' of additional reference number with value '111' must be provided", context.LastNotificationMessage);

			refKey.ReferenceKeyName = Xsd.ReferenceType.AdditionalReferenceNumber;
			refKey.ReferenceKeyType = "";
			refKey.Value = "111";
			refKey.ReferenceKeyCountry = Constants.CountryCodes.Australia;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertEquals("", parentWithSupport.NumberType);
			AssertEquals("", parentWithSupport.Value);
			AssertEquals("", parentWithSupport.CountryCode);
			AssertContains("'Type' of additional reference number with value '111' must be provided", context.LastNotificationMessage);

			refKey.ReferenceKeyName = Xsd.ReferenceType.AdditionalReferenceNumber;
			refKey.ReferenceKeyType = "CANADHlkhslAJSLakjslAKJSLKjsalkAJSLKas";
			refKey.Value = "111";
			refKey.ReferenceKeyCountry = Constants.CountryCodes.Australia;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertEquals("", parentWithSupport.NumberType);
			AssertEquals("", parentWithSupport.Value);
			AssertEquals("", parentWithSupport.CountryCode);
			AssertContains("additional reference number", context.LastNotificationMessage);

			refKey.ReferenceKeyName = Xsd.ReferenceType.AdditionalReferenceNumber;
			refKey.ReferenceKeyType = "CAN";
			refKey.Value = "111'akfr;werp;w34oir2p30i5rw24itfwsp;ogfuvesoirugtae5ygwo8gytaweygt";
			refKey.ReferenceKeyCountry = Constants.CountryCodes.Australia;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertEquals("", parentWithSupport.NumberType);
			AssertEquals("", parentWithSupport.Value);
			AssertEquals("", parentWithSupport.CountryCode);
			AssertContains("additional reference number", context.LastNotificationMessage);

			DummyEnterpriseBusinessObject parent = Factory.New<DummyEnterpriseBusinessObject>();
			value = new Xsd.Event();
			value.Code = Events.Delivered.Code;
			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.TariffLookup;
			refKey.Value = parent.PK.ToString();

			refKey = value.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = Xsd.ReferenceType.AdditionalReferenceNumber;
			refKey.ReferenceKeyType = "CAN";
			refKey.Value = "111";
			refKey.ReferenceKeyCountry = Constants.CountryCodes.Australia;

			DataAdapter.TestParent = parent;
			log = DataAdapter.CreateOrUpdateFromValueObject(value, context);
			AssertNotNull("StmALog", log);
			AssertEquals(parent.PK, log.SL_Parent);
			AssertContains("No support for additional reference numbers", context.LastNotificationMessage);
		}

		#region Implementation

		StmALogValueObjectDataAdapterForBatchImportTestClass DataAdapter;

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new StmALogValueObjectDataAdapterForBatchImportTestClass();
		}

		#endregion

		#region Test Classes

		class StmALogValueObjectDataAdapterForBatchImportTestClass : StmALogValueObjectDataAdapterForBatchImport
		{
			public StmALogValueObjectDataAdapterForBatchImportTestClass()
				: base()
			{
			}

			protected override void CreateReferenceTypesMap(Dictionary<Xsd.ReferenceType, GetBusinessObjectByReference> referenceGetters)
			{
				base.CreateReferenceTypesMap(referenceGetters);
				referenceGetters.Add(Xsd.ReferenceType.TariffLookup, GetTestParent);
			}

			BusinessObject GetTestParent(ZString reference, IValueObjectImportContext context)
			{
				return TestParent;
			}

			public BusinessObject TestParent { get; set; }
		}

		class DummyParent : DummyEnterpriseBusinessObject
		{
			public DummyParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString NumberType;
			public ZString CountryCode;
			public ZString Value;
			public ZDateTime DateTime;
		}

		class DummyCustomsEntryNumsParent : DummyParent, ICusEntryNumberSupporter
		{
			public DummyCustomsEntryNumsParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ICusEntryNumberSupporter Members

			void ICusEntryNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
			{
				NumberType = numberType;
				CountryCode = countryCode;
				Value = value;
			}

			#endregion

			public void UpdateCustomsEntryIssueDate(ZString numberType, ZString countryCode, ZDateTime dateTime)
			{
				DateTime = dateTime;
			}
		}

		class DummyAdditionalNumsParent : DummyParent, IAdditionalReferenceNumberSupporter
		{
			public DummyAdditionalNumsParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IAdditionalReferenceNumberSupporter Members

			void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
			{
				NumberType = numberType;
				CountryCode = countryCode;
				Value = value;
			}

			bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
			{
				get { return false; }
			}

			void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
			{
			}

			CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
			{
				get { throw new System.NotImplementedException(); }
			}

			void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
			{
			}

			#endregion
		}

		#endregion
	}
}
