using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.ZArchitecture.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	[ToolboxItem(true)]
	public partial class ZTextBoxWithDetailsOnNote : ZStmNotePopupBase
	{
		#region Custom Adornment Layout

		class ZTextBoxWithDetailsOnNoteAdornmentLayout : AdornmentLayout<ZTextBoxWithDetailsOnNote>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZTextBoxWithDetailsOnNote source)
			{
				yield return source.textBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZTextBoxWithDetailsOnNote source)
			{
				yield return new IconLayout(source.popupButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Constructors

		static ZTextBoxWithDetailsOnNote() => NotificationAdornmentFactory.RegisterCustomLayout(new ZTextBoxWithDetailsOnNoteAdornmentLayout());

		public ZTextBoxWithDetailsOnNote()
		{
			InitializeComponent();
			TextBox.SetResourceStringIdentifyingControl(this);
		}

		#endregion

		#region Button Text

		[Category(ZGUIConstants.DesignerCategory)]
		public new string ButtonText
		{
			get => base.ButtonText;
			set => base.ButtonText = value;
		}

		protected bool ShouldSerializeButtonText() => !string.IsNullOrEmpty(ButtonText) && !buttonTextDefaulted;
		bool buttonTextDefaulted;

		#endregion

		#region Providing PopupButton text when RS diabled

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Created && Visible && string.IsNullOrEmpty(popupButton.Text))
			{
				popupButton.Text = Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("3a116000-5224-4b99-8a87-2c4fcf7ff354", "Detail");
				buttonTextDefaulted = true;
			}
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			DataBoundControl.Get(textBox).SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		#endregion

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region InnerTextBox

		class InnerTextBox : ZTextBox.Bare
		{
			public InnerTextBox(ZTextBoxWithDetailsOnNote parent) => this.parent = parent ?? throw new ArgumentNullException(nameof(parent));

			#region Overrides

			protected override void OnKeyPress(KeyPressEventArgs e)
			{
				var maxLengthWasExceeded = SelectionLength == 0 && Text.Length == MaxLength;

				base.OnKeyPress(e);

				if (!char.IsControl(e.KeyChar) && maxLengthWasExceeded)
				{
					MaxLengthExceeded(e.KeyChar);
				}
			}

			#endregion

			#region Implementation

			public override bool ShouldAggressivelyTruncateText
			{
				get
				{
					return true;
				}
			}

			void MaxLengthExceeded(char key)
			{
				if (!parent.NoteExists)
				{
					var builder = new StringBuilder(Text);
					_ = builder.Remove(SelectionStart, SelectionLength);
					_ = builder.Insert(SelectionStart, key);

					parent.CreateNewNoteIfNotExists();
					parent.NoteText = builder.ToString();
					parent.ShowEditorAndSetCaretLocation(SelectionStart + 1);
				}
			}

			readonly ZTextBoxWithDetailsOnNote parent;

			#endregion
		}

		protected ZTextBox TextBox => textBox;

		#endregion
	}
}
