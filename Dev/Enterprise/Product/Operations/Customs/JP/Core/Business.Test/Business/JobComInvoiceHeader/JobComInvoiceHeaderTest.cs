using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestDefaultValues()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertEquals("A is the default value", RepresentativeInvoiceTypes.Codes.A, invoiceHeader.JZ_InvoiceType);
			AssertEquals("ZDateTime.Empty should be the default value for JZ_ValuationDateOverride", ZDateTime.Empty, invoiceHeader.JZ_ValuationDateOverride);

			invoiceHeader.JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.C;
			AssertEquals(RepresentativeInvoiceTypes.Codes.C, invoiceHeader.JZ_InvoiceType);
			Factory.Save();

			var headerCopy = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoiceHeader.PK);
			AssertEquals("Not the default after saved with a new value", RepresentativeInvoiceTypes.Codes.C, headerCopy.JZ_InvoiceType);
		}

		public void TestIncoTermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(invoice.NeedToGetNewIncoTermAndChargeFactory);
			AssertType<ExportIncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(invoice.NeedToGetNewIncoTermAndChargeFactory);
			AssertType<ImportIncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);
		}

		public void TestEffectiveValuationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(1);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = ZDateTime.Today;
			AssertEquals("Base class logics should be used.", ZDateTime.Today, invoice.EffectiveValuationDate);

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals("Should get date from the linked entry instructions.", ZDateTime.Today.AddDays(1), invoice.EffectiveValuationDate);
		}

		public void TestJZ_InvoiceAmountDecimalPlaces()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var info = invoice.JZ_InvoiceAmountInfo;
			invoice.JZ_RX_NKInvoice_Currency = "TWD";
			AssertHasDecimalPlacesAttribute(info, 2);

			invoice.JZ_RX_NKInvoice_Currency = "JPY";
			AssertHasDecimalPlacesAttribute(info, 0);
		}

		public void TestDeclarationTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = JPExportDeclarationTypeList.Codes.G;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = JPExportDeclarationTypeList.Codes.G;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			AssertEquals(1, invoiceHeader.DeclarationTypes.Count);

			entryInstruction2.CEI_Style = JPExportDeclarationTypeList.Codes.T;
			AssertEquals(2, invoiceHeader.DeclarationTypes.Count);

			invoiceLine2.JI_CEI = ZGuid.Empty;
			AssertEquals(1, invoiceHeader.DeclarationTypes.Count);
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Japan;

		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.NewWithValidTestData<JobDeclaration>();
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		#endregion
	}

	#region JobComInvoiceHeaderFunctionalTest

	sealed class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		public new void TestReapportionGroupChargeWhenItsOwnChargeDeleted()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				BaseJobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_InvoiceAmount = 180848.58m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				groupHeader.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 1000, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("One apportioned Charge", 1, invoice.GroupCharges.Count);

				invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 1000);
				declaration.ResumeApportionment();
				AssertEquals("no apportioned Charge", 0, invoice.GroupCharges.Count);

				invoice.Charges.RemoveAndDeleteAll();
				declaration.ResumeApportionment();
				AssertEquals("One apportioned Charge", 1, invoice.GroupCharges.Count);
			}
		}

		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}

	#endregion

	#region JobComInvoiceHeaderApportionTest

	sealed class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		public new void TestApportionOverseasFreight()
		{
			Assert("OFT is no longer distributable by value.", true);
		}

		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}

	#endregion

	#region JobComInvoiceHeaderCalculationTest

	sealed class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		public override void TestCalculateFOBValueWithDDP()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = "DDP";
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.LandingCharges, 200);

			header.JZ_InvoiceAmount = 10600;
			var expectedFOB = new ZDecimal(10600 - 10 - 100 - 200);
			AssertEquals("FOB Value / DTD ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public override void TestCalculateFOBValueWithCIF()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = "CIF";
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.LandingCharges, 200);
			header.JZ_InvoiceAmount = 10600;

			var expectedFOB = new ZDecimal(10600 - 10 - 100);
			AssertEquals("FOB Value / CIF ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public override void TestCalculateFOBValueWithCFR()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100);

			header.JZ_InvoiceAmount = 10600;
			var expectedFOB = new ZDecimal(10600 - 100);
			AssertEquals("FOB Value / CFR ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public override void TestCalculateFOBValueWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = "FOB";
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100);

			var expectedFOB = new ZDecimal(10600);
			AssertEquals("FOB Value / FOB ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public new void TestBalanceGroupHeaderChargeAndHeaderCharge()
		{
			groupHeader.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 100, groupHeader.JobDeclaration.LocalCurrencyCode);

			header.JZ_InvoiceAmount = 1000;
			header.JZ_RX_NKInvoice_Currency = groupHeader.JobDeclaration.LocalCurrencyCode;

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 200);

			var expected = header.JZ_InvoiceAmount;
			declaration.ResumeApportionment();
			AssertEquals("The balance should be Invoice Total less cost of its own", expected, header.JZ_Calc_Balance);
		}

		public new void TestCalculateBalanceWithInvalidCurrencyOnInlandFreight()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JobComInvoiceLines.AddNew();

			header.JZ_InvoiceAmount = 10000;
			declaration.ResumeApportionment();
			AssertEquals("Initial Balance", new ZDecimal(10000), header.JZ_Calc_Balance);

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 1000);
			declaration.ResumeApportionment();
			AssertEquals("Balance after adding Inland Freight (defaulted currency)", new ZDecimal(10000), header.JZ_Calc_Balance);
		}

		public override void TestCalculateCIFWithCIP()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			PrepareCharge(header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ExWorks, 100));
			PrepareCharge(header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Discount, 1));
			var preOTH = header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			PrepareCharge(preOTH);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			header.JZ_IncoTerm = "CIP";
			var expected = 10600m;
			declaration.ResumeApportionment();
			AssertEquals("CIF value / CIP ", expected, header.JZ_Calc_CIFAmount);

			var groupHeader = header.Master;

			PrepareCharge(groupHeader.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 100, header.JobDeclaration.LocalCurrencyCode));
			declaration.ResumeApportionment();
			header.GroupCharges[0].J7_IsIncludedInITOT = false;

			expected = 10600m + 100;
			AssertEquals("CIF value / CIP ", expected, header.JZ_Calc_CIFAmount);
		}

		public override void TestCalculateCIFWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			PrepareCharge(header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ExWorks, 100));
			PrepareCharge(header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Discount, 1));
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			PrepareCharge(preOTH);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			PrepareCharge(header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200));

			header.JZ_IncoTerm = "FOB";
			var expected = 10600m;
			AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);

			PrepareCharge(groupHeader.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100, header.JobDeclaration.LocalCurrencyCode));

			expected = 10600m;
			declaration.ResumeApportionment();
			AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);
		}

		public new void TestCalculateRealInvoiceTotal()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.LandingCharges, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Commission, 5);

			//FOB Incoterm
			header.JZ_IncoTerm = "FOB";
			var expected = new ZDecimal(10600 - 200 - 300 - 5);
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);

			//DDP Incoterm
			header.JZ_IncoTerm = "DDP";
			expected = new ZDecimal(10600 - 200 - 300 - 5);
			AssertEquals("Real Invoice / DDP ", expected, header.InvoiceLineTotal);

			//CIF Incoterm
			header.JZ_IncoTerm = "CIF";
			expected = 10600 - 200 - 300 - 5;
			AssertEquals("Real Invoice / CIF ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithCIF()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			PrepareCharge(header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 400));
			PrepareCharge(header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300));

			header.JZ_IncoTerm = "CIF";
			var expected = 10600m;
			AssertEquals("ITOT / CIF ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithCIP()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Discount, 1);
			var preOTH = header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			var oFT = header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 1000);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			header.JZ_IncoTerm = "CIP";
			var expected = 10600m - 100 - 200 - 5;
			AssertEquals("Real Invoice / CIP ", expected, header.InvoiceLineTotal);

			header.Charges.RemoveAndDelete(oFT);
			expected = 10600m - 100 - 200 - 5;
			AssertEquals("Real Invoice / CIP ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithDDU()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 40);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Discount, 1);
			var preOTH = header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			var postOTH = header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 600);
			postOTH.J7_IsDutiable = false;
			postOTH.J7_IsGSTApplicable = false;
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.LandingCharges, 1000);

			header.JZ_IncoTerm = "DDU";

			var expected = 10600m - 40 - 100 - 5 - 600 - 200;
			AssertEquals("Real Invoice / DDU ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithEXW()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Discount, 1);
			var preOTH = header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;

			header.JZ_IncoTerm = "EXW";
			var expected = 10600m - 5;
			AssertEquals("Real Invoice / EXW ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 1000);

			header.JZ_IncoTerm = "FOB";
			var expected = 10600m - 100 - 200 - 5;
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithUFB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = "FOB";

			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 250);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 120);
			header.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Discount, 5);

			ZDecimal expected = 10600m - 300m;
			AssertEquals("Real Invoice / UFB ", expected, header.InvoiceLineTotal);
		}

		public new void TestChangeIncotermResetIncludedInInvoiceFlagsForInvoiceCharges()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			var oFT = invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			var oNS = invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 100m, declaration.LocalCurrencyCode);
			var cOM = invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.Commission, 100m, declaration.LocalCurrencyCode);

			var line1 = invoice.JobComInvoiceLines.AddNew();
			var line1OFT = line1.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 20m, declaration.LocalCurrencyCode);

			invoice.JobComInvoiceLines.AddNew();

			AssertEquals("IsIncludedInInvoice is set for OFT", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for ONS", false, oNS.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for COM", true, cOM.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for Line1 Charge", false, line1OFT.J7_Calc_IsIncludedInInvoiceAmount);

			cOM.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoice.JZ_IncoTerm = "CIF";

			AssertEquals("IsIncludedInInvoice is set for OFT", true, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for ONS", true, oNS.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for COM", true, cOM.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is refreshed for Line1 Charge for the incoterm change", true, line1OFT.J7_Calc_IsIncludedInInvoiceAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = GetNewDeclaration() as JobDeclaration;
			declaration.AutoCreateChargesBasedOnIncoTerm = false;

			header = declaration.Invoices.AddNew();
			groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader header;
		JobComInvoiceGroupHeader groupHeader;
		IDisposable distributeByForExport;

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		#region Implementation

		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		#endregion
	}

	#endregion

	#region JobComInvoiceHeaderTestForDocumentWrappert

	sealed class JobComInvoiceHeaderTestForDocumentWrappert : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		public override void TestCheckingValueOfJZ_Calc_ConversionFactor()
		{
			var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddYears(10), 0.5m, uSDCurrency, "CUS");

			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			invoice.JZ_IncoTerm = "CIF";

			invoice.Charges.RemoveAll();
			var oFT = invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 500m);
			oFT.J7_IsIncludedInITOT = true;
			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50m);

			var expectedValue = invoice.ConvertToLocalAmountRounded(10000m - 500 - 50, uSDCurrency).Amount / 10000m;
			AssertEquals(ZArchitecture.Core.Utilities.Round(invoice.JZ_Calc_ConversionFactor, 4), ZArchitecture.Core.Utilities.Round(expectedValue, 4));
		}

		public override void TestJZ_Calc_OFTInInvoiceCurrency()
		{
			groupHeader.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100, groupHeader.JobDeclaration.LocalCurrencyCode);
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = groupHeader.JobDeclaration.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("JZ_Calc_OFTInInvoiceCurrency", 0m, invoice.JZ_Calc_OFTInInvoiceCurrency);

			groupHeader.Charges.RemoveAndDeleteAll();
			testDec.ResumeApportionment();
			AssertEquals("JZ_Calc_OFTInInvoiceCurrency", 0m, invoice.JZ_Calc_OFTInInvoiceCurrency);

			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 200, groupHeader.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("JZ_Calc_OFTInInvoiceCurrency", 200m, invoice.JZ_Calc_OFTInInvoiceCurrency);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrency.Amount, -400m);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount, -400m);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(100m, invoice.NonDutiableChargesNotIncludedInLinesInLocalCurrency.Amount);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(100m, invoice.NonDutiableChargesNotIncludedInLines.Amount);
		}

		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}

	#endregion
}
