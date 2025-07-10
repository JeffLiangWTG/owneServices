using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutChargeCollection))]
	internal class CalloutChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFindByChargeDescription()
		{
			CalloutCharge charge1 = Collection.AddNew();
			CalloutCharge charge2 = Collection.AddNew();
			charge1.JR_Desc = "CG1";
			charge2.JR_Desc = "CG2";
			CalloutCharge foundCharge = Collection.FindByChargeDescription("CG2");
			AssertEquals("Should find the correct charge", "CG2", foundCharge.JR_Desc);
			CalloutCharge noCharge = Collection.FindByChargeDescription("XXX");
			AssertNull("Should return null if there is no charge", noCharge);
		}

		public void TestAddNew_DefaultValues()
		{
			CalloutCharge charge = Collection.AddNew();
			AssertEquals(GlbDepartment.CurrentDepartment.PK, charge.JR_GE);
			AssertEquals(GlbBranch.CurrentBranch.PK, charge.JR_GB);
			AssertEquals(true, charge.JR_JH.IsValid);
			AssertEquals(Env.Registry.FreightChargeCode, charge.JR_AC);
		}

		public void TestAddNewFromPWSChargeDetails()
		{
			PWSChargeDetails chargeDetails = new PWSChargeDetails("FREIGHT                         45.00           238.00            10.50           350.25");
			CalloutCharge charge = Collection.AddNewFromPWSChargeDetails(chargeDetails);
			AssertEquals(45m, charge.TaxableAmount);
			AssertEquals(238m, charge.NonTaxableAmount);
			AssertEquals(10.5m, charge.Discount);
			AssertEquals(350.25m, charge.NettAmount);
			AssertEquals("FREIGHT", charge.JR_Desc);
		}

		public void TestRelationshipFilter()
		{
			Collection.Load();
			AssertEquals("Pre-condition", 0, Collection.Count);
			CreateCalloutJobCharge(10, "Charge1");
			CreateCalloutJobCharge(20, "Charge2");
			CreateCalloutJobCharge(30, "Charge3");
			Collection.Load();
			Collection.Sort(JobChargeSchema.Constants.JR_Desc, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("Should be picked up by the RelationshipFilter", 3, Collection.Count);
			AssertCalloutCharge(Collection[0], "Charge1", 10m);
			AssertCalloutCharge(Collection[1], "Charge2", 20m);
			AssertCalloutCharge(Collection[2], "Charge3", 30m);
			CalloutJobHeader newJobHeader = CreateJobHeader("JH2");
			CreateCalloutJobCharge(45, "Charge4", newJobHeader);
			CreateCalloutJobCharge(81, "Charge5", newJobHeader);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CalloutChargeCollection newCollection = new CalloutChargeCollection(newFactory.Load<CalloutJobHeader>(newJobHeader.PK));
			newCollection.Load();
			newCollection.Sort(JobChargeSchema.Constants.JR_Desc, System.ComponentModel.ListSortDirection.Ascending);
			AssertCalloutCharge(newCollection[0], "Charge4", 45m);
			AssertCalloutCharge(newCollection[1], "Charge5", 81m);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CalloutChargeCollection(CalloutJobHeader);
		}

		void AssertCalloutCharge(CalloutCharge charge, ZString expectedDesc, ZDecimal expectedTaxableAmount)
		{
			AssertEquals(expectedDesc, charge.JR_Desc);
			AssertEquals(expectedTaxableAmount, charge.TaxableAmount);
		}

		CalloutCharge CreateCalloutJobCharge(ZDecimal taxableAmount, ZString desc)
		{
			return CreateCalloutJobCharge(taxableAmount, desc, CalloutJobHeader);
		}

		CalloutCharge CreateCalloutJobCharge(ZDecimal taxableAmount, ZString desc, CalloutJobHeader jobHeader)
		{
			CalloutCharge charge = Factory.New<CalloutCharge>();
			charge.TaxableAmount = taxableAmount;
			charge.JR_Desc = desc;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GC = GlbCompany.CurrentCompany.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AC = Factory.LoadTop1(typeof(AccChargeCode), new ZQuery()).PK;
			charge.JR_JH = jobHeader.PK;
			return charge;
		}

		CalloutJobHeader CreateJobHeader(ZString jobNum)
		{
			var jobHeader = Factory.NewWithValidTestData<CalloutJobHeader>();
			jobHeader.JH_JobNum = jobNum;
			return jobHeader;
		}

		new CalloutChargeCollection Collection
		{
			get
			{
				return (CalloutChargeCollection)base.Collection;
			}
		}

		CalloutJobHeader CalloutJobHeader
		{
			get
			{
				if (fCalloutJobHeader == null)
				{
					fCalloutJobHeader = Factory.NewWithValidTestData<CalloutJobHeader>();
				}

				return fCalloutJobHeader;
			}
		}

		CalloutJobHeader fCalloutJobHeader;
		#endregion
	}
}
