namespace CargoWise.Integration
{
	public interface IDomainCredentials
	{
		string DomainName { get; set; }
		string DomainUserName { get; set; }
		string DomainUserPassword { get; set; }
		string UserOrganisationalUnit { get; set; }
		string GroupOrganisationalUnit { get; set; }
		string DefaultPassword { get; set; }
		bool IsDefaultDomain { get; set; }
		bool DefaultPasswordFailsToMeetDomainPolicy { get; set; }
	}
}
