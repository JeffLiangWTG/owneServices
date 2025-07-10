using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class ZControlBaseTestCase<T> : ControlTestCase<T> where T : Control, new()
	{
		#region IIsVisibleForBindingControl

		[RequiresSTA]
		public void TestIsVisibleForBinding()
		{
			if (ShouldTestIIsVisibleForBinding())
			{
				// re-entrantly set the IsVisibleForBinding, expect no StackOverflowException because this is what changing TabPage's sometimes does
				Control.VisibleChanged += delegate
				{ ((IIsVisibleForBindingControl)Control).IsVisibleForBinding = !Control.Visible; };

				using (var form = new ZForm(Dummy))
				{
					form.Show();
					form.Controls.Add(Control);
					Application.DoEvents();

					AssertEquals("Expect IsVisibleForBinding=true, without a stack overflow", true, VisibleForBindingControl.IsVisibleForBinding);
					AssertEquals("Expect Visible=true", true, Control.Visible);

					VisibleForBindingControl.IsVisibleForBinding = false;
					AssertEquals("Expect IsVisibleForBinding=false, without a stack overflow", false, VisibleForBindingControl.IsVisibleForBinding);
					AssertEquals("Expect Visible=false", false, Control.Visible);
				}
			}
		}

		[RequiresSTA]
		public void TestIsVisibleForBindingChanged()
		{
			if (ShouldTestIIsVisibleForBinding())
			{
				var isVisibleForBindingChangedCalled = false;
				VisibleForBindingControl.IsVisibleForBindingChanged += delegate
				{ isVisibleForBindingChangedCalled = true; };
				Assert("Pre-condition", VisibleForBindingControl.IsVisibleForBinding);
				Assert("Pre-condition", !isVisibleForBindingChangedCalled);

				VisibleForBindingControl.IsVisibleForBinding = true;
				Assert("Should still be true", VisibleForBindingControl.IsVisibleForBinding);
				Assert("Value is not changed, should not be called", !isVisibleForBindingChangedCalled);

				VisibleForBindingControl.IsVisibleForBinding = false;
				Assert("Should now be changed", !VisibleForBindingControl.IsVisibleForBinding);
				Assert("Value is changed, should be called", isVisibleForBindingChangedCalled);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestIsVisibleForBindingChanged_NoExceptionThrownWhenUnhooked()
		{
			if (ShouldTestIIsVisibleForBinding())
			{
				Assert("Pre-condition", VisibleForBindingControl.IsVisibleForBinding);
				VisibleForBindingControl.IsVisibleForBinding = false;
			}
		}

		bool ShouldTestIIsVisibleForBinding()
		{
			bool result;

			if (VisibleForBindingControl != null)
			{
				result = true;
			}
			else
			{
				result = false;
				Assert("IIsVisibleForBindingControl is not implemented for this control, ignore test", true);
			}

			return result;
		}

		IIsVisibleForBindingControl VisibleForBindingControl
		{
			get { return Control as IIsVisibleForBindingControl; }
		}

		#endregion

		#region IIsEnabledForBindingControl

		[RequiresSTA]
		public void TestIsEnabledForBinding()
		{
			if (ShouldTestIIsEnabledForBinding())
			{
				Control.EnabledChanged += new EventHandler(OnControl_EnabledChanged);
				using (var form = new ZForm())
				{
					form.Show();
					form.Controls.Add(Control);
					Application.DoEvents();

					AssertEquals("Expect IsEnabledForBinding=true, without a stack overflow", true, EnabledForBindingControl.IsEnabledForBinding);
					AssertEquals("Expect Enabled=true", true, Control.Enabled);

					EnabledForBindingControl.IsEnabledForBinding = false;
					AssertEquals("Expect IsEnabledForBinding=false, without a stack overflow", false, EnabledForBindingControl.IsEnabledForBinding);
					AssertEquals("Expect Enabled=false", false, Control.Enabled);
				}
			}
		}

		[RequiresSTA]
		public void TestIsEnabledForBindingChanged()
		{
			if (ShouldTestIIsEnabledForBinding())
			{
				EnabledForBindingControl.IsEnabledForBindingChanged += new EventHandler(EnabledForBindingControl_IsEnabledForBindingChanged);
				Assert("Pre-condition", EnabledForBindingControl.IsEnabledForBinding);
				Assert("Pre-condition", !IsEnabledForBindingChangedCalled);

				EnabledForBindingControl.IsEnabledForBinding = true;
				Assert("Should still be true", EnabledForBindingControl.IsEnabledForBinding);
				Assert("Value is not changed, should not be called", !IsEnabledForBindingChangedCalled);

				EnabledForBindingControl.IsEnabledForBinding = false;
				Assert("Should now be changed", !EnabledForBindingControl.IsEnabledForBinding);
				Assert("Value is changed, should be called", IsEnabledForBindingChangedCalled);
			}
		}

		[ExpectNoExceptions]
		public void TestIsEnabledForBindingChanged_NoExceptionThrownWhenUnhooked()
		{
			if (ShouldTestIIsEnabledForBinding())
			{
				Assert("Pre-condition", EnabledForBindingControl.IsEnabledForBinding);
				EnabledForBindingControl.IsEnabledForBinding = false;
			}
		}

		bool ShouldTestIIsEnabledForBinding()
		{
			bool result;

			if (EnabledForBindingControl != null)
			{
				result = true;
			}
			else
			{
				result = false;
				Assert("IIsEnabledForBindingControl is not implemented for this control, ignore test", true);
			}

			return result;
		}

		void OnControl_EnabledChanged(object sender, EventArgs e)
		{
			// re-entrantly set the IsEnabledForBinding, expect no StackOverflowException because this is what changing TabPage's sometimes does
			EnabledForBindingControl.IsEnabledForBinding = !Control.Enabled;
		}

		void EnabledForBindingControl_IsEnabledForBindingChanged(object sender, EventArgs e)
		{
			IsEnabledForBindingChangedCalled = true;
		}

		IIsEnabledForBindingControl EnabledForBindingControl
		{
			get { return Control as IIsEnabledForBindingControl; }
		}

		bool IsEnabledForBindingChangedCalled;

		#endregion

		#region IExtendedControl

		[RequiresSTA]
		public void TestExtensionsPropertyNotBrowsable()
		{
			var extensionsProperty = TypeDescriptor.GetProperties(Control)["Extensions"];
			if (extensionsProperty != null)
			{
				AssertEquals("Extensions property not browsable", false, extensionsProperty.IsBrowsable);
			}
			else
			{
				Assert("No extensions property", true);
			}
		}

		#endregion

		#region BindTo Properties

		public void TestBindToPropertyIsBackedByInterface()
		{
			var controlType = typeof(T);
			var iBindToType = typeof(IBindTo);
			//WI00213351: Old way of doing this (directly querying TypeDescriptor.GetProperties(controlType)["BindTo"] would, if it wasn't present, call
			//ControlPropertyDescriptorCollection.CreatePropertyDescription and create it from the existing BindTo property. This isn't what we want
			//(since it permanently changes the collection for all future tests, causing amnesties).
			Assert(controlType.Name + " with property BindTo should implement interface " + iBindToType.FullName,
				controlType.GetProperty("BindTo") == null || iBindToType.IsAssignableFrom(controlType));
		}

		[RequiresSTA]
		public void TestBindToProperties_CorrectAttributes()
		{
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(ControlType))
			{
				if (property.Attributes[typeof(ExcludeFromBindToAttributesTestAttribute)] == null && property.Name.IndexOf("BindTo") != -1)
				{
					AssertBindToPropertyHasCorrectAttributes(property);
				}
			}
			Assert("No BindTo properties is valid for this test", true);
		}

		[RequiresSTA]
		public void TestBindToProperties_DefaultValues()
		{
			using (Control control = GetNewControl())
			{
				foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(ControlType))
				{
					if (property.Name.IndexOf("BindTo") != -1)
					{
						var defaultValueAttribute = (DefaultValueAttribute)property.Attributes[typeof(DefaultValueAttribute)];
						var defaultValue = defaultValueAttribute == null ? null : defaultValueAttribute.Value;
						AssertEquals(property.Name + ": should have a default value of \"\" and [DefaultValueAttribute(\"\")] should be applied", defaultValue, property.GetValue(control));
					}
				}
			}
			Assert("No BindTo properties is valid for this test", true);
		}

		void AssertBindToPropertyHasCorrectAttributes(PropertyDescriptor property)
		{
			var attributeProvider = GetBindToPropertyAttributeProvider(property);
			var bindToOrBindToList = property.Name.IndexOf("List") != -1 ? "BindToList" : "BindTo";

			var message = property.Name + ": [AttributeProvider(ZGUIConstants.BindToPropertyAttributes, \"" + bindToOrBindToList + "\")] must be applied to " + bindToOrBindToList + " properties";
			AssertNotNull(message, attributeProvider);
			AssertEquals(message, typeof(BindToPropertyAttributes), Type.GetType(attributeProvider.TypeName));
			AssertEquals(message, bindToOrBindToList, attributeProvider.PropertyName);
			AssertNotNull("If BindToPropertyAttributes is specified, the BindingSourceDataSourceType property must also exist", property.ComponentType.GetProperty("BindingSourceDataSourceType"));
		}

		AttributeProviderAttribute GetBindToPropertyAttributeProvider(PropertyDescriptor property)
		{
			foreach (Attribute attr in property.Attributes)
			{
				var attributeProvider = attr as AttributeProviderAttribute;
				if (attributeProvider != null)
				{
					var attributeProviderType = Type.GetType(attributeProvider.TypeName);
					if (typeof(BindToPropertyAttributes).IsAssignableFrom(attributeProviderType))
					{
						return attributeProvider;
					}
				}
			}
			return null;
		}

		#endregion

		#region Drag and Drop for eDocs

		[RequiresSTA]
		public void TestOnDragDrop_PassedToForm()
		{
			AssertDragMethodIsHandled("OnDragDrop", typeof(DragDropManager).GetField("DragDropHandledForTesting"));
		}

		[RequiresSTA]
		public void TestOnDragOver_PassedToForm()
		{
			AssertDragMethodIsHandled("OnDragOver", typeof(DragDropManager).GetField("DragOverHandledForTesting"));
		}

		void AssertDragMethodIsHandled(string methodName, FieldInfo verificationField)
		{
			using (var form = new ZForm())
			{
				Control control = GetNewControl();
				AddControlToForm(form, control);

				if (IsDragDropHandledByEDocs)
				{
					var info = control.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
					var dragArgs = new DragEventArgs(new DataObject(), 0, 10, 10, DragDropEffects.Move | DragDropEffects.Copy, DragDropEffects.None);
					info.Invoke(control, new object[] { dragArgs });
					AssertEquals("DragDropManager should handle the " + methodName + " event", true, verificationField.GetValue(null));
				}
				else
				{
					var info = control.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (info != null)
					{
						Assert("You have overridden the " + methodName + " method on this control. If the eDocs tab is supposed to handle the drag event, " +
							"please override the IsDragDropHandledByEDocs property in your test case and return true. If the eDocs tab is not supposed to " +
							"handle the drag event and you are overriding for some other reason, override IsDragDropOverriddenWithoutEDocs and return true.", IsDragDropOverriddenWithoutEDocs);
					}
					Assert(true);
				}
			}
		}

		public void TestOnDragDrop_ShouldRunInMainThread()
		{
			AssertDragMethodRunsInMainThread("OnDragDrop");
		}

		public void TestOnDragOver_ShouldRunInMainThread()
		{
			AssertDragMethodRunsInMainThread("OnDragOver");
		}

		public void AssertDragMethodRunsInMainThread(string methodName)
		{
			if (!IsDragDropHandledByEDocs || IsDragDropOverriddenWithoutEDocs)
			{
				Assert(true);
				return;
			}

			using (var form = new ZForm())
			{
				Control control = GetNewControl();
				AddControlToForm(form, control);

				var methodInfo = control.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
				var dragArgs = new DragEventArgs(new DataObject(), 0, 10, 10, DragDropEffects.Move | DragDropEffects.Copy, DragDropEffects.None);

				int GetCurrentThreadID() => Thread.CurrentThread.ManagedThreadId;
				int mainThreadID = GetCurrentThreadID();
				int startThreadID = 0;
				int eventThreadID = 0;

				if (methodName == "OnDragDrop")
				{
					form.DragDrop += (_, __) => eventThreadID = GetCurrentThreadID();
				}

				if (methodName == "OnDragOver")
				{
					form.DragOver += (_, __) => eventThreadID = GetCurrentThreadID();
				}

				DefaultAsyncStrategy.Get().DoAsync(() =>
				{
					startThreadID = GetCurrentThreadID();
					methodInfo.Invoke(control, new object[] { dragArgs });
				}).ConfigureAwait(false).GetAwaiter().GetResult();

				AssertNotEquals("Event should have started by another thread", mainThreadID, startThreadID);
				Application.DoEvents();
				AssertEquals("Event should run in the main thread", mainThreadID, eventThreadID);
			}
		}

		protected virtual void AddControlToForm(ZForm form, Control control)
		{
			form.Controls.Add(control);
		}

		protected virtual bool IsDragDropHandledByEDocs => false;

		protected virtual bool IsDragDropOverriddenWithoutEDocs => false;

		#endregion

		#region Bind / Unbind

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBindUnbind()
		{
			var dataBoundControl = Control as IDataBoundControl;
			if (dataBoundControl != null)
			{
				using (var form = new ZForm())
				{
					BindControl();
					if (IsBindControlOverridden &&
						dataBoundControl != null)
					{
						AssertNotNull("IDataBoundControl.DataSource should be set", dataBoundControl.DataSource);
					}
					form.Show();
					Application.DoEvents();
					UserIdleWorker.Flush();

					if (UsesControlDataBindings)
					{
						AssertEquals("DataBindings were added to the control successfully", true, Control.DataBindings.Count > 0);
					}
					UnbindControl(Control);
					CheckNoDataBindingsExist(Control);
				}
			}
		}

		void UnbindControl(Control control)
		{
			var dataBoundControl = control as IDataBoundControl;
			if (dataBoundControl != null)
			{
				dataBoundControl.SetDataBinding(null, "");
			}
		}

		protected virtual bool UsesControlDataBindings
		{
			get { return true; }
		}

		bool IsBindControlOverridden
		{
			get
			{
				var method = GetType().GetMethod("BindControl", BindingFlags.NonPublic | BindingFlags.Instance);
				return method.DeclaringType != typeof(ZControlBaseTestCase<T>);
			}
		}

		protected virtual void BindControl()
		{
		}

		protected virtual void CheckNoDataBindingsExist(Control control)
		{
			AssertEquals("DataBindings.Count on control " + control.Name + " after Unbind()", 0, control.DataBindings.Count);
			foreach (Control childControl in control.Controls)
			{
				CheckNoDataBindingsExist(childControl);
			}
		}

		#endregion

		#region Exposed Metadata

		protected virtual string[] BindablePropertyNames
		{
			get { return Array.Empty<string>(); }
		}

		protected virtual string InvalidBindablePropertyName
		{
			get { return null; }
		}

		public void TestCheckForCustomTypeDescriptorProviderAttribute()
		{
			var type = typeof(T);

			if (RequiresTypeDescriptor)
			{
				Assert("ZControlTypeDescriptionProvider should be used in TypeDescriptionProvider attribute for type " + type.FullName,
						TypeDescriptionProviderAttributeChecker.IsAppliedTo(type));
			}
			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestBindingToProperties()
		{
			foreach (var propertyName in BindablePropertyNames)
			{
				AddDataBindingToControl(propertyName);
			}
		}

		public void TestBindingToExcludedProperty()
		{
			if (string.IsNullOrEmpty(InvalidBindablePropertyName))
			{
				Assert(true);
				return;
			}

			Assert("Should not run this test in Visual Studio mode", !VisualStudioDetector.IsVisualStudio);
			AssertEquals(typeof(ControlPropertyDescriptorCollection), TypeDescriptor.GetProperties(Control).GetType());
			AssertExceptionThrown(typeof(ArgumentException), () => AddDataBindingToControl(InvalidBindablePropertyName));
		}

		public void TestPropertyDescriptorsAreNotDuplicated()
		{
			if (RequiresTypeDescriptor)
			{
				var properties = ZControlTypeDescriptor.GetPropertyDescriptors(typeof(T));

				foreach (var each in properties)
				{
					var propertyDescriptorsFound = Array.FindAll(properties,
						delegate(PropertyDescriptor candidate)
						{
							return candidate.Name == each.Name &&
								   candidate != each;
						});

					Assert("PropertyDescriptor " + each.Name + " is duplicated", propertyDescriptorsFound.Length == 0);
				}
			}
			Assert(true);
		}

		protected virtual bool RequiresTypeDescriptor
		{
			get { return true; }
		}

		#endregion

		#region Implementation

		readonly object dataSource = new object();

		void AddDataBindingToControl(string propertyName)
		{
			Control.DataBindings.Add(new KBinding(propertyName, GetDataSource(propertyName), ""));
		}

		protected virtual object GetDataSource(string propertyName)
		{
			return dataSource;
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected DummyBusinessObject Dummy
		{
			get { return dummy ?? (dummy = GetDummyForBinding()); }
		}
		DummyBusinessObject dummy;

		protected virtual DummyBusinessObject GetDummyForBinding()
		{
			return Factory.New<DummyBusinessObject>();
		}

		#endregion
	}
}
