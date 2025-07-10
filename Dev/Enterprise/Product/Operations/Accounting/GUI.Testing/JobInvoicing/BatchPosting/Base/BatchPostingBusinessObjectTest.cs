using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.Testing
{
	[TestedType(typeof(BatchPostingBusinessObject))]
	public class BatchPostingBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestObjectsPostedReadOnly()
		{
			AssertEquals("Must be readonly for the gui", true, BO.ObjectsPostedInfo.ReadOnly);
		}

		public void TestRecordAddedUpdatedCounts()
		{
			OrgHeader bizObjInDB = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			OrgHeader bizObjNotInDB = Factory.NewWithValidTestData<OrgHeader>();

			DataImporterBusinessObject importBO = new DataImporterBusinessObject(Factory);
			AssertEquals("No records added initially for test", 0, importBO.RecordsAdded);
			AssertEquals("No records updated initially for test", 0, importBO.RecordsUpdated);

			importBO.Notify(new BusinessObjectCreatedOrUpdatedNotification(bizObjNotInDB));
			AssertEquals("Record added", 1, importBO.RecordsAdded);
			AssertEquals("No records updated initially", 0, importBO.RecordsUpdated);
			importBO.Notify(new BusinessObjectCreatedOrUpdatedNotification(bizObjInDB));
			AssertEquals("Record still added", 1, importBO.RecordsAdded);
			AssertEquals("Record updated", 1, importBO.RecordsUpdated);
		}

		public void TestObjectsPostedLabelTextStripsS()
		{
			AssertEquals("ABCS Posted:", GetObjectWithName("abc").ObjectsPostedLabelText);
			AssertEquals("ABCS Posted:", GetObjectWithName("abcs").ObjectsPostedLabelText);
			AssertEquals("SHIPMENTS Posted:", GetObjectWithName("shipments").ObjectsPostedLabelText);
			AssertEquals("SHIPMENTS Posted:", GetObjectWithName("shipment").ObjectsPostedLabelText);
		}

		public void TestPostOperationDescription()
		{
			Assert(GetObjectWithName("abc").PostOperationDescription_ForTestOnly.Contains("abcs"));
			Assert(!GetObjectWithName("abc").PostOperationDescription_ForTestOnly.Contains("abcss"));

			Assert(GetObjectWithName("abcs").PostOperationDescription_ForTestOnly.Contains("abcs"));
			Assert(!GetObjectWithName("abcs").PostOperationDescription_ForTestOnly.Contains("abcss"));

			Assert(GetObjectWithName("shipments").PostOperationDescription_ForTestOnly.Contains("shipments"));
			Assert(!GetObjectWithName("shipments").PostOperationDescription_ForTestOnly.Contains("shipmentss"));

			Assert(GetObjectWithName("shipment").PostOperationDescription_ForTestOnly.Contains("shipments"));
			Assert(!GetObjectWithName("shipment").PostOperationDescription_ForTestOnly.Contains("shipmentss"));
		}

		BatchPostingBusinessObject GetObjectWithName(string name)
		{
			return new BatchPostingBusinessObject(new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, System.Array.Empty<Job>(), "All", name));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BatchPostingBusinessObject(GetGUIWrapper());
		}

		BaseBatchInvoicingPostManagerGUIWrapper GetGUIWrapper()
		{
			return new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, System.Array.Empty<Job>(), "All", "");
		}

		BatchPostingBusinessObject BO
		{
			get { return base.CachedBusinessObject as BatchPostingBusinessObject; }
		}
		#endregion
	}
}
