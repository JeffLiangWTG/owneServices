using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(InvoiceLineAdditionalInformationDetailsLayout))]
public sealed class InvoiceLineAdditionalInformationDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestNKCountryCodeVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("NKCountryCodeFindBox Visible for IMP and H1 activate", true, Layout.IsVisible(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox, additionalInfo));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("NKCountryCodeFindBox not Visible for EXP and H1 activate", false, Layout.IsVisible(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox, additionalInfo));
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertEquals("NKCountryCodeFindBox not Visible for EXP and H1 not activate", false, Layout.IsVisible(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox, additionalInfo));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("NKCountryCodeFindBox not Visible for IMP and H1 not activate", false, Layout.IsVisible(AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox, additionalInfo));
		}
	});

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	PanelLayout Layout => layout ?? (layout = new InvoiceLineAdditionalInformationDetailsLayout().Layout);
	PanelLayout layout;

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.PlugIn.AdditionalInformationDetailsLayoutBuilder();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.Instance.DetailTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.Instance.CurrencyDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.Instance.AmountCalcEdit, ControlWidthClass.Long);
			yield return (AdditionalInformationDetailsControlBag.Instance.NKCountryCodeFindBox, ControlWidthClass.Auto);
		}
	}
}
