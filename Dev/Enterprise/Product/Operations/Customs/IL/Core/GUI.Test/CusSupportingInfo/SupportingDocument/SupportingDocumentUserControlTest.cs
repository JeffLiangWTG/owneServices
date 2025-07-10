using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class SupportingDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new SupportingDocumentUserControl())
			{
				var supportingDocumentTabControl = control.FindSingle<ZTemplateTabControl>("SupportingDocumentTabControl");
				CombineAssertions("SupportingDocumentTabControl", () =>
				{
					AssertNotNull(supportingDocumentTabControl);

					AssertEquals("Tab Pages count", 1, supportingDocumentTabControl.TabCount);

					AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
					{
						"SupportingDocumentTabPage"
					}, supportingDocumentTabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
				});

				var metaDatTabControl = control.FindSingle<ZTabControl>("MetaDatTabControl");
				CombineAssertions("MetaDatTabControl", () =>
				{
					AssertNotNull(metaDatTabControl);

					AssertEquals("Tab Pages count", 1, metaDatTabControl.TabCount);

					AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
					{
						"MetaDataTabPage"
					}, metaDatTabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
				});
			}
		}
	}
}
