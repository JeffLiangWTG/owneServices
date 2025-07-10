using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSSTAProvider))]
	sealed class CUSSTAProviderTest : InboundDataProviderTestCase<ICUSSTA, CUSSTAProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSSTAProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.MetaData.MessageIdentifier = "CUSSTA58750000000381119050419125839";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSSTA58750000000381119050419125839"));
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
			message.MetaData.InterchangeRecipient.Identification.ReferenceNumber = "XX8999783";
			NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientReferenceNumber, Is.EqualTo("XX8999783").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientSubsidiaryNumber()
		{
			message.MetaData.InterchangeRecipient.Identification.SubsidiaryNumber = "1234";
			NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientSubsidiaryNumber, Is.EqualTo("1234").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalReferenceNumber()
		{
			message.SummaryDeclaration.AdditionalReferenceNumber = "19DE587500003774X7";
			NUnit.Framework.Assert.That(dataProvider.AdditionalReferenceNumber, Is.EqualTo("19DE587500003774X7").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(ICUSSTAGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is ICUSSTAGoodsItem");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new SCSTAB()
			{
				MetaData = new SCSTABMetaData()
				{
					InterchangeRecipient = new SCSTABMetaDataInterchangeRecipient
					{
						Identification = new SCSTABMetaDataInterchangeRecipientIdentification()
					}
				},
				Custodian = new SCSTABCustodian()
				{
					Identification = new SCSTABCustodianIdentification()
				},
				SummaryDeclaration = new SCSTABSummaryDeclaration()
				{
					GoodsItem = new SCSTABSummaryDeclarationGoodsItem[]
					{
						new SCSTABSummaryDeclarationGoodsItem(),
						new SCSTABSummaryDeclarationGoodsItem()
					}
				}
			};
			dataProvider = new CUSSTAProvider(message);
		}
		SCSTAB message;
		ICUSSTA dataProvider;

		protected override CUSSTAProvider GetProvider() => (CUSSTAProvider)dataProvider;
	}
}
