using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Event = Enterprise.DataTransfer.Xml.XsdVersion1.Event;
using Events = Enterprise.ZArchitecture.Business.Events;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestTimeZone]
	public class ValueObjectDataAdapterBaseOnlyTest : TestCaseWithFactory
	{
		public void TestEDIInterchangeAndEDIMessageShouldNotBeCreated()
		{
			var adapter = new TestValueObjectDataAdapter();
			var xmlInterchange = XmlInterchange.NewPopulatedInterchange(Factory);
			PopulateXmlInterchange(xmlInterchange);
			var context = new ValueObjectImportContext(Factory, xmlInterchange, new NotificationBuffer());
			AssertNull(context.EDIInterchange);

			adapter.EnableEDIInterchange = false;
			var valueObject1 = new TestValueObject();
			var valueObject2 = new TestValueObject();
			adapter.CreateOrUpdateFromValueObject(valueObject1, context);
			AssertNull(context.EDIInterchange);

			adapter.CreateOrUpdateFromValueObject(valueObject2, context);
			AssertNull(context.EDIInterchange);
		}

		public void TestEDIInterchangeAndEDIMessage()
		{
			var adapter = new TestValueObjectDataAdapter();
			var xmlInterchange = XmlInterchange.NewPopulatedInterchange(Factory);
			PopulateXmlInterchange(xmlInterchange);
			var context = new ValueObjectImportContext(Factory, xmlInterchange, new NotificationBuffer());
			context.CurrentObjectXMLUTF8 = new MemoryStream(Encoding.UTF8.GetBytes("<TEST>123</TEST>"));
			AssertNull(context.EDIInterchange);

			adapter.EnableEDIInterchange = true;
			var valueObject1 = new TestValueObject();
			var valueObject2 = new TestValueObject();
			adapter.CreateOrUpdateFromValueObject(valueObject1, context);
			AssertNotNull(context.EDIInterchange);
			AssertEquals(1, context.EDIInterchange.ContainedMessages.Count);

			adapter.CreateOrUpdateFromValueObject(valueObject2, context);
			AssertNotNull(context.EDIInterchange);
			AssertEquals(2, context.EDIInterchange.ContainedMessages.Count);
			AssertEDIInterchangeData(context.EDIInterchange);
			AssertEDIMessageData(context.EDIInterchange.ContainedMessages[0]);
		}

		public void TestCreateOrUpdateFromValueObject()
		{
			IValueObjectDataAdapter adapter = new OrganisationValueObjectDataAdapter();
			var notify = new TestCreateOrUpdateFromValueObject_NotificationSubscriber();
			var context = new ValueObjectImportContext(Factory, notify);
			notify.QueryUserResponse = true;
			AssertNull("Returns null when value is null", adapter.CreateOrUpdateFromValueObject(null, context));

			var orgValue = new Organisation();
			orgValue.OrganisationDetails = new OrganisationDetail();
			orgValue.OrganisationDetails.Name = "Test Org";
			orgValue.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "Test Org";

			orgValue.OrganisationDetails.Location = new UNLOCO();
			orgValue.OrganisationDetails.Location.Value = "ZAPPQ";
			var updatedOrganisation = context.FindOrCreateTempOrganisation(orgValue, null, OrganisationTypes.None);
			AssertEquals("Should create a new organisation", "ZAPPQ", updatedOrganisation.OH_RL_NKClosestPort);

			orgValue.OrganisationDetails.Location.Value = "ZAAAA";

			notify.QueryUserResponse = false;
			var secondUpdatedOrganisation = (OrgHeader)adapter.CreateOrUpdateFromValueObject(orgValue, context);
			AssertEquals("Should update the same organisation", secondUpdatedOrganisation.PK, updatedOrganisation.PK);
			AssertEquals("Organisation should not be updated as the user chose no", "ZAPPQ", updatedOrganisation.OH_RL_NKClosestPort);

			notify.QueryUserResponse = true;
			secondUpdatedOrganisation = (OrgHeader)adapter.CreateOrUpdateFromValueObject(orgValue, context);
			AssertEquals("Should update the same organisation", secondUpdatedOrganisation.PK, updatedOrganisation.PK);
			AssertEquals("Organisation should be updated as the user chose yes", "ZAAAA", updatedOrganisation.OH_RL_NKClosestPort);
		}

		public void TestNewBusinessObject()
		{
			var value = new TestValueObject();
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var adapter = new TestValueObjectDataAdapter();
			AssertNotNull(((IValueObjectDataAdapter)adapter).NewBusinessObject(value, context));
			AssertEquals(typeof(TestImportingBizObj), ((IValueObjectDataAdapter)adapter).NewBusinessObject(value, context).GetType());
		}

		public void TestImportFromValueObject()
		{
			var adapter = new TestValueObjectDataAdapter();
			var bO = Factory.New<TestImportingBizObj>();
			var notify = new NotificationBuffer();

			bO.IsImportingData = false;
			var context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(bO, new TestValueObject(), context);
			AssertEquals("Should contain the 'business object created' notification", true, notify.ContainsNotificationType(NotificationSubscriberType.BusinessObjectCreatedOrUpdated));
		}

		public void TestImportFromValueObject_WithWrongFactory()
		{
			var adapter = new TestValueObjectDataAdapter();
			var wrongFactory = new BusinessObjectFactory();
			var bO = wrongFactory.New<TestImportingBizObj>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			adapter.ImportFromValueObject(bO, new TestValueObject(), context);
			AssertEquals("Should have a developer error indicating the wrong factory was used", true, ErrorReporter.LastMessageReported.Contains("same as the Factory in the Context"));
			ErrorReporter.Clear();
		}

		public void TestAddImportEvent()
		{
			var adapter = new TestValueObjectDataAdapter();
			var bizObj = Factory.New<TestImportingBizObj>();
			var dataImportEvents = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("No DIM event should be added.", 0, dataImportEvents.Length);

			adapter.AddImportEvent(bizObj);
			dataImportEvents = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);
		}

		public void TestAddImportEventWithReference()
		{
			var adapter = new TestValueObjectDataAdapter();
			var bizObj = Factory.New<TestImportingBizObj>();
			var dataImportEvents = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("No DIM event should be added.", 0, dataImportEvents.Length);

			ZString reference = "ABC";
			adapter.AddImportEvent(bizObj, reference);
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, reference);

			dataImportEvents = bizObj.GetLogs().Find(query);
			AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);
		}

		public void TestAddExportEvent()
		{
			AssertAddExportEvent_Purpose(null, "");
		}

		public void TestSetAdditionalRequirementsForDataExportEvent()
		{
			var adapter = new TestValueObjectDataAdapter();
			var bizObj = Factory.New<TestImportingBizObj>();
			var dataExportEvents = bizObj.GetLogs().MostRecentLogByEventTime(Events.DataExport);
			AssertNull("No DEX event should be added.", dataExportEvents);

			var notification = new NotificationBuffer();

			var context = new ValueObjectExportContext(notification);
			((IValueObjectDataAdapter)adapter).SetAdditionalRequirementsForDataExportEvent(null);
			adapter.AddExportEvent(new TestValueObject(), bizObj, context);

			dataExportEvents = bizObj.GetLogs().MostRecentLogByEventTime(Events.DataExport);
			AssertNotNull("DEX event should be added.", dataExportEvents);

			bizObj.Logs.RemoveAndDeleteAll();
			notification.Clear();
			((IValueObjectDataAdapter)adapter).SetAdditionalRequirementsForDataExportEvent(() => { return false; });
			adapter.AddExportEvent(new TestValueObject(), bizObj, context);
			dataExportEvents = bizObj.GetLogs().MostRecentLogByEventTime(Events.DataExport);
			AssertNull("No DEX event should be added.", dataExportEvents);

			bizObj.Logs.RemoveAndDeleteAll();
			notification.Clear();

			context.ExportPurpose = "APP";
			((IValueObjectDataAdapter)adapter).SetAdditionalRequirementsForDataExportEvent(() => { return true; });
			adapter.AddExportEvent(new TestValueObject(), bizObj, context);
			dataExportEvents = bizObj.GetLogs().MostRecentLogByEventTime(Events.DataExport);
			AssertNotNull("DEX event should be added.", dataExportEvents);
		}

		public void TestAddExportEvent_Purpose()
		{
			AssertAddExportEvent_Purpose("I was forced to implement this stupid task.", "Purpose: I was forced to implement this stupid task.");
			ZString longText = "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.";
			ZString expected = "Purpose: " + longText;
			AssertAddExportEvent_Purpose(longText, expected.Left(StmALogSchema.SL_Reference.MaxLength));
		}

		public void TestAddExportEventWithReference()
		{
			AssertAddExportEventWithReference_Purpose("");
		}

		public void TestAddExportEventWithReference_Purpose()
		{
			AssertAddExportEventWithReference_Purpose("This is designed to work not all the time. Clever, isn't it?");
		}

		public void TestIsImportingData_SetAndResetCorrectly()
		{
			IValueObjectDataAdapter adapter = new TestValueObjectDataAdapter();
			var bO = Factory.New<TestImportingBizObj>();

			bO.IsImportingData = false;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(bO, new TestValueObject(), context);
			AssertEquals("Should still not be importing data", false, bO.IsImportingData);

			bO.IsImportingData = true;
			adapter.ImportFromValueObject(bO, new TestValueObject(), context);
			AssertEquals("Should still be importing data", true, bO.IsImportingData);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFromXmlInterchange()
		{
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElementsWithinInterchange.xml", ""));
			var reader = new XmlTextReader(new StringReader(inputXmlContent));
			IValueObjectDataAdapter adapter = new TestValueObjectDataAdapter();

			XmlInterchange interchange;
			XmlInterchange.DeserializeInterchangeAndPayload(reader, new XmlValueObjectSerializer(adapter.ValueObjectType), out interchange);

			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, interchange, notify);
			var result = adapter.FromXmlInterchange(collection, context);
			AssertEquals("Should have read 2 collection elements", 2, collection.Count);
			AssertEquals("Should have read 2 collection elements", 2, result.Length);
			AssertEquals("Should be no errors importing the file", false, notify.HasErrors);
		}

		public void TestFromXmlInterchange_BusinessObjectCancellation()
		{
			var inputXmlContent = @"<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange xmlns='http://www.edi.com.au/EnterpriseService/'>
  <InterchangeInfo>
    <EDIOrganisation EDICode='ediorg' />
  </InterchangeInfo>
  <Payload>
    <TestElements>
      <TestElement>
        <Value>splaty</Value>
      </TestElement>
      <TestElement>
        <Value>splaty</Value>
      </TestElement>
    </TestElements>
  </Payload>
</XmlInterchange>".Replace("'", "\"");

			var reader = new XmlTextReader(new StringReader(inputXmlContent));
			XmlInterchange interchange;
			XmlInterchange.DeserializeInterchangeAndPayload(reader, new XmlValueObjectSerializer(typeof(TestValueObject)), out interchange);

			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();

			var mock = new Mock<TestValueObjectDataAdapter>();
			mock.CallBase = true;

			mock.Protected()
				.Setup<bool>("IsValueObjectCancellation", ItExpr.IsAny<TestValueObject>(), ItExpr.IsAny<ValueObjectImportContext>())
				.Returns(true);

			mock.Protected()
				.Setup<BusinessObject>("CancelBusinessObject", ItExpr.IsAny<TestValueObject>(), ItExpr.IsAny<ValueObjectImportContext>())
				.Returns((BusinessObject)null);

			var adapter = mock.Object as IValueObjectDataAdapter;
			var context = new ValueObjectImportContext(Factory, interchange, notify);
			adapter.FromXmlInterchange(collection, context);

			mock.Protected()
				.Verify("IsValueObjectCancellation", Times.Exactly(2), ItExpr.IsAny<TestValueObject>(), ItExpr.IsAny<ValueObjectImportContext>());

			mock.Protected()
				.Verify("CancelBusinessObject", Times.Exactly(2), ItExpr.IsAny<TestValueObject>(), ItExpr.IsAny<ValueObjectImportContext>());

			mock.Verify(m => m.CreateOrUpdateFromValueObject(It.IsAny<TestValueObject>(), It.IsAny<ValueObjectImportContext>()), Times.Never);

			AssertEquals("Should have read 0 collection elements", 0, collection.Count);
			AssertEquals("Should be no errors importing the file", false, notify.HasErrors);
			mock.VerifyAll();

			mock = new Mock<TestValueObjectDataAdapter>();
			mock.CallBase = true;
			mock.Protected()
				.Setup<bool>("IsValueObjectCancellation", ItExpr.IsAny<TestValueObject>(), ItExpr.IsAny<ValueObjectImportContext>())
				.Returns(false);

			adapter = mock.Object;
			adapter.FromXmlInterchange(collection, context);

			mock.Protected()
				.Verify("CancelBusinessObject", Times.Never(), ItExpr.IsAny<TestValueObject>(), ItExpr.IsAny<ValueObjectImportContext>());

			mock.Protected()
				.Verify("IsValueObjectCancellation", Times.Exactly(2), ItExpr.IsAny<IValueObject>(), ItExpr.IsAny<IValueObjectImportContext>());

			AssertEquals("Should have read 2 collection elements", 2, collection.Count);
			AssertEquals("Should be no errors importing the file", false, notify.HasErrors);
			mock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestFromXmlInterchange_NoSchemaExpectNoException()
		{
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElementsWithinInterchange.xml", ""));
			var reader = new XmlTextReader(new StringReader(inputXmlContent));
			var adapter = new TestValueObjectDataAdapter();

			XmlInterchange interchange;
			XmlInterchange.DeserializeInterchangeAndPayload(reader, new XmlValueObjectSerializer(adapter.ValueObjectType), out interchange);

			var collection = new DummyBusinessObjectCollection(Factory);
			var context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			((IValueObjectDataAdapter)adapter).FromXmlInterchange(collection, context);

			adapter.SetSchema(TestXmlSchemaDefinitions.Instance.SingleTestElementSchema);
			adapter.SetCollectionSchema(null);
			((IValueObjectDataAdapter)adapter).FromXmlInterchange(collection, context);
			adapter.SetSchema(null);
			adapter.SetCollectionSchema(TestXmlSchemaDefinitions.Instance.TestElementsSchema);
			((IValueObjectDataAdapter)adapter).FromXmlInterchange(collection, context);
			adapter.SetSchema(null);
			adapter.SetCollectionSchema(null);
			((IValueObjectDataAdapter)adapter).FromXmlInterchange(collection, context);
		}

		public void TestToXmlInterchange()
		{
			IValueObjectDataAdapter adapter = new TestValueObjectDataAdapter();
			TestImportingBizObj elementToImport = Factory.New<TestImportingBizObj>();
			var interchange = adapter.ToXmlInterchange(new[] { elementToImport }, new ValueObjectExportContext(new NotificationBuffer())) as XmlInterchange;

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var organisation = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			AssertEquals("Current company organisation should be put in EDIOrganisation", organisation.OH_Code, interchange.InterchangeInfo.EDIOrganisation.EDICode);
			AssertEquals("Should be version 1", "1", interchange.Version);
			AssertNotNull("Payload should contain data", interchange.Payload.Data);
			AssertEquals("Payload should contain collection element", elementToImport, ((IList)interchange.Payload.Data)[0]);
			AssertEquals("Source company should be current company", Env.CurrentCompany.Code, interchange.InterchangeInfo.Source.CompanyCode);
			AssertEquals("Source enterprise code should be current company", registrationKey.EnterpriseCode, interchange.InterchangeInfo.Source.EnterpriseCode);
			AssertEquals("Origin server should be current machine", registrationKey.ServerCode, interchange.InterchangeInfo.Source.OriginServer);
			AssertEquals("Login Name should be current user", GlbStaff.CurrentUser.GS_LoginName, interchange.InterchangeInfo.Source.LoginName);

			AssertEquals("Payload.DataAdapter should be initialized with actual adapter", adapter, interchange.Payload.DataAdapter);
			AssertEquals("Payload.BusinessObject should be initialized", 1, ((IList)interchange.Payload.Data).Count);
			AssertEquals("Payload.BusinessObject should be initialized", elementToImport, ((IList)interchange.Payload.Data)[0]);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestToXmlInterchange_MustPassMoreThan1BizO()
		{
			IValueObjectDataAdapter adapter = new TestValueObjectDataAdapter();
			var interchange = (XmlInterchange)adapter.ToXmlInterchange(Array.Empty<TestImportingBizObj>(), new ValueObjectExportContext(new NotificationBuffer()));
		}

		public void TestNotifyCreatedOrUpdated()
		{
			var adapter = new TestValueObjectDataAdapter();

			var innerNotify = new NotifyCreatedOrUpdatedTestHelper();
			var notify = new NotificationBuffer(innerNotify);
			adapter.NotifyCreatedOrUpdated = true;
			var bizObj = Factory.New<TestImportingBizObj>();
			var context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(bizObj, new TestValueObject(), context);
			AssertEquals("The user should be notified of a business object created/update", true, notify.ContainsNotificationType(NotificationSubscriberType.BusinessObjectCreatedOrUpdated));
			var expectedMessage = "TestImportingBizObj Import Name";
			AssertEquals("InnerNotify should contain message '" + expectedMessage + "': InnerNotify Contains:" + System.Environment.NewLine + innerNotify.ToString(), true, innerNotify.ToString().IndexOf(expectedMessage) >= 0);
			notify = new NotificationBuffer();
			adapter.NotifyCreatedOrUpdated = false;
			adapter.ImportFromValueObject(Factory.New<TestImportingBizObj>(), new TestValueObject(), context);
			AssertEquals("The user should NOT be notified of a business object created/update", false, notify.ContainsNotificationType(NotificationSubscriberType.BusinessObjectCreatedOrUpdated));
		}

		public void TestNotifyCreatedOrUpdatedWithError()
		{
			var adapter = new TestValueObjectDataAdapter();

			var innerNotify = new NotifyCreatedOrUpdatedTestHelper();
			var notify = new NotificationBuffer(innerNotify);
			adapter.NotifyCreatedOrUpdated = true;
			var bizObj = Factory.New<TestImportingBizObj>();
			var context = new ValueObjectImportContext(Factory, notify);

			notify.Notify(new ErrorNotification(ErrorType.Error));
			adapter.ImportFromValueObject(bizObj, new TestValueObject(), context);
			Assert("Standard error should not prevent notification of a business object created/update",
				notify.ContainsNotificationType(NotificationSubscriberType.BusinessObjectCreatedOrUpdated));

			notify.Clear();
			notify.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave));
			adapter.ImportFromValueObject(bizObj, new TestValueObject(), context);
			Assert("The user shouldn't be notified of a business object created/update because it has data errors preventing save",
				!notify.ContainsNotificationType(NotificationSubscriberType.BusinessObjectCreatedOrUpdated));
		}

		public void TestExportAndImportCustomValues()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.SetUserDefinedValue("Prop1", ZDateTime.BrettsBirthday);
			product.SetUserDefinedValue("Prop2", new ZDecimal(1.23m));
			product.SetUserDefinedValue("Prop3", new ZString("Hello"));
			product.SetUserDefinedValue("Prop4", new ZInt(69));
			product.SetUserDefinedValue("Prop5", ZBool.True);
			product.SetUserDefinedValue("Prop6", new ZShort(49));
			product.SetUserDefinedValue("Prop7", new ZByte(89));

			var adapter = ProductValueObjectDataAdapter.New();
			var notify = new NotificationBuffer();
			var context = new ValueObjectExportContext(notify);

			var productXsd = adapter.ExportToValueObject(product, context);

			ZString expectedCustomValue = (ZString)"Hello".PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength, '1');
			ZString inputedCustomValue = (ZString)"Hello".PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, '1');

			var customValueTooLong = new CustomValue();
			customValueTooLong.Name = "TooLong";
			customValueTooLong.Type = "String";
			customValueTooLong.Value = inputedCustomValue;
			productXsd.CustomValues.Add(customValueTooLong);

			var product2 = Factory.New<OrgSupplierPart>();
			var importContext = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(product2, productXsd, importContext);

			AssertEquals(ZDateTime.BrettsBirthday, product2.GetUserDefinedValue<ZDateTime>("Prop1"));
			AssertEquals(new ZDecimal(1.23m), product2.GetUserDefinedValue<ZDecimal>("Prop2"));
			AssertEquals(new ZString("Hello"), product2.GetUserDefinedValue<ZString>("Prop3"));
			AssertEquals(new ZInt(69), product2.GetUserDefinedValue<ZInt>("Prop4"));
			AssertEquals(ZBool.True, product2.GetUserDefinedValue<ZBool>("Prop5"));
			AssertEquals(new ZShort(49), product2.GetUserDefinedValue<ZShort>("Prop6"));
			AssertEquals(new ZByte(89), product2.GetUserDefinedValue<ZByte>("Prop7"));
			AssertEquals(expectedCustomValue, product2.GetUserDefinedValue<ZString>("TooLong"));
			AssertEquals(ZString.Format("Attempted to insert {0} characters into Field [{1}] which has a maximum length of {2} characters. Field was truncated.", inputedCustomValue.Length, customValueTooLong.Name, GenCustomAddOnValueSchema.XV_Data.MaxLength), notify.Events[0].Message);
		}

		public void TestExportToValueObjectDoesNotRegardEmptyValueAsSpecified()
		{
			var bizObj = Factory.New<StmALog>();
			using (bizObj.LockForUpdatingKeyFieldsForTesting())
			{
				var adapter = StmALogValueObjectDataAdapter.New(bizObj, "");
				var notify = new NotificationBuffer();

				var valueObject = (Event)Activator.CreateInstance(adapter.ValueObjectType);
				AssertEquals("ShouldCreateElementForEmptyValue - Initial value", true, valueObject.ShouldCreateElementForEmptyValue);

				// Set ShouldCreateElementForEmptyValue to TRUE and run export
				bizObj.SL_GS_NKUser = "";
				bizObj.SL_Table = "DummyBizo";
				adapter.ExportToValueObject(bizObj, valueObject, new ValueObjectExportContext(notify));

				AssertEquals("ShouldCreateElementForEmptyValue was TRUE before export, and should still be after it.", true, valueObject.ShouldCreateElementForEmptyValue);
				AssertEquals("User value", "", valueObject.User);
				AssertEquals("User specified?", false, valueObject.UserSpecified);
				AssertEquals("Source value", "DummyBizo", valueObject.Source);
				AssertEquals("Source specified?", true, valueObject.SourceSpecified);

				// Set ShouldCreateElementForEmptyValue to FALSE and run export
				bizObj.SL_GS_NKUser = "X";
				bizObj.SL_Table = "";
				valueObject.ShouldCreateElementForEmptyValue = false;
				adapter.ExportToValueObject(bizObj, valueObject, new ValueObjectExportContext(notify));

				AssertEquals("ShouldCreateElementForEmptyValue was FALSE before export, and should still be after it.", false, valueObject.ShouldCreateElementForEmptyValue);
				AssertEquals("User value", "X", valueObject.User);
				AssertEquals("User specified?", true, valueObject.UserSpecified);
				AssertEquals("Source value", "", valueObject.Source);
				AssertEquals("Source specified?", false, valueObject.SourceSpecified);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory.AllowMultipleBusinessObjectsAroundOneRow = false;
		}

		void PopulateXmlInterchange(XmlInterchange xmlInterchange)
		{
			xmlInterchange.Payload.Data = new[] { Factory.New<TestImportingBizObj>() };
			xmlInterchange.InterchangeInfo.Source.OriginServer = "Test Origin";
			var refKey = xmlInterchange.InterchangeInfo.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = ReferenceType.OwnerReference;
			refKey.Value = "Test Owner Ref";
			refKey = xmlInterchange.InterchangeInfo.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = ReferenceType.UniqueIdentifier;
			refKey.Value = "Test Num";
			refKey = xmlInterchange.InterchangeInfo.ReferenceKeys.AddNew();
			refKey.ReferenceKeyName = ReferenceType.BatchNumber;
			refKey.Value = "Test Batch";
			AssertEquals(3, xmlInterchange.InterchangeInfo.ReferenceKeys.Count);
		}

		void AssertEDIInterchangeData(EDIInterchange interchange)
		{
			AssertEquals("EI_BodyText incorrect", "<TestElements><TestElement><Value>splaty</Value></TestElement></TestElements>", interchange.EI_BodyText);
			AssertEquals("EI_ReceiveTransmit incorrect", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_Status incorrect", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("EI_RetryCount incorrect", 0, interchange.EI_RetryCount);
			AssertEquals("EI_From incorrect", "Test Origin", interchange.EI_From);
			AssertEquals("EI_InterchangeNum incorrect", "Test Num", interchange.EI_InterchangeNum);
		}

		void AssertEDIMessageData(EDIMessage message)
		{
			AssertEquals("EM_ReceiveTransmit incorrect", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EI_Status incorrect", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageText incorrect", "<TEST>123</TEST>", message.EM_MessageText);
		}

		void AssertAddExportEvent_Purpose(string purpose, string expected)
		{
			var adapter = new TestValueObjectDataAdapter();
			var bizObj = Factory.New<TestImportingBizObj>();
			var dataExportEvents = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("No DEX event should be added.", 0, dataExportEvents.Length);

			var context = new ValueObjectExportContext(new NotificationBuffer());
			context.ExportPurpose = purpose;
			adapter.AddExportEvent(new TestValueObject(), bizObj, context);
			dataExportEvents = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added.", 1, dataExportEvents.Length);

			AssertEquals(expected, dataExportEvents[0].SL_Reference);
		}

		void AssertAddExportEventWithReference_Purpose(string purpose)
		{
			var context = new ValueObjectExportContext(new NotificationBuffer());
			var adapter = new TestValueObjectDataAdapter();
			var bizObj = Factory.New<TestImportingBizObj>();

			context.ExportPurpose = purpose;
			adapter.AddExportEvent(new TestValueObject(), bizObj, context, "Data Export Reference");
			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			filter.AddToFilter(StmALogSchema.SL_Reference, "Data Export Reference");

			var dataExportEvents = bizObj.GetLogs().Find(filter);
			AssertEquals("DEX event with Reference should be added.", 1, dataExportEvents.Length);
			var log = dataExportEvents[0];
			AssertEquals("Added DEX's Reference", "Data Export Reference", log.SL_Reference);
		}

		sealed class TestCreateOrUpdateFromValueObject_NotificationSubscriber : NotificationBuffer
		{
			public bool QueryUserResponse;

			protected override void QueryUser(IQueryUserEventArgs e)
			{
				if (e is QueryUserYesNoYesAllNoAllEventArgs)
				{
					((QueryUserYesNoYesAllNoAllEventArgs)e).Response = QueryUserResponse;
				}
			}
		}

		sealed class NotifyCreatedOrUpdatedTestHelper : INotifications
		{
			public void Add(INotificationSubscriberNotification @event)
			{
				Messages.Append(@event.Message.Replace("\r\n", "\n").Replace("\n", "\r\n") + "\r\n");
			}

			public override string ToString() => Messages.ToString();

			void INotifications.Add(INotification @event)
			{
				Add((INotificationSubscriberNotification)@event);
			}

			readonly StringBuilder Messages = new StringBuilder();
		}
	}
}
