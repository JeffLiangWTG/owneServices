using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSFSTProvider))]
	sealed class CUSFSTProviderTest : InboundDataProviderTestCase<IUnderCustomsControl, CUSFSTProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSFSTProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.MetaData.MessageIdentifier = "CUSFIN58750000000381074050419102427";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSFIN58750000000381074050419102427"));
		}

		[ExpectNoExceptions]
		public void TestArrivalDate()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.ArrivalDate, Is.EqualTo(ZDate.Empty), "Arrival Date not specified");
				message.Header.ArrivalDateSpecified = true;
				message.Header.ArrivalDate = new DateTime(2020, 2, 16);
				NUnit.Framework.Assert.That(dataProvider.ArrivalDate, Is.EqualTo(new ZDate(2020, 2, 16)), "Arrival Date specified");
			});
		}

		[ExpectNoExceptions]
		public void TestPresentationDate()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.PresentationDate, Is.EqualTo(ZDate.Empty), "Presentation Date not specified");
				message.Header.PresentationDateSpecified = true;
				message.Header.PresentationDate = new DateTime(2020, 2, 15);
				NUnit.Framework.Assert.That(dataProvider.PresentationDate, Is.EqualTo(new ZDate(2020, 2, 15)), "Presentation Date specified");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			message.Header.ReferenceNumber = "ATB150002110520195875";
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150002110520195875").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			message.Header.MRN = "23DE586601055987B7";
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("23DE586601055987B7"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.Header.LRN = "19DE587500026773M4";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("19DE587500026773M4").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPreviousReferenceType()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceType, Is.EqualTo(ZString.Empty), "Group does not exist");
				message.PreviousAdministrativeReferences = new SCFSTFPreviousAdministrativeReferences
				{
					TypeSpecified = false,
				};
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceType, Is.EqualTo(ZString.Empty), "Group exists, but Type doesn't");

				message.PreviousAdministrativeReferences = new SCFSTFPreviousAdministrativeReferences
				{
					Type = SCFSTFPreviousAdministrativeReferencesType.T,
					TypeSpecified = true,
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
				message.PreviousAdministrativeReferences = new SCFSTFPreviousAdministrativeReferences()
				{
					Type = SCFSTFPreviousAdministrativeReferencesType.OHNE
				};
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceNumber, Is.EqualTo(ZString.Empty), "No Reference Number for Type Ohne");
				message.PreviousAdministrativeReferences.Type = SCFSTFPreviousAdministrativeReferencesType.T;
				message.PreviousAdministrativeReferences.PreviousAdministrativeReference = new SCFSTFPreviousAdministrativeReferencesPreviousAdministrativeReference()
				{
					ReferenceNumber = "19DE587500026773M4"
				};
				NUnit.Framework.Assert.That(dataProvider.PreviousReferenceNumber, Is.EqualTo("19DE587500026773M4").Using(CustomComparers.TypeComparison), "Reference Number exists for Type T");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeReferenceNumber()
		{
			message.MetaData.InterchangeSender.Identification.ReferenceNumber = "DE005875";
			NUnit.Framework.Assert.That(dataProvider.CustomsOfficeReferenceNumber, Is.EqualTo("DE005875").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(IUnderCustomsControlGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is IUnderCustomsControlGoodsItem");
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
			message = new SCFSTF()
			{
				MetaData = new SCFSTFMetaData()
				{
					InterchangeSender = new SCFSTFMetaDataInterchangeSender()
					{
						Identification = new SCFSTFMetaDataInterchangeSenderIdentification()
					}
				},
				Header = new SCFSTFHeader(),
				Body = new SCFSTFGoodsItem[]
				{
					new SCFSTFGoodsItem(),
					new SCFSTFGoodsItem()
				}
			};
			dataProvider = new CUSFSTProvider(message);
		}
		SCFSTF message;
		IUnderCustomsControl dataProvider;

		protected override CUSFSTProvider GetProvider() => (CUSFSTProvider)dataProvider;
	}
}
