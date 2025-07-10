using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Registry
{
	public class CorrelationIDCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public CorrelationIDCustomisationRegistryDataType() : base(new CorrelationIDCustomisation())
		{
			GeneratedNumberName = ResString.GetMultilingualString("90C7E71D-05BC-4724-95E5-AF17CA6AD929", "Correlation ID");
			MaxLength = FRCustomsDataRegistry.Schema.CorrelationIDMaxLength;
		}

		protected override Type DataTypeCore => typeof(CorrelationIDCustomisation);
	}

	[XmlSerializerAssembly("Enterprise.Customs.FR.Registry.XmlSerializers")]
	public class CorrelationIDCustomisation : BillOfLadingNumberCustomisation
	{
		public CorrelationIDCustomisation()
		{
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			RemoveFountainPrefix = true;
			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, e => { e.Detail = "10"; });
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
			return new CorrelationIDCustomisation();
		}
	}
}
