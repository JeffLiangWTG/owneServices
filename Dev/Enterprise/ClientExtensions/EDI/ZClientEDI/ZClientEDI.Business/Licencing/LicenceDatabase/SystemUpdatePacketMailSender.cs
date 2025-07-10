namespace Enterprise.Client.EDI.Licencing.Business
{
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.Environment;
	using Enterprise.Licensing;
	using Enterprise.ZArchitecture.Environment;

	public class SystemUpdatePacketMailSender : ISystemUpdatePacketSender
	{
		public const string AutoDeployLicenceEmailSubject = "ediEnterprise Reference Data Update";
		public const string AutoDeployLicenceKeyFileName = "UpdatedData.xml";

		public void Send(LicenceDatabase db, SystemUpdatePacket packet)
		{
			Env.OutgoingMailManager.CreateAndSave(CreateUpdateMessage(db, packet));
		}

		internal EmailDef CreateUpdateMessage(LicenceDatabase db, SystemUpdatePacket packetToAttach)
		{
			EmailDef email = new EmailDef();

			email.AddRecipientForUserCommunication(db.LD_PublicEmailAddressForUpdate);
			email.Subject = AutoDeployLicenceEmailSubject;

			string attachmentName = AutoDeployLicenceKeyFileName;
			string attachment = packetToAttach.ToXMLString();
			email.Attachments.Add(new AttachmentDef(attachmentName, AttachmentDef.StringToByteArray(attachment)));

			return email;
		}
	}
}

