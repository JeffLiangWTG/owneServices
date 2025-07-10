using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public abstract class UniqueTransactionIdentifierGridContextMenuItemComponent<T> : GridContextMenuItemComponent<ITEDIMessage>
	where T : BusinessObject, ITopLevelBusinessObjectProvider
{
	protected UniqueTransactionIdentifierGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
		parentLazy = new Lazy<T>(() => (T)DataContext.EM_LinkedObject);
	}

	sealed protected override ITEDIMessage GetDataContext()
	{
		return Grid.GetCurrent() as ITEDIMessage;
	}

	sealed protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return base.IsMenuItemVisible(menuItem)
			&& DataContext is ITEDIMessage message
			&& message.EM_MessageType == MessageProcessorConstants.InterchangeTypes.Ucc6AcknowledgementType;
	}

	sealed protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("3F184894-0475-4B15-A8B4-932C8C158ACD", "Request Responses");
		return new ZMenuItem(menuItemText) { Name = "RequestResponsesMenuItem" };
	}

	sealed protected override void Execute(ZMenuItem menuItem)
	{
		var dataContext = DataContext;

		if (!IsFormPreSaved(Parent, menuItem))
		{
			return;
		}

		var uniqueTransactionIdentifier = GetUniqueTransactionIdentifier(dataContext);
		if (uniqueTransactionIdentifier.IsEmpty)
		{
			Globals.Message.ShowError(TheRequestCannotBeProcessedAsTheUniqueTransactionIdentifierCannotBeFound);
			return;
		}

		if (Globals.Message.Show(DoYouConfirmToRequestResponsesForMessage(dataContext.EM_MessageNum), DoYouConfirmToRequestResponsesForMessageCaption, MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
		{
			return;
		}

		try
		{
			var factory = new BusinessObjectFactory();

			var messageCreationContext = GetMessageRequestContext(new GlbCertificateProvider(), uniqueTransactionIdentifier, dataContext.EM_MessageNum);
			var messageCreationStrategy = (IOutgoingCustomsMessageCreationStrategy)new UniqueTransactionIdentifierRequestMessageCreationStrategy(factory, messageCreationContext);
			var generatedMessage = messageCreationStrategy.GenerateMessage();
			AddMessageToBusinessObject(generatedMessage);

			factory.Save();
			Globals.Message.Show(TheRequestMessageHasBeenCreated);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	protected ZString GetUniqueTransactionIdentifier(ITEDIMessage message)
	{
		return UniqueTransactionIdentifierTextExtractor
			.ExtractUniqueTransactionIdentifier(message.EM_MessageText);
	}

	protected abstract IUniqueTransactionIdentifierRequestContext GetMessageRequestContext(GlbCertificateProvider certificateProvider, ZString identifier, ZString messageNum);

	protected abstract void AddMessageToBusinessObject(ITEDIMessage ediMessage);

	protected static string TheRequestCannotBeProcessedAsTheUniqueTransactionIdentifierCannotBeFound => Res.GetString("083534CC-F886-422D-A022-170748F8866F", "The request cannot be processed as the unique transaction identifier (IUT) cannot be found.");

	protected static string DoYouConfirmToRequestResponsesForMessage(string messageNum) => Res.GetString("8AAA1437-ADC0-4BF8-AEEB-38179AE40371", "Do you confirm to request responses for message {0}?", messageNum);

	protected static string DoYouConfirmToRequestResponsesForMessageCaption => Res.GetString("ABC24C61-A4B0-4FCF-94F6-A068267AACFF", "Request Confirmation");

	protected static string TheRequestMessageHasBeenCreated => Res.GetString("30D97341-0951-4D3E-A2EC-C45B27C9BCCD", "The request message has been created.");

	readonly Lazy<T> parentLazy;
	protected T Parent => parentLazy.Value;
}
