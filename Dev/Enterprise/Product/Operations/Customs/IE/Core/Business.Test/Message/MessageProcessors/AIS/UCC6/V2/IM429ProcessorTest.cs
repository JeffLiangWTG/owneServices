using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM429;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM429Processor))]
	class IM429ProcessorTest : EntryHeaderMessageProcessorTest<IM429Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM429Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM429;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => Serialize(new Im429
		{
			ImportOperation = new MCciOperationType46
			{
				Lrn = "LRN001",
				Mrn = "12MRN345ABCDE678R9",
				DeclarationType = "IM",
				AdditionalDeclarationType = "A",
				DeclarationAcceptanceDate = new DateTime(2023, 08, 10, 14, 30, 45),
				ReleaseDate = new DateTime(2023, 08, 11, 14, 30, 45),
				ResponseDateLimit = new DateTime(2023, 08, 15, 14, 30, 45),
				PreferredPaymentMethod = "J",
				Remarks = "Remarks001",
			},
			Authorisation = new Collection<MAuthorisationType01> { },
			CustomsOfficeOfPresentation = new MPcoType { ReferenceNumber = "PCO12345" },
			SupervisingCustomsOffice = new MScoType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new MScoType { ReferenceNumber = "LCO12345" },
			Importer = new MImporterType2 { },
			Declarant = new MDeclarantType { IdentificationNumber = "DECLARANT" },
			PersonProvidingAGuarantee = new MPersonProvidingGuaranteeType { IdentificationNumber = "PPAGID" },
			PersonPayingCustomsDuty = new MPersonPayingCustomsDutyType { IdentificationNumber = "PPCDID" },
			Representative = new MRepresentativeType { IdentificationNumber = "REP001", Status = "0" },
			Guarantee = new Collection<MGuaranteeType> { },
			CurrencyExchange = new MCurrencyExchangeType { },
			DeferredPayment = new Collection<MDeferredPaymentType>
			{
				new MDeferredPaymentType { SequenceNumber = "1", DeferredPayment = "DeferredPayment001" }
			},
			GoodsShipment = new Collection<MGoodsShipmentType01>
			{
				new MGoodsShipmentType01
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
					PreviousDocument = new Collection<MPreviousDocumentType04> { },
					SupportingDocument = new Collection<MSupportingDocumentType02> { },
					AdditionalReference = new Collection<MAdditionalReferenceType> { },
					AdditionalInformation = new Collection<MAdditionalInformationType> { },
					AdditionsAndDeductions = new Collection<MAdditionsAndDeductionsType> { },
					AdditionalFiscalReference = new Collection<MAdditionalFiscalReferenceType> { },
					PostalCharges = new PostalChargesType { },
					Taxes = new Taxes02Type { },
					Consignment = new MConsignmentType04 { ContainerIndicator = "1" },
					GoodsShipmentItem = new Collection<MGoodsShipmentItemType01>
					{
						new MGoodsShipmentItemType01
						{
							SequenceNumber = "1",
							DeclarationGoodsItemNumber = "1",
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
							Taxes = new TaxesItemType
							{
								TaxBoxbis = new Collection<TaxBoxType>
								{
									new TaxBoxType { BoxTaxType = "A00", BoxTaxBaseUnit = "Q", BoxQuantity = 200m, BoxAmount = 0m, BoxTaxRate = 5m, BoxTaxPayableAmount = 10m, BoxTaxPaymentMethod = "A" },
									new TaxBoxType { BoxTaxType = "B00", BoxTaxBaseUnit = "A", BoxQuantity = 0m, BoxAmount = 100m, BoxTaxRate = 5m, BoxTaxPayableAmount = 5m, BoxTaxPaymentMethod = "B" },
								}
							},
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
						},
						new MGoodsShipmentItemType01
						{
							SequenceNumber = "2",
							DeclarationGoodsItemNumber = "2",
							StatisticalValue = 9000m,
							NatureOfTransaction = "12",
							ReferenceNumberUcr = "GSIRNUCR002",
							DateOfAcceptance = new DateTime(2023, 08, 10, 14, 31, 45),
							Authorisation = new Collection<MAuthorisationType02> { },
							Procedure = new MProcedureType01 { RequestedProcedure = "01", PreviousProcedure = "00" },
							AdditionalSupplyChainActor = new Collection<MAdditionalSupplyChainActorType> { },
							Buyer = new MBuyerType { },
							Seller = new MSellerType { },
							Exporter = new MExporterType { },
							Taxes = new TaxesItemType
							{
								TaxBoxbis = new Collection<TaxBoxType>
								{
									new TaxBoxType { BoxTaxType = "A00", BoxTaxBaseUnit = "Q", BoxQuantity = 300m, BoxAmount = 0m, BoxTaxRate = 1m, BoxTaxPayableAmount = 3m, BoxTaxPaymentMethod = "A" },
									new TaxBoxType { BoxTaxType = "B00", BoxTaxBaseUnit = "A", BoxQuantity = 0m, BoxAmount = 400m, BoxTaxRate = 2m, BoxTaxPayableAmount = 8m, BoxTaxPaymentMethod = "B" },
								}
							},
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
						},
					},
				}
			},
			ControlResult = new MControlResultType01
			{
				Code = "A1",
				Remarks = "Control Result Remarks",
				PendingSamplingResults = "1",
			},
		});

		protected override ZString MessageFriendlyName => "IM429: Release for Import";

		protected override IM429Processor Processor => new IM429Processor(logger, typeof(Im429));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Released, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Release for Import (IM429) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Declaration Type</td><td>IM</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Declaration Acceptance Date</td><td>10-Aug-23</td></tr><tr><td>Release Date</td><td>11-Aug-23</td></tr><tr><td>Response Date Limit</td><td>15-Aug-23</td></tr><tr><td>Preferred Payment Method</td><td>J</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Release for Import (IM429) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });

			AssertNotNull("CusEntryHeader should have a CLR event logged", messageAttachee.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsClearedCode).SingleOrDefault());

			AssertConfirmedDutiesAndTaxesAdded(messageAttachee);

			Assert("RequestedDocument CSI_Status", messageAttachee.EntryInstruction.RequestedDocuments.Cast<EU.Business.RequestedDocument>().All(x => x.CSI_Status == RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived));
		}

		void AssertConfirmedDutiesAndTaxesAdded(CusEntryHeader entry)
		{
			var entryLine1 = entry.MergedLines[0];
			AssertEquals(2, entryLine1.ConfirmedFees.Count);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_ChargeType", "A00", entryLine1.ConfirmedFees[0].CF_ChargeType);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_MethodOfCalculation", "Q", entryLine1.ConfirmedFees[0].CF_MethodOfCalculation);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_BaseValue", 200m, entryLine1.ConfirmedFees[0].CF_BaseValue);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_Rate", 5m, entryLine1.ConfirmedFees[0].CF_Rate);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_ChargeAmount", 10m, entryLine1.ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_MethodOfPayment", "A", entryLine1.ConfirmedFees[0].CF_MethodOfPayment);

			var entryLine2 = entry.MergedLines[1];
			AssertEquals(2, entryLine2.ConfirmedFees.Count);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_ChargeType", "B00", entryLine2.ConfirmedFees[1].CF_ChargeType);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_MethodOfCalculation", "A", entryLine2.ConfirmedFees[1].CF_MethodOfCalculation);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_BaseValue", 400m, entryLine2.ConfirmedFees[1].CF_BaseValue);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_Rate", 2m, entryLine2.ConfirmedFees[1].CF_Rate);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_ChargeAmount", 8m, entryLine2.ConfirmedFees[1].CF_ChargeAmount);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_MethodOfPayment", "B", entryLine2.ConfirmedFees[1].CF_MethodOfPayment);
		}

		public void TestEntryStatusWithSpecificAdditionalDeclarationType()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();

			var messageText = GetMessageText();
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				Assert("RequestedDocument CSI_Status", messageAttachee.EntryInstruction.RequestedDocuments.Cast<EU.Business.RequestedDocument>().All(x => x.CSI_Status == RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived));
			}
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();

			var entryHeader = result.messageAttachee;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var existingConfirmedDutiesAndTaxes = entryLine2.ConfirmedFees.AddNew();
			existingConfirmedDutiesAndTaxes.CF_ChargeType = "XXX";

			var instruction = result.messageAttachee.EntryInstruction;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;

			return result;
		}
	}
}
