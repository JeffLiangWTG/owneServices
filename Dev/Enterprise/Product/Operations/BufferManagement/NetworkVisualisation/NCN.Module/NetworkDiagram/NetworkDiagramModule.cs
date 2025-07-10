using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.NetworkVisualisation.Module
{
	public class NetworkDiagramModule : ZFilterGridModule
	{
		#region ZFilterGridModule Overrides

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.NetworkDiagram);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new NetworkDiagramFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new NetworkDiagramFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DiagramShapeCollection(Factory);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = base.GetNewStandardMenuItems();

			if (AllowNew)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("458c1686-ea3e-4b4b-92fa-92627d7f135e", "Non-scaled Diagram"), delegate
				{ ShowNewForm(DiagramType.NonScaled); }));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("4d2f4ad4-bab5-4356-84b1-b1f62e8365c8", "Scaled Diagram"), delegate
				{ ShowNewForm(DiagramType.Scaled); }));
			}

			return menuItems;
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.NetworkDiagram; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.NetworkDiagram; }
		}

		public override bool AllowNew => AddAdditionalDisplayFilter == null;
		public override bool AllowDelete => AddAdditionalDisplayFilter == null;

		#endregion

		#region Implementation

		void ShowNewForm(DiagramType type)
		{
			MainThreadRunner.RunOnMainThread(() =>
			{
				new NetworkDiagramController(type).ShowNewForm();
			});
		}

		#endregion
	}
}
