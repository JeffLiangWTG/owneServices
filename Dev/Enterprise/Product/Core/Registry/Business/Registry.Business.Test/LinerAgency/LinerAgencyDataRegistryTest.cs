using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LinerAgencyDataRegistry))]
	sealed class LinerAgencyDataRegistryTest : RegistryItemSetTestCaseWithFactory<LinerAgencyDataRegistry>
	{
		public void TestDisbursementSubGroups()
		{
			TestGenericRegistryItem(
				ItemSet.DisbursementSubGroups,
				"DisbursementSubGroups",
				"Liner & Agency",
				"Voyage Accounting Disbursement Sub Groups",
				"Voyage Accounting Disbursement Sub Groups",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);

			const string expected =
				"POC - Port Charge\n" +
				"CCH - Cargo Charge\n" +
				"AGF - Agency Fee\n" +
				"ESO - Expense Of Ship Owners" +
				"";

			AssertMultilineASCIIEquals("", expected, ItemSet.DisbursementSubGroups.Value.ElementsAsString);
		}

		public void TestDetentionDefaultDepartment()
		{
			TestGenericRegistryItem(ItemSet.ImportDetentionDefaultDepartment,
				"ImportDetentionDefaultDepartment",
				"Liner & Agency/Container Detention",
				"Import Detention Default Department",
				"The default department to use for import container detention invoices.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default | RegistryOptions.IsValueMandatory,
				ItemSet.ImportDetentionDefaultDepartment.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			BusinessObject sid = (BusinessObject)Factory.Load<Enterprise.Integration.IGlbDepartment>(ItemSet.ImportDetentionDefaultDepartment.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("SID", sid == null ? null : sid[GlbDepartmentSchema.GE_Code]);

			TestGenericRegistryItem(ItemSet.ExportDetentionDefaultDepartment,
				"ExportDetentionDefaultDepartment",
				"Liner & Agency/Container Detention",
				"Export Detention Default Department",
				"The default department to use for export container detention invoices.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default | RegistryOptions.IsValueMandatory,
				ItemSet.ExportDetentionDefaultDepartment.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			BusinessObject sed = (BusinessObject)Factory.Load<Enterprise.Integration.IGlbDepartment>(ItemSet.ExportDetentionDefaultDepartment.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("SED", sed == null ? null : sed[GlbDepartmentSchema.GE_Code]);
		}

		public void TestDetentionChargeCodes()
		{
			TestGenericRegistryItem(ItemSet.ImportDetentionChargeCode,
				"ImportDetentionChargeCode",
				"Liner & Agency/Container Detention",
				"Import Detention Charge Code",
				"The charge code to use for import detention charges.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ItemSet.ImportDetentionChargeCode.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			AssertEquals("IDETS", ItemSet.ImportDetentionChargeCode.DefaultChargeCode);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.ImportDetentionChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);

			TestGenericRegistryItem(ItemSet.ExportDetentionChargeCode,
				"ExportDetentionChargeCode",
				"Liner & Agency/Container Detention",
				"Export Detention Charge Code",
				"The charge code to use for export detention charges.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ItemSet.ExportDetentionChargeCode.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			AssertEquals("EDETS", ItemSet.ExportDetentionChargeCode.DefaultChargeCode);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.ExportDetentionChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
		}

		public void TestDefaultCreateWIPAccrualsForDetentionCharges()
		{
			TestGenericRegistryItem(ItemSet.CreateWIPAccrualsForDetentionCharges,
				"CreateWIPAccrualsForDetentionCharges",
				"Liner & Agency/Container Detention",
				"Detention Charges - WIP/Accrual Creation",
				"Determines whether WIPs and Accruals should ever be created for the container detention module.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestAllowSailingChangeWhenInvoiceIsPosted()
		{
			TestRegistryItem(
				ItemSet.AllowSailingChangeWhenInvoiceIsPosted,
				"AllowSailingChangeWhenInvoiceIsPosted",
				"Liner & Agency",
				"Allow Sailing change when Invoice is posted",
				@"Override this value to allow users to Create/Select/Clear Sailing when invoices are posted.
Leave the setting as No to prevent roll of Booking/Bill of Lading to another vessel once charges have been posted (in which case Invoices will need to be reversed first in order to roll shipment to a different vessel).",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		#region LinerAgencyEnableComplianceWise

		public void TestLinerAgencyEnableComplianceWise()
		{
			AssertEquals("Name", "LinerAgencyEnableComplianceWise", ItemSet.LinerAgencyEnableComplianceWise.Name);
			AssertEquals("Categories.Length", 1, ItemSet.LinerAgencyEnableComplianceWise.Categories.Length);
			AssertEquals("Category", "Liner & Agency/Compliance", ItemSet.LinerAgencyEnableComplianceWise.Category);
			AssertEquals("Caption", "Enable ComplianceWise", ItemSet.LinerAgencyEnableComplianceWise.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LinerAgencyEnableComplianceWise.Storage);
			AssertEquals("Hint", @"When disabled, ComplianceWise will not be available in the Liner & Agency module. 

When enabled, the Liner & Agency module will use the ComplianceWise system to perform compliance risk checks. Compliance workflows and other compliance capabilities will be available to use in these modules. 

Note, once enabled, ComplianceWise cannot be disabled on this module. Refer to WiseTech Academy eLearning to prepare and plan your transition to ComplianceWise.", ItemSet.LinerAgencyEnableComplianceWise.Hint);

			AssertEquals("Registry options", (RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.IsOnlyForSupport : (RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden)), ItemSet.LinerAgencyEnableComplianceWise.Options);
			AssertEquals("Registry visible", RawDataRegistry.Instance.EnableComplianceRisk.Value, ItemSet.LinerAgencyEnableComplianceWise.IsVisible(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		#endregion

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return nameof(LinerAgencyDataRegistry.LinerAgencyEnableComplianceWise);
			}
		}
	}
}
