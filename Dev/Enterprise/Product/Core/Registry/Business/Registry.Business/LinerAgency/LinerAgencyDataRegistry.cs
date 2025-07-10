using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class LinerAgencyDataRegistry : RegistryItemSet
	{
		#region Instance

		LinerAgencyDataRegistry()
		{
		}

		public static LinerAgencyDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new LinerAgencyDataRegistry();
				}
				return fInstance;
			}
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString LinerAgency { get { return ResString.GetMultilingualString("b2b06f63-54cf-4ef6-abfd-a36ba6c5cc35", "Liner & Agency"); } }
			public static MultilingualString LinerAgency_Compliance { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("689790A6-7DFE-4533-B9E0-22C075D91CC3", "Compliance")); } }
			public static MultilingualString LinerAgency_ContainerDetention { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("9c7bec59-12eb-4eef-b76b-414902adf020", "Container Detention")); } }
		}

		#endregion

		public CodeDescriptionPairListRegistryItem DisbursementSubGroups
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("DisbursementSubGroups", delegate
				{
					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();
					defaultValue.AddPair("POC", ResString.GetMultilingualString("26fc07dc-9e56-4c55-8162-44e73ab8bb97", "Port Charge"));
					defaultValue.AddPair("CCH", ResString.GetMultilingualString("62d4fe5d-8e4d-4594-84fc-f7aa52838d85", "Cargo Charge"));
					defaultValue.AddPair("AGF", ResString.GetMultilingualString("62e99255-e8b0-492d-9f1f-dd2dd18cf79f", "Agency Fee"));
					defaultValue.AddPair("ESO", ResString.GetMultilingualString("019abe8c-b1db-4664-8f04-55cc50171702", "Expense Of Ship Owners"));

					return new CodeDescriptionPairListRegistryItem(
						"DisbursementSubGroups",
						Categories.LinerAgency,
						ResString.GetMultilingualString("fc096e2b-cbbc-44b7-89a8-8d6882849914", "Voyage Accounting Disbursement Sub Groups"),
						ResString.GetMultilingualString("fc096e2b-cbbc-44b7-89a8-8d6882849914", "Voyage Accounting Disbursement Sub Groups"),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue);
				});
			}
		}

		public BooleanRegistryItem CreateWIPAccrualsForDetentionCharges
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CreateWIPAccrualsForDetentionCharges", delegate
				{
					return new BooleanRegistryItem(
						"CreateWIPAccrualsForDetentionCharges",
						Categories.LinerAgency_ContainerDetention,
						ResString.GetMultilingualString("53c2f0d6-710b-4fc6-ae57-b7cdef9a5a6f", "Detention Charges - WIP/Accrual Creation"),
						ResString.GetMultilingualString("9f2c6156-0208-4cc5-8509-f789ebde81b5", "Determines whether WIPs and Accruals should ever be created for the container detention module."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public GuidRegistryItem ImportDetentionDefaultDepartment
		{
			get
			{
				return GetItem<GuidRegistryItem>("ImportDetentionDefaultDepartment", delegate
				{
					return new GuidRegistryItem(
						"ImportDetentionDefaultDepartment",
						Categories.LinerAgency_ContainerDetention,
						ResString.GetMultilingualString("fa57a2bd-29a0-4a8d-ba84-dc1c0f007b3b", "Import Detention Default Department"),
						ResString.GetMultilingualString("50acb3a2-747b-4faf-bcbf-280102ccb159", "The default department to use for import container detention invoices."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new Guid("484D677D-CCC2-46B3-93DD-7C64C050351D"));
				});
			}
		}

		public GuidRegistryItem ExportDetentionDefaultDepartment
		{
			get
			{
				return GetItem<GuidRegistryItem>("ExportDetentionDefaultDepartment", delegate
				{
					return new GuidRegistryItem(
						"ExportDetentionDefaultDepartment",
						Categories.LinerAgency_ContainerDetention,
						ResString.GetMultilingualString("abbc2b71-b3cc-4834-90db-605ff9236569", "Export Detention Default Department"),
						ResString.GetMultilingualString("040938a3-26d4-49c9-bbe4-165896831916", "The default department to use for export container detention invoices."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new Guid("FE1F5E82-5F2C-40DF-B3D3-72B136A05CA6"));
				});
			}
		}

		public ChargeCodeRegistryItem ImportDetentionChargeCode
		{
			get
			{
				return GetItem<ChargeCodeRegistryItem>("ImportDetentionChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem(
						"ImportDetentionChargeCode",
						Categories.LinerAgency_ContainerDetention,
						ResString.GetMultilingualString("f8d9d216-97d9-4263-8319-64354c0130ed", "Import Detention Charge Code"),
						ResString.GetMultilingualString("e0161185-51a6-46ff-a805-78bed5768a6e", "The charge code to use for import detention charges."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"IDETS");

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem ExportDetentionChargeCode
		{
			get
			{
				return GetItem<ChargeCodeRegistryItem>("ExportDetentionChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem(
						"ExportDetentionChargeCode",
						Categories.LinerAgency_ContainerDetention,
						ResString.GetMultilingualString("43f6ab42-232f-4fee-baaf-a6b826903373", "Export Detention Charge Code"),
						ResString.GetMultilingualString("bef12f9c-2bbb-4598-9420-384dc062ff43", "The charge code to use for export detention charges."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"EDETS");

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public BooleanRegistryItem AllowSailingChangeWhenInvoiceIsPosted
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowSailingChangeWhenInvoiceIsPosted", () =>
				{
					return new BooleanRegistryItem(
						"AllowSailingChangeWhenInvoiceIsPosted",
						Categories.LinerAgency,
						ResString.GetMultilingualString("cb2d5ebe-8263-4742-a23b-c9845497c382", "Allow Sailing change when Invoice is posted"),
						ResString.GetMultilingualString("4bfa10fa-e797-43b8-9304-ac120e74ddf2", @"Override this value to allow users to Create/Select/Clear Sailing when invoices are posted.
Leave the setting as No to prevent roll of Booking/Bill of Lading to another vessel once charges have been posted (in which case Invoices will need to be reversed first in order to roll shipment to a different vessel)."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#region LinerAgencyEnableComplianceWise

		public EnableComplianceWiseRegistryItem LinerAgencyEnableComplianceWise
		{
			get
			{
				return GetItem("LinerAgencyEnableComplianceWise", delegate
				{
					return new EnableComplianceWiseRegistryItem("LinerAgencyEnableComplianceWise",
						Categories.LinerAgency_Compliance,
						ResString.GetMultilingualString("CCF55064-02B5-4A2C-8D68-2C3ED059D407", "Enable ComplianceWise"),
						ResString.GetMultilingualString("ADAF20D0-E8CD-4058-B55D-B731F4D73AFF", @"When disabled, ComplianceWise will not be available in the Liner & Agency module. 

When enabled, the Liner & Agency module will use the ComplianceWise system to perform compliance risk checks. Compliance workflows and other compliance capabilities will be available to use in these modules. 

Note, once enabled, ComplianceWise cannot be disabled on this module. Refer to WiseTech Academy eLearning to prepare and plan your transition to ComplianceWise."),
						RegistryStorageFlags.System,
						RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.IsOnlyForSupport : (RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden),
						new EnableComplianceWiseRegistryBusinessObject() { EnableComplianceWise = false });
				});
			}
		}

		#endregion

		#region Implementation

		[ThreadStatic]
		static LinerAgencyDataRegistry fInstance;

		#endregion
	}
}
