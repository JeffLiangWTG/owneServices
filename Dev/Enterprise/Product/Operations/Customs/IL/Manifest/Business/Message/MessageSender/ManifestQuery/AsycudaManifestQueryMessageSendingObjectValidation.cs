using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestQueryMessageSendingObjectValidation : AutoAsycudaManifestQueryMessageSendingObjectValidation
	{
		public AsycudaManifestQueryMessageSendingObjectValidation(AutoAsycudaManifestQueryMessageSendingObject parent) : base(parent)
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

		protected override void CheckManifestNumber()
		{
			base.CheckManifestNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ManifestNumberInfo);
		}

		protected override void CheckParentDealNumber()
		{
			base.CheckParentDealNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ParentDealNumberInfo);
		}
	}
}
