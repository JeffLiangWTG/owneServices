using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send
{
	public class DDSendImpMessageWrapper : IDeclarationImportExport
	{
		public DDSendImpMessageWrapper(DeltaGJobDeclarationMessageSendingObject messageToSend, EU.Business.ErrorCollector errorCollector)
		{
			this.messageToSend = Argument.NotNull(messageToSend, nameof(messageToSend));
			itemErrorCollector = Argument.NotNull(errorCollector, nameof(errorCollector));
			entryHeader = messageToSend.Header;
			var mergedLines = entryHeader.MergedLines;
			Argument.GreaterThan(mergedLines.Count, 0,  (NoResString)"CustEntryHeader merged lines should exist");
		}

		public IMetaData MetaData => GetMetaDataWrapper(entryHeader);

		public IMessageEnvelope MessageEnvelope => messageEnvelope ?? (messageEnvelope = new EntryMessageEnvelopeWrapper(entryHeader, MessageSubTypeList.Codes.IMD));
		MessageEnvelopeWrapper messageEnvelope;

		public IHeader Header => GetHeaderWrapper();

		public ICusProcedure CusProcedure => ProcedureWrapperManager.NewProcedureWrapper(entryHeader, Header.ActionCode, messageToSend.DateMessage, itemErrorCollector);

		public IEnumerable<IArticle> Articles => SendWrapperHelper.GetArticles(entryHeader, itemErrorCollector);

		public IHeader LiquidationHeader => null;

		public IEnumerable<ILiquidationItem> Liquidation => SendWrapperHelper.GetLiquidation(entryHeader, false);

		public ZString MessageType => messageToSend.MessageType;

		public ZDateTime MessageDate => messageToSend.DateMessage;

		public ZBool HasIntoWarehouseProcedure => GetHasIntoWarehouseProcedure();

		public ZBool IsOfficeOfLodgementDifferentFromOfficeOfExit => entryHeader.Declaration.IsOfficeOfLodgementDifferentFromOfficeOfExit;

		#region Methods

		IMetaData GetMetaDataWrapper(CusEntryHeader cusEntryHeader)
		{
			return new MetaDataWrapper(cusEntryHeader, MessageSubTypeList.Codes.IMD);
		}

		IHeader GetHeaderWrapper()
		{
			return new HeaderWrapper(messageToSend);
		}

		ZBool GetHasIntoWarehouseProcedure()
		{
			return entryHeader.InvoiceLines?.OfType<JobComInvoiceLine>().Any(i => i.HasIntoWarehouseProcedure) ?? ZBool.False;
		}
		#endregion

		readonly CusEntryHeader entryHeader;
		readonly DeltaGJobDeclarationMessageSendingObject messageToSend;
		readonly EU.Business.ErrorCollector itemErrorCollector;
	}
}
