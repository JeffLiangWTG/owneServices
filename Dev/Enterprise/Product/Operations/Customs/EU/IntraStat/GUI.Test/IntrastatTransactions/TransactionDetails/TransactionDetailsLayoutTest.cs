using System.Collections.Generic;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(TransactionDetailsLayout))]
	sealed class TransactionDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransactionDetailsLayoutBuilder<CusIntrastatHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TransactionDetailsControlBag.Instance.SupplierNameTextBox, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.SupplierVATTextBox, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.ConsigneeNameTextBox, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.ConsigneeVATTextBox, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.CountryOfSupplyDropEdit, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.CountryOfReceiptDropEdit, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.TransactionDateEdit, ControlWidthClass.Auto);
				yield return (TransactionDetailsControlBag.Instance.NatureOfTransactionDropEdit, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.ModeOfTransportDropEdit, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.TradersReferenceTextBox, ControlWidthClass.Long);
				yield return (TransactionDetailsControlBag.Instance.IncoTermDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
