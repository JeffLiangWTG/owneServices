using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public class ZCodeFindBox : ZPopupFindBox, ICustomizableFindBoxPopup, IFindBoxPopupWithBindToForDescription
	{
		#region Bare

		[ToolboxItem(false)]
		public new class Bare : ZCodeFindBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Custom Adornment Layout

		class ZCodeFindBoxAdornmentLayout : AdornmentLayout<ZCodeFindBox>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZCodeFindBox source)
			{
				yield return source.CodeBox;
				yield return source.DescriptionBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZCodeFindBox source)
			{
				yield return new IconLayout(source.PopupButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Constructors

		static ZCodeFindBox()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZCodeFindBoxAdornmentLayout());
		}

		public ZCodeFindBox()
		{
			InitializeComponent();
			CodeBox.TextChanged += delegate
			{ OnTextChanged(EventArgs.Empty); };
		}

		#endregion

		#region Properties

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CurrentCode
		{
			get { return CodeBox.Text; }
			set { CodeBox.Text = value; }
		}

		[SmartTagVisible]
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

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable(false)]
		[DefaultValue(false)]
		public bool AllowTemplateRecords { get; set; }

		[Browsable(true)]
		[DefaultValue(true)]
		public bool ShouldAddFetchHints { get; set; } = true;

		#endregion

		#region CommitBoundValue

		public void CommitBoundValue()
		{
			CodeBox.CommitBoundValue();
		}

		#endregion

		#region Show Description Box

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(true)]
		public bool ShowDescriptionBox
		{
			get { return showDescriptionBox; }
			set
			{
				if (showDescriptionBox != value)
				{
					showDescriptionBox = value;
					SetDescriptionBoxVisibility();
				}
			}
		}

		void SetDescriptionBoxVisibility()
		{
			SuspendLayout();
			try
			{
				if (ShowDescriptionBox)
				{
					ControlDpiScalingHelper.SetWidth(this, this.Width + previousDescriptionBoxWidth, false);
					ControlDpiScalingHelper.SetWidth(ref DescriptionBox, previousDescriptionBoxWidth, false);
					DescriptionBox.Visible = true;
				}
				else
				{
					previousDescriptionBoxWidth = DescriptionBox.Width;
					ControlDpiScalingHelper.SetWidth(this, this.Width - DescriptionBox.Width, false);
					DescriptionBox.Visible = false;
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		internal void EnsureValidControlWidth()
		{
			if (CodeBox != null && PopupButton != null && DescriptionBox != null && Width != 0)
			{
				var codeBoxWidth = CodeBox.Visible ? CodeBox.Width : 0;

				if (!ShowDescriptionBox && Width > codeBoxWidth + PopupButton.Width)
				{
					ControlDpiScalingHelper.SetWidth(this, codeBoxWidth + PopupButton.Width, false);
				}
				else
				{
					var minWidth = codeBoxWidth + PopupButton.Width + (ShowDescriptionBox ? DescriptionBox.Width : 0);
					if (Width < minWidth)
					{
						ControlDpiScalingHelper.SetWidth(this, minWidth, false);
					}
				}
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			EnsureValidControlWidth();
		}

		bool showDescriptionBox = true;
		int previousDescriptionBoxWidth;

		#endregion

		#region BindToForDescription

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public virtual string BindToForDescription
		{
			get { return bindToForDescription; }
			set { bindToForDescription = value; }
		}
		string bindToForDescription = "";

		#endregion

		#region Readonly For Description

		[DefaultValue(false)]
		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;

				DescriptionBox.ReadOnly = !CanReferenceByDescription || value;
				DescriptionBox.TabStop = !DescriptionBox.ReadOnly;
			}
		}

		protected virtual bool CanReferenceByDescription
		{
			get
			{
				if (canReferenceByDescription == null)
				{
					if (this.IsDesignMode())
					{
						canReferenceByDescription = false;
					}
					else
					{
						using (PreventErrorReportForMissingListAttribute())
						{
							var list = ListNoPullIfNull;
							if (list != null || ModuleIDDoNotLookInList != ModuleIDs.NotAssigned)
							{
								var bizoCollection = list as IBusinessObjectCollection;
								if (list == null)
								{
									try
									{
										using (var module = NewModuleFromModuleID())
										{
											if (module != null)
											{
												bizoCollection = module.GridCollection;
											}
										}
									}
									catch (Exception ex)
									{
										if (ex.IsCriticalException())
										{
											throw;
										}
									}
								}
								canReferenceByDescription = bizoCollection != null && DescriptionPropertyAttribute.CanBeReferencedByDescription(bizoCollection.TypeOfElements);
							}
						}
					}
				}
				return canReferenceByDescription != null && canReferenceByDescription.Value;
			}
		}
		bool? canReferenceByDescription;

		protected override void InvalidateListCore()
		{
			base.InvalidateListCore();

			canReferenceByDescription = null;
			ReadOnly = ReadOnly;
		}

		#endregion

		#region SetControlSize

		[DefaultValue(true)]
		public bool ShouldResize { get; set; } = true;

		protected override void OnMaxLengthChanged()
		{
			base.OnMaxLengthChanged();
			if (ShouldResize)
			{
				SetControlSize(MaxLength);
			}
		}

		protected virtual void SetControlSize(int charLength)
		{
			SuspendLayout();

			try
			{
				TextBoxControlSize.ResizeControl(CodeBox, charLength);
			}
			finally
			{
				ResumeLayout();
			}
		}

		#endregion

		#region InitializeComponent

		public ZTextBox DescriptionBox;

		void InitializeComponent()
		{
			this.DescriptionBox = GetDescriptionBox();
			this.SuspendLayout();
			// 
			// DescriptionBox
			// 
			this.DescriptionBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
			this.DescriptionBox.Name = "DescriptionBox";
			this.DescriptionBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.DescriptionBox.TabIndex = 2;
			this.DescriptionBox.Text = "";
			this.DescriptionBox.ReadOnly = true;
			this.DescriptionBox.TabStop = false;
			// 
			// ZCodeFindBox
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DescriptionBox);
			this.Name = "ZCodeFindBox";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.Controls.SetChildIndex(this.DescriptionBox, 0);
			this.ResumeLayout(false);
		}

		#endregion

		#region IDataBoundControl Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constand, Property name")]
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			CodeBox.DataBindings.RemoveBinding("Text");
			DescriptionBox.DataBindings.RemoveBinding("Text");
			DataBindings.RemoveBinding("List");

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null && !this.IsDesignMode())
			{
				CodeBox.DataBindings.RemoveBinding("Text");
				var codeBinding = new KBinding("Text", dataSource, dataMember, true);
				codeBinding.Parse += new ConvertEventHandler(OnParseValue);
				codeBinding.Format += new ConvertEventHandler(OnFormatValue);
				codeBinding.BindingComplete += new BindingCompleteEventHandler(OnBindingComplete);
				CodeBox.DataBindings.Add(codeBinding);

				if (!string.IsNullOrEmpty(BindToForDescription))
				{
					DescriptionBox.DataBindings.Add(new KBinding("Text", dataSource, BindingHelper.GetNestedControlDataMember(BindTo, dataMember, BindToForDescription), true, DataSourceUpdateMode.Never));
				}
				else
				{
					if (CanReferenceByDescription)
					{
						DescriptionBox.ReadOnly = CodeBox.ReadOnly;
						DescriptionBox.TabStop = true;
					}

					UpdateDescription();
				}
			}
		}

		public override Type DataSourceType
		{
			get { return typeof(ZString); }
		}

		protected override bool RequiresListDataBinding
		{
			get { return true; }
		}

		void OnParseValue(object sender, ConvertEventArgs e)
		{
			OnParseValue(e);
		}

		void OnFormatValue(object sender, ConvertEventArgs e)
		{
			OnFormatValue(e);
		}

		void OnBindingComplete(object sender, BindingCompleteEventArgs e)
		{
			if (e.BindingCompleteContext == BindingCompleteContext.DataSourceUpdate)
			{
				e.Binding.ReadValue();
			}
		}

		protected virtual void OnParseValue(ConvertEventArgs e)
		{
		}

		protected virtual void OnFormatValue(ConvertEventArgs e)
		{
		}

		#endregion

		#region IFetchHintGenerator Members

		protected override void AddFetchHints(object dataSource, string dataMember)
		{
			if (ShouldAddFetchHints &&
				(string.IsNullOrEmpty(dataMember) || dataMember == BindTo))
			{
				GetFetchHintHandler(this, dataSource).Add();
			}
		}

		protected virtual ZCodeFindBoxFetchHintHandler GetFetchHintHandler(IBindToList control, object dataSource)
		{
			return new ZCodeFindBoxFetchHintHandler(control, dataSource);
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

		#region ICustomizableFindBoxPopup Members

		string ICustomizableFindBoxPopup.CodeForPopup
		{
			get { return CanReferenceByDescription && ActiveControl == DescriptionBox ? Description : Code; }
		}

		string ICustomizableFindBoxPopup.PropertyNameForPopup
		{
			get
			{
				string result = null;
				var list = List as IBusinessObjectCollection;
				if (list != null)
				{
					var typeOfElement = list.TypeOfElements;
					if (list is ICodePropertyNameProvider provider)
					{
						result = provider.GetCodePropertyName(typeOfElement);
					}
					if (result == null)
					{
						result = CanReferenceByDescription && ActiveControl == DescriptionBox
						? DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeOfElement)
						: CodePropertyAttribute.CodePropertyNameFromType(typeOfElement);
					}
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		protected string cachedCode;
		protected string cachedDescription;
		bool needUpdateCachedDescription;
		int preBoundMaxLength;
		IList cachedList;

		#region CodeFindBoxCodeBox

		public override ZCodeBox GetCodeBox()
		{
			return new CodeFindBoxCodeBox(this);
		}

		class CodeFindBoxCodeBox : ZCodeBox
		{
			public CodeFindBoxCodeBox(ZCodeFindBox owner)
				: base(owner)
			{
				this.owner = owner;
			}

			public override string Text
			{
				get { return base.Text; }
				set
				{
					base.Text = value;

					if (owner.Code != owner.cachedCode || !owner.CanReferenceByDescription)
					{
						owner.UpdateDescription();
					}
				}
			}

			protected override void OnValidated(EventArgs e)
			{
				base.OnValidated(e);
				if (owner.Code != owner.cachedCode || !owner.CanReferenceByDescription)
				{
					owner.UpdateDescription();
				}
			}

			readonly ZCodeFindBox owner;
		}

		#endregion

		#region CodeFindBoxDescriptionBox

		public virtual ZCodeBox GetDescriptionBox()
		{
			return new CodeFindBoxDescriptionBox(this);
		}

		class CodeFindBoxDescriptionBox : ZCodeBox
		{
			public CodeFindBoxDescriptionBox(ZCodeFindBox owner)
				: base(owner)
			{
				this.owner = owner;
			}

			public override string Text
			{
				get { return base.Text; }
				set
				{
					base.Text = value;
					if (owner.CanReferenceByDescription)
					{
						owner.UpdateCode();
					}
				}
			}

			protected override void OnValidated(EventArgs e)
			{
				base.OnValidated(e);
				if (owner.CanReferenceByDescription)
				{
					owner.UpdateCode();
				}
			}

			protected override void OnKeyPress(KeyPressEventArgs e)
			{
				if (e.KeyChar == '=' && owner.CanReferenceByDescription)
				{
					e.Handled = true;
					owner.AutoCompleteDescription(true);
				}
				else
				{
					base.OnKeyPress(e);
				}
			}

			protected override void AutoCompleteOnCommit()
			{
				if (!ReadOnly && ParentFindBox.AutoCompleteOnCommit && owner.CanReferenceByDescription && owner.Description.Trim().Length > 0)
				{
					owner.AutoCompleteDescription(false);
				}
			}

			readonly ZCodeFindBox owner;
		}

		#endregion

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible && !this.IsDesignMode() && FindForm() != null)
			{
				UpdateDescription();
			}
		}

		#region UpdateDescription

		void UpdateDescription()
		{
			if (string.IsNullOrEmpty(BindToForDescription) && !DesignModeFinder.IsDesigning && Visible && Created)
			{
				using (PreventErrorReportForMissingListAttribute())
				{
					UpdateDescriptionFromCode();
				}
			}
		}

		protected virtual void UpdateDescriptionFromCode()
		{
			Description = GetDescription();
		}

		#endregion

		#region UpdateCode

		void UpdateCode()
		{
			if (string.IsNullOrEmpty(BindToForDescription) && !DesignModeFinder.IsDesigning && Visible && Created)
			{
				UpdateCodeFromDescription();
				CodeBox.CommitBoundValue();
			}
		}

		protected virtual void UpdateCodeFromDescription()
		{
			Code = CodeFromDescription(Description);
		}

		#endregion
#if DEBUG
		internal string DescriptionExposed => Description;
#endif
		protected override string Description
		{
			get { return DescriptionBox.Text; }
			set
			{
				needUpdateCachedDescription = Description != value;
				DescriptionBox.Text = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Size MinimumSize
		{
			get { return base.MinimumSize; }
			set { base.MinimumSize = value; }
		}

		protected override bool AutoCompleteText(bool explicitAutoComplete)
		{
			//discern between = press that ended up being a literal = and autocomplete
			var result = base.AutoCompleteText(explicitAutoComplete);
			if (result)
			{
				UpdateDescription();
			}
			return result;
		}

		protected virtual void AutoCompleteDescription(bool explicitAutoComplete)
		{
			var findBoxListProviderDescriptionEx = FindBoxListProviderDescriptionEx;
			if (findBoxListProviderDescriptionEx != null)
			{
				var previousDescription = Description;
				Description = findBoxListProviderDescriptionEx.NearestDescriptionMatch(Description, explicitAutoComplete);
				if (Description.StartsWith(previousDescription))
				{
					DescriptionBox.SelectionStart = Math.Min(Description.Length, previousDescription.Length);
					DescriptionBox.SelectionLength = Math.Max(0, Description.Length - previousDescription.Length);
				}
				else
				{
					DescriptionBox.SelectAll();
				}

				UpdateCodeFromDescription();
				CodeBox.CommitBoundValue();
			}
		}

		IFindBoxListProviderDescriptionEx FindBoxListProviderDescriptionEx
		{
			get
			{
				var result = IFindBox.ListProvider as IFindBoxListProviderDescriptionEx;
				if (result == null && IFindBox.ListProvider != null)
				{
					result = IFindBox.ListProvider.List as IFindBoxListProviderDescriptionEx;
				}
				return result;
			}
		}

		protected virtual string GetDescription()
		{
			var code = Code;

			var codeChanged = cachedCode != code;
			if (codeChanged)
			{
				cachedCode = code;
			}

			var listChanged = cachedList != ListNoPullIfNull;
			if (listChanged)
			{
				cachedList = ListNoPullIfNull;
			}

			if (codeChanged || needUpdateCachedDescription || listChanged)
			{
				cachedDescription = IFindBox.ListProvider.DescriptionFromCode(code);

				if (cachedDescription == null)
				{
					cachedDescription = (string.IsNullOrEmpty(code) && CanReferenceByDescription) || EmptyDescriptionIfCodeNotFound ? string.Empty : InvalidDescription(code);
				}
			}

			return cachedDescription;
		}

		[DefaultValue(false)]
		public bool EmptyDescriptionIfCodeNotFound { get; set; }

		protected virtual string CodeFromDescription(string description)
		{
			if (description != cachedDescription)
			{
				cachedDescription = description;

				var findBoxListProviderDescriptionEx = FindBoxListProviderDescriptionEx;
				if (findBoxListProviderDescriptionEx != null)
				{
					cachedCode = findBoxListProviderDescriptionEx.CodeFromDescription(description) ?? (string.IsNullOrEmpty(description) ? string.Empty : "?");
				}
				else
				{
					cachedCode = string.Empty;
				}
			}
			return cachedCode;
		}

		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			if (module is ZFilterGridModule filterGridModule)
			{
				filterGridModule.AllowLoadTemplateRecords = AllowTemplateRecords;
			}

			return base.CreateEmbeddedPopup(module);
		}

		#endregion
	}
}
