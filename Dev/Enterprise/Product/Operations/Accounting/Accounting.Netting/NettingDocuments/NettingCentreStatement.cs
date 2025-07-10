using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingCentreStatement : NettingStatement
	{
		//This constructor is for reflection unit tests
		public NettingCentreStatement(BusinessObjectFactory factory)
			: this(factory, ZGuid.Empty, StatementType.Trial)
		{
		}

		public NettingCentreStatement(BusinessObjectFactory factory, ZGuid nettingPeriod)
			: this(factory, nettingPeriod, StatementType.Trial)
		{
		}

		public NettingCentreStatement(BusinessObjectFactory factory, ZGuid nettingPeriod, StatementType statementType)
			: base(factory, nettingPeriod, statementType)
		{
		}

		public override IEnumerable<NettingMovement> GetReceivableNettingMovements()
		{
			return receivableNettingMovements ?? (receivableNettingMovements = GetNettingMovements().Where(x => x.Direction == "IN"));
		}
		IEnumerable<NettingMovement> receivableNettingMovements;

		public override IEnumerable<NettingMovement> GetPayableNettingMovements()
		{
			return payableNettingMovements ?? (payableNettingMovements = GetNettingMovements().Where(x => x.Direction == "OUT"));
		}
		IEnumerable<NettingMovement> payableNettingMovements;
	}
}
