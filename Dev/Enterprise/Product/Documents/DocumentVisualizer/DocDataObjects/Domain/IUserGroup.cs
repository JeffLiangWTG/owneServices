using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IUserGroup
	{
		ZString Code { get; }
		ZString Description { get; }
		ZString DomainName { get; }
		ZString Category { get; }
		IUserGroup ParentGroup { get; }
		ZBool IsActive { get; }
		ZBool IsNonSecurity { get; }
	}
}
