using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class MassUpdateWizardForm : ZChildForm
	{
		public MassUpdateWizardForm()
		{
		}

		public MassUpdateWizardForm(IImportCollectionInfo collectionInfo)
			: base(new MassUpdateWizard(collectionInfo))
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeComponentAfterSizeSet();
			MachingFiltersGrid.ShowMassUpdateMenuItem = false;
			BizObjsToUpdateGrid.ShowMassUpdateMenuItem = false;
			var shouldShow = Wizard != null && Wizard.FieldList.Count > 0;
			MainPanel.Visible = shouldShow;
			FindButton.Visible = shouldShow;
			UpdateButton.Visible = shouldShow;
			NoFieldAvailableLabel.Visible = !shouldShow;
			FieldValueToUpdateUserControl.CaptionRenderingEnabled = true;
			TypeDescriptor.AddAttributes(FieldToUpdateDropEdit, new SuppressFormsLocalizedTestAttribute());
		}

		void InitializeComponentAfterSizeSet()
		{
			// this need to be set after the Size of the controls have been set... it's a bug in VS Designer serialisation
			this.MatchingSplitContainer.Panel2MinSize = 200;
		}

		MassUpdateWizard Wizard
		{
			get { return (MassUpdateWizard)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var wizard = (MassUpdateWizard)dataSource;
			if (wizard != null)
			{
				AddColumnsBizObjsToUpdateGrid(wizard);
				AddControlsForFieldValueToUpdate(wizard);
				wizard.FieldInfo.ValueChanged += new EventHandler(FieldInfo_ValueChanged);
			}
			else if (Wizard != null)
			{
				Wizard.FieldInfo.ValueChanged -= new EventHandler(FieldInfo_ValueChanged);
			}
			base.SetDataBinding(dataSource, dataMember);
			if (wizard != null)
			{
				UpdateFieldValueControlsVisibility();
			}
		}

		void FieldInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateFieldValueControlsVisibility();
		}

		void UpdateFieldValueControlsVisibility()
		{
			if (Wizard != null && updateControls != null)
			{
				var mappingName = Wizard.FieldMappingName;
				var foundVisible = false;
				Control firstControl = null;
				foreach (var pair in updateControls)
				{
					if (firstControl == null)
					{
						firstControl = pair.Value;
					}
					var visible = false;
					if (!foundVisible)
					{
						visible = !mappingName.IsEmpty && pair.Key == mappingName;
						if (visible)
						{
							foundVisible = true;
						}
					}
					var control = pair.Value;
					control.Visible = foundVisible && visible;
					control.Enabled = true;
				}
				if (!foundVisible && firstControl != null)
				{
					firstControl.Visible = true;
					firstControl.Enabled = false;
				}
			}
		}

		void AddControlsForFieldValueToUpdate(MassUpdateWizard wizard)
		{
			updateControls = new Dictionary<string, Control>();
			foreach (MassUpdateFieldItem item in wizard.FieldList)
			{
				var property = item.propertyInfo;
				var mappingName = property.MappingName;
				string bindTo = wizard.GetBindToField(wizard.propertyTypeLists[mappingName]);
				var bindToList = "FieldValueBindingList";
				var fieldType = wizard.GetFieldType(mappingName);
				var control = UserControlDecider.CreateControl(fieldType, wizard.GetModuleId(mappingName), bindTo, bindToList, mappingName);
				control.Visible = false;
				control.Dock = DockStyle.Fill;
				updateControls.Add(property.MappingName, control);
				FieldValueToUpdateUserControl.Controls.Add(control);
			}
		}
		Dictionary<string, Control> updateControls;

		void AddColumnsBizObjsToUpdateGrid(MassUpdateWizard wizard)
		{
			foreach (var property in wizard.CollectionInfo.Properties)
			{
				var styleInfoType = GetColumnInfoType(wizard, property);
				var styleInfo = (ZGridColumnInfo)Activator.CreateInstance(styleInfoType);
				styleInfo.ColumnName = property.MappingName;
				styleInfo.Caption = property.HeaderText;
				if (styleInfo is ZMultiControlColumnStyleInfo zMulti)
				{
					zMulti.FieldTypeColumnName = property.FieldTypeColumnName;
				}
				ControlDpiScalingHelper.SetWidth(ref styleInfo, property.ColumnWidth, false);
#if DEBUG
				TypeDescriptor.AddAttributes(styleInfo, new SuppressFormsLocalizedTestAttribute());
#endif
				BizObjsToUpdateGrid.ColumnStyles.Add(styleInfo);
			}
			BizObjsToUpdateGrid.RefreshTableStyles();
		}

		internal static Type GetColumnInfoType(MassUpdateWizard wizard, IImportPropertyInfo property)
		{
			Type styleInfoType;
			if (property.IsMultiControl)
			{
				styleInfoType = typeof(ZMultiControlColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZByte))
				|| ImportWizard.IsType(property.PropertyType, typeof(ZShort))
				|| ImportWizard.IsType(property.PropertyType, typeof(ZInt))
				|| ImportWizard.IsType(property.PropertyType, typeof(ZDecimal))
				|| ImportWizard.IsType(property.PropertyType, typeof(ZLong)))
			{
				styleInfoType = typeof(ZCalcEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZString))
					|| ImportWizard.IsType(property.PropertyType, typeof(ZMultilingual)))
			{
				styleInfoType = typeof(ZTextBoxColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZDateTime))
					|| ImportWizard.IsType(property.PropertyType, typeof(ZDate)))
			{
				styleInfoType = typeof(ZDateEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZDateTimeOffset)))
			{
				styleInfoType = typeof(ZDateTimeOffsetEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZTime)))
			{
				styleInfoType = typeof(ZTimeEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZGeography)))
			{
				styleInfoType = typeof(ZGeographyEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZGuid)))
			{
				var list = wizard.GetBindingList(property.MappingName);
				if ((list as IBusinessObjectCollection) != null)
				{
					styleInfoType = typeof(ZGuidFindBoxColumnStyleInfo);
				}
				else
				{
					styleInfoType = typeof(ZGuidDropEditColumnStyleInfo);
				}
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZBool)))
			{
				styleInfoType = typeof(ZCheckBoxColumnStyleInfo);
			}
			else
			{
				throw new NotSupportedException(string.Format("Cannot determine column style info for property {0}.{1} ({2}).", property.ComponentType.FullName, property.MappingName, property.PropertyType.FullName));
			}

			return styleInfoType;
		}

		#endregion

		void FindButton_Click(object sender, EventArgs e)
		{
			if (Wizard != null)
			{
				Wizard.LoadBizObjsToUpdate();
			}
		}

		void UpdateButton_Click(object sender, EventArgs e)
		{
			if (Wizard != null)
			{
				ValidateAll(ValidationType.Full);
				if (Wizard.HasErrors)
				{
					ZMessageBox msgBox = new ZErrorMessageBox(Wizard, Res.GetString("bd133244-6bf7-4c54-bba0-dc5dfe917575", "Mass Update Wizard"), Res.GetString("fe96ffc1-3380-4ef0-838f-28c2b5ea3aaf", "update"), Res.GetString("55ec5cee-aab2-4505-8aac-b21de8d1f018", "updated"));
					if (msgBox != null)
					{
						ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
						msgBox.Dispose();
					}
				}
				else
				{
					if (Wizard.MatchingFilters.Count != 0 || Globals.Message.ShowConfirmation(Res.GetString("e200075e-17f0-48d7-bf02-d082216da40a", "No filter has been selected which means that all records will be affected.\r\nDo you want to continue?"), Res.GetString("d7716925-ee31-402e-a445-f77467d42747", "Warning No Filter"), Res.GetString("d98a6dbb-5164-4c6e-9b95-441baed94aee", "yes"), MessageBoxIcon.Warning) == DialogResult.OK)
					{
						Wizard.Update();
						Close();
					}
				}
			}
		}

		void CloseButtonX_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
