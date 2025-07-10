using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	[TestedType(typeof(UMIServiceTask))]
	public class UMIServiceTaskTest : ServiceTaskTestCase<UMIServiceTask>
	{
		public void TestConcurrencyErrorIsHandled()
		{
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));

			var messages = Enumerable.Range(0, 1).Select(i => GetMessageRowWithRandomMessageContent(Factory, i)).ToArray();
			Factory.Save();

			var serviceTask1 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			var firstSave = true;
			using (var manager1 = new ProcessingManager_ForConcurrencyTest(Factory, serviceTask1.SupportedMessageSubtypes, GrEngineServiceSetting.Disabled,
					   shouldConflict: thefactory =>
					   {
						   if (thefactory.NameForDebugging == "Universal Message Processing" || thefactory.NameForDebugging == "Universal Message Logging")
						   {
							   firstSave = !firstSave;
							   return firstSave;
						   }
						   else
						   {
							   return false;
						   }
					   }))
			{
				manager1.ExecuteBatch(CancellationToken.None);
				manager1.ExecuteBatch(CancellationToken.None);
				manager1.ExecuteBatch(CancellationToken.None);
				manager1.ExecuteBatch(CancellationToken.None);
				manager1.ExecuteBatch(CancellationToken.None);
				manager1.ExecuteBatch(CancellationToken.None);
			}

			messages.ForEach(m => m.Reload());
			AssertEquals("Everything should be discarded.", "REJ", messages[0].EM_Status);
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Don't use ErrorReporter here as it tries to handled some exceptions, in this case UMI has already retried", 0, ErrorReporter.TotalErrorCount);
		}

		public class ProcessingManager_ForConcurrencyTest : IUniversalProcessingManager
		{
			readonly IUniversalProcessingManager universalProcessingManager;

			public ProcessingManager_ForConcurrencyTest(BusinessObjectFactory factory, IEnumerable<string> messageSubTypes, GrEngineServiceSetting settings, Func<BusinessObjectFactory, bool> shouldConflict)
			{
				universalProcessingManager = new UniversalProcessingManager(messageSubTypes, Enumerable.Empty<string>(), settings);

				BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) =>
				{
					if (shouldConflict(thefactory))
					{
						var dummy = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
						var row = ((INeedRow)dummy).Row;
						dummy.Z0_Code = "ABC";
						dummy.Z0_Number = 123;
						((ILightValidationInternals)dummy).IsValid = true;
						dummy.Factory.Save();

						var otherDummy = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy.PK);
						otherDummy.Z0_Number = 777;
						otherDummy.Factory.Save();

						((ILightValidationInternals)dummy).IsValid = false;
						dummy.Z0_Code = "XYZ";

						ConcurrencyInfo.SetConcurrencyPolicy(row, "Z0_Number", ConcurrencyPolicy.Strict);
						dummy.Factory.Save();
					}
				});
			}

			public XmlEDIGrEngine GrEngine => universalProcessingManager.GrEngine;

			public LoggingInformation Logger { get => universalProcessingManager.Logger; }

			public void ProcessBatch(BaseMessageProcessor<XmlEDIMessage>.DisposableBatch messages, CancellationToken token, FailedMessagesManager failedMessagesManager)
			{
				universalProcessingManager.ProcessBatch(messages, token, failedMessagesManager);
			}

			public void ExecuteBatch(CancellationToken token)
			{
				universalProcessingManager.ExecuteBatch(token);
			}

			public void FlipGrEngine()
			{
				universalProcessingManager.FlipGrEngine();
			}

			public void Dispose()
			{
				universalProcessingManager.Dispose();
			}
		}

		public void TestUniversalShipmentForForwardingFailsGracefullyAndMarksMessageAsFailed()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "In The Moment";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_Code = "INTHEMSYD";

			var add = org.MainAddress;
			add.OA_Address1 = "Unit 12, Level 3";
			add.OA_Address2 = "233 Here St";
			add.OA_City = "ThereVille";
			add.OA_State = "OfBliss";
			add.OA_PostCode = "1233";
			add.OA_Email = "s.m@moment.com.au";
			add.OA_Fax = "234098234";
			add.OA_Mobile = "234098293";
			add.OA_Phone = "1239813209";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Starshine Moonbeam";
			contact.OC_Email = "s.m@moment.com.au";
			contact.OC_Fax = "234098234";
			contact.OC_Mobile = "234098293";
			contact.OC_Phone = "1239813209";

			org.CustomsCodes.AddNew("GST", "55555");
			org.CustomsCodes.AddNew("ATF", "1234F", Factory.Load<RefCountry>(Constants.CountryGuids.NewZealand));

			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			incomingMessage.EM_MessageNum = "12345678";
			incomingMessage.EM_MessageText =
			#region Message Test
 @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>

      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <WayBillNumber>HOUSENOTSAVED</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House WayBill</Description>
    </WayBillType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>LocalClient</AddressType>
        <OrganizationCode>INTHEMSYD</OrganizationCode>
        <Address1>Unit 12, Level 3</Address1>
        <Address2>233 Here St</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ThereVille</City>
        <CompanyName>In The Moment</CompanyName>
        <Contact>Starshine Moonbeam</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>s.m@moment.com.au</Email>
        <Fax>234098234</Fax>
        <GovRegNum>55555</GovRegNum>
        <GovRegNumType>
          <Code>GST</Code>
          <Description>GST Code</Description>
        </GovRegNumType>
        <Mobile>234098293</Mobile>
        <Phone>1239813209</Phone>
        <Postcode>1233</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>OfBliss</State>
        <UniversalNettingCode>545</UniversalNettingCode>
        <UniversalOfficeCode>454</UniversalOfficeCode>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>NZ</Code>
              <Name>New Zealand</Name>
            </CountryOfIssue>
            <Type>
              <Code>ATF</Code>
              <Description>Approved Transitional Facility</Description>
            </Type>
            <Value>1234F</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
			#endregion // Message Text
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);
			var noteText = ((StmNoteCollection)reloadedMessage.Notes.GetAllNotes())[0].ST_NoteDataAsText;

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			CombineAssertions(delegate
			{
				AssertContains("Information|Starting processing Message #12345678", logger.ToString());
				AssertContains("Error|Could not save Local Client Organization - Must have an Origin, Destination and Transport Mode to be able to calculate a Department for a Job Costing record, and the Local Client is saved on the Job Costing record.", logger.ToString());
				AssertContains("Information|No changes were made due to the above errors. Please fix the errors and try again.", logger.ToString());
				AssertContains("Information|No Module used this Universal Shipment data.", logger.ToString());
				AssertContains("Information|Finished processing Message #12345678", logger.ToString());

				AssertMultilineASCIIEquals("noteAsText", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'LocalClient':- Matched to 'INTHEMSYD' by code, address 'Unit 12, Level 3' (only address).
Error - Could not save Local Client Organization - Must have an Origin, Destination and Transport Mode to be able to calculate a Department for a Job Costing record, and the Local Client is saved on the Job Costing record.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), noteText);

				AssertEquals("reloadedMessage.EM_Status", EDIMessage.Status.Discarded, reloadedMessage.EM_Status);
			});
		}

		public void TestMessagesWithUnhandledExceptionsDontProcessInfinitely()
		{
			var inboundXml =
		$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
  </Shipment>
</UniversalShipment>";

			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			incomingMessage.EM_MessageNum = "12345678";
			incomingMessage.EM_MessageText = inboundXml;

			Factory.Save();

			var initialSave = true;

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				if (!initialSave)
				{
					throw new InvalidOperationException();
				}
				initialSave = false;
			});

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
			for (var i = 0; i < retryAttempts + 2; i++)
			{
				serviceTask.RunTask();
				initialSave = true;
			}

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);
			var noteCount = ((StmNoteCollection)reloadedMessage.Notes.GetAllNotes()).Count;

			AssertEquals(XmlEDIMessage.Status.Rejected, reloadedMessage.EM_Status);
			AssertEquals("EM_RetryCount", (byte)(retryAttempts + 1), reloadedMessage.EM_RetryCount);
			ErrorReporter.Clear();
		}

		public void TestImportShipmentWithFailedSaveLogsFailure()
		{
			var inboundXml =
		$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
    <Shipment>
    <DataContext>
        <DataTargetCollection>
        <DataTarget>
            <Type>ForwardingShipment</Type>
        </DataTarget>
        <DataTarget>
            <Type>CustomsDeclaration</Type>
        </DataTarget>
        </DataTargetCollection>
        <Company>
        <Code>GU2</Code>
        </Company>
        <DataProvider>CHIKSUAGATT</DataProvider>
        <EnterpriseID>GEO</EnterpriseID>
        <ServerID>PRO</ServerID>
    </DataContext>
    <Branch>
        <Code>PH2</Code>
    </Branch>
    <ContainerMode>
        <Code>FCL</Code>
    </ContainerMode>
       
    <CustomsContainerMode>
        <Code>CNT</Code>
    </CustomsContainerMode>
    <PortOfDestination>
        <Code>DOCAU</Code>
    </PortOfDestination>
    <OwnerRef>015TSOS10000282973xx</OwnerRef>
    <MessageType>
        <Code>EXP</Code>
    </MessageType>
    <BookingConfirmationReference>015TSOS10000282973xx</BookingConfirmationReference>
    <DocumentedWeight>15932.89</DocumentedWeight>
    <ManifestedWeight>15932.89</ManifestedWeight>
    <PortOfOrigin>
        <Code>USPHL</Code>
    </PortOfOrigin>
    <ServiceLevel>
        <Code>SU9</Code>
    </ServiceLevel>
    <ShipmentIncoTerm>
        <Code>CFR</Code>
        <Description/>
    </ShipmentIncoTerm>
    <GoodsValue/>
    <OuterPacks>30527</OuterPacks>
    <OuterPacksPackageType>
        <Code>PKG</Code>
    </OuterPacksPackageType>
    <TotalNoOfPacks>30527</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
        <Code>PKG</Code>
    </TotalNoOfPacksPackageType>
    <TotalWeight>15932.89</TotalWeight>
    <TotalWeightUnit>
        <Code>KG</Code>
    </TotalWeightUnit>
    <TransportMode>
        <Code>SEA</Code>
    </TransportMode>
    <LocalProcessing>
        <OrderNumberCollection>
        <OrderNumber>015TSOS10000282973_xx</OrderNumber>
        </OrderNumberCollection>
    </LocalProcessing>
    <AddInfoCollection>
        <AddInfo>
        <Key>SchDLoading</Key>
        <Value>1101</Value>
        </AddInfo>
        <AddInfo>
        <Key>SchDExport</Key>
        <Value>1101</Value>
        </AddInfo>
    </AddInfoCollection>
    <AdditionalBillCollection>
        <AdditionalBill>
        <BillNumber/>
        </AdditionalBill>
    </AdditionalBillCollection>
    <DateCollection>
        <Date>
        <Type>LoadingDate</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2025-01-22</Value>
        </Date>
    </DateCollection>
    <OrganizationAddressCollection>
        <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <OrganizationCode>DOSASACAU</OrganizationCode>
        </OrganizationAddress>
        <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <OrganizationCode>CHIKSUAGATT</OrganizationCode>
        </OrganizationAddress>
        <OrganizationAddress>
        <AddressType>Importer</AddressType>
        <OrganizationCode>DOSASACAU</OrganizationCode>
        </OrganizationAddress>
        <OrganizationAddress>
        <AddressType>LocalClient</AddressType>
        <OrganizationCode>USIKPUBSWEP</OrganizationCode>
        </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
        <PackingLine>
        <ContainerNumber>JBHU274791</ContainerNumber>
        <Weight>15932.89</Weight>
        <PackQty>30527</PackQty>
        <PackType>
            <Code>PKG</Code>
        </PackType>
        </PackingLine>
    </PackingLineCollection>
    </Shipment>
</UniversalShipment>";

			var uniFactory = new UniversalObjectFactory();

			var message = TestCaseWithFactoryAndMessagingHelpers.GetQueuedUniversalShipmentMessage(uniFactory, inboundXml, true);

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			serviceTask.RunTask();

			((BusinessObject)message).Reload();
			AssertEquals("Shipment import failed", EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertNotContains("Should not log that import succeeded", "Successfully saved Shipment", logger.ToString());
			AssertContains("Should log that import failed", "Error saving Shipment", logger.ToString());
		}

		[TestDate(2022, 1, 1)]
		public void TestConcurrencyExceptionWhileProcessing()
		{
			var inboundXml =
		$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";
			var uniFactory = new UniversalObjectFactory();

			var message = TestCaseWithFactoryAndMessagingHelpers.GetQueuedUniversalShipmentMessage(uniFactory, inboundXml, true);

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				if (factory.NameForDebugging == "Universal Message Processing")
				{
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("BLAH"), null, null), factory);
				}
			});

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			serviceTask.RunTask();

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("Message EM_Status", EDIMessageStatusList.Codes.Rejected, reloadedMessage.EM_Status);
			AssertEquals("Log Note Count", 10, reloadedMessage.DataImportLogNoteCount);
			AssertEquals("RetryCount", (byte)10, reloadedMessage.EM_RetryCount);
			var expectedLog = "Exception occurred 10 times whilst processing a message individually. The message's status has been set to 'Rejected'";
			var logNote = ((EDIMessage)message).Notes.GetAllNotes().First(bizo => bizo is StmNote note && note.ST_NoteDataAsText.Contains(expectedLog));
			AssertNotNull("Log Note", logNote);

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Don't use ErrorReporter here as it tries to handled some exceptions, in this case UMI has already retried", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestShouldHandleDefaultBranchBeingInactiveWithUniversalMessageByRejectingMessage()
		{
			eAdaptorRegistry.Instance.UniversalXMLInactiveDepartmentFailsMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.Equal, "BRN"));
			department.GE_IsActive = false;
			var shipment = (IForwardingShipment)Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "S00001000";

			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			incomingMessage.EM_MessageNum = "12345678";
			incomingMessage.EM_MessageText =
			@"
			<UniversalEvent xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
			<Event>
					<DataContext>
						<CodesMappedToTarget>true</CodesMappedToTarget>
						<DataTargetCollection>
							<DataTarget>
								<Type>ForwardingShipment</Type>
							<Key>S00001000</Key>
							</DataTarget>
						</DataTargetCollection>
						<Company>
						<Code>EDI</Code>
						</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<ServerID>DAT</ServerID>
					</DataContext>
				<EventType>Z11</EventType>
				<EventReference>To: 15-Feb-21</EventReference>
				<EventTime>2022-07-11T10:10:00</EventTime>
					<EventParameters>
						<Facility>CTO</Facility>
					<Location>SGSIN</Location>
					</EventParameters>
				<IsEstimate>false</IsEstimate>
			</Event>
			</UniversalEvent>";
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();
			var expectedResultStatus = EDIMessage.Status.Rejected;
			var processedMessage = new BusinessObjectFactory().Load<IEDIMessage>(incomingMessage.PK);
			var messageDepartment = new BusinessObjectFactory().Load<IGlbDepartment>(processedMessage.EM_GE);
			AssertEquals(expectedResultStatus, processedMessage.EM_Status);
			AssertEquals("BRN", messageDepartment.GE_Code);
		}

		public void TestShouldHandleGivenDepartmentBeingInactiveWithUniversalMessageByRejectingMessage()
		{
			eAdaptorRegistry.Instance.UniversalXMLInactiveDepartmentFailsMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var inactiveDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			inactiveDepartment.GE_Code = "XYZ";
			inactiveDepartment.GE_IsActive = false;
			var shipment = (IForwardingShipment)Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "S00001000";

			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			incomingMessage.EM_MessageNum = "12345678";
			incomingMessage.EM_MessageText =
			@"
			<UniversalEvent xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
			<Event>
					<DataContext>
						<CodesMappedToTarget>true</CodesMappedToTarget>
						<DataTargetCollection>
							<DataTarget>
								<Type>ForwardingShipment</Type>
							<Key>S00001000</Key>
							</DataTarget>
						</DataTargetCollection>
						<Company>
						<Code>EDI</Code>
						</Company>
						<EventDepartment>
							<Code>XYZ</Code>
						</EventDepartment>
					<EnterpriseID>EDI</EnterpriseID>
					<ServerID>DAT</ServerID>
					</DataContext>
				<EventType>Z11</EventType>
				<EventReference>To: 15-Feb-21</EventReference>
				<EventTime>2022-07-11T10:10:00</EventTime>
					<EventParameters>
						<Facility>CTO</Facility>
					<Location>SGSIN</Location>
					</EventParameters>
				<IsEstimate>false</IsEstimate>
			</Event>
			</UniversalEvent>";
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();
			var expectedResultStatus = EDIMessage.Status.Rejected;
			var processedMessage = new BusinessObjectFactory().Load<IEDIMessage>(incomingMessage.PK);
			AssertEquals(expectedResultStatus, processedMessage.EM_Status);
		}

		public void TestShouldFallbacktoInactiveDefaultDepartmentWhenRegistryOff()
		{
			TestUseDefaultDepartmentWhenRegistryIsOff(false);
		}

		public void TestShouldFallbacktoActiveDefaultDepartmentWhenRegistryOff()
		{
			TestUseDefaultDepartmentWhenRegistryIsOff(true);
		}

		public void TestUseDefaultDepartmentWhenRegistryIsOff(bool isActive)
		{
			eAdaptorRegistry.Instance.UniversalXMLInactiveDepartmentFailsMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.Equal, "BRN"));
			department.GE_IsActive = isActive;
			var inactiveDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			inactiveDepartment.GE_Code = "XYZ";
			inactiveDepartment.GE_IsActive = false;
			var shipment = (IForwardingShipment)Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "S00001000";

			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			incomingMessage.EM_MessageNum = "12345678";
			incomingMessage.EM_MessageText =
			@"
			<UniversalEvent xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
			<Event>
					<DataContext>
						<CodesMappedToTarget>true</CodesMappedToTarget>
						<DataTargetCollection>
							<DataTarget>
								<Type>ForwardingShipment</Type>
							<Key>S00001000</Key>
							</DataTarget>
						</DataTargetCollection>
						<Company>
						<Code>EDI</Code>
						</Company>
						<EventDepartment>
							<Code>XYZ</Code>
						</EventDepartment>
					<EnterpriseID>EDI</EnterpriseID>
					<ServerID>DAT</ServerID>
					</DataContext>
				<EventType>Z11</EventType>
				<EventReference>To: 15-Feb-21</EventReference>
				<EventTime>2022-07-11T10:10:00</EventTime>
					<EventParameters>
						<Facility>CTO</Facility>
					<Location>SGSIN</Location>
					</EventParameters>
				<IsEstimate>false</IsEstimate>
			</Event>
			</UniversalEvent>";
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();
			var expectedResultStatus = EDIMessage.Status.Warning;
			var processedMessage = new BusinessObjectFactory().Load<IEDIMessage>(incomingMessage.PK);
			var messageDepartment = new BusinessObjectFactory().Load<IGlbDepartment>(processedMessage.EM_GE);
			var updatedShipmentWorkflow = (IWorkflowProvider)new BusinessObjectFactory().Load(ObjectFactory.GetType<IForwardingShipment>(), shipment.PK);
			AssertEquals(expectedResultStatus, processedMessage.EM_Status);
			AssertEquals("BRN", updatedShipmentWorkflow.Logs.GetAllLogs().Where(log => log.Event.SE_Code == "Z11").First().SL_GE_NKDepartment);
		}

		public void TestShouldUseGivenDepartmentWithUniversalMessageWhenCheckingIfActive()
		{
			var defaultDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.Equal, "BRN"));
			defaultDepartment.GE_IsActive = false;
			var specifiedDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			specifiedDepartment.GE_Code = "AXZ";
			var shipment = (IForwardingShipment)Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "S00001000";

			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			incomingMessage.EM_MessageNum = "12345678";
			incomingMessage.EM_MessageText =
			@"
			<UniversalEvent xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
			<Event>
					<DataContext>
						<CodesMappedToTarget>true</CodesMappedToTarget>
						<DataTargetCollection>
							<DataTarget>
								<Type>ForwardingShipment</Type>
							<Key>S00001000</Key>
							</DataTarget>
						</DataTargetCollection>
						<Company>
						<Code>EDI</Code>
						</Company>
						<EventDepartment>
							<Code>AXZ</Code>
						</EventDepartment>
					<EnterpriseID>EDI</EnterpriseID>
					<ServerID>DAT</ServerID>
					</DataContext>
				<EventType>Z11</EventType>
				<EventReference>To: 15-Feb-21</EventReference>
				<EventTime>2022-07-11T10:10:00</EventTime>
					<EventParameters>
						<Facility>CTO</Facility>
					<Location>SGSIN</Location>
					</EventParameters>
				<IsEstimate>false</IsEstimate>
			</Event>
			</UniversalEvent>";
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();
			var expectedResultStatus = EDIMessage.Status.ProcessedOK;
			var processedMessage = new BusinessObjectFactory().Load<IEDIMessage>(incomingMessage.PK);
			var messageDepartment = new BusinessObjectFactory().Load<IGlbDepartment>(processedMessage.EM_GE);
			var updatedShipmentWorkflow = (IWorkflowProvider)new BusinessObjectFactory().Load(ObjectFactory.GetType<IForwardingShipment>(), shipment.PK);
			AssertEquals(expectedResultStatus, processedMessage.EM_Status);
			AssertEquals("AXZ", updatedShipmentWorkflow.Logs.GetAllLogs().Where(log => log.Event.SE_Code == "Z11").First().SL_GE_NKDepartment);
		}

		public void TestShouldUseDefaultDepartmentWithUniversalMessageWhenSpecifiedOneIsInactive()
		{
			var defaultDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.Equal, "BRN"));
			defaultDepartment.GE_IsActive = true;
			var specifiedDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			specifiedDepartment.GE_Code = "AXZ";
			specifiedDepartment.GE_IsActive = false;
			var shipment = (IForwardingShipment)Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "S00001000";

			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			incomingMessage.EM_MessageNum = "12345678";
			incomingMessage.EM_MessageText =
			@"
			<UniversalEvent xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
			<Event>
					<DataContext>
						<CodesMappedToTarget>true</CodesMappedToTarget>
						<DataTargetCollection>
							<DataTarget>
								<Type>ForwardingShipment</Type>
							<Key>S00001000</Key>
							</DataTarget>
						</DataTargetCollection>
						<Company>
						<Code>EDI</Code>
						</Company>
						<EventDepartment>
							<Code>AXZ</Code>
						</EventDepartment>
					<EnterpriseID>EDI</EnterpriseID>
					<ServerID>DAT</ServerID>
					</DataContext>
				<EventType>Z11</EventType>
				<EventReference>To: 15-Feb-21</EventReference>
				<EventTime>2022-07-11T10:10:00</EventTime>
					<EventParameters>
						<Facility>CTO</Facility>
					<Location>SGSIN</Location>
					</EventParameters>
				<IsEstimate>false</IsEstimate>
			</Event>
			</UniversalEvent>";
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();
			var expectedResultStatus = EDIMessage.Status.Warning;
			var processedMessage = new BusinessObjectFactory().Load<IEDIMessage>(incomingMessage.PK);
			var messageDepartment = new BusinessObjectFactory().Load<IGlbDepartment>(processedMessage.EM_GE);
			var updatedShipmentWorkflow = (IWorkflowProvider)new BusinessObjectFactory().Load(ObjectFactory.GetType<IForwardingShipment>(), shipment.PK);
			AssertEquals(expectedResultStatus, processedMessage.EM_Status);
			AssertEquals("BRN", updatedShipmentWorkflow.Logs.GetAllLogs().Where(log => log.Event.SE_Code == "Z11").First().SL_GE_NKDepartment);
		}

		EDIMessage CreateSimpleIncomingMessageForTest()
		{
			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			incomingMessage.EM_MessageNum = "12345678";
			incomingMessage.EM_MessageText = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key/>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>ULVTSTWTF</DataProvider>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			Factory.Save();
			return incomingMessage;
		}

		public void TestLogRecordWhenCriticalExceptionIsRaised()
		{
			CreateSimpleIncomingMessageForTest();

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				if (factory.NameForDebugging == "Universal Message Processing")
				{
					throw new Exception("Disaster", new OutOfMemoryException()); // Producing a critical exception in exception hierarchy
				}
			});

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			AssertExceptionThrown<Exception>(serviceTask.RunTask);

			AssertLessThan(0, logger.Count);
			AssertEquals("A log entry should be added for critical exceptions causing service task to get cancelled", "Information|Service Task Shutdown. Reason: Disaster\r\nInsufficient memory to continue the execution of the program.\r\n", logger[logger.Count - 1]);
		}

		public void TestLogRecordByCancellationToken()
		{
			CreateSimpleIncomingMessageForTest();

			var tokenSource = new CancellationTokenSource();

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				if (factory.NameForDebugging == "Universal Message Processing")
				{
					tokenSource.Cancel();
				}
			});

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			AssertExceptionThrown<OperationCanceledException>(() => serviceTask.RunTask(tokenSource.Token));

			AssertLessThan(0, logger.Count);
			AssertEquals("A log entry should be added for manual cancellation of service task", "Information|Service Task Shutdown. Reason: The operation was canceled.\r\n", logger[logger.Count - 1]);
		}

		public void TestTransactinExceptionLogRecordedNoExceptionThrown()
		{
			CreateSimpleIncomingMessageForTest();

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				if (factory.NameForDebugging == "Universal Message Processing")
				{
					Db.NewAdminConnection().ExecuteNonQuery("kill " + Db.Connection.SPID);
				}
			});

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			AssertNoExceptionThrown(serviceTask.RunTask);

			AssertLessThan(0, logger.Count);
			AssertEquals("Information|Service Task Shutdown. Reason: Transaction has been rolled back in the server (application transaction count pending reset).\r\n", logger[logger.Count - 1]);
			Env.Instance.UserContextManagerForTesting.ClearCurrentThread();
		}

		public void TestLockTimeoutsExceptionIsHandled()
		{
			var lockTimeoutException = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(1222, byte.MaxValue, byte.MinValue, "", "Lock request time out period exceeded.", string.Empty, 0)
				));

			var mockUniversalProcessingManager = new Mock<IUniversalProcessingManager>();

			mockUniversalProcessingManager.Setup(x => x.Logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
			mockUniversalProcessingManager.Setup(x => x.ExecuteBatch(It.IsAny<CancellationToken>()))
										  .Callback<CancellationToken>((setting) => {
											  throw lockTimeoutException;
										  });
			mockUniversalProcessingManager.Setup(x => x.FlipGrEngine())
										  .Callback(() =>
										  {
											  throw lockTimeoutException;
										  });

			var serviceTaskMock = new Mock<ServiceTask>();

			serviceTaskMock.Setup(x => x.CreateUniversalProcessingManager(It.IsAny<GrEngineServiceSetting>()))
						   .Returns(mockUniversalProcessingManager.Object);
			AssertNoExceptionThrown(() => serviceTaskMock.Object.RunTask(CancellationToken.None));

			Enterprise.Registry.Business.eServices.eAdaptorRegistry.Instance.AllowParallelUMI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			serviceTaskMock.Setup(x => x.ServiceSetting)
						   .Returns(GrEngineServiceSetting.Flipper);
			AssertNoExceptionThrown(() => serviceTaskMock.Object.RunTask(CancellationToken.None));
		}

		public void TestUserRelatedConstraintViolationsDoNotErrorReport()
		{
			var shipment = (IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "S00001000";

			string messageWithLargeImage = $@"<UniversalEvent>	
	<Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
		<EventTime>2017-07-01T20:17:41</EventTime>
		<EventType>CES</EventType>
		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>More TOAST!!!</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var incomingMessage = GetMessageRowWithValidMessageContent();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			incomingMessage.EM_MessageNum = "87654321";
			incomingMessage.EM_MessageText = messageWithLargeImage;

			Factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				if (factory.NameForDebugging == "Universal Message Processing")
				{
					var row = (Factory.New<DummyBusinessObject>() as INeedRow).Row;
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							SqlExceptionBuilder.CreateSqlError(547,
								byte.MaxValue,
								byte.MinValue,
								Constants.ProductName,
								"The INSERT statement conflicted with the CHECK constraint \"Constraint_ABL_RL_NKPortOfLoading\". The conflict occurred in database \"Odyssey\", table \"dbo.AsycudaBill\", column 'ABL_RL_NKPortOfLoading'.",
								"",
								1)
						));
					throw new ZSaveException(new ZDataException(sqlException, row, Db.Connection), Factory);
				}
			});

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			serviceTask.RunTask();

			var reloadedMessage = new BusinessObjectFactory().Load<IEDIMessage>(incomingMessage.PK);

			AssertEquals("Message EM_Status: ", EDIMessageStatusList.Codes.Rejected, reloadedMessage.EM_Status);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		[TestDate(2022, 1, 1)]
		public void TestMessageProcessing_CannotSaveAnythingToTheDatabase()
		{
			var inboundXml =
		$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";
			var uniFactory = new UniversalObjectFactory();

			var message = TestCaseWithFactoryAndMessagingHelpers.GetQueuedUniversalShipmentMessage(uniFactory, inboundXml, true);

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) => throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, null), factory));

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message = new BusinessObjectFactory().Load<IEDIMessage>(message.PK);
			AssertEquals("We can't save anything to the DB, if this test timesout it implies we tried to process the message infinitely", EDIMessageStatusList.Codes.Queued, message.EM_Status);
		}

		public void TestUnsupportedMessagesAreRejected()
		{
			var invalidMessage1 = GetMessageRowWithValidMessageContent();
			invalidMessage1.EM_ApplicationCode = "UDM";
			invalidMessage1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest;

			Factory.Save();

			// Setup Service Task and Run.
			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			invalidMessage1 = newFactory.Load<EDIMessage>(invalidMessage1.PK);
			AssertEquals("InvalidMessage1 should be rejected", EDIMessage.Status.Rejected, invalidMessage1.EM_Status);

			var noteText = ((StmNoteCollection)invalidMessage1.Notes.GetAllNotes())[0].ST_NoteDataAsText;
			AssertContains("Unsupported Messaged should have a note", $"Messages of Sub Type [{EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest}] can only be processed using eAdaptor HTTP+XML", noteText);
		}

		public void TestCollectionIsDeactivatedAfterProcessing()
		{
			var inboundXml =
@"<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WorkItem</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Complete Work</Summary>
  </Activity>
</UniversalActivity>
";

			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalActivity;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = inboundXml;

			var bizo = message.Factory.New<DummyWithDependentsBusinessObject>();
			var collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(bizo);
			collection.AddNew();
			((IBusiness)collection).IncrementReadOnlyIncludingChildren();

			Factory.Save();

			var processor = new UniversalProcessingManager(new string[] { message.EM_MessageSubType }, Array.Empty<string>());
			var batch = new BaseMessageProcessor<XmlEDIMessage>.DisposableBatch(new XmlEDIMessage[] { message });

			AssertEquals(false, collection.IsDeactivated);
			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
			processor.ProcessBatch(batch, CancellationToken.None, new FailedMessagesManager(RetryType.CurrentExecution, (int x, EDIMessage m, Exception e, int retryAttempts, string additionalErrorReportMessage) => false));
			AssertEquals(true, collection.IsDeactivated);
		}

		public void TestUnkownMessagesAreRejected()
		{
			var invalidMessage1 = GetMessageRowWithValidMessageContent();
			invalidMessage1.EM_ApplicationCode = "UDM";
			invalidMessage1.EM_MessageSubType = "XXX";

			Factory.Save();

			// Setup Service Task and Run.
			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			invalidMessage1 = newFactory.Load<EDIMessage>(invalidMessage1.PK);
			AssertEquals("InvalidMessage1 should be rejected", EDIMessage.Status.Rejected, invalidMessage1.EM_Status);

			var noteText = ((StmNoteCollection)invalidMessage1.Notes.GetAllNotes())[0].ST_NoteDataAsText;
			AssertContains("Unsupported Messaged should have a note", "Invalid Message Sub Type [XXX]", noteText);
		}

		public void TestRightMessageGetProcessed()
		{
			// Setup Valid Consol with Master Bill 123-45678901.
			var consol = SetupConsol();
			var invalidMessage1 = GetMessageRowWithValidMessageContent();
			invalidMessage1.EM_ApplicationCode = "XXX";

			var invalidMessage2 = GetMessageRowWithValidMessageContent();
			invalidMessage2.EM_MessageType = "XXX";

			var invalidMessage3 = GetMessageRowWithValidMessageContent();
			invalidMessage3.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalSchedule;

			var invalidMessage4 = GetMessageRowWithValidMessageContent();
			invalidMessage4.EM_Status = "XXX";

			var invalidMessage5 = GetMessageRowWithValidMessageContent();
			invalidMessage5.EM_MessageText = "Doesn'tHaveRealXMLInIt.";

			var validMessage = GetMessageRowWithValidMessageContent();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH");
			var athLogs = consol.GetLogs().Find(query);
			AssertEquals("Precondition: Should be no ATH logs belongs to consol", 0, athLogs.Length);

			// Setup Service Task and Run.
			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load(ObjectFactory.GetType<IForwardingConsol>(), consol.PK);
			athLogs = consol.GetLogs().Find(query);
			AssertEquals("Should have the new 'ATH' event against the consol", 1, athLogs.Length);

			// Question: Why do I need to create a new Factory here....
			// Make sure "Invalid" messages 1 - 4 are ignored, and 5 is set to "Failed" (Check EM_Status on all).

			invalidMessage1 = newFactory.Load<EDIMessage>(invalidMessage1.PK);
			AssertEquals("InvalidMessage1.EM_Status should not be changed", EDIMessage.Status.Queued, invalidMessage1.EM_Status);

			invalidMessage2 = newFactory.Load<EDIMessage>(invalidMessage2.PK);
			AssertEquals("InvalidMessage2.EM_Status should not be changed", EDIMessage.Status.Queued, invalidMessage2.EM_Status);

			invalidMessage3 = newFactory.Load<EDIMessage>(invalidMessage3.PK);
			AssertEquals("InvalidMessage3.EM_Status should not be changed", EDIMessage.Status.Queued, invalidMessage3.EM_Status);

			invalidMessage4 = newFactory.Load<EDIMessage>(invalidMessage4.PK);
			AssertEquals("InvalidMessage4.EM_Status should not be changed", "XXX", invalidMessage4.EM_Status);

			invalidMessage5 = newFactory.Load<EDIMessage>(invalidMessage5.PK);
			AssertEquals("InvalidMessage5.EM_Status should be changed to 'Failed'", EDIMessage.Status.Rejected, invalidMessage5.EM_Status);

			validMessage = newFactory.Load<EDIMessage>(validMessage.PK);
			AssertEquals("ValidMessage.EM_Status should be set to 'Recognised'", EDIMessage.Status.ProcessedOK, validMessage.EM_Status);
		}

		public void TestImportAttachmentDocumentCollection_UniversalShipment()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			factory.Save();

			var message = CreateQueuedUniversalMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>U2lubmUgRmlhbm5hIEbDoWlsLA0KYXTDoSBmYW9pIGdoZWFsbCBhZyDDiWlyaW5uLA0KQnXDrW9uIGTDoXIgc2x1YQ0KdGhhciB0b2lubiBkbyByw6FpbmlnIGNodWdhaW5uLA0KRmFvaSBtaMOzaWQgYmhlaXRoIHNhb3INClNlYW50w61yIMOhciBzaW5zZWFyIGZlYXN0YSwNCk7DrSBmaMOhZ2ZhciBmYW9pbiB0w61vcsOhbiBuw6EgZmFvaW4gdHLDoWlsbC4NCkFub2NodCBhIHRow6lhbSBzYSBiaGVhcm5hIGJoYW9pbCwNCkxlIGdlYW4gYXIgR2hhZWlsLCBjaHVuIGLDoWlzIG7DsyBzYW9pbCwNCkxlIGd1bm5hLXNjcsOpYWNoIGZhb2kgbMOhbWhhY2ggbmEgYnBpbMOpYXIsDQpTZW8gbGliaCBjYW5haWcgYW1ocsOhbiBuYSBiaEZpYW5uLg==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
      <AttachedDocument>
        <FileName>CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>U2lubmUgRmlhbm5hIEbDoWlsLA0KYXTDoSBmYW9pIGdoZWFsbCBhZyDDiWlyaW5uLA0KQnXDrW9uIGTDoXIgc2x1YQ0KdGhhciB0b2lubiBkbyByw6FpbmlnIGNodWdhaW5uLA0KRmFvaSBtaMOzaWQgYmhlaXRoIHNhb3INClNlYW50w61yIMOhciBzaW5zZWFyIGZlYXN0YSwNCk7DrSBmaMOhZ2ZhciBmYW9pbiB0w61vcsOhbiBuw6EgZmFvaW4gdHLDoWlsbC4NCkFub2NodCBhIHRow6lhbSBzYSBiaGVhcm5hIGJoYW9pbCwNCkxlIGdlYW4gYXIgR2hhZWlsLCBjaHVuIGLDoWlzIG7DsyBzYW9pbCwNCkxlIGd1bm5hLXNjcsOpYWNoIGZhb2kgbMOhbWhhY2ggbmEgYnBpbMOpYXIsDQpTZW8gbGliaCBjYW5haWcgYW1ocsOhbiBuYSBiaEZpYW5uLg==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
  </Shipment>
</UniversalShipment>");

			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadBizo = new BusinessObjectFactory().Load<IForwardingShipment>(bizo.PK);
			var savedEDocs = ((IDocManagerSupport)reloadBizo).DocManagerInfo.Files.Cast<IeDoc>();
			AssertEquals(2, savedEDocs.Count());

			var eDoc1 = savedEDocs.FirstOrDefault(e => e.FileName == "CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf");
			var eDoc2 = savedEDocs.FirstOrDefault(e => e.FileName == "CIV-Commercial_Invoice-DOUCMENT_NUMBER[2].pdf");
			AssertNotNull(eDoc1);
			AssertNotNull(eDoc2);

			var ddiLogs = (reloadBizo as BusinessObject).GetLogs().Find(l => l.SL_SE_NKEvent == Events.DocumentImportedCode);
			var ddi1 = ddiLogs.FirstOrDefault(l => l.SL_Reference == eDoc1.DocType + "|" + eDoc1.UniqueKey.ToString());
			var ddi2 = ddiLogs.FirstOrDefault(l => l.SL_Reference == eDoc2.DocType + "|" + eDoc2.UniqueKey.ToString());
			AssertNotNull("Expecting one DDI event per document", ddi1);
			AssertNotNull("Expecting one DDI event per document", ddi2);

			using (var linkedMessage1 = ddi1.RelatedEDIMessage)
			using (var linkedMessage2 = ddi2.RelatedEDIMessage)
			{
				AssertEquals("DDI event should be linked to message", "XUS", linkedMessage1.Message.EM_MessageSubType);
				AssertEquals("DDI event should be linked to message", "XUS", linkedMessage2.Message.EM_MessageSubType);
			}
		}

		#region Import Document ForwardingBooking

		const string UniversalShipmentImportDocumentToForwardingBooking = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingBooking</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>U2lubmUgRmlhbm5hIEbDoWlsLA0KYXTDoSBmYW9pIGdoZWFsbCBhZyDDiWlyaW5uLA0KQnXDrW9uIGTDoXIgc2x1YQ0KdGhhciB0b2lubiBkbyByw6FpbmlnIGNodWdhaW5uLA0KRmFvaSBtaMOzaWQgYmhlaXRoIHNhb3INClNlYW50w61yIMOhciBzaW5zZWFyIGZlYXN0YSwNCk7DrSBmaMOhZ2ZhciBmYW9pbiB0w61vcsOhbiBuw6EgZmFvaW4gdHLDoWlsbC4NCkFub2NodCBhIHRow6lhbSBzYSBiaGVhcm5hIGJoYW9pbCwNCkxlIGdlYW4gYXIgR2hhZWlsLCBjaHVuIGLDoWlzIG7DsyBzYW9pbCwNCkxlIGd1bm5hLXNjcsOpYWNoIGZhb2kgbMOhbWhhY2ggbmEgYnBpbMOpYXIsDQpTZW8gbGliaCBjYW5haWcgYW1ocsOhbiBuYSBiaEZpYW5uLg==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
      <AttachedDocument>
        <FileName>CIV-Commercial_Invoice-DOUCMENT_NUMBER[2].pdf</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>U2lubmUgRmlhbm5hIEbDoWlsLA0KYXTDoSBmYW9pIGdoZWFsbCBhZyDDiWlyaW5uLA0KQnXDrW9uIGTDoXIgc2x1YQ0KdGhhciB0b2lubiBkbyByw6FpbmlnIGNodWdhaW5uLA0KRmFvaSBtaMOzaWQgYmhlaXRoIHNhb3INClNlYW50w61yIMOhciBzaW5zZWFyIGZlYXN0YSwNCk7DrSBmaMOhZ2ZhciBmYW9pbiB0w61vcsOhbiBuw6EgZmFvaW4gdHLDoWlsbC4NCkFub2NodCBhIHRow6lhbSBzYSBiaGVhcm5hIGJoYW9pbCwNCkxlIGdlYW4gYXIgR2hhZWlsLCBjaHVuIGLDoWlzIG7DsyBzYW9pbCwNCkxlIGd1bm5hLXNjcsOpYWNoIGZhb2kgbMOhbWhhY2ggbmEgYnBpbMOpYXIsDQpTZW8gbGliaCBjYW5haWcgYW1ocsOhbiBuYSBiaEZpYW5uLg==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
  </Shipment>
</UniversalShipment>";

		public void TestImportAttachmentDocumentCollection_ForwardingBooking_UniversalShipment()
		{
			var factory = new BusinessObjectFactory();
			var quickBooking1 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(Freight.Integration.QuoteBookingType.QuickBooking, factory);
			var quickBooking2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(Freight.Integration.QuoteBookingType.QuickBooking, factory);

			(quickBooking1.ForwardingShipment as IForwardingShipment).JS_UniqueConsignRef = "S00001001";
			(quickBooking2.ForwardingShipment as IForwardingShipment).JS_UniqueConsignRef = "S00001002";
			(quickBooking2.ForwardingShipment as IForwardingShipment).JS_IsForwardRegistered = true;

			factory.Save();

			var message1 = CreateQueuedUniversalMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, string.Format(UniversalShipmentImportDocumentToForwardingBooking, "S00001001"));
			var message2 = CreateQueuedUniversalMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, string.Format(UniversalShipmentImportDocumentToForwardingBooking, "S00001002"));
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadQuickBooking1 = new BusinessObjectFactory().Load<IQuotedBooking>(quickBooking1.ForwardingShipment.PK);
			var savedEDocs1 = ((IDocManagerSupport)reloadQuickBooking1).DocManagerInfo.Files.Cast<IeDoc>().ToArray();
			AssertEquals(2, savedEDocs1.Length);

			var eDoc1 = savedEDocs1.FirstOrDefault(e => e.FileName == "CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf");
			var eDoc2 = savedEDocs1.FirstOrDefault(e => e.FileName == "CIV-Commercial_Invoice-DOUCMENT_NUMBER[2].pdf");
			AssertNotNull(eDoc1);
			AssertNotNull(eDoc2);

			var ddiLogs1 = (reloadQuickBooking1 as BusinessObject).GetLogs().Find(l => l.SL_SE_NKEvent == Events.DocumentImportedCode).ToArray();
			AssertEquals(2, ddiLogs1.Length);
			var ddi1 = ddiLogs1.FirstOrDefault(l => l.SL_Reference == eDoc1.DocType + "|" + eDoc1.UniqueKey);
			var ddi2 = ddiLogs1.FirstOrDefault(l => l.SL_Reference == eDoc2.DocType + "|" + eDoc2.UniqueKey);
			AssertNotNull("Expecting one DDI event per document", ddi1);
			AssertNotNull("Expecting one DDI event per document", ddi2);

			using (var linkedMessage1 = ddi1.RelatedEDIMessage)
			using (var linkedMessage2 = ddi2.RelatedEDIMessage)
			{
				AssertEquals("DDI event should be linked to message", "XUS", linkedMessage1.Message.EM_MessageSubType);
				AssertEquals("DDI event should be linked to message", "XUS", linkedMessage2.Message.EM_MessageSubType);
			}

			var reloadedMessage1 = new BusinessObjectFactory().Load<EDIMessage>(message1.PK);
			AssertEquals("PRS", reloadedMessage1.EM_Status);
			AssertMultilineASCIIEquals(@"Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
Successfully Added eDoc: CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf.
Successfully Added eDoc: CIV-Commercial_Invoice-DOUCMENT_NUMBER[2].pdf.
Updated Quick Booking - Booking (S00001001) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001001).", reloadedMessage1.GetLogNoteText());

			var reloadQuickBooking2 = new BusinessObjectFactory().Load<IQuotedBooking>(quickBooking2.ForwardingShipment.PK);
			var savedEDocs2 = ((IDocManagerSupport)reloadQuickBooking2).DocManagerInfo.Files.Cast<IeDoc>().ToArray();
			AssertEquals(0, savedEDocs2.Length);

			var ddiLogs2 = (reloadQuickBooking2 as BusinessObject).GetLogs().Find(l => l.SL_SE_NKEvent == Events.DocumentImportedCode).ToArray();
			AssertEquals(0, ddiLogs2.Length);

			var reloadedMessage2 = new BusinessObjectFactory().Load<EDIMessage>(message2.PK);
			AssertEquals("ERR", reloadedMessage2.EM_Status);
			AssertMultilineASCIIEquals(@"Successfully loaded matching QuotedBooking.
Error - Cannot populate QuotedBooking because:
[*XML Targeted a converted Booking. Target Type should be ForwardingShipment for updating converted Bookings.*]
Error - Error saving, but nothing was reported as being updated.", reloadedMessage2.GetLogNoteText());
		}

		const string UniversalEventImportDocumentToForwardingBooking = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
<Event>
<DataContext>
  <DataTargetCollection>
    <DataTarget>
      <Type>ForwardingBooking</Type>
      <Key>{0}</Key>
    </DataTarget>
  </DataTargetCollection>
</DataContext>
<EventTime>2022-06-09T12:32:12.42</EventTime>
<EventType>DDI</EventType>
<EventReference>PAL</EventReference>
<IsEstimate>false</IsEstimate>
<AttachedDocumentCollection>
  <AttachedDocument>
    <FileName>Tiny PreAlert.txt</FileName>
    <ImageData>SGV5ISBXYWtlIFVwIQ==</ImageData>
    <Type>
      <Code>PAL</Code>
      <Description>Pre Alert</Description>
    </Type>
    <IsPublished>true</IsPublished>
  </AttachedDocument>
</AttachedDocumentCollection>
</Event>
</UniversalEvent>";

		public void TestImportAttachmentDocumentCollection_ForwardingBooking_UniversalEvent()
		{
			var factory = new BusinessObjectFactory();
			var quickBooking1 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(Freight.Integration.QuoteBookingType.QuickBooking, factory);
			var quickBooking2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(Freight.Integration.QuoteBookingType.QuickBooking, factory);

			(quickBooking1.ForwardingShipment as IForwardingShipment).JS_UniqueConsignRef = "S00001001";
			(quickBooking2.ForwardingShipment as IForwardingShipment).JS_UniqueConsignRef = "S00001002";
			(quickBooking2.ForwardingShipment as IForwardingShipment).JS_IsForwardRegistered = true;

			factory.Save();

			var message1 = CreateQueuedUniversalMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, string.Format(UniversalEventImportDocumentToForwardingBooking, "S00001001"));
			var message2 = CreateQueuedUniversalMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, string.Format(UniversalEventImportDocumentToForwardingBooking, "S00001002"));
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadQuickBooking1 = new BusinessObjectFactory().Load<IQuotedBooking>(quickBooking1.ForwardingShipment.PK);
			var savedEDocs1 = ((IDocManagerSupport)reloadQuickBooking1).DocManagerInfo.Files.Cast<IeDoc>().ToArray();
			AssertEquals(1, savedEDocs1.Length);

			var eDoc = savedEDocs1.FirstOrDefault(e => e.FileName == "Tiny PreAlert.txt");
			AssertNotNull(eDoc);

			var ddiLogs1 = (reloadQuickBooking1 as BusinessObject).GetLogs().Find(l => l.SL_SE_NKEvent == Events.DocumentImportedCode).ToArray();
			AssertEquals(1, ddiLogs1.Length);
			var ddi = ddiLogs1.FirstOrDefault(l => l.SL_Reference == eDoc.DocType + "|" + eDoc.UniqueKey);
			AssertNotNull("Expecting one DDI event per document", ddi);

			using (var linkedMessage1 = ddi.RelatedEDIMessage)
			{
				AssertEquals("DDI event should be linked to message", "XUE", linkedMessage1.Message.EM_MessageSubType);
			}

			var reloadedMessage1 = new BusinessObjectFactory().Load<EDIMessage>(message1.PK);
			AssertEquals("PRS", reloadedMessage1.EM_Status);
			AssertMultilineASCIIEquals(@"Successfully Added eDoc: Tiny PreAlert.txt.
Adding eDoc with a document type of PAL and name of Tiny PreAlert.txt.
Linked Event to Quick Booking - Booking (S00001001).", reloadedMessage1.GetLogNoteText());

			var reloadQuickBooking2 = new BusinessObjectFactory().Load<IQuotedBooking>(quickBooking2.ForwardingShipment.PK);
			var savedEDocs2 = ((IDocManagerSupport)reloadQuickBooking2).DocManagerInfo.Files.Cast<IeDoc>().ToArray();
			AssertEquals(0, savedEDocs2.Length);

			var ddiLogs2 = (reloadQuickBooking2 as BusinessObject).GetLogs().Find(l => l.SL_SE_NKEvent == Events.DocumentImportedCode).ToArray();
			AssertEquals(0, ddiLogs2.Length);

			var reloadedMessage2 = new BusinessObjectFactory().Load<EDIMessage>(message2.PK);
			AssertEquals("ERR", reloadedMessage2.EM_Status);
			AssertMultilineASCIIEquals(@"Error - Cannot update QuotedBooking because:
[*XML Targeted a converted Booking. Target Type should be ForwardingShipment for updating converted Bookings.*]", reloadedMessage2.GetLogNoteText());
		}

		#endregion

		public void TestImageFormatExceptionHandling()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			factory.Save();

			var xml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>DDI</EventType>
	<AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>BAD_FILE.JPG</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
  </Event>
</UniversalEvent>";

			var message = CreateQueuedUniversalMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, xml);
			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertNotContains("Message Log Note", "Warning - Could not link Attached Document(file name: BAD_FILE.JPG) to Shipment S00001001. The image data of this file is invalid.".Trim(), message.GetLogNoteText());
		}

		public void TestCorrectUserContextDuringSaveForUniversalEvent()
		{
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "Ref_20130918_1";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			var masterBillNum = "Mbl_20130918_1";
			consol[JobConsolSchema.JK_MasterBillNum] = masterBillNum;

			var messageContent = string.Format(InboundUniversalEvent, masterBillNum);
			var message = CreateQueuedUniversalMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, messageContent);
			Factory.Save();

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var dataMessageFactory = message.GetUniversalDataMessageFactory();
			var codeMapper = new CodeMappingManager(xmlSessionTracker);
			dataMessageFactory.TopLevelDataObjectFactory.TryGetTopLevelDataObject(message, codeMapper, xmlSessionTracker, out var topLevelDataObject);
			dataMessageFactory.UserContextExtractor.TryGetUserContext(message, topLevelDataObject, xmlSessionTracker, out var msgUserContext);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var userContext = new UserContext(user, branch.PK.ToGuid(), department.PK.ToGuid());

			using (Env.Instance.SetTemporaryUserContext(userContext))
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) =>
				{
					AssertEquals(msgUserContext.Branch.PK, Env.Instance.CurrentBranchPK);
					AssertEquals(msgUserContext.Company.PK, Env.Instance.CurrentCompanyPK);
					AssertEquals(msgUserContext.User.PK, Env.Instance.CurrentUserPK);
				});

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();
			}
		}

		const string InboundUniversalEvent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <EventTime>2011-02-15T12:03:04</EventTime>
    <EventType>ATH</EventType>
    <DataContext>
      <DataProvider>ediEnterprise</DataProvider>
    </DataContext>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>{0}</Value>
      </Context>
    </ContextCollection>
</Event>
</UniversalEvent>";

		public void TestCorrectUserContextDuringSaveForUniversalShipment()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var uniqueConsignRef = "Ref_20130918_1";
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = uniqueConsignRef;
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;

			var messageContent = string.Format(InBoundUniversalShipment, uniqueConsignRef);
			var message = CreateQueuedUniversalMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, messageContent);
			Factory.Save();

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var dataMessageFactory = message.GetUniversalDataMessageFactory();
			var codeMapper = new CodeMappingManager(xmlSessionTracker);
			dataMessageFactory.TopLevelDataObjectFactory.TryGetTopLevelDataObject(message, codeMapper, xmlSessionTracker, out var topLevelDataObject);
			dataMessageFactory.UserContextExtractor.TryGetUserContext(message, topLevelDataObject, xmlSessionTracker, out var msgUserContext);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var userContext = new UserContext(user, branch.PK.ToGuid(), department.PK.ToGuid());

			using (Env.Instance.SetTemporaryUserContext(userContext))
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) =>
				{
					AssertEquals(msgUserContext.Branch.PK, Env.Instance.CurrentBranchPK);
					AssertEquals(msgUserContext.Company.PK, Env.Instance.CurrentCompanyPK);
					AssertEquals(msgUserContext.User.PK, Env.Instance.CurrentUserPK);
				});

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();
			}
		}

		const string InBoundUniversalShipment =
@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <CodesMappedToTarget>true</CodesMappedToTarget>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key>{0}</Key>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<JobCosting>
	  <ChargeLineCollection>
		<ChargeLine>
		  <ChargeCode>
			<Code>BAF</Code>
		  </ChargeCode>
		  <ImportMetaData>
			<Instruction>UpdateAndInsertIfNotFound</Instruction>
			<MatchingCriteriaCollection>
			  <MatchingCriteria>
				<FieldName>ChargeCode</FieldName>
				<Value>BAF</Value>
			  </MatchingCriteria>
			  <MatchingCriteria>
				<FieldName>SellOSCurrency</FieldName>
				<Value>JPY</Value>
			  </MatchingCriteria>
			</MatchingCriteriaCollection>
		  </ImportMetaData>
		  <SellOSAmount>00000001418</SellOSAmount>
		  <SellOSCurrency>
			<Code>JPY</Code>
		  </SellOSCurrency>
		</ChargeLine>
	  </ChargeLineCollection>
	</JobCosting>
  </Shipment>
</UniversalShipment>";

		public void TestBadUserContextDuringSaveErrorReported()
		{
			var consol = SetupConsol();
			var validMessage = GetMessageRowWithValidMessageContent();
			var trigger = ((IWorkflowProvider)consol).WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger template application";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			notification.PQ_FieldName = "<JK_MasterBillNum>";
			notification.PQ_FieldValue = "123";
			Factory.Save();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			IDisposable environment = null;
			var userContext = new UserContext(user, branch.PK.ToGuid(), department.PK.ToGuid());

			using (Env.Instance.SetTemporaryUserContext(userContext))
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) =>
				{
					if (thefactory.NameForDebugging == "Universal Message Processing")
					{
						var factory = new BusinessObjectFactory();
						var newUser = factory.NewWithValidTestData<GlbStaff>();
						var newBranch = factory.NewWithValidTestData<GlbBranch>();
						var newDepartment = factory.NewWithValidTestData<GlbDepartment>();

						var newUserContext = new UserContext(newUser, newBranch.PK.ToGuid(), newDepartment.PK.ToGuid());
						environment = Env.Instance.SetTemporaryUserContext(newUserContext);
					}
				});

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();
			}

			AssertEquals("Invalid workflow user context error should have been reported", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			environment?.Dispose();
		}

		public void TestSQLExceptionDoesNotLeakUserContext()
		{
			var consol = SetupConsol();
			var validMessage = GetMessageRowWithValidMessageContent();
			Factory.Save();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var userContext = new UserContext(user, branch.PK.ToGuid(), department.PK.ToGuid());
			var fired = false;
			Env.Instance.UserContextChanging += OnUserContextChanging;

			try
			{
				using (Env.Instance.SetTemporaryUserContext(userContext))
				{
					var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
					// This is enough to assert the correct behaviour will eventuate. An unhandled exception that escapes from
					// a service task will prevent processing in the incorrect user context, and will prevent the UserContextLeakTracker
					// from reporting an issue.
					AssertExceptionThrown<UserContextLostException>(serviceTask.RunTask);
				}

				Env.Instance.UserContextManagerForTesting.ClearCurrentThread();
			}
			finally
			{
				Env.Instance.UserContextChanging -= OnUserContextChanging;
			}

			void OnUserContextChanging(object sender, IUserContextChangingEventArgs userContextChangingDetails)
			{
				if (userContextChangingDetails.IsRevert && !fired)
				{
					fired = true;
					var error = SqlExceptionBuilder.CreateSqlError(942, 1, 1, "", "Test SQL Exception should have been caught", "", 1);
					var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					var sqlException = SqlExceptionBuilder.CreateSqlException(errors);
					throw sqlException;
				}
			}
		}

		public void TestSQLExceptionDoesNotLeakUserContext_OnTheSet()
		{
			var consol = SetupConsol();
			var validMessage = GetMessageRowWithValidMessageContent();
			Factory.Save();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			void OnUserContextChanging(object sender, IUserContextChangingEventArgs userContextChangingDetails)
			{
				var error = SqlExceptionBuilder.CreateSqlError(942, 1, 1, "", "Test SQL Exception should have been caught", "", 1);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errors);
				throw sqlException;
			}
			var userContext = new UserContext(user, branch.PK.ToGuid(), department.PK.ToGuid());
			try
			{
				Env.Instance.UserContextChanged += OnUserContextChanging;

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				// This is enough to assert the correct behaviour will eventuate. An unhandled exception that escapes from
				// a service task will prevent processing in the incorrect user context, and will prevent the UserContextLeakTracker
				// from reporting an issue.
				AssertExceptionThrown<UserContextLostException>(serviceTask.RunTask);

				Env.Instance.UserContextManagerForTesting.ClearCurrentThread();
			}
			finally
			{
				Env.Instance.UserContextChanged -= OnUserContextChanging;
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Shipment",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalShipment),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Event",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalEvent),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Transaction",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransaction),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Transaction Batch",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch),
				};
			}
		}

		#region MessageContent

		const string IndexedXMLEventContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent>
  <Event>
    <EventType>ATH</EventType>
    <EventTime>07-NOV-2010 05:47</EventTime>
    <EventReference>Dummy Description</EventReference>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>{0,3}-45678901</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

		const string ValidXMLEventContentForMasterBill123_45678901 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent>
  <Event>
    <EventType>ATH</EventType>
    <EventTime>07-NOV-2010 05:47</EventTime>
    <EventReference>Dummy Description</EventReference>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>123-45678901</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

		const string DiscardingMessageContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent>
  <Event>
  </Event>
</UniversalEvent>
";

		#endregion

		#region Implementation

		protected BusinessObject SetupConsol() => SetupConsol(Factory);

		internal static BusinessObject SetupConsol(BusinessObjectFactory factory)
		{
			var consol = factory.New(ObjectFactory.GetType<IForwardingConsol>());
			// Always set transport mode before master bill number
			consol[JobConsolSchema.JK_TransportMode] = Constants.TransportModes.Air;
			consol[JobConsolSchema.JK_MasterBillNum] = "12345678901";
			return consol;
		}

		protected EDIMessage GetMessageRowWithValidMessageContent()
		{
			return GetMessageRowWithValidMessageContent(Factory);
		}

		internal static EDIMessage GetMessageRowWithValidMessageContent(BusinessObjectFactory factory)
		{
			return GetMessageRowWithValidMessageContent(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				XmlEDIMessage.Status.Queued);
		}

		internal static EDIMessage GetMessageRowWithRandomMessageContent(BusinessObjectFactory factory, int index)
		{
			return GetMessageRowWithValidMessageContent(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				XmlEDIMessage.Status.Queued,
				string.Format(CultureInfo.InvariantCulture, IndexedXMLEventContent, index));
		}

		internal static EDIMessage GetMessageRowWithDiscardingMessageContent(BusinessObjectFactory factory)
		{
			return GetMessageRowWithValidMessageContent(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				XmlEDIMessage.Status.Queued,
				DiscardingMessageContent);
		}

		[ThreadSafe]
		static int messageId = 0;

		internal static EDIMessage GetMessageRowWithValidMessageContent(BusinessObjectFactory factory, string applicationCode, string messageType, string messageSubType, string status, string message = null)
		{
			var message1 = factory.New<EDIMessage>();

			message1.EM_ApplicationCode = applicationCode;
			message1.EM_MessageType = messageType;
			message1.EM_MessageSubType = messageSubType;
			message1.EM_Status = status;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = message ?? ValidXMLEventContentForMasterBill123_45678901;
			message1.EM_MessageNum = Interlocked.Increment(ref messageId).ToString("D20");
			return message1;
		}

		#endregion

		internal EDIMessage GetMessageRowWithValidMessageContentExposed()
		{
			return GetMessageRowWithValidMessageContent();
		}

		EDIMessage CreateQueuedUniversalMessage(string messageSubType, string messageContent)
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = messageSubType;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = messageContent;
			message.EM_MessageNum = Interlocked.Increment(ref messageId).ToString("D20");
			return message;
		}

		#region UniversalActivity

		public void TestProcessUniversalActivity()
		{
			var inboundXml =
@"<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WorkItem</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Complete Work</Summary>
  </Activity>
</UniversalActivity>
";

			var message1 = EDIMessageTestFactory.New(Factory);

			message1.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message1.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalActivity;
			message1.EM_Status = XmlEDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = inboundXml;

			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message1.PK);

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, reloadedMessage.EM_Status);

			AssertContains("Information|Starting processing Message #", logger.ToString());
			AssertContains("Information|Added Work Item from UniversalActivity.", logger.ToString());
			AssertContains("Information|Successfully saved WI00000001 - Complete Work.", logger.ToString());
			AssertContains("Information|Finished processing Message #", logger.ToString());
		}

		#endregion

		#region UniversalTransactionBatch

		public void TestUniversalTransactionBatchProcess()
		{
			var messageText1 = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<BatchType>
			<Code>TST</Code>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

			var message1 = EDIMessageTestFactory.New(Factory);

			message1.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message1.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			message1.EM_Status = XmlEDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = messageText1;

			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message1.PK);

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, reloadedMessage.EM_Status);

			AssertContains("Information|Starting processing Message #", logger.ToString());
			AssertContains("Information|Finished processing Message #", logger.ToString());

			var messageText2 = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<BatchType>
			<Code>XXX</Code>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

			var message2 = EDIMessageTestFactory.New(Factory);

			message2.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message2.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			message2.EM_Status = XmlEDIMessage.Status.Queued;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = messageText2;

			Factory.Save();

			serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message2.PK);
			var notes = (StmNoteCollection)reloadedMessage.Notes.GetAllNotes();
			AssertEquals("Processed Messages should only have one note", 1, notes.Count);
			var noteText = notes[0].ST_NoteDataAsText;

			logger = (TestServiceLogger)serviceTask.ServiceLogger;
			AssertEquals(EDIMessageStatusList.Codes.Discarded, reloadedMessage.EM_Status);

			AssertContains("Information|Starting processing Message #", logger.ToString());
			AssertContains("Information|No Module used this Transaction Batch data.", logger.ToString());
			AssertContains("Information|Finished processing Message #", logger.ToString());

			AssertMultilineASCIIEquals("Message Log Note", @"
No Module used this Transaction Batch data.
Message Discarded.
".Trim(), noteText);
		}

		#endregion
	}

	sealed class ServiceTaskTestWithoutTransaction : TestCase
	{
		string RunServiceTask()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();
				return serviceTask.ServiceLogger.ToString();
			}
		}

		[UseSnapshotProtection]
		public void TestShouldRetryOnExceptionCore_PrintAdditionalTriggerActionsInformation()
		{
			var factory = new BusinessObjectFactory();
			var shipment = (IForwardingShipment)factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "S00001000";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger Sample";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03.Code;
			trigger.ProcessTaskNotifications.AddNew();

			var message = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
				<Event>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>ForwardingShipment</Type>
								<Key>{shipment.JS_UniqueConsignRef}</Key>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<AttachedDocumentCollection>
						<AttachedDocument>
							<FileName>imgTest.png</FileName>
							<ImageData>1</ImageData>
							<Type>
								<Code>HCC</Code>
							</Type>
							<IsPublished>false</IsPublished>
						</AttachedDocument>
					</AttachedDocumentCollection>
					<EventTime>2019-12-06T12:12:00</EventTime>
					<EventType>Z03</EventType>
				</Event>
				</UniversalEvent>";

			var unifactory = new UniversalObjectFactory();
			var row = (factory.New<DummyBusinessObject>() as INeedRow).Row;

			var incomingMessage = UMIServiceTaskTest.GetMessageRowWithValidMessageContent(factory);
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			incomingMessage.EM_MessageNum = "87654321";
			incomingMessage.EM_MessageText = message;
			factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				if (factory.NameForDebugging == "Universal Message Processing")
				{
					shipment = factory.Load<IForwardingShipment>(shipment.PK);
					((IWorkflowProvider)shipment).Logs.AddNew(Events.CustomisableEvent03);
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							SqlExceptionBuilder.CreateSqlError(2601, byte.MaxValue, byte.MinValue, "", "Data failed to save because unique index conflict 'NR_UX__P9L_ParentId_P9L_ParentTableCode_P9L_P9T_TemplateTrigger'.", "", 1)
						));

					var exception = new ZSaveException(new ZDataException(sqlException, row, Db.Connection), factory);
					throw exception;
				}
			});

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			serviceTask.RunTask();

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

			var actualMessage = ExceptionReporterTestListener.Instance.GetExceptionMessage(0);

			AssertContains("Trigger Action - Trigger Sample", actualMessage);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[UseSnapshotProtection]
		public void TestDuplicateKeyRowErrorMessage()
		{
			var factory = new BusinessObjectFactory();
			var shipment = (IForwardingShipment)factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "S00001000";

			// WI00600300
			const int maxErrorMessageSize = 10000;
			string messageWithLargeImageStart = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
				<Event>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>ForwardingShipment</Type>
								<Key>{shipment.JS_UniqueConsignRef}</Key>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<AttachedDocumentCollection>
						<AttachedDocument>
							<FileName>imgTest.png</FileName>
							<ImageData>";

			const string messageWithLargeImageEnd = @"</ImageData>
							<Type>
								<Code>HCC</Code>
							</Type>
							<IsPublished>false</IsPublished>
						</AttachedDocument>
					</AttachedDocumentCollection>
					<EventTime>2019-12-06T12:12:00</EventTime>
					<EventType>Z03</EventType>
				</Event>
				</UniversalEvent>";

			// New message will be the max size PLUS the header / footer, so the total length will be over the max
			string messageWithLargeImage = messageWithLargeImageStart + new string('1', maxErrorMessageSize) + messageWithLargeImageEnd;

			var unifactory = new UniversalObjectFactory();
			var row = (factory.New<DummyBusinessObject>() as INeedRow).Row;

			var incomingMessage = UMIServiceTaskTest.GetMessageRowWithValidMessageContent(factory);
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			incomingMessage.EM_MessageNum = "87654321";
			incomingMessage.EM_MessageText = messageWithLargeImage;
			factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				if (factory.NameForDebugging == "Universal Message Processing")
				{
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							// Message here needs to include a valid unique index prefix or this will throw ANOTHER exception
							SqlExceptionBuilder.CreateSqlError(2601, byte.MaxValue, byte.MinValue, "", "Data failed to save because unique index conflict 'NR_UX__P9L_ParentId_P9L_ParentTableCode_P9L_P9T_TemplateTrigger'.", "", 1)
						));

					var exception = new ZSaveException(new ZDataException(sqlException, row, Db.Connection), factory);
					throw exception;
				}
			});

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;

			serviceTask.RunTask();

			var reloadedMessage = new BusinessObjectFactory().Load<IEDIMessage>(incomingMessage.PK);
			var errorPreamble = $@"Message processing failed after {retryAttempts} retries. This is likely an issue on our side.
Message Number: {reloadedMessage.EM_MessageNum}
Message Type: {reloadedMessage.EM_MessageType}
Message SubType: {reloadedMessage.EM_MessageSubType}
Message Status: {EDIMessageStatusList.Codes.ProcessedOK}
Message Application Code: {reloadedMessage.EM_ApplicationCode}
Message Content: ";
			try
			{
				AssertEquals("Message EM_Status: ", EDIMessageStatusList.Codes.Rejected, reloadedMessage.EM_Status);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(errorPreamble.Length + maxErrorMessageSize + System.Environment.NewLine.Length, ExceptionReporterTestListener.Instance.GetExceptionMessage(0).Length);
				AssertEquals(errorPreamble + messageWithLargeImage.Substring(0, maxErrorMessageSize - 3) + "..." + System.Environment.NewLine, ExceptionReporterTestListener.Instance.GetExceptionMessage(0));
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[UseSnapshotProtection]
		public void TestDuplicateTransactionShouldNotThrowRollbackException()
		{
			var factory = new BusinessObjectFactory();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var inboundXml =
			#region xml
FormattableString.Invariant($@"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccountingInvoice</Type>
          <Key>AP INV ART123</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
				<Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
				<Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
			</Company>
			<EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
			<ServerID>{registrationKey.ServerCode}</ServerID>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <BranchAddress>
      <AddressType>OFC</AddressType>
      <Address1>10 HUTCHESON STREET</Address1>
      <Address2>ALBION  QLD</Address2>
      <AddressOverride>false</AddressOverride>
      <AddressShortCode>PST: 10 HUTCHESON STREET</AddressShortCode>
      <City>CITY</City>
      <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
      <Country>
        <Code>AU</Code>
        <Name>Australia</Name>
      </Country>
      <Email></Email>
      <Fax></Fax>
      <OrganizationCode>EDICUS</OrganizationCode>
      <Phone></Phone>
      <Port>
        <Code>AUBNE</Code>
        <Name>Brisbane</Name>
      </Port>
      <Postcode>4010</Postcode>
      <ScreeningStatus>
        <Code>UNK</Code>
        <Description>Unknown</Description>
      </ScreeningStatus>
      <State>QLD</State>
    </BranchAddress>
    <Category>STD</Category>
    <CheckDrawer></CheckDrawer>
    <CheckNumberOrPaymentRef></CheckNumberOrPaymentRef>
    <CreateTime>2019-06-05T08:15:00</CreateTime>
    <CreateUser>E</CreateUser>
    <Department>
      <Code>BRN</Code>
      <Name>Branch</Name>
    </Department>
    <Description>AP INVOICE</Description>
    <DrawerBank></DrawerBank>
    <DrawerBranch></DrawerBranch>
    <DueDate>2019-06-05T18:15:00</DueDate>
    <ExchangeRate>1.000000</ExchangeRate>
    <InvoiceTerm>COD</InvoiceTerm>
    <InvoiceTermDays>0</InvoiceTermDays>
    <IsCancelled>false</IsCancelled>
    <IsCreatedByMatchingProcess>false</IsCreatedByMatchingProcess>
    <IsPrinted>false</IsPrinted>
    <Job>
      <Type>Job</Type>
    </Job>
    <JobInvoiceNumber>00001000</JobInvoiceNumber>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </LocalCurrency>
    <LocalExVATAmount>-971.0000</LocalExVATAmount>
    <LocalTotal>-971.0000</LocalTotal>
    <LocalVATAmount>0.0000</LocalVATAmount>
    <Number>ART123</Number>
    <NumberOfSupportingDocuments>1</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>OFC</AddressType>
      <Address1>PO BOX 10446</Address1>
      <Address2>ADELAIDE ST, BRISBANE  QLD</Address2>
      <AddressOverride>false</AddressOverride>
      <AddressShortCode>PST: PO BOX 10446</AddressShortCode>
      <City></City>
      <CompanyName>A.A.L. SHIPPING AGENCIES P/L</CompanyName>
      <Country>
        <Code>AU</Code>
        <Name>Australia</Name>
      </Country>
      <Email></Email>
      <Fax></Fax>
      <OrganizationCode>AALSHI</OrganizationCode>
      <Phone></Phone>
      <Port>
        <Code>AUBNE</Code>
        <Name>Brisbane</Name>
      </Port>
      <Postcode>4000</Postcode>
      <ScreeningStatus>
        <Code>UNK</Code>
        <Description>Unknown</Description>
      </ScreeningStatus>
      <State></State>
    </OrganizationAddress>
    <OSCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-971.0000</OSExGSTVATAmount>
    <OSGSTVATAmount>0.00</OSGSTVATAmount>
    <OSTotal>-971.0000</OSTotal>
    <OutstandingAmount>-971.0000</OutstandingAmount>
    <PlaceOfIssue>Brisbane</PlaceOfIssue>
    <PostDate>2019-06-05T18:15:00</PostDate>
    <ReceiptOrDirectDebitNumber></ReceiptOrDirectDebitNumber>
    <RequisitionDate>2019-06-05T18:15:00</RequisitionDate>
    <RequisitionStatus>NRM</RequisitionStatus>
    <TransactionDate>2019-06-05T18:15:00</TransactionDate>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>BNE</Code>
          <Name>BN - AUBNE</Name>
        </Branch>
        <Department>
          <Code>BRN</Code>
          <Name>Branch</Name>
        </Department>
        <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
        <GLAccount>
          <AccountCode>1210.20.10</AccountCode>
          <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
        </GLAccount>
        <GLPostDate>2019-06-05T18:15:00</GLPostDate>
        <IsFinalCharge>false</IsFinalCharge>
        <Job>
          <Type>Job</Type>
        </Job>
        <LocalAmount>-971.0000</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </LocalCurrency>
        <LocalGSTVATAmount>0.0000</LocalGSTVATAmount>
        <LocalTotalAmount>-971.0000</LocalTotalAmount>
        <Organization>
          <Type>Organization</Type>
          <Key>AALSHI</Key>
        </Organization>
        <OSAmount>-971.00</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </OSCurrency>
        <OSGSTVATAmount>0.00</OSGSTVATAmount>
        <OSTotalAmount>-971.0000</OSTotalAmount>
        <RevenueRecognitionType>IMM</RevenueRecognitionType>
        <Sequence>1</Sequence>
        <TransactionCategory>STD</TransactionCategory>
        <TransactionType>CST</TransactionType>

        <PostingJournalDetailCollection>
          <PostingJournalDetail>
            <CreditGLAccount>
              <AccountCode>8210.00.00</AccountCode>
              <Description>TRADE CREDITORS CONTROL</Description>
            </CreditGLAccount>
            <DebitGLAccount>
              <AccountCode>4900.00.00</AccountCode>
              <Description>RETAINED EARNINGS FROM PREVIOUS YR</Description>
            </DebitGLAccount>
            <PostingAmount>971.0000</PostingAmount>
            <PostingCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </PostingCurrency>
            <PostingDate>2019-06-05T18:15:00</PostingDate>
            <PostingPeriod>202001</PostingPeriod>
          </PostingJournalDetail>
          <PostingJournalDetail>
            <CreditGLAccount>
              <AccountCode>4900.00.00</AccountCode>
              <Description>RETAINED EARNINGS FROM PREVIOUS YR</Description>
            </CreditGLAccount>
            <DebitGLAccount>
              <AccountCode>1210.20.10</AccountCode>
              <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
            </DebitGLAccount>
            <PostingAmount>971.0000</PostingAmount>
            <PostingCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </PostingCurrency>
            <PostingDate>2019-06-05T18:15:00</PostingDate>
            <PostingPeriod>202001</PostingPeriod>
          </PostingJournalDetail>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>");
			#endregion

			var message = factory.New<EDIMessage>();

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
			message.EM_MessageText = inboundXml;

			factory.Save();

			var logger = new TestServiceLogger();
			using (eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = new UMIServiceTask { ServiceLogger = logger };
				serviceTask.RunTask();
			}

			var messageLoad = new BusinessObjectFactory().Load<IEDIMessage>(message.PK);
			AssertEquals("Message EM_Status: ", EDIMessageStatusList.Codes.ProcessedOK, messageLoad.EM_Status);

			var message2 = factory.New<EDIMessage>();

			message2.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;

			message2.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message2.EM_Status = XmlEDIMessage.Status.Queued;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
			message2.EM_MessageText = inboundXml;

			factory.Save();

			using (eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask2 = new UMIServiceTask { ServiceLogger = logger };
				serviceTask2.RunTask();
			}
			var edimessageLoad = new BusinessObjectFactory().Load<EDIMessage>(message2.PK);
			AssertContains("logger error message", $"EDI Message {edimessageLoad.EM_MessageNum} could not be imported due to duplicate Transaction Pending Allocation", logger.ToString());
			AssertEquals("Message EM_Status: ", EDIMessageStatusList.Codes.Discarded, edimessageLoad.EM_Status);
			var noteText = ((StmNoteCollection)edimessageLoad.Notes.GetAllNotes())[0].ST_NoteDataAsText;
			AssertContains("logger error message", $"EDI Message {edimessageLoad.EM_MessageNum} could not be imported due to duplicate Transaction Pending Allocation", noteText);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		[UseSnapshotProtection]
		public void TestOrderAfterFailure()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			factory.Save();

			var xus = CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalShipment, @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
	<DateCollection>
        <Date>
            <Type>Departure</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2017-12-31T23:30:00</Value>
        </Date>
        <Date>
            <Type>Arrival</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2018-02-04T23:30:00</Value>
        </Date>
    </DateCollection>
  </Shipment>
</UniversalShipment>");

			factory.Save();

			var xue = CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>Z06</EventType>
  </Event>
</UniversalEvent>");

			factory.Save();

			var assertionsInCorrectOrder = 0;

			void AssertXUSProcessedFirst(BusinessObjectFactory messageFactory)
			{
				var messagesBeingSaved = ((IBusinessObjectFactoryInternals)messageFactory).AllBusinessObjects.Where(b => b is EDIMessage);
				AssertEquals("Precondition: Expecting 1 message saved at a time in Universal Message Logging factory", 1, messagesBeingSaved.Count());

				var message = messagesBeingSaved.First() as EDIMessage;
				if (message.EM_Status == EDIMessageStatusList.Codes.ProcessedOK && message.EM_MessageNum == xus.EM_MessageNum && assertionsInCorrectOrder == 0)
				{
					// XUS should be processed first
					assertionsInCorrectOrder = 1;
				}

				if (message.EM_Status == EDIMessageStatusList.Codes.ProcessedOK && message.EM_MessageNum == xue.EM_MessageNum && assertionsInCorrectOrder == 1)
				{
					assertionsInCorrectOrder = 2;
				}
			}

			var saveCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "Universal Message Processing" && saveCount == 0)
				{
					//cause concurrency error on first XUS message
					saveCount++;
					var newFac = new BusinessObjectFactory() { NameForDebugging = "TestFactory" };
					var shipment = newFac.Load<IForwardingShipment>(bizo.PK);
					shipment.JS_E_ARV = ZDateTime.Now.AddDays(2);
					newFac.Save();
				}

				if (f.NameForDebugging == "Universal Message Logging")
				{
					AssertXUSProcessedFirst(f);
				}
			});

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			AssertEquals("Order must be maintained after concurrency error and retry", 2, assertionsInCorrectOrder);
			var xusMessage = new BusinessObjectFactory().Load<EDIMessage>(xus.PK);
			var notes = xusMessage.Notes.GetAllNotes();
			var note = notes.First(b => b is StmNote n && n.ST_NoteDataAsText.Contains("Save Aborted Due to Concurrency Check"));
			AssertEquals("1 note for failed attempt and success attempt", 2, notes.Count);
			AssertNotNull("Note should contain failure details", note);
		}

		[UseSnapshotProtection]
		public void TestConcurrencyErrorIsHandled_NoDuplicateDocuments()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";
			factory.Save();

			var universalEvent = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>{shipment.JS_UniqueConsignRef}</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>U2lubmUgRmlhbm5hIEbDoWlsLA0KYXTDoSBmYW9pIGdoZWFsbCBhZyDDiWlyaW5uLA0KQnXDrW9uIGTDoXIgc2x1YQ0KdGhhciB0b2lubiBkbyByw6FpbmlnIGNodWdhaW5uLA0KRmFvaSBtaMOzaWQgYmhlaXRoIHNhb3INClNlYW50w61yIMOhciBzaW5zZWFyIGZlYXN0YSwNCk7DrSBmaMOhZ2ZhciBmYW9pbiB0w61vcsOhbiBuw6EgZmFvaW4gdHLDoWlsbC4NCkFub2NodCBhIHRow6lhbSBzYSBiaGVhcm5hIGJoYW9pbCwNCkxlIGdlYW4gYXIgR2hhZWlsLCBjaHVuIGLDoWlzIG7DsyBzYW9pbCwNCkxlIGd1bm5hLXNjcsOpYWNoIGZhb2kgbMOhbWhhY2ggbmEgYnBpbMOpYXIsDQpTZW8gbGliaCBjYW5haWcgYW1ocsOhbiBuYSBiaEZpYW5uLg==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
      <AttachedDocument>
        <FileName>CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>U2lubmUgRmlhbm5hIEbDoWlsLA0KYXTDoSBmYW9pIGdoZWFsbCBhZyDDiWlyaW5uLA0KQnXDrW9uIGTDoXIgc2x1YQ0KdGhhciB0b2lubiBkbyByw6FpbmlnIGNodWdhaW5uLA0KRmFvaSBtaMOzaWQgYmhlaXRoIHNhb3INClNlYW50w61yIMOhciBzaW5zZWFyIGZlYXN0YSwNCk7DrSBmaMOhZ2ZhciBmYW9pbiB0w61vcsOhbiBuw6EgZmFvaW4gdHLDoWlsbC4NCkFub2NodCBhIHRow6lhbSBzYSBiaGVhcm5hIGJoYW9pbCwNCkxlIGdlYW4gYXIgR2hhZWlsLCBjaHVuIGLDoWlzIG7DsyBzYW9pbCwNCkxlIGd1bm5hLXNjcsOpYWNoIGZhb2kgbMOhbWhhY2ggbmEgYnBpbMOpYXIsDQpTZW8gbGliaCBjYW5haWcgYW1ocsOhbiBuYSBiaEZpYW5uLg==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
  </Shipment>
</UniversalShipment>";
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory, tryHandleConflictsAutomatically: true));

			var message = UMIServiceTaskTest.GetMessageRowWithValidMessageContent(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				XmlEDIMessage.Status.Queued,
				universalEvent);
			factory.Save();

			var serviceTask1 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			var firstSave = true;
			using (var manager1 = new UMIServiceTaskTest.ProcessingManager_ForConcurrencyTest(factory, serviceTask1.SupportedMessageSubtypes, GrEngineServiceSetting.Disabled,
			shouldConflict: thefactory =>
			{
				if (thefactory.NameForDebugging == "Universal Message Logging")
				{
					firstSave = !firstSave;
					return firstSave;
				}
				return false;
			}))
			{
				manager1.ExecuteBatch(CancellationToken.None);
			}

			var reloadBizo = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			var savedEDocs = ((IDocManagerSupport)reloadBizo).DocManagerInfo.Files.Cast<IeDoc>();
			AssertEquals(0, savedEDocs.Count());

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[UseSnapshotProtection]
		public void TestStorageMainExceptionHandling()
		{
			ErrorReporter.Clear();

			var factory = new BusinessObjectFactory();
			var bizo = factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			factory.Save();

			var xml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>DDI</EventType>
	<AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>BAD_FILE.txt</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
  </Event>
</UniversalEvent>";

			var messageId = 0;
			var message = EDIMessageTestFactory.New(factory);
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = xml;
			message.EM_MessageNum = Interlocked.Increment(ref messageId).ToString("D20");
			factory.Save();

			var shouldCreate = true;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(umpFactory =>
			{
				if (shouldCreate && umpFactory.NameForDebugging == "Universal Message Processing")
				{
					shouldCreate = false;

					var anotherFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(new BusinessObjectFactory() { RefreshEnabled = false });
					anotherFactory.RefreshEnabled = false;
					var storageMain = (BusinessObject)anotherFactory.New<IStorageMain>();
					storageMain.FillWithValidTestData();
					storageMain["SM_ParentFK"] = bizo.PK;
					anotherFactory.Save();
				}
			});

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			AssertNoExceptionThrown(serviceTask.RunTask);

			AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
			AssertEquals("Should add eDoc to shipment.", 1, (bizo as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);
		}

		[UseSnapshotProtection]
		public void TestNoDuplicateCreatedAfterError()
		{
			var testHelper = new UMIServiceTaskTest();

			var incomingMessage = testHelper.GetMessageRowWithValidMessageContentExposed();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			incomingMessage.EM_MessageNum = "00000001";
			incomingMessage.EM_MessageText =
			#region Message Test
 @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>

    <DataContext>
      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <WayBillNumber>001</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House WayBill</Description>
    </WayBillType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <PortOfLoading>
      <Code>NZCHC</Code>
      <Name>Christchurch</Name>
    </PortOfLoading>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>

  </Shipment>
</UniversalShipment>";
			#endregion // Message Text

			var messageWithError = testHelper.GetMessageRowWithValidMessageContentExposed();
			messageWithError.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			messageWithError.EM_MessageNum = "00000002";
			messageWithError.EM_MessageText =
			#region Message Test
 @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>

      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <WayBillNumber>002</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House WayBill</Description>
    </WayBillType>
    <TransportMode>
      <Code>Air</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <PortOfLoading>
      <Code>USCHI</Code>
      <Name>Chicago</Name>
    </PortOfLoading>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>

    <JobCosting>
      <ChargeLineCollection>
        <ChargeLine>
          <Branch>
            <Code>BNE</Code>
          </Branch>
          <Department>
            <Code>FIA</Code>
          </Department>
          <ChargeCode>
            <Code>FRT</Code>
          </ChargeCode>
          <CostOSAmount>100</CostOSAmount>
          <SellLocalAmount>100</SellLocalAmount>
          <ImportMetaData>
            <Instruction>INSERT</Instruction>
          </ImportMetaData>
        </ChargeLine>
      </ChargeLineCollection>
    </JobCosting>
  </Shipment>
</UniversalShipment>";
			#endregion // Message Text

			Registry.Business.eServices.eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(
				Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = incomingMessage.Factory.New<GlbStaff>();
			staff.GS_LoginName = @"betty.boop";
			staff.GS_Code = "B.B";
			staff.GS_EmailAddress = "betty.boop@cargowise.com";
			var postMastersGroup = incomingMessage.Factory.Load<GlbGroup>(Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(postMastersGroup);

			incomingMessage.Factory.Save();

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };

			using (CriticalValidationServiceTestOnlyExtensions.TemporaryForceCriticalValidationErrorInAnyFactory_ForTestOnly(CriticalValidationErrorType.WIPMustHaveDebtor_1))
			{
				serviceTask.RunTask();
			}

			Assert("critical validation exception was reported", ExceptionReporterTestListener.Instance.Count > 0);
			ExceptionReporterTestListener.Instance.Clear();

			var newFactory = new BusinessObjectFactory();
			var reloadedIncomingMessage = newFactory.Load<EDIMessage>(incomingMessage.PK);
			var reloadedMessageWithError = newFactory.Load<EDIMessage>(messageWithError.PK);

			AssertEquals("reloadedIncomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, reloadedIncomingMessage.EM_Status);
			AssertEquals("reloadedMessageWithError.EM_Status", EDIMessage.Status.Rejected, reloadedMessageWithError.EM_Status);
			AssertEquals("no duplicate shipments created", 1, newFactory.Load<IForwardingShipment>(new ZQuery()).Length);

			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestUniversalShipmentFailsWithZSaveException()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var testHelper = new UMIServiceTaskTest();
				var incomingMessage = testHelper.GetMessageRowWithValidMessageContentExposed();
				incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
				incomingMessage.EM_MessageNum = "12345678";

				var messageFactory = incomingMessage.Factory;

				var creditor = messageFactory.NewWithValidTestData<OrgHeader>();
				creditor.OH_IsCreditor = true;
				creditor.OH_Code = "WWS";
				var debtor = messageFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ACAINT"));
				debtor.OH_IsDebtor = true;

				var rate = messageFactory.New<RefAccTaxRate>();
				rate.ZAT_StartDate = ZDate.Today.AddDays(-100);
				rate.ZAT_EndDate = ZDate.Today.AddDays(100);
				rate.ZAT_ReferenceRateType = "STD";
				rate.ZAT_RN_NKCountry = "AU";

				var group = messageFactory.New<IGlbGroup>();
				group.GG_Code = "Test";
				group.GG_Desc = "Test";

				var staff = messageFactory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "test@test.com";

				var groupStaffLink = messageFactory.New<GlbGroupLink>();
				groupStaffLink.GK_GG = group.PK;
				groupStaffLink.GK_GS = staff.PK;

				DataRegistry.Instance.RawRegistry.NotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, group.PK.ToGuid());
				#region Message Text

				incomingMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
     <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australia, Dollars</Description>
    </GoodsValueCurrency>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australia, Dollars</Description>
    </InsuranceValueCurrency>
    <JobCosting>
      <AccrualNotRecognized>0.0000</AccrualNotRecognized>
      <AccrualRecognized>-124.0000</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>SYD</Code>
        <Name>EDIHQ</Name>
      </Branch>
      <Currency>
        <Code>AUD</Code>
        <Description>Australia, Dollars</Description>
      </Currency>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>-124.0000</TotalAccrual>
      <TotalCost>0.0000</TotalCost>
      <TotalJobProfit>0.0000</TotalJobProfit>
      <TotalRevenue>0.0000</TotalRevenue>
      <TotalWIP>124.0000</TotalWIP>
      <WIPNotRecognized>0.0000</WIPNotRecognized>
      <WIPRecognized>124.0000</WIPRecognized>

      <ChargeLineCollection>
        <ChargeLine>
          <Branch>
            <Code>SYD</Code>
            <Name>EDIHQ</Name>
          </Branch>
          <ChargeCode>
            <Code>CFSCUST</Code>
            <Description>CFS Customs Hold</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>CSH</Code>
            <Description>CFS Shipment</Description>
          </ChargeCodeGroup>
          <CostOSCurrency>
            <Code>AUD</Code>
            <Description>Australia, Dollars</Description>
          </CostOSCurrency>
          <Creditor>
            <Type>Organization</Type>
            <Key>WWS</Key>
          </Creditor>
          <Debtor>
            <Type>Organization</Type>
            <Key>ACAINT</Key>
          </Debtor>
          <Department>
            <Code>FIS</Code>
            <Name>Forwarding Import Sea</Name>
          </Department>
          <Description>CFS Customs Hold</Description>
					<ImportMetaData>
                <Instruction>UpdateAndInsertIfNotFound</Instruction>
                <MatchingCriteriaCollection>
                  <MatchingCriteria>
                    <FieldName>ChargeCode</FieldName>
                    <Value>CFSCUST</Value>
                  </MatchingCriteria>
                  <MatchingCriteria>
                    <FieldName>SellOSCurrency</FieldName>
                    <Value>AUD</Value>
                  </MatchingCriteria>
                </MatchingCriteriaCollection>
          </ImportMetaData>
        </ChargeLine>
      </ChargeLineCollection>
    </JobCosting>
    <NoCopyBills>3</NoCopyBills>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PortOfDestination>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>USCHI</Code>
      <Name>Chicago</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code>SWB</Code>
      <Description>Sea Waybill</Description>
    </ReleaseType>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0.0000</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber>AD1232138</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

				#endregion

				messageFactory.Save();

				using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
				{
					PersistentFactoryCacheManager.Instance.OnFactoryAdded = f =>
					{
						f.Saving += _ =>
						{
							if (f.NameForDebugging.StartsWith("Universal Message Processing"))
							{
								throw new ZSaveException(new ZDataException(new Exception(), ((INeedRow)incomingMessage).Row, Db.Connection), f);
							}
						};
					};
					int CountEmailsInDB() => Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {MailDBItemsSchema.Constants.SqlSchemaName}.{MailDBItemsSchema.Constants.TableName}");
					var initialEmailCount = CountEmailsInDB();

					var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
					serviceTask.RunTask();
					ErrorReporter.Clear();

					var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);

					var notes = ((StmNoteCollection)reloadedMessage.Notes.GetAllNotes());
					AssertEquals("Should have one note per retry", eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value, notes.Count);

					var logger = (TestServiceLogger)serviceTask.ServiceLogger;

					CombineAssertions(delegate
					{
						var loggerString = logger.ToString();
						AssertContains("logger", "Exception processing message", loggerString);

						var noteAsText = notes[0].ST_NoteDataAsText;
						AssertContains("noteAsText", "Exception of type", noteAsText);

						AssertEquals("reloadedMessage.EM_Status", EDIMessage.Status.Rejected, reloadedMessage.EM_Status);

						AssertEquals("Created email for rejected message", 1, Env.OutgoingMailManager.EmailsCreated.Count);
						AssertEquals("Email should be saved to the db", 1, CountEmailsInDB() - initialEmailCount);
					});
				}
			}
		}

		[UseSnapshotProtection]
		public void TestNumberFountainUniqueConstraintException()
		{
			var xml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
   <Shipment>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
			<Code>DEM</Code>
			<Country>
				<Code>AU</Code>
				<Name>Australia</Name>
			</Country>
			<Name>Demo Company</Name>
			</Company>
			<DataProvider>EDIDATDEM</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
		</DataContext>
	</Shipment>
</UniversalShipment>";

			var factory = new BusinessObjectFactory();

			var consol = factory.New<IForwardingConsol>();
			factory.Save();

			AssertEquals("Precondition", 1, factory.Load<IForwardingConsol>(new ZQuery()).Length);
			var message = CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalShipment, xml);

			var causeConcurrencyException = true;
			factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest((BusinessObjectFactory thefactory) =>
			{
				if (causeConcurrencyException && thefactory.NameForDebugging == "Universal Message Processing")
				{
					var currentConsol = thefactory.LoadTop1<IForwardingConsol>(new ZQuery());
					var newFactory = new BusinessObjectFactory();
					var newConsol = newFactory.New<IForwardingConsol>();
					newFactory.Save();
					currentConsol.JK_UniqueConsignRef = newConsol.JK_UniqueConsignRef;
					causeConcurrencyException = false;
				}
			});

			var logs = RunServiceTask();

			var reloadFactory = new BusinessObjectFactory();
			var consols = reloadFactory.Load<IForwardingConsol>(new ZQuery());
			message = reloadFactory.Load<EDIMessage>(message.PK);

			AssertContains("Cannot insert duplicate key row", logs);
			AssertEquals("DataNote Added", 2, message.DataImportLogNoteCount);
			AssertEquals("1 failed attempts + 1 success", 2, message.DataImportLogNoteCount);
			AssertEquals("New consol created during inbound xml", 2, consols.Length);
			AssertEquals("The message should be processed on retry", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		[UseSnapshotProtection]
		public void TestNumberFountainUniqueConstraintException_JS_UniqueConsignRef_QuotedBooking()
		{
			var xml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
   <Shipment>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingBooking</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
			<Code>DEM</Code>
			<Country>
				<Code>AU</Code>
				<Name>Australia</Name>
			</Country>
			<Name>Demo Company</Name>
			</Company>
			<DataProvider>EDIDATDEM</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
		</DataContext>
	</Shipment>
</UniversalShipment>";

			var factory = new BusinessObjectFactory();
			var shipment = factory.New<IForwardingShipment>();
			factory.Save();
			AssertEquals("S00001000", shipment.JS_UniqueConsignRef);

			var quickBooking1 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, factory);
			(quickBooking1.ForwardingShipment as IForwardingShipment).JS_UniqueConsignRef = "S00001001";

			var message = CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalShipment, xml);

			factory.Save();

			var logs = RunServiceTask();

			var reloadFactory = new BusinessObjectFactory();
			var shipments = reloadFactory.Load<IForwardingShipment>(new ZQuery());
			message = reloadFactory.Load<EDIMessage>(message.PK);

			AssertContains("Cannot insert duplicate key row", logs);
			AssertEquals("1 failed attempts + 1 success", 2, message.DataImportLogNoteCount);
			AssertEquals("Retry Count", (byte)2, message.EM_RetryCount);
			AssertEquals("New shipment created during inbound xml", 3, shipments.Length);
			AssertEquals("The message should be processed on retry", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		[UseSnapshotProtection]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportUniversalShipment_UniqueIndexViolation()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<IForwardingConsol>();
			var container1 = factory.New<IForwardingContainer>();
			var container2 = factory.New<IForwardingContainer>();
			container1.JC_JK = consol.PK;
			container2.JC_JK = consol.PK;
			container2.JC_ContainerJobID = "D00001001"; //cause constraint violation by bypassing number fountain

			var message = CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalShipment, File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Freight\Forwarding\Forwarding.DataTransfer.Test\Universal\Consol\TestFiles\UniversalShipment.xml"));
			factory.Save();

			AssertEquals("D00001001", container2.JC_ContainerJobID);
			AssertEquals("D00001000", container1.JC_ContainerJobID);

			var logs = RunServiceTask();

			var reloadFactory = new BusinessObjectFactory();
			message = reloadFactory.Load<EDIMessage>(message.PK);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var containers = factory.Load<IForwardingContainer>(new ZQuery());

			AssertEquals(3, containers.Length);
			AssertEquals("1 failed attempts + 1 success", 2, message.DataImportLogNoteCount);
			AssertEquals("Retry Count", (byte)2, message.EM_RetryCount);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "D00001000", "D00001001", "D00001002" },
				containers.Select(c => c.JC_ContainerJobID));
			AssertContains("Service Task Log", "Exception processing message", logs);
			AssertContains("Service Task Log", $"Successfully saved Consol C00001001 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment, 1 x CusEntryNumber.", logs);
		}

		[UseSnapshotProtection]
		public void TestImportUniversalShipment_Duplicate_JC_JK_JC_ContainerNum()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<IForwardingConsol>();
			var container = factory.New<IForwardingContainer>();
			container.JC_JK = consol.PK;
			container.JC_ContainerNum = "OOO00000001";
			factory.Save();

			var testHelper = new UMIServiceTaskTest();
			var incomingMessage = testHelper.GetMessageRowWithValidMessageContentExposed();
			incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			incomingMessage.EM_MessageNum = "12345678";

			incomingMessage.EM_MessageText = $@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""INSERT"">
    <DataContext>

      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
		  <Key>{consol.JK_UniqueConsignRef}</Key>
        </DataTarget>
      </DataTargetCollection>

	</DataContext>

    <ContainerCollection>
      <Container Action=""INSERT"">
        <ContainerNumber>{container.JC_ContainerNum}</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
      </Container>
    </ContainerCollection>
  </Shipment>
</UniversalShipment>";

			incomingMessage.Factory.Save();

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				PersistentFactoryCacheManager.Instance.OnFactoryAdded = f =>
				{
					f.Saving += _ =>
					{
						if (f.NameForDebugging.StartsWith("Universal Message Processing"))
						{
							var newContainer = f.New<IForwardingContainer>();
							newContainer.JC_JK = consol.PK;
							newContainer.JC_ContainerNum = container.JC_ContainerNum;
						}
					};
				};
				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();
				ErrorReporter.Clear();

				var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);
				var loggerString = ((TestServiceLogger)serviceTask.ServiceLogger).ToString();

				AssertContains("Adding container should fail",
					$"Error Saving Container {container.JC_ContainerNum}",
					loggerString);
			}
		}

		[UseSnapshotProtection]
		public void TestUMIDuplicateRowException()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			var workflowProvider = bizo as IWorkflowProvider;
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "I'm so popular";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			factory.Save();

			var shouldEdit = 3;

			void ConcurrentlyEditShipment(BusinessObjectFactory thefactory)
			{
				//this change will be rolled back when the processing throws as it happens inside a DelayedTransaction in the ProcessingManager
				if (shouldEdit > 0 && thefactory.NameForDebugging == "Universal Message Processing")
				{
					var l1 = thefactory.New<IProcessJobTriggerLink>();
					var l2 = thefactory.New<IProcessJobTriggerLink>();
					l1.P9L_ParentId = l2.P9L_ParentId = ZGuid.NewZGuid();
					l1.P9L_ParentTableCode = l2.P9L_ParentTableCode = "SHP";
					l1.P9L_P9T_TemplateTrigger = l2.P9L_P9T_TemplateTrigger = ZGuid.NewZGuid();
					shouldEdit--;
				}
			}

			var xml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>Z00</EventType>
  </Event>
</UniversalEvent>";

			var message = CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalEvent, xml);

			factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(ConcurrentlyEditShipment);

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadFactory = new BusinessObjectFactory();
			bizo = reloadFactory.Load<IForwardingShipment>(bizo.PK);
			message = reloadFactory.Load<EDIMessage>(message.PK);

			AssertEquals("Data from the incomming message was saved", 1, (bizo as BusinessObject).GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).Count());
			AssertEquals("3 failed attemps + 1 success", 4, message.DataImportLogNoteCount);
			AssertEquals("Retry Count", (byte)4, message.EM_RetryCount);
			AssertEquals("The message should be processed on retry", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		[UseSnapshotProtection]
		public void TestUMIRetriesMessagesWithTransientErrorsWithinTheSameRun()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			var workflowProvider = bizo as IWorkflowProvider;
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "I'm so popular";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			factory.Save();

			var shouldEdit = true;

			void ConcurrentlyEditShipment(BusinessObjectFactory thefactory)
			{
				//this change will be rolled back when the processing throws as it happens inside a DelayedTransaction in the ProcessingManager
				if (shouldEdit && thefactory.NameForDebugging == "Universal Message Processing")
				{
					var reloadedShipment = new BusinessObjectFactory() { RefreshEnabled = false }.Load<IForwardingShipment>(bizo.PK) as BusinessObject;
					var trigger1 = ((IWorkflowProvider)reloadedShipment).WorkflowItems.Triggers.First() as ProcessTask;
					trigger1.P9_GS_NKAssignedStaffMember = "ABC";
					reloadedShipment.Factory.Save();
					shouldEdit = false;
				}
			}

			var xml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>Z00</EventType>
  </Event>
</UniversalEvent>";

			var message = CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalEvent, xml);

			factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(ConcurrentlyEditShipment);

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadFactory = new BusinessObjectFactory();
			bizo = reloadFactory.Load<IForwardingShipment>(bizo.PK);
			message = reloadFactory.Load<EDIMessage>(message.PK);

			AssertEquals("Data from the incomming message was saved", 1, (bizo as BusinessObject).GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).Count());
			AssertEquals("Data Notes Added", 2, message.DataImportLogNoteCount);
			AssertEquals("Retry Count", (byte)2, message.EM_RetryCount);
			AssertEquals("The message should be processed on retry", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		[UseSnapshotProtection]
		public void TestUMIRetriesMessagesWithTransientErrorsWithinTheSameRun_DoesNotSucceedOnRetry()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			var workflowProvider = bizo as IWorkflowProvider;
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "I'm so popular";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			factory.Save();

			void ConcurrentlyEditShipment(BusinessObjectFactory thefactory)
			{
				//this change will be rolled back when the processing throws as it happens inside a DelayedTransaction in the ProcessingManager
				if (thefactory.NameForDebugging == "Universal Message Processing")
				{
					var reloadedShipment = new BusinessObjectFactory() { RefreshEnabled = false }.Load<IForwardingShipment>(bizo.PK) as BusinessObject;
					var trigger1 = ((IWorkflowProvider)reloadedShipment).WorkflowItems.Triggers.First() as ProcessTask;
					trigger1.P9_GS_NKAssignedStaffMember = "ABC";
					reloadedShipment.Factory.Save();
				}
			}

			var xml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>Z00</EventType>
  </Event>
</UniversalEvent>";

			var message = CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalEvent, xml);

			factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(ConcurrentlyEditShipment);

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var reloadFactory = new BusinessObjectFactory();
			bizo = reloadFactory.Load<IForwardingShipment>(bizo.PK);
			message = reloadFactory.Load<EDIMessage>(message.PK);
			AssertEquals("Notes Added", 10, message.DataImportLogNoteCount);
			AssertEquals("Retry Count", (byte)10, message.EM_RetryCount);
			AssertEquals("The message failed on retry and should be rejected", EDIMessageStatusList.Codes.Rejected, message.EM_Status);
			AssertEquals("Data from the incomming message was not saved as it failed", 0, (bizo as BusinessObject).GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).Count());

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[UseSnapshotProtection]
		public void TestUMIDoesNotErrorReportWhenRetryingMessageWithTransientErrors()
		{
			var xml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>Z00</EventType>
  </Event>
</UniversalEvent>";

			var factory = new BusinessObjectFactory();
			var bizo = factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			factory.Save();

			CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalEvent, xml);
			factory.Save();

			var shouldThrowError = true;
			var exceptionMessage = "Transaction (Process ID 1404) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.";

			BusinessObjectFactory.SetOnFactorySaveHookForTest((BusinessObjectFactory thefactory) =>
			{
				if ((thefactory.NameForDebugging == "Universal Message Processing") && shouldThrowError)
				{
					shouldThrowError = false;
					var sqlException = SqlExceptionBuilder.CreateSqlException(1205, byte.MaxValue, byte.MinValue, null, exceptionMessage, "", 1);
					throw new ZSaveException(new ZDataException(sqlException, null, Db.Connection), thefactory);
				}
			});

			var updatedShipmentWorkflow = (IWorkflowProvider)new BusinessObjectFactory().Load(ObjectFactory.GetType<IForwardingShipment>(), bizo.PK);

			AssertNoExceptionThrown(() => RunServiceTask());
			AssertCollectionContains("Added event", updatedShipmentWorkflow.Logs.GetAllLogs(), log => ((StmALog)log).Event.SE_Code == "Z00");
			AssertCollectionNotContains("No error report", ErrorReporter.ExceptionsThrown, ex => ex.Contains(exceptionMessage));
		}

		[UseSnapshotProtection]
		public void TestDiscardedMessageWithNoChangesToSave()
		{
			var factory = new BusinessObjectFactory();
			AssertEquals("Precondition: No EDIMessages", null, factory.LoadTop1<EDIMessage>(new ZQuery()));
			var requestXml = @"
<UniversalEvent>
<Event>
	<EventType>CCD</EventType>
	<EventTime>10-JUL-2010 18:00</EventTime>
	<EventReference>Dummy Description</EventReference>
	<DataProvider>Dummy</DataProvider>
	<ContextCollection>
		<Context>
			<Type>HBOLNumber</Type>
			<Value>20257654321</Value>
		</Context>
	</ContextCollection>
	<DataContext>
		<DataSource>
			<DataProvider>WTG Tracking &amp; Automation</DataProvider>
		</DataSource>
	</DataContext>
</Event>
</UniversalEvent>
";

			var declaration = factory.New<Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_TransportMode.Name] = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "BJOB1";
			declaration.JE_HouseBill = "20257654321";
			declaration.JE_SystemCreateTimeUtc = new ZDateTime(2010, 7, 10, 17, 55, 0);
			declaration.JE_MasterBill = "081-203212";

			CreateQueuedUniversalMessage(factory, EDIMessageSubTypeList.Codes.XmlUniversalEvent, requestXml);
			factory.Save();

			using (eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();
			}

			var responseMessge = new BusinessObjectFactory().LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals("Declaration is too old so message is discarded", EDIMessageStatusList.Codes.Discarded, responseMessge.EM_Status);
		}

		EDIMessage CreateQueuedUniversalMessage(BusinessObjectFactory factory, string messageSubType, string messageContent)
		{
			var message = EDIMessageTestFactory.New(factory);
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = messageSubType;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = messageContent;
			message.EM_MessageNum = 0.ToString("D20");
			return message;
		}
	}
}
