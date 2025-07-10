using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaEDIMessage))]
	public class AsycudaEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestEM_MessageInterpretation()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			var mrrLog = header.Logs.AddNew(Events.MessageReceived);
			var message = Factory.New<AsycudaEDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.AddUniversalDataLink(mrrLog);
			var messageTextMFA = string.Format(messageText, "MFA");
			message.EM_MessageText = messageTextMFA;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			AssertEquals("EM_MessageInterpretation should be original EM_MessageText", messageTextMFA, message.EM_MessageInterpretation);
			AssertNostmNote(message);

			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals("EM_MessageInterpretation should be original EM_MessageText", messageTextMFA, message.EM_MessageInterpretation);
			AssertNostmNote(message);

			var messageTextAEP = string.Format(messageText, "AEP");
			message.EM_MessageText = messageTextAEP;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			header = factory.Load<AsycudaManifestHeader>(header.PK);
			message = (AsycudaEDIMessage)header.Messages.FindByPK(message.PK);
			AssertEquals(false, header.HasChanges);
			AssertEquals(false, message.HasChanges);
			AssertEquals(0, message.Notes.FindByDescription(PredefinedNoteTypes.Instance.MessageInterpretation.Description).Length);
			AssertMultilineASCIIEquals("EM_MessageInterpretation", expectedHTML, message.EM_MessageInterpretation);
			var notes = message.Notes.FindByDescription(PredefinedNoteTypes.Instance.MessageInterpretation.Description);
			AssertEquals(1, notes.Length);
			var stmNote = notes[0];
			AssertMultilineASCIIEquals("ST_NoteText", expectedHTML, stmNote.ST_NoteText);
			AssertEquals(true, stmNote.HasChanges);
			AssertEquals(true, header.HasChanges);
			AssertEquals(true, message.HasChanges);
		}

		public virtual void TestEM_MessageInterpretation_ErrorDescription()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var mrrLog = header.Logs.AddNew(Events.MessageReceived);
			var message = Factory.New<AsycudaEDIMessage>();
			message.AddUniversalDataLink(mrrLog);
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageText = MessageTextErr;

			AssertMultilineASCIIEquals("EM_MessageInterpretation", MessageTextErr, message.EM_MessageInterpretation);

			var query = new ZQuery(StmNoteSchema.ST_ParentID, message.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, EDIMessageSchema.Constants.TableName);
			query.AddToFilter(StmNoteSchema.ST_Description, "Message Interpretation");
			var stmNote = Factory.LoadTop1<StmNote>(query);
			AssertNull(stmNote);
		}

		public virtual void TestEM_MessageInterpretation_ErrorDescription_WithCommentary()
		{
			const string ErrorCommentaryText = "THIS ERROR INDICATES THAT THE COUNTRY CODE OR THE ORIGIN OF GOODS IS INVALID.  PLEASE CHECK CIF/FOB AND GOODS TYPE/FLIGHT NO";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GlobalManifestErrorCommentary, "Global Manifest Error Commentary");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore,
																Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GlobalManifestErrorCommentary,
																"R01", ErrorCommentaryText, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var mrrLog = header.Logs.AddNew(Events.MessageReceived);
			var message = Factory.New<AsycudaEDIMessage>();
			message.AddUniversalDataLink(mrrLog);
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageText = MessageTextErr;
			AssertNotContains("", ErrorCommentaryText, message.EM_MessageInterpretation);

			AssertMultilineASCIIEquals("EM_MessageInterpretation", MessageTextErr, message.EM_MessageInterpretation);

			var query = new ZQuery(StmNoteSchema.ST_ParentID, message.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, EDIMessageSchema.Constants.TableName);
			query.AddToFilter(StmNoteSchema.ST_Description, "Message Interpretation");
			var stmNote = Factory.LoadTop1<StmNote>(query);
			AssertNull(stmNote);
		}

		static string MessageTextErr => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""AIRERR SG ACCESS"">ERR</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>MAN0000003</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-18T22:49:00</EventTime>
        <EventType>MRR</EventType>
        <EventReference>MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>OriginalInterchangeNumber</Type>
            <Value>6183</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRAED</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20180118</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>2125</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>6726767267</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>BADBILL</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00001</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>2</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00002</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>3</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00003</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>R01</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>COUNTRY CODE/ORIGIN OF GOODS IS INVALID</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentGroup</Type>
                        <Value>2</Value>
                      </SubContext>
                      <SubContext>
                        <Type>GroupOccuranceNumber1</Type>
                        <Value>2</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>CTY</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>14</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>4</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00004</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>5</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00005</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>BADBILL</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00001</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>2</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00002</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>3</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00003</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>4</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00004</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>5</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00005</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>R01</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>COUNTRY CODE/ORIGIN OF GOODS IS INVALID</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentGroup</Type>
                        <Value>2</Value>
                      </SubContext>
                      <SubContext>
                        <Type>GroupOccuranceNumber1</Type>
                        <Value>4</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>CTY</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>14</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>ERR</Value>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";

		protected void AssertNostmNote(AsycudaEDIMessage message)
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, message.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, EDIMessageSchema.Constants.TableName);
			query.AddToFilter(StmNoteSchema.ST_Description, "Message Interpretation");

			var stmNote = Factory.LoadTop1<StmNote>(query);
			AssertNull(stmNote);
		}

		string messageText => @"
<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""AIRAEP SG ACCESS"">{0}</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>MAN0000002</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2017-10-10T22:53:05</EventTime>
        <EventType>MRR</EventType>
        <EventReference>|MSB=ORG|MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>ResponseNumber</Type>
            <Value>01</Value>
          </Context>
          <Context>
            <Type>ResponseTime</Type>
            <Value>20171011064231</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20171010</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>0068</Value>
          </Context>
          <Context>
            <Type>BatchDate</Type>
            <Value>20171010</Value>
          </Context>
          <Context>
            <Type>BatchNumber</Type>
            <Value>0040</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>8434767687</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>HJFGKSD</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00001</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>REG</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ConsignmentStatus</Type>
                        <Value>CR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ManifestPermitNumber</Type>
                        <Value>EUPS17J110001</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>2</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>00002</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>REG</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ConsignmentStatus</Type>
                        <Value>CR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ManifestPermitNumber</Type>
                        <Value>EUPS17J110001</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>REG</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>CR</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ManifestPermitNumber</Type>
                    <Value>EUPS17J110001</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>REG</Value>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
		string expectedHTML => @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td colspan=""7"" font-weight=""bold"" align=""center"">Response Details</td></tr><tr><td colspan=""7"" align=""center"">A response message has been received from Customs.<br />The message type is:  AEP – AIRAEP SG ACCESS</td></tr><tr><td colspan=""2"" font-weight=""bold"">Job Number</td><td colspan=""5"">MAN0000002</td></tr><tr><td colspan=""2"" font-weight=""bold"">Manifest Country</td><td colspan=""5"">SG</td></tr><tr><td colspan=""2"" font-weight=""bold"">Response Number</td><td colspan=""5"">01</td></tr><tr><td colspan=""2"" font-weight=""bold"">Response Time</td><td colspan=""5"">20171011064231</td></tr><tr><td colspan=""2"" font-weight=""bold"">UEN Number</td><td colspan=""5"">198801949D</td></tr><tr><td colspan=""2"" font-weight=""bold"">Original Create Date</td><td colspan=""5"">20171010</td></tr><tr><td colspan=""2"" font-weight=""bold"">Original Serial Number</td><td colspan=""5"">0068</td></tr><tr><td colspan=""2"" font-weight=""bold"">Batch Date</td><td colspan=""5"">20171010</td></tr><tr><td colspan=""2"" font-weight=""bold"">Master Bill</td><td colspan=""3"">8434767687</td><td colspan=""2"">REG</td></tr><tr><td colspan=""2"" font-weight=""bold"">House Bill</td><td>HJFGKSD</td><td>REG</td><td>CR</td><td colspan=""2"">EUPS17J110001</td></tr><tr><td colspan=""2"" font-weight=""bold"">Consignment</td><td>00001</td><td>REG</td><td>CR</td><td colspan=""2"">EUPS17J110001</td></tr><tr><td colspan=""2"" font-weight=""bold"">Consignment</td><td>00002</td><td>REG</td><td>CR</td><td colspan=""2"">EUPS17J110001</td></tr></table>";
	}
}
