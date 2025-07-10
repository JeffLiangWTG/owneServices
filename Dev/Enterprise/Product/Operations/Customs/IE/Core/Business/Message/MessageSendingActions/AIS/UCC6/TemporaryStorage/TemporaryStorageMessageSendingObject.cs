using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObject : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject, IFallbackProcedureSendingObject
	{
		public TemporaryStorageMessageSendingObject(TemporaryStorageHeader header) : base(header)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				SetMessageSendingDefaultValues();
			}
		}

		public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

		public new TemporaryStorageMessageSendingObjectValidation Validation => (TemporaryStorageMessageSendingObjectValidation)base.Validation;

		protected override EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectValidation GetNewValidation() => new TemporaryStorageMessageSendingObjectValidation(this);

		[ResourceStringData("IE.TemporaryStorageMessageSendingObject|DeclarationType", Caption = "Declaration Type")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageMessageSendingObjectLookups.DeclarationTypes))]
		public override ZString DeclarationType
		{
			get => base.DeclarationType;
			set => base.DeclarationType = value;
		}

		protected override bool DeclarationType_ReadOnly => true;

		public new TemporaryStorageMessageSendingObjectLookups Lookups => new TemporaryStorageMessageSendingObjectLookups(this);

		ZString IFallbackProcedureSendingObject.CustomsReferenceNumber => CustomsReference;

		void SetMessageSendingDefaultValues()
		{
			var provider = Header.MessagingProvider;
			if (provider is TemporaryStorageMessagingProvider ieProvider)
			{
				DeclarationType = ieProvider.GetDefaultDeclarationType(this);
			}
		}
	}
}
