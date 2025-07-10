using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	class UnknownTypeMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();

			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.eHub;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.Unknown;
			interchange.EI_Status = EDIInterchange.Status.Failed;
			Message.MessageStream.SeekBegin();
			interchange.SetEI_BodyDataSource(new StreamSource(Message.MessageStream));

			interchange.Notes.AddNew(true, Res.GetString("2009c4db-fdf7-4d79-ace0-c1cd6c89e871", "eHub Error"), Res.GetString("96c7a934-c7c8-40aa-9b38-64e117486ecb", "Interchange type '{0}' is not supported for eHub processing.", Message.SchemaName));
			return interchange;
		}
	}
}
