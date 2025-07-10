using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Core.Modules;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestsSubclassesOf(typeof(ReportsForm))]
	public abstract class ReportsFormAbstractTest<TParent> : ZFormBasherTest
		where TParent : CusIntrastatGroup
	{
		protected override Form GetFormToBashCore()
		{
			var report = IntrastatTestDataHelper.New(Factory).NewCusIntrastatGroupWithValidData<TParent>();
			Factory.Save();

			var form = new ReportsForm(report);
			form.ControllerID = ControllerIDs.Customs.EU.IntrastatReports;

			return form;
		}

		[DeveloperOnlyTest]
		public new void TestCorrectModuleIdAndMenuSection()
		{
			var registry = new Mock<IEUIntrastatCustomsRegistry>();
			registry.SetupGet(r => r.IsIntrastatEnabled).Returns(true);
			using (ObjectFactory.Substitute(registry.Object))
			{
				var tree = new ModuleTree();

				var moduleTreeLoader = ObjectFactory.Get<IModuleTreeLoader>();
				moduleTreeLoader.Initialise(tree, Environment.Env.Security);
				moduleTreeLoader.LoadModules();

				ModuleTree.OverrideTreeForTest(tree);

				base.TestCorrectModuleIdAndMenuSection();
			}
		}
	}
}
