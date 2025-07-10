using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B2JobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCustomizeFieldsValidations()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			var customAttrib1 = organisation.CustomLabels.AddNew();
			customAttrib1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1;
			customAttrib1.OT_IsMandatory = true;
			var customAttrib2 = organisation.CustomLabels.AddNew();
			customAttrib2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute2;
			customAttrib2.OT_IsMandatory = true;
			var customAttrib3 = organisation.CustomLabels.AddNew();
			customAttrib3.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute3;
			customAttrib3.OT_IsMandatory = true;
			var customAttrib4 = organisation.CustomLabels.AddNew();
			customAttrib4.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute4;
			customAttrib4.OT_IsMandatory = true;
			var customAttrib5 = organisation.CustomLabels.AddNew();
			customAttrib5.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute5;
			customAttrib5.OT_IsMandatory = true;
			var customAttrib6 = organisation.CustomLabels.AddNew();
			customAttrib6.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute6;
			customAttrib6.OT_IsMandatory = true;
			var customTextBlob1 = organisation.CustomLabels.AddNew();
			customTextBlob1.OT_FieldName = "ComInvoiceLine.CustomTextBlob1";
			customTextBlob1.OT_IsMandatory = true;
			var customFlag1 = organisation.CustomLabels.AddNew();
			customFlag1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag1;
			customFlag1.OT_IsMandatory = true;
			var customFlag2 = organisation.CustomLabels.AddNew();
			customFlag2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag2;
			customFlag2.OT_IsMandatory = true;
			var customFlag3 = organisation.CustomLabels.AddNew();
			customFlag3.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag3;
			customFlag3.OT_IsMandatory = true;
			var customFlag4 = organisation.CustomLabels.AddNew();
			customFlag4.OT_FieldName = "ComInvoiceLine.CustomFlag4";
			customFlag4.OT_IsMandatory = true;
			var customFlag5 = organisation.CustomLabels.AddNew();
			customFlag5.OT_FieldName = "ComInvoiceLine.CustomFlag5";
			customFlag5.OT_IsMandatory = true;
			var customDate1 = organisation.CustomLabels.AddNew();
			customDate1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomDate1;
			customDate1.OT_IsMandatory = true;
			var customDate2 = organisation.CustomLabels.AddNew();
			customDate2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomDate2;
			customDate2.OT_IsMandatory = true;
			var customDate3 = organisation.CustomLabels.AddNew();
			customDate3.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomDate3;
			customDate3.OT_IsMandatory = true;

			declaration.JE_OH_Importer = organisation.PK;

			var subHeader = declaration.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var asAccountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountLine.CA_OriginalLineNo = "Test01";
			Factory.Save();
			asAccountLine.OnLoaded();

			var claimLine = asAccountLine.CorrespondingAsClaimedForInvoiceLine;
			claimLine.OnLoaded();
			asAccountLine.Validation.ValidateAll();
			claimLine.Validation.ValidateAll();
			AssertEquals("No message errors expected. There is a validation that is not applicable to B2Adjustments", false, asAccountLine.HasMessageErrors);
			AssertEquals("No errors expected. There is a validation that is not applicable to B2Adjustments", false, claimLine.HasErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
		}
		new JobDeclaration declaration;
	}
}
