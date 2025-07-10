using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DialogTest : TestCaseWithFactory
	{
		#region TestCreateNoEvents

		public void TestCreateNoEvents()
		{
			var dialog = new Dialog(null, false);

			AssertNotNull("null events", dialog);

			AssertEquals("TransmissionCode", string.Empty, dialog.TransmissionCode);
			AssertEquals("ResponseCode", string.Empty, dialog.ResponseCode);
			AssertEquals("TransmittedMessageText", string.Empty, dialog.TransmittedMessageText);
			AssertNotNull("Logs", dialog.Logs);
			AssertEquals("Logs Count", 0, dialog.Logs.Length);

			dialog = new Dialog(System.Array.Empty<StmALog>(), false);

			AssertNotNull("empty events", dialog);

			AssertEquals("TransmissionCode", string.Empty, dialog.TransmissionCode);
			AssertEquals("ResponseCode", string.Empty, dialog.ResponseCode);
			AssertEquals("TransmittedMessageText", string.Empty, dialog.TransmittedMessageText);
			AssertNotNull("Logs", dialog.Logs);
			AssertEquals("Logs Count", 0, dialog.Logs.Length);
		}

		#endregion

		#region TestTransmittedMessageText

		public void TestTransmittedMessageText()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent)
				.ToArray();

			var dex = logs.First(log => log.SL_SE_NKEvent == Events.DataExportCode);

			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			const string messageText = @"<ChupaChups></ChupaChups>";

			message.Content = XElement.Parse(messageText);

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = dex.PK;
			pivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
			pivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();

			var dialog = new Dialog(logs, false);
			AssertNotNull("prerequisite: dialog", dialog);
			AssertMultilineASCIIEquals("sent message text", messageText, dialog.TransmittedMessageText);
		}

		#endregion

		#region TransmissionCode

		public void TestTransmissionCode_NoResponse()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent);

			var dialog = new Dialog(logs.ToArray(), false);

			AssertEquals(Events.MessageSent.Code, dialog.TransmissionCode);
		}

		public void TestTransmissionCode_MessageAccepted()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent)
				.Concat(CreateLogs(Events.MessageAccepted));

			var dialog = new Dialog(logs.ToArray(), false);

			AssertEquals(Events.MessageSent.Code, dialog.TransmissionCode);
		}

		public void TestTransmissionCode_MessageRejected()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent)
				.Concat(CreateLogs(Events.MessageRejected));

			var dialog = new Dialog(logs.ToArray(), false);

			AssertEquals(Events.MessageSent.Code, dialog.TransmissionCode);
		}

		#endregion

		#region ResponseCode

		public void TestResponseCode_NoResponse()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent);

			var dialog = new Dialog(logs.ToArray(), false);

			AssertEquals(string.Empty, dialog.ResponseCode);
		}

		public void TestResponseCode_MessageAccepted()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent)
				.Concat(CreateLogs(Events.MessageAccepted));

			var dialog = new Dialog(logs.ToArray(), false);

			AssertEquals(Events.MessageAccepted.Code, dialog.ResponseCode);
		}

		public void TestResponseCode_MessageRejected()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent)
				.Concat(CreateLogs(Events.MessageRejected));

			var dialog = new Dialog(logs.ToArray(), false);

			AssertEquals(Events.MessageRejected.Code, dialog.ResponseCode);
		}

		#endregion

		#region Logs

		public void TestLogs_NoResponse()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent);

			var dialog = new Dialog(logs.ToArray(), false);

			AssertNotNull("Logs", dialog.Logs);
			AssertEquals("Logs Count", 2, dialog.Logs.Length);
			Assert("MessageSent", dialog.Logs.Any(log => log.SL_SE_NKEvent == Events.MessageSentCode));
			Assert("DataExport", dialog.Logs.Any(log => log.SL_SE_NKEvent == Events.DataExportCode));
		}

		public void TestLogs_MessageAccepted()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent)
				.Concat(CreateLogs(Events.MessageAccepted));

			var dialog = new Dialog(logs.ToArray(), false);

			AssertNotNull(dialog.Logs);
			AssertEquals(3, dialog.Logs.Length);
			Assert("MessageSent", dialog.Logs.Any(log => log.SL_SE_NKEvent == Events.MessageSentCode));
			Assert("DataExport", dialog.Logs.Any(log => log.SL_SE_NKEvent == Events.DataExportCode));
			Assert("MessageAccepted", dialog.Logs.Any(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode));
		}

		public void TestLogs_MessageRejected()
		{
			var logs = CreateLogs(Events.DataExport, Events.MessageSent)
				.Concat(CreateLogs(Events.MessageRejected));

			var dialog = new Dialog(logs.ToArray(), false);

			AssertNotNull(dialog.Logs);
			AssertEquals(3, dialog.Logs.Length);
			Assert("MessageSent", dialog.Logs.Any(log => log.SL_SE_NKEvent == Events.MessageSentCode));
			Assert("DataExport", dialog.Logs.Any(log => log.SL_SE_NKEvent == Events.DataExportCode));
			Assert("MessageRejected", dialog.Logs.Any(log => log.SL_SE_NKEvent == Events.MessageRejectedCode));
		}

		#endregion

		#region Implementation

		IEnumerable<StmALog> CreateLogs(params Event[] events)
		{
			var logs = events
				.Select(@event => Dummy.Logs.AddNew(@event))
				.ToList();

			Factory.Save();
			Thread.Sleep(10);

			return logs;
		}

		DummyWithLogs Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithLogs>()); }
		}

		DummyWithLogs dummy;

		#endregion
	}
}
