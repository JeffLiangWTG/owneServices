using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Registry.Business
{
	//This class is used to provide a factory for the ZAddress Control for registry as the RegistryBusinessObjectTemplate does not have a factory
	public class ShippingAgentObjectWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ShippingAgentObjectWrapper(ShippingAgentObject shippingObject)
			: base(new BusinessObjectFactory())
		{
			ShippingObject = shippingObject;
		}
		readonly ShippingAgentObject ShippingObject;

		class Schema
		{
			public const string ShippingAgentAddressOrgPK = "ShippingAgentAddressOrgPK";
		}

		[List("ShippingAgents")]
		public ZGuid ShippingAgentAddress
		{
			get { return ShippingObject.ShippingAgentAddress; }
			set
			{
				ShippingObject.ShippingAgentAddress = value;
				if (!IsValidationSuspended)
				{
					ValidateShippingAgentAddress();
				}
				ShippingAgentAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShippingAgentAddressInfo => GetZPropertyInfo(ShippingAgentObject.Schema.ShippingAgentAddress);

		public ZAddress ShippingAgentAddress_ZAddress
		{
			get
			{
				if (shippingAgentAddress_ZAddress == null)
				{
					shippingAgentAddress_ZAddress = new ZAddress(ShippingAgentAddressInfo)
					{
						DefaultAddressType = AddressType.OFC,
						OrgPKValidation = delegate(ZPropertyInfo info)
							{
								MandatoryValidation.CheckEntered(info);
								TypeValidation.CheckValidGuid(info);
							}
					};
				}
				return shippingAgentAddress_ZAddress;
			}
		}
		ZAddress shippingAgentAddress_ZAddress;

		public void ValidateShippingAgentAddress()
		{
			ShippingAgentAddressInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ShippingAgentAddressInfo);
			TypeValidation.CheckValidGuid(ShippingAgentAddressInfo);
		}

		public OrganisationsFindBoxCollection ShippingAgents => shippingAgents ?? (shippingAgents = new OrganisationsFindBoxCollection(Factory));
		OrganisationsFindBoxCollection shippingAgents;
	}
}
