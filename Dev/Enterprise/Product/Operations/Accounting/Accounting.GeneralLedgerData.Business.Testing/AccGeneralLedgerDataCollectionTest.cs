using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	[TestedType(typeof(AccGeneralLedgerDataCollection))]
	public class AccGeneralLedgerDataCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccGeneralLedgerDataCollection(Factory);
		}
	}
}
