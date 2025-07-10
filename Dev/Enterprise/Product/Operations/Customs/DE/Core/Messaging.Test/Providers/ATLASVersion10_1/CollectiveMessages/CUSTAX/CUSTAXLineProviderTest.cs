using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTAXLineProvider))]
	sealed class CUSTAXLineProviderTest : InboundDataProviderTestCase<ICUSTAXLine, CUSTAXLineProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSTAXLineProvider(null));
		}

		[ExpectNoExceptions]
		public void TestLineNumber()
		{
			line.SequenceNumber = "2";
			NUnit.Framework.Assert.That(dataProvider.LineNumber, Is.EqualTo("2"));
		}

		[ExpectNoExceptions]
		public void TestLineCompletionFlag()
		{
			line.CompletionFlag = "7";
			NUnit.Framework.Assert.That(dataProvider.LineCompletionFlag, Is.EqualTo("7"));
		}

		[ExpectNoExceptions]
		public void TestDuties()
		{
			line.CustomsDuties = new GCTAXMBodyGoodsItemCustomsDuties
			{
				CustomsDuty = new GCTAXMBodyGoodsItemCustomsDutiesCustomsDuty[]
				{
					new GCTAXMBodyGoodsItemCustomsDutiesCustomsDuty(),
					new GCTAXMBodyGoodsItemCustomsDutiesCustomsDuty()
				}
			};
			NUnit.Framework.Assert.That(dataProvider.Duties.Count, Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestDuties_Empty()
		{
			NUnit.Framework.Assert.That(dataProvider.Duties.Count, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestDuties_Empty2()
		{
			line.CustomsDuties = new GCTAXMBodyGoodsItemCustomsDuties
			{
				FlatRateTax = new GCTAXMBodyGoodsItemCustomsDutiesFlatRateTax()
			};
			NUnit.Framework.Assert.That(dataProvider.Duties.Count, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestCustomsValue()
		{
			line.Assessment = new GCTAXMBodyGoodsItemAssessment { CustomsValueSpecified = true, CustomsValue = 5.01m };
			NUnit.Framework.Assert.That(dataProvider.CustomsValue, Is.EqualTo(5.01m));
		}

		[ExpectNoExceptions]
		public void TestCustomsValue_NotSpecified()
		{
			line.Assessment = new GCTAXMBodyGoodsItemAssessment { CustomsValueSpecified = false, CustomsValue = 0.01m };
			NUnit.Framework.Assert.That(dataProvider.CustomsValue, Is.Null);
		}

		[ExpectNoExceptions]
		public void TestExportLimitDate()
		{
			var expectedDate = new DateTime(2028, 3, 4);
			line.CustomsTaxAssessment = new GCTAXMBodyGoodsItemCustomsTaxAssessment { ExportLimitDateSpecified = true, ExportLimitDate = expectedDate };
			NUnit.Framework.Assert.That(dataProvider.ExportLimitDate, Is.EqualTo(expectedDate));
		}

		[ExpectNoExceptions]
		public void TestExportLimitDate_NotSpecified()
		{
			line.CustomsTaxAssessment = new GCTAXMBodyGoodsItemCustomsTaxAssessment { ExportLimitDateSpecified = false, };
			NUnit.Framework.Assert.That(dataProvider.ExportLimitDate, Is.Null);
		}

		protected override CUSTAXLineProvider GetProvider() => dataProvider;

		protected override void SetUp()
		{
			base.SetUp();
			line = new GCTAXMBodyGoodsItem();
			dataProvider = new CUSTAXLineProvider(line);
		}
		GCTAXMBodyGoodsItem line;
		CUSTAXLineProvider dataProvider;
	}
}
