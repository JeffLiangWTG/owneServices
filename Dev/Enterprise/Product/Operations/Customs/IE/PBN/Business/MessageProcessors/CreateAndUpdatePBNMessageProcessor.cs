using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.PBN.Messaging;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.PBN.Business;

public class CreateAndUpdatePBNMessageProcessor : PBNMessageProcessor<CreateAndUpdatePBNProvider>
{
	public CreateAndUpdatePBNMessageProcessor(LoggingInformation logger, Type messageObjectType) : base(logger, messageObjectType)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("E7DCCF55-B9BB-404B-8A7A-D29606BE9F04", "PBN Message Processor for CPB, UPB and UPD");

	protected override Type MessageInterpreterType => typeof(CreateAndUpdatePBNMessageInterpreter);

	public override string GetLogicalStatus(PBNInboundEDIMessage message, IE.Messaging.IMessageAttachee messageAttachee, CreateAndUpdatePBNProvider provider) => LogicalStatusList.Codes.Accepted;

	public override string GetEntryStatus(PBNInboundEDIMessage message, IE.Messaging.IMessageAttachee messageAttachee, CreateAndUpdatePBNProvider provider) => PBNCustomsStatusList.GetCodeFromEnglishDescription(provider.Status);

	protected override IRegistryItem GetEmailGroupRegistryItem() => IECustomsDataRegistry.Instance.SendPreBoardingNotificationIds;

	protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

	protected override void ProcessMessageCore(BusinessObjectFactory factory, PBNInboundEDIMessage message, CreateAndUpdatePBNProvider provider)
	{
		base.ProcessMessageCore(factory, message, provider);
		if (provider.Status.ToString() == PBNCustomsStatusList.EnglishDescriptions.Proceed || provider.Status.ToString() == PBNCustomsStatusList.EnglishDescriptions.Incomplete)
		{
			message.EM_ApplicationReference = provider.PbnID;
			var header = (AsycudaManifestHeader)message.EM_LinkedObject;
			header.RegistrationNumber = provider.PbnID;

			UpdateDeclarationsAdded(header);
			DeleteDeclarations(header);
		}
	}

	void UpdateDeclarationsAdded(AsycudaManifestHeader header)
	{
		foreach (var declaration in header.CustomsReferenceCollection.Where(x => !x.CSI_ReferenceNumber.IsEmpty && x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBA))
		{
			declaration.CSI_Status = PBNDeclarationReferenceStatusList.Codes.HBA;
		}
		foreach (var declaration in header.TransitDeclarationCollection.Where(x => !x.CSI_ReferenceNumber.IsEmpty && x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBA))
		{
			declaration.CSI_Status = PBNDeclarationReferenceStatusList.Codes.HBA;
		}
	}

	void DeleteDeclarations(AsycudaManifestHeader header)
	{
		header.CustomsReferenceCollection.Where(x => x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBD).DeleteAll();
		header.TransitDeclarationCollection.Where(x => x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBD).DeleteAll();
	}
}
