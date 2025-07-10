using System;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(ITCustomsDataRegistry))]
sealed class ITCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<ITCustomsDataRegistry>
{
	public void TestITRecipientID()
	{
		TestGenericRegistryItem(
			ItemSet.ITRecipientID,
			"ITRecipientID",
			CustomsDataRegistry.Categories.Customs_Italy,
			"Recipient ID",
			"Recipient ID",
			RegistryStorageFlags.System,
			RegistryOptions.IsValueMandatory,
			"ITCustomsTest"
		);
	}

	public void TestITGeneratePortTaxes()
	{
		TestGenericRegistryItem(
			ItemSet.ITGeneratePortTaxes,
			"ITGeneratePortTaxes",
			CustomsDataRegistry.Categories.Customs_Italy,
			"Generate Port Taxes",
			"Generate Port Taxes",
			RegistryStorageFlags.Company,
			RegistryOptions.IsValueMandatory,
			true
		);
	}

	public void TestIsPortTaxesCalculationEnabled()
	{
		using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: true))
		{
			AssertEquals(nameof(ITCustomsDataRegistry.Instance.IsPortTaxesCalculationEnabled), true, ITCustomsDataRegistry.Instance.IsPortTaxesCalculationEnabled);
		}

		using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: false))
		{
			AssertEquals(nameof(ITCustomsDataRegistry.Instance.IsPortTaxesCalculationEnabled), false, ITCustomsDataRegistry.Instance.IsPortTaxesCalculationEnabled);
		}
	}

	public void TestIsForProductivityWise()
	{
		AssertEquals(false, ITCustomsDataRegistry.Instance.IsForProductivityWise);
	}

	public void TestExportMessageVersion()
	{
		TestRegistryItem(
			ItemSet.ExportMessageVersion,
			"ExportMessageVersion",
			CustomsDataRegistry.Categories.Customs_Italy,
			"Export Message Version",
			"The field below allows to choose the Message Version for Export declarations.",
			RegistryStorageFlags.Company,
			new ExportMessageVersionList(),
			ExportMessageVersionList.Codes.TXT);
	}

	public void TestIsExportMessageVersionEnabled()
	{
		AssertExportMessageVersionEnabledWithExportMessageVersionValue("TXT", false);
		AssertExportMessageVersionEnabledWithExportMessageVersionValue("XML", false);
		AssertExportMessageVersionEnabledWithExportMessageVersionValue("BTX", true);
		AssertExportMessageVersionEnabledWithExportMessageVersionValue("BXM", true);

		void AssertExportMessageVersionEnabledWithExportMessageVersionValue(string exportMessageVersion, bool expectedEnabled)
		{
			using (ITCustomsDataRegistry.Instance.ExportMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: exportMessageVersion))
			{
				AssertEquals($"For {exportMessageVersion} - {nameof(ITCustomsDataRegistry.Instance.IsExportMessageVersionEnabled)}", expectedEnabled, ITCustomsDataRegistry.Instance.IsExportMessageVersionEnabled);
			}
		}
	}

	public void TestExportMessageVersionList()
	{
		var exportMessageVersionList = new ExportMessageVersionList();
		AssertEquals("Code as string", "BTX, BXM, TXT, XML", exportMessageVersionList.CodesAsString);
	}

	public void TestBondedWarehouseNotificationGroup()
	{
		var bondedWarehouseNotificationGroup = ItemSet.BondedWarehouseNotificationGroup;

		CombineAssertions(() =>
		{
			TestGenericRegistryItem(
				bondedWarehouseNotificationGroup,
				"ITBondedWarehouseNotificationGroup",
				ITCustomsDataRegistry.Categories.Customs_Italy_Notifications,
				"Bonded Warehouse Notification group",
				"Bonded Warehouse movements notification settings.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory,
				Guid.Empty);
			AssertType<NotificationGroupGuidRegistryDataType>("DataType Type", bondedWarehouseNotificationGroup.DataType);
			AssertContainsExactElementsInAnyOrder("CountryFilterPKs Elements", new[] { Constants.CountryGuids.Italy }, bondedWarehouseNotificationGroup.CountryFilterPKs);
		});
	}

	public void TestUseUCMPForCategoryITC()
	{
		TestGenericRegistryItem(
			ItemSet.UseUCMPForCategoryITC,
			"UseUCMPForCategoryITC",
			RawDataRegistry.Categories.Customs_Italy,
			"Use UCMP For Category ITC",
			"If 'Yes', all Service Tasks with ITC category will be processed by UCMP Service Tasks instead of IT Service Tasks ('ITR', 'ITP', 'ITS').",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForDevelopers,
			false);
	}
}
