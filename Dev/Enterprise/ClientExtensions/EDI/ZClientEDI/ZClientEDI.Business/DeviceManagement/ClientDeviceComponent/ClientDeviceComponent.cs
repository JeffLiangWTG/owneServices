using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.RemoteDeviceManagement;
using Enterprise.ZArchitecture.Core;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceComponent : AutoDmgDeviceComponent
	{
		public ClientDeviceComponent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public ZBool IsTemplate => Device.CDH_IsTemplate;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("B28B52D7-B7AF-4C2B-A8F7-09767EACED5B", "Components with a Software Readable Identifier should not be deleted");

		[List("Lookups.ComponentTypeList")]
		public override ZString CDC_ComponentType
		{
			get => base.CDC_ComponentType;
			set => base.CDC_ComponentType = value;
		}

		[ResourceStringData("ClientDeviceComponent|ComponentTypeDescription", Caption = "Component Type", FullDescription = "The type of this specific component.")]
		public ZString ComponentTypeDescription => Lookups.ComponentTypeList.GetDescriptionFromCode(CDC_ComponentType);

		public new ClientDeviceComponentValidation Validation => (ClientDeviceComponentValidation)base.Validation;

		protected override DmgDeviceComponentValidation GetNewValidation() => new ClientDeviceComponentValidation(this);

		public new ClientDeviceComponentLookups Lookups => (ClientDeviceComponentLookups)base.Lookups;

		protected override DmgDeviceComponentLookups GetNewLookups() => new ClientDeviceComponentLookups(this);

		[ReadOnlyMember(nameof(IsTemplate))]
		public override ZBlob CDC_LastKnownDeviceInfoData
		{
			get => base.CDC_LastKnownDeviceInfoData;
			set => base.CDC_LastKnownDeviceInfoData = value;
		}

		[RelatedBusinessObject("Device")]
		public override ZGuid CDC_CDH_Device
		{
			get => base.CDC_CDH_Device;
			set => base.CDC_CDH_Device = value;
		}

		public ClientDeviceHeader Device => Factory.Load<ClientDeviceHeader>(CDC_CDH_Device);

		#endregion

		#region Related Objects

		[ChildEditable]
		public ClientDeviceComponentIdentificationCollection Identifiers
		{
			get
			{
				if (identifiers == null)
				{
					identifiers = new ClientDeviceComponentIdentificationCollection(this);
					RegisterEditableChildObject(identifiers);
				}

				return identifiers;
			}
		}

		ClientDeviceComponentIdentificationCollection identifiers;

		public override void Delete()
		{
			base.Delete();
			Identifiers.DeleteAll();
		}

		#endregion
	}
}

