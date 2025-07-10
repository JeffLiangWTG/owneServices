using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.Forms
{
	public interface IInputBox
	{
		string ReturnValue
		{
			get;
#if DEBUG
			set;
#endif
		}
	}

	partial class InputBoxForm : KForm, IInputBox
	{
		public InputBoxForm()
		{
			InitializeComponent();
			fReturnValue = null;
		}

		public string Title
		{
			set { this.Text = value; }
		}

		public string Prompt
		{
			set { MessageText.Text = value; }
		}

		public string ReturnValue
		{
			get { return fReturnValue; }
#if DEBUG
			set { fReturnValue = value; }
#endif
		}

		public string DefaultResponse
		{
			set
			{
				ResultTextEdit.Text = value;
				ResultTextEdit.SelectAll();
			}
		}

		public Point StartLocation
		{
			set { fStartLocation = value; }
		}

		#region Implementation

		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public bool AllowBlankString;

		void InputBoxForm_Load(object sender, System.EventArgs e)
		{
			if (!fStartLocation.IsEmpty)
			{
				ControlDpiScalingHelper.SetTop(this, fStartLocation.X, false);
				ControlDpiScalingHelper.SetLeft(this, fStartLocation.Y, false);
			}
		}

		void btnOK_Click(object sender, System.EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ResultTextEdit.Text))
			{
				if (AllowBlankString)
				{
					fReturnValue = null;
					Close();
				}
				else
				{
					Globals.Message.Show(Res.GetString("f596d6c7-b7ab-4eaa-9ba4-6ec056c80cfc", "Please enter a non-blank text!"));
					return;
				}
			}
			else
			{
				fReturnValue = ResultTextEdit.Text;
				Close();
			}
		}

		void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion
	}
}
