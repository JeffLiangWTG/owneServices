using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	public class ActivityProcessingTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestProcess()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomerServiceTicket</Type>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>EVT</Code>
        <Description>Event</Description>
      </ActionPurpose>
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
        <Code>ADD</Code>
        <Description>Added a record to the system</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2018-06-25T21:28:42.12</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>CLI</Code>
          <Description>Client</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Summary>Create Documentation</Summary>
    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <OrganizationAddressCollection>
	  <OrganizationAddress>
        <AddressType>Client</AddressType>
        <Address1>1299 BOUNDARY ROAD</Address1>
        <Address2>WACOL, QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>LSKDJFLKJF</AddressShortCode>
        <City>SYDNEY</City>
        <CompanyName>SEALY OF AUSTRALIA</CompanyName>
        <Contact>AARON CROSS</Contact>
        <Country>
        <Code>AU</Code>
        <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <OrganizationCode>SEAOAU</OrganizationCode>
        <Phone></Phone>
        <Port>
        <Code>AUBNE</Code>
        <Name>Brisbane</Name>
        </Port>
        <Postcode>4076</Postcode>
        <ScreeningStatus>
        <Code>NOT</Code>
        <Description>Not Screened</Description>
        </ScreeningStatus>
        <State>QLD</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <Department>
      <Code>BRN</Code>
      <Name>Branch</Name>
    </Department>
    <Description>Documentation will be needed for new features.s</Description>
    <Location>
      <Code>FR</Code>
      <Description>France</Description>
    </Location>
    <SelectionCriterion1>
      <Code>CEL</Code>
      <Description>Celebrity Baby Plastic Surgery Disasters</Description>
    </SelectionCriterion1>
    <SelectionCriterion2>
      <Code>LA1</Code>
      <Description>It's just hot ocean milk with dead animal croutons</Description>
    </SelectionCriterion2>
    <SelectionCriterion3>
      <Code>CH2</Code>
      <Description>It's basically a savoury latte with bugs in it.</Description>
    </SelectionCriterion3>
    <SelectionCriterion4>
      <Code>TWO</Code>
      <Description>Two brothers</Description>
    </SelectionCriterion4>
    <SelectionCriterion5>
      <Code>OLD</Code>
      <Description>Old Women are coming</Description>
    </SelectionCriterion5>
    <Status>
      <Code>OPN</Code>
      <Description>Open</Description>
    </Status>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>Field1</Key>
        <DataType>String</DataType>
        <Value>My custom value</Value>
      </CustomizedField>
      <CustomizedField>
        <Key>Field2</Key>
        <DataType>Boolean</DataType>
        <Value>true</Value>
      </CustomizedField>
    </CustomizedFieldCollection>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Customer Service Ticket - Create Documentation from UniversalActivity.
Successfully saved Customer Service Ticket - CST00000001 - Create Documentation.
".Trim(), serviceTaskLog.ToString());

			AssertMultilineASCIIEquals("Message Log Note", @"
No matching WorkRequest found, creating new WorkRequest.
Populating WorkRequest...
Matching 'Client':- Matched to 'SEAOAU' by code, address 'PST: 1299 BOUNDARY ROAD' with a score of 165.
Added Customer Service Ticket - Create Documentation from UniversalActivity.
Successfully saved Customer Service Ticket - CST00000001 - Create Documentation.
".Trim(), message.GetLogNoteText());
		}

		public void TestProcess_WithNoDataTarget_ShouldDiscardMessage()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CustomerServiceTicket</Type>
          <Key>CST0000001</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <Summary>First Import test</Summary>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
No Module used this Universal Activity data.
".Trim(), serviceTaskLog.ToString());

			AssertMultilineASCIIEquals("Message Log Note", @"
No Module used this Universal Activity data.
Message Discarded.
".Trim(), message.GetLogNoteText());
		}

		public void TestProcessEvent()
		{
			var message = GetQueuedUniversalEventMessage(@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WorkItem</Type>
          <Key>WI00000231</Key>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>EVT</Code>
        <Description>Event</Description>
      </ActionPurpose>
      <Company>
        <Code>WTG</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>WiseTech Global</Name>
      </Company>
      <DataProvider>EDIDATWTG</DataProvider>
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
        <Code>Z69</Code>
        <Description></Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2019-03-14T13:45:45.063</TriggerDate>
      <TriggerDescription>Moose</TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2019-03-14T13:45:45.07</EventTime>
    <EventType>Z69</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
    </ContextCollection>
  </Event>
</UniversalEvent>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
".Trim(), serviceTaskLog.ToString());

			AssertMultilineASCIIEquals("Message Log Note", @"
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), message.GetLogNoteText());

			// Please note: tests where the import is successful can be found in Enterprise.ProcessManagement.DataTransfer.ActivityDataContextManagerTestCase
		}

		#region GrEngine

		public void TestGetKeysForBlockingParallelImport_SimpleJob()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomerServiceTicket</Type>
          <Key>CST12345678</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("The only key in the message was the top-level job number, so that should be the only result. SAD!", new[] { "CST12345678" }, keys);
		}

		public void TestGetKeysForBlockingParallelImport_SimpleJob_NoTargetKeySpecified()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomerServiceTicket</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>New Job</Summary>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("No keys were included (this message will add a new job), so no keys should be returned. SAD!", System.Array.Empty<string>(), keys);
		}

		public void TestGetKeysForBlockingParallelImport_JobWithRelatedItems_WithTargetKeys()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>Project</Type>
          <Key>PRJ0000011</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>

    <RelatedActivityCollection>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
              <Key>WKI00000001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 1</Summary>
      </RelatedActivity>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
              <Key>WKI00000002</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 2</Summary>
      </RelatedActivity>

    </RelatedActivityCollection>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("We should get keys for the top-level job AND all its existing related jobs. SAD!", new[] { "PRJ0000011", "WKI00000001", "WKI00000002" }, keys);
		}

		public void TestGetKeysForBlockingParallelImport_NewJobWithRelatedItems_WithTargetKeys()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomerServiceTicket</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>

    <RelatedActivityCollection>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
              <Key>WKI00000001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 1</Summary>
      </RelatedActivity>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
              <Key>WKI00000002</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 2</Summary>
      </RelatedActivity>

    </RelatedActivityCollection>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("The top-level job is being created as a new job, but the system will link it to existing jobs, so the related jobs' keys should be returned.. SAD!", new[] { "WKI00000001", "WKI00000002" }, keys);
		}

		public void TestGetKeysForBlockingParallelImport_JobWithRelatedItems_WithNoTargetKeysSpecified()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>Project</Type>
          <Key>PRJ0000011</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>

    <RelatedActivityCollection>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 1</Summary>
      </RelatedActivity>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 2</Summary>
      </RelatedActivity>

    </RelatedActivityCollection>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("The top-level job already exists but the related items will be new jobs, so only the top-level key should be returned. SAD!", new[] { "PRJ0000011" }, keys);
		}

		public void TestGetKeysForBlockingParallelImport_NewJobWithRelatedItems_WithNoTargetKeysSpecified()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>Project</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>

    <RelatedActivityCollection>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 1</Summary>
      </RelatedActivity>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 2</Summary>
      </RelatedActivity>

    </RelatedActivityCollection>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("The top-level job AND related jobs are all new, so no keys should be returned. SAD!", System.Array.Empty<string>(), keys);
		}

		public void TestGetKeysForBlockingParallelImport_JobWithRelatedItems_MixOfNewAndExistingRelatedJobs()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>Project</Type>
          <Key>PRJ0000011</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>

    <RelatedActivityCollection>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 1</Summary>
      </RelatedActivity>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
              <Key>WKI00000002</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 2</Summary>
      </RelatedActivity>

    </RelatedActivityCollection>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("There's a mix of new and existing related jobs, so the ones with keys should return those keys. SAD!", new[] { "PRJ0000011", "WKI00000002" }, keys);
		}

		public void TestGetKeysForBlockingParallelImport_JobWithRelatedItems_MultipleNestedLevels()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>Project</Type>
          <Key>PRJ0000011</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>

    <RelatedActivityCollection>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
              <Key>WKI00000001</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 1</Summary>

        <RelatedActivityCollection>

          <RelatedActivity>
            <DataContext>
              <DataTargetCollection>
                <DataTarget>
                  <Type>CustomerServiceTicket</Type>
                  <Key>CST0000001</Key>
                </DataTarget>
              </DataTargetCollection>
            </DataContext>
            <Summary>Sub-related item of a related item 1</Summary>

          </RelatedActivity>

          <RelatedActivity>
            <DataContext>
              <DataTargetCollection>
                <DataTarget>
                  <Type>CustomerServiceTicket</Type>
                  <Key>CST0000002</Key>
                </DataTarget>
              </DataTargetCollection>
            </DataContext>
            <Summary>Sub-related item of a related item 2</Summary>
          </RelatedActivity>

          <RelatedActivity>
            <DataContext>
              <DataTargetCollection>
                <DataTarget>
                  <Type>CustomerServiceTicket</Type>
                </DataTarget>
              </DataTargetCollection>
            </DataContext>
            <Summary>NEW Sub-related item of a related item</Summary>
          </RelatedActivity>

        </RelatedActivityCollection>
      </RelatedActivity>

      <RelatedActivity>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>WorkItem</Type>
              <Key>WKI00000002</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <Summary>Existing related item 2</Summary>
      </RelatedActivity>

    </RelatedActivityCollection>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("We should get keys for the top-level job AND all its existing related jobs. SAD!", new[] { "PRJ0000011", "WKI00000001", "CST0000001", "CST0000002", "WKI00000002" }, keys);
		}

		public void TestGetKeysForBlockingParallelImport_WithMultipleDataTargets_ShouldReturnKeyForAllTargets()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WorkItem</Type>
          <Key>WKI0000001</Key>
        </DataTarget>
        <DataTarget>
          <Type>Project</Type>
          <Key>PRJ0000011</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("Only the first target is going to be used anyway, so that's the only job number we should return. SAD!", new[] { "WKI0000001", "PRJ0000011" }, keys);
		}

		public void TestGetKeysForBlockingParallelImport_ShouldIgnoreBlankKeys()
		{
			var message = GetQueuedUniversalActivityMessage(@"
<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WorkItem</Type>
          <Key></Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Standalone Job</Summary>
  </Activity>
</UniversalActivity>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var keys = manager.GetKeysForBlockingParallelImport(message).Keys;

			AssertContainsExactElementsInAnyOrder("There was a Key element but it was empty, so it should be omitted from the list of keys. SAD!", System.Array.Empty<string>(), keys);
		}

		#endregion
	}
}
