using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class LPCOEntryHeaderController : Customs.Module.EntryHeaderController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.BR.LPCOEntryHeader;

		public override ControllerID ID => ControllerIDs.Customs.BR.LPCOEntryHeader;

		protected override ControllerID JobDeclarationControllerID => ControllerIDs.Customs.BR.LPCODeclaration;

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var declaration = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity) as JobDeclaration;
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.LPCO;
			return declaration;
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusEntryHeader);
	}
}
