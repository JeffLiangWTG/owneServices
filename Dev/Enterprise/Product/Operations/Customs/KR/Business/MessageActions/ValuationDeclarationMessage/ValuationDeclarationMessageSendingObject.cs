using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationDeclarationMessageSendingObject : JobDeclarationMiscMessageSendingObject
	{
		public ValuationDeclarationMessageSendingObject(CusEntryHeader entry) : this(entry, ElectronicDocumentTypeList.Codes._934, MessageFunctions.MessageFunctionCode.Original, null)
		{
		}
		protected ValuationDeclarationMessageSendingObject(CusEntryHeader entry, string messageType, MessageFunctions.MessageFunctionCode functionCode, Func<CusEntryLine, bool> entryLineFilter) : base(entry, messageType, functionCode, entryLineFilter)
		{
		}

		public JobComInvoiceHeader InvoiceHeader => Header.RandomHeader;

		[ResourceStringData("8199B45B-AE18-464F-B7EB-1B2C49E96005", Caption = "Valuation Code")]
		public ZString ValuationCode => InvoiceHeader?.JZ_ValuationCode ?? ZString.Empty;
	}
}
