using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM451;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM451Processor))]
	class IM451ProcessorTest : EntryHeaderMessageProcessorTest<IM451Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM451Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM451;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => Serialize(new Im451
		{
			ImportOperation = new MCciOperationType48
			{
				Mrn = "12MRN345ABCDE678R9",
				Lrn = "LRN001",
				DeclarationType = "CO",
				AdditionalDeclarationType = "A",
				DecisionDate = new DateTime(2023, 08, 11, 14, 30, 45),
				DecisionReason = "Decision Reason",
				PreferredPaymentMethod = "A",
				Remarks = "Remarks001",
			},
			Authorisation = new Collection<MAuthorisationType01>(),
			CustomsOfficeOfPresentation = new MPcoType { ReferenceNumber = "PCO12345" },
			Importer = new MImporterType2 { },
			SupervisingCustomsOffice = new MScoType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new MScoType { ReferenceNumber = "LCO12345" },
			Declarant = new MDeclarantType { IdentificationNumber = "DEC001" },
			PersonProvidingAGuarantee = new MPersonProvidingGuaranteeType { IdentificationNumber = "PPAGID" },
			PersonPayingCustomsDuty = new MPersonPayingCustomsDutyType { IdentificationNumber = "PPCDID" },
			Representative = new MRepresentativeType { IdentificationNumber = "REP001", Status = "0" },
			Guarantee = new Collection<MGuaranteeType> { },
			CurrencyExchange = new MCurrencyExchangeType { },
			DeferredPayment = new Collection<MDeferredPaymentType>
			{
				new MDeferredPaymentType { SequenceNumber = "1", DeferredPayment = "DeferredPayment001" }
			},
			GoodsShipment = new Collection<MGoodsShipmentType07>
			{
				new MGoodsShipmentType07
				{
					SequenceNumber = "1",
					NatureOfTransaction = "1",
					TotalAmountInvoiced = 1000m,
					InvoiceCurrency = Core.Constants.CurrencyCodes.EuropeanUnion,
					DateOfAcceptance = new DateTime(2023, 08, 10, 14, 30, 45),
					ExchangeRate = 1m,
					AdditionalSupplyChainActor = new Collection<MAdditionalSupplyChainActorType> { },
					Buyer = new MBuyerType { },
					Seller = new MSellerType { },
					Exporter = new MExporterType { },
					DeliveryTerms = new MDeliveryTermsType { IncotermCode = "CFR" },
					CountryOfDispatch = new MCountryOfDispatchType { CountryOfDispatch = Core.Constants.CountryCodes.Ireland },
					Destination = new MDestinationType { },
					Warehouse = new MWarehouseType { Type = "R", Identifier = "WH001" },
					PreviousDocument = new Collection<MPreviousDocumentType02> { },
					SupportingDocument = new Collection<MSupportingDocumentType02> { },
					AdditionalReference = new Collection<MAdditionalReferenceType> { },
					AdditionalInformation = new Collection<MAdditionalInformationType> { },
					AdditionsAndDeductions = new Collection<MAdditionsAndDeductionsType> { },
					AdditionalFiscalReference = new Collection<MAdditionalFiscalReferenceType> { },
					PostalCharges = new PostalChargesType { },
					Consignment = new MConsignmentType04 { ContainerIndicator = "1" },
					GoodsShipmentItem = new Collection<MGoodsShipmentItemType01>
					{
						new MGoodsShipmentItemType01
						{
							SequenceNumber = "1",
							DeclarationGoodsItemNumber = "10001",
							StatisticalValue = 9000m,
							NatureOfTransaction = "12",
							ReferenceNumberUcr = "GSIRNUCR001",
							DateOfAcceptance = new DateTime(2023, 08, 10, 14, 31, 45),
							Authorisation = new Collection<MAuthorisationType02> { },
							Procedure = new MProcedureType01 { RequestedProcedure = "01", PreviousProcedure = "00" },
							AdditionalSupplyChainActor = new Collection<MAdditionalSupplyChainActorType> { },
							Buyer = new MBuyerType { },
							Seller = new MSellerType { },
							Exporter = new MExporterType { },
							Taxes = new TaxesItemType { },
							Origin = new MOriginType { },
							CountryOfDispatch = new MCountryOfDispatchType { CountryOfDispatch = Core.Constants.CountryCodes.Ireland },
							Destination = new MDestinationType { },
							Commodity = new MCommodityType01
							{
								DescriptionOfGoods = "GoodsShipmentItem Commodity DescriptionOfGoods 1",
								GoodsMeasure = new MGoodsMeasureType01 { }
							},
							Packaging = new Collection<MPackagingType01>
							{
								new MPackagingType01 { SequenceNumber = "1", TypeOfPackages = "1F" },
							},
						}
					},
				}
			},
			ControlResult = new MControlResultType01
			{
				Code = "A4",
				Date = new DateTime(2023, 08, 11, 14, 30, 45),
				Remarks = "Remarks001",
				PendingSamplingResults = "0",
			},
			ControlResults = new Collection<MItemControlResultsType01>
			{
				new MItemControlResultsType01
				{
					SequenceNumber = "1", DeclarationGoodsItemNumber = "10001", ControlResultCode = "A4",
					ResultsOfControl = new Collection<MControlResultType02>
					{
						new MControlResultType02
						{
							SequenceNumber = "1", RiskAreaCode = "100000", ControlType = "Red", ControlDate = new DateTime(2023, 08, 09, 14, 30, 45), Remarks = "Control Results Remarks 011",
							ControlDetails = new Collection<MControlDetailsType>
							{
								new MControlDetailsType { SequenceNumber = "1", TypeOfDiscrepancies = "TD", AttributePointer = "Attribute Pointer 111", CorrectedValue = "Corrected Value 111", Remarks = "Control Details Remarks 111" },
								new MControlDetailsType { SequenceNumber = "2", TypeOfDiscrepancies = "TE", AttributePointer = "Attribute Pointer 112", CorrectedValue = "Corrected Value 112", Remarks = "Control Details Remarks 112" },
							}
						},
						new MControlResultType02
						{
							SequenceNumber = "2", RiskAreaCode = "100000", ControlType = "Red", ControlDate = new DateTime(2023, 08, 10, 14, 30, 45), Remarks = "Control Results Remarks 012",
							ControlDetails = new Collection<MControlDetailsType>
							{
								new MControlDetailsType { SequenceNumber = "1", TypeOfDiscrepancies = "TF", AttributePointer = "Attribute Pointer 121", CorrectedValue = "Corrected Value 111", Remarks = "Control Details Remarks 121" },
								new MControlDetailsType { SequenceNumber = "2", TypeOfDiscrepancies = "TG", AttributePointer = "Attribute Pointer 122", CorrectedValue = "Corrected Value 112", Remarks = "Control Details Remarks 122" },
							}
						},
					}
				},
				new MItemControlResultsType01
				{
					SequenceNumber = "2", DeclarationGoodsItemNumber = "20001", ControlResultCode = "A4",
					ResultsOfControl = new Collection<MControlResultType02>
					{
						new MControlResultType02
						{
							SequenceNumber = "1", RiskAreaCode = "100000", ControlType = "Red", ControlDate = new DateTime(2023, 08, 11, 14, 30, 45), Remarks = "Control Results Remarks 021",
							ControlDetails = new Collection<MControlDetailsType>
							{
								new MControlDetailsType { SequenceNumber = "1", TypeOfDiscrepancies = "TH", AttributePointer = "Attribute Pointer 211", CorrectedValue = "Corrected Value 211", Remarks = "Control Details Remarks 211" },
							}
						}
					}
				},
			},
		});

		protected override ZString MessageFriendlyName => "IM451: No Release";

		protected override IM451Processor Processor => new IM451Processor(logger, typeof(Im451));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL716", "Control Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL716", "Red", "Physical Control - R", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType("CL740", "Risk Area Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL740", "100000", "Cash control", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			return base.CreateSetupData();
		}

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.NotReleased, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A No Release (IM451) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Declaration Type</td><td>CO</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Decision Date</td><td>11-Aug-23</td></tr><tr><td>Decision Reason</td><td>Decision Reason</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Results</td><td>1</td></tr><tr><td>-Declaration Goods Item Number</td><td>10001</td></tr><tr><td>-Control Result Code</td><td>A4</td></tr><tr><td>-Results of Control</td><td>1</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>09-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 011</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TD</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 111</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 111</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 111</td></tr><tr><td>--Control Details</td><td>2</td></tr><tr><td>---Type of Discrepancies</td><td>TE</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 112</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 112</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 112</td></tr><tr><td>-Results of Control</td><td>2</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>10-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 012</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TF</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 121</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 111</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 121</td></tr><tr><td>--Control Details</td><td>2</td></tr><tr><td>---Type of Discrepancies</td><td>TG</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 122</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 112</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 122</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Results</td><td>2</td></tr><tr><td>-Declaration Goods Item Number</td><td>20001</td></tr><tr><td>-Control Result Code</td><td>A4</td></tr><tr><td>-Results of Control</td><td>1</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>11-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 021</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TH</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 211</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 211</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 211</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A No Release (IM451) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		public void TestEntryStatus()
		{
			var incomingMessage = CreateNewIncomingMessage();

			var messageAttacheeMock = new Mock<IAISMessageAttachee> { CallBase = true };
			messageAttacheeMock.As<Integration.Customs.IEH7.IAsycudaBill>().SetupGet(s => s.ABL_BillStatus)
				.Returns("ACC");
			var status = Processor.GetLogicalStatus(incomingMessage, messageAttacheeMock.Object,
				Processor.GetDataProvider(incomingMessage));
			AssertEquals("Accepted", "ACC", status);

			var messageAttacheeMock1 = new Mock<IAISMessageAttachee> { CallBase = true };
			messageAttacheeMock1.As<Integration.Customs.IE.ICusEntryHeader>().SetupGet(s => s.CH_EntryStatus).Returns("ACC");
			status = Processor.GetLogicalStatus(incomingMessage, messageAttacheeMock1.Object,
				Processor.GetDataProvider(incomingMessage));
			AssertNull(nameof(status), status);
		}
	}
}
