using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class OneOffQuoteMultipleCarrierRunDocsTest : BaseRunDocumentsTest
	{
		public OneOffQuoteMultipleCarrierRunDocsTest() { }

		public void TestTemplate_DocBuilderRegistryDisable()
			=> TestTemplate(useNewDocBuilderRatingAndQuotationDocuments: false);

		public void TestTemplate_DocBuilderRegistryEnable()
			=> TestTemplate(useNewDocBuilderRatingAndQuotationDocuments: true);

		void TestTemplate(bool useNewDocBuilderRatingAndQuotationDocuments)
		{
			RunDocumentWithAllSections = ZBool.True;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "One Off Pricing Page (Multiple Carriers)");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useNewDocBuilderRatingAndQuotationDocuments))
			{
				AssertRunDocument((IDocumentSupportable)GetBusinessObject, @"{C}-[<Image(CompanyLogo,1,47)>]
{C}-[<ShrinkToFit><Rating.OneOffShipment.Mode.Description><AddTitleIfNotEmpty("" from "", ""<Origin.Location.PortName>"", """")><AddTitleIfNotEmpty("" to "", ""<Destination.Location.PortName>"", """")><AddTitleIfNotEmpty("" via "", ""<ShipmentRoutes[2].Origin.PortName>"", """")><AddTitleIfNotEmpty("" ("", ""<IncoTerm.Code>"", "")"")>]   {AO}-[Page <Current Page> of <TotalPages>]


{C}-[<ShrinkToFit><Rating.OneOffShipment.Mode.Description><AddTitleIfNotEmpty("" from "", ""<Origin.Location.PortName>"", """")><AddTitleIfNotEmpty("" to "", ""<Destination.Location.PortName>"", """")><AddTitleIfNotEmpty("" via "", ""<ShipmentRoutes[2].Origin.PortName>"", """")><AddTitleIfNotEmpty("" ("", ""<IncoTerm.Code>"", "")"")>]   {AO}-[Page <Current Page> of <TotalPages>]


{C}-[Quote No]   {O}-[Service Level]   {AA}-[Valid From]   {AM}-[Valid To]
{C}-[<AutoHeight><JobNumber> - <JobHeaderLocalClient.CompanyCode>]   {O}-[<AutoHeight><If(""<ServiceLevel>"" != """", ""<ServiceLevel.Description>"", ""Not Specified"")>]   {AA}-[<DateTimeAsString('<Rating.ValidFrom>', 'dd-MMM-yy')>]   {AM}-[<DateTimeAsString('<Rating.ValidUntil>', 'dd-MMM-yy')>]


{C}-[<AutoHeight><DocumentOpeningText(ReportName, ShipmentTransportMode.Code)>]


{C}-[Shipment Information]   {AQ}-[<If(""<Rating.IsReprint>"" == ""Y"", ""REPRINT"", """")>]


{C}-[This <RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> applies to the following shipment:]


{C}-[<If(""<Rating.OneOffShipment.Direction>"" != ""DOM"", ""Customs Entries"","""")>]   {N}-[`]   {O}-[<If(""<Rating.OneOffShipment.Direction>"" != ""DOM"", ""Entry / Invoice Lines"","""")>]   {AA}-[Value of Goods]   {AM}-[Insurance Value]
{C}-[<If(""<Rating.OneOffShipment.Direction>"" != ""DOM"", ""<Rating.OneOffShipment.NumberOfEntries>"","""")>]   {O}-[<If(""<Rating.OneOffShipment.Direction>"" != ""DOM"", ""<Rating.OneOffShipment.NumberOfEntryLines>"","""")>]   {AA}-[<If(""<GoodsValue.Amount>"" != ""0"", ""<Currency(<GoodsValue.Amount>, <GoodsValue.Currency.Code>)> <GoodsValue.Currency.Code>"", ""Not Specified"")>]   {AM}-[<If(""<Rating.OneOffShipment.InsuranceValue.Amount>"" != """", ""<Currency(<Rating.OneOffShipment.InsuranceValue.Amount>, <Rating.OneOffShipment.InsuranceValue.Currency.Code>)> <Rating.OneOffShipment.InsuranceValue.Currency.Code>"", ""Not Specified"")>]


{C}-[Container Information]

{C}-[Container Information (cont.)]

{C}-[<Containers.ContainerCount>]

{C}-[<Total Containers.ContainerCount>]   {E}-[ x <Containers.Type.Code>]   {AJ}-[<AutoHeight><Containers.Commodities.Format(""{Description}"", Comma)>]






{C}-[Loose Cargo Information]

{C}-[Loose Cargo Information (cont.)]

{C}-[<AutoHeight><Packages.Packages>]   {O}-[<AutoHeight><Packages.Dimensions.ValueAndUnitCodeBlankIfZero>]   {AA}-[<AutoHeight><Packages.Commodity.Format(""{Description}"", Comma)>]







{C}-[<RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> Details]
{AS}-[Quote Currency]

{C}-[<RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> Details (cont.)]
{AS}-[Quote Currency]

{C}-[<AutoHeight><NonCarrierCharges.Description><If(""<NonCarrierCharges.OSSell.Tax>"" != """",""*"","""")>]   {AE}-[<AutoHeight><NonCarrierCharges.OSSell.WithoutTax>]   {AM}-[<AutoHeight><If(""<RegistryItem(DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage)>""==""True""&&""<NonCarrierCharges.OSSell.WithoutTax.Currency.Code>"" != ""<NonCarrierCharges.LocalSell.WithoutTax.Currency.Code>"", ""@<NonCarrierCharges.SellRate>"","""")>]   {AS}-[<AutoHeight><If(""<RegistryItem(DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage)>""==""True"",""<NonCarrierCharges.LocalSell.WithoutTax>"","""")>]
{C}-[<HideRowIfCellIsEmpty><AutoHeight><NonCarrierCharges.CalculationDescription>]


{C}-[Sub-total (<NonCarrierCharges.OSSell.WithoutTax.Currency.Code>)]   {AE}-[<AutoHeight><FormatNumber(<Total NonCarrierCharges.OSSell.WithoutTax.Amount>,<NonCarrierCharges.OSSell.WithoutTax.Currency.Code>)>]





{C}-[TOTAL CHARGES:]   {U}-[<NonCarrierCharges.LocalSell.Currency.Code>]   {AE}-[<AutoHeight><FormatNumber(<Total NonCarrierCharges.LocalSell.WithoutTax.Amount>,<NonCarrierCharges.LocalSell.Currency.Code>)>]


{C}-[A <TaxCode> charge may apply to all items marked with an asterisk (*).]


{C}-[<AutoHeight><CarrierCharges.Description><If(""<CarrierCharges.OSSell.Tax>"" != """",""*"","""")>]   {AE}-[<AutoHeight><CarrierCharges.OSSell.WithoutTax>]   {AM}-[<AutoHeight><If(""<RegistryItem(DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage)>""==""True""&&""<CarrierCharges.OSSell.WithoutTax.Currency.Code>"" != ""<CarrierCharges.LocalSell.WithoutTax.Currency.Code>"", ""@<CarrierCharges.SellRate>"","""")>]   {AS}-[<AutoHeight><If(""<RegistryItem(DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage)>""==""True"",""<CarrierCharges.LocalSell.WithoutTax>"","""")>]
{C}-[<HideRowIfCellIsEmpty><AutoHeight><CarrierCharges.CalculationDescription>]


{C}-[Sub-total (<CarrierCharges.OSSell.WithoutTax.Currency.Code>)]   {AE}-[<AutoHeight><FormatNumber(<Total CarrierCharges.OSSell.WithoutTax.Amount>,<CarrierCharges.OSSell.WithoutTax.Currency.Code>)>]


{C}-[<RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> Details (Carrier: <CarrierCharges.Carrier.Code>), Transit Time: <CarrierCharges.CarrierTransitTime>, Frequency: <CarrierCharges.CarrierFrequency> <CarrierCharges.CarrierFrequencyUnit>]
{AS}-[Quote Currency]


{C}-[TOTAL CHARGES:]   {U}-[<CarrierCharges.LocalSell.Currency.Code>]   {AE}-[<AutoHeight><FormatNumber(<Total CarrierCharges.LocalSell.WithoutTax.Amount>,<CarrierCharges.LocalSell.Currency.Code>)>]


{C}-[A <TaxCode> charge may apply to all items marked with an asterisk (*).]


{C}-[<AutoHeight><DocumentClosing(ReportName, ShipmentTransportMode.Code)>]


{C}-[<AutoHeight>Please note that this <RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> is only applicable to the shipment defined under ""Shipment Information"". This <RegistryItem(Env.Registry.Rating.OneOffQuoteTitleText)> will be deemed invalid after this shipment has been processed.]

{C}-[<AutoHeight>Any foreign currency conversions have been undertaken using current exchange rates and thus local currency charges are subject to change.]


{C}-[<If(""<CurrentPage>"" != ""<TotalPages>"", ""Continued Over…"", """")>]

{C}-[<If(""<CurrentPage>"" != ""<TotalPages>"", ""Continued Over…"", """")>]

{C}-[END OF DOCUMENT]

{C}-[END OF DOCUMENT]");
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestOneOffPricingPageMultipleCarriers_GivenChargeMatchingCarrierOrCreditor_ThenShouldBePrintedInRelatedCarrierSection()
		{
			QuotedBooking.OH_Carrier = Carrier1.PK;
			QuotedBooking.Creditor = Creditor1.PK;

			QuotedBooking.TryLoadOrCreateJob();
			using (var job = (Job)QuotedBooking.Job)
			{
				CreateCharge(TestObjectCreator, job, ChargeCode1, sellCurrency: USDCurrency, osSellAmt: 10m, creditor: Carrier1, sequence: 1);
				CreateCharge(TestObjectCreator, job, ChargeCode2, sellCurrency: USDCurrency, osSellAmt: 20m, creditor: Creditor1, sequence: 2);

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "One Off Pricing Page (Multiple Carriers)");
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertRunDocument(QuotedBooking, @"{AO}-[Page 1 of 1]

{C}-[Quote No]   {O}-[Service Level]   {AA}-[Valid From]   {AM}-[Valid To]
{C}-[ - ]   {O}-[Not Specified]   {AA}-[01-Jan-20]   {AM}-[01-Feb-20]

{C}-[Shipment Information]

{C}-[This Quotation applies to the following shipment:]

{C}-[Customs Entries]   {N}-[`]   {O}-[Entry / Invoice Lines]   {AA}-[Value of Goods]   {AM}-[Insurance Value]
{C}-[1]   {O}-[1]   {AA}-[Not Specified]   {AM}-[0.00 ]

{C}-[Quotation Details]
{AS}-[Quote Currency]
{C}-[Quotation Details (Carrier: CARRIER1), Transit Time: , Frequency: 0 ]
{AS}-[Quote Currency]
{C}-[Charge Code 1 Description]   {AE}-[10.00 USD]   {AM}-[@1.000000]   {AS}-[10.00 ERN]
{C}-[Charge Code 2 Description]   {AE}-[20.00 USD]   {AM}-[@1.000000]   {AS}-[20.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[30.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[30.00]



{C}-[Please note that this Quotation is only applicable to the shipment defined under ""Shipment Information"". This Quotation will be deemed]
{C}-[invalid after this shipment has been processed.]

{C}-[Any foreign currency conversions have been undertaken using current exchange rates and thus local currency charges are subject to]
{C}-[change.]


{C}-[END OF DOCUMENT]");
				}
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestOneOffPricingPageMultipleCarriers_GivenChargeMatchingPossibleCarriers_ThenShouldBePrintedInRelatedCarrierSection()
		{
			var possibleCarriers1 = QuotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarriers1.TTC_OH_Carrier = Carrier1.PK;

			var possibleCarriers2 = QuotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarriers2.TTC_OH_Carrier = Carrier2.PK;

			QuotedBooking.TryLoadOrCreateJob();
			using (var job = (Job)QuotedBooking.Job)
			{
				CreateCharge(TestObjectCreator, job, ChargeCode1, sellCurrency: USDCurrency, osSellAmt: 10m, creditor: Carrier1, sequence: 1);
				CreateCharge(TestObjectCreator, job, ChargeCode2, sellCurrency: USDCurrency, osSellAmt: 20m, creditor: Carrier2, sequence: 2);

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "One Off Pricing Page (Multiple Carriers)");
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertRunDocument(QuotedBooking, @"{AO}-[Page 1 of 1]

{C}-[Quote No]   {O}-[Service Level]   {AA}-[Valid From]   {AM}-[Valid To]
{C}-[ - ]   {O}-[Not Specified]   {AA}-[01-Jan-20]   {AM}-[01-Feb-20]

{C}-[Shipment Information]

{C}-[This Quotation applies to the following shipment:]

{C}-[Customs Entries]   {N}-[`]   {O}-[Entry / Invoice Lines]   {AA}-[Value of Goods]   {AM}-[Insurance Value]
{C}-[1]   {O}-[1]   {AA}-[Not Specified]   {AM}-[0.00 ]

{C}-[Quotation Details]
{AS}-[Quote Currency]
{C}-[Quotation Details (Carrier: CARRIER1), Transit Time: , Frequency: 0 ]
{AS}-[Quote Currency]
{C}-[Charge Code 1 Description]   {AE}-[10.00 USD]   {AM}-[@1.000000]   {AS}-[10.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[10.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[10.00]



{C}-[Quotation Details (Carrier: CARRIER2), Transit Time: , Frequency: 0 ]
{AS}-[Quote Currency]
{C}-[Charge Code 2 Description]   {AE}-[20.00 USD]   {AM}-[@1.000000]   {AS}-[20.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[20.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[20.00]



{C}-[Please note that this Quotation is only applicable to the shipment defined under ""Shipment Information"". This Quotation will be deemed]
{C}-[invalid after this shipment has been processed.]

{C}-[Any foreign currency conversions have been undertaken using current exchange rates and thus local currency charges are subject to]
{C}-[change.]


{C}-[END OF DOCUMENT]");
				}
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestOneOffPricingPageMultipleCarriers_GivenChargeMatchingCreditorOfPossibleCarriers_ThenShouldBePrintedInRelatedCarrierSection()
		{
			var possibleCarriers1 = QuotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarriers1.TTC_OH_Carrier = Carrier1.PK;
			possibleCarriers1.TTC_OH_Creditor = Creditor1.PK;
			var possibleCarriers2 = QuotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarriers2.TTC_OH_Carrier = Carrier2.PK;
			possibleCarriers2.TTC_OH_Creditor = Creditor2.PK;

			QuotedBooking.TryLoadOrCreateJob();
			using (var job = (Job)QuotedBooking.Job)
			{
				CreateCharge(TestObjectCreator, job, ChargeCode1, sellCurrency: USDCurrency, osSellAmt: 10m, creditor: Creditor1, sequence: 1);
				CreateCharge(TestObjectCreator, job, ChargeCode2, sellCurrency: USDCurrency, osSellAmt: 20m, creditor: Creditor2, sequence: 2);

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "One Off Pricing Page (Multiple Carriers)");
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertRunDocument(QuotedBooking, @"{AO}-[Page 1 of 1]

{C}-[Quote No]   {O}-[Service Level]   {AA}-[Valid From]   {AM}-[Valid To]
{C}-[ - ]   {O}-[Not Specified]   {AA}-[01-Jan-20]   {AM}-[01-Feb-20]

{C}-[Shipment Information]

{C}-[This Quotation applies to the following shipment:]

{C}-[Customs Entries]   {N}-[`]   {O}-[Entry / Invoice Lines]   {AA}-[Value of Goods]   {AM}-[Insurance Value]
{C}-[1]   {O}-[1]   {AA}-[Not Specified]   {AM}-[0.00 ]

{C}-[Quotation Details]
{AS}-[Quote Currency]
{C}-[Quotation Details (Carrier: CARRIER1), Transit Time: , Frequency: 0 ]
{AS}-[Quote Currency]
{C}-[Charge Code 1 Description]   {AE}-[10.00 USD]   {AM}-[@1.000000]   {AS}-[10.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[10.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[10.00]



{C}-[Quotation Details (Carrier: CARRIER2), Transit Time: , Frequency: 0 ]
{AS}-[Quote Currency]
{C}-[Charge Code 2 Description]   {AE}-[20.00 USD]   {AM}-[@1.000000]   {AS}-[20.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[20.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[20.00]



{C}-[Please note that this Quotation is only applicable to the shipment defined under ""Shipment Information"". This Quotation will be deemed]
{C}-[invalid after this shipment has been processed.]

{C}-[Any foreign currency conversions have been undertaken using current exchange rates and thus local currency charges are subject to]
{C}-[change.]


{C}-[END OF DOCUMENT]");
				}
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestOneOffPricingPageMultipleCarriers_GivenChargeNotMatchingCarrierOrCreditor_ThenChargeShouldBePrintedInNonCarrierSection()
		{
			QuotedBooking.OH_Carrier = Carrier1.PK;
			QuotedBooking.Creditor = Creditor1.PK;

			var possibleCarriers1 = QuotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarriers1.TTC_OH_Carrier = Carrier2.PK;
			possibleCarriers1.TTC_OH_Creditor = Creditor2.PK;

			QuotedBooking.TryLoadOrCreateJob();
			using (var job = (Job)QuotedBooking.Job)
			{
				CreateCharge(TestObjectCreator, job, ChargeCode1, sellCurrency: USDCurrency, osSellAmt: 10m, creditor: Carrier3, sequence: 1);
				CreateCharge(TestObjectCreator, job, ChargeCode2, sellCurrency: USDCurrency, osSellAmt: 20m, creditor: Creditor3, sequence: 2);

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "One Off Pricing Page (Multiple Carriers)");
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertRunDocument(QuotedBooking, @"{AO}-[Page 1 of 1]

{C}-[Quote No]   {O}-[Service Level]   {AA}-[Valid From]   {AM}-[Valid To]
{C}-[ - ]   {O}-[Not Specified]   {AA}-[01-Jan-20]   {AM}-[01-Feb-20]

{C}-[Shipment Information]

{C}-[This Quotation applies to the following shipment:]

{C}-[Customs Entries]   {N}-[`]   {O}-[Entry / Invoice Lines]   {AA}-[Value of Goods]   {AM}-[Insurance Value]
{C}-[1]   {O}-[1]   {AA}-[Not Specified]   {AM}-[0.00 ]

{C}-[Quotation Details]
{AS}-[Quote Currency]
{C}-[Charge Code 1 Description]   {AE}-[10.00 USD]   {AM}-[@1.000000]   {AS}-[10.00 ERN]
{C}-[Charge Code 2 Description]   {AE}-[20.00 USD]   {AM}-[@1.000000]   {AS}-[20.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[30.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[30.00]



{C}-[Please note that this Quotation is only applicable to the shipment defined under ""Shipment Information"". This Quotation will be deemed]
{C}-[invalid after this shipment has been processed.]

{C}-[Any foreign currency conversions have been undertaken using current exchange rates and thus local currency charges are subject to]
{C}-[change.]



{C}-[END OF DOCUMENT]");
				}
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestOneOffPricingPageMultipleCarriers_GivenCurrencies_ThenEachCurrencyPrintSeparately()
		{
			QuotedBooking.OH_Carrier = Carrier1.PK;

			QuotedBooking.TryLoadOrCreateJob();
			using (var job = (Job)QuotedBooking.Job)
			{
				CreateCharge(TestObjectCreator, job, ChargeCode1, sellCurrency: USDCurrency, osSellAmt: 10m, creditor: Carrier1, sequence: 1);
				CreateCharge(TestObjectCreator, job, ChargeCode2, sellCurrency: AUDCurrency, osSellAmt: 20m, creditor: Carrier1, sequence: 2);

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "One Off Pricing Page (Multiple Carriers)");
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertRunDocument(QuotedBooking, @"{AO}-[Page 1 of 1]

{C}-[Quote No]   {O}-[Service Level]   {AA}-[Valid From]   {AM}-[Valid To]
{C}-[ - ]   {O}-[Not Specified]   {AA}-[01-Jan-20]   {AM}-[01-Feb-20]

{C}-[Shipment Information]

{C}-[This Quotation applies to the following shipment:]

{C}-[Customs Entries]   {N}-[`]   {O}-[Entry / Invoice Lines]   {AA}-[Value of Goods]   {AM}-[Insurance Value]
{C}-[1]   {O}-[1]   {AA}-[Not Specified]   {AM}-[0.00 ]

{C}-[Quotation Details]
{AS}-[Quote Currency]
{C}-[Quotation Details (Carrier: CARRIER1), Transit Time: , Frequency: 0 ]
{AS}-[Quote Currency]
{C}-[Charge Code 2 Description]   {AE}-[20.00 AUD]   {AM}-[@1.000000]   {AS}-[20.00 ERN]

{C}-[Sub-total (AUD)]   {AE}-[20.00]

{C}-[Charge Code 1 Description]   {AE}-[10.00 USD]   {AM}-[@1.000000]   {AS}-[10.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[10.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[30.00]



{C}-[Please note that this Quotation is only applicable to the shipment defined under ""Shipment Information"". This Quotation will be deemed]
{C}-[invalid after this shipment has been processed.]

{C}-[Any foreign currency conversions have been undertaken using current exchange rates and thus local currency charges are subject to]
{C}-[change.]


{C}-[END OF DOCUMENT]");
				}
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestOneOffPricingPageMultipleCarriers_GivenChargeWithTransitTimeAndFrequency_ThenTransitTimeWithFrequencyShouldBePrinted()
		{
			QuotedBooking.OH_Carrier = Carrier1.PK;
			QuotedBooking.Creditor = Creditor1.PK;
			QuotedBooking.TransitTime = "1";
			QuotedBooking.Frequency = 10;
			QuotedBooking.FrequencyUnit = "daily";

			var possibleCarrier = QuotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier.TTC_OH_Carrier = Carrier2.PK;
			possibleCarrier.TTC_OH_Creditor = Creditor2.PK;
			possibleCarrier.TTC_TransitTime = "2";
			possibleCarrier.TTC_Frequency = 20;
			possibleCarrier.TTC_FrequencyUnit = "days";

			QuotedBooking.TryLoadOrCreateJob();
			using (var job = (Job)QuotedBooking.Job)
			{
				CreateCharge(TestObjectCreator, job, ChargeCode1, sellCurrency: USDCurrency, osSellAmt: 10m, creditor: Carrier1, sequence: 1);
				CreateCharge(TestObjectCreator, job, ChargeCode2, sellCurrency: USDCurrency, osSellAmt: 20m, creditor: Creditor1, sequence: 2);
				CreateCharge(TestObjectCreator, job, ChargeCode3, sellCurrency: USDCurrency, osSellAmt: 30m, creditor: Carrier2, sequence: 3);
				CreateCharge(TestObjectCreator, job, ChargeCode4, sellCurrency: USDCurrency, osSellAmt: 40m, creditor: Creditor2, sequence: 4);

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "One Off Pricing Page (Multiple Carriers)");
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertRunDocument(QuotedBooking, @"{AO}-[Page 1 of 1]

{C}-[Quote No]   {O}-[Service Level]   {AA}-[Valid From]   {AM}-[Valid To]
{C}-[ - ]   {O}-[Not Specified]   {AA}-[01-Jan-20]   {AM}-[01-Feb-20]

{C}-[Frequency]   {AA}-[Transit Time]
{C}-[10 per Day]   {AA}-[1 Day]

{C}-[Shipment Information]

{C}-[This Quotation applies to the following shipment:]

{C}-[Customs Entries]   {N}-[`]   {O}-[Entry / Invoice Lines]   {AA}-[Value of Goods]   {AM}-[Insurance Value]
{C}-[1]   {O}-[1]   {AA}-[Not Specified]   {AM}-[0.00 ]

{C}-[Quotation Details]
{AS}-[Quote Currency]
{C}-[Quotation Details (Carrier: CARRIER1), Transit Time: 1, Frequency: 10 daily]
{AS}-[Quote Currency]
{C}-[Charge Code 1 Description]   {AE}-[10.00 USD]   {AM}-[@1.000000]   {AS}-[10.00 ERN]
{C}-[Charge Code 2 Description]   {AE}-[20.00 USD]   {AM}-[@1.000000]   {AS}-[20.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[30.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[30.00]



{C}-[Quotation Details (Carrier: CARRIER2), Transit Time: 2, Frequency: 20 days]
{AS}-[Quote Currency]
{C}-[Charge Code 3 Description]   {AE}-[30.00 USD]   {AM}-[@1.000000]   {AS}-[30.00 ERN]
{C}-[Charge Code 4 Description]   {AE}-[40.00 USD]   {AM}-[@1.000000]   {AS}-[40.00 ERN]

{C}-[Sub-total (USD)]   {AE}-[70.00]


{C}-[TOTAL CHARGES:]   {U}-[ERN]   {AE}-[70.00]



{C}-[Please note that this Quotation is only applicable to the shipment defined under ""Shipment Information"". This Quotation will be deemed]
{C}-[invalid after this shipment has been processed.]

{C}-[Any foreign currency conversions have been undertaken using current exchange rates and thus local currency charges are subject to]
{C}-[change.]


{C}-[END OF DOCUMENT]");
				}
			}
		}

		static Charge CreateCharge(TestObjectCreator testObjectCreator, Job job, AccChargeCode chargeCode, RefCurrency sellCurrency, decimal osSellAmt, OrgHeader creditor, ZShort sequence)
		{
			var charge = testObjectCreator.CreateCharge(job, chargeCode, sellCurrency: sellCurrency, osSellAmt: osSellAmt, creditor: creditor);
			charge.JR_DisplaySequence = sequence;
			return charge;
		}

		#region ChargeCodeFactory

		TestHelper.ChargeCodeFactory ChargeCodeFactory => chargeCodeFactory ??= new TestHelper.ChargeCodeFactory(Factory);
		TestHelper.ChargeCodeFactory chargeCodeFactory;

		#endregion

		#region TestObjectCreator

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;

		#endregion

		#region New Carrier

		public OrgHeader Carrier1
		{
			get
			{
				if (carrier1 == null)
				{
					carrier1 = Factory.NewWithValidTestData<OrgHeader>();
					carrier1.OH_Code = "CARRIER1";
				}

				return carrier1;
			}
		}

		OrgHeader carrier1;

		public OrgHeader Carrier2
		{
			get
			{
				if (carrier2 == null)
				{
					carrier2 = Factory.NewWithValidTestData<OrgHeader>();
					carrier2.OH_Code = "CARRIER2";
				}

				return carrier2;
			}
		}

		OrgHeader carrier2;

		public OrgHeader Carrier3
		{
			get
			{
				if (carrier3 == null)
				{
					carrier3 = Factory.NewWithValidTestData<OrgHeader>();
					carrier3.OH_Code = "CARRIER3";
				}

				return carrier3;
			}
		}

		OrgHeader carrier3;

		#endregion

		#region New Creditor

		public OrgHeader Creditor1
		{
			get
			{
				if (creditor1 == null)
				{
					creditor1 = Factory.NewWithValidTestData<OrgHeader>();
					creditor1.OH_Code = "CREDITOR1";
				}

				return creditor1;
			}
		}

		OrgHeader creditor1;

		public OrgHeader Creditor2
		{
			get
			{
				if (creditor2 == null)
				{
					creditor2 = Factory.NewWithValidTestData<OrgHeader>();
					creditor2.OH_Code = "CREDITOR2";
				}

				return creditor2;
			}
		}

		OrgHeader creditor2;

		public OrgHeader Creditor3
		{
			get
			{
				if (creditor3 == null)
				{
					creditor3 = Factory.NewWithValidTestData<OrgHeader>();
					creditor3.OH_Code = "CREDITOR3";
				}

				return creditor3;
			}
		}

		OrgHeader creditor3;

		#endregion

		#region Currency

		RefCurrency USDCurrency => usdCurrency ??= RefCurrency.LoadFromCurrencyCode(Factory, "USD");
		RefCurrency usdCurrency;

		RefCurrency AUDCurrency => audCurrency ??= RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
		RefCurrency audCurrency;

		#endregion

		#region Charge Code

		AccChargeCode ChargeCode1 => chargeCode1 ??= ChargeCodeFactory.New("CHG1", "Charge Code 1 Description", FlatCalculator.Code);
		AccChargeCode chargeCode1;

		AccChargeCode ChargeCode2 => chargeCode2 ??= ChargeCodeFactory.New("CHG2", "Charge Code 2 Description", FlatCalculator.Code);
		AccChargeCode chargeCode2;

		AccChargeCode ChargeCode3 => chargeCode3 ??= ChargeCodeFactory.New("CHG3", "Charge Code 3 Description", FlatCalculator.Code);
		AccChargeCode chargeCode3;

		AccChargeCode ChargeCode4 => chargeCode4 ??= ChargeCodeFactory.New("CHG4", "Charge Code 4 Description", FlatCalculator.Code);
		AccChargeCode chargeCode4;

		#endregion

		#region Implementation

		public override BusinessObject GetBusinessObject => QuotedBooking;

		public override BusinessContext BusinessContext => BusinessContext.Quotation;

		protected override void SetUp()
		{
			base.SetUp();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
		}

		QuotedBooking QuotedBooking;

		#endregion
	}
}
