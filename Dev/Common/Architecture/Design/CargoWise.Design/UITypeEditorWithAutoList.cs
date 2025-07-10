using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common.Collections;
using CargoWise.Windows.UI;

namespace CargoWise.Design
{
	/// <summary>
	/// An editor for showing an auto-complete list while typing a new value.
	/// </summary>
	public abstract class UITypeEditorWithAutoList : UITypeEditor
	{
		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			this.context = context;
			property = context.PropertyDescriptor;
			propertyGridView = GetPropertyGridView(context);
			propertyGrid = GetPropertyGrid(propertyGridView);

			propertyGrid.SelectedGridItemChanged -= new SelectedGridItemChangedEventHandler(OnSelectedGridItemChanged);
			propertyGrid.SelectedGridItemChanged += new SelectedGridItemChangedEventHandler(OnSelectedGridItemChanged);
			return false;
		}

		protected abstract IAutoListSource NewAutoListSource(ITypeDescriptorContext context);

		protected AutoListManager AutoListManager
		{
			get
			{
				TextBox textBox = TextBox;
				if (autoListManagers == null)
				{
					autoListManagers = new WeakReferencedKeyDictionary<TextBox, AutoListManager>();
				}

				if (currentAutoListManager != null)
				{
					currentAutoListManager.TextBox = null;
					currentAutoListManager.AutoListSource = null;
				}

				currentAutoListManager = autoListManagers[textBox];
				if (currentAutoListManager == null || currentAutoListManager.IsDisposed)
				{
					currentAutoListManager = new AutoListManager();
					autoListManagers[textBox] = currentAutoListManager;
				}
				currentAutoListManager.TextBox = textBox;
				currentAutoListManager.AutoListSource = AutoListSource;
				return currentAutoListManager;
			}
		}
		AutoListManager currentAutoListManager;

		[ThreadStatic]
		internal static WeakReferencedKeyDictionary<TextBox, AutoListManager> autoListManagers;

		#region Implementation

		ITypeDescriptorContext context;
		PropertyDescriptor property;
		PropertyGrid propertyGrid;
		Control propertyGridView;

		IAutoListSource AutoListSource
		{
			get
			{
				if (autoListSource == null || autoListSource.Context != context)
				{
					autoListSource = NewAutoListSource(context);
				}
				return autoListSource;
			}
		}
		IAutoListSource autoListSource;

		protected TextBox TextBox
		{
			get
			{
				PropertyInfo editProperty = propertyGridView.GetType().GetProperty("Edit", BindingFlags.NonPublic | BindingFlags.Instance);
				return (TextBox)editProperty.GetValue(propertyGridView, Array.Empty<object>());
			}
		}

		static Control GetPropertyGridView(ITypeDescriptorContext context)
		{
			PropertyInfo hostProperty = context.GetType().BaseType.GetProperty("GridEntryHost", BindingFlags.NonPublic | BindingFlags.Instance);
			return (Control)hostProperty.GetValue(context, Array.Empty<object>());
		}

		static PropertyGrid GetPropertyGrid(Control propertyGridView)
		{
			PropertyGrid result = null;
			Control currentControl = propertyGridView;
			while (currentControl != null && result == null)
			{
				result = currentControl as PropertyGrid;
				currentControl = currentControl.Parent;
			}
			return result;
		}

		void OnSelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
		{
			if ((e.OldSelection == null || e.OldSelection.PropertyDescriptor != property) &&
				(e.NewSelection != null && e.NewSelection.PropertyDescriptor == property))
			{
				// refresh AutoListManager.AutoListSource and AutoListManager.TextBox
				currentAutoListManager = null;
				currentAutoListManager = AutoListManager;
			}
			else if (currentAutoListManager != null)
			{
				currentAutoListManager.TextBox = null;
			}
		}

		#endregion

		#region TearDown

		public void TearDown()
		{
			context = null;
			propertyGrid = null;
			propertyGridView = null;
			currentAutoListManager.Dispose();
			currentAutoListManager = null;
			autoListSource = null;
		}

		#endregion
	}
}
