using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProviderSS))]
	public class ApplicationGUIProviderSSTest : ApplicationGUIProviderBaseTest<ApplicationGUIProviderSS, AsycudaManifestHeaderSS>
	{
		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[]
				{
					EU.Manifest.Business.AsycudaBill.Schema.SpecialMentions
				}, columnInfos.Select(s => s.ColumnName));

			var specialMentionsDropEditColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == EU.Manifest.Business.AsycudaBill.Schema.SpecialMentions);
			AssertEquals(false, specialMentionsDropEditColumnStyleInfo.IsVisible);
		}

		public void TestManifestChildTabTextShouldBeCorrect()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);
			var manifest = CreateNewManifest();
			wrapper.Headers.Add(manifest);
			manifest.AMA_ManifestType = ManifestType;
			using (GBCustomsDataRegistry.Instance.EnableSSGBManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				var sgTabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "GBS&STabPage");
				AssertEquals("Manifest TabPage Text", "United Kingdom - S&&S GB", sgTabPage.Text);
			}
		}

		protected override ZString ManifestType => ICSManifestTypes.Codes.SAS;
	}
}
