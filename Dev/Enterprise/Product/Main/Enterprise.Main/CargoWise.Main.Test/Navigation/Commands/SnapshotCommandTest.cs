using CargoWise.Main.Navigation;
using CargoWise.Main.Service;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Test.Navigation.Commands;

public class SnapshotCommandTest : TestCase
{
	public void TestFetchValue_WhenSnapshotIsNull()
	{
		AssertNoExceptionThrown(() => SnapshotCommand.FetchValue(null));
	}

	public void TestFetchValue_WhenSnapshotNotNull_WhenError()
	{
		var snapshot = new Snapshot();

		AssertNoExceptionThrown(() => SnapshotCommand.FetchValue(snapshot));

		AssertNull(snapshot.Value);
		Assert("Should have an error", snapshot.HasError);
		Assert("Should have an error message", !string.IsNullOrEmpty(snapshot.ErrorMessage));
	}

	public void TestFEtchValue_WhenSuccess()
	{
		var service = new Mock<ISnapshotQueryService>();
		service.Setup(s => s.QuerySnapShotResult(It.IsAny<Snapshot>()))
			.Returns(12345);

		var snapshot = new Snapshot();
		AssertNoExceptionThrown(() => SnapshotCommand.FetchValue(snapshot, service.Object));

		AssertEquals("Should have value", "12,345", snapshot.Value);
		Assert("Shouldn't have an error", !snapshot.HasError);
		Assert("Shouldn't have an error message", string.IsNullOrEmpty(snapshot.ErrorMessage));
	}
}
