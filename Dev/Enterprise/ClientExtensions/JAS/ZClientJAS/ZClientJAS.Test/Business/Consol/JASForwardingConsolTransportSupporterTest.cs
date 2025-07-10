using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Client.JAS.Business.Testing
{
	internal sealed class JASForwardingConsolTransportSupporterTest : TransportSupporterTestCase<JASForwardingConsolTransportSupporter>
	{
		public void TestGetJXCTransportValidation()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			Transport transport = consol.Transports.AddNew();
			JASForwardingConsolTransportSupporter supporter = (JASForwardingConsolTransportSupporter)((ITransportParent)consol).TransportSupporter;
			AssertEquals("No domain validations registered, should use base type", typeof(JXCForwardingConsolTransportValidation), supporter.GetJXCTransportValidation(transport).GetType());
			Factory.Validation.MainGroup.RegisterValidationType<Transport, JXCForwardingSeaConsolTransportValidation>();
			AssertEquals("Registered to the MainDomainValidationGroup", typeof(JXCForwardingSeaConsolTransportValidation), supporter.GetJXCTransportValidation(transport).GetType());
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Factory.Validation.MainGroup.UnregisterValidationType<Transport, JXCForwardingSeaConsolTransportValidation>();
			newFactory.Validation.MainGroup.RegisterValidationType<Transport, JXCForwardingSeaConsolTransportValidation>();
			Factory.Validation.AddAllFrom(newFactory.Validation);
			AssertEquals("Registered to the AdditionalDomainValidationGroup", typeof(JXCForwardingSeaConsolTransportValidation), supporter.GetJXCTransportValidation(transport).GetType());
			newFactory.Validation.MainGroup.UnregisterValidationType<Transport, JXCForwardingSeaConsolTransportValidation>();
			Factory.Validation.MainGroup.RegisterValidationType<Transport, JASForwardingConsolTest.InvalidDomainValidation>();
			AssertEquals("Domain validation found is not of type JXCForwardingConsolTransportValidation, should return the base type", typeof(JXCForwardingConsolTransportValidation), supporter.GetJXCTransportValidation(transport).GetType());
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent consol = Factory.New<JASForwardingConsol>();
			return consol.TransportSupporter;
		}

		protected override Security.SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get
			{
				return Env.Security.RoadDistanceCalculationServiceForwarding;
			}
		}

		protected override CargoWise.Types.ZString TestingCountry
		{
			get
			{
				return Core.Constants.CountryCodes.Australia;
			}
		}
	}
}
