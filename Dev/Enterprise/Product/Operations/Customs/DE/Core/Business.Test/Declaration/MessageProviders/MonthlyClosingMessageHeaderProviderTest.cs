using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(MonthlyClosingMessageHeaderProvider))]
	public abstract class MonthlyClosingMessageHeaderProviderTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : MonthlyClosingMessageHeaderProvider
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<CusReconDeclaration>();
		}

		protected CusReconDeclaration declaration;
	}
}
