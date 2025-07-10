using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class HouseBilleManifestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.CAHouseBilleManifest; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CAHouseBilleManifest; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusCAeMHMaster); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<CusCAeMHMaster>();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusCAeMHMasterForm((CusCAeMHMaster)businessEntity);
		}

		#region Security Checkpoint

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		#endregion
	}
}
