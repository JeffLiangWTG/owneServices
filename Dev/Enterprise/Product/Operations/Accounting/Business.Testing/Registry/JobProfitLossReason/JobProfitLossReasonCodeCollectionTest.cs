using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobProfitLossReasonCodeCollection))]
	public class JobProfitLossReasonCodeCollectionTest : RegistryBusinessObjectCollectionTestCase<JobProfitLossReasonCodeCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override JobProfitLossReasonCodeCollection GetCollectionToTest()
		{
			return new JobProfitLossReasonCodeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JobProfitLossReasonCode();
		}

		#endregion
	}
}
