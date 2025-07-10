using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(RiskManagement))]
	class RiskManagementTest : Customs.Business.Testing.CusSupportingInfoTest<RiskManagement>
	{
		public void TestDefaultValuesFromEntryIfNeeded_CSI_ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = declaration.JE_MessageType;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.EntryNumber = "ENTRY1";
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2021, 07, 25, 16, 43, 00);
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CustomsValue = 3.14m;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 1.12m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 2.24m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			Factory.Save();

			var riskManagement = instruction.RiskManagements.AddNew();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.EntryDeclaration;
			riskManagement.CSI_ReferenceNumber = "ENTRY1";
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Value", 3.14m, riskManagement.CSI_Value);
				AssertEquals("CSI_Quantity", 1.12m, riskManagement.CSI_Quantity);
				AssertEquals("CSI_Quantity2", 2.24m, riskManagement.CSI_Quantity2);
				AssertEquals("CSI_DateOfIssue", new ZDateTime(2021, 07, 25), riskManagement.CSI_DateOfIssue);
			});
		}

		public void TestDefaultValuesFromEntryIfNeeded_CSI_Code()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = declaration.JE_MessageType;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.EntryNumber = "ENTRY1";
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2021, 07, 25, 16, 43, 00);
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CustomsValue = 3.14m;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 1.12m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 2.24m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			Factory.Save();

			var riskManagement = instruction.RiskManagements.AddNew();
			riskManagement.CSI_ReferenceNumber = "ENTRY1";
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.EntryDeclaration;
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Value", 3.14m, riskManagement.CSI_Value);
				AssertEquals("CSI_Quantity", 1.12m, riskManagement.CSI_Quantity);
				AssertEquals("CSI_Quantity2", 2.24m, riskManagement.CSI_Quantity2);
				AssertEquals("CSI_DateOfIssue", new ZDateTime(2021, 07, 25), riskManagement.CSI_DateOfIssue);
			});
		}

		public void TestCSI_Code_Caption()
		{
			AssertEquals("Entry/Permit Type", DataBoundResourceStrings.GetDataForProperty(Factory.New<RiskManagement>().CSI_CodeInfo).Caption);
		}

		public void TestCSI_Code_MaxLengthAttribute()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(RiskManagement), nameof(RiskManagement.CSI_Code), false, x => x.MaxLength == 3);
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			AssertEquals("Entry/Permit Number", DataBoundResourceStrings.GetDataForProperty(Factory.New<RiskManagement>().CSI_ReferenceNumberInfo).Caption);
		}

		public void TestCSI_DateOfIssue_Caption()
		{
			AssertEquals("Date", DataBoundResourceStrings.GetDataForProperty(Factory.New<RiskManagement>().CSI_DateOfIssueInfo).Caption);
		}

		public void TestCSI_Value_Caption()
		{
			AssertEquals("Customs Value", DataBoundResourceStrings.GetDataForProperty(Factory.New<RiskManagement>().CSI_ValueInfo).Caption);
		}

		public void TestCSI_Value_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(RiskManagement), nameof(RiskManagement.CSI_Value), false, x => x.DecimalPlaces == 2);
		}

		public void TestCSI_Quantity_Caption()
		{
			AssertEquals("Net Weight in KG", DataBoundResourceStrings.GetDataForProperty(Factory.New<RiskManagement>().CSI_QuantityInfo).Caption);
		}

		public void TestCSI_Quantity_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(RiskManagement), nameof(RiskManagement.CSI_Quantity), false, x => x.DecimalPlaces == 3);
		}

		public void TestCSI_Quantity2_Caption()
		{
			AssertEquals("Customs Qty", DataBoundResourceStrings.GetDataForProperty(Factory.New<RiskManagement>().CSI_Quantity2Info).Caption);
		}

		public void TestCSI_Quantity2_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(RiskManagement), nameof(RiskManagement.CSI_Quantity2), false, x => x.DecimalPlaces == 5);
		}

		public void TestCSI_AdditionalDescription_Caption()
		{
			AssertEquals("Comments", DataBoundResourceStrings.GetDataForProperty(Factory.New<RiskManagement>().CSI_AdditionalDescriptionInfo).Caption);
		}

		public void TestLookups()
		{
			AssertType<RiskManagementLookups>(Factory.New<RiskManagement>().Lookups);
		}

		public void TestValidation()
		{
			AssertType<RiskManagementValidation>(Factory.New<RiskManagement>().Validation);
		}

		public void TestSetDefaultValues()
		{
			var riskManagement = Factory.New<RiskManagement>();
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, riskManagement.CSI_RN_NKCountryCode);
		}

		public void TestIsEntryType_True()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.EntryDeclaration;

			AssertEquals(true, riskManagement.IsEntryType);
		}

		public void TestIsEntryType_False()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.TransitPermit;

			AssertEquals(false, riskManagement.IsEntryType);
		}

		public void TestCSI_ReferenceNumberFieldType_WhenENTCode()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.EntryDeclaration;

			AssertEquals(nameof(FieldType.TextCodeFindBox), riskManagement.CSI_ReferenceNumberFieldType);
		}

		public void TestCSI_ReferenceNumberFieldType_WhenPMTCode()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.TransitPermit;

			AssertEquals(nameof(FieldType.Text), riskManagement.CSI_ReferenceNumberFieldType);
		}

		public void TestHasEntryNumber_True()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.EntryDeclaration;
			riskManagement.CSI_ReferenceNumber = "Entry";

			AssertEquals(true, riskManagement.HasEntryNumber);
		}

		public void TestHasEntryNumber_EmptyEntryNumber()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.EntryDeclaration;

			AssertEquals(false, riskManagement.HasEntryNumber);
		}

		public void TestHasEntryNumber_TransitPermitType()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.TransitPermit;
			riskManagement.CSI_ReferenceNumber = "Entry";

			AssertEquals(false, riskManagement.HasEntryNumber);
		}

		protected override IEnumerable<RiskManagement> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (RiskManagement)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			return new RiskManagementCollection(instruction).AddNew();
		}
	}
}
