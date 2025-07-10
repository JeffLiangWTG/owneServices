
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.SeaCargo
{
	public class CusSCADepotContainerModule : ZFilterGridModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.SeaCargoDepot; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AUCustomsSCA; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.CusSCADepotContainer; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.CusSCADepotContainer);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusSCADepotContainerFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusSCADepotContainerList(Factory, "");
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusSCADepotContainerFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
