
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow
{
	public class ThreadSafeTextBox : ZTextBox
	{
		public ThreadSafeTextBox()
		{
		}

		public new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				SafeSetText(value);
			}
		}

		protected delegate void AppendTextDelegate(string value);
		public new void AppendText(string value)
		{
			if (InvokeRequired)
			{
				Invoke(new AppendTextDelegate(this.AppendText), new object[] { value });
			}
			else
			{
				base.AppendText(value);
			}
		}

		#region Implementation

		protected delegate void SafeSetTextDelegate(string value);
		protected void SafeSetText(string value)
		{
			if (InvokeRequired)
			{
				Invoke(new SafeSetTextDelegate(this.SafeSetText), new object[] { value });
			}
			else
			{
				base.Text = value;
			}
		}

		#endregion
	}
}
