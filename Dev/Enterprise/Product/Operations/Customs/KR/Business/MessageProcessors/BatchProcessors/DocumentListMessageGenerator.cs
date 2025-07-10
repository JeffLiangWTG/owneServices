using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class DocumentListMessageGenerator
	{
		public readonly LoggingInformation Logger;
		public DocumentListMessageGenerator(LoggingInformation logger)
		{
			Logger = logger;
		}

		public void GenerateEdiMessage(CancellationToken token, GlbCompany company)
		{
			token.ThrowIfCancellationRequested();

			var factory = new BusinessObjectFactory();
			var message = factory.New<EDIMessage>();

			message.EM_GB = company.ActiveBranches.FirstOrDefault().PK;
			message.EM_MessageType = Constants.EDIInterchangeType.DLT;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			factory.Save();

			Logger.Log(Res.GetString("{96BD8B43-8050-4426-8672-B094C9D9F892}", "'{0}' DLT message has been created successfully.", message.EM_MessageNum));
		}
	}
}
