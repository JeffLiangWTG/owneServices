using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module Controller for VoyageManifest.
	/// </summary>
	public class VoyageManifestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public VoyageManifestController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.VoyageManifest; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusSeaManTranHead); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new VoyageManifestForm((CusSeaManTranHead)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AUCustomsVoyageManifest; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AUCustomsVoyageManifestEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AUCustomsVoyageManifestNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AUCustomsVoyageManifestEdit; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
