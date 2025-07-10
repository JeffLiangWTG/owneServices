using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.TPAR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.TPAR
{
	[TestedType(typeof(TparReport))]
	public class TparReportTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGenerateLines()
		{
			var org1Abn = Creator.ABIGAS.CustomsCodes.AddNew();
			org1Abn.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			org1Abn.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			org1Abn.OK_CustomsRegNo = "22255588899972";
			var org2Abn = Creator.AALSHI.CustomsCodes.AddNew();
			org2Abn.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			org2Abn.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			org2Abn.OK_CustomsRegNo = "44477766621";
			Factory.Save();

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddDataToComplianceReport(complianceReport);

			var tparReport = Factory.New<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals(2, tparReport.Lines.Count);

			var line1 = tparReport.Lines.Cast<TparReportCreditorLine>().First(x => x.ARL_OH_Organisation == Creator.ABIGAS.PK);
			var line2 = tparReport.Lines.Cast<TparReportCreditorLine>().First(x => x.ARL_OH_Organisation == Creator.AALSHI.PK);
			AssertLineResult(line1, Creator.ABIGAS, 10m, 310m, 0m);
			AssertLineResult(line2, Creator.AALSHI, 5m, 205m, 0m);

			void AssertLineResult(TparReportCreditorLine line, OrgHeader org, ZDecimal gstAmount, ZDecimal totalAmount, ZDecimal paymentsAmount)
			{
				AssertEquals(TparReportCreditorLine.LineState.Added, line.State);
				AssertEquals(tparReport.PK, line.ARL_ATR_AccTaxReturn);
				AssertEquals(gstAmount, line.ARL_GSTAmount);
				AssertEquals(totalAmount, line.ARL_TotalAmountIncludingTax);
				AssertEquals(paymentsAmount, line.ARL_PaymentsBasisWithholdingTaxAmount);
				AssertEquals(line.ARL_TotalAmountIncludingTax, line.ARL_OverriddenTotalAmountIncludingTax);
				AssertEquals(line.ARL_GSTAmount, line.ARL_OverriddenGSTAmount);
				AssertEquals(line.ARL_PaymentsBasisWithholdingTaxAmount, line.ARL_OverriddenPaymentsBasisWithholdingTaxAmount);
				AssertEquals(org.LocalBusinessRegNo, line.ARL_OrgRegNo);
				AssertEquals(org.OH_FullName, line.ARL_OrgName);
				AssertEquals(org.MainAddress.OA_Address1, line.ARL_Address1);
				AssertEquals(org.MainAddress.OA_Address2, line.ARL_Address2);
				AssertEquals(org.MainAddress.OA_City, line.ARL_City);
				AssertEquals(TparReportHelper.GetState(org.MainAddress.OA_State, org.CountryCode), line.ARL_State);
				AssertEquals(org.CountryCode, line.ARL_RN_NKCountryCode);
				AssertEquals(TparReportHelper.GetState(org.MainAddress.OA_PostCode, org.CountryCode), line.ARL_PostCode);
				AssertEquals(org.OH_FullName, line.ARL_OrgName);
				AssertEquals(TparReportHelper.GetAustralianBusinessNumber(org), line.ARL_OrgRegNo);
			}
		}

		public void TestLinesOverriddenAmounts()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddDataToComplianceReport(complianceReport);

			var tparReport = Factory.New<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals(2, tparReport.Lines.Count);

			var line1 = tparReport.Lines.Cast<TparReportCreditorLine>().First(x => x.ARL_OH_Organisation == Creator.ABIGAS.PK);
			AssertLineResult(10m, 10m, 310m, 310m, 0m, 0m);

			line1.ARL_OverriddenGSTAmount = 20m;
			line1.ARL_OverriddenTotalAmountIncludingTax = 320m;
			line1.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = 10m;
			Factory.Save();

			AssertLineResult(10m, 20m, 310m, 320m, 0m, 10m);

			AddAdditionalLinesToComplianceReport(complianceReport, Creator.ABIGAS);
			AssertLineResult(15m, 20m, 515m, 320m, 0m, 10m);

			line1.ARL_GSTAmount = 20m;
			line1.ARL_OverriddenGSTAmount = 20m;
			line1.ARL_TotalAmountIncludingTax = 500m;
			line1.ARL_OverriddenTotalAmountIncludingTax = 500m;
			line1.ARL_PaymentsBasisWithholdingTaxAmount = 15m;
			line1.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = 15m;
			Factory.Save();

			AssertLineResult(15m, 15m, 515m, 515m, 15m, 15m);

			void AssertLineResult(decimal gstAmount, decimal gstOverriddenAmount, decimal totalAmount,
				decimal totalOverriddenAmount, decimal paymentAmount, decimal paymentOverriddenAmount)
			{
				tparReport.ClearReportLines_ForTestOnly();
				line1 = tparReport.Lines.Cast<TparReportCreditorLine>().First(x => x.ARL_OH_Organisation == Creator.ABIGAS.PK);

				AssertEquals(gstAmount, line1.ARL_GSTAmount);
				AssertEquals(gstOverriddenAmount, line1.ARL_OverriddenGSTAmount);
				AssertEquals(totalAmount, line1.ARL_TotalAmountIncludingTax);
				AssertEquals(totalOverriddenAmount, line1.ARL_OverriddenTotalAmountIncludingTax);
				AssertEquals(paymentAmount, line1.ARL_PaymentsBasisWithholdingTaxAmount);
				AssertEquals(paymentOverriddenAmount, line1.ARL_OverriddenPaymentsBasisWithholdingTaxAmount);
			}
		}

		public void TestStatusIsUpdatedDuringSaving()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var tparReport = Factory.New<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			Assert(tparReport.ATR_Status.IsEmpty);

			Factory.Save();
			AssertEquals(AccTaxReturn.Status.Saved, tparReport.ATR_Status);

			Factory.Save();
			AssertEquals(AccTaxReturn.Status.Saved, tparReport.ATR_Status);

			tparReport.ATR_Status = AccTaxReturn.Status.Generated;
			Factory.Save();
			AssertEquals(AccTaxReturn.Status.Generated, tparReport.ATR_Status);

			tparReport.ATR_Status = AccTaxReturn.Status.Submitted;
			Factory.Save();
			AssertEquals(AccTaxReturn.Status.Submitted, tparReport.ATR_Status);
		}

		public void TestSubmitReport()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddDataToComplianceReport(complianceReport);
			var tparReport = Factory.New<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			Assert(tparReport.ATR_Status.IsEmpty);

			AssertEquals(2, tparReport.Lines.Count);

			tparReport.SubmitReport();
			AssertEquals(string.Empty, tparReport.ATR_Status);

			Factory.Save();
			AssertEquals(AccTaxReturn.Status.Saved, tparReport.ATR_Status);

			tparReport.SubmitReport();
			AssertEquals(AccTaxReturn.Status.Saved, tparReport.ATR_Status);

			tparReport.GenerateReport();
			AssertEquals(AccTaxReturn.Status.Generated, tparReport.ATR_Status);
			Factory.Save();

			AssertEquals(2, tparReport.Lines.Count);
			AssertNotNull(tparReport.Lines.Cast<TparReportCreditorLine>().FirstOrDefault(x => x.ARL_OH_Organisation == Creator.ABIGAS.PK));
			AssertNotNull(tparReport.Lines.Cast<TparReportCreditorLine>().FirstOrDefault(x => x.ARL_OH_Organisation == Creator.AALSHI.PK));

			AddAdditionalLinesToComplianceReport(complianceReport, Creator.Creditor1);
			tparReport.ClearReportLines_ForTestOnly();
			AssertEquals(3, tparReport.Lines.Count);
			AssertNotNull(tparReport.Lines.Cast<TparReportCreditorLine>().FirstOrDefault(x => x.ARL_OH_Organisation == Creator.Creditor1.PK));

			Creator.AALSHI.CompanyData.OB_APPrintContractorForm = false;
			tparReport.ClearReportLines_ForTestOnly();
			AssertEquals(2, tparReport.Lines.Count);
			AssertNull(tparReport.Lines.Cast<TparReportCreditorLine>().FirstOrDefault(x => x.ARL_OH_Organisation == Creator.AALSHI.PK));

			tparReport.SubmitReport();
			AssertEquals(AccTaxReturn.Status.Submitted, tparReport.ATR_Status);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			tparReport = newFactory.Load<TparReport>(tparReport.PK);

			AssertEquals(AccTaxReturn.Status.Submitted, tparReport.ATR_Status);
			AssertEquals(2, tparReport.Lines.Count);
			AssertNotNull(tparReport.Lines.Cast<TparReportCreditorLine>().FirstOrDefault(x => x.ARL_OH_Organisation == Creator.ABIGAS.PK));
			AssertNotNull(tparReport.Lines.Cast<TparReportCreditorLine>().FirstOrDefault(x => x.ARL_OH_Organisation == Creator.AALSHI.PK));
		}

		public void TestSubmittedReportOrgDetailsAreNotUpdated()
		{
			Creator.ABIGAS.LocalBusinessRegNo = "1234512345No1";
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddDataToComplianceReport(complianceReport);

			var tparReport = Factory.New<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			AssertEquals(2, tparReport.Lines.Count);

			var line1 = tparReport.Lines.Cast<TparReportCreditorLine>().First(x => x.ARL_OH_Organisation == Creator.ABIGAS.PK);
			AssertEquals("12345123451", line1.ARL_OrgRegNo);

			Creator.ABIGAS.LocalBusinessRegNo = "1234512345No2";
			tparReport.ClearReportLines_ForTestOnly();
			Assert(!tparReport.IsSubmitted);
			line1 = tparReport.Lines.Cast<TparReportCreditorLine>().First(x => x.ARL_OH_Organisation == Creator.ABIGAS.PK);
			AssertEquals("line org registration has been updated", "12345123452", line1.ARL_OrgRegNo);

			tparReport.ATR_Status = TparReport.Status.Submitted;
			Creator.ABIGAS.LocalBusinessRegNo = "1234512345No3";
			tparReport.ClearReportLines_ForTestOnly();
			Assert(tparReport.IsSubmitted);
			line1 = tparReport.Lines.Cast<TparReportCreditorLine>().First(x => x.ARL_OH_Organisation == Creator.ABIGAS.PK);
			AssertEquals("line org registration didn't changed because report is already submitted", "12345123452", line1.ARL_OrgRegNo);
		}

		public void TestGenerateReportStatus()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var tparReport = Factory.New<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			Assert(tparReport.ATR_Status.IsEmpty);

			tparReport.GenerateReport();
			AssertEquals(string.Empty, tparReport.ATR_Status);

			tparReport.ATR_Status = AccTaxReturn.Status.Submitted;
			tparReport.GenerateReport();
			AssertEquals(AccTaxReturn.Status.Submitted, tparReport.ATR_Status);

			tparReport.ATR_Status = AccTaxReturn.Status.Saved;
			tparReport.GenerateReport();
			AssertEquals(AccTaxReturn.Status.Generated, tparReport.ATR_Status);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestGenerateReport()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddDataToComplianceReport(complianceReport);
			//Add line with zero and negative amounts, should be included in the TPAR form but not in the generated file
			AddAdditionalLinesToComplianceReport(complianceReport, Creator.Creditor1, 0);
			AddAdditionalLinesToComplianceReport(complianceReport, Creator.Creditor2, -1);

			var abn = complianceReport.Company.OrgProxy.CustomsCodes.AddNew();
			abn.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			abn.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			abn.OK_CustomsRegNo = "12 345678901971";
			var org1Abn = Creator.ABIGAS.CustomsCodes.AddNew();
			org1Abn.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			org1Abn.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			org1Abn.OK_CustomsRegNo = "22255588899972";
			var org2Abn = Creator.AALSHI.CustomsCodes.AddNew();
			org2Abn.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			org2Abn.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			org2Abn.OK_CustomsRegNo = "44 477/766\\621";
			Factory.Save();

			var financialYear = complianceReport.AccountingPeriod.ToString().Substring(0, 4);
			var fileRef = $"EDI-{financialYear}-v1";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "Grégøire Putz";
			staff1.GS_WorkPhone = "+61280012200";
			staff1.GS_FaxNum = "+61280012666";
			staff1.GS_EmailAddress = "ManuelCamp@hotmail.com";
			Factory.Save();
			Env.SetUserContext(new UserContext(staff1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			AssertEquals("Grégøire Putz", EnvProxy.Instance.CurrentUser.FullName);
			AssertEquals("+61280012200", EnvProxy.Instance.CurrentUser.WorkPhone);
			AssertEquals("+61280012666", EnvProxy.Instance.CurrentUser.Fax);
			AssertEquals("ManuelCamp@hotmail.com", EnvProxy.Instance.CurrentUser.EmailAddress);

			complianceReport.Company.GC_Address2 = "Bat B";
			AssertEquals("184 Bourke Road", complianceReport.Company.GC_Address1);
			AssertEquals("Bat B", complianceReport.Company.GC_Address2);
			AssertEquals("Alexandria", complianceReport.Company.GC_City);
			AssertEquals("2015", complianceReport.Company.GC_PostCode);
			AssertEquals("NSW", complianceReport.Company.GC_State);
			AssertEquals("AU", complianceReport.Company.GC_RN_NKCountryCode);

			Creator.ABIGAS.MainAddress.OA_State = "NSW";
			Creator.ABIGAS.MainAddress.OA_PostCode = "2015";
			Creator.ABIGAS.MainAddress.OA_City = "Sydney";
			Creator.ABIGAS.MainAddress.OA_Email = "Abigas@email.au";
			Creator.ABIGAS.MainAddress.OA_Phone = "+61280012777";
			Creator.AALSHI.MainAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.NewZealand;
			Creator.AALSHI.MainAddress.OA_State = "Northland";
			Creator.AALSHI.MainAddress.OA_PostCode = "3072";
			Creator.AALSHI.MainAddress.OA_City = "Kerikeri";
			Creator.ABIGAS.MainAddress.OA_Email = "AALSHI@email.au";
			Creator.ABIGAS.MainAddress.OA_Phone = "+61280012888";

			var tparReport = Factory.New<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals("EDI CUSTOMS BROKERS", complianceReport.Company.OrgProxy.OH_FullName);
			Factory.Save();

			AssertEquals(TparReport.Status.Saved, tparReport.ATR_Status);
			AssertEquals(4, tparReport.Lines.Count);
			AssertEquals(0, tparReport.DocManagerInfo.AllEDocs.Count);
			var fileName = tparReport.GenerateReport();
			Factory.Save();

			AssertEquals(TparReport.Status.Generated, tparReport.ATR_Status);
			AssertEquals("One eDoc added", 1, tparReport.DocManagerInfo.AllEDocs.Count);
			Assert("filename should start by TPAR-EDI-<year>-v1-<Date>", fileName.StartsWith($"TPAR-{fileRef}-{ZDate.Today.ToString("yyyyMMdd")}"));
			AssertContains(fileName, tparReport.DocManagerInfo.AllEDocs[0].FileName);
			var edocData = tparReport.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8();
			var edocLines = Regex.Split(edocData, "\r\n|\r|\n");
			Assert("each record line has the correct length", edocLines.Take(8).All(x => x.Length == TparReportHelper.RecordLength));
			Assert("Last line is empty", edocLines.Last().Length == 0);
			AssertEquals("number of Lines is correct", 9, edocLines.Length);

			var softwareDataRecord = $"996SOFTWARECOMMERCIALWisetech Global CargoWiseOne v{new EnterpriseInformationRetriever().VersionNumber}".PadRight(TparReportHelper.RecordLength);

			var expectedResult = $@"996IDENTREGISTER112345678901T{complianceReport.ACR_DateTo.ToString("ddMMyyyy")}PCMFPAIVV02.0                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
996IDENTREGISTER2EDI CUSTOMS BROKERS                                                                                                                                                                                     Greg0ire Putz                         +61280012200   +61280012666   EDI-{financialYear}-v1                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
996IDENTREGISTER3184 Bourke Road                       Bat B                                 Alexandria                 NSW2015Australia                                                                                                                     0000                    ManuelCamp@hotmail.com                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
996IDENTITY12345678901971{financialYear}EDI CUSTOMS BROKERS                                                                                                                                                                                                                                                                                                                                                                                             184 Bourke Road                       Bat B                                 Alexandria                 NSW2015Australia           Greg0ire Putz                                                                                                                                                                                                                                                                                                                                                                                                                                        
{softwareDataRecord}
996DPAIVS44477766621                                                            A.A.L. SHIPPING AGENCIES P/L                                                                                                                                                                                                                                                                                                                                                                                    PO BOX 10446                          ADELAIDE ST, BRISBANE  QLD            Kerikeri                   Nor3072Australia                          000000000000000000000002050000000000000000000005P00000000                                                                                                                                                                                                                                                                                    NO                                    
996DPAIVS22255588899                                                            ABI GAS & TOOLS                                                                                                                                                                                                                                                                                                                                                                                                 171 ABBOTSFORD ROAD                   MAYNE, QLD                            Sydney                     NSW2015Australia           +61280012888   000000000000000000000003100000000000000000000010P00000000                                                                                                                                                                                                        AALSHI@email.au                                                             NO                                    
996FILE-TOTAL00000008                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
";

			AssertEquals("generated file should be stored in eDoc", expectedResult, edocData);
		}

		public void TestLastGeneratedFileName()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var tparReport = Factory.New<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			Factory.Save();
			AssertEquals(string.Empty, tparReport.LastGeneratedFileName);

			AssertEquals(0, tparReport.DocManagerInfo.AllEDocs.Count);
			var fileName1 = tparReport.GenerateReport();
			Factory.Save();
			AssertEquals("One eDoc added", 1, tparReport.DocManagerInfo.AllEDocs.Count);
			AssertEquals(fileName1, tparReport.LastGeneratedFileName);

			System.Threading.Thread.Sleep(1000);

			tparReport.ATR_Status = TparReport.Status.Saved;
			var fileName2 = tparReport.GenerateReport();
			Factory.Save();
			AssertEquals("One eDoc added", 2, tparReport.DocManagerInfo.AllEDocs.Count);
			AssertEquals(fileName2, tparReport.LastGeneratedFileName);
		}

		public void TestFieldsReadOnly()
		{
			var tparReport = Factory.New<TparReport>();

			tparReport.ATR_Status = ZString.Empty;
			assertReadOnly(true);

			tparReport.ATR_Status = TparReport.Status.Saved;
			Assert(tparReport.IsSaved);
			assertReadOnly(true);

			tparReport.ATR_Status = TparReport.Status.Generated;
			Assert(tparReport.IsGenerated);
			assertReadOnly(true);

			tparReport.ATR_Status = TparReport.Status.Submitted;
			Assert(tparReport.IsSubmitted);
			assertReadOnly(false);

			void assertReadOnly(bool editable)
			{
				AssertEquals(!editable, tparReport.ATR_CommentInfo.ReadOnly);
				Assert(tparReport.ATR_ACR_ComplianceReportInfo.ReadOnly);
				Assert(tparReport.ATR_Address1Info.ReadOnly);
				Assert(tparReport.ATR_Address2Info.ReadOnly);
				Assert(tparReport.ATR_CityInfo.ReadOnly);
				Assert(tparReport.ATR_CompanyNameInfo.ReadOnly);
				Assert(tparReport.ATR_GovtReceiptInformationInfo.ReadOnly);
				Assert(tparReport.ATR_PostCodeInfo.ReadOnly);
				Assert(tparReport.ATR_ReturnTypeInfo.ReadOnly);
				Assert(tparReport.ATR_RN_NKCountryCodeInfo.ReadOnly);
				Assert(tparReport.ATR_StateInfo.ReadOnly);
				Assert(tparReport.ATR_StatusInfo.ReadOnly);
				Assert(tparReport.ATR_VATRegNoInfo.ReadOnly);
				Assert(tparReport.ATR_VersionInfo.ReadOnly);
			}
		}

		public void TestDefaultValues()
		{
			var tparReport = Factory.New<TparReport>();
			Assert(tparReport.ATR_Status.IsEmpty);
			AssertEquals(1, tparReport.ATR_Version);
			AssertEquals(AccTaxReturn.ReturnType.TPAR, tparReport.ATR_ReturnType);
		}

		#region Implementation

		List<AccTransactionHeader> AddDataToComplianceReport(AccComplianceReport report)
		{
			report.ACR_ReportType = "TPR";
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(ZDateTime.Today, report.ACR_GC_Company);
			report.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			report.AccountingPeriod = currentPeriod.AM_Period;

			Creator.CreateConfigurationForComplianceReport(report, "AH", "TPA");

			var invoices = new List<AccTransactionHeader>();

			AssertNotNull(CostGLAccount);
			Creator.ABIGAS.CompanyData.OB_APPrintContractorForm = true;
			var aPInvoice1 = Creator.CreateAPInvoice<APInvoice>("I0001" + report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			invoices.Add(aPInvoice1);
			Assert("Has Lines", aPInvoice1.Lines.Count > 0);
			var line1_1 = aPInvoice1.Lines[0];
			line1_1.AL_AG = CostGLAccount.PK;
			line1_1.AL_AT = Creator.GST1.PK;
			line1_1.AL_InputGSTVATRecoverable = 0.8m;
			var line1_2 = Creator.CreateInvoiceLine(aPInvoice1, Creator.AUD, 1m, 120m);
			line1_2.AL_AG = CostGLAccount.PK;
			line1_2.AL_AT = Creator.GSTFREE1.PK;
			var line1_3 = Creator.CreateInvoiceLine(aPInvoice1, Creator.AUD, 1m, 80m);
			line1_3.AL_AG = CostGLAccount.PK;
			line1_3.AL_AT = Creator.REV.PK;
			Creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice1, aPInvoice1.AH_PostDate);

			Creator.AALSHI.CompanyData.OB_APPrintContractorForm = true;
			var aPInvoice2 = Creator.CreateAPInvoice<APInvoice>("I0002" + report.ReportLines.Count.ToString(), Creator.AUD, 1m, 50m, 10m, 0m, 50m, 10m, 0m, Creator.AALSHI);
			invoices.Add(aPInvoice2);
			Assert("Has Lines", aPInvoice2.Lines.Count > 0);
			var line2_1 = aPInvoice2.Lines[0];
			line2_1.AL_AG = CostGLAccount.PK;
			line2_1.AL_AT = Creator.GST1.PK;
			line2_1.AL_InputGSTVATRecoverable = 0.5m;
			var line2_2 = Creator.CreateInvoiceLine(aPInvoice2, Creator.AUD, 1m, 130m);
			line2_2.AL_AG = CostGLAccount.PK;
			line2_2.AL_AT = Creator.GSTFREE1.PK;
			var line2_3 = Creator.CreateInvoiceLine(aPInvoice2, Creator.AUD, 1m, 20m);
			line2_3.AL_AG = CostGLAccount.PK;
			line2_3.AL_AT = Creator.REV.PK;
			Creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice2, aPInvoice2.AH_PostDate);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(report, aPInvoice1, aPInvoice2);
			report.GenerateFromQueue();

			report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, report.ReportLines.Count);

			return invoices;
		}

		void AddAdditionalLinesToComplianceReport(AccComplianceReport complianceReport, OrgHeader org, int amountMultiplier = 1)
		{
			var nbLineBefore = complianceReport.ReportLines.Count;
			org.CompanyData.OB_APPrintContractorForm = true;
			var additionalApInvoice = Creator.CreateAPInvoice<APInvoice>("I0003" + complianceReport.ReportLines.Count.ToString(), Creator.AUD, 1m, amountMultiplier * 50m, amountMultiplier * 10m, 0m, amountMultiplier * 50m, amountMultiplier * 10m, 0m, org);
			Assert("Has Lines", additionalApInvoice.Lines.Count > 0);
			var line1 = additionalApInvoice.Lines[0];
			line1.AL_AG = CostGLAccount.PK;
			line1.AL_AT = Creator.GST1.PK;
			line1.AL_InputGSTVATRecoverable = 0.5m;
			var line2 = Creator.CreateInvoiceLine(additionalApInvoice, Creator.AUD, 1m, amountMultiplier * 130m);
			line2.AL_AG = CostGLAccount.PK;
			line2.AL_AT = Creator.GSTFREE1.PK;
			var line3 = Creator.CreateInvoiceLine(additionalApInvoice, Creator.AUD, 1m, amountMultiplier * 20m);
			line3.AL_AG = CostGLAccount.PK;
			line3.AL_AT = Creator.REV.PK;
			Creator.CreateAndMatchAPPaymentForAPInvoice(additionalApInvoice, additionalApInvoice.AH_PostDate);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, additionalApInvoice);
			complianceReport.GenerateFromQueue();

			complianceReport.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", nbLineBefore + 1, complianceReport.ReportLines.Count);
		}

		AccGLHeader CostGLAccount => Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1010.20.10"));

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			EnableReports = AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			EnableReports.Dispose();

			base.TearDown();
		}

		IDisposable EnableReports; 

		#endregion
	}
}
