using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoOutturnBillsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.SeaCargoOutturnBillsController; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusOutturnHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SeaCargoDepotOutturnForm((CusOutturnHeader)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.AU.SeaCargoOutturnBills; }
		}

		#region CheckPoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AUCustomsSCAOutturnBillsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AUCustomsSCAOutturnBillsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AUCustomsSCAOutturnBillsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AUCustomsSCAOutturnBills; }
		}

		#endregion
	}
}
