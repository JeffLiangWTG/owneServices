using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	public partial class ZTranslatableTextControl : ZUserControl, IExtendedControl, IDataBoundControl, IGridControl
	{
		public ZTranslatableTextControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);

			zLanguageText.Click += new EventHandler(zLanguageText_Click);

			zDropButton.AllowOverlap(zLanguageText);
		}

		[SmartTagVisible]
		[DefaultValue(CharacterCasing.Upper)]
		public CharacterCasing CharacterCasing
		{
			get { return zTextBox.CharacterCasing; }
			set { zTextBox.CharacterCasing = value; }
		}

		public override string Text
		{
			get { return zTextBox.Text; }
			set { zTextBox.Text = value; }
		}

		public bool ReadOnly
		{
			get { return zTextBox.ReadOnly; }
			set { zTextBox.ReadOnly = value; }
		}

		[Browsable(true)]
		public bool IsMultiLine
		{
			get { return zTextBox.Multiline; }
			set { zTextBox.Multiline = value; }
		}

		public bool IsLanguageEditingEnabled
		{
			get { return zDropButton.Enabled; }
			set
			{
				zDropButton.Enabled = value;
				zLanguageText.Enabled = value;
			}
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			return IsTabbingThroughGridColumn(m) || base.ProcessKeyPreview(ref m);
		}

		protected bool IsTabbingThroughGridColumn(Message m)
		{
			return IsOnGrid && (Keys)(int)m.WParam == Keys.Tab;
		}

		void zDropButton_Click(object sender, EventArgs e)
		{
			OnLanguageClick();
		}

		void zLanguageText_Click(object sender, EventArgs e)
		{
			OnLanguageClick();
		}

		public void OnLanguageClick()
		{
			ObjectFactory.Get<ICustomizableDataTranslationEditor>().EditTranslations(this);
		}

		string IDataBoundControl.DataMember { get { return ((IDataBoundControl)zTextBox).DataMember; } }

		object IDataBoundControl.DataSource { get { return ((IDataBoundControl)zTextBox).DataSource; } }

		Type IDataBoundControl.DataSourceType { get { return ((IDataBoundControl)zTextBox).DataSourceType; } }

		void IDataBoundControl.SetDataBinding(object dataSource, string dataMember)
		{
			if (!IsOnGrid)
			{
				zTextBox.SetDataBinding(dataSource, dataMember);
			}
		}

		Control IExtendedControl.Host { get { return this; } }

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		#region IGridControl

		int IGridControl.SelectionStart
		{
			get { return zTextBox.SelectionStart; }
			set { zTextBox.SelectionStart = value; }
		}

		int IGridControl.SelectionLength
		{
			get { return zTextBox.SelectionStart; }
			set { zTextBox.SelectionLength = value; }
		}

		string IGridControl.Text
		{
			get { return zTextBox.Text; }
			set { zTextBox.Text = value; }
		}

		event EventHandler IGridControl.TextChanged
		{
			add { zTextBox.TextChanged += value; }
			remove { zTextBox.TextChanged -= value; }
		}

		int IGridControl.ButtonWidth
		{
			get { return 0; }
		}

		int IGridControl.MaxLength
		{
			get { return zTextBox.MaxLength; }
			set { zTextBox.MaxLength = value; }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { zTextBox.KeyDown += value; }
			remove { zTextBox.KeyDown -= value; }
		}

		void IGridControl.ActivateEditControl()
		{
			zTextBox.Focus();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return ((IGridControl)zTextBox).ShouldHandleKey(keyData);
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return zTextBox.ReadOnly; }
		}

		#endregion

		public bool AcceptsReturn
		{
			get { return zTextBox.AcceptsReturn; }
			set { zTextBox.AcceptsReturn = value; }
		}

		public override bool IsOnGrid
		{
			get { return base.IsOnGrid; }
			set
			{
				zTextBox.BorderStyle = BorderStyle.None;
				zTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2);
				ControlDpiScalingHelper.SetHeight(ref zDropButton, 14, true);
				ControlDpiScalingHelper.SetHeight(ref zLanguageText, 14, true);
				base.IsOnGrid = value;
			}
		}

		public object GridCurrent
		{
			get;
			set;
		}

		public string GridMember
		{
			get;
			set;
		}
	}
}

