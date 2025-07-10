using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.EU.Business.XmlSerializers")]
	public sealed class LimitedFiscalRepresentationNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public LimitedFiscalRepresentationNumberCustomisation()
		{
		}
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, e => { e.Order = 1; e.Include = true; e.Detail = "4"; });
			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits, e => { e.Order = 2; e.Include = true; });
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
			return new LimitedFiscalRepresentationNumberCustomisation();
		}
	}
}
