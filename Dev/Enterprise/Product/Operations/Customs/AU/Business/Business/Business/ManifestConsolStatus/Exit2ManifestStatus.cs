using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class Exit2ManifestStatus : AUCustomsManifestStatus
	{
		public Exit2ManifestStatus(ForwardingConsol consol)
			: base(consol)
		{
		}

		protected override string MessageApplicationCode
		{
			get { return "EX2"; }
		}

		public override ZString E2_CustomsEntryNumber
		{
			get
			{
				if (OrderedMessages.Length > 0)
				{
					var result = EmptyCustomsEntryNumberString;
					//TODO: Implement Sort Order
					//OrderedMessages.Sort(EDIMessage.Schema.EM_DateTimeMessageSent, System.ComponentModel.ListSortDirection.Descending);
					foreach (var message in OrderedMessages)
					{
						var messageSegments = ConvertDBMessageToUNOA(message.EM_MessageText);
						foreach (var segment in messageSegments)
						{
							if (segment.StartsWith("BGM"))
							{
								var elements = segment.Split('+');
								if (elements.Length > 5)
								{
									result = elements[5];
									return result;
								}
							}
						}
					}
					return result;
				}
				return ZString.Empty;
			}
		}

		public override ZString E2_CustomsEntryNumberHumanReadableName
		{
			get { return "CRN"; }
		}

		protected override ZDateTime GetMessageSentTime(EDIMessage message)
		{
			var result = ZDateTime.Empty;
			var messageSegments = ConvertDBMessageToUNOA(message.EM_MessageText);
			foreach (var segment in messageSegments)
			{
				if (segment.StartsWith("ERP"))
				{
					var elements = segment.Split('+');
					if (elements.Length > 2)
					{
						if (elements[2] == "421" || elements[2] == "410")
						{
							try
							{
								var dateCoded = elements[1].Split(':')[1];
								var year = int.Parse(dateCoded.Substring(0, 2));
								var month = int.Parse(dateCoded.Substring(2, 2));
								var day = int.Parse(dateCoded.Substring(4, 2));
								var hour = int.Parse(dateCoded.Substring(6, 2));
								var minute = int.Parse(dateCoded.Substring(8, 2));
								var second = int.Parse(dateCoded.Substring(10, 2));
								result = new ZDateTime(new DateTime(year, month, day, hour, minute, second));
								return result;
							}
							catch (FormatException)
							{
								result = ZDateTime.Invalid;
							}
						}
					}
				}
			}
			return result;
		}

		protected override Customs.Business.ManifestStatus GetMessageStatus(EDIMessage message)
		{
			var result = ManifestStatus.NotSent;
			var messageSegments = ConvertDBMessageToUNOA(message.EM_MessageText);
			foreach (var segment in messageSegments)
			{
				if (segment.StartsWith("ERP"))
				{
					var elements = segment.Split('+');
					if (elements.Length > 2)
					{
						if (elements[2] == "421")
						{
							result = ManifestStatus.FromString("Line " + elements[1] + " - In Error");
							break;
						}
						else if (elements[2] == "322")
						{
							result = ManifestStatus.Rejected;
							break;
						}
						else if (elements[2] == "410")
						{
							result = ManifestStatus.Cleared;
							break;
						}
					}
				}
			}
			return result;
		}

		public override IManifestMessageBuilder NewCreateOrReplaceMessageBuilder()
		{
			return null;
		}

		protected override IManifestMessageBuilder[] NewMessageBuilders(Common.MessageBuilders.MessageSubTypes messageSubType)
		{
			return null;
		}

		protected override string ValidateEnvironmentForSendingManifests(Customs.Business.ISendsMessagesToCustoms sender)
		{
			return "Unable to send Exit2 message as Exit2 no longer exists.";
		}
	}
}
