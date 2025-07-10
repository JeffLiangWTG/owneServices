using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentJobTypeOption))]
	class NonPersistentJobTypeOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<NonPersistentJobTypeOptionLookups>(jobTypeOption.Lookups);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusInBondApplicationCodeList.Codes.NCTS4, jobTypeOption.JobType);
		}

		public void TestJobType_Caption()
		{
			AssertEquals("Job Type", DataBoundResourceStrings.GetDataForProperty(jobTypeOption.JobTypeInfo).Caption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NonPersistentJobTypeOption(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobTypeOption = GetNewBusinessObject() as NonPersistentJobTypeOption;
		}
		NonPersistentJobTypeOption jobTypeOption;
	}
}
