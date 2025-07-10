namespace Enterprise.ZArchitecture.Business
{
	public interface IUpdateAuditFields
	{
		bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges { get; }
	}
}
