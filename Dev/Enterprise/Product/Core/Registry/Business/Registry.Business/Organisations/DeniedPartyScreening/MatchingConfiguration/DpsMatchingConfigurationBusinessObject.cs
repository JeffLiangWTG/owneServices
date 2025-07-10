using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DpsMatchingConfigurationBusinessObject : RegistryBusinessObjectTemplate
	{
		public DpsMatchingConfigurationBusinessObject() : this(MatchingRules.Strict)
		{
		}

		public DpsMatchingConfigurationBusinessObject(MatchingRules rules)
		{
			using (GetValidationSuspender())
			{
				MatchingConfiguration = rules.ToString();
			}
			HasChanges = false;
		}

		public DpsMatchingConfigurationBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DpsMatchingConfigurationBusinessObject();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			using (GetValidationSuspender())
			{
				MatchingConfiguration = reader.ReadElementString(Schema.MatchingConfiguration);
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.MatchingConfiguration, MatchingConfiguration.ToString());
		}

		#region Schema

		public static class Schema
		{
			public const string MatchingConfiguration = nameof(MatchingConfiguration);
		}

		#endregion

		#region Properties

		public ZBool StrictRadioButtonSelection
		{
			get => MatchingConfiguration == nameof(MatchingRules.Strict);
			set => MatchingConfiguration = nameof(MatchingRules.Strict);
		}

		public ZBool BalancedRadioButtonSelection
		{
			get => MatchingConfiguration == nameof(MatchingRules.Balanced);
			set => MatchingConfiguration = nameof(MatchingRules.Balanced);
		}

		public ZBool ComprehensiveRadioButtonSelection
		{
			get => MatchingConfiguration == nameof(MatchingRules.Comprehensive);
			set => MatchingConfiguration = nameof(MatchingRules.Comprehensive);
		}

		public ZString MatchingConfiguration
		{
			get => matchingConfiguration;
			set => SetNonPersistentPropertyValue(MatchingConfigurationInfo, ref matchingConfiguration, value);
		}
		ZString matchingConfiguration;

		public ZPropertyInfo MatchingConfigurationInfo => GetZPropertyInfo(Schema.MatchingConfiguration);

		#endregion
	}

	public enum MatchingRules
	{
		Strict,
		Balanced,
		Comprehensive
	}
}
