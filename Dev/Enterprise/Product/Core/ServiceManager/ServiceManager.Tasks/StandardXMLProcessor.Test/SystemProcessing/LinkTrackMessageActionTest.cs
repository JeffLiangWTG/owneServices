using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class LinkTrackMessageActionTest : TestCaseWithFactory
	{
		public void TestExecuteAction()
		{
			IMessageAction action = new LinkTrackMessageAction(new BusinessObjectFactoryProvider(Factory));
			var message = Factory.New<XmlEDIMessage>();

			Guid aCampaignPk = Guid.NewGuid();
			Guid aContext1Pk = Guid.NewGuid();
			Guid aContext2Pk = Guid.NewGuid();
			Guid aUser1Pk = Guid.NewGuid();
			Guid aUser2Pk = Guid.NewGuid();
			const string aUrl = "www.wisetechglobal.com/acampaign";
			const string bUrl = "www.wisetechglobal.com/bcampaign";

			Guid bCampaignPk = Guid.NewGuid();
			Guid bContext1Pk = Guid.NewGuid();
			Guid deletedPk = Guid.NewGuid();

			CreateCampaign(aCampaignPk, "TST00001000");
			CreateCampaign(bCampaignPk, "TST00001001");
			CreateCampaignItem(aUser1Pk, aCampaignPk, OrgContactSchema.Constants.Prefix, Guid.NewGuid());
			CreateCampaignItem(aUser2Pk, aCampaignPk, OrgContactSchema.Constants.Prefix, Guid.NewGuid());

			CreateCampaignLink(aContext1Pk, aCampaignPk, aUrl, "Link1");
			CreateCampaignLink(aContext2Pk, aCampaignPk, aUrl, "Link2");
			CreateCampaignLink(bContext1Pk, bCampaignPk, bUrl, "Link1");

			var time1 = new DateTime(2014, 7, 1, 10, 15, 0);
			var time2 = new DateTime(2014, 7, 2, 11, 15, 0);
			var time3 = new DateTime(2014, 7, 3, 12, 15, 0);
			var time4 = new DateTime(2014, 7, 4, 13, 15, 0);

			var xml = new XElement("LinkTrack",
				CreateClickElement(aUser1Pk, aContext1Pk, "1.2.3.4", time1),
				CreateClickElement(aUser1Pk, aContext2Pk, "1.2.3.5", time2),
				CreateClickElement(aUser2Pk, aContext1Pk, "1.2.3.6", time3),
				CreateClickElement(Guid.Empty, bContext1Pk, "A0FF7F09", time4),
				CreateClickElement(deletedPk, aContext1Pk, "1.2.3.9", time4),
				CreateClickElement(aUser1Pk, deletedPk, "1.2.3.9", time4)
				);

			message.EM_MessageText = xml.ToString();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			INotifications notifications = null;
			List<ITransactionParticipant> participants;
			action.ExecuteAction(message, notifications, out participants);
			Factory.Save();

			AssertLinkClick("click1", aUser1Pk, aContext1Pk, "1.2.3.4", time1);
			AssertLinkClick("click2", aUser1Pk, aContext2Pk, "1.2.3.5", time2);
			AssertLinkClick("click3", aUser2Pk, aContext1Pk, "1.2.3.6", time3);
			AssertLinkClick("click4", Guid.Empty, bContext1Pk, "160.255.127.9", time4);

			AssertEquals("no extra participants since using factoryProvider.Current", 0, participants.Count);
		}

		public void TestExecuteAction_FromXml()
		{
			const string TestXml =
	@"<LinkTrack xmlns=""http://www.cargowise.com/Schemas/System"">
  <Click>
    <u>F64C5BF64C8A47888F797EDCAE7B4D46</u>
    <x>14EF8B76EF3A44C8A2712C99A10E152D</x>
    <i>0A3DA2DE</i>
    <t>2014-07-15T23:59:34Z</t>
  </Click>
  <Click>
  </Click>
  <Click>
    <u>F64C5BF64C8A47888F797EDCAE7B4D46</u>
  </Click>
</LinkTrack>";

			Guid campaignPk = Guid.NewGuid();
			Guid userPk = new Guid("F64C5BF64C8A47888F797EDCAE7B4D46");
			Guid contextPk = new Guid("14EF8B76EF3A44C8A2712C99A10E152D");
			CreateCampaign(campaignPk, "TST00001000");
			CreateCampaignItem(userPk, campaignPk, OrgContactSchema.Constants.Prefix, Guid.NewGuid());
			CreateCampaignLink(contextPk, campaignPk, "www.wisetechglobal.com", "Link");

			IMessageAction action = new LinkTrackMessageAction(new BusinessObjectFactoryProvider(Factory));
			var message = Factory.New<XmlEDIMessage>();
			message.EM_MessageText = TestXml;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			INotifications notifications = null;
			List<ITransactionParticipant> participants;
			action.ExecuteAction(message, notifications, out participants);
			Factory.Save();

			AssertLinkClick("clicked", userPk, contextPk, "10.61.162.222", new DateTime(2014, 7, 15, 23, 59, 00));
		}

		static void AssertLinkClick(string msg, Guid userPk, Guid contextPk, string ipString, DateTime clickTimeUtc)
		{
			bool hasUser = userPk != Guid.Empty;
			var cmd = Db.Connection.Command("select count(*) from " + GlbCompanyCampaignClickSchema.Constants.SqlSchemaName + "." + GlbCompanyCampaignClickSchema.Constants.TableName + " where " +
				GlbCompanyCampaignClickSchema.Constants.GCC_G8_Recipient + (hasUser ? " = @g8 and " : " is null and ") +
				GlbCompanyCampaignClickSchema.Constants.GCC_GCL + " = @gcl and " +
				GlbCompanyCampaignClickSchema.Constants.GCC_HostAddress + " = @ipAddress and " +
				GlbCompanyCampaignClickSchema.Constants.GCC_ClickTimeUtc + " = @clickTimeUtc");
			if (hasUser)
			{
				cmd.AddParameterBasedOnDbColumn("@g8", userPk, GlbCompanyCampaignClickSchema.GCC_G8_Recipient);
			}

			var ipAddress = IPAddress.Parse(ipString);
			cmd.AddParameterBasedOnDbColumn("@gcl", contextPk, GlbCompanyCampaignClickSchema.GCC_GCL);
			cmd.AddParameterBasedOnDbColumn("@ipAddress", ipAddress.GetAddressBytes(), GlbCompanyCampaignClickSchema.GCC_HostAddress);
			cmd.AddParameterBasedOnDbColumn("@clickTimeUtc", clickTimeUtc, GlbCompanyCampaignClickSchema.GCC_ClickTimeUtc);
			AssertEquals(msg, 1, (int)cmd.ExecuteScalar());

			if (hasUser)
			{
				var cmd2 = Db.Connection.Command("select " + GlbCompanyCampaignItemSchema.Constants.G8_TrackingStatus +
												" from " + GlbCompanyCampaignItemSchema.Constants.SqlSchemaName + "." + GlbCompanyCampaignItemSchema.Constants.TableName +
												" where " + GlbCompanyCampaignItemSchema.Constants.PK + " = @pk");
				cmd2.AddParameterBasedOnDbColumn("@pk", userPk, GlbCompanyCampaignItemSchema.PK);
				AssertEquals("Campaign item is updated", "VER", (string)cmd2.ExecuteScalar());
			}
		}

		static void CreateCampaign(Guid campaignPk, string campaignID)
		{
			var cmd = Db.Connection.Command(string.Format("insert {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}) VALUES (@pk, @gc, @name, @id, @systemCreateTimeUtc, @systemCreateUser, @systemLastEditTimeUtc, @systemLastEditUser)",
				GlbCompanyCampaignSchema.Constants.TableName,
				GlbCompanyCampaignSchema.Constants.PK,
				GlbCompanyCampaignSchema.Constants.G0_GC,
				GlbCompanyCampaignSchema.Constants.G0_CampaignName,
				GlbCompanyCampaignSchema.Constants.G0_CampaignID,
				GlbCompanyCampaignSchema.Constants.G0_SystemCreateTimeUtc,
				GlbCompanyCampaignSchema.Constants.G0_SystemCreateUser,
				GlbCompanyCampaignSchema.Constants.G0_SystemLastEditTimeUtc,
				GlbCompanyCampaignSchema.Constants.G0_SystemLastEditUser));

			cmd.AddParameterBasedOnDbColumn("@pk", campaignPk, GlbCompanyCampaignSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@gc", Env.CurrentCompany.PK, GlbCompanyCampaignSchema.G0_GC);
			cmd.AddParameterBasedOnDbColumn("@name", ZGuid.NewZGuid().ToString(), GlbCompanyCampaignSchema.G0_CampaignName);
			cmd.AddParameterBasedOnDbColumn("@id", campaignID, GlbCompanyCampaignSchema.G0_CampaignID);
			cmd.AddParameterBasedOnDbColumn("@systemCreateTimeUtc", ZDateTime.UtcNow, GlbCompanyCampaignSchema.G0_SystemCreateTimeUtc);
			cmd.AddParameterBasedOnDbColumn("@systemCreateUser", "E", GlbCompanyCampaignSchema.G0_SystemCreateUser);
			cmd.AddParameterBasedOnDbColumn("@systemLastEditTimeUtc", ZDateTime.UtcNow, GlbCompanyCampaignSchema.G0_SystemLastEditTimeUtc);
			cmd.AddParameterBasedOnDbColumn("@systemLastEditUser", "E", GlbCompanyCampaignSchema.G0_SystemLastEditUser);
			cmd.ExecuteNonQuery();
		}

		static void CreateCampaignItem(Guid itemPk, Guid campaignPk, string recipientTableCode, Guid recipientPk)
		{
			var cmd = Db.Connection.Command(string.Format("insert {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}) VALUES (@pk, @g0, @recipientTableCode, @recipientPk, @systemCreateTimeUtc, @systemCreateUser, @systemLastEditTimeUtc, @systemLastEditUser)",
				GlbCompanyCampaignItemSchema.Constants.TableName,
				GlbCompanyCampaignItemSchema.Constants.PK,
				GlbCompanyCampaignItemSchema.Constants.G8_G0,
				GlbCompanyCampaignItemSchema.Constants.G8_RecipientTableCode,
				GlbCompanyCampaignItemSchema.Constants.G8_RecipientID,
				GlbCompanyCampaignItemSchema.Constants.G8_SystemCreateTimeUtc,
				GlbCompanyCampaignItemSchema.Constants.G8_SystemCreateUser,
				GlbCompanyCampaignItemSchema.Constants.G8_SystemLastEditTimeUtc,
				GlbCompanyCampaignItemSchema.Constants.G8_SystemLastEditUser));
			cmd.AddParameterBasedOnDbColumn("@pk", itemPk, GlbCompanyCampaignItemSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@g0", campaignPk, GlbCompanyCampaignItemSchema.G8_G0);
			cmd.AddParameterBasedOnDbColumn("@recipientTableCode", recipientTableCode, GlbCompanyCampaignItemSchema.G8_RecipientTableCode);
			cmd.AddParameterBasedOnDbColumn("@recipientPk", recipientPk, GlbCompanyCampaignItemSchema.G8_RecipientID);
			cmd.AddParameterBasedOnDbColumn("@systemCreateTimeUtc", ZDateTime.UtcNow, GlbCompanyCampaignItemSchema.G8_SystemCreateTimeUtc);
			cmd.AddParameterBasedOnDbColumn("@systemCreateUser", "E", GlbCompanyCampaignItemSchema.G8_SystemCreateUser);
			cmd.AddParameterBasedOnDbColumn("@systemLastEditTimeUtc", ZDateTime.UtcNow, GlbCompanyCampaignItemSchema.G8_SystemLastEditTimeUtc);
			cmd.AddParameterBasedOnDbColumn("@systemLastEditUser", "E", GlbCompanyCampaignItemSchema.G8_SystemLastEditUser);
			cmd.ExecuteNonQuery();
		}

		static void CreateCampaignLink(Guid linkPk, Guid campaignPk, string url, string context)
		{
			var cmd = Db.Connection.Command(string.Format("insert {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}) VALUES (@pk, @g0, @url, @context, @systemCreateTimeUtc, @systemCreateUser, @systemLastEditTimeUtc, @systemLastEditUser)",
				GlbCompanyCampaignLinkSchema.Constants.TableName,
				GlbCompanyCampaignLinkSchema.Constants.PK,
				GlbCompanyCampaignLinkSchema.Constants.GCL_G0_Campaign,
				GlbCompanyCampaignLinkSchema.Constants.GCL_URL,
				GlbCompanyCampaignLinkSchema.Constants.GCL_Context,
				GlbCompanyCampaignLinkSchema.Constants.GCL_SystemCreateTimeUtc,
				GlbCompanyCampaignLinkSchema.Constants.GCL_SystemCreateUser,
				GlbCompanyCampaignLinkSchema.Constants.GCL_SystemLastEditTimeUtc,
				GlbCompanyCampaignLinkSchema.Constants.GCL_SystemLastEditUser));
			cmd.AddParameterBasedOnDbColumn("@pk", linkPk, GlbCompanyCampaignLinkSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@g0", campaignPk, GlbCompanyCampaignLinkSchema.GCL_G0_Campaign);
			cmd.AddParameterBasedOnDbColumn("@url", url, GlbCompanyCampaignLinkSchema.GCL_URL);
			cmd.AddParameterBasedOnDbColumn("@context", context, GlbCompanyCampaignLinkSchema.GCL_Context);
			cmd.AddParameterBasedOnDbColumn("@systemCreateTimeUtc", ZDateTime.UtcNow, GlbCompanyCampaignLinkSchema.GCL_SystemCreateTimeUtc);
			cmd.AddParameterBasedOnDbColumn("@systemCreateUser", "E", GlbCompanyCampaignLinkSchema.GCL_SystemCreateUser);
			cmd.AddParameterBasedOnDbColumn("@systemLastEditTimeUtc", ZDateTime.UtcNow, GlbCompanyCampaignLinkSchema.GCL_SystemLastEditTimeUtc);
			cmd.AddParameterBasedOnDbColumn("@systemLastEditUser", "E", GlbCompanyCampaignLinkSchema.GCL_SystemLastEditUser);
			cmd.ExecuteNonQuery();
		}

		XElement CreateClickElement(Guid userPk, Guid contextPk, string ipAddress, DateTime clickUtc)
		{
			var elems = new List<object>();
			if (userPk != Guid.Empty)
			{
				elems.Add(new XElement("u", userPk.ToString().Replace("-", "")));
			}
			elems.Add(new XElement("x", contextPk.ToString().Replace("-", "")));
			elems.Add(new XElement("i", ipAddress));
			elems.Add(new XElement("t", clickUtc.ToString("u").Replace(" ", "T")));

			return new XElement("Click", elems.ToArray());
		}
	}
}
