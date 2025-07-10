using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDataElementProvider))]
	public class KoreaSouthEInvoicingDataElementProviderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			return new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo);
		}

		public void TestEvaluateWithDifferentSeparator()
		{
			var info = new AdditionalInfo()
			{
				InvoiceePassportNo = "Dummy AAA",
				OriginalIssueID = "Dummy CCC",
			};

			AssertEvaluateWithDifferentInput(new char[] { '!', '@', '#' });
			AssertEvaluateWithDifferentInput(new char[] { '!', '@', '#', 'A' });
			AssertEvaluateWithDifferentInput(new char[] { '!', '@', '#', 'A', '\r', '\n', ' ' });

			void AssertEvaluateWithDifferentInput(IEnumerable<char> characters)
			{
				var outputseparator = string.Join("", characters);

				var input1 = "[AAA: <PassportNumber>]{0}[BBB: <ForeignerRegistrationNumber>]{0}[CCC: <OriginalApprovalNumber>]";
				AssertEvaluateWithDifferentSeparator(characters, input1, $"AAA: Dummy AAA{outputseparator}CCC: Dummy CCC");

				var input2_1 = "[AAA: <PassportNumber>]{0}[BBB: <ForeignerRegistrationNumber>]";
				var input2_2 = "[AAA: <PassportNumber>]{0}[BBB: <ForeignerRegistrationNumber>][CCC: <OriginalApprovalNumber>]";
				AssertEvaluateWithDifferentSeparator(characters, input2_1, $"AAA: Dummy AAA");
				AssertEvaluateWithDifferentSeparator(characters, input2_2, $"AAA: Dummy AAA{outputseparator}CCC: Dummy CCC");

				var input3_1 = "{0}[AAA: <PassportNumber>]";
				var input3_2 = "{0}[BBB: <ForeignerRegistrationNumber>]";
				var input3_3 = "{0}[AAA: <PassportNumber>][BBB: <ForeignerRegistrationNumber>]";
				var input3_4 = "{0}[BBB: <ForeignerRegistrationNumber>][AAA: <PassportNumber>]";
				AssertEvaluateWithDifferentSeparator(characters, input3_1, $"{outputseparator}AAA: Dummy AAA");
				AssertEvaluateWithDifferentSeparator(characters, input3_2, $"{outputseparator}");
				AssertEvaluateWithDifferentSeparator(characters, input3_3, $"{outputseparator}AAA: Dummy AAA");
				AssertEvaluateWithDifferentSeparator(characters, input3_4, $"{outputseparator}AAA: Dummy AAA");

				var input4_1 = "[AAA: <PassportNumber>]{0}";
				var input4_2 = "[BBB: <ForeignerRegistrationNumber>]{0}";
				var input4_3 = "[AAA: <PassportNumber>][BBB: <ForeignerRegistrationNumber>]{0}";
				var input4_4 = "[BBB: <ForeignerRegistrationNumber>][AAA: <PassportNumber>]{0}";
				AssertEvaluateWithDifferentSeparator(characters, input4_1, "AAA: Dummy AAA");
				AssertEvaluateWithDifferentSeparator(characters, input4_2, "");
				AssertEvaluateWithDifferentSeparator(characters, input4_3, "AAA: Dummy AAA");
				AssertEvaluateWithDifferentSeparator(characters, input4_4, "AAA: Dummy AAA");

				var input5_1 = "[AAA: <PassportNumber>]";
				var input5_2 = "[BBB: <ForeignerRegistrationNumber>]";
				AssertEvaluateWithDifferentSeparator(characters, input5_1, "AAA: Dummy AAA");
				AssertEvaluateWithDifferentSeparator(characters, input5_2, "");
			}

			void AssertEvaluateWithDifferentSeparator(IEnumerable<char> characters, string inputWithUnFormattedSeparator, string expectedOutput)
			{
				var inputSeparator1 = $"[{string.Join("", characters.Select(x => x.ToString()))}]";
				var inputSeparator2 = string.Join("", characters.Select(x => $"[{x}]"));

				var input1 = string.Format(inputWithUnFormattedSeparator, inputSeparator1);
				var input2 = string.Format(inputWithUnFormattedSeparator, inputSeparator2);

				AssertEvaluateMarco(info, input1, expectedOutput);
				AssertEvaluateMarco(info, input2, expectedOutput);
			}
		}

		public void TestEvaluate()
		{
			var info1 = new AdditionalInfo()
			{
				InvoiceePassportNo = "Dummy AAA",
				InvoiceeAlienRegistrationNo = "Dummy BBB",
				OriginalIssueID = "Dummy CCC",
			};

			var info2 = new AdditionalInfo()
			{
				InvoiceePassportNo = "Dummy AAA",
				InvoiceeAlienRegistrationNo = "Dummy BBB",
			};

			var info3 = new AdditionalInfo()
			{
				InvoiceePassportNo = "Dummy AAA",
				OriginalIssueID = "Dummy CCC",
			};

			var info4 = new AdditionalInfo()
			{
				InvoiceeAlienRegistrationNo = "Dummy BBB",
				OriginalIssueID = "Dummy CCC",
			};

			var info5 = new AdditionalInfo()
			{
				InvoiceePassportNo = "Dummy AAA",
			};

			var info6 = new AdditionalInfo()
			{
				InvoiceeAlienRegistrationNo = "Dummy BBB",
			};

			var info7 = new AdditionalInfo()
			{
				OriginalIssueID = "Dummy CCC",
			};

			var info8 = new AdditionalInfo();

			CombineAssertions("When text do not contains '[]'", () =>
			{
				AssertEvaluateMarco(info1, "AAA", "");
				AssertEvaluateMarco(info1, "?", "");
				AssertEvaluateMarco(info1, "<", "");
				AssertEvaluateMarco(info1, ">", "");
				AssertEvaluateMarco(info1, "[", "");
				AssertEvaluateMarco(info1, "]", "");
				AssertEvaluateMarco(info1, "][", "");
				AssertEvaluateMarco(info1, "<>", "");
				AssertEvaluateMarco(info1, "< >", "");
				AssertEvaluateMarco(info1, "<123>", "");
				AssertEvaluateMarco(info1, "<PassportNumber>", "");
			});

			CombineAssertions("When text contains []", () =>
			{
				AssertEvaluateMarco(info1, "[]", "");
				AssertEvaluateMarco(info1, "[&]", "&");
				AssertEvaluateMarco(info1, "[[]", "");
				AssertEvaluateMarco(info1, "[]]", "");
				AssertEvaluateMarco(info1, "[   ]", "   ");

				AssertEvaluateMarco(info1, "[\r\n]", "\r\n");
				AssertEvaluateMarco(info1, "[\r\n  ]", "\r\n  ");
				AssertEvaluateMarco(info1, "[12\r\n]", "12\r\n");
				AssertEvaluateMarco(info1, "[12\r\n13]", "12\r\n13");

				AssertEvaluateMarco(info1, "[\\r\\n]", "\\r\\n");
				AssertEvaluateMarco(info1, "[\\r\\n  ]", "\\r\\n  ");
				AssertEvaluateMarco(info1, "[12\\r\\n]", "12\\r\\n");
				AssertEvaluateMarco(info1, "[12\\r\\n13]", "12\\r\\n13");

				AssertEvaluateMarco(info1, @"[
]", @"
");
				AssertEvaluateMarco(info1, @"[

  ]", @"

  ");
				AssertEvaluateMarco(info1, @"[1

2]", @"1

2");
				AssertEvaluateMarco(info1, "[   ][ ][QW][.]", "    QW.");
				AssertEvaluateMarco(info1, "[AAA][BBB][CCC]", "AAABBBCCC");
				AssertEvaluateMarco(info1, "A [AAA] B [BBB] C [CCC] D", "AAABBBCCC");
				AssertEvaluateMarco(info1, "A [AAA: <PassportNumber>] B [AAA: <PassportNumber>] C [AAA: <PassportNumber>] D", "AAA: Dummy AAAAAA: Dummy AAAAAA: Dummy AAA");
			});

			CombineAssertions("When text contains [] and <>", () =>
			{
				AssertEvaluateMarco(info1, "[<]", "<");
				AssertEvaluateMarco(info1, "[>]", ">");
				AssertEvaluateMarco(info1, "[<>]", "");
				AssertEvaluateMarco(info1, "[< >]", "");
				AssertEvaluateMarco(info1, "[<123>]", "");
				AssertEvaluateMarco(info1, "[<123 131>]", "");
				AssertEvaluateMarco(info1, "[<<>]", "<");
				AssertEvaluateMarco(info1, "[<>>]", ">");
				AssertEvaluateMarco(info1, "[<<>>]", "");
			});

			CombineAssertions("Test Korean", () =>
			{
				AssertEvaluateMarco(info1, "[기재사항]", "기재사항");
				AssertEvaluateMarco(info1, "[기재사항: <PassportNumber>]", "기재사항: Dummy AAA");
				AssertEvaluateMarco(info1, "[기재사항][기재사항][기재사항]", "기재사항기재사항기재사항");
			});

			CombineAssertions("Test different properties", () =>
			{
				var input1 = "[AAA: <PassportNumber>][/][BBB: <ForeignerRegistrationNumber>][/][CCC: <OriginalApprovalNumber>]";
				AssertEvaluateMarco(info1, input1, "AAA: Dummy AAA/BBB: Dummy BBB/CCC: Dummy CCC");
				AssertEvaluateMarco(info2, input1, "AAA: Dummy AAA/BBB: Dummy BBB");
				AssertEvaluateMarco(info3, input1, "AAA: Dummy AAA/CCC: Dummy CCC");
				AssertEvaluateMarco(info4, input1, "BBB: Dummy BBB/CCC: Dummy CCC");
				AssertEvaluateMarco(info5, input1, "AAA: Dummy AAA");
				AssertEvaluateMarco(info6, input1, "BBB: Dummy BBB");
				AssertEvaluateMarco(info7, input1, "CCC: Dummy CCC");
				AssertEvaluateMarco(info8, input1, "");

				var input2 = "[AAA: <PassportNumber>][ and1 ][/1][ 1 ][BBB: <ForeignerRegistrationNumber>][ and2 ][/2][ 2 ][CCC: <OriginalApprovalNumber>]";
				AssertEvaluateMarco(info1, input2, "AAA: Dummy AAA and1 /1 1 BBB: Dummy BBB and2 /2 2 CCC: Dummy CCC");
				AssertEvaluateMarco(info2, input2, "AAA: Dummy AAA and1 /1 1 BBB: Dummy BBB");
				AssertEvaluateMarco(info3, input2, "AAA: Dummy AAA and1 /1 1 CCC: Dummy CCC");
				AssertEvaluateMarco(info4, input2, "BBB: Dummy BBB and2 /2 2 CCC: Dummy CCC");
				AssertEvaluateMarco(info5, input2, "AAA: Dummy AAA");
				AssertEvaluateMarco(info6, input2, "BBB: Dummy BBB");
				AssertEvaluateMarco(info7, input2, "CCC: Dummy CCC");
				AssertEvaluateMarco(info8, input2, "");

				var input3 = "[AAA: <PassportNumber>][ ][/][ ][BBB: <ForeignerRegistrationNumber>][ ][/][ ][CCC: <OriginalApprovalNumber>]";
				AssertEvaluateMarco(info1, input3, "AAA: Dummy AAA / BBB: Dummy BBB / CCC: Dummy CCC");
				AssertEvaluateMarco(info2, input3, "AAA: Dummy AAA / BBB: Dummy BBB");
				AssertEvaluateMarco(info3, input3, "AAA: Dummy AAA / CCC: Dummy CCC");
				AssertEvaluateMarco(info4, input3, "BBB: Dummy BBB / CCC: Dummy CCC");
				AssertEvaluateMarco(info5, input3, "AAA: Dummy AAA");
				AssertEvaluateMarco(info6, input3, "BBB: Dummy BBB");
				AssertEvaluateMarco(info7, input3, "CCC: Dummy CCC");
				AssertEvaluateMarco(info8, input3, "");

				var input4 = "[AAA: <PassportNumber>][ / ][BBB: <ForeignerRegistrationNumber>][ / ][CCC: <OriginalApprovalNumber>]";
				AssertEvaluateMarco(info1, input4, "AAA: Dummy AAA / BBB: Dummy BBB / CCC: Dummy CCC");
				AssertEvaluateMarco(info2, input4, "AAA: Dummy AAA / BBB: Dummy BBB");
				AssertEvaluateMarco(info3, input4, "AAA: Dummy AAA / CCC: Dummy CCC");
				AssertEvaluateMarco(info4, input4, "BBB: Dummy BBB / CCC: Dummy CCC");
				AssertEvaluateMarco(info5, input4, "AAA: Dummy AAA");
				AssertEvaluateMarco(info6, input4, "BBB: Dummy BBB");
				AssertEvaluateMarco(info7, input4, "CCC: Dummy CCC");
				AssertEvaluateMarco(info8, input4, "");
			});
		}

		void AssertEvaluateMarco(AdditionalInfo info, string input, string expectedOutput)
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var provider = new KoreaSouthEInvoicingDataElementProvider(transactionInfo, info);
			AssertEquals(expectedOutput, provider.Evaluate(input));
		}

		public void TestInvalidArguments()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			AssertNoExceptionThrown(() => new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo));
			AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: additionalInfo", () => new KoreaSouthEInvoicingDataElementProvider(transactionInfo, null));
			AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: transactionInfo", () => new KoreaSouthEInvoicingDataElementProvider(null, additionalInfo));
		}

		public void TestInvoiceHeaderDescription()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var provider = new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo);

			transactionInfo.Description = string.Empty;
			AssertNullOrEmpty("Precondition", provider.InvoiceHeaderDescription);

			transactionInfo.Description = "Test Desc";
			AssertEquals("Test Desc", provider.InvoiceHeaderDescription);
		}

		public void TestForeignerRegistrationNumber()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var provider = new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo);

			AssertNullOrEmpty("Precondition", provider.ForeignerRegistrationNumber);

			additionalInfo.InvoiceeAlienRegistrationNo = "Test ForeignerRegistrationNumber";
			AssertEquals("Test ForeignerRegistrationNumber", provider.ForeignerRegistrationNumber);
		}

		public void TestPassportNumber()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var provider = new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo);

			AssertNullOrEmpty("Precondition", provider.PassportNumber);

			additionalInfo.InvoiceePassportNo = "Test PassportNumber";
			AssertEquals("Test PassportNumber", provider.PassportNumber);
		}

		public void TestAmendStatusCodeDescriptionInKorean()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var provider = new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo);

			AssertNullOrEmpty("Precondition", provider.AmendStatusCodeDescriptionInKorean);

			AssertAmendStatusCodeDescriptionInKorean("01", "기재사항 착오.정정"); 
			AssertAmendStatusCodeDescriptionInKorean("02", "공급금액 변동"); 
			AssertAmendStatusCodeDescriptionInKorean("03", "제화의 환입"); 
			AssertAmendStatusCodeDescriptionInKorean("04", "계약의 해제"); 
			AssertAmendStatusCodeDescriptionInKorean("05", "내국신용장등 사후개설"); 
			AssertAmendStatusCodeDescriptionInKorean("06", "착오에 의한 이중발급"); 

			void AssertAmendStatusCodeDescriptionInKorean(string amendStatusCode, string expectedAmendStatusCodeDescriptionInKorean)
			{
				additionalInfo.AmendStatusCode = amendStatusCode;
				AssertEquals(expectedAmendStatusCodeDescriptionInKorean, provider.AmendStatusCodeDescriptionInKorean);
			}
		}

		public void TestOriginalApprovalNumber()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var provider = new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo);

			AssertNullOrEmpty("Precondition", provider.OriginalApprovalNumber);

			additionalInfo.OriginalIssueID = "Test OriginalIssueID";
			AssertEquals("Test OriginalIssueID", provider.OriginalApprovalNumber);
		}

		public void TestOriginalApprovalDate()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var provider = new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo);

			AssertExceptionThrown<FormatException>(() => { var date = provider.OriginalApprovalDate; });

			additionalInfo.OriginalIssueID = "Test";
			AssertExceptionThrown<FormatException>(() => { var date = provider.OriginalApprovalDate; });

			additionalInfo.OriginalIssueID = "Test OriginalIssueID";
			AssertExceptionThrown<FormatException>(() => { var date = provider.OriginalApprovalDate; });

			additionalInfo.OriginalIssueID = "202206201234567800000001";
			AssertOriginalApprovalDate("01", "2022-06-20");
			AssertOriginalApprovalDate("02", "2022-06-20");
			AssertOriginalApprovalDate("03", "2022-06-20");
			AssertOriginalApprovalDate("04", "2022-06-20");
			AssertOriginalApprovalDate("05", "2022-06-20");
			AssertOriginalApprovalDate("06", "2022-06-20");

			void AssertOriginalApprovalDate(string amendStatusCode, string expectedOriginalApprovalDate)
			{
				additionalInfo.AmendStatusCode = amendStatusCode;
				AssertEquals(expectedOriginalApprovalDate, provider.OriginalApprovalDate);
			}
		}

		public void TestOriginalApprovalDateForCode020304()
		{
			var additionalInfo = new AdditionalInfo();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var provider = new KoreaSouthEInvoicingDataElementProvider(transactionInfo, additionalInfo);

			AssertNullOrEmpty("Precondition", additionalInfo.AmendStatusCode);
			AssertNullOrEmpty("Precondition", provider.OriginalApprovalDateForCode020304);

			additionalInfo.AmendStatusCode = "02";
			additionalInfo.OriginalIssueID = "Test";
			AssertExceptionThrown<FormatException>(() => { var date = provider.OriginalApprovalDateForCode020304; });

			additionalInfo.OriginalIssueID = "Test OriginalIssueID";
			AssertExceptionThrown<FormatException>(() => { var date = provider.OriginalApprovalDateForCode020304; });

			additionalInfo.OriginalIssueID = "202206201234567800000001";
			AssertOriginalApprovalDateForCode020304("01", "");
			AssertOriginalApprovalDateForCode020304("02", "2022-06-20");
			AssertOriginalApprovalDateForCode020304("03", "2022-06-20");
			AssertOriginalApprovalDateForCode020304("04", "2022-06-20");
			AssertOriginalApprovalDateForCode020304("05", "");
			AssertOriginalApprovalDateForCode020304("06", "");

			void AssertOriginalApprovalDateForCode020304(string amendStatusCode, string expectedOriginalApprovalDateForCode020304)
			{
				additionalInfo.AmendStatusCode = amendStatusCode;
				AssertEquals(expectedOriginalApprovalDateForCode020304, provider.OriginalApprovalDateForCode020304);
			}
		}
	}
}
