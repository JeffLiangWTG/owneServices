using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF409;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	[TestedType(typeof(RF409Provider))]
	class RF409ProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new RF409Provider(null));
		}

		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ApplicationReferenceId", "Abcd123aisdu3eh2u89764", provider.ApplicationReferenceId);
				AssertEquals("ApplicationDecisionCodeType_1_1", "20230802", provider.ApplicationDecisionCodeType);
				AssertEquals("RefundApplicationAccepted", true, provider.RefundApplicationAccepted);
				AssertEquals("DecisionTakingCustomsAuthority", "SOYJO", provider.DecisionTakingCustomsAuthority);
				AssertEquals("MRN", "123456789IE", provider.MRN);
				AssertEquals("TimeLimit", "1502", provider.TimeLimit);
				AssertEquals("StatementOfTheDecision", "STA_OF_DEC", provider.StatementOfTheDecision);
				AssertEquals("DescriptionOfGrounds", "DESC_OF_GROUNDS", provider.DescriptionOfGrounds);
			});
		}

		public void TestGeneralRemarks()
		{
			AssertNotNull("GeneralRemarks", provider.GeneralRemarks);
			AssertEquals("GeneralRemarks Count", 3, provider.GeneralRemarks.Count);

			var providerWithNoGeneralRemarks = new RF409Provider(new Rf409Type());
			AssertNotNull("GeneralRemarks when RF409 errors are null", providerWithNoGeneralRemarks.GeneralRemarks);
			AssertEquals("GeneralRemarks Count when RF409 errors are null", 0, providerWithNoGeneralRemarks.GeneralRemarks.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new RF409Provider(GenerateMessage());
		}

		RF409Provider provider;

		Rf409Type GenerateMessage()
		{
			return new Rf409Type
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u89764",
					ApplicationDecisionCodeType11 = "20230802",
					RefundApplicationAccepted = "1",
					DecisionTakingCustomsAuthority = "SOYJO",
				},
				Mrn = "123456789IE",
				TimeLimit = "1502",
				StatementOfTheDecision = "STA_OF_DEC",
				DescriptionOfGrounds = "DESC_OF_GROUNDS",
				GeneralRemarks = new Collection<RemarksType>
				{
					new RemarksType
					{
						GeneralRemarks = "ETU",
					},
					new RemarksType
					{
						GeneralRemarks = "EHU",
					},
					new RemarksType
					{
						GeneralRemarks = "EPU",
					}
				}
			};
		}
	}
}
