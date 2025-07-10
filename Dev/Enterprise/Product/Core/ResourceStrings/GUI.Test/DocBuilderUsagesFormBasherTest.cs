using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	[TestedType(typeof(DocBuilderUsagesForm))]
	sealed class DocBuilderUsagesFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DocBuilderUsagesForm(new HelpDataString());
		}

		#endregion

		public void TestDocBuilderUsagesSanity()
		{
			docBuilderUsageFinderSubstitution.Dispose();
			ObjectFactory.Get<IDocBuilderUsageFinder>().Initialize();

			var data = Array.ConvertAll(ResourceStringsFactory.Lookup(Res.DefaultLanguage, "DocLabel|VG90YWw=").DocBuilderUsages.ToArray(), item => (IDocBuilderUsage)item);
			Assert("DocLabel string should have DocBuilderUsages", data.Length > 0);
			Assert("All usages should have a macro value", Array.Find(data, item => item.Macro == string.Empty) == null);
			Assert("All usages should have a template name", Array.Find(data, item => item.TemplateName == string.Empty) == null);
			Assert("At least one template should be mapped to a document", Array.Find(data, item => item.AllDocumentNames != string.Empty) != null);

			data = Array.ConvertAll(ResourceStringsFactory.Lookup(Res.DefaultLanguage, "191979be-0dd6-4aed-b675-b96ae9a8c71f").DocBuilderUsages.ToArray(), item => (IDocBuilderUsage)item);
			Assert("DocBuilder macro string should have DocBuilderUsages", data.Length > 0);
			Assert("All usages should have a macro value", Array.Find(data, item => item.Macro == string.Empty) == null);
			Assert("All usages should have a template name", Array.Find(data, item => item.TemplateName == string.Empty) == null);
			Assert("At least one template should be mapped to a document", Array.Find(data, item => item.AllDocumentNames != string.Empty) != null);
		}

		protected override void SetUp()
		{
			if (!ObjectFactory.HasBeenSubstituted<IDocBuilderUsageFinder>())
			{
				var mockDocBuilderUsageFinder = new Mock<IDocBuilderUsageFinder>();
				docBuilderUsageFinderSubstitution = ObjectFactory.Substitute<IDocBuilderUsageFinder>(mockDocBuilderUsageFinder.Object);
			}
		}

		IDisposable docBuilderUsageFinderSubstitution;
	}
}
