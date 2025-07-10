using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Registry.Business
{
	public static class IncidentGroupStatusConfigurationConstants
	{
		public const string TriggerOnNew = "New";
		public const string TriggerOnEscalated = "Escalated";
		public const string TriggerOnSendOpeningBroadcast = "Send Opening Broadcast";
		public const string TriggerOnSendClosingBroadcast = "Send Closing Broadcast";
		public const string TriggerOnDefaultValue = "Manual";

		public const string MajorIncidentCode = "MIM";
		public const string MajorIncidentDescription = "Major Incident";

		public const string Investigation = "INV";
		public const string Escalated = "ESC";
		public const string ActiveIncident = "ACI";
		public const string PostIncident = "PSI";
		public const string PIRCompleted = "RSV";

		public struct IncidentGroupStatusConfigurationParameter
		{
			public IncidentGroupStatusConfigurationParameter(int sequence, string code, string descriptionOnGroup, string triggerOn)
			{
				Sequence = sequence;
				Code = code;
				DescriptionOnGroup = descriptionOnGroup;
				TriggerOn = triggerOn;
			}
			public readonly int Sequence;
			public readonly string Code;
			public readonly string DescriptionOnGroup;
			public readonly string TriggerOn;
		}

		public static IncidentGroupStatusConfigurationParameter ConfigurationINV => new IncidentGroupStatusConfigurationParameter(1, Investigation, "Investigation", TriggerOnNew);
		public static IncidentGroupStatusConfigurationParameter ConfigurationESC => new IncidentGroupStatusConfigurationParameter(10, Escalated, "Escalated", TriggerOnEscalated);
		public static IncidentGroupStatusConfigurationParameter ConfigurationACI => new IncidentGroupStatusConfigurationParameter(20, ActiveIncident, "Active Incident", TriggerOnSendOpeningBroadcast);
		public static IncidentGroupStatusConfigurationParameter ConfigurationPSI => new IncidentGroupStatusConfigurationParameter(30, PostIncident, "Post Incident", TriggerOnSendClosingBroadcast);
		public static IncidentGroupStatusConfigurationParameter ConfigurationRSV => new IncidentGroupStatusConfigurationParameter(40, PIRCompleted, "PIR Completed", TriggerOnDefaultValue);

		public static ImmutableList<string> EnabledOnlyConfigurations
		{
			get => ImmutableList<string>.Empty.Add(ConfigurationINV.Code);
		}

		public static ZInt GetSequenceByCode(ZString code)
		{
			switch (code)
			{
				case Investigation:
					return ConfigurationINV.Sequence;
				case Escalated:
					return ConfigurationESC.Sequence;
				case ActiveIncident:
					return ConfigurationACI.Sequence;
				case PostIncident:
					return ConfigurationPSI.Sequence;
				case PIRCompleted:
					return ConfigurationRSV.Sequence;

				default:
					return 0;
			}
		}
	}
}
