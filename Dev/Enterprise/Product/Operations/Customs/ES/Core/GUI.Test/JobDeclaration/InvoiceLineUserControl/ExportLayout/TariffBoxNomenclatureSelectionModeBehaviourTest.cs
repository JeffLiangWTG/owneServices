using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class TariffBoxNomenclatureSelectionModeBehaviourTest : TestCaseWithFactory
	{
		public void TestUpdateBehaviour()
		{
			using (var form = new ZForm(invoiceLine))
			{
				var controlBag = CommonInvoiceLineDetailsControlBag.Instance;
				var allControls = new Dictionary<ControlReference, Control>();
				controlBag.CreateControls(form, allControls);
				var tariffFindBoxControl = allControls[controlBag.FormattedWithDescriptionTariffFindBox] as Universal.GUI.TariffFindBox;
				AssertNotNull("TariffFindBoxControl", tariffFindBoxControl);

				var behaviour = new TariffBoxNomenclatureSelectionModeBehaviour();
				behaviour.UpdateBehaviour(tariffFindBoxControl, invoiceLine);

				var selectionModes = tariffFindBoxControl.GetSelectNomenclatureModes?.Invoke();
				AssertSelectionModes(selectionModes, new[] { SelectionStyle.Tariff }, ZString.Empty);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				behaviour.UpdateBehaviour(tariffFindBoxControl, invoiceLine);
				selectionModes = tariffFindBoxControl.GetSelectNomenclatureModes?.Invoke();
				AssertSelectionModes(selectionModes, new[] { SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff }, ExsEntrySubStyleList.Codes.EXS);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
				behaviour.UpdateBehaviour(tariffFindBoxControl, invoiceLine);
				selectionModes = tariffFindBoxControl.GetSelectNomenclatureModes?.Invoke();
				AssertSelectionModes(selectionModes, new[] { SelectionStyle.Tariff }, ZString.Empty);
			}

			void AssertSelectionModes(IReadOnlyCollection<SelectionStyle> actualModes, SelectionStyle[] expectedModes, ZString entrySubStyle)
			{
				var entrySubStyleText = entrySubStyle.IsEmpty ? "EMPTY" : entrySubStyle.ToString();
				AssertNotNull($"Selection Modes for {entrySubStyleText}", actualModes);
				AssertEquals($"Selection Mode for {entrySubStyleText} Count", expectedModes.Length, actualModes.Count);
				AssertArrayEqualsByElements($"Selection Modes for {entrySubStyleText}", expectedModes, actualModes.ToArray());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
		}

		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;
	}
}
