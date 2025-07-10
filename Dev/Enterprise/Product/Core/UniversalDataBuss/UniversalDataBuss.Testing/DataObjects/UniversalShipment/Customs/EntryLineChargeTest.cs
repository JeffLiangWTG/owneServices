using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(EntryLineCharge))]
	sealed class EntryLineChargeTest : DataObjectTestCase<EntryLineCharge>
	{
		public void TestTypeAttributes()
		{
			var propertyInfo = typeof(EntryLineCharge).GetProperties().Single(x => x.Name == nameof(EntryLineCharge.Type));
			CombineAssertions(() =>
			{
				AssertEquals("Mandatory", 1, propertyInfo.GetCustomAttributes(typeof(MandatoryAttribute), false).Length);
				AssertEquals("CandidateKey", 1, propertyInfo.GetCustomAttributes(typeof(CandidateKeyAttribute), false).Length);
			});
		}

		public void TestAmountAttributes()
		{
			var propertyInfo = typeof(EntryLineCharge).GetProperties().Single(x => x.Name == nameof(EntryLineCharge.Amount));
			AssertEquals(1, propertyInfo.GetCustomAttributes(typeof(MandatoryAttribute), false).Length);
		}

		public void TestMethodOfCalculationAttributes()
		{
			var propertyInfo = typeof(EntryLineCharge).GetProperties().Single(x => x.Name == nameof(EntryLineCharge.MethodOfCalculation));
			AssertEquals(1, propertyInfo.GetCustomAttributes(typeof(CandidateKeyAttribute), false).Length);
		}

		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>()
		{
			{ nameof(EntryLineCharge.Source), 3 }
		};
	}
}

