using System;
using System.Globalization;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class ECSResponseMessageProcessor : BaseDeltaGResponseMessageProcessor
	{
		public ECSResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}
		protected override ZString MessageType => MessageTypeList.Codes.ECS;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is FREDIMessage frEDIMessage)
			{
				var emMessageXML = new XmlDocument();
				emMessageXML.LoadXml(frEDIMessage.EM_MessageText);
				var mrnecsNode = emMessageXML.SelectSingleNode("Message/ReponseDeclaration/Entete/mrnecs");

				var mrn = mrnecsNode?.InnerText;
				if (mrn != null)
				{
					var cusExitDetail = frEDIMessage.Factory.LoadTop1<CusExitDetail>(new ZQuery(CusExitDetailSchema.CED_MovementReferenceNumber, mrn));

					if (cusExitDetail != null)
					{
						var etatNode = emMessageXML.SelectSingleNode("Message/ReponseDeclaration/ReponseDatas/Notification/Etat") ?? emMessageXML.SelectSingleNode("Message/ReponseDeclaration/ReponseEtat");
						var etatDescription = etatNode?.SelectSingleNode("etat")?.InnerText;
						if (!string.IsNullOrEmpty(etatDescription))
						{
							var list = new CusExitDetailList();
							cusExitDetail.CED_Status = list.GetCodeFromDescription(etatDescription);

							if (cusExitDetail.CED_Status == CusExitDetailList.Codes.EXT)
							{
								var etatDate = etatNode?.SelectSingleNode("etatDate")?.InnerText;
								if (DateTime.TryParse(etatDate, out var dateTime))
								{
									cusExitDetail.CED_ExitDate = ((ZDateTime)dateTime).Date;
									cusExitDetail.CED_IsFinalized = true;
								}
							}
						}
						cusExitDetail.Messages.Add(message);
						message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
					}
					else
					{
						Logger.Log(string.Format(CultureInfo.InvariantCulture, "FR Customs ECS message {0}(mrnecs = {1}) cannot find matched CusExitDetail.", message.EM_ApplicationReference, mrn), Integration.LogType.Error);
						message.EM_Status = EDIMessageStatusList.Codes.Error;
					}
				}
			}
		}
	}
}
