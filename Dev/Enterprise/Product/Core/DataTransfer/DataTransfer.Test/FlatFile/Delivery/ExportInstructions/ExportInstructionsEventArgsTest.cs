using CargoWise.EntityFramework.Testing;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class ExportInstructionsEventArgsTest : TestCaseWithFactory
	{
		public void TestExportInstructionsEventArgs()
		{
			EmailExportInstructions instructions = new EmailExportInstructions();
			EmailExportInstructionsEventArgs args = new EmailExportInstructionsEventArgs(instructions);

			AssertEquals("Same instance of export instructions in the event args", instructions, args.Instructions);
		}
	}
}
