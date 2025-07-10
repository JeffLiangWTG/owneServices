using System.Linq;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAgencyVoyageAccount))]
	sealed class DocAgencyVoyageAccountTest : DocumentWrapperTestCase
	{
		public void TestJobNumber()
		{
			Account.NA_JobNumber = "Blat";
			AssertEquals("Blat", Wrapper.JobNumber);
		}

		public void TestPrincipal()
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_FullName = "Principal";

			Account.NA_OH = principal.PK;

			AssertEquals("Principal", Wrapper.Principal.Name);
		}

		public void TestVoyage()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "0001";

			Account.NA_JV = voyage.PK;

			AssertEquals("MAJAPAHIT", Wrapper.Voyage.NKVessel.Code);
			AssertEquals("0001", Wrapper.Voyage.VoyageFlight);
		}

		public void TestDisbursmentLines()
		{
			AccChargeCode code = Factory.New<AccChargeCode>();
			code.AC_Code = "XXX";

			var job = new Job.Loader(Account).TryLoadOrCreate();
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = code.PK;
			charge.JR_LocalSellAmt = 543m;

			AssertEquals("543.00 AUD", Wrapper.DisbursmentLines[0].Amounts[0].LocalAmount.AmountAndCurrencyCode);
		}

		#region Implementation

		VoyageAccount Account
		{
			get { return account ?? (account = Factory.New<VoyageAccount>()); }
		}
		VoyageAccount account;

		DocAgencyVoyageAccount Wrapper
		{
			get { return DocAgencyVoyageAccount.New(Account, Factory); }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			VoyageAccount account = Factory.New<VoyageAccount>();

			return new DocumentWrapper[]
			{
				DocAgencyVoyageAccount.New(account, Factory)
			};
		}

		#endregion
	}
}
