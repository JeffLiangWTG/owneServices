using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	public class AccQueryClaimBaseValidationTest : MasterFiles.Business.Testing.AccQueryClaimValidationTest
	{
		public void TestAY_OH_DebtorForIntercompany()
		{
			AssertValidationForIntercompany("AY_OH_Debtor", ZGuid.Invalid, TestObjectCreator.ABIGAS.PK);
		}

		public void TestAY_OCForIntercompany()
		{
			AssertValidationForIntercompany("AY_OC", ZGuid.Invalid, QueryClaim.Lookups.Contacts[0].PK);
		}

		public void TestAY_QueryClaimAmountForIntercompany()
		{
			AssertValidationForIntercompany("AY_QueryClaimAmount", decimal.MinValue, 10m);
		}

		public void TestAY_ShortDescriptionOfClaimForIntercompany()
		{
			AssertValidationForIntercompany("AY_ShortDescriptionOfClaim", String.Empty, "Some short description");
		}

		public void TestAY_QueryClaimTypeForIntercompany()
		{
			AssertValidationForIntercompany("AY_QueryClaimType", String.Empty, QueryClaim.Lookups.ClaimType[0].Code);
		}

		public void TestAY_QueryClaimStatusForIntercompany()
		{
			AssertValidationForIntercompany("AY_QueryClaimStatus", String.Empty, "OPN");
		}

		public void TestAY_QueryClaimNextFollowUpForIntercompany()
		{
			AssertValidationForIntercompany("AY_QueryClaimNextFollowUp", ZDateTime.Invalid, ZDateTime.Now);
		}

		void AssertValidationForIntercompany(string propertyName, object badValue, object goodValue)
		{
			QueryClaim[propertyName] = badValue;
			AssertHasErrors("Non-intercompany, bad value", (ZPropertyInfo)QueryClaim[propertyName + "Info"]);
			QueryClaim[propertyName] = goodValue;
			AssertNoErrors("Non-intercompany, good value", (ZPropertyInfo)QueryClaim[propertyName + "Info"]);
			IntercompanyQueryClaim[propertyName] = badValue;
			AssertNoErrors("Intercompany, bad value", (ZPropertyInfo)IntercompanyQueryClaim[propertyName + "Info"]);
			IntercompanyQueryClaim[propertyName] = goodValue;
			AssertNoErrors("Intercompany, good value", (ZPropertyInfo)IntercompanyQueryClaim[propertyName + "Info"]);
		}

		AccQueryClaimBase fQueryClaim;
		AccQueryClaimBase QueryClaim
		{
			get
			{
				if (fQueryClaim == null)
				{
					fQueryClaim = Factory.New<ARAccQueryClaim>();
					fQueryClaim.AY_OH_Debtor = TestObjectCreator.ABIGAS.PK;
				}

				return fQueryClaim;
			}
		}

		AccQueryClaimBase fIntercompanyQueryClaim;
		AccQueryClaimBase IntercompanyQueryClaim
		{
			get
			{
				if (fIntercompanyQueryClaim == null)
				{
					fIntercompanyQueryClaim = Factory.New<ARAccQueryClaim>();
					APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
					invoice.AH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
					fIntercompanyQueryClaim.AY_AH = invoice.PK;
					fIntercompanyQueryClaim.AY_OH_Debtor = TestObjectCreator.ABIGAS.PK;
				}

				return fIntercompanyQueryClaim;
			}
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}
	}
}
