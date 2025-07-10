using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionHeader : AutoAccCommissionHeader, IAccCommissionHeader
	{
		public AccCommissionHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region Source

		public override AccTransactionHeader Source
		{
			get { return Factory.Load<TransactionHeader>(CH0_AH_Source); } // to use TransactionHeaderTypeDecider
		}

		#endregion

		#region CH0_GroupingSourceID / CH0_GroupingSourceTableCode

		public ZString GroupingSourceUniqueId
		{
			get
			{
				if (CH0_GroupingSourceTableCode == JobHeaderSchema.Constants.Prefix)
				{
					return $"{CH0_GC}-{CH0_JobNumber}";
				}
				else if (CH0_GroupingSourceTableCode == AccTransactionHeaderSchema.Constants.Prefix)
				{
					return $"{CH0_GroupingSourceID}";
				}
				return ZString.Empty;
			}
		}

		public ZString GroupingSourceNumber
		{
			get
			{
				if (CH0_GroupingSourceTableCode == JobHeaderSchema.Constants.Prefix)
				{
					return CH0_JobNumber;
				}
				else if (CH0_GroupingSourceTableCode == AccTransactionHeaderSchema.Constants.Prefix)
				{
					var transaction = Factory.Load<TransactionHeader>(CH0_GroupingSourceID);
					return transaction != null ? transaction.AH_TransactionNum : ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		public JobHeader GroupingSourceJob
		{
			get
			{
				if (CH0_GroupingSourceTableCode == JobHeaderSchema.Constants.Prefix)
				{
					return Factory.Load<JobHeader>(CH0_GroupingSourceID);
				}

				return null;
			}
		}

		public ICommissionableTransaction GroupingSourceTransaction
		{
			get
			{
				if (CH0_GroupingSourceTableCode == AccTransactionHeaderSchema.Constants.Prefix)
				{
					return Factory.Load<TransactionHeader>(CH0_GroupingSourceID) as ICommissionableTransaction;
				}

				return null;
			}
		}

		#endregion

		#region CH0_SnapshotEventDescription

		public ZString CH0_SnapshotEventDescription
		{
			get { return new AccCommissionHeaderSnapshotEventList().GetDescriptionFromCode(CH0_SnapshotEventCode); }
		}

		#endregion

		#endregion

		#region Override

		public ZBool IsOverriden
		{
			get { return !CH0_OverridenDateTimeUtc.IsEmpty; }
		}

		public void MarkAsOverriden(bool isAlreadyReversed = false)
		{
			if (!IsOverriden)
			{
				CH0_OverridenDateTimeUtc = ZDateTime.UtcNow;

				foreach (var line in Lines.ToArray())
				{
					line.MarkAsOverriden(isAlreadyReversed);
				}

				foreach (var lineGroup in LineGroups.ToArray())
				{
					lineGroup.MarkAsOverriden(isAlreadyReversed);
				}
			}
		}

		#endregion

		#region Related Business Objects

		public AccCommissionLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new AccCommissionLineCollection(this);
				}

				return lines;
			}
		}
		AccCommissionLineCollection lines;

		IEnumerable<IAccCommissionLine> IAccCommissionHeader.Lines => Lines.Cast<IAccCommissionLine>();

		public AccCommissionLineGroupCollection LineGroups
		{
			get
			{
				if (lineGroups == null)
				{
					lineGroups = new AccCommissionLineGroupCollection(this);
				}

				return lineGroups;
			}
		}
		AccCommissionLineGroupCollection lineGroups;

		IEnumerable<IAccCommissionLineGroup> IAccCommissionHeader.LineGroups => LineGroups.Cast<IAccCommissionLineGroup>();

		#endregion

		#region Delete

		public override void Delete()
		{
			DeleteRelatedBusinessObjects();
			base.Delete();
		}

		void DeleteRelatedBusinessObjects()
		{
			Lines.DeleteAll();
			LineGroups.DeleteAll();
		}

		#endregion

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (!CH0_GC.IsValid)
			{
				CH0_GC = GlbCompany.CurrentCompany.PK;
			}

			if (!CH0_AH_Source.IsValid)
			{
				CH0_AH_Source = Factory.NewWithValidTestData<ARInvoice>().PK;
			}

			if (!CH0_GroupingSourceID.IsValid && CH0_GroupingSourceTableCode.IsEmpty)
			{
				CH0_GroupingSourceID = CH0_AH_Source;
				CH0_GroupingSourceTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);

			if (CH0_CommissionDate.IsEmpty)
			{
				CH0_CommissionDate = new ZDate(2000, 1, 1);
			}

			if (CH0_SnapshotEventCode.IsEmpty)
			{
				CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Posted;
			}
		}

#endif
		#endregion
	}
}
