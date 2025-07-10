using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public enum UpdateTextOnCheckedMode
	{
		Never,
		AlwaysClear,
		ClearWhenPlaceholderTextIsTruncated
	}

	public class CheckedTextControlOverridingMediator : NonPersistentBusinessObject
	{
		public CheckedTextControlOverridingMediator(IOverridableTextBox control)
			: base()
		{
			this.control = control;
			BindToControl();
		}

		internal IOverridableTextBox control;

		void BindToControl()
		{
			if (control != null)
			{
				control.TextChanged += delegate
				{
					if (ControlText != control.Text)
					{
						ControlText = control.Text;
					}
				};

				control.CheckedChanged += delegate
				{
					if (ControlIsChecked != control.Checked)
					{
						ControlIsChecked = control.Checked;
					}
				};

				control.MaxLengthChanged += delegate
				{
					if (MaxLength != control.MaxLength)
					{
						MaxLength = control.MaxLength;
					}
				};

				control.TextOverrideChanged += delegate
				{
					if (TextOverride != control.TextOverride)
					{
						TextOverride = control.TextOverride;
					}
				};

				control.PlaceholderTextChanged += delegate
				{
					if (PlaceholderText != control.PlaceholderText)
					{
						PlaceholderText = control.PlaceholderText;
					}
				};

				control.TextIsOverriddenChanged += delegate
				{
					if (TextIsOverridden != control.TextIsOverridden)
					{
						TextIsOverridden = control.TextIsOverridden;
					}
				};

				ControlTextInfo.ValueChanged += delegate
				{
					if (control.Text != ControlText)
					{
						control.Text = ControlText;
					}
				};

				ControlIsCheckedInfo.ValueChanged += delegate
				{
					if (control.Checked != ControlIsChecked)
					{
						control.Checked = ControlIsChecked;
					}
				};

				MaxLengthInfo.ValueChanged += delegate
				{
					if (control.MaxLength != MaxLength)
					{
						control.MaxLength = MaxLength;
					}
				};

				TextOverrideInfo.ValueChanged += delegate
				{
					control.TextOverride = TextOverride;
				};

				PlaceholderTextInfo.ValueChanged += delegate
				{
					control.PlaceholderText = PlaceholderText;
				};

				TextIsOverriddenInfo.ValueChanged += delegate
				{
					control.TextIsOverridden = TextIsOverridden;
				};
			}
		}

		#region UpdateTextOnChecked

		public UpdateTextOnCheckedMode UpdateTextOnChecked = UpdateTextOnCheckedMode.ClearWhenPlaceholderTextIsTruncated;

		#endregion

		#region Properties Bound to Business Object - Business Layer

		#region MaxLength

		public ZInt MaxLength
		{
			get
			{
				return maxLength;
			}

			set
			{
				maxLength = value;
				MaxLengthInfo.RefreshBinding();

				if (MaxLength != 0 && TextOverride.Length > MaxLength)
				{
					TextOverride = TextOverride.Substring(0, MaxLength);
				}

				RecalculateControlTextByBusinessAndRefreshProperties();
			}
		}
		ZInt maxLength = 0;

		public ZPropertyInfo MaxLengthInfo
		{
			get { return GetZPropertyInfo(nameof(MaxLength)); }
		}

		#endregion

		#region TextOverride

		public ZString TextOverride
		{
			get
			{
				return textOverride;
			}
			set
			{
				textOverride = MaxLength == 0 || value.Length <= MaxLength ? value : value.SubstringSafe(0, MaxLength);

				TextOverrideInfo.RefreshBinding();
				RecalculateControlTextByBusinessAndRefreshProperties();
			}
		}
		ZString textOverride = ZString.Empty;

		void RecalculateTextOverrideByControlTextAndRefreshProperties()
		{
			textOverride = ControlText;
			TextOverrideInfo.RefreshBinding();
		}

		void RecalculateTextOverrideByControlIsCheckedAndRefreshProperties()
		{
			if (ControlIsChecked)
			{
				textOverride = ControlText;
			}
			else
			{
				textOverride = ZString.Empty;
			}
			TextOverrideInfo.RefreshBinding();
		}

		public ZPropertyInfo TextOverrideInfo
		{
			get { return GetZPropertyInfo(nameof(TextOverride)); }
		}

		#endregion

		#region PlaceholderText

		public ZString PlaceholderText
		{
			get
			{
				return placeholderText;
			}
			set
			{
				placeholderText = value;
				PlaceholderTextInfo.RefreshBinding();
				if (!TextIsOverridden)
				{
					RecalculateControlTextByBusinessAndRefreshProperties();
				}
			}
		}
		ZString placeholderText = ZString.Empty;

		public ZPropertyInfo PlaceholderTextInfo
		{
			get { return GetZPropertyInfo(nameof(PlaceholderText)); }
		}

		#endregion

		#region TextIsOverridden

		public ZBool TextIsOverridden
		{
			get
			{
				return textIsOverridden;
			}

			set
			{
				textIsOverridden = value;
				TextIsOverriddenInfo.RefreshBinding();

				RecalculateControlIsCheckedByBusinessAndRefreshProperties();
				RecalculateControlTextByBusinessAndRefreshProperties();
			}
		}
		ZBool textIsOverridden = false;

		void RecalculateTextIsOverriddenByControlIsCheckedAndRefreshProperties()
		{
			textIsOverridden = ControlIsChecked;
			TextIsOverriddenInfo.RefreshBinding();
		}

		public ZPropertyInfo TextIsOverriddenInfo
		{
			get { return GetZPropertyInfo(nameof(TextIsOverridden)); }
		}

		#endregion

		#region Properties Bound to Control - UI Layer

		#region ControlIsChecked

		public ZBool ControlIsChecked
		{
			get
			{
				return controlIsChecked;
			}
			set
			{
				controlIsChecked = value;
				RecalculateControlTextByControlIsCheckedAndRefreshProperties();
				RecalculateTextIsOverriddenByControlIsCheckedAndRefreshProperties();
				RecalculateTextOverrideByControlIsCheckedAndRefreshProperties();
				ControlIsCheckedInfo.RefreshBinding();
			}
		}
		ZBool controlIsChecked = false;

		void RecalculateControlIsCheckedByBusinessAndRefreshProperties()
		{
			controlIsChecked = TextIsOverridden;
			ControlIsCheckedInfo.RefreshBinding();
		}

		public ZPropertyInfo ControlIsCheckedInfo
		{
			get { return GetZPropertyInfo(nameof(ControlIsChecked)); }
		}

		#endregion

		#region ControlText

		public ZString ControlText
		{
			get
			{
				return controlText;
			}
			set
			{
				controlText = value;
				if (ControlIsChecked)
				{
					RecalculateTextOverrideByControlTextAndRefreshProperties();
				}
				ControlTextInfo.RefreshBinding();
			}
		}
		ZString controlText = ZString.Empty;

		void RecalculateControlTextByBusinessAndRefreshProperties()
		{
			controlText = TextIsOverridden ? TextOverride : TruncatedPlaceholderText;
			ControlTextInfo.RefreshBinding();
		}

		void RecalculateControlTextByControlIsCheckedAndRefreshProperties()
		{
			if (ControlIsChecked)
			{
				if (UpdateTextOnChecked != UpdateTextOnCheckedMode.Never)
				{
					controlText = ShouldClearTextOnOChecked ? ZString.Empty : PlaceholderText;
					ControlTextInfo.RefreshBinding();
				}
			}
			else
			{
				controlText = TruncatedPlaceholderText;
				ControlTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ControlTextInfo
		{
			get { return GetZPropertyInfo(nameof(ControlText)); }
		}

		#endregion

		#endregion

		#region Implementation

		internal bool ShouldClearTextOnOChecked
		{
			get
			{
				return
					UpdateTextOnChecked == UpdateTextOnCheckedMode.AlwaysClear ||
					UpdateTextOnChecked == UpdateTextOnCheckedMode.ClearWhenPlaceholderTextIsTruncated && MaxLength != 0 && PlaceholderText.Length > MaxLength;
			}
		}

		internal ZString TruncatedPlaceholderText
		{
			get
			{
				var result = PlaceholderText;
				if (MaxLength != 0 && PlaceholderText.Length > MaxLength)
				{
					result = MaxLength > 3 ? PlaceholderText.SubstringSafe(0, MaxLength - 3) + "..." : string.Empty;  // Elipsis is something international
				}
				return result;
			}
		}

		#endregion

		#endregion
	}
}
