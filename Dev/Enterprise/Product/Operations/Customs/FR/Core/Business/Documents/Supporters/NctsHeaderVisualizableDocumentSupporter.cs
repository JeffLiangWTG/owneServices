using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class NctsHeaderVisualizableDocumentSupporter : EU.NCTS.Business.Documents.DocDataObjects.NctsHeaderVisualizableDocumentSupporter
	{
		public NctsHeaderVisualizableDocumentSupporter(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		protected override IEnumerable<ICommand> GetCustomCommandsCore(string dataContext)
		{
			var commands = base.GetCustomCommandsCore(dataContext).ToList();
			switch (dataContext)
			{
				case DataContext.FRPortsRegularizationTransitDOA:
				case DataContext.FRPortsCustomsCheckCAED:
					commands.Add(DisabledCommand.SendWithdrawal);
					commands.Add(new SendNativePortMessageOriginal());
					break;
			}
			return commands;
		}
	}
}
