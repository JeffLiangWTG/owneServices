using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	[ToolboxItem(false)]
	public class ReadOnlyCacheExtension : ControlExtension
	{
		public ReadOnlyCacheExtension(string[] namesOfControlsToIgnore)
		{
			this.namesOfControlsToIgnore = namesOfControlsToIgnore;
		}

		readonly string[] namesOfControlsToIgnore;

		public string[] GetNamesOfControlsToIgnore()
		{
			return namesOfControlsToIgnore != null ? (string[])namesOfControlsToIgnore.Clone() : System.Array.Empty<string>();
		}

		public void Backup()
		{
			var control = Owner?.Host;

			if (control != null && !control.IsDisposed)
			{
				BackupReadOnly();
			}
		}

		public void Restore()
		{
			var control = Owner?.Host;

			if (control != null && !control.IsDisposed)
			{
				RestoreReadOnly();
			}
		}

		void BackupReadOnly()
		{
			if (!IsCached)
			{
				var control = Owner.Host;

				OriginalValue = control.GetReadOnly();

				var readOnlyProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(control.GetType(), MetaDataTypes.ReadOnly);
				if (readOnlyProperty != null)
				{
					OriginalBinding = control.DataBindings[readOnlyProperty.Name];
					control.DataBindings.RemoveBinding(readOnlyProperty.Name);
				}
			}

			IsCached = true;
		}

		void RestoreReadOnly()
		{
			if (IsCached)
			{
				var control = Owner.Host;

				if (!OriginalValue)
				{
					control.SetReadOnly(OriginalValue);
				}

				if (OriginalBinding != null)
				{
					var existingBinding = control.DataBindings[OriginalBinding.PropertyName];
					if (existingBinding == null)
					{
						control.DataBindings.Add(OriginalBinding);
					}
				}
			}

			IsCached = false;
		}

		bool IsCached { get; set; }

		bool OriginalValue { get; set; }

		Binding OriginalBinding { get; set; }
	}
}
