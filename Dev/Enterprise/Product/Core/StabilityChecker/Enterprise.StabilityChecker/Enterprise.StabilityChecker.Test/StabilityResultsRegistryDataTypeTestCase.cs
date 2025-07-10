using System.Text;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.StabilityChecker.Testing
{
	[TestedType(typeof(StabilityResultsRegistryDataType))]
	sealed class StabilityResultsRegistryDataTypeTestCase : NonPersistentBusinessObjectRegistryDataTypeTestCase<StabilityResultsRegistryDataType>
	{
		protected override StabilityResultsRegistryDataType GetNewDataType()
		{
			return new StabilityResultsRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			StabilityResults emptyStabilityResults = new StabilityResults();
			emptyStabilityResults.DateTimeCalculated = ZDateTime.BrettsBirthday;

			StabilityResults stabilityResults1 = new StabilityResults();
			stabilityResults1.DateTimeCalculated = ZDateTime.BrettsBirthday;
			stabilityResults1.Results.Add(new StabilityResult(StabilityResultLevel.Warning, "I am sick"));

			StabilityResults stabilityResults2 = new StabilityResults();
			stabilityResults2.DateTimeCalculated = ZDateTime.BrettsBirthday;
			stabilityResults2.Results.Add(new StabilityResult(StabilityResultLevel.Critical, "I am dead"));
			stabilityResults2.Results.Add(new StabilityResult(StabilityResultLevel.Healthy, "I am healthy"));
			stabilityResults2.Results.Add(new StabilityResult(StabilityResultLevel.Exception, "oops"));

			StabilityResults stabilityResults3 = new StabilityResults();
			stabilityResults3.DateTimeCalculated = ZDateTime.BrettsBirthday;
			stabilityResults3.Results.Add(new StabilityResult(StabilityResultLevel.Critical, "I am dead", typeof(IStabilityResultHelper)));

			return new[] {
				new ValidSampleAndBinaryValueInDB(emptyStabilityResults, Encoding.UTF8.GetBytes(@"<StabilityResults><DateTimeCalculated>1971-09-18T00:00:00+10:00</DateTimeCalculated></StabilityResults>")),
				new ValidSampleAndBinaryValueInDB(stabilityResults1,  Encoding.UTF8.GetBytes(@"<StabilityResults><DateTimeCalculated>1971-09-18T00:00:00+10:00</DateTimeCalculated><Results><StabilityResult><StabilityLevel>Warning</StabilityLevel><Description>I am sick</Description></StabilityResult></Results></StabilityResults>")),
				new ValidSampleAndBinaryValueInDB(stabilityResults2,  Encoding.UTF8.GetBytes(@"<StabilityResults><DateTimeCalculated>1971-09-18T00:00:00+10:00</DateTimeCalculated><Results><StabilityResult><StabilityLevel>Critical</StabilityLevel><Description>I am dead</Description></StabilityResult><StabilityResult><StabilityLevel>Healthy</StabilityLevel><Description>I am healthy</Description></StabilityResult><StabilityResult><StabilityLevel>Exception</StabilityLevel><Description>oops</Description></StabilityResult></Results></StabilityResults>")),
				new ValidSampleAndBinaryValueInDB(stabilityResults3,  Encoding.UTF8.GetBytes(@"<StabilityResults><DateTimeCalculated>1971-09-18T00:00:00+10:00</DateTimeCalculated><Results><StabilityResult><StabilityLevel>Critical</StabilityLevel><Description>I am dead</Description><HelperAsm>Enterprise.StabilityChecker</HelperAsm><HelperType>Enterprise.StabilityChecker.IStabilityResultHelper</HelperType></StabilityResult></Results></StabilityResults>")),
			};
		}

		public void TestStabilityResultHelperSerialization()
		{
			StabilityResultsRegistryDataType dataType = GetNewDataType();
			StabilityResults results = (StabilityResults)GetValidSamples()[3].ValidSample;
			StabilityResults results2 = dataType.Deserialise(dataType.Serialise(results));
			AssertEquals(results, results2);
			AssertEquals(typeof(IStabilityResultHelper), results2.Results[0].StabilityResultHelper);
		}

		protected override bool HasEditor
		{
			get { return false; }
		}
	}
}
