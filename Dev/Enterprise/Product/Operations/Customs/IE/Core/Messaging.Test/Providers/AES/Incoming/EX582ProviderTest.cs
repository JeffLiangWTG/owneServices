using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX582;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	class EX582ProviderTest : TestCaseWithFactory
	{
		public void TestDeclaration()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MRN", "MRN", provider.MRN);
				AssertEquals("LRN", "LRN", provider.LRN);
				AssertEquals("RequestDate", new ZDateTime(2022, 07, 29), provider.RequestDate);
				AssertEquals("DateLimit", new ZDateTime(2022, 07, 30), provider.DateLimit);
			});
		}

		public void TestAdditionalInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalInformations.Count", 2, provider.AdditionalInformations.Count);
				var infos = provider.AdditionalInformations.ToArray();
				var info1 = infos[0];
				AssertEquals("DocumentType", "Z740", info1.DocumentType);
				AssertEquals("DocumentComplementaryInformation", "Info1", info1.RequestInformation);
				var info2 = infos[1];
				AssertEquals("DocumentType", "Z750", info2.DocumentType);
				AssertEquals("DocumentComplementaryInformation", "Info2", info2.RequestInformation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			ex582 = new Ex582
			{
				ExportOperation = new DeclarationType1
				{
					Mrn = "MRN",
					Lrn = "LRN",
					DateLimit = new System.DateTime(2022, 07, 30),
					RequestDate = new System.DateTime(2022, 07, 29),
				},
				AdditionalInformation = new System.Collections.ObjectModel.Collection<DocumentAdditionalInformationType>
				{
					new DocumentAdditionalInformationType
					{
						DocumentType = "Z740",
						DocumentComplementaryInformation = "Info1"
					},
					new DocumentAdditionalInformationType
					{
						DocumentType = "Z750",
						DocumentComplementaryInformation = "Info2"
					}
				}
			};
			provider = new EX582Provider(ex582);
		}
		Ex582 ex582;
		EX582Provider provider;
	}
}
