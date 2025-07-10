using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class LegacyModuleMappingsControl : RegistryZUserControl
	{
		public LegacyModuleMappingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.MappingGrid.ReadOnly = readOnly;
		}

		ZString moduleMappingCaption = "Module Mapping";
		public ZString ModuleMappingCaption
		{
			get
			{
				return moduleMappingCaption;
			}
			set
			{
				moduleMappingCaption = value;
				MappingGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(col => col.ColumnName == "ModuleMapping").Caption = Res.GetString("3827CDE6-B1B2-4C13-B663-0B78CBDDD179", "{0}", ModuleMappingCaption);
				MappingGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(col => col.ColumnName == "ModuleMappingDescription").Caption = Res.GetString("D281F6C0-B3B4-48DA-9BAF-49649A75F607", "{0} Description", ModuleMappingCaption);
			}
		}
	}
}
