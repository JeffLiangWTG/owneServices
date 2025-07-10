using System;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IBranch
	{
		string Code { get; }
		Guid CompanyPK { get; }
		string Name { get; }
		string NKUNLOCO { get; }
		string State { get; }
		Guid OrganisationPK { get; }
		string Phone { get; }
		Guid PK { get; }
		string HumanReadableNameForRegistry { get; }
		ICompany Company { get; }
		bool IsActive { get; }
	}
}
