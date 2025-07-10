using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(DocBaseJobComInvoiceGroupHeader))]
	public abstract class DocBaseJobComInvoiceGroupHeaderAbstractTest<T, TWrapper> : DocumentWrapperTestCase
			where T : BaseJobComInvoiceGroupHeader
			where TWrapper : DocBaseJobComInvoiceGroupHeader
	{
		public void TestToString()
		{
			AssertEquals("ToString()", GroupHeaderInternal.JZ_InvoiceNumber, GroupHeaderWrapperInternal.InvoiceNumber);
		}

		#region Abstract

		protected abstract TWrapper CreateGroupHeaderWrapper(T groupHeaderInternal);

		#endregion

		#region ZDecimal Fields

		public void TestTNI()
		{
			AssertEquals("TNI", GroupHeaderInternal.JZ_Calc_TNI, GroupHeaderWrapperInternal.TNI);
		}

		public void TestCalcOverseasFreight()
		{
			AssertCharges(CustomsChargeTypeList.Codes.OverseasFreight, "OverseasFreight", "CalcOverseasFreightCurrency");
		}

		public void TestCalcOverseasInsurance()
		{
			AssertCharges(CustomsChargeTypeList.Codes.OverseasInsurance, "OverseasInsurance", "CalcOverseasInsuranceCurrency");
		}

		public void TestCalcLandingCharges()
		{
			AssertCharges(CustomsChargeTypeList.Codes.LandingCharges, "LandingCharges", "CalcLandingChargesCurrency");
		}

		public void TestCalcExWorks()
		{
			AssertCharges(CustomsChargeTypeList.Codes.ExWorks, "ExWorksAmount", "CalcExWorksCurrency");
		}

		public void TestCalcForeignInlandFreight()
		{
			AssertCharges(CustomsChargeTypeList.Codes.ForeignInlandFreight, "ForeignInlandFreight", "CalcForeignInlandFreightCurrency");
		}

		public void TestCalcPackingCosts()
		{
			AssertCharges(CustomsChargeTypeList.Codes.PackingCost, "PackingCosts", "CalcPackingCostCurrency");
		}

		public void TestCalcOtherCharges1()
		{
			AssertCharges(CustomsChargeTypeList.Codes.OtherCharges, "OtherCharges1", "CalcOtherCharges1Currency");
		}

		public void TestCalcOtherCharges2()
		{
			AssertCharges(CustomsChargeTypeList.Codes.OtherCharges, "OtherCharges2", "CalcOtherCharges2Currency");
		}

		public void TestCalcDiscount()
		{
			AssertCharges(CustomsChargeTypeList.Codes.Discount, "Discount", "CalcDiscountCurrency");
		}

		public void TestCalcCommission()
		{
			AssertCharges(CustomsChargeTypeList.Codes.Commission, "Commission", "CalcCommissionCurrency");
		}

		public void TestCIFAmount()
		{
			AssertEquals("CIFAmount", GroupHeaderInternal.JZ_Calc_CIFAmount, GroupHeaderWrapperInternal.CIFAmount);
		}

		public void TestFOBAmount()
		{
			AssertEquals("FOBAmount", GroupHeaderInternal.JZ_Calc_FOBAmount, GroupHeaderWrapperInternal.FOBAmount);
		}

		public void TestPaymentAmount()
		{
			GroupHeaderInternal.JZ_PaymentAmount = 12.34M;
			AssertEquals("PaymentAmount", GroupHeaderInternal.JZ_PaymentAmount, GroupHeaderWrapperInternal.PaymentAmount);
		}

		public void TestPaymentExRate()
		{
			GroupHeaderInternal.JZ_PaymentExRate = 12.34M;
			AssertEquals("PaymentExRate", GroupHeaderInternal.JZ_PaymentExRate, GroupHeaderWrapperInternal.PaymentExRate);
		}

		public void TestVolume()
		{
			GroupHeaderInternal.JZ_Volume = 12.34M;
			AssertEquals("Volume", GroupHeaderInternal.JZ_Volume, GroupHeaderWrapperInternal.Volume);
		}

		public void TestWeight()
		{
			GroupHeaderInternal.JZ_Weight = 12.34M;
			AssertEquals("Weight", GroupHeaderInternal.JZ_Weight, GroupHeaderWrapperInternal.Weight);
		}

		#endregion

		#region ZString Fields

		//		public void TestIncoTerm()
		//		{
		//			GroupHeader.JZ_IncoTerm = "III";
		//			AssertEquals("IncoTerm", GroupHeader.JZ_IncoTerm, GroupHeaderWrapper.IncoTerm);
		//		}

		public void TestAddInfo()
		{
			GroupHeaderInternal.JZ_AddInfo = "AddInfo";
			AssertEquals("AddInfo", GroupHeaderInternal.JZ_AddInfo, GroupHeaderWrapperInternal.AddInfo);
		}

		public void TestInvoiceNumber()
		{
			AssertEquals("InvoiceNumber", GroupHeaderInternal.JZ_InvoiceNumber, GroupHeaderWrapperInternal.InvoiceNumber);
		}

		public void TestPaymentNo()
		{
			GroupHeaderInternal.JZ_PaymentNo = "PaymentNo";
			AssertEquals("PaymentNo", GroupHeaderInternal.JZ_PaymentNo, GroupHeaderWrapperInternal.PaymentNo);
		}

		public void TestVolumeUQ()
		{
			GroupHeaderInternal.JZ_VolumeUQ = "UQ";
			AssertEquals("VolumeUQ", GroupHeaderInternal.JZ_VolumeUQ, GroupHeaderWrapperInternal.VolumeUQ);
		}

		public void TestWeightUQ()
		{
			GroupHeaderInternal.JZ_WeightUQ = "UQ";
			AssertEquals("WeightUQ", GroupHeaderInternal.JZ_WeightUQ, GroupHeaderWrapperInternal.WeightUQ);
		}

		#endregion

		#region ZBool Fields

		//		public void TestIsCommissionIncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsCommissionIncludedInITOT = ZBool.False;
		//			Assert("!IsCommissionIncludedInITOT", !GroupHeaderWrapper.IsCommissionIncludedInITOT);
		//
		//			GroupHeader.JZ_IsCommissionIncludedInITOT = ZBool.True;
		//			Assert("IsCommissionIncludedInITOT", GroupHeaderWrapper.IsCommissionIncludedInITOT);
		//		}
		//
		//		public void TestIsDiscountIncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsDiscountIncludedInITOT = ZBool.False;
		//			Assert("!IsDiscountIncludedInITOT", !GroupHeaderWrapper.IsDiscountIncludedInITOT);
		//
		//			GroupHeader.JZ_IsDiscountIncludedInITOT = ZBool.True;
		//			Assert("IsDiscountIncludedInITOT", GroupHeaderWrapper.IsDiscountIncludedInITOT);
		//		}
		//
		//		public void TestIsForeignInlandFreightIncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsForeignInlandFreightIncludedInITOT = ZBool.False;
		//			Assert("!IsForeignInlandFreightIncludedInITOT", !GroupHeaderWrapper.IsForeignInlandFreightIncludedInITOT);
		//
		//			GroupHeader.JZ_IsForeignInlandFreightIncludedInITOT = ZBool.True;
		//			Assert("IsForeignInlandFreightIncludedInITOT", GroupHeaderWrapper.IsForeignInlandFreightIncludedInITOT);
		//		}
		//
		//		public void TestIsInsuranceIncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsInsuranceIncludedInITOT = ZBool.False;
		//			Assert("!IsInsuranceIncludedInITOT", !GroupHeaderWrapper.IsInsuranceIncludedInITOT);
		//
		//			GroupHeader.JZ_IsInsuranceIncludedInITOT = ZBool.True;
		//			Assert("IsInsuranceIncludedInITOT", GroupHeaderWrapper.IsInsuranceIncludedInITOT);
		//		}
		//
		//		public void TestIsInternationalFreightIncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsInternationalFreightIncludedInITOT = ZBool.False;
		//			Assert("!IsInternationalFreightIncludedInITOT", !GroupHeaderWrapper.IsInternationalFreightIncludedInITOT);
		//
		//			GroupHeader.JZ_IsInternationalFreightIncludedInITOT = ZBool.True;
		//			Assert("IsInternationalFreightIncludedInITOT", GroupHeaderWrapper.IsInternationalFreightIncludedInITOT);
		//		}
		//
		//		public void TestIsLandingChargeIncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsLandingChargeIncludedInITOT = ZBool.False;
		//			Assert("!IsLandingChargeIncludedInITOT", !GroupHeaderWrapper.IsLandingChargeIncludedInITOT);
		//
		//			GroupHeader.JZ_IsLandingChargeIncludedInITOT = ZBool.True;
		//			Assert("IsLandingChargeIncludedInITOT", GroupHeaderWrapper.IsLandingChargeIncludedInITOT);
		//		}
		//
		//		public void TestIsNonDutiableFOBIncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsNonDutiableFOBIncludedInITOT = ZBool.False;
		//			Assert("!IsNonDutiableFOBIncludedInITOT", !GroupHeaderWrapper.IsNonDutiableFOBIncludedInITOT);
		//
		//			GroupHeader.JZ_IsNonDutiableFOBIncludedInITOT = ZBool.True;
		//			Assert("IsNonDutiableFOBIncludedInITOT", GroupHeaderWrapper.IsNonDutiableFOBIncludedInITOT);
		//		}
		//
		//		public void TestIsOtherCharges1IncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsOtherCharges1IncludedInITOT = ZBool.False;
		//			Assert("!IsOtherCharges1IncludedInITOT", !GroupHeaderWrapper.IsOtherCharges1IncludedInITOT);
		//
		//			GroupHeader.JZ_IsOtherCharges1IncludedInITOT = ZBool.True;
		//			Assert("IsOtherCharges1IncludedInITOT", GroupHeaderWrapper.IsOtherCharges1IncludedInITOT);
		//		}
		//
		//		public void TestIsOtherCharges2IncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsOtherCharges2IncludedInITOT = ZBool.False;
		//			Assert("!IsOtherCharges2IncludedInITOT", !GroupHeaderWrapper.IsOtherCharges2IncludedInITOT);
		//
		//			GroupHeader.JZ_IsOtherCharges2IncludedInITOT = ZBool.True;
		//			Assert("IsOtherCharges2IncludedInITOT", GroupHeaderWrapper.IsOtherCharges2IncludedInITOT);
		//		}
		//
		//		public void TestIsPackingChargeIncludedInITOT()
		//		{
		//			GroupHeader.JZ_IsPackingChargeIncludedInITOT = ZBool.False;
		//			Assert("!IsPackingChargeIncludedInITOT", !GroupHeaderWrapper.IsPackingChargeIncludedInITOT);
		//
		//			GroupHeader.JZ_IsPackingChargeIncludedInITOT = ZBool.True;
		//			Assert("IsPackingChargeIncludedInITOT", GroupHeaderWrapper.IsPackingChargeIncludedInITOT);
		//		}

		#endregion

		#region Wrapper Fields

		public void TestBranch()
		{
			BaseJobComInvoiceGroupHeader groupHeader = (BaseJobComInvoiceGroupHeader)GroupHeaderWrapperInternal.WrappedObject;
			groupHeader.JobDeclaration.JE_GB = ZGuid.Empty;
			AssertNull("Branch", GroupHeaderWrapperInternal.Branch);

			GroupHeaderInternal.JZ_GB = GlbBranch.CurrentBranch.PK;
			AssertNotNull("Branch", GroupHeaderWrapperInternal.Branch);
			AssertEquals("Branch is of type DocBranch", typeof(DocBranch), GroupHeaderWrapperInternal.Branch.GetType());
		}

		public void TestBuyer()
		{
			AssertNull("Buyer", GroupHeaderWrapperInternal.Buyer);

			GroupHeaderInternal.JZ_OH_Buyer = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Buyer", GroupHeaderWrapperInternal.Buyer);
			AssertEquals("Buyer is of type DocOrganisation", typeof(DocOrganisation), GroupHeaderWrapperInternal.Buyer.GetType());
		}

		public void TestSupplier()
		{
			AssertNull("Supplier", GroupHeaderWrapperInternal.Supplier);

			GroupHeaderInternal.JZ_OH_Supplier = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Supplier", GroupHeaderWrapperInternal.Supplier);
			AssertEquals("Supplier is of type DocOrganisation", typeof(DocOrganisation), GroupHeaderWrapperInternal.Supplier.GetType());
		}

		public void TestNKDefaultOrigin()
		{
			AssertNull("NKDefaultOrigin", GroupHeaderWrapperInternal.NKDefaultOrigin);

			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			GroupHeaderInternal.JZ_RN_NKDefaultOrigin = country.RN_Code;
			AssertNotNull("NKDefaultOrigin", GroupHeaderWrapperInternal.NKDefaultOrigin);
			AssertEquals("NKDefaultOrigin is of type DocCountry", typeof(DocCountry), GroupHeaderWrapperInternal.NKDefaultOrigin.GetType());
		}

		public void TestCIFCurrency()
		{
			AssertNull("CIFCurrency", GroupHeaderWrapperInternal.CIFCurrency);
		}

		//TODO:See AssertCharges and add more assertion if necessary 
		//		public void TestCommissionCurrency()
		//		{
		//
		//			AssertNull("CommissionCurrency", GroupHeaderWrapperInternal.CommissionCurrency);
		//		}
		//
		//		public void TestDiscountCurrency()
		//		{
		//			AssertNull("DiscountCurrency", GroupHeaderWrapperInternal.DiscountCurrency);
		//		}

		//		public void TestExWorksCurrency()
		//		{
		//			AssertNull("ExWorksCurrency", GroupHeaderWrapper.ExWorksCurrency);
		//
		//			GroupHeader.JZ_RX_ExWorksCurrency = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
		//			AssertNotNull("ExWorksCurrency", GroupHeaderWrapper.ExWorksCurrency);
		//			AssertEquals("ExWorksCurrency is of type DocCurrency", typeof(DocCurrency), GroupHeaderWrapper.ExWorksCurrency.GetType());
		//		}

		public void TestFOBCurrency()
		{
			AssertNull("FOBCurrency", GroupHeaderWrapperInternal.FOBCurrency);
		}

		//		public void TestForeignInlandFreightCurrency()
		//		{
		//			AssertNull("ForeignInlandFreightCurrency", GroupHeaderWrapper.ForeignInlandFreightCurrency);
		//
		//			GroupHeader.JZ_RX_ForeignInlandFreightCurrency = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
		//			AssertNotNull("ForeignInlandFreightCurrency", GroupHeaderWrapper.ForeignInlandFreightCurrency);
		//			AssertEquals("ForeignInlandFreightCurrency is of type DocCurrency", typeof(DocCurrency), GroupHeaderWrapper.ForeignInlandFreightCurrency.GetType());
		//		}

		//		public void TestInvoice_Currency()
		//		{
		//			AssertNull("Invoice_Currency", GroupHeaderWrapper.Invoice_Currency);
		//
		//			GroupHeader.JZ_RX_NKInvoice_Currency = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).RX_Code;
		//			AssertNotNull("Invoice_Currency", GroupHeaderWrapper.Invoice_Currency);
		//			AssertEquals("Invoice_Currency is of type DocCurrency", typeof(DocCurrency), GroupHeaderWrapper.Invoice_Currency.GetType());
		//		}
		//
		//		public void TestLandingChargesCurrency()
		//		{
		//			AssertNull("LandingChargesCurrency", GroupHeaderWrapper.LandingChargesCurrency);
		//
		//			GroupHeader.JZ_RX_LandingChargesCurrency = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
		//			AssertNotNull("LandingChargesCurrency", GroupHeaderWrapper.LandingChargesCurrency);
		//			AssertEquals("LandingChargesCurrency is of type DocCurrency", typeof(DocCurrency), GroupHeaderWrapper.LandingChargesCurrency.GetType());
		//		}
		//
		//		public void TestNonDutiablePreFOBChargeCurrency()
		//		{
		//			AssertNull("NonDutiablePreFOBChargeCurrency", GroupHeaderWrapper.NonDutiablePreFOBChargeCurrency);
		//		}
		//
		//		public void TestOtherCharges1Currency()
		//		{
		//			AssertNull("OtherCharges1Currency", GroupHeaderWrapper.OtherCharges1Currency);
		//
		//			GroupHeader.JZ_RX_OtherCharges1Currency = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
		//			AssertNotNull("OtherCharges1Currency", GroupHeaderWrapper.OtherCharges1Currency);
		//			AssertEquals("OtherCharges1Currency is of type DocCurrency", typeof(DocCurrency), GroupHeaderWrapper.OtherCharges1Currency.GetType());
		//		}
		//
		//		public void TestOtherCharges2Currency()
		//		{
		//			AssertNull("OtherCharges2Currency", GroupHeaderWrapper.OtherCharges2Currency);
		//		}
		//
		//		public void TestOverseasFreightCurrency()
		//		{
		//			AssertNull("OverseasFreightCurrency", GroupHeaderWrapper.OverseasFreightCurrency);
		//
		//			GroupHeader.JZ_RX_OverseasFreightCurrency = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
		//			AssertNotNull("OverseasFreightCurrency", GroupHeaderWrapper.OverseasFreightCurrency);
		//			AssertEquals("OverseasFreightCurrency is of type DocCurrency", typeof(DocCurrency), GroupHeaderWrapper.OverseasFreightCurrency.GetType());
		//		}
		//
		//		public void TestOverseasInsuranceCurrency()
		//		{
		//			AssertNull("OverseasInsuranceCurrency", GroupHeaderWrapper.OverseasInsuranceCurrency);
		//
		//			GroupHeader.JZ_RX_OverseasInsuranceCurrency = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
		//			AssertNotNull("OverseasInsuranceCurrency", GroupHeaderWrapper.OverseasInsuranceCurrency);
		//			AssertEquals("OverseasInsuranceCurrency is of type DocCurrency", typeof(DocCurrency), GroupHeaderWrapper.OverseasInsuranceCurrency.GetType());
		//		}
		//
		//		public void TestPackingCostCurrency()
		//		{
		//			AssertNull("PackingCostCurrency", GroupHeaderWrapper.PackingCostCurrency);
		//
		//			GroupHeader.JZ_RX_PackingCostCurrency = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
		//			AssertNotNull("PackingCostCurrency", GroupHeaderWrapper.PackingCostCurrency);
		//			AssertEquals("PackingCostCurrency is of type DocCurrency", typeof(DocCurrency), GroupHeaderWrapper.PackingCostCurrency.GetType());
		//		}

		#endregion

		#region ZDateTime Fields

		public void TestInvoiceDate()
		{
			ZDateTime invoiceDate = new ZDateTime(2004, 04, 01);
			GroupHeaderInternal.JZ_InvoiceDate = invoiceDate;
			AssertEquals("InvoiceDate", invoiceDate, GroupHeaderWrapperInternal.InvoiceDate);
		}

		public void TestPaymentDate()
		{
			ZDateTime paymentDate = new ZDateTime(2004, 04, 01);
			GroupHeaderInternal.JZ_PaymentDate = paymentDate;
			AssertEquals("PaymentDate", paymentDate, GroupHeaderWrapperInternal.PaymentDate);
		}

		#endregion

		#region Implementation Fields

		public void TestInvoiceHeadersInternalTestMethod()
		{
			DocBaseJobComInvoiceGroupHeaderTestClass groupHeader = DocBaseJobComInvoiceGroupHeaderTestClass.New(GroupHeaderInternal, Factory);
			GroupHeaderInternal.JobComInvoiceHeaders.AddNew();
			Assert("InvoiceHeadersInternal has at least one element", groupHeader.InvoiceHeadersInternalTestMethod.Count > 0);
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				GroupHeaderWrapperInternal
			};
		}

		#region Implementation

		protected T GroupHeaderInternal;
		protected TWrapper GroupHeaderWrapperInternal
		{
			get { return CreateGroupHeaderWrapper(GroupHeaderInternal); }
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return CreateGroupHeaderWrapper(GroupHeaderInternal);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			GroupHeaderInternal = GetNewInvoiceGroupHeader();
			base.SetUp();
		}

		protected virtual T GetNewInvoiceGroupHeader()
		{
			return (T)Factory.New<BaseJobDeclaration>().JobComInvoiceGroupHeaders[0];
		}

		protected void AssertCharges(string chargeName, string calcAmountName, string calcCurrName)
		{
			if (GroupHeaderInternal.IncoTermAndChargeFactory.GetCharge(chargeName) != null)
			{
				BaseJobComInvHeaderCharge charge = GroupHeaderInternal.Charges.AddNew(chargeName, 12.32m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
				if (calcAmountName == "OtherCharges2")
				{
					charge.J7_IsDutiable = false;
					charge.J7_IsGSTApplicable = false;
				}

				ZDecimal actualAmount = (ZDecimal)GroupHeaderWrapperInternal[calcAmountName];
				DocCurrency actualDocCurrency = (DocCurrency)GroupHeaderWrapperInternal[calcCurrName];

				AssertEquals(calcAmountName, 12.32m, actualAmount);
				AssertNotNull(calcCurrName, actualDocCurrency);
				AssertEquals(calcCurrName, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, actualDocCurrency.Code);
			}
			else
			{
				Assert("Not Applicable", true);
			}
		}

		#endregion
	}
}
