using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CusSCAOceanBillController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.CusSCAOceanBill; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CusSCAOceanBill; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusSCAOceanBill); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<CusSCAOceanBill>();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This controller does not have GUI.");
		}

		#region Security Checkpoint

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
