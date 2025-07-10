using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(TransactionLineDetailsWithGridLayout))]
	sealed class TransactionLineDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override Type ExpectedGridUserControlType => typeof(TransactionLinesGridUserControl);

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransactionDetailsLayoutBuilder<CusIntrastatHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TransactionLineDetailsControlBag.Instance.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
				yield return (TransactionLineDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Long);
				yield return (TransactionLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (TransactionLineDetailsControlBag.Instance.RegionDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (TransactionLineDetailsControlBag.Instance.InvoiceValueDropEdit, ControlWidthClass.Auto);
				yield return (TransactionLineDetailsControlBag.Instance.StatisticalValueDropEdit, ControlWidthClass.Auto);
				yield return (TransactionLineDetailsControlBag.Instance.MassDropEdit, ControlWidthClass.Auto);
				yield return (TransactionLineDetailsControlBag.Instance.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
			}
		}
	}
}
