using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingAdditionalCode))]
	sealed class GuidedDecisionMakingAdditionalCodeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSiblingsAutomaticallyUpdatedWhenIsTickedChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			var additionalCodes = gDMBasic.AdditionalCodes;

			var additionalCode_ADD_C161 = additionalCodes.AddNew();
			additionalCode_ADD_C161.AdditionalCode = "C161";
			additionalCode_ADD_C161.ApplicableToType = "ADD";
			var additionalCode_CVD_C161 = additionalCodes.AddNew();
			additionalCode_CVD_C161.AdditionalCode = "C161";
			additionalCode_CVD_C161.ApplicableToType = "CVD";
			var additionalCode_552_C161 = additionalCodes.AddNew();
			additionalCode_552_C161.AdditionalCode = "C161";
			additionalCode_552_C161.ApplicableToType = "552";
			var additionalCode_554_C161 = additionalCodes.AddNew();
			additionalCode_554_C161.AdditionalCode = "C161";
			additionalCode_554_C161.ApplicableToType = "554";

			var additionalCode_ADD_C161_2 = additionalCodes.AddNew();
			additionalCode_ADD_C161_2.AdditionalCode = "C161";
			additionalCode_ADD_C161_2.ApplicableToType = "ADD";

			additionalCode_ADD_C161.IsTicked = true;
			CombineAssertions("All siblings with same additional code and type should be ticked when an additional code is ticked", () =>
			{
				AssertEquals(true, additionalCode_ADD_C161.IsTicked);
				AssertEquals(false, additionalCode_CVD_C161.IsTicked);
				AssertEquals(false, additionalCode_552_C161.IsTicked);
				AssertEquals(false, additionalCode_554_C161.IsTicked);
				AssertEquals(true, additionalCode_ADD_C161_2.IsTicked);
			});

			additionalCode_ADD_C161.IsTicked = false;
			CombineAssertions("All siblings with same additional code and type should be unticked when an additional code is unticked", () =>
			{
				AssertEquals(false, additionalCode_ADD_C161.IsTicked);
				AssertEquals(false, additionalCode_CVD_C161.IsTicked);
				AssertEquals(false, additionalCode_552_C161.IsTicked);
				AssertEquals(false, additionalCode_554_C161.IsTicked);
				AssertEquals(false, additionalCode_ADD_C161_2.IsTicked);
			});

			additionalCode_ADD_C161_2.IsTicked = true;
			CombineAssertions("All siblings with same additional code and type should be ticked when an additional code is ticked", () =>
			{
				AssertEquals(true, additionalCode_ADD_C161.IsTicked);
				AssertEquals(false, additionalCode_CVD_C161.IsTicked);
				AssertEquals(false, additionalCode_552_C161.IsTicked);
				AssertEquals(false, additionalCode_554_C161.IsTicked);
				AssertEquals(true, additionalCode_ADD_C161_2.IsTicked);
			});
		}

		public void TestLookups()
		{
			var additionalCode = (GuidedDecisionMakingAdditionalCode)GetNewBusinessObject();
			AssertType<GuidedDecisionMakingAdditionalCodeLookups>(additionalCode.Lookups);
		}

		public void TestFieldsMaxLength()
		{
			var additionalCode = (GuidedDecisionMakingAdditionalCode)GetNewBusinessObject();
			AssertEquals("ApplicableToTypeMaxLength: ", 5, additionalCode.ApplicableToTypeInfo.MaxLength);
			AssertEquals("ApplicableToDescriptionMaxLength: ", 2000, additionalCode.ApplicableToDescriptionInfo.MaxLength);
			AssertEquals("AdditionalCodeMaxLength: ", 15, additionalCode.AdditionalCodeInfo.MaxLength);
			AssertEquals("DataGroupingMaxLength: ", 3, additionalCode.DataGroupingInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			return new GuidedDecisionMakingAdditionalCode(gDMBasic);
		}
	}
}
