using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsing.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.BarcodeParsing.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
namespace Enterprise.BarcodeParsing.Module
{
	public class BarcodeValidationModule : ZFilterGridModule
	{
		#region Controller

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.BarcodeValidation);

		#endregion

		#region Module Filter

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new BarcodeValidationRuleFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new BarcodeValidationRuleFilterControl((BarcodeValidationRuleCollection)GridCollection, (BarcodeValidationRuleFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new BarcodeValidationRuleCollection(Factory);

		#endregion

		#region ID

		public override ModuleIdentifier ID => ModuleIDs.BarcodeValidation;

		#endregion

		#region Licence

		//#warning Not sure what Licence we will be using
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.BarcodeParsing;

		#endregion
	}
}
