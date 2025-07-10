using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Test
{
	public class SplitterStateTest : TestCaseWithFactory
	{
		public void TestPersist()
		{
			AssertSplitter("Form.Panel1Fixed", "Splitter", FixedPanel.Panel1, 27, 27, 28);
			AssertSplitter("Form.Panel1Fixed", "Splitter", FixedPanel.Panel1, 49, 28, 29);
			AssertSplitter("AnotherForm11111", "Splitter", FixedPanel.Panel1, 30, 30, 30);
			AssertSplitter("Form.Panel1Fixed", "Splitter", FixedPanel.Panel1, 45, 29, 29);

			AssertSplitter("Form.Panel2Fixed", "Splitter", FixedPanel.Panel2, 27, 27, 28);
			AssertSplitter("Form.Panel2Fixed", "Splitter", FixedPanel.Panel2, 49, 28, 29);
			AssertSplitter("AnotherForm22222", "Splitter", FixedPanel.Panel2, 30, 30, 30);
			AssertSplitter("Form.Panel2Fixed", "Splitter", FixedPanel.Panel2, 45, 29, 29);

			AssertSplitter("Form.NoneFixed__", "Splitter", FixedPanel.None, 27, 27, 28);
			AssertSplitter("Form.NoneFixed__", "Splitter", FixedPanel.None, 49, 28, 29);
			AssertSplitter("AnotherForm33333", "Splitter", FixedPanel.None, 30, 30, 30);
			AssertSplitter("Form.NoneFixed__", "Splitter", FixedPanel.None, 45, 29, 29);

			AssertSplitter("Form.Limits", "Splitter", FixedPanel.None, 10, 25, 90);
			AssertSplitter("Form.Limits", "Splitter", FixedPanel.None, 90, 75, 75);
		}

		void AssertSplitter(string formName, string splitterName, FixedPanel fixedPanel, int createDistance, int restoredDistance, int finalDistance)
		{
			using (ZForm form = new ZForm())
			{
				form.Name = formName;
				form.ClientSize = new System.Drawing.Size(200, 400);

				SplitContainer splitter = new SplitContainer();
				splitter.Name = splitterName;
				splitter.Size = new System.Drawing.Size(100, 200);
				splitter.Panel1MinSize = 25;
				splitter.Panel2MinSize = 24;
				splitter.SplitterWidth = 1;
				splitter.FixedPanel = fixedPanel;
				splitter.SplitterDistance = createDistance;

				form.Controls.Add(splitter);
				form.Show();

				SplitterState.Persist(splitter);
				Application.DoEvents();
				AssertEquals("Distance restored", restoredDistance, splitter.SplitterDistance);
				splitter.SplitterDistance = finalDistance;
				form.Close();
			}
		}
	}
}
