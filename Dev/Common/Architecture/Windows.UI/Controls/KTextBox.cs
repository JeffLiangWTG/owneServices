using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI.Controls;
using CargoWise.Windows.UI.Interop;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KTextBox"/></summary>
	[DesignTimeControlNameGenerator("txt")]
	[DefaultBindingProperty("Text")]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KTextBox : TextBox, IDisposeStackProvider
	{
		int textSettingCount;
		string cachedText;

		IDisposable SetText()
		{
			textSettingCount++;
			return new DisposableAction(() => textSettingCount--);
		}

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		[BindingMetaDataProperty(MetaDataTypes.DecimalPlaces, "DecimalPlaces")]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		[BindingOptions(UseTypeConverters = true)]
		public override string Text
		{
			get
			{
				if (ReadOnly && EnableReadOnlyCheckForTextValue && textSettingCount == 0 && (cachedText == null || cachedText != base.Text))
				{
					using (SetText())
					{
						foreach (Binding b in DataBindings)
						{
							b.ReadValue(); // Read business layer values
						}
						cachedText = base.Text;
					}
				}

				return base.Text;
			}
			set
			{
				using (SetText())
				using (DecimalPlacesHandler.NotifyInTextSetter())
				{
					base.Text = value;
					cachedText = null; // Not sure if this part is needed
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#if !WINZOR
		public virtual bool EnableReadOnlyCheckForTextValue => true;
#else
		public virtual bool EnableReadOnlyCheckForTextValue => false;
#endif

		[SmartTagVisible]
		public override int MaxLength
		{
			get { return base.MaxLength; }
			set { base.MaxLength = value == -1 ? short.MaxValue : value; }
		}

		#region Drag and Drop
#if !WINZOR

		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			if (drgevent.Data.GetDataPresent(DataFormats.UnicodeText) || drgevent.Data.GetDataPresent(DataFormats.Text))
			{
				drgevent.Effect = DragDropEffects.Copy;
				Focus();
			}
			else
			{
				base.OnDragEnter(drgevent);
			}
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			base.OnDragOver(drgevent);
			if (drgevent.AllowedEffect != DragDropEffects.None)
			{
				this.Select(GetDragDropCharIndexPosition(drgevent), 0);
			}
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			string textToDrop = null;
			if (drgevent.Data.GetDataPresent(DataFormats.UnicodeText))
			{
				textToDrop = (string)drgevent.Data.GetData(DataFormats.UnicodeText);
			}
			else if (drgevent.Data.GetDataPresent(DataFormats.Text))
			{
				textToDrop = (string)drgevent.Data.GetData(DataFormats.Text);
			}

			if (textToDrop != null)
			{
				int selectionStart = GetDragDropCharIndexPosition(drgevent);
				this.Focus();
				this.Select(selectionStart, 0);
				this.SelectedText = textToDrop;
				this.Select(selectionStart, textToDrop.Length);
			}
			else
			{
				base.OnDragDrop(drgevent);
			}
		}

		int GetDragDropCharIndexPosition(DragEventArgs drgevent)
		{
			Point clientPoint = this.PointToClient(ControlDpiScalingHelper.NewScaledPoint(drgevent.X, drgevent.Y, false));
			int charPosition = this.GetCharIndexFromPosition(clientPoint);
			if (charPosition == this.Text.Length - 1)
			{
				Point lastCharPosition = this.GetPositionFromCharIndex(this.Text.Length - 1);
				if (clientPoint.X > lastCharPosition.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(5))
				{
					charPosition++;
				}
			}
			return charPosition;
		}

#endif
		#endregion

		#region Binding ReadOnly

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBinding
		{
			get { return ReadOnlyForBindingProperty.ReadOnlyForBinding; }
			set { ReadOnlyForBindingProperty.ReadOnlyForBinding = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		public event EventHandler ReadOnlyForBindingChanged
		{
			add { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged += value; }
			remove { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBindingIsNull
		{
			get { return false; }
			set
			{
				if (value)
				{
					ReadOnlyForBinding = true;
				}
			}
		}

		protected override void OnReadOnlyChanged(EventArgs e)
		{
			base.OnReadOnlyChanged(e);
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
		}

		protected virtual ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						() => ReadOnly,
						value => ReadOnly = value);
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion

		#region DecimalPlaces

		/// <summary>
		/// Get or set the number of DecimalPlaces to use. User input will be restricted to
		/// the decimal places.
		/// </summary>
		[DefaultValue(-1)]
		public virtual int DecimalPlaces
		{
			get { return DecimalPlacesHandler.DecimalPlaces; }
			set { DecimalPlacesHandler.DecimalPlaces = value; }
		}

		TextBoxDecimalPlacesHandler DecimalPlacesHandler
		{ get { return decimalPlacesHandler ?? (decimalPlacesHandler = new TextBoxDecimalPlacesHandler(this)); } }
		TextBoxDecimalPlacesHandler decimalPlacesHandler;

		#endregion

		#region PlaceHolder

		[DefaultValue("")]
		public string PlaceHolderText
		{
			get
			{
#if !WINZOR
#if NET8_0_OR_GREATER
				nint nMaxCount = 4099;
#else // .NET Framework
				var nMaxCount = (IntPtr)4099;
#endif
				StringBuilder buffer = new StringBuilder(4096);
				UnsafeNativeMethods.GetCueBanner(Handle, WindowMessages.EM_GETCUEBANNER, buffer, nMaxCount);
				return buffer.ToString();
#else
				return InternalPlaceholderText;
#endif
			}
			set
			{
#if !WINZOR
				UnsafeNativeMethods.SetCueBanner(Handle, WindowMessages.EM_SETCUEBANNER, UIntPtr.Zero, value);
#else
				InternalPlaceholderText = value;
#endif
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (TrackDisposedAccess)
			{
				disposeStack = new StackTrace();
				disposeControlPath = ControlDescription.GetControlPath(this);
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Handle

		protected override void CreateHandle()
		{
			try
			{
				base.CreateHandle();
			}
			catch (ObjectDisposedException ex)
			{
				throw new ObjectDisposedException(this.BuildDisposeInformation(), ex);
			}
		}

		#endregion

		#region Disposed Access Tracking

		[DefaultValue(false), Browsable(false)]
		public bool TrackDisposedAccess { get; set; }

		public StackTrace DisposeStack => disposeStack;
		StackTrace disposeStack;

		public string DisposeControlPath => disposeControlPath;
		string disposeControlPath;

		#endregion
	}
}
