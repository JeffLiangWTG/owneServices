using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.ELG
{
	public class SagAPExporter : SagAccountsExporter
	{
		public SagAPExporter(int batchNumber, BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(batchNumber, factory, notifications)
		{
		}

		public override ZString FileNamePrefix
		{
			get { return "AP_" + GlbBranch.CurrentBranch.Company.GC_Code; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Sage Accounts Payable Data Export"; }
		}

		protected override AccountingFlatFileConverter Converter
		{
			get { return converter ?? (converter = new SagAPConverter(Factory, notifications)); }
		}
		SagAPConverter converter;

		protected override string CurrentLedgerType
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}
	}
}
