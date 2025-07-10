using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[SuppressFormDesignerAnalysis]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(ZGUIConstants.ButtonGridDesigner)]
	[DefaultMember("DataSource")]
	[SuppressFormsLocalizedTest]
	public partial class ZModuleButtonGrid : ZUserControl, IButtonGrid, IBindingMemberForCompileTimeCheckProvider
	{
		public ZModuleButtonGrid()
		{
			InitializeComponent();

			InnerGrid.MouseDown += InnerGrid_MouseDown;
			InnerGrid.RowsDeleting += InnerGrid_RowDeleting;

			InitializeDefaultCaptions();
			this.Layout += new LayoutEventHandler(ZModuleButtonGrid_Layout);
		}

		#region Layout

		bool boundToDisplayModeChange;
		void ZModuleButtonGrid_Layout(object sender, LayoutEventArgs e)
		{
			if (!boundToDisplayModeChange && Form != null)
			{
				Form.DisplayModeChanged += new DisplayModeChangedEventHandler(Form_DisplayModeChanged);
				boundToDisplayModeChange = true;
				SetButtonsToReadOnlyIfDisplayModeReadOnly();
			}
		}

		void Form_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			SetButtonsToReadOnlyIfDisplayModeReadOnly();
		}

		void SetButtonsToReadOnlyIfDisplayModeReadOnly()
		{
			if (Form != null && Form.DisplayMode == ODisplayMode.ReadOnly)
			{
				SetButtonsReadOnly(true);
			}
		}

		#endregion

#pragma warning disable IDE0001 // Simplify Names
		#region Designable Properties

		[Browsable(true), Category("Design"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.ComponentModel.Description("")]
		[Editor(ZGUIConstants.ColumnStyleEditor, typeof(UITypeEditor))]
		[SuppressWeaklyTypedCollectionMessage]
		public virtual ArrayList ColumnStyles
		{
			get { return InnerGrid.ColumnStyles; }
		}

		[SmartTagVisible(11)]
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("The ModuleID for the attach findbox.")]
		[Editor(typeof(ModuleIDEditor), typeof(UITypeEditor))]
		public ModuleIdentifier ModuleID
		{
			get
			{
				var result = fModuleID;

				if (!ElementsBelongToDifferentModules && (result == null || result == ModuleIDs.NotAssigned) && DataSource != null)
				{
					var list = ZListUserControl.GetList(DataSource, BindToGridList, this, BindToGridList);
					if (list != null)
					{
						result = ZMetaData.GetModuleId(list);
					}
					else
					{
						var dataType =
							DataSource is IBusinessObjectCollection
								? BusinessObjectCollection.GetElementTypeFromCollectionType(DataSource.GetType())
								: DataSource.GetType();
						var type = GetLastTypeFromPath(dataType, BindToGridList);
						if (type != null)
						{
							result = (ModuleIdentifier)CargoWise.ComponentModel.MetaData.GetMetaData(type, ZMetaDataTypes.ModuleId);
						}
					}
				}

				return result;
			}
			set
			{
				if (value != ModuleIDs.NotAssigned)
				{
					BindToListAndModuleIdWarner.ShowOnChangedModuleIDWarning(this);
				}
				fModuleID = value;
			}
		}

		static Type GetLastTypeFromPath(Type root, string bindToList)
		{
			Type current = null;

			var path = bindToList;
			if (!string.IsNullOrEmpty(path))
			{
				var properties = path.Split('.', '+');
				current = root;
				for (var i = 0; i < properties.Length && current != null; i++)
				{
					var info = GetPropertyFromType(current, properties[i]);
					current = info != null ? info.PropertyType : null;
				}
			}

			return current;
		}

		static PropertyInfo GetPropertyFromType(Type type, string propertyName)
		{
			PropertyInfo info = null;

			while (info == null && type != null)
			{
				info = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				type = type.BaseType;
			}

			return info;
		}

		// Do *not* remove this method!
		// .NET will use this method to determine whether or not to serialize the ModuleID property value.
		bool ShouldSerializeModuleID()
		{
			return (ModuleID != ModuleIDs.NotAssigned);
		}

		ModuleIdentifier fModuleID = ModuleIDs.NotAssigned;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindToGridList
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		[SmartTagVisible(13)]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindToList")]
		public string BindToFindBoxList
		{
			get { return fBindToFindBoxList; }
			set { fBindToFindBoxList = value; }
		}
		string fBindToFindBoxList = "";

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("The form must be saved before editing with a popup form.")]
		[DefaultValue(false)]
		public bool AlwaysRequiresSaveBeforeEdit
		{
			get { return fAlwaysRequiresSaveBeforeEdit; }
			set { fAlwaysRequiresSaveBeforeEdit = value; }
		}
		bool fAlwaysRequiresSaveBeforeEdit;

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("Allow creating a new, unattached item that will be automatically attached after the form is saved, or will stay detached if the form is cancelled.")]
		[DefaultValue(false)]
		public bool AllowNewWithoutSaving
		{
			get { return fAllowNewWithoutSaving; }
			set { fAllowNewWithoutSaving = value; }
		}
		bool fAllowNewWithoutSaving;

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("Allow user to attach and detach items when they don't have edit security.")]
		[DefaultValue(false)]
		public bool AllowAttachDetachWithoutEditSecurity
		{
			get { return allowAttachDetachWithoutEditSecurity; }
			set { allowAttachDetachWithoutEditSecurity = value; }
		}
		bool allowAttachDetachWithoutEditSecurity;

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ZGridWithoutColumnStylesSerialisation InnerGrid
		{
			get { return Grid; }
		}

		#region Show / Hide Buttons

		bool fShowNewButton = true;
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("Show or hide the grid's New button.")]
		[DefaultValue(true)]
		public bool ShowNewButton
		{
			get { return fShowNewButton; }
			set
			{
				if (NewButton != null)
				{
					NewButton.Visible = value;
				}

				fShowNewButton = value;
			}
		}

		bool fShowEditButton = true;
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("Show or hide the grid's Edit button.")]
		[DefaultValue(true)]
		public bool ShowEditButton
		{
			get { return fShowEditButton; }
			set
			{
				if (EditButton != null)
				{
					EditButton.Visible = value;
				}

				fShowEditButton = value;
			}
		}

		bool fShowAttachButton = true;
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("Show or hide the grid's Attach button.")]
		[DefaultValue(true)]
		public bool ShowAttachButton
		{
			get { return fShowAttachButton; }
			set
			{
				if (AttachButton != null)
				{
					AttachButton.Visible = value;
				}

				fShowAttachButton = value;
			}
		}

		bool fShowDetachButton = true;
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("Show or hide the grid's Detach button.")]
		[DefaultValue(true)]
		public bool ShowDetachButton
		{
			get { return fShowDetachButton; }
			set
			{
				if (DetachButton != null)
				{
					DetachButton.Visible = value;
				}

				fShowDetachButton = value;
			}
		}

		#endregion

		#region Button Text

		void InitializeDefaultCaptions()
		{
			EditButtonText = DefaultEditButtonText;
			AttachButtonText = DefaultAttachButtonText;
			DetachButtonText = DefaultDetachButtonText;
			NewButtonText = DefaultNewButtonText;
			DetachMessage = DefaultDetachMessage;
			NameOfAGridElement = DefaultNameOfAGridElement;
		}

		ResourceStringData DefaultEditButtonText { get { return Res.GetData("ZModuleButtonGrid|Edit", "Edit"); } }
		ResourceStringData DefaultAttachButtonText { get { return Res.GetData("ZModuleButtonGrid|Attach", "Attach"); } }
		ResourceStringData DefaultDetachButtonText { get { return Res.GetData("ZModuleButtonGrid|Detach", "Detach"); } }
		ResourceStringData DefaultNewButtonText { get { return Res.GetData("ZModuleButtonGrid|New", "New"); } }
		ResourceStringData DefaultViewButtonText { get { return Res.GetData("ZModuleButtonGrid|View", "View"); } }
		ResourceStringData DefaultDetachMessage { get { return Res.GetData("7b2b29c9-40c7-4696-9bf8-ccc92c27bff0", "Are you sure you want to detach the selected records? New records will be deleted."); } }
		ResourceStringData DefaultNameOfAGridElement { get { return Res.GetData("464c4224-9f18-4ede-a36d-5c36fc27df34", "e.g, Order"); } }

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public ResourceStringData EditButtonText
		{
			get { return EditButton != null ? EditButton.CaptionResourceString : null; }
			set
			{
				if (EditButton != null)
				{
					EditButton.CaptionResourceString = value;
				}
			}
		}

		protected virtual bool ShouldSerializeEditButtonText()
		{
			return !EditButtonText.Equals(DefaultEditButtonText);
		}

		void ResetEditButtonText()
		{
			EditButtonText = DefaultEditButtonText;
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public ResourceStringData AttachButtonText
		{
			get { return AttachButton != null ? AttachButton.CaptionResourceString : null; }
			set
			{
				if (AttachButton != null)
				{
					AttachButton.CaptionResourceString = value;
				}
			}
		}

		bool ShouldSerializeAttachButtonText()
		{
			return !AttachButtonText.Equals(DefaultAttachButtonText);
		}

		void ResetAttachButtonText()
		{
			AttachButtonText = DefaultAttachButtonText;
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public ResourceStringData DetachButtonText
		{
			get { return DetachButton != null ? DetachButton.CaptionResourceString : null; }
			set
			{
				if (DetachButton != null)
				{
					DetachButton.CaptionResourceString = value;
				}
			}
		}

		bool ShouldSerializeDetachButtonText()
		{
			return !DetachButtonText.Equals(DefaultDetachButtonText);
		}

		void ResetDetachButtonText()
		{
			DetachButtonText = DefaultDetachButtonText;
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public ResourceStringData NewButtonText
		{
			get { return NewButton != null ? NewButton.CaptionResourceString : null; }
			set
			{
				if (NewButton != null)
				{
					NewButton.CaptionResourceString = value;
				}
			}
		}

		protected virtual bool ShouldSerializeNewButtonText()
		{
			return !NewButtonText.Equals(DefaultNewButtonText);
		}

		void ResetNewButtonText()
		{
			NewButtonText = DefaultNewButtonText;
		}

		#endregion

		#region Messages

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[System.ComponentModel.Description("Confirm message that is shown when a record is detached.")]
		public ResourceStringData DetachMessage
		{
			get;
			set;
		}

		bool ShouldSerializeDetachMessage()
		{
			return !DetachMessage.Equals(DefaultDetachMessage);
		}

		void ResetDetachMessage()
		{
			DetachMessage = DefaultDetachMessage;
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("The name of the things in the grid. e.g, Order or Shipment.")]
		public ResourceStringData NameOfAGridElement
		{
			get;
			set;
		}

		protected virtual bool ShouldSerializeNameOfAGridElement()
		{
			return !NameOfAGridElement.Equals(DefaultNameOfAGridElement);
		}

		void ResetNameOfAGridElement()
		{
			NameOfAGridElement = DefaultNameOfAGridElement;
		}

		#endregion

		#endregion
#pragma warning restore IDE0001 // Simplify Names

		#region Findbox List

		public event EventHandler FindBoxListChanged;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IBusinessObjectCollection FindBoxList
		{
			get { return fFindBoxList; }
			set
			{
				if (fFindBoxList != value)
				{
					if (value == null && string.IsNullOrEmpty(FindBoxListSetToNullStackTrace))
					{
						FindBoxListSetToNullStackTrace = System.Environment.StackTrace;
					}

					fFindBoxList = value;
					OnFindBoxListChanged();
				}
			}
		}

		IBusinessObjectCollection fFindBoxList;

		string FindBoxListSetToNullStackTrace { get; set; }

		void OnFindBoxListChanged()
		{
			if (FindBoxListChanged != null)
			{
				FindBoxListChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Implementation

		#region Buttons

		/// <summary>
		///		Creates action buttons that appear below the grid.
		/// </summary>
		/// <remarks>
		///		The collection can contain default buttons (specify one of <see cref="Buttons"/> value as name of the button) as well as custom buttons.
		///		Buttons should be in order they should be displayed on form. By default, the next buttons are created: <see cref="Buttons.New"/>,
		///		<see cref="Buttons.Edit"/>, <see cref="Buttons.Attach"/>, <see cref="Buttons.Detach"/>.
		/// </remarks>
		/// <returns>
		///		The collection of action buttons to display below the grid.
		/// </returns>
		protected virtual IList<ToolStripItem> CreateButtons()
		{
			return new List<ToolStripItem>()
			{
				CreateButton(Buttons.New, Icons.GetImage(IconTypes.NewButtonRest)),
				CreateButton(Buttons.Edit, Icons.GetImage(IconTypes.EditButtonRest)),
				CreateButton(Buttons.Attach, Icons.GetImage(IconTypes.BlackWhite_Add)),
				CreateButton(Buttons.Detach, Icons.GetImage(IconTypes.BlackWhite_Remove))
			};
		}

		protected ZToolStripButton CreateButton(string name, Image image)
		{
			return new ZToolStripButton
			{
				Name = name,
				Image = image,
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Padding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0)
			};
		}

		protected ZToolStrip toolStrip;

		protected virtual void InitializeButtons()
		{
			var buttons = CreateButtons();
			if (buttons.Any())
			{
				toolStrip = new ZToolStrip
				{
					Name = "toolStrip",
					GripStyle = ToolStripGripStyle.Hidden,
					Anchor = AnchorStyles.Right,
					TabStop = false,
					BackColor = Color.Transparent
				};
				mainLayoutPanel.Controls.Add(toolStrip, 0, 1);

				foreach (var button in buttons)
				{
					button.Click += GetDefaultButtonHandler(button.Name);
					toolStrip.Items.Add(button);
				}
			}
		}

		EventHandler GetDefaultButtonHandler(string part)
		{
			EventHandler handler;

			if (part == Buttons.New)
			{
				handler = NewButton_Click;
			}
			else if (part == Buttons.Edit)
			{
				handler = EditButton_Click;
			}
			else if (part == Buttons.Attach)
			{
				handler = AttachButton_Click;
			}
			else if (part == Buttons.Detach)
			{
				handler = DetachButton_Click;
			}
			else
			{
				handler = (s, e) => { };
			}

			return handler;
		}

		protected ZToolStripButton FindToolStripButton(string buttonName)
		{
			return toolStrip?.Items.Find(buttonName, true).FirstOrDefault() as ZToolStripButton;
		}

		public static class Buttons
		{
			public static string Attach = "AttachButton";
			public static string Detach = "DetachButton";
			public static string New = "NewButton";
			public static string Edit = "EditButton";
		}

		public static string LayoutControlName = "mainLayoutPanel";

		#endregion

		protected virtual ZGridWithoutColumnStylesSerialisation CreateNewGrid()
		{
			return new ZGridWithoutColumnStylesSerialisation();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ReadOnly
		{
			get { return Grid.ReadOnly; }
			set { Grid.ReadOnly = value; }
		}

		#region ZModule Actions

		IZForm lastShownZForm;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IZForm LastShownZForm
		{
			get
			{
				return lastShownZForm;
			}
			set
			{
				lastShownZForm = value;
			}
		}

		public void SelectFirstRowIfOnlyRowInGrid()
		{
			if (Grid.List != null && Grid.List.Count == 1)
			{
				Grid.Select(0);
			}
		}

		#region New

#if DEBUG
		internal
#endif
		ZToolStripButton NewButton
		{
			get { return FindToolStripButton(Buttons.New); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
		public void FireNewButtonClick()
		{
			if (!NeedsSaveToShowNewForm)
			{
				ShowNewForm();
			}
			else if (AllowNewWithoutSaving)
			{
				ShowNewForm();
			}
			else if (ShowConfirmationForSaveBeforeEditAndNew() == DialogResult.Yes)
			{
				if (Form.FireSaveButton(NewButton) == ContinueWithSave.Yes)
				{
					if (!DataSource.HasErrors() && !NeedsSaveToShowNewForm)
					{
						ShowNewForm();
					}
					else if (!DataSource.HasErrors() && NeedsSaveToShowNewForm)
					{
						var message = string.Format("Can't show New form for: {0} from button grid. Probably Collection's Master.IsInDatabase is returning False.", NameOfAGridElement.Caption);
						message += GenerateDebugInfoAboutCollection();
						Globals.Message.ShowDeveloperErrorOnce("CantOpenNewForm" + message, message, Res.GetString("00733b50-1a4d-4aa8-a598-b35266a67a0c", "Can't open form"));
					}
				}
			}
		}

		public event ModuleButtonGridOperationCancelEventHandler CreatingNew;

		protected void OnCreatingNew(ModuleButtonGridOperationCancelEventArgs args)
		{
			if (CreatingNew != null)
			{
				CreatingNew(this, args);
			}
		}

		protected virtual void NewButton_Click(object sender, EventArgs e)
		{
			var args = new ModuleButtonGridOperationCancelEventArgs(null);
			OnCreatingNew(args);
			if (!args.Cancel)
			{
				FireNewButtonClick();
			}
		}

		void ShowNewForm()
		{
			var controller = GetNewController(null);
			controller.SetCollectionForDefaultsAndValidation(FindBoxList);
			if (FindBoxList != null && FindBoxList.Factory != null)
			{
				controller.AddAdditionalDomainValidationGroups(FindBoxList.Factory.Validation);
			}

			var newForm = ShowNewFormFromController(controller) as ZForm;
			if (newForm != null)
			{
				var weakReference = new WeakReference(newForm);
				FormsReferenced.Add(weakReference, weakReference);

				newForm.Saved += NewForm_Saved;

				// Awaiting DisplayMode refactor
				newForm.DisableNewAction();
			}
		}

#if DEBUG
		protected
#endif
 IZForm ShowNewFormFromController(ZController controller)
		{
			var businessEntity = GetNewBusinessEntity(controller);

			if (businessEntity != null)
			{
				SetupNewElementOnCollection(businessEntity);
				LastShownZForm = ShowFormForNewEntity(businessEntity, controller);
				return LastShownZForm;
			}
			else
			{
				return null;
			}
		}

		protected virtual IBusiness GetNewBusinessEntity(ZController controller)
		{
			return controller.GetNewBusinessEntityInLocalFactoryInternal();
		}

		protected virtual IZForm ShowFormForNewEntity(IBusiness businessEntity, ZController controller)
		{
			return controller.ShowFormForNewEntity(businessEntity);
		}

		void SetupNewElementOnCollection(IBusiness businessEntity)
		{
			try
			{
				var topLevelDataSource = DataSource as BusinessObject;
				var isInDatabase = (topLevelDataSource != null && topLevelDataSource.IsInDatabase);
				Collection.SetupNewElementButDoNotAddIt((BusinessObject)businessEntity, isInDatabase);
			}
			catch (Exception e)
			{
				throw new Exception("Cannot set default values & relationships in BusinessObject for new form.", e);
			}
		}

		#region Weak References to NewForms

		HybridDictionary FormsReferenced
		{
			get
			{
				if (fFormsReferenced == null)
				{
					fFormsReferenced = new HybridDictionary();
				}
				return fFormsReferenced;
			}
		}

		bool HasWeakReferencesToNewForms
		{
			get { return fFormsReferenced != null; }
		}

		HybridDictionary fFormsReferenced;

		#endregion

		protected virtual void NewForm_Saved(object sender, EventArgs e)
		{
			var savedForm = (ZForm)sender;

			var pk = savedForm.BusinessEntity.Identifier;
			var objectToAdd = Collection.Factory.Load(Collection.GetTypeOfElementsFromPK(pk), pk);

			if (objectToAdd != null && ShouldAddObjectToCollection(objectToAdd))
			{
				var legacyCollection = Collection as BusinessObjectCollection;
				var activeCollection = Collection as IActiveBusinessObjectCollection;

				if (legacyCollection != null)
				{
					legacyCollection.Add(objectToAdd);
					((ILegacyBusinessObjectCollectionInternals)legacyCollection).SetCollectionRelationships(objectToAdd);
				}
				else if (activeCollection != null && activeCollection.Relationship.SupportsAddToRelationship())
				{
					activeCollection.Add(objectToAdd);
				}

				DataSource.HasChanges = true;
			}

			savedForm.Saved -= new EventHandler(NewForm_Saved);
		}

		protected virtual bool ShouldAddObjectToCollection(BusinessObject objectToAdd)
		{
			return true;
		}

		DialogResult ShowConfirmationForSaveBeforeEditAndNew()
		{
			var caption = Res.GetString("3fdebcf1-6f51-49e2-9e15-318c710fe314", "Save Confirmation");
			var message = Res.GetString("c47edacf-4a7f-4147-b0b9-26801cb1501f", "The form must be saved before a {0} can be edited or a new {1} can be created. Do you wish to save the form?", NameOfAGridElement.Caption, NameOfAGridElement.Caption);

			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);
		}

		#endregion

		#region Edit

#if DEBUG
		internal
#endif
		ZToolStripButton EditButton
		{
			get { return FindToolStripButton(Buttons.Edit); }
		}

		public event EventHandler EditFormClosed;

		protected virtual void EditButton_Click(object sender, EventArgs e)
		{
			SelectFirstRowIfOnlyRowInGrid();
			if (InnerGrid.SelectedElements.Length == 0)
			{
				ShowNotSelectedMessage();
			}
			else
			{
				foreach (var obj in InnerGrid.SelectedElements)
				{
					Edit(obj, sender);
				}
			}
		}

		// double click
		void InnerGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks > 1 && AllowDoubleClick)
			{
				var hitInfo = InnerGrid.HitTest(e.X, e.Y);
				if (hitInfo.Row >= 0 && hitInfo.Row < Grid.ListManager.List.Count) // valid row clicked on
				{
					Edit((BusinessObject)Grid.ListManager.List[hitInfo.Row], sender);
				}
			}
		}

		protected virtual bool AllowDoubleClick => true;

		public event ModuleButtonGridOperationCancelEventHandler Editing;

		protected void OnEditing(ModuleButtonGridOperationCancelEventArgs args)
		{
			if (Editing != null)
			{
				Editing(this, args);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Exception Text")]
		protected virtual void Edit(BusinessObject selected, object sender)
		{
			var cannotEdit =
				(selected == null) ||
				(!selected.HasChanges && !selected.IsInDatabase && !(selected is NonPersistentBusinessObject));

			if (cannotEdit)
			{
				ShowNotSelectedMessage();
			}
			else
			{
				var args = new ModuleButtonGridOperationCancelEventArgs(selected);
				OnEditing(args);

				if (!args.Cancel)
				{
					var objectToEdit = GetObjectToEdit(selected);
					if (objectToEdit == null)
					{
						ShowCantEditMessage();
					}
					var i = Collection.IndexOf(selected);
					if (i != -1)
					{
						((ICancelAddNew)Collection).EndNew(i); // Ensure the element is committed prior to saving.
					}

					var legacyGridCollection = Grid.List as BusinessObjectCollection;
					if (legacyGridCollection != null)
					{
						((IBusinessObjectCollectionInternals)legacyGridCollection).FireListResetEvent();
					}

					if (!NeedsSaveToShowEditForm(objectToEdit))
					{
						ShowEditForm(objectToEdit);
					}
					else
					{
						if (ShowConfirmationForSaveBeforeEditAndNew() == DialogResult.Yes)
						{
							var taskCollection = DataSource.Children?.OfType<IProcessTaskCollection>()?.FirstOrDefault() as BusinessObjectCollection as IBusiness;
							var bizosCaptured = selected == objectToEdit ? new IBusiness[] { selected, DataSource, taskCollection } : new IBusiness[] { selected, DataSource, objectToEdit, taskCollection };

							var continueValidate = false;
							var form = Form; // Find Form
							var originalDebugInfo = GenerateDebugInfoAboutBizO(GetObjectToEdit(selected)); // for Exception E00000862-C0N-ALL - remove when fixed
							var stackCaptureDuringSave = new Dictionary<object, StackTrace>();
							using (CaptureHasChangesStack(stackCaptureDuringSave, bizosCaptured))
							{
								if (form != null && form.FireSaveButton(sender) == ContinueWithSave.Yes)
								{
									continueValidate = true;
								}
							}

							if (continueValidate)
							{
								var debugInfoAfterSaved = GenerateDebugInfoAboutBizO(GetObjectToEdit(selected));
								var stackCaptureAfterSave = new Dictionary<object, StackTrace>();
								var factory = selected.Factory;
								var genericBusinessContext = factory != null
									? ObjectFactory.Get<IGenericBusinessContextProvider>().GetInstance<BusinessContext>(factory)
									: null;
								using (CaptureHasChangesStack(stackCaptureAfterSave, bizosCaptured))
								{
									if (DataSource == null || DataSource.HasErrors())
									{
										return;
									}

									if (NeedsSaveToShowEditForm(objectToEdit))
									{
										var selectedNeedsSaving = selected != null && (!selected.IsInDatabase || selected.HasChanges) && !(selected is NonPersistentBusinessObject);
										var masterNeedsSaving = !((IBusinessObjectCollectionInternals)Collection).MastersAreInDatabase || (AlwaysRequiresSaveBeforeEdit && DataSource.HasChanges);
										var key = "CantOpenEditForm_" + form.GetType().Name + "_3";
										var message = string.Format(CultureInfo.InvariantCulture, "Can't show Edit form for: {0} from button grid. Probably Collection's Master.IsInDatabase is returning False.", NameOfAGridElement.Caption);
										message += "\r\nDebug info before save : " + originalDebugInfo;
										message += "\r\n\r\nDebug info after save : " + debugInfoAfterSaved;
										message += "\r\n\r\nDebug info after HasErrors() : " + GenerateDebugInfoAboutBizO(GetObjectToEdit(selected));
										message += "\r\nselectedNeedsSaving:" + selectedNeedsSaving;
										message += "\r\nmasterNeedsSaving:" + masterNeedsSaving;
										message += "\r\nselected is in Database:" + selected.IsInDatabase;
										message += "\r\nselected has changes:" + selected.HasChanges;
										message += "\r\nDataSource has changes:" + DataSource.HasChanges;
										message += "\r\nMasters are in database:" + ((IBusinessObjectCollectionInternals)Collection).MastersAreInDatabase;
										message += "\r\nAlways requires save before edit:" + AlwaysRequiresSaveBeforeEdit;
										message += "\r\n" + FormatStackTraces(stackCaptureDuringSave);
										message += "\r\n" + FormatStackTraces(stackCaptureAfterSave, true);
										Globals.Message.ShowDeveloperErrorOnce(key, message, "Can't open form");
										return;
									}
								}

								ShowEditForm(objectToEdit);
							}
						}
					}
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		string FormatStackTraces(Dictionary<object, StackTrace> stacks, bool isFormSaved = false)
		{
			var sb = new StringBuilder();
			if (isFormSaved)
			{
				sb.AppendLine("\r\nStack traces for business objects who's HasChanges was set to true after form fires save button:");
			}
			else
			{
				sb.AppendLine("\r\nStack traces for business objects who's HasChanges was set to true during form fires save button:");
			}
			if (stacks.Count == 0)
			{
				sb.AppendLine("No stack traces were recorded. Either HasChanges was never false, or the object that caused the change was not being recorded.");
			}
			else
			{
				var i = 0;
				foreach (var stack in stacks)
				{
					sb.AppendLine("Stack #" + FormattableString.Invariant($"{++i}"));
					sb.AppendLine("Type: " + stack.Key.GetType().FullName);
					sb.AppendLine(stack.Value.ToString());
				}
			}

			return sb.ToString();
		}

		IDisposable CaptureHasChangesStack(Dictionary<object, StackTrace> collector, IBusiness[] objects)
		{
			void AddStackIfChanged(object sender, HasChangesChangedEventArgs e)
			{
				if (e.ObjectJustWasChanged)
				{
					collector[sender] = new StackTrace();
				}
			}

			objects.WhereNotNull().ForEach(bizo => bizo.HasChangesChanged += AddStackIfChanged);

			return new DisposableAction(() => objects.WhereNotNull().ForEach(bizo => bizo.HasChangesChanged -= AddStackIfChanged));
		}

		#endregion

		protected virtual BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			return selected;
		}

		protected virtual bool AllowOpenInEditFormEvenIfListIsReadOnly
		{
			get { return false; }
		}

		protected virtual bool AlwaysShowFormInReadOnlyMode => false;

		protected virtual void ShowEditForm(BusinessObject selected)
		{
			var controller = GetNewController(selected);
			controller.SetCollectionForDefaultsAndValidation(FindBoxList);
			SetControllerModuleResultsBusinessObject(controller);

			if (FindBoxList != null && FindBoxList.Factory != null)
			{
				controller.AddAdditionalDomainValidationGroups(FindBoxList.Factory.Validation);
			}
			var editableSelected = ConvertSelectedObjectToEditableObjectForModule(selected);
			if (editableSelected == null)
			{
				ShowCantEditMessage();
				return;
			}
			if (!AlwaysShowFormInReadOnlyMode && ((!IsListReadOnly && !ButtonsReadOnly) || AllowOpenInEditFormEvenIfListIsReadOnly))
			{
				controller.ShowEditForm(editableSelected);
			}
			else
			{
				controller.ShowViewForm(editableSelected);
			}

			if (controller.LastShownForm != Form && !businessObjectsOpenForEdit.Contains(selected))
			{
				var lastShownForm = controller.LastShownForm;

				if (lastShownForm != null && !((Form)controller.LastShownForm).IsDisposed)
				{
					((ZForm)controller.LastShownForm).DisableNewAction();

					FormToBusinessObjectMap[lastShownForm] = new ElementReadOnlyPair(selected, selected.ReadOnly);
					businessObjectsOpenForEdit.Add(selected);
					((ZForm)lastShownForm).Closed += new EventHandler(EditForm_Closed);
					if (!InnerGrid.ReadOnly && !selected.ReadOnly)
					{
						selected.ReadOnly = true;
					}

					lastShownZForm = lastShownForm;
				}
			}
		}

		void SetControllerModuleResultsBusinessObject(ZController controller)
		{
			var pKs = new List<PKData>();
			foreach (BusinessObject bizo in Collection)
			{
				var provider = bizo as IPKDataProvider;
				pKs.Add(provider == null ? new PKData() { PK = bizo.PK } : provider.GetPKData());
			}

			controller.ModuleResultsPKCollection = new ZPKCollection(pKs);
		}

		protected virtual BusinessObject ConvertSelectedObjectToEditableObjectForModule(BusinessObject originalBizO)
		{
			return originalBizO;
		}

		class ElementReadOnlyPair
		{
			public ElementReadOnlyPair(BusinessObject bizO, bool readOnly)
			{
				this.BizO = bizO;
				this.ReadOnly = readOnly;
			}

			public readonly BusinessObject BizO;
			public readonly bool ReadOnly;
		}

		readonly Hashtable FormToBusinessObjectMap = new Hashtable();
		readonly ArrayList businessObjectsOpenForEdit = new ArrayList();
		void EditForm_Closed(object sender, EventArgs e)
		{
			OnEditFormClosed(sender, e);
		}

		[SuppressWeaklyTypedCollectionMessage]
		public ArrayList BusinessObjectsOpenForEdit
		{
			get { return businessObjectsOpenForEdit; }
		}

		protected virtual void OnEditFormClosed(object sender, EventArgs e)
		{
			var form = sender as ZForm;

			if (form != null)
			{
				form.Closed -= new EventHandler(EditForm_Closed);
				var readOnlyInfo = FormToBusinessObjectMap[form] as ElementReadOnlyPair;
				if (readOnlyInfo != null)
				{
					readOnlyInfo.BizO.ReadOnly = readOnlyInfo.ReadOnly;
					FormToBusinessObjectMap.Remove(form);
					businessObjectsOpenForEdit.Remove(readOnlyInfo.BizO);
				}
			}

			if (EditFormClosed != null)
			{
				EditFormClosed(this, EventArgs.Empty);
			}
		}

		protected virtual void InnerGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			var controller = GetNewControllerSafe(null);
			RowDeleteSecurityRightCheckStrategy.ProcessDeleteRequest(e, controller);

			if (!e.Cancel)
			{
				foreach (var toBeDeleted in e.Objects)
				{
					if (businessObjectsOpenForEdit.Contains(toBeDeleted))
					{
						e.Cancel = true;
						Globals.Message.ShowError(Res.GetString("0c319dfd-3ef2-4fe0-a456-d673869646ba", "Sorry, you cannot delete the selected {0}(s) as one or more of them are being edited on another form.", NameOfAGridElement.Caption));
						break;
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IGridRowDeleteSecurityRightCheckStrategy RowDeleteSecurityRightCheckStrategy
		{
			get
			{
				if (rowDeleteSecurityRightCheckStrategy == null)
				{
					rowDeleteSecurityRightCheckStrategy = InnerGrid.RemoveAction == RemoveAction.Remove ? new GridRowRemoveSecurityRightCheckDefaultStrategy() : new GridRowDeleteSecurityRightCheckDefaultStrategy();
				}
				return rowDeleteSecurityRightCheckStrategy;
			}
			set
			{
				rowDeleteSecurityRightCheckStrategy = value;
			}
		}
		IGridRowDeleteSecurityRightCheckStrategy rowDeleteSecurityRightCheckStrategy;

		#endregion

		#region Attach

#if DEBUG
		internal
#endif
		ZToolStripButton AttachButton
		{
			get { return FindToolStripButton(Buttons.Attach); }
		}

		public event ModuleButtonGridOperationCancelEventHandler Attaching;

		protected void OnAttaching(ModuleButtonGridOperationCancelEventArgs args)
		{
			if (Attaching != null)
			{
				Attaching(this, args);
			}
		}

		public event EventHandler<ModuleButtonGridOnAttachEventArgs> OnAttach;

		void Attacher_OnAttach(object sender, ModuleButtonGridOnAttachEventArgs eventArgs)
		{
			if (OnAttach != null)
			{
				OnAttach(this, eventArgs);
			}
		}

		protected virtual void AttachButton_Click(object sender, EventArgs e)
		{
			var args = new ModuleButtonGridOperationCancelEventArgs(null);
			OnAttaching(args);
			if (!args.Cancel)
			{
				if (!IsAttachAllowed())
				{
					return;
				}

				// we can get to attach button by right clicking a row gutter, which 'makes' an unedited new row. 
				// immediately dumping stuff into it confuses the underlying datagrid. so we need to signal that we want to get rid of any such row first.
				if (Grid.ListManager != null)
				{
					Grid.ListManager.EndCurrentEdit();
				}

				if (FindBoxList == null && LogErrorIfFindBoxListNull)
				{
					ErrorReporter.ReportOnce($@"FindBoxList is null when AttachButton clicked.
TopLevelControl: {TopLevelControl?.Name},
ZModuleButtonGridType: {GetType().Name},
DataSource: {DataSource?.GetType().Name},
BindingMember: {BindToFindBoxList},
FindBoxListSetToNullStackTrace: {FindBoxListSetToNullStackTrace}");
				}

				var attacher = GetNewRecordAttacher(Collection, FindBoxList, ModuleID);
				SetupRecordAtacher(attacher);
				attacher.OnAttach += new EventHandler<ModuleButtonGridOnAttachEventArgs>(Attacher_OnAttach);
				attacher.Show(Form);

#if DEBUG
				LastShownAttachPopupForTesting = attacher.LastShownAttachPopupForTesting;
#endif
			}
		}

		protected virtual bool IsAttachAllowed()
		{
			var controller = GetNewControllerSafe(null);
			if (controller != null && !(FindForm() is ICanAttachWithoutSecurity))
			{
				if (!AllowAttachDetachWithoutEditSecurity)
				{
					var checkPointForEdit = controller.GetCheckPointForEdit(null);
					if (checkPointForEdit != null && !checkPointForEdit.IsAllowed)
					{
						checkPointForEdit.ShowError();
						return false;
					}
				}

				var checkPointForView = controller.GetCheckPointForView(null);
				if (checkPointForView != null && !checkPointForView.IsAllowed)
				{
					checkPointForView.ShowError();
					return false;
				}
			}

			return true;
		}

		protected virtual bool LogErrorIfFindBoxListNull => true;

#if DEBUG
		public EmbeddedModulePopup LastShownAttachPopupForTesting;
		public ZToolStripButton AttachButtonForTest => AttachButton;
		public ZToolStripButton DetachButtonForTest => DetachButton;
		public ZToolStripButton EditButtonForTest => EditButton;
#endif

		protected virtual IModuleDecisionProvider GetNewModuleDecisionProvider(IFindBox findBox)
		{
			return new ButtonGridModuleDecisionProvider(findBox);
		}

		protected virtual ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new ZRecordAttacher(destinationCollection, findBoxList, moduleID);
		}

		protected virtual void SetupRecordAtacher(ZRecordAttacher attacher)
		{
			attacher.ModuleDecisionProvider = GetNewModuleDecisionProvider(attacher);
			attacher.NameOfAnElementInDestinationCollection = NameOfAGridElement.Caption;
		}

		#endregion

		#region Detach

#if DEBUG
		internal
#endif
		ZToolStripButton DetachButton
		{
			get { return FindToolStripButton(Buttons.Detach); }
		}

		public void DetachSelectedElement()
		{
			DetachButton_Click(this, EventArgs.Empty);
		}

		protected virtual void DetachButton_Click(object sender, EventArgs e)
		{
			SelectFirstRowIfOnlyRowInGrid();

			var selected = InnerGrid.SelectedElements;
			if (selected.Length == 0)
			{
				ShowNotSelectedMessage();
			}
			else if (IsDetachAllowed())
			{
				var (editingItems, newItems, changedItems) = FilterSelectedElements(selected);
				var detachedBusinessObjects = new List<BusinessObject>();

				var beforeDetachEventArgs = new ModuleButtonGridBeforeDetachEventArgs(selected.Distinct().ToArray());
				BeforeDetaching(beforeDetachEventArgs);

				if (beforeDetachEventArgs.Cancel)
				{
					return;
				}

				if (editingItems.IsEmpty)
				{
					var reasonNotToBeAbleToDetach = new Dictionary<string, bool>();
					var warningForDetach = new Dictionary<string, bool>();

					foreach (var selectedObject in selected)
					{
						if (!selectedObject.CanDetach)
						{
							var reason = selectedObject.ReasonNotToBeAbleToDetach;
							if (!reasonNotToBeAbleToDetach.ContainsKey(reason))
							{
								reasonNotToBeAbleToDetach.Add(reason, true);
							}
						}
						else
						{
							var warning = selectedObject.GetWarningBeforeBeingDetached();
							if (!string.IsNullOrEmpty(warning) && !warningForDetach.ContainsKey(warning))
							{
								warningForDetach.Add(warning, true);
							}
						}
					}

					if (reasonNotToBeAbleToDetach.Count > 0)
					{
						Globals.Message.ShowError(Res.GetString("4D3390FD-E161-4538-B738-0B6C452F82C4", "{0}", new ZStringBuilder(reasonNotToBeAbleToDetach.Keys).ToString()), Res.GetString("7873D814-AC76-4E5C-AC28-3D53744E9F6E", "Cannot Detach"));
						return;
					}

					var result = DialogResult.Yes;
					if (DetachMessage != null)
					{
						var detachWarningMessage = warningForDetach.Count > 0 ? new ZStringBuilder(warningForDetach.Keys).ToString() : string.Empty;
						var detachMessage = string.IsNullOrEmpty(detachWarningMessage) ? DetachMessage.Caption : DetachMessage.Caption + "\r\n" + detachWarningMessage;
						result = ConfirmDetach(detachMessage);
					}

					var errors = ZString.Empty;
					if (result == DialogResult.Yes)
					{
						var detahRangeObjects = new List<BusinessObject>();

						foreach (var selectedObject in selected)
						{
							if (!businessObjectsOpenForEdit.Contains(selectedObject))
							{
								try
								{
									if (newItems.Contains(selectedObject.PK))
									{
										selectedObject.Delete();
									}
									else if (changedItems.Contains(selectedObject.PK))
									{
										if (!IsDetachRangeEnabled)
										{
											Detach(selectedObject);
										}
										else
										{
											detahRangeObjects.Add(selectedObject);
										}
										selectedObject.CancelChanges();
										(selectedObject as IBusinessObjectState).ClearHasChangesIncludingChildren();
										detachedBusinessObjects.Add(selectedObject);
									}
									else
									{
										if (!IsDetachRangeEnabled)
										{
											Detach(selectedObject);
										}
										else
										{
											detahRangeObjects.Add(selectedObject);
										}
										detachedBusinessObjects.Add(selectedObject);
									}
									DataSource.HasChanges = true;
								}
								catch (CannotDeleteException e1)
								{
									errors += System.Environment.NewLine + selectedObject.HumanReadableName + ": " + e1.Message;
								}
							}
						}
						if(IsDetachRangeEnabled)
						{
							DetachRange(detahRangeObjects.Distinct().ToArray());
						}
					}

					ShowDeleteError(errors);
				}

				ShowEditingError(editingItems);
				OnDetached(new ModuleButtonGridOnDetachedEventArgs(detachedBusinessObjects.Distinct().ToArray()));
			}
		}

		protected virtual bool IsDetachAllowed()
		{
			if (!AllowAttachDetachWithoutEditSecurity)
			{
				var controller = GetNewControllerSafe(null);
				if (controller != null)
				{
					var checkPointForEdit = controller.GetCheckPointForEdit(null);
					if (!(FindForm() is ICanAttachWithoutSecurity) && checkPointForEdit != null && !checkPointForEdit.IsAllowed)
					{
						checkPointForEdit.ShowError();
						return false;
					}
				}
			}

			return true;
		}

		protected virtual bool IsDetachRangeEnabled => false;

		protected (ZString, ZGuid[], ZGuid[]) FilterSelectedElements(BusinessObject[] selected)
		{
			var editingItems = ZString.Empty;
			var newItems = new List<ZGuid>();
			var changedItems = new List<ZGuid>();

			foreach (var selectedObject in selected)
			{
				if (businessObjectsOpenForEdit.Contains(selectedObject))
				{
					editingItems += "\r\n" + selectedObject.HumanReadableName;
				}
				else if (!selectedObject.IsInDatabase)
				{
					newItems.Add(selectedObject.PK);
				}
				else if (selectedObject.HasChanges)
				{
					changedItems.Add(selectedObject.PK);
				}
			}

			return (editingItems, newItems.ToArray(), changedItems.ToArray());
		}

		protected static void ShowDeleteError(ZString errors)
		{
			if (errors != ZString.Empty)
			{
				Globals.Message.ShowError(errors, Res.GetString("7382cd68-3142-41ac-a5ad-f0ec467d4671", "Delete Error"));
			}
		}

		protected void ShowEditingError(ZString editingItems)
		{
			var messageToShow = ZString.Empty;
			if (editingItems.Length != 0)
			{
				messageToShow += Res.GetString("331c4811-a6ea-471a-8572-826fdd25048a", "Sorry, you cannot detach {0} items that are being edited on another form.{1}", NameOfAGridElement.Caption, editingItems);
				messageToShow += "\r\n";
			}
			if (!messageToShow.IsEmpty)
			{
				Globals.Message.ShowError(messageToShow, Res.GetString("840e4d92-8c21-4aa5-9167-cfcc61c92760", "Cannot detach"));
			}
		}

		public event ModuleButtonGridOperationCancelEventHandler Detaching;

		protected void OnDetaching(ModuleButtonGridOperationCancelEventArgs args)
		{
			if (Detaching != null)
			{
				Detaching(this, args);
			}
		}

		public event EventHandler<ModuleButtonGridOnDetachedEventArgs> Detached;

		protected void OnDetached(ModuleButtonGridOnDetachedEventArgs eventArgs)
		{
			if (Detached != null)
			{
				Detached(this, eventArgs);
			}
		}

		protected virtual void Detach(BusinessObject selected)
		{
			var args = new ModuleButtonGridOperationCancelEventArgs(selected);
			OnDetaching(args);
			if (!args.Cancel)
			{
				Collection.RemoveFromRelationship(selected);
				RemoveWeakReferences(selected);
			}
		}

		protected virtual void DetachRange(IEnumerable selectedObjects)
		{
			foreach (BusinessObject selectedObject in selectedObjects)
			{
				Detach(selectedObject);
			}
		}

		protected void RemoveWeakReferences(BusinessObject selected)
		{
			if (HasWeakReferencesToNewForms)
			{
				foreach (WeakReference weakRef in FormsReferenced.Values)
				{
					if (weakRef.Target is ZForm newForm && newForm.BusinessEntity != null && newForm.BusinessEntity.Identifier == selected.PK)
					{
						newForm.Saved -= new EventHandler(NewForm_Saved);
					}
				}
			}
		}

		public event EventHandler<ModuleButtonGridBeforeDetachEventArgs> BeforeDetach;

		protected void BeforeDetaching(ModuleButtonGridBeforeDetachEventArgs eventArgs)
		{
			if (BeforeDetach != null)
			{
				BeforeDetach(this, eventArgs);
			}
		}

		internal virtual DialogResult ConfirmDetach(string message)
		{
			return Globals.Message.Show(Res.GetString("b7db4fb8-9789-4215-9ce3-f0bc3a70474a", "{0}\r\nAll unsaved changes in detached items will be canceled.", message), Res.GetString("d117a42d-abf1-4e0c-ae3c-a7cd75187d98", "Confirm Detach..."), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
		}

		#endregion

		#region Implementation

		ZController GetNewController(BusinessObject selected)
		{
			var controller = GetNewControllerCore(selected);
			if (controller != null)
			{
				SetupNewController(controller);
			}

			return controller;
		}

		protected virtual ZController GetNewControllerCore(BusinessObject selected)
		{
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as ZFilterModule)
			{
				if (preventNullControllerExceptions && (module == null || !module.HasActions))
				{
					return null;
				}
				else if (module == null)
				{
					throw new Exception("ModuleID " + ModuleID.ToString() + " is either not a ZFilterGridModule or cannot be created.");
				}
				else
				{
					return module.GetNewController(selected);
				}
			}
		}

		protected virtual void SetupNewController(ZController controller)
		{
		}

		ZController GetNewControllerSafe(BusinessObject selected)
		{
			try
			{
				preventNullControllerExceptions = true;
				return GetNewController(selected);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				return null;
			}
			finally
			{
				preventNullControllerExceptions = false;
			}
		}

		bool preventNullControllerExceptions;

		protected IBusinessObjectCollection Collection
		{
			get { return (IBusinessObjectCollection)Grid.List; }
		}

		protected ZForm Form
		{
			get { return FindForm() as ZForm; }
		}

		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnly")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new IBusiness DataSource
		{
			get { return (IBusiness)Grid.DataSource; }
		}

		protected virtual bool NeedsSaveToShowEditForm(BusinessObject selected)
		{
			var selectedNeedsSaving = selected != null && (!selected.IsInDatabase || selected.HasChanges) && !(selected is NonPersistentBusinessObject);
			var masterNeedsSaving = !((IBusinessObjectCollectionInternals)Collection).MastersAreInDatabase || (AlwaysRequiresSaveBeforeEdit && DataSource.HasChanges);

			return selectedNeedsSaving || masterNeedsSaving;
		}

		#region SuppressResourceStringsCheckRegion

		string GenerateDebugInfoAboutCollection()
		{
			var result = "";
			try
			{
				if (Collection != null)
				{
					result += "\r\nCollection is " + Collection.GetType().FullName;
				}

				result += "\r\nDataSource is " + DataSource.GetType().FullName;
				result += "\r\nDataSource.PK=" + DataSource.Identifier;
				result += "\r\nDataSource.HasChanges=" + DataSource.HasChanges;
				result += "\r\nDataSource.Factory=" + DataSource.Factory?._Instance ?? string.Empty;
				if (DataSource.HasChanges)
				{
					result += GenerateChildrenWithChanges(DataSource);
				}

				result += "\r\nAlwaysRequiresSaveBeforeEdit=" + AlwaysRequiresSaveBeforeEdit;
				result += "\r\nCollection.MastersAreInDatabase=" + ((IBusinessObjectCollectionInternals)Collection).MastersAreInDatabase;
				var masterNeedsSaving = !((IBusinessObjectCollectionInternals)Collection).MastersAreInDatabase || (AlwaysRequiresSaveBeforeEdit && DataSource.HasChanges);
				result += "\r\nMasterNeedsSaving=" + masterNeedsSaving;
			}
			catch (Exception e) when (!e.IsCriticalException()) // just in case
			{
				ErrorReporter.ReportOnce("Error generating debug info about BizO in ButtonGrid", e);
			}
			return result;
		}

		string GenerateChildrenWithChanges(IBusiness parent)
		{
			var result = "";
			try
			{
				foreach (var child in GetAllChildrenWithChanges(parent))
				{
					if (child is BusinessObject childBizObj)
					{
						var childIsInDatabase = childBizObj.IsInDatabase ? "Yes" : "No";
						var bizobjTypesAroundChild = new ZStringBuilder();
						childBizObj.Factory?.GetBizOsForPK(childBizObj.PK.ToGuid()).Select(x => x.GetType().Name).OrderBy(x => x).ForEach(x => bizobjTypesAroundChild.Append(x));
						result += FormattableString.Invariant($"\r\n    {child.GetType().FullName} : PK = {childBizObj.PK}, IsInDatabase = {childIsInDatabase}, IsDeleted = {childBizObj.IsDeleted}, Type = {bizobjTypesAroundChild.ToStringWithDelimiterBetweenAppends("|")}, HasChanges = true, Factory = {child.Factory?._Instance}");

						if (child.HasChangesNotIncludingChildren)
						{
							foreach (var propertyInfo in childBizObj.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => x.HasChanges))
							{
								result += FormattableString.Invariant($"\r\n    Property {propertyInfo.Name} HasChanges = true");
							}
						}

						//Note: WI00743739 removed debug logging to resolve performance issues in CS01594069.
					}
					else
					{
						result += FormattableString.Invariant($"\r\n    {child.GetType().FullName} : HasChanges = true, Factory = {child.Factory?._Instance}");
					}
				}

				if (string.IsNullOrEmpty(result))
				{
					result = "\r\n    No changes in children";
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error generating debug info about BizO in ButtonGrid - in GenerateChildrenWithChanges", ex);
			}
			return result;
		}

		IEnumerable<IBusiness> GetAllChildrenWithChanges(IBusiness parent)
		{
			var result = new List<IBusiness>();
			if (parent.HasChanges)
			{
				result.Add(parent);
				parent.Children.ForEach(x => result.AddRange(GetAllChildrenWithChanges(x)));
			}
			return result;
		}

		string GenerateDebugInfoAboutBizO(BusinessObject selected)
		{
			var result = "";
			try
			{
				if (selected != null)
				{
					result += "\r\nSelected is " + selected.GetType().FullName;
					result += "\r\nSelected.PK=" + selected.PK;
					result += "\r\nSelected.IsInDatabase=" + selected.IsInDatabase;
					result += "\r\nSelected.HasChanges=" + selected.HasChanges;
					result += "\r\nSelected.Factory=" + selected.Factory?._Instance ?? string.Empty;
					if (selected.HasChanges)
					{
						result += GenerateChildrenWithChanges(selected);
					}

					var selectedNeedsSaving = selected != null && (!selected.IsInDatabase || selected.HasChanges);
					result += "\r\nSelectedNeedsSaving=" + selectedNeedsSaving + "\r\n";
				}
				else
				{
					result += "\r\nSelected is NULL!";
				}

				return result + GenerateDebugInfoAboutCollection();
			}
			catch (Exception e) when (!e.IsCriticalException()) // just in case
			{
				ErrorReporter.ReportOnce("Error generating debug info about BizO in ButtonGrid", e);
			}
			return result;
		}

		#endregion

		protected bool NeedsSaveToShowNewForm
		{
			get { return !((IBusinessObjectCollectionInternals)Collection).MastersAreInDatabase; }
		}

		public void ShowNotSelectedMessage()
		{
			var error = Res.GetString("4b5a25a4-5dda-4bc0-96f3-4934cb17e7e0", "Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.");
			var caption = Res.GetString("bbaedf07-0039-4ac6-b140-cdb556e02419", "Select {0}{1}", Grammar.Instance.IndefiniteArticlePrefix(NameOfAGridElement.Caption), NameOfAGridElement.Caption);

			Globals.Message.ShowError(error, caption);
		}

		public void ShowCantEditMessage()
		{
			Globals.Message.ShowError(Res.GetString("0b554b1c-ab34-43b7-9132-7ece3f4bb623", "The item you are trying to edit has been deleted or does not exist."));
		}

		#endregion

		#endregion

		#region Handle Readonly on Visible Change

		protected bool ButtonsReadOnly { get; private set; }

		public void SetButtonsReadOnly(bool readOnly)
		{
			ButtonsReadOnly = readOnly;
			if (readOnly)
			{
				UpdateButtonsReadOnly();
			}
		}

		#endregion

		#region Control Tab Order

		protected override void OnCreateControl()
		{
			Grid.TabIndex = 0;
			base.OnCreateControl();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (HasWeakReferencesToNewForms)
				{
					foreach (WeakReference weakRef in FormsReferenced.Values)
					{
						var newForm = weakRef.Target as ZForm;
						if (newForm != null)
						{
							newForm.Saved -= new EventHandler(NewForm_Saved);
						}
					}
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Actions Menu Item

		/// <summary>
		/// Allow to skip creating Module Context Menu Actions and its children.
		/// </summary>
		protected virtual bool ShouldSkipActionsMenuItemCreation => false;

		void AddActionsMenuItem()
		{
			RemoveActionsMenuItem();

			var items = GetModuleMenuItems();

			if (!ShouldSkipActionsMenuItemCreation && items != null && items.Length != 0)
			{
				actionsMenuItem = items.Length == 1 ? items[0] : new ZMenuItem(ResString.GetMultilingualString("Grid.Actions", "Actions"));
				actionsMenuItem.MenuItems.AddRange(items);
				Grid.ContextMenu.MenuItems.Add(actionsMenuItem);
			}
		}

		void RemoveActionsMenuItem()
		{
			if (actionsMenuItem != null)
			{
				Grid.ContextMenu.MenuItems.Remove(actionsMenuItem);
				actionsMenuItem.Dispose();
				actionsMenuItem = null;
			}
		}

		MenuItem[] GetModuleMenuItems()
		{
			MenuItem[] result = null;

			if (ModuleID != ModuleIDs.NotAssigned)
			{
				ZModule module;
				using (module = ZModuleFactory.Instance.Create(ModuleID))
				{
					var gridModule = module as ZFilterGridModule;

					if (gridModule != null)
					{
						result = gridModule.Plugins.GetButtonGridMenuItemsToAdd(this);
					}
				}
			}

			return result;
		}
		MenuItem actionsMenuItem;

		#endregion

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource == null)
			{
				if (InnerGrid.ListManager != null)
				{
					InnerGrid.ListManager.ListChanged -= ListManager_ListChanged;
				}
				InnerGrid.SetDataBinding(null, "");
				DataBindings.Clear();
			}
			else
			{
				var requiresBindToFindBoxList = (ShowAttachButton || ShowDetachButton);
				if (!ElementsBelongToDifferentModules && (string.IsNullOrEmpty(BindToGridList) || (requiresBindToFindBoxList && string.IsNullOrEmpty(BindToFindBoxList))))
				{
					throw new Exception("Unable to bind ZModuleButtonGrid as .BindToGridList or .BindToFindBoxList is not filled in.");
				}

				InnerGrid.SetDataBinding(dataSource, dataMember);

				if (!ElementsBelongToDifferentModules && (ModuleID == null || ModuleID == ModuleIDs.NotAssigned))
				{
					throw new Exception("Assign a ModuleID to the ZModuleButtonGrid.");
				}

				if (!string.IsNullOrEmpty(BindToFindBoxList))
				{
					var listBinding = new KBinding(nameof(FindBoxList), dataSource, BindToFindBoxList);
					listBinding.FormattingEnabled = true;
					DataBindings.Add(listBinding);
				}
				if (InnerGrid.ListManager != null)
				{
					InnerGrid.ListManager.ListChanged += ListManager_ListChanged;
				}
				UpdateButtonsReadOnly();

				AddActionsMenuItem();
			}
		}

		protected virtual bool ElementsBelongToDifferentModules
		{
			get { return false; }
		}

		public override Type DataSourceType
		{
			get { return typeof(IList); }
		}

		void ListManager_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemChanged || e.ListChangedType == ListChangedType.Reset || e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted)
			{
				isListReadOnly = null;
				UpdateButtonsReadOnly();
			}
		}

		protected bool IsListReadOnly
		{
			get
			{
				if (isListReadOnly == null)
				{
					var businessObjectCollection = InnerGrid.List as IBusinessObjectCollection;
					isListReadOnly = businessObjectCollection != null && businessObjectCollection.ReadOnly;
				}
				return (bool)isListReadOnly;
			}
		}
		bool? isListReadOnly;

		protected virtual void UpdateButtonsReadOnly()
		{
			if (NewButton != null)
			{
				NewButton.Enabled = !ButtonsReadOnly && !IsListReadOnly;
			}

			if (EditButton != null)
			{
				EditButton.Enabled = InnerGrid.List != null && InnerGrid.List.Count > 0;
				if (EditButton.CaptionResourceString.Equals(DefaultViewButtonText) || EditButton.CaptionResourceString.Equals(DefaultEditButtonText))
				{
					EditButton.CaptionResourceString = (ButtonsReadOnly || IsListReadOnly) ? DefaultViewButtonText : DefaultEditButtonText;
				}
			}

			if (AttachButton != null)
			{
				AttachButton.Enabled = !ButtonsReadOnly && !IsListReadOnly;
			}

			if (DetachButton != null)
			{
				DetachButton.Enabled = !ButtonsReadOnly && !IsListReadOnly;
			}
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			if (!string.IsNullOrEmpty(BindToFindBoxList))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(IList), BindToFindBoxList));
			}
			return result;
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZModuleButtonGrid>()
				.Property("Text", "") // Architecture string
				.Property<IBusinessObjectCollection>("FindBoxList", null)
				.Property("IsVisibleForBinding", ZBool.True)
				.Result;
		}

		#endregion

		#region GridId

		[Browsable(false)]
		public string GridId
		{
			get => InnerGrid.GridId;
			set => InnerGrid.GridId = value;
		}

		#endregion
	}

	#region Events

	public delegate void ModuleButtonGridOperationCancelEventHandler(object sender, ModuleButtonGridOperationCancelEventArgs args);

	public class ModuleButtonGridOperationCancelEventArgs : CancelEventArgs
	{
		public ModuleButtonGridOperationCancelEventArgs(object bizOBeingOperated)
		{
			this.BizOBeingOperated = bizOBeingOperated;
		}

		public readonly object BizOBeingOperated;
	}

	public class ModuleButtonGridOnAttachEventArgs : CancelEventArgs
	{
		public ModuleButtonGridOnAttachEventArgs(BusinessObject[] attachedBusinessObjects)
		{
			this.AttachedBusinessObjects = attachedBusinessObjects;
		}

		public readonly BusinessObject[] AttachedBusinessObjects;
	}

	public class ModuleButtonGridOnDetachedEventArgs : EventArgs
	{
		public ModuleButtonGridOnDetachedEventArgs(BusinessObject[] detachedBusinessObjects)
		{
			this.DetachedBusinessObjects = detachedBusinessObjects;
		}

		public readonly BusinessObject[] DetachedBusinessObjects;
	}

	public class ModuleButtonGridBeforeDetachEventArgs : CancelEventArgs
	{
		public ModuleButtonGridBeforeDetachEventArgs(BusinessObject[] toDetachBusinessObjects)
		{
			this.ToDetachBusinessObjects = toDetachBusinessObjects;
		}

		public readonly BusinessObject[] ToDetachBusinessObjects;
	}

	#endregion

	#region ButtonGridModule Decision Provider

	public class ButtonGridModuleDecisionProvider : ModuleDecisionProvider
	{
		public ButtonGridModuleDecisionProvider(IFindBox findBox)
			: base(findBox, null)
		{
		}

		public override bool ShouldDisplayNotifications
		{
			get { return true; }
		}

		public override bool ShouldIgnoreAdditionalFilter
		{
			get { return true; }
		}

		public override bool AllowExcelExport
		{
			get { return false; }
		}

		public override void SetFindBoxCodeDescription(BusinessObject bo) { }
	}

	#endregion

	#region IButtonGrid

	public interface IButtonGrid
	{
		ZGridWithoutColumnStylesSerialisation InnerGrid { get; }
	}

	#endregion
}

