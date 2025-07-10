using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR082C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class TR082CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			var emptyProvider = new TR082CProvider(new Tr082C());
			CombineAssertions(() =>
			{
				AssertEquals("MRN", ZString.Empty, emptyProvider.MRN);
				AssertEquals("LRN", ZString.Empty, emptyProvider.LRN);
				AssertEquals("Request Date", ZDateTime.Empty, emptyProvider.RequestDate);
				AssertEquals("Date Limit", ZDateTime.Empty, emptyProvider.DateLimit);
				AssertEquals("Additional Information count", 0, emptyProvider.AdditionalInformations.Count());
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "21IEDU4EX144268149", provider.MRN);
		}

		public void TestLRN()
		{
			AssertEquals("LRN", "LRN231234", provider.LRN);
		}

		public void TestRequestDate()
		{
			AssertEquals("Request Date", new ZDateTime(2023, 3, 21), provider.RequestDate);
		}

		public void TestDateLimit()
		{
			AssertEquals("Date Limit", new ZDateTime(2024, 12, 31), provider.DateLimit);
		}

		public void TestAdditionalInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Additional Information Count", 2, provider.AdditionalInformations.Count());
				AssertEquals("Additional Information Document Type", "ABC", provider.AdditionalInformations.First().DocumentType);
				AssertEquals("Additional Information Document Complementary Information", "Comp Info DEF", provider.AdditionalInformations.Last().DocumentComplementaryInformation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR082CProvider(CreateStandardProvider());
		}
		TR082CProvider provider;

		public static Tr082C CreateStandardProvider(string mrn = "21IEDU4EX144268149")
		{
			return new Tr082C
			{
				Declaration = new DeclarationType105
				{
					Mrn = mrn,
					Lrn = "LRN231234",
					RequestDate = new DateTime(2023, 3, 21),
					DateLimit = new DateTime(2024, 12, 31),
				},
				AdditionalInformation = new Collection<AdditionalInformationType101>
				{
					new AdditionalInformationType101
					{
						DocumentType = "ABC",
						DocumentComplementaryInformation = "Comp Info ABC",
					},
					new AdditionalInformationType101
					{
						DocumentType = "DEF",
						DocumentComplementaryInformation = "Comp Info DEF",
					},
				},
			};
		}
	}
}
