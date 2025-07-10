using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Management.ShipmentProcessing.Testing
{
	sealed class ShipmentProcessingTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Selection Branch

		public void TestInboundShipmentUniversalShipmentWithDataTargetInOtherBranch()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode].Include = true;
			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";

			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR2";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUMEL";

			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR3";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUMEL";

			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR4";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUMEL";

			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR5";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUMEL";

			Factory.SaveForTesting();

			IEDIMessage message;
			using (DisposableEnvironment.ForBranch("BR4"))
			{
				message = GetQueuedUniversalShipmentMessage(ShipmentUniversalShipmentWithDataTargetInOtherBranch);
			}
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BR4', Company '~CO' – derived from target branch element (UniversalShipment->Shipment->Branch).
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='FAT_TONY') from UniversalShipment.
Successfully saved Shipment SBR400001000 (House Bill='FAT_TONY').
".Trim(), message.GetLogNoteText());
			});
		}

		#region ShipmentUniversalShipmentWithDataTargetInOtherBranch

		const string ShipmentUniversalShipmentWithDataTargetInOtherBranch = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>~CO</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <Branch>
      <Code>BR4</Code>
    </Branch>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA DELIMA</VesselName>
    <VoyageFlightNo>822</VoyageFlightNo>
    <WayBillNumber>FAT_TONY</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

		#endregion

		public void TestCompanyHasOnlyOneActiveBranch_SelectTheOnlyActiveBranch()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			IEDIMessage message;
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				message = GetQueuedUniversalShipmentMessage(UniversalShipmentHasNoBranch);
			}
			var serviceTaskLog = new ServiceTaskLogForTesting();

			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch '~BR', Company '~CO' – the only one active branch of recipient company.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasTargetBranch_SelectTargetBranch()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasTargetBranch = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasTargetBranch.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasTargetBranch);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'TES', Company 'EDI' – derived from target branch element (UniversalShipment->Shipment->Branch).
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		public void TestUniversalShipmentHasRecipientOrganization_SelectBranchFromRecipientOrganization()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientOrganization = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientOrganization.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientOrganization);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – 'Recipient' OrganizationAddress matches branch's Organisation Proxy.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		public void TestUniversalShipmentHasRecipientOrganizationPort_SelectBranchFromRecipientOrganizationPortViaBranchHomePort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipmentHasRecipientOrganizationPort);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'SYD', Company 'EDI' – 'Recipient' OrganizationAddress Port Code 'AUSYD' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		public void TestUniversalShipmentHasRecipientOrganizationPort_SelectBranchFromRecipientOrganizationPortViaBranchAdditionalRelatedPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			var additionalBranch = Factory.NewWithValidTestData<GlbBranch>();
			additionalBranch.GB_Code = "~ZZ";
			additionalBranch.GB_GC = company.PK;

			var extraPort1 = Factory.NewWithValidTestData<GlbBranchExtraPorts>();
			extraPort1.GY_GB = branch.PK;
			extraPort1.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";
			var extraPort2 = Factory.NewWithValidTestData<GlbBranchExtraPorts>();
			extraPort2.GY_GB = branch.PK;
			extraPort2.GY_RL_NKAdditionalBranchRelatedPort = "AUSYD";

			Factory.SaveForTesting();

			IEDIMessage message;
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				message = GetQueuedUniversalShipmentMessage(UniversalShipmentHasRecipientOrganizationPort);
			}

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch '~BR', Company '~CO' – 'Recipient' OrganizationAddress Port Code 'AUSYD' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleSAGAndPortOfLoading_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleSAGAndPortOfLoading = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleSAGAndPortOfLoading.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleSAGAndPortOfLoading);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfLoading 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		public void TestUniversalShipmentHasRecipientRoleSAGAndPortOfOrigin_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			eAdaptorRegistry.Instance.UniversalXMLTimestampsInProcessingLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SimpleLogger.EnableTimesTimestampsForTest.Value = true;
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipmentHasRecipientRoleSAGAndPortOfOrigin);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfOrigin 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		public void TestUniversalXmlWithTimestampsInProcessingLogsEnabled()
		{
			eAdaptorRegistry.Instance.UniversalXMLTimestampsInProcessingLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SimpleLogger.EnableTimesTimestampsForTest.Value = true;
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipmentHasRecipientRoleSAGAndPortOfOrigin);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					var messageNote = message.GetLogNoteText().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

					var dateTimeRegex = new Regex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}.\d{3}");
					foreach (var line in messageNote)
					{
						AssertEquals($"Expecting datetime at start of message note logs: {line}", true, dateTimeRegex.IsMatch(line));
					}

					foreach (var log in serviceTaskLog.Logs)
					{
						AssertEquals($"Service task logs should not have timestamps: {log}", false, dateTimeRegex.IsMatch(log.ToString()));
					}
				});
			}
		}

		public void TestUniversalShipmentHasRecipientRoleBREAndPortOfLoading_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();
			var universalShipmentHasRecipientRoleBREAndPortOfLoading = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleBREAndPortOfLoading.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleBREAndPortOfLoading);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfLoading 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		public void TestUniversalShipmentHasRecipientRoleBREAndPortOfOrigin_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleBREAndPortOfOrigin = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleBREAndPortOfOrigin.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleBREAndPortOfOrigin);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfOrigin 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		public void TestUniversalShipmentHasRecipientRoleRAGAndPortOfDischarge_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleRAGAndPortOfDischarge = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleRAGAndPortOfDischarge.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleRAGAndPortOfDischarge);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}
			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDischarge 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleRAGAndPortOfDestination_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleRAGAndPortOfDestination = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleRAGAndPortOfDestination.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleRAGAndPortOfDestination);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}
			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDestination 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleBRIAndPortOfDischarge_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleBRIAndPortOfDischarge = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleBRIAndPortOfDischarge.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleBRIAndPortOfDischarge);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDischarge 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleBRIAndPortOfDestination_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleBRIAndPortOfDestination = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleBRIAndPortOfDestination.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleBRIAndPortOfDestination);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDestination 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleDCFAndPortOfLoading_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleDCFAndPortOfLoading = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleDCFAndPortOfLoading.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleDCFAndPortOfLoading);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfLoading 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleDCRAndPortOfLoading_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleDCRAndPortOfLoading = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleDCRAndPortOfLoading.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleDCRAndPortOfLoading);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfLoading 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleDCTAndPortOfLoading_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleDCTAndPortOfLoading = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleDCTAndPortOfLoading.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleDCTAndPortOfLoading);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfLoading 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleDCYAndPortOfLoading_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleDCYAndPortOfLoading = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleDCYAndPortOfLoading.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleDCYAndPortOfLoading);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfLoading 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleACFAndPortOfDischarge_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleACFAndPortOfDischarge = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleACFAndPortOfDischarge.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleACFAndPortOfDischarge);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDischarge 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleACRAndPortOfDischarge_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleACRAndPortOfDischarge = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleACRAndPortOfDischarge.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleACRAndPortOfDischarge);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDischarge 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleACTAndPortOfDischarge_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleACTAndPortOfDischarge = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleACTAndPortOfDischarge.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleACTAndPortOfDischarge);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDischarge 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleACYAndPortOfDischarge_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleACYAndPortOfDischarge = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleACYAndPortOfDischarge.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleACYAndPortOfDischarge);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDischarge 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleCNRAndPortOfOrigin_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleCNRAndPortOfOrigin = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleCNRAndPortOfOrigin.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleCNRAndPortOfOrigin);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfOrigin 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasRecipientRoleCNEAndPortOfDestination_SelectBranchFromPort()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var universalShipmentHasRecipientRoleCNEAndPortOfDestination = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleCNEAndPortOfDestination.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentHasRecipientRoleCNEAndPortOfDestination);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – PortOfDestination 'AUBNE' matches branch's Home/Additional Port.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasNoBranch_ContextShouldNotSwitchIfUnnecessary()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipmentHasNoBranch);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – defaulted to first active branch of recipient company.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentDataContextHasCompanyCode_SelectDataContextCompanyCode()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";

			Factory.SaveForTesting();

			var universalShipmentDataContextHasCompanyCode = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentDataContextHasCompanyCode.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentDataContextHasCompanyCode);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch '~BR', Company '~CO' – the only one active branch of recipient company.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentCompanyIsInactive_MessageRejected()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			Factory.SaveForTesting();

			company.GC_IsActive = false;
			Factory.SaveForTesting();

			IEDIMessage message;
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				message = GetQueuedUniversalShipmentMessage(UniversalShipmentHasNoBranch);
			}
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Message Rejected as Company '~CO' is inactive.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Error - Message Rejected as Company '~CO' is inactive.
Message Rejected.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentCompanyHasNoActiveBranch_MessageRejected()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;

			Factory.SaveForTesting();

			IEDIMessage message;
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				message = GetQueuedUniversalShipmentMessage(UniversalShipmentHasNoBranch);
			}
			branch.GB_IsActive = false;

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Message Rejected as Company '~CO' has no active branches.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Error - Message Rejected as Company '~CO' has no active branches.
Message Rejected.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalShipmentHasInactiveTargetBranch_MessageRejected()
		{
			EnableVerboseLogging();
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "~B1";
			branch1.GB_GC = company.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "~B2";
			branch2.GB_GC = company.PK;
			var inactiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			inactiveBranch.GB_Code = "~IR";
			inactiveBranch.GB_GC = company.PK;
			inactiveBranch.GB_IsActive = false;

			Factory.SaveForTesting();
			IEDIMessage message;
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var universalShipmentHasInactiveTargetBranch = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasInactiveTargetBranch.xml");
				message = GetQueuedUniversalShipmentMessage(universalShipmentHasInactiveTargetBranch);
			}

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Message Rejected as Branch '~IR' is inactive.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch '~IR', Company '~CO' – derived from target branch element (UniversalShipment->Shipment->Branch).
Error - Message Rejected as Branch '~IR' is inactive.
Message Rejected.
".Trim(), message.GetLogNoteText());
			});
		}

		static void EnableVerboseLogging()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		#endregion

		#region ShipmentXMLBadDates
		const string ShipmentXMLBadDates = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>BIG FAT FISH</GoodsDescription>
    <PortOfDestination>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>NZABY</Code>
      <Name>Albany</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.300</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>234.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WayBillNumber>TEST BAD DATE</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised>1788-01-26T00:00:00</DeliveryCartageAdvised>
      <DeliveryCartageCompleted>2188-01-26T00:00:00</DeliveryCartageCompleted>
      <EstimatedDelivery>2012-01-01T00:00:00</EstimatedDelivery>
    </LocalProcessing>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomBlaString</Key>
        <DataType>String</DataType>
        <Value>TEST</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>1788-01-26T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2188-01-26T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <OrganizationCode>BAROPT</OrganizationCode>
        <Address1>12 COOLIBAH DRIVE</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>PALM BEACH</City>
        <CompanyName>BARZ OPTICS</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4221</Postcode>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <City>MOSCOW</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode>113186</Postcode>
        <State>BE</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
		#endregion

		#region TestImportAction

		public void TestImportAction_Merge_CreatedNewShipment()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var usxml = GetShipmentWithImportAction(ImportAction.Merge);
				var message = GetQueuedUniversalShipmentMessage(usxml);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				void Assert()
				{
					AssertEquals("message failed because there was no shipment to link to", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertMultilineASCIIEquals("message import log",
	@"No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='HBL00001001') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='HBL00001001').", message.GetLogNoteText());
				}

				CombineAssertions(Assert);
			}
		}

		public void TestImportAction_Merge_UpdatedExistingShipment()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var shipment = (Forwarding.IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment.JS_UniqueConsignRef = "SXX001001";
				shipment.JS_GoodsDescription = "frozen spinach";

				Factory.SaveForTesting();

				var usxml = GetShipmentWithImportAction(ImportAction.Merge, shipment.JS_UniqueConsignRef);
				var message = GetQueuedUniversalShipmentMessage(usxml);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				void Assert()
				{
					AssertEquals("message failed because there was no shipment to link to", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertMultilineASCIIEquals("message import log",
	@"Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Updated Shipment SXX001001 (House Bill='HBL00001001') from UniversalShipment.
Successfully saved Shipment SXX001001 (House Bill='HBL00001001').", message.GetLogNoteText());

					if (shipment is BusinessObject bizObj)
					{
						bizObj.Reload();
					}
					else
					{
						Fail("Catastrophic failure! Forwarding Shipment got kicked out from the business objects club");
					}

					AssertEquals("Shipment was updated from UXml", "chocolate koalas", shipment.JS_GoodsDescription);
				}

				CombineAssertions(Assert);
			}
		}

		public void TestImportAction_Merge_Discard_UnmatchedKeyInMessage()
		{
			int GetCountOfShipmentsInDatabase() => Factory.BOFactory.GetDatabaseCount(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var shipmentsInDb = GetCountOfShipmentsInDatabase();

			var usxml = GetShipmentWithImportAction(ImportAction.Merge, "SXX001001");
			var message = GetQueuedUniversalShipmentMessage(usxml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			void Assert()
			{
				AssertEquals("message failed because there was no shipment to link to", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertEquals("no new shipments were created", shipmentsInDb, GetCountOfShipmentsInDatabase());
				AssertMultilineASCIIEquals("message import log",
@"Error - Match couldn't be found for ForwardingShipment with Key SXX001001
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.", message.GetLogNoteText());
			}

			CombineAssertions(Assert);
		}

		public void TestImportAction_Link_Discard_NoExistingShipment_NoKeyInMessage()
		{
			int GetCountOfShipmentsInDatabase() => Factory.BOFactory.GetDatabaseCount(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var shipmentsInDb = GetCountOfShipmentsInDatabase();

			var usxml = GetShipmentWithImportAction(ImportAction.LinkOnly);
			var message = GetQueuedUniversalShipmentMessage(usxml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			void Assert()
			{
				AssertEquals("message failed because there was no shipment to link to", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertEquals("no new shipments were created", shipmentsInDb, GetCountOfShipmentsInDatabase());
				AssertMultilineASCIIEquals("message import log",
@"Error - [*Unable to link because existing business object could not be found.*]
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.", message.GetLogNoteText());
			}

			CombineAssertions(Assert);
		}

		public void TestImportAction_Link_Discard_NoExistingShipment_UnmatchedKeyInMessage()
		{
			int GetCountOfShipmentsInDatabase() => Factory.BOFactory.GetDatabaseCount(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var shipmentsInDb = GetCountOfShipmentsInDatabase();

			var usxml = GetShipmentWithImportAction(ImportAction.LinkOnly, "SomeKey");
			var message = GetQueuedUniversalShipmentMessage(usxml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			void Assert()
			{
				AssertEquals("message failed because there was no shipment to link to", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertEquals("no new shipments were created", shipmentsInDb, GetCountOfShipmentsInDatabase());
				AssertMultilineASCIIEquals("message import log",
@"Error - Match couldn't be found for ForwardingShipment with Key SomeKey
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.", message.GetLogNoteText());
			}

			CombineAssertions(Assert);
		}

		public void TestImportAction_Link_LinkedToExistingShipment_UsingKey()
		{
			var shipment = (Forwarding.IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "SXX001001";
			shipment.JS_GoodsDescription = "frozen spinach";

			Factory.SaveForTesting();

			var usxml = GetShipmentWithImportAction(ImportAction.LinkOnly, shipment.JS_UniqueConsignRef);
			var message = GetQueuedUniversalShipmentMessage(usxml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			void Assert()
			{
				AssertEquals("message was linked", EDIMessageStatusList.Codes.Linked, message.EM_Status);
				AssertMultilineASCIIEquals("message import log",
@"Universal Shipment data was linked to Shipment SXX001001.
Successfully saved, but nothing was reported as being updated.",
message.GetLogNoteText());

				if (shipment is BusinessObject bizObj)
				{
					bizObj.Reload();
				}
				else
				{
					Fail("Catastrophic failure! Forwarding Shipment got kicked out from the business objects club");
				}

				AssertEquals("Shipment was not updated from UXml", "frozen spinach", shipment.JS_GoodsDescription);

				var dataLinkedLogQuery = new ZQuery(StmALogSchema.SL_Parent, shipment.PK);
				dataLinkedLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataLinkedCode);

				var dataLinkedEvent = Factory.Load<StmALog>(dataLinkedLogQuery).SingleOrDefault();

				AssertNotNull("DIL event log was found", dataLinkedEvent);
				AssertNotNull("message was attached to the DIL event log", dataLinkedEvent.RelatedEDIMessage);
			}

			CombineAssertions(Assert);
		}

		public void TestImportAction_Link_LinkedToExistingConsol_UsingBusinessRules()
		{
			var consol = (Forwarding.IForwardingConsol)Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol.JK_UniqueConsignRef = "C00001001";
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "057-00000001";
			consol.JK_BookingReference = "booking reference";

			Factory.SaveForTesting();

			var usxml = GetConsolWithImportAction(ImportAction.LinkOnly);
			var message = GetQueuedUniversalShipmentMessage(usxml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			void Assert()
			{
				AssertEquals("message was linked", EDIMessageStatusList.Codes.Linked, message.EM_Status);
				AssertMultilineASCIIEquals("message import log",
@"Universal Shipment data was linked to Consol C00001001 (Master Bill='05700000001').
Successfully saved, but nothing was reported as being updated.",
message.GetLogNoteText());

				if (consol is BusinessObject bizObj)
				{
					bizObj.Reload();
				}
				else
				{
					Fail("Catastrophic failure! Forwarding Consol got kicked out from the business objects club");
				}

				AssertEquals("Consol was not updated from UXml", "booking reference", consol.JK_BookingReference);

				var dataLinkedLogQuery = new ZQuery(StmALogSchema.SL_Parent, consol.PK);
				dataLinkedLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataLinkedCode);

				var dataLinkedEvent = Factory.Load<StmALog>(dataLinkedLogQuery).SingleOrDefault();

				AssertNotNull("DIL event log was found", dataLinkedEvent);
				AssertNotNull("message was attached to the DIL event log", dataLinkedEvent.RelatedEDIMessage);
			}

			CombineAssertions(Assert);
		}

		string GetShipmentWithImportAction(ImportAction? importAction, string key = null)
		{
			var actionElement = importAction.HasValue
				? $"<Action>{importAction}</Action>"
				: null;

			return $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      {actionElement}
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{key}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code></Code>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2021-04-08T16:24:07.52</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <ActualChargeable>200.000</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <CommunityTransitStatus>
      <Code></Code>
    </CommunityTransitStatus>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <DocumentedChargeable>200.000</DocumentedChargeable>
    <DocumentedVolume>1.000</DocumentedVolume>
    <DocumentedWeight>200.000</DocumentedWeight>
    <FreightRate>0.0000</FreightRate>
    <FreightRateCurrency>
      <Code></Code>
    </FreightRateCurrency>
    <GoodsDescription>chocolate koalas</GoodsDescription>
    <GoodsValue>0.0000</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <HBLAWBChargesDisplay>
      <Code>NON</Code>
      <Description>No Charges showing</Description>
    </HBLAWBChargesDisplay>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <HouseBillOfLadingType>
      <Code></Code>
    </HouseBillOfLadingType>
    <InsuranceValue>0.0000</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsCancelled>false</IsCancelled>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsHighRisk>false</IsHighRisk>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>
    <JobCosting>
      <AccrualNotRecognized>0</AccrualNotRecognized>
      <AccrualRecognized>0</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>SYD</Code>
        <Name>EDIHQ</Name>
      </Branch>
      <Currency>
        <Code>AUD</Code>
        <Description>Australian Dollar</Description>
      </Currency>
      <Department>
        <Code>FEA</Code>
        <Name>Forwarding Export Air</Name>
      </Department>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0</TotalAccrual>
      <TotalCost>0</TotalCost>
      <TotalJobProfit>0</TotalJobProfit>
      <TotalRevenue>0</TotalRevenue>
      <TotalWIP>0</TotalWIP>
      <WIPNotRecognized>0</WIPNotRecognized>
      <WIPRecognized>0</WIPRecognized>
    </JobCosting>
    <ManifestedChargeable>200.000</ManifestedChargeable>
    <ManifestedVolume>1.000</ManifestedVolume>
    <ManifestedWeight>200.000</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>1</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code>NZAKL</Code>
      <Name>Auckland</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code></Code>
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
    <TotalVolume>1.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>200.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber>HBL00001001</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

  </Shipment>
</UniversalShipment>";
		}

		string GetConsolWithImportAction(ImportAction importAction) => $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Action>{importAction}</Action>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key></Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code></Code>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2021-04-14T14:01:38.493</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <AgentsReference></AgentsReference>
    <AWBServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </AWBServiceLevel>
    <BookingConfirmationReference>booking reference updated!</BookingConfirmationReference>
    <CarrierCorrectedChargeable>0.000</CarrierCorrectedChargeable>
    <CarrierCorrectedVolume>0.000</CarrierCorrectedVolume>
    <CarrierCorrectedVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </CarrierCorrectedVolumeUnit>
    <CarrierCorrectedWeight>0.000</CarrierCorrectedWeight>
    <CarrierCorrectedWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </CarrierCorrectedWeightUnit>
    <ChargeableRate>0.0000</ChargeableRate>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <DocumentedChargeable>0</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>0</DocumentedWeight>
    <FreightRate>0.0000</FreightRate>
    <FreightRateCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </FreightRateCurrency>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsHazardous>false</IsHazardous>
    <IsNeutralMaster>false</IsNeutralMaster>
    <LloydsIMO></LloydsIMO>
    <ManifestedChargeable>0</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>0</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <PaymentMethod>
      <Code>PPD</Code>
      <Description>Prepaid</Description>
    </PaymentMethod>
    <PaidBy>
      <Code>BRK</Code>
      <Description>Broker</Description>
    </PaidBy>
    <PlaceOfDelivery>
      <Code>NZAKL</Code>
      <Name>Auckland</Name>
    </PlaceOfDelivery>
    <PlaceOfIssue>
      <Code></Code>
    </PlaceOfIssue>
    <PlaceOfReceipt>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PlaceOfReceipt>
    <PortFirstForeign>
      <Code></Code>
    </PortFirstForeign>
    <PortLastForeign>
      <Code></Code>
    </PortLastForeign>
    <PortOfDischarge>
      <Code>NZAKL</Code>
      <Name>Auckland</Name>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code></Code>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfLoading>
    <ReceivingForwarderHandlingType>
      <Code></Code>
    </ReceivingForwarderHandlingType>
    <ReleaseType>
      <Code></Code>
    </ReleaseType>
    <RequiresTemperatureControl>false</RequiresTemperatureControl>
    <ScreeningStatus>
      <Code>NOT</Code>
      <Description>Not Screened</Description>
    </ScreeningStatus>
    <SendingForwarderHandlingType>
      <Code></Code>
    </SendingForwarderHandlingType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TotalPreallocatedChargeable>0.000</TotalPreallocatedChargeable>
    <TotalPreallocatedVolume>0.000</TotalPreallocatedVolume>
    <TotalPreallocatedVolumeUnit>
      <Code></Code>
    </TotalPreallocatedVolumeUnit>
    <TotalPreallocatedWeight>0.000</TotalPreallocatedWeight>
    <TotalPreallocatedWeightUnit>
      <Code></Code>
    </TotalPreallocatedWeightUnit>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo>QF2</VoyageFlightNo>
    <WayBillNumber>057-00000001</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

  </Shipment>
</UniversalShipment>";

		#endregion

		public void TestImportBadDates()
		{
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			var message = GetQueuedUniversalShipmentMessage(ShipmentXMLBadDates);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='TEST BAD DATE') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='TEST BAD DATE').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - Line 56: <Shipment>.<LocalProcessing>.<DeliveryCartageAdvised> - Invalid value [1788-01-26T00:00:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Line 57: <Shipment>.<LocalProcessing>.<DeliveryCartageCompleted> - Invalid value [2188-01-26T00:00:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Line 73: <Shipment>.<DateCollection>.<Date>.<Value> - Invalid value [1788-01-26T00:00:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Line 74: Element <Shipment>.<DateCollection>.<Date> opened at line 70 was excluded as it was missing mandatory elements. Missing: Value.
Warning - Line 78: <Shipment>.<DateCollection>.<Date>.<Value> - Invalid value [2188-01-26T00:00:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Line 79: Element <Shipment>.<DateCollection>.<Date> opened at line 75 was excluded as it was missing mandatory elements. Missing: Value.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Added Shipment (House Bill='TEST BAD DATE') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='TEST BAD DATE').
".Trim(), message.GetLogNoteText());
			});

			var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
			AssertEquals("shipment.JS_HouseBill", "TEST BAD DATE", shipment.JS_HouseBill);
			AssertEquals("shipment.JS_E_DEP", ZDateTime.Empty, shipment.JS_E_DEP);
			AssertEquals("shipment.JS_E_ARV", ZDateTime.Empty, shipment.JS_E_ARV);
			var localCartage = Factory.LoadTop1(ObjectFactory.GetType<IJobDocsAndCartage>(), new ZQuery(JobDocsAndCartageSchema.JP_ParentID, shipment.PK));
			AssertEquals("JobDocsAndCartage.JP_DeliveryCartageAdvised", ZDateTime.Empty, localCartage[JobDocsAndCartageSchema.JP_DeliveryCartageAdvised]);
			AssertEquals("JobDocsAndCartage.JP_DeliveryCartageCompleted", ZDateTime.Empty, localCartage[JobDocsAndCartageSchema.JP_DeliveryCartageCompleted]);
			AssertEquals("JobDocsAndCartage.JP_EstimatedDelivery", new ZDateTime(2012, 1, 1), localCartage[JobDocsAndCartageSchema.JP_EstimatedDelivery]);

			//28 June 1491
		}

		#region ShipmentXMLWithoutJobHeader
		const string ShipmentXMLWithoutJobHeader = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>BIG FAT FISH</GoodsDescription>
    <PortOfDestination>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>NZABY</Code>
      <Name>Albany</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.300</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>234.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WayBillNumber>New Shipment</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomBlaString</Key>
        <DataType>String</DataType>
        <Value>TEST</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <OrganizationCode>BAROPT</OrganizationCode>
        <Address1>12 COOLIBAH DRIVE</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>PALM BEACH</City>
        <CompanyName>BARZ OPTICS</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4221</Postcode>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <City>MOSCOW</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode>113186</Postcode>
        <State>BE</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string ShipmentXMLWithCharge = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>

	<JobCosting>
	  <AccrualNotRecognized>0</AccrualNotRecognized>
	  <AccrualRecognized>0</AccrualRecognized>
	  <AgentRevenue>0</AgentRevenue>
	  <Branch>
		<Code>BNE</Code>
		<Name>BN - AUBNE</Name>
	  </Branch>
	  <Currency>
		<Code>NZD</Code>
		<Description>New Zealand, Dollars</Description>
	  </Currency>
	  <LocalClientRevenue>0</LocalClientRevenue>
	  <OperationsStaff>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </OperationsStaff>
	  <OtherDebtorRevenue>0</OtherDebtorRevenue>
	  <TotalAccrual>0</TotalAccrual>
	  <TotalCost>0</TotalCost>
	  <TotalJobProfit>0</TotalJobProfit>
	  <TotalRevenue>0</TotalRevenue>
	  <TotalWIP>0</TotalWIP>
	  <WIPNotRecognized>0</WIPNotRecognized>
	  <WIPRecognized>0</WIPRecognized>
	  <ChargeLineCollection>
		<ChargeLine>
		  <ChargeCode>
			<Code>BAF</Code>
		  </ChargeCode>
		  <ImportMetaData>
			<Instruction>Insert</Instruction>
			<MatchingCriteriaCollection>
			  <MatchingCriteria>
				<FieldName>ChargeCode</FieldName>
				<Value>BAF</Value>
			  </MatchingCriteria>
			</MatchingCriteriaCollection>
		  </ImportMetaData>
		  <SellOSAmount>00000001418</SellOSAmount>
		  <SellOSCurrency>
			<Code>NZD</Code>
		  </SellOSCurrency>
		</ChargeLine>
	  </ChargeLineCollection>
	</JobCosting>

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>BIG FAT FISH</GoodsDescription>
    <PortOfDestination>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>NZABY</Code>
      <Name>Albany</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.300</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>234.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WayBillNumber>New Shipment</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomBlaString</Key>
        <DataType>String</DataType>
        <Value>TEST</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <OrganizationCode>BAROPT</OrganizationCode>
        <Address1>12 COOLIBAH DRIVE</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>PALM BEACH</City>
        <CompanyName>BARZ OPTICS</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4221</Postcode>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <City>MOSCOW</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode>113186</Postcode>
        <State>BE</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string ShipmentXMLWithFailedException = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>

	<JobCosting>
	  <AccrualNotRecognized>0</AccrualNotRecognized>
	  <AccrualRecognized>0</AccrualRecognized>
	  <AgentRevenue>0</AgentRevenue>
	  <Branch>
		<Code>BNE</Code>
		<Name>BN - AUBNE</Name>
	  </Branch>
	  <Currency>
		<Code>NZD</Code>
		<Description>New Zealand, Dollars</Description>
	  </Currency>
	  <LocalClientRevenue>0</LocalClientRevenue>
	  <OperationsStaff>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </OperationsStaff>
	  <OtherDebtorRevenue>0</OtherDebtorRevenue>
	  <TotalAccrual>0</TotalAccrual>
	  <TotalCost>0</TotalCost>
	  <TotalJobProfit>0</TotalJobProfit>
	  <TotalRevenue>0</TotalRevenue>
	  <TotalWIP>0</TotalWIP>
	  <WIPNotRecognized>0</WIPNotRecognized>
	  <WIPRecognized>0</WIPRecognized>
	  <ChargeLineCollection>
		<ChargeLine>
		  <ChargeCode>
			<Code>BAF</Code>
		  </ChargeCode>
		  <ImportMetaData>
			<Instruction>Update</Instruction>
			<MatchingCriteriaCollection>
			  <MatchingCriteria>
				<FieldName>ChargeCode</FieldName>
				<Value>FRT</Value>
			  </MatchingCriteria>
			</MatchingCriteriaCollection>
		  </ImportMetaData>
		  <SellOSAmount>00000001418</SellOSAmount>
		  <SellOSCurrency>
			<Code>NZD</Code>
		  </SellOSCurrency>
		</ChargeLine>
	  </ChargeLineCollection>
	</JobCosting>

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>BIG FAT FISH</GoodsDescription>
    <PortOfDestination>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>NZABY</Code>
      <Name>Albany</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.300</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>234.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WayBillNumber>New Shipment</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomBlaString</Key>
        <DataType>String</DataType>
        <Value>TEST</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <OrganizationCode>BAROPT</OrganizationCode>
        <Address1>12 COOLIBAH DRIVE</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>PALM BEACH</City>
        <CompanyName>BARZ OPTICS</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4221</Postcode>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <City>MOSCOW</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode>113186</Postcode>
        <State>BE</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string ConsolXMLWithFailedException = @"<UniversalShipment Version = ""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
		  <Key>C00001000</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>NZ</Code>
		  <Name>New Zealand</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <EnterpriseID>HYE</EnterpriseID>
	  <ServerID>DAT</ServerID>
	  <CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>
	<Branch>
		<Code>BNE</Code>
		<Name>BN - AUBNE</Name>
	 </Branch>
    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA DELIMA</VesselName>
    <VoyageFlightNo>822</VoyageFlightNo>
    <WayBillNumber>FAT_TONY</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
	<ConsolCosts>
      <ConsolCostLineCollection>
        <ConsolCostLine>
          <ApportionmentMethod>CHG</ApportionmentMethod>
          <ApportionToSubShipments>true</ApportionToSubShipments>
          <ChargeCode>
            <Code>FRT</Code>
            <Description>INTERNATIONAL FREIGHT</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>FRT</Code>
            <Description>Freight</Description>
          </ChargeCodeGroup>
          <CostExchangeRate>1</CostExchangeRate>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>110</CostLocalAmount>
          <CostOSAmount>110</CostOSAmount>
          <CostOSCurrency>
            <Code>NZD</Code>
            <Description>New Zealand Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0</CostOSGSTVATAmount>
          <IncludeOnCollectInvoice>false</IncludeOnCollectInvoice>
          <PrepaidCollectFilter>ALL</PrepaidCollectFilter>
		  <ImportMetaData>
			<Instruction>Update</Instruction>
			<MatchingCriteriaCollection>
			  <MatchingCriteria>
				<FieldName>ChargeCode</FieldName>
				<Value>BAF</Value>
			  </MatchingCriteria>
			</MatchingCriteriaCollection>
		  </ImportMetaData>
        </ConsolCostLine>
      </ConsolCostLineCollection>
    </ConsolCosts>

    <SubShipmentCollection>
      <SubShipment>
		<DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
              <Key>S00001000</Key>
            </DataTarget>
          </DataTargetCollection>
		  <Company>
			<Code>EDI</Code>
			<Country>
			  <Code>NZ</Code>
			  <Name>New Zealand</Name>
			</Country>
			<Name>Eagle Datamation International</Name>
		  </Company>
		  <EnterpriseID>EDI</EnterpriseID>
		  <ServerID>DAT</ServerID>
		 <CodesMappedToTarget>true</CodesMappedToTarget>
        </DataContext>
        <ContainerMode>
          <Code>LCL</Code>
          <Description>Less Container Load</Description>
        </ContainerMode>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>USSFO</Code>
          <Name>San Francisco</Name>
        </PortOfOrigin>
        <ShipmentType>
          <Code>STD</Code>
          <Description>Standard House</Description>
        </ShipmentType>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>
		<JobCosting>
		  <AccrualNotRecognized>0</AccrualNotRecognized>
		  <AccrualRecognized>0</AccrualRecognized>
		  <AgentRevenue>0</AgentRevenue>
		  <Branch>
			<Code>BNE</Code>
			<Name>BN - AUBNE</Name>
		  </Branch>
		  <Currency>
			<Code>NZD</Code>
			<Description>New Zealand, Dollars</Description>
		  </Currency>
		  <LocalClientRevenue>0</LocalClientRevenue>
		  <OperationsStaff>
			<Code>E</Code>
			<Name>CargoWise Support</Name>
		  </OperationsStaff>
		  <OtherDebtorRevenue>0</OtherDebtorRevenue>
		  <TotalAccrual>0</TotalAccrual>
		  <TotalCost>0</TotalCost>
		  <TotalJobProfit>0</TotalJobProfit>
		  <TotalRevenue>0</TotalRevenue>
		  <TotalWIP>0</TotalWIP>
		  <WIPNotRecognized>0</WIPNotRecognized>
		  <WIPRecognized>0</WIPRecognized>
		  <ChargeLineCollection>
			<ChargeLine>
			  <ChargeCode>
				<Code>BAF</Code>
			  </ChargeCode>
			  <ImportMetaData>
				<Instruction>Insert</Instruction>
				<MatchingCriteriaCollection>
				  <MatchingCriteria>
					<FieldName>ChargeCode</FieldName>
					<Value>BAF</Value>
				  </MatchingCriteria>
				</MatchingCriteriaCollection>
			  </ImportMetaData>
			  <SellOSAmount>00000001418</SellOSAmount>
			  <SellOSCurrency>
				<Code>NZD</Code>
			  </SellOSCurrency>
			</ChargeLine>
		  </ChargeLineCollection>
		</JobCosting>
    </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
		#endregion

		public void TestImportShipmentWithNotFoundChargeError()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
				var message = GetQueuedUniversalShipmentMessage(ShipmentXMLWithoutJobHeader);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='NEW SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='NEW SHIPMENT').
".Trim(), serviceTaskLog.ToString());
				});

				var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertNotNull(shipment);

				message = GetQueuedUniversalShipmentMessage(ShipmentXMLWithFailedException);
				serviceTaskLog.ClearLogs();
				manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Whilst importing Charge Line: Job Number=S00001000 Charge Code=BAF Creditor=, Debtor= Cost OS Amount= Sell OS Amount=1418
Charge not found when updating Charge Line.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());
				});

				shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertNotNull(shipment);

				var loader = new JobHeader.Loader(shipment as IJobHeaderParent);
				using (var job = loader.TryCreateWithMutex())
				{
					AssertNotNull("Job can be created after failure processing", job);
				}
			}
		}

		public void TestImportShipmentWithCriticalValidationError()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
				var message = GetQueuedUniversalShipmentMessage(ShipmentXMLWithoutJobHeader);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='NEW SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='NEW SHIPMENT').
".Trim(), serviceTaskLog.ToString());
				});

				var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertNotNull(shipment);

				message = GetQueuedUniversalShipmentMessage(ShipmentXMLWithCharge);
				serviceTaskLog.ClearLogs();
				manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

				using (CriticalValidationServiceTestOnlyExtensions.TemporaryForceCriticalValidationErrorInAnyFactory_ForTestOnly(CriticalValidationErrorType.CannotSaveAfterError))
				{
					try
					{
						manager.Process(message);
					}
					catch (OnSavingCriticalCheckException)
					{
						ErrorReporter.Clear();
					}
				}

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='NEW SHIPMENT') from UniversalShipment.
".Trim(), serviceTaskLog.ToString());
				});

				shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertNotNull(shipment);

				var loader = new JobHeader.Loader(shipment as IJobHeaderParent);
				using (var job = loader.TryCreateWithMutex())
				{
					AssertNotNull("Job can be created after failure processing", job);
				}
			}
		}

		const string ShipmentXMLWithJobChargeOsCostAmountNotEqualLocalCostAmount_ShipmentNotSetCompanyDefaultCurrentCompany = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>

	<JobCosting>
	  <AccrualNotRecognized>0</AccrualNotRecognized>
	  <AccrualRecognized>0</AccrualRecognized>
	  <AgentRevenue>0</AgentRevenue>
	  <Branch>
		<Code>FOC</Code>
		<Name>Test Branch</Name>
	  </Branch>
	  <Currency>
		<Code>CNY</Code>
		<Description>Chinese, Dollars</Description>
	  </Currency>
	  <LocalClientRevenue>0</LocalClientRevenue>
	  <OperationsStaff>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </OperationsStaff>
	  <OtherDebtorRevenue>0</OtherDebtorRevenue>
	  <TotalAccrual>0</TotalAccrual>
	  <TotalCost>0</TotalCost>
	  <TotalJobProfit>0</TotalJobProfit>
	  <TotalRevenue>0</TotalRevenue>
	  <TotalWIP>0</TotalWIP>
	  <WIPNotRecognized>0</WIPNotRecognized>
	  <WIPRecognized>0</WIPRecognized>
	  <ChargeLineCollection>
		<ChargeLine>
		  <ChargeCode>
			<Code>BAF</Code>
		  </ChargeCode>
		  <ImportMetaData>
			<Instruction>Insert</Instruction>
			<MatchingCriteriaCollection>
			  <MatchingCriteria>
				<FieldName>ChargeCode</FieldName>
				<Value>BAF</Value>
			  </MatchingCriteria>
			</MatchingCriteriaCollection>
		  </ImportMetaData>
		  <CostOSAmount>150.00</CostOSAmount>
		  <CostLocalAmount>1000</CostLocalAmount>
		  <CostOSCurrency>
			<Code>CNY</Code>
		  </CostOSCurrency>
		</ChargeLine>
	  </ChargeLineCollection>
	</JobCosting>

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>BIG FAT FISH</GoodsDescription>
    <PortOfDestination>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>NZABY</Code>
      <Name>Albany</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.300</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>234.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WayBillNumber>New Shipment</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomBlaString</Key>
        <DataType>String</DataType>
        <Value>TEST</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <OrganizationCode>BAROPT</OrganizationCode>
        <Address1>12 COOLIBAH DRIVE</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>PALM BEACH</City>
        <CompanyName>BARZ OPTICS</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4221</Postcode>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <City>MOSCOW</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode>113186</Postcode>
        <State>BE</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string ShipmentXMLWithJobChargeOsCostAmountNotEqualLocalCostAmount_ShipmentHasSetCompany = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
      <Company>
		<Code>USA</Code>
		<Country>
		  <Code>US</Code>
		  <Name>America</Name>
		</Country>
		<Name>America</Name>
	  </Company>
    </DataContext>
	<Branch>
		<Code>LAX</Code>
		<Name>Los Angeles </Name>
	 </Branch>

	<JobCosting>
	  <AccrualNotRecognized>0</AccrualNotRecognized>
	  <AccrualRecognized>0</AccrualRecognized>
	  <AgentRevenue>0</AgentRevenue>
	  <Branch>
		<Code>FOC</Code>
		<Name>Test Branch</Name>
	  </Branch>
	  <Currency>
		<Code>CNY</Code>
		<Description>Chinese, Dollars</Description>
	  </Currency>
	  <LocalClientRevenue>0</LocalClientRevenue>
	  <OperationsStaff>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </OperationsStaff>
	  <OtherDebtorRevenue>0</OtherDebtorRevenue>
	  <TotalAccrual>0</TotalAccrual>
	  <TotalCost>0</TotalCost>
	  <TotalJobProfit>0</TotalJobProfit>
	  <TotalRevenue>0</TotalRevenue>
	  <TotalWIP>0</TotalWIP>
	  <WIPNotRecognized>0</WIPNotRecognized>
	  <WIPRecognized>0</WIPRecognized>
	  <ChargeLineCollection>
		<ChargeLine>
		  <ChargeCode>
			<Code>BAF</Code>
		  </ChargeCode>
		  <ImportMetaData>
			<Instruction>Insert</Instruction>
			<MatchingCriteriaCollection>
			  <MatchingCriteria>
				<FieldName>ChargeCode</FieldName>
				<Value>BAF</Value>
			  </MatchingCriteria>
			</MatchingCriteriaCollection>
		  </ImportMetaData>
		  <CostOSAmount>150.00</CostOSAmount>
		  <CostLocalAmount>1000</CostLocalAmount>
		  <CostOSCurrency>
			<Code>CNY</Code>
		  </CostOSCurrency>
		</ChargeLine>
	  </ChargeLineCollection>
	</JobCosting>

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>BIG FAT FISH</GoodsDescription>
    <PortOfDestination>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>NZABY</Code>
      <Name>Albany</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.300</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>234.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WayBillNumber>New Shipment</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomBlaString</Key>
        <DataType>String</DataType>
        <Value>TEST</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2017-02-23T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <OrganizationCode>BAROPT</OrganizationCode>
        <Address1>12 COOLIBAH DRIVE</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>PALM BEACH</City>
        <CompanyName>BARZ OPTICS</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4221</Postcode>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <City>MOSCOW</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode>113186</Postcode>
        <State>BE</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		[TestDate(2018, 8, 17)]
		public void TestImportShipmentWithCriticalValidationWithJobChargeOsCostAmountNotEqualLocalCostAmount_ShipmentNotSetCompanyDefaultCurrentCompany()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var differentCompany = Factory.New<GlbCompany>();
				differentCompany.GC_Code = "JCN";
				differentCompany.GC_RN_NKCountryCode = "CN";
				differentCompany.SetCurrency("CNY");
				differentCompany.GC_IsReciprocal = true;

				var differentBranch = differentCompany.Branches.AddNew();
				differentBranch.GB_Code = "FOC";
				differentBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, differentCompany.GC_RN_NKCountryCode)).Code;
				differentBranch.GB_BranchName = "Test Branch";

				Factory.SaveForTesting();

				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var service = ObjectFactory.Get<Enterprise.Integration.Accounting.IAccounting>();
				service.Registry.CreateWIPOrAccrualWhenNoInvoicesPosted_ForTestOnly.SetValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				var currencyCNY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
				CreateExchangeRate(currencyCNY, "BUY", 165.510000M, new DateTime(2018, 8, 1), new DateTime(2018, 8, 31));

				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);

				var message = GetQueuedUniversalShipmentMessage(ShipmentXMLWithJobChargeOsCostAmountNotEqualLocalCostAmount_ShipmentNotSetCompanyDefaultCurrentCompany);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

				try
				{
					manager.Process(message);
				}
				catch (Exception ex)
				{
					AssertMultilineASCIIEquals("Exception message", @"The branch 'FOC' in the <JobCosting> does not belong to the system company 'EDI' processing the XML import.
Please ensure that the correct <Company> data is specified in the <DataTargetCollection>.", ex.Message);
				}

				for (int index = 0; index < ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count; index++)
				{
					AssertNotContains("OS cost amount should be same as local cost amount when Local Currency is used.", ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance[index].Message);
				}
				ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[TestDate(2018, 8, 17)]
		public void TestImportShipmentWithCriticalValidationWithJobChargeOsCostAmountNotEqualLocalCostAmount_ShipmentHasSetCompany()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var differentCompany = Factory.New<GlbCompany>();
				differentCompany.GC_Code = "JCN";
				differentCompany.GC_RN_NKCountryCode = "CN";
				differentCompany.SetCurrency("CNY");
				differentCompany.GC_IsReciprocal = true;

				var differentBranch = differentCompany.Branches.AddNew();
				differentBranch.GB_Code = "FOC";
				differentBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, differentCompany.GC_RN_NKCountryCode)).Code;
				differentBranch.GB_BranchName = "Test Branch";

				var shipmentXMLCompany = Factory.New<GlbCompany>();
				shipmentXMLCompany.GC_Code = "USA";
				shipmentXMLCompany.GC_RN_NKCountryCode = "US";
				shipmentXMLCompany.SetCurrency("USD");
				shipmentXMLCompany.GC_IsReciprocal = true;

				var shipmentXMLBranch = shipmentXMLCompany.Branches.AddNew();
				shipmentXMLBranch.GB_Code = "LAX";
				shipmentXMLBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, shipmentXMLCompany.GC_RN_NKCountryCode)).Code;
				shipmentXMLBranch.GB_BranchName = "Test Branch LAX";
				Factory.SaveForTesting();

				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var service = ObjectFactory.Get<Enterprise.Integration.Accounting.IAccounting>();
				service.Registry.CreateWIPOrAccrualWhenNoInvoicesPosted_ForTestOnly.SetValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				var currencyCNY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
				CreateExchangeRate(currencyCNY, "BUY", 165.510000M, new DateTime(2018, 8, 1), new DateTime(2018, 8, 31));

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, shipmentXMLBranch.PK.ToGuid(), Factory.NewWithValidTestData<GlbDepartment>().PK.ToGuid()))
				{
					CreateExchangeRate(currencyCNY, "BUY", 10M, new DateTime(2018, 8, 1), new DateTime(2018, 8, 31));
				}
				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);

				var message = GetQueuedUniversalShipmentMessage(ShipmentXMLWithJobChargeOsCostAmountNotEqualLocalCostAmount_ShipmentHasSetCompany);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

				AssertExceptionThrown<Exception>(@"The branch 'FOC' in the <JobCosting> does not belong to the system company 'USA' processing the XML import.
Please ensure that the correct <Company> data is specified in the <DataTargetCollection>.", new AnonymousMethod(() =>
				{
					manager.Process(message);
				}));
				for (int index = 0; index < ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count; index++)
				{
					AssertNotContains("OS cost amount should be same as local cost amount when Local Currency is used.", ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance[index].Message);
				}
				ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void CreateExchangeRate(RefCurrency currency, string rateType, decimal rate, ZDateTime startDate, ZDateTime endDate)
		{
			var curencyInNewFactory = (new BusinessObjectFactory()).Load<RefCurrency>(currency.PK);
			RefExchangeRate exchangeRate = curencyInNewFactory.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = rateType;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_StartDate = new ZDateTime(startDate.Year, startDate.Month, startDate.Day);
			exchangeRate.RE_ExpiryDate = new ZDateTime(endDate.Year, endDate.Month, endDate.Day);
			curencyInNewFactory.Factory.Save();

			ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestImportConsolWithNotFoundChargeError()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);
				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);

				var message = GetQueuedUniversalShipmentMessage(ForwardingConsolUniversalShipment);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
				});

				var consol = Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), new ZQuery());
				AssertNotNull(consol);

				var shipment = Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertNotNull(shipment);

				message = GetQueuedUniversalShipmentMessage(ConsolXMLWithFailedException);
				serviceTaskLog.ClearLogs();
				manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", string.Format(@"
Updated Shipment S00001000 (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
ERROR - Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=, Cost OS Amount=110
Consol cost not found when updating consol cost Line.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
", consol.PK).Trim(), serviceTaskLog.ToString());
				});

				consol = Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), new ZQuery());
				AssertNotNull(consol);

				shipment = Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertNotNull(shipment);

				var shipmentLoader = new JobHeader.Loader(shipment as IJobHeaderParent);
				using (var job = shipmentLoader.TryCreateWithMutex())
				{
					AssertNotNull("Job can be created after failure processing", job);
				}
			}
		}

		#region Internal Universal XML Sending

		public void TestProcessShipmentDataObjectWithNoDataTarget()
		{
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var message = GetQueuedUniversalShipmentMessage(string.Empty);

			var sessionTracker = manager.Process(message, universalShipment);
			var attemptedImports = sessionTracker.ImportResults;
			AssertMultilineASCIIEquals("attemptedImports", "False|No Module used this Universal Shipment data.|NULL", attemptedImports.FormatAndOrderImportAttempts());
		}

		public void TestProcessShipmentDataObjectLogsDataObjectReadFailureExceptions()
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.WarehouseOrder, null);
			universalShipment.DataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = null,
				EventBranch = null,
				EventDepartment = null,
				EventType = null,
				EventUser = null,
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.FOR } },
				TriggerCount = 0,
				TriggerDate = ZDateTimeOffset.Empty,
				TriggerDescription = "",
				TriggerReference = "",
				TriggerType = TriggerType.Manual
			});
			universalShipment.DataContext.CodesMappedToTarget = true;

			var localClient = Factory.BOFactory.Load<IOrgHeader>(Env.CurrentCompany.OrganisationPK);
			universalShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ OrganizationCode = localClient.OH_Code, Address1 = localClient.Address1, Address2 = localClient.Address2, AddressType = "LocalClient" } });

			var message = GetQueuedUniversalShipmentMessage(string.Empty);

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var sessionTracker = manager.Process(message, universalShipment);
			var attemptedImports = sessionTracker.ImportResults;

			AssertMultilineASCIIEquals("attemptedImports", @"
False|No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'LocalClient':- Matched to 'EDICUS' by code, main address used.
Error - Could not save Local Client Organization - Must have an Origin, Destination and Transport Mode to be able to calculate a Department for a Job Costing record, and the Local Client is saved on the Job Costing record.
No changes were made due to the above errors. Please fix the errors and try again.|ForwardingShipment
-----<<<<NEXT>>>>-----
False|No Module used this Universal Shipment data.|NULL
				".Trim(), attemptedImports.FormatAndOrderImportAttempts());
		}

		public void TestProcessShipmentDataObjectWhenImportSuccessful()
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingBooking, null);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalShipmentMessage(string.Empty);
			var sessionTracker = manager.Process(message, universalShipment);
			var attemptedImports = sessionTracker.ImportResults;

			AssertMultilineASCIIEquals("attemptedImports", @"
True|No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
Added Consol from UniversalShipment.
Successfully saved Consol C00001000.|ForwardingConsol-C00001000
-----<<<<NEXT>>>>-----
True|No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).|ForwardingBooking-S00001000
".Trim(), attemptedImports.FormatAndOrderImportAttempts());

			var consol = (IStmALogParent)Factory.BOFactory.LoadTop1<Forwarding.IForwardingConsol>(new ZQuery());
			AssertEquals(1, consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DIM")).Length);

			var shipment = (IStmALogParent)Factory.BOFactory.LoadTop1<Forwarding.IForwardingShipment>(new ZQuery()); // Forwarding Booking has a shipment
			AssertNotNull(shipment);

			var pk = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 VB_PK FROM dbo.ViewQuotedBooking");
			var query = new ZQuery(StmALogSchema.SL_Parent, pk);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "DIM");
			AssertEquals(1, Factory.Load<StmALog>(query).Length);
		}

		public void TestProcessShipmentDataObjectWithSubshipmentWhenImportSuccessful()
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);

			var universalSubShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			universalSubShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			universalShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { universalSubShipment });

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalShipmentMessage(string.Empty);
			var sessionTracker = manager.Process(message, universalShipment);
			var attemptedImports = sessionTracker.ImportResults;
			AssertMultilineASCIIEquals("Should create Consol only as there is no subshipment collection", @"
True|No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment from UniversalShipment.
Added Consol from UniversalShipment.
Successfully saved Consol C00001000 with 1 x ForwardingShipment.|ForwardingConsol-C00001000
".Trim(), attemptedImports.FormatAndOrderImportAttempts());

			var shipment = (IStmALogParent)Factory.BOFactory.LoadTop1<Forwarding.IForwardingShipment>(new ZQuery());
			var consol = (IStmALogParent)Factory.BOFactory.LoadTop1<Forwarding.IForwardingConsol>(new ZQuery());

			AssertEquals(1, shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DIM")).Length);
			AssertEquals(1, consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DIM")).Length);
		}

		#endregion

		public void TestInboundUniversalShipmentWithDataTargetDoesNotCheckRecipientRoleCollectionForMappings()
		{
			EnableVerboseLogging();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithRandomDataTargetAndRecipientRoleCollection);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Cannot Import Order
No Client Address was provided.
No Warehouse was provided.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – defaulted to first active branch of recipient company.
No matching WhsOrder found, creating new WhsOrder.
Populating WhsOrder...
Error - Cannot Import Order
No Client Address was provided.
No Warehouse was provided.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), message.GetLogNoteText());
			});
		}

		#region ConsolUniversalShipmentWithRandomDataTargetAndRecipientRoleCollection

		const string ConsolUniversalShipmentWithRandomDataTargetAndRecipientRoleCollection = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
    <DataContext>
			<DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001286</Key>
        </DataSource>
      </DataSourceCollection>
      <DataTargetCollection>
        <DataTarget>
          <Type>WarehouseOrder</Type>
        </DataTarget>
      </DataTargetCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>FOR</Code>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA DELIMA</VesselName>
    <VoyageFlightNo>822</VoyageFlightNo>
    <WayBillNumber>FAT_TONY</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <SubShipmentCollection>
      <SubShipment>
        <ContainerMode>
          <Code>LCL</Code>
          <Description>Less Container Load</Description>
        </ContainerMode>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>USSFO</Code>
          <Name>San Francisco</Name>
        </PortOfOrigin>
        <ShipmentType>
          <Code>STD</Code>
          <Description>Standard House</Description>
        </ShipmentType>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		public void TestInboundUniversalConsolWithShipmentMatchShipmentUsingPartyIdAndReference()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OOLU";
			var orgAddress = orgHeader.Addresses.AddNewMainAddress();
			orgAddress.OA_City = "SINGAPORE";
			orgAddress.OA_Address1 = "79 ANSON ROAD";
			orgAddress.OA_Address2 = "#14-00";
			orgAddress.OA_Code = "SGSIN - 79ANSONROAD";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ConsolWithShipmentMatchShipmentUsingPartyIdAndReference);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='S89901890') from UniversalShipment.
Added Consol (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
			});

			message = GetQueuedUniversalShipmentMessage(ConsolWithShipmentMatchShipmentUsingPartyIdAndReference);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='S89901890') from UniversalShipment.
Updated Consol C00001000 (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				Assert("Matched to Consol C00001000 (Master Bill='FUL423189120') with a Reference/Party ID match score of 180", message.GetLogNoteText().Contains("Matched to Consol C00001000 (Master Bill='FUL423189120') with a Reference/Party ID match score of 180"));
				Assert("Matched to Shipment S00001000 (House Bill='S89901890') with a Reference/Party ID match score of 180", message.GetLogNoteText().Contains("Matched to Shipment S00001000 (House Bill='S89901890') with a Reference/Party ID match score of 180"));
			});
		}

		#region ConsolWithShipmentMatchShipmentUsingPartyIdAndReference

		const string ConsolWithShipmentMatchShipmentUsingPartyIdAndReference = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>DNZ</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>NZ Demo Company</Name>
      </Company>
      <DataProvider>HYEIKBDNZ</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>AKL</Code>
        <Name>AKL</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>DEX</Code>
        <Description>Data Export</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>IKB</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2014-10-31T10:19:08.4</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <AgentsReference></AgentsReference>
    <AWBServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </AWBServiceLevel>
    <BookingConfirmationReference></BookingConfirmationReference>
    <ChargeableRate>0.0000</ChargeableRate>
    <ContainerCount>1</ContainerCount>
    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <DocumentedChargeable>0</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>0</DocumentedWeight>
    <FreightRate>0.0000</FreightRate>
    <FreightRateCurrency>
      <Code>NZD</Code>
      <Description>New Zealand Dollar</Description>
    </FreightRateCurrency>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsNeutralMaster>false</IsNeutralMaster>
    <LloydsIMO>9159646</LloydsIMO>
    <ManifestedChargeable>0</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>0</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>144</OuterPacks>
    <PaymentMethod>
      <Code>PPD</Code>
      <Description>Prepaid</Description>
    </PaymentMethod>
    <PaidBy>
      <Code>BRK</Code>
      <Description>Broker</Description>
    </PaidBy>
    <PortFirstForeign>
      <Code></Code>
    </PortFirstForeign>
    <PortLastForeign>
      <Code></Code>
    </PortLastForeign>
    <PortOfDischarge>
      <Code>ZAJNB</Code>
      <Name>Johannesburg</Name>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code></Code>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfLoading>
    <ReleaseType>
      <Code></Code>
    </ReleaseType>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TotalPreallocatedChargeable>0.000</TotalPreallocatedChargeable>
    <TotalPreallocatedVolume>0.000</TotalPreallocatedVolume>
    <TotalPreallocatedVolumeUnit>
      <Code></Code>
    </TotalPreallocatedVolumeUnit>
    <TotalPreallocatedWeight>0.000</TotalPreallocatedWeight>
    <TotalPreallocatedWeightUnit>
      <Code></Code>
    </TotalPreallocatedWeightUnit>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Metres</Description>
    </TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA TERATAI 3</VesselName>
    <VoyageFlightNo>345</VoyageFlightNo>
    <WayBillNumber>FUL423189120</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <ContainerCollection Content=""Complete"">
      <Container>
        <AirVentFlow>0.0</AirVentFlow>
        <AirVentFlowRateUnit>
          <Code></Code>
        </AirVentFlowRateUnit>
        <ArrivalCartageAdvised></ArrivalCartageAdvised>
        <ArrivalCartageComplete></ArrivalCartageComplete>
        <ArrivalCartageDemurrageCharge>0.0000</ArrivalCartageDemurrageCharge>
        <ArrivalCartageDemurrageTime></ArrivalCartageDemurrageTime>
        <ArrivalCartageRef></ArrivalCartageRef>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ArrivalEstimatedDelivery></ArrivalEstimatedDelivery>
        <ArrivalPickupByRail>false</ArrivalPickupByRail>
        <ArrivalSlotDateTime></ArrivalSlotDateTime>
        <ArrivalSlotReference></ArrivalSlotReference>
        <Commodity>
          <Code></Code>
        </Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
        <ContainerDetentionDays>0</ContainerDetentionDays>
        <ContainerImportDORelease></ContainerImportDORelease>
        <ContainerNumber>OOCL0000006</ContainerNumber>
        <ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
        <ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
        <ContainerQuality>
          <Code></Code>
        </ContainerQuality>
        <ContainerStatus>
          <Code></Code>
        </ContainerStatus>
        <ContainerType>
          <Code>20GP</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DeliveryMode>CY/CY</DeliveryMode>
        <DeliverySequence>0</DeliverySequence>
        <DepartureCartageAdvised></DepartureCartageAdvised>
        <DepartureCartageComplete></DepartureCartageComplete>
        <DepartureCartageDemurrageCharge>0.0000</DepartureCartageDemurrageCharge>
        <DepartureCartageDemurrageTime></DepartureCartageDemurrageTime>
        <DepartureCartageRef></DepartureCartageRef>
        <DepartureDeliveryByRail>false</DepartureDeliveryByRail>
        <DepartureDockReceipt></DepartureDockReceipt>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DepartureSlotDateTime></DepartureSlotDateTime>
        <DepartureSlotReference></DepartureSlotReference>
        <DunnageWeight>0.000</DunnageWeight>
        <EmptyReadyForReturn></EmptyReadyForReturn>
        <EmptyRequired></EmptyRequired>
        <EmptyReturnedBy></EmptyReturnedBy>
        <EmptyReturnRef></EmptyReturnRef>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <FCL_LCL_AIR>
          <Code>FCL</Code>
          <Description>Full Container Load</Description>
        </FCL_LCL_AIR>
        <FCLAvailable></FCLAvailable>
        <FCLHeldInTransitStaging>false</FCLHeldInTransitStaging>
        <FCLOnBoardVessel></FCLOnBoardVessel>
        <FCLStorageArrivedUnderbond>false</FCLStorageArrivedUnderbond>
        <FCLStorageCharge>0.0000</FCLStorageCharge>
        <FCLStorageCommences></FCLStorageCommences>
        <FCLStorageDays>0</FCLStorageDays>
        <FCLStorageModuleOnlyMaster></FCLStorageModuleOnlyMaster>
        <FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
        <FCLUnloadFromVessel>2011-01-20T00:00:00</FCLUnloadFromVessel>
        <FCLWharfGateIn></FCLWharfGateIn>
        <FCLWharfGateOut></FCLWharfGateOut>
        <GoodsValue>0.0000</GoodsValue>
        <GoodsValueCurrency>
          <Code></Code>
        </GoodsValueCurrency>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>2280.000</GrossWeight>
        <HumidityPercent>0</HumidityPercent>
        <IsCFSRegistered>false</IsCFSRegistered>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsDamaged>false</IsDamaged>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <IsShipperOwned>false</IsShipperOwned>
        <LCLAvailable></LCLAvailable>
        <LCLStorageCommences></LCLStorageCommences>
        <LCLUnpack></LCLUnpack>
        <LengthUnit>
          <Code>FT</Code>
          <Description>Feet</Description>
        </LengthUnit>
        <Link>1</Link>
        <OverhangBack>0.000</OverhangBack>
        <OverhangFront>0</OverhangFront>
        <OverhangHeight>0</OverhangHeight>
        <OverhangLeft>0</OverhangLeft>
        <OverhangRight>0.000</OverhangRight>
        <OverrideFCLAvailableStorage>false</OverrideFCLAvailableStorage>
        <OverrideLCLAvailableStorage>false</OverrideLCLAvailableStorage>
        <PackDate></PackDate>
        <RefrigGeneratorID></RefrigGeneratorID>
        <ReleaseNum></ReleaseNum>
        <Seal>SEL2389</Seal>
        <SecondSeal></SecondSeal>
        <SetPointTemp>0.000</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <StowagePosition></StowagePosition>
        <TareWeight>2280.000</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <TotalHeight>8.500</TotalHeight>
        <TotalLength>20.000</TotalLength>
        <TotalWidth>8.000</TotalWidth>
        <TrainWagonNumber></TrainWagonNumber>
        <UnpackGang></UnpackGang>
        <UnpackShed></UnpackShed>
        <VolumeCapacity>0.000</VolumeCapacity>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Metres</Description>
        </VolumeUnit>
        <WeightCapacity>0.000</WeightCapacity>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
      </Container>
    </ContainerCollection>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomBlaBool</Key>
        <DataType>Boolean</DataType>
        <Value>false</Value>
      </CustomizedField>
      <CustomizedField>
        <Key>MegaDate</Key>
        <DataType>DateTime</DataType>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <Key>CustomBlaDec</Key>
        <DataType>Decimal</DataType>
        <Value>0</Value>
      </CustomizedField>
      <CustomizedField>
        <Key>CustomBlaInt</Key>
        <DataType>Integer</DataType>
        <Value>0</Value>
      </CustomizedField>
      <CustomizedField>
        <Key>CustomBlaStr</Key>
        <DataType>String</DataType>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <Key>dATEsFF</Key>
        <DataType>DateTime</DataType>
        <Value></Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>FirstForeignArrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>LastForeignDeparture</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>CutOffDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AddressShortCode>SGSIN - 79ANSONROAD</AddressShortCode>
        <OrganizationCode>OOLU</OrganizationCode>
        <Address1>79 ANSON ROAD</Address1>
        <Address2>#14-00</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SINGAPORE</City>
        <CompanyName></CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email>Ilya.KirsanovBelov@cargowise.com</Email>
        <Fax>4382966</Fax>
        <Phone>4383383</Phone>
        <Port>
          <Code>SGSIN</Code>
          <Name>Singapore</Name>
        </Port>
        <Postcode>079906</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>GCR</Code>
              <Description>Accounting and Corporate Regulatory</Description>
            </Type>
            <CountryOfIssue>
              <Code>SG</Code>
              <Name>Singapore</Name>
            </CountryOfIssue>
            <Value>197801878K</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AddressShortCode>PST: 1/19-21 BOURKE ROAD</AddressShortCode>
        <OrganizationCode>AUSTOR</OrganizationCode>
        <Address1>1/19-21 BOURKE ROAD</Address1>
        <Address2>ALEXANDRIA, NSW</Address2>
        <AddressOverride>false</AddressOverride>
        <City>GG</City>
        <CompanyName>AUSTORIENT FREIGHT SERVICES</CompanyName>
        <Contact>ROSS FEHLBERG</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>Ilya.KirsanovBelov@cargowise.com</Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2015</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>INT</Code>
              <Description>INTTRA Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>INTRACODE</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>

        <ActualChargeable>0.000</ActualChargeable>
        <AdditionalTerms></AdditionalTerms>
        <BookingConfirmationReference></BookingConfirmationReference>
        <CartageWaybillNumber></CartageWaybillNumber>
        <CFSReference></CFSReference>
        <ContainerCount>1</ContainerCount>
        <ContainerMode>
          <Code>FCL</Code>
          <Description>Full Container Load</Description>
        </ContainerMode>
        <DocumentedChargeable>0.000</DocumentedChargeable>
        <DocumentedVolume>0.000</DocumentedVolume>
        <DocumentedWeight>0.000</DocumentedWeight>
        <FreightRate>0.0000</FreightRate>
        <FreightRateCurrency>
          <Code></Code>
        </FreightRateCurrency>
        <GoodsDescription></GoodsDescription>
        <GoodsValue>0.0000</GoodsValue>
        <GoodsValueCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </GoodsValueCurrency>
        <HBLAWBChargesDisplay>
          <Code>SHW</Code>
          <Description>Show Collect Charges</Description>
        </HBLAWBChargesDisplay>
        <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
        <InsuranceValue>0.0000</InsuranceValue>
        <InsuranceValueCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </InsuranceValueCurrency>
        <InterimReceiptNumber></InterimReceiptNumber>
        <IsBooking>false</IsBooking>
        <IsCFSRegistered>false</IsCFSRegistered>
        <IsDirectBooking>false</IsDirectBooking>
        <IsForwardRegistered>true</IsForwardRegistered>
        <IsNeutralMaster>false</IsNeutralMaster>
        <IsShipping>false</IsShipping>
        <IsSplitShipment>false</IsSplitShipment>
        <LloydsIMO>9159646</LloydsIMO>
        <ManifestedChargeable>0.000</ManifestedChargeable>
        <ManifestedVolume>0.000</ManifestedVolume>
        <ManifestedWeight>0.000</ManifestedWeight>
        <NoCopyBills>1</NoCopyBills>
        <NoOriginalBills>0</NoOriginalBills>
        <OuterPacks>0</OuterPacks>
        <OuterPacksPackageType>
          <Code>PKG</Code>
          <Description>Package</Description>
        </OuterPacksPackageType>
        <PackingOrder>0</PackingOrder>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfDischarge>
          <Code>ZAJNB</Code>
          <Name>Johannesburg</Name>
        </PortOfDischarge>
        <PortOfFirstArrival>
          <Code></Code>
        </PortOfFirstArrival>
        <PortOfLoading>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfLoading>
        <PortOfOrigin>
          <Code>ZAJNB</Code>
          <Name>Johannesburg</Name>
        </PortOfOrigin>
        <ReleaseType>
          <Code>EBL</Code>
          <Description>Express Bill of Lading</Description>
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
          <Code>ASM</Code>
          <Description>Assembly Master</Description>
        </ShipmentType>
        <ShippedOnBoard>
          <Code>SHP</Code>
          <Description>Shipped</Description>
        </ShippedOnBoard>
        <ShipperCODAmount>0.0000</ShipperCODAmount>
        <ShipperCODPayMethod>
          <Code></Code>
        </ShipperCODPayMethod>
        <TotalNoOfPacks>144</TotalNoOfPacks>
        <TotalNoOfPacksPackageType>
          <Code>PKG</Code>
          <Description>Package</Description>
        </TotalNoOfPacksPackageType>
        <TotalVolume>0.000</TotalVolume>
        <TotalVolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Metres</Description>
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
        <VesselName>BUNGA TERATAI 3</VesselName>
        <VoyageFlightNo>345</VoyageFlightNo>
        <WarehouseLocation></WarehouseLocation>
        <WayBillNumber>S89901890</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <LocalProcessing>
          <ArrivalCartageRef></ArrivalCartageRef>
          <DeliveryCartageAdvised></DeliveryCartageAdvised>
          <DeliveryCartageCompleted></DeliveryCartageCompleted>
          <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
          <DeliveryLabourTime></DeliveryLabourTime>
          <DeliveryRequiredBy></DeliveryRequiredBy>
          <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
          <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
          <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
          <DemurrageOnPickupTime></DemurrageOnPickupTime>
          <EstimatedDelivery></EstimatedDelivery>
          <EstimatedPickup></EstimatedPickup>
          <ExportStatement>
            <Code></Code>
          </ExportStatement>
          <FCLAvailable></FCLAvailable>
          <FCLDeliveryEquipmentNeeded>
            <Code>LOF</Code>
            <Description>Drop Container - Premise supplies Lift</Description>
          </FCLDeliveryEquipmentNeeded>
          <FCLPickupEquipmentNeeded>
            <Code>TRL</Code>
            <Description>Drop Trailer</Description>
          </FCLPickupEquipmentNeeded>
          <FCLStorageCommences></FCLStorageCommences>
          <HasProhibitedPackaging>false</HasProhibitedPackaging>
          <InsuranceRequired>false</InsuranceRequired>
          <IsContingencyRelease>false</IsContingencyRelease>
          <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
          <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
          <LCLAvailable></LCLAvailable>
          <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
          <LCLStorageCommences></LCLStorageCommences>
          <PickupCartageAdvised></PickupCartageAdvised>
          <PickupCartageCompleted></PickupCartageCompleted>
          <PickupLabourCharge>0.0000</PickupLabourCharge>
          <PickupLabourTime></PickupLabourTime>
          <PickupRequiredBy></PickupRequiredBy>
          <PrintOptionForPackagesOnAWB>
            <Code>DEF</Code>
            <Description>Default (Dims, fallback to Vol)</Description>
          </PrintOptionForPackagesOnAWB>
        </LocalProcessing>

        <CustomizedFieldCollection>
          <CustomizedField>
            <Key>CustomBlaString</Key>
            <DataType>String</DataType>
            <Value></Value>
          </CustomizedField>
          <CustomizedField>
            <Key>Test1</Key>
            <DataType>String</DataType>
            <Value></Value>
          </CustomizedField>
          <CustomizedField>
            <Key>Test2</Key>
            <DataType>Integer</DataType>
            <Value>0</Value>
          </CustomizedField>
          <CustomizedField>
            <Key>Test3</Key>
            <DataType>Boolean</DataType>
            <Value>false</Value>
          </CustomizedField>
        </CustomizedFieldCollection>

        <DateCollection>
          <Date>
            <Type>BookingConfirmed</Type>
            <IsEstimate>false</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Received</Type>
            <IsEstimate>false</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Departure</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2013-04-19T00:00:00</Value>
          </Date>
          <Date>
            <Type>Arrival</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2013-04-25T07:51:00</Value>
          </Date>
          <Date>
            <Type>ShippedOnBoard</Type>
            <IsEstimate>false</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>BillIssued</Type>
            <IsEstimate>false</IsEstimate>
            <Value></Value>
          </Date>
        </DateCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
            <OrganizationCode>AASDRA</OrganizationCode>
            <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
            <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
            <AddressOverride>false</AddressOverride>
            <City>KOWLOON</City>
            <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
            <Country>
              <Code>HK</Code>
              <Name>Hong Kong</Name>
            </Country>
            <Email>Ilya.KirsanovBelov@cargowise.com</Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>HKHKG</Code>
              <Name>Hong Kong</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
            <OrganizationCode>AASDRA</OrganizationCode>
            <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
            <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
            <AddressOverride>false</AddressOverride>
            <City>KOWLOON</City>
            <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
            <Country>
              <Code>HK</Code>
              <Name>Hong Kong</Name>
            </Country>
            <Email>Ilya.KirsanovBelov@cargowise.com</Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>HKHKG</Code>
              <Name>Hong Kong</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
            <OrganizationCode>ABABEU</OrganizationCode>
            <Address1>DIESLSTR 11</Address1>
            <Address2>57439 ATTENDORN, GERMANY</Address2>
            <AddressOverride>false</AddressOverride>
            <City>MOSCOW</City>
            <CompanyName>ABA BEUL</CompanyName>
            <Country>
              <Code>DE</Code>
              <Name>Germany</Name>
            </Country>
            <Email>Ilya.KirsanovBelov@cargowise.com</Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>DEFRA</Code>
              <Name>Frankfurt am Main</Name>
            </Port>
            <Postcode>113186</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>BE</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AddressShortCode>Pick Up Address</AddressShortCode>
            <OrganizationCode>ABABEU</OrganizationCode>
            <Address1>DIESLSTR 11</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>ATTENDORN?, GERMANY</City>
            <CompanyName>ABA BEUL</CompanyName>
            <Country>
              <Code>DE</Code>
              <Name>Germany</Name>
            </Country>
            <Email>Ilya.KirsanovBelov@cargowise.com</Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code></Code>
            </Port>
            <Postcode>57439</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>BE</State>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <Commodity>
              <Code>GEN</Code>
              <Description>General</Description>
            </Commodity>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>OOCL0000006</ContainerNumber>
            <ContainerPackingOrder>1</ContainerPackingOrder>
            <CountryOfOrigin>
              <Code></Code>
            </CountryOfOrigin>
            <DetailedDescription></DetailedDescription>
            <EndItemNo>0</EndItemNo>
            <GoodsDescription></GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <Height>0.000</Height>
            <ItemNo>0</ItemNo>
            <Length>0.000</Length>
            <LengthUnit>
              <Code>M</Code>
              <Description>Metres</Description>
            </LengthUnit>
            <LinePrice>0.0000</LinePrice>
            <LoadingMeters>0.000</LoadingMeters>
            <MarksAndNos></MarksAndNos>
            <OutturnComment></OutturnComment>
            <OutturnDamagedQty>0</OutturnDamagedQty>
            <OutturnedHeight>0.000</OutturnedHeight>
            <OutturnedLength>0.000</OutturnedLength>
            <OutturnedVolume>0.000</OutturnedVolume>
            <OutturnedWeight>0.000</OutturnedWeight>
            <OutturnedWidth>0.000</OutturnedWidth>
            <OutturnPillagedQty>0</OutturnPillagedQty>
            <OutturnQty>0</OutturnQty>
            <PackQty>0</PackQty>
            <PackType>
              <Code>PKG</Code>
              <Description>Package</Description>
            </PackType>
            <ReferenceNumber></ReferenceNumber>
            <Volume>0.000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Metres</Description>
            </VolumeUnit>
            <Weight>0.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
            <Width>0.000</Width>

            <PackedItemCollection>
            </PackedItemCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge>
          <Code>ZACPT</Code>
          <Name>Cape Town</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualArrivalInPortOfLoading></ActualArrivalInPortOfLoading>
        <ActualDeparture></ActualDeparture>
        <ArrivalBerth></ArrivalBerth>
        <ArrivalReference></ArrivalReference>
        <Carrier>
          <AddressType>Carrier</AddressType>
          <AddressShortCode>***Address Not On File***</AddressShortCode>
          <OrganizationCode>OOLU</OrganizationCode>
          <Address1>***Address Not On File***</Address1>
          <Address2></Address2>
          <AddressOverride>false</AddressOverride>
          <City></City>
          <CompanyName></CompanyName>
          <Country>
            <Code>HK</Code>
            <Name>Hong Kong</Name>
          </Country>
          <Email></Email>
          <Fax></Fax>
          <Phone></Phone>
          <Port>
            <Code>HKWNI</Code>
            <Name>Wan Chai</Name>
          </Port>
          <Postcode></Postcode>
          <ScreeningStatus>
            <Code>UNK</Code>
            <Description>Unknown</Description>
          </ScreeningStatus>
          <State></State>

          <RegistrationNumberCollection>
            <RegistrationNumber>
              <Type>
                <Code>GCR</Code>
                <Description>Accounting and Corporate Regulatory</Description>
              </Type>
              <CountryOfIssue>
                <Code>SG</Code>
                <Name>Singapore</Name>
              </CountryOfIssue>
              <Value>197801878K</Value>
            </RegistrationNumber>
          </RegistrationNumberCollection>
        </Carrier>
        <CarrierBookingReference></CarrierBookingReference>
        <CarrierServiceLevel>
          <Code></Code>
        </CarrierServiceLevel>
        <DepartureBerth></DepartureBerth>
        <DepartureReference></DepartureReference>
        <DocumentCutOff></DocumentCutOff>
        <EstimatedArrival>2013-04-25T07:51:00</EstimatedArrival>
        <EstimatedArrivalInPortOfLoading></EstimatedArrivalInPortOfLoading>
        <EstimatedDeparture>2013-04-19T07:51:00</EstimatedDeparture>
        <FCLAvailability></FCLAvailability>
        <FCLCutOff></FCLCutOff>
        <FCLReceivalCommences></FCLReceivalCommences>
        <FCLStorage></FCLStorage>
        <HazzardCutOffDate></HazzardCutOffDate>
        <HazzardReceivalCommences></HazzardReceivalCommences>
        <IsCargoOnly>true</IsCargoOnly>
        <LCLAvailability></LCLAvailability>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LCLStorageDate></LCLStorageDate>
        <LegNotes></LegNotes>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>9159646</VesselLloydsIMO>
        <VesselName>BUNGA TERATAI 3</VesselName>
        <VGMCutOff></VGMCutOff>
        <VoyageFlightNo>345</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		public void TestInboundUniversalConsolWithShipmentMatchedShipmentByDataTargetKey()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithSubShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HNLTNBA1408819AA') from UniversalShipment.
Added Consol (Master Bill='EGLV143480143234') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='EGLV143480143234') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingShipmentStmNote, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
			});

			var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery(JobShipmentSchema.JS_HouseBill, "HNLTNBA1408819AA"));
			Db.Connection.ExecuteNonQuery(string.Format("DELETE dbo.JobConShipLink WHERE JN_JS = '{0}'", shipment.PK)); // I don't have access to JobConShipLink BO from Universal solution

			message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithSubShipmentAndDataTargetKey.Replace("KeyToReplacePlaceholder", shipment.JS_UniqueConsignRef));
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='HNLTNBA1408819AA') from UniversalShipment.
Updated Consol C00001000 (Master Bill='EGLV143480143234') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='EGLV143480143234') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingShipmentStmNote, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestInboundUniversalConsolWithShipmentMatchedShipmentByDataTargetKey_MatchNotFound()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithSubShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HNLTNBA1408819AA') from UniversalShipment.
Added Consol (Master Bill='EGLV143480143234') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='EGLV143480143234') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingShipmentStmNote, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
			});

			var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery(JobShipmentSchema.JS_HouseBill, "HNLTNBA1408819AA"));
			Db.Connection.ExecuteNonQuery(string.Format("DELETE dbo.JobConShipLink WHERE JN_JS = '{0}'", shipment.PK));  // I don't have access to JobConShipLink BO from Universal solution

			message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithSubShipmentAndDataTargetKey);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Match couldn't be found for ForwardingShipment with Key KeyToReplacePlaceholder
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
			});
		}

		#region ConsolUniversalShipmentWithSubShipment

		const string ConsolUniversalShipmentWithSubShipment = @"<UniversalShipment version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
      <Shipment>
        <DataContext>
          <EnterpriseID>TRA</EnterpriseID>
          <ServerID>TRA</ServerID>
          <Company>
            <Code>TST</Code>
          </Company>
          <DataProvider>TRATRATST</DataProvider>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <PortLastForeign>
          <Code>CNNGB</Code>
        </PortLastForeign>
        <PortOfDischarge>
          <Code>USTIW</Code>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>CNNGB</Code>
        </PortOfLoading>
        <VesselName>EVER SIGMA</VesselName>
        <VoyageFlightNo>0359E</VoyageFlightNo>
        <WayBillNumber>EGLV143480143234</WayBillNumber>
        <WayBillType>
          <Code>MWB</Code>
        </WayBillType>
        <ContainerCollection Content=""Complete"">
          <Container>
            <ContainerCount>1</ContainerCount>
            <ContainerNumber>FSCU7026310</ContainerNumber>
            <ContainerType>
              <Code>45HC</Code>
            </ContainerType>
            <DeliveryMode>CY/CY</DeliveryMode>
            <FCL_LCL_AIR>
              <Code>FCL</Code>
            </FCL_LCL_AIR>
            <GoodsWeight>7481.5</GoodsWeight>
            <GrossWeight>7481.5</GrossWeight>
            <Link>1</Link>
            <Seal>EMCBDH2053</Seal>
            <WeightUnit>
              <Code>KG</Code>
            </WeightUnit>
          </Container>
        </ContainerCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>SendingForwarderAddress</AddressType>
            <OrganizationCode>HONLANNGB</OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ReceivingForwarderAddress</AddressType>
            <OrganizationCode>TRATRASEA</OrganizationCode>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <SubShipmentCollection>
          <SubShipment>
            <DataContext>
              <DataTargetCollection>
                <DataTarget>
                  <Type>ForwardingShipment</Type>
                </DataTarget>
              </DataTargetCollection>
            </DataContext>
            <OuterPacks>1080</OuterPacks>
            <OuterPacksPackageType>
              <Code>CTN</Code>
            </OuterPacksPackageType>
            <PortOfOrigin>
              <Code>CNNGB</Code>
            </PortOfOrigin>
            <TotalVolume>73.82</TotalVolume>
            <TotalVolumeUnit>
              <Code>M3</Code>
            </TotalVolumeUnit>
            <TotalWeight>7481.5</TotalWeight>
            <TotalWeightUnit>
              <Code>KG</Code>
            </TotalWeightUnit>
            <WayBillNumber>HNLTNBA1408819AA</WayBillNumber>
            <WayBillType>
              <Code>HWB</Code>
            </WayBillType>
            <DateCollection>
              <Date>
                <Type>Departure</Type>
                <IsEstimate>true</IsEstimate>
                <Value>2014-08-31T00:00:00</Value>
              </Date>
            </DateCollection>
            <NoteCollection>
              <Note>
                <Description>Marks &amp; Numbers</Description>
                <IsCustomDescription>false</IsCustomDescription>
                <NoteText>SANTA'S FOREST INC.,
OAKVILLE,WA
PO#
ITEM#
DESC:
QTY:
P.O.E.:TACOMA,WA
MADE IN CHINA
C/NO.
</NoteText>
              </Note>
            </NoteCollection>
            <PackingLineCollection>
              <PackingLine>
                <Commodity>
                  <Code>GEN</Code>
                </Commodity>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>FSCU7026310</ContainerNumber>
                <ContainerPackingOrder>1</ContainerPackingOrder>
                <PackQty>1080</PackQty>
                <PackType>
                  <Code>CTN</Code>
                </PackType>
                <Volume>73.82</Volume>
                <VolumeUnit>
                  <Code>M3</Code>
                </VolumeUnit>
                <Weight>7481.5</Weight>
                <WeightUnit>
                  <Code>KG</Code>
                </WeightUnit>
              </PackingLine>
            </PackingLineCollection>
          </SubShipment>
        </SubShipmentCollection>
        <TransportLegCollection>
          <TransportLeg>
            <LegOrder>1</LegOrder>
            <LegType>Main</LegType>
            <TransportMode>Sea</TransportMode>
            <PortOfLoading>
              <Code>CNNGB</Code>
            </PortOfLoading>
            <PortOfDischarge>
              <Code>USTIW</Code>
            </PortOfDischarge>
            <EstimatedDeparture>2014-08-31T00:00:00</EstimatedDeparture>
            <EstimatedArrival>2014-09-13T00:00:00</EstimatedArrival>
            <VesselName>EVER SIGMA</VesselName>
            <VoyageFlightNo>0359E</VoyageFlightNo>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>";

		#endregion

		#region ConsolUniversalShipmentWithSubShipmentAndDataTargetKey

		const string ConsolUniversalShipmentWithSubShipmentAndDataTargetKey = @"<UniversalShipment version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
      <Shipment>
        <DataContext>
          <EnterpriseID>TRA</EnterpriseID>
          <ServerID>TRA</ServerID>
          <Company>
            <Code>TST</Code>
          </Company>
          <DataProvider>TRATRATST</DataProvider>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <PortLastForeign>
          <Code>CNNGB</Code>
        </PortLastForeign>
        <PortOfDischarge>
          <Code>USTIW</Code>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>CNNGB</Code>
        </PortOfLoading>
        <VesselName>EVER SIGMA</VesselName>
        <VoyageFlightNo>0359E</VoyageFlightNo>
        <WayBillNumber>EGLV143480143234</WayBillNumber>
        <WayBillType>
          <Code>MWB</Code>
        </WayBillType>
        <ContainerCollection Content=""Complete"">
          <Container>
            <ContainerCount>1</ContainerCount>
            <ContainerNumber>FSCU7026310</ContainerNumber>
            <ContainerType>
              <Code>45HC</Code>
            </ContainerType>
            <DeliveryMode>CY/CY</DeliveryMode>
            <FCL_LCL_AIR>
              <Code>FCL</Code>
            </FCL_LCL_AIR>
            <GoodsWeight>7481.5</GoodsWeight>
            <GrossWeight>7481.5</GrossWeight>
            <Link>1</Link>
            <Seal>EMCBDH2053</Seal>
            <WeightUnit>
              <Code>KG</Code>
            </WeightUnit>
          </Container>
        </ContainerCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>SendingForwarderAddress</AddressType>
            <OrganizationCode>HONLANNGB</OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ReceivingForwarderAddress</AddressType>
            <OrganizationCode>TRATRASEA</OrganizationCode>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <SubShipmentCollection>
          <SubShipment>
            <DataContext>
              <DataTargetCollection>
                <DataTarget>
                  <Type>ForwardingShipment</Type>
                  <Key>KeyToReplacePlaceholder</Key>
                </DataTarget>
              </DataTargetCollection>
            </DataContext>
            <OuterPacks>1080</OuterPacks>
            <OuterPacksPackageType>
              <Code>CTN</Code>
            </OuterPacksPackageType>
            <PortOfOrigin>
              <Code>CNNGB</Code>
            </PortOfOrigin>
            <TotalVolume>73.82</TotalVolume>
            <TotalVolumeUnit>
              <Code>M3</Code>
            </TotalVolumeUnit>
            <TotalWeight>7481.5</TotalWeight>
            <TotalWeightUnit>
              <Code>KG</Code>
            </TotalWeightUnit>
            <WayBillNumber>HNLTNBA1408819AA</WayBillNumber>
            <WayBillType>
              <Code>HWB</Code>
            </WayBillType>
            <DateCollection>
              <Date>
                <Type>Departure</Type>
                <IsEstimate>true</IsEstimate>
                <Value>2014-08-31T00:00:00</Value>
              </Date>
            </DateCollection>
            <NoteCollection>
              <Note>
                <Description>Marks &amp; Numbers</Description>
                <IsCustomDescription>false</IsCustomDescription>
                <NoteText>SANTA'S FOREST INC.,
OAKVILLE,WA
PO#
ITEM#
DESC:
QTY:
P.O.E.:TACOMA,WA
MADE IN CHINA
C/NO.
</NoteText>
              </Note>
            </NoteCollection>
            <PackingLineCollection>
              <PackingLine>
                <Commodity>
                  <Code>GEN</Code>
                </Commodity>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>FSCU7026310</ContainerNumber>
                <ContainerPackingOrder>1</ContainerPackingOrder>
                <PackQty>1080</PackQty>
                <PackType>
                  <Code>CTN</Code>
                </PackType>
                <Volume>73.82</Volume>
                <VolumeUnit>
                  <Code>M3</Code>
                </VolumeUnit>
                <Weight>7481.5</Weight>
                <WeightUnit>
                  <Code>KG</Code>
                </WeightUnit>
              </PackingLine>
            </PackingLineCollection>
          </SubShipment>
        </SubShipmentCollection>
        <TransportLegCollection>
          <TransportLeg>
            <LegOrder>1</LegOrder>
            <LegType>Main</LegType>
            <TransportMode>Sea</TransportMode>
            <PortOfLoading>
              <Code>CNNGB</Code>
            </PortOfLoading>
            <PortOfDischarge>
              <Code>USTIW</Code>
            </PortOfDischarge>
            <EstimatedDeparture>2014-08-31T00:00:00</EstimatedDeparture>
            <EstimatedArrival>2014-09-13T00:00:00</EstimatedArrival>
            <VesselName>EVER SIGMA</VesselName>
            <VoyageFlightNo>0359E</VoyageFlightNo>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>";

		#endregion

		public void TestInboundUniversalShipmentWithNoDataTargetChecksRecipientRoleCollectionForMappings()
		{
			EnableVerboseLogging();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithNoDataTargetAndRecipientRoleCollection);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Targeting Branch 'BNE', Company 'EDI' – defaulted to first active branch of recipient company.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		[TestDate(2019, 07, 03)]
		public void TestInboundUniversalShipment_BespokeStaff_FromEI_CodesMappedToTarget()
		{
			EnableVerboseLogging();
			var senderCode = "WORMWOOD";
			var staffCode = "MAT";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			Factory.SaveForTesting();
			// Populate the registry item so it will use our bespoke staff member
			var list = eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value;
			var pair = list.AddNew();
			pair.Code = senderCode;
			pair.DescriptionValue = staffCode;
			eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			// Init stuff needed for setting Env context
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			Factory.SaveForTesting();

			// Message matching registry item
			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithNoDataTargetAndRecipientRoleCollection_CodesMappedToTarget, createInterchange: true);
			message.Interchange.EI_From = senderCode;

			// Run the service task
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}

			var shipment = Factory.BOFactory.Load<Forwarding.IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_SystemCreateTimeUtc, ZDateTime.UtcNow)).Single();
			var messageText = FormattableString.Invariant($@"<UniversalEvent>
  <Event>
    <DataContext>
      <CodesMappedToTarget>true</CodesMappedToTarget>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{shipment.JS_UniqueConsignRef}</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventType>ARV</EventType>
    <EventTime>10-SEP-2016 18:00</EventTime>
    <EventReference>Hello PUP!</EventReference>
    <DataProvider>Pupping Dummy</DataProvider>

    <ContextCollection>
      <Context>
        <Type>ShippersReference</Type>
        <Value>SOMEREFERENCE</Value>
      </Context>
    </ContextCollection>

  </Event>
</UniversalEvent>");
			var message2 = DataMessageProcessorTest.GetQueuedUniversalEventMessage(Factory, messageText, createInterchange: true);
			message2.Interchange.EI_From = senderCode;

			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message2);
			}

			var consol = (IStmALogProvider)Factory.BOFactory.Load<Forwarding.IForwardingConsol>(new ZQuery(JobConsolSchema.JK_SystemCreateTimeUtc, ZDateTime.UtcNow)).Single();
			var log1 = ((IStmALogProvider)shipment).Logs.Find(f => f.SL_SE_NKEvent == "ADD").Single();
			var log2 = consol.Logs.Find(f => f.SL_SE_NKEvent == "ARV").Single();
			AssertEquals("The new shipments ADD log under the custom staff.", staffCode, log1.SL_GS_NKUser);
			AssertEquals("The new shipments ARV log under the custom staff.", staffCode, log2.SL_GS_NKUser);
		}

		[TestDate(2019, 07, 03)]
		public void TestInboundUniversalShipment_BespokeStaff_FromEI_FROM()
		{
			EnableVerboseLogging();
			var senderCode = "WORMWOOD";
			var staffCode = "MAT";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			Factory.SaveForTesting();
			// Populate the registry item so it will use our bespoke staff member
			var list = eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value;
			var pair = list.AddNew();
			pair.Code = senderCode;
			pair.DescriptionValue = staffCode;
			eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			// Init stuff needed for setting Env context
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			Factory.SaveForTesting();

			// Message matching registry item
			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithNoDataTargetAndRecipientRoleCollection, createInterchange: true);
			message.Interchange.EI_From = senderCode;

			// Run the service task
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				manager.Process(message);
			}
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var shipment = (IStmALogProvider)Factory.BOFactory.Load<Forwarding.IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_SystemCreateTimeUtc, ZDateTime.UtcNow)).Single();
			var log = shipment.Logs.Find(f => f.SL_SE_NKEvent == "ADD").Single();
			AssertEquals("The new shipments add log under the custom staff.", staffCode, log.SL_GS_NKUser);
		}

		[TestDate(2019, 07, 03)]
		public void TestInboundUniversalShipment_BespokeStaff_NoBranchSwitchRequired()
		{
			EnableVerboseLogging();
			var senderCode = "WORMWOOD";
			var staffCode = "MAT";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			Factory.SaveForTesting();
			// Populate the registry item so it will use our bespoke staff member
			var list = eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value;
			var pair = list.AddNew();
			pair.Code = senderCode;
			pair.DescriptionValue = staffCode;
			eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			// Message matching registry item
			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithNoDataTargetAndRecipientRoleCollection, createInterchange: true);
			message.Interchange.EI_From = senderCode;

			// Run the service task
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var shipment = (IStmALogProvider)Factory.BOFactory.Load<Forwarding.IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_SystemCreateTimeUtc, ZDateTime.UtcNow)).Single();
			var log = shipment.Logs.Find(f => f.SL_SE_NKEvent == "ADD").Single();
			AssertEquals("The new shipments add log under the custom staff.", staffCode, log.SL_GS_NKUser);
		}

		[TestDate(2019, 07, 03)]
		public void TestInboundUniversalShipment_BespokeStaff_DontLoadInactiveStaff()
		{
			EnableVerboseLogging();
			var senderCode = "WORMWOOD";
			var staffCode = "MAT";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			Factory.SaveForTesting();
			// Populate the registry item so it will use our bespoke staff member
			var list = eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value;
			var pair = list.AddNew();
			pair.Code = senderCode;
			pair.DescriptionValue = staffCode;
			eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			// Init stuff needed for setting Env context
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "~BR";
			branch.GB_GC = company.PK;
			Factory.SaveForTesting();
			staff.GS_IsActive = false;
			Factory.SaveForTesting();

			// Message matching registry item
			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithNoDataTargetAndRecipientRoleCollection, createInterchange: true);
			message.Interchange.EI_From = senderCode;

			// Run the service task
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			using (Env.Instance.SetTemporaryUserContext(User.InterchangeUserName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				manager.Process(message);
			}
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var shipment = (IStmALogProvider)Factory.BOFactory.Load<Forwarding.IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_SystemCreateTimeUtc, ZDateTime.UtcNow)).Single();
			var log = shipment.Logs.Find(f => f.SL_SE_NKEvent == "ADD").Single();
			AssertEquals("The new shipments add log under the custom staff.", "~AD", log.SL_GS_NKUser);
		}

		#region ConsolUniversalShipmentWithNoDataTargetAndRecipientRoleCollection

		const string ConsolUniversalShipmentWithNoDataTargetAndRecipientRoleCollection_CodesMappedToTarget = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
    <DataContext>
      <Workflow>
         <CodesMappedToTarget>true</CodesMappedToTarget>
      </Workflow>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001286</Key>
        </DataSource>
      </DataSourceCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>FOR</Code>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA DELIMA</VesselName>
    <VoyageFlightNo>822</VoyageFlightNo>
    <WayBillNumber>FAT_TONY</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <SubShipmentCollection>
      <SubShipment>
        <ContainerMode>
          <Code>LCL</Code>
          <Description>Less Container Load</Description>
        </ContainerMode>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>USSFO</Code>
          <Name>San Francisco</Name>
        </PortOfOrigin>
        <ShipmentType>
          <Code>STD</Code>
          <Description>Standard House</Description>
        </ShipmentType>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		const string ConsolUniversalShipmentWithNoDataTargetAndRecipientRoleCollection = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001286</Key>
        </DataSource>
      </DataSourceCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>FOR</Code>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA DELIMA</VesselName>
    <VoyageFlightNo>822</VoyageFlightNo>
    <WayBillNumber>FAT_TONY</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <SubShipmentCollection>
      <SubShipment>
        <ContainerMode>
          <Code>LCL</Code>
          <Description>Less Container Load</Description>
        </ContainerMode>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>USSFO</Code>
          <Name>San Francisco</Name>
        </PortOfOrigin>
        <ShipmentType>
          <Code>STD</Code>
          <Description>Standard House</Description>
        </ShipmentType>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		public void TestInboundUniversalShipmentTellsUsWhenMatchingOccursByJobNumberIfVerboseLoggingIsEnabled()
		{
			EnableVerboseLogging();

			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001286";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_MasterBillNum] = "FILLET-O-FISH";
			consol[JobConsolSchema.JK_BookingReference] = "";

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001278";
			shipment[JobShipmentSchema.JS_HouseBill] = "JIMMY_THE_SNITCH";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ConsolUniversalShipmentWithConsolNumberDataTarget);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001278 (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Updated Consol C00001286 (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001286 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Found matching ForwardingConsol using the DataTarget Key C00001286.
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Found match using Combination Key Match. 1 possible match found.
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Updated Shipment S00001278 (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Updated Consol C00001286 (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001286 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		#region ConsolUniversalShipmentWithConsolNumberDataTarget

		const string ConsolUniversalShipmentWithConsolNumberDataTarget = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>C00001286</Key>
        </DataTarget>
      </DataTargetCollection>

			<CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA DELIMA</VesselName>
    <VoyageFlightNo>822</VoyageFlightNo>
    <WayBillNumber>FAT_TONY</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <SubShipmentCollection>
      <SubShipment>
        <ContainerMode>
          <Code>LCL</Code>
          <Description>Less Container Load</Description>
        </ContainerMode>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>USSFO</Code>
          <Name>San Francisco</Name>
        </PortOfOrigin>
        <ShipmentType>
          <Code>STD</Code>
          <Description>Standard House</Description>
        </ShipmentType>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		public void TestInboundUniversalShipmentWithNoDataContextShowsAppropriateMessage()
		{
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var universalShipmentNoDataContext = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.UniversalShipmentNoDataContext.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentNoDataContext);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
No Module used this Universal Shipment data.
Hint: Adding a DataContext element with an element in the DataTargetCollection will make the specified Module import where no existing data matches.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
No Module used this Universal Shipment data.
Hint: Adding a DataContext element with an element in the DataTargetCollection will make the specified Module import where no existing data matches.
Message Discarded."
				.Trim(), message.GetLogNoteText());
			});
		}

		public void TestInboundUniversalShipmentWithNoDataTargetShowsAppropriateMessage()
		{
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var message = GetQueuedUniversalShipmentMessage(ForwardingConsolUniversalShipment);
			message.EM_MessageText = message.EM_MessageText.Replace("<DataTarget>", "");
			message.EM_MessageText = message.EM_MessageText.Replace("<Type>ForwardingConsol</Type>", "");
			message.EM_MessageText = message.EM_MessageText.Replace("</DataTarget>", "");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
No Module used this Universal Shipment data.
Hint: Adding an element in the DataTargetCollection will make the specified Module import where no existing data matches.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
No Module used this Universal Shipment data.
Hint: Adding an element in the DataTargetCollection will make the specified Module import where no existing data matches.
Message Discarded."
				.Trim(), message.GetLogNoteText());
			});
		}

		public void TestInboundUniversalShipment()
		{
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var message = GetQueuedUniversalShipmentMessage(ForwardingConsolUniversalShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});

			var consol = Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), new ZQuery());
			var consolDataImportLog = AssertConsolContentsReturningLog(consol);

			var shipment = Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
			var shipmentDataImportLog = AssertShipmentContentsReturningLog(shipment);

			CombineAssertions(delegate
			{
				var consolPivots = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, consolDataImportLog.PK));
				AssertEquals("GenPivots attached to Consol 'Data Import' Log", 1, consolPivots.Length);
				var consolPivot = consolPivots[0];
				AssertEquals("consolPivot.XX_Relation1TableCode", StmALogSchema.Constants.Prefix, consolPivot.XX_Relation1TableCode);
				AssertEquals("consolPivot.XX_Relation1ID", consolDataImportLog.PK, consolPivot.XX_Relation1ID);
				AssertEquals("consolPivot.XX_Relation2TableCode", EDIMessageSchema.Constants.Prefix, consolPivot.XX_Relation2TableCode);
				AssertEquals("consolPivot.XX_Relation2ID", message.PK, consolPivot.XX_Relation2ID);
				AssertEquals("consolPivot.XX_RelationType", Constants.GenPivotTypes.XmlEdiMessage, consolPivot.XX_RelationType);
			});

			CombineAssertions(delegate
			{
				var shipmentPivots = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, shipmentDataImportLog.PK));
				AssertEquals("GenPivots attached to Shipment 'Data Import' Log", 1, shipmentPivots.Length);
				var shipmentPivot = shipmentPivots[0];
				AssertEquals("shipmentPivot.XX_Relation1TableCode", StmALogSchema.Constants.Prefix, shipmentPivot.XX_Relation1TableCode);
				AssertEquals("shipmentPivot.XX_Relation1ID", shipmentDataImportLog.PK, shipmentPivot.XX_Relation1ID);
				AssertEquals("shipmentPivot.XX_Relation2TableCode", EDIMessageSchema.Constants.Prefix, shipmentPivot.XX_Relation2TableCode);
				AssertEquals("shipmentPivot.XX_Relation2ID", message.PK, shipmentPivot.XX_Relation2ID);
				AssertEquals("shipmentPivot.XX_RelationType", Constants.GenPivotTypes.XmlEdiMessage, shipmentPivot.XX_RelationType);
			});
		}

		public void TestUniversalShipmentCorrectParsing()
		{
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var forwardingUniversalShipmentParsing = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.ForwardingUniversalShipmentParsing.xml");
			var message = GetQueuedUniversalShipmentMessage(forwardingUniversalShipmentParsing);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - Line 14: <Shipment>.<DataContext>.<Company> - Unrecognized element <FakeTag> found. Element and all its children skipped.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestInboundUniversalShipmentAddressNotUsedInForwardingShipment()
		{
			ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().LocalCountryCustomsInterfaceSubmissionType = "ITF";

			var registration = ObjectFactory.Get<IProductRegistration>();
			try
			{
				registration.KeyForTest.EnterpriseCodeForTest = "JSC";
				registration.KeyForTest.ServerCodeForTest = "BWI";
				var branch = Factory.New<GlbBranch>();
				branch.GB_Code = "BWI";
				branch.GB_GC = Env.CurrentCompany.PK;
				branch.GB_RL_NKHomePort = "AUSYD";
				var company = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
				company.GC_Code = "BWI";
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "KLMROY_WW";
				org = Factory.New<OrgHeader>();
				org.OH_Code = "TRABURACW";
				org = Factory.New<OrgHeader>();
				org.OH_Code = "ROYNETAMS";
				org = Factory.New<OrgHeader>();
				org.OH_Code = "JOHSCOBWI1";

				TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);
				var universalShipmentAddressNotUsedInForwardingShipment = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.UniversalShipmentAddressNotUsedInForwardingShipment.xml");
				var message = GetQueuedUniversalShipmentMessage(universalShipmentAddressNotUsedInForwardingShipment);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration (Master Bill='07436925851') from UniversalShipment.
Added Shipment (House Bill='07436925851') from UniversalShipment.
Added Consol (Master Bill='07436925851') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='07436925851') with 1 x Transport, 1 x ForwardingPackLine, 1 x OrderItem, 1 x Bill, 1 x GroupInvoiceCharge, 38 x InvoiceCharge, 1 x JobDeclaration, 1 x ShipmentExportAWBRateLine, 1 x ShipmentExportAWBHeader, 1 x CusEntryNumber, 1 x ForwardingShipment, 1 x ConsolExportAWBHeader.
".Trim(), serviceTaskLog.ToString());
				});
			}
			finally
			{
				registration.ResetKeyToDefault();
			}
		}

		public void TestDatesAreCulturalInvariant()
		{
			var initialCulture = CultureInfo.CurrentCulture;

			try
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("EN-AU");
				AssertDatesAreCulturalInvariant();
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("EN-US");
				AssertDatesAreCulturalInvariant();
			}
			finally
			{
				CultureInfo.CurrentCulture = initialCulture;
			}
		}

		void AssertDatesAreCulturalInvariant()
		{
			var inboundXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>{0}</GoodsDescription>
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
	<LocalProcessing>
        <EstimatedPickup>25/10/20</EstimatedPickup>
        <InsuranceRequired>false</InsuranceRequired>
        <OrderNumberCollection>
        <OrderNumber>
            <OrderReference>64123</OrderReference>
            <Sequence>1</Sequence>
        </OrderNumber>
        </OrderNumberCollection>
    </LocalProcessing>
    <WayBillNumber>ANGRY</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			var message = GetQueuedUniversalShipmentMessage(inboundXml);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertContains("The format we use is YYYY-MM-DDTHH:MM:SS, I am unaware if this format is officially supported however the MM/DD/YY has always worked (DateTimeOffset.TryParse) and there are clients using this when sending XML for import", "Invalid value [25/10/20]", manager.Logger.ToString());
		}

		public void TestImportCustomFieldsAreNotDuplicatedFromOrganizationAndTemplate()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "WHS";
			processTaskTemplate.P0_IsActive = true;

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "GFGFD";
			organization.OH_FullName = "SDSD";
			organization.OH_IsWarehouseClient = true;

			OrgCustomLabels orgCustomLabel1 = organization.CustomLabels.AddNew();
			orgCustomLabel1.OT_FieldName = "WhsDocket.CustomAttrib1";
			orgCustomLabel1.OT_Caption = "Test";

			var warehouse = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(WhsOrderXMLCreate);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Added Warehouse Order from UniversalShipment.
Successfully saved Warehouse Order W00000001.
".Trim(), serviceTaskLog.ToString());

			var whsOrder = (IWhsOrder)Factory.LoadTop1(ObjectFactory.GetType<IWhsOrder>(), new ZQuery());

			var customFields = ((ICustomFieldProvider)whsOrder).GetCustomBusinessObject();
			var propertyNames = ((IDynamicBusinessObject)customFields).PropertyNames;

			AssertEquals("CustomFields should be loaded", 2, propertyNames.Length);

			AssertEquals(CustomPropertyHelper.GeneratePropertyIdentifier("WD_CUSTOMATTRIB1", typeof(ZString)), propertyNames[0]);
			AssertEquals("44", customFields[propertyNames[0]]);
		}

		#region WhsOrderXMLCreate
		const string WhsOrderXMLCreate = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WarehouseOrder</Type>
          <Key></Key>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>DNZ</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>NZ Demo Company</Name>
      </Company>
      <DataProvider>HYEIKBDNZ</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>AKL</Code>
        <Name>AKL</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code></Code>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>IKB</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2013-11-12T16:17:11.53</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <CarrierServiceLevel>
      <Code></Code>
    </CarrierServiceLevel>
    <ContainerMode>
      <Code>LTL</Code>
      <Description>Less Truck Load</Description>
    </ContainerMode>
    <GoodsDescription></GoodsDescription>
    <GoodsValue>0.0000</GoodsValue>
    <GoodsValueCurrency>
      <Code></Code>
    </GoodsValueCurrency>
    <JobCosting>
      <AccrualNotRecognized>0</AccrualNotRecognized>
      <AccrualRecognized>0</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>AKL</Code>
        <Name>AKL</Name>
      </Branch>
      <Currency>
        <Code>NZD</Code>
        <Description>New Zealand Dollar</Description>
      </Currency>
      <Department>
        <Code>WFS</Code>
        <Name>Warehouse Free Store</Name>
      </Department>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0</TotalAccrual>
      <TotalCost>0</TotalCost>
      <TotalJobProfit>0</TotalJobProfit>
      <TotalRevenue>0</TotalRevenue>
      <TotalWIP>0</TotalWIP>
      <WIPNotRecognized>0</WIPNotRecognized>
      <WIPRecognized>0</WIPRecognized>
    </JobCosting>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code></Code>
    </OuterPacksPackageType>
    <PortOfDestination>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfOrigin>
    <ServiceLevel>
      <Code></Code>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <ShipperCODAmount>0.0000</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PCE</Code>
      <Description>Piece</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Metres</Description>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>ROA</Code>
      <Description>Road Freight</Description>
    </TransportMode>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <DeliveryRequiredBy>2013-11-12T23:59:00</DeliveryRequiredBy>
    </LocalProcessing>

    <Order>
      <OrderNumber>W00000003</OrderNumber>
      <AddPalletWeightToOrder>false</AddPalletWeightToOrder>
      <ClientReference></ClientReference>
      <DropMode>
        <Code>HSL</Code>
        <Description>Haulier Supplies Lift</Description>
      </DropMode>
      <FulfillmentRule>
        <Code>NON</Code>
        <Description>None</Description>
      </FulfillmentRule>
      <LocalCartageInsuranceValue>0.0000</LocalCartageInsuranceValue>
      <OrderNumberSplit>0</OrderNumberSplit>
      <PalletsSent>0</PalletsSent>
      <PickOption>
        <Code>AUT</Code>
        <Description>Auto Pick</Description>
      </PickOption>
      <Status>
        <Code>ENT</Code>
        <Description>Entered (Saved)</Description>
      </Status>
      <TotalLineVolume>0.000</TotalLineVolume>
      <TotalLineWeight>0.000</TotalLineWeight>
      <TotalNetWeightSent>0.000</TotalNetWeightSent>
      <TotalUnits>0.000</TotalUnits>
      <TransportReference></TransportReference>
      <Type>
        <Code>ORD</Code>
        <Description>ORDER</Description>
      </Type>
      <UnitsSent>0.000</UnitsSent>
      <Warehouse>
        <Code>WOH</Code>
        <Name>ABC WAREHOUSE</Name>
      </Warehouse>
    </Order>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>Test</Key>
        <DataType>String</DataType>
        <Value>44</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>SD</AddressShortCode>
        <OrganizationCode>GFGFD</OrganizationCode>
        <Address1>SD</Address1>
        <Address2>SD</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>SDSD</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2230</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendersLocalClient</AddressType>
        <AddressShortCode>SD</AddressShortCode>
        <OrganizationCode>GFGFD</OrganizationCode>
        <Address1>SD</Address1>
        <Address2>SD</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>SDSD</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2230</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeAddress</AddressType>
        <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
        <OrganizationCode>AASDRA</OrganizationCode>
        <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
        <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
        <AddressOverride>false</AddressOverride>
        <City>KOWLOON</City>
        <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email>Ilya.KirsanovBelov@cargowise.com</Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AddressShortCode>99 GLIMMER PARK DR</AddressShortCode>
        <OrganizationCode>BENGOVSYD</OrganizationCode>
        <Address1>99 GLIMMER PARK DR</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>GLENWOOD</City>
        <CompanyName>Ben Govett</CompanyName>
        <Contact>Ben Govett</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>Ilya.KirsanovBelov@cargowise.com</Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2768</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>EPL</Code>
              <Description>eParcel Merchant Location ID</Description>
            </Type>
            <Value>dfghdfgh</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CustomsWarehouseAddress</AddressType>
        <AddressShortCode>99 GLIMMER PARK DR</AddressShortCode>
        <OrganizationCode>BENGOVSYD</OrganizationCode>
        <Address1>99 GLIMMER PARK DR</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>GLENWOOD</City>
        <CompanyName>Ben Govett</CompanyName>
        <Contact>Ben Govett</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>Ilya.KirsanovBelov@cargowise.com</Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2768</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>EPL</Code>
              <Description>eParcel Merchant Location ID</Description>
            </Type>
            <Value>dfghdfgh</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
		#endregion"__WD_CustomAttrib1__prop"

		public void TestImportCustomFieldsAreNotDuplicatedFromRegistryAndTemplate()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				MasterFilesTestHelper.ClearWorkflowTables();

				var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				processTaskTemplate.P0_ProcessType = "SHP";
				processTaskTemplate.P0_IsActive = true;

				FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, new CaptionAndHint("CustomBlaString", "RegistryValue"));

				Factory.SaveForTesting();

				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
				var message = GetQueuedUniversalShipmentMessage(ShipmentXMLDuplicate);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine.
".Trim(), serviceTaskLog.ToString());

					AssertMultilineASCIIEquals("Message Log Note", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'SendersLocalClient':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine.
".Trim(), message.GetLogNoteText());
				});

				var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertEquals("MEGA SHIPMENT", shipment.JS_HouseBill);

				var customFields = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();
				var propertyNames = ((IDynamicBusinessObject)customFields).PropertyNames;

				AssertEquals("CustomFields should be loaded", 2, propertyNames.Length);

				AssertEquals(CustomPropertyHelper.GeneratePropertyIdentifier("DOCSANDCARTAGE+JP_CUSTOMATTRIB1", typeof(ZString)), propertyNames[0]);
				AssertEquals("TEST", customFields[propertyNames[0]]);
			}
		}

		#region ShipmentXML
		const string ShipmentXMLDuplicate = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		</DataTarget>
	  </DataTargetCollection>

	  <ActionPurpose>
		<Code>APP</Code>
		<Description>As Per Payload</Description>
	  </ActionPurpose>
	  <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>New Zealand</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <DataProvider>HYEIKBDNZ</DataProvider>
	  <EnterpriseID>EDI</EnterpriseID>
	  <ServerID>DAT</ServerID>
	  <EventBranch>
		<Code>SYD</Code>
		<Name>SYD</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code></Code>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2013-05-28T15:28:25.413</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>

	  <RecipientRoleCollection>
		<RecipientRole>
		  <Code>ORP</Code>
		  <Description>Organization Proxy</Description>
		</RecipientRole>
	  </RecipientRoleCollection>
	</DataContext>

	<ActualChargeable>234.000</ActualChargeable>
	<AdditionalTerms></AdditionalTerms>
	<BookingConfirmationReference></BookingConfirmationReference>
	<CartageWaybillNumber></CartageWaybillNumber>
	<CFSReference></CFSReference>
	<ContainerCount>0</ContainerCount>
	<ContainerMode>
	  <Code>LSE</Code>
	  <Description>Loose</Description>
	</ContainerMode>
	<DocumentedChargeable>234.000</DocumentedChargeable>
	<DocumentedVolume>0.300</DocumentedVolume>
	<DocumentedWeight>234.000</DocumentedWeight>
	<FreightRate>0.0000</FreightRate>
	<FreightRateCurrency>
	  <Code></Code>
	</FreightRateCurrency>
	<GoodsDescription></GoodsDescription>
	<GoodsValue>0.0000</GoodsValue>
	<GoodsValueCurrency>
	  <Code>NZD</Code>
	  <Description>New Zealand, Dollars</Description>
	</GoodsValueCurrency>
	<HBLAWBChargesDisplay>
	  <Code></Code>
	</HBLAWBChargesDisplay>
	<HBLContainerPackModeOverride></HBLContainerPackModeOverride>
	<InsuranceValue>0.0000</InsuranceValue>
	<InsuranceValueCurrency>
	  <Code>NZD</Code>
	  <Description>New Zealand, Dollars</Description>
	</InsuranceValueCurrency>
	<InterimReceiptNumber></InterimReceiptNumber>
	<IsBooking>false</IsBooking>
	<IsCFSRegistered>false</IsCFSRegistered>
	<IsDirectBooking>false</IsDirectBooking>
	<IsForwardRegistered>true</IsForwardRegistered>
	<IsNeutralMaster>false</IsNeutralMaster>
	<IsShipping>false</IsShipping>
	<IsSplitShipment>false</IsSplitShipment>
	<JobCosting>
	  <AccrualNotRecognized>0</AccrualNotRecognized>
	  <AccrualRecognized>0</AccrualRecognized>
	  <AgentRevenue>0</AgentRevenue>
	  <Branch>
		<Code>AKL</Code>
		<Name>AKL</Name>
	  </Branch>
	  <Currency>
		<Code>NZD</Code>
		<Description>New Zealand, Dollars</Description>
	  </Currency>
	  <LocalClientRevenue>0</LocalClientRevenue>
	  <OperationsStaff>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </OperationsStaff>
	  <OtherDebtorRevenue>0</OtherDebtorRevenue>
	  <TotalAccrual>0</TotalAccrual>
	  <TotalCost>0</TotalCost>
	  <TotalJobProfit>0</TotalJobProfit>
	  <TotalRevenue>0</TotalRevenue>
	  <TotalWIP>0</TotalWIP>
	  <WIPNotRecognized>0</WIPNotRecognized>
	  <WIPRecognized>0</WIPRecognized>
	</JobCosting>
	<ManifestedChargeable>234.000</ManifestedChargeable>
	<ManifestedVolume>0.300</ManifestedVolume>
	<ManifestedWeight>234.000</ManifestedWeight>
	<NoCopyBills>3</NoCopyBills>
	<NoOriginalBills>3</NoOriginalBills>
	<OuterPacks>0</OuterPacks>
	<OuterPacksPackageType>
	  <Code>PLT</Code>
	  <Description>Pallet</Description>
	</OuterPacksPackageType>
	<PackingOrder>0</PackingOrder>
	<PortOfDestination>
	  <Code>AUBNE</Code>
	  <Name>Brisbane</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>NZABY</Code>
	  <Name>Albany</Name>
	</PortOfOrigin>
	<ReleaseType>
	  <Code></Code>
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
	<TotalVolume>0.300</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>234.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TranshipToOtherCFS>false</TranshipToOtherCFS>
	<TransportMode>
	  <Code>AIR</Code>
	  <Description>Air Freight</Description>
	</TransportMode>
	<WarehouseLocation></WarehouseLocation>
	<WayBillNumber>Mega Shipment</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>

	<LocalProcessing>
	  <ArrivalCartageRef></ArrivalCartageRef>
	  <DeliveryCartageAdvised></DeliveryCartageAdvised>
	  <DeliveryCartageCompleted></DeliveryCartageCompleted>
	  <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
	  <DeliveryLabourTime></DeliveryLabourTime>
	  <DeliveryRequiredBy></DeliveryRequiredBy>
	  <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
	  <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
	  <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
	  <DemurrageOnPickupTime></DemurrageOnPickupTime>
	  <EstimatedDelivery></EstimatedDelivery>
	  <EstimatedPickup></EstimatedPickup>
	  <ExportStatement>
		<Code></Code>
	  </ExportStatement>
	  <FCLAvailable></FCLAvailable>
	  <FCLDeliveryEquipmentNeeded>
		<Code>ASK</Code>
		<Description>Ask Client</Description>
	  </FCLDeliveryEquipmentNeeded>
	  <FCLPickupEquipmentNeeded>
		<Code>HSL</Code>
		<Description>Haulier Supplies Lift</Description>
	  </FCLPickupEquipmentNeeded>
	  <FCLStorageCommences></FCLStorageCommences>
	  <HasProhibitedPackaging>false</HasProhibitedPackaging>
	  <InsuranceRequired>false</InsuranceRequired>
	  <IsContingencyRelease>false</IsContingencyRelease>
	  <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
	  <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
	  <LCLAvailable></LCLAvailable>
	  <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
	  <LCLStorageCommences></LCLStorageCommences>
	  <PickupCartageAdvised></PickupCartageAdvised>
	  <PickupCartageCompleted></PickupCartageCompleted>
	  <PickupLabourCharge>0.0000</PickupLabourCharge>
	  <PickupLabourTime></PickupLabourTime>
	  <PickupRequiredBy></PickupRequiredBy>
	  <PrintOptionForPackagesOnAWB>
		<Code>DEF</Code>
		<Description>Default (Dims, fallback to Vol)</Description>
	  </PrintOptionForPackagesOnAWB>
	</LocalProcessing>

	<CustomizedFieldCollection>
	  <CustomizedField>
		<Key>CustomBlaString</Key>
		<DataType>String</DataType>
		<Value>TEST</Value>
	  </CustomizedField>
	</CustomizedFieldCollection>

	<DateCollection>
	  <Date>
		<Type>BookingConfirmed</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Received</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Departure</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2012-08-10T15:28:00</Value>
	  </Date>
	  <Date>
		<Type>Arrival</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2012-08-12T15:28:00</Value>
	  </Date>
	  <Date>
		<Type>ShippedOnBoard</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>BillIssued</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	</DateCollection>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<City>MOSCOW</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode>113186</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>BE</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>BE</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>SendersLocalClient</AddressType>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>

	<PackingLineCollection>
	  <PackingLine>
		<Commodity>
		  <Code>GEN</Code>
		  <Description>General</Description>
		</Commodity>
		<ContainerPackingOrder>0</ContainerPackingOrder>
		<CountryOfOrigin>
		  <Code></Code>
		</CountryOfOrigin>
		<DetailedDescription></DetailedDescription>
		<EndItemNo>0</EndItemNo>
		<GoodsDescription></GoodsDescription>
		<HarmonisedCode></HarmonisedCode>
		<Height>0.000</Height>
		<ItemNo>0</ItemNo>
		<Length>0.000</Length>
		<LengthUnit>
		  <Code>M</Code>
		  <Description>Meters</Description>
		</LengthUnit>
		<LinePrice>0.0000</LinePrice>
		<LoadingMeters>0.000</LoadingMeters>
		<MarksAndNos></MarksAndNos>
		<OutturnComment></OutturnComment>
		<OutturnDamagedQty>0</OutturnDamagedQty>
		<OutturnedHeight>0.000</OutturnedHeight>
		<OutturnedLength>0.000</OutturnedLength>
		<OutturnedVolume>0.000</OutturnedVolume>
		<OutturnedWeight>0.000</OutturnedWeight>
		<OutturnedWidth>0.000</OutturnedWidth>
		<OutturnPillagedQty>0</OutturnPillagedQty>
		<OutturnQty>0</OutturnQty>
		<PackQty>0</PackQty>
		<PackType>
		  <Code>PLT</Code>
		  <Description>Pallet</Description>
		</PackType>
		<ReferenceNumber></ReferenceNumber>
		<Volume>0.300</Volume>
		<VolumeUnit>
		  <Code>M3</Code>
		  <Description>Cubic Meters</Description>
		</VolumeUnit>
		<Weight>234.000</Weight>
		<WeightUnit>
		  <Code>KG</Code>
		  <Description>Kilograms</Description>
		</WeightUnit>
		<Width>0.000</Width>
	  </PackingLine>
	</PackingLineCollection>
  </Shipment>
</UniversalShipment>";
		#endregion

		public void TestImportAllCustomFieldsForLocalClient()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_FullName = "BLABLABLA";

				MasterFilesTestHelper.ClearWorkflowTables();

				var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				processTaskTemplate.P0_ProcessType = "SHP";
				processTaskTemplate.P0_IsActive = true;
				processTaskTemplate.P0_OH_Client = orgHeader.PK;

				var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
				genCustomColumnBool.XC_Name = "CustomBlaBool";
				genCustomColumnBool.XC_Type = "BOO";
				processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

				var genCustomColumnEmptyBool = Factory.New<GenCustomColumnDefinition>();
				genCustomColumnEmptyBool.XC_Name = "CustomBlaEmptyBool";
				genCustomColumnEmptyBool.XC_Type = "BOO";
				processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnEmptyBool);

				var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
				genCustomColumnString.XC_Name = "CustomBlaString";
				genCustomColumnString.XC_Type = "STR";
				processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

				var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
				genCustomColumnInt.XC_Name = "CustomBlaInt";
				genCustomColumnInt.XC_Type = "INT";
				processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

				var genCustomDuplicateCol1 = Factory.New<GenCustomColumnDefinition>();
				genCustomDuplicateCol1.XC_Name = "CustomDuplicate";
				genCustomDuplicateCol1.XC_Type = "INT";
				processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomDuplicateCol1);

				var genCustomDuplicateCol2 = Factory.New<GenCustomColumnDefinition>();
				genCustomDuplicateCol2.XC_Name = "CustomDuplicate";
				genCustomDuplicateCol2.XC_Type = "STR";
				processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomDuplicateCol2);

				Factory.SaveForTesting();

				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
				var message = GetQueuedUniversalShipmentMessage(ShipmentXML);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine.
".Trim(), serviceTaskLog.ToString());

					AssertMultilineASCIIEquals("Message Log Note", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'SendersLocalClient':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine.
".Trim(), message.GetLogNoteText());
				});

				var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertEquals("MEGA SHIPMENT", shipment.JS_HouseBill);

				var customFields = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();
				var propertyNames = ((IDynamicBusinessObject)customFields).PropertyNames;

				AssertEquals("4 CustomFields should be loaded", 10, propertyNames.Length);

				AssertEquals("__CUSTOMBLABOOL__prop__ZBool", propertyNames[0]);
				AssertEquals(true, customFields[propertyNames[0]]);

				AssertEquals("__CUSTOMBLAINT__prop__ZInt", propertyNames[1]);
				AssertEquals(44, customFields[propertyNames[1]]);

				AssertEquals("__CUSTOMBLASTRING__prop__ZString", propertyNames[2]);
				AssertEquals("TEST", customFields[propertyNames[2]]);

				AssertEquals("__CUSTOMDUPLICATE__prop__ZInt", propertyNames[3]);
				AssertEquals(44, customFields[propertyNames[3]]);

				AssertEquals("__CUSTOMDUPLICATE__prop__ZString", propertyNames[4]);
				AssertEquals("TEST", customFields[propertyNames[4]]);
			}
		}

		#region ShipmentXML
		const string ShipmentXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		</DataTarget>
	  </DataTargetCollection>
 	  <ActionPurpose>
		<Code>APP</Code>
		<Description>As Per Payload</Description>
	  </ActionPurpose>
	  <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>New Zealand</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <DataProvider>HYEIKBDNZ</DataProvider>
	  <EnterpriseID>EDI</EnterpriseID>
	  <ServerID>DAT</ServerID>
	  <EventBranch>
		<Code>SYD</Code>
		<Name>SYD</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code></Code>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2013-05-28T15:28:25.413</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>

	  <RecipientRoleCollection>
		<RecipientRole>
		  <Code>ORP</Code>
		  <Description>Organization Proxy</Description>
		</RecipientRole>
	  </RecipientRoleCollection>
	</DataContext>

	<ActualChargeable>234.000</ActualChargeable>
	<AdditionalTerms></AdditionalTerms>
	<BookingConfirmationReference></BookingConfirmationReference>
	<CartageWaybillNumber></CartageWaybillNumber>
	<CFSReference></CFSReference>
	<ContainerCount>0</ContainerCount>
	<ContainerMode>
	  <Code>LSE</Code>
	  <Description>Loose</Description>
	</ContainerMode>
	<DocumentedChargeable>234.000</DocumentedChargeable>
	<DocumentedVolume>0.300</DocumentedVolume>
	<DocumentedWeight>234.000</DocumentedWeight>
	<FreightRate>0.0000</FreightRate>
	<FreightRateCurrency>
	  <Code></Code>
	</FreightRateCurrency>
	<GoodsDescription></GoodsDescription>
	<GoodsValue>0.0000</GoodsValue>
	<GoodsValueCurrency>
	  <Code>NZD</Code>
	  <Description>New Zealand, Dollars</Description>
	</GoodsValueCurrency>
	<HBLAWBChargesDisplay>
	  <Code></Code>
	</HBLAWBChargesDisplay>
	<HBLContainerPackModeOverride></HBLContainerPackModeOverride>
	<InsuranceValue>0.0000</InsuranceValue>
	<InsuranceValueCurrency>
	  <Code>NZD</Code>
	  <Description>New Zealand, Dollars</Description>
	</InsuranceValueCurrency>
	<InterimReceiptNumber></InterimReceiptNumber>
	<IsBooking>false</IsBooking>
	<IsCFSRegistered>false</IsCFSRegistered>
	<IsDirectBooking>false</IsDirectBooking>
	<IsForwardRegistered>true</IsForwardRegistered>
	<IsNeutralMaster>false</IsNeutralMaster>
	<IsShipping>false</IsShipping>
	<IsSplitShipment>false</IsSplitShipment>
	<JobCosting>
	  <AccrualNotRecognized>0</AccrualNotRecognized>
	  <AccrualRecognized>0</AccrualRecognized>
	  <AgentRevenue>0</AgentRevenue>
	  <Branch>
		<Code>AKL</Code>
		<Name>AKL</Name>
	  </Branch>
	  <Currency>
		<Code>NZD</Code>
		<Description>New Zealand, Dollars</Description>
	  </Currency>
	  <LocalClientRevenue>0</LocalClientRevenue>
	  <OperationsStaff>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </OperationsStaff>
	  <OtherDebtorRevenue>0</OtherDebtorRevenue>
	  <TotalAccrual>0</TotalAccrual>
	  <TotalCost>0</TotalCost>
	  <TotalJobProfit>0</TotalJobProfit>
	  <TotalRevenue>0</TotalRevenue>
	  <TotalWIP>0</TotalWIP>
	  <WIPNotRecognized>0</WIPNotRecognized>
	  <WIPRecognized>0</WIPRecognized>
	</JobCosting>
	<ManifestedChargeable>234.000</ManifestedChargeable>
	<ManifestedVolume>0.300</ManifestedVolume>
	<ManifestedWeight>234.000</ManifestedWeight>
	<NoCopyBills>3</NoCopyBills>
	<NoOriginalBills>3</NoOriginalBills>
	<OuterPacks>0</OuterPacks>
	<OuterPacksPackageType>
	  <Code>PLT</Code>
	  <Description>Pallet</Description>
	</OuterPacksPackageType>
	<PackingOrder>0</PackingOrder>
	<PortOfDestination>
	  <Code>AUBNE</Code>
	  <Name>Brisbane</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>NZABY</Code>
	  <Name>Albany</Name>
	</PortOfOrigin>
	<ReleaseType>
	  <Code></Code>
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
	<TotalVolume>0.300</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>234.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TranshipToOtherCFS>false</TranshipToOtherCFS>
	<TransportMode>
	  <Code>AIR</Code>
	  <Description>Air Freight</Description>
	</TransportMode>
	<WarehouseLocation></WarehouseLocation>
	<WayBillNumber>Mega Shipment</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>

	<LocalProcessing>
	  <ArrivalCartageRef></ArrivalCartageRef>
	  <DeliveryCartageAdvised></DeliveryCartageAdvised>
	  <DeliveryCartageCompleted></DeliveryCartageCompleted>
	  <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
	  <DeliveryLabourTime></DeliveryLabourTime>
	  <DeliveryRequiredBy></DeliveryRequiredBy>
	  <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
	  <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
	  <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
	  <DemurrageOnPickupTime></DemurrageOnPickupTime>
	  <EstimatedDelivery></EstimatedDelivery>
	  <EstimatedPickup></EstimatedPickup>
	  <ExportStatement>
		<Code></Code>
	  </ExportStatement>
	  <FCLAvailable></FCLAvailable>
	  <FCLDeliveryEquipmentNeeded>
		<Code>ASK</Code>
		<Description>Ask Client</Description>
	  </FCLDeliveryEquipmentNeeded>
	  <FCLPickupEquipmentNeeded>
		<Code>HSL</Code>
		<Description>Haulier Supplies Lift</Description>
	  </FCLPickupEquipmentNeeded>
	  <FCLStorageCommences></FCLStorageCommences>
	  <HasProhibitedPackaging>false</HasProhibitedPackaging>
	  <InsuranceRequired>false</InsuranceRequired>
	  <IsContingencyRelease>false</IsContingencyRelease>
	  <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
	  <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
	  <LCLAvailable></LCLAvailable>
	  <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
	  <LCLStorageCommences></LCLStorageCommences>
	  <PickupCartageAdvised></PickupCartageAdvised>
	  <PickupCartageCompleted></PickupCartageCompleted>
	  <PickupLabourCharge>0.0000</PickupLabourCharge>
	  <PickupLabourTime></PickupLabourTime>
	  <PickupRequiredBy></PickupRequiredBy>
	  <PrintOptionForPackagesOnAWB>
		<Code>DEF</Code>
		<Description>Default (Dims, fallback to Vol)</Description>
	  </PrintOptionForPackagesOnAWB>
	</LocalProcessing>

	<CustomizedFieldCollection>
	  <CustomizedField>
		<Key>CustomBlaString</Key>
		<DataType>String</DataType>
		<Value>TEST</Value>
	  </CustomizedField>
	  <CustomizedField>
		<Key>CustomBlaInt</Key>
		<DataType>Integer</DataType>
		<Value>44</Value>
	  </CustomizedField>
	  <CustomizedField>
		<Key>CustomBlaBool</Key>
		<DataType>Boolean</DataType>
		<Value>true</Value>
	  </CustomizedField>
	  <CustomizedField>
		<Key>CustomDuplicate</Key>
		<DataType>String</DataType>
		<Value>TEST</Value>
	  </CustomizedField>
	  <CustomizedField>
		<Key>CustomDuplicate</Key>
		<DataType>Integer</DataType>
		<Value>44</Value>
	  </CustomizedField>
	</CustomizedFieldCollection>

	<DateCollection>
	  <Date>
		<Type>BookingConfirmed</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Received</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Departure</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2012-08-10T15:28:00</Value>
	  </Date>
	  <Date>
		<Type>Arrival</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2012-08-12T15:28:00</Value>
	  </Date>
	  <Date>
		<Type>ShippedOnBoard</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>BillIssued</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	</DateCollection>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<City>MOSCOW</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode>113186</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>BE</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>BE</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>SendersLocalClient</AddressType>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>

	<PackingLineCollection>
	  <PackingLine>
		<Commodity>
		  <Code>GEN</Code>
		  <Description>General</Description>
		</Commodity>
		<ContainerPackingOrder>0</ContainerPackingOrder>
		<CountryOfOrigin>
		  <Code></Code>
		</CountryOfOrigin>
		<DetailedDescription></DetailedDescription>
		<EndItemNo>0</EndItemNo>
		<GoodsDescription></GoodsDescription>
		<HarmonisedCode></HarmonisedCode>
		<Height>0.000</Height>
		<ItemNo>0</ItemNo>
		<Length>0.000</Length>
		<LengthUnit>
		  <Code>M</Code>
		  <Description>Meters</Description>
		</LengthUnit>
		<LinePrice>0.0000</LinePrice>
		<LoadingMeters>0.000</LoadingMeters>
		<MarksAndNos></MarksAndNos>
		<OutturnComment></OutturnComment>
		<OutturnDamagedQty>0</OutturnDamagedQty>
		<OutturnedHeight>0.000</OutturnedHeight>
		<OutturnedLength>0.000</OutturnedLength>
		<OutturnedVolume>0.000</OutturnedVolume>
		<OutturnedWeight>0.000</OutturnedWeight>
		<OutturnedWidth>0.000</OutturnedWidth>
		<OutturnPillagedQty>0</OutturnPillagedQty>
		<OutturnQty>0</OutturnQty>
		<PackQty>0</PackQty>
		<PackType>
		  <Code>PLT</Code>
		  <Description>Pallet</Description>
		</PackType>
		<ReferenceNumber></ReferenceNumber>
		<Volume>0.300</Volume>
		<VolumeUnit>
		  <Code>M3</Code>
		  <Description>Cubic Meters</Description>
		</VolumeUnit>
		<Weight>234.000</Weight>
		<WeightUnit>
		  <Code>KG</Code>
		  <Description>Kilograms</Description>
		</WeightUnit>
		<Width>0.000</Width>
	  </PackingLine>
	</PackingLineCollection>
  </Shipment>
</UniversalShipment>";
		#endregion

		#region ShipmentNotMappedToTargetXML
		const string ShipmentNotMappedToTargetXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		</DataTarget>
	  </DataTargetCollection>
	  <ActionPurpose>
		<Code>APP</Code>
		<Description>As Per Payload</Description>
	  </ActionPurpose>
	  <Company>
		<Code>DEM</Code>
		<Country>
		  <Code>NZ</Code>
		  <Name>New Zealand</Name>
		</Country>
		<Name>NZ Demo Company</Name>
	  </Company>
	  <DataProvider>HYEIKBDNZ</DataProvider>
	  <EnterpriseID>HYE</EnterpriseID>
	  <EventBranch>
		<Code>AKL</Code>
		<Name>AKL</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code></Code>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>IKB</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2013-05-28T15:28:25.413</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>

	  <RecipientRoleCollection>
		<RecipientRole>
		  <Code>ORP</Code>
		  <Description>Organization Proxy</Description>
		</RecipientRole>
	  </RecipientRoleCollection>
	</DataContext>

	<ActualChargeable>234.000</ActualChargeable>
	<AdditionalTerms></AdditionalTerms>
	<BookingConfirmationReference></BookingConfirmationReference>
	<CartageWaybillNumber></CartageWaybillNumber>
	<CFSReference></CFSReference>
	<ContainerCount>0</ContainerCount>
	<ContainerMode>
	  <Code>LSE</Code>
	  <Description>Loose</Description>
	</ContainerMode>
	<DocumentedChargeable>234.000</DocumentedChargeable>
	<DocumentedVolume>0.300</DocumentedVolume>
	<DocumentedWeight>234.000</DocumentedWeight>
	<FreightRate>0.0000</FreightRate>
	<FreightRateCurrency>
	  <Code></Code>
	</FreightRateCurrency>
	<GoodsDescription></GoodsDescription>
	<GoodsValue>0.0000</GoodsValue>
	<GoodsValueCurrency>
	  <Code>NZD</Code>
	  <Description>New Zealand, Dollars</Description>
	</GoodsValueCurrency>
	<HBLAWBChargesDisplay>
	  <Code></Code>
	</HBLAWBChargesDisplay>
	<HBLContainerPackModeOverride></HBLContainerPackModeOverride>
	<InsuranceValue>0.0000</InsuranceValue>
	<InsuranceValueCurrency>
	  <Code>NZD</Code>
	  <Description>New Zealand, Dollars</Description>
	</InsuranceValueCurrency>
	<InterimReceiptNumber></InterimReceiptNumber>
	<IsBooking>false</IsBooking>
	<IsCFSRegistered>false</IsCFSRegistered>
	<IsDirectBooking>false</IsDirectBooking>
	<IsForwardRegistered>true</IsForwardRegistered>
	<IsNeutralMaster>false</IsNeutralMaster>
	<IsShipping>false</IsShipping>
	<IsSplitShipment>false</IsSplitShipment>
	<JobCosting>
	  <AccrualNotRecognized>0</AccrualNotRecognized>
	  <AccrualRecognized>0</AccrualRecognized>
	  <AgentRevenue>0</AgentRevenue>
	  <Branch>
		<Code>AKL</Code>
		<Name>AKL</Name>
	  </Branch>
	  <Currency>
		<Code>NZD</Code>
		<Description>New Zealand, Dollars</Description>
	  </Currency>
	  <LocalClientRevenue>0</LocalClientRevenue>
	  <OperationsStaff>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </OperationsStaff>
	  <OtherDebtorRevenue>0</OtherDebtorRevenue>
	  <TotalAccrual>0</TotalAccrual>
	  <TotalCost>0</TotalCost>
	  <TotalJobProfit>0</TotalJobProfit>
	  <TotalRevenue>0</TotalRevenue>
	  <TotalWIP>0</TotalWIP>
	  <WIPNotRecognized>0</WIPNotRecognized>
	  <WIPRecognized>0</WIPRecognized>
	</JobCosting>
	<ManifestedChargeable>234.000</ManifestedChargeable>
	<ManifestedVolume>0.300</ManifestedVolume>
	<ManifestedWeight>234.000</ManifestedWeight>
	<NoCopyBills>3</NoCopyBills>
	<NoOriginalBills>3</NoOriginalBills>
	<OuterPacks>0</OuterPacks>
	<OuterPacksPackageType>
	  <Code>PLT</Code>
	  <Description>Pallet</Description>
	</OuterPacksPackageType>
	<PackingOrder>0</PackingOrder>
	<PortOfDestination>
	  <Code>AUBNE</Code>
	  <Name>Brisbane</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>NZABY</Code>
	  <Name>Albany</Name>
	</PortOfOrigin>
	<ReleaseType>
	  <Code></Code>
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
	<TotalVolume>0.300</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>234.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TranshipToOtherCFS>false</TranshipToOtherCFS>
	<TransportMode>
	  <Code>AIR</Code>
	  <Description>Air Freight</Description>
	</TransportMode>
	<WarehouseLocation></WarehouseLocation>
	<WayBillNumber>Mega Shipment</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>

	<LocalProcessing>
	  <ArrivalCartageRef></ArrivalCartageRef>
	  <DeliveryCartageAdvised></DeliveryCartageAdvised>
	  <DeliveryCartageCompleted></DeliveryCartageCompleted>
	  <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
	  <DeliveryLabourTime></DeliveryLabourTime>
	  <DeliveryRequiredBy></DeliveryRequiredBy>
	  <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
	  <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
	  <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
	  <DemurrageOnPickupTime></DemurrageOnPickupTime>
	  <EstimatedDelivery></EstimatedDelivery>
	  <EstimatedPickup></EstimatedPickup>
	  <ExportStatement>
		<Code></Code>
	  </ExportStatement>
	  <FCLAvailable></FCLAvailable>
	  <FCLDeliveryEquipmentNeeded>
		<Code>ASK</Code>
		<Description>Ask Client</Description>
	  </FCLDeliveryEquipmentNeeded>
	  <FCLPickupEquipmentNeeded>
		<Code>HSL</Code>
		<Description>Haulier Supplies Lift</Description>
	  </FCLPickupEquipmentNeeded>
	  <FCLStorageCommences></FCLStorageCommences>
	  <HasProhibitedPackaging>false</HasProhibitedPackaging>
	  <InsuranceRequired>false</InsuranceRequired>
	  <IsContingencyRelease>false</IsContingencyRelease>
	  <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
	  <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
	  <LCLAvailable></LCLAvailable>
	  <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
	  <LCLStorageCommences></LCLStorageCommences>
	  <PickupCartageAdvised></PickupCartageAdvised>
	  <PickupCartageCompleted></PickupCartageCompleted>
	  <PickupLabourCharge>0.0000</PickupLabourCharge>
	  <PickupLabourTime></PickupLabourTime>
	  <PickupRequiredBy></PickupRequiredBy>
	  <PrintOptionForPackagesOnAWB>
		<Code>DEF</Code>
		<Description>Default (Dims, fallback to Vol)</Description>
	  </PrintOptionForPackagesOnAWB>
	</LocalProcessing>

	<CustomizedFieldCollection>
	  <CustomizedField>
		<Key>CustomBlaString</Key>
		<DataType>String</DataType>
		<Value>TEST</Value>
	  </CustomizedField>
	  <CustomizedField>
		<Key>CustomBlaInt</Key>
		<DataType>Integer</DataType>
		<Value>44</Value>
	  </CustomizedField>
	  <CustomizedField>
		<Key>CustomBlaBool</Key>
		<DataType>Boolean</DataType>
		<Value>true</Value>
	  </CustomizedField>
	</CustomizedFieldCollection>

	<DateCollection>
	  <Date>
		<Type>BookingConfirmed</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Received</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Departure</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2012-08-10T15:28:00</Value>
	  </Date>
	  <Date>
		<Type>Arrival</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2012-08-12T15:28:00</Value>
	  </Date>
	  <Date>
		<Type>ShippedOnBoard</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>BillIssued</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	</DateCollection>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<City>MOSCOW</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode>113186</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>BE</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>BE</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>SendersLocalClient</AddressType>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>

	<PackingLineCollection>
	  <PackingLine>
		<Commodity>
		  <Code>GEN</Code>
		  <Description>General</Description>
		</Commodity>
		<ContainerPackingOrder>0</ContainerPackingOrder>
		<CountryOfOrigin>
		  <Code></Code>
		</CountryOfOrigin>
		<DetailedDescription></DetailedDescription>
		<EndItemNo>0</EndItemNo>
		<GoodsDescription></GoodsDescription>
		<HarmonisedCode></HarmonisedCode>
		<Height>0.000</Height>
		<ItemNo>0</ItemNo>
		<Length>0.000</Length>
		<LengthUnit>
		  <Code>M</Code>
		  <Description>Meters</Description>
		</LengthUnit>
		<LinePrice>0.0000</LinePrice>
		<LoadingMeters>0.000</LoadingMeters>
		<MarksAndNos></MarksAndNos>
		<OutturnComment></OutturnComment>
		<OutturnDamagedQty>0</OutturnDamagedQty>
		<OutturnedHeight>0.000</OutturnedHeight>
		<OutturnedLength>0.000</OutturnedLength>
		<OutturnedVolume>0.000</OutturnedVolume>
		<OutturnedWeight>0.000</OutturnedWeight>
		<OutturnedWidth>0.000</OutturnedWidth>
		<OutturnPillagedQty>0</OutturnPillagedQty>
		<OutturnQty>0</OutturnQty>
		<PackQty>0</PackQty>
		<PackType>
		  <Code>PLT</Code>
		  <Description>Pallet</Description>
		</PackType>
		<ReferenceNumber></ReferenceNumber>
		<Volume>0.300</Volume>
		<VolumeUnit>
		  <Code>M3</Code>
		  <Description>Cubic Meters</Description>
		</VolumeUnit>
		<Weight>234.000</Weight>
		<WeightUnit>
		  <Code>KG</Code>
		  <Description>Kilograms</Description>
		</WeightUnit>
		<Width>0.000</Width>
	  </PackingLine>
	</PackingLineCollection>
  </Shipment>
</UniversalShipment>";
		#endregion

		static StmALog AssertConsolContentsReturningLog(BusinessObject consol)
		{
			StmALog consolDataImportLog = null;

			AssertNotNull("Must have created a Consol.", consol);

			CombineAssertions(delegate
			{
				AssertEquals("consol.JK_UniqueConsignRef", "C00001000", consol[JobConsolSchema.JK_UniqueConsignRef]);
				AssertEquals("consol.JK_MasterBillNum", "AGT", consol[JobConsolSchema.JK_AgentType]);
				AssertEquals("consol.JK_TransportMode", "SEA", consol[JobConsolSchema.JK_TransportMode]);
				AssertEquals("consol.JK_ConsolMode", "FCL", consol[JobConsolSchema.JK_ConsolMode]);
				AssertEquals("consol.JK_MasterBillNum", "FAT_TONY", consol[JobConsolSchema.JK_MasterBillNum]);
				AssertEquals("consol.JK_RL_NKLoadPort", "USLAX", consol[JobConsolSchema.JK_RL_NKLoadPort]);
				AssertEquals("consol.JK_RL_NKDischargePort", "AUSYD", consol[JobConsolSchema.JK_RL_NKDischargePort]);

				var consolDataImportLogs = consol.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImportCode));
				AssertEquals("Consol 'Data Import' Logs", 1, consolDataImportLogs.Length);
				consolDataImportLog = consolDataImportLogs[0];
			});

			return consolDataImportLog;
		}

		static StmALog AssertShipmentContentsReturningLog(BusinessObject shipment)
		{
			StmALog shipmentDataImportLog = null;

			AssertNotNull("Must have created a shipment.", shipment);

			CombineAssertions(delegate
			{
				AssertEquals("shipment.JS_UniqueConsignRef", "S00001000", shipment[JobShipmentSchema.JS_UniqueConsignRef]);
				AssertEquals("shipment.JS_TransportMode", "SEA", shipment[JobShipmentSchema.JS_TransportMode]);
				AssertEquals("shipment.JS_PackingMode", "LCL", shipment[JobShipmentSchema.JS_PackingMode]);
				AssertEquals("shipment.JS_ShipmentType", "STD", shipment[JobShipmentSchema.JS_ShipmentType]);
				AssertEquals("shipment.JS_HouseBill", "JIMMY_THE_SNITCH", shipment[JobShipmentSchema.JS_HouseBill]);
				AssertEquals("shipment.JS_RL_NKDestination", "USSFO", shipment[JobShipmentSchema.JS_RL_NKOrigin]);
				AssertEquals("shipment.JS_RL_NKDestination", "AUMEL", shipment[JobShipmentSchema.JS_RL_NKDestination]);

				var shipmentDataImportLogs = shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImportCode));
				AssertEquals("shipment 'Data Import' Logs", 1, shipmentDataImportLogs.Length);
				shipmentDataImportLog = shipmentDataImportLogs[0];
			});

			return shipmentDataImportLog;
		}

		#region ShipmentXMLComments
		const string ShipmentXMLComments = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		</DataTarget>
	  </DataTargetCollection>
 	  <ActionPurpose>
		<Code>APP</Code>
		<Description>As Per Payload</Description>
	  </ActionPurpose>
	  <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>New Zealand</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <DataProvider>HYEIKBDNZ</DataProvider>
	  <EnterpriseID>EDI</EnterpriseID>
	  <ServerID>DAT</ServerID>
	  <EventBranch>
		<Code>SYD</Code>
		<Name>SYD</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code></Code>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2013-05-28T15:28:25.413</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>

	  <RecipientRoleCollection>
		<RecipientRole>
		  <Code>ORP</Code>
		  <Description>Organization Proxy</Description>
		</RecipientRole>
	  </RecipientRoleCollection>
	</DataContext>

	<ActualChargeable>234.000</ActualChargeable>
	<AdditionalTerms></AdditionalTerms>
	<BookingConfirmationReference></BookingConfirmationReference>
	<CartageWaybillNumber></CartageWaybillNumber>
	<CFSReference></CFSReference>
	<ContainerCount>0</ContainerCount>
	<ContainerMode>
	  <Code>LSE</Code>
	  <Description>Loose</Description>
	</ContainerMode>
	<DocumentedChargeable>234.000</DocumentedChargeable>
	<DocumentedVolume>0.300</DocumentedVolume>
	<DocumentedWeight>234.000</DocumentedWeight>
	<FreightRate>0.0000</FreightRate>
	<FreightRateCurrency>
	  <Code></Code>
	</FreightRateCurrency>
	<GoodsDescription>UeYTL3HohEgo2U2xNRPoRbdeTkMJ5n9Ye5IGYdlqs6k8h8M2LR7K1AgVxTNyKzI07rCAKHUkhadBVAANzCxWQTJiPHBNHij4ctdbhwhiIhzuJY6SWBpK7t0JyXbwBlxQUixf4ueA4er6ejA0aB34kdriXJVtsyOK7gsbFJbRdNgNx6ogdo03s2tlxV5chh9IxxZijom0E4Ezjk</GoodsDescription>
	<GoodsValue>0.0000</GoodsValue>
	<GoodsValueCurrency>
	  <Code>NZD</Code>
	  <Description>New Zealand, Dollars</Description>
	</GoodsValueCurrency>
	<!-- My Word!!! -->
	<HBLAWBChargesDisplay>
	  <Code></Code>
	</HBLAWBChargesDisplay>
	<HBLContainerPackModeOverride></HBLContainerPackModeOverride>
	<InsuranceValue>0.0000</InsuranceValue>
	<InsuranceValueCurrency>
	  <Code>NZD</Code>
	  <Description>New Zealand, Dollars</Description>
	</InsuranceValueCurrency>
	<InterimReceiptNumber></InterimReceiptNumber>
	<IsBooking>false</IsBooking>
	<IsCFSRegistered>false</IsCFSRegistered>
	<IsDirectBooking>false</IsDirectBooking>
	<IsForwardRegistered>true</IsForwardRegistered>
	<IsNeutralMaster>false</IsNeutralMaster>
	<IsShipping>false</IsShipping>
	<IsSplitShipment>false</IsSplitShipment>
	<JobCosting>
	  <AccrualNotRecognized>0</AccrualNotRecognized>
	  <AccrualRecognized>0</AccrualRecognized>
	  <AgentRevenue>0</AgentRevenue>
	  <Branch>
		<Code>AKL</Code>
		<Name>AKL</Name>
	  </Branch>
	  <Currency>
		<Code>NZD</Code>
		<Description>New Zealand, Dollars</Description>
	  </Currency>
	  <LocalClientRevenue>0</LocalClientRevenue>
	  <OperationsStaff>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </OperationsStaff>
	  <OtherDebtorRevenue>0</OtherDebtorRevenue>
	  <TotalAccrual>0</TotalAccrual>
	  <TotalCost>0</TotalCost>
	  <TotalJobProfit>0</TotalJobProfit>
	  <TotalRevenue>0</TotalRevenue>
	  <TotalWIP>0</TotalWIP>
	  <WIPNotRecognized>0</WIPNotRecognized>
	  <WIPRecognized>0</WIPRecognized>
	</JobCosting>
	<ManifestedChargeable>234.000</ManifestedChargeable>
	<ManifestedVolume>0.300</ManifestedVolume>
	<ManifestedWeight>234.000</ManifestedWeight>
	<NoCopyBills>3</NoCopyBills>
	<NoOriginalBills>3</NoOriginalBills>
	<OuterPacks>0</OuterPacks>
	<OuterPacksPackageType>
	  <Code>PLT</Code>
	  <Description>Pallet</Description>
	</OuterPacksPackageType>
	<PackingOrder>0</PackingOrder>
	<PortOfDestination>
	  <Code>AUBNE</Code>
	  <Name>Brisbane</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>NZABY</Code>
	  <Name>Albany</Name>
	</PortOfOrigin>
	<ReleaseType>
	  <Code></Code>
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
	<TotalVolume>0.300</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>234.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TranshipToOtherCFS>false</TranshipToOtherCFS>
	<TransportMode>
	  <Code>AIR</Code>
	  <Description>Air Freight</Description>
	</TransportMode>
	<WarehouseLocation></WarehouseLocation>
	<WayBillNumber>Mega Shipment</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>

	<LocalProcessing>
	  <ArrivalCartageRef></ArrivalCartageRef>
	  <DeliveryCartageAdvised></DeliveryCartageAdvised>
	  <DeliveryCartageCompleted></DeliveryCartageCompleted>
	  <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
	  <DeliveryLabourTime></DeliveryLabourTime>
	  <DeliveryRequiredBy></DeliveryRequiredBy>
	  <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
	  <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
	  <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
	  <DemurrageOnPickupTime></DemurrageOnPickupTime>
	  <EstimatedDelivery></EstimatedDelivery>
	  <EstimatedPickup></EstimatedPickup>
	  <ExportStatement>
		<Code></Code>
	  </ExportStatement>
	  <FCLAvailable></FCLAvailable>
	  <FCLDeliveryEquipmentNeeded>
		<Code>ASK</Code>
		<Description>Ask Client</Description>
	  </FCLDeliveryEquipmentNeeded>
	  <FCLPickupEquipmentNeeded>
		<Code>HSL</Code>
		<Description>Haulier Supplies Lift</Description>
	  </FCLPickupEquipmentNeeded>
	  <FCLStorageCommences></FCLStorageCommences>
	  <HasProhibitedPackaging>false</HasProhibitedPackaging>
	  <InsuranceRequired>false</InsuranceRequired>
	  <IsContingencyRelease>false</IsContingencyRelease>
	  <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
	  <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
	  <LCLAvailable></LCLAvailable>
	  <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
	  <LCLStorageCommences></LCLStorageCommences>
	  <PickupCartageAdvised></PickupCartageAdvised>
	  <PickupCartageCompleted></PickupCartageCompleted>
	  <PickupLabourCharge>0.0000</PickupLabourCharge>
	  <PickupLabourTime></PickupLabourTime>
	  <PickupRequiredBy></PickupRequiredBy>
	  <PrintOptionForPackagesOnAWB>
		<Code>DEF</Code>
		<Description>Default (Dims, fallback to Vol)</Description>
	  </PrintOptionForPackagesOnAWB>
	</LocalProcessing>

	<CustomizedFieldCollection>
	  <CustomizedField>
		<Key>CustomBlaString</Key>
		<DataType>String</DataType>
		<Value>TEST</Value>
	  </CustomizedField>
	  <CustomizedField>
		<Key>CustomBlaInt</Key>
		<DataType>Integer</DataType>
		<Value>44</Value>
	  </CustomizedField>
	  <CustomizedField>
		<Key>CustomBlaBool</Key>
		<DataType>Boolean</DataType>
		<Value>true</Value>
	  </CustomizedField>
	</CustomizedFieldCollection>

	<DateCollection>
	  <Date>
		<Type>BookingConfirmed</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Received</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>Departure</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2012-08-10T15:28:00</Value>
	  </Date>
	  <Date>
		<Type>Arrival</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2012-08-12T15:28:00</Value>
	  </Date>
	  <Date>
		<Type>ShippedOnBoard</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	  <Date>
		<Type>BillIssued</Type>
		<IsEstimate>false</IsEstimate>
		<Value></Value>
	  </Date>
	</DateCollection>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<City>MOSCOW</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode>113186</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>BE</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State>BE</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>SendersLocalClient</AddressType>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email>Ilya.KirsanovBelov@cargowise.com</Email>
		<Fax></Fax>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>

	<PackingLineCollection>
	  <PackingLine>
		<Commodity>
		  <Code>GEN</Code>
		  <Description>General</Description>
		</Commodity>
		<ContainerPackingOrder>0</ContainerPackingOrder>
		<CountryOfOrigin>
		  <Code></Code>
		</CountryOfOrigin>
		<DetailedDescription></DetailedDescription>
		<EndItemNo>0</EndItemNo>
		<GoodsDescription></GoodsDescription>
		<HarmonisedCode></HarmonisedCode>
		<Height>0.000</Height>
		<ItemNo>0</ItemNo>
		<Length>0.000</Length>
		<LengthUnit>
		  <Code>M</Code>
		  <Description>Meters</Description>
		</LengthUnit>
		<LinePrice>0.0000</LinePrice>
		<LoadingMeters>0.000</LoadingMeters>
		<MarksAndNos></MarksAndNos>
		<OutturnComment></OutturnComment>
		<OutturnDamagedQty>0</OutturnDamagedQty>
		<OutturnedHeight>0.000</OutturnedHeight>
		<OutturnedLength>0.000</OutturnedLength>
		<OutturnedVolume>0.000</OutturnedVolume>
		<OutturnedWeight>0.000</OutturnedWeight>
		<OutturnedWidth>0.000</OutturnedWidth>
		<OutturnPillagedQty>0</OutturnPillagedQty>
		<OutturnQty>0</OutturnQty>
		<PackQty>0</PackQty>
		<PackType>
		  <Code>PLT</Code>
		  <Description>Pallet</Description>
		</PackType>
		<ReferenceNumber></ReferenceNumber>
		<Volume>0.300</Volume>
		<VolumeUnit>
		  <Code>M3</Code>
		  <Description>Cubic Meters</Description>
		</VolumeUnit>
		<Weight>234.000</Weight>
		<WeightUnit>
		  <Code>KG</Code>
		  <Description>Kilograms</Description>
		</WeightUnit>
		<Width>0.000</Width>
	  </PackingLine>
	</PackingLineCollection>
  </Shipment>
</UniversalShipment>";
		#endregion

		public void TestUniversalShipmentWithComments()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var message = GetQueuedUniversalShipmentMessage(ShipmentXMLComments);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				AssertEquals(@"Comment found at Line 75 :-  My Word!!! 
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'SendersLocalClient':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Warning - Attempted to insert 206 characters into Field [JS_GoodsDescription] which has a maximum length of 35 characters. Field was truncated.
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine."
					, message.GetLogNoteText());
			}
		}

		public void TestUniversalShipmentFailImportWhenSentToAndFromSameModule()
		{
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);
			EnableVerboseLogging();

			var universalShipmentSameModuleSender = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.UniversalShipmentSameModuleSender.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentSameModuleSender);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertMultilineASCIIEquals("Message Log Note", @"
Import into ForwardingConsol was skipped as it was the original source of this data.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestJobCostingProcessedEvenWithoutChargeLines()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				MasterFilesTestHelper.ClearWorkflowTables();

				var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				processTaskTemplate.P0_GB = Env.CurrentBranch.PK;
				processTaskTemplate.P0_ProcessType = "SHP";
				processTaskTemplate.P0_IsActive = true;

				Factory.SaveForTesting();

				TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
				TestCaseHelper.ClearTable(JobHeaderSchema.Constants.TableName);

				var message = GetQueuedUniversalShipmentMessage(ShipmentXML);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine.
".Trim(), serviceTaskLog.ToString());

					AssertMultilineASCIIEquals("Message Log Note", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'SendersLocalClient':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine.
".Trim(), message.GetLogNoteText());
				});

				var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertNotNull(shipment);

				// Job must be created even without any ChargeLines

				var job = (IJobHeader)Factory.LoadTop1(ObjectFactory.GetType<IJobHeader>(), new ZQuery());
				AssertNotNull("Job must be created by processing of the JobCosting details even there are no ChargeLines but DataContext.CodesMappedToTarget is true", job);
			}
		}

		public void TestAddWarningForJobCostingWhenDataContextIsNotMappedToTargetXML()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_GB = Env.CurrentBranch.PK;
			processTaskTemplate.P0_ProcessType = "SHP";
			processTaskTemplate.P0_IsActive = true;

			Factory.SaveForTesting();

			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(JobHeaderSchema.Constants.TableName);

			var message = GetQueuedUniversalShipmentMessage(ShipmentNotMappedToTargetXML);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Matching 'SendersLocalClient':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Added Shipment (House Bill='MEGA SHIPMENT') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='MEGA SHIPMENT') with 1 x ForwardingPackLine.
".Trim(), message.GetLogNoteText());
			});

			var shipment = (Forwarding.IForwardingShipment)Factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
			AssertNotNull(shipment);

			// Job must not be created

			var job = (IJobHeader)Factory.LoadTop1(ObjectFactory.GetType<IJobHeader>(), new ZQuery());
			AssertNull("Job must not be created by processing of the JobCosting details because it is not mapped to target by providing correct Company, ServerID and EnterpriseID", job);
		}

		public void TestWeightVolumeNotClearedWhenNotInXML()
		{
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			var consol = Factory.BOFactory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001286";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_MasterBillNum] = "FILLET-O-FISH";
			consol[JobConsolSchema.JK_BookingReference] = "";
			consol[JobConsolSchema.JK_TotalShipmentActWeightCheck] = 1.0;
			consol[JobConsolSchema.JK_TotalShipmentActVolumeCheck] = 1.0;
			consol[JobConsolSchema.JK_TotalShipmentActOtherUnit] = "KG";
			consol[JobConsolSchema.JK_TotalShipmentChargeableUnit] = "M3";

			var shipment = Factory.BOFactory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001278";
			shipment[JobShipmentSchema.JS_HouseBill] = "JIMMY_THE_SNITCH";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ForwardingConsolPreAllocationWeightNoUnit);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001278 (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Updated Consol C00001286 (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001286 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var factory = new BusinessObjectFactory();
				var loadedConsol = factory.LoadTop1(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), new ZQuery());

				AssertEquals("TotalPreallocatedWeight should have updated", (ZDecimal)1000, loadedConsol[JobConsolSchema.JK_TotalShipmentActWeightCheck]);
				AssertEquals("TotalPreallocatedVolume should have updated", (ZDecimal)1000, loadedConsol[JobConsolSchema.JK_TotalShipmentActVolumeCheck]);
				AssertEquals("Weight unit should not have changed", "KG", loadedConsol[JobConsolSchema.JK_TotalShipmentActOtherUnit]);
				AssertEquals("Volume unit should not have changed", "M3", loadedConsol[JobConsolSchema.JK_TotalShipmentChargeableUnit]);
			});
		}

		#region ForwardingConsolPreAllocationWeightNoUnit

		string ForwardingConsolPreAllocationWeightNoUnit
		{
			get
			{
				return @"
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>C00001286</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
	<TotalPreallocatedWeight>1000</TotalPreallocatedWeight>
	<TotalPreallocatedVolume>1000</TotalPreallocatedVolume>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA DELIMA</VesselName>
    <VoyageFlightNo>822</VoyageFlightNo>
    <WayBillNumber>FAT_TONY</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <SubShipmentCollection>
      <SubShipment>
        <ContainerMode>
          <Code>LCL</Code>
          <Description>Less Container Load</Description>
        </ContainerMode>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>USSFO</Code>
          <Name>San Francisco</Name>
        </PortOfOrigin>
        <ShipmentType>
          <Code>STD</Code>
          <Description>Standard House</Description>
        </ShipmentType>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
			}
		}

		#endregion

		public void TestImportUniversalShipmentWithEstimatedArrivalOutOfSmallDateTimeRange()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OOLU";
			var orgAddress = orgHeader.Addresses.AddNewMainAddress();
			orgAddress.OA_City = "SINGAPORE";
			orgAddress.OA_Address1 = "79 ANSON ROAD";
			orgAddress.OA_Address2 = "#14-00";
			orgAddress.OA_Code = "SGSIN - 79ANSONROAD";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipmentWithEstimatedArrivalOutOfSmallDateTimeRange);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='S89901890') from UniversalShipment.
Added Consol (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				AssertContains("Warning - Line 913: <Shipment>.<TransportLegCollection>.<TransportLeg>.<EstimatedArrival> - Invalid value [2313-04-25T07:51:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.",
												message.GetLogNoteText());

				AssertContains("Successfully saved Consol C00001000 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment.",
												message.GetLogNoteText());
			});
		}

		public void TestShipmentKeyGen_ImportAction()
		{
			var importActionsToTest = new ImportAction?[]
			{
				null,
				ImportAction.Merge,
				ImportAction.LinkOnly
			};

			foreach (var importAction in importActionsToTest)
			{
				var usxml = GetShipmentWithImportAction(importAction);
				var message = GetQueuedUniversalShipmentMessage(usxml);
				var log = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(log);
				var keys = manager.GetKeysForBlockingParallelImport(message);

				AssertContainsExactElementsInAnyOrder($"USXml keys for Action: {importAction}",
					new[]
					{
						"HBL00001001"
					},
					keys.Keys);
			}
		}

		[ExpectNoExceptions]
		public void TestShipmentKeyGen_AvoidNulls()
		{
			var message = GetQueuedUniversalShipmentMessage(UniversalShipmentWithEstimatedArrivalOutOfSmallDateTimeRange);
			var log = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(log);
			Assert(manager.GetKeysForBlockingParallelImport(message).Keys.Any());
		}

		public void TestShipmentKeyGen_AvoidNulls2()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			IShipmentDataContextManager manager = new ShipmentCtx();
			AssertEquals(0, manager.GetKeysForBlockingParallelImport(shipment, new Logger(shipment), new UniversalObjectFactory()).Keys.Count());
		}

		public void TestShipmentKeyGen_AvoidNulls3()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			IShipmentDataContextManager manager = new ShipmentCtx();
			shipment.SetAdditionalBillCollection(() => new List<AdditionalBill> { null });
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>(new AdditionalReference[] { null }));
			shipment.CommercialInfo = new DataObjects.Universal.Customs.CommercialInfo();
			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { null });
			shipment.SetContainerCollection(() => new DataObjectList<Container> { null });
			shipment.SetEntryNumberCollection(() => new List<EntryNumber> { null });
			shipment.SetEntryHeaderCollection(() => new List<DataObjects.Universal.Customs.EntryHeader> { null });
			AssertEquals(0, manager.GetKeysForBlockingParallelImport(shipment, new Logger(shipment), new UniversalObjectFactory()).Keys.Count());

			var entryHeader = new DataObjects.Universal.Customs.EntryHeader(DefaultDataObjectWriterStrategy.TestInstance);
			entryHeader.SetEntryNumberCollection(() => new List<DataObjects.Universal.Customs.EntryNumber> { null });
			shipment.SetEntryHeaderCollection(() => new List<DataObjects.Universal.Customs.EntryHeader> { entryHeader });
			AssertEquals(0, manager.GetKeysForBlockingParallelImport(shipment, new Logger(shipment), new UniversalObjectFactory()).Keys.Count());
		}

		public void TestShipmentKeyGen_AvoidNulls4()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			IShipmentDataContextManager manager = new ShipmentCtx();
			var entryNumber = new EntryNumber();
			entryNumber.Type = null;
			entryNumber.Number = null;

			var entryHeader = new DataObjects.Universal.Customs.EntryHeader(DefaultDataObjectWriterStrategy.TestInstance);
			var entryNumberInEntryHeader = new DataObjects.Universal.Customs.EntryNumber();
			entryNumberInEntryHeader.Type = null;
			entryNumberInEntryHeader.Number = null;
			entryHeader.SetEntryNumberCollection(() => new List<DataObjects.Universal.Customs.EntryNumber> { entryNumberInEntryHeader });

			shipment.SetEntryNumberCollection(() => new List<EntryNumber> { entryNumber });
			shipment.SetEntryHeaderCollection(() => new List<DataObjects.Universal.Customs.EntryHeader> { entryHeader });
			AssertEquals(0, manager.GetKeysForBlockingParallelImport(shipment, new Logger(shipment), new UniversalObjectFactory()).Keys.Count());
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		string UniversalShipmentHasRecipientOrganizationPort => resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientOrganizationPort.xml");

		string UniversalShipmentHasRecipientRoleSAGAndPortOfOrigin => resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasRecipientRoleSAGAndPortOfOrigin.xml");

		string UniversalShipmentHasNoBranch => resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.BranchSelection.UniversalShipmentHasNoBranch.xml");

		string ForwardingConsolUniversalShipment => resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.ForwardingConsolUniversalShipment.xml");

		string UniversalShipmentWithEstimatedArrivalOutOfSmallDateTimeRange => resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.SampleXML.UniversalShipmentWithEstimatedArrivalOutOfSmallDateTimeRange.xml");

		sealed class Logger : IXmlImportLogger
		{
			public Logger(ITopLevelDataObject topLevel)
			{
				TopLevelDataObject = topLevel;
			}

			public bool IsUpdatingConsol { get; set; }
			public bool HasIgnoredModule { get; set; }
			public bool OrgMatchingDisabled => false;
			public ITopLevelDataObject TopLevelDataObject { get; }
			public IDataContextDataObject TopLevelDataContext { get; }
			public IEnumerable<ISimpleLog> Logs => Enumerable.Empty<ISimpleLog>();

			public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

			public void FireDataImportedToBusinessObject(BusinessObject targetBO)
			{
			}

			public void Log(LogType type, string message)
			{
			}

			public void LogBoth(LogType type, string message)
			{
			}

			public void LogErrorToServiceTaskOnly(string message)
			{
			}

			public void LogTopLevelDataContextKey(string dataContextKey)
			{
			}

			public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
			{
			}
		}

		sealed class ShipmentCtx : ShipmentDataContextManager<BusinessObject>
		{
			public override bool ManagesShipments => throw new NotImplementedException();
			public override DataContextType DataContextType => DataContextType.ForwardingShipment;
			public override ZString DataContextKey => throw new NotImplementedException();
			public override string DefaultOutputDirectory => throw new NotImplementedException();
			protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => new ZQuery();
			protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;
			protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory) => null;
			protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => null;
			protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;
			protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}
	}
}
