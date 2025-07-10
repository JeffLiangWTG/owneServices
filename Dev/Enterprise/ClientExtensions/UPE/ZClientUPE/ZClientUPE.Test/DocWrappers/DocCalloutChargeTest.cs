using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(DocCalloutCharge))]
	public class DocCalloutChargeTest : DocumentWrapperTestCase
	{
		public void TestNew()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Callout callout = (Callout)mAWB.ChildBills.AddNew(typeof(Callout));
			callout.EnsureJobHeaderExists();
			JobHeader lazyCreated = callout.JobHeader;
			lazyCreated.JH_JobNum = "JobNumber";
			Factory.Save();
			CalloutCharge charge = callout.JobHeader.Charges.AddNew();
			DocCalloutCharge doc = DocCalloutCharge.New(charge, Factory);
			AssertNotNull("Doc wrapper of correct type", doc);
		}

		public void TestIsFirstChargeInCollection()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Callout callout = (Callout)mAWB.ChildBills.AddNew(typeof(Callout));
			callout.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Collect;
			Factory.Save();
			callout.EnsureJobHeaderExists();
			CalloutCharge charge1 = callout.JobHeader.Charges.AddNew();
			CalloutCharge charge2 = callout.JobHeader.Charges.AddNew();
			DocCallout doc = DocCallout.New(callout, Factory);
			AssertEquals("First charge in list", true, doc.Charges[0].IsFirstChargeInCollection);
			AssertEquals("Second charge in list", false, doc.Charges[1].IsFirstChargeInCollection);
		}

		public void TestCostSplitMethod()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Callout callout = (Callout)mAWB.ChildBills.AddNew(typeof(Callout));
			callout.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Collect;
			Factory.Save();
			callout.EnsureJobHeaderExists();
			CalloutCharge charge1 = callout.JobHeader.Charges.AddNew();
			JobConsolCost cost = Factory.New<JobConsolCost>();
			cost.E6_ApportionmentMethod = "CHG";
			charge1.JR_E6 = cost.PK;
			DocCallout doc = DocCallout.New(callout, Factory);
			AssertEquals("Should have correct FK to Cost", cost.PK, doc.Charges[0].CostSplitGroup);
			AssertEquals("Should return correct cost item", "CHG", doc.Charges[0].CostSplitMethod);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Callout callout = (Callout)mAWB.ChildBills.AddNew(typeof(Callout));
			callout.EnsureJobHeaderExists();
			CalloutCharge charge = callout.JobHeader.Charges.AddNew();
			DocCalloutCharge result = DocCalloutCharge.New(charge, Factory);
			return new DocumentWrapper[] { result };
		}
		#endregion
	}
}
