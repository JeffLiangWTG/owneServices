using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageDefinitions.TemporaryStorage.TEMPORARY_STORAGE_OUTPUT;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using IXmlCustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.ITemporaryStorageResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class NewTemporaryStorageResponseMessageSubProcessor
{
	public void ProcessMessage(IXmlCustomsLinkedObjectAdapter adapter, IXmlCustomsResponseMessage responseMessageWrapper)
	{
		Argument.NotNull(adapter, nameof(adapter));
		Argument.NotNull(responseMessageWrapper, nameof(responseMessageWrapper));

		var billsInformation = responseMessageWrapper.BillInformation;

		var billsWithPositiveOutcomes = billsInformation.Where(b => b.Esito == nameof(EsitoOperazioneType.P)).ToList();

		if (billsWithPositiveOutcomes.Count == 0)
		{
			return;
		}

		var latestMrnIssueDate = ZDateTime.Empty;

		foreach (var billInfo in billsWithPositiveOutcomes)
		{
			var lrn = billInfo.Lrn;
			var firstRegInfo = billInfo.RegistrationInformation.FirstOrDefault();
			var mrn = firstRegInfo?.Mrn;
			var issueDate = firstRegInfo?.RegistrationDate ?? ZDateTime.Now;

			if (!string.IsNullOrWhiteSpace(mrn))
			{
				_ = adapter.UpdateOrInsertEntryNumber(
					CusEntryNumberConstants.EntryTypes.Mrn,
					mrn,
					lrn,
					issueDate,
					null);

				if (latestMrnIssueDate.IsEmpty || issueDate > latestMrnIssueDate)
				{
					latestMrnIssueDate = issueDate;
				}
			}

			foreach (var regInfo in billInfo.RegistrationInformation)
			{
				var reg = $"{regInfo.RegisterCode}-{regInfo.RegistrationNumber}{regInfo.CinIdentifier}";
				if (int.TryParse(regInfo.Item, out var lineNumber))
				{
					_ = adapter.UpdateOrInsertEntryNumber(
						CusEntryNumberConstants.EntryTypes.RegistrationNumber,
						reg,
						lrn,
						issueDate,
						lineNumber);
				}
			}
		}

		adapter.SetStatusAsRegistered(acceptanceDate: latestMrnIssueDate);
	}
}
