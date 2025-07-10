using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	[CodeProperty(KREntryHeaderDetailsView.Schema.KEH_EntryNum), DescriptionProperty(KREntryHeaderDetailsView.Schema.KEH_EntryNum)]
	public partial class KREntryHeaderDetailsView : AutoKREntryHeaderDetailsView
	{
		public KREntryHeaderDetailsView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZQuery GetQueryFor5SG(IEnumerable<ZString> entryNumbers = null)
			{
				var result = new ZQuery(KREntryHeaderDetailsViewSchema.KEH_EntryNumIssueDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
				result.AddToFilter(KREntryHeaderDetailsViewSchema.KEH_EstimatedDateOfFinalPrice, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
				if (entryNumbers != null)
				{
					result.AddToFilter(KREntryHeaderDetailsViewSchema.KEH_EntryNum, entryNumbers);
				}
				result.AddToFilter(KREntryHeaderDetailsViewSchema.KEH_MessageStatus, SQLComparisonOperator.NotEqual, new string[] { CustomsMessageStatusTypeList.Codes.CancellationByCustoms, CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms });
				return result;
			}

			public ZQuery GetQueryFor5ACAnd5GW()
			{
				return new ZQuery(KREntryHeaderDetailsViewSchema.KEH_MessageStatus, CustomsMessageStatusTypeList.GetCodesToAllowOriginalMessage());
			}

			public ZQuery GetQueryFor5UL()
			{
				var result = new ZQuery(KREntryHeaderDetailsViewSchema.KEH_EntryReleaseDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
				//result.AddToFilter(KREntryHeaderDetailsViewSchema.KEH_MessageStatus, SQLComparisonOperator.NotEqual, new string[] { CustomsMessageStatusTypeList.Codes.CancellationByCustoms, CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms });
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(KREntryHeaderDetailsView);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new KREntryHeaderDetailsViewFetchHintStrategy(this);

		[ResourceStringData("6BE3F696-FD53-4A39-AC8D-FFA878E3315C", Caption = "Entry Number")]
		public ZString FormattedEntryNum => MessageFunctions.DeclarationNumberFormat(KEH_EntryNum);
		[DecimalPlaces(0)]
		[ResourceStringData("83718D76-6DBD-48C5-9231-A6C83D399F17", Caption = "Total Packages")]
		public override ZDecimal KEH_ExportPackQty { get => base.KEH_ExportPackQty; set => base.KEH_ExportPackQty = value; }
		[DecimalPlaces(0)]
		[ResourceStringData("B29A8D89-4883-4998-8096-F876F2BAE35D", Caption = "Customs Value (KRW)")]
		public override ZDecimal KEH_TotalCustomsValueInKRW { get => base.KEH_TotalCustomsValueInKRW; set => base.KEH_TotalCustomsValueInKRW = value; }
		[DecimalPlaces(3)]
		[ResourceStringData("3D9B6240-7547-4111-AAE8-414C6FA3D4D8", Caption = "Total Gross Weight (KG)")]
		public override ZDecimal KEH_TotalWeightInKG { get => base.KEH_TotalWeightInKG; set => base.KEH_TotalWeightInKG = value; }
		[ResourceStringData("A4D8E51A-6358-424D-8695-6EBCB7CA57E1", Caption = "Supplier")]
		public override ZGuid KEH_OA_SupplierAddress { get => base.KEH_OA_SupplierAddress; set => base.KEH_OA_SupplierAddress = value; }
		[ResourceStringData("F8AA118D-E3FF-4436-93A0-1D123A2EED71", Caption = "Supplier Company Name")]
		public override ZString KEH_SupplierName { get => base.KEH_SupplierName; set => base.KEH_SupplierName = value; }
		[ResourceStringData("029F8AB2-2EED-4663-AA08-9E5A5D2496AA", Caption = "Declaration Date (Local)")]
		public override ZDateTime KEH_EntryNumIssueDate { get => base.KEH_EntryNumIssueDate; set => base.KEH_EntryNumIssueDate = value; }
		[ResourceStringData("791F008C-849C-48E1-878F-FB35966C9327", Caption = "Entry Created Date")]
		public override ZDateTime KEH_EntryCreatedLocalTime { get => base.KEH_EntryCreatedLocalTime; set => base.KEH_EntryCreatedLocalTime = value; }
		[ResourceStringData("C6D00B0C-75AE-436E-8497-1E9BC83BE787", Caption = "Accepted Date")]
		public ZDateTime EntryNumIssueDateFor5SG => KEH_EntryNumIssueDate;

		[ResourceStringData("B20BC7D7-6BE8-4DFA-9532-5652AD33CD6E", Caption = "Cleared Date")]
		public override ZDateTime KEH_EntryReleaseDate { get => base.KEH_EntryReleaseDate; set => base.KEH_EntryReleaseDate = value; }

		[ResourceStringData("474BA651-DDE4-4C11-BA4F-DEB595C7076F", Caption = "Payer")]
		[List(nameof(Lookups) + "." + nameof(KREntryHeaderDetailsViewLookups.ConsigneeList))]
		public override ZGuid KEH_OH_Payer { get => base.KEH_OH_Payer; set => base.KEH_OH_Payer = value; }

		[ResourceStringData("{3BE0BCD9-7C9B-42A1-B47D-5689784C98BF}", Caption = "Payer Company Name")]
		public override ZString KEH_PayerName { get => base.KEH_PayerName; set => base.KEH_PayerName = value; }

		[ResourceStringData("237BF386-4B0D-4BC6-B88E-0363B0D9FBAA", Caption = "Total Payable Amount")]
		public override ZDecimal KEH_TotalPaid { get => base.KEH_TotalPaid; set => base.KEH_TotalPaid = value; }

		[ResourceStringData("A743AB3C-3F71-469C-93CE-98A0E9D64C74", Caption = "Estimated Date Of Final Price")]
		public override ZDateTime KEH_EstimatedDateOfFinalPrice { get => base.KEH_EstimatedDateOfFinalPrice; set => base.KEH_EstimatedDateOfFinalPrice = value; }

		[ResourceStringData("091F3D7A-1DD5-4096-AF53-106EA60026D6", Caption = "Contact Expiration Date")]
		public override ZDate KEH_ContractExpirationDate { get => base.KEH_ContractExpirationDate; set => base.KEH_ContractExpirationDate = value; }
		[ResourceStringData("6C3D14DD-CF48-4224-B3B6-FDD5070C88FC", Caption = "Provisional Additional Rate")]
		public override ZDecimal KEH_ProvAdditionalRate { get => base.KEH_ProvAdditionalRate; set => base.KEH_ProvAdditionalRate = value; }

		[ResourceStringData("E9868E63-DE79-4479-9C2F-2FA4AC8C95E4", Caption = "Provisional Additional Amount")]
		public override ZDecimal KEH_TotalProvAdditionalAmount { get => base.KEH_TotalProvAdditionalAmount; set => base.KEH_TotalProvAdditionalAmount = value; }

		[ResourceStringData("0A326850-9568-48B2-A01A-BF7B86612738", Caption = "Importer")]
		[List(nameof(Lookups) + "." + nameof(KREntryHeaderDetailsViewLookups.ConsigneeList))]
		public override ZGuid KEH_OH_Importer { get => base.KEH_OH_Importer; set => base.KEH_OH_Importer = value; }

		[ResourceStringData("CF55987B-0604-45A7-B434-8DF671EBA6A1", Caption = "Importer Company Name")]
		public override ZString KEH_ImporterName { get => base.KEH_ImporterName; set => base.KEH_ImporterName = value; }

		public OrgAddress SupplierAddress => supplierAddress ??= Factory.Load<OrgAddress>(KEH_OA_SupplierAddress);
		OrgAddress supplierAddress;
		[List(nameof(Lookups) + "." + nameof(KREntryHeaderDetailsViewLookups.OrganisationList))]
		[ResourceStringData("AC160B52-1941-404F-B95D-983682E44457", Caption = "Supplier")]
		public ZGuid Supplier => SupplierAddress?.Header.PK ?? ZGuid.Empty;

		public OrganizationDocWrapper Payer => payer ?? (payer = new OrganizationDocWrapper(Factory.Load<OrgHeader>(KEH_OH_Payer)));
		OrganizationDocWrapper payer;

		[ResourceStringData("4A5CBDBF-53CC-492D-A36B-FD70CC420D39", Caption = "House Bill")]
		public override ZString KEH_BillNum { get => base.KEH_BillNum; set => base.KEH_BillNum = value; }

		[ResourceStringData("B74A83A4-CBDB-4F3E-8754-6B8EBC028783", Caption = "Cargo Management No.")]
		public override ZString KEH_CargoManagementNumber { get => base.KEH_CargoManagementNumber; set => base.KEH_CargoManagementNumber = value; }

		[ResourceStringData("9BC70615-4C06-459F-A7B8-C5B6F1F0A125", Caption = "Tariff Description")]
		public override ZString KEH_HSDescription { get => base.KEH_HSDescription; set => base.KEH_HSDescription = value; }

		[ResourceStringData("532E1738-E2B1-485A-9F2E-5685201F5D6C", Caption = "Total Packages")]
		public override ZInt KEH_ImportPackQty { get => base.KEH_ImportPackQty; set => base.KEH_ImportPackQty = value; }

		[ResourceStringData("24BE30A6-B6AB-48AD-BB8F-E13D04143F13", Caption = "Bonded Area Code")]
		public override ZString KEH_BondedAreaCode { get => base.KEH_BondedAreaCode; set => base.KEH_BondedAreaCode = value; }
	}
}
