using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module for VoyageManifest.
	/// </summary>
	public class VoyageManifestModule : CMRModule
	{
		public VoyageManifestModule()
		{
		}

		#region Overrides

		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.VoyageManifest; }
		}

		#endregion

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ShippingManagerCMRImportDeclaration; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AUCustomsVoyageManifest; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.VoyageManifest);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new VoyageManifestFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusSeaManTranHeadCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new VoyageManifestFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion

	}
}
