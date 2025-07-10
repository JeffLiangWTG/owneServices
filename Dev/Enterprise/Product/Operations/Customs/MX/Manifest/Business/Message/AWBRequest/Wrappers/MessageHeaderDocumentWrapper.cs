using System;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class MessageHeaderDocumentWrapper : IMessageHeaderDocument
	{
		internal MessageHeaderDocumentWrapper(AsycudaBill bill, string action)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.action = action;
		}
		readonly AsycudaBill bill;
		readonly string action;

		string IMessageHeaderDocument.ID => bill.ABL_SequenceNumber.ToString("00000");

		string IMessageHeaderDocument.Name => AWBRequestConstants.MessageName;

		string IMessageHeaderDocument.TypeCode => AWBRequestConstants.TypeCodeDefault;

		DateTime IMessageHeaderDocument.IssueDateTime => MXMessageHelper.SafeDateTime(ZDateTime.Now);

		string IMessageHeaderDocument.PurposeCode
		{
			get
			{
				switch (action)
				{
					case MessageSubTypeCodes.Codes.Change:
						return MessageActions.Amendment;
					case MessageSubTypeCodes.Codes.Original:
						return MessageActions.New;
					default:
						return MessageActions.Delete;
				}
			}
		}

		string IMessageHeaderDocument.VersionID => AWBRequestConstants.MessageVersion;

		string IMessageHeaderDocument.SenderPartyPrimaryID => GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
	}
}
