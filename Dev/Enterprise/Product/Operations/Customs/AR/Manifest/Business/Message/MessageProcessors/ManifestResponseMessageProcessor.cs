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

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class ManifestResponseMessageProcessor : ARMessageProcessor<ARMessage>
	{
		public ManifestResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
			tableCreator = new HtmlTableCreator(new string[] { Res.GetString("B3EA9073-18FF-4E7E-B77A-77956373C7A2", "Column"), Res.GetString("4AD960AB-7396-4EDC-8276-1E18B6BA727F", "Value") });
		}

		protected override string MessageFriendlyNameCore => Res.GetString("FC5F87FE-3EC4-4B67-94B7-DCDF7F7E3A65", "AR Message");

		protected override ZString GetMailSubject() => Res.GetString("9FE55977-21D0-4717-B08D-0F4A14AB2C08", "Manifest Status Message");

		protected override ZString ProcessMessageCore(ARMessage message) => ProcessManifestMessageCore(message);

		protected virtual ZString ProcessManifestMessageCore(ARMessage message) => ZString.Empty;

		protected override ZString GetShowEditFormUrlCreator(IMessageAttachee messageAttachee) => ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, messageAttachee.PK.ToGuid());

		protected override ZString GetFailureTitle(IMessageAttachee messageAttachee)
		{
			return Res.GetString("BD182B1E-BC15-4A04-9208-3D2D640DCA76", "Manifest Message for job {0} has been rejected. For details please follow the Link to the Manifest", messageAttachee.JobReference);
		}

		protected override ZString GetSuccessfulTitle(IMessageAttachee messageAttachee)
		{
			return Res.GetString("93F227E6-29E0-4A61-8B69-EA1C194A30C4", "Manifest Message for job {0} has been cleared. For details please follow the Link to the Manifest", messageAttachee.JobReference);
		}
	}
}
