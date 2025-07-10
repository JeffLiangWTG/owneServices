using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	public class IncidentGroupStatusConfigurationConstantsTest : TestCase
	{
		public void TestGetSequenceByCode()
		{
			var invSequence = IncidentGroupStatusConfigurationConstants.GetSequenceByCode(IncidentGroupStatusConfigurationConstants.Investigation);
			var escSequence = IncidentGroupStatusConfigurationConstants.GetSequenceByCode(IncidentGroupStatusConfigurationConstants.Escalated);
			var aciSequence = IncidentGroupStatusConfigurationConstants.GetSequenceByCode(IncidentGroupStatusConfigurationConstants.ActiveIncident);
			var psiSequence = IncidentGroupStatusConfigurationConstants.GetSequenceByCode(IncidentGroupStatusConfigurationConstants.PostIncident);
			var rsvSequence = IncidentGroupStatusConfigurationConstants.GetSequenceByCode(IncidentGroupStatusConfigurationConstants.PIRCompleted);

			AssertEquals(invSequence, IncidentGroupStatusConfigurationConstants.ConfigurationINV.Sequence);
			AssertEquals(escSequence, IncidentGroupStatusConfigurationConstants.ConfigurationESC.Sequence);
			AssertEquals(aciSequence, IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence);
			AssertEquals(psiSequence, IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Sequence);
			AssertEquals(rsvSequence, IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Sequence);
		}
	}
}
