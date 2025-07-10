using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZLabelCaptionRendererTest : TestCase
	{
		public void TestFont()
		{
			AssertEquals(OFont.GetFont(), Renderer.Font);
		}

		public void TestIsCaptionOverridden()
		{
			AssertEquals("Captions not overridden initially", false, Renderer.IsCaptionOverridden);
			Renderer.Captions = new string[] { "NewCaption" };
			AssertEquals("Captions are overridden", true, Renderer.IsCaptionOverridden);
		}

		public void TestICaptionRenderingSupport()
		{
			TextBox.Text = "Caption";
			UserControl.Controls.Add(TextBox);
			Form.Controls.Add(UserControl);

			renderer = null;
			UserControl.CaptionRenderingEnabled = null; // the default
			Form.CaptionRenderingEnabled = true;
			AssertEquals("Caption found if caption rendering is supported", 3, Renderer.Captions.Length);

			renderer = null;
			Form.CaptionRenderingEnabled = false;
			AssertEquals("No captions if caption rendering is not supported", 0, Renderer.Captions.Length);

			renderer = null;
			UserControl.CaptionRenderingEnabled = true;
			AssertEquals("No captions if caption rendering is not supported", 3, Renderer.Captions.Length);
		}

		public void TestICaptionRenderingSupport_QueriedOnParent()
		{
			UserControl.Controls.Add(TextBox);
			Form.Controls.Add(UserControl);

			Form.CaptionRenderingEnabled = false; // parent
			UserControl.CaptionRenderingEnabled = true; // control under test
			AssertEquals("CaptionRenderingEnabled queried on parent, not on control itself", 0, new TestLabelCaptionRenderer(UserControl).Captions.Length);
		}

		public void TestUpdatesWhenBindingsModified()
		{
			TextBox.Text = "Caption";
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetLeft(TextBox, 200, true);
			Form.Controls.Add(TextBox);
			var activeRenderer = Renderer;
			Form.Show();
			Application.DoEvents();
			FireApplicationIdle();

			AssertEquals("Initial caption", "CAPTION", Renderer.LastCaptionMeasurement.Caption);
			TextBox.Text = "";
			TextBox.Name = "InvalidatingCaptionCache1";
			FireApplicationIdle();
			AssertEquals("Initial caption", "CAPTION", Renderer.LastCaptionMeasurement.Caption);

			TextBox.Name = "InvalidatingCaptionCache2";
			TextBox.DataBindings.Add(new Binding("Text", new object(), ""));
			FireApplicationIdle();
			AssertEquals("Caption updated when bindings modified", "Binding.PropertyName=Text", Renderer.LastCaptionMeasurement.Caption);
		}

		public void TestRefresh()
		{
			TextBox.Text = "Caption";
			TextBox.Left = 200;
			Form.Controls.Add(TextBox);
			var activeRenderer = Renderer;
			Form.Show();
			Application.DoEvents();

			ForcePaintEvenWhenComputerLocked(Form);
			AssertEquals("Initial caption", "CAPTION", Renderer.LastCaptionMeasurement.Caption);
			TextBox.Text = "UpdatedCaption";
			AssertEquals("Caption is not automatically updated", "CAPTION", Renderer.LastCaptionMeasurement.Caption);

			Renderer.Refresh();
			FireApplicationIdle();
			AssertEquals("Caption is updated after Refresh() called", "UPDATEDCAPTION", Renderer.LastCaptionMeasurement.Caption);
		}

		#region CopyCaptionToPropertyHumanReadableName

		public void TestCopyCaptionToPropertyHumanReadableName()
		{
			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			using (var mockData = Res.UseMockData())
			using (var testForm = new ZForm(dummy))
			using (var editControl = new ZTextBox())
			using (var textBox = new ZTextBox())
			using (var findBox = new ZGuidFindBox())
			{
				editControl.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				editControl.Location = new Point(200, 10);
				editControl.BindTo = DummyBusinessObject.Schema.Z0_VarCharMax;
				editControl.CaptionResourceString = Res.GetData("EditControl", "GUI Caption");

				textBox.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				textBox.Location = new Point(200, 40);
				textBox.BindTo = DummyBusinessObject.Schema.Z0_Description;
				textBox.CaptionResourceString = Res.GetData("TextBox", "Description");  //Note: caption should be same as field name

				findBox.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				findBox.Location = new Point(200, 80);
				findBox.BindTo = DummyBusinessObject.Schema.Z0_Guid;
				findBox.BindToList = "Collection";
				findBox.CaptionResourceString = Res.GetData("FindBox", "GUI Guid Caption");

				testForm.Size = new Size(300, 200);
				testForm.CaptionRenderingEnabled = true;
				testForm.Controls.Add(editControl);
				testForm.Controls.Add(textBox);
				testForm.Controls.Add(findBox);

				mockData.Put("DummyBizo|Z0_VarCharMax", new ResourceStringData("", "Data Caption"));
				mockData.Put("DummyBizo|Z0_Guid", new ResourceStringData("", "Data Guid Caption"));

				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.Description);
				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.HumanReadableName);

				AssertEquals("Data Guid Caption", dummy.Z0_GuidInfo.Description);
				AssertEquals("Z0_Guid HasHumanReadableName", true, dummy.Z0_GuidInfo.HasHumanReadableName);
				AssertEquals("Data Guid Caption", dummy.Z0_GuidInfo.HumanReadableName);

				AssertEquals("Description", dummy.Z0_DescriptionInfo.HumanReadableName);

				testForm.Show();
				Application.DoEvents();

				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.Description);
				AssertEquals("GUI Caption", dummy.Z0_VarCharMaxInfo.HumanReadableName);

				AssertEquals("Data Guid Caption", dummy.Z0_GuidInfo.Description);
				AssertEquals("Z0_Guid HasHumanReadableName", true, dummy.Z0_GuidInfo.HasHumanReadableName);
				AssertEquals("GUI Guid Caption", dummy.Z0_GuidInfo.HumanReadableName);

				AssertEquals("Description", dummy.Z0_DescriptionInfo.HumanReadableName);
			}
		}

		public void TestCopyCaptionToPropertyHumanReadableName_ControlVisibility()
		{
			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			using (var mockData = Res.UseMockData())
			using (var testForm = new ZForm(dummy))
			using (var editControl = new ZTextBox())
			using (var textBox = new ZTextBox())
			using (var findBox = new ZGuidFindBox())
			{
				editControl.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				editControl.Location = new Point(200, 10);
				editControl.BindTo = DummyBusinessObject.Schema.Z0_VarCharMax;
				editControl.CaptionResourceString = Res.GetData("EditControl", "GUI Caption");

				textBox.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				textBox.Location = new Point(200, 40);
				textBox.BindTo = DummyBusinessObject.Schema.Z0_Description;
				textBox.CaptionResourceString = Res.GetData("TextBox", "Description");  //Note: caption should be same as field name

				findBox.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				findBox.Location = new Point(200, 80);
				findBox.BindTo = DummyBusinessObject.Schema.Z0_Guid;
				findBox.BindToList = "Collection";
				findBox.CaptionResourceString = Res.GetData("FindBox", "GUI Guid Caption");

				testForm.Size = new Size(300, 200);
				testForm.CaptionRenderingEnabled = true;
				testForm.Controls.Add(editControl);
				testForm.Controls.Add(textBox);
				testForm.Controls.Add(findBox);
				textBox.Visible = false;
				editControl.Visible = false;
				findBox.Visible = false;

				mockData.Put("DummyBizo|Z0_VarCharMax", new ResourceStringData("", "Data Caption"));
				mockData.Put("DummyBizo|Z0_Guid", new ResourceStringData("", "Data Guid Caption"));

				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.Description);
				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.HumanReadableName);

				AssertEquals("Data Guid Caption", dummy.Z0_GuidInfo.Description);
				AssertEquals("Z0_Guid HasHumanReadableName", true, dummy.Z0_GuidInfo.HasHumanReadableName);
				AssertEquals("Data Guid Caption", dummy.Z0_GuidInfo.HumanReadableName);

				AssertEquals("Description", dummy.Z0_DescriptionInfo.HumanReadableName);

				testForm.Show();
				Application.DoEvents();

				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.Description);
				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.HumanReadableName);

				AssertEquals("Data Guid Caption", dummy.Z0_GuidInfo.Description);
				AssertEquals("Z0_Guid HasHumanReadableName", true, dummy.Z0_GuidInfo.HasHumanReadableName);
				AssertEquals("Data Guid Caption", dummy.Z0_GuidInfo.HumanReadableName);

				AssertEquals("Description", dummy.Z0_DescriptionInfo.HumanReadableName);
			}
		}

		public void TestCopyCaptionToPropertyHumanReadableNameWithCollection()
		{
			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			using (var mockData = Res.UseMockData())
			using (var testForm = new ZForm(dummy))
			using (var editControl = new ZTextBox())
			{
				editControl.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				editControl.Location = new Point(200, 10);
				editControl.BindTo = "Collection." + DummyChildBusinessObject.Schema.Z0_VarCharMax;
				editControl.CaptionResourceString = Res.GetData("GUI Caption", "EditControl");

				testForm.Size = new Size(300, 200);
				testForm.CaptionRenderingEnabled = true;
				testForm.Controls.Add(editControl);

				mockData.Put("DummyBizo|Z0_VarCharMax", new ResourceStringData("", "Data Caption"));

				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.Description);
				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.HumanReadableName);

				editControl.GetExtension<ILabelCaptionRenderer>().Caption = "GUI Caption";

				testForm.Show();
				Application.DoEvents();

				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.Description);
				AssertEquals("Data Caption", dummy.Z0_VarCharMaxInfo.HumanReadableName);

				var child = dummy.Collection.AddNew();
				var otherChild = dummy.Collection.AddNew();

				AssertEquals("Data Caption", child.Z0_VarCharMaxInfo.Description);
				AssertEquals("Data Caption", child.Z0_VarCharMaxInfo.HumanReadableName);
				AssertEquals("Data Caption", otherChild.Z0_VarCharMaxInfo.Description);
				AssertEquals("Data Caption", otherChild.Z0_VarCharMaxInfo.HumanReadableName);

				Application.DoEvents();

				AssertEquals("Data Caption", child.Z0_VarCharMaxInfo.Description);
				AssertEquals("GUI Caption", child.Z0_VarCharMaxInfo.HumanReadableName);
				AssertEquals("Data Caption", otherChild.Z0_VarCharMaxInfo.Description);
				AssertEquals("Data Caption", otherChild.Z0_VarCharMaxInfo.HumanReadableName);

				dummy.Collection.Remove(child); // ListManager.Current will be changed to otherChild
				Application.DoEvents();

				AssertEquals("Data Caption", child.Z0_VarCharMaxInfo.Description);
				AssertEquals("GUI Caption", child.Z0_VarCharMaxInfo.HumanReadableName);
				AssertEquals("Data Caption", otherChild.Z0_VarCharMaxInfo.Description);
				AssertEquals("GUI Caption", otherChild.Z0_VarCharMaxInfo.HumanReadableName);
			}
		}

		public void TestCopyCaptionToPropertyHumanReadableNameValidateOnceOnlyForWrappedInfo()
		{
			var factory = new BusinessObjectFactory();
			var mock = new Mock<DummyDependantBusinessObject>(factory, new RowFactory(factory).New(DummyDependantBusinessObject.Schema.TableName)) { CallBase = true };
			var dummy = mock.Object;
			var validationMock = new Mock<DummyDependentBizoValidation>(dummy);
			mock.Protected().Setup<DummyDependentBizoValidation>("GetNewValidation").Returns(validationMock.Object);
			validationMock.Protected().Setup("CheckZD1_Number").Callback(() => dummy.ZD1_NumberInfo.AddMessageError("Some error"));
			dummy.ZD1_NumberInfo.AddMessageError("Some error");
			using (var mockData = Res.UseMockData())
			using (var testForm = new ZForm(dummy))
			using (var textBox = new ZTextBox())
			{
				testForm.CaptionResourceString = Res.GetData("TestForm", "Test Form");

				textBox.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				textBox.Location = new Point(200, 10);
				textBox.BindTo = DummyDependantBusinessObject.Schema.ZD1_WrappedNumberProperty;
				textBox.CaptionResourceString = Res.GetData("TextBox", "GUI Caption");

				testForm.Size = new Size(300, 200);
				testForm.CaptionRenderingEnabled = true;
				testForm.Controls.Add(textBox);

				testForm.Show();
				Application.DoEvents();

				AssertEquals("GUI Caption", dummy.ZD1_WrappedNumberPropertyInfo.HumanReadableName);
			}
			validationMock.Protected().Verify("CheckZD1_Number", Times.Once());
			if (CargoWise.Common.ErrorReporter.LastKeyReported == "Validation:ZD1_Number")
			{
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		#endregion

		#region Test Classes

		class TestUserControl : ZUserControl, IVariableLengthCaptionRenderer
		{
			public string[] Captions { get; set; }

			bool IVariableLengthCaptionRenderer.IsCaptionOverridden
			{
				get { return false; }
				set { throw new NotImplementedException(); }
			}
		}

		class TestLabelCaptionRenderer : ZLabelCaptionRenderer
		{
			public TestLabelCaptionRenderer(Control control) : base(control)
			{
			}

			public LabelCaptionMeasurement LastCaptionMeasurement { get; set; }

			protected override LabelCaptionMeasurement MeasureCaptionCore()
			{
				LastCaptionMeasurement = base.MeasureCaptionCore();
				return LastCaptionMeasurement;
			}

			internal override ZLabelCaptionCache GetLabelCaptionCache()
			{
				return new TestLabelCaptionCache();
			}
		}

		class TestLabelCaptionCache : ZLabelCaptionCache
		{
			public override string[] GetCaptions(Control control, bool invalidateCache)
			{
				var caption = control.DataBindings.Count == 0 ? control.Text : ("Binding.PropertyName=" + control.DataBindings[0].PropertyName);
				return new ResourceStringData(string.Empty, caption, caption, caption, string.Empty).GetCaptions();
			}
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (textBox != null)
			{
				textBox.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		void FireApplicationIdle()
		{
			var form = new Form();

			var timer = new Timer();
			timer.Tick += delegate
			{ form.Dispose(); };
			timer.Interval = 200;
			timer.Start();

			form.ShowDialog();
			timer.Dispose();
		}

		static void ForcePaintEvenWhenComputerLocked(Control control)
		{
#if !WINZOR
			control.DrawToBitmap(new Bitmap(control.Width, control.Height), control.Bounds);
#endif
		}

		TestLabelCaptionRenderer Renderer
		{
			get { return renderer ?? (renderer = new TestLabelCaptionRenderer(TextBox)); }
		}
		TestLabelCaptionRenderer renderer;

		ZTextBox TextBox
		{
			get { return textBox ?? (textBox = new ZTextBox.Bare()); }
		}
		ZTextBox textBox;

		TestUserControl UserControl
		{
			get { return userControl ?? (userControl = new TestUserControl()); }
		}
		TestUserControl userControl;

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm();
					form.CaptionRenderingEnabled = true;
				}
				return form;
			}
		}
		ZForm form;

		#endregion
	}
}
