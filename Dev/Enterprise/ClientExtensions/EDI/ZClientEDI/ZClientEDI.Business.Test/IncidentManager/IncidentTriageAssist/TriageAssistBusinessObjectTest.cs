using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(TriageAssistBusinessObject))]
	public class TriageAssistBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateProduct()
		{
			Incident.IM_Product = "ENT";
			var obj = new TriageAssistBusinessObject(Incident);
			AssertEquals("ENT", obj.Product);
			obj.Product = "BOR";
			AssertHasWarning(obj.ProductInfo, "This product does not match the incident.");
			obj.Product = "ENT";
			AssertNoWarnings(obj.ProductInfo);
		}

		public void TestSearchTermOperator()
		{
			Incident.IM_Product = "ENT";
			var obj = new TriageAssistBusinessObject(Incident);
			AssertEquals(TriageAssistBusinessObject.ComparisonConstants.ContainsAny, obj.SearchTermOperator);
			obj.SearchTermOperator = "";
			AssertHasError(obj.SearchTermOperatorInfo, "Please enter a value.");
			obj.SearchTermOperator = "xxx";
			AssertHasError(obj.SearchTermOperatorInfo, "Enter a valid selection.");
			obj.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.Exact;
			AssertNoErrors(obj.SearchTermOperatorInfo);
		}

		public void TestCriteriaTypeFilter()
		{
			Incident.IM_Product = "ENT";
			var obj = new TriageAssistBusinessObject(Incident);
			AssertEquals(TriageAssistBusinessObject.ComparisonConstants.Any, obj.CriteriaTypeFilter);
			AssertEquals(ZString.Empty, obj.CriteriaTypeFilterCode);
			obj.CriteriaTypeFilter = "";
			AssertHasError(obj.CriteriaTypeFilterInfo, "Please enter a Criteria Type.");
			obj.CriteriaTypeFilter = "xxx";
			AssertHasError(obj.CriteriaTypeFilterInfo, "Enter a valid Criteria Type.");
			AssertEquals(ZString.Empty, obj.CriteriaTypeFilterCode);
			obj.CriteriaTypeFilter = IncidentDiagnosticCriteriaTypes.Descriptions.PrimarySymptom;
			AssertNoErrors(obj.CriteriaTypeFilterInfo);
			AssertEquals(IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom, obj.CriteriaTypeFilterCode);
		}

		public void TestSearchScope()
		{
			Incident.IM_Product = "ENT";
			var obj = new TriageAssistBusinessObject(Incident);
			AssertEquals(true, obj.ShouldSearchKeywords);
			AssertEquals(true, obj.ShouldSearchDescription);
			obj.ShouldSearchKeywords = false;
			obj.ShouldSearchDescription = false;
			AssertHasError(obj.ShouldSearchDescriptionInfo, "Please select at least one option.");
			obj.ShouldSearchKeywords = true;
			obj.ShouldSearchDescription = true;
			obj.ShouldSearchDescription = false;
			obj.ShouldSearchKeywords = false;
			AssertHasError(obj.ShouldSearchKeywordsInfo, "Please select at least one option.");
			obj.ShouldSearchKeywords = true;
			obj.ShouldSearchDescription = true;
			AssertNoErrors(obj.ShouldSearchDescriptionInfo);
			AssertNoErrors(obj.ShouldSearchKeywordsInfo);
		}

		public void TestOnRelevantCriteriaPropertyValueChanged()
		{
			var searchTerm = ZGuid.NewZGuid().ToSqlGuid();
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_Description = $"{searchTerm} - 001";
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria2.IMD_Description = $"{searchTerm} - 002";
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria3.IMD_Description = "something else";
			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "INV";
			Factory.Save();

			var obj = new TriageAssistBusinessObject(Incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.RefreshSearchTerm(searchTerm, "");

			AssertEquals(criteria1.PK, obj.LinkedCriteriaCollection.Single().PK);
			AssertEquals(2, obj.SearchedCriteriaCollection.Count);
			AssertEquals(criteria1.PK, obj.SearchedCriteriaCollection[0].PK);
			AssertEquals(criteria2.PK, obj.SearchedCriteriaCollection[1].PK);

			obj.SearchedCriteriaCollection[0].Investigate = false;
			obj.SearchedCriteriaCollection[1].Confirm = true;

			AssertEquals("criteria2 moved to Linked", criteria2.PK, obj.LinkedCriteriaCollection.Single().PK);
			AssertEquals(2, obj.SearchedCriteriaCollection.Count);
			AssertEquals(criteria1.PK, obj.SearchedCriteriaCollection[0].PK);
			AssertEquals(criteria2.PK, obj.SearchedCriteriaCollection[1].PK);
		}

		public void TestLog()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			Factory.Save();

			Assist.Parent.TriagePK = triage.PK;
			Factory.Save();
			var log = Incident.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode).Single();
			AssertEquals($"|DES=Triage node applied to {Assist.Parent.TableName}|RFN={triage.IMT_TriageNumber}", log.SL_Reference);
		}

		public void TestFocusedObjects()
		{
			AssertEquals(false, Assist.ShowFocusedObjectsOnly);
			AssertEquals(false, Assist.ShowFocusedSuggestedCriteriaOnly);
			AssertEquals(false, Assist.ShowFocusedTriageNodesOnly);

			Assist.ShowFocusedSuggestedCriteriaOnly = true;
			AssertEquals(true, Assist.ShowFocusedObjectsOnly);
			AssertEquals(true, Assist.ShowFocusedSuggestedCriteriaOnly);
			AssertEquals(false, Assist.ShowFocusedTriageNodesOnly);

			Assist.ShowFocusedObjectsOnly = true;
			AssertEquals(true, Assist.ShowFocusedObjectsOnly);
			AssertEquals(true, Assist.ShowFocusedSuggestedCriteriaOnly);
			AssertEquals(true, Assist.ShowFocusedTriageNodesOnly);

			Assist.ShowFocusedObjectsOnly = false;
			AssertEquals(false, Assist.ShowFocusedObjectsOnly);
			AssertEquals(false, Assist.ShowFocusedSuggestedCriteriaOnly);
			AssertEquals(false, Assist.ShowFocusedTriageNodesOnly);
		}

		public void TestTextProperties_RTF()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_Description = "IMD_Description:001";
			criteria1.IMD_InternalSupportNote = "IMD_InternalSupportNote:001, click the url: https://www.cw1.com/a001.html to continue.";
			criteria1.IMD_Question = "IMD_Question:001, click the url: \r\nhttp://www.cw1.com/b002.html\r\n to continue.";

			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria2.IMD_Description = "IMD_Description:002";
			criteria2.IMD_InternalSupportNote = "IMD_InternalSupportNote:002, click the url: https://www.cw1.com/c003.html?q=c456 to continue.";
			criteria2.IMD_Question = "IMD_Question:002, click the url: \r\nhttp://www.cw1.com/d004.html?q=123\r\n and url2 https://www.cw1.com/link2.html?q=456 to continue.";

			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria3.IMD_Description = "IMD_Description:003";
			criteria3.IMD_InternalSupportNote = "IMD_InternalSupportNote:003, click the url: https://www.cw1.com/e005.html?q=e789 to continue.";
			criteria3.IMD_Question = "IMD_Question:003, click the url: \r\nhttp://www.cw1.com/d004.html?q=123\r\n to continue.";

			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "VER";

			var pivot2 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot2.LinkParent(Incident);
			pivot2.IMV_IMD_DiagnosticCriteria = criteria2.PK;
			pivot2.IMV_Status = "INV";

			var pivot3 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot3.IMO_IMT_Triage = Triage.PK;
			pivot3.IMO_IMD_DiagnosticCriteria = criteria1.PK;

			var pivot4 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot4.IMO_IMT_Triage = Triage.PK;
			pivot4.IMO_IMD_DiagnosticCriteria = criteria3.PK;

			Incident.IM_IMT_Triage = Triage.PK;

			var checklist1 = Factory.New<IncidentTriageChecklistItem>();
			checklist1.IMC_SupportDescription = "IMC_SupportDescription:001";
			checklist1.IMC_IsPublished = true;
			checklist1.PublishedDescriptionText = "PublishedDescriptionText:001, click the url: https://www.cw1.com/g005.html?g=e789 to continue.";

			var checklist2 = Factory.New<IncidentTriageChecklistItem>();
			checklist2.IMC_SupportDescription = "IMC_SupportDescription:002";
			checklist2.IMC_IsPublished = false;
			checklist2.PublishedDescriptionText = "PublishedDescriptionText:002, click the url: https://www.cw1.com/h009.html?z=z789 to continue.";

			var pivot5 = Factory.New<IncidentTriageChecklistItemPivot>();
			pivot5.IMP_IMT_Triage = Triage.PK;
			pivot5.IMP_Sequence = 1;
			pivot5.IMP_IMC_ChecklistItem = checklist1.PK;

			var pivot6 = Factory.New<IncidentTriageChecklistItemPivot>();
			pivot6.IMP_IMT_Triage = Triage.PK;
			pivot6.IMP_Sequence = 2;
			pivot6.IMP_IMC_ChecklistItem = checklist2.PK;

			Factory.Save();

			var incidentInNewFactory = new BusinessObjectFactory().Load<SupportIncident>(Incident.PK);
			var obj = new TriageAssistBusinessObject(incidentInNewFactory);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			obj.SuggestedCriteriaCollection.LoadSuggestedCriteria();

			AssertEquals(2, obj.DiagnosticGuideItemCount);
			AssertEquals(2, obj.FinalisedTriageNodeActionCount);

			CombineAssertions(() =>
			{
				AssertRTF(obj.DiagnosticGuideInternalSupportNoteRTF,
@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{\*\generator RTFConverter W.X.Y.Z}{{}{\b\ul INVESTIGATE}\par}{\par}{{*** IMD_Description:002 ***}\par}{{IMD_InternalSupportNote:002, click the url: }{{\field{\*\fldinst HYPERLINK ""https://www.cw1.com/c003.html?q=c456"" }{\fldrslt https://www.cw1.com/c003.html?q=c456}}}{ to continue.}\par}{\par}{{}{\b\ul CONFIRM}\par}{\par}{{*** IMD_Description:001 ***}\par}{{IMD_InternalSupportNote:001, click the url: }{{\field{\*\fldinst HYPERLINK ""https://www.cw1.com/a001.html"" }{\fldrslt https://www.cw1.com/a001.html}}}{ to continue.}\par}{\par}{\par}}",
@"INVESTIGATE

*** IMD_Description:002 ***
IMD_InternalSupportNote:002, click the url: https://www.cw1.com/c003.html?q=c456 to continue.

CONFIRM

*** IMD_Description:001 ***
IMD_InternalSupportNote:001, click the url: https://www.cw1.com/a001.html to continue.

");
				AssertHtml(obj.DiagnosticGuideInternalSupportNoteRTF_HTML,
					"<p><strong><span style=\"text-decoration-line: underline;\">INVESTIGATE</span></strong></p><p></p><p>*** IMD_Description:002 ***</p><p>IMD_InternalSupportNote:002, click the url: <a href=\"https://www.cw1.com/c003.html?q=c456\" target=\"_blank\">https://www.cw1.com/c003.html?q=c456</a> to continue.</p><p></p><p><strong><span style=\"text-decoration-line: underline;\">CONFIRM</span></strong></p><p></p><p>*** IMD_Description:001 ***</p><p>IMD_InternalSupportNote:001, click the url: <a href=\"https://www.cw1.com/a001.html\" target=\"_blank\">https://www.cw1.com/a001.html</a> to continue.</p><p></p><p></p>");

				AssertRTF(obj.DiagnosticGuideClientQuestionRTF,
@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{\*\generator RTFConverter W.X.Y.Z}{{}{\b\ul INVESTIGATE}\par}{\par}{{*** IMD_Description:002 ***}\par}{{IMD_Question:002, click the url: }\par}{{{\field{\*\fldinst HYPERLINK ""http://www.cw1.com/d004.html?q=123"" }{\fldrslt http://www.cw1.com/d004.html?q=123}}}\par}{{ and url2 }{{\field{\*\fldinst HYPERLINK ""https://www.cw1.com/link2.html?q=456"" }{\fldrslt https://www.cw1.com/link2.html?q=456}}}{ to continue.}\par}{\par}{{}{\b\ul CONFIRM}\par}{\par}{{*** IMD_Description:001 ***}\par}{{IMD_Question:001, click the url: }\par}{{{\field{\*\fldinst HYPERLINK ""http://www.cw1.com/b002.html"" }{\fldrslt http://www.cw1.com/b002.html}}}\par}{{ to continue.}\par}{\par}{\par}}",
@"INVESTIGATE

*** IMD_Description:002 ***
IMD_Question:002, click the url: 
http://www.cw1.com/d004.html?q=123
 and url2 https://www.cw1.com/link2.html?q=456 to continue.

CONFIRM

*** IMD_Description:001 ***
IMD_Question:001, click the url: 
http://www.cw1.com/b002.html
 to continue.

");
				AssertHtml(obj.DiagnosticGuideClientQuestionRTF_HTML,
					"<p><strong><span style=\"text-decoration-line: underline;\">INVESTIGATE</span></strong></p><p></p><p>*** IMD_Description:002 ***</p><p>IMD_Question:002, click the url: </p><p><a href=\"http://www.cw1.com/d004.html?q=123\" target=\"_blank\">http://www.cw1.com/d004.html?q=123</a></p><p> and url2 <a href=\"https://www.cw1.com/link2.html?q=456\" target=\"_blank\">https://www.cw1.com/link2.html?q=456</a> to continue.</p><p></p><p><strong><span style=\"text-decoration-line: underline;\">CONFIRM</span></strong></p><p></p><p>*** IMD_Description:001 ***</p><p>IMD_Question:001, click the url: </p><p><a href=\"http://www.cw1.com/b002.html\" target=\"_blank\">http://www.cw1.com/b002.html</a></p><p> to continue.</p><p></p><p></p>");

				AssertRTF(obj.FinalisedTriageNodeInternalSupportActionRTF,
@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{\*\generator RTFConverter W.X.Y.Z}{{*** IMC_SupportDescription:002 ***}\par}{{PublishedDescriptionText:002, click the url: }{{\field{\*\fldinst HYPERLINK ""https://www.cw1.com/h009.html?z=z789"" }{\fldrslt https://www.cw1.com/h009.html?z=z789}}}{ to continue.}\par}{\par}}",
@"*** IMC_SupportDescription:002 ***
PublishedDescriptionText:002, click the url: https://www.cw1.com/h009.html?z=z789 to continue.
");
				AssertHtml(obj.FinalisedTriageNodeInternalSupportActionRTF_HTML,
					"<p>*** IMC_SupportDescription:002 ***</p><p>PublishedDescriptionText:002, click the url: <a href=\"https://www.cw1.com/h009.html?z=z789\" target=\"_blank\">https://www.cw1.com/h009.html?z=z789</a> to continue.</p><p></p>");

				AssertRTF(obj.FinalisedTriageNodeClientMessageRTF,
@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{\*\generator RTFConverter W.X.Y.Z}{{*** IMC_SupportDescription:001 ***}\par}{{PublishedDescriptionText:001, click the url: }{{\field{\*\fldinst HYPERLINK ""https://www.cw1.com/g005.html?g=e789"" }{\fldrslt https://www.cw1.com/g005.html?g=e789}}}{ to continue.}\par}{\par}}",
@"*** IMC_SupportDescription:001 ***
PublishedDescriptionText:001, click the url: https://www.cw1.com/g005.html?g=e789 to continue.
");
				AssertHtml(obj.FinalisedTriageNodeClientMessageRTF_HTML,
					"<p>*** IMC_SupportDescription:001 ***</p><p>PublishedDescriptionText:001, click the url: <a href=\"https://www.cw1.com/g005.html?g=e789\" target=\"_blank\">https://www.cw1.com/g005.html?g=e789</a> to continue.</p><p></p>");
			});
		}

		void AssertRTF(ZBlob rtfBytes, string rtfExpected, string textExpected)
		{
			var rtfText = ORtfTextUtilTest.GetRtfStringIndependentOfMachineSettings(rtfBytes.ToUTF8());
			AssertEquals(rtfText, ORtfTextUtilTest.GetRtfStringIndependentOfMachineSettings(rtfExpected));
			AssertEquals(ORtfTextUtil.RtfToText(rtfText), textExpected);
		}

		void AssertHtml(ZBlob htmlBytes, string htmlExpected)
		{
			AssertEquals(htmlBytes.ToUTF8(), htmlExpected);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TriageAssistBusinessObject(Incident);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Triage = Factory.NewWithValidTestData<IncidentTriage>();
			Triage.IMT_IsActive = true;
			Assist = new TriageAssistBusinessObject(Incident);
		}

		SupportIncident Incident;
		TriageAssistBusinessObject Assist;
		IncidentTriage Triage;
	}
}
