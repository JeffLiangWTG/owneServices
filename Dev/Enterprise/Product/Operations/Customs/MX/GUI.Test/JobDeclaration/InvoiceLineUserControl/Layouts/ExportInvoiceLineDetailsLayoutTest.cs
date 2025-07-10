using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
	sealed class ExportInvoiceLineDetailsLayoutTest : BaseInvoiceLineDetailsLayoutTest<ExportInvoiceLineDetailsLayout>
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfExportCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.ObservationsTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.VehicleDetailsUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			}
		}
	}
}
