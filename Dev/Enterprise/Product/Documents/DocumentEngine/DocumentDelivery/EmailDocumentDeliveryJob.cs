using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2229:ImplementSerializationConstructors")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
	[Serializable]
	public class EmailDocumentDeliveryJob : AutoDocumentDeliveryJob
	{
		public EmailDocumentDeliveryJob(IDocumentSupportable businessObject, ZGuid documentCommandPK, bool isFactoryPopulateButDoNotSave, params string[] emailAddresses)
			: base(businessObject, isFactoryPopulateButDoNotSave, documentCommandPK)
		{
			this.emailAddresses = emailAddresses;
		}

#if NETFRAMEWORK
		EmailDocumentDeliveryJob(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		readonly string[] emailAddresses;

		protected override DeliveryInstructions GetDeliveryInstructions(DocumentPack pack)
		{
			var instructions = IsFactoryPopulateButDoNotSave ? new DeliveryInstructions(new FactoryStrategy.PopulateButDoNotSave(Factory)) : new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

			foreach (var emailAddress in emailAddresses)
			{
				instructions.Recipients.Add(new DocDeliveryContact(Factory)
				{
					DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
					AttachmentType = !DocumentCommand.SU_DefaultAttachmentType.IsEmpty
						? DocumentCommand.SU_DefaultAttachmentType.ToString()
						: OrgConstants.AttachmentType.PDF,
					Email = emailAddress,
				});
			}

			return instructions;
		}
	}
}
