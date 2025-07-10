using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class CDSEntryLocalReferenceNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public CDSEntryLocalReferenceNumberCustomisation()
		{
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			RemoveFountainPrefix = true;

			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode, e => { e.Include = ZBool.True; e.OverrideStrategy(new GBElementStrategy(BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode, "Enterprise Code", 3)); });
			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.CompanyCode, e => { e.Include = ZBool.True; e.OverrideStrategy(new GBElementStrategy(BillOfLadingNumberCustomisationElement.Keys.CompanyCode, "Company Code", 3)); });
			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.ServerCode, e => { e.Include = ZBool.True; e.OverrideStrategy(new GBElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ServerCode, "Server Code", 3)); });
			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, e => { e.Detail = "13"; });
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
			return new CDSEntryLocalReferenceNumberCustomisation();
		}
	}
}
