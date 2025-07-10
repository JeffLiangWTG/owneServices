using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(DsdtSdFormatUserControl))]
sealed class DsdtSdFormatUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(TemporaryStorageHeader), control.BindingSource.DataSourceType);
	}

	public void TestDsdtSdFormatNumberWhenHasMrnNumberBindingIsFalse()
	{
		var dsdtSdFormatNumberTextBox = control.DsdtSdFormatNumberTextBox;
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Type", dsdtSdFormatNumberTextBox);
			AssertEquals("IsReadOnly", true, dsdtSdFormatNumberTextBox.ReadOnly);
			dsdtSdFormatNumberTextBox.AssertThisControl(x => x.WithBindTo("DsdtMrnNumberSdFormat"));
		});
	}

	public void TestDsdtSdFormatNumberWhenHasMrnNumberBindingIsTrue()
	{
		using var controlHasMrnNumberBinding = new DsdtSdFormatUserControl(true, true);
		var dsdtSdFormatNumberTextBox = controlHasMrnNumberBinding.DsdtSdFormatNumberTextBox;
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Type", dsdtSdFormatNumberTextBox);
			AssertEquals("IsReadOnly set to false", false, dsdtSdFormatNumberTextBox.ReadOnly);
			dsdtSdFormatNumberTextBox.AssertThisControl(x => x.WithBindTo("DsdtMrnNumber"));
		});
	}

	public void TestGoToUrlButton()
	{
		var goToUrlButton = control.GoToUrlButton;
		CombineAssertions(() =>
		{
			AssertType<ZButton>("Type", goToUrlButton);
			AssertEquals("Background Image", Icons.GetImage(IconTypes.Globe20x16), goToUrlButton.BackgroundImage);
		});
	}

	public void TestGoToUrlButtonIsNotVisible()
	{
		using var controlGoToUrlIsHidden = new DsdtSdFormatUserControl(true, false);
		var goToUrlButton = controlGoToUrlIsHidden.GoToUrlButton;
		CombineAssertions(() =>
		{
			AssertEquals(false, goToUrlButton.Visible);
		});
	}

	public void TestGoToUrlButtonIsVisible()
	{
		using var controlGoToUrlIsHidden = new DsdtSdFormatUserControl(true, true);
		var goToUrlButton = controlGoToUrlIsHidden.GoToUrlButton;
		CombineAssertions(() =>
		{
			AssertEquals(true, goToUrlButton.Visible);
		});
	}

	public void TestGoToUrlButtonUrlValue()
	{
		var summaryUrl = "https://url.com?Recinto=%recinto%&Anio=%anio%&Numero=%numero%&MRN=%mrn%";
		using (ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, summaryUrl))
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			Factory.Save();
			header.Factory.SuspendValidation();

			AssertLaunchedUrl(string.Empty);

			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = header.PK;
			entryNumber.CE_ParentTable = header.TableName;
			entryNumber.CE_EntryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
			entryNumber.CE_EntryNum = "99994000152";
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			Factory.Save();
			var expectedUrl = "https://url.com?Recinto=9999&Anio=4&Numero=000152&MRN=";
			AssertLaunchedUrl(expectedUrl);

			void AssertLaunchedUrl(string expected)
			{
				using var form = new ZForm(header);
				using var control = new DsdtSdFormatUserControl(false, true);
				form.Controls.Add(control);
				form.Show();
				WebUrlLauncher.ClearLastUrlLaunched();
				control.GoToUrlButton.PerformClick();
				AssertEquals(expected, WebUrlLauncher.LastUrlLaunched);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new DsdtSdFormatUserControl(false, true);
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	DsdtSdFormatUserControl control;
}
