using System;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(InvoiceDetailsLayoutBuilder<JobComInvoiceHeader>))]
sealed class InvoiceDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceDetailsLayoutBuilder<JobComInvoiceHeader>, JobComInvoiceHeader, EU.GUI.InvoiceDetailsControlBag>
{
	public void TestSetDefaultVisibility_IncoTermPlaceTextBox_IMP()
	{
		AssertUcc6ControlVisibility(CommercialInvoiceDetailsControlBag.Instance.IncoTermPlaceTextBox, x => x.JZ_IncoTerm != IncoTerms.Other, true, messageType: MessageTypeList.Codes.Import);
	}

	public void TestSetDefaultVisibility_IncoTermPlaceTextBox_EXP()
	{
		AssertUcc6ControlVisibility(CommercialInvoiceDetailsControlBag.Instance.IncoTermPlaceTextBox, x => x.JZ_IncoTerm != IncoTerms.Other, true, messageType: MessageTypeList.Codes.Export);
	}

	public void TestSetDefaultVisibility_IncoTermsAgreedPlaceLongTextControl_IMP()
	{
		AssertUcc6ControlVisibility(EU.GUI.InvoiceDetailsControlBag.Instance.IncoTermsAgreedPlaceLongTextControl, x => x.JZ_IncoTerm == IncoTerms.Other, false, messageType: MessageTypeList.Codes.Import);
	}

	public void TestSetDefaultVisibility_IncoTermsAgreedPlaceLongTextControl_EXP()
	{
		AssertUcc6ControlVisibility(EU.GUI.InvoiceDetailsControlBag.Instance.IncoTermsAgreedPlaceLongTextControl, x => x.JZ_IncoTerm == IncoTerms.Other, false, messageType: MessageTypeList.Codes.Export);
	}

	protected override InvoiceDetailsLayoutBuilder<JobComInvoiceHeader> GetColumnLayoutBuilderForTesting() => new InvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();

	void AssertUcc6ControlVisibility(ControlReference controlReference, Func<JobComInvoiceHeader, bool> visible, bool visibleWhenNotUCC6 = false, string messageType = MessageTypeList.Codes.Export)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var invoice = declaration.Invoices.AddNew();

		PanelLayout layout;
		switch (messageType)
		{
			case MessageTypeList.Codes.Export:
				layout = new ExportInvoiceDetailsLayout().Layout;
				break;
			case MessageTypeList.Codes.Import:
				layout = new ImportInvoiceDetailsLayout().Layout;
				break;
			default:
				layout = new PanelLayout();
				break;
		}

		CombineAssertions(() =>
		{
			AssertEquals("Is UCC6", true, declaration.Configuration.IsUCC6(declaration));
			AssertControlVisibility("UCC6", visible);

			EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false);
			AssertEquals("Is Non UCC6", false, declaration.Configuration.IsUCC6(declaration));
			AssertControlVisibility("Non UCC6", (x) => visibleWhenNotUCC6);
		});

		void AssertControlVisibility(string description, Func<JobComInvoiceHeader, bool> isVisible)
		{
			foreach (var incoTerm in Incoterms)
			{
				invoice.JZ_IncoTerm = incoTerm;
				AssertEquals($"{description}: Message type: {messageType}, Incoterm: {incoTerm}", isVisible(invoice), layout.IsVisible(controlReference, invoice));
			}
		}
	}

	string[] Incoterms => new[]
	{
		IncoTerms.CarriageAndInsurancePaidTo,
		IncoTerms.CarriagePaidTo,
		IncoTerms.CostAndFreight,
		IncoTerms.CostAndInsurance,
		IncoTerms.CostFreightWithAmpersand,
		IncoTerms.CostInsuranceAndFreight,
		IncoTerms.DeliveredAtFrontier,
		IncoTerms.Other,
		string.Empty
	};
}
