using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZControlTypeDescriptorTest : TestCase
	{
		public void TestProxiedPropertiesFromDotNetTypeDescriptor()
		{
			DesignModeFinder.SetIsDesigningForTest(true);
			try
			{
				var customTypeDescriptor = GetNewControlTypeDescriptor(typeof(TestControl));
				var dotNetTypeDescriptionProvider = TypeDescriptor.GetProvider(typeof(Component));
				var dotNetTypeDescriptor = dotNetTypeDescriptionProvider.GetTypeDescriptor(typeof(TestControl));

				AssertEquals(dotNetTypeDescriptor.GetConverter(), customTypeDescriptor.GetConverter());
				AssertEquals(dotNetTypeDescriptor.GetEvents(null), customTypeDescriptor.GetEvents(null));
				AssertEquals(dotNetTypeDescriptor.GetEvents(), customTypeDescriptor.GetEvents());
				AssertEquals(dotNetTypeDescriptor.GetAttributes(), customTypeDescriptor.GetAttributes());
				AssertEquals(
					dotNetTypeDescriptor.GetEditor(typeof(System.Drawing.Design.UITypeEditor)),
					customTypeDescriptor.GetEditor(typeof(System.Drawing.Design.UITypeEditor)));
				AssertEquals(dotNetTypeDescriptor.GetDefaultProperty()?.Name, customTypeDescriptor.GetDefaultProperty()?.Name);
				AssertContainsExactElementsInAnyOrder(dotNetTypeDescriptor.GetProperties(), customTypeDescriptor.GetProperties());
				AssertEquals(dotNetTypeDescriptor.GetDefaultEvent(), customTypeDescriptor.GetDefaultEvent());
			}
			finally
			{
				DesignModeFinder.SetIsDesigningForTest(false);
			}
		}

		public void TestGetClassName()
		{
			AssertEquals(typeof(TestControl).FullName, GetNewControlTypeDescriptor(typeof(TestControl)).GetClassName());
		}

		public void TestReturnsProxyEventDescriptorCollection()
		{
			var descriptor = GetNewControlTypeDescriptor(typeof(TestControl));

			DesignModeFinder.SetIsDesigningForTest(true);
			try
			{
				AssertEquals(typeof(EventDescriptorCollection), descriptor.GetEvents().GetType());
			}
			finally
			{
				DesignModeFinder.SetIsDesigningForTest(false);
			}
			AssertEquals(typeof(ControlEventDescriptorCollection), descriptor.GetEvents().GetType());
		}

		static ZControlTypeDescriptor GetNewControlTypeDescriptor(Type controlType)
		{
			return new ZControlTypeDescriptor(controlType);
		}

		[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
		class TestControl : Control
		{ }
	}
}
