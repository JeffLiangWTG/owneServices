using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	public class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestEntryInstructionCSI_SubType_ReadOnly()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var additionalInfo = entryInstruction.AdditionalInfos.AddNew();
			AssertEquals("Readonly should be true for UCC5 Import", true, additionalInfo.CSI_SubType_ReadOnly);
			AssertEquals("Readonly should be true for UCC5 Import", true, additionalInfo.CSI_SubTypeInfo.ReadOnly);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertEquals("Readonly should be false for UCC6 Import", false, additionalInfo.CSI_SubType_ReadOnly);
			AssertEquals("Readonly should be false for UCC6 Import", false, additionalInfo.CSI_SubTypeInfo.ReadOnly);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Readonly should be false for Export", false, additionalInfo.CSI_SubType_ReadOnly);
			AssertEquals("Readonly should be false for Export", false, additionalInfo.CSI_SubTypeInfo.ReadOnly);
		}

		public void TestCSI_SubType_ReadOnly()
		{
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			AssertEquals("CSI_SubType_ReadOnly.ReadOnly true for UCC5 Import", true, additionalInfo.CSI_SubType_ReadOnly);
			AssertEquals("CSI_SubType_ReadOnly.ReadOnly true for UCC5 Import", true, additionalInfo.CSI_SubTypeInfo.ReadOnly);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additionalInfo2 = invoiceLine2.AdditionalInfos.AddNew();
			AssertEquals("CSI_SubType_ReadOnly.ReadOnly ", false, additionalInfo2.CSI_SubType_ReadOnly);
			AssertEquals("CSI_SubType_ReadOnly.ReadOnly", false, additionalInfo2.CSI_SubTypeInfo.ReadOnly);
		}

		public void TestCSI_ReferenceNumber_ReadOnly()
		{
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("INF - CSI_ReferenceNumber.ReadOnly", true, additionalInfo.CSI_ReferenceNumber_ReadOnly);
			AssertEquals("INF - CSI_ReferenceNumber.ReadOnly", true, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals("REF - CSI_ReferenceNumber.ReadOnly", false, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
			AssertEquals("REF - CSI_ReferenceNumber.ReadOnly", false, additionalInfo.CSI_ReferenceNumber_ReadOnly);
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals("TRA - CSI_ReferenceNumber.ReadOnly", false, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
			AssertEquals("TRA - CSI_ReferenceNumber.ReadOnly", false, additionalInfo.CSI_ReferenceNumber_ReadOnly);
			additionalInfo.CSI_SubType = "!";
			AssertEquals("Unknow - CSI_ReferenceNumber.ReadOnly", false, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
			AssertEquals("Unknow - CSI_ReferenceNumber.ReadOnly", false, additionalInfo.CSI_ReferenceNumber_ReadOnly);
		}

		public void TestCSI_Description_ReadOnly()
		{
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals("REF - CSI_Description.ReadOnly", true, additionalInfo.CSI_Description_ReadOnly);
			AssertEquals("REF - CSI_Description.ReadOnly", true, additionalInfo.CSI_DescriptionInfo.ReadOnly);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("INF - CSI_Description.ReadOnly", false, additionalInfo.CSI_Description_ReadOnly);
			AssertEquals("INF - CSI_Description.ReadOnly", false, additionalInfo.CSI_DescriptionInfo.ReadOnly);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals("TRA - CSI_Description.ReadOnly", true, additionalInfo.CSI_Description_ReadOnly);
			AssertEquals("TRA - CSI_Description.ReadOnly", true, additionalInfo.CSI_DescriptionInfo.ReadOnly);

			additionalInfo.CSI_SubType = "!";
			AssertEquals("Unknow - CSI_Description.ReadOnly", false, additionalInfo.CSI_Description_ReadOnly);
			AssertEquals("Unknow - CSI_Description.ReadOnly", false, additionalInfo.CSI_DescriptionInfo.ReadOnly);
		}

		public void TestCSI_DescriptionMaxLength()
		{
			AssertEquals("DescriptionMaxLength", 512, additionalInfo.CSI_DescriptionInfo.MaxLength);
		}

		public void TestValidation()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportAdditionalInfoValidation>("Export", additionalInfo.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportAdditionalInfoValidation>("Import", additionalInfo.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<AdditionalInfoValidation>("MiscellaneousCustoms", additionalInfo.Validation);
		}

		public void TestLookups()
		{
			AssertType<AdditionalInfoLookups>(additionalInfo.Lookups);
		}

		public void TestCSI_SubType()
		{
			AssertEquals(3, additionalInfo.CSI_SubTypeInfo.MaxLength);
		}

		public void TestCSI_SubType_Default()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var additionalInfo = entryInstruction.AdditionalInfos.AddNew();
			AssertEquals("CSI_SubType default value should be 'INF' for UCC5 Import", "INF", additionalInfo.CSI_SubType);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			var additionalInfo2 = entryInstruction.AdditionalInfos.AddNew();
			AssertEquals("CSI_SubType default value should be empty for UCC6 Import", string.Empty, additionalInfo2.CSI_SubType);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var additionalInfo3 = entryInstruction.AdditionalInfos.AddNew();
			AssertEquals("CSI_SubType default value should be empty for Export", string.Empty, additionalInfo3.CSI_SubType);
		}

		public void TestInvoiceLine()
		{
			AssertNull("Parent Is Declaration, Not InvoiceLine", additionalInfo.InvoiceLine);
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
			AssertEquals("Parent Is InvoiceLine", invoiceLine.PK, invoiceLineAdditionalInfo.InvoiceLine.PK);
		}

		public void TestParentIsInvoiceHeader()
		{
			Assert("Parent Is Declaration, Not InvoiceHeader", !additionalInfo.ParentIsInvoiceHeader);
			var invoice = declaration.Invoices.AddNew();
			var invoiceAdditionalInfo = invoice.AdditionalInfos.AddNew();
			Assert("Parent Is InvoiceHeader", invoiceAdditionalInfo.ParentIsInvoiceHeader);
		}

		public void TestInvoiceHeader()
		{
			AssertNull("Parent Is Declaration, Not InvoiceHeader", additionalInfo.InvoiceHeader);
			var invoice = declaration.Invoices.AddNew();
			var invoiceAdditionalInfo = invoice.AdditionalInfos.AddNew();
			AssertEquals("Parent Is InvoiceHeader", invoice.PK, invoiceAdditionalInfo.InvoiceHeader.PK);
		}

		public void TestParentIsCusEntryInstruction()
		{
			Assert("Parent Is Declaration, Not CusEntryInstruction", !additionalInfo.ParentIsCusEntryInstruction);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceAdditionalInfo = instruction.AdditionalInfos.AddNew();
			Assert("Parent Is CusEntryInstruction", invoiceAdditionalInfo.ParentIsCusEntryInstruction);
		}

		public void TestParentEntryInstruction()
		{
			AssertNull("Parent Is Declaration, Not InvoiceHeader", additionalInfo.EntryInstruction);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceAdditionalInfo = instruction.AdditionalInfos.AddNew();
			AssertEquals("Parent Is CusEntryInstruction", instruction.PK, invoiceAdditionalInfo.EntryInstruction.PK);
		}

		public void TestParentIsCusClassPartPivot()
		{
			Assert("Parent Is Declaration, Not CusClassPartPivot", !additionalInfo.ParentIsCusClassPartPivot);
			var pivot = Factory.New<CusClassPartPivot>();
			var pivotAdditionalInfo = pivot.AdditionalInfos.AddNew();
			Assert("Parent Is CusClassPartPivot", pivotAdditionalInfo.ParentIsCusClassPartPivot);
		}

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.AdditionalInfos.AddNew();

			var invoice = declaration.Invoices.AddNew();
			yield return invoice.AdditionalInfos.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.AdditionalInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.AdditionalInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => additionalInfo;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			additionalInfo = declaration.AdditionalInfos.AddNew();
		}
		JobDeclaration declaration;
		AdditionalInfo additionalInfo;
	}
}
