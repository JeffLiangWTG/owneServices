using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using IXmlCustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class ReleaseItemsHandler : IResponseMessageHandler
{
	public ReleaseItemsHandler(IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter)
	{
		this.customsLinkedObjectAdapter = Argument.NotNull(customsLinkedObjectAdapter, nameof(customsLinkedObjectAdapter));
	}

	readonly IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter;

	public void Handle(IXmlCustomsResponseMessage responseMessage)
	{
		Argument.NotNull(responseMessage, nameof(responseMessage));

		var releaseItems = responseMessage.ReleaseItems;

		if (releaseItems.Count == 0)
		{
			return;
		}

		var headerReleaseItem = releaseItems.HeaderItem;
		if (HasValidReferenceNumber(headerReleaseItem))
		{
			ProcessReleaseItemAtParentLevel(headerReleaseItem);
		}
		else
		{
			ProcessAllReleaseItems(releaseItems);
			DetermineEntryHasFullyReleased();
		}
	}

	#region Implementation

	void DetermineEntryHasFullyReleased()
	{
		var allEntryLines = customsLinkedObjectAdapter.GetAllEntryLines().ToArray();
		if (allEntryLines.Length > 0 && allEntryLines.All(HasClearanceCode))
		{
			customsLinkedObjectAdapter.SetStatusAsCleared(ZDateTime.Empty);
		}

		bool HasClearanceCode(BusinessObject parent)
			=> CusEntryNumber.Load(parent, CusEntryNumberConstants.EntryTypes.ClereanceCode, Core.Constants.CountryCodes.Italy) != null;
	}

	void ProcessReleaseItemAtParentLevel(IReleaseInformation item)
	{
		ManageReleaseItem(item);
		customsLinkedObjectAdapter.SetStatusAsCleared(item.ReleaseDate ?? ZDateTime.Now);
	}

	void ProcessAllReleaseItems(IReadOnlyCollection<IReleaseInformation> items)
	{
		items.ForEach(ManageReleaseItem);
	}

	void ManageReleaseItem(IReleaseInformation releaseItem)
	{
		if (!HasValidReferenceNumber(releaseItem))
		{
			return;
		}

		var issueDate = releaseItem.ReleaseDate ?? ZDateTime.Now;
		customsLinkedObjectAdapter.UpdateOrInsertEntryNumber(CusEntryNumberConstants.EntryTypes.ClereanceCode,
			releaseItem.ReferenceNumber,
			string.Empty,
			issueDate,
			releaseItem.ItemNumber);
	}

	bool HasValidReferenceNumber(IReleaseInformation releaseItem) => !string.IsNullOrEmpty(releaseItem?.ReferenceNumber);

	#endregion
}
