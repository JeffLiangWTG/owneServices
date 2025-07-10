using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.Module
{
	public abstract class BaseSendCatalogApplicator : OperationalActionMethodApplicator
	{
		public BaseSendCatalogApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		public override void Apply(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (Globals.Message.Show(ConfirmationMessage, ConfirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
			{
				ApplyCore(log, targets);
			}
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			CreateMessageSender(targets.Cast<CusGoodsCatalog>(), log).SendMessagesAndInterchange();
		}

		protected abstract BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log);

		protected abstract string ConfirmationCaption { get; }

		protected abstract string ConfirmationMessage { get; }
	}
}
