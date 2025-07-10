using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestVehiclesTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var frm = new ZForm(declaration))
			using (var userControl = new ExportInvoiceLineUserControl())
			{
				frm.Controls.Add(userControl);
				userControl.JobDeclaration = declaration;
				frm.Show();

				var tabPage = userControl.FindSingle<ZTabPage>("VehiclesTabPage");
				tabPage.Show();
				CombineAssertions(() =>
				{
					AssertEquals("VehiclesTabPage visible", true, tabPage.TabVisible);
					AssertEquals("Caption", "Vehicles", tabPage.CaptionResourceString.Caption);
				});
			}
		}

		public void TestAdditionalInfosTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var additionalInfosTabPage = control.FindSingle<ZTabPage>("AdditionalInfosTabPage");
				AssertEquals("AdditionalInfosTabPage visible", true, additionalInfosTabPage.TabVisible);
				AssertEquals("Caption", "Additional Documents", additionalInfosTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestGridColumns()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = MessageTypeList.Codes.Export;
				control.JobDeclaration = dec;
				control.InitializeGridLayout();

				var lineGrid = control.CustomsInvoiceLinesBoundGrid;
				var styles = lineGrid.ColumnStyles;
				var box34Column = lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin);
				AssertNotNull(box34Column);
				AssertEquals("[34b] Orig. State/Island is visible for Export Declarations", true, box34Column.IsVisible);
			}
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestAddtionalProcedureCodeAsStringName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			{
				using (var control = new ExportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					AssertEquals("[37.2] Nat./UE Reg.", control.FindSingle<EU.GUI.AdditionalProcedureCodesUserControl>(x => x.Name == "AdditionalProcedureCodesUserControl").GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
		}

		public void TestFieldVisible()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				AssertNull("DestinationCodeFindBox is not available", control.FindSingleOrDefault<ZCodeFindBox>("DestinationCodeFindBox"));
				AssertEquals(true, control.FindSingle<ZDropEdit>("OriginStateDropEdit").Visible);
			}
		}

		public void TestCommercialReferenceVisibility()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			dec.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.SetDataBinding(dec, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = dec;
				form.Show();
				var commercialReferenceBox = control.Controls.Find("CommercialReferenceTextBox", true)[0];
				AssertNotNull("For Export declaraction, Commercial Reference must be available on the screen", commercialReferenceBox);
				Assert("Commercial Reference should be visible for Export Declaration ES", commercialReferenceBox.Visible);
			}
		}

		public void TestPreviousDocumentsUserControlType()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				AssertEquals(typeof(PreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
			}
		}

		public void TestTariffFindBoxGetSelectNomenclatureModes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var tariffBox = control.GetTariffFindBoxExposed();
				AssertNotNull("TariffCodeFindBox", tariffBox);

				var selectionModes = tariffBox.GetSelectNomenclatureModes?.Invoke();
				AssertSelectionModes(selectionModes, new[] { SelectionStyle.Tariff }, ZString.Empty);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				selectionModes = tariffBox.GetSelectNomenclatureModes?.Invoke();
				AssertSelectionModes(selectionModes, new[] { SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff }, ExsEntrySubStyleList.Codes.EXS);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
				selectionModes = tariffBox.GetSelectNomenclatureModes?.Invoke();
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

		class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
		{
			public Type GetPreviousDocumentsUserControlTypeExposed() => base.GetPreviousDocumentsUserControlType();

			internal Universal.GUI.TariffFindBox GetTariffFindBoxExposed() => InvoiceLineDetailsUserControl
					.FindSingleOrDefault<Universal.GUI.TariffFindBox>("FormattedWithDescriptionTariffFindBox");
		}
	}
}
