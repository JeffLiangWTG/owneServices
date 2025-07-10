using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// A control for selecting an organisation and address.
	/// SetDataBinding() takes a dataSource and dataMember that point to the OrgAddress foreign key.
	/// For example, SetDataBinding(shipment, "JS_OA_NotifyParty");
	/// In this example, property JS_OA_NotifyParty_ZAddress of type ZAddress is also expected.
	/// </summary>
	[DefaultDataSourceBindingMember(null)]
	[CompositeFieldControl]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZAddressControl : ZUserControl, IFetchHintGenerator, IExtendedControl, INotificationDataMembers, IZAddressParent, IBindTo, IReadOnlyToggleControl, IDynamicLayoutAddressControl
	{
		internal ZAddressFindBox.Bare OrganisationFindBox;
		internal ZAddressDropEdit.Bare AddressDropEdit;
		internal ZTextBox.Bare AddressTextBox;
		protected internal ZButton AddressStatusButton;

		#region Auto

		readonly Container components;

		#endregion

		#region Custom Adornment Layout

		class ZAddressControlAdornmentLayout : AdornmentLayout<ZAddressControl>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZAddressControl source)
			{
				yield return source.OrganisationFindBox.CodeBox;
				yield return source.OrganisationFindBox.DescriptionBox;
				yield return source.AddressDropEdit.CodeBox;
				yield return source.AddressDropEdit.DescriptionBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZAddressControl source)
			{
				yield return new IconLayout(source.OrganisationFindBox.PopupButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Constructors

		static ZAddressControl()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZAddressControlAdornmentLayout());
		}

		public ZAddressControl()
		{
			InitializeComponent();
			InitializeControl();
			InitializeExtensions();
			InitializeEventHandlers();
		}

		#endregion

		#region Initialization

		void InitializeControl()
		{
			AddressTextBox.ReadOnly = true;
			AddressTextBox.BackColor = Color.White;
		}

		void InitializeExtensions()
		{
			Extensions = new DefaultControlExtensionCollection(this);
			Extensions.Substitute<IHintExtension>(CompositeHintExtension.Create(OrganisationFindBox, AddressDropEdit));

			OrganisationFindBox.Extensions.Add(new HintExtension());
			AddressDropEdit.Extensions.Add(new HintExtension());

			if (AddressDropEdit.GetExtension<INotificationExtension>() == null)
			{
				AddressDropEdit.Extensions.Add(new NotificationExtension()); // For validation notification icons
			}
		}

		void InitializeEventHandlers()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				OrganisationFindBox.Leave += (object sender, EventArgs e) => UpdateIsOrgVisibleOnDataSource();
				OrganisationFindBox.Validated += (sender, args) => Extensions.Get<IValidationExtension>().Validate();
				AddressDropEdit.SelectedIndexChanged += AddressDropEdit_SelectedIndexChanged;
				AddressDropEdit.BoundValueCommitted += AddressDropEdit_BoundValueCommitted;
			}
		}

		#endregion

		#region Auto

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Bind to Org List

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindToList")]
		public string BindToOrgList
		{
			get { return OrganisationFindBox.BindToList; }
			set { OrganisationFindBox.BindToList = value; }
		}

		public void CorrectBindToOrgList(string prefix)
		{
			if (!BindToOrgList.StartsWith(prefix, StringComparison.Ordinal))
			{
				BindToOrgList = prefix + BindToOrgList;
			}
		}

		#endregion

		#region Popup Caption

		[Category(ZGUIConstants.DesignerCategory)]
		public string PopupCaption
		{
			get { return OrganisationFindBox.PopupCaption; }
			set { OrganisationFindBox.PopupCaption = value; }
		}

		#endregion

		#region Stack Controls

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(false)]
		public bool StackControls
		{
			get { return fStackControls; }
			set
			{
				if (fStackControls != value)
				{
					fStackControls = value;
					SetControlSize();
				}
			}
		}

		bool fStackControls;

		#endregion

		#region Show Address

		[Category(ZGUIConstants.DesignerCategory), DefaultValue(true)]
		public bool ShowAddress
		{
			get { return fShowAddress; }
			set
			{
				if (fShowAddress != value)
				{
					fShowAddress = value;
					AddressTextBox.Visible = value;
					SetControlSize();
				}
			}
		}

		bool fShowAddress = true;

		#endregion

		#region Show Address Drop Edit

		[Category(ZGUIConstants.DesignerCategory), DefaultValue(true)]
		public bool ShowAddressDropEdit
		{
			get { return fShowAddressDropEdit; }
			set
			{
				if (fShowAddressDropEdit != value)
				{
					fShowAddressDropEdit = value;
					AddressDropEdit.Visible = value;
					SetControlSize();
				}
			}
		}

		bool fShowAddressDropEdit = true;

		#endregion

		#region Show Organisation

		[Category(ZGUIConstants.DesignerCategory), DefaultValue(true)]
		public bool ShowOrganisation
		{
			get { return fShowOrganisation; }
			set
			{
				if (fShowOrganisation != value)
				{
					fShowOrganisation = value;
					OrganisationFindBox.Visible = value;
					SetControlSize();
				}
			}
		}

		// if we show a ZBool in the designer it will display "Y" / "N" (ZBool.ToString())
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool ShowOrganisationForBinding
		{
			get { return ShowOrganisation; }
			set { /* keep binding happy (we don't want users setting this via business */ }
		}

		bool fShowOrganisation = true;

		#endregion

		#region Show Organisation Name

		[Category(ZGUIConstants.DesignerCategory), DefaultValue(false)]
		public bool ShowOrganisationName
		{
			get { return this.showOrganisationName; }
			set
			{
				if (this.showOrganisationName != value)
				{
					this.showOrganisationName = value;
					OrganisationFindBox.ShowDescriptionBox = value;
					SetControlSize();
				}
			}
		}

		bool showOrganisationName;

		public void SetOrganisationFindBoxPreBoundMaxLength(int length)
		{
			OrganisationFindBox.PreBoundMaxLength = length;
			SetControlSize();
		}

		#endregion

		#region Set Control Size

		internal void SetControlSize()
		{
			if (AddressDropEdit != null && OrganisationFindBox != null && !isSettingControlSize)
			{
				SuspendLayout();
				isSettingControlSize = true;
				try
				{
					OrganisationFindBox.Size = ShowOrganisationName && !StackControls ? ControlDpiScalingHelper.NewScaledSize(200, 20) : ControlDpiScalingHelper.NewScaledSize(98, 20);

					AddressDropEdit.PreBoundMaxLength = ShowOrganisationName && !StackControls ? 18 : 29;

					if (this.Height != ControlHeight)
					{
						ControlDpiScalingHelper.SetHeight((Control)this, ControlHeight, false);
					}

					if (this.Width < MinimumControlWidth)
					{
						ControlDpiScalingHelper.SetWidth((Control)this, MinimumControlWidth, false);
					}

					ControlDpiScalingHelper.SetLeft(ref AddressDropEdit, AddressDropEditLeft, false);
					ControlDpiScalingHelper.SetTop(ref AddressDropEdit, AddressDropEditTop, false);
					AddressDropEdit.SetControlWidth(AddressDropEdit.Width - AddressStatusButton.Width - DropEditToStatusButtonGap);

					((IVariableLengthCaptionRenderer)AddressStatusButton).IsCaptionOverridden = true;
					ControlDpiScalingHelper.SetLeft(ref AddressStatusButton, AddressDropEdit.Right + ControlDpiScalingHelper.OnePixel, false);
					ControlDpiScalingHelper.SetTop(ref AddressStatusButton, AddressDropEditTop, false);
					ControlDpiScalingHelper.SetHeight(ref AddressStatusButton, AddressDropEdit.Height - ControlDpiScalingHelper.OnePixel, false);

					if (AddressTextBox.Visible)
					{
						if (AddressTextBox.Height != ControlDpiScalingHelper.ScaleToCurrentDpiY(AddressTextHeight))
						{
							ControlDpiScalingHelper.SetHeight(ref AddressTextBox, AddressTextHeight, true);
						}

						if (AddressTextBox.Top != AddressTextTop)
						{
							ControlDpiScalingHelper.SetTop(ref AddressTextBox, AddressTextTop, false);
						}

						if (AddressTextBox.Width < MinimumAddressTextWidth)
						{
							ControlDpiScalingHelper.SetWidth(ref AddressTextBox, MinimumAddressTextWidth, false);
						}

						if (AddressTextBox.Left + AddressTextBox.Width > this.Width)
						{
							ControlDpiScalingHelper.SetWidth(ref AddressTextBox, this.Width - AddressTextBox.Left, false);
						}
					}
					else
					{
						ControlDpiScalingHelper.SetWidth((Control)this, MinimumControlWidth, false);
					}
				}
				finally
				{
					isSettingControlSize = false;
					ResumeLayout();
				}
			}
		}

		bool isSettingControlSize;

		internal const int AddressTextHeight = 26;
		internal const int FindBoxToDropEditGap = 1;
		internal const int DropEditToAddressGap = 12;

		[DpiState(DpiState.ScaleX)]
		internal int DropEditToStatusButtonGap = ControlDpiScalingHelper.OnePixel;

		int AddressDropEditLeft
		{
			get { return ShowOrganisation && !StackControls ? OrganisationFindBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(FindBoxToDropEditGap) : 0; }
		}

		int AddressDropEditTop
		{
			get { return ShowOrganisation && StackControls ? OrganisationFindBox.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(FindBoxToDropEditGap) : 0; }
		}

		int AddressTextTop
		{
			get { return AddressDropEdit.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(DropEditToAddressGap); }
		}

		int MinimumAddressTextWidth
		{
			get { return AddressDropEdit.Right; }
		}

		int MinimumControlWidth
		{
			get
			{
				int result = 0;

				if (ShowOrganisation && ShowAddressDropEdit && !StackControls) // need to include org in width
				{
					result = OrganisationFindBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(FindBoxToDropEditGap) + WidthOfAddressDropEditAndValidationStatusButton;
				}
				else if (ShowOrganisation && !StackControls)
				{
					result = OrganisationFindBox.Width;
				}
				else if (ShowOrganisation)
				{
					result = Math.Max(OrganisationFindBox.Width, WidthOfAddressDropEditAndValidationStatusButton);
				}
				else
				{
					result = WidthOfAddressDropEditAndValidationStatusButton;
				}

				return result;
			}
		}

		int WidthOfAddressDropEditAndValidationStatusButton
		{
			get { return AddressDropEdit.Width + DropEditToStatusButtonGap + AddressStatusButton.Width; }
		}

		int ControlHeight
		{
			get
			{
				int result = 0;

				if (ShowOrganisation && ShowAddressDropEdit && StackControls)
				{
					result = OrganisationFindBox.Height + AddressDropEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(FindBoxToDropEditGap);
				}
				else if (ShowOrganisation && StackControls)
				{
					result = OrganisationFindBox.Height;
				}
				else
				{
					result = Math.Max(OrganisationFindBox.Height, AddressDropEdit.Height);
				}

				if (AddressTextBox.Visible)
				{
					result += ControlDpiScalingHelper.ScaleToCurrentDpiY(DropEditToAddressGap + AddressTextHeight);
				}

				return result;
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			SetControlSize();
		}

		#endregion

		#region IFetchHintGenerator Members
		void IFetchHintGenerator.AddFetchHint(object dataSource, string dataMember)
		{
			if ((string.IsNullOrEmpty(dataMember) || dataMember == BindTo) &&
				!BindTo.Contains(".") && !BindTo.Contains("+"))
			{
				BusinessObject bizO = dataSource as BusinessObject;
				if (bizO != null && !bizO.IsDeleted)
				{
					bizO.Factory.AddFetchHint(ObjectFactory.GetType<IOrgAddress>(), (ZGuid)bizO[BindTo]);
				}
			}
		}
		#endregion

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindToAddress
		{
			get { return BindTo; }
			set { BindTo = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		#region IDataBoundControl Members

		protected override object DataSourceCore
		{ get { return dataSource; } }
		object dataSource;

		protected override string DataMemberCore
		{ get { return dataMember; } }
		string dataMember;

		public override Type DataSourceType
		{
			get { return typeof(ZGuid); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Binding Property Name")]
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			this.dataSource = dataSource;
			this.dataMember = dataMember;

			OrganisationFindBox.SetDataBinding(dataSource, string.IsNullOrEmpty(dataMember) ? "" : dataMember + ZAddress.Schema.OrgPK);
			AddressDropEdit.SetDataBinding(dataSource, string.IsNullOrEmpty(dataMember) ? "" : dataMember + ZAddress.Schema.AddressBindingPK);

			AddressTextBox.DataBindings.RemoveBinding("Text");
			DataBindings.RemoveBinding(nameof(ShowOrganisationForBinding));
			if (dataSource != null)
			{
				AddressTextBox.DataBindings.Add(new KBinding("Text", dataSource, dataMember + ZAddress.Schema.Address));
				DataBindings.Add(new KBinding(nameof(ShowOrganisationForBinding), dataSource, dataMember + ZAddress.Schema.IsOrgVisible));
				UpdateIsOrgVisibleOnDataSource();
			}
			BusinessObjectBindingManager = dataSource == null ? null : BindingContext[dataSource, new KBindingMemberInfo(dataMember).BindingPath];

			DataBoundControl.SetDataBindingForMetadataProperties(this, dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, "");
		}

		BindingManagerBase BusinessObjectBindingManager
		{
			set
			{
				if (businessObjectBindingManager != null)
				{
					businessObjectBindingManager.CurrentChanged -= new EventHandler(ZAddressControl_CurrentChanged);
				}
				businessObjectBindingManager = value;
				if (businessObjectBindingManager != null)
				{
					businessObjectBindingManager.CurrentChanged += new EventHandler(ZAddressControl_CurrentChanged);
				}
			}
		}
		BindingManagerBase businessObjectBindingManager;

		void ZAddressControl_CurrentChanged(object sender, EventArgs e)
		{
			UpdateIsOrgVisibleOnDataSource();
			if (ParentGroupBox != null)
			{
				ParentGroupBox.Invalidate();
			}
		}

		void UpdateIsOrgVisibleOnDataSource()
		{
			Binding binding = DataBindings[nameof(ShowOrganisationForBinding)];
			if (binding != null && binding.BindingManagerBase != null && binding.BindingManagerBase.Position != -1 && binding.BindingManagerBase.Count > 0)
			{
				binding.WriteValue();
			}
		}

		#endregion

		#region INotificationDataMembers Members

		string[] INotificationDataMembers.NotificationDataMembers
		{
			get { return new string[] { DataMember + ZAddress.Schema.OrgPK, DataMember }; }
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

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new PropertyDescriptor[]
			{
				new ControlPropertyDescriptor<ZAddressControl, ZBool>("ShowOrganisationForBinding", ZBool.True, false)
			};
		}

		#endregion

		#region IReadOnlyToggleControl Members

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get
			{
				return OrganisationFindBox.ReadOnly;
			}
			set
			{
				OrganisationFindBox.ReadOnly = value;
				AddressStatusButton.ReadOnly = value;
			}
		}

		#endregion

		/// <summary>
		/// Show an edit or view form the same way as 'F3' shortcut does.
		/// </summary>
		public void ShowEditOrViewForm()
		{
			((IFindBoxUserControl)OrganisationFindBox).ShowEditOrViewForm();
		}

		protected string DetailedAddressForBalloon
		{
			get
			{
				string result = "";

				BindingManagerBase bindingManager = BindingContext[DataSource, new KBindingMemberInfo(DataMember).BindingPath];
				if (bindingManager.Position != -1 && bindingManager.GetCurrent() != null)
				{
					BusinessObject current = (BusinessObject)bindingManager.GetCurrent();
					PropertyDescriptor addressProperty = bindingManager.GetItemProperties()[new KBindingMemberInfo(BindToAddress + ZAddress.Schema.BindingSuffix).BindingField];
					ZAddress address = (ZAddress)addressProperty.GetValue(current);
					result = address.AddressDetailed;
				}

				return result;
			}
		}

		internal ZGuid ParseCode(string code)
		{
			ZGuid result = ZGuid.Invalid;
			if (Parse != null)
			{
				result = Parse(code);
			}
			return result;
		}

		/// <summary>
		/// Parsing the string code which is entered into the address drop edit control into guid pk
		/// </summary>
		public Converter<ZString, ZGuid> Parse;

		#region IZAddressParent Members

		void IZAddressParent.SetControlSize()
		{
			SetControlSize();
		}

		ZGuid IZAddressParent.ParseCode(string code)
		{
			return ParseCode(code);
		}

		bool IZAddressParent.CheckZAddressBindingSuffix => true;

		#endregion

		#region Address Validation

		string CurrentAddressValidationStatus
		{
			get
			{
				var address = AddressDropEdit.Addy as ZAddress;
				if (address != null && address.OrgAddress is IOrgAddress)
				{
					return ((IOrgAddress)address.OrgAddress).ValidationStatus;
				}
				return string.Empty;
			}
		}

		ZGroupBox ParentGroupBox
		{
			get
			{
				return Parent as ZGroupBox;
			}
		}

		void AddressDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshValidationStatusControls();
		}

		void AddressDropEdit_BoundValueCommitted(object sender, EventArgs e)
		{
			RefreshValidationStatusControls();
		}

		void RefreshValidationStatusControls()
		{
			if (!this.ReadOnly)
			{
				var addressValidation = AddressDropEdit.Addy as ISupportWebAddressValidation;
				var isErrorSuppressed =
					addressValidation?.IsErrorSuppressed ??
					false;

				AddressValidationUIHelper.SetButtonValidationStatus(
					AddressStatusButton,
					CurrentAddressValidationStatus,
					isErrorSuppressed);
			}

			if (ParentGroupBox != null)
			{
				ParentGroupBox.Invalidate();
			}
		}

#if DEBUG
		internal
#endif
		void AddressStatusButton_Click(object sender, EventArgs e)
		{
			try
			{
				var address = AddressDropEdit.Addy as ZAddress;
				if (address != null && address.OrgAddress is IOrgAddress && address.OrgHeader != null)
				{
					var controller = ZControllerFactory.Create(ControllerIDs.Organisation);
					var form = ((IOrganisationController)controller).ShowForm(address.OrgHeader, OrganisationTabPages.Address, FormAction.Edit);
					if (form != null)
					{
						form.Closed += form_Closed;
					}
				}
			}
			catch (NullReferenceException ex)
			{
				Environment.Globals.Message.ShowDeveloperException("This should be fixed by WI00170467, please contact the ROPE team. The NullReferenceException was caught from AddressStatusButton_Click().", ex);
			}
		}

		void form_Closed(object sender, EventArgs e)
		{
			RefreshValidationStatusControls();
		}

		#endregion
	}
}
