using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceDateConfiguration))]
	public class InvoiceDateConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			InvoiceDateConfiguration result = new InvoiceDateConfiguration();

			result.JobType = "SHP";
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.Mode = Enterprise.Core.Constants.TransportModes.Air;
			result.SignificantDateCode = InvoiceDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate;

			return result;
		}

		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new InvoiceDateConfiguration BizObj
		{
			get
			{
				return (InvoiceDateConfiguration)base.BizObj;
			}
		}
	}
}
