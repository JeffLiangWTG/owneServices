using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstructionComparer))]
	class CusEntryInstructionComparerTest : Customs.Business.Testing.CusEntryInstructionComparerAbstractTest<CusEntryInstructionComparer>
	{
		public override void TestCompare()
		{
			var testInstruction1 = Factory.New<CusEntryInstruction>();
			var testInstruction2 = Factory.New<CusEntryInstruction>();

			testInstruction1.CEI_Style = "12";
			testInstruction1.CEI_SubStyle = "3";
			testInstruction2.CEI_Style = "12";
			testInstruction2.CEI_SubStyle = "3";
			CombineAssertions(() =>
			{
				AssertEquals("same2 CusEntryInstruction", 0, comparer.Compare(testInstruction1, testInstruction2));

				testInstruction1.CEI_SubStyle = "3";
				testInstruction2.CEI_SubStyle = "4";
				AssertEquals("1<2 SubStyle", -1, comparer.Compare(testInstruction1, testInstruction2));

				testInstruction1.CEI_SubStyle = "4";
				testInstruction2.CEI_SubStyle = "3";
				AssertEquals("1>2 SubStyle", 1, comparer.Compare(testInstruction1, testInstruction2));

				testInstruction1.CEI_Style = "11";
				testInstruction2.CEI_Style = "12";
				AssertEquals("1<2 Style", -1, comparer.Compare(testInstruction1, testInstruction2));

				testInstruction1.CEI_Style = "12";
				testInstruction2.CEI_Style = "11";
				AssertEquals("1>2 Style", 1, comparer.Compare(testInstruction1, testInstruction2));
			});
		}
	}
}
