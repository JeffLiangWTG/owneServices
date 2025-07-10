using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaWriterTest
	{
		public void TestCreateCustomsDeclaration()
		{
			var bill = CreateBillForExportBillForDeclaration(Core.Constants.CountryCodes.Singapore, "SGVLI", "MGI", Core.Constants.TransportModes.Air);
			AssertEquals("bill.ABL_ShipmentType", ShipmentTypeList.Codes.Import23, bill.ABL_ShipmentType);
			AssertEquals("bill.CustomsJobNumber", ZString.Empty, bill.CustomsJobNumber);
			Factory.SaveForTesting();
			var declaration = bill.CreateCustomsDeclaration();
			AssertEquals("bill.CustomsJobNumber", declaration.JE_DeclarationReference, bill.CustomsJobNumber);
			var dataImportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataImport);
			var relatedEDIMessage = dataImportLog.RelatedEDIMessage;
			var message = relatedEDIMessage.Message;
			var shipment = message.GetEM_MessageTextReader().Parse<Shipment>();
			AssertExportBillForDeclaration(shipment, MessageTypeCodeList_Codes_INP, DeclarationTypeCodeList_Codes_SFZ, "HAWB1234", WayBillTypeList.Codes.House, CargoPackingCodeList_Codes_PackingType5, 0, Core.Constants.TransportModes.Air, null, "BT123", "", "BA123", 4);
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);

			var tmcLog = declaration.Logs.MostRecentLogByEventTime(Events.TransferFromManifestToCustoms);
			AssertNotNullOrEmpty("pre-condition", bill.Header.AMA_JobReference);
			AssertEquals(bill.Header.AMA_JobReference, tmcLog.SL_Reference);

			bill.ABL_BillNumber = "UPDATE123";
			Factory.SaveForTesting();
			AssertEquals("bill.ABL_BillNumber", "UPDATE123", bill.ABL_BillNumber);
			AssertEquals("bill.CustomsJobNumber", declaration.JE_DeclarationReference, bill.CustomsJobNumber);
			var declarationUpdate = bill.CreateCustomsDeclaration();
			AssertEquals("bill.CustomsJobNumber", declarationUpdate.JE_DeclarationReference, bill.CustomsJobNumber);
			AssertEquals("Update declaration", "UPDATE123", declarationUpdate.Invoices[0].JZ_InvoiceNumber);
		}

		public void TestCreateCustomsDeclarationForRoadTransport()
		{
			var bill = CreateBillForExportBillForDeclaration(Core.Constants.CountryCodes.Singapore, "SGVLI", "MGI", Core.Constants.TransportModes.Road);
			AssertEquals("bill.ABL_ShipmentType", ShipmentTypeList.Codes.Import23, bill.ABL_ShipmentType);
			AssertEquals("bill.CustomsJobNumber", ZString.Empty, bill.CustomsJobNumber);
			Factory.SaveForTesting();
			var declaration = bill.CreateCustomsDeclaration();
			AssertEquals("bill.CustomsJobNumber", declaration.JE_DeclarationReference, bill.CustomsJobNumber);
			var dataImportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataImport);
			var relatedEDIMessage = dataImportLog.RelatedEDIMessage;
			var message = relatedEDIMessage.Message;
			var shipment = message.GetEM_MessageTextReader().Parse<Shipment>();
			AssertExportBillForDeclaration(shipment, MessageTypeCodeList_Codes_INP, DeclarationTypeCodeList_Codes_SFZ, "HAWB1234", WayBillTypeList.Codes.House, CargoPackingCodeList_Codes_PackingType5, 0, Core.Constants.TransportModes.Road, null, "BT123", "", "YKK729", 4);
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);

			var tmcLog = declaration.Logs.MostRecentLogByEventTime(Events.TransferFromManifestToCustoms);
			AssertNotNullOrEmpty("pre-condition", bill.Header.AMA_JobReference);
			AssertEquals(bill.Header.AMA_JobReference, tmcLog.SL_Reference);
			AssertEquals("pre-condition", bill.Header.AMA_VehicleRegistration, "YKK729");
			AssertEquals("For Road Transport job, the vehicle registration number should be loaded into the declaration VoyageFlightNo", bill.Header.AMA_VehicleRegistration, declaration.JE_VoyageFlightNo);

			bill.ABL_BillNumber = "UPDATE123";
			Factory.SaveForTesting();
			AssertEquals("bill.ABL_BillNumber", "UPDATE123", bill.ABL_BillNumber);
			AssertEquals("bill.CustomsJobNumber", declaration.JE_DeclarationReference, bill.CustomsJobNumber);
			var declarationUpdate = bill.CreateCustomsDeclaration();
			AssertEquals("bill.CustomsJobNumber", declarationUpdate.JE_DeclarationReference, bill.CustomsJobNumber);
			AssertEquals("Update declaration", "UPDATE123", declarationUpdate.Invoices[0].JZ_InvoiceNumber);
		}

		public void TestGetMostSevereValueMessageStatus_Bill()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();

			var bill = header.Bills.AddNew();

			var pack1 = bill.Packs.AddNew();
			pack1.ConsignmentReference = 1;
			var pack1PackedItem = pack1.PackedItemForTesting();
			pack1PackedItem.API_MessageStatus = MessageStatusCodeList.Codes.Unknown;

			var messagingProvider = header.ApplicationBusinessProvider.MessagingProvider;
			AssertEquals("UNK", messagingProvider.GetMostSevereValueMessageStatus(bill));

			var pack2 = bill.Packs.AddNew();
			pack2.ConsignmentReference = 1;
			var pack2PackedItem = pack2.PackedItemForTesting();
			pack2PackedItem.API_MessageStatus = MessageStatusCodeList.Codes.Error;

			AssertEquals("ERR", messagingProvider.GetMostSevereValueMessageStatus(bill));
		}

		public void TestGetMostSevereValueCustomsStatus_Bill()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();

			var bill = header.Bills.AddNew();

			var pack1 = bill.Packs.AddNew();
			pack1.ConsignmentReference = 1;
			var packedItem1 = pack1.PackedItemForTesting();
			packedItem1.API_PackStatus = "CAN";

			var messagingProvider = header.ApplicationBusinessProvider.MessagingProvider;
			AssertEquals("CN", messagingProvider.GetMostSevereValueCustomsStatus(bill));

			var pack2 = bill.Packs.AddNew();
			pack2.ConsignmentReference = 1;
			var packedItem2 = pack2.PackedItemForTesting();
			packedItem2.API_PackStatus = Common.SG.GlobalManifestStatusList.Codes.InspectionRequired;

			AssertEquals("IP", messagingProvider.GetMostSevereValueCustomsStatus(bill));
		}

		public void TestIsSGAccessMessageEvent()
		{
			AssertIsSGAccessMessageEvent(NonSGAMessageText, false);
			AssertIsSGAccessMessageEvent(NonMRROrMRJMessageText, false);
			AssertIsSGAccessMessageEvent(NormalMessageText, true);
			AssertIsSGAccessMessageEvent(NormalMessageTextMRJ, true);
			AssertIsSGAccessMessageEvent(NormalMessageTextMDL, true);
		}

		void AssertIsSGAccessMessageEvent(string messageText, bool expectedValue)
		{
			var messagenonSGA = Factory.New<AsycudaEDIMessage>();
			messagenonSGA.EM_MessageText = messageText;
			messagenonSGA.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			messagenonSGA.EM_SystemCreateUser = "NS2";

			var universalEventnonSGA = messagenonSGA.GetEM_MessageTextReader().Parse<UniversalDataBuss.DataObjects.Universal.Event>();
			AssertEquals(expectedValue, universalEventnonSGA.IsSGAccessMessageEvent());
		}

		const string MessageTypeCodeList_Codes_INP = "INP";
		const string DeclarationTypeCodeList_Codes_SFZ = "SFZ";
		const string CargoPackingCodeList_Codes_PackingType5 = "5";

		string NormalMessageText => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
					<Key>MAN0000131</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-10-11T21:58:05</EventTime>
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
				<Value>3889</Value>
			</Context>
			<Context>
				<Type>MessageType</Type>
				<Value>AIRPCM</Value>
			</Context>
			<Context>
				<Type>UENNumber</Type>
				<Value>198801949D</Value>
			</Context>
			<Context>
				<Type>OriginalCreateDate</Type>
				<Value>20171011</Value>
			</Context>
			<Context>
				<Type>OriginalSerialNumber</Type>
				<Value>0150</Value>
			</Context>
			<Context>
				<Type>MasterBill</Type>
				<Value>5464564646</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
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
										<Value>2</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber1</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MEA</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>SNT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MOA</Value>
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

		string NonMRROrMRJMessageText => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
					<Key>MAN0000131</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-10-11T21:58:05</EventTime>
		<EventType>MRT</EventType>
		<EventReference>MST=MGI</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>ManifestCountry</Type>
				<Value>SG</Value>
			</Context>
			<Context>
				<Type>OriginalInterchangeNumber</Type>
				<Value>3889</Value>
			</Context>
			<Context>
				<Type>MessageType</Type>
				<Value>AIRPCM</Value>
			</Context>
			<Context>
				<Type>UENNumber</Type>
				<Value>198801949D</Value>
			</Context>
			<Context>
				<Type>OriginalCreateDate</Type>
				<Value>20171011</Value>
			</Context>
			<Context>
				<Type>OriginalSerialNumber</Type>
				<Value>0150</Value>
			</Context>
			<Context>
				<Type>MasterBill</Type>
				<Value>5464564646</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
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
										<Value>2</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber1</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MEA</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>SNT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MOA</Value>
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

		string NonSGAMessageText => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<Workflow>
				<ActionPurpose Description=""AIRERR SG ACCESS"">ERR</ActionPurpose>
			</Workflow>
			<DataSource>
				<DataProvider>DUM</DataProvider>
			</DataSource>
			<DataTargetCollection>
				<DataTarget>
					<Key>MAN0000131</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-10-11T21:58:05</EventTime>
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
				<Value>3889</Value>
			</Context>
			<Context>
				<Type>MessageType</Type>
				<Value>AIRPCM</Value>
			</Context>
			<Context>
				<Type>UENNumber</Type>
				<Value>198801949D</Value>
			</Context>
			<Context>
				<Type>OriginalCreateDate</Type>
				<Value>20171011</Value>
			</Context>
			<Context>
				<Type>OriginalSerialNumber</Type>
				<Value>0150</Value>
			</Context>
			<Context>
				<Type>MasterBill</Type>
				<Value>5464564646</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
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
										<Value>2</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber1</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MEA</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>SNT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MOA</Value>
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

		string NormalMessageTextMRJ => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
					<Key>MAN0000131</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-10-11T21:58:05</EventTime>
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
				<Value>3889</Value>
			</Context>
			<Context>
				<Type>MessageType</Type>
				<Value>AIRPCM</Value>
			</Context>
			<Context>
				<Type>UENNumber</Type>
				<Value>198801949D</Value>
			</Context>
			<Context>
				<Type>OriginalCreateDate</Type>
				<Value>20171011</Value>
			</Context>
			<Context>
				<Type>OriginalSerialNumber</Type>
				<Value>0150</Value>
			</Context>
			<Context>
				<Type>MasterBill</Type>
				<Value>5464564646</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
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
										<Value>2</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber1</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MEA</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>SNT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MOA</Value>
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

		string NormalMessageTextMDL => @"<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
					<Key>MAN0000131</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-10-11T21:58:05</EventTime>
		<EventType>MDL</EventType>
		<EventReference>MST=MGI</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>ManifestCountry</Type>
				<Value>SG</Value>
			</Context>
			<Context>
				<Type>OriginalInterchangeNumber</Type>
				<Value>3889</Value>
			</Context>
			<Context>
				<Type>MessageType</Type>
				<Value>AIRPCM</Value>
			</Context>
			<Context>
				<Type>UENNumber</Type>
				<Value>198801949D</Value>
			</Context>
			<Context>
				<Type>OriginalCreateDate</Type>
				<Value>20171011</Value>
			</Context>
			<Context>
				<Type>OriginalSerialNumber</Type>
				<Value>0150</Value>
			</Context>
			<Context>
				<Type>MasterBill</Type>
				<Value>5464564646</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>ERR</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>N13</Value>
									</SubContext>
									<SubContext>
										<Type>ErrorDescription</Type>
										<Value>SEGMENT MUST START WITH A DATA ELEMENT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>CTY</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
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
										<Value>2</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber1</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MEA</Value>
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
						<Value>FGHJFGHJ</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ConsignmentNumber</Type>
										<Value>02738</Value>
									</SubContext>
									<SubContext>
										<Type>MessageStatusCode</Type>
										<Value>SNT</Value>
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
										<Type>GroupOccuranceNumber2</Type>
										<Value>1</Value>
									</SubContext>
									<SubContext>
										<Type>SegmentTag</Type>
										<Value>MOA</Value>
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
	}
}
