using System;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IDepartment
	{
		string Code { get; }
		string Description { get; }
		Guid PK { get; }
	}
}
