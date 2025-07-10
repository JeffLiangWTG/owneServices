using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class ManifestResponseMessageProcessor : MXMessageProcessor<MXMessage>
	{
		public ManifestResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
			tableCreator = new HtmlTableCreator(new string[] { Res.GetString("59D78375-7547-45A4-9014-3E2DF7BC6B79", "Column"), Res.GetString("0D13618D-5A98-4CC6-8669-FACADA3D89F9", "Value") });
		}

		protected override string MessageFriendlyNameCore => Res.GetString("52A6A944-3DB7-4646-A5B1-B2F264986C5F", "MX Message");

		protected override ZString GetMailSubject() => Res.GetString("6B0198B2-76FD-4F4F-AA17-71134239A783", "Manifest Status Message");

		protected override ZString ProcessMessageCore(MXMessage message) => ProcessManifestMessageCore(message);

		protected virtual ZString ProcessManifestMessageCore(MXMessage message) => ZString.Empty;

		protected override ZString GetShowEditFormUrlCreator(IMessageAttachee messageAttachee) => ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, messageAttachee.PK.ToGuid());

		protected override ZString GetFailureTitle(IMessageAttachee messageAttachee)
		{
			var bill = messageAttachee as IManifestMessageAttachee;
			return Res.GetString("1FA43760-FC3E-4DDA-88F5-4FA754F3EA0A", "Manifest Message for job {0}, bill {1} has been rejected. For details please follow the Link to the Manifest", messageAttachee.JobReference, bill?.BillNumber ?? ZString.Empty);
		}

		protected override ZString GetSuccessfulTitle(IMessageAttachee messageAttachee)
		{
			var bill = messageAttachee as IManifestMessageAttachee;
			return Res.GetString("6EE75EF3-6174-4BD9-990C-521998779DC8", "Manifest Message for job {0}, bill {1} has been cleared. For details please follow the Link to the Manifest", messageAttachee.JobReference, bill?.BillNumber ?? ZString.Empty);
		}
	}
}
