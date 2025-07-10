using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
using Enterprise.ZArchitecture.Modules;
#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	public interface ZDropEditInternals
	{
		void SetControlWidth(int width);
	}

	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(Designer))]
	[DefaultBindingProperty("Text")] // the attributes on the Text are used to express the meta-data bindings
	public partial class ZDropEdit : ZListUserControl, IExtendedControl, IIsOnGrid, ZDropEditInternals, IHotkeyProvider, IDropFormParent, IShowEditOrViewForm
	{
		#region Bare

		[ToolboxItem(false)]
		public class Bare : ZDropEdit
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Custom Adornment Layout

		class ZDropEditAdornmentLayout : AdornmentLayout<ZDropEdit>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZDropEdit source)
			{
				yield return source.DescriptionBox;
				yield return source.CodeBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZDropEdit source)
			{
				yield return new IconLayout(source.DropButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Designer Support

		internal sealed class Designer : GenericControlDesignerWithTextBoxSnapLine<ZDropEdit>
		{
			protected override TextBox GetTextBox(ZDropEdit userControl)
			{
				return userControl.DescriptionBox;
			}
		}

		#endregion

		#region Constructors

		static ZDropEdit()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZDropEditAdornmentLayout());
		}

		public ZDropEdit()
		{
			InitializeComponent();
			CodeBox.BackColorChanged += delegate
			{ DropButton.Invalidate(false); };
			CodeBox.TextChanged += delegate
			{ OnTextChanged(EventArgs.Empty); };
			Extensions = NewExtensionCollection();
			codeBoxTranslationFeedbackManager = new DropEditTranslationFeedbackManager(CodeBox);
			descriptionBoxTranslationFeedbackManager = new DropEditTranslationFeedbackManager(DescriptionBox);
#if WINZOR
			OnHandleCreated(EventArgs.Empty);
			CodeBox.Click += (_, _) => { DropButton.HideDropDown(); };
			DescriptionBox.Click += (_, _) => { DropButton.HideDropDown(); };
			DropButton.ZIndex = 1;
			ReadOnlyChanged += (_, _) => { NotifyRenderRequired(); };
#endif

			CodeBox.AllowOverlap(DropButton);
			DropButton.AllowOutsideOfParent();
		}

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new DefaultControlExtensionCollection(this);
		}

		#endregion

		#region DropButton / DescriptionBox

		protected internal ZDropButton DropButton { get; private set; }
		public ZTextBox DescriptionBox { get; private set; }

		public void ShowDropDown()
		{
			if (!IsDroppedDown)
			{
				DropButton.ShowDropDown(false);
			}
		}

		#endregion

		#region DropDownClosed / SelectedIndexChanged

		public event EventHandler DropDownClosed;
		public event EventHandler SelectedIndexChanged;
		public event EventHandler BoundValueCommitted;

		#endregion

		#region Properties
#if WINZOR
		//This constant is to reduce the width of the DropEdit by 2 pixels to size it closer to the WinForms counterpart, without affecting other controls
		//(the primary constant for this is WidthAdjustment in TextRenderer.cs, which is used by many other controls also and thus cannot be changed just for the DropEdit)
		const int DropEditWidthAdjustmentForWinzor = 2;
#endif

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(0)]
		public int PreBoundMaxLength
		{
			get { return preBoundMaxLength; }
			set
			{
				preBoundMaxLength = value;
				SetControlSize(value);
			}
		}

		[DefaultValue(CharacterCasing.Upper)]
		public CharacterCasing CharacterCasing
		{
			get { return CodeBox.CharacterCasing; }
			set { CodeBox.CharacterCasing = value; }
		}

		[DefaultValue('\0')]
		public char PasswordChar
		{
			get { return CodeBox.PasswordChar; }
			set { CodeBox.PasswordChar = value; }
		}

		/// <summary>
		/// The index into the bound list which represents the selected item, or -1 if invalid.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectedIndex
		{
			get { return selectedIndex; }
		}

		public enum ShowInDropDownList
		{
			ShowCodeAndDescription,
			OnlyShowCode,
			OnlyShowDescription
		}

		[DefaultValue(ShowInDropDownList.ShowCodeAndDescription)]
		public ShowInDropDownList ShowInDropDown
		{
			get { return showInDropDown; }
			set { showInDropDown = value; }
		}
		ShowInDropDownList showInDropDown = ShowInDropDownList.ShowCodeAndDescription;

		[DefaultValue(false)]
		public bool ShowColorInDropDown { get; set; }

		[DefaultValue(true)]
		public bool ShowDescriptionInDropDown
		{
			get { return ShowInDropDown != ShowInDropDownList.OnlyShowCode; }
		}

		[DefaultValue(true)]
		public bool ShowDescriptionBox
		{
			get { return showDescriptionBox; }
			set
			{
				if (ShowDescriptionBox != value)
				{
					showDescriptionBox = value;
					SetDescriptionBoxVisibility();
				}
			}
		}
		bool showDescriptionBox = true;

		[DefaultValue(true)]
		public bool ShowCodeInDropDown
		{
			get { return ShowInDropDown != ShowInDropDownList.OnlyShowDescription; }
		}

		[DefaultValue(false)]
		public bool ShowHorizontalScrollBar
		{
			get
			{
				return this.DropButton.ShowHorizontalScrollBar;
			}
			set
			{
				this.DropButton.ShowHorizontalScrollBar = value;
			}
		}

		[DefaultValue(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SupportsEmptyCode { get; set; }

		public MultilingualString GetMultilingualValue(ICodeDescription item)
		{
			return ShowCodeInDropDown ? item.GetMultilingualCode() : item.GetMultilingualDescription();
		}

		[DefaultValue(false)]
		public bool UseFullWidthForCodeBox
		{
			get { return useFullWidthForCodeBox; }
			set
			{
				if (useFullWidthForCodeBox != value)
				{
					useFullWidthForCodeBox = value;
					SetDescriptionBoxVisibility();
				}
			}
		}
		bool useFullWidthForCodeBox;

		protected int fMaxItemsToShowInDropDown = 10;

		/// <summary>
		/// The maximum number of items to show when Dropped Down, more will result in a scroll bar.
		/// </summary>
		[DefaultValue(10)]
		[Description("The maximum number of items to show when Dropped Down, more will result in a scroll bar.")]
		public int MaxItemsToShowInDropDown
		{
			get { return fMaxItemsToShowInDropDown; }
			set { fMaxItemsToShowInDropDown = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		public override string Text
		{
			get { return CodeBox.Text; }
			set { CodeBox.Text = value; }
		}

		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set
			{
				base.ForeColor = value;
				CodeBox.ForeColor = ForeColor;
				DescriptionBox.ForeColor = ForeColor;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDroppedDown
		{
			get { return DropButton.IsDroppedDown; }
		}

		[DefaultValue(true)]
		public bool EnableTimeRecording { get; set; } = true;

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get { return CodeBox.ReadOnly; }
			set
			{
				CodeBox.ReadOnly = value;

				if (designTimeTabStop)
				{
					TabStop = !value;
				}

				if (CodeBox.ReadOnly && IsDroppedDown)
				{
					DropButton.HideDropDown();
				}
				DropButton.Invalidate();

				OnReadOnlyChanged(EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBinding
		{
			get { return ReadOnlyForBindingCore; }
			set { ReadOnlyForBindingCore = value; }
		}

		bool ReadOnlyForBindingCore // non-obsolete version for private use
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

		void OnReadOnlyChanged(EventArgs e)
		{
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
			if (ReadOnlyChanged != null)
			{
				ReadOnlyChanged(this, e);
			}
		}

		protected virtual ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						delegate
						{ return ReadOnly; },
						(bool value) => { ReadOnly = value; }
					);
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion

		#region MaxLength

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MaxLength
		{
			get { return CodeBox.MaxLength; }
			set { CodeBox.MaxLength = value; }
		}

		#endregion

		#region IReadOnlyControl

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyIsNull
		{
			get { return false; }
			set
			{
				if (value)
				{
					ReadOnly = true;
				}
			}
		}

		public event EventHandler ReadOnlyChanged;

		#endregion

		#region CommitBoundValue

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable reason")]
		public void OnItemSelected(ICodeDescription selectedItem, bool commitValue)
		{
			if (selectedItem != null)
			{
				if (TranslationFeedbackManager.InTranslationFeedbackMode())
				{
					if (List is UntranslatableCodeDescriptionPairList)
					{
						Globals.Message.Show("Translation of this list is not supported: " + ((UntranslatableCodeDescriptionPairList)List).UntranslatableReason);
					}
					else
					{
						if (!ShowDescriptionBox && !ShowDescriptionInDropDown)
						{
							TranslationFeedbackManager.OpenFeedbackForm(this, selectedItem.GetMultilingualCode());
						}
						else
						{
							TranslationFeedbackManager.OpenFeedbackForm(this, selectedItem);
						}
					}
				}
				else
				{
					SetSelection(selectedItem, 0);
				}
			}

			if (commitValue)
			{
				CommitBoundValue();
			}
		}

		/// <summary>
		/// Send the value from the control into the bound Business Object.
		/// </summary>
		public virtual void CommitBoundValue()
		{
			var binding = CodeBox.DataBindings[nameof(Text)];
			if (binding != null)
			{
				binding.WriteValue();
				binding.ReadValue();
				OnBoundValueCommitted();
			}
		}

		#endregion

		#region Code Box

		public virtual ZDropCodeBox CodeBox
		{
			get
			{
				if (fCodeBox == null)
				{
					fCodeBox = NewCodeBox();
					LabelCaptionRenderProvider.SetLabelCaptionVisible(fCodeBox, false);
				}
				return fCodeBox;
			}
		}

		protected virtual ZDropCodeBox NewCodeBox()
		{
			return new ZDropCodeBox.Bare();
		}

		ZDropCodeBox fCodeBox;

		#endregion

		#region Synchronise Child Controls Appearance

		public ICodeDescription LastSelectedItem
		{
			get
			{
				if (lastSelectedItem != null)
				{
					if (ListNoPullIfNull == null ||
						ListNoPullIfNull is IBusinessObjectCollection collection && !collection.OfType<ICodeDescription>().Contains(lastSelectedItem) ||
						!ListNoPullIfNull.Contains(lastSelectedItem))
					{
						lastSelectedItem = null;
					}
				}
				return lastSelectedItem;
			}
			private set
			{
				var hasChanged = lastSelectedItem != value;
				lastSelectedItem = value;
				if (hasChanged)
				{
					OnLastSelectedItemChanged(this, EventArgs.Empty);
				}
			}
		}
		ICodeDescription lastSelectedItem;

		public event EventHandler LastSelectedItemChanged;

		protected virtual void OnLastSelectedItemChanged(object sender, EventArgs e)
		{
			if (LastSelectedItemChanged != null)
			{
				LastSelectedItemChanged(sender, e);
			}
		}

		internal void SetSelection(ICodeDescription selectedItem, int selectionStart)
		{
			LastSelectedItem = selectedItem;

			CodeBox.Text = GetMultilingualValue(selectedItem);
#if WINZOR
			CodeBox.Select(selectionStart + CodeBox.Text.Length, 0);
			CodeBox.ScrollToCaret();
#endif
			CodeBox.Select(selectionStart, CodeBox.Text.Length);

			if (DescriptionBox.Visible)
			{
				DescriptionBox.Text = selectedItem.Description;
			}
		}

		internal void SetEmptyDescription()
		{
			DescriptionBox.Text = "";
		}

		internal protected void UpdateDropDown()
		{
			DropButton.UpdateSelectedItem();
		}

		public override string TypeNameForDisplay => Res.GetString("c4aa2b23-adb2-4909-82f2-4420d8ae8558", "Drop Edit");
		public override HotkeyRegister Hotkeys => DropButton.Hotkeys;
		protected override bool ProcessHotkeys { get; }

		internal void HandleCommandKey(KeyEventArgs e)
		{
			DropButton.HandleCommandKey(e);
		}

		[DefaultValue(true)]
		public bool ShouldResizeByMaxLength { get; set; } = true;

		protected internal virtual void SetControlSize(int maxLength)
		{
			var newCodeBoxWidth = TextBoxControlSize.GetControlWidth(CodeBox, maxLength);
#if WINZOR
			SetCodeBoxWidth(newCodeBoxWidth - DropEditWidthAdjustmentForWinzor);
#else
			SetCodeBoxWidth(newCodeBoxWidth);
#endif
		}

		public void SetControlWidth(int width)
		{
			SetCodeBoxWidth(width - GetButtonWidth());
		}

		void ZDropEditInternals.SetControlWidth(int width)
		{
			this.SetControlWidth(width);
		}

		void SetCodeBoxWidth(int width)
		{
			SuspendLayout();
			try
			{
				if (width > 0)
				{
					ControlDpiScalingHelper.SetWidth(CodeBox, width, false);
				}
				ControlDpiScalingHelper.SetWidth(DropButton, CodeBox.Width + GetButtonWidth(), false);
				ControlDpiScalingHelper.SetLeft(DescriptionBox, DropButton.Right, false);
				ControlDpiScalingHelper.SetWidth(DescriptionBox, Width - DescriptionBox.Left, false);
			}
			finally
			{
				ResumeLayout();
			}
		}

		protected virtual int GetButtonWidth()
		{
			return ButtonWidth;
		}

		public static int ButtonWidth
		{
			get
			{
				// This will return the width required to render the DropButton (button + included text box)
				// The addition is split into two, being the first, the padding of the CodeBox inside the control, and the second an additional space left so the button doesn't overlap with the CodeBox
				// They're split to account for rounding errors when rendering on high DPI monitors
				return (DesignModeFinder.IsDesigning ? ControlDpiScalingHelper.ScaleToCurrentDpiX(16) :
					ZGUISystemInformation.VerticalScrollBarWidth) + ControlDpiScalingHelper.ScaleToCurrentDpiX(3) + ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
			}
		}

		void SetDescriptionBoxVisibility()
		{
			SuspendLayout();
			try
			{
				if (ShowDescriptionBox)
				{
					if (CodeBox.MaxLength > 0 && CodeBox.MaxLength < short.MaxValue || !UseFullWidthForCodeBox)
					{
						ControlDpiScalingHelper.SetWidth(this, this.Width + oldDescriptionBoxWidth, false);
						ControlDpiScalingHelper.SetWidth(DescriptionBox, oldDescriptionBoxWidth, false);
					}
					else
					{
						SetCodeBoxWidth(CodeBox.Width - oldDescriptionBoxWidth);
					}
					DescriptionBox.Visible = true;
				}
				else
				{
					oldDescriptionBoxWidth = DescriptionBox.Width;
					if (CodeBox.MaxLength > 0 && CodeBox.MaxLength < short.MaxValue || !UseFullWidthForCodeBox)
					{
						ControlDpiScalingHelper.SetWidth(this, this.Width - DescriptionBox.Width, false);
					}
					else
					{
						SetCodeBoxWidth(CodeBox.Width + DescriptionBox.Width);
					}
					DescriptionBox.Visible = false;
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			SynchroniseControlSizes();
			base.OnLayout(e);
			Invalidate(true);
		}

		protected virtual void SynchroniseControlSizes()
		{
			// Since the size of the DescriptionBox depends on the font inside of it, adapt the rest of the controls to its size
			if (Height != DescriptionBox.Height)
			{
				ControlDpiScalingHelper.SetHeight(this, DescriptionBox.Height, false);
			}

			if (DropButton.Height != DescriptionBox.Height)
			{
				ControlDpiScalingHelper.SetHeight(DropButton, DescriptionBox.Height, false);
			}

			// Set the CodeBox height to the DropButton height - offset
			if (CodeBox.Height != DropButton.Height - (CodeBox.Bounds.Y * 2))
			{
				ControlDpiScalingHelper.SetHeight(CodeBox, DropButton.Height - (CodeBox.Bounds.Y * 2), false);
			}

			if (UseFullWidthForCodeBox)
			{
				SetControlWidth(Width);
			}
			else if (DropButton != null && Width != 0)
			{
				if (DropButton.Width > Width)
				{
					ControlDpiScalingHelper.SetWidth(this, DropButton.Width, false);
				}

				if (!ShowDescriptionBox && Width > DropButton.Width)
				{
					ControlDpiScalingHelper.SetWidth(this, DropButton.Width, false);
				}
			}
		}

		#endregion

		#region Designer Generated

		protected virtual ZDropButton NewDropButton()
		{
			return new ZDropButton();
		}

		void InitializeComponent()
		{
			this.DropButton = NewDropButton();
			this.DescriptionBox = new ZTextBox.Bare();
			this.SuspendLayout();
			// 
			// CodeBox
			//
			this.CodeBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.CodeBox.Name = "CodeBox";
			this.CodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.CodeBox.TabIndex = 1;
			this.CodeBox.Text = "";
			this.CodeBox.AutoSize = false;
			// 
			// DropButton
			// 
			this.DropButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DropButton.Name = "DropButton";
			this.DropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DropButton.TabIndex = 0;
			// 
			// DescriptionBox
			// 
			this.DescriptionBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left;
			this.DescriptionBox.BackColor = System.Drawing.SystemColors.Control;
			this.DescriptionBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			this.DescriptionBox.Name = "DescriptionBox";
			this.DescriptionBox.ReadOnly = true;
			this.DescriptionBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.DescriptionBox.TabIndex = 2;
			this.DescriptionBox.TabStop = false;
			this.DescriptionBox.Text = "";
			// 
			// ZDropEdit
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DescriptionBox);
			this.Controls.Add(this.CodeBox);
			this.Controls.Add(this.DropButton);
			this.Name = "ZDropEdit";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ResumeLayout(false);
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region BindToForDescription

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToForDescription
		{
			get { return bindToForDescription; }
			set { bindToForDescription = value; }
		}
		string bindToForDescription = "";

		#endregion

		protected override void OnHandleDestroyed(EventArgs e)
		{
			var binding = CodeBox.DataBindings[nameof(Text)];

			if (binding != null)
			{
				binding.Format -= OnFormatValue;
				binding.Parse -= OnParseValue;
			}

			base.OnHandleDestroyed(e);
		}

		#region Format & Parse

		void OnFormatValue(object sender, ConvertEventArgs e)
		{
			OnFormatValue(e);

			using (PreventErrorReportForMissingListAttribute())
			{
				if (Res.CurrentLanguage != Res.DefaultLanguage && (e.Value is string && !string.IsNullOrEmpty((string)e.Value) || e.Value is ZString && !((ZString)e.Value).IsEmpty) && List != null)
				{
					var translatableItem = List.OfType<CodeDescriptionPair>().FirstOrDefault(x => x.Code.Equals(e.Value.ToString(), StringComparison.OrdinalIgnoreCase));
					if (translatableItem != null && !string.IsNullOrEmpty(translatableItem.MultilingualCode))
					{
						if (e.DesiredType == typeof(ZString))
						{
							e.Value = new ZString(translatableItem.MultilingualCode);
						}
						else
						{
							e.Value = translatableItem.MultilingualCode.ToString();
						}
					}
				}
			}
		}

		void OnParseValue(object sender, ConvertEventArgs e)
		{
			using (PreventErrorReportForMissingListAttribute())
			{
				if (Res.CurrentLanguage != Res.DefaultLanguage && (e.Value is string && !string.IsNullOrEmpty((string)e.Value) || e.Value is ZString && !((ZString)e.Value).IsEmpty) && List != null)
				{
					var translatableItem = List.OfType<CodeDescriptionPair>().FirstOrDefault(x => x.MultilingualCode.ToString().Equals(e.Value.ToString(), StringComparison.OrdinalIgnoreCase));
					if (translatableItem != null)
					{
						if (e.DesiredType == typeof(ZString))
						{
							e.Value = new ZString(translatableItem.Code);
						}
						else
						{
							e.Value = translatableItem.Code;
						}
					}
				}
			}

			OnParseValue(e);
		}

		protected virtual void OnFormatValue(ConvertEventArgs e)
		{
			InvalidateList();
		}

		protected virtual void OnParseValue(ConvertEventArgs e)
		{
		}

		void OnBindingComplete(object sender, BindingCompleteEventArgs e)
		{
			if (e.BindingCompleteContext == BindingCompleteContext.DataSourceUpdate)
			{
				e.Binding.ReadValue();
			}
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		protected override CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			result.AddRange(base.GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember));
			if (!string.IsNullOrEmpty(BindToForDescription))
			{
				result.Add(new CompileTimeCheckBindingMember(dataSourceType, typeof(string), BindingHelper.GetNestedControlDataMember(BindTo, dataMember, BindToForDescription)));
			}
			return result;
		}

		#endregion

		#region IDataBoundControl Members

		bool designTimeTabStop;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			LastSelectedItem = null;

			CodeBox.DataBindings.RemoveBinding(nameof(Text)); // Property name
			CodeBox.DataBindings.RemoveBinding(nameof(MaxLength));
			DataBindings.RemoveBinding(nameof(ReadOnly));
			DescriptionBox?.DataBindings.RemoveBinding(nameof(Text)); // Property name

			base.SetDataBinding(dataSource, dataMember);
			OnSetDataBinding(dataSource, dataMember);
		}

		void OnSetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null && !this.IsDesignMode())
			{
				AddCodeBinding();

				if (!string.IsNullOrEmpty(BindToForDescription))
				{
					DescriptionBox.DataBindings.RemoveBinding(nameof(Text)); // Property name
					DescriptionBox.DataBindings.Add(new KBinding(nameof(Text), dataSource, BindingHelper.GetNestedControlDataMember(BindTo, dataMember, BindToForDescription), true, DataSourceUpdateMode.Never));
				}

				UpdateSelection();
				Extensions.SetDataBinding(dataSource, dataMember);
			}
		}

		protected virtual void UpdateSelection()
		{
		}

		protected virtual BindingManagerBase BindingManager
		{
			get { return DataSource == null ? null : BindingContext[DataSource, new KBindingMemberInfo(DataMember).BindingPath]; }
		}

		#endregion

		#region AddCodeBinding

		void AddCodeBinding()
		{
			if (codeBinder != null)
			{
				codeBinder.Dispose();
			}

			codeBinder = null;
			CodeBox.DataBindings.RemoveBinding(nameof(Text)); // Property name
			if (DataSource != null)
			{
				var bindToBinding = new KBinding(nameof(Text), DataSource, DataMember, true);
				bindToBinding.Format += new ConvertEventHandler(OnFormatValue);
				bindToBinding.Parse += new ConvertEventHandler(OnParseValue);
				bindToBinding.BindingComplete += new BindingCompleteEventHandler(OnBindingComplete);
				using (PreventErrorReportForMissingListAttribute())
				{
					CodeBox.DataBindings.Add(bindToBinding);
				}
			}
#if DEBUG
			CodeBindingAdded = true;
#endif
		}
		IDisposable codeBinder;

#if DEBUG
		public bool CodeBindingAdded;
#endif

		#endregion

		#region IIsOnGrid Members

		bool IIsOnGrid.IsOnGrid
		{
			get { return onGrid; }
			set
			{
				onGrid = value;
				foreach (Control childControl in Controls)
				{
					var childControlOnGrid = childControl as IIsOnGrid;
					if (childControlOnGrid != null)
					{
						childControlOnGrid.IsOnGrid = value;
					}
				}
			}
		}

		Font IDropFormParent.Font => Font;
		string IDropFormParent.Code => CodeBox.Text;
		int IDropFormParent.MinDropDownWidth => DropButton.Width;

		bool onGrid;

		#endregion

		#region IShowEditOrViewForm Members

		[SmartTagVisible]
		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(ModuleIDEditor), typeof(UITypeEditor))]
		public ModuleIdentifier ModuleID
		{
			get
			{
				return GetListRelatedModuleID();
			}
		}

		public string SearchCode { get => CodeBox.Text; }

		[DefaultValue(true)]
		public bool ShowNewFormWhenEmpty { get => true; }

		[DefaultValue(true)]
		public bool AllowNewForm { get => true; }

		[DefaultValue(true)]
		public bool AllowEditForm { get => true; }

		[DefaultValue(false)]
		public bool EnableShowEditOrViewForm { get; set; }

		object IShowEditOrViewForm.DataSource => DataSource;

		public IEnumerable<BusinessObject> GetBizObjsToEditOrView()
		{
			return GetBizObjsToEditOrViewCore();
		}

		public ShowingEditOrViewFormEventArgs InitialiseForm(ZFilterModule module)
		{
			module.SetFormsModalTo(FindForm());

			var showingEditOrViewFormEventArgs = new ShowingEditOrViewFormEventArgs(module, module.AllowEdit && AllowEditForm, module.AllowView);
			OnShowingEditOrViewForm(showingEditOrViewFormEventArgs);
			return showingEditOrViewFormEventArgs;
		}

		public void SetCodePropertyFromText(IZForm form)
		{
		}

		public DialogResult ShowDefaultMessageWhenCreatingANewBizObjFromFindBox(ZFilterModule module)
		{
			return DialogResult.Yes;
		}

		public bool ShowEditOrViewForm()
		{
			if (EnableShowEditOrViewForm)
			{
				var showForm = new ShowSelectedInEditOrViewForm(this);
				return showForm.ShowEditOrViewForm();
			}
			return false;
		}

		public void ShowMoreThanOneSelectedMessageAndPopup(bool isEdit)
		{
		}

		protected virtual IEnumerable<BusinessObject> GetBizObjsToEditOrViewCore()
		{
			var factory = new BusinessObjectFactory();
			var collection = (IBusinessObjectCollection)List;
			var query = new ZQuery(GetSchemaColumnFromLookup(collection), CodeBox.Text);
			query.AddToFilter(collection.CompleteFilter);
			return factory.Load(collection.TypeOfElements, query);
		}

		protected SchemaColumn GetSchemaColumnFromLookup(IList lookupCollection)
		{
			var bizoCollection = lookupCollection as IBusinessObjectCollection;
			if (bizoCollection == null)
			{
				Globals.Message.ShowDeveloperErrorOnce("ZDropEdit Collection", $"Collection {lookupCollection.GetType().FullName} needs to implement IBusinessObjectCollection", "ZDropEdit bound lookup collection error"); // Developer exception
			}

			var type = bizoCollection.TypeOfElements;
			var schema = BusinessObjectFactory.GetTableSchemaFromType(type, false);
			if (schema == null)
			{
				Globals.Message.ShowDeveloperErrorOnce("ZDropEdit Collection", $"The business object for Collection {lookupCollection.GetType().FullName} needs a schema TableName", "ZDropEdit bound lookup collection error"); // Developer exception
			}

			var code = string.Empty;
			try
			{
				code = CodePropertyAttribute.CodePropertyNameFromType(type);
			}
			catch (NoCodePropertyException)
			{
				Globals.Message.ShowDeveloperErrorOnce("ZDropEdit Collection", $"Collection {lookupCollection.GetType().FullName} needs a CodeProperty attribute for the class", "ZDropEdit bound lookup collection error"); // Developer exception
			}

			return schema.GetSchemaColumn(code);
		}

		protected virtual ZFilterModule NewModuleFromModuleID()
		{
			var moduleID = ModuleID;
			var e = new ModuleShowingEventArgs(moduleID);
			OnModuleShowing(e);

			moduleID = e.ModuleID;
			return ZFilterModule.GetZFilterModule(moduleID);
		}

		protected void OnShowingEditOrViewForm(ShowingEditOrViewFormEventArgs args)
		{
			ShowingEditOrViewForm?.Invoke(this, args);
		}

		ModuleIdentifier GetListRelatedModuleID()
		{
			if (Equals(listRelatedModuleID, ModuleIDs.NotAssigned) && !this.IsDesignMode() && RequiresList && List != null)
			{
				listRelatedModuleID = ZMetaData.GetModuleId(List) ?? ModuleIDs.NotAssigned;
			}
			return listRelatedModuleID;
		}

		ZFilterModule IShowEditOrViewForm.NewModuleFromModuleID()
		{
			return NewModuleFromModuleID();
		}

		void OnModuleShowing(ModuleShowingEventArgs e)
		{
			ModuleShowing?.Invoke(this, e);
		}

		public event EventHandler<ModuleShowingEventArgs> ModuleShowing;
		public event EventHandler<ShowingEditOrViewFormEventArgs> ShowingEditOrViewForm;

		ModuleIdentifier listRelatedModuleID = ModuleIDs.NotAssigned;

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZDropEdit>()
				.Property("Text", "") // Property name
				.Property("ReadOnly", false)
				.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Property("IsVisibleForBinding", ZBool.True)
				.Property("ShowDescriptionBox", false, false)
				.Property("ShowDescriptionInDropDown", false, false)
				.Result;
		}

		#endregion

		#region Implementation

		Form containerForm;
		int selectedIndex = -1;
		int preBoundMaxLength;
		int oldDescriptionBoxWidth;

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (Parent != null)
			{
				designTimeTabStop = TabStop;
			}
		}

		protected override void OnEnter(EventArgs e)
		{
			try
			{
				if (!IsDisposed && IsHandleCreated)
				{
					base.OnEnter(e);

					if (containerForm == null)
					{
						if ((containerForm = FindForm()) != null)
						{
							containerForm.Deactivate += new EventHandler(ContainerForm_Deactivate);
						}
					}

					if (DataSource != null && !ReadOnly && !PreventOnEnterFromPullingList) // && Droplist was not clicked on
					{
						InvalidateList();
					}

					CodeBox.SelectAll();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ExceptionInZDropOnEnterOrOnLeave" + Parent?.Parent?.Name,
					"Exception occurred in ZDropEdit in OnEnter or OnLeave."
					, ex);
				throw;
			}
		}

		internal bool PreventOnEnterFromPullingList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "We turn it off immediately. It's basically just a way to perform an action once on a delay.")]
		void ContainerForm_Deactivate(object sender, EventArgs e)
		{
#if !WINZOR

			//if we actually DID de-activate the program...
			if (Form.ActiveForm == null || (Form.ActiveForm != FindForm() && Form.ActiveForm != DropButton.DropDown))
			{
				DropButton.HideDropDown();
			}
			else
			{
				//the ActiveForm might be about to change - check again in a few milliseconds
				var timer = new Timer();
				timer.Tick += (o, ee) =>
				{
					if (Form.ActiveForm == null || (Form.ActiveForm != FindForm() && Form.ActiveForm != DropButton.DropDown))
					{
						DropButton.HideDropDown();
					}
					timer.Stop();
					timer.Dispose();
				};
				timer.Interval = 1;
				timer.Start();
			}

#endif
		}

		protected override void OnLeave(EventArgs e)
		{
			try
			{
				if (!IsDisposed && IsHandleCreated)
				{
					PreventOnEnterFromPullingList = false;
					DropButton.HideDropDown();
					base.OnLeave(e);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ExceptionInZDropOnEnterOrOnLeave" + Parent?.Parent?.Name,
					"Exception occurred in ZDropEdit in OnEnter or OnLeave."
					, ex);
				throw;
			}
		}

		protected internal void OnSelectedIndexChanged(int newSelectedIndex)
		{
			if (newSelectedIndex != selectedIndex)
			{
				selectedIndex = newSelectedIndex;

				if (SelectedIndexChanged != null)
				{
					SelectedIndexChanged(this, EventArgs.Empty);
				}
			}
		}

		protected internal void OnBoundValueCommitted()
		{
			if (BoundValueCommitted != null)
			{
				BoundValueCommitted(this, EventArgs.Empty);
			}
		}

		IList IDropFormParent.GetFilteredListForDropDown()
		{
			return GetFilteredListForDropDown();
		}

		protected virtual IList GetFilteredListForDropDown()
		{
			return List;
		}

		void IDropFormParent.OnDropDownClosed()
		{
			OnDropDownClosed();
		}

		protected internal void OnDropDownClosed()
		{
			if (DropDownClosed != null)
			{
				DropDownClosed(this, EventArgs.Empty);
			}
		}

		public void SetDropButtonAvailability(bool value)
		{
			DropButton.Enabled = value;
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			try
			{
				if (isNotFinalizing)
				{
					if (codeBoxTranslationFeedbackManager != null)
					{
						codeBoxTranslationFeedbackManager.Dispose();
					}
					if (descriptionBoxTranslationFeedbackManager != null)
					{
						descriptionBoxTranslationFeedbackManager.Dispose();
					}
					if (Extensions != null)
					{
						Extensions.Dispose();
					}

					if (containerForm != null)
					{
						containerForm.Deactivate -= new EventHandler(ContainerForm_Deactivate);
						containerForm = null;
					}
				}
			}
			finally
			{
				base.Dispose(isNotFinalizing);
			}
		}

		#endregion

		public int ListNumberForLogging()
		{
			return ListNoPullIfNull == null ? -1 : ListNoPullIfNull.Count;
		}

		bool IDropFormParent.ShouldCloseOnMouseDown(Point mousePosition)
		{
			var dropButtonLocation = ControlDpiScalingHelper.NewScaledRectangle(PointToScreen(DropButton.Location), DropButton.Size, false);
			return !dropButtonLocation.Contains(mousePosition);
		}

		public void SelectItem(string value)
		{
			DropButton.SelectItem(value);
		}

		bool IDropFormParent.IsItemValidForAutoComplete(ICodeDescription item)
		{
			return IsItemValidForAutoCompleteCore(item);
		}

		protected virtual bool IsItemValidForAutoCompleteCore(ICodeDescription item)
		{
			return true;
		}

		readonly internal DropEditTranslationFeedbackManager codeBoxTranslationFeedbackManager;
		readonly internal DropEditTranslationFeedbackManager descriptionBoxTranslationFeedbackManager;

		internal class DropEditTranslationFeedbackManager : TranslationFeedbackManager
		{
			public DropEditTranslationFeedbackManager(ZTextBox control)
				: base(control)
			{ }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable reason")]
			protected override void OpenFeedbackForm()
			{
				var dropEdit = (ZDropEdit)control.Parent;
				if (dropEdit.SelectedIndex != -1)
				{
					if (dropEdit.List is UntranslatableCodeDescriptionPairList)
					{
						Globals.Message.Show("Translation of this list is not supported: " + ((UntranslatableCodeDescriptionPairList)dropEdit.List).UntranslatableReason);
					}
					else
					{
						if (!dropEdit.ShowDescriptionBox && !dropEdit.ShowDescriptionInDropDown)
						{
							TranslationFeedbackManager.OpenFeedbackForm(control, ((ICodeDescription)dropEdit.List[dropEdit.SelectedIndex]).GetMultilingualCode());
						}
						else
						{
							TranslationFeedbackManager.OpenFeedbackForm(control, (ICodeDescription)dropEdit.List[dropEdit.SelectedIndex]);
						}
					}
				}
			}
		}

		[DefaultValue(UserIdleWorkItemOptions.None)]
		public virtual UserIdleWorkItemOptions WorkItemOption => UserIdleWorkItemOptions.None;

#if DEBUG

		[DefaultValue(false)]
		public virtual bool TestRunFindItemWithLongTime { get; }

#endif
#if WINZOR
		internal void ValidateControl()
		{
			DropButton.Validate();
		}
#endif
	}
}
