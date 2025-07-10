using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionListEditControlForRegistry : CodeDescriptionListEditControl, ICustomHasChangesControl
	{
		/// <summary>
		/// Empty constructor for VS controls designer.
		/// </summary>
		public CodeDescriptionListEditControlForRegistry()
		{
		}

		public CodeDescriptionListEditControlForRegistry(IRegistryItem registryItem, bool showCodeColumn, bool showDescriptionColumn,
			CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, string codeColumnCaption, string descriptionColumnCaption, int codeColumnMaxLength)
			: base(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption, descriptionColumnCaption)
		{
			if (showCodeColumn)
			{
				ZTextBoxColumnStyle column = (ZTextBoxColumnStyle)CodeDescriptionGrid.Columns[CodeColumnName].ColumnStyle;
				column.TextBox.MaxLength = codeColumnMaxLength;
			}

			this.registryItem = registryItem;
			CodeDescriptionGrid.ColumnLayoutContext = registryItem.Name;
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			SetDataBinding(null, "");
			base.Dispose(isNotFinalizing);
		}

		readonly IRegistryItem registryItem;

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			var keyProcessed = base.ProcessCmdKey(ref msg, keyData);

			if (!keyProcessed &&
				keyData != (Keys.ShiftKey | Keys.Shift) &&
				keyData != (Keys.ControlKey | Keys.Control) &&
				keyData != (Keys.Menu | Keys.Alt) &&
				keyData != (Keys.Shift | Keys.Tab) &&
				keyData != Keys.Tab &&
				keyData != Keys.LWin &&
				keyData != Keys.RWin &&
				keyData != Keys.Up &&
				keyData != Keys.Down &&
				keyData != Keys.Left &&
				keyData != Keys.Right)
			{
				HasChangesChanged?.Invoke(this, HasChangesChangedEventArgs.Create(true, this));
			}

			return keyProcessed;
		}

		public event EventHandler<HasChangesChangedEventArgs> HasChangesChanged;

		#region Test
#if DEBUG
		internal ZGrid CodeDescriptionGridForTest
		{
			get { return base.CodeDescriptionGrid; }
		}

		internal IRegistryItem RegistryItemForTest
		{
			get { return registryItem; }
		}

#endif
		#endregion
	}
}
