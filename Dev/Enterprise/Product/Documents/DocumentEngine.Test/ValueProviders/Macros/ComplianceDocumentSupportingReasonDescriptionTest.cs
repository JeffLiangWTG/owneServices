using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ComplianceDocumentSupportingReasonDescription))]
	sealed class ComplianceDocumentSupportingReasonDescriptionTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ComplianceDocumentSupportingReasonDescription(AA,AR)>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ComplianceDocumentSupportingReasonDescription( BB , AP )>", Passes.FirstPass));
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<ComplianceDocumentSupportingReasonDescription (AA,AR) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var newlist1 = new ComplianceDocumentSupportingReasonCollection();
			newlist1.Add(new ComplianceDocumentSupportingReason() { Code = "AAA", EnglishDescription = "test aaa" });
			newlist1.Add(new ComplianceDocumentSupportingReason() { Code = "BBB", EnglishDescription = "test bbb" });
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsReceivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist1);

			var newlist2 = new ComplianceDocumentSupportingReasonCollection();
			newlist2.Add(new ComplianceDocumentSupportingReason() { Code = "CCC", EnglishDescription = "test ccc" });
			newlist2.Add(new ComplianceDocumentSupportingReason() { Code = "DDD", EnglishDescription = "test ddd" });
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsPayables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);

			AssertEquals("test aaa", ValueProviderToTest.GetReplacement("<ComplianceDocumentSupportingReasonDescription(AAA,AR)>", Report));
			AssertEquals("test bbb", ValueProviderToTest.GetReplacement("<ComplianceDocumentSupportingReasonDescription(BBB,AR)>", Report));
			AssertEquals("test ccc", ValueProviderToTest.GetReplacement("<ComplianceDocumentSupportingReasonDescription(CCC,AP)>", Report));
			AssertEquals("test ddd", ValueProviderToTest.GetReplacement("<ComplianceDocumentSupportingReasonDescription(DDD,AP)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ComplianceDocumentSupportingReasonDescription();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var newlist = new ComplianceDocumentSupportingReasonCollection();
			newlist.Add(new ComplianceDocumentSupportingReason() { Code = "COD", EnglishDescription = "REASON" });
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsReceivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist);
			Factory.Save();
		}
	}
}
