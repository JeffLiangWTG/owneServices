using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GB.Registry.Business;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	class CDSJobComInvoiceLineValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSettingValuationMethodsCreatesSupportingDocuments()
		{
			var supplier = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			var supplierAddress = supplier.MainAddress;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B0000100";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var inv = dec.Invoices.AddNew();
			inv.JZ_InvoiceNumber = "123456";
			inv.JZ_OH_Supplier = supplier.PK;
			dec.JE_OH_Supplier = supplier.PK;
			supplierAddress.OA_RN_NKCountryCode = "IT";
			inv.JZ_InvoiceDate = ZDateTime.Today;
			AssertEquals(1, inv.SupportingDocuments.Count);
			AssertEquals("N935", inv.SupportingDocuments[0].CSI_Code);
			AssertEquals("123456", inv.SupportingDocuments[0].CSI_ReferenceNumber);

			var line = (EU.Business.Declaration.JobComInvoiceLine)inv.InvoiceLines.AddNew();
			line.JI_ValuationCode = ValuationMethodList.Codes._1;
			AssertEquals("N935={JZ_InvoiceNumber}  already exists on the invoice header", 0, line.SupportingDocuments.Count);
			inv.SupportingDocuments.DeleteAllDocumentsHavingCode("N935");
			AssertOtherValuationMethods(line);

			inv.SupportingDocuments.RemoveAndDeleteAll();
			line.JI_ValuationCode = ValuationMethodList.Codes._2;
			AssertEquals(1, line.SupportingDocuments.Count);
			AssertEquals("N935", line.SupportingDocuments[0].CSI_Code);
			AssertEquals("123456", line.SupportingDocuments[0].CSI_ReferenceNumber);
			AssertOtherValuationMethods(line);
		}

		void AssertOtherValuationMethods(EU.Business.Declaration.JobComInvoiceLine line)
		{
			foreach (var vm in new ZString[] { ValuationMethodList.Codes._2, ValuationMethodList.Codes._3 })
			{
				line.JI_ValuationCode = vm;
				AssertEquals(1, line.SupportingDocuments.Count);
				AssertEquals("N935", line.SupportingDocuments[0].CSI_Code);
				AssertEquals("123456", line.SupportingDocuments[0].CSI_ReferenceNumber);
			}

			foreach (var vm in new ZString[] { ValuationMethodList.Codes._4, ValuationMethodList.Codes._5, ValuationMethodList.Codes._6 })
			{
				line.JI_ValuationCode = vm;
				AssertEquals(1, line.SupportingDocuments.Count);
				AssertEquals("9WKS", line.SupportingDocuments[0].CSI_Code);
				AssertEquals(line.Declaration.JE_DeclarationReference, line.SupportingDocuments[0].CSI_ReferenceNumber);
				AssertEquals(string.Format("SEE ATTACHED WORKSHEET {0}", line.Declaration.JE_DeclarationReference), line.SupportingDocuments[0].CSI_Description);
			}

			line.JI_ValuationCode = ValuationMethodList.Codes._7;
			AssertEquals(0, line.SupportingDocuments.Count);
			line.JI_ValuationCode = "";
			AssertEquals(0, line.SupportingDocuments.Count);
		}

		public void TestAutoConvertExciseUnits()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var additionalCode = invoiceLine.AdditionalSupplementaryCodes.AddNew("X321");

			CombineAssertions("CDS declaration", () =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				invoiceLine.JI_CustomsSecondQuantity = 7500m;
				invoiceLine.JI_CustomsSecondUnitQty = "LTR";

				ResetInvoiceLine(invoiceLine, "ASV", "ASVX");
				invoiceLine.JI_CustomsThirdQuantity = 5m;
				AssertQuantitiesEqual("LTR, ASV > ASVX", 375m, invoiceLine);

				ResetInvoiceLine(invoiceLine, "ASV", "LPA");
				invoiceLine.JI_CustomsThirdQuantity = 5m;
				AssertQuantitiesEqual("LTR, ASV > LPA", 375m, invoiceLine);

				ResetInvoiceLine(invoiceLine, "ASVX", "ASV");
				invoiceLine.JI_CustomsThirdQuantity = 375m;
				AssertQuantitiesEqual("LTR, ASVX > ASV", 5m, invoiceLine);

				ResetInvoiceLine(invoiceLine, "ASVX", "ASV");
				invoiceLine.JI_CustomsFifthUnitQty = "LPA";
				invoiceLine.JI_CustomsThirdQuantity = 100m;
				AssertEquals("LTR, ASVX > ASV", 1.333333m, invoiceLine.JI_CustomsFourthQuantity);
				AssertEquals("ASVX should be directly copied to LPA without rounding error", 100m, invoiceLine.JI_CustomsFifthQuantity);

				ResetInvoiceLine(invoiceLine, "ASVX", "HLT");
				invoiceLine.JI_CustomsThirdQuantity = 375m;
				AssertQuantitiesEqual("Cannot convert from LTR and ASVX to HLT", 0m, invoiceLine);

				using (GBCustomsDataRegistry.Instance.CDSEnableAutoConvertExciseUnits.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					ResetInvoiceLine(invoiceLine, "ASVX", "ASV");
					invoiceLine.JI_CustomsThirdQuantity = 375m;
					AssertQuantitiesEqual("Disabled conversion by registry", 0m, invoiceLine);
				}

				ResetInvoiceLine(invoiceLine, "ASVX", "ASV");
				additionalCode.CY_Code = "123";
				invoiceLine.JI_CustomsThirdQuantity = 375m;
				AssertQuantitiesEqual("Without excise code", 0m, invoiceLine);

				ResetInvoiceLine(invoiceLine, "ASV", "ASVX");
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
				additionalCode.CY_Code = "X321";
				invoiceLine.JI_CustomsThirdQuantity = 5m;
				AssertQuantitiesEqual("CHEIF declaration", 0m, invoiceLine);
			});
		}

		void ResetInvoiceLine(JobComInvoiceLine invoiceLine, ZString thirdQuantityUnit, ZString targetQuantityUnit)
		{
			invoiceLine.JI_CustomsThirdQuantity = 0m;
			invoiceLine.JI_CustomsThirdUnitQty = thirdQuantityUnit;
			invoiceLine.JI_CustomsFourthQuantity = 0m;
			invoiceLine.JI_CustomsFourthUnitQty = targetQuantityUnit;
			invoiceLine.JI_CustomsFifthQuantity = 0m;
			invoiceLine.JI_CustomsFifthUnitQty = targetQuantityUnit;
		}

		void AssertQuantitiesEqual(string message, decimal quantity, JobComInvoiceLine invoiceLine)
		{
			AssertEquals(message, quantity, invoiceLine.JI_CustomsFourthQuantity);
			AssertEquals(message, quantity, invoiceLine.JI_CustomsFifthQuantity);
		}
	}
}
