using System;
using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class LabelCaptionRenderProviderTest : TestCase
	{
		public void TestExtenderProviderImpl()
		{
			AssertEquals("Implements IExtenderProvider", true, LabelCaptionRenderProvider is IExtenderProvider);
			AssertEquals("Doesn't extend controls that don't have ILabelCaptionRenderers", false, ((IExtenderProvider)LabelCaptionRenderProvider).CanExtend(Control));
			AssertEquals("Extends controls with ILabelCaptionRenderer", true, ((IExtenderProvider)LabelCaptionRenderProvider).CanExtend(ControlWithCaptionRenderer));
			AssertEquals("Doesn't extend other types", false, ((IExtenderProvider)LabelCaptionRenderProvider).CanExtend(new object()));

			ProvidePropertyAttribute[] attrs = (ProvidePropertyAttribute[])typeof(LabelCaptionRenderProvider).GetCustomAttributes(typeof(ProvidePropertyAttribute), false);
			AssertEquals("Provides 3 properties to controls", 3, attrs.Length);

			Array.Sort(attrs, delegate(ProvidePropertyAttribute lhs, ProvidePropertyAttribute rhs)
			{ return lhs.PropertyName.CompareTo(rhs.PropertyName); });
			AssertEquals("PropertyName", "LabelCaptionAlignment", attrs[0].PropertyName);
			AssertEquals("ReceiverType", typeof(Control).AssemblyQualifiedName, attrs[0].ReceiverTypeName);
			AssertEquals("PropertyName", "LabelCaptionVisible", attrs[1].PropertyName);
			AssertEquals("ReceiverType", typeof(Control).AssemblyQualifiedName, attrs[1].ReceiverTypeName);
			AssertEquals("PropertyName", "LabelTop", attrs[2].PropertyName);
			AssertEquals("ReceiverType", typeof(Control).AssemblyQualifiedName, attrs[2].ReceiverTypeName);
		}

		public void TestGetSetLabelCaptionAlignment()
		{
			LabelCaptionRenderProvider.SetLabelCaptionAlignment(ControlWithCaptionRenderer, LabelCaptionAlignment.Top);
			AssertEquals(LabelCaptionAlignment.Top, LabelCaptionRenderProvider.GetLabelCaptionAlignment(ControlWithCaptionRenderer));
			AssertEquals(LabelCaptionAlignment.Top, LabelCaptionRenderer.Alignment);

			LabelCaptionRenderProvider.SetLabelCaptionAlignment(ControlWithCaptionRenderer, LabelCaptionAlignment.Left);
			AssertEquals(LabelCaptionAlignment.Left, LabelCaptionRenderProvider.GetLabelCaptionAlignment(ControlWithCaptionRenderer));
			AssertEquals(LabelCaptionAlignment.Left, LabelCaptionRenderer.Alignment);
		}

		public void TestShouldSerializeLabelCaptionAlignment()
		{
			AssertEquals("Should not serialize the default value", false, LabelCaptionRenderProvider.ShouldSerializeLabelCaptionAlignment(ControlWithCaptionRenderer));
			LabelCaptionRenderProvider.SetLabelCaptionAlignment(ControlWithCaptionRenderer, LabelCaptionAlignment.Top);
			AssertEquals("Should serialize a non-default value", true, LabelCaptionRenderProvider.ShouldSerializeLabelCaptionAlignment(ControlWithCaptionRenderer));
			AssertEquals(
				"DefaultValueAttribute should not be applied otherwise ShouldSerializeXXX methods will not be effective",
				0, typeof(LabelCaptionRenderProvider).GetMethod("GetLabelCaptionAlignment").GetCustomAttributes(typeof(DefaultValueAttribute), true).Length);
		}

		public void TestGetSetLabelCaptionVisible()
		{
			LabelCaptionRenderProvider.SetLabelCaptionVisible(ControlWithCaptionRenderer, false);
			AssertEquals(false, LabelCaptionRenderProvider.GetLabelCaptionVisible(ControlWithCaptionRenderer));
			AssertEquals(false, LabelCaptionRenderer.Visible);

			LabelCaptionRenderProvider.SetLabelCaptionVisible(ControlWithCaptionRenderer, true);
			AssertEquals(true, LabelCaptionRenderProvider.GetLabelCaptionVisible(ControlWithCaptionRenderer));
			AssertEquals(true, LabelCaptionRenderer.Visible);
		}

		public void TestShouldSerializeLabelCaptionVisible()
		{
			AssertEquals("Should not serialize the default value", false, LabelCaptionRenderProvider.ShouldSerializeLabelCaptionVisible(ControlWithCaptionRenderer));
			LabelCaptionRenderProvider.SetLabelCaptionVisible(ControlWithCaptionRenderer, false);
			AssertEquals("Should serialize a non-default value", true, LabelCaptionRenderProvider.ShouldSerializeLabelCaptionVisible(ControlWithCaptionRenderer));
			AssertEquals(
				"DefaultValueAttribute should not be applied otherwise ShouldSerializeXXX methods will not be effective",
				0, typeof(LabelCaptionRenderProvider).GetMethod("GetLabelCaptionVisible").GetCustomAttributes(typeof(DefaultValueAttribute), true).Length);
		}

		#region Implementation

		TextBox Control
		{
			get { return control ?? (control = new TextBox()); }
		}
		TextBox control;

		GenericExtendedControl ControlWithCaptionRenderer
		{
			get
			{
				if (controlWithCaptionRenderer == null)
				{
					controlWithCaptionRenderer = new GenericExtendedControl();
					controlWithCaptionRenderer.Extensions = new ControlExtensionCollection(controlWithCaptionRenderer);
					controlWithCaptionRenderer.Extensions.Add(LabelCaptionRenderer);
				}
				return controlWithCaptionRenderer;
			}
		}
		GenericExtendedControl controlWithCaptionRenderer;

		LabelCaptionRenderer LabelCaptionRenderer
		{
			get { return labelCaptionRenderer ?? (labelCaptionRenderer = new LabelCaptionRenderer()); }
		}
		LabelCaptionRenderer labelCaptionRenderer;

		TestLabelCaptionRenderProvider LabelCaptionRenderProvider
		{
			get { return labelCaptionRenderProvider ?? (labelCaptionRenderProvider = new TestLabelCaptionRenderProvider()); }
		}
		TestLabelCaptionRenderProvider labelCaptionRenderProvider;

		protected override void TearDown()
		{
			base.TearDown();
			if (labelCaptionRenderer != null)
			{
				labelCaptionRenderer.Dispose();
			}
			if (controlWithCaptionRenderer != null)
			{
				controlWithCaptionRenderer.Dispose();
			}
		}

		#endregion

		#region Test Classes

		class TestLabelCaptionRenderProvider : LabelCaptionRenderProvider
		{
			public new bool ShouldSerializeLabelCaptionVisible(Control control)
			{
				return base.ShouldSerializeLabelCaptionVisible(control);
			}

			public new bool ShouldSerializeLabelTop(Control control)
			{
				return base.ShouldSerializeLabelTop(control);
			}

			public new bool ShouldSerializeLabelCaptionAlignment(Control control)
			{
				return base.ShouldSerializeLabelCaptionAlignment(control);
			}
		}

		#endregion
	}
}
