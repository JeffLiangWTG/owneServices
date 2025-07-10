using System;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class WarehouseFact : IWarehouseFact
	{
		public WarehouseFact(Guid pk, string code)
		{
			PK = pk;
			Code = Argument.NotNull(code, nameof(code));
		}

		public Guid PK { get; }
		public string Code { get; }
	}
}
