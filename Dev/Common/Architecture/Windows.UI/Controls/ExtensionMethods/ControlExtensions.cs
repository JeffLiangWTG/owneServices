using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI.Controls;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	public interface IDisposeStackProvider
	{
		StackTrace DisposeStack { get; }

		string DisposeControlPath { get; }

		bool TrackDisposedAccess { get; set; }
	}

	public static class ControlExtensionsForIDisposeStackProvider
	{
		public static string BuildDisposeInformation<T>(this T control, string additionalInfo = "") where T : Control, IDisposeStackProvider
		{
			var controlPath = ControlDescription.GetControlPath(control);
			var messageBuilder = new ZStringBuilder();
			messageBuilder.AppendFormat((NoResString)"Control [name:'{0}' type:'{1}'] is already disposed.", control.Name, control.GetType().FullName).AppendLine();
			messageBuilder.AppendLine((NoResString)"Wish you can find out why this control has been disposed with the help of the below dispose information.");
			messageBuilder.AppendLine((NoResString)"If you want to see the details of Dispose Information below, please open TrackDisposedAccess in this control.");
			messageBuilder.AppendLine((NoResString)"------------Dispose Information Begin------------");
			messageBuilder.AppendFormat((NoResString)"Control Path: {0}", string.IsNullOrEmpty(controlPath) ? (NoResString)"Empty" : controlPath).AppendLine();
			messageBuilder.AppendFormat((NoResString)"Disposed Control Path: {0}", string.IsNullOrEmpty(control.DisposeControlPath) ? (NoResString)"Empty" : control.DisposeControlPath).AppendLine();
			messageBuilder.AppendLine((NoResString)"Control Dispose stack trace:");
			messageBuilder.AppendLine(control.DisposeStack?.ToString() ?? (NoResString)"Empty");
			messageBuilder.AppendLine((NoResString)"------------Dispose Information End------------");
			if (!additionalInfo.IsNullOrEmpty())
			{
				messageBuilder.AppendLine((NoResString)"------------Additional Information Begin------------");
				messageBuilder.AppendLine(additionalInfo);
				messageBuilder.AppendLine((NoResString)"------------Additional Information End------------");
			}
			return messageBuilder.ToString();
		}
	}

	/// <summary>
	/// Extension methods for the System.Windows.Forms.Control class.
	/// </summary>
	public static class ControlExtensions
	{
		/// <summary>
		/// Get the IControlExtension object of the given type.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T GetExtension<T>(this Control control) where T : class, IControlExtension
		{
			IExtendedControl extendedControl = control as IExtendedControl;
			return extendedControl == null ? null : extendedControl.Extensions.Get<T>();
		}

		/// <summary>
		/// Get the control that currently has focus on the given control or it's children.
		/// </summary>
		public static Control GetFrontMostActiveControl(this Control control)
		{
			if (control == null)
			{
				return null;
			}

			DefaultFrontMostControlProvider provider = FrontMostControlProviderAttribute.GetFrontMostControlProvider(control);
			return provider.GetFrontMostControl(control);
		}

		/// <summary>
		/// Is 'control' within this control or any of it's children?
		/// </summary>
		public static bool ContainsIncludingChildren(this Control containerControl, Control control)
		{
			if (containerControl.Contains(control))
			{
				return true;
			}

			foreach (Control child in containerControl.Controls)
			{
				if (ContainsIncludingChildren(child, control))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get the top-level control of a control.
		/// If the control is on a form, the form is returned.
		/// If the control is not placed on a control or is hosted within the browser,
		/// the root level control is returned.
		/// </summary>
		public static Control GetTopLevelNonParentedControl(this Control control)
		{
			Control result = control;
			while (result.Parent != null)
			{
				result = result.Parent;
			}
			return result;
		}

		/// <summary>
		/// Get whether the given control or any of its parents are in design mode.
		/// This will return false until the Site is assigned, in which case the value of this method
		/// cannot be relied upon when:<br/>
		/// <br/>
		/// In the constructor of the control.<br/>
		/// In the constructor of a parent control.<br/>
		/// </summary>
		public static bool IsDesignMode(this Control control)
		{
			bool result = (control as IComponent).IsDesignMode();
			if (!result)
			{
				Control parent = control.Parent;
				result = parent != null && parent.IsDesignMode();
				if (!result)
				{
					result = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
				}
			}
			return result;
		}

		#region Get/SetBindingMember

		/// <summary>
		/// Gets the binding member for the control. The BindingMember will be taken from the most immediate parent that
		/// implements ICompositeControlBindingSourceProvider.
		/// </summary>
		public static string GetBindingMember(this Control control)
		{
			if (control == null)
			{
				throw new ArgumentNullException(nameof(control));
			}
			return ControlBindingMemberHelper.Get(control).BindingMember;
		}

		/// <summary>
		/// Sets the binding member for the control. The BindingMember will be set on the KBindingSource of
		/// the most immediate parent that implements ICompositeControlBindingSourceProvider. If one is not
		/// available, it is stored temporarily until an ancestor that implements ICompositeControlBindingSourceProvider
		/// becomes available.
		/// </summary>
		public static void SetBindingMember(this Control control, string value)
		{
			if (control == null)
			{
				throw new ArgumentNullException(nameof(control));
			}
			ControlBindingMemberHelper.Get(control).BindingMember = value;
		}

		#endregion

		#region AncestorChanged event

		/// <summary>
		/// Add an event that is fired when the Parent property or the Parent property of a control's ancestor has changed.
		/// </summary>
		public static void AddAncestorChanged(this Control control, EventHandler handler)
		{
			ControlAncestorChangedEvent.Get(control).AncestorChanged += handler;
		}

		/// <summary>
		/// Remove an AncestorChanged event handler.
		/// </summary>
		public static void RemoveAncestorChanged(this Control control, EventHandler handler)
		{
			ControlAncestorChangedEvent.Get(control).AncestorChanged -= handler;
		}

		#endregion

		#region GetReadOnly / SetReadOnly

		public static bool GetReadOnly(this Control control)
		{
			var readOnlyProperty = control.GetReadOnlyPropertyDescriptor();
			if (readOnlyProperty != null)
			{
				return (bool)readOnlyProperty.GetValue(control);
			}
			else
			{
				return !control.Enabled;
			}
		}

		public static void SetReadOnly(this Control control, bool value)
		{
			var readOnlyProperty = control.GetReadOnlyPropertyDescriptor();
			if (readOnlyProperty != null && readOnlyProperty.HasSetter())
			{
				readOnlyProperty.SetValue(control, value);
			}
			else
			{
				control.Enabled = !value;
			}
		}

		public static PropertyDescriptor GetReadOnlyPropertyDescriptor(this Control control)
		{
			return TypeDescriptor.GetProperties(control)["ReadOnly"]
				?? BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(control.GetType(), MetaDataTypes.ReadOnly);
		}

		#endregion

		#region SelectNextControlNonTabStopNonReadOnly

		/// <summary>
		/// Activates the next control. Controls with TabStop=false and GetReadOnly()=true are skipped.
		/// </summary>
		public static bool SelectNextControlNonTabStopNonReadOnly(this Control @this, Control control, bool forward, bool nested, bool wrap)
		{
			// this code was taken from reflector.
			if (!@this.Contains(control) || (!nested && (control.Parent != @this)))
			{
				control = null;
			}
			bool flag1 = false;
			Control current = control;
			do
			{
				control = @this.GetNextControl(control, forward);
				if (control == null)
				{
					if (!wrap)
					{
						break;
					}
					if (flag1)
					{
						return false;
					}
					flag1 = true;
				}
				else if ((control.CanSelect && (control.TabStop && !control.GetReadOnly())) && (nested || (control.Parent == @this)))
				{
					SelectNonReadOnlyIfPossible(control, true, forward);
					return true;
				}
			}
			while (control != current);
			return false;
		}

		static void SelectNonReadOnlyIfPossible(Control @this, bool directed, bool forward)
		{
			ContainerControl containerControl = @this as ContainerControl;
			if (containerControl != null && IsSelectMethodDeclaredOnContainerControl(containerControl))
			{
				SelectNonReadOnly(containerControl, directed, forward);
			}
			else
			{
				typeof(Control).InvokeMember("Select", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, @this, new object[] { directed, forward }, CultureInfo.InvariantCulture);
			}
		}

		static bool IsSelectMethodDeclaredOnContainerControl(ContainerControl control)
		{
			MethodInfo method = control.GetType().GetMethod("Select", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(bool), typeof(bool) }, null);
			return method.DeclaringType == typeof(ContainerControl);
		}

		public static void SelectNonReadOnly(ContainerControl control, bool directed, bool forward)
		{
			bool flag = true;
			if (control.Parent != null)
			{
				IContainerControl current = control.Parent.GetContainerControl();
				if (current != null)
				{
					current.ActiveControl = control;
					flag = current.ActiveControl == control;
				}
			}
			if (directed && flag)
			{
				control.SelectNextControlNonTabStopNonReadOnly(null, forward, true, false);
			}
		}

		internal static MethodInfo PerformControlValidationMethodInfo => performControlValidationMethodInfo ?? (performControlValidationMethodInfo = typeof(Control).GetMethod("PerformControlValidation", BindingFlags.NonPublic | BindingFlags.Instance));

		[ThreadStatic]
		static MethodInfo performControlValidationMethodInfo;

		public static void PerformControlValidation(this Control control)
		{
			PerformControlValidationMethodInfo?.Invoke(control, new object[] { false });
		}

		#endregion

		#region GetUserData

		/// <summary>
		/// Create a key suitable to be passed to GetUserData / SetUserData.
		/// </summary>
		/// <returns></returns>
		public static int CreateUserDataKey()
		{
			lock (createUserDataKeyMutex)
			{
				return (int)ControlPropertiesCreateKeyMethod.Invoke(null, null);
			}
		}
		[ThreadSafe]
		static readonly object createUserDataKeyMutex = new object();

		/// <summary>
		/// Get a user-defined property value from a control.
		/// </summary>
		public static object GetUserData(this Control control, int key)
		{
			if (control.IsDisposed)
			{
				IDisposeStackProvider disposeStackProvider = control as IDisposeStackProvider;
				string disposeStackString = disposeStackProvider != null && disposeStackProvider.DisposeStack != null
					? (NoResString)"\r\nDispose stack trace:\r\n" + disposeStackProvider.DisposeStack.ToString()
					: string.Empty;
				throw new ObjectDisposedException("control", string.Format(CultureInfo.CurrentCulture, "Cannot access a disposed object with type '{0}' and name '{1}'.{2}",
					control.GetType().FullName, control.Name, disposeStackString));
			}
			return GetControlObjectPropertyDelegate(control, key);
		}

		/// <summary>
		/// Set a user-defined property value on a control.
		/// </summary>
		public static void SetUserData(this Control control, int key, object value)
		{
			if (control.IsDisposed)
			{
				throw new ObjectDisposedException("control");
			}

			SetControlObjectPropertyDelegate(control, key, value);
		}

		delegate object ControlObjectPropertyGetter(Control control, int key);
		delegate void ControlObjectPropertySetter(Control control, int key, object value);

		static ControlObjectPropertyGetter GetControlObjectPropertyDelegate
		{
			get
			{
				if (getControlObjectPropertyDelegate == null)
				{
					DynamicMethod method = new DynamicMethod("GetControlObjectProperty", typeof(object), new Type[] { typeof(Control), typeof(int) }, typeof(ControlExtensions), true);
					ILGenerator il = method.GetILGenerator();
					il.Emit(OpCodes.Ldarg_0);
					il.EmitCall(OpCodes.Call, ControlPropertiesProperty.GetGetMethod(true), null);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Call, ControlPropertiesGetObjectMethod, null);
					il.Emit(OpCodes.Ret);
					getControlObjectPropertyDelegate = (ControlObjectPropertyGetter)method.CreateDelegate(typeof(ControlObjectPropertyGetter));
				}
				return getControlObjectPropertyDelegate;
			}
		}
		[ThreadSafe]
		static ControlObjectPropertyGetter getControlObjectPropertyDelegate;

		static ControlObjectPropertySetter SetControlObjectPropertyDelegate
		{
			get
			{
				if (setControlObjectPropertyDelegate == null)
				{
					DynamicMethod method = new DynamicMethod("GetControlObjectProperty", typeof(void), new Type[] { typeof(Control), typeof(int), typeof(object) }, typeof(ControlExtensions), true);
					ILGenerator il = method.GetILGenerator();
					il.Emit(OpCodes.Ldarg_0);
					il.EmitCall(OpCodes.Call, ControlPropertiesProperty.GetGetMethod(true), null);
					il.Emit(OpCodes.Ldarg_1);
					il.Emit(OpCodes.Ldarg_2);
					il.EmitCall(OpCodes.Call, ControlPropertiesSetObjectMethod, null);
					il.Emit(OpCodes.Ret);
					setControlObjectPropertyDelegate = (ControlObjectPropertySetter)method.CreateDelegate(typeof(ControlObjectPropertySetter));
				}
				return setControlObjectPropertyDelegate;
			}
		}
		[ThreadSafe]
		static ControlObjectPropertySetter setControlObjectPropertyDelegate;

		[ThreadSafe]
		static PropertyInfo ControlPropertiesProperty
		{
			get { return controlPropertiesProperty ?? (controlPropertiesProperty = typeof(Control).GetProperty("Properties", BindingFlags.NonPublic | BindingFlags.Instance)); }
		}
		[ThreadSafe]
		static PropertyInfo controlPropertiesProperty;

		[ThreadSafe]
		static MethodInfo ControlPropertiesGetObjectMethod
		{
			get { return controlPropertiesGetObjectMethod ?? (controlPropertiesGetObjectMethod = ControlPropertiesProperty.PropertyType.GetMethod("GetObject", new Type[] { typeof(int) }, null)); }
		}
		[ThreadSafe]
		static MethodInfo controlPropertiesGetObjectMethod;

		[ThreadSafe]
		static MethodInfo ControlPropertiesSetObjectMethod
		{
			get { return controlPropertiesSetObjectMethod ?? (controlPropertiesSetObjectMethod = ControlPropertiesProperty.PropertyType.GetMethod("SetObject", new Type[] { typeof(int), typeof(object) }, null)); }
		}
		[ThreadSafe]
		static MethodInfo controlPropertiesSetObjectMethod;

		static MethodInfo ControlPropertiesCreateKeyMethod
		{
			get { return controlPropertiesCreateKeyMethod ?? (controlPropertiesCreateKeyMethod = ControlPropertiesProperty.PropertyType.GetMethod("CreateKey", BindingFlags.Public | BindingFlags.Static, null, Array.Empty<Type>(), null)); }
		}
		[ThreadSafe]
		static MethodInfo controlPropertiesCreateKeyMethod;

		#endregion

		#region GetParent

		/// <summary>
		/// Traverses the control.Parent hierarchy until .Parent is of type T or null.
		/// </summary>
		/// <returns>The first parent of type T found. Null if no parent of type T is found.</returns>
		public static T GetParent<T>(this Control control)
			where T : Control
		{
			var parent = (control != null) ? control.Parent : null;

			while (parent != null)
			{
				var result = parent as T;
				if (result != null)
				{
					return result;
				}
				parent = parent.Parent;
			}

			return null;
		}

		#endregion

		#region Suspend/Resume Drawing

		public static void SuspendDrawing(this Control parent)
		{
			if (!parent.Created || !parent.IsHandleCreated)
			{
				return;
			}

#if !WINZOR
			NativeMethods.SendMessage(parent.Handle, NativeMethods.WM_SETREDRAW, UIntPtr.Zero, IntPtr.Zero);
#else
			parent.SuspendRedraw = true;
#endif
		}

		public static void ResumeDrawing(this Control parent, bool refresh = true)
		{
			if (!parent.Created || !parent.IsHandleCreated)
			{
				return;
			}

#if !WINZOR
			NativeMethods.SendMessage(parent.Handle, NativeMethods.WM_SETREDRAW, new UIntPtr(1), IntPtr.Zero);
#else
			parent.SuspendRedraw = false;
#endif

			if (refresh)
			{
				parent.Refresh();
			}
		}

		public static bool IsLayoutSuspended(this Control parent)
		{
			#if NETFRAMEWORK || WINZOR
			const string fieldName = "layoutSuspendCount";
			#else
			const string fieldName = "<LayoutSuspendCount>k__BackingField";
			#endif
			return (byte)(typeof(Control).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(parent)) > 0;
		}

		#endregion

		#region RemoveAndDisposeAll

		public static void RemoveAndDisposeAll(this Control.ControlCollection controls)
		{
			foreach (var control in controls.Cast<Control>().ToArray())
			{
				control.Dispose();
			}
			controls.Clear();
		}

		#endregion

		#region InvokeSafe

		public static void BeginInvokeSafe(this Control control, Action action)
		{
			InvokeCore(control, control.BeginInvoke, action);
		}

		public static void InvokeSafe(this Control control, Action action)
		{
			InvokeCore(control, control.Invoke, action);
		}

		static void InvokeCore(Control control, Func<Delegate, object> invoke, Action action)
		{
			if (IsInvokeForced || control.InvokeRequired)
			{
				if (control.IsHandleCreated && !control.IsDisposed)
				{
					invoke(action);
				}
			}
			else if (control.IsHandleCreated)
			{
				action();
			}
		}

		public static bool IsCallingFromOwnedThread(this Control control)
		{
			if (control.InvokeRequired)
			{
				ErrorReporter.ReportOnce("3c0529aa-57ae-4517-8953-d818052e544d", string.Format(CultureInfo.InvariantCulture, "This operation should not be called on a thread that does not belong to the control [{0}].", control));
				return false;
			}
			else
			{
				return true;
			}
		}

		[ThreadStatic]
		static int forceInvokeCounter;
		static bool IsInvokeForced => forceInvokeCounter > 0;

		static void ForceInvokeCallsCore()
		{
			forceInvokeCounter++;
		}

		static void ReleaseInvokeCallsCore()
		{
			if (forceInvokeCounter > 0)
			{ forceInvokeCounter--; }
		}

		public static IDisposable ForceInvokeCalls()
		{
			ForceInvokeCallsCore();
			return new DisposableAction(ReleaseInvokeCallsCore);
		}

		#endregion

		#region PaintGradientBackground

		public static void PaintGradientBackground(this Control control, Color startColor, Color endColor, float gradientAngle, Graphics g, ColorBlend blend = null)
		{
			using (var brush = new LinearGradientBrush(control.ClientRectangle, startColor, endColor, gradientAngle))
			{
				if (blend != null)
				{
					brush.InterpolationColors = blend;
				}

				g.FillRectangle(brush, control.ClientRectangle);
			}
		}

		#endregion

#if !WINZOR

		static class NativeMethods
		{
			[DllImport("user32.dll")]
			internal static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, UIntPtr wParam, IntPtr lParam);

			internal const uint WM_SETREDRAW = 11;
		}

#endif
	}
}
