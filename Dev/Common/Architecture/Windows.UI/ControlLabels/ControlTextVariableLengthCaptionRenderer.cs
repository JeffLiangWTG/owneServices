using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Renders a variable length caption on a control that has a Text property.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
	public class ControlTextVariableLengthCaptionRenderer : IVariableLengthCaptionRenderer, IDisposable
	{
		public ControlTextVariableLengthCaptionRenderer(Control control)
		{
			this.Control = control;
			control.TextChanged += new EventHandler(Control_TextChanged);
			control.SizeChanged += new EventHandler(Control_SizeChanged);
			control.Disposed += new EventHandler(Control_Disposed);
		}

		/// <summary>
		/// Get whether the control text is being set by this component.
		/// </summary>
		public bool IsSettingText { get; private set; }

		/// <summary>
		/// Get the best fit string to render on the control's Text property.
		/// </summary>
		protected virtual string MeasureBestFitCaption()
		{
			string[] captionsRef = Captions;
			return (captionsRef == null || captionsRef.Length == 0) ? "" : captionsRef[0];
		}

		protected Control Control { get; private set; }

		#region IVariableLengthCaptionRenderer Members

		public string[] Captions
		{
			get { return captions; }
			set
			{
				if (!ArrayUtil.ArrayEquals(captions, value) || (value != null && value.Length > 0 && string.IsNullOrEmpty(Control.Text)))
				{
					captions = value;
					UpdateText();
				}
			}
		}
		string[] captions;

		/// <summary>
		/// Get whether the Text property on the control has been set explicitly.
		/// You should return this value from the ShouldSerializeText() method of the control.
		/// </summary>
		public bool IsCaptionOverridden { get; set; }

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (disposed)
			{
				return;
			}

			try
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			finally
			{
				disposed = true;
			}
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				Control.SizeChanged -= Control_SizeChanged;
				Control.TextChanged -= Control_TextChanged;
				Control.Disposed -= Control_Disposed;
			}
		}

		bool disposed;

		#endregion

		#region Implementation

		void Control_TextChanged(object sender, EventArgs e)
		{
			if (!IsSettingText)
			{
				IsCaptionOverridden = true;
			}
		}

		void Control_SizeChanged(object sender, EventArgs e)
		{
			if (Control.IsHandleCreated)
			{
				UpdateText();
			}
		}

		void Control_Disposed(object sender, EventArgs e)
		{
			Dispose();
		}

		void UpdateText()
		{
			if (!IsSettingText && !IsCaptionOverridden)
			{
				IsSettingText = true;
				try
				{
					Control.Text = MeasureBestFitCaption();
				}
				finally
				{
					IsSettingText = false;
				}
			}
		}

		#endregion
	}
}
