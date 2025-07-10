using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	public class CDSFECChallengeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_IsOverriddenWhenNoParent()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
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
			fec1.Validation.ValidateCY_IsOverridden();
			CombineAssertions("The FEC challenge validation should not trigger any notification when no parent.", () =>
			{
				AssertNoMessageErrors(fec1.CY_IsOverriddenInfo);
				AssertNoWarningContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});

			fec1.CY_ParentID = entryHeader.PK;
			fec1.Validation.ValidateCY_IsOverridden();
			CombineAssertions("FEC challenge validations for CDS should only trigger warning type notifications.", () =>
			{
				AssertNoMessageErrors(fec1.CY_IsOverriddenInfo);
				AssertHasWarningContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});

			var fec2 = Factory.New<FECChallenge>();
			fec2.CY_ParentTableCode = "CL";
			fec2.CY_Code = "JI_ORG";
			fec2.CY_Data = "CN";
			fec2.CY_Order = 1;
			fec2.CY_IsOverridden = false;
			fec2.Validation.ValidateCY_IsOverridden();
			CombineAssertions("The FEC challenge validation should not trigger any notification when no parent.", () =>
			{
				AssertNoMessageErrors(fec2.CY_IsOverriddenInfo);
				AssertNoWarningContaining(fec2.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});

			fec2.CY_ParentID = entryLine.PK;
			fec2.Validation.ValidateCY_IsOverridden();
			CombineAssertions("FEC challenge validations for CDS should only trigger warning type notifications.", () =>
			{
				AssertNoMessageErrors(fec2.CY_IsOverriddenInfo);
				AssertHasWarningContaining(fec2.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});
		}

		public void TestCheckCY_IsOverridden()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
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
			var fec2 = Factory.New<FECChallenge>();
			fec2.CY_ParentID = entryLine.PK;
			fec2.CY_ParentTableCode = "CL";
			fec2.CY_Order = 1;
			fec2.CY_Code = "JI_ORG";
			fec2.CY_Data = "CN";
			fec2.CY_IsOverridden = false;

			fec1.Validation.ValidateCY_IsOverridden();
			CombineAssertions("FEC challenge validations for CDS should only trigger warning type notifications.", () =>
			{
				AssertNoMessageErrors(fec1.CY_IsOverriddenInfo);
				AssertHasWarningContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});

			fec2.Validation.ValidateCY_IsOverridden();
			CombineAssertions("FEC challenge validations for CDS should only trigger warning type notifications.", () =>
			{
				AssertNoMessageErrors(fec2.CY_IsOverriddenInfo);
				AssertHasWarningContaining(fec2.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});

			fec1.CY_IsOverridden = true;
			CombineAssertions("The FEC challenge validation should not trigger any notification when CY_IsOverridden is flagged.", () =>
			{
				AssertNoMessageErrors(fec1.CY_IsOverriddenInfo);
				AssertNoWarningContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});

			fec2.CY_IsOverridden = true;
			CombineAssertions("The FEC challenge validation should not trigger any notification when CY_IsOverridden is flagged.", () =>
			{
				AssertNoMessageErrors(fec2.CY_IsOverriddenInfo);
				AssertNoWarningContaining(fec2.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});

			dec.JE_RN_NKTransportNationality = "GB";
			invoiceLine.JI_CountryOfOrigin = "GB";

			fec1.CY_IsOverridden = false;
			CombineAssertions("The FEC challenge validation should not trigger any notification for valid setup.", () =>
			{
				AssertNoMessageErrors(fec1.CY_IsOverriddenInfo);
				AssertNoWarningContaining(fec1.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});

			CombineAssertions("The FEC challenge validation should not trigger any notification for valid setup.", () =>
			{
				fec2.CY_IsOverridden = false;
				AssertNoMessageErrors(fec2.CY_IsOverriddenInfo);
				AssertNoWarningContaining(fec2.CY_IsOverriddenInfo, "A FEC challenge has been received. You should either tick the confirm box or change the value. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.");
			});
		}
	}
}
