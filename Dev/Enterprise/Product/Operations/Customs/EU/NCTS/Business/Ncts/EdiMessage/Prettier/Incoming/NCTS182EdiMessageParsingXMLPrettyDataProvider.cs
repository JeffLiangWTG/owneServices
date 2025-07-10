using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using static System.FormattableString;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTS182EdiMessageParsingXMLPrettyDataProvider : EdiMessageParsingXMLPrettyDataProvider
	{
		public NCTS182EdiMessageParsingXMLPrettyDataProvider(EDIMessage message) : base(message)
		{
		}
		public ZString MRNVersion => SelectSingleNodeInnerText(MRNVersionXPath);
		public ZString TransitOperationIncidentNotificationDateAndTime => SelectSingleNodeInnerText(IncidentNotificationDateAndTimeXPath);
		public ZString CustomsOfficeOfIncidentRegistrationReferenceNumber => SelectSingleNodeInnerText(CustomsOfficeOfIncidentRegistrationXPath);
		public IReadOnlyCollection<INCTSIncidentData> Incidents => incidents ??= GetConsignmentIncidents();
		IReadOnlyCollection<INCTSIncidentData> incidents;

		protected virtual IReadOnlyCollection<INCTSIncidentData> GetConsignmentIncidents()
		{
			var list = new List<INCTSIncidentData>();
			var consignmentNodes = SelectNodes(ConsignmentXPath);
			foreach (var consignmentNode in consignmentNodes)
			{
				foreach (var incidentNode in SelectNodes(consignmentNode, ConsignmentIncidentNodeXPath))
				{
					var sequenceNumber = GetNodeInnerText(incidentNode, ConsignmentIncidentSequenceNumberNodeXPath);
					var code = GetNodeInnerText(incidentNode, ConsignmentIncidentCodeNodeXPath);
					var text = GetNodeInnerText(incidentNode, ConsignmentIncidentTextNodeXPath);

					var transhipmentData = SelectNodes(incidentNode, ConsignmentIncidentTranshipmentNodeXPath).Select(node =>
						new NCTSPrettierTranshipmentData(GetNodeInnerText(node, ConsignmentIncidentTranshipmentContainerIndicatorNodeXPath),
						GetNodeInnerText(node, ConsignmentIncidentTranshipmentTransportMeansNationalityNodeXPath),
						GetNodeInnerText(node, ConsignmentIncidentTranshipmentTransportMeansIdentificationNumberNodeXPath),
						GetNodeInnerText(node, ConsignmentIncidentTranshipmentTransportMeansTypeOfIdentificationNodeXPath))).FirstOrDefault();

					var locationAddressData = SelectNodes(incidentNode, ConsignmentIncidentLocationAddressNodeXPath).Select(node =>
						new NCTSPrettierIncidentLocationAddressData(
						GetNodeInnerText(node, ConsignmentIncidentLocationAddressStreetAndNumberNodeXPath),
						GetNodeInnerText(node, ConsignmentIncidentLocationAddressPostCodeNodeXPath),
						GetNodeInnerText(node, ConsignmentIncidentLocationAddressCityNodeXPath))).FirstOrDefault();

					var transportEquipments = GetIncidentTransportEquipments(SelectNodes(incidentNode, ConsignmentIncidentTransportEquipmentNodeXPath));

					var nctsIncidentData = new NCTSPrettierIncidentData(sequenceNumber, code, text, transhipmentData, locationAddressData, transportEquipments);

					var endorsmentNode = SelectNodes(incidentNode, ConsignmentIncidentEndorsementNodeXPath).FirstOrDefault();
					if (endorsmentNode != null)
					{
						nctsIncidentData.EndorsementDate = ZDateTime.TryParseExact(GetNodeInnerText(endorsmentNode, ConsignmentIncidentEndorsementDateNodeXPath), out var endorsementDate, "yyyy-MM-dd") ? endorsementDate.ToDateTime() : null;
						nctsIncidentData.EndorsementAuthority = GetNodeInnerText(endorsmentNode, ConsignmentIncidentEndorsementAuthorityNodeXPath);
						nctsIncidentData.EndorsementPlace = GetNodeInnerText(endorsmentNode, ConsignmentIncidentEndorsementPlaceNodeXPath);
						nctsIncidentData.EndorsementCountry = GetNodeInnerText(endorsmentNode, ConsignmentIncidentEndorsementCountryNodeXPath);
					}

					var locationNode = SelectNodes(incidentNode, ConsignmentIncidentLocationNodeXPath).FirstOrDefault();
					if (locationNode != null)
					{
						nctsIncidentData.LocationQualifierOfIdentification = GetNodeInnerText(locationNode, ConsignmentIncidentLocationQualifierOfIdentificationNodeXPath);
						nctsIncidentData.LocationUNLocode = GetNodeInnerText(locationNode, ConsignmentIncidentLocationUNLocodeNodeXPath);
						nctsIncidentData.LocationCountry = GetNodeInnerText(locationNode, ConsignmentIncidentLocationCountryNodeXPath);
					}

					var gnssNode = SelectNodes(incidentNode, ConsignmentIncidentLocationGNSSNodeXPath).FirstOrDefault();
					if (gnssNode != null)
					{
						nctsIncidentData.LocationLatitude = GetNodeInnerText(gnssNode, ConsignmentIncidentLocationGNSSLatitudeNodeXPath);
						nctsIncidentData.LocationLongitude = GetNodeInnerText(gnssNode, ConsignmentIncidentLocationGNSSLongitudeNodeXPath);
					}

					list.Add(nctsIncidentData);
				}
			}
			return list;
		}

		protected virtual IReadOnlyCollection<INCTSTransportEquipment> GetIncidentTransportEquipments(IEnumerable<XmlNode> nodes)
		{
			var list = new List<INCTSTransportEquipment>();
			foreach (var transportEquipmentNode in nodes)
			{
				var transportEquipmentSequenceNumber = GetNodeInnerText(transportEquipmentNode, ConsignmentIncidentTransportEquipmentSequenceNumberNodeXPath);
				var transportEquipmentcontainerIdentificationNumber = GetNodeInnerText(transportEquipmentNode, ConsignmentIncidentTransportEquipmentContainerIdentificationNumberNodeXPath);
				var transportEquipmentnumberOfSeals = GetNodeInnerText(transportEquipmentNode, ConsignmentIncidentTransportEquipmentNumberOfSealsNodeXPath);
				var transportEquipmentSeals = GetSeals(SelectNodes(transportEquipmentNode, ConsignmentIncidentTransportEquipmentSealsNodeXPath));
				var transportEquipmentGoodsReferences = GetGoodsReferences(SelectNodes(transportEquipmentNode, ConsignmentIncidentTransportEquipmentGoodsReferenceNodeXPath));
				list.Add(new NCTSPrettierIncidentTransportEquipmentData(transportEquipmentSequenceNumber, transportEquipmentcontainerIdentificationNumber, transportEquipmentnumberOfSeals,
					transportEquipmentSeals,
					transportEquipmentGoodsReferences));
			}
			return list;
		}

		protected virtual IReadOnlyCollection<INCTSGoodsReference> GetGoodsReferences(IEnumerable<XmlNode> nodes)
			=> nodes.Select(node => new NCTSPrettierGoodsReferenceData(
				GetNodeInnerText(node, ConsignmentIncidentTransportEquipmentGoodsReferenceSequenceNumberNodeXPath),
				GetNodeInnerText(node, ConsignmentIncidentTransportEquipmentGoodsReferenceDeclarationGoodsItemNumberNodeXPath))).ToList<INCTSGoodsReference>();

		protected virtual IReadOnlyCollection<INCTSSeal> GetSeals(IEnumerable<XmlNode> nodes)
			=> nodes.Select(node => new NCTSPrettierSealData(
				GetNodeInnerText(node, ConsignmentIncidentTransportEquipmentSealSequenceNumberNodeXPath),
				GetNodeInnerText(node, ConsignmentIncidentTransportEquipmentSealIdentifierNodeXPath))).ToList<INCTSSeal>();

		protected virtual string MRNVersionXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='MRNVersion']");
		protected virtual string IncidentNotificationDateAndTimeXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='incidentNotificationDateAndTime']");
		protected virtual string CustomsOfficeOfIncidentRegistrationXPath => Invariant($"//*[local-name()='CustomsOfficeOfIncidentRegistration']/*[local-name()='referenceNumber']");
		protected virtual string ConsignmentIncidentNodeXPath => Invariant($"*[local-name()='Incident']");
		protected virtual string ConsignmentIncidentSequenceNumberNodeXPath => Invariant($"*[local-name()='sequenceNumber']");
		protected virtual string ConsignmentIncidentCodeNodeXPath => Invariant($"*[local-name()='code']");
		protected virtual string ConsignmentIncidentTextNodeXPath => Invariant($"*[local-name()='text']");
		protected virtual string ConsignmentIncidentEndorsementNodeXPath => Invariant($"*[local-name()='Endorsement']");
		protected virtual string ConsignmentIncidentEndorsementDateNodeXPath => Invariant($"*[local-name()='date']");
		protected virtual string ConsignmentIncidentEndorsementAuthorityNodeXPath => Invariant($"*[local-name()='authority']");
		protected virtual string ConsignmentIncidentEndorsementPlaceNodeXPath => Invariant($"*[local-name()='place']");
		protected virtual string ConsignmentIncidentEndorsementCountryNodeXPath => Invariant($"*[local-name()='country']");
		protected virtual string ConsignmentIncidentLocationNodeXPath => Invariant($"*[local-name()='Location']");
		protected virtual string ConsignmentIncidentLocationQualifierOfIdentificationNodeXPath => Invariant($"*[local-name()='qualifierOfIdentification']");
		protected virtual string ConsignmentIncidentLocationUNLocodeNodeXPath => Invariant($"*[local-name()='UNLocode']");
		protected virtual string ConsignmentIncidentLocationCountryNodeXPath => Invariant($"*[local-name()='country']");
		protected virtual string ConsignmentIncidentLocationGNSSNodeXPath => Invariant($"*[local-name()='GNSS']");
		protected virtual string ConsignmentIncidentLocationGNSSLatitudeNodeXPath => Invariant($"*[local-name()='latitude']");
		protected virtual string ConsignmentIncidentLocationGNSSLongitudeNodeXPath => Invariant($"*[local-name()='longitude']");
		protected virtual string ConsignmentIncidentLocationAddressNodeXPath => Invariant($"*[local-name()='Address']");
		protected virtual string ConsignmentIncidentLocationAddressStreetAndNumberNodeXPath => Invariant($"*[local-name()='streetAndNumber']");
		protected virtual string ConsignmentIncidentLocationAddressPostCodeNodeXPath => Invariant($"*[local-name()='postcode']");
		protected virtual string ConsignmentIncidentLocationAddressCityNodeXPath => Invariant($"*[local-name()='city']");
		protected virtual string ConsignmentIncidentTransportEquipmentNodeXPath => Invariant($"*[local-name()='TransportEquipment']");
		protected virtual string ConsignmentIncidentTransportEquipmentSequenceNumberNodeXPath => Invariant($"*[local-name()='sequenceNumber']");
		protected virtual string ConsignmentIncidentTransportEquipmentContainerIdentificationNumberNodeXPath => Invariant($"*[local-name()='containerIdentificationNumber']");
		protected virtual string ConsignmentIncidentTransportEquipmentNumberOfSealsNodeXPath => Invariant($"*[local-name()='numberOfSeals']");
		protected virtual string ConsignmentIncidentTransportEquipmentSealsNodeXPath => Invariant($"*[local-name()='Seal']");
		protected virtual string ConsignmentIncidentTransportEquipmentGoodsReferenceNodeXPath => Invariant($"*[local-name()='GoodsReference']");
		protected virtual string ConsignmentIncidentTransportEquipmentGoodsReferenceSequenceNumberNodeXPath => Invariant($"*[local-name()='sequenceNumber']");
		protected virtual string ConsignmentIncidentTransportEquipmentGoodsReferenceDeclarationGoodsItemNumberNodeXPath => Invariant($"*[local-name()='declarationGoodsItemNumber']");
		protected virtual string ConsignmentIncidentTransportEquipmentSealSequenceNumberNodeXPath => Invariant($"*[local-name()='sequenceNumber']");
		protected virtual string ConsignmentIncidentTransportEquipmentSealIdentifierNodeXPath => Invariant($"*[local-name()='identifier']");
		protected virtual string ConsignmentIncidentTranshipmentNodeXPath => Invariant($"*[local-name()='Transhipment']");
		protected virtual string ConsignmentIncidentTranshipmentContainerIndicatorNodeXPath => Invariant($"*[local-name()='containerIndicator']");
		protected virtual string ConsignmentIncidentTranshipmentTransportMeansTypeOfIdentificationNodeXPath => Invariant($"//*[local-name()='TransportMeans']/*[local-name()='typeOfIdentification']");
		protected virtual string ConsignmentIncidentTranshipmentTransportMeansIdentificationNumberNodeXPath => Invariant($"//*[local-name()='TransportMeans']/*[local-name()='identificationNumber']");
		protected virtual string ConsignmentIncidentTranshipmentTransportMeansNationalityNodeXPath => Invariant($"//*[local-name()='TransportMeans']/*[local-name()='nationality']");
	}
}
