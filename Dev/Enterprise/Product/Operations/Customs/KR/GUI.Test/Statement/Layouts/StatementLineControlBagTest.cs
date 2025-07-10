using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(StatementLineControlBag))]
	sealed class StatementLineControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(StatementLineControlBag.DutyAmountCalcEdit);
				yield return nameof(StatementLineControlBag.LiquorTaxCalcEdit);
				yield return nameof(StatementLineControlBag.AgricultureTaxCalcEdit);
				yield return nameof(StatementLineControlBag.TransportationTaxCalcEdit);
				yield return nameof(StatementLineControlBag.EducationTaxCalcEdit);
				yield return nameof(StatementLineControlBag.InterestCalcEdit);
				yield return nameof(StatementLineControlBag.SpecialConsumptionTaxCalcEdit);
				yield return nameof(StatementLineControlBag.VATCalcEdit);
				yield return nameof(StatementLineControlBag.DeclarationPenaltyCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => StatementLineControlBag.Instance;
	}
}
