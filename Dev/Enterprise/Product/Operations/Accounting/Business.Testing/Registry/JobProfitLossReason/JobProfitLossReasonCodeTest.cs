using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobProfitLossReasonCode))]
	public class JobProfitLossReasonCodeTest : RegistryBusinessObjectTest
	{
		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 80, BizObj.MaxDescriptionLength_ForTestOnly);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BizObj.Code = "TST";
			BizObj.Description = (NoResString)"Test code";

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new JobProfitLossReasonCode BizObj
		{
			get { return (JobProfitLossReasonCode)base.BizObj; }
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			JobProfitLossReasonCode clone = clone1 as JobProfitLossReasonCode;
			AssertEquals("Code", "TST", clone.Code);
			AssertEquals("Description", "Test code", clone.Description);
			AssertEquals("CodeMaxLength", 3, clone.CodeMaxLength);
		}

		#endregion
	}
}
