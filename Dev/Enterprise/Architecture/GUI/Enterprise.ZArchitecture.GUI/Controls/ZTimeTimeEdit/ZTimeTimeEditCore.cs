using System;
using System.Windows.Forms;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	#region Time Edit Helper

	public class ZTimeTimeEditHelper
	{
		internal string InputText = "";

		protected internal string GetTextFromTime(ZTime time, bool allowNegative)
		{
			return time.ToString();
		}

		internal virtual int MaximumHours
		{
			get { return 23; }
		}
	}

	#endregion

	public class ZTimeTimeEditCore : IDisposable
	{
		internal ZTimeTimeEditCore(ZTimeTimeEdit editor)
		{
			this.Editor = editor;
			this.Editor.KeyDown += new KeyEventHandler(Editor_KeyDown);
			this.Editor.KeyPress += new KeyPressEventHandler(Editor_KeyPress);
		}

		protected readonly ZTimeTimeEdit Editor;

		public virtual bool AllowNegative { get; set; }

		#region Parse / Format

		public string EmptyText
		{
			get { return Editor.IsOnGrid ? "    :" : "     :"; }
		}

		internal string InputText
		{
			get { return Helper.InputText; }
			set { Helper.InputText = value; }
		}

		#region Parse

		ZTimeParser Parser
		{
			get { return new ZTimeParser(Helper.MaximumHours); }
		}

		protected virtual internal ZTime GetTimeFromText(string value)
		{
			return Parser.GetTimeFromText(value, false);
		}

		#endregion

		#region Format

		protected virtual internal string GetTextFromTime(ZTime time, bool allowNegativeOverride)
		{
			return time.ToTimeString();
		}

		#endregion

		#region Helper

		protected ZTimeTimeEditHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = GetNewHelper();
				}
				return fHelper;
			}
		}

		protected virtual ZTimeTimeEditHelper GetNewHelper()
		{
			return new ZTimeTimeEditHelper();
		}

		ZTimeTimeEditHelper fHelper;

		#endregion

		#endregion

		#region Key Down / Press

		protected virtual void Editor_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!Char.IsControl(e.KeyChar))
			{
				var allowedCharacters = "1234567890:";
				if (AllowNegative)
				{
					allowedCharacters += "-";
				}
				if (allowedCharacters.IndexOf(e.KeyChar) == -1)
				{
					e.Handled = true;
				}
				else if (char.IsDigit(e.KeyChar) && Editor.NonSelectedText.Length == Editor.MaxLength - 1 && !Editor.NonSelectedText.Contains(":"))
				{
					e.Handled = true; // cannot type 5 numbers
				}
				else if (e.KeyChar == ':' && Editor.NonSelectedText.Contains(":"))
				{
					e.Handled = true; // cannot type 2 ":"
				}
				else if (Editor.MaxLength == Editor.Text.Length && Editor.SelectionLength == 0) // cannot overflow the text box
				{
					e.Handled = true;
				}
			}
		}

		protected virtual void Editor_KeyDown(object sender, KeyEventArgs e)
		{
			if (!Editor.ReadOnly)
			{
				switch (e.KeyData)
				{
					case Keys.Alt | Keys.Up:
						IncrementHours();
						e.Handled = true;
						break;
					case Keys.Alt | Keys.Down:
						DecrementHours();
						e.Handled = true;
						break;
					case Keys.Control | Keys.Up:
						IncrementMins();
						e.Handled = true;
						break;
					case Keys.Control | Keys.Down:
						DecrementMins();
						e.Handled = true;
						break;

					case Keys.F2:

						if (Editor.IsOnGrid)
						{
							Editor.SelectionLength = 0;
							Editor.SelectionStart = Editor.Text.Length;
						}
						break;
				}
			}
		}

		#region +/- Hours

		protected void IncrementHours()
		{
			AddHours(1);
		}

		protected void DecrementHours()
		{
			AddHours(-1);
		}

		void AddHours(int amount)
		{
			var time = GetTimeFromText(Editor.Text);

			if (time.IsValid)
			{
				Editor.Text = GetTextFromTime(time.AddHours(amount), AllowNegative);
			}
		}

		#endregion

		#region +/- Minutes

		protected void IncrementMins()
		{
			AddMinutes(15);
		}

		protected void DecrementMins()
		{
			AddMinutes(-15);
		}

		protected void AddMinutes(int amount)
		{
			var time = GetTimeFromText(Editor.Text);

			if (time.IsValid)
			{
				time = time.AddMinutes(amount);
				Editor.Text = GetTextFromTime(time, AllowNegative);
			}
		}

		#endregion

		#endregion

		#region Dispose

		public void Dispose()
		{
			if (Editor != null)
			{
				Editor.KeyDown -= new KeyEventHandler(Editor_KeyDown);
				Editor.KeyPress -= new KeyPressEventHandler(Editor_KeyPress);
			}
		}

		#endregion
	}
}
