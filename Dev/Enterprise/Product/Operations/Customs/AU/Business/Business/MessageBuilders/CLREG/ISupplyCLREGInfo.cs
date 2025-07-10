using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISupplyCLREGInfo
	{
		ZGuid OrganizationPK { get; }
		EDIMessageCollection Messages { get; }
		ZBool IsIndividual { get; }
		ZBool IsOrganisation { get; }
		ZString ABN { get; }
		ZString CAC { get; }
		ZString CACType { get; }
		ZString Title { get; }
		ZString FirstName { get; }
		ZString SecondName { get; }
		ZString FamilyName { get; }
		ZString Suffix { get; }
		ZString ContactName { get; }
		ZString ContactPurpose { get; }
		ZString BusinessName { get; }
		ZBool IsEvidenceOfID { get; }
		ZBool IsExDocsUser { get; }
		List<TravelDocument> TravelDocuments { get; }
		List<ZString> Rolls { get; }

		ZString BusinessAddress1 { get; }
		ZString BusinessAddress2 { get; }
		ZString BusinessAddressCity { get; }
		ZString BusinessAddressPostCode { get; }
		ZString BusinessAddressState { get; }
		ZString BusinessAddressCountry { get; }

		ZString PostalAddress1 { get; }
		ZString PostalAddress2 { get; }
		ZString PostalAddressCity { get; }
		ZString PostalAddressPostCode { get; }
		ZString PostalAddressState { get; }
		ZString PostalAddressCountry { get; }

		ZString ContactAddress1 { get; }
		ZString ContactAddress2 { get; }
		ZString ContactAddressCity { get; }
		ZString ContactAddressPostCode { get; }
		ZString ContactAddressState { get; }
		ZString ContactAddressCountry { get; }

		ZString ContactPostalAddress1 { get; }
		ZString ContactPostalAddress2 { get; }
		ZString ContactPostalAddressCity { get; }
		ZString ContactPostalAddressPostCode { get; }
		ZString ContactPostalAddressState { get; }
		ZString ContactPostalAddressCountry { get; }

		ZString ContactPhPrefix { get; }
		ZString ContactPh { get; }
		ZString ContactPhComment { get; }
		ZString ContactFaxPrefix { get; }
		ZString ContactFax { get; }
		ZString ContactFaxComment { get; }
		ZString ContactAHPrefix { get; }
		ZString ContactAH { get; }
		ZString ContactAHComment { get; }
		ZString ContactMobile { get; }
		ZString ContactMobileComment { get; }
		ZString ContactEmail { get; }
		ZDate DateofBirth { get; }
		ZString Gender { get; }
	}
}
