using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class LicenseEntryHeaderController : Customs.Module.EntryHeaderController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.BR.LicenseEntryHeader;

		public override ControllerID ID => ControllerIDs.Customs.BR.LicenseEntryHeader;

		protected override ControllerID JobDeclarationControllerID => ControllerIDs.Customs.BR.License;

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var declaration = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity) as JobDeclaration;
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
			return declaration;
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusEntryHeader);
	}
}
