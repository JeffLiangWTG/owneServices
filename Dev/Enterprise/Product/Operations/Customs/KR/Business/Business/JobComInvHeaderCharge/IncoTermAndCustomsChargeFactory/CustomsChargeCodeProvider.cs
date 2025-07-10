using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business
{
	public static class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode ForeignInlandFreight => new CustomsChargeCode(CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeTypeList.Descriptions.ForeignInlandFreight)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true
		};

		public static CustomsChargeCode A102 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A102, ImportChargeMethodOneCodeList.Descriptions.A102)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A104 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A104, ImportChargeMethodOneCodeList.Descriptions.A104)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A105 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A105, ImportChargeMethodOneCodeList.Descriptions.A105)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A106 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A106, ImportChargeMethodOneCodeList.Descriptions.A106)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A107 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A107, ImportChargeMethodOneCodeList.Descriptions.A107)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A108 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A108, ImportChargeMethodOneCodeList.Descriptions.A108)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A109 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A109, ImportChargeMethodOneCodeList.Descriptions.A109)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A110 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A110, ImportChargeMethodOneCodeList.Descriptions.A110)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A111 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A111, ImportChargeMethodOneCodeList.Descriptions.A111)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A112 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A112, ImportChargeMethodOneCodeList.Descriptions.A112)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A114 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A114, ImportChargeMethodOneCodeList.Descriptions.A114)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A115 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A115, ImportChargeMethodOneCodeList.Descriptions.A115)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A116 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A116, ImportChargeMethodOneCodeList.Descriptions.A116)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A118 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A118, ImportChargeMethodOneCodeList.Descriptions.A118)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A119 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A119, ImportChargeMethodOneCodeList.Descriptions.A119)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A120 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A120, ImportChargeMethodOneCodeList.Descriptions.A120)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode A121 => new CustomsChargeCode(ImportChargeMethodOneCodeList.Codes.A121, ImportChargeMethodOneCodeList.Descriptions.A121)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B303 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B303, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B303)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B304 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B304, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B304)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B305 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B305, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B305)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B306 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B306, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B306)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B307 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B307, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B307)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B309 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B309, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B309)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B310 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B310, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B310)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B311 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B311, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B311)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B312 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B312, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B312)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};
		public static CustomsChargeCode B313 => new CustomsChargeCode(ImportChargeMethodTwoAndThreeCodeList.Codes.B313, ImportChargeMethodTwoAndThreeCodeList.Descriptions.B313)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B404 => new CustomsChargeCode(ImportChargeMethodFourCodeList.Codes.B404, ImportChargeMethodFourCodeList.Descriptions.B404)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B405 => new CustomsChargeCode(ImportChargeMethodFourCodeList.Codes.B405, ImportChargeMethodFourCodeList.Descriptions.B405)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B406 => new CustomsChargeCode(ImportChargeMethodFourCodeList.Codes.B406, ImportChargeMethodFourCodeList.Descriptions.B406)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B407 => new CustomsChargeCode(ImportChargeMethodFourCodeList.Codes.B407, ImportChargeMethodFourCodeList.Descriptions.B407)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B408 => new CustomsChargeCode(ImportChargeMethodFourCodeList.Codes.B408, ImportChargeMethodFourCodeList.Descriptions.B408)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B409 => new CustomsChargeCode(ImportChargeMethodFourCodeList.Codes.B409, ImportChargeMethodFourCodeList.Descriptions.B409)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B410 => new CustomsChargeCode(ImportChargeMethodFourCodeList.Codes.B410, ImportChargeMethodFourCodeList.Descriptions.B410)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B411 => new CustomsChargeCode(ImportChargeMethodFourCodeList.Codes.B411, ImportChargeMethodFourCodeList.Descriptions.B411)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B501 => new CustomsChargeCode(ImportChargeMethodFiveAndSixCodeList.Codes.B501, ImportChargeMethodFiveAndSixCodeList.Descriptions.B501)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B502 => new CustomsChargeCode(ImportChargeMethodFiveAndSixCodeList.Codes.B502, ImportChargeMethodFiveAndSixCodeList.Descriptions.B502)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode B503 => new CustomsChargeCode(ImportChargeMethodFiveAndSixCodeList.Codes.B503, ImportChargeMethodFiveAndSixCodeList.Descriptions.B503)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine
		};

		public static ICustomsChargeCode[] Charges
		{
			get
			{
				return new[]
				{
					A102, A104, A105, A106, A107, A108, A109, A110, A111, A112, A114, A115, A116, A118, A119, A120, A121,
					B303, B304, B305, B306, B307, B309, B310, B311, B312, B313,
					B404, B405, B406, B407, B408, B409, B410, B411,
					B501, B502, B503
				};
			}
		}
	}
}
