using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.Plugin;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoicePaymentLayouts))]
	sealed class InvoicePaymentLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoicePaymentLayoutBuilder<JobComInvoiceHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoicePaymentControlBag.Instance.CommercialPaymentCodeDropEdit, ControlWidthClass.Long);
				yield return (InvoicePaymentControlBag.Instance.PaymentAmountCalcEdit, ControlWidthClass.Long);
				yield return (InvoicePaymentControlBag.Instance.PaymentNoTextBox, ControlWidthClass.Long);
				yield return (InvoicePaymentControlBag.Instance.PaymentDateEdit, ControlWidthClass.Long);
			}
		}
	}
}
