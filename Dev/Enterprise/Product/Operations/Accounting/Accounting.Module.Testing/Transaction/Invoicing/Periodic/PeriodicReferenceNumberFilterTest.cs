using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module.Testing
{
	public class PeriodicReferenceNumberFilterTest : TestCaseWithFactory
	{
		public void TestAdditionalReferenceFilter_ForCommonShipment()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S1");
			var shipment2 = TestObjectCreator.CreateShipment("S2");
			PrepareJobAndCharge(shipment1, InvoiceTypesList.Codes.FinalInvoice_Batching, TestObjectCreator.AALSHI, TestObjectCreator.AUD);
			PrepareJobAndCharge(shipment2, InvoiceTypesList.Codes.FinalInvoice_Batching, TestObjectCreator.AALSHI, TestObjectCreator.AUD);
			NewReferenceNumber(shipment1.Numbers, "AU", "COC", "MUNDANE");
			NewReferenceNumber(shipment2.Numbers, "US", "COC", "MAGIC");
			Factory.Save();

			var periodicInvoiceBase = new PeriodicInvoice(Factory);
			periodicInvoiceBase.CurrencyNK = "AUD";
			periodicInvoiceBase.DebtorPK = TestObjectCreator.AALSHI.PK;
			periodicInvoiceBase.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = true);

			var filter = (ReferenceNumberFilter)periodicInvoiceBase.JobsFilter["Additional Reference #"];

			SetFilter(filter, "AU", "COC", "MUNDANE");
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection to contain job1", periodicInvoiceBase.Jobs.Contains(shipment1.Job));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(shipment2.Job));

			SetFilter(filter, "US", "COC", "MAGIC");
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(shipment1.Job));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(shipment2.Job));

			SetFilter(filter, "US", "COC", "A");
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection not to contain job1", !periodicInvoiceBase.Jobs.Contains(shipment1.Job));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(shipment2.Job));

			SetFilter(filter, string.Empty, "COC", "M");
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(shipment1.Job));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(shipment2.Job));
		}

		public void TestAdditionalReferenceFilter_ForJobDeclaration()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			PrepareJobAndCharge(declaration1, InvoiceTypesList.Codes.FinalInvoice_Batching, TestObjectCreator.AALSHI, TestObjectCreator.AUD);
			PrepareJobAndCharge(declaration2, InvoiceTypesList.Codes.FinalInvoice_Batching, TestObjectCreator.AALSHI, TestObjectCreator.AUD);
			NewReferenceNumber(declaration1.AdditionalReferenceNumbers, "AU", "COC", "MUNDANE");
			NewReferenceNumber(declaration2.AdditionalReferenceNumbers, "US", "COC", "MAGIC");
			Factory.Save();

			var periodicInvoiceBase = new PeriodicInvoice(Factory);
			periodicInvoiceBase.CurrencyNK = "AUD";
			periodicInvoiceBase.DebtorPK = TestObjectCreator.AALSHI.PK;
			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var filter = (ReferenceNumberFilter)periodicInvoiceBase.JobsFilter["Additional Reference #"];

			SetFilter(filter, "AU", "COC", "MUNDANE");
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection to contain job1", periodicInvoiceBase.Jobs.Contains(declaration1.Job));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(declaration2.Job));

			SetFilter(filter, "US", "COC", "MAGIC");
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(declaration1.Job));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(declaration2.Job));

			SetFilter(filter, "US", "COC", "A");
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection not to contain job1", !periodicInvoiceBase.Jobs.Contains(declaration1.Job));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(declaration2.Job));

			SetFilter(filter, string.Empty, "COC", "M");
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(declaration1.Job));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(declaration2.Job));
		}

		public void TestAdditionalReferenceFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S1");
			var shipment2 = TestObjectCreator.CreateShipment("S2");
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			PrepareJobAndCharge(shipment1, InvoiceTypesList.Codes.FinalInvoice_Batching, TestObjectCreator.AALSHI, TestObjectCreator.AUD);
			PrepareJobAndCharge(shipment2, InvoiceTypesList.Codes.FinalInvoice_Batching, TestObjectCreator.AALSHI, TestObjectCreator.AUD);
			PrepareJobAndCharge(declaration1, InvoiceTypesList.Codes.FinalInvoice_Batching, TestObjectCreator.AALSHI, TestObjectCreator.AUD);
			PrepareJobAndCharge(declaration2, InvoiceTypesList.Codes.FinalInvoice_Batching, TestObjectCreator.AALSHI, TestObjectCreator.AUD);
			NewReferenceNumber(shipment1.Numbers, "AU", "COC", "TEST1");
			NewReferenceNumber(shipment2.Numbers, "AU", "COC", "TEST2");
			NewReferenceNumber(declaration1.AdditionalReferenceNumbers, "AU", "COC", "TEST3");
			NewReferenceNumber(declaration2.AdditionalReferenceNumbers, "AU", "COC", "TEST4");
			Factory.Save();

			var periodicInvoiceBase = new PeriodicInvoice(Factory);
			periodicInvoiceBase.CurrencyNK = "AUD";
			periodicInvoiceBase.DebtorPK = TestObjectCreator.AALSHI.PK;
			periodicInvoiceBase.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var filter = (ReferenceNumberFilter)periodicInvoiceBase.JobsFilter["Additional Reference #"];
			SetFilter(filter, "AU", "COC", "TEST");

			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = false);
			periodicInvoiceBase.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)).Value = true;
			periodicInvoiceBase.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).Value = true;
			periodicInvoiceBase.LoadJobs();
			AssertContainsExactElementsInAnyOrder(new List<ZGuid> { shipment1.Job.PK, shipment2.Job.PK, declaration1.Job.PK, declaration2.Job.PK }, periodicInvoiceBase.Jobs.OfType<PeriodicInvoiceSelectableJob>().Select(x => x.Parent.PK));

			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = false);
			periodicInvoiceBase.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)).Value = true;
			periodicInvoiceBase.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).Value = false;
			periodicInvoiceBase.LoadJobs();
			AssertContainsExactElementsInAnyOrder(new List<ZGuid> { declaration1.Job.PK, declaration2.Job.PK }, periodicInvoiceBase.Jobs.OfType<PeriodicInvoiceSelectableJob>().Select(x => x.Parent.PK));

			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = false);
			periodicInvoiceBase.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)).Value = false;
			periodicInvoiceBase.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).Value = true;
			periodicInvoiceBase.LoadJobs();
			AssertContainsExactElementsInAnyOrder(new List<ZGuid> { shipment1.Job.PK, shipment2.Job.PK }, periodicInvoiceBase.Jobs.OfType<PeriodicInvoiceSelectableJob>().Select(x => x.Parent.PK));

			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = false);
			periodicInvoiceBase.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)).Value = false;
			periodicInvoiceBase.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).Value = false;
			periodicInvoiceBase.LoadJobs();
			AssertEquals(0, periodicInvoiceBase.Jobs.Count);
		}

		void PrepareJobAndCharge(IJobHeaderParent parent, ZString invoiceType, OrgHeader org, RefCurrency currency)
		{
			var job = new Job.Loader(parent).TryLoadOrCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", currency, 10M, org, "INV001", currency, 10M, org);
			charge.JR_InvoiceType = invoiceType;
		}

		static void SetFilter(ReferenceNumberFilter filter, string country, string type, string number)
		{
			filter.Country = country;
			filter.Type = type;
			filter.Property = number;
			filter.IsActive = true;
		}

		static CusEntryNumber NewReferenceNumber(CusEntryNumAdditionalReferenceCollection numbers, string countryCode, string type, string number)
		{
			var result = numbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}
