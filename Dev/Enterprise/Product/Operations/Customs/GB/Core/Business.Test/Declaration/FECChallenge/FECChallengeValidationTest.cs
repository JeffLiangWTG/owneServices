using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class FECChallengeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_CodeAndCY_Type()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.InvoiceLines.AddNew();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var fec = Factory.New<FECChallenge>();
			fec.CY_ParentID = entryHeader.PK;
			fec.CY_ParentTableCode = "CH";
			fec.CY_Code = "JE_FLG";
			fec.CY_Data = "CN";
			fec.CY_Order = 1;
			fec.CY_IsOverridden = false;
			fec.Validation.ValidateAll();
			AssertNoMessageErrors(fec.CY_CodeInfo);
			AssertNoMessageErrors(fec.CY_TypeInfo);
		}

		public void TestCheckCY_IsOverriddenWhenNoParent()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceLine = dec.InvoiceLines.AddNew();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			dec.JE_RN_NKTransportNationality = "CN";
			invoiceLine.JI_CountryOfOrigin = "CN";

			var fec1 = Factory.New<FECChallenge>();
			fec1.CY_ParentTableCode = "CH";
			fec1.CY_Code = "JE_FLG";
			fec1.CY_Data = "CN";
			fec1.CY_Order = 1;
			fec1.CY_IsOverridden = false;
			fec1.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");

			fec1.CY_ParentID = entryHeader.PK;
			fec1.Validation.ValidateAll();
			AssertHasMessageErrorContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");

			var fec2 = Factory.New<FECChallenge>();
			fec2.CY_ParentTableCode = "CL";
			fec2.CY_Code = "JI_ORG";
			fec2.CY_Data = "CN";
			fec2.CY_Order = 1;
			fec2.CY_IsOverridden = false;
			fec2.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fec2.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");

			fec2.CY_ParentID = entryLine.PK;
			fec2.Validation.ValidateAll();
			AssertHasMessageErrorContaining(fec2.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
		}

		public void TestCheckCY_IsOverridden()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceLine = dec.InvoiceLines.AddNew();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			dec.JE_RN_NKTransportNationality = "CN";
			invoiceLine.JI_CountryOfOrigin = "CN";
			var fec1 = Factory.New<FECChallenge>();
			fec1.CY_ParentID = entryHeader.PK;
			fec1.CY_ParentTableCode = "CH";
			fec1.CY_Order = 1;
			fec1.CY_Code = "JE_FLG";
			fec1.CY_Data = "CN";
			fec1.CY_IsOverridden = false;
			var fec4 = Factory.New<FECChallenge>();
			fec4.CY_ParentID = entryLine.PK;
			fec4.CY_ParentTableCode = "CL";
			fec4.CY_Code = "JI_ORG";
			fec4.CY_Data = "CN";
			fec4.CY_Order = 1;
			fec4.CY_IsOverridden = false;
			fec1.Validation.ValidateAll();
			fec4.Validation.ValidateAll();
			AssertHasMessageErrorContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			AssertHasMessageErrorContaining(fec4.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");

			dec.JE_RouteFRequested = true;
			fec1.Validation.ValidateAll();
			fec4.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			AssertNoMessageErrorContaining(fec4.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");

			dec.JE_RouteFRequested = false;
			fec1.CY_IsOverridden = true;
			fec4.CY_IsOverridden = true;
			fec1.Validation.ValidateAll();
			fec4.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			AssertNoMessageErrorContaining(fec4.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");

			fec1.CY_IsOverridden = false;
			fec4.CY_IsOverridden = false;
			dec.JE_RN_NKTransportNationality = "GB";
			invoiceLine.JI_CountryOfOrigin = "GB";
			fec1.Validation.ValidateAll();
			fec4.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			AssertNoMessageErrorContaining(fec4.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
		}
	}
}
