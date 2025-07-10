using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class EventLogsListProvidersAndTheirCapacity : AutoEventLogsListProvidersAndTheirCapacity
	{
		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventLogsListProvidersAndTheirCapacity();
		}

		#endregion

		[List("GlobalCapabilityList")]
		public override ZGuid CapabilityCode
		{
			get { return base.CapabilityCode; }
			set	{ base.CapabilityCode = value;	}
		}

		GlbCapabilityCollection globalCapabilityList;
		public GlbCapabilityCollection GlobalCapabilityList
		{
			get
			{
				return globalCapabilityList ?? (globalCapabilityList = new GlbCapabilityCollection(CurrentFactory));
			}
		}

		#region Validation

		public override void ValidateProviderCode()
		{
			base.ValidateProviderCode();
			MandatoryValidation.CheckEntered(ProviderCodeInfo);
		}

		public override void ValidateLevelCode()
		{
			base.ValidateLevelCode();
			MandatoryValidation.CheckEntered(LevelCodeInfo);
		}

		public override void ValidateCapabilityCode()
		{
			base.ValidateCapabilityCode();
			ListValidation.ErrorIfInvalidPK(CapabilityCodeInfo, GlobalCapabilityList);
		}

		#endregion
	}
}

