using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class StlRawUsageReportRefCaption : AutoStlRawUsageReportRefCaption
	{
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			Category = BillingConstants.BillingSystem.STL;
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StlRawUsageReportRefCaption();
		}

		#endregion
	}
}

