using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingCentreStatement))]
	public class NettingCentreStatementTest : NettingStatementTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NettingCentreStatement(Factory, Period.PK);
		}

		protected override void AssertSignsForNetMovements()
		{
			NettingCentreStatement bizO = (NettingCentreStatement)GetNewBusinessObject();

			var nettingcentreReceivables = bizO.GetReceivableNettingMovements();
			var nettingcentrePayables = bizO.GetPayableNettingMovements();

			foreach (var item in nettingcentreReceivables)
			{
				Assert("Amount should be positive for all receivable movement for Netting centre", item.SignedMovementAmount > 0);
			}

			foreach (var item in nettingcentrePayables)
			{
				Assert("Amount should be negetive for all payable movement for Netting centre", item.SignedMovementAmount < 0);
			}
		}
	}
}
