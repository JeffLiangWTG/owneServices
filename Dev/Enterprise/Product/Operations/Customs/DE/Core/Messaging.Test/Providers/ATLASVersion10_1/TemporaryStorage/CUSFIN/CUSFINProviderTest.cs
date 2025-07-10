using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSFINProvider))]
	sealed class CUSFINProviderTest : InboundDataProviderTestCase<ICUSFIN, CUSFINProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSFINProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.MetaData.MessageIdentifier = "CUSFIN58750000000381074050419102427";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSFIN58750000000381074050419102427"));
		}

		[ExpectNoExceptions]
		public void TestCustodianReferenceNumber()
		{
			message.Custodian.Identification.ReferenceNumber = "DE8999783";
			NUnit.Framework.Assert.That(dataProvider.CustodianReferenceNumber, Is.EqualTo("DE8999783").Using(CustomComparers.TypeComparison));

			message.Custodian.Identification = null;
			NUnit.Framework.Assert.That(dataProvider.CustodianReferenceNumber, Is.EqualTo(ZString.Empty));

			message.Custodian = null;
			NUnit.Framework.Assert.That(dataProvider.CustodianReferenceNumber, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCustodianSubsidiaryNumber()
		{
			message.Custodian.Identification.SubsidiaryNumber = "0000";
			NUnit.Framework.Assert.That(dataProvider.CustodianSubsidiaryNumber, Is.EqualTo("0000").Using(CustomComparers.TypeComparison));

			message.Custodian.Identification = null;
			NUnit.Framework.Assert.That(dataProvider.CustodianSubsidiaryNumber, Is.EqualTo(ZString.Empty));

			message.Custodian = null;
			NUnit.Framework.Assert.That(dataProvider.CustodianSubsidiaryNumber, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientReferenceNumber()
		{
			message.MetaData.InterchangeRecipient.Identification.ReferenceNumber = "DE8999789";
			NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientReferenceNumber, Is.EqualTo("DE8999789").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientSubsidiaryNumber()
		{
			message.MetaData.InterchangeRecipient.Identification.SubsidiaryNumber = "0001";
			NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientSubsidiaryNumber, Is.EqualTo("0001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(ICUSFINGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is ICUSFINGoodsItem");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalRegistrationNumber()
		{
			message.SummaryDeclaration = new SCFINGSummaryDeclaration
			{
				AdditionalRegistrationNumber = "ATO310000090420195875"
			};
			NUnit.Framework.Assert.That(dataProvider.AdditionalRegistrationNumber, Is.EqualTo("ATO310000090420195875").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalReferenceNumber()
		{
			message.SummaryDeclaration = new SCFINGSummaryDeclaration
			{
				AdditionalReferenceNumber = "ATO310000090420193481"
			};
			NUnit.Framework.Assert.That(dataProvider.AdditionalReferenceNumber, Is.EqualTo("ATO310000090420193481").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "Null, if summary declaration is empty - should be [null]");
				message.SummaryDeclaration = new SCFINGSummaryDeclaration
				{
					MRN = "24DE123050554788M5"
				};
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("24DE123050554788M5"));
			});
		}

		[ExpectNoExceptions]
		public void TestCompletionType()
		{
			message.SummaryDeclaration = new SCFINGSummaryDeclaration
			{
				CompletionType = "W-VV"
			};
			NUnit.Framework.Assert.That(dataProvider.CompletionType, Is.EqualTo("W-VV").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new SCFING
			{
				MetaData = new SCFINGMetaData
				{
					InterchangeRecipient = new SCFINGMetaDataInterchangeRecipient
					{
						Identification = new SCFINGMetaDataInterchangeRecipientIdentification()
					}
				},
				Custodian = new SCFINGCustodian
				{
					Identification = new SCFINGCustodianIdentification()
				},
				SummaryDeclaration = new SCFINGSummaryDeclaration
				{
					GoodsItem = new SCFINGSummaryDeclarationGoodsItem[]
					{
						new SCFINGSummaryDeclarationGoodsItem(),
						new SCFINGSummaryDeclarationGoodsItem()
					}
				}
			};
			dataProvider = new CUSFINProvider(message);
		}
		SCFING message;
		ICUSFIN dataProvider;

		protected override CUSFINProvider GetProvider() => (CUSFINProvider)dataProvider;
	}
}
