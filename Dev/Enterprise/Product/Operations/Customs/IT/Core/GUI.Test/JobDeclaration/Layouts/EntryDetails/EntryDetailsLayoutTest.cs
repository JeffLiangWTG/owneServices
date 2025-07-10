using System.Collections.Generic;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EntryDetailsLayout))]
sealed class EntryDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestCaptions()
	{
		var layout = GetLayout();

		CombineAssertions(() =>
		{
			AssertCaption(itBag.EntryTypeTextBox, expectedCaption: "Entry Type");
			AssertCaption(itBag.ControlChannelDropEdit, expectedCaption: "Control Channel");
			AssertCaption(itBag.RegistrationNumberTextBox, expectedCaption: "Registration No.");
			AssertCaption(itBag.ExitDateDateEdit, expectedCaption: "Exit Date");
			AssertCaption(itBag.SubmittedDateDateEdit, expectedCaption: "Submitted Date");
			AssertCaption(itBag.MRNTextBox, expectedCaption: "MRN");
			AssertCaption(itBag.ReleaseDateDateEdit, expectedCaption: "Release Date");
			AssertCaption(itBag.EntryStatusDropEdit, expectedCaption: "Entry Status");
			AssertCaption(itBag.ReleaseCodeTextBox, expectedCaption: "Reference Number");
		});

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCaption(itBag.ReleaseCodeTextBox, expectedCaption: "Release Code");
		}

		void AssertCaption(ControlReference controlReference, string expectedCaption)
		{
			layout.TryGetCaption(controlReference, declaration, out var captionData);
			AssertEquals($"Caption for {controlReference.ControlName}", expectedCaption, captionData?.Caption);
		}
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryDetailsLayoutBuilder();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (itBag.ReferenceLabel, ControlWidthClass.LongNoCaption);
			yield return (itBag.EntryTypeTextBox, ControlWidthClass.Auto);
			yield return (itBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			yield return (itBag.IssueDateDateEdit, ControlWidthClass.Auto);
			yield return (itBag.IncotermTextBox, ControlWidthClass.Auto);
			yield return (itBag.TotalsLabel, ControlWidthClass.LongNoCaption);
			yield return (commonBag.NoPacksCalcEdit, ControlWidthClass.Auto);
			yield return (itBag.GrossWeightUserControl, ControlWidthClass.Auto);
			yield return (itBag.NetWeightUserControl, ControlWidthClass.Auto);
			yield return (itBag.CustomsQuantityUserControl, ControlWidthClass.Auto);
			yield return (itBag.InvoiceAmountUserControl, ControlWidthClass.Auto);
			yield return (itBag.FreightAdjustmentCalcEdit, ControlWidthClass.Auto);
			yield return (commonBag.DutyCalcEdit, ControlWidthClass.Auto);
			yield return (commonBag.VatCalcEdit, ControlWidthClass.Auto);
			yield return (commonBag.EntryLinesCountCalcEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (itBag.StatusLabel, ControlWidthClass.LongNoCaption);
			yield return (itBag.MessageStatusUserControl, ControlWidthClass.Auto);
			yield return (itBag.EntryStatusDropEdit, ControlWidthClass.Auto);
			yield return (itBag.ControlChannelDropEdit, ControlWidthClass.Auto);
			yield return (itBag.MessageTypeDropEdit, ControlWidthClass.Auto);
			yield return (itBag.WarehouseStatusDropEdit, ControlWidthClass.Auto);
			yield return (itBag.CustomsLabel, ControlWidthClass.LongNoCaption);
			yield return (itBag.RegistrationNumberTextBox, ControlWidthClass.Auto);
			yield return (itBag.CustomsOfficeTextBox, ControlWidthClass.Auto);
			yield return (itBag.SubmittedDateDateEdit, ControlWidthClass.Auto);
			yield return (itBag.MRNTextBox, ControlWidthClass.Auto);
			yield return (itBag.ReleaseCodeTextBox, ControlWidthClass.Auto);
			yield return (itBag.ReleaseDateDateEdit, ControlWidthClass.Auto);
			yield return (itBag.A93Label, ControlWidthClass.LongNoCaption);
			yield return (itBag.A93Grid, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (itBag.ExitLabel, ControlWidthClass.LongNoCaption);
			yield return (itBag.ExitDateDateEdit, ControlWidthClass.Auto);
			yield return (itBag.ExitOfficeUserControl, ControlWidthClass.Auto);
			yield return (itBag.ExitStatusUserControl, ControlWidthClass.Auto);
		}
	}

	EntryDetailsControlBag itBag => EntryDetailsControlBag.Instance;

	CommonEntryDetailsControlBag commonBag => CommonEntryDetailsControlBag.Instance;

	IPanelLayoutProvider GetLayoutProvider() => new EntryDetailsLayout();

	PanelLayout GetLayout() => GetLayoutProvider()?.Layout;

	JobDeclaration declaration;
}
