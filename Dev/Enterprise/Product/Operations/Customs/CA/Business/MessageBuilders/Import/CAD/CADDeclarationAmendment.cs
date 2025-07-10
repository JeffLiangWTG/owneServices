using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationAmendment : ICADMessageDeclarationAmendment
{
	CADDeclarationAmendment(IEnumerable<CADCorrectionMessageSendingAction> actions)
	{
		this.firstAction = actions.FirstOrDefault();
		this.actions = actions;
	}
	readonly CADCorrectionMessageSendingAction firstAction;
	readonly IEnumerable<CADCorrectionMessageSendingAction> actions;

	public static IEnumerable<CADDeclarationAmendment> GenerateAmendmentsBySendingActions(CADCorrectionMessageSendingActionCollection sendingActions)
	{
		if (sendingActions == null || !sendingActions.Any())
		{
			return Enumerable.Empty<CADDeclarationAmendment>();
		}
		return sendingActions.OfType<CADCorrectionMessageSendingAction>()
			.GroupBy(x => new { x.CSI_Code, x.CSI_SubType })
			.Select(group => group
				.DistinctBy(x => new { x.InvoiceSequence, x.InvoiceLineSequence })
			)
			.Select(x => new CADDeclarationAmendment(x));
	}

	#region ICADMessageDeclarationAmendment

	string ICADMessageDeclarationAmendment.ChangeReasonCode => firstAction.CSI_Code;

	IEnumerable<ICADMessageDeclarationAdditionalInformation> ICADMessageDeclarationAmendment.AdditionalInformation
	{
		get
		{
			yield return new CADDeclarationAdditionalInformation(statementTypeCode: "CHG", statementDescription: firstAction.CSI_Description);
			yield return new CADDeclarationAdditionalInformation(statementCode: firstAction.CSI_SubType, statementTypeCode: "APC");
		}
	}

	IEnumerable<string> ICADMessageDeclarationAmendment.Pointer
	{
		get
		{
			foreach (var action in actions)
			{
				yield return ZString.Format("DocumentMetaData/Declaration/GoodsShipment[{0}]/GovernmentAgencyGoodsItem/Commodity[{1}]", action.InvoiceSequence, action.InvoiceLineSequence);
			}
		}
	}

	public static (ZString invoiceSequence, ZString lineSequence) DeserializePointer(string pointer)
	{
		ZString invoiceSequence = ZString.Empty, lineSequence = ZString.Empty;
		var match = Regex.Match(pointer, @"\[(\d+)\]/GovernmentAgencyGoodsItem/Commodity\[(\d+)\]");
		if (match.Success && match.Groups.Count == 3)
		{
			invoiceSequence = match.Groups[1].Value;
			lineSequence = match.Groups[2].Value;
		}
		return (invoiceSequence, lineSequence);
	}
	#endregion
}
