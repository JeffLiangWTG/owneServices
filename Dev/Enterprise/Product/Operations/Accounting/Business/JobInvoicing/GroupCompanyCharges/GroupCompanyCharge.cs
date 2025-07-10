using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Used only for binding in DebtorsAcceptGroupChargesForm.
	/// </summary>
	public class GroupCompanyCharge : NonPersistentBusinessObject<GroupCompanyChargeValidation>
	{
		public GroupCompanyCharge(Charge costCharge, Charge sellCharge, AcceptAction acceptAction, BusinessObjectFactory factory)
			: base(factory)
		{
			GroupCompanySellCharge = sellCharge;
			GroupCompanyCostCharge = costCharge;
			AcceptActionForCost = acceptAction;
		}

		#region Properties

		public Charge GroupCompanySellCharge { get; }
		public Charge GroupCompanyCostCharge { get; }
		public AcceptAction AcceptActionForCost { get; }

		public enum AcceptAction
		{
			NoAction,
			Create,
			Update
		}

		[ResourceStringData("b63385d4-15b0-4adf-b46a-08e11d2f49bb", Caption = "Accept Action", FullDescription = "The action undertaken when accepting this Group Company Charge.")]
		public ZString AcceptActionDescription
		{
			get
			{
				switch (AcceptActionForCost)
				{
					case AcceptAction.Create:
						return Res.GetString("630eb338-2bd8-4fcd-9e4d-3b0852c81dbe", "Create");

					case AcceptAction.Update:
						return Res.GetString("3808c869-09c9-44f1-96d7-22be9775ce8c", "Update");

					case AcceptAction.NoAction:
						return Res.GetString("cea1513b-d2b5-41c9-b4db-54332462c0d2", "No Action");

					default:
						return Res.GetString("4a450bcf-e6ba-4e82-b5d3-1070adfcf8cc", "Invalid");
				}
			}
		}

		public ZPropertyInfo AcceptActionDescriptionInfo => GetZPropertyInfo(nameof(AcceptActionDescription));

		[List("GroupCompanySellCharge.Lookups.ChargeCodes")]
		[ResourceStringData("a0330aca-72a1-4759-8baf-f00703615ef9", ShortCaption = "Group Com. Chrg.", Caption = "Group Company Charge Code")]
		public ZGuid JR_AC => GroupCompanySellCharge.JR_AC;
		public ZPropertyInfo JR_ACInfo => GetZPropertyInfo(nameof(JR_AC));

		[List("GroupCompanySellCharge.Lookups.ChargeCodes")]
		[RelatedBusinessObject("CostCompanyChargeCode")]
		[ResourceStringData("275d923d-d88b-478b-9966-6049abdf3433", ShortCaption = "Local Chrg.", Caption = "Local Charge Code", FullDescription = "Group Company Charge's Code is mapped to Local Company's Charge Code with Intercompany Mapping or Global Charge.")]
		public ZGuid CostCompanyChargeCodePK => Factory.GetCachedValue("CostCompanyChargeCodePK" + GroupCompanySellCharge?.JR_AC, GetMappedChargeCode);
		public ZPropertyInfo CostCompanyChargeCodePKInfo => GetZPropertyInfo(nameof(CostCompanyChargeCodePK));

		public AccChargeCode CostCompanyChargeCode => CostCompanyChargeCodePK.IsEmpty ? null : Factory.Load<AccChargeCode>(CostCompanyChargeCodePK);

		ZGuid GetMappedChargeCode()
		{
			if (GroupCompanySellCharge?.ChargeCode != null)
			{
				var mappedGlobalChargeCode = GroupCompanySellCharge.ChargeCode.GetGlobalChargeCode(LedgerTypes.AccountsReceivable, null);
				var mappedLocalChargeCode = mappedGlobalChargeCode.IsGlobal
					? mappedGlobalChargeCode.GetLocalChargeCode(LedgerTypes.AccountsPayable, null)
					: mappedGlobalChargeCode;

				return mappedLocalChargeCode.PK;
			}

			return ZGuid.Empty;
		}

		[ResourceStringData("a519d0b5-fb5f-4feb-9272-1ff1ff174800", ShortCaption = "Desc. ", Caption = "Description", FullDescription = "Description of the Charge Line.")]
		public ZString JR_Desc => GroupCompanySellCharge.JR_Desc;
		public ZPropertyInfo JR_DescInfo => GetZPropertyInfo(nameof(JR_Desc));

		[ResourceStringData("0133c7dd-469b-46d1-b5f0-05500310df88", ShortCaption = "Curr. ", Caption = "Sell Currency", FullDescription = "The Sell Currency of the Group Company Charge Line.")]
		public ZString JR_RX_NKSellCurrency => GroupCompanySellCharge.JR_RX_NKSellCurrency;
		public ZPropertyInfo JR_RX_NKSellCurrencyInfo => GetZPropertyInfo(nameof(JR_RX_NKSellCurrency));

		[ResourceStringData("3be11682-4cb0-428b-8cc2-5fb336177c5c", ShortCaption = "Sell Amt", Caption = "Sell Amount", FullDescription = "The Sell Amount of the Group Company Charge Line.")]
		public ZDecimal JR_OSSellAmt => GroupCompanySellCharge.JR_OSSellAmt;
		public ZPropertyInfo JR_OSSellAmtInfo => GetZPropertyInfo(nameof(JR_OSSellAmt));

		[List("GroupCompanySellCharge.Lookups.SellAccounts")]
		public ZGuid JR_OH_SellAccount => GroupCompanySellCharge.JR_OH_SellAccount;
		public ZPropertyInfo JR_OH_SellAccountInfo => GetZPropertyInfo(nameof(JR_OH_SellAccount));

		public ZString JR_InvoiceType => GroupCompanySellCharge.JR_InvoiceType;
		public ZPropertyInfo JR_InvoiceTypeInfo => GetZPropertyInfo(nameof(JR_InvoiceType));

		[ResourceStringData("304b2fba-ea8e-4914-960d-ca8e83d1ae55", ShortCaption = "Sell Rec.", Caption = "Sell Recognition")]
		public ZString SellRecognition => GroupCompanySellCharge.SellRecognition;
		public ZPropertyInfo SellRecognitionInfo => GetZPropertyInfo(nameof(SellRecognition));

		[ResourceStringData("09dfdcae-a9cb-48e9-8f15-4a263a6c488d", ShortCaption = "Posted", Caption = "Is Revenue Posted")]
		public ZBool JR_IsRevenuePosted => GroupCompanySellCharge.JR_IsRevenuePosted;
		public ZPropertyInfo JR_IsRevenuePostedInfo => GetZPropertyInfo(nameof(JR_IsRevenuePosted));

		[ResourceStringData("a70b2617-3ed3-4611-bb4b-8fdd4ee9f0f7", ShortCaption = "Job Num.", Caption = "Job Number", FullDescription = "The number of the Job in the Group Company.")]
		public ZString JR_JobNumber => GroupCompanySellCharge.JR_JobNumber;
		public ZPropertyInfo JR_JobNumberInfo => GetZPropertyInfo(nameof(JR_JobNumber));

		[ResourceStringData("beb0a4a7-9af4-4ccc-979c-39ce43b79f9b", ShortCaption = "Sell Ref.", Caption = "Sell Reference", FullDescription = "A secondary Invoice reference for this Charge Line, used to post invoices separately.")]
		public ZString JR_SellReference => GroupCompanySellCharge.JR_SellReference;
		public ZPropertyInfo JR_SellReferenceInfo => GetZPropertyInfo(nameof(JR_SellReference));

		[List("GroupCompanySellCharge.Lookups.SellAccounts")]
		[ResourceStringData("c4bba55c-6f47-40aa-b35e-8142bb10fa29", ShortCaption = "Proxy", Caption = "Group Company Proxy", FullDescription = "The Creditor for the accrual is the Group Company's Branch Proxy.")]
		public ZGuid Creditor => GroupCompanySellCharge.Branch.GB_OH_OrgProxy;
		public ZPropertyInfo JR_GCInfo => GetZPropertyInfo(nameof(Creditor));

		[List("GroupCompanySellCharge.DisplaySellInvoiceAddresses")]
		[ResourceStringData("c430b56b-fcd1-4734-9bc6-b64194db02be", ShortCaption = "Address", Caption = "Sell Address", FullDescription = "The address associated with the accrual.")]
		public ZGuid DisplaySellInvoiceAddress => GroupCompanySellCharge.DisplaySellInvoiceAddress;
		public ZPropertyInfo DisplaySellInvoiceAddressInfo => GetZPropertyInfo(nameof(DisplaySellInvoiceAddress));

		[List("GroupCompanySellCharge.DisplaySellInvoiceContacts")]
		[ResourceStringData("299f16b6-5870-4a0c-ba25-b382ff92e65a", ShortCaption = "Contact", Caption = "Sell Contact", FullDescription = "The contact person associated with the accrual.")]
		public ZGuid DisplaySellInvoiceContact => GroupCompanySellCharge.DisplaySellInvoiceContact;
		public ZPropertyInfo DisplaySellInvoiceContactInfo => GetZPropertyInfo(nameof(DisplaySellInvoiceContact));

		#endregion

		#region Validation

		public override GroupCompanyChargeValidation GetNewValidation()
		{
			return new GroupCompanyChargeValidation(this);
		}

		#endregion
	}
}
