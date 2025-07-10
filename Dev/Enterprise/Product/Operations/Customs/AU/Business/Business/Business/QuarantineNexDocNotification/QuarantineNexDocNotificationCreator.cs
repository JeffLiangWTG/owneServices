using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineNexDocNotificationCreator
	{
		IEnumerable<NotificationCreator> Creators { get; }

		public QuarantineNexDocNotificationCreator(BusinessObjectFactory factory)
		{
			Creators = new List<NotificationCreator> { new NotificationCreatorFA(factory), new NotificationCreatorTA(factory) };
		}

		public QuarantineNexDocNotification Create(ZString type, ZString title, ZString text)
		{
			return Creators.FirstOrDefault(creator => creator.NotificationType == type)?.Create(type, title, text);
		}

		abstract class NotificationCreator
		{
			BusinessObjectFactory Factory { get; }

			public abstract ZString NotificationType { get; }

			public NotificationCreator(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			public QuarantineNexDocNotification Create(ZString type, ZString title, ZString text)
			{
				QuarantineNexDocNotification result = null;
				if (NotificationType == type)
				{
					result = Factory.New<QuarantineNexDocNotification>();
					result.QN_NotificationType = type;
					result.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.NotActioned;
					result.QN_SystemCreateTimeUtc = ZDateTime.UtcNow;
					Initialize(result, title, text);
				}
				return result;
			}

			protected abstract void Initialize(QuarantineNexDocNotification bizObj, ZString title, ZString text);
		}

		class NotificationCreatorFA : NotificationCreator
		{
			public NotificationCreatorFA(BusinessObjectFactory factory) : base(factory) { }

			public override ZString NotificationType => NEXDOCNotificationType.Codes.ForwardAcceptanceRequired;

			protected override void Initialize(QuarantineNexDocNotification bizObj, ZString title, ZString text)
			{
				bizObj.QN_RexNumber = Regex.Match(title, @"(?<=Forward Notification requires approval: )REX\d+")?.Value ?? ZString.Empty;
				bizObj.QN_ExporterReference = Regex.Match(title, @"(?<=\(Exporter Reference: )\w+")?.Value ?? ZString.Empty;
				bizObj.QN_ForwardingGroupID = Regex.Match(text, @"(?<=The forward was sent by )\w+(?=\.?$)")?.Value ?? ZString.Empty;
			}
		}

		class NotificationCreatorTA : NotificationCreator
		{
			public NotificationCreatorTA(BusinessObjectFactory factory) : base(factory) { }

			public override ZString NotificationType => NEXDOCNotificationType.Codes.TransferAcceptanceRequired;

			protected override void Initialize(QuarantineNexDocNotification bizObj, ZString title, ZString text)
			{
				bizObj.QN_RexNumber = Regex.Match(title, @"(?<=Transfer Notification requires approval: )REX\d+")?.Value ?? ZString.Empty;
				bizObj.QN_ExporterReference = Regex.Match(title, @"(?<=\(Exporter Reference: )\w+")?.Value ?? ZString.Empty;
				bizObj.QN_ForwardingGroupID = Regex.Match(text, @"(?<=The transfer was sent by )\w+")?.Value ?? ZString.Empty;
				bizObj.QN_TransferringExporterID = Regex.Match(text, @"(?<=The transfer was sent by \w+ and exporter )\w+(?=\.?$)")?.Value ?? ZString.Empty;
				bizObj.QN_ReceivingExporterID = Regex.Match(text, @"(?<=The new exporter is )\w+(?=\. The transfer was sent by \w+)")?.Value ?? ZString.Empty;
			}
		}
	}
}
