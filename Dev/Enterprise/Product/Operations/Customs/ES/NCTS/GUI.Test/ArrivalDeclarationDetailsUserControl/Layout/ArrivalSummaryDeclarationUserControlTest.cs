using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.ES.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalSummaryDeclarationUserControl))]
sealed class ArrivalSummaryDeclarationUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
	}

	public void TestSummaryDeclarationTextBox()
	{
		var textbox = control.SummaryDeclarationTextBox;
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Type", textbox);
			AssertEquals("BindTo", nameof(NctsHeader.ArrivalSummaryDeclaration), textbox.BindTo);
		});
	}

	public void TestSummaryDeclarationGoToUrlButton()
	{
		AssertType<ZButton>("Type", control.GoToUrlButton);
	}

	public void TestSummaryDeclarationUrlButtonVisibility() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.Factory.Save();
		nctsHeader.Factory.SuspendValidation();

		AssertVisibility("Has no DSDT", false);

		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = SummaryEntryNumber;
		newEntryNumber.CE_IssueDate = ZDateTime.Today;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		nctsHeader.Factory.Save();

		AssertVisibility("Has DSDT", true);

		void AssertVisibility(string message, bool expectedVisibility)
		{
			using var form = new ZForm(nctsHeader);
			var control = new ArrivalSummaryDeclarationUserControl();
			form.Controls.Add(control);
			form.Show();
			AssertEquals(message, expectedVisibility, control.GoToUrlButton.Visible);

			if(!expectedVisibility)
			{
				AssertEquals(message, control.GoToUrlButton.Location.X + control.GoToUrlButton.Width - control.SummaryDeclarationTextBox.Location.X, control.SummaryDeclarationTextBox.Width);
			}
		}
	});

	public void TestSummaryDeclarationGoToUrlButtonClick() => CombineAssertions(() =>
	{
		var summaryUrl = "https://url.com?Recinto=%recinto%&Anio=%anio%&Numero=%numero%&MRN=%mrn%";
		using (ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, summaryUrl))
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.Factory.Save();
			nctsHeader.Factory.SuspendValidation();

			AssertLaunchedUrl(string.Empty);

			var newEntryNumberSummary = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumberSummary.CE_EntryNum = "99982000174";
			newEntryNumberSummary.CE_EntryIsSystemGenerated = true;
			var newEntryNumberMrn = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumberMrn.CE_EntryNum = "20ES00999830001277";
			newEntryNumberMrn.CE_EntryIsSystemGenerated = true;
			Factory.Save();

			var expectedUrl = "https://url.com?Recinto=9998&Anio=2&Numero=000174&MRN=";
			AssertLaunchedUrl(expectedUrl);

			void AssertLaunchedUrl(string expected)
			{
				using var form = new ZForm(nctsHeader);
				using var control = new ArrivalSummaryDeclarationUserControl();
				form.Controls.Add(control);
				form.Show();
				WebUrlLauncher.ClearLastUrlLaunched();
				control.GoToUrlButton.PerformClick();
				AssertEquals(expected, WebUrlLauncher.LastUrlLaunched);
			}
		}
	});

	protected override void SetUp()
	{
		base.SetUp();
		control = new ArrivalSummaryDeclarationUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ArrivalSummaryDeclarationUserControl control;

	const string SummaryEntryNumber = "99982000174";
}

