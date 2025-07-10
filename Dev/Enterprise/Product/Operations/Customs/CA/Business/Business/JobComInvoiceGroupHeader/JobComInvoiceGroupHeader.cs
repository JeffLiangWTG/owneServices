using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobComInvoiceGroupHeader : BaseJobComInvoiceGroupHeader, ICurrencyConverterDataProvider
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportAdditionalDeclarations
		{
			get
			{
				return !JZ_JE.IsEmpty && JobDeclaration != null && JobDeclaration.IsPersistent && JobDeclaration.IsLVX;
			}
		}

		public override void Delete()
		{
			var invoiceHeaders = JobComInvoiceHeaders.Cast<JobComInvoiceHeader>();
			try
			{
				invoiceHeaders.ForEach(x => x.IsGotingToBeDeleted = true);
				base.Delete();
			}
			finally
			{
				invoiceHeaders.ForEach(x => x.IsGotingToBeDeleted = false);
			}
		}

		#region ICurrencyConverterDataProvider

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get
			{
				if (JobDeclaration != null)
				{
					return JobDeclaration.CurrencyConverterMaximumDaysToFallBack;
				}
				else
				{
					return 365;
				}
			}
		}

		#endregion

		public const string AsClaimed = "As Claimed";
	}
}
