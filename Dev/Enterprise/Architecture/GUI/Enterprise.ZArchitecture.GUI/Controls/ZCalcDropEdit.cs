using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(Designer))]
	public class ZCalcDropEdit : ZUserControl, IExtendedControl, INotificationDataMembers, IBindToList, IResourceStringBindingMember, IBindingMemberForCompileTimeCheckProvider
	{
		#region Designer Support

		internal sealed class Designer : GenericControlDesignerWithTextBoxSnapLine<ZCalcDropEdit>
		{
			protected override TextBox GetTextBox(ZCalcDropEdit userControl)
			{
				return userControl.AmountCalcEdit;
			}
		}

		#endregion

		#region Auto

		ZDropEdit UnitDropEdit;
		ZCalcEdit AmountCalcEdit;

		void InitializeComponent()
		{
			this.AmountCalcEdit = new ZCalcEdit.Bare();
			this.UnitDropEdit = new ZDropEdit.Bare();
			this.SuspendLayout();
			// 
			// AmountCalcEdit
			// 
			this.AmountCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.AmountCalcEdit.TabIndex = 0;
			this.AmountCalcEdit.Text = "";
			this.AmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitDropEdit
			// 
			this.UnitDropEdit.Dock = System.Windows.Forms.DockStyle.Right;
			this.UnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 0, true);
			this.UnitDropEdit.Name = "UnitDropEdit";
			this.UnitDropEdit.ShowDescriptionBox = false;
			this.UnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.UnitDropEdit.TabIndex = 1;
			// 
			// ZCalcDropEdit
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AmountCalcEdit);
			this.Controls.Add(this.UnitDropEdit);
			this.Name = "ZCalcDropEdit";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.ResumeLayout(false);
		}

		#endregion

		#region Custom Adornment Layout

		class ZCalcDropEditAdornmentLayout : AdornmentLayout<ZCalcDropEdit>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZCalcDropEdit source)
			{
				yield return source.AmountCalcEdit;
				yield return source.UnitDropEdit.CodeBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZCalcDropEdit source)
			{
				yield return new IconLayout(source.UnitDropEdit.DropButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Constructors

		static ZCalcDropEdit()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZCalcDropEditAdornmentLayout());
		}

		public ZCalcDropEdit()
		{
			InitializeComponent();
			InitializeExtensions();
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(this.UnitDropEdit);
#endif
		}

		#endregion

		#region Initialization

		void InitializeExtensions()
		{
			Extensions = new DefaultControlExtensionCollection(this);

			AmountCalcEdit.Extensions.Add(new HintExtension());
			UnitDropEdit.Extensions.Add(new HintExtension());

			Extensions.Substitute<IHintExtension>(CompositeHintExtension.Create(AmountCalcEdit, UnitDropEdit));
			Extensions.Substitute<IStatusbarExtension>(MultiControlStatusbarExtension.Create(AmountCalcEdit, UnitDropEdit));
		}

		#endregion

		#region Decimals

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(ZCalcEditCore.DefaultDecimals)]
		public int Decimals
		{
			get { return AmountCalcEdit.Decimals; }
			set { AmountCalcEdit.Decimals = value; }
		}

		/// <summary>
		/// Keep at 0 for 'use the maximum value of the numerical type of the calc edit core'.
		/// </summary>
		[DefaultValue(typeof(decimal), "0")]
		public decimal MaxValue
		{
			get { return AmountCalcEdit.MaxValue; }
			set { AmountCalcEdit.MaxValue = value; }
		}

		#endregion

		#region AllowNegative

		[DefaultValue(true)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowNegative
		{
			get { return AmountCalcEdit.AllowNegative; }
			set { AmountCalcEdit.AllowNegative = value; }
		}

		#endregion

		#region Show Group Separators?

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(true)]
		public bool ShowGroupSeparators
		{
			get { return AmountCalcEdit.ShowGroupSeparators; }
			set { AmountCalcEdit.ShowGroupSeparators = value; }
		}

		#endregion

		#region Show Description Box

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(false)]
		public bool ShowDescriptionBox
		{
			get { return UnitDropEdit.ShowDescriptionBox; }
			set { UnitDropEdit.ShowDescriptionBox = value; }
		}

		#endregion

		#region PreBoundMaxLength

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(0)]
		public int UnitPreBoundMaxLength
		{
			get { return UnitDropEdit.PreBoundMaxLength; }
			set { UnitDropEdit.PreBoundMaxLength = value; }
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(true)]
		public bool UnitShouldResizeByMaxLength
		{
			get { return UnitDropEdit.ShouldResizeByMaxLength; }
			set { UnitDropEdit.ShouldResizeByMaxLength = value; }
		}
		#endregion

		#region OnLayout

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);

			if (UnitDropEdit != null && Height != UnitDropEdit.Height)
			{
				ControlDpiScalingHelper.SetHeight(this, UnitDropEdit.Height, false);
			}
		}

		#endregion

		#region BindToAmount / BindToUnit

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToAmount
		{
			get { return BindingSource.GetBindingMember(AmountCalcEdit); }
			set
			{
				BindingSource.SetBindingMember(AmountCalcEdit, value);
				if (string.IsNullOrEmpty(BindingMemberHelper.BindingMember))
				{
					BindingMemberHelper.BindingMember = ".";
				}
			}
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToUnit
		{
			get { return BindingSource.GetBindingMember(UnitDropEdit); }
			set
			{
				BindingSource.SetBindingMember(UnitDropEdit, value);
				if (string.IsNullOrEmpty(BindingMemberHelper.BindingMember))
				{
					BindingMemberHelper.BindingMember = ".";
				}
			}
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

		public override Type DataSourceType
		{
			get { return typeof(object); }
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZCalcDropEdit>()
				.Property("Text", "") // Property name
				.Property("IsVisibleForBinding", ZBool.True)
				.Result;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		#endregion

		#region INotificationDataMembers Members

		string[] INotificationDataMembers.NotificationDataMembers
		{
			get { return new string[] { new KBindingMemberInfo(DataMember, BindToAmount).BindingMember, new KBindingMemberInfo(DataMember, BindToUnit).BindingMember }; }
		}

		#endregion

		#region IBindToList Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToList))]
		public string BindToList
		{
			get { return UnitDropEdit.BindToList; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					BindToListAndModuleIdWarner.ShowOnChangedBindToListWarning(this);
				}
				UnitDropEdit.BindToList = value;
			}
		}

		#endregion

		#region IResourceStringBindingMember Members

		string IResourceStringBindingMember.ResourceStringBindingMember
		{
			get { return BindToAmount; }
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			if (!string.IsNullOrEmpty(BindToAmount))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(INumericZType), BindToAmount));
			}
			if (!string.IsNullOrEmpty(BindToUnit))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(ZString), BindToUnit));
			}
			if (!string.IsNullOrEmpty(BindToList))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(IList), BindToList));
			}
			return result;
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
	}
}
