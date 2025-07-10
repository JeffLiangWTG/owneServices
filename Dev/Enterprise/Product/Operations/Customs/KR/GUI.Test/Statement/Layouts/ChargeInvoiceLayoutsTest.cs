using System.Collections.Generic;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ChargeInvoiceLayouts))]
	sealed class ChargeInvoiceLayoutsTest : LayoutsAbstractTest
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
				yield return (StatementHeaderControlBag.Instance.RelatedFormattedAccountNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (StatementHeaderControlBag.Instance.RelatedProcessDateEdit, ControlWidthClass.Auto);
			}
		}
	}
}
