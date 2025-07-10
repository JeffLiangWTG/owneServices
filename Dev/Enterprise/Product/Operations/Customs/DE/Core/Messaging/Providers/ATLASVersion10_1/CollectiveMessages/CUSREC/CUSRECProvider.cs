using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSRECProvider : ICUSREC
	{
		public CUSRECProvider(GCRECF message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly GCRECF message;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public ZString ReferencedMessageIdentifier => message.Header?.ReferencedMessageIdentifier;

		public string ReferenceNumber => message.Header?.ReferenceNumber;

		public string MRN => message.Header?.MRN;

		public ZString LocalReferenceNumber => message.Header?.LRN ?? ZString.Empty;

		public ZDate RegistrationDate => (message.Header?.RegistrationDateSpecified ?? false) ? new ZDate(message.Header.RegistrationDate.Date) : ZDate.Empty;

		public IReadOnlyCollection<ZString> NotificationSeverity
		{
			get
			{
				if (notificationSeverity == null)
				{
					var notification = message.Notification;
					if (notification != null)
					{
						notificationSeverity = notification.Select(x => (ZString)x.Severity).ToArray();
					}
					else
					{
						notificationSeverity = Array.Empty<ZString>();
					}
				}
				return notificationSeverity;
			}
		}
		IReadOnlyCollection<ZString> notificationSeverity;

		public IReadOnlyCollection<ICUSRECGoodsItem> GoodsItems => goodsItems ??= message.Body?.Select(x => new CUSRECGoodsItemProvider(x)).ToArray() ?? Array.Empty<ICUSRECGoodsItem>();

		IReadOnlyCollection<ICUSRECGoodsItem> goodsItems;
	}
}
