using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIRefZonePivot))]
	public class EDIRefZonePivotTest : RefZonePivotTest
	{
		public void TestTypeDecider()
		{
			AssertEquals(typeof(EDIRefZonePivot), Factory.New<RefZonePivot>().GetType());
		}

		public void TestGetNewValidation()
		{
			EDIRefZonePivot pivot = Factory.New<EDIRefZonePivot>();

			AssertEquals(typeof(EDIRefZonePivotValidation), pivot.Validation.GetType());
		}
	}
}
