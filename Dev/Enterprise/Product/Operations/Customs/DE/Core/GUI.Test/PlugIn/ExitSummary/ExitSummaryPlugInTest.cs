using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExitSummaryPlugInTest : EU.GUI.PlugIn.Testing.ExitSummaryPlugInTest
	{
		protected override Type GetUserControlType() => typeof(ExitSummaryUserControl);

		protected override ITestableExitSummaryPlugIn GetTestableExitSummaryPlugInWrapper(EU.Business.Declaration.JobDeclaration declaration) => new TestExitSummaryPlugIn(declaration);

		class TestExitSummaryPlugIn : ExitSummaryPlugIn, ITestableExitSummaryPlugIn
		{
			public TestExitSummaryPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
			{
			}

			public bool EnabledExposed => Enabled;

			public EU.GUI.PlugIn.ExitSummaryPlugIn PlugIn => this;

			public void ChangeTheVisibilityExposed() => base.ChangeTheVisibility();

			public bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed() => base.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}
	}
}
