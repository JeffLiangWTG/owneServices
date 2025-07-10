using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ZArchitecture.Modules
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not needed for this simple struct")]
	public struct OrganisationTabPageType
	{
		public OrganisationTabPageType(string name)
		{
			this.Name = name;
		}

		public readonly string Name;
	}

	public sealed class OrganisationTabPages
	{
		OrganisationTabPages()
		{
		}

		public static OrganisationTabPageType Details => new OrganisationTabPageType("DetailsTabPage");
		public static OrganisationTabPageType Address => new OrganisationTabPageType("AddressesTabPage");
		public static OrganisationTabPageType Contacts => new OrganisationTabPageType("ContactsTabPage");
		public static OrganisationTabPageType Sales => new OrganisationTabPageType("SalesTabPage");
		public static OrganisationTabPageType Sales_ClientRelationship => new OrganisationTabPageType("SalesTabPage+SalesClientRelTabPage");
		public static OrganisationTabPageType EDICodeMappings => new OrganisationTabPageType("DetailsTabPage+ConfigTabPage+EDICodeMappingTabPage");
		public static OrganisationTabPageType ServiceLevel => new OrganisationTabPageType("TransportTabPage+TransportTabPage1+ServiceLevelTabPage");
	}
}
