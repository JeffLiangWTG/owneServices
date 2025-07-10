using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore
{
	public interface IDocumentDeliveryContact
	{
		IOrgHeader OrgHeader { get; }
		IOrgContact OrgContact { get; }

		/// <summary>
		/// Autodelivery requires a related organisation, such that if the contact type is Consignee, then you
		/// need to also provide the Consignor as related organisation, vise versa.
		/// </summary>
		IOrgHeader RelatedOrgHeader { get; }

		IOrgAddress OrgAddress { get; }
	}
}
