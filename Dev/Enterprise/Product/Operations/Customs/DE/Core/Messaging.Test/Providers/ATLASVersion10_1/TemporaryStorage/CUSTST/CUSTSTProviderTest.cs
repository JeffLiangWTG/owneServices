using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTSTProvider))]
	sealed class CUSTSTProviderTest : InboundDataProviderTestCase<IUnderCustomsControl, CUSTSTProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSTSTProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.MetaData.MessageIdentifier = "CUSTST58750000000375302250219160050";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSTST58750000000375302250219160050"));
		}

		[ExpectNoExceptions]
		public void TestArrivalDate()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.ArrivalDate, Is.EqualTo(ZDate.Empty), "Arrival Date not specified");
				message.Header.ArrivalDateSpecified = true;
				message.Header.ArrivalDate = new DateTime(2020, 2, 18);
				NUnit.Framework.Assert.That(dataProvider.ArrivalDate, Is.EqualTo(new ZDate(2020, 2, 18)), "Arrival Date specified");
			});
		}

		[ExpectNoExceptions]
		public void TestPresentationDate()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.PresentationDate, Is.EqualTo(ZDate.Empty), "Presentation Date not specified");
				message.Header.PresentationDateSpecified = true;
				message.Header.PresentationDate = new DateTime(2020, 2, 17);
				NUnit.Framework.Assert.That(dataProvider.PresentationDate, Is.EqualTo(new ZDate(2020, 2, 17)), "Presentation Date specified");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			message.Header.ReferenceNumber = "ATB150002930220195875";
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150002930220195875").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRecipientReferenceNumber()
		{
			NUnit.Framework.Assert.That(dataProvider.RecipientReferenceNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison));
			message.MetaData.InterchangeRecipient = new SCTSTJMetaDataInterchangeRecipient
			{
				Identification = new SCTSTJMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = "DE8999783",
				},
			};
			NUnit.Framework.Assert.That(dataProvider.RecipientReferenceNumber, Is.EqualTo("DE8999783").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.Header.LRN = "19DE587500026775M6";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("19DE587500026775M6").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			message.Header.MRN = "23DE586601055987B7";
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("23DE586601055987B7"));
		}

		[ExpectNoExceptions]
		public void TestPreviousReferenceType()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceType, Is.EqualTo(ZString.Empty), "Group does not exist");
				message.PreviousAdministrativeReferences = new SCTSTJPreviousAdministrativeReferences()
				{
					TypeSpecified = false
				};
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceType, Is.EqualTo(ZString.Empty), "Group exists, but Type doesn't");

				message.PreviousAdministrativeReferences = new SCTSTJPreviousAdministrativeReferences()
				{
					Type = SCTSTJPreviousAdministrativeReferencesType.T,
					TypeSpecified = true
				};
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceType, Is.EqualTo(PreviousReferenceType.Codes._T).Using(CustomComparers.TypeComparison), "Previous Reference Value");
			});
		}

		[ExpectNoExceptions]
		public void TestPreviousReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceNumber, Is.EqualTo(ZString.Empty), "1st Group does not exist");
				message.PreviousAdministrativeReferences = new SCTSTJPreviousAdministrativeReferences()
				{
					Type = SCTSTJPreviousAdministrativeReferencesType.OHNE
				};
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceNumber, Is.EqualTo(ZString.Empty), "No Reference Number for Type Ohne");
				message.PreviousAdministrativeReferences.Type = SCTSTJPreviousAdministrativeReferencesType.T;
				message.PreviousAdministrativeReferences.PreviousAdministrativeReference = new SCTSTJPreviousAdministrativeReferencesPreviousAdministrativeReference
				{
					ReferenceNumber = "19DE587500026775M6"
				};
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceNumber, Is.EqualTo("19DE587500026775M6").Using(CustomComparers.TypeComparison), "Reference Number exists for Type T");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeReferenceNumber()
		{
			message.MetaData.InterchangeSender.Identification.ReferenceNumber = "DE005976";
			NUnit.Framework.Assert.That(dataProvider.CustomsOfficeReferenceNumber, Is.EqualTo("DE005976").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			CombineAssertions(() =>
			{
				var goodsItems = dataProvider.GoodsItems;
				NUnit.Framework.Assert.That(goodsItems.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(ICUSTSTGoodsItem).IsAssignableFrom(goodsItems.First().GetType()), Is.EqualTo(true), "Is ICUSTSTGoodsItem");
				NUnit.Framework.Assert.That(dataProvider.GoodsItems, Is.SameAs(goodsItems), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.Header.ReferencedMessageIdentifier = "DE899978300000000812";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("DE899978300000000812").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new SCTSTJ()
			{
				MetaData = new SCTSTJMetaData()
				{
					InterchangeSender = new SCTSTJMetaDataInterchangeSender()
					{
						Identification = new SCTSTJMetaDataInterchangeSenderIdentification()
					}
				},
				Header = new SCTSTJHeader(),
				Body = new SCTSTJGoodsItem[]
				{
					new SCTSTJGoodsItem(),
					new SCTSTJGoodsItem()
				}
			};
			dataProvider = new CUSTSTProvider(message);
		}
		SCTSTJ message;
		ICUSTST dataProvider;

		protected override CUSTSTProvider GetProvider() => (CUSTSTProvider)dataProvider;
	}
}
