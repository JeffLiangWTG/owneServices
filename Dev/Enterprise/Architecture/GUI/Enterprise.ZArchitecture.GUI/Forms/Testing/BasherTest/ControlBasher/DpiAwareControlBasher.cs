#if DEBUG

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	internal class DpiAwareControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			AssertControlIncludesSerializer(control, notifications);

			var container = control as ContainerControl;
			if (container != null)
			{
				AssertContainerHasStandardAutoScaling(container, notifications);
			}
		}

		void AssertControlIncludesSerializer(Control control, INotifications notifications)
		{
			var hasSerializer =
				control.GetType().GetCustomAttributes(typeof(DesignerSerializerAttribute)).Any(attribute =>
				{
					var serializerType = Type.GetType(((DesignerSerializerAttribute)attribute).SerializerTypeName);
					return serializerType == typeof(ControlDpiScalingCodeDomSerializer) || serializerType.IsSubclassOf(typeof(ControlDpiScalingCodeDomSerializer));
				});

			if (!hasSerializer)
			{
				notifications.AddError(String.Format(CultureInfo.InvariantCulture, "Control {1}({0}) included in {3}({2}) doesn't have a ControlDpiScalingCodeDomSerializer associated. {4}",
					control.GetType().FullName, control.Name, control.Parent != null ? control.Parent.GetType().FullName : "", control.Parent.Name, bsTestFailures));
			}
		}

		void AssertContainerHasStandardAutoScaling(ContainerControl container, INotifications notifications)
		{
			if ((container.AutoScaleMode != ControlDpiScalingHelper.DpiScaleMode))
			{
				notifications.AddError(String.Format(CultureInfo.InvariantCulture, "Container {1}({0}) doesn't have the standard AutoScaleMode set. {2}", container.GetType().FullName, container.Name, bsTestFailures));
			}

			if ((container.AutoScaleDimensions != ControlDpiScalingHelper.DpiScaleDimensions))
			{
				notifications.AddError(String.Format(CultureInfo.InvariantCulture, "Container {1}({0}) doesn't have the standard AutoScaleDimensions set. {2}", container.GetType().FullName, container.Name, bsTestFailures));
			}
		}

		public static bool NeedsDpiAwarenessValidation(Control control)
		{
			return TypeDescriptor.GetAttributes(control)[typeof(SuppressDpiAwareBasherAttribute)] == null && // This attribute can be applied to controls to exclude them from the basher
				!(control is CargoWise.BrandManager.CargoWiseProgressBar); // This is a tricky control, since it cannot reference the attribute
		}

		const string bsTestFailures = " If you see a whole bunch of crappy test failures for one control, all moaning about *scaling* properties not being set/associated, check that your Container control is CargoWise.Windows.UI.KSplitContainer and not the standard Winforms version. That is a likely reason for the member lacking these essential properties (although your form proably works perfectly well alreaady, eh?)";
	}
}

#endif // DEBUG
