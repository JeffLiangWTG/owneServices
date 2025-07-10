using CargoWise.EntityFramework;
using Enterprise.Client.AUS.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.AUS.Modules
{
	public class OriginPreferenceMappingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.OriginPreferenceMapping; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get	{ return Env.Security.OriginPreferenceMapping; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		#region Implementation

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.OriginPreferenceMapping);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OriginPreferenceMappingFilterControl((ClientAUSOriginPreferenceMappingCollection)GridCollection, (OriginPreferenceMappingBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ClientAUSOriginPreferenceMappingCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OriginPreferenceMappingBusinessObject();
		}

		#endregion
	}
}
