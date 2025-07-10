using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class LPCODeclarationController : JobDeclarationController
	{
		public LPCODeclarationController()
		{
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.BR.LPCODeclaration;

		public override ControllerID ID => ControllerIDs.Customs.BR.LPCODeclaration;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.BRLPCODeclarationView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.BRLPCODeclarationNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.BRLPCODeclarationDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.BRLPCODeclarationEdit;

		#endregion

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return CreateNewBusinessObject(Factory);
		}

		public static JobDeclaration CreateNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.LPCO;
			return declaration;
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var declaration = (JobDeclaration)base.LoadBusinessEntity(factory, sourceEntityPK);
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.LPCO;
			return declaration;
		}
	}
}
