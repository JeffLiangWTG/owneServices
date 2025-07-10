using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class OrgEDITransmissionConfigurationXmlMappings : EnterpriseCodeExternalCodeMappings
	{
		protected OrgEDITransmissionConfigurationXmlMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.EMA));
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.EMT));
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.FIL));
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.FTP, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.FTP));
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.HUB));
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.EDP));
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.XTT));
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.UDB));
			yield return new Mapping(EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector, nameof(Xsd.OrganisationDetailEDITransmissionDetailsType.NXC));
		}

		public static readonly OrgEDITransmissionConfigurationXmlMappings Instance = new OrgEDITransmissionConfigurationXmlMappings();

		public new Xsd.OrganisationDetailEDITransmissionDetailsType GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, (Xsd.OrganisationDetailEDITransmissionDetailsType)Xsd.RegistrationNumberTypes.GST, errorContext, notifications);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Transmission Configuration"; }
		}
	}
}
