using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class SystemProductRegistryControl : RegistryZUserControl
	{
		public SystemProductRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ProductsGrid.ReadOnly = readOnly;
			ModuleGrid.ReadOnly = readOnly;
			SourceModuleGrid.ReadOnly = readOnly;
		}

		public ZBool IsCodeDropDown
		{
			get { return isCodeDropDown; }
			set
			{
				isCodeDropDown = value;

				ProductsGrid.SetAvailability(!isCodeDropDown, "Code");
				ProductsGrid.SetAvailability(isCodeDropDown, "CodeExisting");
				ProductsGrid.ReOrderColumns(["Description", "Code", "CodeExisting"]);

				if (value)
				{
					var desciptionInfo = ProductsGrid.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().FirstOrDefault(s => s.ColumnName.Equals("Description"));

					if (desciptionInfo != null)
					{
						desciptionInfo.IsReadOnly = true;
					}
				}
			}
		}

		ZString moduleCaption = "Module";
		ZBool isCodeDropDown;

		public ZString ModuleCaption
		{
			get
			{
				return moduleCaption;
			}
			set
			{
				moduleCaption = value;
				ModuleMappingsLabel.Text = Res.GetString("266AA46E-211D-4630-B968-CE69D69244CA", "{0} Mappings", moduleCaption);
				ModuleGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(col => col.ColumnName == "ModuleCode").Caption = Res.GetString("5D532DD4-B747-4AF6-A448-84685E798979", "{0} Code", ModuleCaption);
				ModuleGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(col => col.ColumnName == "ModuleDescriptionMultilingual").Caption = Res.GetString("9255DD88-DCD7-4EB3-9AF6-56B4137E4810", "{0} Description", ModuleCaption);
			}
		}
	}
}
