using System.IO;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class TestHelper : IAccountingDataTransferTestHelper
	{
		string IAccountingDataTransferTestHelper.SerializeDataObject<T>(T dataObject, IUniversalXmlSchema schema)
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
			using (var reader = new StreamReader(stream))
			{
				new XmlWriter().WriteXML(dataObject, stream, schema != null ? schema.Namespace : logger.OutboundSessionTracker.GetValueSafe(x => x.Schema).GetValueSafe(x => x.Namespace));
				stream.Flush();
				stream.Position = 0;
				return reader.ReadToEnd();
			}
		}
	}
}
