using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMManifestMessageSendingOperationalActionRunner))]
sealed class CGMManifestMessageSendingOperationalActionRunnerTest : TestCaseWithFactory
{
	public void TestSendMessage()
	{
		var log = new DummyOperationalActionSectionLog();
		var header1 = Factory.New<CGMAsycudaManifestHeader>();
		header1.AMA_JobReference = "IN123";
		header1.AMA_TransportMode = Core.Constants.TransportModes.Sea;
		header1.AMA_CustomsOffice = "INMUM";

		var header2 = Factory.New<CGMAsycudaManifestHeader>();
		header2.AMA_JobReference = "IN456";
		header2.AMA_TransportMode = Core.Constants.TransportModes.Sea;
		header2.AMA_CustomsOffice = "INMUM";

		var header3 = Factory.New<CGMAsycudaManifestHeader>();
		header3.AMA_JobReference = "IN789";
		header3.AMA_TransportMode = Core.Constants.TransportModes.Sea;
		Factory.Save();

		var runner = new CGMManifestMessageSendingOperationalActionRunner();
		runner.SendMessage(new[] { header1, header2, header3 }, log, false);
		var message = log.MessagesString();

		CombineAssertions(() =>
		{
			AssertContains("WARNING: [HL IN123] Skipped. Errors & Warning observed.", message);
			AssertContains("WARNING: [HL IN456] Skipped. Errors & Warning observed.", message);
			AssertContains("WARNING: [HL IN789] Skipped. Errors & Warning observed.", message);

			log = new DummyOperationalActionSectionLog();
			runner.SendMessage(new[] { header1, header2, header3 }, log, true);
			message = log.MessagesString();
			AssertContains("INFO: [HL IN123] Successful. Message Number: [HL 0000001]", message);
			AssertContains("INFO: [HL IN456] Successful. Message Number: [HL 0000002]", message);
			AssertContains("WARNING: [HL IN789] Skipped. Errors & Warning observed.", message);
			AssertContains("INFO: Total Transactions Selected\t: 3", message);
			AssertContains("Transactions Successful\t: 2", message);
			AssertContains("Transactions Skipped\t: 1", message);

			log = new DummyOperationalActionSectionLog();
			runner.SendMessage(new[] { header1, header2, header3 }, log, true);
			message = log.MessagesString();
			AssertContains("INFO: [HL IN123] Skipped. Message already exists.", message);
			AssertContains("INFO: [HL IN456] Skipped. Message already exists.", message);
			AssertContains("WARNING: [HL IN789] Skipped. Errors & Warning observed.", message);
			AssertContains("Transactions Skipped\t: 3", message);
		});
	}
}
