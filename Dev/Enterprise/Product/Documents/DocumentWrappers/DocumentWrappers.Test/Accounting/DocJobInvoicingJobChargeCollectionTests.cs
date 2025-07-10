using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobInvoicingJobChargeCollection))]
	public class DocJobInvoicingJobChargeCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocJobInvoicingJobChargeCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var chargeBizO = Factory.New<Charge>();
			return DocJobInvoicingJobCharge.New(chargeBizO, Factory);
		}

		protected override DocJobInvoicingJobChargeCollection GetCollectionToTest()
		{
			return new DocJobInvoicingJobChargeCollection(Factory);
		}

		public void TestChargeSheetOSAmountDisplay()
		{
			var jobHeaderBisObj = Factory.NewJobForTesting<JobHeader>();
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeaderBisObj.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeaderBisObj.JH_JobNum = "Job1";

			var jobHeaderDocWrapper = DocJobHeader.New(jobHeaderBisObj, Factory);
			var coll = DocJobChargeCollection.GetCollection(jobHeaderDocWrapper, "TEST0");
			AssertEquals("", coll.ChargeSheetOSAmountDisplay);

			var orgFactory = new BusinessObjectFactory();
			var client = orgFactory.LoadTop1<OrgHeader>(new ZQuery());
			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = Enterprise.ZArchitecture.Core.InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();
			jobHeaderBisObj.LocalChargesPK = client.PK;

			var code = CreateChargeCode("TESTCH");
			var lineCharge = CreateLineCharge(jobHeaderBisObj, client.PK, 100M, code.PK);
			lineCharge.JR_OSSellAmt = 100M;

			Factory.Save();

			coll = DocJobChargeCollection.GetCollection(jobHeaderDocWrapper, "TEST1", (collection) =>
			{
				collection.Add(DocJobCharge.New(lineCharge, Factory));
			});

			AssertEquals("", coll.ChargeSheetOSAmountDisplay);

			group.PG_InvoiceLineDisplayOption = Enterprise.ZArchitecture.Core.InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();
			AssertEquals("OS AMOUNT", coll.ChargeSheetOSAmountDisplay);
		}

		AccChargeCode CreateChargeCode(string chargeCode)
		{
			var code = Factory.New<AccChargeCode>();
			code.AC_Code = chargeCode;
			code.AC_Desc = "Test Charge Code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;
			code.AC_IsActive = ZBool.True;
			code.FillWithValidTestData();
			return code;
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBisObj.PK;
			lineCharge.JR_GE = jobHeaderBisObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBisObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_LocalSellAmt = amount;
			lineCharge.JR_OH_SellAccount = localChargesPK;

			return lineCharge;
		}
	}
}
