using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTAXProvider))]
	sealed class CUSTAXProviderTest : InboundDataProviderTestCase<ICUSTAX, CUSTAXProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSTAXProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)), "No MetaData");

				message.MetaData = new GCTAXMMetaData { MessageIdentifier = "CUSTAX58750000000375302250219160050" };
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSTAX58750000000375302250219160050"), "Has MetaData");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(ZString.Empty), "No Header");

				message.Header = new GCTAXMHeader { ReferenceNumber = "ATB150000620520195875" };
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150000620520195875").Using(CustomComparers.TypeComparison), "Has Header");
			});
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "No Header - should be [null]");

				message.Header = new GCTAXMHeader { MRN = "19DE485154386041M4" };
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("19DE485154386041M4"), "Has Header");
			});
		}

		[ExpectNoExceptions]
		public void TestCompletionFlag()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CompletionFlag, Is.EqualTo(ZString.Empty), "No Header");

				message.Header = new GCTAXMHeader { CompletionFlag = "2" };
				NUnit.Framework.Assert.That(dataProvider.CompletionFlag, Is.EqualTo("2").Using(CustomComparers.TypeComparison), "Has Header");
			});
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(ZString.Empty), "No Header");

				message.Header = new GCTAXMHeader { LRN = "MAS/22/11/22027" };
				NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("MAS/22/11/22027").Using(CustomComparers.TypeComparison), "Has Header");
			});
		}

		[ExpectNoExceptions]
		public void TestRegistrationDate()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.RegistrationDate, Is.Null, "Header NULL");

				message.Header = new GCTAXMHeader { RegistrationDateSpecified = true, RegistrationDate = new DateTime(2021, 10, 07) };
				NUnit.Framework.Assert.That(dataProvider.RegistrationDate, Is.EqualTo(new DateTime(2021, 10, 07)), "Registration Date Specified");

				message.Header.RegistrationDateSpecified = false;
				NUnit.Framework.Assert.That(dataProvider.RegistrationDate, Is.Null, "Registration Date Not Specified");
			});
		}

		[ExpectNoExceptions]
		public void TestLines()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.Lines.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(ICUSTAXLine).IsAssignableFrom(dataProvider.Lines.First().GetType()), Is.EqualTo(true), "Is ICUSTAXLine");
			});
		}

		[ExpectNoExceptions]
		public void TestTotalCustomsDutyAmount()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.TotalCustomsDutyAmount, Is.EqualTo(ZDecimal.Zero), "No CustomsDuties");

				message.CustomsDuties = new GCTAXMCustomsDuties { TotalCustomsDuty = new GCTAXMCustomsDutiesTotalCustomsDuty { Amount = 7.7m } };
				NUnit.Framework.Assert.That(dataProvider.TotalCustomsDutyAmount, Is.EqualTo(7.7m).Using(CustomComparers.TypeComparison), "Have a CustomsDuties");
			});
		}

		[ExpectNoExceptions]
		public void TestAcceptanceDate()
		{
			var acceptanceDate = new DateTime(2023, 02, 15);
			goodsItem1.CustomsTaxAssessment = new GCTAXMBodyGoodsItemCustomsTaxAssessment { AcceptanceDateSpecified = true, AcceptanceDate = acceptanceDate };
			NUnit.Framework.Assert.That(dataProvider.AcceptanceDate, Is.EqualTo(acceptanceDate));
		}

		[ExpectNoExceptions]
		public void TestAcceptanceDate_NotSpecified()
		{
			goodsItem1.CustomsTaxAssessment = new GCTAXMBodyGoodsItemCustomsTaxAssessment { AcceptanceDateSpecified = false, AcceptanceDate = DateTime.MinValue };
			NUnit.Framework.Assert.That(dataProvider.AcceptanceDate, Is.Null);
		}

		[ExpectNoExceptions]
		public void TestAcceptanceDate_MapLatest()
		{
			var acceptanceDate1 = new DateTime(2023, 01, 15);
			var acceptanceDate2 = new DateTime(2023, 02, 01);
			goodsItem1.CustomsTaxAssessment = new GCTAXMBodyGoodsItemCustomsTaxAssessment { AcceptanceDateSpecified = true, AcceptanceDate = acceptanceDate1 };
			goodsItem2.CustomsTaxAssessment = new GCTAXMBodyGoodsItemCustomsTaxAssessment { AcceptanceDateSpecified = true, AcceptanceDate = acceptanceDate2 };
			NUnit.Framework.Assert.That(dataProvider.AcceptanceDate, Is.EqualTo(acceptanceDate1), "Smaller Date is mapped");
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new GCTAXM
			{
				Body = new GCTAXMBody
				{
					GoodsItem = new GCTAXMBodyGoodsItem[]
					{
						goodsItem1 = new GCTAXMBodyGoodsItem(),
						goodsItem2 = new GCTAXMBodyGoodsItem()
					}
				}
			};
			dataProvider = new CUSTAXProvider(message);
		}
		GCTAXM message;
		GCTAXMBodyGoodsItem goodsItem1;
		GCTAXMBodyGoodsItem goodsItem2;
		ICUSTAX dataProvider;

		protected override CUSTAXProvider GetProvider() => (CUSTAXProvider)dataProvider;
	}
}
