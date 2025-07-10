using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class DataObjectExtensionsTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestHasRecipientRole()
		{
			Shipment dataObject = null;
			Assert(!dataObject.HasRecipientRole(RecipientRoleType.HCA));

			dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			Assert(!dataObject.HasRecipientRole(RecipientRoleType.HCA));

			var dataContext = DataContextFactory.New();
			dataObject.DataContext = dataContext;
			Assert(!dataObject.HasRecipientRole(RecipientRoleType.HCA));

			dataContext.SetWorkflowInfo(new WorkflowInfo()
				{
					RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.HCA } }
				});
			Assert(dataObject.HasRecipientRole(RecipientRoleType.HCA));
		}

		public void TestHasRecipientRoleAndService()
		{
			Shipment dataObject = null;
			Assert(!dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWD));

			dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			Assert(!dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWD));

			var dataContext = DataContextFactory.New();
			dataObject.DataContext = dataContext;
			Assert(!dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWD));

			dataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.DTW } } });
			Assert(!dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWD));

			dataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			Assert(!dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWD));

			dataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			Assert(!dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWD));

			dataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			Assert(dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWD));
		}

		public void TestOrganizationAddressFirstOrDefault()
		{
			var list = new List<OrganizationAddress>();
			list.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "ABC", CompanyName = "BOB" });
			list.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "DEF", CompanyName = "JANE" });
			list.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "ABC", CompanyName = "JOE" });
			AssertNull(list.FirstOrDefault("BDG"));
			AssertEquals("JANE", list.FirstOrDefault("DEF").CompanyName);
			AssertEquals("BOB", list.FirstOrDefault("ABC").CompanyName);
			AssertEquals("JANE", list.FirstOrDefault("DEF", "ABC").CompanyName);
			AssertEquals("BOB", list.FirstOrDefault().CompanyName);
			list = null;
			AssertNull(list.FirstOrDefault("BDG"));
		}

		public void TestGetOrCreateCodeDictionary()
		{
			IDictionary<ZString, List<EntryType>> dictionary = null;
			List<EntryType> list = null;
			AssertNotNull(list.GetOrCreateCodeDictionary(ref dictionary, (x) => x));
			AssertEquals("dictionary.Count", 0, dictionary.Count);
			list = new List<EntryType>
			{
				new EntryType { Code = "ABC", Description = "BOB" },
				new EntryType { Code = "DEF", Description = "JANE" },
				new EntryType { Code = "ABC", Description = "JOE" },
				new EntryType { Code = "JKL", Description = "MIKE" },
			};
			dictionary = null;
			AssertNotNull(list.GetOrCreateCodeDictionary(ref dictionary, (x) => x));
			AssertEquals("dictionary.Count", 3, dictionary.Count);
			var codeList = dictionary["ABC"];
			AssertEquals("codeList.Count", 2, codeList.Count);
			AssertEquals("codeList[0].Description", "BOB", codeList[0].Description);
			AssertEquals("codeList[1].Description", "JOE", codeList[1].Description);
			codeList = dictionary["DEF"];
			AssertEquals("codeList.Count", 1, codeList.Count);
			AssertEquals("codeList[0].Description", "JANE", codeList[0].Description);
			codeList = dictionary["JKL"];
			AssertEquals("codeList.Count", 1, codeList.Count);
			AssertEquals("codeList[0].Description", "MIKE", codeList[0].Description);
		}

		public void TestGetOrCreateCodeDictionary_GenericKey()
		{
			IDictionary<TransportTypeCode, List<TransportMeans>> dictionary = null;
			List<TransportMeans> list = null;
			AssertNotNull(list.GetOrCreateCodeDictionary(ref dictionary, (x) => (TransportTypeCode)x.TransportType));
			AssertEquals("dictionary.Count", 0, dictionary.Count);
			list = new List<TransportMeans>
			{
				new TransportMeans { TransportType = TransportTypeCode.Inland, IdentificationNumber = "BOB" },
				new TransportMeans { TransportType = TransportTypeCode.Active, IdentificationNumber = "JANE" },
				new TransportMeans { TransportType = TransportTypeCode.Inland, IdentificationNumber = "JOE" },
				new TransportMeans { TransportType = TransportTypeCode.Equipment, IdentificationNumber = "MIKE" },
			};
			dictionary = null;
			AssertNotNull(list.GetOrCreateCodeDictionary(ref dictionary, (x) => (TransportTypeCode)x.TransportType));
			AssertEquals("dictionary.Count", 3, dictionary.Count);
			var codeList = dictionary[TransportTypeCode.Inland];
			AssertEquals("codeList.Count", 2, codeList.Count);
			AssertEquals("codeList[0].IdentificationNumber", "BOB", codeList[0].IdentificationNumber);
			AssertEquals("codeList[1].IdentificationNumber", "JOE", codeList[1].IdentificationNumber);
			codeList = dictionary[TransportTypeCode.Active];
			AssertEquals("codeList.Count", 1, codeList.Count);
			AssertEquals("codeList[0].IdentificationNumber", "JANE", codeList[0].IdentificationNumber);
			codeList = dictionary[TransportTypeCode.Equipment];
			AssertEquals("codeList.Count", 1, codeList.Count);
			AssertEquals("codeList[0].IdentificationNumber", "MIKE", codeList[0].IdentificationNumber);
		}

		public void TestCodeDataObjectFirstOrDefault()
		{
			var list = new List<EntryType>();
			list.Add(new EntryType() { Code = "ABC", Description = "BOB" });
			list.Add(new EntryType() { Code = "DEF", Description = "JANE" });
			AssertNull(list.FirstOrDefault("BDG"));
			AssertEquals("JANE", list.FirstOrDefault("DEF").Description);
			AssertEquals("BOB", list.FirstOrDefault("ABC").Description);
			AssertEquals("JANE", list.FirstOrDefault("DEF", "ABC").Description);
			AssertEquals("BOB", list.FirstOrDefault().Description);
			list = null;
			AssertNull(list.FirstOrDefault("BDG"));
		}

		public void TestDateFirstOrDefault()
		{
			var list = new List<Date>();
			var date1 = Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2012, 2, 1));
			list.Add(date1);
			var date2 = Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2012, 2, 2));
			list.Add(date2);
			var date3 = Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2012, 3, 1));
			list.Add(date3);
			var date4 = Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2012, 3, 2));
			list.Add(date4);

			AssertEquals(true, object.ReferenceEquals(date2, list.FirstOrDefault(DateType.Arrival, ZBool.True)));
			AssertEquals(true, object.ReferenceEquals(date1, list.FirstOrDefault(DateType.Arrival, ZBool.False)));
			AssertNull(list.FirstOrDefault(DateType.Delivery, ZBool.False));
		}

		public void TestDateAdd()
		{
			var list = new List<Date>();
			var bizObj = Factory.New<DummyBusinessObject>();
			var addedDate = list.Add(DateType.Arrival, ZBool.True, bizObj.Z0_Date);
			AssertEquals(addedDate, list.AssertDateExists(DateType.Arrival, ZBool.True, ZDateTime.Empty));

			var value = ZDateTime.BrettsBirthday.AddDays(1);
			bizObj.Z0_Date = value;
			addedDate = list.Add(DateType.AvailableExFactory, ZBool.True, bizObj.Z0_Date);
			AssertEquals(addedDate, list.AssertDateExists(DateType.AvailableExFactory, ZBool.True, value));
		}

		public void TestGetMatchedByCandidateKey()
		{
			var list = new List<CustomizedField>();
			var item1 = list.Add("HELLO", new ZString("WORLD"));
			var item2 = list.Add("HI", new ZString("WORLD"));

			var newItem = CustomizedField.New("HELLO", new ZString("BOB"));
			var matchedItem = list.GetMatchedByCandidateKey(newItem);
			AssertEquals(true, object.ReferenceEquals(item1, matchedItem));

			newItem = CustomizedField.New("BYE", new ZString("WORLD"));
			AssertNull(list.GetMatchedByCandidateKey(newItem));
		}

		public void TestCustomizedFieldAdd()
		{
			var list = new List<CustomizedField>();
			var item = list.Add("HELLO", new ZString("WORLD"));
			AssertEquals(1, list.Count);
			AssertEquals(item, list[0]);
			list.AssertCustomFieldWasExported(DataType.String, "HELLO", "WORLD");
		}

		public void TestCustomizedField_Over80characters_IncreasedMaximunLengthTo214748367()
		{
			var list = new List<CustomizedField>();
			var testString = "12345678910111213141516171819202122232425262728293031323334353637383940414243444";
			var item = list.Add("HELLO", new ZString(testString));
			AssertEquals(1, list.Count);
			AssertEquals(item, list[0]);
			list.AssertCustomFieldWasExported(DataType.String, "HELLO", testString);
		}

		public void TestOrganizationHasTypeOnly()
		{
			CombineAssertions(() =>
			{
				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "ABC", CompanyName = "BOB" };
				AssertEquals("Meaningful address", false, addressData.HasTypeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE" };
				AssertEquals("Only addressType", true, addressData.HasTypeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", Address1 = "1" };
				AssertEquals("addressType with some field", false, addressData.HasTypeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", Address1 = "", AddressOverride = true };
				AssertEquals("addressType with some empty field and override flag", true, addressData.HasTypeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", AddressOverride = true };
				AssertEquals("addressType with override flag only", true, addressData.HasTypeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", OrganizationCode = "code", AddressOverride = true };
				AssertEquals("addressType with orgcode and override flag", false, addressData.HasTypeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", OrganizationCode = "code", AddressOverride = false };
				AssertEquals("addressType with orgcode and false override flag", false, addressData.HasTypeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", OrganizationCode = "code" };
				AssertEquals("addressType with orgcode", false, addressData.HasTypeOnly());
			});
		}

		public void TestOrganizationHasTypeAndCodeOnly()
		{
			CombineAssertions(() =>
			{
				var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "ABC", CompanyName = "BOB" };
				AssertEquals("Meaningful address", false, addressData.HasTypeAndCodeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE" };
				AssertEquals("Only addressType", true, addressData.HasTypeAndCodeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", Address1 = "1" };
				AssertEquals("addressType with some field", false, addressData.HasTypeAndCodeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", Address1 = "", AddressOverride = true };
				AssertEquals("addressType with some empty field and override flag", true, addressData.HasTypeAndCodeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", AddressOverride = true };
				AssertEquals("addressType with override flag only", true, addressData.HasTypeAndCodeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", OrganizationCode = "code", AddressOverride = true };
				AssertEquals("addressType with orgcode and override flag", true, addressData.HasTypeAndCodeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", OrganizationCode = "code", AddressOverride = false };
				AssertEquals("addressType with orgcode and false override flag", true, addressData.HasTypeAndCodeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", OrganizationCode = "code" };
				AssertEquals("addressType with orgcode", true, addressData.HasTypeAndCodeOnly());

				addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "TSTCODE", OrganizationCode = "code", Port = new UNLOCO() { Code = "AUSYD" } };
				AssertEquals("addressType with orgcode and Port", false, addressData.HasTypeAndCodeOnly());
			});
		}

		public void TestEqualsDateObject()
		{
			var shipmentX = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			Assert("A shipment should be equals to itself", shipmentX.EqualsDateObject(shipmentX));
			Assert("A shipment should not be equals to Null", !shipmentX.EqualsDateObject(null));

			var shipmentY = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			shipmentX.ActualChargeable = 10;
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.ActualChargeable = 11;
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.ActualChargeable = 10;
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			shipmentX.VesselName = "12345";
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.VesselName = "12346";
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.VesselName = "12345";
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			shipmentX.MessageType = new CodeDescriptionPair() { Code = "FTZ" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.MessageType = new CodeDescriptionPair() { Code = "IMP" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.MessageType = new CodeDescriptionPair() { Code = "FTZ" };
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			shipmentX.CountryOfSupply = new Country() { Code = "AU", Name = "Australia" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.CountryOfSupply = new Country() { Code = "US", Name = "Australia" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.CountryOfSupply = new Country() { Code = "AU", Name = "Australia" };
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			shipmentX.CommercialInfo = new CommercialInfo();
			shipmentX.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			shipmentY.CommercialInfo = new CommercialInfo();
			shipmentY.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			var invoiceX = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceX.InvoiceDate = new ZDateTime(2015, 6, 30);
			shipmentX.CommercialInfo.CommercialInvoiceCollection.Add(invoiceX);
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			var invoiceY = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentY.CommercialInfo.CommercialInvoiceCollection.Add(invoiceY);
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			invoiceY.InvoiceDate = new ZDateTime(2015, 6, 30);
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			invoiceX.Supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "ABC", CompanyName = "BOB" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			invoiceY.Supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "EDF", CompanyName = "BOB" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			invoiceY.Supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = "ABC", CompanyName = "BOB" };
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			invoiceX.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLineX1 = new CommercialInvoiceLine();
			invoiceX.CommercialInvoiceLineCollection.Add(invoiceLineX1);
			invoiceLineX1.EntryLineNumber = 1;
			invoiceLineX1.ContainerMode = new ContainerMode() { Code = "LB", Description = "Liquid Bulk" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			invoiceY.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLineY1 = new CommercialInvoiceLine();
			invoiceY.CommercialInvoiceLineCollection.Add(invoiceLineY1);
			invoiceLineY1.EntryLineNumber = 1;
			invoiceLineY1.ContainerMode = new ContainerMode() { Code = "LB", Description = "Liquid Bulk" };
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			var invoiceLineX2 = new CommercialInvoiceLine();
			invoiceX.CommercialInvoiceLineCollection.Add(invoiceLineX2);
			invoiceLineX2.EntryLineNumber = 2;
			invoiceLineX2.ContainerMode = new ContainerMode() { Code = "DB", Description = "Dry Bulk" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			var invoiceLineY2 = new CommercialInvoiceLine();
			invoiceY.CommercialInvoiceLineCollection.Add(invoiceLineY2);
			invoiceLineY2.EntryLineNumber = 1;
			invoiceLineY2.ContainerMode = new ContainerMode() { Code = "LB", Description = "Liquid Bulk" };
			Assert("Two shipments should not be equals", !shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should not be equals", !shipmentY.EqualsDateObject(shipmentX));
			invoiceLineY2.EntryLineNumber = 2;
			invoiceLineY2.ContainerMode = new ContainerMode() { Code = "DB", Description = "Dry Bulk" };
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));

			invoiceY.CommercialInvoiceLineCollection.Remove(invoiceLineY1);
			invoiceY.CommercialInvoiceLineCollection.Remove(invoiceLineY2);
			invoiceY.CommercialInvoiceLineCollection.Add(invoiceLineY2);
			invoiceY.CommercialInvoiceLineCollection.Add(invoiceLineY1);
			Assert("Two shipments should be equals", shipmentX.EqualsDateObject(shipmentY));
			Assert("Two shipments should be equals", shipmentY.EqualsDateObject(shipmentX));
		}
	}
}
