using System.Collections;
using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	public class ZDocumentMenuItem : ZMenuItem, IZDocumentMenuItem
	{
		public ZDocumentMenuItem()
			: base(ZDocumentsMenuItemMenuHelper.DocMenuName)
		{
			this.helper = new ZDocumentsMenuItemMenuHelper(this);
#if DEBUG
			if (Globals.IsTest)
			{
				this.HelperForTesting = this.helper;
			}
#endif
		}

		ZDocumentsMenuItemMenuHelper helper;

		public void Setup(IDocumentSupportable parentBusinessObject, IDocumentEventsForMenu documentEventsForMenu, UserControlProviderList parentUserFieldList, UserControlProviderList parentSystemDefinedFieldList)
		{
			helper.Setup(parentBusinessObject, documentEventsForMenu, parentUserFieldList, parentSystemDefinedFieldList);
		}

		public void LoadMenus(Form parentForm)
		{
			helper.LoadMenus(parentForm, (form, parentBusinessObject, parentUserFieldList) => { return GetDocumentCustomisationMenuItemMenusMaker(form, parentBusinessObject, parentUserFieldList, helper); });
		}

		DocumentCustomisationMenuItemMenusMaker GetDocumentCustomisationMenuItemMenusMaker(Form form, IDocumentSupportable parentBusinessObject, UserControlProviderList parentUserFieldList, ZDocumentsMenuItemMenuHelper documentsMenuItemMenuHelper)
		{
			var shouldUseCombinationMenusMaker = false;
			if (form is ZForm zForm)
			{
				shouldUseCombinationMenusMaker = zForm.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer) != null;
			}

			return shouldUseCombinationMenusMaker
				? new CombinationCustomisationMenuItemMenusMaker(form, parentBusinessObject, parentUserFieldList, documentsMenuItemMenuHelper)
				: new DocumentCustomisationMenuItemMenusMaker(form, parentBusinessObject, parentUserFieldList, documentsMenuItemMenuHelper);
		}

		protected override void Dispose(bool disposing)
		{
			helper?.Dispose(disposing);
			helper = null;
			base.Dispose(disposing);
		}

		#region IZDocumentMenuItem

		IList IZDocumentMenuItem.Items
		{
			get { return MenuItems; }
		}

		#endregion

		#region HelperForTesting
#if DEBUG
		public ZDocumentsMenuItemMenuHelper HelperForTesting { get; private set; }
#endif
		#endregion
	}
}
