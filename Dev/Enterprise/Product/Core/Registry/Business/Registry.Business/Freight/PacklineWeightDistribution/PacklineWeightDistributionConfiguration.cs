using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class PacklineWeightDistributionConfiguration : RegistryBusinessObjectTemplate
	{
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			EnablePacklineWeightDistribution = ZBool.False;
			EnableActualWeightDistribution = ZBool.False;
			EnableVolumetricWeightDistribution = ZBool.False;
		}

		#region Schema

		public static class Schema
		{
			public const string EnablePacklineWeightDistribution = "EnablePacklineWeightDistribution";
			public const string EnableActualWeightDistribution = "EnableActualWeightDistribution";
			public const string EnableVolumetricWeightDistribution = "EnableVolumetricWeightDistribution";
		}
		#endregion

		#region Clone
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PacklineWeightDistributionConfiguration
			{
				EnablePacklineWeightDistribution = EnablePacklineWeightDistribution,
				EnableActualWeightDistribution = EnableActualWeightDistribution,
				EnableVolumetricWeightDistribution = EnableVolumetricWeightDistribution,
			};
		}
		#endregion

		#region Properties

		#region EnablePacklineWeightDistribution
		public ZBool EnablePacklineWeightDistribution
		{
			get => enablePacklineWeightDistribution;
			set
			{
				if (SetNonPersistentPropertyValue(EnablePacklineWeightDistributionInfo, ref enablePacklineWeightDistribution, value))
				{
					ValidateEnablePacklineWeightDistribution();
				}
			}
		}
		public ZPropertyInfo EnablePacklineWeightDistributionInfo => GetZPropertyInfo(Schema.EnablePacklineWeightDistribution);
		ZBool enablePacklineWeightDistribution;

		#endregion

		#region EnableActualWeightDistribution
		public ZBool EnableActualWeightDistribution
		{
			get => enableActualWeightDistribution;
			set
			{
				if (SetNonPersistentPropertyValue(EnableActualWeightDistributionInfo, ref enableActualWeightDistribution, value))
				{
					ValidateEnableActualWeightDistribution();
				}
			}
		}
		public ZPropertyInfo EnableActualWeightDistributionInfo => GetZPropertyInfo(Schema.EnableActualWeightDistribution);
		ZBool enableActualWeightDistribution;

		#endregion

		#region EnablePacklineWeightDistribution
		public ZBool EnableVolumetricWeightDistribution
		{
			get => enableVolumetricWeightDistribution;
			set
			{
				if (SetNonPersistentPropertyValue(EnableVolumetricWeightDistributionInfo, ref enableVolumetricWeightDistribution, value))
				{
					ValidateEnableVolumetricWeightDistribution();
				}
			}
		}
		public ZPropertyInfo EnableVolumetricWeightDistributionInfo => GetZPropertyInfo(Schema.EnableVolumetricWeightDistribution);
		ZBool enableVolumetricWeightDistribution;

		#endregion

		#endregion

		#region Validation

		public void ValidateEnablePacklineWeightDistribution()
		{
			EnablePacklineWeightDistributionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EnablePacklineWeightDistributionInfo);
		}

		public void ValidateEnableActualWeightDistribution()
		{
			EnableActualWeightDistributionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EnableActualWeightDistributionInfo);
		}

		public void ValidateEnableVolumetricWeightDistribution()
		{
			EnableVolumetricWeightDistributionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EnableVolumetricWeightDistributionInfo);
		}

		#endregion

		#region Xml Serialization
		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EnablePacklineWeightDistribution, EnablePacklineWeightDistribution.ToString());
			writer.WriteElementString(Schema.EnableActualWeightDistribution, EnableActualWeightDistribution.ToString());
			writer.WriteElementString(Schema.EnableVolumetricWeightDistribution, EnableVolumetricWeightDistribution.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnablePacklineWeightDistribution = new ZBool(reader.ReadElementString(Schema.EnablePacklineWeightDistribution));
			EnableActualWeightDistribution = new ZBool(reader.ReadElementString(Schema.EnableActualWeightDistribution));
			EnableVolumetricWeightDistribution = new ZBool(reader.ReadElementString(Schema.EnableVolumetricWeightDistribution));
		}
		#endregion
	}
}
