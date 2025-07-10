using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class LinkTrackMessageAction : IMessageAction
	{
		public LinkTrackMessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
			this.factoryProvider = factoryProvider;
		}

		readonly BusinessObjectFactoryProvider factoryProvider;

		bool IMessageAction.ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			var factory = factoryProvider.Current;
			var campaignClickList = new List<IGlbCompanyCampaignClick>();
			HashSet<Guid> contextPks = new HashSet<Guid>();
			HashSet<Guid> userPks = new HashSet<Guid>();

			using (var reader = message.GetEM_MessageTextReader(false))
			{
				var root = XElement.Load(reader);
				var node = root.Nodes().First();

				foreach (var click in root.Elements().Where(x => x.Name.LocalName == "Click"))
				{
					ProcessCampaignClick(factory, campaignClickList, contextPks, userPks, click);
				}
			}

			RemoveExistingPks(contextPks, GlbCompanyCampaignLinkSchema.Constants.TableName, GlbCompanyCampaignLinkSchema.Constants.PK);
			RemoveExistingPks(userPks, GlbCompanyCampaignItemSchema.Constants.TableName, GlbCompanyCampaignItemSchema.Constants.PK);

			if (contextPks.Count > 0 || userPks.Count > 0)
			{
				foreach (var campaignClick in campaignClickList)
				{
					if (contextPks.Contains(campaignClick.GCC_GCL.ToGuid()) ||
						(campaignClick.GCC_G8_Recipient.IsValid && userPks.Contains(campaignClick.GCC_G8_Recipient.ToGuid())))
					{
						campaignClick.Delete();
					}
				}
			}

			UpdateCampaignItems(factory, campaignClickList);

			participants = new List<ITransactionParticipant>(1);
			return true;
		}

		static void ProcessCampaignClick(BusinessObjectFactory factory, List<IGlbCompanyCampaignClick> campaignClickList, HashSet<Guid> contextPks, HashSet<Guid> userPks, XElement clickElement)
		{
			string user = null;
			string context = null;
			string ipAddress = null;
			string time = null;

			foreach (var element in clickElement.Elements())
			{
				switch (element.Name.LocalName)
				{
					case "u":
						user = element.Value;
						break;
					case "x":
						context = element.Value;
						break;
					case "i":
						ipAddress = element.Value;
						break;
					case "t":
						time = element.Value;
						break;
					default:
						break;
				}
			}

			bool hasUser = !string.IsNullOrEmpty(user);
			Guid userPk = Guid.Empty;
			Guid contextPk;
			DateTime clickTimeUtc;
			IPAddress hostAddress;
			if ((!hasUser || Guid.TryParse(user, out userPk)) &&
				Guid.TryParse(context, out contextPk) &&
				DateTime.TryParseExact(time, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out clickTimeUtc))
			{
				var campaignClick = factory.New<IGlbCompanyCampaignClick>();
				contextPks.Add(contextPk);
				campaignClick.GCC_GCL = contextPk;
				campaignClick.GCC_ClickTimeUtc = clickTimeUtc;
				if (ipAddress.IndexOf('.') > 0)
				{
					if (IPAddress.TryParse(ipAddress, out hostAddress))
					{
						campaignClick.GCC_HostAddress = hostAddress.GetAddressBytes();
					}
				}
				else
				{
					byte[] hostAddressBytes;
					if (TryParseHexStringToByteArray(ipAddress, out hostAddressBytes))
					{
						campaignClick.GCC_HostAddress = hostAddressBytes;
					}
				}
				if (hasUser)
				{
					userPks.Add(userPk);
					campaignClick.GCC_G8_Recipient = userPk;
				}
				campaignClickList.Add(campaignClick);
			}
		}

		void UpdateCampaignItems(BusinessObjectFactory factory, List<IGlbCompanyCampaignClick> bizoList)
		{
			var campaignItemPks = bizoList.Where(bizO => !((BusinessObject)bizO).IsDeleted).Select(link => link.GCC_G8_Recipient).Distinct().ToArray();
			var campaignItems = factory.Load<IGlbCompanyCampaignItem>(new ZQuery(GlbCompanyCampaignItemSchema.PK, campaignItemPks));
			foreach (var campaignItem in campaignItems)
			{
				campaignItem.MarkAsVerified();
			}
		}

		static void RemoveExistingPks(HashSet<Guid> set, string tableName, string pkColumn)
		{
			if (set.Count > 0)
			{
				string sql = "select " + pkColumn +
					" from " + tableName +
					" where " + pkColumn +
					" in (" + string.Join(",", set.Select(x => x.ToSqlGuid())) + ")";
				using (var reader = Db.Connection.Command(sql).ExecuteReader())
				{
					while (reader.Read())
					{
						Guid pk = reader.GetGuid(0);
						set.Remove(pk);
					}
				}
			}
		}

		void IMessageAction.SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
		}

		static bool TryParseHexStringToByteArray(string hex, out byte[] bytes)
		{
			bytes = null;

			if (hex.Length % 2 == 1)
			{
				return false;
			}

			bytes = new byte[hex.Length >> 1];

			for (int i = 0; i < bytes.Length; ++i)
			{
				int hi = GetHexVal(hex[i << 1]);
				int lo = GetHexVal(hex[(i << 1) + 1]);
				if (hi < 0 || hi > 15 || lo < 0 || lo > 15)
				{
					return false;
				}
				bytes[i] = (byte)((hi << 4) + lo);
			}

			return true;
		}

		public static int GetHexVal(char hex)
		{
			var val = (int)hex;
			// Allow for upper or lower case letters
			return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
		}
	}
}
