using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Response.Import;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaCImportResponsePrettier : FREDIMessagePrettier<TMessage>
	{
		public DeltaCImportResponsePrettier(DeltaCImportResponseMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		public new DeltaCImportResponseMessageDataObject MessageDataObject => (DeltaCImportResponseMessageDataObject)base.MessageDataObject;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
		public override ZString GetMessageInterpretation()
		{
			var result = ZString.Empty;

			if (ResponseMessage != null)
			{
				var responseDeclaration = ResponseMessage.ReponseDeclaration;
				var etat = responseDeclaration?.ReponseDatas?.Notification?.Etat;
				var entete = responseDeclaration?.Entete;
				result = ToH1IfNotEmpty("Delta response")
					+ "<br />"
					+ ToH3IfNotEmpty("TransactionID: ", ResponseMessage.EnveloppeMessage?.TransactionId)
					+ ToH3IfNotEmpty("Entry Status : ", etat?.Etat)
					+ ToH3IfNotEmpty("Entry Status date: ", etat?.EtatDate)
					+ ToH3IfNotEmpty("Entry Status hour: ", etat?.EtatHeure)
					+ ToH3IfNotEmpty("Entry Evenement tag: ", etat?.Evenement)
					+ ToH3IfNotEmpty("Entry Reference: ", entete?.Refdos)
					+ ToH3IfNotEmpty("Delta Reference: ", entete?.Refdec)
					+ GetRequestStatus()
					+ GetAlerts()
					+ "<br />"
					+ GetErrorsTableIfNeeded();
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		string GetRequestStatus()
		{
			var result = ZString.Empty;
			var reponseToRequestNode = ResponseMessage?.ReponseDeclaration?.ReponseDatas?.Notification?.Etat?.ReponseDemande;
			if (reponseToRequestNode != null)
			{
				var motivation = reponseToRequestNode.Motivation;
				result = MakeRequestStatusPrettier(reponseToRequestNode.Typedemande ?? ZString.Empty,
					ZString.Empty,
					ZString.Empty,
					motivation?.Motiv ?? ZString.Empty,
					motivation?.Justifreg ?? ZString.Empty,
					motivation?.Commentaire ?? ZString.Empty,
					motivation?.Nouvelledest ?? ZString.Empty,
					reponseToRequestNode.Motivservice ?? ZString.Empty,
					reponseToRequestNode.Numdemande ?? ZString.Empty,
					reponseToRequestNode.Nomagent ?? ZString.Empty,
					reponseToRequestNode.Bureauagent ?? ZString.Empty);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
		string GetAlerts()
		{
			var result = ZString.Empty;
			var alertNode = ResponseMessage?.ReponseDeclaration?.ReponseDatas?.Alerte;
			if (alertNode != null && alertNode.Any())
			{
				result = ToH3IfNotEmpty($"Alerts:");
				result += "<ul>";
				foreach (var goodsItemAlert in alertNode)
				{
					foreach (var documentAlert in goodsItemAlert.AlertesDocs)
					{
						result += ToH4IfNotEmpty($"<li>{documentAlert.AlerteDescription ?? ZString.Empty}");
					}
				}
				result += "</ul>";
			}
			return result;
		}

		string GetErrorsTableIfNeeded()
		{
			var result = ZString.Empty;
			var erreurGens = ResponseMessage?.ReponseDeclaration?.ReponseDatas?.Erreur?.ErreurGen;
			if (erreurGens != null && erreurGens.Any())
			{
				var tableCreator = new HtmlTableCreator(TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };
				tableCreator.WriteRow((NoResString)"Error Code", (NoResString)"Error Description");

				foreach (var responseError in erreurGens)
				{
					tableCreator.WriteRow(responseError.ErreurCode, responseError.ErreurDescription);
				}

				result = tableCreator.ToHtml();
			}

			return result;
		}
	}
}
