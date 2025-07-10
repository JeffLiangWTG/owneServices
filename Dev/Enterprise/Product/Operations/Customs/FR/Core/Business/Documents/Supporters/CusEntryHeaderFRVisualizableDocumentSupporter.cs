using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using ForwardingDataContext = Enterprise.Freight.Forwarding.Documents.DataContext;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class CusEntryHeaderFRVisualizableDocumentSupporter : CusEntryHeaderEUVisualizableDocumentSupporter
	{
		public CusEntryHeaderFRVisualizableDocumentSupporter(Declaration.CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected new Declaration.CusEntryHeader parent => (Declaration.CusEntryHeader)base.parent;

		protected override string GetDocProviderKey() => Core.Constants.CountryCodes.France;

		protected override IEnumerable<ICommand> GetCustomCommandsCore(string dataContext)
		{
			var commands = base.GetCustomCommandsCore(dataContext).ToList();
			switch (dataContext)
			{
				case DataContext.FRPortsCustomsCheckCAED:
					commands.Add(DisabledCommand.SendWithdrawal);
					commands.Add(new SendNativePortMessageOriginal());
					break;
			}
			return commands;
		}

		public override IMessageLogCreator GetMessageLogCreator(IDocument document)
		{
			switch (document.DataContext)
			{
				case ForwardingDataContext.FRPortsTrackingRequestTRC:
					return new DemandeDeTracingMessageLogCreator(parent.TRCDetailsProvider);
				default:
					return base.GetMessageLogCreator(document);
			}
		}
	}
}
