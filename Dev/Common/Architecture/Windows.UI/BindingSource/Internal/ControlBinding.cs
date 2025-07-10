using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Manages the data bindings for the value and meta-data of a control.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	[ProvideProperty("BindingMember", typeof(Control))]
	[ProvideProperty("BindingMembersForCompileTimeCheck", typeof(Control))]
	internal sealed class ControlBinding : IDisposable
	{
		public ControlBinding(Control control, KBindingSource owner)
		{
			if (control == null)
			{
				throw new ArgumentNullException(nameof(control));
			}
			this.Control = control;
			this.Owner = owner;

			Control.Disposed += new EventHandler(Control_Disposed);
			BindingReadyNotifier.ReadyChanged += delegate
			{ UpdateIsBinding(); };
			UpdateIsBinding();
		}

		/// <summary>
		/// Get the control this binding manages.
		/// </summary>
		public Control Control { get; private set; }

		/// <summary>
		/// Get the KBindingSource this binding belongs to.
		/// </summary>
		public KBindingSource Owner { get; private set; }

		#region Enabled / IsBinding / UpdateIsBinding

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				if (enabled != value)
				{
					enabled = value;
					UpdateIsBinding();
				}
			}
		}
		bool enabled;

		bool IsBinding
		{
			get { return isBinding; }
			set
			{
				if (isBinding != value)
				{
					if (value)
					{
						StartBinding();
					}
					else
					{
						StopBinding();
					}
					isBinding = value;
				}
			}
		}
		bool isBinding;

		void UpdateIsBinding()
		{ IsBinding = (Enabled && Owner.DataSource != null && (Owner.IsDesignMode || BindingReadyNotifier.Ready)); }

		ReadyForBindingNotifier BindingReadyNotifier
		{ get { return bindingReadyNotifier ?? (bindingReadyNotifier = new ReadyForBindingNotifier(Control)); } }
		ReadyForBindingNotifier bindingReadyNotifier;

		#endregion

		#region StartBinding

		void StartBinding()
		{
			if (Owner.IsDesignMode)
			{
				StartBindingForDesignMode();
			}
			else
			{
				StartBindingForRuntimeNow();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void StartBindingForDesignMode()
		{
			fullBindingMemberPopulated = false;
			IDataBoundControl boundControl = Control as IDataBoundControl;
			if (boundControl != null &&
				(boundControl.DataSource == null || boundControl.DataSource is DesignTimeBindingSource))
			{
				try
				{
					boundControl.SetDataBinding(Owner.DataSource, FullBindingMember.BindingMember);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		void StartBindingForRuntimeNow()
		{
			fullBindingMemberPopulated = false;
			CheckControlIsBindable(Control);

			if (Owner.SupportMultipleTwoWayBoundPropertiesOnOneControl)
			{
				Control.Validating += Control_ValidatingFor2BoundPropertiesHack;
			}
			Control.SuspendLayout();

			// order is important for the Validated handlers
			Control.Validated += Control_ValidatedForEndCurrentEdit;
			if (Owner.SupportMultipleTwoWayBoundPropertiesOnOneControl)
			{
				Control.Validated += Control_ValidatedFor2BoundPropertiesHack;
				Control.Enter += new EventHandler(Control_Enter);
			}

			try
			{
				var form = Control.FindForm();
				using (PerformanceStatisticsCollector.StartMonitoring("BindControl", form != null ? form.GetType().FullName : null))
				{
					DataBoundControl.Get(Control).SetDataBinding(Owner.DataSource, FullBindingMember.BindingMember);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!Control.Disposing)
				{
					Control.ResumeLayout();
				}
				string message = ex.Message;
				if (Owner.DataSource != null && ex is ArgumentException && ex.Message.StartsWith(BindingMemberNotFoundExceptionMessageStart, StringComparison.Ordinal))
				{
					message = (NoResString)"Could not find BindingMember '" + FullBindingMember.BindingMember + (NoResString)"' on DataSource type '" + Owner.DataSource.GetType().FullName + "'";
					message += (NoResString)"\r\nEnsure that the binding member is a property, not a field. Also check whether its need to be a collection rather than a list";
				}
				else
				{
					message += $" Data Source: {Owner?.DataSource?.GetType().FullName}, Control Type: {Control.GetType().FullName}, Control Name: {Control.Name}, Binding Member: {FullBindingMember.BindingMember}";
				}
				throw new KDataBindingException(message, ex);
			}
			if (!Control.Disposing)
			{
				Control.ResumeLayout();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "notUsed")]
		static string BindingMemberNotFoundExceptionMessageStart
		{
			get
			{
				if (bindingMemberNotFoundExceptionMessageStart == null)
				{
					try
					{
						var notUsed = new BindingContext()[new object(), "BindingMember"];
					}
					catch (ArgumentException ex)
					{
						bindingMemberNotFoundExceptionMessageStart = ex.Message.Substring(0, ex.Message.IndexOf("BindingMember", StringComparison.Ordinal)); // localization has been taken into account
					}
				}
				return bindingMemberNotFoundExceptionMessageStart;
			}
		}
		[ThreadStatic]
		static string bindingMemberNotFoundExceptionMessageStart;

		#endregion

		#region StopBinding

		void StopBinding()
		{
			if (Owner.IsDesignMode)
			{
				StopBindingForDesignMode();
			}
			else
			{
				StopBindingForRuntime();
			}
		}

		void StopBindingForDesignMode()
		{
			IDataBoundControl boundControl = Control as IDataBoundControl;
			if (boundControl != null && boundControl.DataSource is DesignTimeBindingSource)
			{
				boundControl.SetDataBinding(null, "");
			}
		}

		void StopBindingForRuntime()
		{
			Control.Validating -= Control_ValidatingFor2BoundPropertiesHack;
			Control.Validated -= Control_ValidatedFor2BoundPropertiesHack;
			Control.Validated -= Control_ValidatedForEndCurrentEdit;
			Control.Enter -= Control_Enter;
			Control.Disposed -= Control_Disposed;

			if (Control is IDataBoundControl)
			{
				((IDataBoundControl)Control).SetDataBinding(null, "");
				DataBoundControl.SetDataBindingForMetadataProperties(Control, null, "");
			}
			else
			{
				DataBoundControl.GetDefaultImplementation(Control).SetDataBinding(null, "");
			}
		}

		#endregion

		#region ForceBinding

		public void ForceBinding()
		{
			BindingReadyNotifier.Force();
		}

		#endregion

		#region RelativeBindingMember / FullBindingMember

		/// <summary>
		/// Get the value of BindingMember for a given control. This information is taken from
		/// Control.DataBindings.
		/// </summary>
		public string RelativeBindingMember
		{
			get { return relativeBindingMember; }
			set
			{
				value = value == null ? null : value.Trim();
				CheckNotNullOrEmpty(value, (NoResString)"value");
				CheckControlIsBindable(Control);

				using (ModifyBindingInformation())
				{
					relativeBindingMember = value;
					fullBindingMemberPopulated = false;
				}
				SetNameFromBindingMemberIfRequired();
			}
		}
		string relativeBindingMember = "";

		/// <summary>
		/// Get the full bind to member, including the path. This is a blank string
		/// if RelativeBindingMember is blank, or is a dot ('.').
		/// </summary>
		internal KBindingMemberInfo FullBindingMember
		{
			get
			{
				if (!fullBindingMemberPopulated)
				{
					string newFullBindingMember = "";
					string topLevel = Owner.DataMember;
					if (string.IsNullOrEmpty(RelativeBindingMember))
					{
						newFullBindingMember = "";
					}
					else if (!string.IsNullOrEmpty(topLevel))
					{
						newFullBindingMember = topLevel;
						if (!string.IsNullOrEmpty(RelativeBindingMember) && RelativeBindingMember != ".")
						{
							newFullBindingMember += "." + RelativeBindingMember;
						}
					}
					else
					{
						newFullBindingMember = (RelativeBindingMember == ".") ? "" : RelativeBindingMember;
					}
					fullBindingMember = new KBindingMemberInfo(newFullBindingMember);
					fullBindingMemberPopulated = true;
				}
				return fullBindingMember;
			}
		}
		KBindingMemberInfo fullBindingMember;
		bool fullBindingMemberPopulated;

		#endregion

		#region SetNameFromBindingMemberIfRequired

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		void SetNameFromBindingMemberIfRequired()
		{
			if (Owner.SynchroniseNameWithBindingMemberInSmartTags &&
				KDesignerActionList.IsSettingDesignerActionListProperty &&
				Owner.IsDesignMode() && // too early to access BindingSource.IsDesignMode here
				DesignTimeControlNameGeneratorAttribute != null)
			{
				PropertyDescriptor nameProperty = TypeDescriptor.GetProperties(Control)["Name"];
				string newName = DesignTimeControlNameGeneratorAttribute.GenerateNameFromBindingMember(RelativeBindingMember);

				if (newName.Replace("_", "").Length > 0 && newName != (string)nameProperty.GetValue(Control))
				{
					nameProperty.SetValue(Control, CreateUniqueComponentName(newName));
				}
			}
		}

		string CreateUniqueComponentName(string name)
		{
			string result = name;
			if (Owner.Site != null)
			{
				for (int i = 1; Owner.Site.Container.Components[result] != null; i++)
				{
					result = name + i;
				}
			}
			return result;
		}

		DesignTimeControlNameGeneratorAttribute DesignTimeControlNameGeneratorAttribute
		{
			get
			{
				DesignTimeControlNameGeneratorAttribute attr = (DesignTimeControlNameGeneratorAttribute)TypeDescriptor.GetAttributes(Control)[typeof(DesignTimeControlNameGeneratorAttribute)];
				return attr;
			}
		}

		#endregion

		#region Compile Time Checking Code Serialization

		/// <summary>
		/// This method is intended for compile-time checking only and should not be used in code.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "fullBindingMember")]
		internal CompileTimeCheckBindingMemberCollection BindingMembersForCompileTimeCheck
		{
			get
			{
				CompileTimeCheckBindingMemberCollection result = new CompileTimeCheckBindingMemberCollection();
				if (!string.IsNullOrEmpty(FullBindingMember.BindingMember) || (GetControlValuePropertyType() != typeof(object) && Owner.DataSourceType != GetControlValuePropertyType()))
				{
					string fullBindingMember = string.IsNullOrEmpty(FullBindingMember.BindingMember) ? "." : FullBindingMember.BindingMember;
					result.Add(Owner.NewCompileTimeCheckBindingMember(Owner.DataSourceType, GetControlValuePropertyType(), fullBindingMember));
				}

				IBindingMemberForCompileTimeCheckProvider provider = Control as IBindingMemberForCompileTimeCheckProvider;
				if (provider != null)
				{
					result.AddRange(provider.GetBindingMembersForCompileTimeCheck(Owner.DataSourceType, fullBindingMember));
				}
				return result;
			}
		}

		internal bool ShouldSerializeBindingMembersForCompileTimeCheck
		{ get { return BindingMembersForCompileTimeCheck.Count > 0; } }

		Type GetControlValuePropertyType()
		{
			Type result;
			if (Control is IDataBoundControl)
			{
				result = ((IDataBoundControl)Control).DataSourceType ?? typeof(object);
			}
			else
			{
				result = DataBoundControl.GetDefaultImplementation(Control).DataSourceType;
			}
			return result;
		}

		#endregion

		#region Hack to allow 2 DataBindings on the same control

		readonly List<IDisposable> changeSuspenders = new List<IDisposable>();
		bool controlValidating;

		void Control_ValidatingFor2BoundPropertiesHack(object sender, CancelEventArgs e)
		{
			controlValidating = true;
			ResumeBindingsIfRequired();
			if (HasMoreThanOneTwoWayKBinding(Control.DataBindings))
			{
				foreach (Binding binding in new ArrayList(Control.DataBindings))
				{
					if (binding.BindingManagerBase != null &&
						!binding.BindingManagerBase.IsBindingSuspended)
					{
						try
						{
							// EndCurrentEdit is performed as part of SuspendBinding.
							// Exception is caught in case bindings are removed during the SuspendBinding.
							binding.BindingManagerBase.EndCurrentEdit();
						}
						catch (ArgumentOutOfRangeException)
						{
						}
						binding.BindingManagerBase.SuspendBinding();
						changeSuspenders.Add(new DisposableAction(binding.BindingManagerBase.ResumeBinding));
					}
				}
			}
		}

		static bool HasMoreThanOneTwoWayKBinding(ControlBindingsCollection dataBindings)
		{
			bool foundOne = false;
			foreach (Binding binding in dataBindings)
			{
				if (binding.DataSourceUpdateMode != DataSourceUpdateMode.Never)
				{
					if (foundOne)
					{
						return true;
					}
					foundOne = true;
				}
			}
			return false;
		}

		void Control_Enter(object sender, EventArgs e)
		{
			if (controlValidating)
			{
				ResumeBindingsIfRequired();
			}
			controlValidating = false;
		}

		void Control_ValidatedFor2BoundPropertiesHack(object sender, EventArgs e)
		{
			if (controlValidating)
			{
				Control parent = Control.Parent;
				if (string.IsNullOrEmpty(Owner.GetBindingMember(parent)))
				{
					foreach (Control control in Control.Controls)
					{
						ControlBinding binding = Owner.GetControlBinding(control);
						if (binding != null)
						{
							binding.ResumeBindingsIfRequired();
						}
					}
					ResumeBindingsIfRequired();
				}
			}
			controlValidating = false;
		}

		void ResumeBindingsIfRequired()
		{
			IDisposable[] changeSuspenderList = changeSuspenders.ToArray();
			changeSuspenders.Clear();
			foreach (IDisposable changeSuspender in changeSuspenderList)
			{
				try
				{
					changeSuspender.Dispose();
				}
				catch (IndexOutOfRangeException)
				{
				}
			}
		}

		#endregion

		#region IDisposable Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		public void Dispose()
		{
			Control.Disposed -= Control_Disposed;
			Enabled = false;
			BindingReadyNotifier.Dispose();
		}

		#endregion

		#region Implementation

		IDisposable ModifyBindingInformation()
		{
			bool currentlyEnabled = IsBinding;
			IsBinding = false;
			return new DisposableAction(delegate
			{ IsBinding = currentlyEnabled; });
		}

		static void CheckControlIsBindable(Control control)
		{
			PropertyDescriptor controlProperty = BindableComponentMetaDataPropertyLocator.GetInstance(control.GetType()).DefaultBindingProperty;
			if (controlProperty == null && !(control is IDataBoundControl))
			{
				throw new KDataBindingException(
					"Cannot bind to control of type " + control.GetType().FullName + ". " +
					"If you want to enable binding on this type of control, apply " +
					nameof(DefaultBindingPropertyAttribute) +
					" to the control type, or implement " +
					typeof(IDataBoundControl).FullName + " on the control.");
			}
		}

		static void CheckNotNullOrEmpty(string value, string parameterName)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("value cannot be an empty string", parameterName);
			}
		}

		void Control_ValidatedForEndCurrentEdit(object sender, EventArgs e)
		{ KEndCurrentEdit.EndCurrentEdit(Control); }

		void Control_Disposed(object sender, EventArgs e)
		{ Dispose(); }

		#endregion
	}
}
