using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: RegimeDouanier
	///</summary>
	public interface IOrganisation
	{
		///<summary>
		/// Xml Tag: tin
		///</summary>
		ZString OrganisationNumber { get; }
		///<summary>
		/// Xml Tag: tin
		///</summary>
		ZString OrganisationNumberEoriOnly { get; }

		///<summary>
		/// Xml Tag: nomoperateur
		///</summary>
		ZString FullName { get; }

		///<summary>
		/// Xml Tag: rueoperateur
		///</summary>
		ZString Address { get; }

		///<summary>
		/// Xml Tag: paysoperateur
		///</summary>
		ZString CountryCode { get; }

		///<summary>
		/// Xml Tag: codepostaloperateur
		///</summary>
		ZString PostCode { get; }

		///<summary>
		/// Xml Tag: villeoperateur
		///</summary>
		ZString City { get; }

		#region Export

		///<summary>
		/// Xml Tag: villeoperateur
		///</summary>
		ZString PartnerConsigneeID { get; }

		///<summary>
		/// Xml Tag: idDestPartenaire
		///</summary>
		ZString PartnerDestIDInfo { get; }

		ZString EoriCode { get; }

		#endregion
	}
}
