using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AddressMatchingLevelBusinessObject : RegistryBusinessObjectTemplate
	{
		public AddressMatchingLevelBusinessObject() : this(MatchingRules.Balanced)
		{
		}

		public AddressMatchingLevelBusinessObject(MatchingRules rules)
		{
			using (GetValidationSuspender())
			{
				MatchingLevel = rules.ToString();
			}
			HasChanges = false;
		}

		public AddressMatchingLevelBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AddressMatchingLevelBusinessObject();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			using (GetValidationSuspender())
			{
				MatchingLevel = reader.ReadElementString(Schema.MatchingLevel);
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.MatchingLevel, MatchingLevel.ToString());
		}

		#region Schema

		public static class Schema
		{
			public const string MatchingLevel = nameof(MatchingLevel);
		}

		#endregion

		#region Properties

		public ZBool StrictRadioButtonSelection
		{
			get => MatchingLevel == nameof(MatchingRules.Strict);
			set => MatchingLevel = nameof(MatchingRules.Strict);
		}

		public ZBool BalancedRadioButtonSelection
		{
			get => MatchingLevel == nameof(MatchingRules.Balanced);
			set => MatchingLevel = nameof(MatchingRules.Balanced);
		}

		public ZBool ComprehensiveRadioButtonSelection
		{
			get => MatchingLevel == nameof(MatchingRules.Comprehensive);
			set => MatchingLevel = nameof(MatchingRules.Comprehensive);
		}

		public ZString MatchingLevel
		{
			get => matchingLevel;
			set => SetNonPersistentPropertyValue(MatchingLevelInfo, ref matchingLevel, value);
		}
		ZString matchingLevel;

		public ZPropertyInfo MatchingLevelInfo => GetZPropertyInfo(Schema.MatchingLevel);

		#endregion
	}
}
