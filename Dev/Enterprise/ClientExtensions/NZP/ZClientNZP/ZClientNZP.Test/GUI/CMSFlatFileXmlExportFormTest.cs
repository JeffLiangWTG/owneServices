using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.NZP.GUI
{
	[TestedType(typeof(CMSFlatFileXmlExportForm))]
	public class CMSFlatFileXmlExportFormTest : ZFormBasherTest
	{
		public void TestToZdateEditControlIsEnabled()
		{
			using (CMSFlatFileXmlExportForm form = new CMSFlatFileXmlExportForm(new CMSExportGUIWrapper(Factory)))
			{
				Assert(form.InternalNewExportBatchGroupBoxTest.Enabled);
				Assert(form.InternalDatesGroupBoxTest.Enabled);
				Assert(form.InternalToZDateEditTest.Enabled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new CMSFlatFileXmlExportForm(new CMSExportGUIWrapper(Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();
			NZPDataRegistry.Instance.CMSLastDateExported = ZDateTime.Now;
		}
	}
}
