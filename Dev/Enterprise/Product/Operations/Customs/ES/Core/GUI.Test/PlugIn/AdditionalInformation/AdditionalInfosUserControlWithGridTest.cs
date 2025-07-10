using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class AdditionalInfosUserControlWithGridTest : TestCaseWithFactory
{
	public void TestBindingSourceType()
	{
		using (var control = new AdditionalInfosUserControlWithGrid())
		{
			AssertEquals(typeof(EU.Business.Declaration.JobDeclaration), control.BindingSource.DataSourceType);
		}
	}

	public void TestNKCountryCodeVisibility() => CombineAssertions(() =>
	{
		const string Import = EU.Business.MessageTypeList.Codes.Import;
		const string Export = EU.Business.MessageTypeList.Codes.Export;

		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var instructionDocument = instruction.AdditionalInfos.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceDocument = invoice.AdditionalInfos.AddNew();

		var invoiceLine = invoice.InvoiceLines.AddNew();
		var lineDocument = invoiceLine.AdditionalInfos.AddNew();

		var invoiceLineBindingMember = "FilteredInvoiceLines.AdditionalInfos";
		var invoiceHeaderBindingMember = "Invoices.AdditionalInfos";
		var entryInstructionBindingMember = "CustomsEntryInstructions.AdditionalInfos";
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertIsVisible("InvoiceLine: CSI_RN_NKCountryCode Visible for IMP and H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), invoiceLineBindingMember, Import);
			AssertIsVisible("InvoiceHeader: CSI_RN_NKCountryCode Visible for IMP and H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), invoiceHeaderBindingMember, Import);
			AssertIsVisible("EntryInstruction: CSI_RN_NKCountryCode Visible for IMP and H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), entryInstructionBindingMember, Import);

			AssertNotVisible("InvoiceLine: CSI_RN_NKCountryCode Visible for EXP and H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), invoiceLineBindingMember, Export);
			AssertNotVisible("InvoiceHeader: CSI_RN_NKCountryCode Visible for EXP and H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), invoiceHeaderBindingMember, Export);
			AssertNotVisible("EntryInstruction: CSI_RN_NKCountryCode Visible for EXP and H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), entryInstructionBindingMember, Export);
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		using (var form = new ZForm(declaration))
		using (var control = new AdditionalInfosUserControlWithGrid())
		{
			AssertNotVisible("InvoiceLine: CSI_RN_NKCountryCode not Visible for EXP and not H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), invoiceLineBindingMember, Export);
			AssertNotVisible("InvoiceHeader: CSI_RN_NKCountryCode not Visible for EXP and not H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), invoiceHeaderBindingMember, Export);
			AssertNotVisible("EntryInstruction: CSI_RN_NKCountryCode not Visible for EXP and not H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), entryInstructionBindingMember, Export);

			AssertNotVisible("InvoiceLine: CSI_RN_NKCountryCode not Visible for IMP and not H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), invoiceLineBindingMember, Import);
			AssertNotVisible("InvoiceHeader: CSI_RN_NKCountryCode not Visible for IMP and not H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), invoiceHeaderBindingMember, Import);
			AssertNotVisible("EntryInstruction: CSI_RN_NKCountryCode not Visible for IMP and not H1 activate", nameof(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox), entryInstructionBindingMember, Import);
		}

		void AssertIsVisible(string description, string componentName, string dataMemberName, string declarationType)
			=> AssertVisibility(description, componentName, dataMemberName, declarationType, true);

		void AssertNotVisible(string description, string componentName, string dataMemberName, string declarationType)
			=> AssertVisibility(description, componentName, dataMemberName, declarationType, false);

		void AssertVisibility(string description, string componentName, string dataMemberName, string declarationType, bool isVisible)
		{
			using (var form = new ZForm())
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				declaration.JE_MessageType = declarationType;
				form.Controls.Add(control);
				control.Grid.DataMember = dataMemberName;
				control.SetDataBinding(declaration, string.Empty);
				form.Show();

				var additionalInfosPanel = control.Controls.Find("AdditionalInfosPanel", true).First();
				var additionalInfosGroupBox = additionalInfosPanel.Controls.Find("AdditionalInfosGroupBox", true).First();
				var detailsLayoutControl = additionalInfosGroupBox.Controls.Find("DetailsLayoutControl", true).First();
				var detailsPanel = detailsLayoutControl.Controls.Find("DetailsPanel", true).First();
				var component = detailsPanel.Controls.Find(componentName, true).First();
				AssertEquals($"{description} {componentName} visibility for {declarationType}", expected: isVisible, component.Visible);
			}
		}
	});

	public void TestIsUCC6AndIsImportProperty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		using (var control = new AdditionalInfosUserControlWithGrid())
		{
			control.SetDataBinding(declaration, string.Empty);
			var value = control.GetType().GetProperty("IsUCC6AndIsImport", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.GetValue(control);
			AssertEquals("IsUCC6AndIsImport should be true", expected: true, value);
		}
	}
}
