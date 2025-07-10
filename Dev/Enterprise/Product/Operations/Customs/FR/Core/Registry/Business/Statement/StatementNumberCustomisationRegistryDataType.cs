using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Registry
{
	public class StatementNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public StatementNumberCustomisationRegistryDataType() : base(new StatementNumberCustomisation())
		{
			GeneratedNumberName = ResString.GetMultilingualString("015489C2-68F6-44FE-8358-5E385DA275EB", "Statement Number");
			MaxLength = FRCustomsDataRegistry.Schema.StatementNumberMaxLength;
		}

		protected override Type DataTypeCore => typeof(StatementNumberCustomisation);
	}

	[XmlSerializerAssembly("Enterprise.Customs.FR.Registry.XmlSerializers")]
	public class StatementNumberCustomisation : BillOfLadingNumberCustomisation
	{
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, e => { e.Detail = "8"; });
		}

		void SetUnFilteredElement(ZString key, Action<BillOfLadingNumberCustomisationElement> setter)
		{
			var element = UnFilteredElements[key];
			if (element != null)
			{
				using (element.SuspendSettingHasChanges())
				using (element.GetValidationSuspender())
				{
					setter?.Invoke(element);
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StatementNumberCustomisation();
		}
	}
}
