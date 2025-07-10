using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class LicenseController : JobDeclarationController
	{
		public LicenseController()
		{
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.BR.License;

		public override ControllerID ID => ControllerIDs.Customs.BR.License;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.BRLicenseView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.BRLicenseNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.BRLicenseDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.BRLicenseEdit;

		#endregion

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var declaration = (JobDeclaration)base.GetNewBusinessEntityInLocalFactory();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
			return declaration;
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var declaration = (JobDeclaration)base.LoadBusinessEntity(factory, sourceEntityPK);
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
			return declaration;
		}
	}
}
