using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals
{
	public class WIPAccrualReversingTransactionExportFilter : WIPAccrualTransactionExportFilterBase
	{
		public WIPAccrualReversingTransactionExportFilter(BusinessObjectFactory factory,	 TransactionExportFilterProvider filterProvider) : base(factory, filterProvider)
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
				return FilterProvider.AtLeastOneTypeOfWIPAccrualReversingIsSelected;
			}
		}

		protected override void AddAdditionalFilter(ZDBOnlyQuery dBOnlyQuery)
		{
			dBOnlyQuery.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.NotEqual, null);
		}

		protected override string GetGenExportBatchSequenceType()
		{
			return Constants.DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport;
		}

		protected override bool IncludeWIPs
		{
			get { return FilterProvider.IncludeWIPsReversing; }
		}

		protected override bool IncludeAccruals
		{
			get { return FilterProvider.IncludeAccrualsReversing; }
		}

		protected override SchemaDateTimeColumn PostOrReverseDate
		{
			get { return AccTransactionLinesSchema.AL_ReverseDate; }
		}

		#endregion
	}
}
