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
	public class ProductImportAndExportModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.ProductImportAndExport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get	{ return Env.Security.ProductImportAndExport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		#region Implementation

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.ProductImportAndExport);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ProductImportRegistryControl((ClientAUSProductImportRegistryCollection)GridCollection, (ProductImportRegistryBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ClientAUSProductImportRegistryCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ProductImportRegistryBusinessObject();
		}

		#endregion
	}
}
