using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.OrgCollectionCalls;
using Enterprise.Accounting.GUI.OrgCollectionCalls;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI
{
	public class CollectionCallsTransactionsPrintingPlugIn : ZPlugIn
	{
		public CollectionCallsTransactionsPrintingPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		#region Overrides

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return null; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			CollectionCallsTransactionsPrintingControl result = new CollectionCallsTransactionsPrintingControl();
			result.Dock = DockStyle.Fill;
			return result;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return TransactionsFilter;
		}

		public override string Name
		{
			get { return (NoResString)"Transactions"; } // Hard-coded constant
		}

		#endregion

		CollectionNotesTransactionsFilter TransactionsFilter
		{
			get
			{
				if (fTransactionsFilter == null)
				{
					fTransactionsFilter = new CollectionNotesTransactionsFilter((OrgHeader)HostBusinessEntity, GlbBranch.CurrentBranch);
				}

				return fTransactionsFilter;
			}
		}
		CollectionNotesTransactionsFilter fTransactionsFilter;
	}
}
