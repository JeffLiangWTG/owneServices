using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ResourceStrings.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ResourceStrings.Module
{
	public class LocalLanguagesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.LocalLanguages;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.LocalLanguages;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LocalLanguages;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new LocalLanguagesController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new LocalLanguagesFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LocalLanguagesFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefLocalLanguageCollection(Factory);
		}

		public override bool AllowUniversalCopy => false;
	}
}
