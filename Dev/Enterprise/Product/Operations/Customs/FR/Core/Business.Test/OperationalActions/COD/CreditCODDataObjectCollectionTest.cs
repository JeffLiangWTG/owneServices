using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(CreditCODDataObjectCollection))]
	public class CreditCODDataObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CreditCODDataObjectCollection>
	{
		protected override CreditCODDataObjectCollection GetCollectionToTest()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			return applicator.FrCreditCODItemApplicators;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			return applicator.FrCreditCODItemApplicators.AddNew();
		}
	}
}
