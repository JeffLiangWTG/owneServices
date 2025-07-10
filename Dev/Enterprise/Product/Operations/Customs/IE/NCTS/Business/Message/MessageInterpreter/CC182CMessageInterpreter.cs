using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC182CMessageInterpreter : InboundMessageInterpreter<CC182CProvider>
	{
		public CC182CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC182CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"CAA0CA1E-1569-4A2D-9AED-7694A92D2E29",
			"A Forwarded Incident Notification Message (IE182) has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("DA6245EE-2654-4EA0-8163-1D0AAFC10102", "Incident Notification Date & Time"), provider.IncidentNotificationDateAndTime.ToLongTimeString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var incident in provider.Incidents)
			{
				yield return (string.Empty, GetIncident(incident));
			}
		}

		IEnumerable<(string key, string value)> GetIncident(CC182CIncidentProvider incident)
		{
			var result = new List<(string key, string value)>
			{
				(Res.GetString("DBCB9EA9-0B71-4D17-AEAB-D7860EFE1775", "Incident Code"), incident.IncidentCode),
				(Res.GetString("64414710-7F31-4B65-8F9A-BC50E7ABC036", "Incident Text"), incident.IncidentText),
				(Res.GetString("3B103EC5-7051-4181-8879-606A4688DB5E", "Endorsement Date"), incident.EndorsementDate.ToShortDateString()),
				(Res.GetString("B1341E85-9FA4-49A5-9E8C-4B12D5A1DCB9", "Endorsement Authority"), incident.EndorsementAuthority),
				(Res.GetString("30D8FBF6-E04A-4040-9005-F258227ADB67", "Endorsement Place"), incident.EndorsementPlace),
				(Res.GetString("B384DEDE-BDB1-406B-B15B-A980BBB7884C", "Endorsement Country"), incident.EndorsementCountry),
			};

			foreach (var transportEquipment in incident.TransportEquipment)
			{
				result.AddRange(new (string key, string value)[]
				{
					(Res.GetString("47C42ECA-D5A2-413D-B098-DDAFAC712577", "Container Number"), transportEquipment.ContainerNumber),
					(OneDash + Res.GetString("7A7C2795-96EB-458E-963C-01474A4AE2EB", "Number of Seals"), transportEquipment.NumberofSeals),
				});

				foreach (var identifier in transportEquipment.SealsIdentifier)
				{
					result.AddRange(new (string key, string value)[]
					{
						(OneDash + Res.GetString("C3CADC7B-4AC3-4BBC-A37F-09DD350AEE44", "Seals Identifier"), identifier),
					});
				}
			}

			return result;
		}

		const string OneDash = "-";
	}
}
