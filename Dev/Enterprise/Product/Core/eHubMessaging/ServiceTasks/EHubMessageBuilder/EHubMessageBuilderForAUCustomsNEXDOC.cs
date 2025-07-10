using System.IO;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForAUCustomsNEXDOC : EHubMessageBuilder
	{
		public EHubMessageBuilderForAUCustomsNEXDOC(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override IeHubMessage BuildCore()
		{
			var messageStream = new VirtualMemoryStream();
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				var writer = new StreamWriter(messageStream);
				writer.Write(reader.ReadToEnd());
				writer.Flush();
			}

			messageStream.Seek(0, SeekOrigin.Begin);

			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				interchange.EI_To + Res.GetString("C71B26BB-52C7-474F-BE62-667345E34A16", "-REX"),
				MessageSchemaType.Xml,
				interchange.EI_ApplicationCode,
				schemaName,
				messageStream);
		}
	}
}
