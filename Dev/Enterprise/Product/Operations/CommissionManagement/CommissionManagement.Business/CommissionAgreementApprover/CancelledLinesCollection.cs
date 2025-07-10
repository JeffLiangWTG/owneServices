using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CancelledLinesCollection : NonPersistentBusinessObjectCollection<ExcludeViewCommissionLine>
	{
		public CancelledLinesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void AddLines(IEnumerable<ViewCommissionLine> linesToAdd)
		{
			Argument.NotNull(linesToAdd, nameof(linesToAdd));

			linesToAdd.ForEach(line => Add(new ExcludeViewCommissionLine(line)));
		}

		public void TickAll()
		{
			this.ForEach(line => ((ExcludeViewCommissionLine)line).IsExcluded = true);
		}

		public void UntickAll()
		{
			this.ForEach(line => ((ExcludeViewCommissionLine)line).IsExcluded = false);
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("You cannot directly modify this collection.");
		}
	}

	public class ExcludeViewCommissionLine : NonPersistentBusinessObject
	{
		public ExcludeViewCommissionLine(ViewCommissionLine dataItem)
		{
			DataItem = dataItem;
			IsExcluded = true;
		}

		public ViewCommissionLine DataItem { get; }

		public ZString SourceNumber
		{
			get
			{
				switch (DataItem.VCL_GroupingSourceTableCode)
				{
					case JobHeaderSchema.Constants.Prefix:
						return DataItem.VCL_JobNumber;
					case AccTransactionHeaderSchema.Constants.Prefix:
						var transactionHeader = DataItem.Factory.Load<AccTransactionHeader>(DataItem.VCL_GroupingSourceID);
						return transactionHeader != null ? transactionHeader.AH_TransactionNum : ZString.Empty;
					default:
						return ZString.Empty;
				}
			}
		}

		public ZBool IsExcluded
		{
			get => isExcluded;
			set
			{
				if (isExcluded != value)
				{
					isExcluded = value;
					IsExcludedInfo.RefreshBinding();
				}
			}
		}
		ZBool isExcluded;

		public ZPropertyInfo IsExcludedInfo => GetZPropertyInfo(nameof(IsExcluded));
	}
}
