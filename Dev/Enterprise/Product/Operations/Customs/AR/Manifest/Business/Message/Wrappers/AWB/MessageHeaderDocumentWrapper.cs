using System;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class MessageHeaderDocumentWrapper : IMessageHeaderDocument
	{
		internal MessageHeaderDocumentWrapper(ZString action)
		{
			this.action = action;
		}
		readonly ZString action;

		string IMessageHeaderDocument.ID => ARMessage.MessageNumberPlaceHolder;

		string IMessageHeaderDocument.Name => ARAWBMessageConstants.MessageName;

		public string TypeCode => ARAWBMessageConstants.TypeCodeDefault;

		DateTime IMessageHeaderDocument.IssueDateTime => ARHelperClass.SafeDateTime(ZDateTime.Now);

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

		string IMessageHeaderDocument.VersionID => ARAWBMessageConstants.MessageVersion;

		string IMessageHeaderDocument.SenderPartyID => GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
	}
}
