using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(MiscOptionsLayout))]
sealed class MiscOptionsLayoutTest : LayoutsAbstractTest
{
	public void TestFieldsVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CombineAssertions("When Declaration Is Import", () =>
		{
			AssertEquals("AuthPerDeclaration CheckBox", true, Layout.IsVisible(MiscOptionsControlBag.Instance.AuthPerDeclarationCheckBox, declaration));
			AssertEquals("DeclEmailAddr TextBox", true, Layout.IsVisible(MiscOptionsControlBag.Instance.DeclEmailAddrTextBox, declaration));
			AssertEquals("DontSendImporterId CheckBox", false, Layout.IsVisible(MiscOptionsControlBag.Instance.DontSendImporterIdCheckBox, declaration));
			AssertEquals("ItineraryCountriesSeparator UserControl", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
			AssertEquals("ItineraryCountries UserControl", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		CombineAssertions("When Declaration Is Export", () =>
		{
			AssertEquals("AuthPerDeclaration CheckBox", true, Layout.IsVisible(MiscOptionsControlBag.Instance.AuthPerDeclarationCheckBox, declaration));
			AssertEquals("DeclEmailAddr TextBox", true, Layout.IsVisible(MiscOptionsControlBag.Instance.DeclEmailAddrTextBox, declaration));
			AssertEquals("DontSendImporterId CheckBox", true, Layout.IsVisible(MiscOptionsControlBag.Instance.DontSendImporterIdCheckBox, declaration));
			AssertEquals("ItineraryCountriesSeparator UserControl", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
			AssertEquals("ItineraryCountries UserControl", true, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		CombineAssertions("When Declaration Is MiscellaneousCustoms", () =>
		{
			AssertEquals("AuthPerDeclaration CheckBox", false, Layout.IsVisible(MiscOptionsControlBag.Instance.AuthPerDeclarationCheckBox, declaration));
			AssertEquals("DeclEmailAddr TextBox", false, Layout.IsVisible(MiscOptionsControlBag.Instance.DeclEmailAddrTextBox, declaration));
			AssertEquals("DontSendImporterId CheckBox", false, Layout.IsVisible(MiscOptionsControlBag.Instance.DontSendImporterIdCheckBox, declaration));
			AssertEquals("ItineraryCountriesSeparator UserControl", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, declaration));
			AssertEquals("ItineraryCountries UserControl", false, layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, declaration));
		});
	}

	public void TestTrainingCheckBoxVisibility()
	{
		var productRegistrationMock = new Mock<IProductRegistration>();
		var keyMock = new Mock<IProductRegistrationKey>();
		productRegistrationMock.Setup(m => m.Key).Returns(keyMock.Object);

		productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
		using (ObjectFactory.Substitute(productRegistrationMock.Object))
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("TrainingCheckBox not visible when not internal environment", false, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.TrainingCheckBox, declaration));
		}

		productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
		using (ObjectFactory.Substitute(productRegistrationMock.Object))
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("TrainingCheckBox visible when internal environment", true, Layout.IsVisible(EU.GUI.MiscOptionsControlBag.Instance.TrainingCheckBox, declaration));
		}
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 3;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (Customs.GUI.CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
			yield return (Customs.GUI.CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.CertificateDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPInspectDateEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.LCPDepartDateEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.CommonMiscOptionsControlBag.Instance.EntryAuthorisationDateEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.AuthPerDeclarationCheckBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.DeclEmailAddrTextBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.OtherEmailAddrTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.RouteFRequestedCheckBox, ControlWidthClass.Long);
			yield return (MiscOptionsControlBag.Instance.DontSendImporterIdCheckBox, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.DeferralSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.MiscOptionsControlBag.Instance.ItineraryCountriesUserControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.GUI.MiscOptionsControlBag.Instance.RelatedDeclarationsUserControl, ControlWidthClass.LongControl);
			yield return (MiscOptionsControlBag.Instance.SupportingInformationUserControl, ControlWidthClass.LongControl);
		}
	}

	PanelLayout Layout => layout ?? (layout = new MiscOptionsLayout().Layout);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.MiscOptionsLayoutBuilder<JobDeclaration>();

	PanelLayout layout;
}
