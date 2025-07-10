using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericJob.Testing
{
	[TestedType(typeof(GenericJob))]
	public class GenericJobTest : BusinessObjectBaseTestCase
	{
		public void TestJobType()
		{
			foreach (JobInvoicingConsumerType consumerType in JobInvoicingConsumerTypes.New())
			{
				var job = Factory.New<GenericJob>();
				job.VJ_JobType = consumerType.Code;
				AssertEquals(consumerType, job.JobType);
			}
		}

		public void TestNonConsumerTypeCode()
		{
			GenericJob job = Factory.New<GenericJob>();
			job.VJ_JobType = "ABC";

			AssertNull(job.GetConsumerController());
			AssertNull(job.GetConsumerType());
			AssertEquals(ZString.Empty, job.JobTypeDescription);

			AssertNull(job.JobType);
			AssertEquals("Do not report a key, otherwise problems for all job types will be collected under the same Issue. Without a key, the message can be used to determine uniqueness and so problems with different job types can be turned into separate issues",
					null, ErrorReporter.LastKeyReported);
			AssertEquals("There is no entry in JobInvoicingConsumerTypes for job type: 'ABC'. Please read the comment above JobInvoicingConsumerTypes for instructions on adding an entry.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestInvoicingPluginWithJobHeaderLoad()
		{
			BusinessObject shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			Factory.Save();

			AssertJobConsumerAndOriginalObjectEqual(shipment);

			GenericJob jobToTest = RetrieveJob(shipment.PK, shipment.TablePrefix);
			AssertNotNull(jobToTest.Job_ForTestOnly);
		}

		public void TestJobDetail()
		{
			BusinessObject shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();

			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			job.JH_Status = JobHeaderStatus.CustomsProcessActive.Code;
			job.JH_A_JOP = new ZDateTime(2005, 04, 03);
			job.JH_A_JCL = new ZDateTime(2005, 04, 13);

			Factory.Save();

			GenericJob jobToTest = RetrieveJob(shipment.PK, shipment.TablePrefix);

			AssertEquals("VJ_CompanyPK", jobToTest.VJ_CompanyPK, GlbCompany.CurrentCompany.PK);
			AssertEquals("VJ_JH", jobToTest.VJ_JH, job.PK);
			AssertEquals("VJ_Status", jobToTest.VJ_JobStatus, JobHeaderStatus.CustomsProcessActive.Code);
			AssertEquals("VJ_JobOpenDate", jobToTest.VJ_JobOpenDate, new ZDateTime(2005, 04, 03));
			AssertEquals("VJ_JobCloseDate", jobToTest.VJ_JobCloseDate, new ZDateTime(2005, 04, 13));
			AssertEquals("VJ_Company", jobToTest.VJ_Company, GlbCompany.CurrentCompany.PK);
			AssertEquals("VJ_Branch", jobToTest.VJ_Branch, GlbBranch.CurrentBranch.PK);
			AssertEquals("VJ_Department", jobToTest.VJ_Department, GlbDepartment.CurrentDepartment.PK);
			AssertEquals("VJ_ForeignKey", jobToTest.VJ_ForeignKey, shipment.PK);
		}

		public void TestInvoicingPluginWithoutJobHeaderLoad()
		{
			BusinessObject shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();

			Factory.Save();

			AssertJobConsumerAndOriginalObjectEqual(shipment);

			GenericJob jobToTest = RetrieveJob(shipment.PK, shipment.TablePrefix);
			AssertNull(jobToTest.Job_ForTestOnly);
		}

		public void TestJobConsumerTypeAndControllerIDForAllConsumers()
		{
			foreach (JobInvoicingConsumerType type in JobInvoicingConsumerTypes.New())
			{
				if (type != JobInvoicingConsumerTypes.Organisation)
				{
					BusinessObject bizObj;
					GenericJob job;

					if (type == JobInvoicingConsumerTypes.QuotedBooking)
					{
						var booking = QuotedBooking.CreateNewBooking(Factory);
						bizObj = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
						Factory.Save();

						var quotedBooking = bizObj as QuotedBooking;
						job = RetrieveJob(quotedBooking.Booking.PK, quotedBooking.Booking.TablePrefix);

						AssertJobConsumerAndOriginalObjectEqual(bizObj, true);
					}
					else
					{
						bizObj = Factory.NewWithValidTestData(type.BizoType);
						Factory.Save();

						job = RetrieveJob(bizObj.PK, bizObj.TablePrefix);

						AssertJobConsumerAndOriginalObjectEqual(bizObj, false);
					}

					AssertEquals("Expected ZController - " + bizObj.GetType().FullName, type.ControllerID, job.GetConsumerController());
				}
			}
		}

		public void TestOperationsRevenueRecognitionDate_PickupAndDeliveryDate()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting1.Mode = Core.Constants.TransportModes.Air;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;

			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			CommonShipment shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Today.AddDays(2);

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			Factory.Save();

			AssertJobConsumerAndOriginalObjectEqual(shipment);
		}

		#region IJobInvoicingPlugIn Members

		public void TestIJobInvoicingPlugIn_DefaultDebtor()
		{
			BusinessObject shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
			Factory.Save();

			JobInvoicingConsumerType consumerType = new DummyConsumerType(typeof(DummyJobParent));

			GenericJob genericJob = RetrieveJob(shipment.PK, shipment.TablePrefix);
			typeof(GenericJob).InvokeMember("fJobType", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, genericJob, new object[] { consumerType });

			IJobInvoicingPlugIn testJob = genericJob;
			AssertNull(testJob.InvoicingSupporter.GetDefaultDebtor(null));
		}

		public void TestInvoicingPluginEditSecurity()
		{
			BusinessObject shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
			Factory.Save();

			JobInvoicingConsumerType consumerType = new DummyConsumerType(typeof(DummyJobParent));

			GenericJob genericJob = RetrieveJob(shipment.PK, shipment.TablePrefix);
			typeof(GenericJob).InvokeMember("fJobType", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, genericJob, new object[] { consumerType });

			IJobInvoicingPlugIn testJob = genericJob;
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.AgencyBillContainers, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", "Boop Boop Be Doop", testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", true, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			BusinessObject shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
			Factory.Save();

			JobInvoicingConsumerType consumerType = new DummyConsumerType(typeof(DummyJobParent));

			GenericJob genericJob = RetrieveJob(shipment.PK, shipment.TablePrefix);
			typeof(GenericJob).InvokeMember("fJobType", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, genericJob, new object[] { consumerType });

			IJobInvoicingPlugIn testJob = genericJob;
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent genericJob = Factory.New<GenericJob>();
			Assert(genericJob.AllowInvoiceDeletion);
		}

		#endregion

		#region IBusinessObjectReload

		public void TestReload()
		{
			var shipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			GenericJob jobToTest = RetrieveJob(shipment.PK, shipment.TablePrefix);

			ErrorReporter.Clear();
			new BusinessObjectFactory { RefreshEnabled = false }.Load<GenericJob>(jobToTest.PK);
			AssertEquals("ZSqlLoaderViewGenericJobByPK", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
			var reloadedJob = ((IBusinessObjectReload)jobToTest).Reload(new BusinessObjectFactory { RefreshEnabled = false });
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals(jobToTest.PK, reloadedJob.PK);
		}

		#endregion

		void AssertJobConsumerAndOriginalObjectEqual(BusinessObject originalObject, bool isForQuotedBooking)
		{
			var quotedBooking = originalObject as QuotedBooking;

			var job = (isForQuotedBooking && quotedBooking != null)
				? RetrieveJob(quotedBooking.Booking.PK, quotedBooking.Booking.TablePrefix)
				: RetrieveJob(originalObject.PK, originalObject.TablePrefix);

			AssertNotNull("Job should not be null - " + originalObject.GetType().FullName, job);

			AssertEquals("Generic Job should return original object as IJobInvoicingPlugin",
				originalObject, job.Consumer);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.Consignee, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.Consignee);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.Consignor, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.Consignor);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.Broker, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.Broker);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.Origin, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.Origin);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.Destination, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.Destination);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.TransportMode, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.TransportMode);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.ConsumerType, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.ConsumerType);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate),
				((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.IsImport, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.IsImport);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.IsExport, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.IsExport);

			AssertEquals("IJobInvoicingPlugin Implementation should be equal",
				job.InvoicingSupporter.IsDomestic, ((IJobInvoicingPlugIn)originalObject).InvoicingSupporter.IsDomestic);
		}

		void AssertJobConsumerAndOriginalObjectEqual(BusinessObject originalObject)
		{
			AssertJobConsumerAndOriginalObjectEqual(originalObject, false);
		}

		GenericJob RetrieveJob(ZGuid pK, ZString tableCode)
		{
			return Factory.LoadGenericJob<GenericJob>(pK, tableCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			BusinessObject result = Factory.New(GetExpectedBusinessObjectType());
			return result;
		}

		class DummyConsumerType : JobInvoicingConsumerType
		{
			public DummyConsumerType(Type bizObjType)
				: base("blt", (NoResString)"blaticus")
			{
				this.bizObjType = bizObjType;
			}

			public override Type BizoType
			{
				get { return bizObjType; }
			}

			public override ControllerID ControllerID
			{
				get { throw new NotImplementedException(); }
			}

			public override SecurityCheckpoint DistanceCalculationCheckpoint
			{
				get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
			}

			readonly Type bizObjType;
		}
	}
}
