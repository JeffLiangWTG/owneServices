using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI.Layout;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KSplitterTest : TestCase
	{
#if !WINZOR
		[ExpectNoExceptions]
		public void TestConstructAndReflectWithoutCrashing()
		{
			using (KForm form = new KForm())
			{
				var kSplitter = new KSplitter();
				form.Controls.Add(kSplitter);

				Application.DoEvents();

				var splitTargetReflection = typeof(KSplitter).GetField("splitTarget", BindingFlags.NonPublic | BindingFlags.Instance);
				splitTargetReflection.SetValue(kSplitter, form);

				var drawSplitHelperReflection = typeof(KSplitter).GetMethod("DrawSplitHelper", BindingFlags.NonPublic | BindingFlags.Instance);
				drawSplitHelperReflection.Invoke(kSplitter, new object[] { 0 });
				Application.DoEvents();
				drawSplitHelperReflection.Invoke(kSplitter, new object[] { 1 });
				Application.DoEvents();
				drawSplitHelperReflection.Invoke(kSplitter, new object[] { 2 });
				Application.DoEvents();
				drawSplitHelperReflection.Invoke(kSplitter, new object[] { 3 });
				Application.DoEvents();
				drawSplitHelperReflection.Invoke(kSplitter, new object[] { 4 });
				Application.DoEvents();
			}
		}
#endif

		public void TestDoNotSaveSplitterLayout()
		{
			using (var splitter = new KSplitter())
			{
				var splitterLayoutSaveProvider = (ISplitterLayoutSaveProvider)splitter;

				Assert("Default condition", !splitter.DoNotSaveSplitterLayout);
				Assert(!splitterLayoutSaveProvider.IsSplitterFixed);

				splitter.DoNotSaveSplitterLayout = true;
				Assert(splitterLayoutSaveProvider.IsSplitterFixed);
			}
		}
	}
}
