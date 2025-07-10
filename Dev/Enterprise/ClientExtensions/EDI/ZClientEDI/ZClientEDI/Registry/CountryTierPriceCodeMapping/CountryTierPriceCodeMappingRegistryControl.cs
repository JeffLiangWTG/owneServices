using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class CountryTierPriceCodeMappingRegistryControl : RegistryZUserControl
	{
		public CountryTierPriceCodeMappingRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PriceCodeGrid.ReadOnly = MappingLineGrid.ReadOnly = readOnly;
		}
	}
}
