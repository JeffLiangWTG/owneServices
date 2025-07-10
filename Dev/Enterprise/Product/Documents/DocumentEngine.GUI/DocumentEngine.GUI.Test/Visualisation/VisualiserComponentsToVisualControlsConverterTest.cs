using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	sealed class VisualiserComponentsToVisualControlsConverterTest : NUnit.Framework.TestCase
	{
		public void TestGetCommonText()
		{
			using (var control = new TabPage())
			{
				VisualiserComponentsToVisualControlsConverter visualiser = new VisualiserComponentsToVisualControlsConverter(control);
				List<string> strs = new List<string>();

				AssertEquals("", visualiser.GetCommonText(strs));

				strs.Add("ABC");
				strs.Add("ACC");
				strs.Add("ADD");
				strs.Add("AEE");

				AssertEquals("A", visualiser.GetCommonText(strs));

				strs.Clear();
				strs.Add("ABCABC");
				strs.Add("ABCACC");
				strs.Add("ABCADD");
				strs.Add("ABCAEE");

				AssertEquals("ABCA", visualiser.GetCommonText(strs));

				strs.Clear();
				strs.Add("ABCABC");
				strs.Add("ABCACC");
				strs.Add("");
				strs.Add("ABCADD");
				strs.Add("ABCAEE");

				AssertEquals("", visualiser.GetCommonText(strs));

				strs.Clear();
				strs.Add("XBCABC");
				strs.Add("ABCACC");
				strs.Add("ABCADD");
				strs.Add("ABCAEE");

				AssertEquals("", visualiser.GetCommonText(strs));

				strs.Clear();
				strs.Add("ABCABC");
				strs.Add("abcacc");
				strs.Add("ABCADD");
				strs.Add("ABCAEE");

				AssertEquals("", visualiser.GetCommonText(strs));

				strs.Clear();
				strs.Add("ABCABC");

				AssertEquals("", visualiser.GetCommonText(strs));
			}
		}
	}
}
