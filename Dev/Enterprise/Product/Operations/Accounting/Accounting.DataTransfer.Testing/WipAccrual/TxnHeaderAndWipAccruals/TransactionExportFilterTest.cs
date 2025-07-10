using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class TransactionExportFilterTest : TransactionExportFilterTestBase
	{
		public new void TestGetFilterPks()
		{
			Assert("The test is not suitable here.", true);
		}

		public void TestNumberOfObjects()
		{
			int numberOfObjectsInDb = Factory.GetDatabaseCount(typeof(StmALog));
			TransactionExportFilterTestImplementation exportFilter = new TransactionExportFilterTestImplementation(Factory, FilterProvider, typeof(StmALog));
			exportFilter.SetAtLeastOneTypeOfTransactionIsSelected(false);
			AssertEquals("NumberOfObjects when creating a new batch and no transaction types are selected.", 0, exportFilter.NumberOfObjects);

			exportFilter = new TransactionExportFilterTestImplementation(Factory, FilterProvider, typeof(StmALog));
			exportFilter.SetAtLeastOneTypeOfTransactionIsSelected(true);
			AssertEquals(numberOfObjectsInDb, exportFilter.NumberOfObjects);
		}

		public void TestBatchNumberSetOnFilterIsSetOnFilterProvider()
		{
			TransactionExportFilterProvider filterProvider = new TransactionExportFilterProvider(Factory);
			TransactionExportFilterTestImplementation filter = new TransactionExportFilterTestImplementation(Factory, filterProvider, typeof(Object));

			AssertEquals(0, filter.BatchNumber);
			filterProvider.CurrentBatchNo = 500;
			AssertEquals(500, filter.BatchNumber);
		}

		public new void TestAddHighWaterMarkFilterIfApplicable()
		{
			Assert("The test is not suitable here.", true);
		}

		public new void TestSystemLastEditTimeColumn()
		{
			Assert("The test is not suitable here.", true);
		}

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { throw new NotImplementedException(); }
		}

		#region Implementation

		protected override TransactionExportFilter GetNewExportFilter()
		{
			throw new NotImplementedException();
		}

		class TransactionExportFilterTestImplementation : TransactionExportFilter
		{
			public TransactionExportFilterTestImplementation(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider, Type businessObjectType) : base(factory, filterProvider)
			{
				this.fBusinessObjectType = businessObjectType;
			}

			protected override Type BusinessObjectTypeCore
			{
				get { return fBusinessObjectType; }
			}

			public override SchemaDateTimeColumn SystemLastEditTimeColumn
			{
				get { throw new NotImplementedException(); }
			}

			protected override ZQuery CreateFilterForBatch()
			{
				return new ZQuery();
			}

			protected override bool AtLeastOneTypeOfTransactionIsSelected
			{
				get { return fAtLeastOneTypeOfTransactionIsSelected; }
			}

			public void SetAtLeastOneTypeOfTransactionIsSelected(bool value)
			{
				fAtLeastOneTypeOfTransactionIsSelected = value;
			}

			public new void AddOrganisationsSubQuery(ZDBOnlyQuery mainQuery, SchemaGuidColumn foreignKeyToMainTable, SchemaGuidColumn foreignKeyToBranch)
			{
				base.AddOrganisationsSubQuery(mainQuery, foreignKeyToMainTable, foreignKeyToBranch);
			}

			bool fAtLeastOneTypeOfTransactionIsSelected;

			readonly Type fBusinessObjectType;
		}

		#endregion
	}
}
