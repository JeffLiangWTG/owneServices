using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

#if WINZOR
using Graphics = System.Drawing.BGraphics;
#endif

namespace Enterprise.ZArchitecture.GUI.Internal
{
	#region IZAddressDropEdit

	public interface IZAddressDropEdit
	{
		IZAddress Addy { get; }
		bool FilterAddressedByDefaultType { get; set; }
		ZAddressDropButton AddressDropButton { get; }
	}

	#endregion

	[ToolboxItem(false)]
	[SuppressFormDesignerAnalysis]
	[CompositeFieldControl]
	public class ZAddressDropEdit : ZGuidDropEdit, IZAddressDropEdit
	{
		public ZAddressDropEdit()
		{
			Hotkeys.RegisterHotKey(Keys.F3, HandleShowOrganisation, Res.GetString("ce6cceed-4ee2-4adf-83ae-7d100ff45200", "Open organization form"));
		}

		[ToolboxItem(false)]
		public new class Bare : ZAddressDropEdit
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		protected override void SetControlSize(int maxLength)
		{
			base.SetControlSize(maxLength);
			if (ParentAddressControl != null)
			{
				ParentAddressControl.SetControlSize();
			}
		}

		public override UserIdleWorkItemOptions WorkItemOption => UserIdleWorkItemOptions.DisableSlowRunningWarning;

		AddressType DefaultAddressType
		{
			get { return Addy?.DefaultAddressType ?? AddressType.NoDefault; }
		}

		bool ShowAddressTypeFilter
		{
			get { return DefaultAddressType != AddressType.NoDefault; }
		}

		protected override IList GetFilteredListForDropDown()
		{
			return FilterAddressedByDefaultType && ShowAddressTypeFilter ? FilteredList : List;
		}

		CodeDescriptionPairList FilteredList
		{
			get
			{
				if (filteredList == null && base.List != null)
				{
					filteredList = new CodeDescriptionPairList();
					foreach (var addressItem in base.List.OfType<ZAddressItem>())
					{
						if (addressItem.GetCapability(DefaultAddressType.ToString()) != null ||
							((DefaultAddressType == AddressType.DLV || DefaultAddressType == AddressType.PIC) && addressItem.GetCapability(OrgConstants.AddressType.PickupAndDelivery) != null))
						{
							filteredList.Add(addressItem);
						}
					}
				}
				return filteredList;
			}
		}
		CodeDescriptionPairList filteredList;

		protected override void InvalidateListCore()
		{
			base.InvalidateListCore();
			filteredList = null;
		}

		protected override IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return IsBound && Addy != null && !Addy.IsDeleted ? Addy.AddressList : null;
		}

		bool HandleShowOrganisation(object sender, Keys key)
		{
			if (!ReadOnly)
			{
				BusinessObject org = this.Org;
				if (org != null)
				{
					IOrganisationController orgController = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
					orgController.ShowForm(org, OrganisationTabPages.Address, FormAction.Edit);
					return true;
				}
			}

			return false;
		}

		protected override void OnParseValue(ConvertEventArgs e)
		{
			ZString code = e.Value.ToString();
			ZGuid value = GetPK(code);
			IZAddressParent parentAddressControl = this.ParentAddressControl;

			if (!value.IsValid && parentAddressControl != null)
			{
				value = parentAddressControl.ParseCode(code);
			}

			if (!value.IsValid)
			{
				FieldInvalidTextMemory.SetInvalidText(CurrentItem, DataPropertyName, Text);
			}

			e.Value = value;
		}

		#region Implementation

		protected IZAddressParent ParentAddressControl
		{
			get
			{
				var currentParent = Parent;
				while (currentParent != null)
				{
					var iZAddressParent = currentParent as IZAddressParent;
					if (iZAddressParent != null)
					{
						return iZAddressParent;
					}
					currentParent = currentParent.Parent;
				}

				return null;
			}
		}

		internal IZAddress Addy
		{
			get
			{
				var result = BizObj as IZAddress;

				if (result == null)
				{
					var addressControl = ParentAddressControl as IDataBoundControl;
					if (addressControl != null)
					{
						string addressPropertyName = addressControl.DataMember == null ? null : new KBindingMemberInfo(addressControl.DataMember).BindingField;
						if (ParentAddressControl.CheckZAddressBindingSuffix && addressPropertyName != null && !addressPropertyName.EndsWith(ZAddress.Schema.BindingSuffix, StringComparison.InvariantCulture))
						{
							addressPropertyName += ZAddress.Schema.BindingSuffix;
						}
						result = addressPropertyName == null || BizObj == null || BizObj.IsDeleted ? null : (IZAddress)BizObj[addressPropertyName];
					}
				}

				return result;
			}
		}

		IZAddress IZAddressDropEdit.Addy
		{
			get { return Addy; }
		}

		internal BusinessObject BizObj
		{
			get
			{
				if (BindingManager == null)
				{
					return null;
				}

				return BindingManager.Count > 0 ? (BusinessObject)BindingManager.GetCurrent() : null;
			}
		}

		protected internal BusinessObject Org
		{
			get
			{
				BusinessObject result = null;

				if (!Addy?.IsDeleted ?? false)
				{
					result = (BusinessObject)Addy.Factory.Load<IOrgHeader>(Addy.OrgPK);
				}

				return result;
			}
		}

		protected override ZDropButton NewDropButton()
		{
			return new ZAddressDropButton();
		}

		public ZAddressDropButton AddressDropButton => DropButton as ZAddressDropButton;

		public bool FilterAddressedByDefaultType
		{
			get { return filterAddressedByDefaultType; }
			set
			{
				filterAddressedByDefaultType = value;
				UpdateDropDown();
			}
		}
		bool filterAddressedByDefaultType = true;

		#endregion
	}

	#region ZAddressDropButton

	public class ZAddressDropButton : ZDropButton
	{
		public ZAddressDropButton()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				maxDropDownItemsCount = OrganisationsDataRegistry.Instance.DropdownAddressCountThreshold.Value;
			}
		}

		protected override ZDropForm NewDropForm()
		{
			return new ZAddressDropForm(ParentAddressDropEdit);
		}

		readonly int maxDropDownItemsCount;

		protected override void ShowDropDown(bool lButtonDown)
		{
			if (!PopupFormShowed && !ShowPopupFormIfNeeded())
			{
				base.ShowDropDown(lButtonDown);
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			popupForm?.Dispose();
			base.Dispose(isNotFinalizing);
		}

		bool? rawFilterValue;

		internal bool ShouldPopupForm
		{
			get
			{
				var addy = ParentAddressDropEdit.Addy;
				if (addy == null)
				{
					return false;
				}

				var org = addy.Factory.Load<OrgHeader>(addy.OrgPK);
				if (org == null)
				{
					return false;
				}

				if (org != Organisation || rawFilterValue.HasValue && rawFilterValue.Value != ParentAddressDropEdit.FilterAddressedByDefaultType)
				{
					rawFilterValue = ParentAddressDropEdit.FilterAddressedByDefaultType;
					shouldPopupForm = maxDropDownItemsCount < GetFilteredAddressCount(org);

					Organisation = org;

					SetNewPopupForm();
				}
				else if (popupForm != null && popupForm.IsDisposed)
				{
					SetNewPopupForm();
				}

				return shouldPopupForm;
			}
		}

		internal bool PopupFormShowed { get; private set; }

		internal bool ShowPopupFormIfNeeded()
		{
			var popupFormShow = false;

			if (ShouldPopupForm)
			{
				PopupFormShowed = true;
				popupForm.Show(ParentForm);
				ZFormModaliser.EnableForm((ZForm)ParentForm, false);
				popupFormShow = true;
			}

			return popupFormShow;
		}

		int GetFilteredAddressCount(OrgHeader org)
		{
			var query = new ZDBOnlyQuery(typeof(OrgAddress));
			query.AddToFilter(OrgAddressSchema.OA_OH, org.PK);
			query.AddToFilter(OrgAddressSchema.OA_IsActive, ZBool.True);

			if (ParentAddressDropEdit.FilterAddressedByDefaultType)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
				var addressCapabilities = new List<string>(new[] { DefaultAddressType.ToString() });
				if (DefaultAddressType == AddressType.DLV || DefaultAddressType == AddressType.PIC)
				{
					addressCapabilities.Add(nameof(AddressType.PAD));
				}

				subQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, addressCapabilities);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			return org.Factory.GetDatabaseCount(typeof(OrgAddress), query);
		}

		BusinessObject Organisation;

		bool shouldPopupForm;

		void Popup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var address = e.SelectedBusinessObjects[0] as OrgAddress;
			if (address != null)
			{
				ParentDropEdit.OnItemSelected(new ZAddressItem(address.PK, address.OA_Code, address.AddressDescription, null), true);
			}
		}

		IZAddressDropEdit ParentAddressDropEdit => (IZAddressDropEdit)ParentDropEdit;
		AddressType DefaultAddressType => ParentAddressDropEdit.Addy.DefaultAddressType;

		void SetNewPopupForm()
		{
			if (shouldPopupForm)
			{
				AddressType? filterValue;
				if (ParentAddressDropEdit.FilterAddressedByDefaultType)
				{
					filterValue = DefaultAddressType;
				}
				else
				{
					filterValue = null;
				}

				popupForm?.Dispose();
				popupForm = ObjectFactory.Get<EmbeddedModulePopup>("OrgAddressEmbeddedModulePopup", ZFilterModule.GetZFilterModule(ModuleIDs.OrgAddresses), ParentAddressDropEdit.Addy.OrgPK, filterValue);
				popupForm.Selected += Popup_Selected;
				popupForm.FormClosing += PopupForm_FormClosing;
				popupForm.VisibleChanged += PopupForm_VisibleChanged;
			}
		}

		void PopupForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			PopupFormShowed = false;

			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				popupForm.Hide();
			}
		}

		void PopupForm_VisibleChanged(object sender, EventArgs e)
		{
			if (!popupForm.Visible)
			{
				ZFormModaliser.EnableForm((ZForm)ParentForm, true);
				ParentForm?.Activate();
			}
		}

		internal EmbeddedModulePopup popupForm;
	}
	#endregion

	#region ZAddressDropForm

	partial class ZAddressDropForm : ZDropForm
	{
		public ZAddressDropForm(IZAddressDropEdit parentDropEdit) : base((ZDropEdit)parentDropEdit) { }

		protected new IZAddressDropEdit ParentDropEdit
		{
			get { return (IZAddressDropEdit)base.ParentDropEdit; }
		}

		internal protected bool ShowAddressTypeFilter => ParentDropEdit.Addy != null && ParentDropEdit.Addy.DefaultAddressType != AddressType.NoDefault;

		[DpiState(DpiState.Unscaled)]
		protected override int ExtraSpaceHeight
		{
			get { return ShowAddressTypeFilter ? SecondLineNeededForFilterCheckBox ? 35 : 20 : 2; }
		}

		protected override int MinimumWidth
		{
			get { return textWidth + ControlDpiScalingHelper.ScaleToCurrentDpiX(35); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "CW1040", Justification = "Baseline")]
		protected override int VerticalIndent
		{
			get { return base.VerticalIndent + ExtraSpaceHeight; }
		}

		protected override IEnumerable<Rectangle> GetSpecialAreasCore()
		{
			foreach (var rectangle in base.GetSpecialAreasCore())
			{
				yield return rectangle;
			}
			if (ShowAddressTypeFilter)
			{
				yield return AddressTypeFilterRectangle;
			}
		}

		Rectangle AddressTypeFilterRectangle
		{
			get { return ControlDpiScalingHelper.NewScaledRectangle(0, 0, Width, ControlDpiScalingHelper.ScaleToCurrentDpiY(ExtraSpaceHeight + 1), false); }
		}

		protected override bool HandleClickInSpecialArea(Point locationForDropForm)
		{
			var handled = base.HandleClickInSpecialArea(locationForDropForm);

			if (!handled && ShowAddressTypeFilter)
			{
				if (AddressTypeFilterCheckBox.Bounds.Contains(locationForDropForm) && Env.Security.OrgAddressChooseUnmatchingOnJob.IsAllowed)
				{
					if (ParentDropEdit.AddressDropButton == null || !ParentDropEdit.AddressDropButton.PopupFormShowed) // Avoid double click
					{
						ParentDropEdit.FilterAddressedByDefaultType = !ParentDropEdit.FilterAddressedByDefaultType;

						if (ParentDropEdit.AddressDropButton == null || !ParentDropEdit.AddressDropButton.ShowPopupFormIfNeeded())
						{
							SetSizing();
							Invalidate();
#if WINZOR
							NotifyRenderRequired();
#endif
						}
					}

					handled = true;
				}
			}

			return handled;
		}

		public override bool IsItemSelectable(ICodeDescription item)
		{
			return base.IsItemSelectable(item) && item != null;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (ShowAddressTypeFilter)
			{
				PaintFilter(e.Graphics);
			}
		}

		void PaintFilter(Graphics graphics)
		{
			if (Env.Security.OrgAddressChooseUnmatchingOnJob.IsAllowed)
			{
				AddressTypeFilterCheckBox.ReadOnly = false;
			}
			else
			{
				AddressTypeFilterCheckBox.ReadOnly = true;
			}
			AddressTypeFilterCheckBox.Checked = ParentDropEdit.FilterAddressedByDefaultType;

#if !WINZOR
			graphics.FillRectangle(SystemBrushes.ControlLight, AddressTypeFilterRectangle);
			AddressTypeFilterCheckBox.Draw(graphics, AddressTypeFilterRectangle);
			PaintBorder(graphics, AddressTypeFilterRectangle);
#endif
		}

		bool SecondLineNeededForFilterCheckBox
		{
			get
			{
				return !Env.Security.OrgAddressChooseUnmatchingOnJob.IsAllowed;
			}
		}

#if WINZOR
		protected override ZCheckBox ExtraControl => AddressTypeFilterCheckBox;
#endif

		internal protected ZCheckBox AddressTypeFilterCheckBox
		{
			get
			{
				if (addressTypeFilterCheckBox == null && ShowAddressTypeFilter)
				{
					addressTypeFilterCheckBox = new ZCheckBox();
#if WINZOR
					addressTypeFilterCheckBox.Click += (sender, args) => HandleClickInSpecialArea(Point.Empty);
#endif
					var defaultAddressType = ParentDropEdit.Addy?.DefaultAddressType ?? AddressType.NoDefault;
					addressTypeFilterCheckBox.Text = defaultAddressType.ToString();
					if (defaultAddressType == AddressType.DLV || defaultAddressType == AddressType.PIC)
					{
						addressTypeFilterCheckBox.Text += ", " + OrgConstants.AddressType.PickupAndDelivery;
					}

					var orgCodeLists = ObjectFactory.Get<IOrgCodeLists>();
					if (orgCodeLists != null)
					{
						var factory = ParentDropEdit?.Addy?.Factory ?? new BusinessObjectFactory();
						var addressTypeList = orgCodeLists.GetAddressTypesList(factory);
						if (addressTypeList != null)
						{
							var description = addressTypeList.GetDescriptionFromCode(defaultAddressType.ToString());
							if (!string.IsNullOrEmpty(description))
							{
								addressTypeFilterCheckBox.Text += " - " + description;

								if (defaultAddressType == AddressType.DLV || defaultAddressType == AddressType.PIC)
								{
									description = addressTypeList.GetDescriptionFromCode(OrgConstants.AddressType.PickupAndDelivery);
									if (!string.IsNullOrEmpty(description))
									{
										addressTypeFilterCheckBox.Text += ", " + description;
									}
								}
							}
						}

						if (!Env.Security.OrgAddressChooseUnmatchingOnJob.IsAllowed)
						{
							addressTypeFilterCheckBox.Text += "\r\n" + Res.GetString("a0714916-6e65-4ab6-95cd-f053ee5ee1af", "You do not have security access to uncheck filter");
						}
					}
					addressTypeFilterCheckBox.Font = OFont.GetFontBold();
					addressTypeFilterCheckBox.Padding = ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 0); // For drawing on parent graphics shifted from top left corner
					addressTypeFilterCheckBox.Size = addressTypeFilterCheckBox.GetPreferredSize(ControlDpiScalingHelper.NewScaledSize(Width, ControlDpiScalingHelper.ScaleToCurrentDpiY(ExtraSpaceHeight), false));
					using (Graphics g = CreateGraphics())
					{
						textWidth = (int)g.MeasureString(addressTypeFilterCheckBox.Text, addressTypeFilterCheckBox.Font).Width;
					}

					SetSizing();
				}
				return addressTypeFilterCheckBox;
			}
		}
		ZCheckBox addressTypeFilterCheckBox;
		int textWidth;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (addressTypeFilterCheckBox != null)
				{
					addressTypeFilterCheckBox.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}

#endregion
}
