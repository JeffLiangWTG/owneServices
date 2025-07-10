using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[DefaultBindingProperty("Text")]
	public partial class AWBCustomTextControl : ZUserControl
	{
		public AWBCustomTextControl()
		{
			InitializeComponent();
			TextBox.Leave += delegate { textBoxSelectionStart = TextBox.SelectionStart; };
		}

		public bool Multiline
		{
			get { return TextBox.Multiline; }
			set
			{
				TextBox.Multiline = value;
				if (value)
				{
					TextBox.ScrollBars = ScrollBars.Vertical;
					TextBox.AcceptsReturn = true;
				}
				else
				{
					TextBox.ScrollBars = ScrollBars.None;
					TextBox.AcceptsReturn = false;
				}
			}
		}

		int textBoxSelectionStart;

		public override string Text
		{
			get { return TextBox.Text; }
			set { TextBox.Text = value; }
		}

		public bool ReadOnly
		{
			get { return TextBox.ReadOnly; }
			set
			{
				TextBox.ReadOnly = value;
				ViewButton.Enabled = !ReadOnly;
			}
		}

		#region AWB Map

		void ViewButton_Click(object sender, System.EventArgs e)
		{
			using (var mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>())
			{
				mapTreePresenter.ParentTypes = new Type[] { ObjectFactory.GetType<DocumentWrappers.IDocAWB>() };
				var result = mapTreePresenter.GetUserSelectionMacro();
				if (result != null)
				{
					TextBox.SelectionStart = textBoxSelectionStart;
					TextBox.SelectedText = result;
				}
			}
		}

		#endregion
	}
}
