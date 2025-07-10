using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module Controller for SeaCargo.
	/// </summary>
	public class SeaCargoStandAloneController : BaseSeaCargoController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.SeaCargoStandAloneController; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.AU.SeaCargo; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusSCAOceanBill); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SeaCargoStandAloneForm((CusSCAOceanBill)businessEntity);
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AUCustomsSCAImportModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AUCustomsSCAImportModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AUCustomsSCAImportModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AUCustomsSCAImportModify; }
		}

		#endregion Security Check Points

	}
}
