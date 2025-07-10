using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DefaultDestinationPremiseID))]
	sealed class DefaultDestinationPremiseIDTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.AirlineCode = "QF";
			BizObj.PortOfDischarge = "AUSYD";
			BizObj.PremiseID = "9914N";
			BizObj.UseDischargePort = true;

			return BizObj;
		}

		new DefaultDestinationPremiseID BizObj
		{
			get { return (DefaultDestinationPremiseID)base.BizObj; }
		}
	}
}
