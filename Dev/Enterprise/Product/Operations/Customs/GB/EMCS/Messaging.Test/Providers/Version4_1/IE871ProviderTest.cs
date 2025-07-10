using System;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie871;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE871ProviderTest : Business.Testing.DataProviderTestCase<IE871Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE871Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals("20GB41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", Provider.MessageIdentifier);
		}

		public void TestExciseMovementEad()
		{
			AssertNotNull(Provider.ExciseMovementEad);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("Ead", "20GB41000000001870745", Provider.ExciseMovementEad.AdministrativeReferenceCode);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Sequence", "1", Provider.ExciseMovementEad.SequenceNumber);
		}

		public void TestGlobalExplanation()
		{
			AssertEquals("GlobalExplanation", "Global Explanation for Shrtage or Excess", Provider.GlobalExplanation);
		}

		public void TestLines()
		{
			message.Body.ExplanationOnReasonForShortage.BodyAnalysis = new System.Collections.ObjectModel.Collection<BodyAnalysisType>
{
				new BodyAnalysisType(),
				new BodyAnalysisType()
			};

			CombineAssertions(() =>
			{
				var lines = Provider.Lines;
				AssertEquals("Count", 2, lines.Count);
				AssertSame("Cached", lines, Provider.Lines);
			});
		}

		public void TestLines_Empty()
		{
			CombineAssertions(() =>
			{
				var lines = Provider.Lines;
				AssertEquals("Count", 0, lines.Count);
				AssertSame("Cached", lines, Provider.Lines);
			});
		}

		public void TestBodyAnalysisProviderConstructor()
		{
			message.Body.ExplanationOnReasonForShortage.BodyAnalysis = new System.Collections.ObjectModel.Collection<BodyAnalysisType>
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = Provider.Lines);
		}

		public void TestBodyAnalysisProviderValues()
		{
			message.Body.ExplanationOnReasonForShortage.BodyAnalysis = new System.Collections.ObjectModel.Collection<BodyAnalysisType>
			{
				new BodyAnalysisType { BodyRecordUniqueReference = "1", ExciseProductCode = "T200", ActualQuantity = 12.5m, Explanation = new LsdExplanationType() { Language = "en", Value = "Shortage or Excess explanation" } }
			};

			CombineAssertions(() =>
			{
				var line = Provider.Lines.First();
				AssertEquals("Line Number", "1", line.LineNumber);
				AssertEquals("Excise Product Code", "T200", line.ExciseProductCode);
				AssertEquals("Actual Quantity", 12.5m, line.ActualQuantity);
				AssertEquals("Explanation", "Shortage or Excess explanation", line.Explanation);
			});
		}

		public void TestAnalysisGlobalExplanation_Empty()
		{
			GetProvider();
			message.Body.ExplanationOnReasonForShortage.Analysis = null;
			AssertEquals("Global Explanation is empty", ZString.Empty, Provider.GlobalExplanation);
		}

		protected override IE871Provider GetProvider() => new IE871Provider(message);

		protected override void SetUp()
		{
			base.SetUp();
			message = new Ie871Type()
			{
				Header = new HeaderType
				{
					MessageIdentifier = "0072260102"
				},
				Body = new BodyType()
				{
					ExplanationOnReasonForShortage = new ExplanationOnReasonForShortageType()
					{
						ExciseMovement = new ExciseMovementType()
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							SequenceNumber = "1"
						},
						Analysis = new AnalysisType()
						{
							GlobalExplanation = new LsdGlobalExplanationType() { Language = "en", Value = "Global Explanation for Shrtage or Excess" }
						},
					}
				}
			};
		}
		Ie871Type message;
	}
}
