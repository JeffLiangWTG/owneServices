using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.ELG
{
	public class SagARExporter : SagAccountsExporter
	{
		public SagARExporter(int batchNumber, BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(batchNumber, factory, notifications)
		{
		}

		public override ZString FileNamePrefix
		{
			get { return "AR_" + GlbBranch.CurrentBranch.Company.GC_Code; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Sage Accounts Receivable Data Export"; }
		}

		protected override AccountingFlatFileConverter Converter
		{
			get { return converter ?? (converter = new SagARConverter(Factory, notifications)); }
		}
		SagARConverter converter;

		protected override string CurrentLedgerType
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}
	}
}
