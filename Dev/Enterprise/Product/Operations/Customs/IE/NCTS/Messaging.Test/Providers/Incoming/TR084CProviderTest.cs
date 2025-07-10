using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR084C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class TR084CProviderTest : TestCaseWithFactory
	{
		public void TestDeclaration()
		{
			CombineAssertions("All properties should return correct value.", () =>
			{
				AssertEquals("MRN", "MRN", provider.MRN);
				AssertEquals("LRN", "LRN", provider.LRN);
				AssertEquals("DateLimit", new ZDateTime(2024, 3, 21, 12, 51, 36), provider.DateLimit);
				AssertEquals("RequestDate", new ZDateTime(2023, 1, 25, 10, 9, 24), provider.RequestDate);
			});
		}

		public void TestAdditionalInformation()
		{
			CombineAssertions(() =>
			{
				var additionalInformations = provider.AdditionalInformations.ToList();
				AssertEquals("Type1", "Type1", additionalInformations[0].DocumentType);
				AssertEquals("Info1", "Info1", additionalInformations[0].DocumentComplementaryInformation);
				AssertEquals("Type2", "Type2", additionalInformations[1].DocumentType);
				AssertEquals("Info2", "Info2", additionalInformations[1].DocumentComplementaryInformation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR084CProvider(new Tr084C
			{
				Declaration = new DeclarationType105
				{
					Mrn = "MRN",
					Lrn = "LRN",
					DateLimit = new DateTime(2024, 3, 21, 12, 51, 36),
					RequestDate = new DateTime(2023, 1, 25, 10, 9, 24)
				},
				AdditionalInformation = new Collection<AdditionalInformationType101> {
					new AdditionalInformationType101 {
						DocumentType = "Type1",
						DocumentComplementaryInformation = "Info1"
					},
					new AdditionalInformationType101 {
						DocumentType = "Type2",
						DocumentComplementaryInformation = "Info2"
					}
				}
			});
		}
		TR084CProvider provider;
	}
}
