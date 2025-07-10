using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NveCusCodeDataCollection))]
	public class NveCusCodeDataCollectionTest : CusCodeDataCollectionTest<NveCusCodeData>
	{
		public void TestRebuildFromCharacteristicsInvoiceLine()
		{
			AssertRebuildFromCharacteristicsInvoiceLine(InvoiceLine);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertRebuildFromCharacteristicsInvoiceLine(InvoiceLine);
		}

		void AssertRebuildFromCharacteristicsInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			AssertType<JobComInvoiceLine>(invoiceLine.NVECusCodeDataCollection.Master);

			invoiceLine.JI_Tariff = "00000000";
			AssertEquals("NVE should be 0", 0, invoiceLine.NVECusCodeDataCollection.Count);

			invoiceLine.JI_Tariff = "11111111";
			AssertEquals(2, InvoiceLine.NVECusCodeDataCollection.Count);
			var nveCusCodeData01 = AssertNveCusCodeData(0, "BA", invoiceLine);
			var nveCusCodeData02 = AssertNveCusCodeData(1, "BB", invoiceLine);

			invoiceLine.JI_Tariff = "56049000";
			AssertEquals(1, InvoiceLine.NVECusCodeDataCollection.Count);
			AssertNull(InvoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("BB"));
			var nveCusCodeData12 = AssertNveCusCodeData(0, "AA", invoiceLine);

			AssertEquals("NveCusCodeData BA was deleted", true, nveCusCodeData01.IsDeleted);
			AssertEquals("NveCusCodeData BB was deleted", true, nveCusCodeData02.IsDeleted);
			AssertEquals("NveCusCodeData AA was not deleted", false, nveCusCodeData12.IsDeleted);

			invoiceLine.JI_Tariff = "00000000";
			AssertEquals("Attribute should be 0", 0, invoiceLine.NVECusCodeDataCollection.Count);
			AssertEquals("AttributeCusCodeData AA was deleted", true, nveCusCodeData12.IsDeleted);

			invoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoiceLine.JI_Tariff = "56049000";
			AssertEquals("Attribute should be 0", 0, invoiceLine.NVECusCodeDataCollection.Count);
		}

		public void TestRebuildFromCharacteristicsCusClassPartPivot()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			AssertType<CusClassPartPivot>(pivot.NveCusCodeDataCollection.Master);

			pivot.CI_TariffNum = "00000000";
			AssertEquals("NVE should be 0", 0, pivot.NveCusCodeDataCollection.Count);

			pivot.CI_TariffNum = "11111111";
			AssertEquals(2, pivot.NveCusCodeDataCollection.Count);
			var nveCusCodeData01 = AssertNveCusCodeData(0, "BA", pivot);
			var nveCusCodeData02 = AssertNveCusCodeData(1, "BB", pivot);

			pivot.CI_TariffNum = "56049000";
			AssertEquals(1, pivot.NveCusCodeDataCollection.Count);
			AssertNull(pivot.NveCusCodeDataCollection.GetFirstElementHaving("BB"));
			var nveCusCodeData12 = AssertNveCusCodeData(0, "AA", pivot);

			AssertEquals("NveCusCodeData BA was deleted", true, nveCusCodeData01.IsDeleted);
			AssertEquals("NveCusCodeData BB was deleted", true, nveCusCodeData02.IsDeleted);
			AssertEquals("NveCusCodeData AA was not deleted", false, nveCusCodeData12.IsDeleted);

			pivot.CI_TariffNum = "00000000";
			AssertEquals("Attribute should be 0", 0, pivot.NveCusCodeDataCollection.Count);
			AssertEquals("AttributeCusCodeData AA was deleted", true, nveCusCodeData12.IsDeleted);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = "56049000";
			AssertEquals("Attribute should be 0", 0, pivot.NveCusCodeDataCollection.Count);
		}

		public void TestReadOnlyWhenClonedFromAttached()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "TEST1";

			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var licEntryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			licEntryInstruction.CEI_JE = licDeclaration.PK;
			licEntryInstruction.CEI_Description = "TEST1";

			var licInvHeader = licDeclaration.Invoices.AddNew();
			var licInvLine = licInvHeader.InvoiceLines.AddNew();
			licInvLine.NVECusCodeDataCollection.AddNew();
			licInvLine.JI_CEI = licEntryInstruction.PK;

			var licEntryHeader = licDeclaration.CustomsEntryHeaders.AddNew();
			licEntryHeader.CH_CEI_Instruction = licEntryInstruction.PK;
			var licEntryLine = licEntryHeader.AllEntryLines.AddNew();
			licInvLine.JI_CL = licEntryLine.PK;

			licEntryInstruction.EntryHeader.MovementReferenceNumberSetter("TST_LIC", ZDateTime.Now);
			declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licEntryInstruction) });

			var newInvHeader = declaration.Invoices.AddNew();
			var newInvLine = invHeader.InvoiceLines.AddNew();
			newInvLine.NVECusCodeDataCollection.AddNew();

			var clonedInvoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Single(x => x.ImportLicenseNumber == "TST_LIC");

			Assert("Cloned NVECusCodeDataCollection should be ReadOnly", clonedInvoiceLine.NVECusCodeDataCollection.ReadOnly);
			Assert("New NVECusCodeDataCollection should NOT be ReadOnly", !newInvLine.NVECusCodeDataCollection.ReadOnly);
		}

		NveCusCodeData AssertNveCusCodeData(int index, string code, BusinessObject businessObject)
		{
			NveCusCodeData nveCusCodeData = null;
			if (businessObject is JobComInvoiceLine invoiceLine)
			{
				nveCusCodeData = invoiceLine.NVECusCodeDataCollection[index];
			}
			else if (businessObject is CusClassPartPivot pivot)
			{
				nveCusCodeData = pivot.NveCusCodeDataCollection[index];
			}
			AssertEquals($"AttributeCusCodeData created for {code}", code, nveCusCodeData.CY_Code);
			return nveCusCodeData;
		}

		public void TestAllowNewCore()
		{
			var nve = GetCusCodeDataCollection();
			Assert(!nve.AllowNew);
		}

		public void TestAllowRemoveCore()
		{
			var nve = GetCusCodeDataCollection();
			Assert(!nve.AllowRemove);
		}

		protected override CusCodeDataCollection<NveCusCodeData> GetCusCodeDataCollection()
		{
			return new NveCusCodeDataCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<NveCusCodeData>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_InvoiceNumber = "1111";
					invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "55555555";
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}
