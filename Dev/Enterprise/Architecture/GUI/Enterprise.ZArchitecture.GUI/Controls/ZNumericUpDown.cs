using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI.Internal;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[DefaultBindingProperty("Text")]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZNumericUpDown : KNumericUpDown, IDataBoundControl, IExtendedControl, IResCaptionedControl, IBindTo
	{
		Container components;

		public ZNumericUpDown()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			InitializeComponent();
			Extensions = NewExtensionCollection();
		}

		[DefaultValue(false)]
		public bool ReverseUpDownButtons
		{
			get { return fReverseUpDownButtons; }
			set { fReverseUpDownButtons = value; }
		}

		/// <summary>
		/// Use this property when you want this control to remain active on a readonly form
		/// </summary>
		[DefaultValue(false)]
		public bool IsNeverReadOnly
		{
			get { return isNeverReadOnly; }
			set
			{
				ReadOnly = false;
				isNeverReadOnly = value;
			}
		}
		bool isNeverReadOnly;

		[DefaultValue(false)]
		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				if (!IsNeverReadOnly)
				{
					for (var i = 0; i < Controls.Count; i++)
					{
						Controls[i].SetReadOnly(value);
					}
					base.ReadOnly = value;
				}
			}
		}

		public override void UpButton()
		{
			if (ReverseUpDownButtons)
			{
				base.DownButton();
			}
			else
			{
				base.UpButton();
			}

			OnValidated(EventArgs.Empty);
			OnValidating(new CancelEventArgs());
		}

		public override void DownButton()
		{
			if (ReverseUpDownButtons)
			{
				base.UpButton();
			}
			else
			{
				base.DownButton();
			}

			OnValidated(EventArgs.Empty);
			OnValidating(new CancelEventArgs());
		}

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new Container();
		}
		#endregion

		#region Parse/Format

#if DEBUG
		internal object GetTextBinding_ParseResultForTesting(string input, Type desiredType)
		{
			var args = new ConvertEventArgs(input, desiredType);
			TextBinding_Parse(null, args);
			return args.Value;
		}
#endif

		void TextBinding_Parse(object sender, ConvertEventArgs e)
		{
			var value = StripInvalidCharacters(e.Value.ToString());
			var desiredType = e.DesiredType;
			if (desiredType == typeof(ZByte))
			{
				var result = ZByte.Zero;
				ZByte.TryParse(value, out result);
				if (result <= base.Minimum)
				{
					result = (ZByte)base.Minimum;
				}

				if (result >= base.Maximum)
				{
					result = (ZByte)base.Maximum;
				}

				e.Value = result;
			}
			else if (desiredType == typeof(ZDecimal))
			{
				var result = ZDecimal.Zero;
				ZDecimal.TryParse(value, out result);
				if (result <= base.Minimum)
				{
					result = (ZDecimal)base.Minimum;
				}

				if (result >= base.Maximum)
				{
					result = (ZDecimal)base.Maximum;
				}

				e.Value = result;
			}
			else if (desiredType == typeof(ZShort))
			{
				var result = ZShort.Zero;
				ZShort.TryParse(value, out result);
				if (result <= base.Minimum)
				{
					result = (ZShort)base.Minimum;
				}

				if (result >= base.Maximum)
				{
					result = (ZShort)base.Maximum;
				}

				e.Value = result;
			}
			else
			{
				var result = ZInt.Zero;
				ZInt.TryParse(value, out result);
				if (result <= base.Minimum)
				{
					result = (ZInt)base.Minimum;
				}

				if (result >= base.Maximum)
				{
					result = (ZInt)base.Maximum;
				}

				e.Value = result;
			}
		}

		static string StripInvalidCharacters(string sourceString)
		{
			var result = new List<char>();
			var alreadyHasPeriod = false;
			var numbers = new List<char>(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' });
			foreach (var character in sourceString.ToCharArray())
			{
				if (numbers.Contains(character))
				{
					result.Add(character);
				}
				else if (character == '-')
				{
					if (result.Count == 0)
					{
						result.Add(character);
					}
				}
				else if (character == '.')
				{
					if (!alreadyHasPeriod)
					{
						result.Add(character);
						alreadyHasPeriod = true;
					}
				}
			}
			//return result.ToString();
			return new string(result.ToArray());
		}

		void TextBinding_Format(object sender, ConvertEventArgs e)
		{
			e.Value = e.Value.ToString();
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZNumericUpDown>()
				.Property("Text", "") // Property name
				.Property("Value", 0m) // Property name
				.Property("ReadOnly", false, false)
				.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Result;
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new DefaultControlExtensionCollection(this);
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region IDataBoundControl Members

		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				var textBinding = DataBindings[nameof(Text)];
				textBinding.Parse -= new ConvertEventHandler(TextBinding_Parse);
				textBinding.Parse += new ConvertEventHandler(TextBinding_Parse);
				textBinding.Format -= new ConvertEventHandler(TextBinding_Format);
				textBinding.Format += new ConvertEventHandler(TextBinding_Format);
			}
		}

		object IDataBoundControl.DataSource
		{
			get { return DataBoundControl.GetDefaultImplementation(this).DataSource; }
		}

		string IDataBoundControl.DataMember
		{
			get { return DataBoundControl.GetDefaultImplementation(this).DataMember; }
		}

		Type IDataBoundControl.DataSourceType
		{
			get { return typeof(decimal); }
		}

		#endregion

		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion

		[Browsable(true)]
		public ResourceStringData CaptionResourceString
		{
			get { return captionResourceString ?? ResourceStringData.Empty; }
			set
			{
#if DEBUG
				if (DesignMode && value == null)
				{
					return;
				}
#endif
				captionResourceString = value;
				this.RefreshCaptionLabel();
			}
		}
		ResourceStringData captionResourceString;

		bool ShouldSerializeCaptionResourceString()
		{
			return !CaptionResourceString.IsEmpty();
		}

		bool fReverseUpDownButtons;
	}
}
