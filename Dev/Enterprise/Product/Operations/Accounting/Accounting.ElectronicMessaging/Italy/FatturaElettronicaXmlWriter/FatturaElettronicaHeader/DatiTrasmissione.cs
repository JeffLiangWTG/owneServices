using System;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class DatiTrasmissione
	{
		public XStreamingElement BuildXML(TransactionInfo transaction, ZString recipientOrgCategory)
		{
			var codiceDestinarioElement = BuildXmlForCodiceDestinatario(transaction, recipientOrgCategory);
			var recipientOrgCountry = transaction?.OrganizationAddress?.Country?.Code ?? ZString.Empty;
			var transmissionFormat = FatturaElettronicaDataHelper.GetTransmissionFormat(recipientOrgCategory, recipientOrgCountry);

			return new XStreamingElement("DatiTrasmissione",
				BuildXmlForIdTrasmittente(),
				new XElement("ProgressivoInvio", "PLACEHOLDER"), //EServices will fill this up - String10Type
				new XElement("FormatoTrasmissione", transmissionFormat),
				codiceDestinarioElement.Item1,
				codiceDestinarioElement.Item2 ? BuildXmlForPECDestinario(transaction) : null // TODO: This should be filled correctly once WI00206652 is completed
				);
		}

		XStreamingElement BuildXmlForIdTrasmittente()
		{
			return new XStreamingElement("IdTrasmittente",
				new XElement("IdPaese", CountryCodes.Italy),
				new XElement("IdCodice", "13149600150")
				);
		}

		Tuple<XElement, bool> BuildXmlForCodiceDestinatario(TransactionInfo transaction, ZString receipientOrgCategory)
		{
			var codiceDestinatario = ZString.Empty;
			var shouldCreatePECDestinatarioElement = false;
			var orgAddress = transaction.OrganizationAddress;
			var recipientOrgCountry = orgAddress?.Country?.Code ?? ZString.Empty;
			var cuuRegistrationNumber = FatturaElettronicaDataHelper.GetItalyRegistrationNumber(orgAddress?.RegistrationNumberCollection, ItalyOrgCusCodeInfo.OrgCusCodes.CUU)?.Value ?? ZString.Empty;
			const string defaultCodiceDestinatarioWhenRecipientCountryIsNotItaly = "XXXXXXX";
			const string defaultCodiceDestinatarioWhenRecipientCountryIsItaly = "0000000";
			const string defaultCodiceDestinatarioWhenRecipientCountryIsSanMarino = "2R4GTO8";

			if (FatturaElettronicaDataHelper.IsPayable(transaction))
			{
				codiceDestinatario = defaultCodiceDestinatarioWhenRecipientCountryIsItaly;
			}
			else if (recipientOrgCountry == CountryCodes.Italy)
			{
				if (receipientOrgCategory == OrgConstants.Category.NaturalPersonIndividual || cuuRegistrationNumber.IsEmpty) // 0000000 for no available CUU, or is NAT type
				{
					codiceDestinatario = defaultCodiceDestinatarioWhenRecipientCountryIsItaly;
				}
				else
				{
					codiceDestinatario = cuuRegistrationNumber;
				}

				if (codiceDestinatario == defaultCodiceDestinatarioWhenRecipientCountryIsItaly)
				{
					var registrationsNumbers = orgAddress?.RegistrationNumberCollection;
					if (registrationsNumbers != null && registrationsNumbers.Exists(x => x.Type.Code.HasValue && x.Type.Code.Value == ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail
																					&& x.CountryOfIssue.Code.HasValue && x.CountryOfIssue.Code.Value == CountryCodes.Italy))
					{
						shouldCreatePECDestinatarioElement = true;
					}
				}
			}
			else if (recipientOrgCountry == CountryCodes.SanMarino)
			{
				codiceDestinatario = defaultCodiceDestinatarioWhenRecipientCountryIsSanMarino; // 2R4GTO8 for S.Marino categories
			}
			else
			{
				codiceDestinatario = defaultCodiceDestinatarioWhenRecipientCountryIsNotItaly; // XXXXXXX for all non italian categories
			}

			return Tuple.Create(new XElement("CodiceDestinatario", codiceDestinatario), shouldCreatePECDestinatarioElement);
		}

		XElement BuildXmlForPECDestinario(TransactionInfo transaction)
		{
			var pecDesctinarioEmailAddress = FatturaElettronicaDataHelper.GetItalyRegistrationNumber(transaction.OrganizationAddress?.RegistrationNumberCollection, ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, transaction)?.Value ?? ZString.Empty;
			return new XElement("PECDestinatario", pecDesctinarioEmailAddress);
		}
	}
}
