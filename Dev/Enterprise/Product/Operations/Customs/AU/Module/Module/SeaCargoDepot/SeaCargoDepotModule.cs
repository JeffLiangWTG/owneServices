using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoDepotModule : CMRModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.SeaCargoDepot; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SeaCargoDepotFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new SeaCargoDepotFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusOutturnHeaderCollection(Factory);
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion

		#region CheckPoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.SeaCargoDepot; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AUCustomsSCADepot; }
		}

		#endregion

		public new SeaCargoDepotOutturnForm ShowNewForm()
		{
			return (SeaCargoDepotOutturnForm)base.ShowNewForm();
		}

		public new SeaCargoDepotOutturnForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			return (SeaCargoDepotOutturnForm)base.ShowEditForm(selectedBusinessObject);
		}

		public new SeaCargoDepotOutturnForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			return (SeaCargoDepotOutturnForm)base.ShowViewForm(selectedBusinessObject);
		}
	}
}
