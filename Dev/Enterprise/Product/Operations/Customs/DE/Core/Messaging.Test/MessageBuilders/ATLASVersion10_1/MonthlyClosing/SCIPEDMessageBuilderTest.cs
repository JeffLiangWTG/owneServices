using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Messaging.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class SCIPEDMessageBuilderTest : MessageBuilderTest<SCIPEDMessageBuilder, VSCIPK>
	{
		[TestDate(2022, 1, 5, 15, 13, 23, 111)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			NUnit.Framework.Assert.That(scipedBuilder.GetXMLMessage().AsString(), Is.EqualTo(new CargoWise.IO.EmbeddedResourceRetriever().GetString($"{TestExtensionsATLASVersion10_1.MonthlyClosingTestFilesResourcePath}.TestSCIPEDMessage.txt")));
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader_LRNTruncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns("LRN0000000000000000000111");
			NUnit.Framework.Assert.That(scipedBuilder.GenerateMessage().Header.LRN, Is.EqualTo("LRN0000000000000000000"), "LRN should be truncated to 22 characters.");
		}

		[ExpectNoExceptions]
		public void TestStartAccountingPeriodDateNull()
		{
			headerMock.Update(x => x.StartAccountingPeriodDate, null);
			CombineAssertions(() =>
			{
				var message = scipedBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.Header.StartAccountingPeriodDate, Is.EqualTo(default(DateTime)), "StartAccountingPeriodDate");
				NUnit.Framework.Assert.That(message.Header.StartAccountingPeriodDateSpecified, Is.EqualTo(false), "StartAccountingPeriodDateSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestEndAccountingPeriodDateNull()
		{
			headerMock.Update(x => x.EndAccountingPeriodDate, null);
			CombineAssertions(() =>
			{
				var message = scipedBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.Header.EndAccountingPeriodDate, Is.EqualTo(default(DateTime)), "EndAccountingPeriodDate");
				NUnit.Framework.Assert.That(message.Header.EndAccountingPeriodDateSpecified, Is.EqualTo(false), "EndAccountingPeriodDateSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarantIsConsigneeFlagFalse()
		{
			headerMock.Update(x => x.DeclarantIsConsigneeFlag, false);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Header.DeclarantIsConsigneeFlag, Is.EqualTo("N"));
		}

		[ExpectNoExceptions]
		public void TestCurrencyCodeEUR()
		{
			headerMock.Update(x => x.CurrencyCode, "EUR");
			var message = scipedBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Header.CurrencyCode, Is.EqualTo(VSCIPKHeaderCurrencyCode.EUR));
				NUnit.Framework.Assert.That(message.Header.CurrencyCodeSpecified, Is.EqualTo(true));
			});
		}

		[ExpectNoExceptions]
		public void TestCurrencyCodeDifferentToEUR()
		{
			headerMock.Update(x => x.CurrencyCode, "USD");
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Header.CurrencyCode, Is.EqualTo(VSCIPKHeaderCurrencyCode.EUR));
		}

		[ExpectNoExceptions]
		public void TestAuthorizationNumberTruncate()
		{
			messageHeaderMock.Update(x => x.AuthorisationNumber, '1'.Replicate(26));
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Header.AuthorisationNumber, Is.EqualTo('1'.Replicate(25)));
		}

		[ExpectNoExceptions]
		public void TestDeclarantNull()
		{
			headerMock.Update(x => x.Declarant, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Declarant, Is.EqualTo(default(VSCIPKDeclarant)));
		}

		[ExpectNoExceptions]
		public void TestDeclarantNoEORINumber()
		{
			declarantMock.Update(x => x.Identification, new Mock<IPartyID>().Object);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Declarant, Is.EqualTo(default(VSCIPKDeclarant)), "No EORI Identification - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestRepresentativeNull()
		{
			headerMock.Update(x => x.Representative, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(VSCIPKRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestRepresentativeNoEORINumber()
		{
			representativeMock.Update(x => x.EoriNumber, string.Empty);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(VSCIPKRepresentative)), "No EORI Identification - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPrincipalNull()
		{
			headerMock.Update(x => x.Principal, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Principal, Is.EqualTo(default(VSCIPKPrincipal)));
		}

		[ExpectNoExceptions]
		public void TestPrincipalNoEORINumber()
		{
			principalMock
				.Update(x => x.Identification, new Mock<IPartyID>().Object)
				.Setup(x => x.Address, Address);

			var message = scipedBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				var principal = message.Principal;
				NUnit.Framework.Assert.That(principal.Identification, Is.EqualTo(default(VSCIPKPrincipalIdentification)), "No EORI Identification - should be [null]");
				NUnit.Framework.Assert.That(principal.Name, Is.EqualTo("Max Mustermann"), "Name");
				NUnit.Framework.Assert.That(principal.Address.Postcode, Is.EqualTo("55126"), "Postcode");
				NUnit.Framework.Assert.That(principal.Address.City, Is.EqualTo("Mainz"), "City");
				NUnit.Framework.Assert.That(principal.Address.District, Is.EqualTo("Finthen"), "District");
				NUnit.Framework.Assert.That(principal.Address.Line, Is.EqualTo("Poststraße 1"), "Address");
				NUnit.Framework.Assert.That(principal.Address.Country, Is.EqualTo("DE"), "Country");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsAuthorisation_LocalClearanceProcedureEmpty()
		{
			headerMock.Update(x => x.LocalClearanceProcedure, string.Empty);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Header.CustomsAuthorisation.CurrentProcedure, Is.EqualTo("ENDUSEAUTHNUMBER"));
		}

		[ExpectNoExceptions]
		public void TestCustomsAuthorisation_ProcedureAuthorizationEmpty()
		{
			headerMock.Update(x => x.ProcedureAuthorization, string.Empty);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Header.CustomsAuthorisation.LocalClearanceProcedure, Is.EqualTo("AUTHNUMBER"));
		}

		[ExpectNoExceptions]
		public void TestCustomsAuthorisation_NotPopulated()
		{
			headerMock
				.Update(x => x.LocalClearanceProcedure, string.Empty)
				.Update(x => x.ProcedureAuthorization, string.Empty);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Header.CustomsAuthorisation, Is.EqualTo(default(VSCIPKHeaderCustomsAuthorisation)));
		}

		[ExpectNoExceptions]
		public void TestBodiesMultiple()
		{
			headerMock.Update(x => x.Bodies, new[] { bodyMock.Object, bodyMock.Object });
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body.Length, Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestBodyCustomsValueFlag()
		{
			bodyMock.Update(x => x.CustomsValue, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].CustomsValueFlag, Is.EqualTo("0"));
		}

		[ExpectNoExceptions]
		public void TestConsignorNull()
		{
			bodyMock.Update(x => x.Consignor, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].Consignor, Is.EqualTo(default(VSCIPKBodyConsignor)));
		}

		[ExpectNoExceptions]
		public void TestConsignorNoEORINumber()
		{
			consignorMock
				.Update(x => x.Identification, new Mock<IPartyID>().Object)
				.Setup(x => x.Address, Address);

			var message = scipedBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				var consignor = message.Body[0].Consignor;
				NUnit.Framework.Assert.That(consignor.Identification, Is.EqualTo(default(VSCIPKBodyConsignorIdentification)), "No EORI Identification - should be [null]");
				NUnit.Framework.Assert.That(consignor.Name, Is.EqualTo("Max Mustermann"), "Name");
				NUnit.Framework.Assert.That(consignor.Address.Postcode, Is.EqualTo("55126"), "Postcode");
				NUnit.Framework.Assert.That(consignor.Address.City, Is.EqualTo("Mainz"), "City");
				NUnit.Framework.Assert.That(consignor.Address.District, Is.EqualTo("Finthen"), "District");
				NUnit.Framework.Assert.That(consignor.Address.Line, Is.EqualTo("Poststraße 1"), "Address");
				NUnit.Framework.Assert.That(consignor.Address.Country, Is.EqualTo("DE"), "Country");
			});
		}

		[ExpectNoExceptions]
		public void TestConsigneeNull()
		{
			bodyMock.Update(x => x.Consignee, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].Consignee, Is.EqualTo(default(VSCIPKBodyConsignee)));
		}

		[ExpectNoExceptions]
		public void TestConsigneeNoEORINumber()
		{
			consigneeMock
				.Update(x => x.Identification, new Mock<IPartyID>().Object)
				.Setup(x => x.Address, Address);

			var message = scipedBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				var consignee = message.Body[0].Consignee;
				NUnit.Framework.Assert.That(consignee.Identification, Is.EqualTo(default(VSCIPKBodyConsigneeIdentification)), "No EORI Identification - should be [null]");
				NUnit.Framework.Assert.That(consignee.Name, Is.EqualTo("Max Mustermann"), "Name");
				NUnit.Framework.Assert.That(consignee.Address.Postcode, Is.EqualTo("55126"), "Postcode");
				NUnit.Framework.Assert.That(consignee.Address.City, Is.EqualTo("Mainz"), "City");
				NUnit.Framework.Assert.That(consignee.Address.District, Is.EqualTo("Finthen"), "District");
				NUnit.Framework.Assert.That(consignee.Address.Line, Is.EqualTo("Poststraße 1"), "Address");
				NUnit.Framework.Assert.That(consignee.Address.Country, Is.EqualTo("DE"), "Country");
			});
		}

		[ExpectNoExceptions]
		public void TestVendorNull()
		{
			bodyCustomsValueMock.Update(x => x.Vendor, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].CustomsValue.Vendor, Is.EqualTo(default(VSCIPKBodyCustomsValueVendor)));
		}

		[ExpectNoExceptions]
		public void TestVendorHasEORINumber()
		{
			vendorMock.Update(x => x.Identification, new Mock<IPartyID>()
				.Setup(x => x.EoriNumber, "GR234567890")
				.Object);

			var message = scipedBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				var vendor = message.Body[0].CustomsValue.Vendor;
				NUnit.Framework.Assert.That(vendor.Name, Is.EqualTo(default(string)), "Name - should be [null]");
				NUnit.Framework.Assert.That(vendor.Address, Is.EqualTo(default(VSCIPKBodyCustomsValueVendorAddress)), "Address - should be [null]");
				NUnit.Framework.Assert.That(vendor.Identification.ReferenceNumber, Is.EqualTo("GR234567890"), "EORINumber");
			});
		}

		[ExpectNoExceptions]
		public void TestVendeeNull()
		{
			bodyCustomsValueMock.Update(x => x.Vendee, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].CustomsValue.Vendee, Is.EqualTo(default(VSCIPKBodyCustomsValueVendee)));
		}

		[ExpectNoExceptions]
		public void TestVendeeNoEORINumber()
		{
			vendeeMock
				.Update(x => x.Identification, new Mock<IPartyID>().Object)
				.Setup(x => x.Address, Address);

			var message = scipedBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				var vendee = message.Body[0].CustomsValue.Vendee;
				NUnit.Framework.Assert.That(vendee.Identification, Is.EqualTo(default(VSCIPKBodyCustomsValueVendeeIdentification)), "No EORI Identification - should be [null]");
				NUnit.Framework.Assert.That(vendee.Name, Is.EqualTo("Max Mustermann"), "Name");
				NUnit.Framework.Assert.That(vendee.Address.Postcode, Is.EqualTo("55126"), "Postcode");
				NUnit.Framework.Assert.That(vendee.Address.City, Is.EqualTo("Mainz"), "City");
				NUnit.Framework.Assert.That(vendee.Address.District, Is.EqualTo("Finthen"), "District");
				NUnit.Framework.Assert.That(vendee.Address.Line, Is.EqualTo("Poststraße 1"), "Address");
				NUnit.Framework.Assert.That(vendee.Address.Country, Is.EqualTo("DE"), "Country");
			});
		}

		[ExpectNoExceptions]
		public void TestPaymentTransactionNull()
		{
			bodyMock.Update(x => x.PaymentTransaction, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].PaymentTransaction, Is.EqualTo(default(VSCIPKBodyPaymentTransaction)));
		}

		[ExpectNoExceptions]
		public void TestBodyCustomsValueNull()
		{
			bodyMock.Update(x => x.CustomsValue, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].CustomsValue, Is.EqualTo(default(VSCIPKBodyCustomsValue)));
		}

		[ExpectNoExceptions]
		public void TestGoodsItemMultiple()
		{
			bodyMock.Update(x => x.Lines, new[] { goodsItemMock.Object, goodsItemMock.Object });
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem.Length, Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestReferencedSequenceNumberNull()
		{
			goodsItemMock.Update(x => x.ReferencedSequenceNumber, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].ReferredSequenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestInvoiceAmountNull()
		{
			goodsItemMock.Update(x => x.InvoiceAmount, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].InvoiceAmount, Is.EqualTo(0.0m));
		}

		[ExpectNoExceptions]
		public void TestForeignTradeStatisticsGrossMassMeasure_NotSpecified()
		{
			goodsItemMock.Update(x => x.ForeignTradeStatisticsGrossMassMeasure, decimal.Zero);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].ForeignTradeStatistics.GrossMassMeasureSpecified, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestGoodsItemCustomsValueNull()
		{
			goodsItemMock.Update(x => x.CustomsValue, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].CustomsValue, Is.EqualTo(default(VSCIPKBodyGoodsItemCustomsValue)));
		}

		[ExpectNoExceptions]
		public void TestGoodsItemCustomsValueNetPriceCurrencyRateAgreedFlagNull()
		{
			netPriceMock.Update(x => x.CurrencyCode, "EUR");
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].CustomsValue.NetPrice.CurrencyRateAgreedFlag, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestGoodsItemCustomsValueNetPriceCurrencyRateSpecifiedFalse()
		{
			CombineAssertions(() =>
			{
				netPriceMock.Update(x => x.CurrencyRate, 0.0m);
				var message = scipedBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].CustomsValue.NetPrice.CurrencyRateSpecified, Is.EqualTo(false), "CurrencyRate empty");

				netPriceMock.Update(x => x.CurrencyCode, "EUR");
				message = scipedBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].CustomsValue.NetPrice.CurrencyRateSpecified, Is.EqualTo(false), "Is EUR");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsItemAssessmentAmountMultiple()
		{
			goodsItemMock.Update(x => x.AssessmentAmount, new[] { GetAmount(18219, "NAR", "X").Object, GetAmount(18219, "NAR", "X").Object });
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].Assessment.Amount.Length, Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestPreferentialTreatmentNotPopulated()
		{
			goodsItemMock.Update(x => x.RequestedPreferentialTreatment, string.Empty);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].PreferentialTreatment, Is.EqualTo(default(VSCIPKBodyGoodsItemPreferentialTreatment)));
		}

		[ExpectNoExceptions]
		public void TestForeignTradeAmountNotPopulated()
		{
			goodsItemMock.Update(x => x.ForeignTradeStatisticsAmount, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].ForeignTradeStatistics.Amount, Is.EqualTo(default(VSCIPKBodyGoodsItemForeignTradeStatisticsAmount)));
		}

		[ExpectNoExceptions]
		public void TestInwardMovementNotPopulated()
		{
			goodsItemMock.Update(x => x.InwardMovementAmount, null);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].GoodsItem[0].InwardMovement, Is.EqualTo(default(VSCIPKBodyGoodsItemInwardMovement)));
		}

		[ExpectNoExceptions]
		public void TestLineNumbersInMessage()
		{
			bodyMock.Update(x => x.Lines, new[] { goodsItemMock.Object, GetLineMock(22).Object });
			_ = scipedBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(scipedBuilder.LineNumbersInMessage.Count, Is.EqualTo(2));
				NUnit.Framework.Assert.That(scipedBuilder.LineNumbersInMessage, Has.Some.EqualTo(1));
				NUnit.Framework.Assert.That(scipedBuilder.LineNumbersInMessage, Has.Some.EqualTo(22));
			});
		}

		[ExpectNoExceptions]
		public void TestForeignTradeStatisticsNotPopulated()
		{
			bodyMock.Update(x => x.ForeignTradeStatisticsEntryCustomsOffice, string.Empty);
			var message = scipedBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Body[0].ForeignTradeStatistics, Is.EqualTo(default(VSCIPKBodyForeignTradeStatistics)));
		}

		[ExpectNoExceptions]
		public void TestHeader_MRN()
		{
			headerMock.Update(x => x.ReferenceNumber, "23DE586601055987B7");
			CombineAssertions(() =>
			{
				var message = scipedBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.Header.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(message.Header.ReferenceNumber, Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestBody_MRN()
		{
			bodyMock.Update(x => x.ReferenceNumber, "23DE586601055987B7");
			CombineAssertions(() =>
			{
				var message = scipedBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.Body[0].MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(message.Body[0].ReferenceNumber, Is.Null.Or.Empty);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupGoodsItem();
			SetupBody();
			SetupHeader();
			SetupMessageHeader();
			scipedBuilder = new SCIPEDMessageBuilder(messageHeaderMock.Object);
		}
		SCIPEDMessageBuilder scipedBuilder;
		Mock<ISCIPEDHeader> headerMock;
		Mock<ISCIPEDBody> bodyMock;
		Mock<ISCIPEDLine> goodsItemMock;
		Mock<IImportMessageHeader> messageHeaderMock;
		Mock<IImportParty> declarantMock;
		Mock<IImportParty> principalMock;
		Mock<IImportParty> consignorMock;
		Mock<IImportParty> consigneeMock;
		Mock<IPartyID> representativeMock;
		Mock<IImportParty> vendorMock;
		Mock<IImportParty> vendeeMock;
		Mock<ICustomsValue> bodyCustomsValueMock;
		Mock<IImportLineCustomsValue> lineCustomsValueMock;
		Mock<IImportCosts> netPriceMock;

		void SetupGoodsItem()
		{
			var additionDeductionMock = new Mock<IAdditionDeduction>()
				.Setup(x => x.Type, "015")
				.Setup(x => x.Value, 827.25m)
				.Setup(x => x.CurrencyCode, "RUB")
				.Setup(x => x.CurrencyRateIATA, true)
				.Setup(x => x.CurrencyRateAgreedFlag, false)
				.Setup(x => x.CurrencyRate, 1)
				.Setup(x => x.CurrencyRateDate, new DateTime(2020, 10, 30))
				.Setup(x => x.Percentage, 0.15m);

			var airFreightCostsMock = new Mock<IAirFreightCosts>()
				.Setup(x => x.Value, 624.51m)
				.Setup(x => x.CurrencyCode, "RUB")
				.Setup(x => x.CurrencyRateIATA, true)
				.Setup(x => x.CurrencyRateAgreedFlag, false)
				.Setup(x => x.CurrencyRate, 1)
				.Setup(x => x.CurrencyRateDate, new DateTime(2020, 10, 30));

			lineCustomsValueMock = new Mock<IImportLineCustomsValue>()
				.Setup(x => x.CustomsValueDepartureAirport, "DXB")
				.Setup(x => x.CustomsValueDestinationPlace, "Hamburg")
				.Setup(x => x.CustomsValueAdditionDeductionDescription, "Hinzurechnungen/Abzüge")
				.Setup(x => x.CustomsValueNetPrice, (netPriceMock = MessageBuilderTestHelper.MockImportCosts(123877.150m, "USD", false, 1.111111112m)).Object)
				.Setup(x => x.CustomsValueIndirectPayment, MessageBuilderTestHelper.MockImportCosts(2939.04m, "USD", false, 1m).Object)
				.Setup(x => x.CustomsValueAirFreightCosts, airFreightCostsMock.Object)
				.Setup(x => x.CustomsValueAdditionDeduction, new IAdditionDeduction[] { additionDeductionMock.Object });

			goodsItemMock = GetLineMock(1);
		}

		void SetupBody()
		{
			bodyMock = new Mock<ISCIPEDBody>()
				.Setup(x => x.ReferenceNumber, "REF1234")
				.Setup(x => x.Consignor, (consignorMock = new Mock<IImportParty>()
					.Setup(x => x.Identification, new Mock<IPartyID>()
						.Setup(x => x.EoriNumber, "GR567890123")
						.Object)
					).Object)
				.Setup(x => x.Consignee, (consigneeMock = new Mock<IImportParty>()
					.Setup(x => x.Identification, new Mock<IPartyID>()
						.Setup(x => x.EoriNumber, "GR667890124")
						.Setup(x => x.EoriBranchSuffix, "0005")
						.Object)
					).Object)
				.Setup(x => x.DeliveryTermsCode, "CFR")
				.Setup(x => x.DeliveryTermsDescription, "Description")
				.Setup(x => x.DeliveryTermsPlace, "Place")
				.Setup(x => x.DeliveryTermsKey, "0")
				.Setup(x => x.PaymentTransaction, new Mock<IMoney>()
					.Setup(x => x.Value, 12345678901.23m)
					.Setup(x => x.CurrencyCode, "INR").Object)
				.Setup(x => x.ForeignTradeStatisticsEntryCustomsOffice, "DE006665")
				.Setup(x => x.CustomsValue, (bodyCustomsValueMock = new Mock<ICustomsValue>()
					.Setup(x => x.FormerDecisions, "Former Decisions")
					.Setup(x => x.Vendee, (vendeeMock = new Mock<IImportParty>()
						.Setup(x => x.Identification, new Mock<IPartyID>()
							.Setup(x => x.EoriNumber, "DE123456789")
							.Object)
						).Object)
					.Setup(x => x.Vendor, (vendorMock = new Mock<IImportParty>()
						.Setup(x => x.Address, new Mock<IImportPartyIdAddress>()
							.Setup(x => x.Address, "Poststrasse 1")
							.Setup(x => x.City, "Mainz")
							.Setup(x => x.Country, "DE")
							.Setup(x => x.District, "Finthen")
							.Setup(x => x.Name, "Bob Baumeister")
							.Setup(x => x.Postcode, "55126").Object)
						).Object)
					.Setup(x => x.AffiliationType, "X")
					.Setup(x => x.AffiliationDescription, "AffiliationDescription")
					.Setup(x => x.RestrictionFlag, true)
					.Setup(x => x.ConditionFlag, false)
					.Setup(x => x.RestrictionOrConditionDescription, "Restriction Description")
					.Setup(x => x.LicenseFeeFlag, true)
					.Setup(x => x.LicenseFeeDescription, "LicenseDescription")
					.Setup(x => x.ResaleFlag, true)
					.Setup(x => x.ResaleDescription, "ResaleDescription")).Object
				)
				.Setup(x => x.Documents, new IImportDocument[] {
					MessageBuilderTestHelper.GetImportDocument("N380", "DOC1REFERENCE", new DateTime(2020, 10, 23)).Object,
					MessageBuilderTestHelper.GetImportDocument("X123", "DOC2REFERENCE", new DateTime(2020, 10, 24)).Object
				})
				.Setup(x => x.Lines, new ISCIPEDLine[] { goodsItemMock.Object });
		}

		void SetupHeader()
		{
			headerMock = new Mock<ISCIPEDHeader>()
				.Setup(x => x.MessageRole, "47")
				.Setup(x => x.DeclarationKind, "Y")
				.Setup(x => x.ReferenceNumber, "DEF12345")
				.Setup(x => x.LocalReferenceNumber, "ABC12345")
				.Setup(x => x.StartAccountingPeriodDate, new DateTime(2021, 12, 31))
				.Setup(x => x.EndAccountingPeriodDate, new DateTime(2022, 1, 15))
				.Setup(x => x.DeclarantIsConsigneeFlag, true)
				.Setup(x => x.LocalClearanceProcedure, "AUTHNUMBER")
				.Setup(x => x.ProcedureAuthorization, "ENDUSEAUTHNUMBER")
				.Setup(x => x.CurrencyCode, "EUR")
				.Setup(x => x.RepresentativeRelationshipFlag, "0")
				.Setup(x => x.DeclarationPlace, "Hamburg")
				.Setup(x => x.Declarant, (declarantMock = new Mock<IImportParty>()
					.Setup(x => x.Identification, new Mock<IPartyID>()
						.Setup(x => x.EoriNumber, "GR234567890")
						.Setup(x => x.EoriBranchSuffix, "0001")
						.Object)
					).Object)
				.Setup(x => x.Representative, (representativeMock = new Mock<IPartyID>()
					.Setup(x => x.EoriNumber, "GR345678901")
					.Setup(x => x.EoriBranchSuffix, "0002")
					).Object)
				.Setup(x => x.Principal, (principalMock = new Mock<IImportParty>()
					.Setup(x => x.Identification, new Mock<IPartyID>()
						.Setup(x => x.EoriNumber, "GR456789012")
						.Setup(x => x.EoriBranchSuffix, "0003")
						.Object)
					).Object)
				.Setup(x => x.ContactPerson, new Mock<IImportPartyContactPerson>()
					.Setup(x => x.MailAddress, "bob.baumeister@samplefreight.com")
					.Setup(x => x.PersonName, "Bob Baumeister")
					.Setup(x => x.PhoneNumber, "06131-477447")
					.Setup(x => x.Position, "Sachbearbeiter")
					.Object)
				.Setup(x => x.Bodies, new ISCIPEDBody[] { bodyMock.Object });
		}

		void SetupMessageHeader()
		{
			messageHeaderMock = new Mock<IImportMessageHeader>()
				.Setup(x => x.InterchangeSender, new Mock<IPartyID>()
					.Setup(x => x.EoriNumber, "DE8999783")
					.Setup(x => x.EoriBranchSuffix, "0000").Object)
				.Setup(x => x.InterchangeRecipientID, "DE005875")
				.Setup(x => x.AuthorisationNumber, "1234567890AUTH")
				.Setup(x => x.MessageGroup, "ZSZ")
				.Setup(x => x.Header, headerMock.Object)
				.Setup(x => x.PreparationDateAndTimeCET, new CentralEuropeanStandardDateAndTimeProvider(true));
		}

		Mock<ISCIPEDLine> GetLineMock(int lineNumber) => new Mock<ISCIPEDLine>()
			.Setup(x => x.SequenceNumber, lineNumber)
			.Setup(x => x.ReferencedSequenceNumber, 2)
			.Setup(x => x.MatterCode, "MATTER")
			.Setup(x => x.ArticleNumber, "ARTICLENUMBER")
			.Setup(x => x.InvoiceAmount, 123877.15031000m)
			.Setup(x => x.NetMassMeasure, 11866.40005000m)
			.Setup(x => x.NetMassMeasureSpecified, true)
			.Setup(x => x.OriginCountry, "CN")
			.Setup(x => x.DepartureCountry, "TW")
			.Setup(x => x.SupplementaryInformation, "Positionszusatz")
			.Setup(x => x.CompleteDeclarationFlag, true)
			.Setup(x => x.CommodityCode, "62034311000")
			.Setup(x => x.AdditionalProcedure, new[] { "A12", "A13" })
			.Setup(x => x.SupplementaryCodes, new[] { "A12", "A13" })
			.Setup(x => x.ForeignTradeStatisticsGoodsStatus, "1")
			.Setup(x => x.ForeignTradeStatisticsTransactionType, "T")
			.Setup(x => x.ForeignTradeStatisticsDestinationCountry, "DE")
			.Setup(x => x.ForeignTradeStatisticsDestinationFederalState, "RLP")
			.Setup(x => x.ForeignTradeStatisticsInlandTransportMode, "ROA")
			.Setup(x => x.ForeignTradeStatisticsQuantity, 109513m)
			.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure, 11860.5m)
			.Setup(x => x.ForeignTradeStatisticsAmount, GetAmount(18219, "NAR", "X").Object)
			.Setup(x => x.InwardMovementAmount, GetAmount(12.3m, "NAR", "X").Object)
			.Setup(x => x.CustomsValue, lineCustomsValueMock.Object)
			.Setup(x => x.AssessmentCustomsValue, 100628.61m)
			.Setup(x => x.AssessmentAmount, new IAmount[] { GetAmount(18219, "NAR", "X").Object })
			.Setup(x => x.AssessmentSpecificRate, new IImportSpecificRate[] {
				MessageBuilderTestHelper.MockImportSpecificRate("X", 10.02m).Object,
				MessageBuilderTestHelper.MockImportSpecificRate("X", 10.0260m).Object
			})
			.Setup(x => x.AssessmentContentInformation, new IContentInformation[] {
				MessageBuilderTestHelper.MockContentInformation("X", 0.01m).Object,
				MessageBuilderTestHelper.MockContentInformation("X", 0.0160m).Object
			})
			.Setup(x => x.ExciseDuty, new IExciseDuty[] {
				MessageBuilderTestHelper.MockExciseDuty("A123", 0.02m, 1626.28m, 18219, "NAR", "X").Object,
				MessageBuilderTestHelper.MockExciseDuty("A124", 0.03m, 1626.29m, 18219, "NAR", "X").Object
			})
			.Setup(x => x.RequestedPreferentialTreatment, "200")
			.Setup(x => x.Documents, new IImportLineDocument[] {
				new Mock<IImportLineDocument>()
					.Setup(x => x.Division, "4")
					.Setup(x => x.DocumentType, "7HHF")
					.Setup(x => x.ReferenceNumber, "COSU6271657530")
					.Setup(x => x.IssuingDate, new DateTime(2020, 08, 12))
					.Setup(x => x.AtHandFlag, "J")
					.Setup(x => x.WriteOff, GetAmount(1500, "NAR", "Z").Object).Object
			})
			.Setup(x => x.BorderTransportMeansMode, "1")
			.Setup(x => x.BorderTransportMeansType, "07")
			.Setup(x => x.BorderTransportMeansInformation, "123456789012345678")
			.Setup(x => x.BorderTransportMeansNationality, "DE");

		IImportPartyIdAddress Address => MessageBuilderTestHelper.GetImportPartyIdAddress("Max Mustermann", "55126", "Mainz", "Finthen", "Poststraße 1", "DE").Object;
	}
}
