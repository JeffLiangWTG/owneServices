using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MY.Business
{
	public class JobDeclaration : TypeSafeJobDeclaration, Integration.Customs.MY.IJobDeclaration
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobDeclarationMessageManager MessageManager => new JobDeclarationMessageManager(this);

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		#region Implementation

		#region protected override

		protected override ZBool IsReciprocalRatesCore => IsReciprocalRatesConstant;

		internal static bool IsReciprocalRatesConstant => true;

		protected override ZString LocalCurrencyCodeCore => LocalCurrencyConstantCode;

		internal static ZString LocalCurrencyConstantCode => Core.Constants.CurrencyCodes.Malaysia;

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new BaseCusContainerCollection<CusContainer>(this, Factory);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override Customs.Business.JobDeclarationValidation GetNewValidation() => new JobDeclarationValidation(this);

		protected override Customs.Business.JobDeclarationLookups GetNewLookups() => new JobDeclarationLookups(this);

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection<JobComInvoiceLine>(this);

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(this, Factory);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("MY"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		#endregion

		#endregion
	}
}
