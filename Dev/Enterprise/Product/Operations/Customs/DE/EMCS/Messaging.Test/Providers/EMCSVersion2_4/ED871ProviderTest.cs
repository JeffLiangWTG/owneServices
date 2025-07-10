using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	[TestedType(typeof(ED871Provider))]
	class ED871ProviderTest : InboundDataProviderTestCase<IED871, ED871Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED871Provider(null));
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestMessageGroup()
		{
			AssertEquals(EmcsMessageSubTypeList.Codes.Eme, dataProvider.MessageGroup);
		}

		public void TestExciseMovement()
		{
			AssertNotNull(dataProvider.ExciseMovement);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("Ead", "20DE41000000001870745", dataProvider.ExciseMovement.AdministrativeReferenceCode);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Sequence", "1", dataProvider.ExciseMovement.SequenceNumber);
		}

		public void TestGlobalExplanation()
		{
			AssertEquals("GlobalExplanation", "Global Explanation for Shrtage or Excess", dataProvider.GlobalExplanation);
		}

		public void TestLines()
		{
			message.Body.ExplanationOnReasonForShortage.BodyAnalysis = new[]
			{
				new ED871CBodyExplanationOnReasonForShortageBodyAnalysis(),
				new ED871CBodyExplanationOnReasonForShortageBodyAnalysis()
			};

			CombineAssertions(() =>
			{
				var lines = dataProvider.Lines;
				AssertEquals("Count", 2, lines.Count);
				AssertSame("Cached", lines, dataProvider.Lines);
			});
		}

		public void TestLines_Empty()
		{
			CombineAssertions(() =>
			{
				var lines = dataProvider.Lines;
				AssertEquals("Count", 0, lines.Count);
				AssertSame("Cached", lines, dataProvider.Lines);
			});
		}

		public void TestBodyAnalysisProviderConstructor()
		{
			message.Body.ExplanationOnReasonForShortage.BodyAnalysis = new ED871CBodyExplanationOnReasonForShortageBodyAnalysis[]
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.Lines);
		}

		public void TestBodyAnalysisProviderValues()
		{
			message.Body.ExplanationOnReasonForShortage.BodyAnalysis = new[]
			{
				new ED871CBodyExplanationOnReasonForShortageBodyAnalysis { BodyRecordUniqueReference = "1", ExciseProductCode = "T200", ActualQuantity = 12.5m, Explanation = "Shortage or Excess explanation" }
			};

			CombineAssertions(() =>
			{
				var line = dataProvider.Lines.Single();
				AssertEquals("Line Number", "1", line.LineNumber);
				AssertEquals("Excise Product Code", "T200", line.ExciseProductCode);
				AssertEquals("Actual Quantity", 12.5m, line.ActualQuantity);
				AssertEquals("Explanation", "Shortage or Excess explanation", line.Explanation);
			});
		}

		public void TestAnalysisGlobalExplanation_Empty()
		{
			message.Body.ExplanationOnReasonForShortage.Analysis = null;
			AssertEquals("Global Explanation is empty", ZString.Empty, dataProvider.GlobalExplanation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED871C()
			{
				Header = new ED871CHeader()
				{
					MessageGroup = ED871CHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102"
				},
				Body = new ED871CBody()
				{
					ExplanationOnReasonForShortage = new ED871CBodyExplanationOnReasonForShortage()
					{
						ExciseMovementEad = new ED871CBodyExplanationOnReasonForShortageExciseMovementEad()
						{
							AdministrativeReferenceCode = "20DE41000000001870745",
							SequenceNumber = "1"
						},
						Analysis = new ED871CBodyExplanationOnReasonForShortageAnalysis()
						{
							GlobalExplanation = "Global Explanation for Shrtage or Excess"
						}
					}
				}
			};
			dataProvider = new ED871Provider(message);
		}
		ED871C message;
		IED871 dataProvider;

		protected override ED871Provider GetProvider() => (ED871Provider)dataProvider;
	}
}
