using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using IXmlCustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class MrnFieldHandler : IResponseMessageHandler
{
	public MrnFieldHandler(IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter)
	{
		this.customsLinkedObjectAdapter = Argument.NotNull(customsLinkedObjectAdapter, nameof(customsLinkedObjectAdapter));
	}

	public void Handle(IXmlCustomsResponseMessage responseMessage)
	{
		Argument.NotNull(responseMessage, nameof(responseMessage));

		var mrn = responseMessage.Mrn;
		if (string.IsNullOrWhiteSpace(mrn))
		{
			return;
		}

		var issueDate = (responseMessage.ProcessingStartedAtUtc ?? ZDateTime.Now).ToLocalBranchTime();
		var entryNum = CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn(mrn);
		var entryLineReference = customsLinkedObjectAdapter.CustomsOfficeOfPresentation.SubstringSafe(2);
		var endDateTime = (responseMessage.ProcessingEndAtUtc ?? ZDateTime.Now).ToLocalBranchTime();

		customsLinkedObjectAdapter.UpdateOrInsertEntryNumber(CusEntryNumberConstants.EntryTypes.RegistrationNumber,
			entryNum,
			entryLineReference,
			issueDate,
			null);
		customsLinkedObjectAdapter.UpdateOrInsertEntryNumber(CusEntryNumberConstants.EntryTypes.Mrn,
			mrn,
			entryLineReference,
			issueDate,
			null);

		customsLinkedObjectAdapter.SetStatusAsRegistered(endDateTime);
		customsLinkedObjectAdapter.SetSentEntryLinesCount();
	}

	readonly IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter;
}
