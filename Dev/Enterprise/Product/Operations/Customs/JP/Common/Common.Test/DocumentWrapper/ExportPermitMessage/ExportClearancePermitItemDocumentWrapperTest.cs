using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(ExportClearancePermitItemDocumentWrapper))]
sealed class ExportClearancePermitItemDocumentWrapperTest : InboundMessageDocumentItemWrapperTest<ExportClearancePermitItemDocumentWrapper, ExportPermitMessageDocumentWrapper>
{
	public void TestItemProperties()
	{
		var wrapper = DocItemWrapper;

		CombineAssertions(() =>
		{
			AssertEquals("I_105", "55", wrapper.I_105);
			AssertEquals("I_106", "66", wrapper.I_106);
			AssertEquals("I_107", "7", wrapper.I_107);
			AssertEquals("I_108", "8888888888888888888888888888888888888888", wrapper.I_108);
			AssertEquals("I_109", "999999999", wrapper.I_109);
			AssertEquals("I_110", "0", wrapper.I_110);
			AssertEquals("I_111", "1,111,111.11111", wrapper.I_111);
			AssertEquals("I_112", "222,222,222,222", wrapper.I_112);
			AssertEquals("I_113", "3333", wrapper.I_113);
			AssertEquals("I_114", "4,444,444,444,444", wrapper.I_114);
			AssertEquals("I_115", "555,555,555,555", wrapper.I_115);
			AssertEquals("I_116", "6666", wrapper.I_116);
			AssertEquals("I_117", "777,777,777,777,777,777", wrapper.I_117);
			AssertEquals("I_118", "888", wrapper.I_118);
			AssertEquals("I_119", "999,999,999,999,999,999", wrapper.I_119);
			AssertEquals("I_120_1", "01", wrapper.I_120_1);
			AssertEquals("I_120_2", "02", wrapper.I_120_2);
			AssertEquals("I_120_3", "03", wrapper.I_120_3);
			AssertEquals("I_120_4", "04", wrapper.I_120_4);
			AssertEquals("I_120_5", "05", wrapper.I_120_5);
			AssertEquals("I_121", "11111", wrapper.I_121);
			AssertEquals("I_122", "2", wrapper.I_122);
			AssertEquals("I_123", "33333", wrapper.I_123);
			AssertEquals("I_124", "444444", wrapper.I_124);
			AssertEquals("I_125", "55555555555555555555555555", wrapper.I_125);
			AssertEquals("I_126", "66666666666666666666666666", wrapper.I_126);
			AssertEquals("I_127", "7", wrapper.I_127);
			AssertEquals("I_128", "8", wrapper.I_128);
			AssertEquals("I_129", "9999999999999999999999", wrapper.I_129);
		});
	}

	protected override string GetDefaultMessageTestFile() => "ExportClearancePermitTestMessage.txt";
}
