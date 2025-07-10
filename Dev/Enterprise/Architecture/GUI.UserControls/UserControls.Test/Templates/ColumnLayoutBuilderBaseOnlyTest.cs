using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ColumnLayoutBuilderTestClass))]
	sealed class ColumnLayoutBuilderBaseOnlyTest : ColumnLayoutBuilderAbstractTest<ColumnLayoutBuilderTestClass, DummyBusinessObject, BagForTest>
	{
		public void TestAddColumn()
		{
			ColumnLayoutBuilderForTesting.AddControlBag(BagForTest.Instance);
			CombineAssertions(() =>
			{
				AssertExceptionThrown<InvalidOperationException>("column needed", "You need to add a column before adding controls to it.", () => ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control1, ControlWidthClass.Auto));
				AssertNoExceptionThrown("column added", () =>
				{
					ColumnLayoutBuilderForTesting.AddColumn();
					ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control1, ControlWidthClass.Auto);
				});
			});
		}

		public void TestAdd()
		{
			ColumnLayoutBuilderForTesting.AddControlBag(BagForTest.Instance);
			ColumnLayoutBuilderForTesting.AddColumn();
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control1, ControlWidthClass.Auto);
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control2, ControlWidthClass.Auto);
			var layout = ColumnLayoutBuilderForTesting.Build();
			AssertEquals(true, layout.ShouldInclude(BagForTest.Instance.Control1));
		}

		public void TestAlignToControlVerticalAlignment()
		{
			ColumnLayoutBuilderForTesting.AddControlBag(BagForTest.Instance);
			ColumnLayoutBuilderForTesting.AddColumn();
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.ComplexControl3, ControlWidthClass.Auto);
			ColumnLayoutBuilderForTesting.AddColumn();
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control2, ControlWidthClass.Auto, BagForTest.Instance.ComplexControl3, System.Windows.Forms.VisualStyles.VerticalAlignment.Bottom);

			var layout = ColumnLayoutBuilderForTesting.Build();
			Assert(layout.ShouldInclude(BagForTest.Instance.Control2));
			var columns = layout.Columns;
			AssertEquals("Columns", 2, columns.Count);
			var column1 = columns[0];
			AssertEquals("Rows in 1st column", 1, column1.Rows.Count);
			var column1Row1 = column1.Rows[0];
			AssertNull("ComplexControl3 aligned to control", column1Row1.AlignToControl);
			AssertEquals("ComplexControl3 vertical alignment default", System.Windows.Forms.VisualStyles.VerticalAlignment.Top, column1Row1.VerticalAlignment);
			var column2 = columns[1];
			AssertEquals("Rows in 2nd column", 1, column2.Rows.Count);
			var column2Row1 = column2.Rows[0];
			AssertEquals("Control2 aligned to control", BagForTest.Instance.ComplexControl3, column2Row1.AlignToControl);
			AssertEquals("Control2 vertical alignment", System.Windows.Forms.VisualStyles.VerticalAlignment.Bottom, column2Row1.VerticalAlignment);
		}

		public void TestSetVisibility()
		{
			ColumnLayoutBuilderForTesting.AddControlBag(BagForTest.Instance);
			ColumnLayoutBuilderForTesting.AddColumn();
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control1, ControlWidthClass.Auto);
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control2, ControlWidthClass.Auto);

			var bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			ColumnLayoutBuilderForTesting.SetVisibility(BagForTest.Instance.Control1, d => true, d => null);
			ColumnLayoutBuilderForTesting.SetVisibility(BagForTest.Instance.Control2, d => false, d => null);

			var layout = ColumnLayoutBuilderForTesting.Build();
			CombineAssertions(() =>
			{
				AssertEquals("SetVisibility for control1", true, layout.IsVisible(BagForTest.Instance.Control1, bo));
				AssertEquals("SetVisibility for control2", false, layout.IsVisible(BagForTest.Instance.Control2, bo));
			});
		}

		public void TestSetCaption()
		{
			ColumnLayoutBuilderForTesting.AddControlBag(BagForTest.Instance);
			ColumnLayoutBuilderForTesting.AddColumn();
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control1, ControlWidthClass.Auto);
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control2, ControlWidthClass.Auto);

			var bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			ColumnLayoutBuilderForTesting.SetCaption(BagForTest.Instance.Control1, d => Res.GetData("BF332FD0-8E81-46F3-9D77-B008839B2898", "control1"), d => null);

			var layout = ColumnLayoutBuilderForTesting.Build();
			CombineAssertions(() =>
			{
				ResourceStringData resourceStringData;
				AssertEquals("SetCaption for control1", true, layout.TryGetCaption(BagForTest.Instance.Control1, bo, out resourceStringData));
				AssertEquals("caption should be control1", "control1", resourceStringData.Caption);
				AssertEquals("SetCaption not set for control2", false, layout.TryGetCaption(BagForTest.Instance.Control2, bo, out resourceStringData));
			});
		}

		public void TestSetCaptions()
		{
			ColumnLayoutBuilderForTesting.AddControlBag(BagForTest.Instance);
			ColumnLayoutBuilderForTesting.AddColumn();
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control1, ControlWidthClass.Auto);
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control2, ControlWidthClass.Auto);
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.ComplexControl3, ControlWidthClass.Auto);

			var bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			ColumnLayoutBuilderForTesting.SetCaptions(BagForTest.Instance.Control1, d => new Dictionary<string, ResourceStringData>() { { nameof(BagForTest.Instance.Control1), Res.GetData("ABC", "control1") } }, d => null);
			ColumnLayoutBuilderForTesting.SetCaptions(BagForTest.Instance.ComplexControl3, d => new Dictionary<string, ResourceStringData>()
			{
				{ nameof(BagForTest.Instance.ComplexControl3), Res.GetData("ABC2", "control3", "longer control3") },
				{ nameof(ComplexControl3.Child2), Res.GetData("ABC3", "control3 child 2") }
			}, d => null);

			var layout = ColumnLayoutBuilderForTesting.Build();
			CombineAssertions("Control1 and Control2", () =>
			{
				AssertEquals("SetCaptions for Control1", true, layout.TryGetCaptionData(BagForTest.Instance.Control1, bo, out var captionsData));
				AssertEquals("data.Length", 1, captionsData.Count);
				var captionData = captionsData.Single();
				AssertEquals("controlName", "Control1", captionData.Key);
				AssertEquals("captionData.captionData.Caption", "control1", captionData.Value.Caption);

				AssertEquals("SetCaptions not set for Control2", false, layout.TryGetCaptionData(BagForTest.Instance.Control2, bo, out captionsData));
			});
			CombineAssertions("ComplexControl3", () =>
			{
				AssertEquals("SetCaptions for ComplexControl3", true, layout.TryGetCaptionData(BagForTest.Instance.ComplexControl3, bo, out var captionsData));
				AssertEquals("data.Length", 2, captionsData.Count);
				var captionData1 = captionsData["ComplexControl3"];
				AssertEquals("captionData.captionData.Caption", "control3", captionData1.Caption);
				AssertEquals("captionData.captionData.FullDescription", "longer control3", captionData1.FullDescription);
				var captionData2 = captionsData["Child2"];
				AssertEquals("captionData2.captionData.Caption", "control3 child 2", captionData2.Caption);
			});
		}

		public void TestNarrowColumnForMediumControls_Build()
		{
			var builder = new LayoutBuilderForNarrowColumnForMediumControlsTest();
			var commonBag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(commonBag.Separator, ControlWidthClass.Medium);
			var layout = builder.Build();
			var row = layout.Columns.First().Rows.First();
			CombineAssertions(() =>
			{
				AssertEquals("widthClass is ControlWidthClass.Medium", 4, row.Parts.Count);
				AssertNotNull("MediumColumnWidth applied", row.Parts.FirstOrDefault(x => x is PanelLayoutRuler ruler && ruler.Position == 236));
			});
		}

		public void TestAddControlBehaviour()
		{
			ColumnLayoutBuilderForTesting.AddControlBag(BagForTest.Instance);
			ColumnLayoutBuilderForTesting.AddColumn();
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control1, ControlWidthClass.Auto);
			var bo = Factory.NewWithValidTestData<DummyBusinessObject>();

			ColumnLayoutBuilderForTesting.AddControlBehaviour(BagForTest.Instance.Control1, new ControlBehaviourForTest(), d => null);
			ColumnLayoutBuilderForTesting.AddControlBehaviour(BagForTest.Instance.Control1, new ReadOnlyControlBehaviourForTest(), d => null);

			var layout = ColumnLayoutBuilderForTesting.Build();
			AssertEquals(true, layout.ShouldInclude(BagForTest.Instance.Control1));

			var controlStates = layout.GetControlBehaviours(BagForTest.Instance.Control1);
			AssertEquals("Control States Count", 2, controlStates.Count);
		}

		public void TestAddControlBehaviour_WithBehaviourChangeAction()
		{
			ColumnLayoutBuilderForTesting.AddControlBag(BagForTest.Instance);
			ColumnLayoutBuilderForTesting.AddColumn();
			ColumnLayoutBuilderForTesting.Add(BagForTest.Instance.Control1, ControlWidthClass.Auto);

			ColumnLayoutBuilderForTesting.AddControlBehaviour<Control>(BagForTest.Instance.Control1, (c, b) => c.Enabled = false, d => null);
			ColumnLayoutBuilderForTesting.AddControlBehaviour<Control>(BagForTest.Instance.Control1, "MarkControlEnabledBehaviour", (c, b) => c.Enabled = false, d => null);

			var layout = ColumnLayoutBuilderForTesting.Build();
			AssertEquals("Has InternalCustomizableControlBehaviour", true, layout.HasBehaviourByBehaviourType(BagForTest.Instance.Control1, typeof(InternalCustomizableControlBehaviour<,>)));
			AssertEquals("Has MarkControlEnabledBehaviour", true, layout.HasBehaviourByBehaviourName(BagForTest.Instance.Control1, "MarkControlEnabledBehaviour"));
		}

		protected override ColumnLayoutBuilderTestClass GetColumnLayoutBuilderForTesting() => new ColumnLayoutBuilderTestClass();
	}

	sealed class ColumnLayoutBuilderTestClass : ColumnLayoutBuilder<DummyBusinessObject, BagForTest>
	{
		public ColumnLayoutBuilderTestClass()
		{
			CaptionWidthForTesting = base.CaptionWidth;
		}

		public override BagForTest CommonBag => BagForTest.Instance;

		public ColumnLayoutBuilderCaptionWidthSize CaptionWidthForTesting;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => CaptionWidthForTesting;
	}

	sealed class LayoutUserControlForTest : ZUserControl
	{
		public LayoutUserControlForTest()
		{
			InitializeComponent();
		}

		void InitializeComponent()
		{
			this.Separator = new SeparatorUserControl();
			// 
			// Separator
			// 
			this.Separator.AllowDrop = true;
			this.Separator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Separator.Name = "Separator";
			this.Separator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.Separator.TabIndex = 0;
			// 
			// LayoutUserControlForTest
			// 
			this.Controls.Add(this.Separator);
			this.Name = "LayoutUserControlForTest";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			this.Separator.ResumeLayout(true);
			this.Separator.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		SeparatorUserControl Separator;
	}

	sealed class LayoutBuilderForNarrowColumnForMediumControlsTest : ColumnLayoutBuilder<BusinessObject, ControlBagForNarrowColumnForMediumControlsTest>
	{
		public override ControlBagForNarrowColumnForMediumControlsTest CommonBag { get; } = ControlBagForNarrowColumnForMediumControlsTest.Instance;

		public override bool NarrowColumnForMediumControls => true;
	}

	sealed class ControlBagForNarrowColumnForMediumControlsTest : ControlBag
	{
		public static ControlBagForNarrowColumnForMediumControlsTest Instance => instance ?? (instance = new ControlBagForNarrowColumnForMediumControlsTest());

		[ThreadStatic]
		static ControlBagForNarrowColumnForMediumControlsTest instance;

		protected override Control CreateTemplate() => new LayoutUserControlForTest();

		public ControlBagForNarrowColumnForMediumControlsTest()
		{
			Separator = RegisterControl(nameof(Separator));
		}

		public ControlReference Separator { get; }
	}

	sealed class ControlBehaviourForTest : ControlBehaviour
	{
		public override void UpdateBehaviour(Control control, object dataItem)
		{
			throw new NotImplementedException();
		}
	}

	sealed class ReadOnlyControlBehaviourForTest : ControlBehaviour
	{
		public override void UpdateBehaviour(Control control, object dataItem)
		{
			throw new NotImplementedException();
		}
	}
}
