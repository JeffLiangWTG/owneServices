using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM460;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM460MessageInterpreter))]
	sealed class IM460MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM460MessageInterpreter, IIM460Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM460;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", AISInterchangeProcessorTestHelper.GetStandardIM460Text("LRN123456789", "21IEDUB11A782454R2", "23IECUSREG000072U1"));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => Interpretation("B00000012");

		internal static ZString Interpretation(string jobNumber)
		{
			return $@"A Control Notice (IM460) message has been received for Job {jobNumber}.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr>
					<td>LRN</td><td>LRN123456789</td>
				</tr>
				<tr>
					<td>Customs Registration Number</td><td>23IECUSREG000072U1</td>
				</tr>
				<tr>
					<td>MRN</td><td>21IEDUB11A782454R2</td>
				</tr>
				<tr>
					<td>Notification Date</td><td>14-Sep-23</td>
				</tr>
				<tr>
					<td>Notification Type</td><td>4</td>
				</tr>
				<tr>
					<td>Anticipated Control Date</td><td>21-Sep-23</td>
				</tr>
				<tr>
					<td>Text</td><td>Text</td>
				</tr>
				<tr>
					<td>Overall Control Type</td><td>Orange - Documentary Control</td>
				</tr>
			</table><br />
			<br />
			Type of Control:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr>
					<td>Sequence Number</td><td>1</td>
				</tr>
				<tr>
					<td>Control Type</td><td>10 - Documentary controls</td>
				</tr>
				<tr>
					<td>Control Text</td><td>Test Remarks 1</td>
				</tr>
			</table><br />
			<br />
			Type of Control:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr>
					<td>Sequence Number</td><td>2</td>
				</tr>
				<tr>
					<td>Control Type</td><td>50 - Other</td>
				</tr>
				<tr>
					<td>Control Text</td><td>Test Remarks 2</td>
				</tr>
			</table><br />
			<br />
			Requested Document:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr>
					<td>Sequence Number</td><td>1</td>
				</tr>
				<tr>
					<td>Requested Document Type</td><td>Y057 - Goods not requiring the presentation of a FLEGT import licence for timber</td>
				</tr>
				<tr>
					<td>cc Qualifier</td><td>AB</td>
				</tr>
				<tr>
					<td>Reference Number</td><td>12345</td>
				</tr>
				<tr>
					<td>Requested Document Description</td><td>Please provide</td>
				</tr>
			</table><br />
			<br />
			Requested Document:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr>
					<td>Sequence Number</td><td>2</td>
				</tr>
				<tr>
					<td>Requested Document Type</td><td>Y022 - Consignor / exporter (AEO certificate number)</td>
				</tr>
				<tr>
					<td>cc Qualifier</td><td>CD</td>
				</tr>
				<tr>
					<td>Reference Number</td><td>67890</td>
				</tr>
				<tr>
					<td>Requested Document Description</td><td>Description</td>
				</tr>
			</table>";
		}

		protected override IIM460Provider GetProvider(TextReader reader) => new IM460Provider(new MailBoxItemProvider<Im460>(reader).Message);

		protected override void SetUp()
		{
			MessageTestHelper.SetupCL716Types(Factory);
			MessageTestHelper.SetupCL215Types(Factory);
			base.SetUp();
		}
	}
}
