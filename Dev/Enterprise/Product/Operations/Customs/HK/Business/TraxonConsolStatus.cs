using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.HK.Business
{
	public class TraxonConsolStatus : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constants
		public const string NoMessagesSent = "No Messages have been sent";
		public const string AwaitingResponse = "Currently Awaiting Response.";
		#endregion

		public TraxonConsolStatus(ForwardingConsol consol)
			: base(consol.Factory)
		{
			this.Consol = consol;
		}

		public readonly ForwardingConsol Consol;

		public TraxonMessageCollection OrderedMessages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new TraxonMessageCollection(this);
				}
				fMessages.Load();
				fMessages.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc, ListSortDirection.Descending);
				return fMessages;
			}
		}

		public bool IsWaitingForResponse
		{
			get { return Traxon_MessageStatus == AwaitingResponse; }
		}

		public virtual ZString Traxon_MessageStatus
		{
			get
			{
				ZString result = NoMessagesSent;
				if (OrderedMessages.Count > 0)
				{
					TraxonMessageCollection transmitted = TransmittedMessages;
					transmitted.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc, ListSortDirection.Descending);
					TraxonMessageCollection received = ReceivedMessages;
					received.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc, ListSortDirection.Descending);
					EDIMessage lastTransmitted = transmitted[0]; // Last transmitted cannot be null as OrderedMessages.Count is greater then 0
					EDIMessage lastReceived = received.Count != 0 ? received[0] : null;
					if (lastReceived == null || lastTransmitted.EM_SystemCreateTimeUtc > lastReceived.EM_SystemCreateTimeUtc)
					{
						result = AwaitingResponse;
					}
					else
					{
						string[][][] messagePart = MessageProcessingUtils.ReadMessage(new UNOACharacterSet(), lastReceived.EM_MessageText);
						result = "";

						bool insideUNH = false;
						foreach (string[][] segment in messagePart)
						{
							if (SafeGet(segment, 1, 0) == "Z03")
							{
								result = "Acknowledged : " + SafeGet(segment, 4, 0) + SafeGet(segment, 4, 1);
								break;
							}
							if (SafeGet(segment, 1, 0) == "AA0")
							{
								result = "Error : " + SafeGet(segment, 4, 0) + SafeGet(segment, 4, 1);
								break;
							}
							if (SafeGet(segment, 0, 0).Left(3) == "FMA")
							{
								ZInt firstSlashPosition = SafeGet(segment, 0, 0).IndexOf('/');
								result = "Acknowledged : AWB " + SafeGet(segment, 0, 0).SubstringSafe(firstSlashPosition + 1);
								break;
							}
							if (SafeGet(segment, 0, 0).Left(3) == "FNA")
							{
								ZInt firstSlashPosition = SafeGet(segment, 0, 0).IndexOf('/');
								result = "Error : " + SafeGet(segment, 0, 0).SubstringSafe(firstSlashPosition + 1);
								break;
							}

							if (SafeGet(segment, 0, 0) == "UNH")
							{
								insideUNH = true;
							}
							else if (SafeGet(segment, 0, 0) == "UNT")
							{
								insideUNH = false;
							}
							else if (insideUNH)
							{
								var inner = SafeGet(segment, 2, 0);
								if (!string.IsNullOrEmpty(inner))
								{
									try
									{
										var messageNumber = int.Parse(SafeGet(segment, 0, 0));

										result = "Acknowledged : " + inner;
										break;
									}
									catch (Exception ex) when (!ex.IsCriticalException())
									{
									}
								}
							}
						}
					}
				}
				return result;
			}
		}

		public virtual void ValidateTraxon_MessageStatus()
		{
			Traxon_MessageStatusInfo.ClearAllNotifications();
		}

		public ZPropertyInfo Traxon_MessageStatusInfo
		{
			get { return GetZPropertyInfo(nameof(Traxon_MessageStatus)); }
		}

		#region Implementation

		protected TraxonMessageCollection TransmittedMessages
		{
			get
			{
				ZQuery filter = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				TraxonMessageCollection result = new TraxonMessageCollection(this);
				result.Load(filter);
				return result;
			}
		}

		protected TraxonMessageCollection ReceivedMessages
		{
			get
			{
				ZQuery filter = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				TraxonMessageCollection result = new TraxonMessageCollection(this);
				result.Load(filter);
				return result;
			}
		}

		protected TraxonMessageCollection fMessages;

		protected ZString SafeGet(string[][] segment, int i, int j)
		{
			if (segment.Length > i && segment[i].Length > j)
			{
				return segment[i][j];
			}

			return null;
		}

		#endregion
	}
}
