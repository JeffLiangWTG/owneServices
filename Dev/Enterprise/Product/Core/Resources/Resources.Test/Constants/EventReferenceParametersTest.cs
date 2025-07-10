using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class EventReferenceParametersTest : TestCase
	{
		public void TestAllEventReferenceParameters()
		{
			var declaredConstants = typeof(CargoWise.EventReference.Constants.EventReferenceParameters.Codes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral)
				.Select(fi => fi.GetValue(null));

			AssertContainsExactElementsInAnyOrder("All EventReferenceParameters",
				declaredConstants,
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.All);
		}

		public void TestAllEventReferenceParameters_Descriptions()
		{
			var descriptions = typeof(Constants.EventReferenceParameters.Descriptions)
				.GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Select(m => m.Name);
			var codes = typeof(CargoWise.EventReference.Constants.EventReferenceParameters.Codes)
				.GetFields()
				.Select(c => c.Name);

			AssertContainsExactElementsInAnyOrder("All Codes from CargoWise.EventReference.Constants.EventReferenceParameters have Core Descriptions",
				codes,
				descriptions);
		}
	}
}
