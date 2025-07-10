using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
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
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public enum FindBoxType
	{
		Guid, Code
	}

	public interface IZCalcFindBoxInternals
	{
		ZCodeFindBox UnitFindBox { get; }
	}

	[DefaultDataSourceBindingMember(null)]
	[SuppressFormDesignerAnalysis]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(Designer))]
	public class ZCalcFindBox : ZUserControl, IZCalcFindBoxInternals, IExtendedControl, INotificationDataMembers, IBindingMemberForCompileTimeCheckProvider, IResourceStringBindingMember
	{
		#region Custom Adornment Layout

		class ZCalcFindBoxAdornmentLayout : AdornmentLayout<ZCalcFindBox>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZCalcFindBox source)
			{
				yield return source.AmountCalcEdit;
				yield return source.UnitFindBox.CodeBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZCalcFindBox source)
			{
				yield return new IconLayout(source.UnitFindBox.PopupButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Designer Support

		internal sealed class Designer : GenericControlDesignerWithTextBoxSnapLine<ZCalcFindBox>
		{
			protected override TextBox GetTextBox(ZCalcFindBox userControl)
			{
				return userControl.AmountCalcEdit;
			}
		}

		#endregion

		#region Constructors

		static ZCalcFindBox()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZCalcFindBoxAdornmentLayout());
		}

		public ZCalcFindBox()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
			CreateFindBoxFromType();
		}

		#endregion

		#region Initialization

		void BuildHintExtension()
		{
			if (AmountCalcEdit.Extensions.Get<IHintExtension>() == null)
			{
				AmountCalcEdit.Extensions.Add(new HintExtension());
			}

			if (UnitFindBox.Extensions.Get<IHintExtension>() == null)
			{
				UnitFindBox.Extensions.Add(new HintExtension());
			}

			Extensions.Substitute<IHintExtension>(
				CompositeHintExtension.Create(AmountCalcEdit, UnitFindBox));
		}

		#endregion

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return AmountCalcEdit.ReadOnly && UnitFindBox.ReadOnly; }
		}

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(ZCalcEditCore.DefaultDecimals)]
		public int Decimals
		{
			get { return AmountCalcEdit.Decimals; }
			set { AmountCalcEdit.Decimals = value; }
		}

		[DefaultValue(true)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowNegative
		{
			get { return AmountCalcEdit.AllowNegative; }
			set { AmountCalcEdit.AllowNegative = value; }
		}

		[DefaultValue(typeof(decimal), "0")]
		public decimal MaxValue
		{
			get { return AmountCalcEdit.MaxValue; }
			set { AmountCalcEdit.MaxValue = value; }
		}

		[DefaultValue("")]
		public string PopupCaption
		{
			get { return fPopupCaption; }
			set
			{
				fPopupCaption = value;
				UnitFindBox.PopupCaption = value;
			}
		}

		#region ModuleID

		[Editor(typeof(ModuleIDEditor), typeof(UITypeEditor))]
		public ModuleIdentifier ModuleID
		{
			get { return UnitFindBox.ModuleID; }
			set
			{
				if (value != ModuleIDs.NotAssigned)
				{
					BindToListAndModuleIdWarner.ShowOnChangedModuleIDWarning(this);
				}
				fModuleID = value;
				UnitFindBox.ModuleID = value;

				//				if (IsDesigning && value == ModuleIDs.NotAssigned) throw new ApplicationException("You must assign a ModuleID.");
			}
		}

		bool ShouldSerializeModuleID()
		{
			return (ModuleID != ModuleIDs.NotAssigned);
		}

		protected ModuleIdentifier fModuleID = ModuleIDs.NotAssigned;

		#endregion

		[DefaultValue(DefaultPreBoundMaxLength)]
		public int PreBoundMaxLength
		{
			get { return fPreBoundMaxLength; }
			set
			{
				fPreBoundMaxLength = value;
				UnitFindBox.PreBoundMaxLength = value;
			}
		}

		const int DefaultPreBoundMaxLength = 3;

		/// <summary>
		/// Determines whether to use a ZGuidFindBox or a ZCodeFindBox.
		/// </summary>
		[DefaultValue(FindBoxType.Guid)]
		[Description("Determines whether to use a ZGuidFindBox or a ZCodeFindBox.")]
		public FindBoxType FindBoxType
		{
			get { return fFindBoxType; }
			set
			{
				fFindBoxType = value;
				CreateFindBoxFromType();
			}
		}

		#region Auto

		void InitializeComponent()
		{
			this.AmountCalcEdit = new ZCalcEdit.Bare();
			this.SuspendLayout();
			// 
			// AmountCalcEdit
			// 
			this.AmountCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.AmountCalcEdit.TabIndex = 0;
			this.AmountCalcEdit.Text = "";
			this.AmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZCalcFindBox
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AmountCalcEdit);
			this.Name = "ZCalcFindBox";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.ResumeLayout(false);
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZCalcFindBox>()
				.Property("ReadOnly", false, false)
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

		#region IZCalcFindBoxInternals Members

		ZCodeFindBox IZCalcFindBoxInternals.UnitFindBox
		{
			get { return UnitFindBox; }
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

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		#endregion

		#region BindToAmount / BindToUnit / BindToDecimalPlaces

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToAmount
		{
			get { return AmountCalcEdit.BindTo; }
			set
			{
				AmountCalcEdit.BindTo = value;
				if (string.IsNullOrEmpty(BindingMember))
				{
					BindingMember = ".";
				}
			}
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToUnit
		{
			get { return UnitFindBox.BindTo; }
			set
			{
				UnitFindBox.BindTo = "";
				SetFindBoxTypeFromBindToUnit(value);
				UnitFindBox.BindTo = value;
			}
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToDecimalPlaces
		{
			get { return AmountCalcEdit.BindToDecimalPlaces; }
			set { AmountCalcEdit.BindToDecimalPlaces = value; }
		}

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		string BindingMember
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

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
			get { return UnitFindBox.BindToList; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					BindToListAndModuleIdWarner.ShowOnChangedBindToListWarning(this);
				}
				UnitFindBox.BindToList = value;
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
				result.Add(new CompileTimeCheckBindingMember(dataSourceType, typeof(ZDecimal), BindToAmount));
			}
			if (!string.IsNullOrEmpty(BindToUnit))
			{
				result.Add(new CompileTimeCheckBindingMember(dataSourceType, FindBoxType == FindBoxType.Guid ? typeof(ZGuid) : typeof(ZString), BindToUnit));
			}
			if (!string.IsNullOrEmpty(BindToList))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(IList), BindToList));
			}
			return result;
		}

		#endregion

		#region Implementation

		protected internal ZCodeFindBox UnitFindBox;
		protected internal ZCalcEdit AmountCalcEdit;
		protected FindBoxType fFindBoxType = FindBoxType.Guid;
		protected int fPreBoundMaxLength = DefaultPreBoundMaxLength;
		protected string fPopupCaption = "";

		protected void CreateFindBoxFromType()
		{
			SuspendLayout();

			try
			{
				var unitFindBoxBindTo = "";
				var unitFindBoxBindToList = "";
				if (UnitFindBox != null)
				{
					unitFindBoxBindTo = UnitFindBox.BindTo;
					unitFindBoxBindToList = UnitFindBox.BindToList;
					Controls.Remove(UnitFindBox);
					UnitFindBox.Dispose();
				}

				UnitFindBox = NewFindBoxFromType();
				UnitFindBox.Dock = System.Windows.Forms.DockStyle.Right;
				UnitFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 0);
				UnitFindBox.Name = "UnitFindBox";
				UnitFindBox.ShowDescriptionBox = false;
				UnitFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20);
				UnitFindBox.TabIndex = 1;
				UnitFindBox.PreBoundMaxLength = fPreBoundMaxLength;
				UnitFindBox.PopupCaption = fPopupCaption;
				UnitFindBox.ModuleID = fModuleID;

				if (!string.IsNullOrEmpty(unitFindBoxBindTo))
				{
					UnitFindBox.BindTo = unitFindBoxBindTo;
				}
				if (!string.IsNullOrEmpty(unitFindBoxBindToList))
				{
					UnitFindBox.BindToList = unitFindBoxBindToList;
				}

				Controls.Add(this.UnitFindBox);
				BuildHintExtension();
			}
			finally
			{
				ResumeLayout(false);
			}
		}

		void SetFindBoxTypeFromBindToUnit(string value)
		{
			if (value.IndexOf("_NK") != -1)
			{
				if (FindBoxType != FindBoxType.Code)
				{
					FindBoxType = FindBoxType.Code;
				}
			}
			else
			{
				if (FindBoxType != FindBoxType.Guid)
				{
					FindBoxType = FindBoxType.Guid;
				}
			}
		}

		ZCodeFindBox NewFindBoxFromType()
		{
			return FindBoxType == FindBoxType.Code ? new ZCodeFindBox.Bare() : new ZGuidFindBox.Bare();
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			var desiredHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
			if (Height != desiredHeight)
			{
				ControlDpiScalingHelper.SetHeight(this, desiredHeight, false);
			}

			base.OnLayout(e);
			base.OnLayout(e); //Fixes a defect at 150% DPI on ZA Customs Declaration Inv Headers tab, where UnitFindBox.Location wasn't updated after AmountCalcEdit.Size was decreased, the first time after opening the tab
		}

		#endregion
	}
}
