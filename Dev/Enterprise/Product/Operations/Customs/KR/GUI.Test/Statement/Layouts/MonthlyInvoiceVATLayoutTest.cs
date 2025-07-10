using System.Collections.Generic;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MonthlyInvoiceVATLayout))]
	sealed class MonthlyInvoiceVATLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new StatementHeaderLayoutBuilder<CusStatementHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (StatementHeaderControlBag.Instance.FormattedNumberTextBox, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.ProcessPortCodeFindBox, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.ProcessDateEdit, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.PaymentDateEdit, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.TotalVATAmountCalcEdit, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.StatementAmountCalcEdit, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.ImporterGuidFindBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (StatementHeaderControlBag.Instance.StatusDropEdit, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.StatementTypeDropEdit, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.PaymentTypeDropEdit, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.PaymentPartyDropEdit, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.PeriodDateUserControl, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.RelatedFormattedAccountNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.RelatedProcessDateEdit, ControlWidthClass.Auto);
			}
		}
	}
}
