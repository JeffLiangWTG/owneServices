using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.ResourceStrings.Module
{
	public class ResourceStringsFilterControlTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetTranslationForResource()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			mockData.Put("K1", new ResourceStringData("K1", string.Empty, string.Empty, "C1", string.Empty));
			var h1 = ResourceStringsFactory.Lookup(Res.DefaultLanguage, "K1");

			using (Business.ResourceStrings.Instance.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				HelpDataString h2 = ResourceStringsFilterControl.GetTranslation(h1);

				Assert(!h1.HD_IsCheckedOut);

				AssertEquals("K1", h2.HD_Code);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, h2.HD_Language);
				AssertEquals("C1", h2.HD_Caption);
				Assert(h2.HD_IsCheckedOut);
			}
		}

		public void TestGetTranslationForResourceWithTranslation()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			mockData.Put("K1", new ResourceStringData("K1", string.Empty, string.Empty, "C1", string.Empty));
			var h1 = ResourceStringsFactory.Lookup(Res.DefaultLanguage, "K1");

			mockData = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.ChineseSimplified);
			mockData.Put("K1", new ResourceStringData("K1", string.Empty, string.Empty, "C2", string.Empty));
			var h2 = ResourceStringsFactory.Lookup(Core.SharedConstants.Languages.ChineseSimplified, "K1");

			using (Business.ResourceStrings.Instance.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				HelpDataString h3 = ResourceStringsFilterControl.GetTranslation(h1);

				Assert(!h1.HD_IsCheckedOut);
				Assert(!h2.HD_IsCheckedOut);

				AssertEquals("K1", h3.HD_Code);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, h3.HD_Language);
				AssertEquals("C2", h3.HD_Caption);
			}
		}

		public void TestGetTranslationForesourceWithCheckedOutTranslation()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			mockData.Put("K1", new ResourceStringData("K1", string.Empty, string.Empty, "C1", string.Empty));
			var h1 = ResourceStringsFactory.Lookup(Res.DefaultLanguage, "K1");

			var h2 = new HelpDataString();
			h2.HD_Language = Core.SharedConstants.Languages.ChineseSimplified;
			h2.HD_Code = "K1";
			h2.HD_Caption = "C2";
			ResourceStringsFactory.Save("TST", h2);

			using (Business.ResourceStrings.Instance.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				HelpDataString h3 = ResourceStringsFilterControl.GetTranslation(h1);

				Assert(h2.HD_IsCheckedOut);

				AssertEquals("K1", h3.HD_Code);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, h3.HD_Language);
				AssertEquals("C2", h3.HD_Caption);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			mockSources = ResourceStringsFactory.MockSources();
			base.SetUp();
		}

		protected override void TearDown()
		{
			if (mockSources != null)
			{
				mockSources.Dispose();
			}
			base.TearDown();
		}

		IDisposable mockSources;

		#endregion
	}
}
