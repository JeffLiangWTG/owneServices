using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CA.Messaging.Testing
{
	sealed class B3CusdecMessageBuilderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCombineRFFLISegmentWhenMoreThan999SegmentsExist()
		{
			var resourceRetriever = new EmbeddedResourceRetriever();
			var messageBuilder = new B3CusdecMessageBuilder<EDIMessage>(GetB3HeaderWithClassificationLines("X"), MessageSubTypes.Create);
			var expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Messaging.Testing.MessageBuilders.TestFiles.MoreThan999RFFLISegmentCombinedMessage.txt");
			var messageText = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "'\r\n");
			NUnit.Framework.Assert.That(messageText, CustomConstraints.MultilineASCIIEquals(expectedMessageText), "Empty B3 CUSDEC Message");
		}

		[ExpectNoExceptions]
		public void TestMessageText()
		{
			AssertFullyPopulatedB3Message(B3EntryTypeList.Codes.Confirming);
			AssertFullyPopulatedB3Message(B3EntryTypeList.Codes.AutomotiveP);
			AssertFullyPopulatedB3Message(B3EntryTypeList.Codes.AutomotiveS);

			foreach (var warehouseType in B3EntryTypeList.WarehouseEntryTypes)
			{
				AssertFullyPopulatedB3Message(warehouseType);
			}

			var messageBuilder = new B3CusdecMessageBuilder<EDIMessage>(GetEmptyB3Header(), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			NUnit.Framework.Assert.That(message.EM_MessageText.Replace("'", "'\r\n"), CustomConstraints.MultilineASCIIEquals(ExpectedEmptyMessage.Replace("'", "'\r\n")), "Empty B3 CUSDEC Message");

			messageBuilder = new B3CusdecMessageBuilder<EDIMessage>(GetEmptyB3Header(true), MessageSubTypes.Create);
			message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			NUnit.Framework.Assert.That(message.EM_MessageText.Replace("'", "'\r\n"), CustomConstraints.MultilineASCIIEquals(ExpectedEmptyAndNoLinesMessage.Replace("'", "'\r\n")), "Empty B3 CUSDEC Message");
		}

		IB3Header GetB3HeaderWithClassificationLines(string entryType)
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mock = new Mock<IB3Header>();
			mock.Setup(m => m.BatchNumber).Returns("B9999999");
			mock.Setup(m => m.B3TypeCode).Returns(entryType);
			mock.Setup(m => m.PaymentCode).Returns("I");
			mock.Setup(m => m.CBSAOffice).Returns("1234");
			mock.Setup(m => m.PortOfUnlading).Returns("2345");
			mock.Setup(m => m.WarehouseNumber).Returns("345");
			mock.Setup(m => m.TransactionNumber).Returns("123456789");
			mock.Setup(m => m.BusinessNumber).Returns("BUSINESSNUM1234");
			mock.Setup(m => m.GSTNumber).Returns("GSTNUMBER");
			mock.Setup(m => m.TransportMode).Returns("A");
			mock.Setup(m => m.CarrierCodeAtImportation).Returns("COD1");
			mock.Setup(m => m.Messages).Returns(messages);
			mock.Setup(m => m.SumPosAndNeg).Returns(false);

			var mockRelease1 = new Mock<IB3BRelease>();
			mockRelease1.Setup(m => m.CargoControlNumber).Returns("CNN 1");
			mockRelease1.Setup(m => m.DateOfRelease).Returns(testDate);
			var mockRelease2 = new Mock<IB3BRelease>();
			mockRelease2.Setup(m => m.CargoControlNumber).Returns("CNN2");
			mockRelease2.Setup(m => m.DateOfRelease).Returns(testDate);
			mock.Setup(m => m.B3BInputReleases).Returns(new[] { mockRelease1.Object, mockRelease2.Object });
			mock.Setup(m => m.TotalValueForDuty).Returns(testValue);
			mock.Setup(m => m.PositiveB3SubHeaders).Returns(new[] { GetSubHeader(1), GetSubHeader(2) });
			mock.Setup(m => m.PositiveClassificationLines).Returns(new[] { GetClassificationLine(1, 1, 2, 998), GetClassificationLine(1, 2, 499, 500) });
			mock.Setup(m => m.NegativeB3SubHeaders).Returns(new[] { GetSubHeader(1) });
			mock.Setup(m => m.NegativeClassificationLines).Returns(new[] { GetClassificationLine(1, 1, 2, 998) });

			var mockAmounts = new Mock<ITotalAmounts>();
			mockAmounts.Setup(m => m.TotalCustomsDuty).Returns(testValue);
			mockAmounts.Setup(m => m.TotalSIMAAssessment).Returns(testValue);
			mockAmounts.Setup(m => m.TotalExciseTax).Returns(testValue);
			mockAmounts.Setup(m => m.TotalGST).Returns(testValue);
			mockAmounts.Setup(m => m.TotalAllDutyAndTaxes).Returns(testValue);

			mock.Setup(m => m.PositiveTotalAmounts).Returns(mockAmounts.Object);
			mock.Setup(m => m.NegativeTotalAmounts).Returns(mockAmounts.Object);
			return mock.Object;
		}

		IClassificationLine1 GetClassificationLine(short num, int subHeaderNum, int invoiceCrossReferenceCount1, int invoiceCrossReferenceCount2)
		{
			var mockLine = new Mock<IClassificationLine1>();
			mockLine.Setup(m => m.B3LineNumber).Returns(num);
			mockLine.Setup(m => m.RecordIdentifier).Returns("POS");
			mockLine.Setup(m => m.B3SubHeaderNumber).Returns(subHeaderNum);
			mockLine.Setup(m => m.ClassificationNumber).Returns("2");
			mockLine.Setup(m => m.ValueForDutyCode).Returns("123");
			mockLine.Setup(m => m.TariffCode).Returns("10.10.10.10");
			mockLine.Setup(m => m.ValueForCurrency).Returns(testValue);
			mockLine.Setup(m => m.ValueForDuty).Returns(testValue);
			mockLine.Setup(m => m.ValueForTax).Returns(testValue);
			mockLine.Setup(m => m.AuthorityNumber).Returns("AUTHORITYNUMBER");
			mockLine.Setup(m => m.TRSNumber).Returns("TRSNUMBER");
			mockLine.Setup(m => m.PartNumberDescriptions).Returns(new ZString[] { "NUMBER DESCRIPTION UP TO 39 CHARACTERS1NUMBER DESCRIPTION UP TO 39 CHARS" });

			var invoiceCrossReferenceList = new ArrayList();
			for (int i = 1; i <= invoiceCrossReferenceCount1; i++)
			{
				var mockInvoice1 = new Mock<IInvoiceCrossReference>();
				mockInvoice1.Setup(m => m.InvoiceLineNumber).Returns(i);
				mockInvoice1.Setup(m => m.InvoicePageNumber).Returns(1);
				mockInvoice1.Setup(m => m.InvoiceValue).Returns(testValue);
				invoiceCrossReferenceList.Add(mockInvoice1.Object);
			}

			for (int i = 1; i <= invoiceCrossReferenceCount2; i++)
			{
				var mockInvoice1 = new Mock<IInvoiceCrossReference>();
				mockInvoice1.Setup(m => m.InvoiceLineNumber).Returns(i);
				mockInvoice1.Setup(m => m.InvoicePageNumber).Returns(2);
				mockInvoice1.Setup(m => m.InvoiceValue).Returns(testValue);
				invoiceCrossReferenceList.Add(mockInvoice1.Object);
			}

			mockLine.Setup(m => m.InvoiceCrossReferences).Returns(() => { return (IEnumerable<IInvoiceCrossReference>)invoiceCrossReferenceList.ToArray(typeof(IInvoiceCrossReference)); });
			mockLine.Setup(m => m.SIMACode).Returns("123");
			mockLine.Setup(m => m.SIMAAssessment).Returns(testValue);
			mockLine.Setup(m => m.ExciseTaxRate).Returns(15);
			mockLine.Setup(m => m.ExciseTaxRateType).Returns(RateTypes.Codes.Specific);
			mockLine.Setup(m => m.ExciseExemptionCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.ExciseTaxAmount).Returns(testValue);
			mockLine.Setup(m => m.IsDummyExciseTaxRate).Returns(false);
			mockLine.Setup(m => m.RateOfGST).Returns(15);
			mockLine.Setup(m => m.GSTRateType).Returns(RateTypes.Codes.AdValorem);
			mockLine.Setup(m => m.GSTExemptionCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.GSTAmount).Returns(testValue);
			mockLine.Setup(m => m.ClassificationLines).Returns(new[] { GetClassificationLine(num), GetClassificationLine(num) });
			return mockLine.Object;
		}

		[ExpectNoExceptions]
		void AssertFullyPopulatedB3Message(string entryType)
		{
			entry = entryType;
			var b3Header = GetB3Header(entryType);
			var messageBuilder = new B3CusdecMessageBuilder<EDIMessage>(b3Header, MessageSubTypes.Create);
			var expectedMessage = ExpectedMessage.Replace("'", "\r\n");
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "\r\n");
			NUnit.Framework.Assert.That(message, CustomConstraints.MultilineASCIIEquals(expectedMessage), "Fully Populated B3 CUSDEC Message, Entry Type : " + entry);
		}

		#region Fully Populated

		#region Expected Message

		string ExpectedMessage
		{
			get
			{
				not1330 = !new[] { "13", "20", "21", "22", "30" }.Contains(entry);
				not2130 = !new[] { "21", "22", "30" }.Contains(entry);
				var is20 = entry == "20";

				var builder = new ZStringBuilder();
				/*Message Header*/
				builder.Append("UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:S:99B:UN");
				/*B3 Type Code*/
				builder.Append("BGM+:::" + entry + "+B9999999+9");
				/*Payment Code*/
				builder.Append("CST++I");
				/*CBSA Office*/
				builder.Append("LOC+41+1234");
				/*Port of Unlading*/
				if (not1330)
				{
					builder.Append("LOC+11+2345");
				}
				/*Warehouse Number*/
				builder.Append("LOC+18+345");
				/*Transaction Number*/
				builder.Append("RFF+TN:123456789");
				/*Business Number*/
				builder.Append("RFF+ARA:BUSINESSNUM1234");
				/*GST Number*/
				builder.Append("RFF+AEA:GSTNUMBER");
				/*Mode of Transport*/
				if (not1330)
				{
					builder.Append("TDT+11++A++COD1");
				}

				if (is20)
				{
					/*CCN 1*/
					builder.Append("DOC+785");
					/*Date of Release 1*/
					builder.Append("DTM+204:20091010:102");
				}
				else
				{
					/*CCN 1*/
					if (not1330)
					{
						builder.Append("DOC+785+CNN1");
					}
					/*Date of Release 1*/
					if (not1330)
					{
						builder.Append("DTM+204:20091010:102");
					}
					/*CCN 2*/
					if (not1330)
					{
						builder.Append("DOC+785+CNN2");
					}
					/*Date of Release 2*/
					if (not1330)
					{
						builder.Append("DTM+204:20091010:102");
					}
				}
				/*Total Value for Duty*/
				if (not1330)
				{
					builder.Append("MOA+43:123");
				}
				/*Section control */
				builder.Append("UNS+D");

				/*Sub Header 1*/
				AppendSubHeader(builder, 1);
				/*Sub Header 2*/
				AppendSubHeader(builder, 2);
				/*Classification Line 1*/
				AppendClassificationLine(builder, 1, 1);
				/*Classification Line 2*/
				AppendClassificationLine(builder, 2, 1);
				/*Classification Line 3*/
				AppendClassificationLine(builder, 1, 2);
				/*Classification Line 4*/
				AppendClassificationLine(builder, 2, 2);
				/*Classification Line 3*/
				AppendClassificationLine(builder, 1, 2, true);
				/*Classification Line 3*/
				AppendClassificationLine(builder, 1, 2, isForDummyExciseTaxRate: true);
				/*Section control*/
				builder.Append("UNS+S");

				/*Total Positive Amounts*/
				AppendTotalAmounts(builder, true);
				/*Total Negative Amounts*/
				AppendTotalAmounts(builder, false);
				var dictionary = new Dictionary<string, int> { { "AB", 212 }, { "10", 208 }, { "13", 191 }, { "20", 193 }, { "21", 131 }, { "22", 131 }, { "30", 131 }, { "S", 224 }, { "P", 224 } };
				/*Message Trailer*/
				builder.Append("UNT+" + dictionary[entry] + "+<<MSGNO PLACEHOLDER>>'");

				return builder.ToStringWithDelimiterBetweenAppends("'");
			}
		}

		void AppendSubHeader(ZStringBuilder builder, int num)
		{
			/*B3 Sub-Header No*/
			builder.Append("DMS+" + num);
			/*Freight Charges*/
			if (not1330)
			{
				builder.Append("MOA+64:123");
			}
			/*Vendor*/
			builder.Append("NAD+SE++VENDOR NAME++++UCA+12345");
			/*Customs Invoice*/
			builder.Append("DOC+935");
			/*Date of Direct Shipment*/
			builder.Append("DTM+129:20091010:102");
			/*Country/Region of Origin
			  Place of Export
			  US Port of Exit*/
			builder.Append(not1330 ? "LOC+27+US+AN24+12345" : "LOC+27+US+AN24");
			/*Tariff Treatment Code*/
			builder.Append(entry == "30" || not2130 ? "PAT+1+CONSIGN:::12+66::D:23" : "PAT+1+CONSIGN:::12");
			/*Currency Code*/
			builder.Append("MOA+6::CAD");
		}

		void AppendClassificationLine(ZStringBuilder builder, int num, int subHeaderNum, bool isForExemptTest = false, bool isForDummyExciseTaxRate = false)
		{
			/*B3 Line Number*/
			builder.Append(string.Format("CST+{0}+POS+{1}+2+123+10.10.10.10", num, subHeaderNum));
			/*Value for Currency*/
			builder.Append("MOA+40:12345");
			/*Value for Duty*/
			builder.Append("MOA+43:12345");
			/*Value for Tax*/
			if (not2130)
			{
				builder.Append("MOA+125:12345");
			}
			/*Authority Number*/
			builder.Append("RFF+ABG:AUTHORITYNUMBER");
			/*TRS Number*/
			if (not2130)
			{
				builder.Append("RFF+ABA:TRSNUMBER:" + num);
			}

			if (new[] { "S", "P" }.Contains(entry))
			{
				/*B3 Line Number*/
				builder.Append("RFF+MF::" + num);
				/*Part Number*/
				builder.Append("GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1+NUMBER DESCRIPTION UP TO 39 CHARS");
			}
			/*Invoice Pg/Line No 1*/
			builder.Append("RFF+LI:1:1");
			/*Invoice Value 1*/
			builder.Append("MOA+38:12345");
			/*Invoice Pg/Line No 2*/
			builder.Append("RFF+LI:2:2");
			/*Invoice Value 2*/
			builder.Append("MOA+38:12345");
			/*SIMA Code*/
			builder.Append("TAX+1+ADD++123");
			if (isForExemptTest)
			{
				/*SIMA Assessment*/
				builder.Append("MOA+46:000");
				if (not2130)
				{
					/*Excise Tax Rate*/
					builder.Append("TAX+1+EXC++99");
					/*Excise Tax Amount*/
					builder.Append("MOA+161:000");
					/*Rate of GST*/
					builder.Append("TAX+7+VAT++48");
					/*GST Amount*/
					builder.Append("MOA+1:000");
				}
			}
			else
			{
				/*SIMA Assessment*/
				builder.Append("MOA+46:12345");
				if (not2130)
				{
					/*Excise Tax Rate*/
					builder.Append("TAX+1+EXC++15.00");
					if (isForDummyExciseTaxRate)
					{
						/*Excise Tax Amount*/
						builder.Append("MOA+161:000");
					}
					else
					{
						/*Excise Tax Amount*/
						builder.Append("MOA+161:12345");
					}
					/*Rate of GST*/
					builder.Append("TAX+7+VAT++15.0");
					/*GST Amount*/
					builder.Append("MOA+1:12345");
				}
			}
			/*Classification Line 1*/
			AppendClassificationLine2(builder, num);
			/*Classification Line 2*/
			AppendClassificationLine2(builder, num);
		}

		void AppendClassificationLine2(ZStringBuilder builder, int num)
		{
			/*B3 Line Number*/
			builder.Append("GIR+1+" + num);
			/*Class. Line Quantity*/
			builder.Append("MEA+AAR++KGM:123450");
			/*Weight in KGM*/
			if (not1330)
			{
				builder.Append("MEA+AAA++KGM:123");
			}
			/*Customs Duty Rate*/
			if (not2130)
			{
				builder.Append("TAX+5+++12.00");
			}
			/*Customs Duty Amount*/
			if (not2130)
			{
				builder.Append("MOA+155:12345");
			}
			/*Previous Trans. Number*/
			if (num > 1 && entry != "10")
			{
				builder.Append("DOC+998+12345678901234::" + (num - 1));
			}
		}

		void AppendTotalAmounts(ZStringBuilder builder, bool isPositive)
		{
			var type = isPositive ? "K90" : "K92";
			/*Customs Duty*/
			builder.Append("TAX+5+:::" + type);
			/*Total Customs Duty*/
			builder.Append("MOA+155:12345");
			if (isPositive)
			{
				/*Other Charges*/
				builder.Append("TAX+1+:::" + type);
				/*Total SIM	A Assessment*/
				builder.Append("MOA+105:12345");
			}
			/*Excise Tax*/
			builder.Append("TAX+3+:::" + type);
			/*Total Excise Tax*/
			builder.Append("MOA+4:12345");
			/*GST*/
			builder.Append("TAX+7+:::" + type);
			/*Total GST*/
			builder.Append("MOA+1:12345");
			/*Total All*/
			builder.Append("TAX+4+:::" + type);
			/*Total All Duty/Taxes*/
			builder.Append("MOA+176:12345");
		}

		#endregion

		#region B3 Header

		IB3Header GetB3Header(string entryType)
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mock = new Mock<IB3Header>();
			mock.Setup(m => m.BatchNumber).Returns("B9999999");
			mock.Setup(m => m.B3TypeCode).Returns(entry);
			mock.Setup(m => m.PaymentCode).Returns("I");
			mock.Setup(m => m.CBSAOffice).Returns("1234");
			mock.Setup(m => m.PortOfUnlading).Returns("2345");
			mock.Setup(m => m.WarehouseNumber).Returns("345");
			mock.Setup(m => m.TransactionNumber).Returns("123456789");
			mock.Setup(m => m.BusinessNumber).Returns("BUSINESSNUM1234");
			mock.Setup(m => m.GSTNumber).Returns("GSTNUMBER");
			mock.Setup(m => m.TransportMode).Returns("A");
			mock.Setup(m => m.CarrierCodeAtImportation).Returns("COD1");
			mock.Setup(m => m.Messages).Returns(messages);
			mock.Setup(m => m.SumPosAndNeg).Returns(false);

			var mockRelease1 = new Mock<IB3BRelease>();
			if (entryType != B3EntryTypeList.Codes.ExWarehouse20)
			{
				mockRelease1.Setup(m => m.CargoControlNumber).Returns("CNN 1");
			}
			else
			{
				mockRelease1.Setup(m => m.CargoControlNumber).Returns(ZString.Empty);
			}
			mockRelease1.Setup(m => m.DateOfRelease).Returns(testDate);
			if (entryType != B3EntryTypeList.Codes.ExWarehouse20)
			{
				var mockRelease2 = new Mock<IB3BRelease>();
				mockRelease2.Setup(m => m.CargoControlNumber).Returns("CNN2");
				mockRelease2.Setup(m => m.DateOfRelease).Returns(testDate);
				mock.Setup(m => m.B3BInputReleases).Returns(new[] { mockRelease1.Object, mockRelease2.Object });
			}
			else
			{
				mock.Setup(m => m.B3BInputReleases).Returns(new[] { mockRelease1.Object });
			}
			mock.Setup(m => m.TotalValueForDuty).Returns(testValue);
			mock.Setup(m => m.PositiveB3SubHeaders).Returns(new[] { GetSubHeader(1), GetSubHeader(2) });
			mock.Setup(m => m.PositiveClassificationLines).Returns(new[] { GetClassificationLine(1, 1), GetClassificationLine(2, 1), GetClassificationLine(1, 2), GetClassificationLine(2, 2), GetClassificationLine(1, 2, true), GetClassificationLine(1, 2, isForDummyExciseTaxRate: true) });
			mock.Setup(m => m.NegativeB3SubHeaders).Returns(Array.Empty<IB3SubHeader>());
			mock.Setup(m => m.NegativeClassificationLines).Returns(Array.Empty<IClassificationLine1>());

			var mockAmounts = new Mock<ITotalAmounts>();
			mockAmounts.Setup(m => m.TotalCustomsDuty).Returns(testValue);
			mockAmounts.Setup(m => m.TotalSIMAAssessment).Returns(testValue);
			mockAmounts.Setup(m => m.TotalExciseTax).Returns(testValue);
			mockAmounts.Setup(m => m.TotalGST).Returns(testValue);
			mockAmounts.Setup(m => m.TotalAllDutyAndTaxes).Returns(testValue);

			mock.Setup(m => m.PositiveTotalAmounts).Returns(mockAmounts.Object);
			mock.Setup(m => m.NegativeTotalAmounts).Returns(mockAmounts.Object);
			return mock.Object;
		}

		IB3SubHeader GetSubHeader(int num)
		{
			var mockSubHeader = new Mock<IB3SubHeader>();
			mockSubHeader.Setup(m => m.B3SubHeaderNumber).Returns(num);
			mockSubHeader.Setup(m => m.FreightCharges).Returns(testValue);

			var vendor = Factory.New<JobDocAddress>();
			vendor.E2_AddressOverride = true;
			vendor.E2_RN_NKCountryCode = "US";
			vendor.E2_CompanyName = "VENDOR NAME";
			vendor.E2_State = "CA";
			vendor.E2_Postcode = " 123456";
			mockSubHeader.Setup(m => m.Vendor).Returns(vendor);
			mockSubHeader.Setup(m => m.DateOfDirectShipment).Returns(testDate);
			mockSubHeader.Setup(m => m.CountryOfOrigin).Returns("US");
			mockSubHeader.Setup(m => m.PlaceOfExport).Returns("AN24");
			mockSubHeader.Setup(m => m.USPortOfExit).Returns("12345");
			mockSubHeader.Setup(m => m.TariffTreatmentCode).Returns("12");
			mockSubHeader.Setup(m => m.TimeLimitUnit).Returns("D");
			mockSubHeader.Setup(m => m.B3TimeLimits).Returns(new ZInt("23"));
			mockSubHeader.Setup(m => m.CurrencyCode).Returns("CAD");
			mockSubHeader.Setup(m => m.VendorStateAndZip).Returns(new VendorStateAndZipStruct("UCA", " 123456"));

			return mockSubHeader.Object;
		}

		IClassificationLine1 GetClassificationLine(short num, int subHeaderNum, bool isForExemptTest = false, bool isForDummyExciseTaxRate = false)
		{
			var mockLine = new Mock<IClassificationLine1>();
			mockLine.Setup(m => m.B3LineNumber).Returns(num);
			mockLine.Setup(m => m.RecordIdentifier).Returns("POS");
			mockLine.Setup(m => m.B3SubHeaderNumber).Returns(subHeaderNum);
			mockLine.Setup(m => m.ClassificationNumber).Returns("2");
			mockLine.Setup(m => m.ValueForDutyCode).Returns("123");
			mockLine.Setup(m => m.TariffCode).Returns("10.10.10.10");
			mockLine.Setup(m => m.ValueForCurrency).Returns(testValue);
			mockLine.Setup(m => m.ValueForDuty).Returns(testValue);
			mockLine.Setup(m => m.ValueForTax).Returns(testValue);
			mockLine.Setup(m => m.AuthorityNumber).Returns("AUTHORITYNUMBER");
			mockLine.Setup(m => m.TRSNumber).Returns("TRSNUMBER");
			mockLine.Setup(m => m.PartNumberDescriptions).Returns(new ZString[] { "NUMBER DESCRIPTION UP TO 39 CHARACTERS1NUMBER DESCRIPTION UP TO 39 CHARS" });

			var mockInvoice1 = new Mock<IInvoiceCrossReference>();
			mockInvoice1.Setup(m => m.InvoiceLineNumber).Returns(1);
			mockInvoice1.Setup(m => m.InvoicePageNumber).Returns(1);
			mockInvoice1.Setup(m => m.InvoiceValue).Returns(testValue);
			var mockInvoice2 = new Mock<IInvoiceCrossReference>();
			mockInvoice2.Setup(m => m.InvoiceLineNumber).Returns(2);
			mockInvoice2.Setup(m => m.InvoicePageNumber).Returns(2);
			mockInvoice2.Setup(m => m.InvoiceValue).Returns(testValue);
			mockLine.Setup(m => m.InvoiceCrossReferences).Returns(new[] { mockInvoice1.Object, mockInvoice2.Object });

			mockLine.Setup(m => m.SIMACode).Returns("123");
			if (isForExemptTest)
			{
				mockLine.Setup(m => m.SIMAAssessment).Returns(0);
				mockLine.Setup(m => m.ExciseTaxRate).Returns(0);
				mockLine.Setup(m => m.ExciseTaxRateType).Returns(RateTypes.Codes.Specific);
				mockLine.Setup(m => m.ExciseExemptionCode).Returns("99");
				mockLine.Setup(m => m.ExciseTaxAmount).Returns(ZDecimal.Zero);
				mockLine.Setup(m => m.IsDummyExciseTaxRate).Returns(false);
				mockLine.Setup(m => m.RateOfGST).Returns(0);
				mockLine.Setup(m => m.GSTRateType).Returns(RateTypes.Codes.AdValorem);
				mockLine.Setup(m => m.GSTExemptionCode).Returns("48");
				mockLine.Setup(m => m.GSTAmount).Returns(0);
			}
			else
			{
				mockLine.Setup(m => m.SIMAAssessment).Returns(testValue);
				mockLine.Setup(m => m.ExciseTaxRate).Returns(15);
				mockLine.Setup(m => m.ExciseTaxRateType).Returns(RateTypes.Codes.Specific);
				mockLine.Setup(m => m.ExciseExemptionCode).Returns(ZString.Empty);
				mockLine.Setup(m => m.ExciseTaxAmount).Returns(isForDummyExciseTaxRate ? ZDecimal.Zero : testValue);
				mockLine.Setup(m => m.IsDummyExciseTaxRate).Returns(isForDummyExciseTaxRate);
				mockLine.Setup(m => m.RateOfGST).Returns(15);
				mockLine.Setup(m => m.GSTRateType).Returns(RateTypes.Codes.AdValorem);
				mockLine.Setup(m => m.GSTExemptionCode).Returns(ZString.Empty);
				mockLine.Setup(m => m.GSTAmount).Returns(testValue);
			}
			mockLine.Setup(m => m.ClassificationLines).Returns(new[] { GetClassificationLine(num), GetClassificationLine(num) });
			return mockLine.Object;
		}

		IClassificationLine2 GetClassificationLine(int num)
		{
			var mockLine = new Mock<IClassificationLine2>();
			mockLine.Setup(m => m.B3LineNumber).Returns(num);
			mockLine.Setup(m => m.UnitOfMeasureCode).Returns("KGM");
			mockLine.Setup(m => m.ClassificationLineQuantity).Returns(testValue);
			mockLine.Setup(m => m.WeightInKGM).Returns(testValue);
			mockLine.Setup(m => m.CustomsDutyRate).Returns(12);
			mockLine.Setup(m => m.CustomsDutyRateType).Returns(RateTypes.Codes.Specific);
			mockLine.Setup(m => m.CustomsDutyAmount).Returns(testValue);
			mockLine.Setup(m => m.PreviousTransactionNumber).Returns("12345678901234");
			mockLine.Setup(m => m.PreviousLineNumber).Returns(num - 1);
			return mockLine.Object;
		}

		readonly ZDateTime testDate = new ZDateTime(2009, 10, 10);
		readonly ZDecimal testValue = 123.45m;
		bool not1330;
		bool not2130;
		string entry;

		#endregion

		#endregion

		#region Empty

		#region Expected Message

		string ExpectedEmptyMessage
		{
			get
			{
				var builder = new ZStringBuilder();

				/*Message Header*/
				builder.Append("UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:S:99B:UN");
				/*B3 Type Code*/
				builder.Append("BGM+++9");
				/*CBSA Office*/
				builder.Append("LOC+41");
				/*Transaction Number*/
				builder.Append("RFF+TN");
				/*Business Number*/
				builder.Append("RFF+ARA");
				/*CCN 1*/
				builder.Append("DOC+785");
				/*Section control */
				builder.Append("UNS+D");

				/*Sub Header 1*/
				/*B3 Sub-Header No*/
				builder.Append("DMS+0");
				/*Vendor*/
				builder.Append("NAD+SE");
				/*Customs Invoice*/
				builder.Append("DOC+935");
				/*Country/Region of Origin
				  Place of Export*/
				builder.Append("LOC+27");
				/*Tariff Treatment Code*/
				builder.Append("PAT+1+CONSIGN");
				/*Currency Code*/
				builder.Append("MOA+6");

				/*Classification Line 1*/
				/*B3 Line Number*/
				builder.Append("CST+0++0");
				/*Value for Currency*/
				builder.Append("MOA+40:000");
				/*Value for Duty*/
				builder.Append("MOA+43:000");
				/*Invoice Pg/Line No 1*/
				builder.Append("RFF+LI:0:0");
				/*Invoice Value 1*/
				builder.Append("MOA+38:000");
				/*Rate of GST*/
				builder.Append("TAX+7+VAT++0.0");
				/*GST Amount*/
				builder.Append("MOA+1:000");
				/*Classification Line 2*/
				/*B3 Line Number*/
				builder.Append("GIR+1+0");
				/*Customs Duty Rate*/
				builder.Append("TAX+5+++0.0");
				/*Customs Duty Amount*/
				builder.Append("MOA+155:000");
				/*Section control*/
				builder.Append("UNS+S");

				/*Total All*/
				builder.Append("TAX+4+:::K90");
				/*Total All Duty/Taxes*/
				builder.Append("MOA+176:000");

				/*Message Trailer*/
				builder.Append("UNT+27+<<MSGNO PLACEHOLDER>>'");

				return builder.ToStringWithDelimiterBetweenAppends("'");
			}
		}

		string ExpectedEmptyAndNoLinesMessage
		{
			get
			{
				var builder = new ZStringBuilder();

				/*Message Header*/
				builder.Append("UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:S:99B:UN");
				/*B3 Type Code*/
				builder.Append("BGM+++9");
				/*CBSA Office*/
				builder.Append("LOC+41");
				/*Transaction Number*/
				builder.Append("RFF+TN");
				/*Business Number*/
				builder.Append("RFF+ARA");
				/*CCN 1*/
				builder.Append("DOC+785");
				/*Section control */
				builder.Append("UNS+D");

				/*Sub Header 1*/
				/*B3 Sub-Header No*/
				builder.Append("DMS+0");
				/*Vendor*/
				builder.Append("NAD+SE");
				/*Customs Invoice*/
				builder.Append("DOC+935");
				/*Country/Region of Origin
				  Place of Export*/
				builder.Append("LOC+27");
				/*Tariff Treatment Code*/
				builder.Append("PAT+1+CONSIGN");
				/*Currency Code*/
				builder.Append("MOA+6");

				/*Classification Line 1*/
				/*B3 Line Number*/
				builder.Append("CST+0++0");
				/*Value for Currency*/
				builder.Append("MOA+40:000");
				/*Value for Duty*/
				builder.Append("MOA+43:000");
				/*Invoice Pg/Line No 1*/
				builder.Append("RFF+LI:0:0");
				/*Invoice Value 1*/
				builder.Append("MOA+38:000");
				/*Rate of GST*/
				builder.Append("TAX+7+VAT++0.0");
				/*GST Amount*/
				builder.Append("MOA+1:000");
				/*Classification Line 2*/
				/*B3 Line Number*/
				builder.Append("GIR+1+0");
				/*Section control*/
				builder.Append("UNS+S");

				/*Total All*/
				builder.Append("TAX+4+:::K90");
				/*Total All Duty/Taxes*/
				builder.Append("MOA+176:000");

				/*Message Trailer*/
				builder.Append("UNT+25+<<MSGNO PLACEHOLDER>>'");

				return builder.ToStringWithDelimiterBetweenAppends("'");
			}
		}

		#endregion

		#region B3 Header

		IB3Header GetEmptyB3Header(bool noLines = false)
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mock = new Mock<IB3Header>();
			mock.Setup(m => m.BatchNumber).Returns(ZString.Empty);
			mock.Setup(m => m.B3TypeCode).Returns(ZString.Empty);
			mock.Setup(m => m.PaymentCode).Returns(ZString.Empty);
			mock.Setup(m => m.CBSAOffice).Returns(ZString.Empty);
			mock.Setup(m => m.PortOfUnlading).Returns(ZString.Empty);
			mock.Setup(m => m.WarehouseNumber).Returns(ZString.Empty);
			mock.Setup(m => m.TransactionNumber).Returns(ZString.Empty);
			mock.Setup(m => m.BusinessNumber).Returns(ZString.Empty);
			mock.Setup(m => m.GSTNumber).Returns(ZString.Empty);
			mock.Setup(m => m.TransportMode).Returns(ZString.Empty);
			mock.Setup(m => m.CarrierCodeAtImportation).Returns(ZString.Empty);
			mock.Setup(m => m.Messages).Returns(messages);
			mock.Setup(m => m.SumPosAndNeg).Returns(false);

			var mockRelease1 = new Mock<IB3BRelease>();
			mockRelease1.Setup(m => m.CargoControlNumber).Returns(ZString.Empty);
			mockRelease1.Setup(m => m.DateOfRelease).Returns(ZDateTime.Empty);
			mock.Setup(m => m.B3BInputReleases).Returns(new[] { mockRelease1.Object });
			mock.Setup(m => m.TotalValueForDuty).Returns(ZDecimal.Zero);

			var mockSubHeader = new Mock<IB3SubHeader>();
			mockSubHeader.Setup(m => m.B3SubHeaderNumber).Returns(ZInt.Zero);
			mockSubHeader.Setup(m => m.FreightCharges).Returns(ZDecimal.Zero);
			mockSubHeader.Setup(m => m.Vendor).Returns(Factory.New<JobDocAddress>());
			mockSubHeader.Setup(m => m.DateOfDirectShipment).Returns(ZDateTime.Empty);
			mockSubHeader.Setup(m => m.CountryOfOrigin).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.PlaceOfExport).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.USPortOfExit).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.TariffTreatmentCode).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.TimeLimitUnit).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.B3TimeLimits).Returns(ZInt.Zero);
			mockSubHeader.Setup(m => m.CurrencyCode).Returns(ZString.Empty);
			mockSubHeader.Setup(m => m.VendorStateAndZip).Returns(new VendorStateAndZipStruct());
			mock.Setup(m => m.PositiveB3SubHeaders).Returns(new[] { mockSubHeader.Object });
			mock.Setup(m => m.NegativeB3SubHeaders).Returns(Array.Empty<IB3SubHeader>());

			var mockLine = new Mock<IClassificationLine1>();
			mockLine.Setup(m => m.B3LineNumber).Returns(ZShort.Zero);
			mockLine.Setup(m => m.RecordIdentifier).Returns(ZString.Empty);
			mockLine.Setup(m => m.B3SubHeaderNumber).Returns(ZInt.Zero);
			mockLine.Setup(m => m.ClassificationNumber).Returns(ZString.Empty);
			mockLine.Setup(m => m.ValueForDutyCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.TariffCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.ValueForCurrency).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.ValueForDuty).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.ValueForTax).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.AuthorityNumber).Returns(ZString.Empty);
			mockLine.Setup(m => m.TRSNumber).Returns(ZString.Empty);
			mockLine.Setup(m => m.PartNumberDescriptions).Returns(Array.Empty<ZString>());

			var mockInvoice1 = new Mock<IInvoiceCrossReference>();
			mockInvoice1.Setup(m => m.InvoiceLineNumber).Returns(ZInt.Zero);
			mockInvoice1.Setup(m => m.InvoicePageNumber).Returns(ZInt.Zero);
			mockInvoice1.Setup(m => m.InvoiceValue).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.InvoiceCrossReferences).Returns(new[] { mockInvoice1.Object });
			mockLine.Setup(m => m.SIMACode).Returns(ZString.Empty);
			mockLine.Setup(m => m.SIMAAssessment).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.ExciseTaxRate).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.ExciseTaxRateType).Returns(RateTypes.Codes.Specific);
			mockLine.Setup(m => m.ExciseExemptionCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.ExciseTaxAmount).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.IsDummyExciseTaxRate).Returns(false);
			mockLine.Setup(m => m.RateOfGST).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.GSTRateType).Returns(RateTypes.Codes.AdValorem);
			mockLine.Setup(m => m.GSTAmount).Returns(ZDecimal.Zero);
			mockLine.Setup(m => m.GSTExemptionCode).Returns(ZString.Empty);

			var mockLine2 = new Mock<IClassificationLine2>();
			mockLine2.Setup(m => m.B3LineNumber).Returns(ZInt.Zero);
			mockLine2.Setup(m => m.UnitOfMeasureCode).Returns(ZString.Empty);
			mockLine2.Setup(m => m.ClassificationLineQuantity).Returns(ZDecimal.Zero);
			mockLine2.Setup(m => m.WeightInKGM).Returns(ZDecimal.Zero);
			mockLine2.Setup(m => m.CustomsDutyRate).Returns(ZDecimal.Zero);
			mockLine2.Setup(m => m.CustomsDutyRateType).Returns(RateTypes.Codes.AdValorem);
			mockLine2.Setup(m => m.CustomsDutyAmount).Returns(ZDecimal.Zero);
			mockLine2.Setup(m => m.PreviousTransactionNumber).Returns(ZString.Empty);
			mockLine2.Setup(m => m.PreviousLineNumber).Returns(ZInt.Zero);
			if (noLines)
			{
				mockLine.Setup(m => m.ClassificationLines).Returns(Array.Empty<IClassificationLine2>());
			}
			else
			{
				mockLine.Setup(m => m.ClassificationLines).Returns(new[] { mockLine2.Object });
			}
			mock.Setup(m => m.PositiveClassificationLines).Returns(new[] { mockLine.Object });
			mock.Setup(m => m.NegativeClassificationLines).Returns(Array.Empty<IClassificationLine1>());

			var mockAmounts = new Mock<ITotalAmounts>();
			mockAmounts.Setup(m => m.TotalCustomsDuty).Returns(ZDecimal.Zero);
			mockAmounts.Setup(m => m.TotalSIMAAssessment).Returns(ZDecimal.Zero);
			mockAmounts.Setup(m => m.TotalExciseTax).Returns(ZDecimal.Zero);
			mockAmounts.Setup(m => m.TotalGST).Returns(ZDecimal.Zero);
			mockAmounts.Setup(m => m.TotalAllDutyAndTaxes).Returns(ZDecimal.Zero);
			mock.Setup(m => m.PositiveTotalAmounts).Returns(mockAmounts.Object);
			mock.Setup(m => m.NegativeTotalAmounts).Returns(mockAmounts.Object);

			return mock.Object;
		}

		#endregion

		#endregion
	}
}
