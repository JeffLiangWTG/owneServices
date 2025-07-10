using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Module
{
	public class EUH7BillModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.EUH7Bill;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EuH7;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EU.EUH7Bill);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EUH7BillFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new EUH7BillFilterStripControl(GridCollection, (EUH7BillFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new EUH7BillModuleCollection<AsycudaBill>(Factory);

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems())
			{
				new ConvertToStandAloneDeclarationMenuItem(GetSelectedBusinessObjectsInNewFactory, shouldAutomaticallySaveAddressChanges: true)
			};
			return menuItems.ToArray();
		}

		BusinessObject[] GetSelectedBusinessObjectsInNewFactory()
		{
			var query = new ZQuery(AsycudaBillSchema.PK, SelectedBusinessObjects.Select(b => b.PK)) { ReLoadExistingRows = true };
			return new BusinessObjectFactory().Load<AsycudaBill>(query);
		}

		public override bool AllowNew => false;
	}
}
