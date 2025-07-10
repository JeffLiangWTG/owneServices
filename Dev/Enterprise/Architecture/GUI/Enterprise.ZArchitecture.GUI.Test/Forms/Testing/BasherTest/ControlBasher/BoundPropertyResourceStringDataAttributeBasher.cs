using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class BoundPropertyResourceStringDataAttributeBasher : IControlBasher
	{
		void IControlBasher.Bash(Control control, INotifications notifications)
		{
			var boundDataControl = control as IDataBoundControl;
			var boundObject = KBindingSource.GetBindingSource(control) as KBindingSource;
			var suppressed = TypeDescriptor.GetAttributes(control)[typeof(SuppressControlRequiresTextBasherAttribute)] != null;
			if (boundDataControl is null
				|| CanIgnoreControlType(control)
				|| string.IsNullOrEmpty(boundDataControl.DataMember)
				|| boundObject?.DataSource is null
				|| boundObject.Current is null
				|| suppressed)
			{
				return;
			}

			var currentBoundItem = boundObject.Current;
			var currentItem = currentBoundItem is IList || currentBoundItem is IZType _ ? boundDataControl.DataSource : currentBoundItem;
			var (boundProperty, boundPropertyWithFullPath) = control is IResourceStringBindingMember resourceStringBindingMember
				? (resourceStringBindingMember.ResourceStringBindingMember, boundDataControl.DataMember + "." + resourceStringBindingMember.ResourceStringBindingMember)
				: (boundDataControl.DataMember, boundDataControl.DataMember);

			var propertyInfo = GetPropertyInfo(currentItem, boundProperty) ?? GetPropertyInfo(currentItem, boundPropertyWithFullPath);
			if (propertyInfo is null)
			{
				notifications.AddError($"Property Info not found for Data Member: {boundProperty}  bound to a Control: {control.Name}. This is an issue with the test itself, nothing to be done here.");
				return;
			}

			var resourceStringDataAttributes = propertyInfo.GetCustomAttributes<ResourceStringDataAttribute>(inherit: true).ToArray();
			if (resourceStringDataAttributes.Length == 0)
			{
				var controlCaptionKey = control is IResCaptionedControl resCaptionedControl ? resCaptionedControl.CaptionResourceString?.Key : "None";
				notifications.AddError($"Property {boundProperty} bound to a Control: {control.Name} is not decorated with a ResourceStringDataAttribute. This may result in Caption inconsistencies if used on different forms. " +
										$"It is recommended to have at least one ResourceStringDataAttribute for every bound member. The control was found with a resource string key {controlCaptionKey}");
			}
		}

		PropertyInfo GetPropertyInfo(object currentItem, string dataMember)
		{
			var componentType = DataMemberTypeProvider.GetFinalComponentTypeFromDataMember(currentItem.GetType(), dataMember);
			if (componentType is null)
			{
				return null;
			}
			var propertyName = dataMember.Split(new[] { '.', '+' }).Last();
			return componentType
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(p => p.Name == propertyName);
		}

		bool CanIgnoreControlType(Control control)
		{
			return control is ZLabel
					|| control is ZGrid
					|| control is ZTabControl
					|| control is ProgressBar
					|| control is PictureBox
					|| control is Button
					|| control.GetType().Name == "DynamicLayoutPanel";
		}
	}
}
