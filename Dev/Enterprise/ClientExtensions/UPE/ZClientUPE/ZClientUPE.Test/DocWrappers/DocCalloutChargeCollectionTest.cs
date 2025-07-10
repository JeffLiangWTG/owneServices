using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(DocCalloutChargeCollection))]
	public class DocCalloutChargeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCalloutChargeCollection>
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestConstructor()
		{
			Callout callout = CreateSavableCallout();
			Factory.Save();
			CalloutCharge charge1 = callout.JobHeader.Charges.AddNew();
			charge1.JR_ChargeType = "1";
			CalloutCharge charge2 = callout.JobHeader.Charges.AddNew();
			charge2.JR_ChargeType = "2";
			DocCalloutChargeCollection docCollection = new DocCalloutChargeCollection(callout.JobHeader.Charges, Factory);
			AssertEquals("There should be 2 wrapped charges", 2, docCollection.Count);
			AssertEquals("Correct DocCalloutCharge should be returned", "1", docCollection[0].ChargeType);
			AssertEquals("Correct DocCalloutCharge should be returned", "2", docCollection[1].ChargeType);
		}

		public void TestConstructor_EmptyCollection()
		{
			DocCalloutChargeCollection docCollection = new DocCalloutChargeCollection(Factory);
			AssertEquals("No charges", 0, docCollection.Count);
		}

		protected override DocCalloutChargeCollection GetCollectionToTest()
		{
			Callout callout = CreateSavableCallout();
			Factory.Save();
			DocCalloutChargeCollection result = new DocCalloutChargeCollection(callout.JobHeader.Charges, Factory);
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Callout callout = CreateSavableCallout();
			Factory.Save();
			CalloutCharge charge = callout.JobHeader.Charges.AddNew();
			charge.JR_ChargeType = "1";
			DocCalloutCharge docCalloutCharge = DocCalloutCharge.New(charge, Factory);
			return docCalloutCharge;
		}

		int x;
		Callout CreateSavableCallout()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Callout result = (Callout)mAWB.ChildBills.AddNew(typeof(Callout));
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = (x++).ToString();
			job.JH_ParentID = result.PK;
			job.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(result.TableName);
			return result;
		}
	}
}
