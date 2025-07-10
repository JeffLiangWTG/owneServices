
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.NCTS.GUI;

#if DEBUG
class ClickableIrildesRequestContext : ClickableContext
#else
sealed class ClickableIrildesRequestContext : ClickableContext
#endif
{
	public ClickableIrildesRequestContext(NctsHeader header) : base(header)
	{
	}

	#region ClickableContext

	public override ResourceString Caption => ResString.GetMultilingualString("54200F3E-2E07-4A11-91C6-50D68A882703", "IRILDES Request");

	public override string Name => "IrildesRequest";

	public override bool Visible => IsPhase5DepartureHeader();

	public override bool Enabled => HeaderHasBothMrnAndReleaseCode();

	public override void Execute(IClickableItem clickableItem)
	{
		var nctsHeader = Header;
		if (nctsHeader is null)
		{
			return;
		}

		SendIrildesRequest(nctsHeader);
	}

	#endregion

	#region Implementation

	bool HeaderHasBothMrnAndReleaseCode()
	{
		var nctsHeader = Header;
		return nctsHeader is not null
			&& !nctsHeader.MovementReferenceNumber.IsEmpty
			&& nctsHeader.EntryNumbersProvider.ReleaseInfo != null;
	}

	void SendIrildesRequest(NctsHeader nctsHeader)
	{
		try
		{
			GetNewIrildesRequestMessageFactory().CreateMessage(nctsHeader);
			Globals.Message.Show(IrildesRequestSentMessageConfirmation);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	ResourceString IrildesRequestSentMessageConfirmation => ResString.GetMultilingualString("F8C39C2D-B78B-458D-9BBC-85D482916CA2", "IRILDES Request has been sent to customs.");

#if DEBUG
	protected virtual IrildesRequestMessageFactory GetNewIrildesRequestMessageFactory()
#else
	IrildesRequestMessageFactory GetNewIrildesRequestMessageFactory()
#endif
	{
		return new IrildesRequestMessageFactory();
	}

	#endregion
}
