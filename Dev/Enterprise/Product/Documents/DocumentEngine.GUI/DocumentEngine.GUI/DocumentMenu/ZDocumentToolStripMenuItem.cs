using System.Collections;
using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	public class ZDocumentToolStripMenuItem : ToolStripMenuItem, IZDocumentMenuItem
	{
		public ZDocumentToolStripMenuItem()
		{
			this.Text = ZDocumentsToolStripMenuHelper.DocMenuName;
			this.Name = ZDocumentsToolStripMenuHelper.DocMenuName;
			this.helper = new ZDocumentsToolStripMenuHelper(this);
		}

		readonly ZDocumentsToolStripMenuHelper helper;

		public void Setup(IDocumentSupportable parentBusinessObject, IDocumentEventsForMenu documentEventsForMenu, UserControlProviderList parentUserFieldList, UserControlProviderList parentSystemDefinedFieldList)
		{
			helper.Setup(parentBusinessObject, documentEventsForMenu, parentUserFieldList, parentSystemDefinedFieldList);
		}

		public void LoadMenus(Form parentForm)
		{
			helper.LoadMenus(parentForm, (form, parentBusinessObject, parentUserFieldList) => { return new DocumentCustomisationToolStripMenusMaker(form, parentBusinessObject, parentUserFieldList, helper); });
		}

		protected override void Dispose(bool disposing)
		{
			helper.Dispose(disposing);
			base.Dispose(disposing);
		}

		#region IZDocumentMenuItem

		IList IZDocumentMenuItem.Items
		{
			get { return DropDownItems; }
		}

		#endregion

		#region HelperForTesting
#if DEBUG
		public ZDocumentsToolStripMenuHelper HelperForTesting { get { return helper; } }
#endif
		#endregion
	}
}
