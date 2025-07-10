using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Registry.GUI
{
	[SuppressBindingMemberBashingTest] // for CustomiseByServiceLevelCheckBox
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	partial class BillOfLadingNumberCustomisationByServiceLevelControl : RegistryBusinessObjectTemplateZUserControl
	{
		public BillOfLadingNumberCustomisationByServiceLevelControl(BillCustomisationByServiceLevelRegistryDataType dataType)
		{
			InitializeComponent();
			CustomisationControl.Initialize(dataType);
			SetDataSourceBinding("ServiceLevel", "BillOfLadingNumberCustomisations.ServiceLevel");
			this.dataType = dataType;
		}

		readonly BillCustomisationByServiceLevelRegistryDataType dataType;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			CustomiseByServiceLevelCheckBox.Checked = CustomiseByServiceLevel;
			splitContainer1.Panel1Collapsed = !CustomiseByServiceLevelCheckBox.Checked;
		}

		#region ServiceLevel

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZString ServiceLevel
		{
			get { return serviceLevel; }
			set
			{
				if (serviceLevel != value)
				{
					serviceLevel = value;

					var serviceLevelText = serviceLevel == "ALL" ? Res.GetString("cde4e12e-a589-4b39-85e1-96465e1ba4f0", "For ALL Service Levels") : Res.GetString("a933b213-7e54-492a-88ac-3f7575ead1dc", "For Service Level {0}", serviceLevel);
					if (dataType.Categories.HasFlag(NumberCustomisationElementCategories.Domestic))
					{
						HouseBillNumberByServiceLevelGroupBox.Text = Res.GetString("97b625b2-0eaf-4422-sda0-f2db4f771944", "Transport Number Customization {0}", serviceLevelText);
					}
					else if (dataType.Categories.HasFlag(NumberCustomisationElementCategories.WarehouseOrder))
					{
						HouseBillNumberByServiceLevelGroupBox.Text = Res.GetString("f08bfc81-193e-4007-9f9f-6974bda64a68", "Warehouse Order ID Customization {0}", serviceLevelText);
					}
					else if (dataType.Categories.HasFlag(NumberCustomisationElementCategories.WarehouseReceive))
					{
						HouseBillNumberByServiceLevelGroupBox.Text = Res.GetString("52c0db2e-585c-4b8c-971e-06f32d2dd003", "Warehouse Receive ID Customization {0}", serviceLevelText);
					}
					else
					{
						HouseBillNumberByServiceLevelGroupBox.Text = Res.GetString("97b625b2-0eaf-4422-9210-f2db4f771944", "Bill Of Lading {0} Number Customization {1}", dataType.GeneratedNumberName, serviceLevelText);
					}
				}
			}
		}
		ZString serviceLevel;

		#endregion

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<BillOfLadingNumberCustomisationByServiceLevelControl>()
			.Property(nameof(ServiceLevel), ZString.Empty, false)
			.Result;
		}

		#endregion

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CustomisationControl.ReadOnly = readOnly;
			CustomiseByServiceLevelCheckBox.ReadOnly = readOnly;
		}

		void CustomiseByServiceLevelCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			CustomiseByServiceLevel = CustomiseByServiceLevelCheckBox.Checked;
		}

		public ZBool CustomiseByServiceLevel
		{
			get
			{
				if (customiseByServiceLevel == null)
				{
					customiseByServiceLevel = HasServiceLevelCustomisations;
				}
				return customiseByServiceLevel.Value;
			}
			set
			{
				if (customiseByServiceLevel == null || customiseByServiceLevel != value)
				{
					if (!value && HasServiceLevelCustomisations)
					{
						Globals.Message.ShowError(Res.GetString("b491f817-35ee-4202-8ac9-3c038fb4ff80", "Cannot untick as Service Level Customizations exist."));
						CustomiseByServiceLevelCheckBox.Checked = true;
					}
					else
					{
						customiseByServiceLevel = value;
						CustomiseByServiceLevelCheckBox.Checked = value;
						splitContainer1.Panel1Collapsed = !value;
					}
				}
			}
		}
		ZBool? customiseByServiceLevel;

		ZBool HasServiceLevelCustomisations => Customisations?.BillOfLadingNumberCustomisations.Count > 1;

		BillOfLadingNumberCustomisationsByServiceLevel Customisations => (BillOfLadingNumberCustomisationsByServiceLevel)DataSource;
	}
}
