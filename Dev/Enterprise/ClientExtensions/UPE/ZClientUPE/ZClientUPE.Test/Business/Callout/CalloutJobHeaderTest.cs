using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutJobHeader))]
	internal class CalloutJobHeaderTest : JobHeaderTest
	{
		public void TestCharges()
		{
			CreateCalloutJobCharge(JobHeader);
			CreateCalloutJobCharge(JobHeader);
			AssertEquals("Should be loaded in the getter", 2, JobHeader.Charges.Count);
			AssertEquals("Should be read-only", true, JobHeader.Charges.ReadOnly);
			CalloutJobHeader localJobHeader = Factory.New<CalloutJobHeader>();
			CreateCalloutJobCharge(localJobHeader);
			AssertEquals("Should be loaded in the getter", 1, localJobHeader.Charges.Count);
			AssertEquals("Should be read-only", true, localJobHeader.Charges.ReadOnly);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();
			AssertEquals(CusHAWBSchema.Constants.Prefix, JobHeader.JH_ParentTableCode);
			AssertEquals(GlbBranch.CurrentBranch.PK, JobHeader.JH_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, JobHeader.JH_GE);
		}

		public void TestJobNumSetOnSaving()
		{
			CalloutJobHeader cachedJobHeader = JobHeader;
			string expectedJobNum = UPENumberFountains.Instance.CalloutJobHeaderNumberFountain.PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("Should be set OnSaving", expectedJobNum, JobHeader.JH_JobNum);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CalloutJobHeader newJobHeader = newFactory.Load<CalloutJobHeader>(cachedJobHeader.PK);
			newJobHeader.HasChanges = true;
			newFactory.Save();
			AssertEquals("Should not change", expectedJobNum, JobHeader.JH_JobNum);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override Type GetExpectedJobTypeToLoad()
		{
			return null;
		}

		CalloutCharge CreateCalloutJobCharge(CalloutJobHeader jobHeader)
		{
			CalloutCharge charge = Factory.New<CalloutCharge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AC = Factory.LoadTop1(typeof(AccChargeCode), new ZQuery()).PK;
			charge.JR_JH = jobHeader.PK;
			return charge;
		}

		CalloutJobHeader JobHeader
		{
			get
			{
				if (fJobHeader == null)
				{
					fJobHeader = Factory.New<CalloutJobHeader>();
				}

				return fJobHeader;
			}
		}

		CalloutJobHeader fJobHeader;
		#endregion
	}
}
