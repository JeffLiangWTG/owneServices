using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class CusEntryNumberHelperTest : TestCase
{
	public void TestMovementReferenceNumberWithoutVersion()
	{
		CombineAssertions(() =>
		{
			AssertEquals("M123.45", "M123", CusEntryNumberHelper.MovementReferenceNumberWithoutVersion("M123.45"));
			AssertEquals("M123", "M123", CusEntryNumberHelper.MovementReferenceNumberWithoutVersion("M123"));
			AssertEquals("M123.", "M123", CusEntryNumberHelper.MovementReferenceNumberWithoutVersion("M123"));
			AssertEquals("empty", string.Empty, CusEntryNumberHelper.MovementReferenceNumberWithoutVersion(ZString.Empty));
		});
	}

	public void TestMovementReferenceNumberVersion()
	{
		CombineAssertions(() =>
		{
			AssertEquals("M123.45", 45, CusEntryNumberHelper.MovementReferenceNumberVersion("M123.45"));
			AssertNull("M123", CusEntryNumberHelper.MovementReferenceNumberVersion("M123"));
			AssertNull("M123.", CusEntryNumberHelper.MovementReferenceNumberVersion("M123"));
			AssertNull("empty", CusEntryNumberHelper.MovementReferenceNumberVersion(ZString.Empty));
		});
	}
}
