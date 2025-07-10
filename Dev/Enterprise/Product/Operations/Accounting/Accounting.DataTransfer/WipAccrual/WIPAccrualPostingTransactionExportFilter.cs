using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals
{
	public class WIPAccrualPostingTransactionExportFilter : WIPAccrualTransactionExportFilterBase
	{
		public WIPAccrualPostingTransactionExportFilter(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider) : base(factory, filterProvider)
		{
		}

		#region Implementation

		protected override Type BusinessObjectTypeCore
		{
			get
			{
				return typeof(BaseWIPAccrual);
			}
		}

		public override SchemaDateTimeColumn SystemLastEditTimeColumn
		{
			get { return AccTransactionLinesSchema.AL_SystemLastEditTimeUtc; }
		}

		protected override bool AtLeastOneTypeOfTransactionIsSelected
		{
			get
			{
				return FilterProvider.AtLeastOneTypeOfWIPAccrualPostingIsSelected;
			}
		}

		protected override string GetGenExportBatchSequenceType()
		{
			return Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport;
		}

		protected override bool IncludeWIPs
		{
			get { return FilterProvider.IncludeWIPsPosting; }
		}

		protected override bool IncludeAccruals
		{
			get { return FilterProvider.IncludeAccrualsPosting; }
		}

		protected override SchemaDateTimeColumn PostOrReverseDate
		{
			get { return AccTransactionLinesSchema.AL_PostDate; }
		}

		#endregion
	}
}
