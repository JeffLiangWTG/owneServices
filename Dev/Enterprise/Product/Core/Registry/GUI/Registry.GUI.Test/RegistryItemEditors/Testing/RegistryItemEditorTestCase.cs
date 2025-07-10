using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestsSubclassesOf(typeof(RegistryItemEditor))]
	public abstract class RegistryItemEditorTestCase : TestCaseWithFactory
	{
		[GuiTest]
		[RequiresSTA]
		public void TestEditorPaneFitsOnRegistryForm()
		{
			using (RegistryFormForTest registryForm = GetNewRegistryFormForTest(RegistryItem))
			{
				registryForm.Show();
				registryForm.Width = registryForm.MinimumSize.Width;
				registryForm.Height = registryForm.MinimumSize.Height;

				registryForm.DisplayRegistryItem();

				int heightDifference = registryForm.PluginControl.Bottom - registryForm.PluginPanel.Height;
				int widthDifference = registryForm.PluginControl.Right - registryForm.PluginPanel.Width;

				Assert(GetScreenDetails("EditorPane is " + heightDifference + " pixels too tall to fit on the Registry Form.", registryForm),
					heightDifference <= 0);
				Assert(GetScreenDetails("EditorPane is " + widthDifference + " pixels too wide to fit on the Registry Form.", registryForm),
					widthDifference <= 0);
			}
		}

		[RequiresSTA]
		public void TestNotifyChangesCallUpdatesHasChanges()
		{
			using (var registryForm = GetNewRegistryFormForTest(RegistryItem))
			{
				registryForm.Show();
				registryForm.DisplayRegistryItem();

				Assert("PRE: HasChanges is false", !registryForm.HasChanges);

				RegistryItemEditor.NotifyChanges(registryForm.PluginControl);

				Assert("NotifyChanges should update the parent form's HasChanges", registryForm.HasChanges);
			}
		}

		[RequiresSTA]
		public void TestNoChangeWhenFormHasNoControl()
		{
			using (var editorPane = Editor.NewWinFormsEditorPane())
			{
				AssertNoExceptionThrown(() => RegistryItemEditor.NotifyChanges(editorPane));
			}
		}

		protected virtual RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		[RequiresSTA]
		public void TestExpectedEditorPaneType()
		{
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.GetType()", ExpectedEditorPaneType, editorPane.GetType());
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestSetAndGetValueFromEditorPane()
		{
			using (ZForm testForm = new ZForm())
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				testForm.Controls.Add(editorPane);
				testForm.Show();

				foreach (object validRegistryValue in GetValidRegistryValues())
				{
					Editor.SetValueFromEditorPane(editorPane, validRegistryValue);
					object getValue = Editor.GetValueFromEditorPane(editorPane);
					AssertSetAndGetValuesEqual(validRegistryValue, getValue);
					AssertGetValueTypeIsRegistryValueType(getValue);

					AssertImplies("When you set the value for the control you should clone it, otherwise you're fiddling with the registry's actual value", CanNotHaveReferenceEquality, !ReferenceEquals(validRegistryValue, getValue));
				}
			}
		}

		protected virtual bool CanNotHaveReferenceEquality
		{
			get
			{
				var dataType = RegistryItem.DataType.DataType;
				return dataType.IsClass && dataType != typeof(string) && dataType != typeof(ZString);
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestEnableEditorPane()
		{
			object value = GetValidRegistryValues()[0];

			using (ZForm testForm = new ZForm())
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				testForm.Controls.Add(editorPane);
				testForm.Show();

				Editor.EnableEditorPane(editorPane, true);
				AssertEquals("EditorPane should be enabled.", true, GetEditorPaneEnabledState(editorPane));

				Editor.SetValueFromEditorPane(editorPane, value);
				AssertEquals("EditorPane should be enabled.", true, GetEditorPaneEnabledState(editorPane));

				Editor.EnableEditorPane(editorPane, false);
				Application.DoEvents();
				AssertEquals("EditorPane should be disabled.", false, GetEditorPaneEnabledState(editorPane));
			}

			using (ZForm testForm = new ZForm())
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				testForm.Controls.Add(editorPane);
				testForm.Show();

				Editor.EnableEditorPane(editorPane, false);
				Application.DoEvents();
				AssertEquals("EditorPane should be disabled.", false, GetEditorPaneEnabledState(editorPane));

				Editor.SetValueFromEditorPane(editorPane, value);
				AssertEquals("EditorPane should be disabled.", false, GetEditorPaneEnabledState(editorPane));

				Editor.EnableEditorPane(editorPane, true);
				AssertEquals("EditorPane should be enabled.", true, GetEditorPaneEnabledState(editorPane));
			}
		}

		[GuiTest]
		[RequiresSTA]
		public virtual void TestEditorPaneLayout()
		{
			using (ZForm testForm = new ZForm())
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				testForm.Controls.Add(editorPane);
				testForm.Show();

				Assert("Precondition: Width should not be 100.", editorPane.Width != ControlDpiScalingHelper.ScaleToCurrentDpiX(100));
				Assert("Precondition: Height should not be 200.", editorPane.Height != ControlDpiScalingHelper.ScaleToCurrentDpiY(200));

				CombineAssertions(FormattableString.Invariant($"You probably want to override {nameof(ExpectedAnchor)} if these assertions fail. It should match RegistryItemEditor.Anchor."), () =>
				{
					switch (ExpectedAnchor)
					{
						case (RegistryItemEditor.EditorPaneAnchor.TopLeft):
							{
								Size originalSize = editorPane.Size;
								Editor.SetEditorPaneLayout(editorPane, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(200));
								AssertEquals("Size", originalSize, editorPane.Size);
								AssertEquals("Anchor", AnchorStyles.Top | AnchorStyles.Left, editorPane.Anchor);

								break;
							}

						case (RegistryItemEditor.EditorPaneAnchor.TopLeftRight):
							{
								int expectedHeight = editorPane.Height;
								Editor.SetEditorPaneLayout(editorPane, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(200));
								AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), editorPane.Width);
								AssertEquals("Anchor", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, editorPane.Anchor);

								break;
							}

						case (RegistryItemEditor.EditorPaneAnchor.All):
							{
								Editor.SetEditorPaneLayout(editorPane, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(200));
								AssertEquals("Size", new Size(ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(200)), editorPane.Size);
								AssertEquals("Anchor", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, editorPane.Anchor);

								break;
							}

						default:
							throw new ArgumentException("Unknown layout type.");
					}
				});
			}
		}

		[ExpectNoExceptions]
		public virtual void TestRegistryItemAcceptsEditorValue()
		{
			foreach (object validRegistryValue in GetValidRegistryValues())
			{
				RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validRegistryValue);
			}
		}

		protected virtual RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeft; }
		}

		protected virtual void AssertGetValueTypeIsRegistryValueType(object getValue)
		{
			AssertEquals("GetValueFromEditorPane().GetType()", RegistryItem.DataType.DataType, getValue.GetType());
		}

		protected virtual void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			BusinessObject setBusinessObject = setValue as BusinessObject;

			if (setBusinessObject != null)
			{
				CheckAllPropertiesInZPropertyInfoHashAreEqual(setBusinessObject, (BusinessObject)getValue);
			}
			else
			{
				BusinessObjectCollection setCollection = setValue as BusinessObjectCollection;
				if (setCollection != null)
				{
					BusinessObject[] setArray = setCollection.ToArray();
					BusinessObject[] getArray = ((BusinessObjectCollection)getValue).ToArray();

					AssertEquals("getValue.Count", setArray.Length, getArray.Length);

					for (int i = 0; i < setArray.Length; ++i)
					{
						CheckAllPropertiesInZPropertyInfoHashAreEqual(setArray[i], getArray[i]);
					}
				}
				else
				{
					Array setArray = setValue as Array;
					if (setArray != null)
					{
						Array getArray = getValue as Array;
						AssertEquals("getValue.Length", setArray.Length, getArray.Length);
						for (int i = 0; i < setArray.Length; i++)
						{
							AssertEquals("getValue[i]", setArray.GetValue(i), getArray.GetValue(i));
						}
					}
					else
					{
						AssertEquals("GetValueFromEditorPane()", setValue, getValue);
					}
				}
			}
		}

		void CheckAllPropertiesInZPropertyInfoHashAreEqual(BusinessObject setValue, BusinessObject getValue)
		{
			foreach (ZPropertyInfo propertyInfo in setValue.ZPropertyInfoHash)
			{
				AssertEquals("GetValue." + propertyInfo.Name, propertyInfo.Value, getValue.ZPropertyInfoHash[propertyInfo.Name].Value);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "Testing")]
		string GetScreenDetails(string prependMessage, RegistryFormForTest form)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine(prependMessage);
			Screen[] screens = Screen.AllScreens;
			for (int i = 0; i < screens.Length; i++)
			{
				result.AppendLine("Screen " + i + " size: " + screens[i].Bounds.Size);
			}
			result.AppendLine("RegistryForm size: " + form.Size);
			result.AppendLine("PluginPanel size: " + form.PluginPanel.Size);
			result.AppendLine("PluginControl size: " + form.PluginControl.Size);
			result.AppendLine("PluginControl location: " + form.PluginControl.Location);

			return result.ToString();
		}

		protected abstract IRegistryItem GetRegistryItemWithSystemStorageLevel();
		protected abstract RegistryItemEditor GetEditor();
		protected abstract Type GetExpectedEditorPaneType();
		protected abstract object[] GetValidRegistryValues();
		protected abstract bool GetEditorPaneEnabledState(Control editorPane);

		#region Properties

		#region Editor

		protected RegistryItemEditor Editor
		{
			get
			{
				if (fEditor == null)
				{
					fEditor = GetEditor();
				}
				return fEditor;
			}
		}

		RegistryItemEditor fEditor;

		#endregion

		#region ExpectedEditorPaneType

		protected Type ExpectedEditorPaneType
		{
			get
			{
				if (fExpectedEditorPaneType == null)
				{
					fExpectedEditorPaneType = GetExpectedEditorPaneType();
				}
				return fExpectedEditorPaneType;
			}
		}

		Type fExpectedEditorPaneType;

		#endregion

		#region RegistryItem

		protected IRegistryItem RegistryItem
		{
			get
			{
				if (fRegistryItem == null)
				{
					fRegistryItem = GetRegistryItemWithSystemStorageLevel();
				}
				return fRegistryItem;
			}
		}

		IRegistryItem fRegistryItem;

		#endregion

		#endregion

		#region RegistryFormForTest

		public class RegistryFormForTest : RegistryForm
		{
			public RegistryFormForTest(IRegistryItem registryItem)
			{
				this.RegistryItem = registryItem;
				FillRegistriesTreeView();
			}

			public virtual void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0];
			}

			public new ZPanel PluginPanel => base.PluginPanel;

			public new ZCheckBox OverrideCheckBox => base.OverrideCheckBox;

			public new Control PluginControl => base.PluginControl;

			public new ZButton SaveButton => base.SaveButton;

			protected override void FillRegistriesTreeView()
			{
				if (RegistryItem != null)
				{
					TreeNode node = new TreeNode();
					node.Tag = new RegistryItemTag(RegistryItem);
					RegistriesTreeView.Nodes.Add(node);
				}
			}
			protected override DialogResult GetSaveDialogResult()
			{
				base.GetSaveDialogResult();
				return OnSaveResult;
			}

			public DialogResult OnSaveResult;

			protected IRegistryItem RegistryItem;
		}

		#endregion
	}
}
