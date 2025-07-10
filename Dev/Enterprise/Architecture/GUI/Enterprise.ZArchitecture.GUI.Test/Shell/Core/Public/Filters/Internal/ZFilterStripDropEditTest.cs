using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZFilterStripDropEditTest : TestCaseWithFactory
	{
		public void TestResetsAfterValueWasSelected()
		{
			var dummy = new DummyWithList();
			using (var dropEdit = new ZFilterStripDropEdit())
			using (var control = new ZFilterStripDropForm(dropEdit))
			{
				dropEdit.DataSourceType = typeof(DummyWithList);
				dropEdit.SetDataBinding(dummy, "SomeCode");

				control.Show();

				AssertEquals("List should be full", 8, control.List_Exposed.Count);

				dropEdit.ProposedText = "A";
				control.RefreshList();

				AssertEquals("Should filter 'DEF'", 7, control.List_Exposed.Count);

				dropEdit.ProposedText = "AABC";
				control.RefreshList();

				AssertEquals("Should reset since we have a match", 8, control.List_Exposed.Count);

				dropEdit.ProposedText = "SPA";
				control.RefreshList();

				AssertEquals("Should filter 'SPACE'", 3, control.List_Exposed.Count);

				dropEdit.ProposedText = "SPACE";
				control.RefreshList();

				AssertEquals("Should reset since we have a match", 8, control.List_Exposed.Count);

				dropEdit.ProposedText = " SPACE";
				control.RefreshList();

				AssertEquals("Should reset since we have a match", 8, control.List_Exposed.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestShowingControl()
		{
			using (var form = new ZForm())
			{
				var control = new ZFilterStripDropEdit();
				control.BindTo = "Z0_Description";
				control.BindToList = "Z0_Description_List";

				form.Controls.Add(control);
				form.Show();
			}
		}

		public void TestDescriptionBoxTopPosition()
		{
			using (var form = new ZForm())
			{
				var control = new ZFilterStripDropEdit
				{
					BindTo = "Z0_Description",
					BindToList = "Z0_Description_List"
				};

				form.Controls.Add(control);
				form.Show();

				AssertEquals(control.DropButton.Top, control.DescriptionBox.Top);
			}
		}

		public void TestDropButtonWidth()
		{
			using (var form = new ZForm())
			{
				var control = new ZFilterStripDropEdit
				{
					BindTo = "Z0_Description",
					BindToList = "Z0_Description_List"
				};

				form.Controls.Add(control);
				form.Show();

				control.ShowDescriptionBox = false;
				AssertEquals(control.Width, control.DropButton.Width);
			}
		}

		public void TestControltHeightIsSufficient()
		{
			using (var control = new ZFilterStripDropEdit())
			{
				control.Height = 18;
				control.CodeBox.Height = 20;
				AssertEquals($"Control height ({control.Height}) should be equal to CodeBox height ({control.CodeBox.Height})", 20, control.Height);
			}
		}

		#region IncrementalSearch

		public void TestAutoCompletingIncrementalSearch()
		{
			using (var form = new ZForm())
			{
				var filters = new ModuleFilterCollection();
				filters.AddNumberRangeFilter("Number Range (Decimal)", DummyBizoSchema.Z0_Decimal);
				filters.AddNumberRangeFilter("Number Range (Short)", DummyBizoSchema.Z0_Short);
				filters.AddTextRangeFilter("Declaration", DummyBizoSchema.Z0_Code);
				filters.AddTextRangeFilter("Description", DummyBizoSchema.Z0_Description);

				var strip = new FilterStrip(filters);

				var parent = new ZFilterStripDropEdit();

				parent.SetDataBinding(strip, "FilterDescriptionLocalized");
				parent.BindToList = "FilterDescriptionLocalizedList";
				parent.CodeBox.CharacterCasing = CharacterCasing.Lower;

				parent.Parent = form;
				form.Show();

				AssertEquals(true, parent.CodeBox.Focused);
				AssertEquals("", parent.CodeBox.Text);

				form.Controls[0].Focus();
				AssertEquals(false, parent.CodeBox.Focused);
				AssertEquals(FilterStrip.SelectFilterDescriptionText, parent.CodeBox.Text);

				parent.CodeBox.Focus();

				var boundList = new string[]
					{
						"",
						"Numbers and References",
						"Number Range (Decimal)",
						"Number Range (Short)",
						"",
						"Text Search",
						"Declaration",
						"Description",
					};

				parent.DropButton.ShowDropDown(true);
				AssertEquals(true, parent.DropButton.IsDroppedDown);

				AssertEquals("Ensure bound list matched", FormatFilterList(boundList), FormatFilterList(strip));
				AssertEquals("Base list matched to bound list", FormatFilterList(boundList), FormatFilterList(parent.List));
				AssertEquals("Shown list matched to bound list", FormatFilterList(boundList), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				var expected = Array.Empty<string>();

				KeySender.SendKeyPress(parent.CodeBox, Keys.D);
				AssertEquals("d", parent.CodeBox.Text);
				expected = new string[]
				{
					"",
					"Numbers and References",
					"Number Range (Decimal)",
					"",
					"Text Search",
					"Declaration",
					"Description",
				};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyDownToProcessCmdKey(parent.CodeBox, Keys.Enter);
				AssertEquals("declaration", parent.CodeBox.Text);

				parent.CodeBox.Text = "";

				KeySender.SendKeyPress(parent.CodeBox, Keys.D);
				KeySender.SendKeyPress(parent.CodeBox, Keys.E);
				KeySender.SendKeyPress(parent.CodeBox, Keys.C);
				KeySender.SendKeyPress(parent.CodeBox, Keys.L);
				AssertEquals("decl", parent.CodeBox.Text);
				expected = new string[]
					{
						"",
						"Text Search",
						"Declaration",
					};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyDownToProcessCmdKey(parent.CodeBox, Keys.Tab);
				AssertEquals("declaration", parent.CodeBox.Text);

				parent.CodeBox.Text = "";

				KeySender.SendKeyPress(parent.CodeBox, Keys.T);
				AssertEquals("t", parent.CodeBox.Text);
				expected = new string[]
					{
						"",
						"Numbers and References",
						"Number Range (Short)",
						"",
						"Text Search",
						"Declaration",
						"Description",
					};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyDownToProcessCmdKey(parent.CodeBox, Keys.Enter);
				AssertEquals("declaration", parent.CodeBox.Text);
			}
		}

		public void TestIncrementalSearch()
		{
			using (var form = new ZForm())
			{
				var filters = new ModuleFilterCollection();
				filters.AddNumberRangeFilter("Number Range (Decimal)", DummyBizoSchema.Z0_Decimal);
				filters.AddNumberRangeFilter("Number Range (Short)", DummyBizoSchema.Z0_Short);
				filters.AddTextRangeFilter("Declaration", DummyBizoSchema.Z0_Code);
				filters.AddTextRangeFilter("Description", DummyBizoSchema.Z0_Description);

				var strip = new FilterStrip(filters);
				var parent = new ZFilterStripDropEdit();

				parent.SetDataBinding(strip, "FilterDescriptionLocalized");
				parent.BindToList = "FilterDescriptionLocalizedList";
				parent.CodeBox.CharacterCasing = CharacterCasing.Lower;

				parent.Parent = form;
				form.Show();
				parent.CodeBox.Focus();

				var boundList = new string[]
					{
						"",
						"Numbers and References",
						"Number Range (Decimal)",
						"Number Range (Short)",
						"",
						"Text Search",
						"Declaration",
						"Description",
					};

				parent.DropButton.ShowDropDown(true);
				AssertEquals(true, parent.DropButton.IsDroppedDown);

				AssertEquals("Ensure bound list matched", FormatFilterList(boundList), FormatFilterList(strip));
				AssertEquals("Base list matched to bound list", FormatFilterList(boundList), FormatFilterList(parent.List));
				AssertEquals("Shown list matched to bound list", FormatFilterList(boundList), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				var expected = Array.Empty<string>();

				KeySender.SendKeyPress(parent.CodeBox, Keys.D);
				AssertEquals("d", parent.CodeBox.Text);
				expected = new string[]
				{
					"",
					"Numbers and References",
					"Number Range (Decimal)",
					"",
					"Text Search",
					"Declaration",
					"Description",
				};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyPress(parent.CodeBox, Keys.E);
				AssertEquals("de", parent.CodeBox.Text);
				expected = new string[]
				{
					"",
					"Numbers and References",
					"Number Range (Decimal)",
					"",
					"Text Search",
					"Declaration",
					"Description",
				};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyPress(parent.CodeBox, Keys.C);
				AssertEquals("dec", parent.CodeBox.Text);
				expected = new string[]
					{
						"",
						"Numbers and References",
						"Number Range (Decimal)",
						"",
						"Text Search",
						"Declaration",
					};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyPress(parent.CodeBox, Keys.I);
				AssertEquals("deci", parent.CodeBox.Text);
				expected = new string[]
					{
						"",
						"Numbers and References",
						"Number Range (Decimal)",
					};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyPress(parent.CodeBox, Keys.Back);
				AssertEquals("dec", parent.CodeBox.Text);
				expected = new string[]
					{
						"",
						"Numbers and References",
						"Number Range (Decimal)",
						"",
						"Text Search",
						"Declaration",
					};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyPress(parent.CodeBox, Keys.L);
				AssertEquals("decl", parent.CodeBox.Text);
				expected = new string[]
					{
						"",
						"Text Search",
						"Declaration",
					};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				parent.CodeBox.SelectAll();
				KeySender.SendKeyPress(parent.CodeBox, Keys.Back);
				AssertEquals(String.Empty, parent.CodeBox.Text);
				AssertEquals("Shown list matched to bound list", FormatFilterList(boundList), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyPress(parent.CodeBox, Keys.D);
				KeySender.SendKeyPress(parent.CodeBox, Keys.E);
				KeySender.SendKeyPress(parent.CodeBox, Keys.C);
				KeySender.SendKeyPress(parent.CodeBox, Keys.L);
				AssertEquals("decl", parent.CodeBox.Text);
				expected = new string[]
					{
						"",
						"Text Search",
						"Declaration",
					};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				parent.DropButton.HideDropDown();
				parent.DropButton.ShowDropDown(true);
				AssertEquals("decl", parent.CodeBox.Text);
				expected = new string[]
					{
						"",
						"Text Search",
						"Declaration",
					};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));
			}
		}

		public void TestIncrementalSearch_FullMatch()
		{
			using (var form = new ZForm())
			{
				var filters = new ModuleFilterCollection();
				filters.AddNumberFilter("Code Mapping", DummyBizoSchema.Z0_VarCharMax);
				filters.AddTextRangeFilter("Code", DummyBizoSchema.Z0_Code);
				filters.AddTextRangeFilter("Description", DummyBizoSchema.Z0_Description);

				var strip = new FilterStrip(filters);

				var parent = new ZFilterStripDropEdit();

				parent.SetDataBinding(strip, "FilterDescriptionLocalized");
				parent.BindToList = "FilterDescriptionLocalizedList";
				parent.CodeBox.CharacterCasing = CharacterCasing.Lower;

				parent.Parent = form;
				form.Show();
				parent.CodeBox.Focus();

				var boundList = new string[]
				{
					"",
					"Numbers and References",
					"Code Mapping",
					"",
					"Text Search",
					"Code",
					"Description",
				};

				parent.DropButton.ShowDropDown(true);
				AssertEquals("Shown list matched to bound list", FormatFilterList(boundList), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyPress(parent.CodeBox, Keys.C);
				KeySender.SendKeyPress(parent.CodeBox, Keys.O);
				KeySender.SendKeyPress(parent.CodeBox, Keys.D);
				AssertEquals("cod", parent.CodeBox.Text);

				var expected = new string[]
				{
					"",
					"Numbers and References",
					"Code Mapping",
					"",
					"Text Search",
					"Code",
				};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));

				KeySender.SendKeyPress(parent.CodeBox, Keys.E);
				AssertEquals("code", parent.CodeBox.Text);

				expected = new string[]
				{
					"",
					"Numbers and References",
					"Code Mapping",
					"",
					"Text Search",
					"Code",
					"Description",
				};
				AssertEquals(String.Format("Shown list filtered '{0}'", parent.CodeBox.Text), FormatFilterList(expected), FormatFilterList(parent.DropButton.DropDown_Exposed.List_Exposed));
				AssertEquals("Code", parent.DropButton.DropDown_Exposed.SelectedItem.Code);
			}
		}

		public void TestNoGroup()
		{
			var dummy = new DummyWithList();
			using (var dropEdit = new ZFilterStripDropEdit())
			using (var control = new ZFilterStripDropForm(dropEdit))
			{
				dropEdit.DataSourceType = typeof(DummyWithList);
				dropEdit.SetDataBinding(dummy, "NoCategoryCode");

				control.Show();

				AssertEquals("List should be full", 5, control.List_Exposed.Count);

				dropEdit.ProposedText = "A";
				control.RefreshList();

				AssertEquals("Should filter 'DEF'", 4, control.List_Exposed.Count);

				dropEdit.ProposedText = "AABC";
				control.RefreshList();

				AssertEquals("Should reset since we have a match", 5, control.List_Exposed.Count);
			}
		}

		string FormatFilterList(string[] list)
		{
			return String.Join(System.Environment.NewLine, list);
		}

		string FormatFilterList(FilterStrip strip)
		{
			return String.Join(System.Environment.NewLine, strip.FilterDescriptionLocalizedList.Cast<CodeDescriptionPair>().Select(p => p.Code));
		}

		string FormatFilterList(IList codeDescriptionList)
		{
			return String.Join(System.Environment.NewLine, codeDescriptionList.OfType<ICodeDescription>().Select(pair => pair.Code));
		}

		#endregion // IncrementalSearch
	}
}
