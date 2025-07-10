using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Module
{
	public class EUH7Controller : ASYCUDA.Module.ASYCUDAManifestController
	{
		public override ControllerID ID => ControllerIDs.Customs.EU.EUH7;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.EUH7;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.EuH7;

		protected override Security.SecurityCheckpoint CheckPointForNew => Env.Security.EuH7;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.EuH7;

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.EuH7;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var typeDecider = new ASYCUDA.Business.AsycudaManifestHeaderTypeDecider();
			var applicationCode = GetCorrectApplicationCodeForH7();
			return Factory.New(typeDecider.GetGlobalManifestType(Factory, Env.CurrentCompany.Country.Code, EUH7ManifestTypes.Codes.EH7, applicationCode));
		}

		string GetCorrectApplicationCodeForH7()
		{
			switch (Env.CurrentCompany.Country.Code)
			{
				case CountryCodes.Ireland:
					return ApplicationCodeTypeList.Codes.EuH7V1;
				default:
					return ApplicationCodeTypeList.Codes.EuH7;
			}
		}

#if DEBUG
		protected override IBusiness GetNewBusinessEntityInFactory(BusinessObjectFactory factory)
		{
			var typeDecider = new ASYCUDA.Business.AsycudaManifestHeaderTypeDecider();
			return factory.New(typeDecider.GetGlobalManifestType(factory, Core.Constants.CountryCodes.Ireland, EUH7ManifestTypes.Codes.EH7, ApplicationCodeTypeList.Codes.EuH7V1));
		}
#endif
	}
}
