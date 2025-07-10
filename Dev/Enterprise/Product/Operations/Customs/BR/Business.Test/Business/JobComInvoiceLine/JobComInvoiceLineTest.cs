using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.BR.Business.Constants;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be BR", Core.Constants.CountryCodes.Brazil, InvoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Brazil, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestPartType()
		{
			var factory2 = new BusinessObjectFactory();
			var importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();
			var bRCompany = Factory.New<GlbCompany>();
			bRCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			var bRBranch = bRCompany.Branches.AddNew();
			bRBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil)).RL_Code;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.JE_GB = bRBranch.PK;
			var invoiceLine = Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var factory3 = new BusinessObjectFactory();
				var declarationLoaded = factory3.Load<JobDeclaration>(Declaration.PK);
				AssertEquals("product type still the type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
			}
		}

		public void TestJI_Calc_InvAmount_Scenario1()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				InvoiceHeader.JZ_IncoTerm = "TET";
				InvoiceHeader.JZ_InvoiceAmount = 1412.00m;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = "BRL";
				InvoiceLine.JI_LinePrice = 500m;
				var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 500m;
				var testCharge2 = InvoiceLine.Charges.AddNew();
				testCharge2.J7_ChargeType = "OTH";
				testCharge2.J7_RX_NKCurrency = "BRL";
				testCharge2.J7_IsIncludedInITOT = false;
				testCharge2.J7_IsDutiable = false;
				testCharge2.J7_Calc_IsIncludedInInvoiceAmount = true;
				testCharge2.J7_Amount = 12m;
				var charge = InvoiceHeader.Charges.AddNew();
				charge.J7_ChargeType = "TET";
				charge.J7_RX_NKCurrency = "BRL";
				charge.J7_Calc_IsIncludedInInvoiceAmount = true;
				charge.J7_Amount = 400m;
				Declaration.ResumeApportionment();
				AssertEquals(712m, InvoiceLine.JI_Calc_InvAmount);

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals(712m, InvoiceLine.JI_Calc_InvAmount);
			}
		}

		public void TestJI_Calc_InvAmount_Scenario2()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_IncoTerm = "TET";
			InvoiceHeader.JZ_InvoiceAmount = 1000;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "BRL";
			InvoiceLine.JI_LinePrice = 500m;
			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			var testCharge2 = InvoiceLine.Charges.AddNew();
			testCharge2.J7_ChargeType = "TET";
			testCharge2.J7_RX_NKCurrency = "BRL";
			testCharge2.J7_IsIncludedInITOT = false;
			testCharge2.J7_IsDutiable = false;
			testCharge2.J7_Calc_IsIncludedInInvoiceAmount = false;
			testCharge2.J7_Amount = 12m;
			var invoiceCharge = InvoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "TET";
			invoiceCharge.J7_RX_NKCurrency = "BRL";
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceCharge.J7_Amount = 400m;
			Declaration.ResumeApportionment();
			AssertEquals(500m, InvoiceLine.JI_Calc_InvAmount);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(500m, InvoiceLine.JI_Calc_InvAmount);
		}

		public void TestJI_Calc_InvAmount_Scenario3()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				InvoiceHeader.JZ_IncoTerm = "TET";
				InvoiceHeader.JZ_InvoiceAmount = 1000;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = "BRL";
				InvoiceLine.JI_LinePrice = 500m;
				var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 500m;
				var testCharge2 = InvoiceLine.Charges.AddNew();
				testCharge2.J7_ChargeType = "TET";
				testCharge2.J7_RX_NKCurrency = "BRL";
				testCharge2.J7_IsIncludedInITOT = false;
				testCharge2.J7_IsDutiable = true;
				testCharge2.J7_Calc_IsIncludedInInvoiceAmount = true;
				testCharge2.J7_Amount = 12m;
				var invoiceCharge = InvoiceHeader.Charges.AddNew();
				invoiceCharge.J7_ChargeType = "TET";
				invoiceCharge.J7_RX_NKCurrency = "BRL";
				invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
				invoiceCharge.J7_Amount = 400m;
				Declaration.ResumeApportionment();
				AssertEquals(712m, InvoiceLine.JI_Calc_InvAmount);

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals(712m, InvoiceLine.JI_Calc_InvAmount);
			}
		}

		public void TestJI_Calc_InvAmount_Scenario4()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_IncoTerm = "TET";
			InvoiceHeader.JZ_InvoiceAmount = 1000;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "BRL";
			InvoiceLine.JI_LinePrice = 500m;
			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			var testCharge2 = InvoiceLine.Charges.AddNew();
			testCharge2.J7_ChargeType = "TET";
			testCharge2.J7_RX_NKCurrency = "BRL";
			testCharge2.J7_IsIncludedInITOT = false;
			testCharge2.J7_IsDutiable = true;
			testCharge2.J7_Calc_IsIncludedInInvoiceAmount = false;
			testCharge2.J7_Amount = 12m;
			var invoiceCharge = InvoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "TET";
			invoiceCharge.J7_RX_NKCurrency = "BRL";
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceCharge.J7_Amount = 400m;
			Declaration.ResumeApportionment();
			AssertEquals(500m, InvoiceLine.JI_Calc_InvAmount);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(500m, InvoiceLine.JI_Calc_InvAmount);
		}

		public void TestJI_Calc_InvAmount_Scenario5()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_IncoTerm = "TET";
			InvoiceHeader.JZ_InvoiceAmount = 1412;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "BRL";
			InvoiceLine.JI_LinePrice = 500m;
			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			var testCharge2 = InvoiceLine.Charges.AddNew();
			testCharge2.J7_ChargeType = "TET";
			testCharge2.J7_RX_NKCurrency = "BRL";
			testCharge2.J7_IsIncludedInITOT = true;
			testCharge2.J7_IsDutiable = false;
			testCharge2.J7_Calc_IsIncludedInInvoiceAmount = true;
			testCharge2.J7_Amount = 12m;
			var invoiceCharge = InvoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "TET";
			invoiceCharge.J7_RX_NKCurrency = "BRL";
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceCharge.J7_Amount = 400m;
			Declaration.ResumeApportionment();
			AssertEquals(500m, InvoiceLine.JI_Calc_InvAmount);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(500m, InvoiceLine.JI_Calc_InvAmount);
		}

		public void TestJI_Calc_InvAmount_Scenario6()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_IncoTerm = "TET";
			InvoiceHeader.JZ_InvoiceAmount = 1000;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "BRL";
			InvoiceLine.JI_LinePrice = 500m;
			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			var testCharge2 = InvoiceLine.Charges.AddNew();
			testCharge2.J7_ChargeType = "TET";
			testCharge2.J7_RX_NKCurrency = "BRL";
			testCharge2.J7_IsIncludedInITOT = true;
			testCharge2.J7_IsDutiable = true;
			testCharge2.J7_Calc_IsIncludedInInvoiceAmount = true;
			testCharge2.J7_Amount = 12m;
			var invoiceCharge = InvoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "TET";
			invoiceCharge.J7_RX_NKCurrency = "BRL";
			invoiceCharge.J7_IsIncludedInITOT = true;
			invoiceCharge.J7_Amount = 400m;
			Declaration.ResumeApportionment();
			AssertEquals(500m, InvoiceLine.JI_Calc_InvAmount);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(500m, InvoiceLine.JI_Calc_InvAmount);
		}

		public void TestJI_Calc_InvAmount_CurrencyConsolidationScenario()
		{
			using (SetReciprocalFlagForCurrentCompany(true))
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				InvoiceHeader.JZ_IncoTerm = "TET";
				InvoiceHeader.JZ_InvoiceAmount = 2600.00m;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
				InvoiceLine.JI_LinePrice = 700.00m;
				var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 300.00m;
				var oUSDRate = InvoiceLine.InvoiceHeader.Invoice_Currency.ExchangeRates.AddNew();
				oUSDRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				oUSDRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
				oUSDRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
				oUSDRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
				oUSDRate.RE_SellRate = 5.0001m;
				var oEURRate = InvoiceLine.InvoiceHeader.Invoice_Currency.ExchangeRates.AddNew();
				oEURRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				oEURRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
				oEURRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
				oEURRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
				oEURRate.RE_SellRate = 6.0001m;
				var testUSDCharge = InvoiceLine.Charges.AddNew();
				testUSDCharge.J7_ChargeType = "OTH";
				testUSDCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				testUSDCharge.J7_IsIncludedInITOT = false;
				testUSDCharge.J7_IsDutiable = false;
				testUSDCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
				testUSDCharge.J7_Amount = 100.00m;
				var testEURCharge = InvoiceLine.Charges.AddNew();
				testEURCharge.J7_ChargeType = "OTH";
				testEURCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				testEURCharge.J7_IsIncludedInITOT = false;
				testEURCharge.J7_IsDutiable = false;
				testEURCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
				testEURCharge.J7_Amount = 100.00m;
				var oInvoiceUSDCharge = InvoiceHeader.Charges.AddNew();
				oInvoiceUSDCharge.J7_ChargeType = "TET";
				oInvoiceUSDCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				oInvoiceUSDCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
				oInvoiceUSDCharge.J7_Amount = 100.00m;
				Declaration.ResumeApportionment();
				AssertEquals(2150.03m, InvoiceLine.JI_Calc_InvAmount);
				AssertEquals(450m, invoiceLine2.JI_Calc_InvAmount);

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				oUSDRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				oEURRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				AssertEquals(2150.03m, InvoiceLine.JI_Calc_InvAmount);
				AssertEquals(450m, invoiceLine2.JI_Calc_InvAmount);
			}
		}

		public void TestJI_Calc_InvAmount_DiscountScenario()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_IncoTerm = "TET";
			InvoiceHeader.JZ_InvoiceAmount = 2500.00m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			InvoiceLine.JI_LinePrice = 1000.00m;
			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1500.00m;
			var discountCharge = InvoiceLine.Charges.AddNew();
			discountCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			discountCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			discountCharge.J7_IsIncludedInITOT = false;
			discountCharge.J7_IsNotIncludedInInvoice = false;
			discountCharge.J7_Amount = 100.00m;
			Declaration.ResumeApportionment();
			AssertEquals(900m, InvoiceLine.JI_Calc_InvAmount);
			AssertEquals(1500m, invoiceLine2.JI_Calc_InvAmount);
		}

		public void TestJI_Calc_InvAmount_DiscountScenario1()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				InvoiceHeader.JZ_IncoTerm = "TET";
				InvoiceHeader.JZ_InvoiceAmount = 2000.00m;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
				InvoiceLine.JI_LinePrice = 1000.00m;
				var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 1500.00m;
				var discountCharge = InvoiceLine.Charges.AddNew();
				discountCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				discountCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
				discountCharge.J7_IsIncludedInITOT = false;
				discountCharge.J7_IsNotIncludedInInvoice = false;
				discountCharge.J7_Amount = 100.00m;
				var discountInvoiceCharge = InvoiceHeader.Charges.AddNew();
				discountInvoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				discountInvoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
				discountInvoiceCharge.J7_IsIncludedInITOT = false;
				discountInvoiceCharge.J7_IsNotIncludedInInvoice = false;
				discountInvoiceCharge.J7_IsDutiable = true;
				discountInvoiceCharge.J7_Amount = 400m;
				Declaration.ResumeApportionment();
				AssertEquals(740m, InvoiceLine.JI_Calc_InvAmount);
				AssertEquals(1260m, invoiceLine2.JI_Calc_InvAmount);
			}
		}

		public void TestJI_Calc_InvAmount_DiscountScenario2()
		{
			using (SetReciprocalFlagForCurrentCompany(true))
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				InvoiceHeader.JZ_IncoTerm = "TET";
				InvoiceHeader.JZ_InvoiceAmount = 1600.00m;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
				InvoiceLine.JI_LinePrice = 1000.00m;
				var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 1500.00m;
				var oUSDRate = InvoiceLine.InvoiceHeader.Invoice_Currency.ExchangeRates.AddNew();
				oUSDRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				oUSDRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
				oUSDRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
				oUSDRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
				oUSDRate.RE_SellRate = 5m;
				var oDiscountCharge = InvoiceLine.Charges.AddNew();
				oDiscountCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				oDiscountCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				oDiscountCharge.J7_IsIncludedInITOT = false;
				oDiscountCharge.J7_IsNotIncludedInInvoice = false;
				oDiscountCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
				oDiscountCharge.J7_Amount = 100.00m;
				var oDiscountInvoiceCharge = InvoiceHeader.Charges.AddNew();
				oDiscountInvoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				oDiscountInvoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
				oDiscountInvoiceCharge.J7_IsIncludedInITOT = false;
				oDiscountInvoiceCharge.J7_IsNotIncludedInInvoice = false;
				oDiscountInvoiceCharge.J7_IsDutiable = true;
				oDiscountInvoiceCharge.J7_Amount = 400m;
				Declaration.ResumeApportionment();
				AssertEquals(340m, InvoiceLine.JI_Calc_InvAmount);
				AssertEquals(1260m, invoiceLine2.JI_Calc_InvAmount);
			}
		}

		public void TestJI_Calc_InvAmount_Rounded()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceHeader.JZ_IncoTerm = "TET";
			InvoiceHeader.JZ_InvoiceAmount = 1000.00m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			InvoiceLine.JI_LinePrice = 1000.00m;
			
			var firstCharge = InvoiceLine.Charges.AddNew();
			firstCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			firstCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;
			firstCharge.J7_IsIncludedInITOT = false;
			firstCharge.J7_IsNotIncludedInInvoice = false;
			firstCharge.J7_Amount = 100.42m;

			var secondCharge = InvoiceLine.Charges.AddNew();
			secondCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			secondCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;
			secondCharge.J7_IsIncludedInITOT = false;
			secondCharge.J7_IsNotIncludedInInvoice = false;
			secondCharge.J7_Amount = 100.42m;

			Declaration.ResumeApportionment();
			AssertEquals(1201.00m, InvoiceLine.JI_Calc_InvAmount);
		}

		public static IDisposable SetReciprocalFlagForCurrentCompany(bool isReciprocal)
		{
			return new ReciprocalSetter(isReciprocal);
		}

		class ReciprocalSetter : IDisposable
		{
			public ReciprocalSetter(bool isReciprocal)
			{
				originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
				GlbCompany.CurrentCompany.GC_IsReciprocal = isReciprocal;
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
				try
				{
					GlbCompany.CurrentCompany.Factory.Save();
				}
				finally
				{
					((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
				}
			}

			readonly bool originalReciprocal;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
				try
				{
					GlbCompany.CurrentCompany.Factory.Save();
				}
				finally
				{
					((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
				}
			}

			#endregion
		}

		public void TestReadOnlyWhenClonedFromAttached()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "TEST1";
			supplier.Addresses.AddNew();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "TEST1";

			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_OA_SupplierAddress = supplier.Addresses[0].PK;
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.DutyTaxRegime = "2";
			invLine.DutyLegalBase = "5";

			var editableProperties = invLine.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => !x.ReadOnly).Select(x => x.Name);

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var licEntryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			licEntryInstruction.CEI_JE = licDeclaration.PK;
			licEntryInstruction.CEI_Description = "TEST1";

			var licInvHeader = licDeclaration.Invoices.AddNew();
			var licInvLine = licInvHeader.InvoiceLines.AddNew();
			licInvLine.JI_CEI = licEntryInstruction.PK;

			var licEntryHeader = licDeclaration.CustomsEntryHeaders.AddNew();
			licEntryHeader.CH_CEI_Instruction = licEntryInstruction.PK;
			var licEntryLine = licEntryHeader.AllEntryLines.AddNew();
			licInvLine.JI_CL = licEntryLine.PK;

			licEntryInstruction.EntryHeader.MovementReferenceNumberSetter("TST_LIC", ZDateTime.Now);
			declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licEntryInstruction) });

			var clonedInvoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Single(x => x.ImportLicenseNumber == "TST_LIC");

			var readOnlyPropertiesList = new[] { JobComInvoiceLine.Schema.JI_Tariff, JobComInvoiceLine.Schema.FullGoodsDescription, JobComInvoiceLine.Schema.NaladiHs, JobComInvoiceLine.Schema.JI_InvoiceQuantity, JobComInvoiceLine.Schema.JI_InvoiceUQ, JobComInvoiceLine.Schema.JI_CustomsQuantity,
				JobComInvoiceLine.Schema.JI_LinePrice, JobComInvoiceLine.Schema.JI_NetWeight, JobComInvoiceLine.Schema.JI_NetWeightUQ, JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress, JobComInvoiceLine.Schema.JI_CountryOfOrigin, JobComInvoiceLine.Schema.JI_FormattedTariff,
				JobComInvoiceLine.Schema.JI_CC, JobComInvoiceLine.Schema.JI_PartAttrib1, JobComInvoiceLine.Schema.JI_PartAttrib2, JobComInvoiceLine.Schema.JI_PartAttrib3, JobComInvoiceLine.Schema.JI_PartNo, JobComInvoiceLine.Schema.JI_SerialNumber, JobComInvoiceLine.Schema.JI_ManufacturerIndicator,
				JobComInvoiceLine.Schema.JI_RequiresImportLicense, JobComInvoiceLine.Schema.DutyTaxRegime, JobComInvoiceLine.Schema.DutyLegalBase, JobComInvoiceLine.Schema.JI_SecondaryPreference };

			CombineAssertions("Cloned Invoice Line", () =>
			{
				Assert("HasLinkedInvoiceLine should be true", clonedInvoiceLine.HasLinkedInvoiceLine);

				foreach (var propertyName in editableProperties)
				{
					if (readOnlyPropertiesList.Contains(propertyName))
					{
						Assert($"{propertyName} should be ReadOnly", clonedInvoiceLine.FindPropertyInfo(propertyName).ReadOnly);
					}
					else
					{
						Assert($"{propertyName} should NOT be ReadOnly", !clonedInvoiceLine.FindPropertyInfo(propertyName).ReadOnly);
					}
				}

				Assert("NVECusCodeDataCollection should be ReadOnly", clonedInvoiceLine.NVECusCodeDataCollection.ReadOnly);
				Assert("TariffDetachCollection should be ReadOnly", clonedInvoiceLine.TariffDetachs.ReadOnly);
			});

			var newInvHeader = declaration.Invoices.AddNew();
			var newInvLine = invHeader.InvoiceLines.AddNew();

			CombineAssertions("New Invoice Line", () =>
			{
				Assert("HasLinkedInvoiceLine should be false", !newInvLine.HasLinkedInvoiceLine);

				foreach (var propertyName in editableProperties)
				{
					Assert($"{propertyName} should NOT be ReadOnly", !newInvLine.FindPropertyInfo(propertyName).ReadOnly);
				}

				Assert("NVECusCodeDataCollection should NOT be ReadOnly", !newInvLine.NVECusCodeDataCollection.ReadOnly);
				Assert("TariffDetachCollection should NOT be ReadOnly", !newInvLine.TariffDetachs.ReadOnly);
			});
		}

		public void TestHasLinkedInvoiceLine()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals(false, invoiceLine1.HasLinkedInvoiceLine);
			invoiceLine1.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invoiceLine1.JI_ParentID = invoiceLine2.PK;
			AssertEquals(true, invoiceLine1.HasLinkedInvoiceLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals(true, invoiceLine1.HasLinkedInvoiceLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals(true, invoiceLine1.HasLinkedInvoiceLine);
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals(false, invoiceLine1.HasLinkedInvoiceLine);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestJI_NFeLinePriceReadOnly()
		{
			AssertEquals("JI_NFeLinePrice.ReadOnly", true, InvoiceLine.JI_NFeLinePriceInfo.ReadOnly);
		}

		public void TestJI_NFeLinePriceCaption()
		{
			AssertEquals($"{nameof(InvoiceLine.JI_NFeLinePriceInfo)} caption", "NF-e Total Value", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_NFeLinePriceInfo).Caption);
		}

		public void TestDutiesCaption()
		{
			CombineAssertions(() =>
			{
				AssertEquals($"{nameof(InvoiceLine.DutyTaxRegimeInfo)} caption", "Duty Tax Regime", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.DutyTaxRegimeInfo).Caption);
				AssertEquals($"{nameof(InvoiceLine.DutyLegalBaseInfo)} caption", "Duty Legal Base", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.DutyLegalBaseInfo).Caption);
				AssertEquals($"{nameof(InvoiceLine.IPITaxRegimeInfo)} caption", "IPI Tax Regime", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.IPITaxRegimeInfo).Caption);
				AssertEquals($"{nameof(InvoiceLine.JI_ComplementaryNoteInfo)} caption", "TIPI Complementary Note", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_ComplementaryNoteInfo).Caption);
				AssertEquals($"{nameof(InvoiceLine.PisCofinsTaxRegimeInfo)} caption", "PIS/COFINS Tax Regime", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.PisCofinsTaxRegimeInfo).Caption);
				AssertEquals($"{nameof(InvoiceLine.PisCofinsLegalBaseInfo)} caption", "PIS/COFINS Legal Base", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.PisCofinsLegalBaseInfo).Caption);
				AssertEquals($"{nameof(InvoiceLine.ICMSTaxRegimeInfo)} caption", "ICMS Tax Regime", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ICMSTaxRegimeInfo).Caption);
				AssertEquals($"{nameof(InvoiceLine.ICMSLegalBaseInfo)} caption", "ICMS Legal Base", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ICMSLegalBaseInfo).Caption);
			});
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("There is no tariff and CustomsQuantity should be readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			InvoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", true, InvoiceLine.JI_CustomsUnitQty.IsEmpty);
			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestUpdateCompDescriptionFromPivot()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PartNum";
			var supplier = part.RelatedOrganisations.AddNew();
			var supplierOrg = Factory.New<OrgHeader>();
			supplier.OU_OH = supplierOrg.PK;
			supplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_OH = supplier.OU_OH;
			pivot.ComplementaryDescription = new ZString('X', pivot.ComplementaryDescriptionInfo.MaxLength);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var oJobComInvoiceLine = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			oJobComInvoiceLine.JI_PartNo = part.OP_PartNum;
			oJobComInvoiceLine.ComplementaryDescription = "JobComInvLine complementary description";
			oJobComInvoiceLine.InvoiceHeader.JZ_OA_SupplierAddress = supplierOrg.MainAddress.PK;
			AssertEquals("Comp Description are Equals", new ZString('X', oJobComInvoiceLine.ComplementaryDescriptionInfo.MaxLength), oJobComInvoiceLine.ComplementaryDescription);
		}

		public void TestComplementaryDescripton()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.ComplementaryDescription = "ComplementaryDescriptionExport";
			Factory.Save();
			AssertEquals("ComplementaryDescriptionExport", InvoiceLine.ComplementaryDescription);
			AssertEquals(2000, InvoiceLine.ComplementaryDescriptionInfo.MaxLength);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.ComplementaryDescription = "ComplementaryDescriptionImport";
			Factory.Save();
			AssertEquals("ComplementaryDescriptionImport", InvoiceLine.ComplementaryDescription);
			AssertEquals(4000, InvoiceLine.ComplementaryDescriptionInfo.MaxLength);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Factory.Save();
			AssertEquals(ZString.Empty, InvoiceLine.ComplementaryDescription);
			AssertEquals(2000, InvoiceLine.ComplementaryDescriptionInfo.MaxLength);
		}

		public void TestMultipleKeysToUse()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var multipleKeySupport = (ISupportMultipleResourceStringData)InvoiceLine;
			AssertSequencesEqual($"JE_MessageType={InvoiceLine.MessageType}", new[] { BRJobMessageTypeList.Codes.Import }, multipleKeySupport.MultipleKeysToUse);
		}

		public override void TestWipeNKTaxType()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "099", "11111", "", "", "Exportacao Normal", "EXP", group: "IFD");
			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_ZZF_NKTaxType = "605";
			InvoiceLine.JI_Procedure = "11111";
			Assert(InvoiceLine.ShouldWipeNKTaxType);
			AssertEquals(InvoiceLine.ShouldWipeNKTaxType ? "" : "605", InvoiceLine.JI_ZZF_NKTaxType);
			InvoiceLine.JI_ZZF_NKTaxType = "605";
			InvoiceLine.JI_Procedure = "11222";
			Assert(!InvoiceLine.ShouldWipeNKTaxType);
			AssertEquals("605", InvoiceLine.JI_ZZF_NKTaxType);
			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			InvoiceLine.JI_Procedure = "11111";
			Assert(!InvoiceLine.ShouldWipeNKTaxType);
			AssertEquals("605", InvoiceLine.JI_ZZF_NKTaxType);
		}

		#region Cargo Priority

		public void TestCargoPriorityList()
		{
			CombineAssertions(() =>
			{
				var list = Lookups.CargoPriorityList;
				AssertEquals("CargoPriorityList", "5001, 5002, 5003, 5006", list.CodesAsString);
				AssertSame(list, InvoiceLine.Lookups.CargoPriorityList);
			}

			);
		}

		JobComInvoiceLineLookups Lookups
		{
			get
			{
				return InvoiceLine.Lookups;
			}
		}

		public void TestJI_CargoPriority()
		{
			CombineAssertions(() =>
			{
				InvoiceLine.JI_CargoPriority = "1111";
				AssertHasMessageError("Invalid Code", InvoiceLine.JI_CargoPriorityInfo, ListValidation.InvalidCodeMessageError);
				InvoiceLine.JI_CargoPriority = "5001";
				AssertNoMessageError("Valid Code", InvoiceLine.JI_CargoPriorityInfo, ListValidation.InvalidCodeMessageError);
			}

			);
		}

		public void TestLookups()
		{
			AssertEquals(Factory.GetCachedValue<CargoPriorityList>(), InvoiceLine.Lookups.CargoPriorityList);
		}

		#endregion

		public void TestLpcoJobComInvLineRefsCollection()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var lpco = InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
			lpco.JG_ReferenceNumber = "123";
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(JobComInvLineRefs));
			query.AddToFilter(JobComInvLineRefsSchema.JG_JI, InvoiceLine.PK);
			query.AddToFilter(JobComInvLineRefsSchema.JG_ReferenceType, JobComInvLineRefsType.Codes.Lpco);
			var jobComInvLineRefs = Factory.Load(typeof(JobComInvLineRefs), query);
			AssertEquals(1, jobComInvLineRefs.Length);
		}

		public void TestTariffDetachCollection()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var lpco = InvoiceLine.TariffDetachs.AddNew();
			lpco.CY_Code = "999";
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(CusCodeData));
			query.AddToFilter(CusCodeDataSchema.CY_ParentID, InvoiceLine.PK);
			query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, JobComInvoiceLineSchema.Constants.Prefix);
			query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.TariffDetach);
			var jobComInvLineTariffsDetach = Factory.Load(typeof(CusCodeData), query);
			AssertEquals(1, jobComInvLineTariffsDetach.Length);
		}

		public void TestIChargeApportionee()
		{
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			InvoiceHeader.JZ_InvoiceAmount = 10000m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceHeader.JZ_IncoTerm = "FOB";
			InvoiceHeader.JZ_Weight = 600m;
			InvoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceHeader.JZ_NetWeight = 450m;
			InvoiceHeader.JZ_NetWeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.JI_Weight = 150m;
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.JI_NetWeight = 100m;
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.JI_LinePrice = 500m;
			invoiceLine2.JI_Weight = 200m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_NetWeight = 150m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_LinePrice = 300m;
			invoiceLine3.JI_Weight = 250m;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine3.JI_NetWeight = 200m;
			invoiceLine3.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine3.JI_LinePrice = 200m;
			var charge = InvoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 500m;
			charge.J7_RX_NKCurrency = Declaration.LocalCurrencyCode;
			charge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Weight;
			Declaration.ResumeApportionment();
			CombineAssertions("Apportioned by Gross Weight", () =>
			{
				AssertEquals("Apportioned OFT on Line 1", 125.00m, InvoiceLine.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
				AssertEquals("Apportioned OFT on Line 2", 166.67m, invoiceLine2.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
				AssertEquals("Apportioned OFT on Line 3", 208.33m, invoiceLine3.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
			}

			);
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.NetWeight;
			Declaration.ResumeApportionment();
			CombineAssertions("Apportioned by Net Weight", () =>
			{
				AssertEquals("Apportioned OFT on Line 1", 111.11m, InvoiceLine.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
				AssertEquals("Apportioned OFT on Line 2", 166.67m, invoiceLine2.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
				AssertEquals("Apportioned OFT on Line 3", 222.22m, invoiceLine3.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
			}

			);

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.FOB;
			Declaration.ResumeApportionment();
			CombineAssertions("Apportioned by FOB Value", () =>
			{
				AssertEquals("Apportioned OFT on Line 1", 250m, InvoiceLine.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
				AssertEquals("Apportioned OFT on Line 2", 150m, invoiceLine2.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
				AssertEquals("Apportioned OFT on Line 3", 100m, invoiceLine3.ApportionedCharges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight).First().J7_Amount);
			}

			);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)InvoiceLine;
			AssertEquals(typeof(SuspensionDrawback), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.BR.CusSupportingInfoTypeList.Codes.SuspensionDrawback]);
			AssertEquals(typeof(PreviousDocument), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.BR.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			AssertEquals(typeof(ReferenceInvoiceManual), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.BR.CusSupportingInfoTypeList.Codes.ReferenceInvoiceManual]);
			AssertEquals(typeof(ElectronicLogisticInvoice), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.BR.CusSupportingInfoTypeList.Codes.ElectronicLogisticInvoice]);
			AssertEquals(typeof(ComplementaryLogisticInvoice), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.BR.CusSupportingInfoTypeList.Codes.ComplementaryLogisticInvoice]);
			AssertEquals(typeof(DrawbackImportLicense), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.BR.CusSupportingInfoTypeList.Codes.Drawback]);
			AssertEquals(typeof(DuimpTaxRegime), cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Common.BR.CusSupportingInfoTypeList.Codes.DuimpTaxRegime]);
		}

		public void TestRebuildCollectionFromCharacteristicOnTariffChanged()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = "56049000";

			AssertEquals("AttributeCusCodeDataCollection count should be", 1, InvoiceLine.Attributes.Count);
			AssertEquals("NVECusCodeDataCollection count should be", 0, InvoiceLine.NVECusCodeDataCollection.Count);

			var att1 = InvoiceLine.Attributes.GetFirstElementHaving("ATT_2557");
			AssertEquals("CY_DataFieldType", "ATT_2557", att1.CY_Code);
			AssertEquals("Forma Preenchimento = LISTA_ESTATICA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, att1.TariffProfileQuestion.AnswerDataType);

			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals("Attribute should be 0", 0, InvoiceLine.Attributes.Count);
			AssertEquals("NVECusCodeDataCollection count should be", 0, InvoiceLine.NVECusCodeDataCollection.Count);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "56049000";
			AssertEquals("AttributeCusCodeDataCollection count should be", 0, InvoiceLine.Attributes.Count);
			AssertEquals("NVECusCodeDataCollection count should be", 1, InvoiceLine.NVECusCodeDataCollection.Count);

			var nveAA = InvoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AA");
			AssertEquals("CY_DataFieldType", "AA", nveAA.CY_Code);
			AssertEquals("Forma Preenchimento = LISTA_ESTATICA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, nveAA.TariffCharacteristic.ZB1_Style);

			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals("AttributeCusCodeDataCollection should be 0", 0, InvoiceLine.Attributes.Count);
			AssertEquals("NVECusCodeDataCollection count should be", 0, InvoiceLine.NVECusCodeDataCollection.Count);
		}

		public void TestResetTaxDetailsDataOnTariffChanged()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "56049000", 50m);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 50m, rateType: Constants.RateTypes.IPI, rateCode: Constants.RateCodes.IPI);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "06056800", 50m, rateType: Constants.RateTypes.IPI, rateCode: Constants.RateCodes.IPI, tradeGroupCountry: "ZA");

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("IPITaxRegime should be", ZString.Empty, InvoiceLine.IPITaxRegime);

			InvoiceLine.JI_CountryOfOrigin = "CA";
			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals("IPITaxRegime default to empty when Tariff is invalid", ZString.Empty, InvoiceLine.IPITaxRegime);

			InvoiceLine.JI_Tariff = "56049000";
			AssertEquals("IPITaxRegime default to 3 when Tariff has no IPI rate", IPITaxRegimeList.Codes.NonTaxable, InvoiceLine.IPITaxRegime);

			InvoiceLine.JI_Tariff = "03024100";
			AssertEquals("IPITaxRegime default to empty when Tariff has IPI rate", ZString.Empty, InvoiceLine.IPITaxRegime);

			InvoiceLine.JI_CountryOfOrigin = "ZA";
			InvoiceLine.JI_Tariff = "06056800";
			AssertEquals("IPITaxRegime default to empty when Tariff has IPI rate", ZString.Empty, InvoiceLine.IPITaxRegime);

			InvoiceLine.JI_Tariff = "56049000";
			InvoiceLine.JI_CountryOfOrigin = "CA";
			AssertEquals("IPITaxRegime default to 3 when Tariff has no IPI rate", IPITaxRegimeList.Codes.NonTaxable, InvoiceLine.IPITaxRegime);

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
			InvoiceLine.IPIRateIsOverridden = true;
			InvoiceLine.PisRateIsOverridden = true;
			InvoiceLine.CofinsRateIsOverridden = true;
			InvoiceLine.AdditionalTariffs.AddNew();
			InvoiceLine.SpecialCaseTaxes.AddNew();

			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("IPIRateIsOverridden should be reset", false, InvoiceLine.IPIRateIsOverridden);
			AssertEquals("PisRateIsOverridden should be reset", false, InvoiceLine.PisRateIsOverridden);
			AssertEquals("CofinsRateIsOverridden should be reset", false, InvoiceLine.CofinsRateIsOverridden);
			AssertEquals("AdditionalTariffs should be clear", 0, InvoiceLine.AdditionalTariffs.Count);
			AssertEquals("SpecialCaseTaxes should be clear", 0, InvoiceLine.SpecialCaseTaxes.Count);
			AssertEquals("IPITaxRegime default to empty when Tariff has IPI rate", ZString.Empty, InvoiceLine.IPITaxRegime);
			AssertEquals("JI_PrimaryPreference should be reset", Constants.RatePreferenceType.Normal, InvoiceLine.JI_PrimaryPreference);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.SpecialCaseTaxes.AddNew();

			InvoiceLine.JI_CountryOfOrigin = "CA";
			AssertEquals("SpecialCaseTaxes should be clear", 0, InvoiceLine.SpecialCaseTaxes.Count);
		}

		public override void TestChargeTypeList()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			ICommonInvoice commonInvoice = Declaration.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;

			AssertContainsExactElementsInAnyOrder(new[] { "EIC", "OAC", "CIA", "CBC", "DEC", "ENG", "FIN", "IFE", "IFI", "ILO", "INS", "ISE", "ISI", "LOA", "LOE", "LOI", "MAT", "MCP", "DPA", "PAR", "ROT", "ROY", "TMM", "ONS", "FCO", "FNT", "OFC", "OFP" }, chargeTypeList1.GetAllCodes());

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			ICommonInvoice commonInvoice2 = Declaration.TopGroupInvoice;
			var chargeTypeList3 = commonInvoice2.ChargeTypeList;

			AssertContainsExactElementsInAnyOrder(new[] { "ADD", "COM", "DED", "DIS", "EXW", "FIF", "LCH", "OFT", "ONS", "OTH", "PAC" }, chargeTypeList3.GetAllCodes());

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			ICommonInvoice commonInvoice3 = Declaration.TopGroupInvoice;
			var chargeTypeList4 = commonInvoice3.ChargeTypeList;

			AssertContainsExactElementsInAnyOrder(new[] { "EIC", "OAC", "CIA", "CBC", "DEC", "ENG", "FIN", "IFE", "IFI", "ILO", "INS", "ISE", "ISI", "LOA", "LOE", "LOI", "MAT", "MCP", "DPA", "PAR", "ROT", "ROY", "TMM", "ONS", "FCO", "FNT", "OFC", "OFP" }, chargeTypeList1.GetAllCodes());
		}

		public void TestNaladiNcca()
		{
			Declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.NaladiNcca = "32081020";

			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 1, InvoiceLine.CusLineTariffDetails.Count);
			var tariffDetail = InvoiceLine.CusLineTariffDetails[0];
			AssertCusLineTariffDetail(tariffDetail, Constants.TariffTypes.NCCA, "32081020");

			InvoiceLine.NaladiNcca = "32081021";
			AssertCusLineTariffDetail(tariffDetail, Constants.TariffTypes.NCCA, "32081021");

			InvoiceLine.NaladiNcca = ZString.Empty;
			Assert("CusLineTariffDetail should be deleted", tariffDetail.IsDeleted);
			AssertEquals(0, InvoiceLine.CusLineTariffDetails.Count);
		}

		public void TestNaladiHs()
		{
			Declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.NaladiHs = "32081020";

			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 1, InvoiceLine.CusLineTariffDetails.Count);
			var tariffDetail = InvoiceLine.CusLineTariffDetails[0];
			AssertCusLineTariffDetail(tariffDetail, Constants.TariffTypes.NALADIHS, "32081020");

			InvoiceLine.NaladiHs = "32081021";
			AssertCusLineTariffDetail(tariffDetail, Constants.TariffTypes.NALADIHS, "32081021");

			InvoiceLine.NaladiHs = ZString.Empty;
			Assert("CusLineTariffDetail should be deleted", tariffDetail.IsDeleted);
			AssertEquals(0, InvoiceLine.CusLineTariffDetails.Count);
		}

		public void TestLPCOConcatenated()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
			InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "001";
			InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "999";
			InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "555";

			AssertEquals("LPCOConcatenated should be", "001,555,999", InvoiceLine.LPCOConcatenated);
		}

		public void TestTariffDetachConcatenated()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			InvoiceLine.TariffDetachs.AddNew();
			InvoiceLine.TariffDetachs.AddNew().CY_Code = "001";
			InvoiceLine.TariffDetachs.AddNew().CY_Code = "999";
			InvoiceLine.TariffDetachs.AddNew().CY_Code = "555";

			AssertEquals("TariffDetachConcatenated should be", "001,555,999", InvoiceLine.TariffDetachConcatenated);
		}

		public void TestNVEConcatenated()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			AssertEquals("NVEConcatenated should be", ZString.Empty, InvoiceLine.NVEConcatenated);

			var nve = InvoiceLine.NVECusCodeDataCollection.AddNew();
			nve.CY_Code = "AA";
			nve.CY_Data = "0001";
			nve.CY_Order = 6;

			nve = InvoiceLine.NVECusCodeDataCollection.AddNew();
			nve.CY_Code = "AA";
			nve.CY_Data = "0002";
			nve.CY_Order = 6;

			nve = InvoiceLine.NVECusCodeDataCollection.AddNew();
			nve.CY_Code = "AB";
			nve.CY_Data = "0006";
			nve.CY_Order = 6;

			AssertEquals("NVEConcatenated should be", "6|AA|0001,6|AA|0002,6|AB|0006", InvoiceLine.NVEConcatenated);
		}

		public void TestConsentingProcessConcatenated()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			AssertEquals("ConsentingProcessConcatenated should be", ZString.Empty, InvoiceLine.ConsentingProcessConcatenated);

			var nve = InvoiceLine.ConsentingProcessCollection.AddNew();
			nve.CSI_ReferenceNumber = "2";
			nve.CSI_CustomsOffice = "AA";

			nve = InvoiceLine.ConsentingProcessCollection.AddNew();
			nve.CSI_ReferenceNumber = "1";
			nve.CSI_CustomsOffice = "AA";

			nve = InvoiceLine.ConsentingProcessCollection.AddNew();
			nve.CSI_ReferenceNumber = "2";
			nve.CSI_CustomsOffice = "AB";

			AssertEquals("ConsentingProcessConcatenated should be", "1|AA,2|AA,2|AB", InvoiceLine.ConsentingProcessConcatenated);
		}

		public void TestImportLicenseNumber()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var importLicense = InvoiceLine.ImportLicenseSupportingInfo;
			AssertEquals("A new ImportLicense should be created", 1, InvoiceLine.ImportLicenseInfos.Count);
			AssertEquals(CusSupportingInfoTypeList.Codes.ImportLicense, importLicense.CSI_Type);

			InvoiceLine.ImportLicenseType = "1";
			InvoiceLine.ImportLicenseNumber = "123456";
			InvoiceLine.ImportLicenseAuthorizationDate = new ZDateTime(2022, 10, 26);

			AssertEquals(CusSupportingInfoTypeList.Codes.ImportLicense, importLicense.CSI_Type);
			AssertEquals("CSI_Code", "1", importLicense.CSI_Code);
			AssertEquals("CSI_ReferenceNumber", "123456", importLicense.CSI_ReferenceNumber);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2022, 10, 26), importLicense.CSI_DateOfIssue);
			AssertEquals("Only one ImportLicense should be created", 1, InvoiceLine.ImportLicenseInfos.Count);
		}

		public void TestJI_CustomsValue()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_IncoTerm = "CIF";
			InvoiceHeader.JZ_InvoiceAmount = 1412.00m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			InvoiceLine.JI_LinePrice = 500m;

			var charge = InvoiceLine.Charges.AddNew();
			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.AdditionCharge;
			charge.J7_Amount = 50m;
			charge.J7_RX_NKCurrency = "USD";
			charge.J7_IsDutiable = false;
			charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			Declaration.ResumeApportionment();

			AssertNotEquals("JI_Calc_CIF_InLocalCurrency should not be equals to JI_Calc_CIF_InLocalCurrency", InvoiceLine.JI_Calc_CIF_InLocalCurrency, InvoiceLine.JI_Calc_FOB_InLocalCurrency);

			AssertEquals("JI_CustomsValue should be the JI_Calc_CIF_InLocalCurrency", InvoiceLine.JI_Calc_CIF_InLocalCurrency, InvoiceLine.JI_CustomsValue);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("JI_CustomsValue should be the JI_Calc_FOB_InLocalCurrency", InvoiceLine.JI_Calc_FOB_InLocalCurrency, InvoiceLine.JI_CustomsValue);
		}

		public void TestGetNewValidation()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Export Validation", typeof(ExportJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("Import Validation", typeof(ImportJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("Import Siscomex Validation", typeof(ImportSiscomexJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Drawback;
			AssertEquals("Other Message Type", typeof(JobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
		}

		public void TestOnFactorySaving()
		{
			void SetDateOnInvoiceLine(string messageType)
			{
				Declaration.JE_MessageType = messageType;
				InvoiceLine.IPITaxRegime = "1";
				InvoiceLine.PisCofinsTaxRegime = "2";
				InvoiceLine.DutyTaxRegime = "3";
				InvoiceLine.DutyLegalBase = "4";
				InvoiceLine.ICMSTaxRegime = "1";
				InvoiceLine.FMMBenefit = "1";

				InvoiceLine.CertificateOfOriginCollection.AddNew().CSI_SubType = CertificateTypeList.Codes.CCROM;
				InvoiceLine.ComplementaryLogisticInvoiceCollection.AddNew().CSI_ReferenceNumber = "1";
				InvoiceLine.ElectronicLogisticInvoiceCollection.AddNew().CSI_ReferenceNumber = "1";
				InvoiceLine.ReferenceInvoiceManualCollection.AddNew().CSI_ReferenceNumber = "1";
				InvoiceLine.SuspensionDrawbackCollection.AddNew().CSI_ReferenceNumber = "1";
				InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "REF";
				InvoiceLine.MercosulForeignDeclarations.AddNew().CSI_Description = "1";
				InvoiceLine.PreviousDocuments.AddNew().CSI_ReferenceNumber = "1";
				InvoiceLine.TariffDetachs.AddNew().CY_Code = "999";
				InvoiceLine.ConsentingProcessCollection.AddNew().CSI_ReferenceNumber = "1";
				InvoiceLine.DrawbackImportLicenseCollection.AddNew().CSI_Code = DrawbackModalityList.Codes.GenericSuspension;
				InvoiceLine.AdditionalTariffs.AddNew().ExNumber = "001";
				InvoiceLine.ImportLicenseInfos.AddNew().CSI_Code = "1";

				var specialCaseTax = InvoiceLine.SpecialCaseTaxes.AddNew();
				specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
				specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.AdValoremRate;
				specialCaseTax.LegalActType = "1";

				Factory.Save();
			}

			void AssertAnyInDatabase<T>(string message, bool expected, BusinessObjectCollection<T> collection, Func<T, bool> predicate = null) where T : BusinessObject
			{
				AssertEquals(message, expected, collection.Cast<T>().Any(x => x.IsInDatabase && (predicate?.Invoke(x) ?? true)));
			}

			SetDateOnInvoiceLine(BRJobMessageTypeList.Codes.Import);
			CombineAssertions(Declaration.JE_MessageType, () =>
			{
				AssertAnyInDatabase("There are no CertificateOfOriginCollection", false, InvoiceLine.CertificateOfOriginCollection);
				AssertAnyInDatabase("There are no ComplementaryLogisticInvoiceCollection", false, InvoiceLine.ComplementaryLogisticInvoiceCollection);
				AssertAnyInDatabase("There are no ElectronicLogisticInvoiceCollection", false, InvoiceLine.ElectronicLogisticInvoiceCollection);
				AssertAnyInDatabase("There are no ReferenceInvoiceManualCollection", false, InvoiceLine.ReferenceInvoiceManualCollection);
				AssertAnyInDatabase("There are no SuspensionDrawbackCollection", false, InvoiceLine.SuspensionDrawbackCollection);
				AssertAnyInDatabase("There are no LPCOJobComInvLineRefsCollection", false, InvoiceLine.LPCOJobComInvLineRefsCollection);
				AssertAnyInDatabase("There are MercosulForeignDeclarations", true, InvoiceLine.MercosulForeignDeclarations);
				AssertAnyInDatabase("There are PreviousDocumentCollection", true, InvoiceLine.PreviousDocuments);
				AssertAnyInDatabase("There are TariffDetachCollection", true, InvoiceLine.TariffDetachs);
				AssertAnyInDatabase("There are no ConsentingProcessCollection", false, InvoiceLine.ConsentingProcessCollection);
				AssertAnyInDatabase("There are no DrawbackImportLicenseCollection", false, InvoiceLine.DrawbackImportLicenseCollection);
				AssertEquals("There are no AdditionalTariffs", false, InvoiceLine.AdditionalTariffs.Count > 0);
				AssertEquals("There are SpecialCaseTaxes", true, InvoiceLine.SpecialCaseTaxes.Count > 0);
				AssertAnyInDatabase("There are no LegalActInfos", false, InvoiceLine.LegalActInfos);
				AssertAnyInDatabase("There are no IPITaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.IPI);
				AssertAnyInDatabase("There are no PISTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.PisCofins);
				AssertAnyInDatabase("There are ICMSTaxRegime", true, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.ICMS);
				AssertAnyInDatabase("There are no FMMBenefit", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.FMM);
				AssertAnyInDatabase("There are DutyTaxRegime", true, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.Duty);
				AssertAnyInDatabase("There are Taxes", true, InvoiceLine.Taxes);
				AssertAnyInDatabase("There are no importLicense", true, InvoiceLine.ImportLicenseInfos);
			});

			SetDateOnInvoiceLine(BRJobMessageTypeList.Codes.Drawback);
			CombineAssertions(Declaration.JE_MessageType, () =>
			{
				AssertAnyInDatabase("There are no CertificateOfOriginCollection", false, InvoiceLine.CertificateOfOriginCollection);
				AssertAnyInDatabase("There are no ComplementaryLogisticInvoiceCollection", false, InvoiceLine.ComplementaryLogisticInvoiceCollection);
				AssertAnyInDatabase("There are no ElectronicLogisticInvoiceCollection", false, InvoiceLine.ElectronicLogisticInvoiceCollection);
				AssertAnyInDatabase("There are no ReferenceInvoiceManualCollection", false, InvoiceLine.ReferenceInvoiceManualCollection);
				AssertAnyInDatabase("There are no SuspensionDrawbackCollection", false, InvoiceLine.SuspensionDrawbackCollection);
				AssertAnyInDatabase("There are no LPCOJobComInvLineRefsCollection", false, InvoiceLine.LPCOJobComInvLineRefsCollection);
				AssertAnyInDatabase("There are no MercosulForeignDeclarations", false, InvoiceLine.MercosulForeignDeclarations);
				AssertAnyInDatabase("There are no PreviousDocumentCollection", false, InvoiceLine.PreviousDocuments);
				AssertAnyInDatabase("There are no TariffDetachCollection", false, InvoiceLine.TariffDetachs);
				AssertAnyInDatabase("There are no ConsentingProcessCollection", false, InvoiceLine.ConsentingProcessCollection);
				AssertAnyInDatabase("There are no DrawbackImportLicenseCollection", false, InvoiceLine.DrawbackImportLicenseCollection);
				AssertEquals("There are no AdditionalTariffs", false, InvoiceLine.AdditionalTariffs.Count > 0);
				AssertEquals("There are no SpecialCaseTaxes", false, InvoiceLine.SpecialCaseTaxes.Count > 0);
				AssertAnyInDatabase("There are no LegalActInfos", false, InvoiceLine.LegalActInfos);
				AssertAnyInDatabase("There are no IPITaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.IPI);
				AssertAnyInDatabase("There are no PISTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.PisCofins);
				AssertAnyInDatabase("There are no DutyTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.Duty);
				AssertAnyInDatabase("There are no ICMSTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.ICMS);
				AssertAnyInDatabase("There are no FMMBenefit", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.FMM);
				AssertAnyInDatabase("There are no Taxes", false, InvoiceLine.Taxes);
				AssertAnyInDatabase("There are no importLicense", false, InvoiceLine.ImportLicenseInfos);
			});

			SetDateOnInvoiceLine(BRJobMessageTypeList.Codes.Export);
			CombineAssertions(Declaration.JE_MessageType, () =>
			{
				AssertAnyInDatabase("There are CertificateOfOriginCollection", true, InvoiceLine.CertificateOfOriginCollection);
				AssertAnyInDatabase("There are ComplementaryLogisticInvoiceCollection", true, InvoiceLine.ComplementaryLogisticInvoiceCollection);
				AssertAnyInDatabase("There are ElectronicLogisticInvoiceCollection", true, InvoiceLine.ElectronicLogisticInvoiceCollection);
				AssertAnyInDatabase("There are ReferenceInvoiceManualCollection", true, InvoiceLine.ReferenceInvoiceManualCollection);
				AssertAnyInDatabase("There are SuspensionDrawbackCollection", true, InvoiceLine.SuspensionDrawbackCollection);
				AssertAnyInDatabase("There are LPCOJobComInvLineRefsCollection", true, InvoiceLine.LPCOJobComInvLineRefsCollection);
				AssertAnyInDatabase("There are no MercosulForeignDeclarations", false, InvoiceLine.MercosulForeignDeclarations);
				AssertAnyInDatabase("There are PreviousDocumentCollection", true, InvoiceLine.PreviousDocuments);
				AssertAnyInDatabase("There are no TariffDetachCollection", false, InvoiceLine.TariffDetachs);
				AssertAnyInDatabase("There are no ConsentingProcessCollection", false, InvoiceLine.ConsentingProcessCollection);
				AssertAnyInDatabase("There are no DrawbackImportLicenseCollection", false, InvoiceLine.DrawbackImportLicenseCollection);
				AssertEquals("There are no AdditionalTariffs", false, InvoiceLine.AdditionalTariffs.Count > 0);
				AssertEquals("There are no SpecialCaseTaxes", false, InvoiceLine.SpecialCaseTaxes.Count > 0);
				AssertAnyInDatabase("There are no LegalActInfos", false, InvoiceLine.LegalActInfos);
				AssertAnyInDatabase("There are no IPITaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.IPI);
				AssertAnyInDatabase("There are no PISTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.PisCofins);
				AssertAnyInDatabase("There are no DutyTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.Duty);
				AssertAnyInDatabase("There are no ICMSTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.ICMS);
				AssertAnyInDatabase("There are no FMMBenefit", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.FMM);
				AssertAnyInDatabase("There are no Taxes", false, InvoiceLine.Taxes);
				AssertAnyInDatabase("There are no ImportLicense", false, InvoiceLine.ImportLicenseInfos);
			});

			SetDateOnInvoiceLine(BRJobMessageTypeList.Codes.ImportLicense);
			CombineAssertions(Declaration.JE_MessageType, () =>
			{
				AssertAnyInDatabase("There are no CertificateOfOriginCollection", false, InvoiceLine.CertificateOfOriginCollection);
				AssertAnyInDatabase("There are no ComplementaryLogisticInvoiceCollection", false, InvoiceLine.ComplementaryLogisticInvoiceCollection);
				AssertAnyInDatabase("There are no ElectronicLogisticInvoiceCollection", false, InvoiceLine.ElectronicLogisticInvoiceCollection);
				AssertAnyInDatabase("There are no ReferenceInvoiceManualCollection", false, InvoiceLine.ReferenceInvoiceManualCollection);
				AssertAnyInDatabase("There are no SuspensionDrawbackCollection", false, InvoiceLine.SuspensionDrawbackCollection);
				AssertAnyInDatabase("There are no LPCOJobComInvLineRefsCollection", false, InvoiceLine.LPCOJobComInvLineRefsCollection);
				AssertAnyInDatabase("There are no MercosulForeignDeclarations", false, InvoiceLine.MercosulForeignDeclarations);
				AssertAnyInDatabase("There are no PreviousDocumentCollection", false, InvoiceLine.PreviousDocuments);
				AssertAnyInDatabase("There are TariffDetachCollection", true, InvoiceLine.TariffDetachs);
				AssertAnyInDatabase("There are ConsentingProcessCollection", true, InvoiceLine.ConsentingProcessCollection);
				AssertAnyInDatabase("There are DrawbackImportLicenseCollection", true, InvoiceLine.DrawbackImportLicenseCollection);
				AssertEquals("There are no AdditionalTariffs", false, InvoiceLine.AdditionalTariffs.Count > 0);
				AssertEquals("There are no SpecialCaseTaxes", false, InvoiceLine.SpecialCaseTaxes.Count > 0);
				AssertAnyInDatabase("There are no LegalActInfos", false, InvoiceLine.LegalActInfos);
				AssertAnyInDatabase("There are no IPITaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.IPI);
				AssertAnyInDatabase("There are no PISTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.PisCofins);
				AssertAnyInDatabase("There are DutyTaxRegime", true, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.Duty);
				AssertAnyInDatabase("There are no ICMSTaxRegime", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.ICMS);
				AssertAnyInDatabase("There are no FMMBenefit", false, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.FMM);
				AssertAnyInDatabase("There are no Taxes", false, InvoiceLine.Taxes);
				AssertAnyInDatabase("There are no ImportLicense", false, InvoiceLine.ImportLicenseInfos);
			});

			SetDateOnInvoiceLine(BRJobMessageTypeList.Codes.ImportSiscomex);
			CombineAssertions(Declaration.JE_MessageType, () =>
			{
				AssertAnyInDatabase("There are no CertificateOfOriginCollection", false, InvoiceLine.CertificateOfOriginCollection);
				AssertAnyInDatabase("There are no ComplementaryLogisticInvoiceCollection", false, InvoiceLine.ComplementaryLogisticInvoiceCollection);
				AssertAnyInDatabase("There are no ElectronicLogisticInvoiceCollection", false, InvoiceLine.ElectronicLogisticInvoiceCollection);
				AssertAnyInDatabase("There are no ReferenceInvoiceManualCollection", false, InvoiceLine.ReferenceInvoiceManualCollection);
				AssertAnyInDatabase("There are no SuspensionDrawbackCollection", false, InvoiceLine.SuspensionDrawbackCollection);
				AssertAnyInDatabase("There are no LPCOJobComInvLineRefsCollection", false, InvoiceLine.LPCOJobComInvLineRefsCollection);
				AssertAnyInDatabase("There are MercosulForeignDeclarations", true, InvoiceLine.MercosulForeignDeclarations);
				AssertAnyInDatabase("There are PreviousDocumentCollection", true, InvoiceLine.PreviousDocuments);
				AssertAnyInDatabase("There are TariffDetachCollection", true, InvoiceLine.TariffDetachs);
				AssertAnyInDatabase("There are no ConsentingProcessCollection", false, InvoiceLine.ConsentingProcessCollection);
				AssertAnyInDatabase("There are no DrawbackImportLicenseCollection", false, InvoiceLine.DrawbackImportLicenseCollection);
				AssertEquals("There are AdditionalTariffs", true, InvoiceLine.AdditionalTariffs.Count > 0);
				AssertEquals("There are SpecialCaseTaxes", true, InvoiceLine.SpecialCaseTaxes.Count > 0);
				AssertAnyInDatabase("There are LegalActInfos", true, InvoiceLine.LegalActInfos);
				AssertAnyInDatabase("There are IPITaxRegime", true, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.IPI);
				AssertAnyInDatabase("There are PISTaxRegime", true, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.PisCofins);
				AssertAnyInDatabase("There are DutyTaxRegime", true, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.Duty);
				AssertAnyInDatabase("There are ICMSTaxRegime", true, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.ICMS);
				AssertAnyInDatabase("There are FMMBenefit", true, InvoiceLine.TaxRegimeCollection, x => x.CSI_SubType == TaxRegimeTypeList.Codes.FMM);
				AssertAnyInDatabase("There are Taxes", true, InvoiceLine.Taxes);
				AssertAnyInDatabase("There are importLicense", true, InvoiceLine.ImportLicenseInfos);
			});
		}

		public void TestGoodsConditionFieldsOnFactorySaving()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			InvoiceLine.JI_UsedMaterialOperationType = GoodsConditionOperationTypeList.Codes.ExTariff;
			InvoiceLine.JI_UsedMaterialSerialNumber = "XXXX";
			InvoiceLine.JI_UsedMaterialManufactureYear = "2022";
			InvoiceLine.JI_Model = "XXX";
			InvoiceLine.JI_BrandName = "XXXXXX";
			Factory.Save();
			AssertEquals("JI_UsedMaterialRegime should be", UsedMaterialRegimeList.Codes.Nationalization, InvoiceLine.JI_UsedMaterialRegime);
			AssertEquals("JI_UsedMaterialOperationType should be", GoodsConditionOperationTypeList.Codes.ExTariff, InvoiceLine.JI_UsedMaterialOperationType);
			AssertEquals("JI_UsedMaterialSerialNumber should be", "XXXX", InvoiceLine.JI_UsedMaterialSerialNumber);
			AssertEquals("JI_UsedMaterialManufactureYear should be", "2022", InvoiceLine.JI_UsedMaterialManufactureYear);
			AssertEquals("JI_Model should be", "XXX", InvoiceLine.JI_Model);
			AssertEquals("JI_BrandName should be", "XXXXXX", InvoiceLine.JI_BrandName);

			AssertEquals("JI_UsedMaterialOperationType.ReadOnly", false, InvoiceLine.JI_UsedMaterialOperationTypeInfo.ReadOnly);
			AssertEquals("JI_UsedMaterialSerialNumber.ReadOnly", false, InvoiceLine.JI_UsedMaterialSerialNumberInfo.ReadOnly);
			AssertEquals("JI_UsedMaterialManufactureYear.ReadOnly", false, InvoiceLine.JI_UsedMaterialManufactureYearInfo.ReadOnly);
			AssertEquals("JI_Model.ReadOnly", false, InvoiceLine.JI_ModelInfo.ReadOnly);
			AssertEquals("JI_BrandName.ReadOnly", false, InvoiceLine.JI_BrandNameInfo.ReadOnly);

			InvoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.TemporaryAdmission;
			AssertEquals("JI_UsedMaterialRegime should be", UsedMaterialRegimeList.Codes.TemporaryAdmission, InvoiceLine.JI_UsedMaterialRegime);
			AssertEquals("JI_UsedMaterialOperationType should be", ZString.Empty, InvoiceLine.JI_UsedMaterialOperationType);
			AssertEquals("JI_UsedMaterialSerialNumber should be", ZString.Empty, InvoiceLine.JI_UsedMaterialSerialNumber);
			AssertEquals("JI_UsedMaterialManufactureYear should be", ZString.Empty, InvoiceLine.JI_UsedMaterialManufactureYear);
			AssertEquals("JI_Model should be", ZString.Empty, InvoiceLine.JI_Model);
			AssertEquals("JI_BrandName should be", ZString.Empty, InvoiceLine.JI_BrandName);

			AssertEquals("JI_UsedMaterialOperationType.ReadOnly", true, InvoiceLine.JI_UsedMaterialOperationTypeInfo.ReadOnly);
			AssertEquals("JI_UsedMaterialSerialNumber.ReadOnly", true, InvoiceLine.JI_UsedMaterialSerialNumberInfo.ReadOnly);
			AssertEquals("JI_UsedMaterialManufactureYear.ReadOnly", true, InvoiceLine.JI_UsedMaterialManufactureYearInfo.ReadOnly);
			AssertEquals("JI_Model.ReadOnly", true, InvoiceLine.JI_ModelInfo.ReadOnly);
			AssertEquals("JI_BrandName.ReadOnly", true, InvoiceLine.JI_BrandNameInfo.ReadOnly);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertEquals("JI_UsedMaterialRegime should be", ZString.Empty, InvoiceLine.JI_UsedMaterialRegime);
			AssertEquals("JI_UsedMaterialOperationType should be", ZString.Empty, InvoiceLine.JI_UsedMaterialOperationType);
			AssertEquals("JI_UsedMaterialSerialNumber should be", ZString.Empty, InvoiceLine.JI_UsedMaterialSerialNumber);
			AssertEquals("JI_UsedMaterialManufactureYear should be", ZString.Empty, InvoiceLine.JI_UsedMaterialManufactureYear);
			AssertEquals("JI_Model should be", ZString.Empty, InvoiceLine.JI_Model);
			AssertEquals("JI_BrandName should be", ZString.Empty, InvoiceLine.JI_BrandName);
		}

		public void TestFOBValueForApportionment()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_IncoTerm = "TET";
			InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			InvoiceHeader.JZ_NetWeight = 200;

			InvoiceLine.JI_LinePrice = 700m;
			InvoiceLine.JI_NetWeight = 100;

			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 300m;
			invoiceLine2.JI_NetWeight = 100;

			var charge1 = InvoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			charge1.J7_IsDutiable = true;
			charge1.J7_IsIncludedInITOT = true;
			charge1.J7_Amount = 30.00m;

			var charge2 = invoiceLine2.Charges.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			charge2.J7_IsDutiable = true;
			charge2.J7_IsIncludedInITOT = true;
			charge2.J7_Amount = 40.00m;

			var charge3 = invoiceLine2.Charges.AddNew();
			charge3.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge3.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			charge3.J7_IsDutiable = false;
			charge3.J7_IsIncludedInITOT = false;
			charge3.J7_Amount = 10.00m;

			Declaration.ResumeApportionment();
			AssertEquals(670m, InvoiceLine.FOBValueForApportionment);
			AssertEquals(260m, invoiceLine2.FOBValueForApportionment);
		}

		public void TestDistributeByFOB()
		{
			var currencyCode = Core.Constants.CurrencyCodes.Brazil;
			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_InvoiceAmount = 270968.65m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_NetWeight = 23611.00m;
			invoiceHeader.JZ_Weight = 23836.00m;

			invoiceHeader.GroupHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.Charges.RemoveAndDeleteAll();

			AddNewCharge(invoiceHeader, CustomsChargeTypeList.Codes.OverseasFreight, currencyCode, ChargeDistributeByList.Codes.NetWeight, 7901.73m, true, false);
			AddNewCharge(invoiceHeader, CustomsChargeTypeList.Codes.OverseasInsurance, currencyCode, ChargeDistributeByList.Codes.FOB, 954.31m, false, false);
			AddNewCharge(invoiceHeader, CustomsChargeTypeList.Codes.OtherCharges, currencyCode, ChargeDistributeByList.Codes.NetWeight, 2265.35m, false, true);
			AddNewCharge(invoiceHeader, CustomsChargeTypeList.Codes.Discount, currencyCode, ChargeDistributeByList.Codes.NetWeight, 10m, false, true);

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 131004.91m;
			invoiceLine1.JI_Weight = 10898.88m;
			invoiceLine1.JI_NetWeight = 10796m;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 139963.74m;
			invoiceLine2.JI_Weight = 12937.12m;
			invoiceLine2.JI_NetWeight = 12815m;

			Declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertEquals("Charges on Invoice Line 1", 4, invoiceLine1.ApportionedCharges.Count);
				AssertEquals("OFT on Invoice Line 1", 3613.02m, invoiceLine1.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OverseasFreight).J7_Amount);
				AssertEquals("ONS on Invoice Line 1", 461.91m, invoiceLine1.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OverseasInsurance).J7_Amount);
				AssertEquals("OTH on Invoice Line 1", 1035.82m, invoiceLine1.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OtherCharges).J7_Amount);
				AssertEquals("DIS on Invoice Line 1", 4.57m, invoiceLine1.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.Discount).J7_Amount);

				AssertEquals("Charges on Invoice Line 2", 4, invoiceLine2.ApportionedCharges.Count);
				AssertEquals("OFT on Invoice Line 2", 4288.71m, invoiceLine2.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OverseasFreight).J7_Amount);
				AssertEquals("ONS on Invoice Line 2", 492.40m, invoiceLine2.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OverseasInsurance).J7_Amount);
				AssertEquals("OTH on Invoice Line 2", 1229.53m, invoiceLine2.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OtherCharges).J7_Amount);
				AssertEquals("DIS on Invoice Line 2", 5.43m, invoiceLine2.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.Discount).J7_Amount);
			});
		}

		void AddNewCharge(JobComInvoiceHeader invoice, string chargeType, string chargeCurrency, string distributeBy, ZDecimal chargeAmount, bool isIncludedInITOT, bool isDutiable)
		{
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = chargeType;
			charge.J7_RX_NKCurrency = chargeCurrency;
			charge.J7_DistributeBy = distributeBy;
			charge.J7_Amount = chargeAmount;
			charge.J7_IsIncludedInITOT = isIncludedInITOT;
			charge.J7_IsDutiable = isDutiable;
		}

		public void TestDrawbackFieldsForImportLicense()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			InvoiceLine.DrawbackCANumber = "XXXXX-YY";
			InvoiceLine.DrawbackItemNumber = 1;

			AssertEquals("CSI_Code should be", DrawbackModalityList.Codes.GenericSuspension, InvoiceLine.DrawbackImportLicense.CSI_Code);
			AssertEquals("CSI_ReferenceNumber should be", "XXXXX-YY", InvoiceLine.DrawbackImportLicense.CSI_ReferenceNumber);
			AssertEquals("CSI_ItemNumber should be", (ZShort)1, InvoiceLine.DrawbackImportLicense.CSI_ItemNumber);
		}

		public void TestDrawbackCANumber_ReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			AssertEquals("DrawbackCANumberInfo should be", false, InvoiceLine.DrawbackCANumberInfo.ReadOnly);

			InvoiceLine.DrawbackModality = DrawbackModalityList.Codes.NoDrawback;
			AssertEquals("DrawbackCANumberInfo should be", true, InvoiceLine.DrawbackCANumberInfo.ReadOnly);
		}

		public void TestDrawbackItemNumber_ReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			AssertEquals("DrawbackItemNumberInfo should be", false, InvoiceLine.DrawbackItemNumberInfo.ReadOnly);

			InvoiceLine.DrawbackModality = DrawbackModalityList.Codes.NoDrawback;
			AssertEquals("DrawbackItemNumberInfo should be", true, InvoiceLine.DrawbackItemNumberInfo.ReadOnly);

			InvoiceLine.DrawbackModality = DrawbackModalityList.Codes.ExemptionPaper;
			AssertEquals("DrawbackItemNumberInfo should be", true, InvoiceLine.DrawbackItemNumberInfo.ReadOnly);
		}

		public void TestGetTariffDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "123", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "123");
			Factory.Save();

			AssertEquals("TariffDescription should be equal", ZString.Empty, InvoiceLine.TariffDescription);

			InvoiceLine.JI_Tariff = "123";
			AssertEquals("TariffDescription should be equal", "123", InvoiceLine.TariffDescription);
		}

		public void TestJI_CustomsUnitQty()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			anotherFactory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			helper.CreateTariffUOM(tariff1, "CU1", "KG", Core.Constants.CountryCodes.Brazil);

			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals("JI_CustomsUnitQty should be equal", ZString.Empty, InvoiceLine.JI_CustomsUnitQty);

			InvoiceLine.JI_Tariff = "56049000";
			AssertEquals("JI_CustomsUnitQty should be equal", "KG", InvoiceLine.JI_CustomsUnitQty);
		}

		public void TestMercosulForeignDeclarationType()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.MercosulForeignDeclarationType = ZString.Empty;
			AssertEquals("MercosulForeignDeclaration should NOT be created", 0, InvoiceLine.MercosulForeignDeclarations.Count);

			InvoiceLine.MercosulForeignDeclarationType = CertificateTypeList.Codes.CCPTC;
			AssertEquals("A new MercosulForeignDeclaration should be created", 1, InvoiceLine.MercosulForeignDeclarations.Count);
			AssertEquals("MercosulForeignDeclarationType count should be", CertificateTypeList.Codes.CCPTC, InvoiceLine.MercosulForeignDeclarationType);

			var merc = InvoiceLine.MercosulForeignDeclarations.AddNew();
			merc.CSI_SubType = CertificateTypeList.Codes.CCROM;

			AssertEquals("MercosulForeignDeclarationType count should be", ZString.Empty, InvoiceLine.MercosulForeignDeclarationType);

			merc.CSI_SubType = CertificateTypeList.Codes.CCPTC;
			AssertEquals("MercosulForeignDeclarationType count should be", CertificateTypeList.Codes.CCPTC, InvoiceLine.MercosulForeignDeclarationType);

			InvoiceLine.MercosulForeignDeclarationType = CertificateTypeList.Codes.CCROM;

			foreach (var mercosulForeignDeclaration in InvoiceLine.MercosulForeignDeclarations.Cast<MercosulForeignDeclaration>())
			{
				AssertEquals("CSI_SubType should be updated", CertificateTypeList.Codes.CCROM, mercosulForeignDeclaration.CSI_SubType);
			}
		}

		public void TestIPIVigentRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 50m, rateType: Constants.RateTypes.IPI, rateCode: Constants.RateCodes.IPI);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, Constants.RateTypes.IPI);
			var rateCodeIpi = helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateCodes.IPI, rateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup("BR", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, rateCodeIpi.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.5", null, "50", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			Factory.Save();

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "99999999";
			AssertEquals("IPIVigentRateValue should be", 0m, InvoiceLine.IPIVigentRateValue);

			Assert("IPIVigentRateValue should be Empty", InvoiceLine.IPIVigentRateValue.IsEmpty);
			Assert("IPIVigentRateValue should be ReadOnly", InvoiceLine.IPIVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 1038", InvoiceLine.Taxes.FindByType(Constants.RateCodes.IPI));

			InvoiceLine.JI_CountryOfOrigin = "CA";
			AssertEquals("IPIVigentRateValue should be", 50m, InvoiceLine.IPIVigentRateValue);
			Assert("IPIVigentRateValue should be ReadOnly", InvoiceLine.IPIVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 1038", InvoiceLine.Taxes.FindByType(Constants.RateCodes.IPI));

			InvoiceLine.IPIRateIsOverridden = true;
			var invoiceLineTax = InvoiceLine.Taxes.FindByType(Constants.RateCodes.IPI);
			AssertEquals("IPIVigentRateValue should be", 50m, InvoiceLine.IPIVigentRateValue);
			Assert("CofinsVigentRateValue should NOT be ReadOnly", !InvoiceLine.IPIVigentRateValueInfo.ReadOnly);
			AssertNotNull("Taxes must contain object with type equal to 1038", invoiceLineTax);

			InvoiceLine.IPIVigentRateValue = 10m;
			AssertEquals("IPIVigentRateValue must be equal to 10", 10m, InvoiceLine.IPIVigentRateValue);
			AssertEquals("JLT_Rate must be equal to 10", 10m, invoiceLineTax.JLT_Rate);

			InvoiceLine.IPIVigentRateValue = 25m;
			AssertEquals("JLT_Rate must be equal to 25", 25m, invoiceLineTax.JLT_Rate);
			AssertEquals("Changed IPI Rate and not create another JobComInvoiceLineTax", 1, InvoiceLine.Taxes.Count);

			InvoiceLine.IPIRateIsOverridden = false;
			AssertEquals("IPIVigentRateValue count should be", 50m, InvoiceLine.IPIVigentRateValue);
			Assert("IPIVigentRateValue should be ReadOnly", InvoiceLine.IPIVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 1038", InvoiceLine.Taxes.FindByType(Constants.RateCodes.IPI));
			Assert("IPI JobComInvoiceLineTax should be deleted", invoiceLineTax.IsDeleted);
		}

		public void TestDefaultExIPITariff()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("There are no AdditionalTariffs", false, InvoiceLine.AdditionalTariffs.Any());

			InvoiceLine.IPIRateIsOverridden = true;
			AssertEquals("One AdditionalTariff added", 1, InvoiceLine.AdditionalTariffs.Count);
			var additionalTariff = InvoiceLine.AdditionalTariffs[0];
			AssertEquals("Legal Act (Subject)", AdditionalTaxTypeList.Codes.ExIPITariff, additionalTariff.LegalActSubject);
			AssertEquals("TariffType", ChildTariffTypeList.Codes.IPI, additionalTariff.TariffType);

			InvoiceLine.IPIRateIsOverridden = false;
			InvoiceLine.IPIRateIsOverridden = true;
			AssertEquals("No AdditionalTariff added", 1, InvoiceLine.AdditionalTariffs.Count);
		}

		public void TestPISVigentRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 8.65m, rateType: Constants.RateTypes.PIS, rateCode: Constants.RateCodes.PIS);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, Constants.RateTypes.PIS, "Social Integration Program");
			var rateCodeDuty = helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateCodes.PIS, rateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup("BR", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, rateCodeDuty.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.865", null, "8.65", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "99999999";
			Assert("PisVigentRateValue should be Empty", InvoiceLine.PisVigentRateValue.IsEmpty);
			Assert("PisVigentRateValue should be ReadOnly", InvoiceLine.PisVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 5602", InvoiceLine.Taxes.FindByType(Constants.RateCodes.PIS));

			InvoiceLine.JI_CountryOfOrigin = "CA";
			AssertEquals("PisVigentRateValue should be", 8.65m, InvoiceLine.PisVigentRateValue);
			Assert("PisVigentRateValue should be ReadOnly", InvoiceLine.PisVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 5602", InvoiceLine.Taxes.FindByType(Constants.RateCodes.PIS));

			InvoiceLine.PisRateIsOverridden = true;
			AssertEquals("PisVigentRateValue should be", 8.65m, InvoiceLine.PisVigentRateValue);
			Assert("PisVigentRateValue should NOT be ReadOnly", !InvoiceLine.PisVigentRateValueInfo.ReadOnly);
			AssertNotNull("Taxes must contain object with type equal to 5602", InvoiceLine.Taxes.FindByType(Constants.RateCodes.PIS));

			InvoiceLine.PisVigentRateValue = 10m;
			AssertEquals("PisVigentRateValue must be equal to 10", 10m, InvoiceLine.PisVigentRateValue);
			AssertEquals("JLT_Rate must be equal to 10", 10m, InvoiceLine.Taxes.FindByType(Constants.RateCodes.PIS).JLT_Rate);

			InvoiceLine.PisRateIsOverridden = false;
			AssertEquals("PisVigentRateValue count should be", 8.65m, InvoiceLine.PisVigentRateValue);
			Assert("PisVigentRateValue should be ReadOnly", InvoiceLine.PisVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 5602", InvoiceLine.Taxes.FindByType(Constants.RateCodes.PIS));
		}

		public void TestCofinsVigentRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 1.5m, rateType: Constants.RateTypes.Cofins, rateCode: Constants.RateCodes.Cofins);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, Constants.RateTypes.Cofins, "Social Security Financing Contribution");
			var rateCodeDuty = helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateCodes.Cofins, rateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup("BR", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, rateCodeDuty.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.15", null, "1.5", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "99999999";
			Assert("CofinsVigentRateValue should be Empty", InvoiceLine.CofinsVigentRateValue.IsEmpty);
			Assert("CofinsVigentRateValue should be ReadOnly", InvoiceLine.CofinsVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 5629", InvoiceLine.Taxes.FindByType(Constants.RateCodes.Cofins));

			InvoiceLine.JI_CountryOfOrigin = "CA";
			AssertEquals("CofinsVigentRateValue should be", 1.5m, InvoiceLine.CofinsVigentRateValue);
			Assert("CofinsVigentRateValue should be ReadOnly", InvoiceLine.CofinsVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 5629", InvoiceLine.Taxes.FindByType(Constants.RateCodes.Cofins));

			InvoiceLine.CofinsRateIsOverridden = true;
			AssertEquals("CofinsVigentRateValue should be", 1.5m, InvoiceLine.CofinsVigentRateValue);
			Assert("CofinsVigentRateValue should NOT be ReadOnly", !InvoiceLine.CofinsVigentRateValueInfo.ReadOnly);
			AssertNotNull("Taxes must contain object with type equal to 5629", InvoiceLine.Taxes.FindByType(Constants.RateCodes.Cofins));

			InvoiceLine.CofinsVigentRateValue = 10m;
			AssertEquals("CofinsVigentRateValue must be equal to 10", 10m, InvoiceLine.CofinsVigentRateValue);
			AssertEquals("JLT_Rate must be equal to 10", 10m, InvoiceLine.Taxes.FindByType(Constants.RateCodes.Cofins).JLT_Rate);

			InvoiceLine.CofinsRateIsOverridden = false;
			AssertEquals("CofinsVigentRateValue count should be", 1.5m, InvoiceLine.CofinsVigentRateValue);
			Assert("CofinsVigentRateValue should be ReadOnly", InvoiceLine.CofinsVigentRateValueInfo.ReadOnly);
			AssertNull("Taxes must NOT contain object with type equal to 5629", InvoiceLine.Taxes.FindByType(Constants.RateCodes.Cofins));
		}

		public void TestChargeTypesConcatenated()
		{
			InvoiceLine.Charges.AddNew();
			InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			InvoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.Discount);

			AssertEquals("ChargeTypesConcatenated should DIS,OFT,ONS", "DIS,OFT,ONS", InvoiceLine.ChargeTypesConcatenated);
		}

		public void TestPisCofinsTaxRegimeAndPisCofinsLegalBase()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Declaration.JE_MessageSubType = ZString.Empty;

			InvoiceLine.DutyLegalBase = "";
			AssertEquals("DutyLegalBase Should be empty", ZString.Empty, InvoiceLine.DutyLegalBase);

			InvoiceLine.DutyLegalBase = "01";
			AssertEquals("DutyLegalBase Should be 01", "01", InvoiceLine.DutyLegalBase);

			InvoiceLine.PisCofinsTaxRegime = "";
			AssertEquals("PisCofinsTaxRegime Should be empty", ZString.Empty, InvoiceLine.PisCofinsTaxRegime);

			InvoiceLine.PisCofinsTaxRegime = "1";
			AssertEquals("PisCofinsTaxRegime  Should be 1", "1", InvoiceLine.PisCofinsTaxRegime);
		}

		public void TestDutyTaxRegimeAndLegalBase()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.Duty);
			CombineAssertions(() =>
			{
				Assert("DutyTaxRegime must be empty", InvoiceLine.DutyTaxRegime.IsEmpty);
				Assert("DutyLegalBase must be empty", InvoiceLine.DutyLegalBase.IsEmpty);
				AssertNull("TaxRegime CusSupportingInfo must be empty", taxRegime);
			});

			InvoiceLine.DutyTaxRegime = "2";
			InvoiceLine.DutyLegalBase = "05";

			taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.Duty);
			CombineAssertions(() =>
			{
				AssertEquals("DutyTaxRegime must be equal to 2", "2", InvoiceLine.DutyTaxRegime);
				AssertEquals("DutyLegalBase must be equal to 05", "05", InvoiceLine.DutyLegalBase);
				AssertEquals("CSI_Type must be equal to TXR", CusSupportingInfoTypeList.Codes.TaxRegime, taxRegime.CSI_Type);
				AssertEquals("CSI_SubType must be equal to 1", TaxRegimeTypeList.Codes.Duty, taxRegime.CSI_SubType);
				AssertEquals("CSI_Code must be equal to 2", "2", taxRegime.CSI_Code);
				AssertEquals("CSI_Procedure must be equal to 05", "05", taxRegime.CSI_Procedure);
			});

			taxRegime.CSI_Code = "3";
			taxRegime.CSI_Procedure = "06";
			CombineAssertions(() =>
			{
				AssertEquals("DutyTaxRegime must be equal to 3", "3", InvoiceLine.DutyTaxRegime);
				AssertEquals("DutyLegalBase must be equal to 06", "06", InvoiceLine.DutyLegalBase);
			});
		}

		public void TestUpdateJI_Procedure()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			InvoiceLine.DutyTaxRegime = "1";
			InvoiceLine.DutyLegalBase = "01";
			AssertEquals("JI_Procedure Should be 011", "01101", InvoiceLine.JI_Procedure);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			InvoiceLine.DutyTaxRegime = "1";
			InvoiceLine.DutyLegalBase = "01";
			Assert("JI_Procedure Should be empty", InvoiceLine.JI_Procedure.IsEmpty);
		}

		public void TestIPITaxRegime()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.IPI);
			CombineAssertions(() =>
			{
				Assert("IPITaxRegime must be empty", InvoiceLine.IPITaxRegime.IsEmpty);
				AssertNull("TaxRegime CusSupportingInfo must be empty", taxRegime);
			});

			InvoiceLine.IPITaxRegime = "2";

			taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.IPI);

			CombineAssertions(() =>
			{
				AssertEquals("IPITaxRegime must be equal to 2", "2", InvoiceLine.IPITaxRegime);
				AssertEquals("CSI_Type must be equal to TXR", CusSupportingInfoTypeList.Codes.TaxRegime, taxRegime.CSI_Type);
				AssertEquals("CSI_SubType must be equal to 1", TaxRegimeTypeList.Codes.IPI, taxRegime.CSI_SubType);
				AssertEquals("CSI_Code must be equal to 2", "2", taxRegime.CSI_Code);
			});

			taxRegime.CSI_Code = "3";
			CombineAssertions(() =>
			{
				AssertEquals("IPITaxRegime must be equal to 3", "3", InvoiceLine.IPITaxRegime);
			});
		}

		public void TestPisCofinsTaxRegimeAndLegalBase()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.PisCofins);
			CombineAssertions(() =>
			{
				Assert("PisCofinsTaxRegime must be empty", InvoiceLine.PisCofinsTaxRegime.IsEmpty);
				Assert("PisCofinsLegalBase must be empty", InvoiceLine.PisCofinsLegalBase.IsEmpty);
				AssertNull("TaxRegime CusSupportingInfo must be empty", taxRegime);
			});

			InvoiceLine.PisCofinsTaxRegime = "2";
			InvoiceLine.PisCofinsLegalBase = "05";

			taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.PisCofins);
			CombineAssertions(() =>
			{
				AssertEquals("PisCofinsTaxRegime must be equal to 2", "2", InvoiceLine.PisCofinsTaxRegime);
				AssertEquals("PisCofinsLegalBase must be equal to 05", "05", InvoiceLine.PisCofinsLegalBase);
				AssertEquals("CSI_Type must be equal to TXR", CusSupportingInfoTypeList.Codes.TaxRegime, taxRegime.CSI_Type);
				AssertEquals("CSI_SubType must be equal to 1", TaxRegimeTypeList.Codes.PisCofins, taxRegime.CSI_SubType);
				AssertEquals("CSI_Code must be equal to 2", "2", taxRegime.CSI_Code);
				AssertEquals("CSI_Procedure must be equal to 05", "05", taxRegime.CSI_Procedure);
			});

			taxRegime.CSI_Code = "3";
			taxRegime.CSI_Procedure = "06";
			CombineAssertions(() =>
			{
				AssertEquals("PisCofinsTaxRegime must be equal to 3", "3", InvoiceLine.PisCofinsTaxRegime);
				AssertEquals("PisCofinsLegalBase must be equal to 06", "06", InvoiceLine.PisCofinsLegalBase);
			});
		}

		public void TestIPIReadOnlyOnDutyTaxRegimeChanged()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertIPITaxRegime(TaxRegimeList.Codes.FullCollection, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.Immunity, true);
			AssertIPITaxRegime(TaxRegimeList.Codes.Exemption, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.Reduction, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.Suspension, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.NoIncident, true);
			AssertIPITaxRegime(TaxRegimeList.Codes.PaymentMade, false);
			AssertIPITaxRegime(string.Empty, false);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertIPITaxRegime(TaxRegimeList.Codes.FullCollection, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.Immunity, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.Exemption, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.Reduction, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.Suspension, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.NoIncident, false);
			AssertIPITaxRegime(TaxRegimeList.Codes.PaymentMade, false);
			AssertIPITaxRegime(string.Empty, false);

			void AssertIPITaxRegime(string taxRegime, bool readOnly)
			{
				InvoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
				InvoiceLine.IPIRateIsOverridden = true;
				InvoiceLine.IPIVigentRateValue = 10m;
				InvoiceLine.DutyTaxRegime = taxRegime;
				CombineAssertions($"job is {Declaration.JE_MessageType} and DutyTaxRegime is {taxRegime}", () =>
				{
					AssertEquals("IPITaxRegime", readOnly ? IPITaxRegimeList.Codes.NonTaxable : IPITaxRegimeList.Codes.FullCollection, InvoiceLine.IPITaxRegime);
					AssertEquals("IPITaxRegime is read only", readOnly, InvoiceLine.IPITaxRegimeInfo.ReadOnly);
					AssertEquals("IPIRateIsOverridden is read only", readOnly, InvoiceLine.IPIRateIsOverriddenInfo.ReadOnly);
					AssertEquals("IPIRateIsOverridden", !readOnly, InvoiceLine.IPIRateIsOverridden);
					AssertEquals("IPIVigentRateValue", readOnly ? 0m : 10m, InvoiceLine.IPIVigentRateValue);
				});
			}
		}

		public void TestDutyLegalBaseReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertDutyLegalBase(TaxRegimeList.Codes.FullCollection, true);
			AssertDutyLegalBase(TaxRegimeList.Codes.PaymentMade, true);
			AssertDutyLegalBase(TaxRegimeList.Codes.Reduction, false);
			AssertDutyLegalBase(string.Empty, false);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertDutyLegalBase(TaxRegimeList.Codes.FullCollection, false);
			AssertDutyLegalBase(TaxRegimeList.Codes.PaymentMade, false);

			void AssertDutyLegalBase(string taxRegime, bool readOnly)
			{
				InvoiceLine.DutyLegalBase = "05";
				InvoiceLine.DutyTaxRegime = taxRegime;
				CombineAssertions($"job is {Declaration.JE_MessageType} and DutyTaxRegime is {taxRegime}", () =>
				{
					AssertEquals("DutyLegalBase is read only", readOnly, InvoiceLine.DutyLegalBaseInfo.ReadOnly);
				});
			}
		}

		public void TestDutyLegalBaseClear()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceLine.DutyTaxRegime = "1";
			Assert("DutyLegalBase should be Empty", InvoiceLine.DutyLegalBase.IsEmpty);

			InvoiceLine.DutyLegalBase = "2";
			AssertEquals("DutyLegalBase should be", "2", InvoiceLine.DutyLegalBase);

			InvoiceLine.DutyTaxRegime = "5";
			Assert("DutyLegalBase should be Empty", InvoiceLine.DutyLegalBase.IsEmpty);
		}

		public void TestIPILegalBaseReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertIPITaxBenefitLegalAct(IPITaxRegimeList.Codes.FullCollection, true);
			AssertIPITaxBenefitLegalAct(IPITaxRegimeList.Codes.NonTaxable, true);
			AssertIPITaxBenefitLegalAct(IPITaxRegimeList.Codes.Reduction, false);
			AssertIPITaxBenefitLegalAct(string.Empty, false);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertIPITaxBenefitLegalAct(IPITaxRegimeList.Codes.FullCollection, false);
			AssertIPITaxBenefitLegalAct(IPITaxRegimeList.Codes.NonTaxable, false);

			void AssertIPITaxBenefitLegalAct(string taxRegime, bool readOnly)
			{
				InvoiceLine.IPITaxBenefitLegalActType = "05";
				InvoiceLine.IPITaxBenefitLegalActIssuingBody = "01";
				InvoiceLine.IPITaxBenefitLegalActNumber = "123";
				InvoiceLine.IPITaxBenefitLegalActYear = "2022";
				InvoiceLine.IPITaxRegime = taxRegime;

				CombineAssertions($"job is {Declaration.JE_MessageType} and IPITaxRegime is {taxRegime}", () =>
				{
					AssertEquals("IPITaxBenefitLegalActType is read only", readOnly, InvoiceLine.IPITaxBenefitLegalActTypeInfo.ReadOnly);
					AssertEquals("IPITaxBenefitLegalActIssuingBody is read only", readOnly, InvoiceLine.IPITaxBenefitLegalActIssuingBodyInfo.ReadOnly);
					AssertEquals("IPITaxBenefitLegalActNumber is read only", readOnly, InvoiceLine.IPITaxBenefitLegalActNumberInfo.ReadOnly);
					AssertEquals("IPITaxBenefitLegalActYear is read only", readOnly, InvoiceLine.IPITaxBenefitLegalActYearInfo.ReadOnly);
				});
			}
		}

		public void TestIPIFieldsClear()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceLine.IPITaxRegime = "1";
			CombineAssertions(() =>
			{
				Assert("IPITaxBenefitLegalActType should be Empty", InvoiceLine.IPITaxBenefitLegalActType.IsEmpty);
				Assert("IPITaxBenefitLegalActIssuingBody should be Empty", InvoiceLine.IPITaxBenefitLegalActIssuingBody.IsEmpty);
				Assert("IPITaxBenefitLegalActNumber should be Empty", InvoiceLine.IPITaxBenefitLegalActNumber.IsEmpty);
				Assert("IPITaxBenefitLegalActYear should be Empty", InvoiceLine.IPITaxBenefitLegalActYear.IsEmpty);
			});

			CombineAssertions(() =>
			{
				InvoiceLine.IPITaxBenefitLegalActType = "2";
				AssertEquals("IPITaxBenefitLegalActType should be", "2", InvoiceLine.IPITaxBenefitLegalActType);

				InvoiceLine.IPITaxBenefitLegalActIssuingBody = "5";
				AssertEquals("IPITaxBenefitLegalActIssuingBody should be", "5", InvoiceLine.IPITaxBenefitLegalActIssuingBody);

				InvoiceLine.IPITaxBenefitLegalActNumber = "20";
				AssertEquals("IPITaxBenefitLegalActNumber should be", "20", InvoiceLine.IPITaxBenefitLegalActNumber);

				InvoiceLine.IPITaxBenefitLegalActYear = "2023";
				AssertEquals("IPITaxBenefitLegalActYear should be", "2023", InvoiceLine.IPITaxBenefitLegalActYear);
			});

			InvoiceLine.IPITaxRegime = "5";
			CombineAssertions(() =>
			{
				Assert("IPITaxBenefitLegalActType should be Empty", InvoiceLine.IPITaxBenefitLegalActType.IsEmpty);
				Assert("IPITaxBenefitLegalActIssuingBody should be Empty", InvoiceLine.IPITaxBenefitLegalActIssuingBody.IsEmpty);
				Assert("IPITaxBenefitLegalActNumber should be Empty", InvoiceLine.IPITaxBenefitLegalActNumber.IsEmpty);
				Assert("IPITaxBenefitLegalActYear should be Empty", InvoiceLine.IPITaxBenefitLegalActYear.IsEmpty);
			});
		}

		public void TestJI_ComplementaryNote_ReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertComplementaryNote(IPITaxRegimeList.Codes.FullCollection, false);
			AssertComplementaryNote(IPITaxRegimeList.Codes.NonTaxable, true);
			AssertComplementaryNote(string.Empty, false);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertComplementaryNote(IPITaxRegimeList.Codes.FullCollection, false);
			AssertComplementaryNote(IPITaxRegimeList.Codes.NonTaxable, false);

			void AssertComplementaryNote(string taxRegime, bool readOnly)
			{
				InvoiceLine.JI_ComplementaryNote = "XX";
				InvoiceLine.IPITaxRegime = taxRegime;
				CombineAssertions($"job is {Declaration.JE_MessageType} and IPIDutyTaxRegime is {taxRegime}", () =>
				{
					AssertEquals("JI_ComplementaryNote is read only", readOnly, InvoiceLine.JI_ComplementaryNoteInfo.ReadOnly);
					AssertEquals("JI_ComplementaryNote is cleared", readOnly, InvoiceLine.JI_ComplementaryNote.IsEmpty);
				});
			}
		}

		public void TestPisCofinsRateReadOnlyOnPisCofinsTaxRegimeChanged()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.FullCollection, false, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.Immunity, true, true);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.Exemption, false, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.Reduction, true, true);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.Suspension, true, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.NoIncident, true, true);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.PaymentMade, false, false);
			AssertPisCofinsRateIsOverridden(string.Empty, false, false);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.FullCollection, false, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.Immunity, false, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.Exemption, false, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.Reduction, true, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.Suspension, true, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.NoIncident, false, false);
			AssertPisCofinsRateIsOverridden(TaxRegimeList.Codes.PaymentMade, false, false);
			AssertPisCofinsRateIsOverridden(string.Empty, false, false);

			void AssertPisCofinsRateIsOverridden(string taxRegime, bool readOnly, bool rateIsReadOnly)
			{
				InvoiceLine.PisRateIsOverridden = true;
				InvoiceLine.PisVigentRateValue = 10m;
				InvoiceLine.CofinsRateIsOverridden = true;
				InvoiceLine.CofinsVigentRateValue = 10m;
				InvoiceLine.PisCofinsTaxRegime = taxRegime;

				CombineAssertions($"job is {Declaration.JE_MessageType} and PisCofinsTaxRegime is {taxRegime}", () =>
				{
					AssertEquals("PisRateIsOverriddenInfo is read only", rateIsReadOnly, InvoiceLine.PisRateIsOverriddenInfo.ReadOnly);
					AssertEquals("CofinsRateIsOverriddenInfo is read only", rateIsReadOnly, InvoiceLine.CofinsRateIsOverriddenInfo.ReadOnly);
					AssertEquals("PisRateIsOverridden", !readOnly, InvoiceLine.PisRateIsOverridden);
					AssertEquals("PisVigentRateValue", readOnly ? 0m : 10m, InvoiceLine.PisVigentRateValue);
					AssertEquals("CofinsRateIsOverridden", !readOnly, InvoiceLine.CofinsRateIsOverridden);
					AssertEquals("CofinsVigentRateValue", readOnly ? 0m : 10m, InvoiceLine.CofinsVigentRateValue);
				});
			}
		}

		public void TestPisCofinsLegalBaseReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertPisCofinsLegalBase(TaxRegimeList.Codes.Reduction, false);
			AssertPisCofinsLegalBase(TaxRegimeList.Codes.FullCollection, true);
			AssertPisCofinsLegalBase(TaxRegimeList.Codes.PaymentMade, true);
			AssertPisCofinsLegalBase(string.Empty, false);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertPisCofinsLegalBase(TaxRegimeList.Codes.FullCollection, false);
			AssertPisCofinsLegalBase(TaxRegimeList.Codes.PaymentMade, false);

			void AssertPisCofinsLegalBase(string taxRegime, bool readOnly)
			{
				InvoiceLine.PisCofinsLegalBase = "05";
				InvoiceLine.PisCofinsTaxRegime = taxRegime;
				CombineAssertions($"job is {Declaration.JE_MessageType} and PisCofinsTaxRegime is {taxRegime}", () =>
				{
					AssertEquals("PisCofinsLegalBase is read only", readOnly, InvoiceLine.PisCofinsLegalBaseInfo.ReadOnly);
				});
			}
		}

		public void TestPisCofinsLegalBaseClear()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceLine.PisCofinsTaxRegime = "1";
			Assert("PisCofinsLegalBase should be Empty", InvoiceLine.PisCofinsLegalBase.IsEmpty);

			InvoiceLine.PisCofinsLegalBase = "2";
			AssertEquals("PisCofinsLegalBase should be", "2", InvoiceLine.PisCofinsLegalBase);

			InvoiceLine.PisCofinsTaxRegime = "5";
			Assert("PisCofinsLegalBase should be Empty", InvoiceLine.PisCofinsLegalBase.IsEmpty);
		}

		public void TestOverseasFreightInLocalCurrency()
		{
			var newCurrency = CreateNewCurrency();

			var declaration = ImportJobDeclaration as JobDeclaration;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10.0m, newCurrency.Code);
			invoiceLine.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 3.0m, newCurrency.Code);
			invoiceLine.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 7.0m, newCurrency.Code);

			AssertEquals("OverseasFreightInLocalCurrency", 4.74m, invoiceLine.OverseasFreightInLocalCurrency);
		}

		public void TestOverseasInsuranceInLocalCurrency()
		{
			var newCurrency = CreateNewCurrency();

			var declaration = ImportJobDeclaration as JobDeclaration;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10.0m, newCurrency.Code);
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10.0m, newCurrency.Code);

			AssertEquals("OverseasInsuranceInLocalCurrency", 4.74m, invoiceLine.OverseasInsuranceInLocalCurrency);
		}

		public void TestJI_Calc_EICAmount()
		{
			var newCurrency = CreateNewCurrency();

			var declaration = ImportJobDeclaration as JobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 300m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;
			invoiceHeader.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, 50.0m);

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 100.0m;
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 200.0m;

			line1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, 20.0m);
			declaration.ResumeApportionment();

			AssertEquals("EIC Charge on Invoice", 20.0m, line1.JI_Calc_EICAmount);
			AssertEquals("Apportioned EIC Charge", 30.0m, line2.JI_Calc_EICAmount);
		}

		public void TestJI_ProcedureForExport()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("JI_Procedure must be empty", InvoiceLine.JI_Procedure.IsEmpty);

			InvoiceLine.JI_Procedure = "12345";
			Factory.Save();
			AssertEquals("JI_Procedure must be equal to", "12345", InvoiceLine.JI_Procedure);
		}

		public void TestDefaultAdditionalTariffOnJI_PrimaryPreferenceChanged()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
			AssertEquals("A new Tariff Agreement added", 1, InvoiceLine.AdditionalTariffs.Cast<AdditionalTariff>().Count(x => x.LegalActSubject == AdditionalTaxTypeList.Codes.TariffAgreement));
			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			AssertEquals("A new Ex Duty Tariff added", 1, InvoiceLine.AdditionalTariffs.Cast<AdditionalTariff>().Count(x => x.LegalActSubject == AdditionalTaxTypeList.Codes.ExDutyTariff));

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
			AssertEquals("A new Tariff Agreement added", 1, InvoiceLine.AdditionalTariffs.Cast<AdditionalTariff>().Count(x => x.LegalActSubject == AdditionalTaxTypeList.Codes.TariffAgreement));
			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			AssertEquals("A new Ex Duty Tariff added", 1, InvoiceLine.AdditionalTariffs.Cast<AdditionalTariff>().Count(x => x.LegalActSubject == AdditionalTaxTypeList.Codes.ExDutyTariff));
		}

		public void TestDutyRateIsOverridden()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 50m, tariffType: ChildTariffTypeList.Codes.LEBIT);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024200", 50m);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "03024100";
			InvoiceLine.JI_CountryOfOrigin = "CA";

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			InvoiceLine.DutyRateIsOverridden = true;
			var tax = InvoiceLine.Taxes.FindByType(Constants.RateCodes.ImportDuty);
			AssertNotNull("JobComInvoiceLineTax with JLT_Type='0086' created", tax);

			CombineAssertions(InvoiceLine.JI_PrimaryPreference, () =>
			{
				AssertEquals("JLT_Rate must be equal to UniversalDutyRate.ZZ2_RateFormulaDerivedFrom", 0m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be equal to ADV", SpecialCaseTaxTypeList.Codes.AdValoremRate, tax.JLT_MethodOfCalculation);

				InvoiceLine.DutyRateIsOverridden = false;
				AssertEquals("JLT_Rate must be cleared", 0m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be cleared", ZString.Empty, tax.JLT_MethodOfCalculation);
			});

			InvoiceLine.JI_Tariff = "03024200";
			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
			InvoiceLine.DutyRateIsOverridden = true;
			CombineAssertions(InvoiceLine.JI_PrimaryPreference, () =>
			{
				Assert("FTAMarginRateValue must be equal to 0", InvoiceLine.FTAMarginRateValue.IsEmpty);

				InvoiceLine.FTAMarginRateValue = 10m;
				AssertEquals("JLT_Rate must be equal to 45", 45m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be equal to FTA", SpecialCaseTaxTypeList.Codes.TariffAgreement, tax.JLT_MethodOfCalculation);

				InvoiceLine.DutyRateIsOverridden = false;
				AssertEquals("JLT_Rate must be cleared", 0m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be cleared", ZString.Empty, tax.JLT_MethodOfCalculation);
			});

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
			InvoiceLine.DutyRateIsOverridden = true;
			CombineAssertions(InvoiceLine.JI_PrimaryPreference, () =>
			{
				AssertEquals("FTAMarginRateValue must be equal to 100", 100m, InvoiceLine.ReductionMarginRateValue);

				AssertEquals("JLT_Rate must be equal to 100", 0m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be equal to MAR", SpecialCaseTaxTypeList.Codes.Reduction, tax.JLT_MethodOfCalculation);

				InvoiceLine.DutyRateIsOverridden = false;
				AssertEquals("JLT_Rate must be cleared", 0m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be cleared", ZString.Empty, tax.JLT_MethodOfCalculation);
			});

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
			InvoiceLine.DutyRateIsOverridden = true;
			CombineAssertions(InvoiceLine.JI_PrimaryPreference, () =>
			{
				AssertEquals("JLT_Rate must be equal to 0", 0m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be equal to RED", SpecialCaseTaxTypeList.Codes.Reduced, tax.JLT_MethodOfCalculation);

				InvoiceLine.DutyRateIsOverridden = false;
				AssertEquals("JLT_Rate must be cleared", 0m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be cleared", ZString.Empty, tax.JLT_MethodOfCalculation);
			});

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.Normal;
			CombineAssertions(InvoiceLine.JI_PrimaryPreference, () =>
			{
				AssertEquals("JLT_Rate must be cleared", 0m, tax.JLT_Rate);
				AssertEquals("JLT_MethodOfCalculation must be cleared", ZString.Empty, tax.JLT_MethodOfCalculation);
				AssertEquals("DutyRateIsOverridden must be reset ", false, InvoiceLine.DutyRateIsOverridden);
			});
		}

		public void TestDutyVigentRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100_001", 50m, tariffType: ChildTariffTypeList.Codes.LEBIT);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 20m);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "03024100";
			InvoiceLine.JI_CountryOfOrigin = "CA";

			var additionalTariff = InvoiceLine.AdditionalTariffs.AddNew();

			additionalTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
			additionalTariff.ExNumber = "001";

			CombineAssertions(() =>
			{
				AssertEquals("DutyVigentRateValue value should be Normal rate", 20m, InvoiceLine.DutyVigentRateValue);
				Assert("DutyVigentRateValue must be ReadOnly", InvoiceLine.DutyVigentRateValueInfo.ReadOnly);
				AssertNull("DO NOT create JobComInvoiceLineTax when getting Duty Rate", InvoiceLine.Taxes.FindByType(Constants.RateCodes.ImportDuty));

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
				AssertEquals("DutyVigentRateValue value should be Normal rate", 50m, InvoiceLine.DutyVigentRateValue);
				Assert("DutyVigentRateValue must be ReadOnly", InvoiceLine.DutyVigentRateValueInfo.ReadOnly);

				InvoiceLine.DutyRateIsOverridden = true;
				AssertEquals("DutyVigentRateValue value should be default to Normal rate", 50m, InvoiceLine.DutyVigentRateValue);
				Assert("DutyVigentRateValue must NOT be ReadOnly", !InvoiceLine.DutyVigentRateValueInfo.ReadOnly);

				InvoiceLine.DutyVigentRateValue = 15m;
				AssertEquals("DutyVigentRateValue value should be overridden rate", 15m, InvoiceLine.DutyVigentRateValue);
				AssertEquals("Tax rate should be 15m", 15m, InvoiceLine.Taxes.FindByType(Constants.RateCodes.ImportDuty).JLT_Rate);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
				AssertEquals("DutyVigentRateValue value should be ", 20m, InvoiceLine.DutyVigentRateValue);
				Assert("DutyVigentRateValue must be ReadOnly", InvoiceLine.DutyVigentRateValueInfo.ReadOnly);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
				AssertEquals("DutyVigentRateValue value should be ", 20m, InvoiceLine.DutyVigentRateValue);
				Assert("DutyVigentRateValue must be ReadOnly", InvoiceLine.DutyVigentRateValueInfo.ReadOnly);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
				AssertEquals("DutyVigentRateValue value should be ", 20m, InvoiceLine.DutyVigentRateValue);
				Assert("DutyVigentRateValue must be ReadOnly", InvoiceLine.DutyVigentRateValueInfo.ReadOnly);
			});
		}

		public void TestFTAMarginRateValueandFTADutyRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 10m);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "03024100";
			InvoiceLine.JI_CountryOfOrigin = "CA";

			CombineAssertions(() =>
			{
				Assert("FTAMarginRateValue value should be 0", InvoiceLine.FTAMarginRateValue.IsEmpty);
				Assert("FTAMarginRateValue must be ReadOnly", InvoiceLine.FTAMarginRateValueInfo.ReadOnly);
				Assert("FTADutyRateValue value should be 0", InvoiceLine.FTADutyRateValue.IsEmpty);
				AssertNull("DO NOT create JobComInvoiceLineTax when getting Duty Rate", InvoiceLine.Taxes.FindByType(Constants.RateCodes.ImportDuty));

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
				Assert("FTAMarginRateValue value should be 0", InvoiceLine.FTAMarginRateValue.IsEmpty);
				Assert("FTAMarginRateValue must be ReadOnly", InvoiceLine.FTAMarginRateValueInfo.ReadOnly);
				Assert("FTADutyRateValue value should be 0", InvoiceLine.FTADutyRateValue.IsEmpty);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
				InvoiceLine.DutyRateIsOverridden = true;
				Assert("FTAMarginRateValue value should be default to 0", InvoiceLine.FTAMarginRateValue.IsEmpty);
				Assert("FTADutyRateValue value should NOT be 0", !InvoiceLine.FTADutyRateValue.IsEmpty);

				InvoiceLine.FTAMarginRateValue = 10m;
				AssertEquals("Tax rate should be 15m", 9m, InvoiceLine.Taxes.FindByType(Constants.RateCodes.ImportDuty).JLT_Rate);
				AssertEquals("FTADutyRateValue value should be", 9m, InvoiceLine.FTADutyRateValue);
				Assert("FTAMarginRateValue must NOT be ReadOnly", !InvoiceLine.FTAMarginRateValueInfo.ReadOnly);

				InvoiceLine.DutyRateIsOverridden = false;
				AssertEquals("FTAMarginRateValue value should be", 0m, InvoiceLine.FTAMarginRateValue);
				Assert("FTAMarginRateValue must be ReadOnly", InvoiceLine.FTAMarginRateValueInfo.ReadOnly);
				Assert("FTADutyRateValue value should be 0", InvoiceLine.FTADutyRateValue.IsEmpty);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
				Assert("FTAMarginRateValue value should be 0", InvoiceLine.FTAMarginRateValue.IsEmpty);
				Assert("FTAMarginRateValue must be ReadOnly", InvoiceLine.FTAMarginRateValueInfo.ReadOnly);
				Assert("FTADutyRateValue value should be 0", InvoiceLine.FTADutyRateValue.IsEmpty);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
				Assert("FTAMarginRateValue value should be 0", InvoiceLine.FTAMarginRateValue.IsEmpty);
				Assert("FTAMarginRateValue must be ReadOnly", InvoiceLine.FTAMarginRateValueInfo.ReadOnly);
				Assert("FTADutyRateValue value should be 0", InvoiceLine.FTADutyRateValue.IsEmpty);
			});
		}

		public void TestReductionMarginRateValueandReductionDutyRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 10m);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "03024100";
			InvoiceLine.JI_CountryOfOrigin = "CA";

			CombineAssertions(() =>
			{
				Assert("ReductionMarginRateValue value should be 0", InvoiceLine.ReductionMarginRateValue.IsEmpty);
				Assert("ReductionMarginRateValue must be ReadOnly", InvoiceLine.ReductionMarginRateValueInfo.ReadOnly);
				AssertNull("DO NOT create JobComInvoiceLineTax when getting Duty Rate", InvoiceLine.Taxes.FindByType(Constants.RateCodes.ImportDuty));

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
				Assert("ReductionMarginRateValue value should be 0", InvoiceLine.ReductionMarginRateValue.IsEmpty);
				Assert("ReductionMarginRateValue must be ReadOnly", InvoiceLine.ReductionMarginRateValueInfo.ReadOnly);
				Assert("ReductionDutyRateValue value should be 0", InvoiceLine.ReductionDutyRateValue.IsEmpty);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
				Assert("ReductionMarginRateValue value should be 0", InvoiceLine.ReductionMarginRateValue.IsEmpty);
				Assert("ReductionMarginRateValue must be ReadOnly", InvoiceLine.ReductionMarginRateValueInfo.ReadOnly);
				Assert("ReductionDutyRateValue value should be 0", InvoiceLine.ReductionDutyRateValue.IsEmpty);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
				InvoiceLine.DutyRateIsOverridden = true;
				AssertEquals("ReductionMarginRateValue value should be default to 100", 100m, InvoiceLine.ReductionMarginRateValue);
				Assert("ReductionDutyRateValue value should be 0", InvoiceLine.ReductionDutyRateValue.IsEmpty);

				InvoiceLine.ReductionMarginRateValue = 15m;
				AssertEquals("Tax rate should be 15m", 8.5m, InvoiceLine.Taxes.FindByType(Constants.RateCodes.ImportDuty).JLT_Rate);
				AssertEquals("ReductionMarginRateValue value should be", 15m, InvoiceLine.ReductionMarginRateValue);
				AssertEquals("ReductionDutyRateValue value should be 0", 8.5m, InvoiceLine.ReductionDutyRateValue);
				Assert("ReductionMarginRateValue must NOT be ReadOnly", !InvoiceLine.ReductionMarginRateValueInfo.ReadOnly);

				InvoiceLine.DutyRateIsOverridden = false;
				AssertEquals("ReductionMarginRateValue value should be", 0m, InvoiceLine.ReductionMarginRateValue);
				Assert("ReductionMarginRateValue must be ReadOnly", InvoiceLine.ReductionMarginRateValueInfo.ReadOnly);
				Assert("ReductionDutyRateValue value should be 0", InvoiceLine.ReductionDutyRateValue.IsEmpty);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
				Assert("ReductionMarginRateValue value should be 0", InvoiceLine.ReductionMarginRateValue.IsEmpty);
				Assert("ReductionMarginRateValue must be ReadOnly", InvoiceLine.ReductionMarginRateValueInfo.ReadOnly);
				Assert("ReductionDutyRateValue value should be 0", InvoiceLine.ReductionDutyRateValue.IsEmpty);
			});
		}

		public void TestReducedDutyRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 10m);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "03024100";
			InvoiceLine.JI_CountryOfOrigin = "CA";

			CombineAssertions(() =>
			{
				Assert("ReducedDutyRateValue value should be 0", InvoiceLine.ReducedDutyRateValue.IsEmpty);
				Assert("ReducedDutyRateValue must be ReadOnly", InvoiceLine.ReducedDutyRateValueInfo.ReadOnly);
				AssertNull("DO NOT create JobComInvoiceLineTax when getting Duty Rate", InvoiceLine.Taxes.FindByType(Constants.RateCodes.ImportDuty));

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
				Assert("ReducedDutyRateValue value should be 0", InvoiceLine.ReducedDutyRateValue.IsEmpty);
				Assert("ReducedDutyRateValue must be ReadOnly", InvoiceLine.ReducedDutyRateValueInfo.ReadOnly);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
				Assert("ReducedDutyRateValue value should be 0", InvoiceLine.ReducedDutyRateValue.IsEmpty);
				Assert("ReducedDutyRateValue must be ReadOnly", InvoiceLine.ReducedDutyRateValueInfo.ReadOnly);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
				Assert("ReducedDutyRateValue value should be 0", InvoiceLine.ReducedDutyRateValue.IsEmpty);
				Assert("ReducedDutyRateValue must be ReadOnly", InvoiceLine.ReducedDutyRateValueInfo.ReadOnly);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
				InvoiceLine.DutyRateIsOverridden = true;
				AssertEquals("ReducedDutyRateValue value should be default to 0", 0m, InvoiceLine.ReducedDutyRateValue);

				InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
				InvoiceLine.ReductionMarginRateValue = 15m;
				AssertEquals("Tax rate should be 8.5m", 8.5m, InvoiceLine.Taxes.FindByTypeAndMethod(Constants.RateCodes.ImportDuty, SpecialCaseTaxTypeList.Codes.Reduction).JLT_Rate);
				AssertEquals("ReducedDutyRateValue value should be", 0m, InvoiceLine.ReducedDutyRateValue);
				Assert("ReducedDutyRateValue must be ReadOnly", InvoiceLine.ReducedDutyRateValueInfo.ReadOnly);

				InvoiceLine.DutyRateIsOverridden = false;
				AssertEquals("ReducedDutyRateValue value should be", 0m, InvoiceLine.ReducedDutyRateValue);
				Assert("ReducedDutyRateValue must be ReadOnly", InvoiceLine.ReducedDutyRateValueInfo.ReadOnly);
			});
		}

		public void TestAntidumpingRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 3.15m, rateType: Constants.RateTypes.Antidumping, rateCode: Constants.RateCodes.Antidumping);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, Constants.RateTypes.Antidumping, "Anti - Dumping");
			var rateCodeDuty = helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateCodes.Antidumping, rateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup("BR", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, rateCodeDuty.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.315", null, "3.15", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "99999999";
			Assert("AntidumpingRateValue should be Empty", InvoiceLine.AntidumpingRateValue.IsEmpty);
			AssertEquals("AntidumpingRateIsOverridden", false, InvoiceLine.AntidumpingRateIsOverridden);

			InvoiceLine.JI_CountryOfOrigin = "CA";
			AssertEquals("AntidumpingRateValue", 3.15m, InvoiceLine.AntidumpingRateValue);
			AssertEquals("AntidumpingRateIsOverridden", false, InvoiceLine.AntidumpingRateIsOverridden);

			var antidumping = InvoiceLine.Taxes.AddNew();
			antidumping.JLT_Type = Constants.RateCodes.Antidumping;
			antidumping.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			antidumping.JLT_Rate = 10m;
			AssertEquals("AntidumpingRateValue", 10m, InvoiceLine.AntidumpingRateValue);
			AssertEquals("AntidumpingRateIsOverridden", true, InvoiceLine.AntidumpingRateIsOverridden);
			AssertEquals("JLT_Rate must be equal to 10", 10m, InvoiceLine.Taxes.FindByType(Constants.RateCodes.Antidumping).JLT_Rate);
		}

		public void TestJI_PrimaryPreference()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			AssertEquals("JI_PrimaryPreference must be equal to EXTARIFF", Constants.RatePreferenceType.ExTariff, InvoiceLine.JI_PrimaryPreference);

			InvoiceLine.DutyRateIsOverridden = true;
			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
			Factory.Save();
			AssertEquals("JI_PrimaryPreference must be equal to EXTARIFF", Constants.RatePreferenceType.ReducedRate, InvoiceLine.JI_PrimaryPreference);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();
			Assert("JI_PrimaryPreference must be Empty", InvoiceLine.JI_PrimaryPreference.IsEmpty);
		}

		public void TestExDutyTariff()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000_001", 50m, tariffType: ChildTariffTypeList.Codes.LEBIT, preference: Constants.RatePreferenceType.ExTariff);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000_002", 40m, tariffType: ChildTariffTypeList.Codes.LETEC, preference: Constants.RatePreferenceType.ExTariff);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "09022000";
			var additionalTariff = InvoiceLine.AdditionalTariffs.AddNew();

			additionalTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
			additionalTariff.ExNumber = "001";
			AssertEquals("ExDutyTariff", ChildTariffTypeList.Codes.LEBIT, InvoiceLine.ExDutyTariff.ZZ1_ZZI_NKTariffType);
			AssertEquals("ExDutyTariff", "09022000_001", InvoiceLine.ExDutyTariff.ZZ1_TariffCode);

			additionalTariff.ExNumber = "002";
			AssertNull("ExDutyTariff", InvoiceLine.ExDutyTariff);

			additionalTariff.TariffType = ChildTariffTypeList.Codes.LETEC;
			AssertEquals("ExDutyTariff", ChildTariffTypeList.Codes.LETEC, InvoiceLine.ExDutyTariff.ZZ1_ZZI_NKTariffType);
			AssertEquals("ExDutyTariff", "09022000_002", InvoiceLine.ExDutyTariff.ZZ1_TariffCode);
		}

		public void TestExDutyTariffRate()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000_001", 50m, tariffType: ChildTariffTypeList.Codes.LEBIT, preference: Constants.RatePreferenceType.ExTariff);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000_002", 40m, tariffType: ChildTariffTypeList.Codes.LETEC, preference: Constants.RatePreferenceType.ExTariff);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "09022000";
			InvoiceLine.JI_CountryOfOrigin = "CA";
			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			var additionalTariff = InvoiceLine.AdditionalTariffs.FirstOrDefault() as AdditionalTariff;

			additionalTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
			additionalTariff.ExNumber = "001";
			AssertEquals("ExDutyTariffRate", "50", InvoiceLine.ExDutyTariffRate.ZZ2_RateFormulaDerivedFrom);

			additionalTariff.ExNumber = "002";
			AssertNull("ExDutyTariffRate", InvoiceLine.ExDutyTariffRate);

			additionalTariff.TariffType = ChildTariffTypeList.Codes.LETEC;
			AssertEquals("ExDutyTariffRate", "40", InvoiceLine.ExDutyTariffRate.ZZ2_RateFormulaDerivedFrom);
		}

		public void TestExTariffDutyRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000", 60m);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000_001", 50m, tariffType: ChildTariffTypeList.Codes.LEBIT);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000_002", 40m, tariffType: ChildTariffTypeList.Codes.LETEC);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "09022000";
			InvoiceLine.JI_CountryOfOrigin = "CA";
			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			var additionalTariff = InvoiceLine.AdditionalTariffs.FirstOrDefault() as AdditionalTariff;

			additionalTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
			additionalTariff.ExNumber = "001";
			AssertEquals("ExTariffDutyRateValue", 50m, InvoiceLine.ExTariffDutyRateValue);

			additionalTariff.ExNumber = "002";
			AssertEquals("Fallback to NormalDutyRate", 60m, InvoiceLine.ExTariffDutyRateValue);

			additionalTariff.TariffType = ChildTariffTypeList.Codes.LETEC;
			AssertEquals("ExDutyTariffRate", 40m, InvoiceLine.ExTariffDutyRateValue);
		}

		public void TestDutyVigentRateValue_OnSettingJI_PrimaryPreferenceToExTariff()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000_001", 50m, tariffType: ChildTariffTypeList.Codes.LEBIT);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "09022000";
			InvoiceLine.JI_CountryOfOrigin = "CA";

			var additionalTariff = InvoiceLine.AdditionalTariffs.AddNew();

			additionalTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
			additionalTariff.ExNumber = "001";

			AssertEquals("ExTariffDutyRateValue should be", 50m, InvoiceLine.ExTariffDutyRateValue);
			AssertEquals("DutyVigentRateValue should be ", 0m, InvoiceLine.DutyVigentRateValue);

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;

			AssertEquals("DutyVigentRateValue should be set to ExTariffDutyRateValue", InvoiceLine.ExTariffDutyRateValue, InvoiceLine.DutyVigentRateValue);
			AssertEquals("DutyVigentRateValue should be ", 50m, InvoiceLine.DutyVigentRateValue);
		}

		public void TestIPITaxationRegimeLegalBasis()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			AssertEquals("LegalActInfos count should be", 0, InvoiceLine.LegalActInfos.Count);
			AssertEquals("IPITaxBenefitLegalActIssuingBody count should be", ZString.Empty, InvoiceLine.IPITaxBenefitLegalActIssuingBody);

			InvoiceLine.IPITaxBenefitLegalActType = "AD";
			InvoiceLine.IPITaxBenefitLegalActIssuingBody = "ALADI";
			InvoiceLine.IPITaxBenefitLegalActNumber = "54878";
			InvoiceLine.IPITaxBenefitLegalActYear = "2022";

			var legalAct = InvoiceLine.LegalActInfos.FindBySubject(AdditionalTaxTypeList.Codes.IPITaxBenefit);

			AssertEquals("LegalActInfos count should be", 1, InvoiceLine.LegalActInfos.Count);
			AssertEquals("LegalActInfos.CSI_Code count should be", "AD", legalAct.CSI_Code);
			AssertEquals("LegalActInfos.CSI_IssuerType count should be", "ALADI", legalAct.CSI_IssuerType);
			AssertEquals("LegalActInfos.CSI_ReferenceNumber count should be", "54878", legalAct.CSI_ReferenceNumber);
			AssertEquals("LegalActInfos.CSI_YearOfIssue count should be", "2022", legalAct.CSI_YearOfIssue);
			AssertEquals("LegalActInfos.CSI_SubType count should be", AdditionalTaxTypeList.Codes.IPITaxBenefit, legalAct.CSI_SubType);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Factory.Save();

			AssertEquals("LegalActInfos count should be", 0, InvoiceLine.LegalActInfos.Count);
		}

		public void TestDeleteInvoiceLine()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			InvoiceLine.DutyRateIsOverridden = true;

			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(JobComInvoiceLineTax));
			query.AddToFilter(JobComInvoiceLineTaxSchema.JLT_JI, InvoiceLine.PK);
			var taxes = Factory.Load(typeof(JobComInvoiceLineTax), query);
			Assert("Must contain JobComInvoiceLineTax", taxes.Any());

			InvoiceLine.Delete();
			Factory.Save();
			query = new ZDBOnlyQuery(typeof(JobComInvoiceLineTax));
			query.AddToFilter(JobComInvoiceLineTaxSchema.PK, ((JobComInvoiceLineTax)taxes[0]).PK);
			taxes = Factory.Load(typeof(JobComInvoiceLineTax), query);
			Assert("Must NOT contain JobComInvoiceLineTax", !taxes.Any());
		}

		public void TestJI_TemporaryAdmissionReason()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("JI_TemporaryAdmissionReason must be read only", InvoiceLine.JI_TemporaryAdmissionReasonInfo.ReadOnly);
			Assert("JI_TemporaryAdmissionReason must be empty", InvoiceLine.JI_TemporaryAdmissionReason.IsEmpty);

			Declaration.JE_MessageSubType = MessageSubTypeList.Codes._05;
			InvoiceLine.DutyTaxRegime = TaxRegimeList.Codes.Suspension;
			InvoiceLine.DutyLegalBase = "00";
			InvoiceLine.JI_TemporaryAdmissionReason = "60";
			Assert("IsTemporaryAdmissionReasonApplicable", InvoiceLine.IsTemporaryAdmissionReasonApplicable);
			Assert("JI_TemporaryAdmissionReason must NOT be read only", !InvoiceLine.JI_TemporaryAdmissionReasonInfo.ReadOnly);
			AssertEquals("JI_TemporaryAdmissionReason must be equal to", "60", InvoiceLine.JI_TemporaryAdmissionReason);

			InvoiceLine.DutyTaxRegime = "1";
			InvoiceLine.DutyLegalBase = "00";
			Assert("IsTemporaryAdmissionReasonApplicable", !InvoiceLine.IsTemporaryAdmissionReasonApplicable);
			Assert("JI_TemporaryAdmissionReason must be read only", InvoiceLine.JI_TemporaryAdmissionReasonInfo.ReadOnly);
			Assert("JI_TemporaryAdmissionReason must be empty", InvoiceLine.JI_TemporaryAdmissionReason.IsEmpty);

			Declaration.JE_MessageSubType = MessageSubTypeList.Codes._12;
			InvoiceLine.DutyLegalBase = "00";
			Assert("IsTemporaryAdmissionReasonApplicable", !InvoiceLine.IsTemporaryAdmissionReasonApplicable);
			Assert("JI_TemporaryAdmissionReason must be read only", InvoiceLine.JI_TemporaryAdmissionReasonInfo.ReadOnly);
			Assert("JI_TemporaryAdmissionReason must be empty", InvoiceLine.JI_TemporaryAdmissionReason.IsEmpty);

			Declaration.JE_MessageSubType = MessageSubTypeList.Codes._12;
			InvoiceLine.DutyTaxRegime = TaxRegimeList.Codes.Suspension;
			InvoiceLine.DutyLegalBase = "00";
			InvoiceLine.JI_TemporaryAdmissionReason = "60";
			Factory.Save();
			Assert("IsTemporaryAdmissionReasonApplicable", InvoiceLine.IsTemporaryAdmissionReasonApplicable);
			Assert("JI_TemporaryAdmissionReason must NOT be read only", !InvoiceLine.JI_TemporaryAdmissionReasonInfo.ReadOnly);
			AssertEquals("JI_TemporaryAdmissionReason must be equal to", "60", InvoiceLine.JI_TemporaryAdmissionReason);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Factory.Save();
			Assert("IsTemporaryAdmissionReasonApplicable", !InvoiceLine.IsTemporaryAdmissionReasonApplicable);
			Assert("JI_TemporaryAdmissionReason must be read only", InvoiceLine.JI_TemporaryAdmissionReasonInfo.ReadOnly);
			Assert("JI_TemporaryAdmissionReason must be empty", InvoiceLine.JI_TemporaryAdmissionReason.IsEmpty);
		}

		public void TestJI_ComplementaryNote()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("JI_ComplementaryNote must be empty", InvoiceLine.JI_ComplementaryNote.IsEmpty);

			InvoiceLine.JI_ComplementaryNote = "60";
			AssertEquals("JI_ComplementaryNote must be equal to 60", "60", InvoiceLine.JI_ComplementaryNote);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Factory.Save();
			Assert("JI_ComplementaryNote must be empty", InvoiceLine.JI_ComplementaryNote.IsEmpty);
		}

		public void TestJI_OverseasFreight()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;

			InvoiceLine.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code, 500m, Core.Constants.CurrencyCodes.Brazil);
			AssertEquals("Overseas Freight for ISW with only OFC charge", 500m, InvoiceLine.JI_OverseasFreight.Amount);

			InvoiceLine.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code, 200m, Core.Constants.CurrencyCodes.Brazil);
			AssertEquals("Overseas Freight for ISW with OFC and OFP charges", 700m, InvoiceLine.JI_OverseasFreight.Amount);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("Overseas Freight for LIC with OFC and OFP charges", 700m, InvoiceLine.JI_OverseasFreight.Amount);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("Overseas Freight for EXP without OFT charge", 0m, InvoiceLine.JI_OverseasFreight.Amount);

			InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, Core.Constants.CurrencyCodes.Brazil);
			AssertEquals("Overseas Freight for EXP with OFT", 100m, InvoiceLine.JI_OverseasFreight.Amount);
		}

		public void TestAntidumpingLegalBasis()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			AssertEquals("LegalActInfos count should be", 0, InvoiceLine.LegalActInfos.Count);

			var specalCaseTax = InvoiceLine.SpecialCaseTaxes.AddNew();
			specalCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			specalCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			specalCaseTax.LegalActType = "AD";
			specalCaseTax.LegalActIssuingBody = "ALADI";
			specalCaseTax.LegalActNumber = "54878";
			specalCaseTax.LegalActYear = "2022";

			var legalAct = InvoiceLine.LegalActInfos.FindBySubject(AdditionalTaxTypeList.Codes.Antidumping);

			AssertEquals("LegalActInfos count should be", 1, InvoiceLine.LegalActInfos.Count);
			AssertEquals("LegalActInfos.CSI_Code count should be", "AD", legalAct.CSI_Code);
			AssertEquals("LegalActInfos.CSI_IssuerType count should be", "ALADI", legalAct.CSI_IssuerType);
			AssertEquals("LegalActInfos.CSI_ReferenceNumber count should be", "54878", legalAct.CSI_ReferenceNumber);
			AssertEquals("LegalActInfos.CSI_YearOfIssue count should be", "2022", legalAct.CSI_YearOfIssue);
			AssertEquals("LegalActInfos.CSI_SubType count should be", AdditionalTaxTypeList.Codes.Antidumping, legalAct.CSI_SubType);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Factory.Save();

			AssertEquals("LegalActInfos count should be", 0, InvoiceLine.LegalActInfos.Count);
		}

		public void TestManufacturerAddressReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			AssertEquals("ManufacturerAddress should be readonly", true, InvoiceLine.JI_OA_ManufacturerAddressInfo.ReadOnly);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			AssertEquals("ManufacturerAddress shouldn't be readonly", false, InvoiceLine.JI_OA_ManufacturerAddressInfo.ReadOnly);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;
			AssertEquals("ManufacturerAddress should be readonly", true, InvoiceLine.JI_OA_ManufacturerAddressInfo.ReadOnly);
		}

		public void TestManufacturerDocAddressPK()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			AssertEquals("ManufacturerDocAddressPK", ZGuid.Empty, InvoiceLine.ManufacturerDocAddressPK);

			var manufacturerAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			InvoiceLine.ManufacturerDocAddressPK = manufacturerAddress.PK;

			AssertEquals("ManufacturerDocAddressPK", manufacturerAddress.PK, InvoiceLine.ManufacturerDocAddressPK);
			AssertEquals("ManufacturerDocAddress.PK", manufacturerAddress.PK, InvoiceLine.ManufacturerDocAddress.E2_OA_Address);
		}

		public void TestManufacturerName()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var manufacturerOrg = Factory.NewWithValidTestData<OrgHeader>();
			manufacturerOrg.OH_FullName = "Manufacturer Name if it is a string with more than fifty characters";
			var manufacturerAddress = manufacturerOrg.MainAddress;

			InvoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;

			AssertEquals("ManufacturerName", "Manufacturer Name if it is a string with more than", InvoiceLine.ManufacturerName);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("ManufacturerName", "Manufacturer Name if it is a string with more than", InvoiceLine.ManufacturerName);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("Manufacturer Name should be empty before setting ManufacturerDocAddressPK", ZString.Empty, InvoiceLine.ManufacturerName);

			InvoiceLine.ManufacturerDocAddressPK = manufacturerAddress.PK;
			AssertEquals("ManufacturerName", "Manufacturer Name if it is a string with more than", InvoiceLine.ManufacturerName);
		}

		public void TestNormalTariffDutyRateSelectionCriteria()
		{
			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			var selectionriteria = InvoiceLine.NormalDutyRateSelectionCriteria;
			CombineAssertions(() =>
			{
				AssertType<JobComInvoiceLine.NormalRateSelectionCriteria>(selectionriteria);
				AssertEquals("PrimaryPreference must be NORMAL", Constants.RatePreferenceType.Normal, selectionriteria.PrimaryPreference);
				AssertEquals("RateType must be DTY", Universal.Constants.RateTypes.Duty, selectionriteria.RateType);
				AssertEquals("RateCode must be 0086", Constants.RateCodes.ImportDuty, selectionriteria.RateCode);
			});
		}

		public void TestDeleteManufacturerContactDetailAddressOnSaving()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var manufacturerContactDetailAddress = InvoiceLine.ManufacturerDocAddress;
			manufacturerContactDetailAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			Factory.Save();
			AssertEquals("ManufacturerDocAddress should be saved", true, manufacturerContactDetailAddress.IsInDatabase);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertEquals("ManufacturerDocAddress should be deleted", true, manufacturerContactDetailAddress.IsDeleted);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			manufacturerContactDetailAddress = InvoiceLine.ManufacturerDocAddress;
			manufacturerContactDetailAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			manufacturerContactDetailAddress.Delete();
			AssertEquals("ManufacturerDocAddress should be deleted", true, manufacturerContactDetailAddress.IsDeleted);
		}

		public void TestManufacturerDocAddressReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;

			AssertEquals("ManufacturerDocAddressPK should be readonly", true, InvoiceLine.ManufacturerDocAddressPKInfo.ReadOnly);
			AssertEquals("ManufacturerDocOrgPK should be readonly", true, InvoiceLine.ManufacturerDocOrgPKInfo.ReadOnly);
			AssertEquals("ManufacturerDocAddress should be readonly", true, InvoiceLine.ManufacturerDocAddress.ReadOnly);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;

			AssertEquals("ManufacturerDocAddressPK should NOT be readonly", false, InvoiceLine.ManufacturerDocAddressPKInfo.ReadOnly);
			AssertEquals("ManufacturerDocOrgPK should NOT be readonly", false, InvoiceLine.ManufacturerDocOrgPKInfo.ReadOnly);
			AssertEquals("ManufacturerDocAddress should NOT be readonly", false, InvoiceLine.ManufacturerDocAddress.ReadOnly);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;

			AssertEquals("ManufacturerDocAddressPK should be readonly", true, InvoiceLine.ManufacturerDocAddressPKInfo.ReadOnly);
			AssertEquals("ManufacturerDocOrgPK should be readonly", true, InvoiceLine.ManufacturerDocOrgPKInfo.ReadOnly);
			AssertEquals("ManufacturerDocAddress should be readonly", true, InvoiceLine.ManufacturerDocAddress.ReadOnly);
		}

		public void TestManufacturerVersionReadOnly()
		{
			Assert("JI_ManufacturerAuthorityIdentifier should NOT be ReadOnly", !InvoiceLine.JI_ManufacturerAuthorityIdentifierInfo.ReadOnly);
			Assert("JI_ManufacturerAuthorityVersion should NOT be ReadOnly", !InvoiceLine.JI_ManufacturerAuthorityVersionInfo.ReadOnly);
		}

		public void TestUpdateDetailsFromPivotOnPartChange_CopyNveAndTariffDetach_LIC()
		{
			TestUpdateDetailsFromPivotOnPartChange_CopyNveAndTariffDetach(BRJobMessageTypeList.Codes.ImportLicense);
		}

		public void TestUpdateDetailsFromPivotOnPartChange_CopyNveAndTariffDetach_ISW()
		{
			TestUpdateDetailsFromPivotOnPartChange_CopyNveAndTariffDetach(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		void TestUpdateDetailsFromPivotOnPartChange_CopyNveAndTariffDetach(string messageType)
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_Tariff = "11111111";
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog.CGC_Description = "Test Descr";

			var classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "11111111";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TEST_PROD";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "11111111";

			AssertEquals("NveCusCodeDataCollection.Count should be", 2, pivot.NveCusCodeDataCollection.Count);
			pivot.TariffDetachs.AddNew("333");
			pivot.TariffDetachs.AddNew("555");
			pivot.NveCusCodeDataCollection[0].CY_Data = "0001";
			pivot.NveCusCodeDataCollection[1].CY_Data = "0002";

			Declaration.JE_MessageType = messageType;
			InvoiceLine.JI_Tariff = "11111111";

			InvoiceLine.SetPartForTesting(part);
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertPivot();

			var emptyPart = Factory.New<OrgSupplierPart>();
			emptyPart.OP_PartNum = "EMPTY_PART";
			pivot = emptyPart.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "11111111";
			AssertEquals("NveCusCodeDataCollection.Count should be", 2, pivot.NveCusCodeDataCollection.Count);
			InvoiceLine.SetPartForTesting(emptyPart);
			InvoiceLine.JI_PartNo = emptyPart.OP_PartNum;
			AssertEquals("NVECusCodeDataCollection.Count should be", 2, InvoiceLine.NVECusCodeDataCollection.Count);
			AssertEquals("NVE should NOT be cleared", "0001", InvoiceLine.NVECusCodeDataCollection[0].CY_Data);
			AssertEquals("NVE should NOT be cleared", "0002", InvoiceLine.NVECusCodeDataCollection[1].CY_Data);
			AssertEquals("TariffDetachCollection.Count should be cleard", 0, InvoiceLine.TariffDetachs.Count);

			pivot.CI_CGC_Catalog = catalog.PK;
			InvoiceLine.SetPartForTesting(part);
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertPivot();

			pivot.CI_CGC_Catalog = ZGuid.Empty;
			pivot.CI_CC = classification.PK;
			InvoiceLine.SetPartForTesting(part);
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertPivot();

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.NVECusCodeDataCollection.RemoveAndDeleteAll();
			InvoiceLine.TariffDetachs.RemoveAndDeleteAll();
			InvoiceLine.SetPartForTesting(part);
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("NVECusCodeDataCollection should NOT be copied", 0, InvoiceLine.NVECusCodeDataCollection.Count);
			AssertEquals("TariffDetachCollection should NOT be copied", 0, InvoiceLine.TariffDetachs.Count);
		}

		void AssertPivot()
		{
			AssertEquals("NVECusCodeDataCollection.Count should be", 2, InvoiceLine.NVECusCodeDataCollection.Count);
			AssertEquals("NVE should be copied", "0001", InvoiceLine.NVECusCodeDataCollection[0].CY_Data);
			AssertEquals("NVE should be copied", "0002", InvoiceLine.NVECusCodeDataCollection[1].CY_Data);
			AssertEquals("TariffDetachCollection.Count should be", 2, InvoiceLine.TariffDetachs.Count);
			AssertEquals("TariffDetach should be copied", "333", InvoiceLine.TariffDetachs[0].CY_Code);
			AssertEquals("TariffDetach should be copied", "555", InvoiceLine.TariffDetachs[1].CY_Code);
		}

		public void TestUpdateDetailsFromPivotOnPartChange_CopyAttributes()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TEST_PROD";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = "99999999";
			pivot.Attributes[0].CY_Data = "TEST1";
			pivot.Attributes[1].CY_Data = "TEST2";

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = "55555555";
			AssertEquals(0, InvoiceLine.Attributes.Count);
			InvoiceLine.JI_Tariff = "99999999";

			InvoiceLine.SetPartForTesting(part);
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("99999999", InvoiceLine.JI_Tariff);
			AssertEquals(2, InvoiceLine.Attributes.Count);
			AssertEquals("TEST1", InvoiceLine.Attributes[0].CY_Data);
			AssertEquals("TEST2", InvoiceLine.Attributes[1].CY_Data);

			pivot.Attributes[0].CY_Data = "TEST3";
			pivot.Attributes[1].CY_Data = "";

			InvoiceLine.JI_PartNo = ZString.Empty;
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("TEST3", InvoiceLine.Attributes[0].CY_Data);
			AssertEquals("TEST2", InvoiceLine.Attributes[1].CY_Data);

			InvoiceLine.Attributes.RemoveAll();
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_PartNo = ZString.Empty;
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(0, InvoiceLine.Attributes.Count);
		}

		public void TestUpdateDetailsFromPivotOnPartChange_CopyAdditionalTariffs()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TEST_PROD";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "99999999";
			var additionalTariff1 = pivot.AdditionalTariffs.AddNew();
			additionalTariff1.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			additionalTariff1.LegalActYear = "2011";

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "99999999";
			AssertEquals(0, InvoiceLine.AdditionalTariffs.Count);

			InvoiceLine.SetPartForTesting(part);
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("99999999", InvoiceLine.JI_Tariff);
			AssertEquals(1, InvoiceLine.AdditionalTariffs.Count);
			AssertEquals(AdditionalTaxTypeList.Codes.ExDutyTariff, InvoiceLine.AdditionalTariffs[0].LegalActSubject);
			AssertEquals("2011", InvoiceLine.AdditionalTariffs[0].LegalActYear);

			additionalTariff1.LegalActYear = "2012";
			var additionalTariff2 = pivot.AdditionalTariffs.AddNew();
			additionalTariff2.LegalActSubject = AdditionalTaxTypeList.Codes.ExIPITariff;
			additionalTariff2.LegalActYear = "2013";

			pivot.AdditionalTariffs.AddNew().LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;

			InvoiceLine.JI_PartNo = ZString.Empty;
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(3, InvoiceLine.AdditionalTariffs.Count);
			AssertEquals("2012", InvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff).LegalActYear);
			AssertEquals("2013", InvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExIPITariff).LegalActYear);
			AssertNotNull(InvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement));

			InvoiceLine.AdditionalTariffs.RemoveAll();
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.JI_PartNo = ZString.Empty;
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Should not copy AdditionalTariffs", 0, InvoiceLine.AdditionalTariffs.Count);
		}

		public void TestUpdateDetailsFromPivotOnPartChange_CopyTariffFromCatalog()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "XXX";
			supplier.OH_FullName = "TEST COMPANY";

			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_AuthorityIdentifier = "01";
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog.CGC_CatalogCode = "C123";
			catalog.CGC_Description = "Test";
			catalog.CGC_Tariff = "11111111";
			catalog.CGC_OH_Owner = supplier.PK;

			var partRelated = Factory.New<OrgSupplierPart>();
			partRelated.OP_PartNum = "TEST1_PROD";
			partRelated.RelatedOrganisations.AddSupplier(supplier);

			var pivot = partRelated.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_CGC_Catalog = catalog.PK;
			pivot.CI_OH = supplier.PK;

			Factory.Save();

			InvoiceHeader.JZ_OH_Supplier = supplier.PK;

			AssertSetTariffFromProductPivot(BRJobMessageTypeList.Codes.Export, "11111111", catalog.PK);
			AssertSetTariffFromProductPivot(BRJobMessageTypeList.Codes.Import, "11111111", catalog.PK);
			AssertSetTariffFromProductPivot(BRJobMessageTypeList.Codes.ImportSiscomex, "11111111", catalog.PK);
			AssertSetTariffFromProductPivot(BRJobMessageTypeList.Codes.ImportLicense, "11111111", catalog.PK);

			pivot.CI_CGC_Catalog = ZGuid.Empty;
			pivot.CI_TariffNum = "00000000";
			Factory.Save();

			AssertSetTariffFromProductPivot(BRJobMessageTypeList.Codes.Export, "00000000", ZGuid.Empty);
			AssertSetTariffFromProductPivot(BRJobMessageTypeList.Codes.Import, "00000000", ZGuid.Empty);
			AssertSetTariffFromProductPivot(BRJobMessageTypeList.Codes.ImportSiscomex, "00000000", ZGuid.Empty);
			AssertSetTariffFromProductPivot(BRJobMessageTypeList.Codes.ImportLicense, "00000000", ZGuid.Empty);

			void AssertSetTariffFromProductPivot(string shipmentType, string tariffExpected, ZGuid catalogExpected)
			{
				InvoiceLine.JI_PartNo = ZString.Empty;
				Declaration.JE_MessageType = shipmentType;
				InvoiceLine.JI_PartNo = partRelated.OP_PartNum;
				AssertEquals("JI_CGC_Catalog should be", catalogExpected, InvoiceLine.JI_CGC_Catalog);
				AssertEquals("JI_Tariff should be", tariffExpected, InvoiceLine.JI_Tariff);
			}
		}

		public void TestICMSTaxRegimeAndLegalBase()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.ICMS);
			CombineAssertions(() =>
			{
				Assert("ICMSTaxRegime must be empty", InvoiceLine.ICMSTaxRegime.IsEmpty);
				Assert("ICMSLegalBase must be empty", InvoiceLine.ICMSLegalBase.IsEmpty);
				AssertNull("TaxRegime CusSupportingInfo must be empty", taxRegime);
			});

			InvoiceLine.ICMSTaxRegime = "1";
			InvoiceLine.ICMSLegalBase = "01";

			taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.ICMS);
			CombineAssertions(() =>
			{
				AssertEquals("ICMSTaxRegime must be equal", "1", InvoiceLine.ICMSTaxRegime);
				AssertEquals("ICMSLegalBase must be equal", "01", InvoiceLine.ICMSLegalBase);
				AssertEquals("CSI_Type must be equal", CusSupportingInfoTypeList.Codes.TaxRegime, taxRegime.CSI_Type);
				AssertEquals("CSI_SubType must be equal", TaxRegimeTypeList.Codes.ICMS, taxRegime.CSI_SubType);
				AssertEquals("CSI_Code must be equal", "1", taxRegime.CSI_Code);
				AssertEquals("CSI_Procedure must be equal", "01", taxRegime.CSI_Procedure);
			});

			taxRegime.CSI_Code = "2";
			taxRegime.CSI_Procedure = "02";
			CombineAssertions(() =>
			{
				AssertEquals("DutyTaxRegime must be equal", "2", InvoiceLine.ICMSTaxRegime);
				AssertEquals("DutyLegalBase must be equal", "02", InvoiceLine.ICMSLegalBase);
			});
		}

		public void TestJI_ICMSRate()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			CombineAssertions(() =>
			{
				AssertEquals("JI_ICMSRate value should be 0", 0m, InvoiceLine.JI_ICMSRate);

				InvoiceLine.JI_ICMSRate = 12m;
				AssertEquals("JI_ICMSRate value should be", 12m, InvoiceLine.JI_ICMSRate);

				InvoiceLine.JI_ICMSRate = 18m;
				AssertEquals("JI_ICMSRate value should be", 18m, InvoiceLine.JI_ICMSRate);

				InvoiceLine.JI_ICMSRate = 0m;
				AssertEquals("AntidumpingRateValue value should", 0m, InvoiceLine.JI_ICMSRate);

				InvoiceLine.JI_ICMSBaseValueReductionPercentage = 5m;
				InvoiceLine.JI_ICMSRate = 0m;
				AssertEquals("JI_ICMSRate value should", 0m, InvoiceLine.JI_ICMSRate);
			});
		}

		public void TestJI_ICMSBaseValueReductionPercentage()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			CombineAssertions(() =>
			{
				AssertEquals("JI_ICMSBaseValueReductionPercentage value should be 0", 0m, InvoiceLine.JI_ICMSBaseValueReductionPercentage);

				InvoiceLine.JI_ICMSBaseValueReductionPercentage = 5.12345m;
				AssertEquals("JI_ICMSBaseValueReductionPercentage value should be", 5.12345m, InvoiceLine.JI_ICMSBaseValueReductionPercentage);

				InvoiceLine.JI_ICMSBaseValueReductionPercentage = 180m;
				AssertEquals("JI_ICMSBaseValueReductionPercentage value should be", 180m, InvoiceLine.JI_ICMSBaseValueReductionPercentage);

				InvoiceLine.ICMSTaxRegime = "1";
				AssertEquals("JI_ICMSBaseValueReductionPercentage value should", 0m, InvoiceLine.JI_ICMSBaseValueReductionPercentage);

				InvoiceLine.JI_ICMSRate = 5m;
				InvoiceLine.JI_ICMSBaseValueReductionPercentage = 0m;
				AssertEquals("JI_ICMSBaseValueReductionPercentage value should", 0m, InvoiceLine.JI_ICMSBaseValueReductionPercentage);
			});
		}

		public void TestJI_ICMSFormula()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			CombineAssertions(() =>
			{
				AssertEquals("JI_ICMSFormula_ReadOnly value should be true", true, InvoiceLine.JI_ICMSFormula_ReadOnly);
				AssertEquals("JI_ICMSBaseValueReductionPercentage value should be 0", 0m, InvoiceLine.JI_ICMSBaseValueReductionPercentage);

				InvoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
				AssertEquals("JI_ICMSFormula value should be Empty", ZString.Empty, InvoiceLine.JI_ICMSFormula);
				AssertEquals("JI_ICMSFormula_ReadOnly value should be true", true, InvoiceLine.JI_ICMSFormula_ReadOnly);

				InvoiceLine.JI_ICMSBaseValueReductionPercentage = 5.12345m;
				AssertEquals("JI_ICMSBaseValueReductionPercentage value should be", 5.12345m, InvoiceLine.JI_ICMSBaseValueReductionPercentage);
				AssertEquals("JI_ICMSFormula_ReadOnly value should be false", false, InvoiceLine.JI_ICMSFormula_ReadOnly);
				AssertEquals("JI_ICMSFormula value should be Empty", ZString.Empty, InvoiceLine.JI_ICMSFormula);

				InvoiceLine.JI_ICMSFormula = ICMSFormulaList.Codes.BC;
				AssertEquals("JI_ICMSFormula value should be BC", ICMSFormulaList.Codes.BC, InvoiceLine.JI_ICMSFormula);

				InvoiceLine.JI_ICMSFormula = ICMSFormulaList.Codes.BCR;
				AssertEquals("JI_ICMSFormula value should be BCR", ICMSFormulaList.Codes.BCR, InvoiceLine.JI_ICMSFormula);

				InvoiceLine.JI_ICMSBaseValueReductionPercentage = 0m;
				AssertEquals("JI_ICMSFormula value should be Empty", ZString.Empty, InvoiceLine.JI_ICMSFormula);
			});
		}

		public void TestICMSBaseValueReductionPercentageReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			CombineAssertions("Base Amount Reduction (%) is Read Only", () =>
			{
				InvoiceLine.ICMSTaxRegime = "1";
				Assert("ICMSBaseValueReductionPercentageReadOnly should be ReadOnly when ICMSTaxRegime = 1", InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);

				InvoiceLine.ICMSTaxRegime = "2";
				Assert("ICMSBaseValueReductionPercentageReadOnly should be ReadOnly when ICMSTaxRegime = 2", InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);

				InvoiceLine.ICMSTaxRegime = "3";
				Assert("ICMSBaseValueReductionPercentageReadOnly should be ReadOnly when ICMSTaxRegime = 3", InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);

				InvoiceLine.ICMSTaxRegime = "4";
				Assert("ICMSBaseValueReductionPercentageReadOnly should NOT be ReadOnly when ICMSTaxRegime = 4", !InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);

				InvoiceLine.ICMSTaxRegime = "5";
				Assert("ICMSBaseValueReductionPercentageReadOnly should be ReadOnly when ICMSTaxRegime = 5", InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);

				InvoiceLine.ICMSTaxRegime = "6";
				Assert("ICMSBaseValueReductionPercentageReadOnly should be ReadOnly when ICMSTaxRegime = 6", InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);

				InvoiceLine.ICMSTaxRegime = "7";
				Assert("ICMSBaseValueReductionPercentageReadOnly should be ReadOnly when ICMSTaxRegime = 7", InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);

				InvoiceLine.ICMSTaxRegime = "8";
				Assert("ICMSBaseValueReductionPercentageReadOnly should be ReadOnly when ICMSTaxRegime = 8", InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);

				InvoiceLine.ICMSTaxRegime = "9";
				Assert("ICMSBaseValueReductionPercentageReadOnly should be ReadOnly when ICMSTaxRegime = 9", InvoiceLine.ICMSBaseValueReductionPercentageReadOnly);
			});
		}

		public void TestJI_ICMSTotalAmountReductionPercentage()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			CombineAssertions(() =>
			{
				AssertEquals("JI_ICMSTotalAmountReductionPercentage value should be 0", 0m, InvoiceLine.JI_ICMSTotalAmountReductionPercentage);

				InvoiceLine.ICMSTaxRegime = "4";
				InvoiceLine.JI_ICMSTotalAmountReductionPercentage = 5.12m;
				AssertEquals("JI_ICMSTotalAmountReductionPercentage value should be", 5.12m, InvoiceLine.JI_ICMSTotalAmountReductionPercentage);

				InvoiceLine.JI_ICMSTotalAmountReductionPercentage = 90m;
				AssertEquals("JI_ICMSTotalAmountReductionPercentage value should be", 90m, InvoiceLine.JI_ICMSTotalAmountReductionPercentage);

				InvoiceLine.ICMSTaxRegime = "1";
				AssertEquals("JI_ICMSTotalAmountReductionPercentage value should", 0m, InvoiceLine.JI_ICMSTotalAmountReductionPercentage);
			});
		}

		public void TestICMSTotalAmountReductionPercentageReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			foreach (var icmsTaxRegime in new ICMSTaxRegimeList().GetAllCodes().Except(new[] { ICMSTaxRegimeList.Codes.Reduction }))
			{
				InvoiceLine.ICMSTaxRegime = icmsTaxRegime;
				Assert($"JI_ICMSTotalAmountReductionPercentage should be ReadOnly when ICMSTaxRegime = {icmsTaxRegime}", InvoiceLine.ICMSTotalAmountReductionPercentageReadOnly);
			}

			InvoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			Assert("JI_ICMSTotalAmountReductionPercentage should NOT be ReadOnly when ICMSTaxRegime = 4", !InvoiceLine.ICMSTotalAmountReductionPercentageReadOnly);
		}

		public void TestIsICMSFeeCalculationApplicable()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			foreach (var icmsTaxRegime in new ICMSTaxRegimeList().GetAllCodes().Except(new[] { ICMSTaxRegimeList.Codes.FullCollection, ICMSTaxRegimeList.Codes.Reduction }))
			{
				InvoiceLine.ICMSTaxRegime = icmsTaxRegime;
				Assert($"IsICMSFeeCalculationApplicable should be FALSE when ICMSTaxRegime = {icmsTaxRegime}", !InvoiceLine.IsICMSFeeCalculationApplicable);
			}

			InvoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.FullCollection;
			Assert("IsICMSFeeCalculationApplicable should be TRUE when ICMSTaxRegime = 1", InvoiceLine.IsICMSFeeCalculationApplicable);

			InvoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			Assert("IsICMSFeeCalculationApplicable should be TRUE when ICMSTaxRegime = 4", InvoiceLine.IsICMSFeeCalculationApplicable);
		}

		public void TestFeesApportionment()
		{
			var newCurrency = CreateNewCurrency();

			Declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			InvoiceHeader.JZ_InvoiceNumber = "INV01";
			InvoiceHeader.JZ_InvoiceAmount = 200m;
			InvoiceHeader.JZ_IncoTerm = "CIF";
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "MDD";

			InvoiceLine.JI_LineNo = 1;
			InvoiceLine.JI_PartNo = "PROD01";
			InvoiceLine.JI_Tariff = "001";
			InvoiceLine.JI_Description = "PRODUCT DESCRIPTION";
			InvoiceLine.JI_LinePrice = 100m;
			InvoiceLine.JI_NetWeight = 50000m;
			InvoiceLine.JI_NetWeightUQ = "G";
			InvoiceLine.JI_InvoiceQuantity = 50m;
			InvoiceLine.JI_InvoiceUQ = "KG";
			InvoiceLine.JI_CustomsUnitQty = "KG";
			InvoiceLine.DutyTaxRegime = "1";
			InvoiceLine.IPITaxRegime = "1";
			InvoiceLine.PisCofinsTaxRegime = "1";
			InvoiceLine.ICMSTaxRegime = "1";
			InvoiceLine.JI_ICMSFormula = ICMSFormulaList.Codes.BCR;
			InvoiceLine.JI_ICMSRate = 5m;
			InvoiceLine.ICMSFCPRateValue = 1.5m;
			InvoiceLine.JI_ICMSBaseValueReductionPercentage = 2m;
			InvoiceLine.JI_ICMSTotalAmountReductionPercentage = 2.5m;

			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_PartNo = "PROD02";
			invoiceLine2.JI_Tariff = "001";
			invoiceLine2.JI_Description = "PRODUCT DESCRIPTION";
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_NetWeight = 150m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_InvoiceQuantity = 50m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.DutyTaxRegime = "1";
			invoiceLine2.IPITaxRegime = "1";
			invoiceLine2.PisCofinsTaxRegime = "1";
			invoiceLine2.ICMSTaxRegime = "1";
			invoiceLine2.JI_ICMSFormula = ICMSFormulaList.Codes.BCR;
			invoiceLine2.JI_ICMSRate = 5m;
			invoiceLine2.ICMSFCPRateValue = 1.5m;
			invoiceLine2.JI_ICMSBaseValueReductionPercentage = 2m;
			invoiceLine2.JI_ICMSTotalAmountReductionPercentage = 2.5m;

			var entryHeader = Declaration.ActiveEntryHeaders.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			InvoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			var feeTypes = new string[] { ChargeTypesList.Codes.DTY, RateTypes.IPI, RateTypes.PIS, RateTypes.Cofins, RateTypes.ICMS, RateTypes.Antidumping, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax,
												Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode, Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode,
												Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee };
			var baseValue = 1m;
			feeTypes.ForEach(feeType =>
			{
				CreateFeeAndSetValues(entryLine, feeType, baseValue++);
			});

			entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(invoiceLine =>
			{
				CombineAssertions(() =>
				{
					AssertEquals("JI_Calc_DutyBaseAmount", 11.1111m, invoiceLine.JI_Calc_DutyBaseAmount);
					AssertEquals("JI_Calc_DutyAmount", 1.1111m, invoiceLine.JI_Calc_DutyAmount);
					AssertEquals("JI_Calc_IPIBaseAmount", 22.2222m, invoiceLine.JI_Calc_IPIBaseAmount);
					AssertEquals("JI_Calc_IPIAmount", 2.2222m, invoiceLine.JI_Calc_IPIAmount);
					AssertEquals("JI_Calc_PISBaseAmount", 33.3333m, invoiceLine.JI_Calc_PISBaseAmount);
					AssertEquals("JI_Calc_PISAmount", 3.3333m, invoiceLine.JI_Calc_PISAmount);
					AssertEquals("JI_Calc_CofinsBaseAmount", 44.4444m, invoiceLine.JI_Calc_CofinsBaseAmount);
					AssertEquals("JI_Calc_CofinsAmount", 4.4444m, invoiceLine.JI_Calc_CofinsAmount);
					AssertEquals("JI_Calc_AntidumpingBaseAmount", 66.6666m, invoiceLine.JI_Calc_AntidumpingBaseAmount);
					AssertEquals("JI_Calc_AntidumpingAmount", 6.6666m, invoiceLine.JI_Calc_AntidumpingAmount);
					AssertEquals("JI_Calc_AfrmmAmount", invoiceLine.JI_NetWeight == 150m ? 5.833275m : 1.944425m, invoiceLine.JI_Calc_AfrmmAmount);
					AssertEquals("JI_Calc_ImportLicenseFineAmount", 18.8887m, invoiceLine.JI_Calc_ImportLicenseFineAmount);
					AssertEquals("JI_Calc_SiscomexUsageAmount", 11.1110m, invoiceLine.JI_Calc_SiscomexUsageAmount);
					AssertEquals("JI_Calc_ICMSBaseAmount", invoiceLine.JI_NetWeight == 150m ? 85.92335523m : 81.85300064m, invoiceLine.JI_Calc_ICMSBaseAmount);
					AssertEquals("JI_Calc_ICMSAmount", invoiceLine.JI_NetWeight == 150m ? 4.18876357m : 3.99033378m, invoiceLine.JI_Calc_ICMSAmount);
					AssertEquals("JI_Calc_FCPAmount", invoiceLine.JI_NetWeight == 150m ? 1.2888503284m : 1.2277950096m, invoiceLine.JI_Calc_FCPAmount);
				});
			});
		}

		void CreateFeeAndSetValues(CusEntryLine entryLine, string feeType, decimal baseValue)
		{
			var fee = entryLine.Fees.GetOrAddFeeByFeeType(feeType);
			fee.CF_BaseValue = baseValue * 11.1111m;
			fee.CF_ChargeAmount = baseValue * 1.1111m;
		}

		public void TestAllowParentTariffLineAndThisLineHavingDifferentHeader()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var invoiceHeader = declaration2.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();

			var invoiceHeader1 = declaration1.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();

			invoiceLine1.JI_ParentID = invoiceLine2.PK;
			invoiceLine1.JI_JZ = declaration1.PK;

			AssertEquals("JI_parentID shouldn't be cleared", invoiceLine1.JI_ParentID, invoiceLine2.PK);
		}

		public void TestSetConcurrencyPolicy_JI_ParentID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			factory.Load<JobComInvoiceLine>(invoiceLine.PK).JI_ParentID = ZGuid.NewZGuid();
			factory.Save();

			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());
			AssertEquals("Concurrency Policy for JI_ParentID should be Strict", ConcurrencyPolicy.Strict, invoiceLine.JI_ParentIDInfo.ConcurrencyPolicy);

			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestImportLicenseFeeType()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var importLicense = InvoiceLine.ImportLicenseSupportingInfo;
			AssertEquals("A new ImportLicense should be created", 1, InvoiceLine.ImportLicenseInfos.Count);
			AssertEquals(CusSupportingInfoTypeList.Codes.ImportLicense, importLicense.CSI_Type);

			InvoiceLine.ImportLicenseFeeType = "F1D5";
			AssertEquals("CSI_SubType should be", "F1D5", importLicense.CSI_SubType);

			InvoiceLine.ImportLicenseFeeType = "F1ND";
			AssertEquals("CSI_SubType should be", "F1ND", importLicense.CSI_SubType);
		}

		public void TestImportLicense_ReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			AssertEquals("ImportLicenseTypeInfo", true, InvoiceLine.ImportLicenseTypeInfo.ReadOnly);
			AssertEquals("ImportLicenseAuthorizationDateInfo", true, InvoiceLine.ImportLicenseAuthorizationDateInfo.ReadOnly);
			AssertEquals("ImportLicenseFeeTypeInfo", true, InvoiceLine.ImportLicenseFeeTypeInfo.ReadOnly);

			InvoiceLine.ImportLicenseNumber = "1234";
			InvoiceLine.ImportLicenseType = "1";
			InvoiceLine.ImportLicenseAuthorizationDate = new ZDateTime(2023, 01, 13);
			InvoiceLine.ImportLicenseFeeType = "F1D5";

			AssertEquals("ImportLicenseTypeInfo", false, InvoiceLine.ImportLicenseTypeInfo.ReadOnly);
			AssertEquals("ImportLicenseTypeInfo value", "1", InvoiceLine.ImportLicenseType);

			AssertEquals("ImportLicenseAuthorizationDateInfo", false, InvoiceLine.ImportLicenseAuthorizationDateInfo.ReadOnly);
			AssertEquals("ImportLicenseAuthorizationDateInfo value", new ZDateTime(2023, 01, 13), InvoiceLine.ImportLicenseAuthorizationDate);

			AssertEquals("ImportLicenseFeeTypeInfo", false, InvoiceLine.ImportLicenseFeeTypeInfo.ReadOnly);
			AssertEquals("ImportLicenseFeeTypeInfo value", "F1D5", InvoiceLine.ImportLicenseFeeType);

			InvoiceLine.ImportLicenseNumber = ZString.Empty;
			AssertEquals("ImportLicenseTypeInfo", true, InvoiceLine.ImportLicenseTypeInfo.ReadOnly);
			AssertEquals("ImportLicenseTypeInfo value", ZString.Empty, InvoiceLine.ImportLicenseType);

			AssertEquals("ImportLicenseAuthorizationDateInfo", true, InvoiceLine.ImportLicenseAuthorizationDateInfo.ReadOnly);
			AssertEquals("ImportLicenseAuthorizationDateInfo value", ZDateTime.Empty, InvoiceLine.ImportLicenseAuthorizationDate);

			AssertEquals("ImportLicenseFeeTypeInfo", true, InvoiceLine.ImportLicenseFeeTypeInfo.ReadOnly);
			AssertEquals("ImportLicenseFeeTypeInfo value", ZString.Empty, InvoiceLine.ImportLicenseFeeType);
		}

		public void TestInvoiceLineTariffDetachWhenDeleted()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "87";
			var tariffDetach = InvoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "AA";
			AssertEquals(1, InvoiceLine.TariffDetachs.Count);
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(CusCodeData));
			query.AddToFilter(CusCodeDataSchema.PK, tariffDetach.PK);
			query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, JobComInvoiceLineSchema.Constants.Prefix);
			query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.TariffDetach);
			var tariffsDetach = Factory.Load(typeof(CusCodeData), query);
			AssertEquals(1, tariffsDetach.Length);

			InvoiceLine.Delete();
			Factory.Save();
			tariffsDetach = Factory.Load(typeof(CusCodeData), query);
			AssertEquals(0, tariffsDetach.Length);
		}

		public void TestJI_CountryOfOriginReadonly()
		{
			foreach (var messageType in new[] { BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.ImportSiscomex })
			{
				Declaration.JE_MessageType = messageType;
				CombineAssertions($"JE_MessageType={messageType}", () =>
				{
					InvoiceLine.JI_ManufacturerIndicator = ZString.Empty;
					Assert("JI_CountryOfOrigin should be ReadOnly when is IMP and JI_ManufacturerIndicator = Empty", InvoiceLine.JI_CountryOfOriginInfo.ReadOnly);

					InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
					Assert("JI_CountryOfOrigin should be ReadOnly when is IMP and JI_ManufacturerIndicator = 1", InvoiceLine.JI_CountryOfOriginInfo.ReadOnly);

					InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
					Assert("JI_CountryOfOrigin should be ReadOnly when is IMP and JI_ManufacturerIndicator = 2", InvoiceLine.JI_CountryOfOriginInfo.ReadOnly);

					InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;
					Assert("JI_CountryOfOrigin should NOT be ReadOnly when is IMP and JI_ManufacturerIndicator = 3", !InvoiceLine.JI_CountryOfOriginInfo.ReadOnly);
				});
			}
		}

		public void TestEffectiveManufacturerAddressPK()
		{
			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerAddress1 = manufacturer1.Addresses.AddNew();
			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerAddress2 = manufacturer2.Addresses.AddNew();

			AssertEquals("EffectiveManufacturerAddressPK", ZGuid.Empty, InvoiceLine.EffectiveManufacturerAddressPK);
			InvoiceLine.EffectiveManufacturerAddressPK = manufacturerAddress1.PK;
			AssertEquals("EffectiveManufacturerAddressPK", manufacturerAddress1.PK, InvoiceLine.EffectiveManufacturerAddressPK);
			AssertEquals("JI_OA_ManufacturerAddress", manufacturer1.PK, InvoiceLine.JI_OA_ManufacturerAddress_ZAddress.OrgPK);
			AssertEquals("JI_OA_ManufacturerAddress_ZAddress", manufacturerAddress1.PK, InvoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("ManufacturerDocOrgPK", ZGuid.Empty, InvoiceLine.ManufacturerDocOrgPK);
			AssertEquals("ManufacturerDocAddressPK", ZGuid.Empty, InvoiceLine.ManufacturerDocAddressPK);

			InvoiceLine.EffectiveManufacturerAddressPK = ZGuid.Empty;
			AssertEquals("EffectiveManufacturerAddressPK", ZGuid.Empty, InvoiceLine.EffectiveManufacturerAddressPK);
			AssertEquals("JI_OA_ManufacturerAddress", ZGuid.Empty, InvoiceLine.JI_OA_ManufacturerAddress_ZAddress.OrgPK);
			AssertEquals("JI_OA_ManufacturerAddress_ZAddress", ZGuid.Empty, InvoiceLine.JI_OA_ManufacturerAddress);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("EffectiveManufacturerAddressPK", ZGuid.Empty, InvoiceLine.EffectiveManufacturerAddressPK);
			InvoiceLine.EffectiveManufacturerAddressPK = manufacturerAddress2.PK;
			AssertEquals("JI_OA_ManufacturerAddress", ZGuid.Empty, InvoiceLine.JI_OA_ManufacturerAddress_ZAddress.OrgPK);
			AssertEquals("JI_OA_ManufacturerAddress_ZAddress", ZGuid.Empty, InvoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("ManufacturerDocOrgPK", manufacturer2.PK, InvoiceLine.ManufacturerDocOrgPK);
			AssertEquals("ManufacturerDocAddressPK", manufacturerAddress2.PK, InvoiceLine.ManufacturerDocAddressPK);

			InvoiceLine.EffectiveManufacturerAddressPK = ZGuid.Empty;
			AssertEquals("EffectiveManufacturerAddressPK", ZGuid.Empty, InvoiceLine.EffectiveManufacturerAddressPK);
			AssertEquals("ManufacturerDocOrgPK", ZGuid.Empty, InvoiceLine.ManufacturerDocOrgPK);
			AssertEquals("ManufacturerDocAddressPK", ZGuid.Empty, InvoiceLine.ManufacturerDocAddressPK);
		}

		public void TestDefaultManufacturerAddressOnIndicatorChanged_ISW()
		{
			AssertDefaultManufacturerAddressOnIndicatorChanged(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestDefaultManufacturerAddressOnIndicatorChanged_LIC()
		{
			AssertDefaultManufacturerAddressOnIndicatorChanged(BRJobMessageTypeList.Codes.ImportLicense);
		}

		void AssertDefaultManufacturerAddressOnIndicatorChanged(ZString messageType)
		{
			var isLIC = messageType == BRJobMessageTypeList.Codes.ImportLicense;
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.OA_RN_NKCountryCode = "CA";
			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.OA_RN_NKCountryCode = "US";

			Declaration.JE_MessageType = messageType;

			AssertManufacturerAddress(ZString.Empty, ZGuid.Empty, ZGuid.Empty, true);

			InvoiceHeader.JZ_OA_SupplierAddress = supplierAddress.PK;
			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			AssertManufacturerAddress("US", supplier.PK, supplierAddress.PK, true);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			AssertManufacturerAddress(ZString.Empty, ZGuid.Empty, ZGuid.Empty, false);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			AssertManufacturerAddress("US", supplier.PK, supplierAddress.PK, true);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;
			AssertManufacturerAddress("US", ZGuid.Empty, ZGuid.Empty, true);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			AssertManufacturerAddress(ZString.Empty, ZGuid.Empty, ZGuid.Empty, false);

			void AssertManufacturerAddress(string countryCode, ZGuid manufacturerPK, ZGuid manufacturerAddressPK, bool readOnly)
			{
				CombineAssertions($"ManufacturerIndicator = {InvoiceLine.JI_ManufacturerIndicator}", () =>
				{
					if (isLIC)
					{
						AssertEquals("ManufacturerDocOrgPK", manufacturerPK, InvoiceLine.ManufacturerDocOrgPK);
						AssertEquals("ManufacturerDocAddressPK", manufacturerAddressPK, InvoiceLine.ManufacturerDocAddressPK);
					}
					else
					{
						AssertEquals("JI_OA_ManufacturerAddress", manufacturerPK, InvoiceLine.JI_OA_ManufacturerAddress_ZAddress.OrgPK);
						AssertEquals("JI_OA_ManufacturerAddress_ZAddress", manufacturerAddressPK, InvoiceLine.JI_OA_ManufacturerAddress);
					}
					AssertEquals("EffectiveManufacturerAddressPK", manufacturerAddressPK, InvoiceLine.EffectiveManufacturerAddressPK);
					AssertEquals("JI_CountryOfOrigin", countryCode, InvoiceLine.JI_CountryOfOrigin);
				});
			}
		}

		public void TestJI_SecondaryPreference()
		{
			Declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.JI_SecondaryPreference = "XXX";
			AssertEquals("JI_SecondaryPreference should be", "XXX", InvoiceLine.JI_SecondaryPreference);

			InvoiceLine.JI_SecondaryPreference = ZString.Empty;
			AssertEquals("JI_SecondaryPreference should be", ZString.Empty, InvoiceLine.JI_SecondaryPreference);
		}

		public void TestFormatTariffForSaving()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "1234.56.78";
			AssertEquals("Keep Numeric Only", "12345678", invoiceLine.JI_Tariff);
		}

		public void TestShouldReCalculateCustomsQtyOnLineQuantityChange()
		{
			var packConvertion = Factory.New<CusRefPacks>();
			packConvertion.RP_CustomsCountry = "BR";
			packConvertion.RP_Type = "CIP";
			packConvertion.RP_ConversionFactor = 2;
			packConvertion.RP_CustomsPack = "UN";
			packConvertion.RP_CommercialPack = "BBG";
			Factory.Save();

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_InvoiceQuantity = 100;
			InvoiceLine.JI_CustomsUnitQty = "UN";
			InvoiceLine.JI_InvoiceUQ = "BBG";

			AssertEquals("JI_CustomsQuantity should be the double of JI_InvoiceQuantity", 200m, InvoiceLine.JI_CustomsQuantity);

			InvoiceLine.JI_NFeItemNumber = "123";
			InvoiceLine.JI_InvoiceQuantity = 200;
			AssertEquals("JI_CustomsQuantity should have the same value", 200m, InvoiceLine.JI_CustomsQuantity);

			InvoiceLine.JI_NFeItemNumber = ZString.Empty;
			InvoiceLine.JI_NFeNumber = ZString.Empty;
			InvoiceLine.JI_InvoiceQuantity = 400;
			AssertEquals("JI_CustomsQuantity should be the double of JI_InvoiceQuantity", 800m, InvoiceLine.JI_CustomsQuantity);

			InvoiceLine.JI_NFeItemNumber = ZString.Empty;
			InvoiceLine.JI_NFeNumber = "123";
			InvoiceLine.JI_InvoiceQuantity = 200;
			AssertEquals("JI_CustomsQuantity should have the same value as before", 800m, InvoiceLine.JI_CustomsQuantity);

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			InvoiceLine.JI_NFeNumber = ZString.Empty;
			InvoiceLine.JI_CEI = entryInstruction.PK;
			InvoiceLine.JI_InvoiceQuantity = 200;
			AssertEquals("JI_CustomsQuantity should have the same value as before", 800m, InvoiceLine.JI_CustomsQuantity);

			entryInstruction.CEI_LegalDocument = "NFF";
			InvoiceLine.JI_InvoiceQuantity = 300;
			AssertEquals("JI_CustomsQuantity should be the double of JI_InvoiceQuantity", 600m, InvoiceLine.JI_CustomsQuantity);

			entryInstruction.CEI_LegalDocument = ZString.Empty;
			InvoiceLine.JI_InvoiceQuantity = 100;
			AssertEquals("JI_CustomsQuantity should be the double of JI_InvoiceQuantity", 200m, InvoiceLine.JI_CustomsQuantity);

			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			InvoiceLine.JI_NFeNumber = "123";
			InvoiceLine.JI_NFeItemNumber = "123";
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_InvoiceQuantity = 200;
			AssertEquals("JI_CustomsQuantity should be the double of JI_InvoiceQuantity", 400m, InvoiceLine.JI_CustomsQuantity);
		}

		public void TestNetWeightShouldReCalculateCustomsQtyOnLineQuantityChange()
		{
			var packConvertion = Factory.New<CusRefPacks>();
			packConvertion.RP_CustomsCountry = "BR";
			packConvertion.RP_Type = "CIP";
			packConvertion.RP_ConversionFactor = 2;
			packConvertion.RP_CustomsPack = "KG";
			packConvertion.RP_CommercialPack = "BBG";

			Factory.Save();

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_NetWeight = 100;
			InvoiceLine.JI_NetWeightUQ = "KG";
			InvoiceLine.JI_CustomsUnitQty = "KG";
			InvoiceLine.JI_InvoiceUQ = "BBG";
			AssertEquals("JI_CustomsQuantity should be", 100m, InvoiceLine.JI_CustomsQuantity);

			InvoiceLine.JI_NFeItemNumber = "123";
			InvoiceLine.JI_NetWeight = 200;
			AssertEquals("JI_CustomsQuantity should have the same value", 100m, InvoiceLine.JI_CustomsQuantity);

			InvoiceLine.JI_NFeItemNumber = ZString.Empty;
			InvoiceLine.JI_NFeNumber = ZString.Empty;
			InvoiceLine.JI_NetWeight = 400;
			AssertEquals("JI_CustomsQuantity should be", 400m, InvoiceLine.JI_CustomsQuantity);

			InvoiceLine.JI_NFeItemNumber = ZString.Empty;
			InvoiceLine.JI_NFeNumber = "123";
			InvoiceLine.JI_NetWeight = 200;
			AssertEquals("JI_CustomsQuantity should have the same value as before", 400m, InvoiceLine.JI_CustomsQuantity);

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			InvoiceLine.JI_NFeNumber = ZString.Empty;
			InvoiceLine.JI_CEI = entryInstruction.PK;
			InvoiceLine.JI_NetWeight = 200;
			AssertEquals("JI_CustomsQuantity should have the same value as before", 400m, InvoiceLine.JI_CustomsQuantity);

			entryInstruction.CEI_LegalDocument = "NFF";
			InvoiceLine.JI_NetWeight = 300;
			AssertEquals("JI_CustomsQuantity should be", 300m, InvoiceLine.JI_CustomsQuantity);

			entryInstruction.CEI_LegalDocument = ZString.Empty;
			InvoiceLine.JI_NetWeight = 100;
			AssertEquals("JI_CustomsQuantity should be", 100m, InvoiceLine.JI_CustomsQuantity);

			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			InvoiceLine.JI_NFeNumber = "123";
			InvoiceLine.JI_NFeItemNumber = "123";
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_NetWeight = 200;
			AssertEquals("JI_CustomsQuantity should be", 200m, InvoiceLine.JI_CustomsQuantity);
		}

		public void TestAdditionalTariffConcatenated()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("AdditionalTariffConcatenated should be", ZString.Empty, InvoiceLine.AdditionalTariffConcatenated);

			var additionalTariff = InvoiceLine.AdditionalTariffs.AddNew();
			additionalTariff.TariffCode = "111111_001";
			additionalTariff.ExNumber = "001";
			additionalTariff.TariffType = "XXXX";
			additionalTariff.LegalActType = "XX";
			additionalTariff.LegalActIssuingBody = "X";
			additionalTariff.LegalActNumber = "123";
			additionalTariff.LegalActYear = "2021";
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;

			additionalTariff = InvoiceLine.AdditionalTariffs.AddNew();
			additionalTariff.TariffCode = "111111_002";
			additionalTariff.ExNumber = "002";
			additionalTariff.TariffType = "YYYY";
			additionalTariff.LegalActType = "YY";
			additionalTariff.LegalActIssuingBody = "Y";
			additionalTariff.LegalActNumber = "456";
			additionalTariff.LegalActYear = "2021";
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;

			AssertEquals("AdditionalTariffConcatenated should be", $"1|002|YYYY|YY|Y|456|2021,3|001|XXXX|XX|X|123|2021", InvoiceLine.AdditionalTariffConcatenated);
		}

		public void TestPreviousDocumentConcatenated()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("PreviousDocumentConcatenated should be", ZString.Empty, InvoiceLine.PreviousDocumentConcatenated);

			var previousDoc1 = InvoiceLine.PreviousDocuments.AddNew();
			previousDoc1.CSI_Code = ImportSiscomexPreviousDocumentList.Codes.RE;
			previousDoc1.CSI_ReferenceNumber = "7";

			var previousDoc2 = InvoiceLine.PreviousDocuments.AddNew();
			previousDoc2.CSI_Code = ImportSiscomexPreviousDocumentList.Codes.DI;
			previousDoc2.CSI_ReferenceNumber = "8";

			var previousDoc3 = InvoiceLine.PreviousDocuments.AddNew();
			previousDoc3.CSI_Code = ImportSiscomexPreviousDocumentList.Codes.DI;
			previousDoc3.CSI_ReferenceNumber = "6";

			AssertEquals("PreviousDocumentConcatenated should be", $"DI|6,DI|8,RE|7", InvoiceLine.PreviousDocumentConcatenated);
		}

		public void TestMercosulForeignDeclarationConcatenated()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("MercosulForeignDeclarationConcatenated should be", ZString.Empty, InvoiceLine.MercosulForeignDeclarationConcatenated);

			var mercosulDec1 = InvoiceLine.MercosulForeignDeclarations.AddNew();
			mercosulDec1.CSI_Description = "023";
			mercosulDec1.CSI_ReferenceNumber2 = "046";
			mercosulDec1.CSI_ItemNumber = 7676;

			var mercosulDec2 = InvoiceLine.MercosulForeignDeclarations.AddNew();
			mercosulDec2.CSI_Description = "013";
			mercosulDec2.CSI_ReferenceNumber2 = "026";
			mercosulDec2.CSI_ItemNumber = 5656;

			AssertEquals("MercosulForeignDeclarationConcatenated should be", $"013|026|5656,023|046|7676", InvoiceLine.MercosulForeignDeclarationConcatenated);
		}

		public void TestAttachingImportLicenseLine()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC000001";

			var licInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			licInstruction.CEI_Description = "TEST";

			var licEntryHeader = licDeclaration.ActiveEntryHeaders.AddNew();
			licEntryHeader.CH_CEI_Instruction = licInstruction.PK;
			licEntryHeader.MovementReferenceNumberSetter("2000010001");

			var licInvoiceHeader = licDeclaration.Invoices.AddNew();
			licInvoiceHeader.JZ_InvoiceNumber = "LICINV";
			var licInvoiceLine = licInvoiceHeader.InvoiceLines.AddNew();
			licInvoiceLine.JI_CEI = licInstruction.PK;
			licInvoiceLine.JI_LineNo = 1;

			Declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licInstruction) });
			var licPivots = Declaration.AttachedImportLicenseEntries;
			AssertEquals("One LIC GenPivot should be created", 1, licPivots.Count);

			var iswInvoiceLineFromLic = Declaration.Invoices[0].InvoiceLines.Cast<JobComInvoiceLine>().First();
			AssertEquals("ISW Line's AttachedImportLicenseLine", licInvoiceLine.PK, iswInvoiceLineFromLic.AttachedImportLicenseLine.PK);
			AssertNull("ISW Line's AttachedImportSiscomexLine", iswInvoiceLineFromLic.AttachedImportSiscomexLine);
			AssertNull("LIC Line's AttachedImportSiscomexLine", licInvoiceLine.AttachedImportSiscomexLine);
			AssertNull("LIC Line's AttachedImportLicenseLine", licInvoiceLine.AttachedImportLicenseLine);

			AssertEquals("IsImportSiscomexWithGeneratedImportLicenseLine", false, iswInvoiceLineFromLic.IsImportSiscomexWithGeneratedImportLicenseLine);
			AssertEquals("IsImportLicenseGeneratedFromImportSiscomexLine", false, licInvoiceLine.IsImportLicenseGeneratedFromImportSiscomexLine);

			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			AssertEquals("IsImportSiscomexWithGeneratedImportLicenseLine", false, iswInvoiceLineFromLic.IsImportSiscomexWithGeneratedImportLicenseLine);
			AssertEquals("IsImportLicenseGeneratedFromImportSiscomexLine", false, licInvoiceLine.IsImportLicenseGeneratedFromImportSiscomexLine);

			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			licInvoiceLine.JI_LineNo = 2;
			AssertNull("AttachedImportLicenseLine", invoiceLine.AttachedImportLicenseLine);
			AssertEquals("IsImportSiscomexWithGeneratedImportLicenseLine", false, iswInvoiceLineFromLic.IsImportSiscomexWithGeneratedImportLicenseLine);
			AssertEquals("IsImportLicenseGeneratedFromImportSiscomexLine", false, licInvoiceLine.IsImportLicenseGeneratedFromImportSiscomexLine);

			InvoiceLine.JI_ParentID = Factory.New<JobComInvoiceLine>().PK;
			AssertNull(InvoiceLine.AttachedImportLicenseLine);
			AssertNull(InvoiceLine.AttachedImportSiscomexLine);
		}

		public void TestGeneratingImportLicenseLine()
		{
			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC_DEC";

			var iswDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			iswDeclaration.JE_DeclarationReference = "ISW_DEC";
			var iswInstruction = iswDeclaration.CustomsEntryInstructions.AddNew();
			iswInstruction.CEI_Description = "ISW_TEST";
			var iswInvHeader = iswDeclaration.Invoices.AddNew();
			var iswInvLine = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine.JI_CEI = iswInstruction.PK;
			var iswEntryHeader = iswDeclaration.ActiveEntryHeaders.AddNew();
			iswEntryHeader.CH_JE = iswDeclaration.PK;
			iswEntryHeader.CH_CEI_Instruction = iswInstruction.PK;
			var iswEntryLine = iswEntryHeader.MergedLines.AddNew();
			iswInvLine.JI_CL = iswEntryLine.PK;

			var generator = new GenerateImportLicenseObject(iswEntryLine);
			generator.ImportLicenseDeclarationPK = licDeclaration.PK;
			generator.GenerateImportLicense();
			var licInvLine = licDeclaration.InvoiceLines[0];

			CombineAssertions(() =>
			{
				AssertEquals("ISW Line's AttachedImportLicenseLine", licInvLine.PK, iswInvLine.AttachedImportLicenseLine.PK);
				AssertNull("ISW Line's AttachedImportSiscomexLine", iswInvLine.AttachedImportSiscomexLine);
				AssertNull("LIC Line's AttachedImportLicenseLine", licInvLine.AttachedImportLicenseLine);
				AssertEquals("LIC Line's AttachedImportSiscomexLine", iswInvLine, licInvLine.AttachedImportSiscomexLine);

				AssertEquals("LIC Line's IsImportSiscomexWithGeneratedImportLicenseLine", false, licInvLine.IsImportSiscomexWithGeneratedImportLicenseLine);
				AssertEquals("LIC Line's IsImportLicenseGeneratedFromImportSiscomexLine", true, licInvLine.IsImportLicenseGeneratedFromImportSiscomexLine);
				AssertEquals("LIC Line's JI_CEI", true, licInvLine.JI_CEIInfo.ReadOnly);

				AssertEquals("ISW Line's IsImportSiscomexWithGeneratedImportLicenseLine", true, iswInvLine.IsImportSiscomexWithGeneratedImportLicenseLine);
				AssertEquals("ISW Line's IsImportLicenseGeneratedFromImportSiscomexLine", false, iswInvLine.IsImportLicenseGeneratedFromImportSiscomexLine);
			});
		}

		public void TestCopyingImportLienseLine()
		{
			var licDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration1.JE_DeclarationReference = "LIC_DEC";
			var licInvoice1 = licDeclaration1.Invoices.AddNew();
			var licInvLine1 = licInvoice1.InvoiceLines.AddNew();

			var licDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var licInvoice2 = licDeclaration2.Invoices.AddNew();
			new BRJobComInvoiceHeaderCopyBO(licInvoice1, licInvoice2, new JobComInvoiceHeaderCopyOptions()).CopyInvoice();
			var licInvLine2 = licInvoice2.InvoiceLines[0];

			CombineAssertions(() =>
			{
				AssertNull("LIC Line 1's AttachedImportLicenseLine", licInvLine1.AttachedImportLicenseLine);
				AssertNull("LIC Line 1's AttachedImportSiscomexLine", licInvLine1.AttachedImportSiscomexLine);
				AssertEquals("LIC Line 2's AttachedImportLicenseLine", licInvLine1.PK, licInvLine2.AttachedImportLicenseLine.PK);
				AssertNull("LIC Line 2's AttachedImportSiscomexLine", licInvLine2.AttachedImportSiscomexLine);

				AssertEquals("LIC Line 1's IsImportSiscomexWithGeneratedImportLicenseLine", false, licInvLine1.IsImportSiscomexWithGeneratedImportLicenseLine);
				AssertEquals("LIC Line 1's IsImportLicenseGeneratedFromImportSiscomexLine", false, licInvLine1.IsImportLicenseGeneratedFromImportSiscomexLine);

				AssertEquals("LIC Line 2's IsImportSiscomexWithGeneratedImportLicenseLine", false, licInvLine2.IsImportSiscomexWithGeneratedImportLicenseLine);
				AssertEquals("LIC Line 2's IsImportLicenseGeneratedFromImportSiscomexLine", false, licInvLine2.IsImportLicenseGeneratedFromImportSiscomexLine);
			});
		}

		public void TestClearTaxDetailsWhenTariffCodeChanges()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "123456789";

			var taxDuty = InvoiceLine.AdditionalTariffs.AddNew();
			taxDuty.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			taxDuty.TariffType = ChildTariffTypeList.Codes.LEBIT;
			var taxIPI = InvoiceLine.AdditionalTariffs.AddNew();
			taxIPI.LegalActSubject = AdditionalTaxTypeList.Codes.ExIPITariff;
			taxIPI.TariffType = ChildTariffTypeList.Codes.IPI;

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			InvoiceLine.DutyRateIsOverridden = true;
			InvoiceLine.IPIRateIsOverridden = true;
			InvoiceLine.PisRateIsOverridden = true;
			InvoiceLine.CofinsRateIsOverridden = true;

			var specalCaseTax = InvoiceLine.SpecialCaseTaxes.AddNew();
			specalCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			specalCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

			CombineAssertions(() =>
			{
				AssertEquals("AdditionalTariffs should have 2 elements", 2, InvoiceLine.AdditionalTariffs.Count);
				AssertEquals("JI_PrimaryPreference must be EXTARIFF", Constants.RatePreferenceType.ExTariff, InvoiceLine.JI_PrimaryPreference);
				Assert("DutyRateIsOverridden should be TRUE", InvoiceLine.DutyRateIsOverridden);
				Assert("IPIRateIsOverridden should be TRUE", InvoiceLine.IPIRateIsOverridden);
				Assert("PisRateIsOverridden should be TRUE", InvoiceLine.PisRateIsOverridden);
				Assert("CofinsRateIsOverridden should be TRUE", InvoiceLine.CofinsRateIsOverridden);
				AssertEquals("SpecialCaseTaxes should have 1 elements", 1, InvoiceLine.SpecialCaseTaxes.Count);
			});

			InvoiceLine.JI_Tariff = "000000000";
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalTariffs should be removed", 0, InvoiceLine.AdditionalTariffs.Count);
				AssertEquals("JI_PrimaryPreference must be NORMAL", Constants.RatePreferenceType.Normal, InvoiceLine.JI_PrimaryPreference);
				Assert("DutyRateIsOverridden should be FALSE", !InvoiceLine.DutyRateIsOverridden);
				Assert("IPIRateIsOverridden should be FALSE", !InvoiceLine.IPIRateIsOverridden);
				Assert("PisRateIsOverridden should be FALSE", !InvoiceLine.PisRateIsOverridden);
				Assert("CofinsRateIsOverridden should be FALSE", !InvoiceLine.CofinsRateIsOverridden);
				AssertEquals("SpecialCaseTaxes should be removed", 0, InvoiceLine.SpecialCaseTaxes.Count);
			});
		}

		public void TestDeleteRowsFromAdditionalTariffs()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceLine.JI_Tariff = "123456789";
			Assert("AdditionalTariffs must NOT contain any object", !InvoiceLine.AdditionalTariffs.Any());

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			AssertNotNull("AdditionalTariffs must contain ExDutyTariff", InvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff));
			AssertNull("AdditionalTariffs must NOT contain TariffAgreement", InvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement));

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
			AssertNull("AdditionalTariffs must NOT contain ExDutyTariff", InvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff));
			AssertNotNull("AdditionalTariffs must contain TariffAgreement", InvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement));

			InvoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.Normal;
			Assert("AdditionalTariffs must NOT contain any object", !InvoiceLine.AdditionalTariffs.Any());
		}

		public void TestIAdditionalTariffParent()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.JI_Tariff = "12345678";
			var additionalTariff = InvoiceLine.AdditionalTariffs.AddNew();
			AssertSame(InvoiceLine, additionalTariff.Parent);
			AssertEquals("Tariff", "12345678", additionalTariff.Parent.Tariff);
			AssertEquals("EffectiveAssessmentDate", InvoiceLine.EffectiveAssessmentDate, additionalTariff.Parent.EffectiveAssessmentDate);
		}

		public void TestResetJI_ManufacturerIndicator()
		{
			var supplierA = OrgHeader.New(Factory);
			var supplierB = OrgHeader.New(Factory);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceHeader.JZ_OA_SupplierAddress = supplierA.MainAddress.PK;

			var invoice2 = Declaration.Invoices.AddNew();
			invoice2.JZ_OA_SupplierAddress = supplierA.MainAddress.PK;

			var invoice3 = Declaration.Invoices.AddNew();
			invoice3.JZ_OA_SupplierAddress = supplierB.MainAddress.PK;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;

			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;

			invoiceLine2.JI_JZ = InvoiceHeader.PK;
			AssertEquals("JI_ManufacturerIndicator must NOT be reseted", ManufacturerIndicatorList.Codes._1, invoiceLine2.JI_ManufacturerIndicator);

			invoiceLine3.JI_JZ = InvoiceHeader.PK;
			AssertEquals("JI_ManufacturerIndicator must be reseted", ZString.Empty, invoiceLine3.JI_ManufacturerIndicator);
		}

		public void TestManufacturerOrgCode()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var supplierA = OrgHeader.New(Factory);
			supplierA.OH_Code = "TEST1";

			var invoice2 = Declaration.Invoices.AddNew();
			invoice2.JZ_OA_SupplierAddress = supplierA.MainAddress.PK;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ManufacturerIndicator = "2";
			invoiceLine2.ManufacturerDocAddress.OrganisationPK = supplierA.PK;

			var supplierB = OrgHeader.New(Factory);
			supplierB.OH_Code = "TEST2";

			var invoice3 = Declaration.Invoices.AddNew();
			invoice3.JZ_OA_SupplierAddress = supplierB.MainAddress.PK;

			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_ManufacturerIndicator = "2";
			invoiceLine3.JI_CountryOfOrigin = "AU";
			invoiceLine3.JI_OA_ManufacturerAddress = supplierB.MainAddress.PK;

			AssertEquals("ManufacturerOrgCode", ZString.Empty, InvoiceLine.ManufacturerOrgCode);
			AssertEquals("ManufacturerOrgCode", "TEST1", invoiceLine2.ManufacturerOrgCode);
			AssertEquals("ManufacturerOrgCode", ZString.Empty, invoiceLine3.ManufacturerOrgCode);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			AssertEquals("ManufacturerOrgCode", ZString.Empty, InvoiceLine.ManufacturerOrgCode);
			AssertEquals("ManufacturerOrgCode", ZString.Empty, invoiceLine2.ManufacturerOrgCode);
			AssertEquals("ManufacturerOrgCode", "TEST2", invoiceLine3.ManufacturerOrgCode);
		}

		public void TestJI_ManufacturerIndicatorReadOnly()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "TEST1";
			supplier.Addresses.AddNew();

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			Assert(InvoiceLine.JI_ManufacturerIndicatorInfo.ReadOnly);
			Assert(InvoiceLine.JI_ManufacturerIndicator.IsEmpty);

			InvoiceHeader.JZ_OA_SupplierAddress = supplier.Addresses[0].PK;
			InvoiceLine.JI_ManufacturerIndicator = "1";
			Assert(!InvoiceLine.JI_ManufacturerIndicatorInfo.ReadOnly);
			AssertEquals("1", InvoiceLine.JI_ManufacturerIndicator);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceHeader.SupplierDocAddressPK = ZGuid.Invalid;
			Assert(InvoiceLine.JI_ManufacturerIndicatorInfo.ReadOnly);
			Assert(InvoiceLine.JI_ManufacturerIndicator.IsEmpty);

			InvoiceHeader.SupplierDocAddressPK = supplier.Addresses[0].PK;
			InvoiceLine.JI_ManufacturerIndicator = "1";
			Assert(!InvoiceLine.JI_ManufacturerIndicatorInfo.ReadOnly);
			AssertEquals("1", InvoiceLine.JI_ManufacturerIndicator);
		}

		public void TestCanDeleteAndReasonForNotAbleToDelete_AttachedToImportLicense()
		{
			var reasonForNotAbleToDelete = "The Invoice Line cannot be deleted, because there is Import License line reference it.";

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				Assert("Can Delete when not attached to any LIC lines", ((ICanDelete)InvoiceLine).CanDelete);
				AssertNotEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)InvoiceLine).ReasonForNotAbleToDelete);
			});

			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC000001";
			var invoice = licDeclaration.Invoices.AddNew();
			new BRJobComInvoiceHeaderCopyBO(InvoiceHeader, invoice, null).CopyInvoice();

			foreach (var messageType in new[] { BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.Export })
			{
				Declaration.JE_MessageType = messageType;
				CombineAssertions(messageType, () =>
				{
					Assert("Can NOT Delete when attached to any LIC lines", !((ICanDelete)InvoiceLine).CanDelete);
					AssertEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)InvoiceLine).ReasonForNotAbleToDelete);
				});
			}

			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				Assert("Can Delete for when attached to any IMP lines", ((ICanDelete)InvoiceLine).CanDelete);
				AssertNotEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)InvoiceLine).ReasonForNotAbleToDelete);
			});
		}

		public void TestCanDeleteAndReasonForNotAbleToDelete_GeneratedImportLicense()
		{
			var reasonForNotAbleToDelete = "The License Line can not be deleted, because it is attached to an Import Entry.";

			var iswDeclaration = Factory.New<JobDeclaration>();
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC000001";
			var licInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			var licInvoice = licDeclaration.Invoices.AddNew();
			var licInvLine = licInvoice.InvoiceLines.AddNew();
			licInvLine.JI_CEI = licInstruction.PK;

			CombineAssertions(() =>
			{
				Assert("Can Delete when not attached to any lines", ((ICanDelete)licInvLine).CanDelete);
				AssertNotEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)licInvLine).ReasonForNotAbleToDelete);
			});

			iswDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licInstruction) });

			CombineAssertions(() =>
			{
				Assert("Can Delete when attached to ISW Line", ((ICanDelete)licInvLine).CanDelete);
				AssertNotEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)licInvLine).ReasonForNotAbleToDelete);
			});

			var iswInstruction = iswDeclaration.CustomsEntryInstructions[0];
			var iswInvLine = iswDeclaration.InvoiceLines[0];
			var iswEntryHeader = iswDeclaration.ActiveEntryHeaders.AddNew();
			iswEntryHeader.CH_JE = iswDeclaration.PK;
			iswEntryHeader.CH_CEI_Instruction = iswInstruction.PK;
			var iswEntryLine = iswEntryHeader.MergedLines.AddNew();
			iswInvLine.JI_CL = iswEntryLine.PK;

			licDeclaration.InvoiceLines.RemoveAndDeleteAll();
			var generator = new GenerateImportLicenseObject(iswEntryLine);
			generator.ImportLicenseDeclarationPK = licDeclaration.PK;
			generator.GenerateImportLicense();
			var generatedLicInvLine = licDeclaration.InvoiceLines[0];

			CombineAssertions(() =>
			{
				Assert("Can NOT Delete when generated from ISW Line", !((ICanDelete)generatedLicInvLine).CanDelete);
				AssertEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)generatedLicInvLine).ReasonForNotAbleToDelete);
			});

			iswDeclaration.DetachImportLicense(licDeclaration.CustomsEntryInstructions);

			CombineAssertions(() =>
			{
				Assert("Can Delete when detached to ISW Line", ((ICanDelete)generatedLicInvLine).CanDelete);
				AssertNotEquals("ReasonForNotAbleToDelete", reasonForNotAbleToDelete, ((ICanDelete)generatedLicInvLine).ReasonForNotAbleToDelete);
			});
		}

		public void TestFullGoodsDescription()
		{
			InvoiceLine.JI_Tariff = "123456789";

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals(3900, InvoiceLine.FullGoodsDescriptionInfo.MaxLength);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals(3900, InvoiceLine.FullGoodsDescriptionInfo.MaxLength);

			var goodsDescriptionNote = new HiddenTextNote(InvoiceLine, PredefinedNoteTypes.Instance.DeclarationGoodsDescription.Description);

			AssertEquals("FullGoodsDescription", 0, InvoiceLine.FullGoodsDescription.Length);
			AssertEquals("JI_Description", 0, InvoiceLine.JI_Description.Length);
			AssertEquals("DeclarationGoodsDescription Note", 0, goodsDescriptionNote.Text.Length);

			var fullDescription = new string('X', InvoiceLine.JI_DescriptionInfo.MaxLength);
			InvoiceLine.FullGoodsDescription = fullDescription;
			AssertEquals("FullGoodsDescription", fullDescription, InvoiceLine.FullGoodsDescription);
			AssertEquals("JI_Description", JobComInvoiceLine.Schema.JI_DescriptionMaxLength, InvoiceLine.JI_Description.Length);
			AssertEquals("DeclarationGoodsDescription Note", 0, goodsDescriptionNote.Text.Length);

			fullDescription = new string('X', InvoiceLine.FullGoodsDescriptionInfo.MaxLength);
			InvoiceLine.FullGoodsDescription = fullDescription;
			AssertEquals("FullGoodsDescription", fullDescription, InvoiceLine.FullGoodsDescription);
			AssertEquals("JI_Description", JobComInvoiceLine.Schema.JI_DescriptionMaxLength, InvoiceLine.JI_Description.Length);
			AssertEquals("DeclarationGoodsDescription Note", InvoiceLine.FullGoodsDescriptionInfo.MaxLength - JobComInvoiceLine.Schema.JI_DescriptionMaxLength, goodsDescriptionNote.Text.Length);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals(525, InvoiceLine.JI_DescriptionInfo.MaxLength);

			fullDescription = new string('X', JobComInvoiceLine.Schema.JI_DescriptionMaxLength + 1);
			AssertExceptionThrown<MaxLengthExceededException>(() => InvoiceLine.FullGoodsDescription = fullDescription);
			ErrorReporter.Clear();
		}

		public void TestFMMBenefit()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.FMM);
			CombineAssertions(() =>
			{
				Assert("FMMBenefit must be empty", InvoiceLine.FMMBenefit.IsEmpty);
				AssertNull("TaxRegime CusSupportingInfo must be empty", taxRegime);
			});

			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			InvoiceLine.FMMBenefit = FMMBenefitTypeList.Codes.Exemption;

			taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.FMM);
			CombineAssertions(() =>
			{
				AssertEquals("FMMBenefit must be equal to", FMMBenefitTypeList.Codes.Exemption, InvoiceLine.FMMBenefit);
				AssertEquals("FMMBenefitDescription must be equal to", FMMBenefitTypeList.Descriptions.Exemption, InvoiceLine.FMMBenefitDescription);
				AssertEquals("CSI_Type must be equal to TXR", CusSupportingInfoTypeList.Codes.TaxRegime, taxRegime.CSI_Type);
				AssertEquals("CSI_SubType must be equal to 8", TaxRegimeTypeList.Codes.FMM, taxRegime.CSI_SubType);
				AssertEquals("CSI_Code must be equal to E", "E", taxRegime.CSI_Code);
			});

			taxRegime.CSI_Code = "N";
			CombineAssertions(() =>
			{
				AssertEquals("FMMBenefit must be equal to N", "N", InvoiceLine.FMMBenefit);
				AssertEquals("FMMBenefitDescription must be equal to", ZString.Empty, InvoiceLine.FMMBenefitDescription);
			});
		}

		public void TestFMMBenefit_ReadOnly()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Assert("FMMBenefit must be readonly", InvoiceLine.FMMBenefitInfo.ReadOnly);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Assert("FMMBenefit must NOT be readonly", !InvoiceLine.FMMBenefitInfo.ReadOnly);

			Declaration.JE_TransportMode = TransportTypeList.Codes.River;
			Assert("FMMBenefit must NOT be readonly", !InvoiceLine.FMMBenefitInfo.ReadOnly);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			Assert("FMMBenefit must NOT be readonly", !InvoiceLine.FMMBenefitInfo.ReadOnly);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			Assert("FMMBenefit must be readonly", InvoiceLine.FMMBenefitInfo.ReadOnly);
		}

		public void TestInvoiceLineMessageType()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			CombineAssertions("When JE_MessageType = Export", () =>
			{
				Assert("IsExport", InvoiceLine.IsExport);
				Assert("IsImport", !InvoiceLine.IsImport);
				Assert("IsImportSiscomex", !InvoiceLine.IsImportSiscomex);
				Assert("IsImportLicense", !InvoiceLine.IsImportLicense);
				Assert("IsImportExcludingLicense", !InvoiceLine.IsImportExcludingLicense);
				Assert("IsLPCO", !InvoiceLine.IsLPCO);
				Assert("IsImportOnly", !InvoiceLine.IsImportOnly);
			});

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			CombineAssertions("When JE_MessageType = ImportSiscomex", () =>
			{
				Assert("IsExport", !InvoiceLine.IsExport);
				Assert("IsImport", InvoiceLine.IsImport);
				Assert("IsImportSiscomex", InvoiceLine.IsImportSiscomex);
				Assert("IsImportLicense", !InvoiceLine.IsImportLicense);
				Assert("IsImportExcludingLicense", InvoiceLine.IsImportExcludingLicense);
				Assert("IsLPCO", !InvoiceLine.IsLPCO);
				Assert("IsImportOnly", !InvoiceLine.IsImportOnly);
			});

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			CombineAssertions("When JE_MessageType = ImportLicense", () =>
			{
				Assert("IsExport", !InvoiceLine.IsExport);
				Assert("IsImport", InvoiceLine.IsImport);
				Assert("IsImportSiscomex", !InvoiceLine.IsImportSiscomex);
				Assert("IsImportLicense", InvoiceLine.IsImportLicense);
				Assert("IsImportExcludingLicense", !InvoiceLine.IsImportExcludingLicense);
				Assert("IsLPCO", !InvoiceLine.IsLPCO);
				Assert("IsImportOnly", !InvoiceLine.IsImportOnly);
			});

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			CombineAssertions("When JE_MessageType = Import", () =>
			{
				Assert("IsExport", !InvoiceLine.IsExport);
				Assert("IsImport", InvoiceLine.IsImport);
				Assert("IsImportSiscomex", !InvoiceLine.IsImportSiscomex);
				Assert("IsImportLicense", !InvoiceLine.IsImportLicense);
				Assert("IsImportExcludingLicense", InvoiceLine.IsImportExcludingLicense);
				Assert("IsLPCO", !InvoiceLine.IsLPCO);
				Assert("IsImportOnly", InvoiceLine.IsImportOnly);
			});

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			CombineAssertions("When JE_MessageType = LPCO", () =>
			{
				Assert("IsExport", !InvoiceLine.IsExport);
				Assert("IsImport", !InvoiceLine.IsImport);
				Assert("IsImportSiscomex", !InvoiceLine.IsImportSiscomex);
				Assert("IsImportLicense", !InvoiceLine.IsImportLicense);
				Assert("IsImportExcludingLicense", !InvoiceLine.IsImportExcludingLicense);
				Assert("IsLPCO", InvoiceLine.IsLPCO);
				Assert("IsImportOnly", !InvoiceLine.IsImportOnly);
			});

			var invoice = Factory.New<JobComInvoiceHeader>();
			InvoiceLine.JI_JZ = invoice.PK;

			invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
			CombineAssertions("When JZ_MessageType = Export", () =>
			{
				Assert("IsExport", InvoiceLine.IsExport);
				Assert("IsImport", !InvoiceLine.IsImport);
				Assert("IsImportSiscomex", !InvoiceLine.IsImportSiscomex);
				Assert("IsImportLicense", !InvoiceLine.IsImportLicense);
				Assert("IsImportExcludingLicense", !InvoiceLine.IsImportExcludingLicense);
				Assert("IsLPCO", !InvoiceLine.IsLPCO);
				Assert("IsImportOnly", !InvoiceLine.IsImportOnly);
			});

			invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			CombineAssertions("When JZ_MessageType = Import", () =>
			{
				Assert("IsExport", !InvoiceLine.IsExport);
				Assert("IsImport", InvoiceLine.IsImport);
				Assert("IsImportSiscomex", !InvoiceLine.IsImportSiscomex);
				Assert("IsImportLicense", !InvoiceLine.IsImportLicense);
				Assert("IsImportExcludingLicense", InvoiceLine.IsImportExcludingLicense);
				Assert("IsLPCO", !InvoiceLine.IsLPCO);
				Assert("IsImportOnly", InvoiceLine.IsImportOnly);
			});
		}

		public void TestPropertiesToExcludeFromCloning()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceHeader.JZ_InvoiceNumber = "AAA";

			InvoiceLine.JI_LineNo = 1;
			InvoiceLine.JI_ParentID = new ZGuid();
			InvoiceLine.JI_ParentTableCode = "JI";

			var declarationCloned = (JobDeclaration)Declaration.TemplateCopy();
			var invoiceCloned = declarationCloned.Invoices[0];
			var linecloned = invoiceCloned.InvoiceLines[0];

			AssertEquals("JI_ParentID is empty", ZGuid.Empty, linecloned.JI_ParentID);
			AssertEquals("JI_ParentTableCode is empty", ZString.Empty, linecloned.JI_ParentTableCode);
		}

		public void TestRequiresImportLicense()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.ImportLicenseNumber = "0123456789";
			Assert(invoiceLine.JI_RequiresImportLicense);
			Assert(invoiceLine.JI_RequiresImportLicenseInfo.ReadOnly);

			invoiceLine.ImportLicenseNumber = ZString.Empty;
			Assert(!invoiceLine.JI_RequiresImportLicense);
			Assert(!invoiceLine.JI_RequiresImportLicenseInfo.ReadOnly);

			invoiceLine.JI_RequiresImportLicense = true;
			Assert(invoiceLine.JI_RequiresImportLicense);
			Assert(!invoiceLine.JI_RequiresImportLicenseInfo.ReadOnly);
		}

		public void TestIPIRateIsOverridden_OnIPITaxRegimeChanged()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			Assert(!InvoiceLine.IPIRateIsOverridden);

			InvoiceLine.IPIRateIsOverridden = true;
			InvoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Reduction;
			Assert(!InvoiceLine.IPIRateIsOverridden);
		}

		public void TestReducedRateOnIPITaxRegimeChanged()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.IPI);
			CombineAssertions(() =>
			{
				Assert("IPITaxRegime must be empty", InvoiceLine.IPITaxRegime.IsEmpty);
				AssertNull("TaxRegime CusSupportingInfo must be empty", taxRegime);
			});

			InvoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Exemption;

			taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.IPI);

			AssertNull(InvoiceLine.Taxes.FindByType(Constants.RateCodes.IPI));

			InvoiceLine.IPIRateIsOverridden = true;
			InvoiceLine.IPIVigentRateValue = 15m;

			var ipiTaxRate = InvoiceLine.Taxes.FindByType(Constants.RateCodes.IPI);

			CombineAssertions(() =>
			{
				AssertEquals("JLT_MethodOfCalculation", SpecialCaseTaxTypeList.Codes.AdValoremRate, ipiTaxRate.JLT_MethodOfCalculation);
				AssertEquals("JLT_Rate", 15m, ipiTaxRate.JLT_Rate);
			});

			InvoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Suspension;
			CombineAssertions(() =>
			{
				Assert("IPI Tax Rate should be deleted", !ipiTaxRate.IsDeleted);
				AssertEquals("IPIRateIsOverridden", true, InvoiceLine.IPIRateIsOverridden);
				AssertEquals("IPIVigentRateValue", 15m, InvoiceLine.IPIVigentRateValue);
			});

			InvoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Reduction;

			CombineAssertions(() =>
			{
				Assert("IPIRateIsOverridden should be cleared", !InvoiceLine.IPIRateIsOverridden);
				Assert("IPIVigentRateValue should be readonly", InvoiceLine.IPIVigentRateValueReadOnly);
				AssertEquals("IPIVigentRateValue should be cleared", 0m, InvoiceLine.IPIVigentRateValue);

				Assert("IPI Tax Rate should be deleted", ipiTaxRate.IsDeleted);
				AssertEquals("One SpecialCaseTax added", 1, InvoiceLine.SpecialCaseTaxes.Count);

				var specialCaseTax = InvoiceLine.SpecialCaseTaxes[0];
				AssertEquals("SpecialCaseTax.TaxGroup", Constants.RateCodes.IPI, specialCaseTax.TaxGroup);
				AssertEquals("SpecialCaseTax.TaxType", SpecialCaseTaxTypeList.Codes.Reduced, specialCaseTax.TaxType);
				AssertEquals("SpecialCaseTax.RateOrUnitValue", 0m, specialCaseTax.RateOrUnitValue);
				AssertEquals("JLT_MethodOfCalculation", SpecialCaseTaxTypeList.Codes.Reduced, specialCaseTax.InvoiceLineTax.JLT_MethodOfCalculation);
			});

			InvoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.NonTaxable;
			Assert("IPI Tax Rate should be deleted", ipiTaxRate.IsDeleted);
		}

		public void TestPisCofinsRateIsOverridden_OnTaxRegimeChanged()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.FullCollection;
			Assert(!InvoiceLine.PisRateIsOverridden);
			Assert(!InvoiceLine.CofinsRateIsOverridden);

			InvoiceLine.PisRateIsOverridden = true;
			InvoiceLine.CofinsRateIsOverridden = true;
			InvoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Reduction;

			Assert(!InvoiceLine.PisRateIsOverridden);
			Assert(!InvoiceLine.CofinsRateIsOverridden);
		}

		public void TestReducedRateOnPisCofinsTaxRegimeChanged()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.PisCofins);
			CombineAssertions(() =>
			{
				Assert("IPITaxRegime must be empty", InvoiceLine.PisCofinsTaxRegime.IsEmpty);
				AssertNull("TaxRegime CusSupportingInfo must be empty", taxRegime);
			});

			InvoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.FullCollection;

			taxRegime = InvoiceLine.TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.PisCofins);

			AssertNull(InvoiceLine.Taxes.FindByType(Constants.RateCodes.PIS));
			AssertNull(InvoiceLine.Taxes.FindByType(Constants.RateCodes.Cofins));

			InvoiceLine.PisRateIsOverridden = true;
			InvoiceLine.PisVigentRateValue = 15m;

			InvoiceLine.CofinsRateIsOverridden = true;
			InvoiceLine.CofinsVigentRateValue = 15m;

			var pisTaxRate = InvoiceLine.Taxes.FindByType(Constants.RateCodes.PIS);
			var cofinsTaxRate = InvoiceLine.Taxes.FindByType(Constants.RateCodes.Cofins);

			CombineAssertions(() =>
			{
				AssertEquals("JLT_MethodOfCalculation", SpecialCaseTaxTypeList.Codes.AdValoremRate, pisTaxRate.JLT_MethodOfCalculation);
				AssertEquals("JLT_Rate", 15m, pisTaxRate.JLT_Rate);

				AssertEquals("JLT_MethodOfCalculation", SpecialCaseTaxTypeList.Codes.AdValoremRate, cofinsTaxRate.JLT_MethodOfCalculation);
				AssertEquals("JLT_Rate", 15m, cofinsTaxRate.JLT_Rate);
			});

			InvoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.PaymentMade;

			CombineAssertions(() =>
			{
				Assert("PIS Tax Rate should be deleted", !pisTaxRate.IsDeleted);
				Assert("Cofins Tax Rate should be deleted", !cofinsTaxRate.IsDeleted);
				AssertEquals("PisVigentRateValue", 15m, InvoiceLine.PisVigentRateValue);
				AssertEquals("CofinsVigentRateValue", 15m, InvoiceLine.CofinsVigentRateValue);
			});

			InvoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Reduction;

			CombineAssertions(() =>
			{
				Assert("PisRateIsOverridden should be cleared", !InvoiceLine.PisRateIsOverridden);
				Assert("PisVigentRateValue should be readonly", InvoiceLine.PisVigentRateValueReadOnly);
				AssertEquals("PisVigentRateValue should be cleared", 0m, InvoiceLine.PisVigentRateValue);

				Assert("CofinsRateIsOverridden should be cleared", !InvoiceLine.CofinsRateIsOverridden);
				Assert("CofinsVigentRateValue should be readonly", InvoiceLine.CofinsVigentRateValueReadOnly);
				AssertEquals("CofinsVigentRateValue should be cleared", 0m, InvoiceLine.CofinsVigentRateValue);

				Assert("IPI Tax Rate should be deleted", pisTaxRate.IsDeleted);
				Assert("IPI Tax Rate should be deleted", cofinsTaxRate.IsDeleted);
				AssertEquals("2 SpecialCaseTaxes added", 2, InvoiceLine.SpecialCaseTaxes.Count);

				var specialCaseTaxPIS = InvoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().FirstOrDefault(x => x.TaxGroup == Constants.RateCodes.PIS);
				AssertEquals("PIS SpecialCaseTax.TaxType", SpecialCaseTaxTypeList.Codes.Reduced, specialCaseTaxPIS.TaxType);
				AssertEquals("PIS SpecialCaseTax.RateOrUnitValue", 0m, specialCaseTaxPIS.RateOrUnitValue);
				AssertEquals("PIS JLT_MethodOfCalculation", SpecialCaseTaxTypeList.Codes.Reduced, specialCaseTaxPIS.InvoiceLineTax.JLT_MethodOfCalculation);

				var specialCaseTaxcofins = InvoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().FirstOrDefault(x => x.TaxGroup == Constants.RateCodes.Cofins);
				AssertEquals("PIS SpecialCaseTax.TaxType", SpecialCaseTaxTypeList.Codes.Reduced, specialCaseTaxcofins.TaxType);
				AssertEquals("PIS SpecialCaseTax.RateOrUnitValue", 0m, specialCaseTaxcofins.RateOrUnitValue);
				AssertEquals("PIS JLT_MethodOfCalculation", SpecialCaseTaxTypeList.Codes.Reduced, specialCaseTaxcofins.InvoiceLineTax.JLT_MethodOfCalculation);
			});

			InvoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Exemption;

			Assert("PIS Tax Rate should be deleted", pisTaxRate.IsDeleted);
			Assert("Cofins Tax Rate should be deleted", cofinsTaxRate.IsDeleted);
		}

		public void TestICMSFCPRateValue()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceLine.ICMSFCPRateValue = 10m;
			AssertEquals("ICMSFCPRateValue should be 10", 10m, InvoiceLine.ICMSFCPRateValue);

			var tax = InvoiceLine.Taxes.FindByType(Constants.RateCodes.ICMSFCP);
			AssertNotNull("ICMSFCPRateValue should NOT be null", tax);
			AssertEquals("JLT_Rate", 10m, tax.JLT_Rate);
			AssertEquals("JLT_MethodOfCalculation", "%", tax.JLT_MethodOfCalculation);

			InvoiceLine.ICMSFCPRateValue = 0m;
			AssertNull("ICMSFCPRateValue should be null", InvoiceLine.Taxes.FindByType(Constants.RateCodes.ICMSFCP));
		}

		public void TestPermits()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			Factory.New<Permit>().CSI_ParentID = InvoiceLine.PK;
			Factory.New<Permit>().CSI_ParentID = InvoiceLine.PK;
			AssertEquals("Permits count should be", 2, InvoiceLine.Permits.Count);

			Factory.Save();
			AssertEquals("Permits should be saved when job is IMP", 2, InvoiceLine.Permits.Count);

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Factory.Save();
			AssertEquals("Permits should be deleted when job is not IMP", 0, InvoiceLine.Permits.Count);
		}

		public void TestQuantityPerUnitInfos()
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			InvoiceLine.QuantityPerUnitInfos.AddNew();
			InvoiceLine.QuantityPerUnitInfos.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("QuantityPerUnitInfos count should be", 2, InvoiceLine.QuantityPerUnitInfos.Count);
				AssertEquals("CSI_Type should be", CusSupportingInfoTypeList.Codes.QuantityPerUnit, InvoiceLine.QuantityPerUnitInfos[0].CSI_Type);
				AssertEquals("CSI_Type should be", CusSupportingInfoTypeList.Codes.QuantityPerUnit, InvoiceLine.QuantityPerUnitInfos[1].CSI_Type);
			});
		}

		public void TestDefaultGoodsCatalogProperties()
		{
			var goodsCatalog = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			goodsCatalog.CGC_AuthorityVersion = "1";
			goodsCatalog.CGC_AuthorityIdentifier = "123";
			goodsCatalog.CGC_Tariff = "11111";

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CGC_Catalog = goodsCatalog.PK;

			AssertEquals("JI_CatalogAuthorityIdentifier should be", "123", InvoiceLine.JI_CatalogAuthorityIdentifier);
			AssertEquals("JI_CatalogAuthorityVersion should be", "1", InvoiceLine.JI_CatalogAuthorityVersion);
			AssertEquals("JI_CatalogAuthorityVersion should be", "1", InvoiceLine.JI_CatalogAuthorityVersion);
			AssertEquals("JI_Tariff should be", "11111", InvoiceLine.JI_Tariff);

			InvoiceLine.JI_CGC_Catalog = ZGuid.Empty;

			AssertEquals("JI_CatalogAuthorityIdentifier should be empty", ZString.Empty, InvoiceLine.JI_CatalogAuthorityIdentifier);
			AssertEquals("JI_CatalogAuthorityVersion should be empty", ZString.Empty, InvoiceLine.JI_CatalogAuthorityVersion);
			AssertEquals("JI_Tariff should not be changed", "11111", InvoiceLine.JI_Tariff);
		}

		public void TestJI_CatalogAuthorityIdentifierReadOnly()
		{
			Assert("JI_CatalogAuthorityIdentifier should be read only", InvoiceLine.JI_CatalogAuthorityIdentifierInfo.ReadOnly);
		}

		public void TestJI_CatalogAuthorityVersionReadOnly()
		{
			Assert("JI_CatalogAuthorityVersion should be read only", InvoiceLine.JI_CatalogAuthorityVersionInfo.ReadOnly);
		}

		public void TestGoodsCatalogPropertiesAreClearedWhenProductIsCleared()
		{
			var goodsCatalog = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			goodsCatalog.CGC_AuthorityVersion = "1";
			goodsCatalog.CGC_AuthorityIdentifier = "123";
			goodsCatalog.CGC_Tariff = "11111111";

			foreach (var messageType in new[] { BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.Export })
			{
				Declaration.JE_MessageType = messageType;

				InvoiceLine.JI_PartNo = "20";
				InvoiceLine.JI_CGC_Catalog = goodsCatalog.PK;

				AssertEquals("JI_CGC_Catalog should be", goodsCatalog.PK, InvoiceLine.JI_CGC_Catalog);
				AssertEquals("JI_CatalogAuthorityIdentifier should be", "123", InvoiceLine.JI_CatalogAuthorityIdentifier);
				AssertEquals("JI_CatalogAuthorityVersion should be", "1", InvoiceLine.JI_CatalogAuthorityVersion);

				InvoiceLine.JI_PartNo = "60";

				AssertEquals("JI_CGC_Catalog should be", ZGuid.Empty, InvoiceLine.JI_CGC_Catalog);
				AssertEquals("BR_CatalogAuthorityIdentifier should be", ZString.Empty, InvoiceLine.JI_CatalogAuthorityIdentifier);
				AssertEquals("BR_CatalogAuthorityVersion should be", ZString.Empty, InvoiceLine.JI_CatalogAuthorityVersion);
				AssertEquals("JI_Tariff should be", "11111111", InvoiceLine.JI_Tariff);
			}
		}

		public void TestGoodsCatalogPropertiesCaption()
		{
			CombineAssertions(() =>
			{
				AssertEquals($"{nameof(InvoiceLine.JI_CatalogAuthorityIdentifier)} caption", "Catalog Authority", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CatalogAuthorityIdentifierInfo).Caption);
				AssertEquals($"{nameof(InvoiceLine.JI_CatalogAuthorityVersion)} caption", "Catalog Version", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CatalogAuthorityVersionInfo).Caption);
			});
		}

		public void TestAddAndRemoveDuimpTaxRegimes_FromZZData()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				Factory.Save();

				InvoiceLine.JI_Tariff = "12345678";
				InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				AssertContainsExactElementsInAnyOrder(new[] { "P01" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT1_1" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
				InvoiceLine.DuimpLegalBase = "P02";
				InvoiceLine.AddDuimpTaxRegimes();
				AssertContainsExactElementsInAnyOrder(new[] { "P01", "P02", "P02" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT1_1", "ATT2", "ATT3", "ATT12", "ATT12_1", "ATT13", "ATT13_1" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
				InvoiceLine.DuimpLegalBase = "P04";
				InvoiceLine.AddDuimpTaxRegimes();
				AssertContainsExactElementsInAnyOrder(new[] { "P01", "P02", "P02", "P04" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT1_1", "ATT2", "ATT3", "ATT12", "ATT12_1", "ATT13", "ATT13_1" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
				InvoiceLine.RemoveDuimpTaxRegimes(InvoiceLine.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "P04"));
				AssertContainsExactElementsInAnyOrder(new[] { "P01", "P02", "P02" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT1_1", "ATT2", "ATT3", "ATT12", "ATT12_1", "ATT13", "ATT13_1" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
				InvoiceLine.RemoveDuimpTaxRegimes(InvoiceLine.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "P02"));
				AssertContainsExactElementsInAnyOrder(new[] { "P01" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT1_1" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
				InvoiceLine.DuimpLegalBase = "P06";
				InvoiceLine.AddDuimpTaxRegimes();
				AssertContainsExactElementsInAnyOrder(new[] { "P01", "P06" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT1_1", "ATT10", "ATT10_1", "ATT10_2", "ATT11", "ATT11_1", "ATT11_11" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
				InvoiceLine.RemoveDuimpTaxRegimes(InvoiceLine.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "P06"));
				AssertContainsExactElementsInAnyOrder(new[] { "P01" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT1_1" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
			}
		}

		public void TestAddAndRemoveDuimpTaxRegimes_FromMessage()
		{
			var date = ZDateTime.Now;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010101", date.AddDays(-5), date.AddDays(5));
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "02020202", date.AddDays(-5), date.AddDays(5));

			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationReference = $"01010101|CN|{date:yyyyMMdd}";
			message.EM_MessageNum = "1";
			message.EM_MessageType = MessageTypeList.Codes.RTT;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json").Replace("\"dataFatoGerador\": \"2023-04-17\"", $"\"dataFatoGerador\": \"{date.ToISO8601ShortDateString()}\"");

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Declaration.JE_ValuationDate = date.Date;
			message.EM_LinkedObject = Declaration;
			Factory.Save();

			InvoiceLine.JI_Tariff = "01010101";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.Label));

			InvoiceLine.DuimpLegalBase = "0006";
			InvoiceLine.AddDuimpTaxRegimes();
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1", "1" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100", "0006" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins, Constants.RateTypes.ImportDuty }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715", "ATT_15574" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002", "Ex Tarifário II" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.Label));

			InvoiceLine.DuimpLegalBase = "0007";
			InvoiceLine.AddDuimpTaxRegimes();
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1", "1", "4" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100", "0006", "0007" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins, Constants.RateTypes.ImportDuty, Constants.RateTypes.ImportDuty }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715", "ATT_15574", "ATT_2874", "ATT_13743" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002", "Ex Tarifário II", "NCM SH 2002", "Observação do Acordo Comercial" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.Label));

			InvoiceLine.RemoveDuimpTaxRegimes(InvoiceLine.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "0006"));
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1", "4" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100", "0007" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins, Constants.RateTypes.ImportDuty }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715", "ATT_2874", "ATT_13743" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002", "NCM SH 2002", "Observação do Acordo Comercial" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.Label));

			InvoiceLine.RemoveDuimpTaxRegimes(InvoiceLine.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "0007"));
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100" }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins }, InvoiceLine.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002" }, InvoiceLine.TaxRegimeAttributes.Select(s => s.Label));

			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "01010101";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1" }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100" }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715" }, invoiceLine2.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002" }, invoiceLine2.TaxRegimeAttributes.Select(s => s.Label));

			invoiceLine2.DuimpLegalBase = "0006";
			invoiceLine2.AddDuimpTaxRegimes();
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1", "1" }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100", "0006" }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins, Constants.RateTypes.ImportDuty }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715", "ATT_15574" }, invoiceLine2.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002", "Ex Tarifário II" }, invoiceLine2.TaxRegimeAttributes.Select(s => s.Label));

			invoiceLine2.DuimpLegalBase = "0008";
			invoiceLine2.AddDuimpTaxRegimes();
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1", "1", "4" }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100", "0006", "0008" }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins, Constants.RateTypes.ImportDuty, Constants.RateTypes.IPI }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715", "ATT_15574" }, invoiceLine2.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002", "Ex Tarifário II" }, invoiceLine2.TaxRegimeAttributes.Select(s => s.Label));

			invoiceLine2.RemoveDuimpTaxRegimes(invoiceLine2.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "0008"));
			AssertContainsExactElementsInAnyOrder("CSI_Code", new[] { "1", "1", "1" }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_Code));
			AssertContainsExactElementsInAnyOrder("CSI_Procedure", new[] { "1100", "1100", "0006" }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_Procedure));
			AssertContainsExactElementsInAnyOrder("CSI_SubType", new[] { Constants.RateTypes.PIS, Constants.RateTypes.Cofins, Constants.RateTypes.ImportDuty }, invoiceLine2.DuimpTaxRegimes.Select(s => s.CSI_SubType));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes CY_Code", new[] { "ATT_13741", "ATT_13715", "ATT_15574" }, invoiceLine2.TaxRegimeAttributes.Select(s => s.CY_Code));
			AssertContainsExactElementsInAnyOrder("TaxRegimeAttributes Label", new[] { "Uso autopeças", "Anexo II da Lei 10.485/2002", "Ex Tarifário II" }, invoiceLine2.TaxRegimeAttributes.Select(s => s.Label));

			invoiceLine2.JI_Tariff = "02020202";
			AssertEquals("Should not contain DuimpTaxRegimes", 0, invoiceLine2.DuimpTaxRegimes.Count);
			AssertEquals("Should not contain TaxRegimeAttributes", 0, invoiceLine2.TaxRegimeAttributes.Count);
		}

		public void TestGetRequiredTTProfiles_FromZZData()
		{
			var dbProfiles = ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				invoiceLine.JI_Tariff = "12345678";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;

				var profiles = invoiceLine.GetRequiredTTProfiles().Select(x => x.RefCusProfile).ToArray();
				AssertEquals(11, profiles.Length);
				AssertContainsExactElementsInAnyOrder(dbProfiles.Where(w => w.XX0_TariffCode == "12345678" && w.XX0_QuestionCode != "ATT8" && w.XX0_QuestionCode != "ATT9"), profiles);

				invoiceLine.JI_Tariff = "87654321";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Angola;

				profiles = invoiceLine.GetRequiredTTProfiles().Select(x => x.RefCusProfile).ToArray();
				AssertContainsExactElementsInAnyOrder(dbProfiles.Where(w => w.XX0_TariffCode.StartsWith("87654")), profiles);

				invoiceLine.JI_Tariff = "87654321";
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;

				profiles = invoiceLine.GetRequiredTTProfiles().Select(x => x.RefCusProfile).ToArray();
				AssertEquals(0, profiles.Length);

				invoiceLine.JI_Tariff = "12345678";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;

				Assert(invoiceLine.GetRequiredTTProfiles(true).All(p => p.IsMandatory));
				Assert(invoiceLine.GetRequiredTTProfiles(false).All(p => !p.IsMandatory));
			}
		}

		public void TestGetRequiredTTProfiles_FromMessage()
		{
			var date = ZDateTime.Now;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010101", date.AddDays(-5), date.AddDays(5));
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "02020202", date.AddDays(-5), date.AddDays(5));

			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_ApplicationReference = $"01010101|CN|{date:yyyyMMdd}";
			message.EM_MessageNum = "1";
			message.EM_MessageType = MessageTypeList.Codes.RTT;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json").Replace("\"dataFatoGerador\": \"2023-04-17\"", $"\"dataFatoGerador\": \"{date.ToISO8601ShortDateString()}\"");

			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Declaration.JE_ValuationDate = date.Date;
			message.EM_LinkedObject = Declaration;
			Factory.Save();

			InvoiceLine.JI_Tariff = "01010101";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var tariffProfiles = InvoiceLine.GetRequiredTTProfiles();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("LegalCode", new[] { "0006", "0007", "0007", "0008", "0009", "0009", "1100", "1100", "1100", "1100" }, tariffProfiles.Select(s => s.LegalCode));
				AssertContainsExactElementsInAnyOrder("QuestionCode", new[] { "ATT_15574", "ATT_2874", "ATT_13743", "ATT_2872", "ATT_13743", "ATT_13741", "ATT_13741", "ATT_13715", "ATT_13715", "ATT_13715" }, tariffProfiles.Select(s => s.QuestionCode));
			});

			InvoiceLine.JI_Tariff = "02020202";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(0, InvoiceLine.GetRequiredTTProfiles().Length);

			InvoiceLine.JI_Tariff = "01010101";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Macau;
			AssertEquals(0, InvoiceLine.GetRequiredTTProfiles().Length);
		}

		public void TestAttributes()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "00000000";
				AssertEquals("Attributes Count", 0, invoiceLine.Attributes.Count);

				invoiceLine.JI_Tariff = "87654321";
				AssertEquals("Attributes Count", 8, invoiceLine.Attributes.Count);

				Factory.Save();
				var newInvoiceLine = new BusinessObjectFactory().Load<JobComInvoiceLine>(invoiceLine.PK);
				AssertEquals("Attributes Count", 8, newInvoiceLine.Attributes.Count);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				Factory.Save();
				AssertEquals("Attributes Count", 0, invoiceLine.Attributes.Count);
			}
		}

		public void TestGetAttributes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertSame("When Type is ATT, should return Attributes", invoiceLine.Attributes, invoiceLine.GetAttributes(CusCodeDataTypeList.Codes.Attribute));
				AssertSame("When Type is TRA, should return TaxRegimeAttributes", invoiceLine.TaxRegimeAttributes, invoiceLine.GetAttributes(CusCodeDataTypeList.Codes.TaxRegimeAttribute));
				AssertNull("When Type is not ATT or TRA, should return null", invoiceLine.GetAttributes(CusCodeDataTypeList.Codes.NVE));
			});
		}

		public void TestCatalogLinkedByLocalPartNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ORGTEST";
			orgHeader.OH_FullName = "TEST ORG";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = orgHeader.PK;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = ZString.Empty;

			var goodsCatalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog1.CGC_Tariff = "56049000";
			goodsCatalog1.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			goodsCatalog1.CGC_OH_Owner = orgHeader.PK;
			var localPartNumber1 = goodsCatalog1.LocalPartNumbers.AddNew();
			localPartNumber1.CGI_Reference = "UTPART1";
			Factory.Save();
			invoiceLine.JI_PartNo = "UTPART1";
			AssertEquals("GoodsCatalog should be set", goodsCatalog1.PK, invoiceLine.JI_CGC_Catalog);
			AssertEquals("Tariff should be set", "56049000", invoiceLine.JI_Tariff);

			invoiceLine.JI_PartNo = ZString.Empty;
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "UTPART2";
			var goodsCatalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog2.CGC_Tariff = "72058000";
			goodsCatalog2.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			goodsCatalog2.CGC_OH_Owner = orgHeader.PK;
			var pivot = part1.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "72058000";
			var localPartNumber2 = goodsCatalog2.LocalPartNumbers.AddNew();
			localPartNumber2.CGI_Reference = "UTPART2";
			Factory.Save();
			invoiceLine.JI_PartNo = "UTPART2";

			AssertEquals("GoodsCatalog should be set", goodsCatalog2.PK, invoiceLine.JI_CGC_Catalog);
			AssertEquals("Tariff should be set", "72058000", invoiceLine.JI_Tariff);

			invoiceLine.JI_PartNo = ZString.Empty;
			var goodsCatalog3 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog3.CGC_Tariff = "56049000";
			goodsCatalog3.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			goodsCatalog3.CGC_OH_Owner = orgHeader.PK;
			var localPartNumber3 = goodsCatalog3.LocalPartNumbers.AddNew();
			localPartNumber3.CGI_Reference = "UTPART1";

			Factory.Save();
			invoiceLine.JI_PartNo = "UTPART1";
			AssertEquals("GoodsCatalog should be null when having more then one Catalog with same local part number", ZGuid.Empty, invoiceLine.JI_CGC_Catalog);
			AssertEquals("Tariff shouldn't be set", "72058000", invoiceLine.JI_Tariff);
		}

		public void TestPopulateValuesFromForeignOperator()
		{
			var importer1 = Factory.New<OrgHeader>();
			var importer2 = Factory.New<OrgHeader>();
			var manufacturer1 = Factory.New<OrgHeader>();
			var manufacturer2 = Factory.New<OrgHeader>();

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = importer1.PK;
			foreignOperator.BFR_OH_ForeignOperator = manufacturer1.PK;
			foreignOperator.BFR_AuthorityIdentifier = "OPE_1";
			foreignOperator.BFR_AuthorityVersion = "1";

			var invoiceLine = Factory.New<JobComInvoiceHeader>().InvoiceLines.AddNew();
			AssertNull("Declaration is null", invoiceLine.ForeignOperator);

			var declaration = Factory.New<JobDeclaration>();
			invoiceLine.InvoiceHeader.JZ_JE = declaration.PK;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer1.PK;
			AssertNull("ForeignOperator", invoiceLine.ForeignOperator);

			invoiceLine.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			AssertEquals("ForeignOperator", foreignOperator, invoiceLine.ForeignOperator);
			AssertEquals("JI_ManufacturerAuthorityIdentifier", foreignOperator.BFR_AuthorityIdentifier, invoiceLine.JI_ManufacturerAuthorityIdentifier);
			AssertEquals("JI_ManufacturerAuthorityVersion", foreignOperator.BFR_AuthorityVersion, invoiceLine.JI_ManufacturerAuthorityVersion);

			invoiceLine.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			AssertNull("ForeignOperator", invoiceLine.ForeignOperator);
			AssertEquals("JI_ManufacturerAuthorityIdentifier", ZString.Empty, invoiceLine.JI_ManufacturerAuthorityIdentifier);
			AssertEquals("JI_ManufacturerAuthorityVersion", ZString.Empty, invoiceLine.JI_ManufacturerAuthorityVersion);

			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertNull("ForeignOperator", invoiceLine.ForeignOperator);
			AssertEquals("JI_ManufacturerAuthorityIdentifier", ZString.Empty, invoiceLine.JI_ManufacturerAuthorityIdentifier);
			AssertEquals("JI_ManufacturerAuthorityVersion", ZString.Empty, invoiceLine.JI_ManufacturerAuthorityVersion);

			declaration.JE_OH_Importer = importer2.PK;
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			AssertNull("ForeignOperator", invoiceLine.ForeignOperator);
			AssertEquals("JI_ManufacturerAuthorityIdentifier", ZString.Empty, invoiceLine.JI_ManufacturerAuthorityIdentifier);
			AssertEquals("JI_ManufacturerAuthorityVersion", ZString.Empty, invoiceLine.JI_ManufacturerAuthorityVersion);
		}

		public void TestMarkApportionmentDirty_NetWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();

			declaration.ApportionmentDirty = false;
			AssertEquals("PreCondition:Apportionement is not dirty", false, declaration.ApportionmentDirty);

			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.NetWeight;

			declaration.ApportionmentDirty = false;
			AssertEquals("PreCondition:Apportionement is not dirty", false, declaration.ApportionmentDirty);
			line.JI_NetWeight = 10m;
			AssertEquals("Apportionment is dirty now as there is a charge distributed by net weight", true, declaration.ApportionmentDirty);

			declaration.ApportionmentDirty = false;
			AssertEquals("PreCondition:Apportionement is not dirty", false, declaration.ApportionmentDirty);
			line.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("Apportionment is dirty now as there is a charge distributed by net weight", true, declaration.ApportionmentDirty);

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.FOB;
			declaration.ApportionmentDirty = false;
			AssertEquals("PreCondition:Apportionement is not dirty", false, declaration.ApportionmentDirty);
			line.JI_NetWeight = 20m;
			AssertEquals("Apportionment is not mark dirty as there is no charge distributed by net weight", false, declaration.ApportionmentDirty);
			line.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Apportionment is not mark dirty as there is no charge distributed by net weight", false, declaration.ApportionmentDirty);
		}

		RefCurrency CreateNewCurrency()
		{
			var currency = RefCurrency.New(Factory);
			currency.RX_Code = "MDD";
			currency.SetCustomsRate(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), 2.1111m);
			return currency;
		}

		#region Implementation

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		}

		void AssertCusLineTariffDetail(CusLineTariffDetail tariffDetail, ZString type, ZString tariff)
		{
			AssertEquals("tariffDetail.BZ_Type", type, tariffDetail.BZ_Type);
			AssertEquals("tariffDetail.BZ_Tariff", tariff, tariffDetail.BZ_Tariff);
		}

		#endregion

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}
}
