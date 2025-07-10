using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Registry
{
	public class FRCustomsFallbackEntryNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public FRCustomsFallbackEntryNumberCustomisationRegistryDataType() : base(new FRCustomsFallbackEntryNumberCustomisation())
		{
			GeneratedNumberName = ResString.GetMultilingualString("72635E2A-FD97-4B6E-B007-114527EAF25A", "FR Customs Fallback Entry Number");
			MaxLength = 10;
		}
		protected override Type DataTypeCore => typeof(FRCustomsFallbackEntryNumberCustomisation);
	}

	[XmlSerializerAssembly("Enterprise.Customs.FR.Registry.XmlSerializers")]
	public class FRCustomsFallbackEntryNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public FRCustomsFallbackEntryNumberCustomisation()
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
			return new FRCustomsFallbackEntryNumberCustomisation();
		}
	}
}
