using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ShipmentDetailsLayout))]
sealed class ShipmentDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestPaymentMethodUserControl_Visibility()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP PaymentMethodUserControl Visible", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.PaymentMethodUserControl, declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("EXP PaymentMethodUserControl Visible", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.PaymentMethodUserControl, declaration));
	}

	public void TestVatPaidByUserControl_Visibility()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP VatPaidByUserControl Visible", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.VatPaidByUserControl, declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("EXP VatPaidByUserControl Visible", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.VatPaidByUserControl, declaration));
	}

	public void TestClearanceLocationDropEdit_Visibility()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP ClearanceLocationDropEdit Visible", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ClearanceLocationDropEdit, declaration));

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		AssertEquals("EXP ClearanceLocationDropEdit Visible", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.ClearanceLocationDropEdit, declaration));
	}

	public void TestDeclarationLanguageDropEdit_Visibility()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP DeclarationLanguageDropEdit Visible", true, Layout.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.DeclarationLanguageDropEdit, declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("EXP DeclarationLanguageDropEdit Visible", true, Layout.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.DeclarationLanguageDropEdit, declaration));
	}

	public void TestAdditionalDecisionInfoCheckBox_Visibility()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("IMP AdditionalDecisionInfoCheckBox Visible", true, Layout.IsVisible(ShipmentDetailsControlBag.Instance.AdditionalDecisionInfoCheckBox, declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("EXP AdditionalDecisionInfoCheckBox Visible", false, Layout.IsVisible(ShipmentDetailsControlBag.Instance.AdditionalDecisionInfoCheckBox, declaration));
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.PaymentMethodUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.VatPaidByUserControl, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.ClearanceLocationDropEdit, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.LocationOfGoodsDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.DeclarationLanguageDropEdit, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.AdditionalDecisionInfoCheckBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 2;

	PanelLayout Layout => layout ?? (layout = new ShipmentDetailsLayout().Layout);
	PanelLayout layout;

	JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentDetailsLayoutBuilder<JobDeclaration>();

	JobDeclaration declaration;
}
