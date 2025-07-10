using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	sealed class IM482ProviderTest : TestCaseWithFactory
	{
		public void TestMRN()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MRN);
		}

		public void TestCaseId()
		{
			AssertEquals("123", provider.CaseId);
		}

		public void TestRequestDate()
		{
			AssertEquals(new ZDateTime(2023, 9, 21, 12, 23, 7), provider.RequestDate);
		}

		public void TestDateLimit()
		{
			AssertEquals(new ZDateTime(2023, 9, 21, 12, 23, 7), provider.DateLimit);
		}

		public void TestAdditionalInformation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalInformations.Count", 2, provider.AdditionalInformations.Count);
				var additionalInformations = provider.AdditionalInformations.ToArray();
				var additionalInformation1 = additionalInformations[0];
				AssertEquals("DocumentType", "Z740", additionalInformation1.DocumentType);
				AssertEquals("DocumentComplementaryInformation", "Info1", additionalInformation1.RequestInformation);
				var additionalInformation2 = additionalInformations[1];
				AssertEquals("DocumentType", "Z750", additionalInformation2.DocumentType);
				AssertEquals("DocumentComplementaryInformation", "Info2", additionalInformation2.RequestInformation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM482Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM482.Im482
			{
				ImportOperation = new MCciOperationType55
				{
					Mrn = "12MRN345CDEFG678R9",
					Lrn = "123",
					RequestDate = new DateTime(2023, 9, 21, 12, 23, 7),
					DateLimit = new DateTime(2023, 9, 21, 12, 23, 7),
				},
				DocumentAdditionalInformation = new System.Collections.ObjectModel.Collection<DocumentAdditionalInformationType>
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
			});
		}
		IM482Provider provider;
	}
}
