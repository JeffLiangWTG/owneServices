using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Xml.Testing
{
	class NativeXmlRequestHandlerTest : TestCase
	{
		public void TestProcess()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <Organization>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""OrgHeader"" FieldName=""Code"">
          AUSDISMEL
        </Criteria>
      </CriteriaGroup>
    </Organization>
  </Body>
</NativeRequest>")))
			{
				var handler = new NativeXmlRequestHandler("Organization", ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new DummySimpleLogger()));
				var message = new BusinessObjectFactory().New<IHttpXmlEDIMessage>();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.NativeDataQuery;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
				message.EM_MessageSubType = "XRO";
				message.EM_Status = EDIMessageStatusList.Codes.Received;
				message.SetEM_MessageTextOrDataSource(stream);

				AssertExceptionThrown<NativeXMLUserVisibleException>("NativeXMLUserVisibleException should be thrown.", "The 'Native' start tag on line 1 position 2 does not match the end tag of 'NativeRequest'. Line 11, position 3.", () => handler.Process(message));
			}
		}

		[UseSnapshotProtection]
		public void TestInvalidXmlFailsGracefully()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(
@"<Organization xmlns=""http://www.cargowise.com/Schemas/Native"">
      <CriteriaGroup Type=""Partial""></CriteriaGroup>
    </Organization>")))
			{
				var logger = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new DummySimpleLogger());
				var handler = new NativeXmlRequestHandler("Organization", logger);
				var message = new BusinessObjectFactory().New<IHttpXmlEDIMessage>();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.NativeDataQuery;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
				message.EM_MessageSubType = "XRO";
				message.EM_Status = EDIMessageStatusList.Codes.Received;
				message.SetEM_MessageTextOrDataSource(stream);

				using (var response = handler.Process(message))
				{
					AssertContains("Missing mandatory element OrgHeader", string.Join(System.Environment.NewLine, logger.Logs.Select(l => l.Message).ToArray()));
				}
			}
		}

		class DummySimpleLogger : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => System.Array.Empty<ISimpleLog>();

			public void Log(LogType type, string message)
			{
			}
		}
	}
}
