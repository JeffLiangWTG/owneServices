using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC182C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC182CMessagePrettier : NCTSMessagePrettier<Cc182CType>
	{
		public CC182CMessagePrettier(NCTSMessageDataObject<Cc182CType> messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString", Justification = "Html strings")]
		protected override ZString GetMessageInterpretationCore(Cc182CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			var consignment = messageObject.Consignment;

			var rows = new List<(ZString key, ZString value)>
			{
				("Status", TP5ResponseMessageSubTypeList.Descriptions.ForwardedIncidentNotificationToEd ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				("Incident Notification Date", transitOperation?.IncidentNotificationDateAndTime.ToString() ?? ZString.Empty),
				("Customs Office of Departure", messageObject.CustomsOfficeOfDeparture?.ReferenceNumber ?? ZString.Empty),
				("Customs Office of Incident Registration", messageObject.CustomsOfficeOfIncidentRegistration?.ReferenceNumber ?? ZString.Empty)
			};

			if (consignment != null)
			{
				foreach (var incident in consignment)
				{
					var endorsement = incident.Endorsement;
					var location = incident.Location;
					var equipments = incident.TransportEquipment;

					var incidentCode = incident.Code ?? ZString.Empty;
					var incidentCodeDescription = GetEUNCodeDescription(incidentCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL019);

					rows.Add((ZString.Empty, "\n"));
					rows.Add(("Incident", incident.SequenceNumber ?? ZString.Empty));
					rows.Add(("  Text", incident.Text ?? ZString.Empty));
					rows.Add(("  Code", incidentCode.IsEmpty() ? ZString.Empty : $"{incidentCode} - {incidentCodeDescription}"));

					if (endorsement != null)
					{
						rows.Add(("  Endorsement", ZString.Empty));
						rows.Add(("    Date", endorsement.Date.ToString() ?? ZString.Empty));
						rows.Add(("    Authority", endorsement.Authority ?? ZString.Empty));
						rows.Add(("    Place", endorsement.Place ?? ZString.Empty));
						rows.Add(("    Country", endorsement.Country ?? ZString.Empty));
					}

					if (location != null)
					{
						rows.Add(("  Location", ZString.Empty));
						if (location.UnLocode != null)
						{
							rows.Add(("    UNLocode", location.UnLocode ?? ZString.Empty));
						}
						else if (location.Address != null)
						{
							var address = location.Address;
							rows.Add(("    Street", address.StreetAndNumber ?? ZString.Empty));
							rows.Add(("    City", address.City ?? ZString.Empty));
							rows.Add(("    Post Code", address.Postcode ?? ZString.Empty));
						}
						else if (location.Gnss != null)
						{
							var gnss = location.Gnss;
							rows.Add(("    Longitude", gnss.Longitude ?? ZString.Empty));
							rows.Add(("    Latitude", gnss.Latitude ?? ZString.Empty));
						}
					}

					if (equipments != null && equipments.Count > 0)
					{
						rows.Add(("  Equipment", ZString.Empty));
						foreach (var equipment in equipments)
						{
							var identificationNumber = equipment.ContainerIdentificationNumber ?? ZString.Empty;
							var numberOfSeals = equipment.NumberOfSeals ?? ZString.Empty;
							var seals = equipment.Seal;

							rows.Add(("    Container", $"{identificationNumber} - {numberOfSeals}"));
							if (seals != null && seals.Count > 0)
							{
								foreach (var seal in seals)
								{
									rows.Add(("      Seal #", seal.Identifier ?? ZString.Empty));
								}
							}
						}
					}
				}
			}
			return $"<pre>{ToKeyValuePairSection(rows, true)}</pre>";
		}

		ZString GetEUNCodeDescription(string code, string codeType)
		{
			return string.IsNullOrEmpty(code) ? ZString.Empty : (ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, code, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty);
		}
	}
}
