using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AutomaticContainerCreation : RegistryBusinessObjectTemplate
	{
		public AutomaticContainerCreation() : this(CreateRules.CreateUpToATDOrShippingInstruction)
		{
		}

		public AutomaticContainerCreation(CreateRules rule)
		{
			using (GetValidationSuspender())
			{
				CreateConfiguration = rule.ToString();
			}
			HasChanges = false;
		}

		#region Schema

		public static class Schema
		{
			public const string CreateConfiguration = nameof(CreateConfiguration);
		}

		#endregion

		#region Properties

		public ZBool IsAlwaysCreate
		{
			get => CreateConfiguration == nameof(CreateRules.AlwaysCreate);
			set
			{
				if (value)
				{
					CreateConfiguration = nameof(CreateRules.AlwaysCreate);
				}
			}
		}

		public ZBool IsNeverCreate
		{
			get => CreateConfiguration == nameof(CreateRules.NeverCreate);
			set
			{
				if (value)
				{
					CreateConfiguration = nameof(CreateRules.NeverCreate);
				}
			}
		}

		public ZBool IsCreateUpToATDOrShippingInstruction
		{
			get => CreateConfiguration == nameof(CreateRules.CreateUpToATDOrShippingInstruction);
			set
			{
				if (value)
				{
					CreateConfiguration = nameof(CreateRules.CreateUpToATDOrShippingInstruction);
				}
			}
		}

		public ZString CreateConfiguration
		{
			get => createConfiguration;
			set => SetNonPersistentPropertyValue(CreateConfigurationInfo, ref createConfiguration, value);
		}
		ZString createConfiguration;

		public ZPropertyInfo CreateConfigurationInfo => GetZPropertyInfo(Schema.CreateConfiguration);

		#endregion

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CreateConfiguration, CreateConfiguration.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			using (GetValidationSuspender())
			{
				CreateConfiguration = reader.ReadElementString(Schema.CreateConfiguration);
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutomaticContainerCreation()
			{
				CreateConfiguration = CreateConfiguration
			};
		}

		public enum CreateRules
		{
			AlwaysCreate,
			NeverCreate,
			CreateUpToATDOrShippingInstruction
		}
	}
}
