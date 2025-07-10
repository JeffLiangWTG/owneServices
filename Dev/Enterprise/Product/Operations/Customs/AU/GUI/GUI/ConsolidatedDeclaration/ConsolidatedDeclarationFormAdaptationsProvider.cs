using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public class ConsolidatedDeclarationFormAdaptationsProvider : IConsolidatedDeclarationFormAdaptationsProvider
	{
		public IPanelLayoutProvider HeaderDetailsLayout => new ConsolidatedDeclarationLayoutProvider();

		public IEnumerable<ZGridColumnInfo> DeclarationGridExtraColumnInfos => Enumerable.Empty<ZGridColumnInfo>();

		public ZUserControl MessagesTabUserControl => new BaseMessagesTabUserControl();

		public IConsolidatedDeclarationMenuBuilder EDIMenuBuilder => new Declaration.GUI.EDIMenu();

		public bool EnableDocumentMenuItem => true;
	}
}
