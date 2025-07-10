using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTS182ResponsePrettyFormatter : NCTS182EdiMessageXmlPrettier
	{
		public NCTS182ResponsePrettyFormatter(EDIMessage message) : base(message)
		{
		}

		readonly IncidentCodeList incidentCodeList = new IncidentCodeList();
		readonly NctsTransportTypeOfIdList transportTypeOfIdList = new NctsTransportTypeOfIdList();

		protected override string MakeInboundPrettyForPhase5Interpretation()
		{
			var htmlBuilder = new ZStringBuilder();

			htmlBuilder.Append($"<h2>{Title}</h2>");

			var mainDataTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(mainDataTable, MrnCaption, $"{dataProvider.MRN}.{dataProvider.MRNVersion}");
			WriteRow(mainDataTable, DateCaption, GetDateFormat($"{dataProvider.TransitOperationIncidentNotificationDateAndTime}"));

			var departureCustomsOffice = GetCustomsOfficeDescription(dataProvider.CustomsOfficeOfDeparture);
			var incidentCustomsOffice = GetCustomsOfficeDescription(dataProvider.CustomsOfficeOfIncidentRegistrationReferenceNumber);
			WriteRow(mainDataTable, CustomsOfficeDepartureCaption, $"{dataProvider.CustomsOfficeOfDeparture} {departureCustomsOffice}");
			WriteRow(mainDataTable, CustomsOfficeIncidentCaption, $"{dataProvider.CustomsOfficeOfIncidentRegistrationReferenceNumber} {incidentCustomsOffice}");
			htmlBuilder.Append(mainDataTable.ToHtml());

			foreach (var incident in dataProvider.Incidents)
			{
				AppendIncident(htmlBuilder, incident);
			}

			return htmlBuilder.ToString();
		}

		protected virtual string GetDateFormat(ZString dateInput)
		{
			var dateFormateed = ZDateTime.TryParseExact(dateInput, out var result, (NoResString)"dd-MM-yyyy HH:mm:ss");
			return dateFormateed ? result.ToString() : dateInput;
		}

		void AppendIncident(ZStringBuilder htmlBuilder, INCTSIncidentData incident)
		{
			htmlBuilder.Append($"<h3>{IncidentSubTitle}</h3>");
			var incidentTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(incidentTable, NumberCaption, incident.SequenceNumber.ToString(CultureInfo.InvariantCulture));

			var incidentCode = incident.Code.ToString(CultureInfo.InvariantCulture);
			var incidentCodeDescription = incidentCodeList.GetDescriptionFromCode(incidentCode);
			WriteRow(incidentTable, CodeCaption, incidentCodeDescription ?? incidentCode);

			WriteRow(incidentTable, TextCaption, incident.Text);
			htmlBuilder.Append(incidentTable.ToHtml());

			htmlBuilder.Append($"<h4>{EndorsementSubTitle}</h4>");
			var endorsementTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(endorsementTable, AuthorityCaption, incident.EndorsementAuthority);
			WriteRow(endorsementTable, CountryCaption, incident.EndorsementCountry);
			WriteRow(endorsementTable, DateCaption, $"{incident.EndorsementDate:dd-MM-yyyy}");
			WriteRow(endorsementTable, PlaceCaption, incident.EndorsementPlace);
			htmlBuilder.Append(endorsementTable.ToHtml());

			htmlBuilder.Append($"<h4>{LocationSubTitle}</h4>");
			var locationTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(locationTable, QualifierCaption, incident.LocationQualifierOfIdentification);
			WriteRow(locationTable, UNLocoCaption, incident.LocationUNLocode);
			WriteRow(locationTable, LatitudeCaption, incident.LocationLatitude);
			WriteRow(locationTable, LongitudeCaption, incident.LocationLongitude);

			if (incident.LocationAddress != null)
			{
				var address = incident.LocationAddress;
				var formattedAddress = $"{address.StreetAndNumber} {address.Postcode} {address.City}";
				locationTable.WriteRow(AddressCaption, formattedAddress);
			}
			htmlBuilder.Append(locationTable.ToHtml());

			htmlBuilder.Append($"<h4>{TranshipmentSubTitle}</h4>");
			var transhipmentTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(transhipmentTable, ContainerCaption, incident.Transhipment?.ContainerIndicator ?? false ? Res.GetString("C141E920-C5F4-4B86-B438-F7AF31468229", "Yes") : Res.GetString("80986362-2462-446A-A89E-4F6E110E8B21", "No"));

			var transhipmentTransportMeansTypeOfIdentification = incident.Transhipment?.TransportMeansTypeOfIdentification;
			var formattedTransportMeans = $"{incident.Transhipment?.TransportMeansNationality} {incident.Transhipment?.TransportMeansIdentificationNumber} {transportTypeOfIdList.GetDescriptionFromCode(transhipmentTransportMeansTypeOfIdentification) ?? transhipmentTransportMeansTypeOfIdentification}";
			WriteRow(transhipmentTable, TransportMeans, formattedTransportMeans);
			htmlBuilder.Append(transhipmentTable.ToHtml());

			if (incident.TransportEquipments.Any())
			{
				htmlBuilder.Append($"<h4>{TransportEquipmentSubTitle}</h4>");
				var transportEquipmentTable = new HtmlTableCreator(new NameValueCollection());
				foreach (var transportEquipment in incident.TransportEquipments)
				{
					WriteRow(transportEquipmentTable, NumberCaption, transportEquipment.SequenceNumber.ToString(CultureInfo.InvariantCulture));
					WriteRow(transportEquipmentTable, ContainerCaption, transportEquipment.ContainerIdentificationNumber);
					WriteRow(transportEquipmentTable, NumberOfSealsCaption, transportEquipment.NumberOfSeals.HasValue ? transportEquipment.NumberOfSeals.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);

					if (transportEquipment.Seals?.Count > 0)
					{
						var formattedSeal = string.Join(", ", transportEquipment.Seals.Select(x => x.Identifier));
						WriteRow(transportEquipmentTable, SealsCaption, formattedSeal);
					}

					if (transportEquipment.GoodsReferences?.Count > 0)
					{
						var formattedGoodsReference = string.Join(", ", transportEquipment.GoodsReferences.Select(x => x.DeclarationGoodsItemNumber));
						WriteRow(transportEquipmentTable, GoodsReferencesCaption, formattedGoodsReference);
					}
				}
				htmlBuilder.Append(transportEquipmentTable.ToHtml());
			}
		}

		void WriteRow(HtmlTableCreator table, string caption, string value)
		{
			table.WriteRow(caption, WebUtility.HtmlEncode(value));
		}

		string GetCustomsOfficeDescription(ZString customsOfficeCode)
		{
			var dataGroupingCode = customsOfficeCode.SubstringSafe(0, 2);
			if (!dataGroupingCode.IsEmpty)
			{
				return Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(message.Factory, customsOfficeCode, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now)?.ZZD_Description ?? ZString.Empty;
			}
			return ZString.Empty;
		}

		#region Titles and Captions

		protected virtual string Title => Res.GetString("52342B94-435A-489F-AB63-6448FCFF0A12", "Events during the journey");

		protected virtual string IncidentSubTitle => Res.GetString("303C53F6-A8E0-430C-81E5-2E4B2336E4C2", "Incident");
		protected virtual string EndorsementSubTitle => Res.GetString("F8A5BCCF-F5F7-4816-AFB1-CC4FDC88BFC5", "Endorsement");
		protected virtual string LocationSubTitle => Res.GetString("807AC3B2-852D-4444-B8DA-9DA67E17D84B", "Location");
		protected virtual string TranshipmentSubTitle => Res.GetString("3670253F-BC31-40F5-8488-CA9B9782D13B", "Transhipment");
		protected virtual string TransportEquipmentSubTitle => Res.GetString("0F855458-9DA1-4915-A476-617C1CBAF2FB", "Transport Equipment");

		protected virtual string MrnCaption => Res.GetString("EED0C3DE-6EF9-4546-8011-47E9F33202EB", "MRN:");
		protected virtual string DateCaption => Res.GetString("2E54039F-230B-41AC-AEBF-F7A66489ED2E", "Date:");

		protected virtual string CustomsOfficeDepartureCaption => Res.GetString("B3EF61EF-44A3-4B9A-8D73-B0B34D10A258", "Customs Office Departure:");
		protected virtual string CustomsOfficeIncidentCaption => Res.GetString("E941E665-6B9D-49BC-9DF0-6F288F92A609", "Customs Office Incident:");

		protected virtual string NumberCaption => Res.GetString("7B966E7C-F4B8-4583-9FE7-3DC22B7DAE60", "Number:");
		protected virtual string CodeCaption => Res.GetString("0C42095A-014C-4DA3-A6C0-C11E8ED8BFE0", "Code:");
		protected virtual string TextCaption => Res.GetString("D8567267-E49F-4047-A5E4-B17D02FC64D8", "Text:");
		protected virtual string AuthorityCaption => Res.GetString("D972EB94-41F1-42B5-B30E-A478216B9CB3", "Authority:");
		protected virtual string CountryCaption => Res.GetString("803DC1E1-17FB-4273-8DEC-AC2FD3025D12", "Country:");
		protected virtual string PlaceCaption => Res.GetString("0340A828-9761-4A09-BDE2-2EFD840CBE5A", "Place:");
		protected virtual string QualifierCaption => Res.GetString("848F35BC-1CAB-49A6-AA6C-1F1A6B3CDD52", "Qualifier:");
		protected virtual string UNLocoCaption => Res.GetString("280BF969-7ED5-46CB-9B95-F2062FA80C5F", "UNLOCODE:");
		protected virtual string LatitudeCaption => Res.GetString("ABA974B6-1864-4964-B152-DEE986BD02AF", "GNNS Latitude:");
		protected virtual string LongitudeCaption => Res.GetString("D887E54F-705A-43F2-8CDD-76A93106A8D8", "GNNS Longitude:");
		protected virtual string AddressCaption => Res.GetString("58119080-C85E-4CB3-9831-64C5C824299D", "Address:");
		protected virtual string ContainerCaption => Res.GetString("D5D25878-FCEF-49CF-893C-AD66868DC16D", "Container:");
		protected virtual string TransportMeans => Res.GetString("04312AC1-CD9D-4F59-99F0-F42EA129BB8A", "Transport Means:");
		protected virtual string NumberOfSealsCaption => Res.GetString("10A1A4B9-DC0D-42EA-B22D-FA821901DF43", "Number of Seals:");
		protected virtual string SealsCaption => Res.GetString("DDDB3806-B51D-43A7-AE35-0BD67EA20EE0", "Seals:");
		protected virtual string GoodsReferencesCaption => Res.GetString("5624E28F-E02E-4B02-9547-A719345FBDA2", "Goods References:");

		#endregion
	}
}
