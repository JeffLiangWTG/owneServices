namespace Enterprise.ComplianceRisk.Integration
{
	public interface INaturalPerson
	{
		string Name { get; }
		string Address1 { get; }
		string Address2 { get; }
		string City { get; }
		string State { get; }
		string PostCode { get; }
		string Country { get; }
		string AdditionalAddressLine { get; }
		bool IsConsideredAsOrganization { get; }
	}
}
