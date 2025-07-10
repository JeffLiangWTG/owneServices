using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
	sealed class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestFormattedWithDescriptionTariffFindBoxBehaviour()
		{
			var tariffFindBoxControlReference = CommonInvoiceLineDetailsControlBag.Instance.FormattedWithDescriptionTariffFindBox;
			AssertEquals("Has TariffBoxNomenclatureSelectionModeBehaviour", true, Layout.HasBehaviourByBehaviourType(tariffFindBoxControlReference, typeof(TariffBoxNomenclatureSelectionModeBehaviour)));
		}

		public void TestTariffBoxSelectionModeChangeOnDependencyValueChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var tariffFindBoxControlReference = CommonInvoiceLineDetailsControlBag.Instance.FormattedWithDescriptionTariffFindBox;
			var behaviourContainer = Layout.GetControlBehaviourContainer<TariffBoxNomenclatureSelectionModeBehaviour>(tariffFindBoxControlReference);

			AssertEquals("IsRefreshRequired, No Link between Invoice Line and Entry Instruction", true, behaviourContainer.IsRefreshRequired(invoiceLine, control));
			behaviourContainer.UpdateControlBehaviour(control, invoiceLine);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = "EXS";
			AssertEquals("IsRefreshRequired, No Link between Invoice Line and Entry Instruction, CEI_SubStyle=EXS", false, behaviourContainer.IsRefreshRequired(invoiceLine, control));

			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals("IsRefreshRequired, Invoice Line with an Entry Instruction, CEI_SubStyle=EXS", true, behaviourContainer.IsRefreshRequired(invoiceLine, control));
			behaviourContainer.UpdateControlBehaviour(control, invoiceLine);

			entryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsRefreshRequired, Invoice Line with an Entry Instruction, CEI_SubStyle=A", true, behaviourContainer.IsRefreshRequired(invoiceLine, control));
			behaviourContainer.UpdateControlBehaviour(control, invoiceLine);

			entryInstruction.CEI_SubStyle = "X";
			AssertEquals("IsRefreshRequired, Invoice Line with an Entry Instruction, CEI_SubStyle=X", true, behaviourContainer.IsRefreshRequired(invoiceLine, control));
			behaviourContainer.UpdateControlBehaviour(control, invoiceLine);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Universal.GUI.TariffFindBox();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		Universal.GUI.TariffFindBox control;

		protected override int ControlBagCount => 3;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportInvoiceLineDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.OriginStateDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.CommercialReferenceTextBox, ControlWidthClass.Long);
			}
		}

		PanelLayout Layout => layout ?? (layout = new ExportInvoiceLineDetailsLayout().Layout);
		PanelLayout layout;
	}
}
