using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
#if WINZOR
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Enums;
using WinzorFramework.JSInterop;
#endif
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public interface IFindBoxUserControl
	{
		void SelectFromPopupForm(bool autoSelect = false);
		void SelectFromPopupFormWithoutDisplaying();
		void ShowEditOrViewForm();
		bool AutoCompleteText(bool explicitAutoComplete);
		bool AutoCompleteOnCommit { get; }
		bool PrefixExistsInModule(string prefix);
		bool AutoCompleteDisabled { get; set; }
		bool AllowModuleMultiSelect { get; set; }
	}

	interface IHookableControl
	{
		void ClearHooks();
	}

	sealed class ZFindBoxUserControlDesigner : GenericControlDesignerWithTextBoxSnapLine<ZFindBoxUserControl>
	{
		protected override TextBox GetTextBox(ZFindBoxUserControl userControl)
		{
			return userControl.CodeBox;
		}
	}

	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(ZFindBoxUserControlDesigner))]
	[DefaultBindingProperty("Text")] // the attributes on the Text are used to express the meta-data bindings
	public abstract partial class ZFindBoxUserControl : ZListUserControl, IFindBoxUserControl, IExtendedControl, IHookableControl
	{
		#region Constructors

		protected ZFindBoxUserControl()
		{
			CodeBox = GetCodeBox();
			InitializeComponent();
			Extensions = NewExtensionCollection();
			CodeBox.ReadOnlyChanged += delegate
			{ OnReadOnlyChanged(EventArgs.Empty); };
			PopupButton.EnabledChanged += PopupButton_EnabledChanged;
		}

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new DefaultControlExtensionCollection(this);
		}

		#endregion

		#region ZCodeBox

		public virtual ZCodeBox GetCodeBox()
		{
			return new ZCodeBox(this);
		}

		public virtual bool PrefixExistsInModule(string prefix)
		{
			return false;
		}

		[ToolboxItem(false)]
		public class ZCodeBox : ZTextBox.Bare, IAdditionalInformation
		{
			public ZCodeBox()
			{
				Hotkeys.RegisterHotKey(Keys.F4, SearchInModuleHotkey, Res.GetString("d72473c6-bf1f-4ec2-affc-064c0eba86f3", "Show module"));
				Hotkeys.RegisterHotKey(Keys.F3, ShowFormHotkey, Res.GetString("4ff68099-fe12-4f25-ba7e-86816c142147", "Create new (if empty) / Show (if set)"));
				Hotkeys.AddDescription("'='", Res.GetString("86b3c030-2447-4f8a-b0ca-946484f384db", "Auto complete"));
			}

			public ZCodeBox(IFindBoxUserControl parentFindBox)
				: this()
			{
				this.ParentFindBox = parentFindBox;
			}

			public override bool ShouldAggressivelyTruncateText
			{
				get
				{
					return false;
				}
			}

			public override string Text
			{
				get { return base.Text; }
				set { base.Text = (value != null && MaxLength > 0 && value.Length > MaxLength) ? value.Substring(0, MaxLength) : value; }
			}

			public void CommitBoundValue()
			{
				OnValidating(new CancelEventArgs(false));
				OnValidated(EventArgs.Empty);
			}

			bool SearchInModuleHotkey(object sender, Keys code)
			{
				if (!ReadOnly)
				{
					UserEventTracker.Instance.AddUserEvent(this, "KeyPress", "F4");
					ParentFindBox.SelectFromPopupForm(true);
					return true;
				}

				return false;
			}

			void ShowFormHotkey()
			{
				UserEventTracker.Instance.AddUserEvent(this, "KeyPress", "F3");
				ParentFindBox.ShowEditOrViewForm();
			}

			public bool HandleKey(Keys keyData)
			{
				return Hotkeys.ProcessCmdKey(this, keyData);
			}

			// This getter was added to help debug the issue described by WI00770316. Please remove once the issue is resolved
			public string GetDataMember()
			{
				return base.DataMember;
			}

			protected readonly IFindBoxUserControl ParentFindBox;
#if WINZOR

			protected override async Task OnAfterRenderAsync(bool firstRender)
			{
				if (firstRender)
				{
					if (IsElementReferenceCaptured)
					{
						var cargoWiseClientServices = this.FindForm()?.CargoWiseClientServices;
						if (cargoWiseClientServices?.ClientEventService is not null)
						{
							await cargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(OnKeyPressAsync, ClientKeyEvent.KeyPress, new ClientKeyEventData("=", false, true), ElementReference);
						}
					}
				}
			}

			protected override int ClientSideEventHandlers => (int)ElementEventHandlers.None;

			Task OnKeyPressAsync()
			{
				return InvokeWinzorDispatcherAsync(() =>
				{
					if (!ReadOnly && !ParentFindBox.AutoCompleteDisabled)
					{
						ParentFindBox.AutoCompleteText(true);
					}
				});
			}
#else
			protected override void OnKeyPress(KeyPressEventArgs e)
			{
				//if user entered a literal =, keep it that way. else it's autocomplete
				if (e.KeyChar == '=' && !ReadOnly && !ParentFindBox.AutoCompleteDisabled)
				{
					var result = ParentFindBox.AutoCompleteText(true);
					if (result)
					{
						e.Handled = true;
						return;
					}
				}
				base.OnKeyPress(e);
			}
#endif

			protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
			{
				// OnLeave fires after Commit if you press Tab - so we need to do this as early as possible
				if (keyData == Keys.Tab || keyData == Keys.Enter)
				{
					AutoCompleteOnCommit();
				}

				return base.ProcessCmdKey(ref msg, keyData);
			}

			protected override void OnLeave(EventArgs e)
			{
				base.OnLeave(e);
				if (!this.IsDesignMode())
				{
					AutoCompleteOnCommit();
				}
			}

			protected virtual void AutoCompleteOnCommit()
			{
				var hookableControl = ParentFindBox as IHookableControl;
				if (hookableControl != null)
				{
					hookableControl.ClearHooks();
				}

				if (ParentFindBox.AutoCompleteOnCommit && Text.Trim().Length > 0)
				{
					ParentFindBox.AutoCompleteText(false);
				}
				if (IsPrefixed)
				{
					ParentFindBox.SelectFromPopupFormWithoutDisplaying();
				}
			}

			bool IsPrefixed
			{
				get
				{
					var index = Text.IndexOf(':');
					if (index >= 0)
					{
						var prefix = Text.Substring(0, index);
						return ParentFindBox.PrefixExistsInModule(prefix);
					}
					return false;
				}
			}

			#region IAdditionalInformation Members
			string IAdditionalInformation.AdditionalInformation
			{
				get => (ParentFindBox as IAdditionalInformation)?.AdditionalInformation;
			}
			#endregion;
		}

#endregion

		#region Binding Mappings

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		public override string Text
		{
			get { return CodeBox.Text; }
		}

		#endregion

		#region MaxLength

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MaxLength
		{
			get { return CodeBox.MaxLength; }
			set
			{
				if (CodeBox.MaxLength != value)
				{
					CodeBox.MaxLength = value;
					OnMaxLengthChanged();
				}
			}
		}

		protected virtual void OnMaxLengthChanged()
		{
			if (MaxLengthChanged != null)
			{
				MaxLengthChanged(this, EventArgs.Empty);
			}
		}
		public event EventHandler MaxLengthChanged;

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
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

		#region IFindBoxUserControl Members

		void IFindBoxUserControl.SelectFromPopupForm(bool autoSelect)
		{
			SelectFromPopupForm(autoSelect);
		}

		void IFindBoxUserControl.SelectFromPopupFormWithoutDisplaying()
		{
			var info = CurrentBoundPropertyInfo;
			if (info != null)
			{
				UnhookPropertyInfo(info);
			}

			if (SelectFromPopupFormWithoutDisplaying() == SilentSelectResult.MultipleResults)
			{
				if (info != null)
				{
					HookPropertyInfo(info, new MultipleResultsCodePrefixValidator(info, info.Value.IsEmpty || info.Value.IsValid, RelatedBizoName));
				}
			}
		}

		void IFindBoxUserControl.ShowEditOrViewForm()
		{
			ShowEditOrViewForm();
		}

		[Browsable(true), DefaultValue(false)]
		public bool AllowModuleMultiSelect { get; set; }

		[Browsable(true), DefaultValue(false)]
		public bool AutoCompleteDisabled
		{
			get; set;
		}

		bool IFindBoxUserControl.AutoCompleteText(bool explicitAutoComplete)
		{
			return AutoCompleteText(explicitAutoComplete);
		}

		bool IFindBoxUserControl.AutoCompleteOnCommit
		{
			get { return AutoCompleteOnCommit; }
		}

		protected virtual bool AutoCompleteOnCommit
		{
			get { return false; }
		}

		ZPropertyInfo CurrentBoundPropertyInfo
		{
			get
			{
				if (currentBoundPropertyInfo == null || currentBoundPropertyInfo.BizObj != CurrentItem)
				{
					currentBoundPropertyInfo = null;

					var bizo = CurrentItem as BusinessObject;

					if (bizo != null)
					{
						var propertyPath = DataPropertyName;
						currentBoundPropertyInfo = bizo.FindPropertyInfo(propertyPath);
						if (currentBoundPropertyInfo == null)
						{
							var index = Math.Max(propertyPath.IndexOf('.'), propertyPath.IndexOf('+'));
							if (index >= 0)
							{
								currentBoundPropertyInfo = bizo.FindPropertyInfo(propertyPath.Substring(index + 1));
							}
						}
					}
				}
				return currentBoundPropertyInfo;
			}
		}
		ZPropertyInfo currentBoundPropertyInfo;

		string RelatedBizoName
		{
			get
			{
				if (string.IsNullOrEmpty(bizoName))
				{
					bizoName = Res.GetString("7138490a-be8d-4ebe-95a1-459a137adee9", "element");

					if (List != null)
					{
						var bizoType = BusinessObjectCollection.GetElementTypeFromCollectionType(List.GetType());
						var tableName = bizoType != null ? BusinessObjectFactory.GetTableNameFromType(bizoType) : null;
						if (!string.IsNullOrEmpty(tableName))
						{
							bizoName = DataBoundResourceStrings.GetTableDescriptiveName(tableName);
						}
					}
				}
				return bizoName;
			}
		}
		string bizoName;

		void IHookableControl.ClearHooks()
		{
			UnhookPropertyInfo(CurrentBoundPropertyInfo);
		}

		#region CodePrefixValidation

		abstract class CodePrefixValidator
		{
			protected CodePrefixValidator(ZPropertyInfo info, bool delayedUnhook)
			{
				Info = info;

				Info.AdditionalValidation += DoValidation;
				if (delayedUnhook)
				{
					Info.ValueChanged += HookUnhook;
				}
				else
				{
					Info.ValueChanged += UnhookInfo;
				}
			}

			protected ZPropertyInfo Info { get; private set; }

			protected abstract void DoValidation();

			public void UnhookInfo(object sender, EventArgs e)
			{
				if (Info != null)
				{
					Info.AdditionalValidation -= DoValidation;
					Info.ValueChanged -= UnhookInfo;
					Info.ValueChanged -= HookUnhook;
					Info = null;
				}
			}

			void HookUnhook(object sender, EventArgs e)
			{
				if (Info != null)
				{
					Info.ValueChanged -= HookUnhook;
					Info.ValueChanged -= UnhookInfo;
					Info.ValueChanged += UnhookInfo;
				}
			}
		}

		class MultipleResultsCodePrefixValidator : CodePrefixValidator
		{
			public MultipleResultsCodePrefixValidator(ZPropertyInfo info, bool delayedUnhook, string bizoName)
				: base(info, delayedUnhook)
			{
				this.bizoName = bizoName;
			}

			readonly string bizoName;

			protected override void DoValidation()
			{
				Info.AddError(Res.GetString("acb2e6a3-0f5d-4306-bcf7-b45d68018e36",
					"There are multiple possible {0}. Select the {1} cell and press F4 to choose one.",
					Grammar.Instance.Pluralize(bizoName), Info.HumanReadableName));
			}
		}

		void HookPropertyInfo(ZPropertyInfo info, CodePrefixValidator validator)
		{
			if (info != null)
			{
				propertyInfoCodePrefixValidators[info] = validator;
			}
		}

		void UnhookPropertyInfo(ZPropertyInfo info)
		{
			if (info != null)
			{
				CodePrefixValidator validator;
				propertyInfoCodePrefixValidators.TryGetValue(info, out validator);
				if (validator != null)
				{
					validator.UnhookInfo(info, EventArgs.Empty);
					propertyInfoCodePrefixValidators.Remove(info);
				}
			}
		}

		readonly Dictionary<ZPropertyInfo, CodePrefixValidator> propertyInfoCodePrefixValidators = new Dictionary<ZPropertyInfo, CodePrefixValidator>();

		#endregion

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool ReadOnly
		{
			get { return CodeBox.ReadOnly; }
			set
			{
				if (CodeBox.ReadOnly != value)
				{
					CodeBox.ReadOnly = value;
					UpdatePopupButtonReadonly();
				}
			}
		}

		public event EventHandler ReadOnlyChanged
		{
			add { CodeBox.ReadOnlyChanged += value; }
			remove { CodeBox.ReadOnlyChanged -= value; }
		}

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

		void PopupButton_EnabledChanged(object sender, EventArgs e)
		{
			if (!isUpdatingPopupButtonReadonly)
			{
				UpdatePopupButtonReadonly();
			}
		}

		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			UpdatePopupButtonReadonly();
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
		}

		void UpdatePopupButtonReadonly()
		{
			if (!PopupButtonReadonlyCanBeDifferent && !isUpdatingPopupButtonReadonly && PopupButton.ReadOnly != CodeBox.ReadOnly)
			{
				isUpdatingPopupButtonReadonly = true;
				try
				{
					PopupButton.ReadOnly = CodeBox.ReadOnly;
				}
				finally
				{
					isUpdatingPopupButtonReadonly = false;
				}
			}
		}

		public bool PopupButtonReadonlyCanBeDifferent
		{
			get
			{
				return GetPopupButtonReadonlyCanBeDifferentCore();
			}
			protected set
			{
				popupButtonReadonlyCanBeDifferent = value;
			}
		}

		protected virtual bool GetPopupButtonReadonlyCanBeDifferentCore() => popupButtonReadonlyCanBeDifferent;

		bool popupButtonReadonlyCanBeDifferent;
		bool isUpdatingPopupButtonReadonly;

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

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZFindBoxUserControl>()
				.Property("ReadOnly", false, false)
				.Property("ReadOnlyForBinding", true, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Property("IsVisibleForBinding", ZBool.True)
				.Property("MaxLength", -1)
				.Property<IList>("List", null, false) // Property name
				.Result;
		}

		#endregion

		#region Implementation

		public ZCodeBox CodeBox;
		public ZButton.Bare PopupButton;

		protected virtual bool AutoCompleteText(bool explicitAutoComplete) { return false; }
		public virtual void SelectFromPopupForm(bool autoSelect = false) { }

		public virtual SilentSelectResult SelectFromPopupFormWithoutDisplaying()
		{
			return SilentSelectResult.None;
		}

		protected virtual void ShowEditOrViewForm() { }

		void PopupButton_Click(object sender, EventArgs e)
		{
			SelectFromPopupForm();
		}

		void PopupButton_MouseUp(object sender, MouseEventArgs e)
		{
			if (Visible && CodeBox.Visible && CodeBox.Enabled)
			{
				try
				{
					ActiveControl = CodeBox;
				}
				catch (ArgumentException) { } //Invisible or disabled control cannot be activated
			}
		}

		protected string InvalidDescription(string code)
		{
			return string.IsNullOrEmpty(code) ? Constants.FindBoxMessages.NoneSelected : Constants.FindBoxMessages.InvalidSelection;
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			SuspendLayout();
			if (RestrictHeight && Height > ControlDpiScalingHelper.ScaleToCurrentDpiY(21))
			{
				ControlDpiScalingHelper.SetHeight((Control)this, 21, true);
			}

			SynchronizeLayout();
			ResumeLayout(false);

			base.OnLayout(e);
		}

		protected virtual bool RestrictHeight
		{
			get { return true; }
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			CodeBox.SelectAll();
		}

		internal void SetWidth(int width)
		{
			var newWidth = width - PopupButton.Width;
			if (newWidth <= 0)
			{
				return;
			}

			try
			{
				SuspendLayout();
				ControlDpiScalingHelper.SetWidth((Control)CodeBox, newWidth, false);
			}
			finally
			{
				ResumeLayout();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		void SynchronizeLayout()
		{
			// The textbox's size depends on the runtime font size. So the control's heght has to be adapted to mimic that size
			if (!IsOnGrid && Height != CodeBox.Height)
			{
				SuspendLayout();
				ControlDpiScalingHelper.SetHeight((Control)this, CodeBox.Height, false);
				ResumeLayout(false);
			}
		}
	}
}

