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
	public class SeaCargoOutturnBillsModule : CMRModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.SeaCargoOutturnBills; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SeaCargoOutturnBillsFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new SeaCargoOutturnBillsFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			//return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoOutturnBillsController);
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DepotCusOutturnCollection(Factory);
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion

		#region New / Edit / View Form

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			DepotCusOutturn outturn = (DepotCusOutturn)selectedBusinessObject;
			CusOutturnHeader header = GetOutturnHeader(outturn);
			if (header != null)
			{
				SeaCargoDepotOutturnForm form = (SeaCargoDepotOutturnForm)base.ShowEditForm(header);
				if (form != null)
				{
					form.SelectOutturn(outturn);
					return form;
				}
			}
			return null;
		}

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			DepotCusOutturn outturn = (DepotCusOutturn)selectedBusinessObject;
			CusOutturnHeader header = GetOutturnHeader(outturn);
			if (header != null)
			{
				SeaCargoDepotOutturnForm form = (SeaCargoDepotOutturnForm)base.ShowViewForm(header);
				if (form != null)
				{
					form.SelectOutturn(outturn);
					return form;
				}
			}
			return null;
		}

		CusOutturnHeader GetOutturnHeader(DepotCusOutturn outturn)
		{
			return outturn.Header;
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		#endregion

		#region CheckPoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.SeaCargoDepot; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AUCustomsSCAOutturnBills; }
		}

		#endregion

		#region Workflow

		public override bool SupportsWorkflow => true;

		#endregion
	}
}
