using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.EDI.MasterFiles
{
	public class EDIRefDocOrgCusCodeModule : RefDocOrgCusCodeModule
	{
		#region Allow New / Edit / Delete

		public override bool AllowNew => EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.Value;

		public override bool AllowEdit => EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.Value;

		public override bool AllowDelete => EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule.Value;

		#endregion
	}
}
