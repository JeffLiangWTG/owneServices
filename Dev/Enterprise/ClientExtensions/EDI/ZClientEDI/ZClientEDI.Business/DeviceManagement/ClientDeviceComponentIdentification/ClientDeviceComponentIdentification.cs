using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.RemoteDeviceManagement;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceComponentIdentification : AutoDmgDeviceComponentIdentification
	{
		public ClientDeviceComponentIdentification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.IdentificationTypeList")]
		public override ZString CDD_IdentificationType
		{
			get => base.CDD_IdentificationType;
			set => base.CDD_IdentificationType = value;
		}

		[ResourceStringData("ClientDeviceComponentIdentification|IdentificationTypeDescription", Caption = "Identification Type", FullDescription = "The type of this ID number.")]
		public ZString IdentificationTypeDescription => Lookups.IdentificationTypeList.GetDescriptionFromCode(CDD_IdentificationType);

		public new ClientDeviceComponentIdentificationLookups Lookups => (ClientDeviceComponentIdentificationLookups)base.Lookups;

		protected override DmgDeviceComponentIdentificationLookups GetNewLookups() => new ClientDeviceComponentIdentificationLookups(this);

		[RelatedBusinessObject("Component")]
		public override ZGuid CDD_CDC_Component
		{
			get => base.CDD_CDC_Component;
			set => base.CDD_CDC_Component = value;
		}

		public new ClientDeviceComponentIdentificationValidation Validation => (ClientDeviceComponentIdentificationValidation)base.Validation;

		protected override DmgDeviceComponentIdentificationValidation GetNewValidation() => new ClientDeviceComponentIdentificationValidation(this);

		public ClientDeviceComponent Component => Factory.Load<ClientDeviceComponent>(CDD_CDC_Component);

		#endregion
	}
}

