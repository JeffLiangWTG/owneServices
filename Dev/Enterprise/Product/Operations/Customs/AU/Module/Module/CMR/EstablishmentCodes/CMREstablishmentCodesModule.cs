using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class CMREstablishmentCodesModule : CMRSearchOnlyModule
	{
		#region Module Overriden Properties

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.CMREstablishmentCodes; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CMREstablishmentCodesFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CMREstablishmentCodesFilterControl(GridCollection, (CMREstablishmentCodesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CMREstablishmentCodesCollection(Factory);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.CMREstablishmentCodes);
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion
	}
}
