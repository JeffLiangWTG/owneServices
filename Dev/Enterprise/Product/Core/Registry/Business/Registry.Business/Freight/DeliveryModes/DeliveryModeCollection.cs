using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("DeliveryModeCollection")]
	public class DeliveryModeCollection : RegistryBusinessObjectCollection
	{
		public new DeliveryMode AddNew()
		{
			return (DeliveryMode)base.AddNew();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var mode = child as DeliveryMode;
			mode.IsSystemDefined = false;
			mode.CodeMaxLength = CodeMaxLength;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DeliveryModeCollection() { DefaultCode = this.DefaultCode };
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DeliveryMode() { IsSystemDefined = false };
		}

		public new DeliveryMode this[int i]
		{
			get { return (DeliveryMode)Elements[i]; }
		}

		public DeliveryMode this[string key]
		{
			get
			{
				foreach (DeliveryMode mode in this)
				{
					if (mode.Code == key)
					{
						return mode;
					}
				}

				return null;
			}
		}

		public ICodeDescriptionPairList ToCodeDescription()
		{
			var result = new CodeDescriptionPairList();

			for (var i = 0; i < this.Count; i++)
			{
				result.AddPair(this[i].Code, this[i].Description);
			}

			return result;
		}

		public ICodeDescriptionPairList ToUserDefinedCodeDescription()
		{
			if (!FreightDataRegistry.Instance.ContainerDeliveryModeOverride.Value)
			{
				return ToCodeDescription();
			}

			var result = new CodeDescriptionPairList();

			for (var i = 0; i < this.Count; i++)
			{
				result.AddPair(this[i].UserDefinedCode, this[i].UserDefinedDescription);
			}

			return result;
		}

		public ZString ConvertToCode(ZString userDefinedCode)
		{
			if (!FreightDataRegistry.Instance.ContainerDeliveryModeOverride.Value)
			{
				return userDefinedCode;
			}

			for (var i = 0; i < this.Count; i++)
			{
				if (this[i].UserDefinedCode == userDefinedCode)
				{
					return this[i].Code;
				}
			}

			return null;
		}

		public ZString ConvertToUserDefinedCode(ZString code)
		{
			if (!FreightDataRegistry.Instance.ContainerDeliveryModeOverride.Value)
			{
				return code;
			}

			for (var i = 0; i < this.Count; i++)
			{
				if (this[i].Code == code)
				{
					return this[i].UserDefinedCode;
				}
			}

			return null;
		}

		public string DefaultCode { get; set; }

		public static DeliveryModeCollection GetDefault()
		{
			var result = new DeliveryModeCollection();

			var mode = result.AddNew();
			mode.Code = Constants.DeliveryModes.Codes.CFS_CFS;
			mode.Description = Constants.DeliveryModes.Descriptions.CFS_CFS;
			mode.UserDefinedCode = Constants.DeliveryModes.Codes.CFS_CFS;
			mode.UserDefinedDescription = Constants.DeliveryModes.Descriptions.CFS_CFS;
			mode.IsSystemDefined = true;

			mode = result.AddNew();
			mode.Code = Constants.DeliveryModes.Codes.CY_CY;
			mode.Description = Constants.DeliveryModes.Descriptions.CY_CY;
			mode.UserDefinedCode = Constants.DeliveryModes.Codes.CY_CY;
			mode.UserDefinedDescription = Constants.DeliveryModes.Descriptions.CY_CY;
			mode.IsSystemDefined = true;

			mode = result.AddNew();
			mode.Code = Constants.DeliveryModes.Codes.CY_CFS;
			mode.Description = Constants.DeliveryModes.Descriptions.CY_CFS;
			mode.UserDefinedCode = Constants.DeliveryModes.Codes.CY_CFS;
			mode.UserDefinedDescription = Constants.DeliveryModes.Descriptions.CY_CFS;
			mode.IsSystemDefined = true;

			mode = result.AddNew();
			mode.Code = Constants.DeliveryModes.Codes.CFS_CY;
			mode.Description = Constants.DeliveryModes.Descriptions.CFS_CY;
			mode.UserDefinedCode = Constants.DeliveryModes.Codes.CFS_CY;
			mode.UserDefinedDescription = Constants.DeliveryModes.Descriptions.CFS_CY;
			mode.IsSystemDefined = true;

			result.DefaultCode = Constants.DeliveryModes.Codes.CFS_CFS;
			return result;
		}
	}
}
