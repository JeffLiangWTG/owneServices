using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public partial class JobComInvoiceHeader : AutoKRJobComInvoiceHeader
	{
		#region ValuataionQuestions
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion5A
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._5A)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion5AInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._5A).CY_Data = value;
				Set5ASubQuestions();
				ValuationQuestion5AInfo.RefreshBinding();
				ValuationQuestion5EBInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion5AInfo => GetZPropertyInfo(nameof(ValuationQuestion5A));

		bool IsValuationQuestion5ANo => ValuationQuestion5A != YesNoList.Codes.Yes && Is5SM;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SpecialRelationshipCodeList))]
		[MaxLength(Schema.ValuationQuestion5BMaxLength)]
		[ReadOnlyMember(nameof(IsValuationQuestion5ANo))]
		public ZString ValuationQuestion5B
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._5B)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion5BInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._5B).CY_Data = value;
				ValuationQuestion5BInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ValuationQuestion5BInfo => GetZPropertyInfo(nameof(ValuationQuestion5B));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		[ReadOnlyMember(nameof(IsValuationQuestion5ANo))]
		public ZString ValuationQuestion5C
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._5C)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion5CInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._5C).CY_Data = value;
				ValuationQuestion5CInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ValuationQuestion5CInfo => GetZPropertyInfo(nameof(ValuationQuestion5C));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		[ReadOnlyMember(nameof(IsValuationQuestion5ANo))]
		public ZString ValuationQuestion5D
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._5D)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion5DInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._5D).CY_Data = value;
				ValuationQuestion5DInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ValuationQuestion5DInfo => GetZPropertyInfo(nameof(ValuationQuestion5D));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationDeclarationPricingMethodsList))]
		[MaxLength(Schema.ValuationQuestion5EAMaxLength)]
		[ReadOnlyMember(nameof(IsValuationQuestion5ANo))]
		public ZString ValuationQuestion5EA
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._5EA)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion5EAInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._5EA).CY_Data = value;
				if (IsValuationQuestion5EBReadOnly)
				{
					ValuationQuestion5EB = ZString.Empty;
				}
				ValuationQuestion5EAInfo.RefreshBinding();
				ValuationQuestion5EBInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ValuationQuestion5EAInfo => GetZPropertyInfo(nameof(ValuationQuestion5EA));

		[ReadOnlyMember(nameof(IsValuationQuestion5EBReadOnly))]
		[MaxLength(Schema.ValuationQuestion5EBMaxLength)]
		public ZString ValuationQuestion5EB
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._5EB)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion5EBInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._5EB).CY_Data = value;
				ValuationQuestion5EBInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ValuationQuestion5EBInfo => GetZPropertyInfo(nameof(ValuationQuestion5EB));

		bool IsValuationQuestion5EBReadOnly => ValuationQuestion5EA != PricingCodeList.Codes._99;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion6A
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._6A)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion6AInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._6A).CY_Data = value;
				ValuationQuestion6AInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion6AInfo => GetZPropertyInfo(nameof(ValuationQuestion6A));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion6B
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._6B)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion6BInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._6B).CY_Data = value;
				ValuationQuestion6BInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion6BInfo => GetZPropertyInfo(nameof(ValuationQuestion6B));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion7A_5SM
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._7A)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion7A_5SMInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._7A).CY_Data = value;
				ValuationQuestion7A_5SMInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion7A_5SMInfo => GetZPropertyInfo(nameof(ValuationQuestion7A_5SM));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion7B_5SM
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._7B)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion7B_5SMInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._7B).CY_Data = value;
				ValuationQuestion7B_5SMInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion7B_5SMInfo => GetZPropertyInfo(nameof(ValuationQuestion7B_5SM));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion7A_IMP
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._7A)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion7A_IMPInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._7A).CY_Data = value;
				ValuationQuestion7A_IMPInfo.RefreshBinding();
				if (IsImport)
				{
					Set7ASubQuestions();
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion7A_IMP();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion7A_IMPInfo => GetZPropertyInfo(nameof(ValuationQuestion7A_IMP));
		bool IsValuationQuestion7ANo => ValuationQuestion7A_IMP != YesNoList.Codes.Yes && IsImport;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SpecialRelationshipCodeList))]
		[MaxLength(Schema.ValuationQuestion7BMaxLength)]
		[ReadOnlyMember(nameof(IsValuationQuestion7ANo))]
		public ZString ValuationQuestion7B_IMP
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._7B)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion7B_IMPInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._7B).CY_Data = value;
				ValuationQuestion7B_IMPInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion7B_IMP();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion7B_IMPInfo => GetZPropertyInfo(nameof(ValuationQuestion7B_IMP));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		[ReadOnlyMember(nameof(IsValuationQuestion7ANo))]
		public ZString ValuationQuestion7C
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._7C)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion7CInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._7C).CY_Data = value;
				ValuationQuestion7CInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion7C();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion7CInfo => GetZPropertyInfo(nameof(ValuationQuestion7C));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		[ReadOnlyMember(nameof(IsValuationQuestion7ANo))]
		public ZString ValuationQuestion7D
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._7D)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion7DInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._7D).CY_Data = value;
				ValuationQuestion7DInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion7D();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion7DInfo => GetZPropertyInfo(nameof(ValuationQuestion7D));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationDeclarationPricingMethodsList))]
		[MaxLength(Schema.ValuationQuestion7EAMaxLength)]
		[ReadOnlyMember(nameof(IsValuationQuestion7ANo))]
		public ZString ValuationQuestion7EA
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._7EA)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion7EAInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._7EA).CY_Data = value;
				if (IsValuationQuestion7EBReadOnly)
				{
					ValuationQuestion7EB = ZString.Empty;
				}
				ValuationQuestion7EAInfo.RefreshBinding();
				ValuationQuestion7EBInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion7EA();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion7EAInfo => GetZPropertyInfo(nameof(ValuationQuestion7EA));

		[ReadOnlyMember(nameof(IsValuationQuestion7EBReadOnly))]
		[MaxLength(Schema.ValuationQuestion5EBMaxLength)]
		public ZString ValuationQuestion7EB
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._7EB)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion7EBInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._7EB).CY_Data = value;
				ValuationQuestion7EBInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion7EB();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion7EBInfo => GetZPropertyInfo(nameof(ValuationQuestion7EB));

		bool IsValuationQuestion7EBReadOnly => ValuationQuestion7EA != PricingCodeList.Codes._99;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion8A
		{
			get => GetValuationQuestion(PriceQuestionCodeList.Codes._8A)?.CY_Data ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ValuationQuestion8AInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._8A).CY_Data = value;
				ValuationQuestion8AInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion8A();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion8AInfo => GetZPropertyInfo(nameof(ValuationQuestion8A));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion8B
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._8B)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion8BInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._8B).CY_Data = value;
				ValuationQuestion8BInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion8B();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion8BInfo => GetZPropertyInfo(nameof(ValuationQuestion8B));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion8C
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._8C)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion8CInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._8C).CY_Data = value;
				ValuationQuestion8CInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion8CInfo => GetZPropertyInfo(nameof(ValuationQuestion8C));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion8D
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._8D)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion8DInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._8D).CY_Data = value;
				ValuationQuestion8DInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion8DInfo => GetZPropertyInfo(nameof(ValuationQuestion8D));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion9A
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._9A)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion9AInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._9A).CY_Data = value;
				ValuationQuestion9AInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion9A();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion9AInfo => GetZPropertyInfo(nameof(ValuationQuestion9A));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion9B
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._9B)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion9BInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._9B).CY_Data = value;
				ValuationQuestion9BInfo.RefreshBinding();
				if (IsImport)
				{
					((IMPJobComInvoiceHeaderValidation)Validation).ValidateValuationQuestion9B();
				}
			}
		}
		public ZPropertyInfo ValuationQuestion9BInfo => GetZPropertyInfo(nameof(ValuationQuestion9B));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion10A
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._10A)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion10AInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._10A).CY_Data = value;
				ValuationQuestion10AInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion10AInfo => GetZPropertyInfo(nameof(ValuationQuestion10A));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion10B
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._10B)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion10BInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._10B).CY_Data = value;
				ValuationQuestion10BInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion10BInfo => GetZPropertyInfo(nameof(ValuationQuestion10B));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion10C
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._10C)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion10CInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._10C).CY_Data = value;
				ValuationQuestion10CInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion10CInfo => GetZPropertyInfo(nameof(ValuationQuestion10C));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion10D
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._10D)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion10DInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._10D).CY_Data = value;
				ValuationQuestion10DInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion10DInfo => GetZPropertyInfo(nameof(ValuationQuestion10D));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion11A
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._11A)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion11AInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._11A).CY_Data = value;
				ValuationQuestion11AInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion11AInfo => GetZPropertyInfo(nameof(ValuationQuestion11A));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion11B
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._11B)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion11BInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._11B).CY_Data = value;
				ValuationQuestion11BInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion11BInfo => GetZPropertyInfo(nameof(ValuationQuestion11B));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion11C
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._11C)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion11CInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._11C).CY_Data = value;
				ValuationQuestion11CInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion11CInfo => GetZPropertyInfo(nameof(ValuationQuestion11C));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.YNMaxLength)]
		public ZString ValuationQuestion11D
		{
			get
			{
				return GetValuationQuestion(PriceQuestionCodeList.Codes._11D)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(ValuationQuestion11DInfo, value);
				GetOrCreateValuationQuestion(PriceQuestionCodeList.Codes._11D).CY_Data = value;
				ValuationQuestion11DInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValuationQuestion11DInfo => GetZPropertyInfo(nameof(ValuationQuestion11D));

		[ResourceStringData("AB17E859-2568-4353-BCBD-69CA07E215D1", Caption = "Indirect Amount")]
		public ZDecimal IndirectAmount => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A102);
		[ResourceStringData("863BF902-945B-41C1-A59A-1A16930E8A39", Caption = "Purchase Cost")]
		public ZDecimal PurchaseCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A104);
		[ResourceStringData("476623B6-00D5-4A32-84F9-28B92B2EF675", Caption = "Brokerage Fee")]
		public ZDecimal BrokerageFee => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A105);
		[ResourceStringData("5161EEF5-CD59-4A52-A60F-14233CE19545", Caption = "Container Packaging Cost")]
		public ZDecimal ContainerPackagingCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A106);
		[ResourceStringData("2829653D-763D-4926-89C0-FEEFB90B32F8", Caption = "Goods Cost")]
		public ZDecimal GoodsCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A107);
		[ResourceStringData("68B60160-3D85-4573-B2A3-4B9E43456E0C", Caption = "Product Tool Costs")]
		public ZDecimal ProductToolCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A108);
		[ResourceStringData("A5729217-B1FF-46D0-84D5-8A60CA8C531E", Caption = "Commodity Usage Costs")]
		public ZDecimal CommodityUsageCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A109);
		[ResourceStringData("22F52EAA-CA8C-497E-9E69-3227C2B520B3", Caption = "Product Dev. Costs")]
		public ZDecimal ProductDevCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A110);
		[ResourceStringData("069C011E-2037-42FB-91D9-0F5159A417EA", Caption = "Royalty")]
		public ZDecimal Royalty => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A111);
		[ResourceStringData("570091FD-CBC2-4273-B695-A30894F4E33D", Caption = "Profit Amount")]
		public ZDecimal ProfitAmount => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A112);
		[ResourceStringData("517A0514-ACBD-414F-AE23-BBD262E9CFB3", Caption = "Subtotal (Excluding Transportation Costs)")]
		public ZDecimal ExcludingTransportationCost => PurchaseCost + BrokerageFee + ContainerPackagingCost + GoodsCost + ProductToolCost + CommodityUsageCost + ProductDevCost + Royalty + ProfitAmount;
		[ResourceStringData("CF68F082-855E-422F-AC67-DC51FA49363D", Caption = "Freight")]
		public ZDecimal Freight => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A114);
		[ResourceStringData("6E2C9326-4B4A-41C0-8868-6363A05C67BD", Caption = "Unload Cost")]
		public ZDecimal UnloadCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A115);
		[ResourceStringData("7C69AE4F-1255-466D-9755-86FE4B404551", Caption = "Insurance")]
		public ZDecimal Insurance => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A116);
		[ResourceStringData("65A82080-F0C6-44AB-9984-A2036C0CC014", Caption = "Subtotal (Transportation Cost)")]
		public ZDecimal TransportationCost => Freight + UnloadCost + Insurance;
		[ResourceStringData("F0EC8A27-BE60-49EA-B651-E0191F78A04F", Caption = "Total Additional Amount")]
		public ZDecimal TotalAdditionalAmount => ExcludingTransportationCost + TransportationCost;
		[ResourceStringData("D7C1F543-C311-4C80-B5FB-8B0EB71E55A5", Caption = "Local Transportation Cost")]
		public ZDecimal LocalTransportationCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A118);
		[ResourceStringData("E7CB90DD-EEEF-4FE2-95CC-014BC9F4DB05", Caption = "Technical Cost")]
		public ZDecimal TechnicalCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A119);
		[ResourceStringData("0B95D321-E3DF-4F89-BB57-8AAAC681A626", Caption = "Other Costs")]
		public ZDecimal OtherCost => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A120);
		[ResourceStringData("D80A51F5-6948-44F2-B4C5-56DC6021EA9E", Caption = "Discount Amount")]
		public ZDecimal DiscountAmount => this.GetTotalChargesAmount(ImportChargeMethodOneCodeList.Codes.A121);
		[ResourceStringData("8D11DDA9-6D5D-4EED-A4E1-DA012EC0AE26", Caption = "Total Deduction Amount")]
		public ZDecimal TotalDeductionAmount => LocalTransportationCost + TechnicalCost + OtherCost + DiscountAmount;

		[DecimalPlaces(DecimalPlacesConstants.TotalInvoiceAmount)]
		[ResourceStringData("B1D2E819-7B4C-4F13-A478-10BA251CFEED", Caption = "Amount")]
		public ZDecimal ReplaceAmount => JZ_InvoiceAmount;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CurrencyList))]
		public ZString ReplacementCurrency => JZ_RX_NKInvoice_Currency;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("54247DF8-BBFF-4A73-B6B7-D504B35D1E91", Caption = "Amount (KRW)")]
		public ZDecimal ReplacementAmountKRW => CurrencyConverter.ConvertExact(new Money(JZ_InvoiceAmount, Invoice_Currency), CurrencyConverter.LocalCurrency).Amount;

		[DecimalPlaces(DecimalPlacesConstants.Qty)]
		[ResourceStringData("BD13870C-33DB-4A48-A5ED-693418909140", Caption = "Quantity Discount")]
		public ZDecimal AdditionalAdjustmentQuantityDiscount => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B309);

		[DecimalPlaces(DecimalPlacesConstants.CommercialChargeAmount)]
		[ResourceStringData("AC235D7C-654F-408A-B1DA-F490D7711E18", Caption = "Commercial Amount")]
		public ZDecimal AdditionalAdjustmentCommercialAmount => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B310);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("30AEC10E-1224-4336-9C08-6599B6180BB5", Caption = "Transportation Cost")]
		public ZDecimal AdditionalAdjustmentTransportationCost => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B311);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("2A753641-CEE4-4B33-92D5-9EE3D9108193", Caption = "Shipping Port Cost")]
		public ZDecimal AdditionalAdjustmentShippingPortCost => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B312);

		[DecimalPlaces(DecimalPlacesConstants.Insurance)]
		[ResourceStringData("7BF50EDB-0023-4B90-B920-DBEC2F87B53D", Caption = "Insurance")]
		public ZDecimal AdditionalAdjustmentInsurance => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B313);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("78421D3A-882F-4543-B5DC-C63C570C3A3E", Caption = "Total Additional Amount")]
		public ZDecimal TotalAdditionalAdjustmentAmount => AdditionalAdjustmentQuantityDiscount + AdditionalAdjustmentCommercialAmount + AdditionalAdjustmentTransportationCost + AdditionalAdjustmentShippingPortCost + AdditionalAdjustmentInsurance;

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("0D8F5DB6-9312-46B5-9022-9F05CE688D1A", Caption = "Quantity Discount")]
		public ZDecimal DeductionAdjustmentQuantityDiscount => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B303);

		[DecimalPlaces(DecimalPlacesConstants.CommercialChargeAmount)]
		[ResourceStringData("C523C3CC-2A2A-46D7-9E05-2FD554FE8675", Caption = "Commercial Amount")]
		public ZDecimal DeductionAdjustmentCommercialAmount => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B304);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("2DD5BDA0-A142-4E4E-9634-D9E1FEFB063D", Caption = "Transportation Cost ")]
		public ZDecimal DeductionAdjustmentTransportationCost => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B305);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("8CE8572E-8264-41C6-A2CD-53CBD2151C57", Caption = "Shipping Port Cost")]
		public ZDecimal DeductionAdjustmentShippingPortCost => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B306);

		[DecimalPlaces(DecimalPlacesConstants.Insurance)]
		[ResourceStringData("F4F5DE74-9DDD-4017-B528-C6A8964B0FE3", Caption = "Insurance")]
		public ZDecimal DeductionAdjustmentInsurance => this.GetTotalChargesAmount(ImportChargeMethodTwoAndThreeCodeList.Codes.B307);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("A2D5FED3-7BEA-4CC8-87D2-0DAF576798CE", Caption = "Total Deduction Amount")]
		public ZDecimal TotalDeductionAdjustmentAmount => DeductionAdjustmentQuantityDiscount + DeductionAdjustmentCommercialAmount + DeductionAdjustmentTransportationCost + DeductionAdjustmentShippingPortCost + DeductionAdjustmentInsurance;

		[MaxLength(Schema.CustomsReferenceNumberMaxLength)]
		[ResourceStringData("1B70FDEE-6776-4B78-B1C3-D748F7624DAC", Caption = "Customs Reference Number")]
		public ZString CustomsReferenceNumber
		{
			get => InvoiceHeaderRef?.J2_ReferenceNumber ?? ZString.Empty;
			set
			{
				if (InvoiceHeaderRef == null)
				{
					InvoiceHeaderRefs.AddNew();
				}
				InvoiceHeaderRef.J2_ReferenceNumber = value;
			}
		}
		JobComInvoiceHeaderRefs InvoiceHeaderRef
		{
			get
			{
				if (invoiceHeaderRef?.IsDeleted ?? true)
				{
					invoiceHeaderRef = InvoiceHeaderRefs.FirstOrDefault();
				}
				return invoiceHeaderRef;
			}
		}
		JobComInvoiceHeaderRefs invoiceHeaderRef;

		[DecimalPlaces(DecimalPlacesConstants.TotalInvoiceAmount)]
		[ResourceStringData("12FCE31A-FCE5-4E57-A73B-9370022E76E2", Caption = "Amount", FullDescription = "Item Amount")]
		public ZDecimal DeductionCostAmount => JZ_InvoiceAmount;

		[MaxLength(Schema.JZ_DeductionTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CostRateCodeList))]
		[ResourceStringData("BFC56AC2-919F-4C4F-85FF-D56610C53CB6", Caption = "Cost Rate Code")]
		public override ZString JZ_DeductionType { get => base.JZ_DeductionType; set => base.JZ_DeductionType = value; }
		[DecimalPlaces(DecimalPlacesConstants.CostRate)]
		[ResourceStringData("3181A29F-100D-4C8D-8347-3AB2CE8A7115", Caption = "Cost Rate")]
		public override ZDecimal JZ_DeductionRate { get => base.JZ_DeductionRate; set => base.JZ_DeductionRate = value; }

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("64A5E717-BFDE-4DDC-ACEC-438E546396F9", Caption = "Consignment Sales Fee")]
		public ZDecimal ConsignmentSalesFee => this.GetTotalChargesAmount(ImportChargeMethodFourCodeList.Codes.B404);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("10A389F6-BF4D-494E-BC33-9F5269D75766", Caption = "General Cost")]
		public ZDecimal GeneralCost => this.GetTotalChargesAmount(ImportChargeMethodFourCodeList.Codes.B405);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("CBA7D04B-92AD-4A5E-98FC-1AA854A2023B", Caption = "Transportation Cost")]
		public ZDecimal DeductionTransportationCost => this.GetTotalChargesAmount(ImportChargeMethodFourCodeList.Codes.B406);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("EBAFC812-1CD8-486A-85FD-F248E9466035", Caption = "Insurance")]
		public ZDecimal DeductionInsurance => this.GetTotalChargesAmount(ImportChargeMethodFourCodeList.Codes.B407);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("3FA339D6-6E3A-4963-8B1C-106FCD5E7157", Caption = "Unload Cost")]
		public ZDecimal DeductionUnloadCost => this.GetTotalChargesAmount(ImportChargeMethodFourCodeList.Codes.B408);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("7DCE6DB0-42AB-429F-99B8-F36F7508B480", Caption = "Other Transportation Costs")]
		public ZDecimal OtherTransportationCosts => this.GetTotalChargesAmount(ImportChargeMethodFourCodeList.Codes.B409);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("C97BA5CD-85C7-498B-92DE-95A04381E82F", Caption = "Additional Cost")]
		public ZDecimal AdditionalCost => this.GetTotalChargesAmount(ImportChargeMethodFourCodeList.Codes.B410);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("DFEC0101-1856-4495-8259-A0E7D93A2410", Caption = "Tax")]
		public ZDecimal Tax => this.GetTotalChargesAmount(ImportChargeMethodFourCodeList.Codes.B411);

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("B826979E-EDFC-45A7-809F-30E0260CCBEC", Caption = "Total Deduction Amount")]
		public ZDecimal DeductionCostTotalDeductionAmount => ConsignmentSalesFee + GeneralCost + DeductionTransportationCost + DeductionInsurance + DeductionUnloadCost + OtherTransportationCosts + AdditionalCost + Tax;

		void PopulateValuationQuestionsForMethodOne()
		{
			var questions = Is5SM ? PriceQuestionCodeList.MandatoryQuestionsForMethodOneFor5SM() : PriceQuestionCodeList.MandatoryQuestionsForMethodOneForIMP();
			foreach (var question in questions)
			{
				var valuationQuestion = GetOrCreateValuationQuestion(question);
				valuationQuestion.CY_Data = YesNoList.Codes.No;
			}
		}

		void Set5ASubQuestions()
		{
			if (IsValuationQuestion5ANo)
			{
				foreach (var question in PriceQuestionCodeList.SubQuestionsOf5A())
				{
					var valuationQuestion = GetValuationQuestion(question);
					if (valuationQuestion != null)
					{
						valuationQuestion.CY_Data = ZString.Empty;
					}
				}
			}
			else
			{
				ValuationQuestion5C = YesNoList.Codes.No;
				ValuationQuestion5D = YesNoList.Codes.No;
			}
		}

		void Set7ASubQuestions()
		{
			if (IsValuationQuestion7ANo)
			{
				foreach (var question in PriceQuestionCodeList.SubQuestionsOf7A())
				{
					var valuationQuestion = GetValuationQuestion(question);
					if (valuationQuestion != null)
					{
						valuationQuestion.CY_Data = ZString.Empty;
					}
				}
			}
			else
			{
				ValuationQuestion7C = YesNoList.Codes.No;
				ValuationQuestion7D = YesNoList.Codes.No;
			}
		}

		ValuationQuestion GetValuationQuestion(ZString code)
		{
			return ValuationQuestions.Cast<ValuationQuestion>().FirstOrDefault(x => x.CY_Code == code);
		}
		ValuationQuestion GetOrCreateValuationQuestion(ZString code)
		{
			var result = GetValuationQuestion(code);
			if (result == null)
			{
				result = ValuationQuestions.AddNew();
				result.CY_Code = code;
			}
			return result;
		}
		#endregion
		#region ValuationDeclarationCodes Details
		[ResourceStringData("DC83E2DF-80F5-4726-AE66-245CA3E09BD4", Caption = "Commission")]
		public ZBool ProvisionalPricingReason101
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._101)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._101).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("A31FE82A-5506-4D10-A56E-32F81D1C8767", Caption = "Brokerage fee")]
		public ZBool ProvisionalPricingReason102
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._102)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._102).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("34078519-EC95-4414-B9ED-E2D9808D76F5", Caption = "Container cost")]
		public ZBool ProvisionalPricingReason103
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._103)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._103).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("0F7087A1-EF1C-4609-BF9A-CF6DC0C66FDC", Caption = "Packaging Labor costs")]
		public ZBool ProvisionalPricingReason104
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._104)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._104).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("73ABAEF0-A07A-4E68-BC91-931EBFCF591D", Caption = "Packaging material cost")]
		public ZBool ProvisionalPricingReason105
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._105)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._105).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("A2C19CCA-CAA3-42F2-83FC-AC93FF4B146B", Caption = "Production support cost")]
		public ZBool ProvisionalPricingReason106
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._106)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._106).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("716F1C6D-9C59-4B3C-8917-E56B4004F45E", Caption = "Fees for use of rights")]
		public ZBool ProvisionalPricingReason107
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._107)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._107).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("A7EBA9DA-7863-48FB-B705-C13E99922028", Caption = "Post Morten attributable profit")]
		public ZBool ProvisionalPricingReason108
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._108)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._108).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("497F86FD-A31C-49B2-98A1-07A295B0F5F0", Caption = "Insurance")]
		public ZBool ProvisionalPricingReason109
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._109)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._109).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("9B702C01-FEEB-4A88-AEE3-8652AB15A779", Caption = "Freight")]
		public ZBool ProvisionalPricingReason110
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._110)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._110).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("1A8B1ABE-F6A2-43DE-96B6-0C1D070AFE7F", Caption = "Transportation related costs")]
		public ZBool ProvisionalPricingReason111
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._111)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._111).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("4345D36D-BF9C-438D-BC7F-B44C3B1B150C", Caption = "Actual paid price")]
		public ZBool ProvisionalPricingReason112
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._112)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._112).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("7592F1F7-F4EA-400B-B20D-88E382257739", Caption = "For primary products such as crude oil, grain, ore, the price of which is not set as of the date of import declaration")]
		public ZBool ProvisionalPricingReason113
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._113)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._113).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("C0C7A8C7-1742-4CE1-B3EA-ACBD2EFF48C4", Caption = "In case of a company applying for prior review (ACVA) for determining transaction price between related parties")]
		public ZBool ProvisionalPricingReason114
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._114)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._114).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("83279F80-B4CB-4A56-B8B5-7AC7BF462B90", Caption = "When determining the taxable price based on the domestic sales price (Method 4) takes a long time to determine the price")]
		public ZBool ProvisionalPricingReason115
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._115)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._115).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("A017282C-3402-456A-BBB5-96AA20432B47", Caption = "When delivery is completed a considerable period of time after the initial order of a turnkey plant, etc.")]
		public ZBool ProvisionalPricingReason116
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._116)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._116).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("4F7A96C2-FC42-4C28-9436-672A06E309AA", Caption = "In cases where the transaction price of imported goods under the main text of Article 30 (1) of the Act during transactions between related\n parties is expected to be adjusted to the normal price under Article 5 of the Act on Adjustment of International Taxes after acceptance of the import declaration", FullDescription = "In cases where the transaction price of imported goods under the main text of Article 30 (1) of the Act during transactions between related parties is expected to be adjusted to the normal price under Article 5 of the Act on Adjustment of International Taxes after acceptance of the import declaration")]
		public ZBool ProvisionalPricingReason117
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._117)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._117).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[MaxLength(50)]
		[ResourceStringData("3DCC71A3-4654-43A7-A603-6A140D44C256", Caption = "Other reasons")]
		public ZString ProvisionalPricingReason119
		{
			get
			{
				return GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._119)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._119).CY_Data = value;
			}
		}
		[ResourceStringData("98CA2FB5-BC9D-4591-B568-34C41F9F0312", Caption = "When the final price calculation formula is confirmed before import, the calculation formula is based on variables that occur after import, and those variables are beyond the control of the transaction parties.")]
		public ZBool ProvisionalPricingReason120
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._120)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._120).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		#endregion
		#region ValuationDeclarationCodes
		[ResourceStringData("68F925DE-4C43-44E8-9EF1-636C82832083", Caption = "Sample Item")]
		public ZBool ValuationDeclarationCode301
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._301)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._301).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("5D55056C-7DAD-43E4-9000-31A642704DC5", Caption = "Advertising Use")]
		public ZBool ValuationDeclarationCode302
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._302)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._302).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("555736AF-BE1A-469B-ABFA-BFBA36BFEF04", Caption = "Use of Defective Repair")]
		public ZBool ValuationDeclarationCode303
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._303)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._303).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("3FE14258-F895-4391-8211-618C0A22C4F1", Caption = "Replacement Item")]
		public ZBool ValuationDeclarationCode304
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._304)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._304).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("EA6B2805-B7C3-4C24-B0BB-5B9246AED22A", Caption = "A Gift or Free Donation")]
		public ZBool ValuationDeclarationCode305
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._305)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._305).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("193C9FD0-9685-4237-A89D-886A28A443B7", Caption = "For Production And Manufacture")]
		public ZBool ValuationDeclarationCode306
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._306)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._306).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("727D8067-1D3C-491C-BDDA-68136A18B17D", Caption = "Other Reasons")]
		public ZString ValuationDeclarationCode307
		{
			get
			{
				return GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._307)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._307).CY_Data = value;
			}
		}

		[ResourceStringData("C49B9EC7-A076-4E2C-AB2F-FC422DD5C863", Caption = "Performance Price Of Paid Transactions")]
		public ZBool ValuationDeclarationCode401
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._401)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._401).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("B933B5B6-4DF1-44DB-A23F-C2059EAC1634", Caption = "Price List")]
		public ZBool ValuationDeclarationCode402
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._402)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._402).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("A04B9848-69B0-464A-86D8-62AAA9BFEAD5", Caption = "Manufacturing Cost")]
		public ZBool ValuationDeclarationCode403
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._403)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._403).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("94B2240F-FB31-462A-B27B-C6ED7415D64D", Caption = "Invoice")]
		public ZBool ValuationDeclarationCode404
		{
			get
			{
				return (GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._404)?.CY_Data ?? ZString.Empty) == YesNoList.Codes.Yes;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._404).CY_Data = value ? YesNoList.Codes.Yes : ZString.Empty;
			}
		}
		[ResourceStringData("3188529E-C5BD-4322-A226-C7D83794D7FC", Caption = "Other Reasons")]
		public ZString ValuationDeclarationCode405
		{
			get
			{
				return GetValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._405)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				GetOrCreateValuationDeclarationCode(PriceDeclarationItemCodeList.Codes._405).CY_Data = value;
			}
		}

		void PopulateValuationDeclarationCodes()
		{
			foreach (var question in PriceDeclarationItemCodeList.MandatoryQuestionsForMethodTwoToSix())
			{
				GetOrCreateValuationDeclarationCode(question);
			}
		}
		ValuationDeclarationCode GetValuationDeclarationCode(ZString code)
		{
			return ValuationDeclarationCodes.Cast<ValuationDeclarationCode>().FirstOrDefault(x => x.CY_Code == code);
		}

		ValuationDeclarationCode GetOrCreateValuationDeclarationCode(ZString code)
		{
			var result = GetValuationDeclarationCode(code);
			if (result == null)
			{
				result = ValuationDeclarationCodes.AddNew();
				result.CY_Code = code;
			}
			return result;
		}
		#endregion

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("612C7B2A-A664-4BD4-B1DD-3F834A7D6176", Caption = "Expected Customs Value")]
		public ZDecimal ExpectedCustomsValue => Entries.FirstOrDefault()?.CustomsValue ?? ZDecimal.Zero;
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("06B8098E-383F-4220-A1A9-EF6FE2D57BBD", Caption = "Amount (KRW)", FullDescription = "Item Amount (KRW)")]
		public ZDecimal AmountAgreedUponWithCustomsKRW => CurrencyConverter.ConvertExact(new Money(JZ_InvoiceAmount, Invoice_Currency), CurrencyConverter.LocalCurrency).Amount;
		[DecimalPlaces(DecimalPlacesConstants.Freight)]
		[ResourceStringData("05636DB2-C8BC-40B0-BF64-629711C182D5", Caption = "Freight (to Arr. Port) ")]
		public ZDecimal AdditionalCostFreightToArrivalPort => this.GetTotalChargesAmount(ImportChargeMethodFiveAndSixCodeList.Codes.B501);
		[DecimalPlaces(DecimalPlacesConstants.Freight)]
		[ResourceStringData("C0C5FDF9-6849-4ACB-8B73-3C816FC08E19", Caption = "Freight (to Dep. Port) ")]
		public ZDecimal AdditionalCostFreightToDeparturePort => this.GetTotalChargesAmount(ImportChargeMethodFiveAndSixCodeList.Codes.B502);
		[DecimalPlaces(DecimalPlacesConstants.Insurance)]
		[ResourceStringData("64516D9C-DFAC-419A-A538-1EA803B189BF", Caption = "Insurance")]
		public ZDecimal AdditionalCostInsurance => this.GetTotalChargesAmount(ImportChargeMethodFiveAndSixCodeList.Codes.B503);
		[DecimalPlaces(DecimalPlacesConstants.CommercialChargeAmount)]
		[ResourceStringData("F73FF195-10D3-454B-A51B-D1E4D5DBA8F6", Caption = "Total Additional Amount")]
		public ZDecimal AdditionalCostTotalAdditionalAmount => AdditionalCostFreightToArrivalPort + AdditionalCostFreightToDeparturePort + AdditionalCostInsurance;
	}
}
