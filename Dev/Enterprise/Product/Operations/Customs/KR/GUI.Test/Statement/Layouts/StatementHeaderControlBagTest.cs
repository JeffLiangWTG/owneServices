using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(StatementHeaderControlBag))]
	sealed class StatementHeaderControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(StatementHeaderControlBag.ProcessPortCodeFindBox);
				yield return nameof(StatementHeaderControlBag.ProcessAndDueDateUserControl);

				yield return nameof(StatementHeaderControlBag.StatementAmountCalcEdit);
				yield return nameof(StatementHeaderControlBag.FormattedNumberTextBox);
				yield return nameof(StatementHeaderControlBag.StatusDropEdit);
				yield return nameof(StatementHeaderControlBag.StatementTypeDropEdit);

				yield return nameof(StatementHeaderControlBag.PeriodDateUserControl);

				yield return nameof(StatementHeaderControlBag.ImporterGuidFindBox);
				yield return nameof(StatementHeaderControlBag.PayerFromCustomsTextBox);
				yield return nameof(StatementHeaderControlBag.RelatedFormattedAccountNumberCodeFindBox);
				yield return nameof(StatementHeaderControlBag.RelatedProcessDateEdit);

				yield return nameof(StatementHeaderControlBag.ProcessDateEdit);
				yield return nameof(StatementHeaderControlBag.PaymentDateEdit);
				yield return nameof(StatementHeaderControlBag.TotalVATAmountCalcEdit);
				yield return nameof(StatementHeaderControlBag.PaymentTypeDropEdit);
				yield return nameof(StatementHeaderControlBag.PaymentPartyDropEdit);

				yield return nameof(StatementHeaderControlBag.FormattedEntryNumberTextBox);
				yield return nameof(StatementHeaderControlBag.BillTypeDropEdit);
				yield return nameof(StatementHeaderControlBag.CustomsAccountIDTextBox);
				yield return nameof(StatementHeaderControlBag.DueDateEdit);
				yield return nameof(StatementHeaderControlBag.IssueDateEdit);
				yield return nameof(StatementHeaderControlBag.TotalAmountAfterDueDateCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => StatementHeaderControlBag.Instance;
	}
}
