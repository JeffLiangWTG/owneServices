using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestMessageSendingObjectValidation : AutoAsycudaManifestMessageSendingObjectValidation
	{
		public AsycudaManifestMessageSendingObjectValidation(AutoAsycudaManifestMessageSendingObject parent) : base(parent)
		{
		}

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			MandatoryValidation.CheckEntered(Parent.MessageTypeInfo);
		}

		protected override void CheckMessageSubType()
		{
			base.CheckMessageSubType();
			MandatoryValidation.CheckEntered(Parent.MessageSubTypeInfo);
		}
	}
}
