using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	[TestedType(typeof(AsycudaUniversalEventMessageFailureProcessor))]
	sealed class AsycudaUniversalEventMessageFailureProcessorTest : AsycudaUniversalEventMessageProcessorTest
	{
		public void TestGetGroupToSendCore()
		{
			using (ManifestCustomsDataRegistry.Instance.GroupToSendErrorNotification.SetTemporaryValue(Guid.Empty, Env.CurrentCompany.PK, Guid.Empty, newGroup.PK.ToGuid()))
			{
				AssertEquals(newGroup, Processor.GetGroupToSend());
			}
		}

		public void TestSendToGroup()
		{
			ManifestCustomsDataRegistry.Instance.SendErrorNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(Processor.SendToGroupForTest());

			ManifestCustomsDataRegistry.Instance.SendErrorNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.NominatedGroup);
			Assert(Processor.SendToGroupForTest());

			ManifestCustomsDataRegistry.Instance.SendErrorNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.NoEmails);
			Assert(!Processor.SendToGroupForTest());
		}

		public void TestSendToStaff()
		{
			ManifestCustomsDataRegistry.Instance.SendErrorNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(Processor.SendToStaffForTest());

			ManifestCustomsDataRegistry.Instance.SendErrorNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.NominatedGroup);
			Assert(!Processor.SendToStaffForTest());

			ManifestCustomsDataRegistry.Instance.SendErrorNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.NoEmails);
			Assert(!Processor.SendToStaffForTest());
		}

		public void TestBillLogs()
		{
			// Add a user description
			const string userCommentaryText = "USER GENERATED MESSAGE";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GlobalManifestErrorCommentary, "Global Manifest Error Commentary");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore,
																Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GlobalManifestErrorCommentary,
																"R01", userCommentaryText, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			header.AMA_ManifestType = "MGI";
			var existingBill = header.Bills.AddNew();
			existingBill.ABL_BillNumber = "48736874";
			Factory.Save();

			message.EM_MessageText = SGErrorMessageText;
			var errEvent = message.GetEM_MessageTextReader().Parse<Event>();
			header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(logger, errEvent, message, header).Process();

			ZString errLog = "ERR - ERR  - R01 USER GENERATED MESSAGE - N14 HSCODE IS MANDATORY,PLEASE CHECK CIF/FOB AND GOODS TYPE/FLIGHT NO";
			AssertNotNull(existingBill.Logs.Find(log => log.ReferenceFreeText == errLog).FirstOrDefault());

			var logCount = existingBill.Logs.DatabaseCount;

			message.EM_MessageText = NonSGAMessageText;
			var nonSGAEvent = message.GetEM_MessageTextReader().Parse<Event>();
			new AsycudaUniversalEventMessageFailureProcessor(logger, nonSGAEvent, message, header).Process();

			AssertEquals("No new log should be added to Bill", logCount, existingBill.Logs.DatabaseCount);
		}

		public void TestBillLogs_MalformedXML()
		{
			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			var existingBill = header.Bills.AddNew();
			existingBill.ABL_BillNumber = "48736874";
			Factory.Save();

			message.EM_MessageText = SGErrorMessageTextMalformed;
			var errEvent = message.GetEM_MessageTextReader().Parse<Event>();
			header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(logger, errEvent, message, header).Process();

			CombineAssertions(() =>
			{
				AssertNotNull("Empty", existingBill.Logs.Find(log => log.ReferenceFreeText == " - ERR ").FirstOrDefault());
				AssertNotNull("Complete", existingBill.Logs.Find(log => log.ReferenceFreeText == "ERR - R01 COUNTRY CODE/ORIGIN OF GOODS IS INVALID").FirstOrDefault());
			});
		}

		public void TestUpdateSGBillAndPacks_MGI()
		{
			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "48736874";
			bill.ABL_BillStatus = "IP";
			var billCusEntryNumber = bill.CustomsEntryNumbers.AddNew();
			billCusEntryNumber.CE_EntryType = "ASY";
			billCusEntryNumber.CE_EntryNum = "E1";
			bill.Logs.AddNew(AutoEvents.StatusChange, "IP", ZDateTimeOffset.Now);

			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 1;
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_PackStatus = "CR";
			pack.Logs.AddNew(AutoEvents.StatusChange, "CR", ZDateTimeOffset.Now);
			var packCusEntryNumber = packedItem.CustomsEntryNumbers.AddNew();
			packCusEntryNumber.CE_EntryType = "ASY";
			packCusEntryNumber.CE_EntryNum = "A1";

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("E1", bill.RegistrationEntryNumber.CE_EntryNum);
				Assert(bill.RegistrationEntryNumber.IsInDatabase);

				AssertEquals("A1", packedItem.RegistrationEntryNumber.CE_EntryNum);
				Assert(packedItem.RegistrationEntryNumber.IsInDatabase);
			});

			message.EM_MessageText = SGErrorMessageText_MGI;
			var errEvent = message.GetEM_MessageTextReader().Parse<Event>();
			header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(logger, errEvent, message, header).Process();

			CombineAssertions(() =>
			{
				AssertNull(bill.RegistrationEntryNumber);
				AssertEquals(string.Empty, bill.ABL_BillStatus);
				AssertNull(packedItem.RegistrationEntryNumber);
				AssertEquals(string.Empty, packedItem.API_PackStatus);
				AssertNull("STC event has been cancled", bill.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange));
				AssertNull("STC event has been cancled", packedItem.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange));
			});
		}

		string SGErrorMessageText => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
					<Key>MAN0000130</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-04-01T01:13:24</EventTime>
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
				<Value>11462</Value>
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
				<Value>20180401</Value>
			</Context>
			<Context>
				<Type>OriginalSerialNumber</Type>
				<Value>7113</Value>
			</Context>
			<Context>
				<Type>MasterBill</Type>
				<Value>66755757577</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>48736874</Value>
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
								<Type>MessageStatusCode</Type>
								<Value>ERR</Value>
							</SubContext>
						</SubContextCollection>
					</SubContext>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>48736874</Value>
						<SubContextCollection>
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
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>DUPLICATE SHOULD NOT BE LOGGED</Value>
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
						<Value>48736874</Value>
						<SubContextCollection>
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
										<Value>1</Value>
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
						<Type>HouseBill</Type>
						<Value>48736874</Value>
						<SubContextCollection>
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
										<Value>N14</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>HSCODE IS MANDATORY,PLEASE CHECK CIF/FOB AND GOODS TYPE/FLIGHT NO</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentGroup</Type>
										<Value>3</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber1</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
									</SubContext>
									<SubContext>
										<Type>OrdinalNumber1</Type>
										<Value>15</Value>
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

		string SGErrorMessageTextMalformed => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
					<Key>MAN0000130</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-04-01T01:13:24</EventTime>
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
				<Value>11462</Value>
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
				<Value>20180401</Value>
			</Context>
			<Context>
				<Type>OriginalSerialNumber</Type>
				<Value>7113</Value>
			</Context>
			<Context>
				<Type>MasterBill</Type>
				<Value>66755757577</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
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
								<Type>MessageStatusCode</Type>
								<Value>ERR</Value>
							</SubContext>
						</SubContextCollection>
					</SubContext>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>48736874</Value>
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
						</SubContextCollection>
					</SubContext>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>48736874</Value>
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
										<Value>1</Value>
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

		string SGErrorMessageText_MGI => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
					<Key>MAN0000130</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-04-01T01:13:24</EventTime>
		<EventType>MRR</EventType>
		<EventReference>MST=MGI</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>ManifestCountry</Type>
				<Value>SG</Value>
			</Context>
			<Context>
				<Type>OriginalInterchangeNumber</Type>
				<Value>11462</Value>
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
				<Value>20180401</Value>
			</Context>
			<Context>
				<Type>OriginalSerialNumber</Type>
				<Value>7113</Value>
			</Context>
			<Context>
				<Type>MasterBill</Type>
				<Value>66755757577</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>48736874</Value>
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
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>COUNTRY CODE/ORIGIN OF GOODS IS INVALID</Value>
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

		AsycudaUniversalEventMessageFailureProcessorForTest processor;
		AsycudaUniversalEventMessageFailureProcessorForTest Processor => processor ??= new AsycudaUniversalEventMessageFailureProcessorForTest(logger, universalEvent, message, header);
	}

	sealed class AsycudaUniversalEventMessageFailureProcessorForTest : AsycudaUniversalEventMessageFailureProcessor
	{
		public AsycudaUniversalEventMessageFailureProcessorForTest(IXmlSessionTracker logger, Event universalEvent, AsycudaEDIMessage message, AsycudaManifestHeader header)
			: base(logger, universalEvent, message, header)
		{
		}

		internal bool SendToGroupForTest() => SendToGroup();

		internal bool SendToStaffForTest() => SendToStaff();
	}
}
