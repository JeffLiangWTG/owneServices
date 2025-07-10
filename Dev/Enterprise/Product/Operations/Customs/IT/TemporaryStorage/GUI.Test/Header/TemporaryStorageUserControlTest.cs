using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

sealed class TemporaryStorageUserControlTest : TestCaseWithFactory
{
	public void TestAccountNameDropEdit()
	{
		using var userControl = new TemporaryStorageUserControl();

		userControl.AssertContainsControl<ZDropEdit>(nameof(userControl.AccountNameDropEdit), x => x
			.WithBindTo(nameof(TemporaryStorageHeader.AMA_CustomsProfile)));
	}

	public void TestRepresentativeQualificationDropEdit()
	{
		using var userControl = new TemporaryStorageUserControl();

		userControl.AssertContainsControl<ZDropEdit>(nameof(userControl.RepresentativeQualificationDropEdit), x => x
			.WithBindTo(nameof(TemporaryStorageHeader.AMA_AgentType)));
	}
}
