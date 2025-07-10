using System;
using System.Linq;
using CargoWise.ResourceStrings.Cache.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.Testing
{
	[TestedType(typeof(ResourceStringsFilterBusinessObject))]
	public class ResourceStringsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLanguages()
		{
			ResourceStringsFilterBusinessObject bo = (ResourceStringsFilterBusinessObject)GetNewBusinessObject();
			AssertEquals(OLookUpEditType.Language, bo.Languages.LookupEditType);
		}

		public void TestCheckOutStatus()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			mockData.Put("checkedout", new ResourceStringData("checkedout", "checkedin"));

			HelpDataString resourceString1 = new HelpDataString();
			resourceString1.HD_Language = Core.SharedConstants.Languages.French;
			resourceString1.HD_Code = "checkedout";
			resourceString1.HD_Caption = "checkedout";
			resourceString1.HD_IsCheckedOut = true;
			ResourceStringsFactory.Save("TST", resourceString1);

			mockData.Put("checkedin", new ResourceStringData("checkedin", "checkedin"));

			ResourceStringsFilterBusinessObject bo = (ResourceStringsFilterBusinessObject)GetNewBusinessObject();
			((ModuleTextFilter)bo["Check Out Status"]).IsActive = true;

			((ModuleTextFilter)bo["Check Out Status"]).Property = ResourceStringsFilterBusinessObject.CheckOutStatus.CheckedOut;
			var results = ResourceStringsFactory.Load(bo.Filter);
			AssertEquals("One item found", 1, results.Length);
			AssertEquals("Checked out item found", "checkedout", ResourceStringsFactory.Load(bo.Filter)[0].HD_Code);
			AssertEquals("Checked out item found", "checkedout", ResourceStringsFactory.Load(bo.Filter)[0].HD_Caption);

			((ModuleTextFilter)bo["Check Out Status"]).Property = ResourceStringsFilterBusinessObject.CheckOutStatus.NotCheckedOut;
			results = ResourceStringsFactory.Load(bo.Filter);
			AssertEquals("One item found", 2, results.Length);
			AssertEquals("Non-Checked out item found", "checkedin", ResourceStringsFactory.Load(bo.Filter)[0].HD_Caption);
			AssertEquals("Non-Checked out item found", "checkedin", ResourceStringsFactory.Load(bo.Filter)[1].HD_Caption);

			((ModuleTextFilter)bo["Check Out Status"]).Property = ResourceStringsFilterBusinessObject.CheckOutStatus.AllFiles;
			results = ResourceStringsFactory.Load(bo.Filter);
			AssertEquals("Two items found", 3, results.Length);
			AssertNotNull("Checked out item found", Array.Find(ResourceStringsFactory.Load(bo.Filter), item => item.HD_Code == "checkedout"));
			AssertNotNull("Non-Checked out item found", Array.Find(ResourceStringsFactory.Load(bo.Filter), item => item.HD_Code == "checkedin"));
		}

		public void TestCaptionOrDescription()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			HelpDataString resourceString2 = new HelpDataString();
			mockData.Put("checkedin", new ResourceStringData("key", "ShortCaption", "MidCaption", "Caption", "FullDescription"));

			const string captionOrDescription = "Caption or Description";
			ResourceStringsFilterBusinessObject bo = (ResourceStringsFilterBusinessObject)GetNewBusinessObject();
			((ModuleTextFilter)bo[captionOrDescription]).IsActive = true;

			((ModuleTextFilter)bo[captionOrDescription]).Property = "ShortCaption";
			AssertEquals("Match on ShortCaption", 1, ResourceStringsFactory.Load(bo.Filter).Length);
			((ModuleTextFilter)bo[captionOrDescription]).Property = "MidCaption";
			AssertEquals("Match on MidCaption", 1, ResourceStringsFactory.Load(bo.Filter).Length);
			((ModuleTextFilter)bo[captionOrDescription]).Property = "Caption";
			AssertEquals("Match on ShortCaption", 1, ResourceStringsFactory.Load(bo.Filter).Length);
			((ModuleTextFilter)bo[captionOrDescription]).Property = "FullDescription";
			AssertEquals("Match on FullDescription", 1, ResourceStringsFactory.Load(bo.Filter).Length);
			((ModuleTextFilter)bo[captionOrDescription]).Property = "Miss";
			AssertEquals(null, 0, ResourceStringsFactory.Load(bo.Filter).Length);
		}

		public void TestFailedLastTest()
		{
			var wasRunningOnDAT = TestingState.IsRunningOnDAT;
			TestingState.IsRunningOnDAT = false;
			ResourceStringContentTestTracker.ResetLastFailures();
			try
			{
				var mockGrm = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.German);
				mockGrm.Put("pass", new ResourceStringData("pass", "pass"));
				mockGrm.Put("fail", new ResourceStringData("fail", "fail"));
				var mockFrn = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French);
				mockFrn.Put("fail", new ResourceStringData("fail", "pass"));

				ResourceStringContentTestTracker.LogFailure(Core.SharedConstants.Languages.German, "fail", "", "", "");

				var bo = (ResourceStringsFilterBusinessObject)GetNewBusinessObject();
				((ModuleFlagsFilter)bo["String Content Test Failures"]).IsActive = true;
				((ModuleFlagsFilter)bo["String Content Test Failures"])["Failed Last String Content Unit Test"] = true;
				var results = ResourceStringsFactory.Load(bo.Filter);
				AssertEquals(1, results.Length);
				AssertEquals("fail", results[0].HD_Code);
				AssertEquals(Core.SharedConstants.Languages.German, results[0].HD_Language);

				((ModuleFlagsFilter)bo["String Content Test Failures"])["Failed Last String Content Unit Test"] = false;
				results = ResourceStringsFactory.Load(bo.Filter);
				AssertEquals(2, results.Length);
			}
			finally
			{
				ResourceStringContentTestTracker.ResetLastFailures();
				TestingState.IsRunningOnDAT = wasRunningOnDAT;
			}
		}

		public void TestEditReason()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			mockData.Put("p1", new ResourceStringData("p1", "p1"));
			mockData.Put("p2", new ResourceStringData("p2", "p2"));
			mockData.Put("t1", new ResourceStringData("t1", "t1"));
			mockData.Put("t2", new ResourceStringData("t2", "t2"));

			ResourceStringsFactory.Save(EditReasons.Codes.TradosPreview, new HelpDataString() { HD_Code = "p1", HD_Language = Core.SharedConstants.Languages.German, HD_Caption = "Preview 1" }, new HelpDataString() { HD_Code = "p2", HD_Language = Core.SharedConstants.Languages.German, HD_Caption = "Preview 2" });
			ResourceStringsFactory.Save(EditReasons.Codes.CustomizableDataTranslation, new HelpDataString() { HD_Code = "t1", HD_Language = Core.SharedConstants.Languages.German, HD_Caption = "Translate 1" }, new HelpDataString() { HD_Code = "t2", HD_Language = Core.SharedConstants.Languages.German, HD_Caption = "Translate 2" });
			var bo = (ResourceStringsFilterBusinessObject)GetNewBusinessObject();
			bo["Edit Reason"].IsActive = true;
			((ModuleTextFilter)bo["Edit Reason"]).Property = EditReasons.Codes.TradosPreview;
			AssertContainsExactElementsInAnyOrder(new string[] { "p1", "p2" }, ResourceStringsFactory.Load(bo.Filter).Select(hs => (string)hs.HD_Code));
			((ModuleTextFilter)bo["Edit Reason"]).Property = EditReasons.Codes.CustomizableDataTranslation;
			AssertContainsExactElementsInAnyOrder(new string[] { "t1", "t2" }, ResourceStringsFactory.Load(bo.Filter).Select(hs => (string)hs.HD_Code));
		}

		public void TestCustomSqlFilter_ShouldNotBeAdded()
		{
			var filterBizo = new ResourceStringsFilterBusinessObject();

			AssertEquals(false, filterBizo.Any(x => x is ModuleSQLFilter));
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ResourceStringsFilterBusinessObject();
		}

		protected override void SetUp()
		{
			mockSources = ResourceStringsFactory.MockSources();
			base.SetUp();
			ResourceStringsFactory.InLastFailures = ResourceStringContentTestTracker.InLastFailures;
		}

		protected override void TearDown()
		{
			ResourceStringsFactory.InLastFailures = null;

			if (mockSources != null)
			{
				mockSources.Dispose();
			}
			base.TearDown();
		}

		IDisposable mockSources;

		#endregion

		protected override bool ShouldBeLocalizable
		{
			get { return false; }
		}
	}
}
