using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	[TestsSubclassesOf(typeof(OperationalActionMethod))]
	public abstract class OperationalActionMethodTest<MethodT> : TestCaseWithFactory where MethodT : OperationalActionMethod
	{
		public void TestName()
		{
			Assert("Name cannot be empty", !string.IsNullOrEmpty(Method.Name));
		}

		public void TestDescription()
		{
			Assert("Description cannot be empty", !string.IsNullOrEmpty(Method.Description));
		}

		public void TestMethodsMustNotHoldAReferenceToTheApplicator()
		{
			WeakReference weakApplicatorRef = GetWeakApplicatorRef();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			const string message = "The method supporter must not hold a reference to the applicator as the method supporter\n" + "has a much longer life span than the applicator and would result in a memory leak\n" + "";
			AssertEquals(message, false, weakApplicatorRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference GetWeakApplicatorRef()
		{
			return new WeakReference(NewApplicator(Method, new BusinessObjectFactory()));
		}

		public void TestNewGuiControl()
		{
			if (Method.HasControl)
			{
				using (IComponent control = Method.NewGuiControl())
				{
					AssertNotNull(string.Format("{0} claimed to have a gui but NewGuiControl returned null", Method.GetType().Name), control);
				}
			}
			else
			{
				AssertExceptionThrown("NewGuiControl should throw a NotSupportedException if HasGuiControl returns false", typeof(NotSupportedException), delegate
				{
					using (Method.NewGuiControl())
					{
					}
				});
			}
		}

		public void TestNewApplicator()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OperationalActionMethodApplicator applicator = NewApplicator(Method, factory);
			AssertNotNull("NewApplicator should not return null", applicator);
			if (applicator.Factory != null)
			{
				AssertEquals("if the applicator has a factory, it must be the passed in factory", factory, applicator.Factory);
			}
		}

		[RequiresSTA]
		public void TestApplicatorBinding()
		{
			if (Method.HasControl)
			{
				using (ZForm form = new ZForm(Factory.New<DummyBusinessObject>()))
				{
					form.ClientSize = new Size(300, 300);
					ApplicatorGuiHost control = new ApplicatorGuiHost(Method);
					control.Dock = DockStyle.Fill;
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();
				}
			}

			Assert(true);
		}

		public void TestMethodsMustNotHoldAReferenceToTheSettings()
		{
			if (!Method.HasSettings)
			{
				Assert(true);
			}
			else
			{
				WeakReference weakSettingRef = GetWeakSettingRef();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				const string message = "The method supporter must not hold a reference to the setting object as the method supporter\n" + "has a much longer life span than the setting and would result in a memory leak\n" + "";
				AssertEquals(message, false, weakSettingRef.IsAlive);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference GetWeakSettingRef()
		{
			return new WeakReference(Method.NewSetting(new BusinessObjectFactory()));
		}

		[RequiresSTA]
		public void TestNewSettingsControl()
		{
			if (Method.HasSettings)
			{
				using (IComponent control = Method.NewSettingsControl())
				{
					AssertNotNull(string.Format("{0} claimed to have settings but NewSettingsControl returned null", Method.GetType().Name), control);
				}
			}
			else
			{
				AssertExceptionThrown("NewSettingControl should throw a NotSupportedException if HasSettings returns false", typeof(NotSupportedException), delegate
				{
					using (Method.NewSettingsControl())
					{
					}
				});
			}
		}

		public void TestNewSetting()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			if (Method.HasSettings)
			{
				OperationalActionMethodSettings setting = Method.NewSetting(factory);
				AssertNotNull("NewSetting should not return null", setting);
				if (setting.Factory != null)
				{
					AssertEquals("if the setting has a factory, it must be the passed in factory", factory, setting.Factory);
				}
			}
			else
			{
				AssertExceptionThrown("NewSetting should throw a NotSupportedException if HasSettings returns false", typeof(NotSupportedException), delegate
				{
					Method.NewSetting(factory);
				});
			}
		}

		public void TestSettingBinding()
		{
			if (Method.HasSettings)
			{
				using (ZForm form = new ZForm(Factory.New<DummyBusinessObject>()))
				{
					form.ClientSize = new Size(170, 170);
					SettingsGuiHost control = new SettingsGuiHost(Method);
					control.Dock = DockStyle.Fill;
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();
				}
			}

			Assert(true);
		}

		public void TestSecurityCheckpoints()
		{
			SecurityCheckpoint[] list = Method.GetRequiredSecurityCheckpoints();
			if (list == null || list.Length == 0)
			{
				Assert(true);
			}
			else
			{
				foreach (SecurityCheckpoint checkpoint in list)
				{
					AssertNotNull("The array returned by GetRequiredCheckpoints must not contain nulls", checkpoint);
				}
			}
		}

		public void TestLicenceCheckpoints()
		{
			LicenceCheckpoint[] list = Method.GetRequiredLicenceCheckpoints();
			if (list == null || list.Length == 0)
			{
				Assert(true);
			}
			else
			{
				foreach (LicenceCheckpoint checkpoint in list)
				{
					AssertNotNull("The array returned by GetRequiredLicences must not contain nulls", checkpoint);
				}
			}
		}

		#region Implementation

		public MethodT Method
		{
			get
			{
				return method ?? (method = NewMethod());
			}
		}

		MethodT method;
		static OperationalActionMethodApplicator NewApplicator(OperationalActionMethod method, BusinessObjectFactory factory)
		{
			if (method.HasSettings)
			{
				OperationalActionMethodSettings settings = method.NewSetting(factory);
				return method.NewApplicator(factory, settings);
			}
			else
			{
				return method.NewApplicator(factory, null);
			}
		}

		protected abstract MethodT NewMethod();
		class SettingsGuiHost : ZUserControl
		{
			public SettingsGuiHost(MethodT method)
			{
				this.method = method;
			}

			protected override void OnCurrentDataItemChanging(EventArgs e)
			{
				RemoveAllChildren();
				base.OnCurrentDataItemChanging(e);
			}

			protected override void OnCurrentDataItemChanged(EventArgs e)
			{
				base.OnCurrentDataItemChanged(e);
				BusinessObject bo = CurrentDataItem as BusinessObject;
				if (bo != null && method.HasSettings)
				{
					OperationalActionMethodSettings settings = method.NewSetting(bo.Factory);
					Control control = (Control)method.NewSettingsControl();
					control.Dock = DockStyle.Fill;
					ZBindingSource bindingSource = new ZBindingSource();
					bindingSource.SetBindingMember(control, ".");
					bindingSource.SetDataBinding(settings, "");
					Controls.Add(control);
				}
			}

			void RemoveAllChildren()
			{
				Control[] children = new Control[Controls.Count];
				Controls.CopyTo(children, 0);
				foreach (Control child in children)
				{
					child.Dispose();
				}
			}

			readonly MethodT method;
		}

		class ApplicatorGuiHost : ZUserControl
		{
			public ApplicatorGuiHost(MethodT method)
			{
				this.method = method;
			}

			protected override void OnCurrentDataItemChanging(EventArgs e)
			{
				RemoveAllChildren();
				base.OnCurrentDataItemChanging(e);
			}

			protected override void OnCurrentDataItemChanged(EventArgs e)
			{
				base.OnCurrentDataItemChanged(e);
				BusinessObject bo = CurrentDataItem as BusinessObject;
				if (bo != null && method.HasControl)
				{
					OperationalActionMethodSettings settings;
					if (method.HasSettings)
					{
						settings = method.NewSetting(bo.Factory);
					}
					else
					{
						settings = null;
					}

					OperationalActionMethodApplicator applicator = method.NewApplicator(bo.Factory, settings);
					Control control = (Control)method.NewGuiControl();
					control.Dock = DockStyle.Fill;
					ZBindingSource bindingSource = new ZBindingSource();
					bindingSource.SetBindingMember(control, ".");
					bindingSource.SetDataBinding(applicator, "");
					Controls.Add(control);
				}
			}

			void RemoveAllChildren()
			{
				Control[] children = new Control[Controls.Count];
				Controls.CopyTo(children, 0);
				foreach (Control child in children)
				{
					child.Dispose();
				}
			}

			readonly MethodT method;
		}

		#endregion
	}
}
