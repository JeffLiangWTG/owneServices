using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration;

public static partial class Customs
{
	public interface IConsignmentAddressProvider
	{
		/// <summary>
		/// Business object factory that created this consignment
		/// </summary>
		BusinessObjectFactory Factory { get; }

		string WaybillNumber { get; }

		/// <summary>
		/// Flag indicating whether consignee is linked to an organization
		/// </summary>
		bool ConsigneeIsOrganisation { get; }

		/// <summary>
		/// Flag indicating whether shipper is linked to an organization
		/// </summary>
		bool ShipperIsOrganisation { get; }

		/// <summary>
		/// FK to organization address for consignee
		/// </summary>
		ZGuid ConsigneeAddressId { get; set; }

		/// <summary>
		/// FK to organization address for shipper
		/// </summary>
		ZGuid ShipperAddressId { get; set; }

		// Free text consignee properties
		string ConsigneeName { get; }
		string ConsigneeAddress1 { get; }
		string ConsigneeAddress2 { get; }
		string ConsigneeCity { get; }
		string ConsigneeState { get; }
		string ConsigneePostcode { get; }
		string ConsigneeCountryCode { get; }
		string ConsigneePhone { get; }
		string ConsigneeMobile { get; }
		string ConsigneeFax { get; }
		string ConsigneeEmail { get; }

		// Free text shipper properties
		string ShipperName { get; }
		string ShipperAddress1 { get; }
		string ShipperAddress2 { get; }
		string ShipperCity { get; }
		string ShipperState { get; }
		string ShipperPostcode { get; }
		string ShipperCountryCode { get; }
		string ShipperPhone { get; }
		string ShipperMobile { get; }
		string ShipperFax { get; }
		string ShipperEmail { get; }

		/// <summary>
		/// Notifies the consignment that binding should be refreshed
		/// </summary>
		void RefreshBinding();
	}

	/// <summary>
	/// Types of addresses that can be associated with a consignment
	/// </summary>
	public enum ConsignmentAddressType
	{
		Consignee,
		Shipper,
		Return
	}
}
