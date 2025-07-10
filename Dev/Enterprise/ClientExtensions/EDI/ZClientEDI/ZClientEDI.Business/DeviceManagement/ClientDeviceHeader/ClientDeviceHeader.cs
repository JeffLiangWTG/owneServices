using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.RemoteDeviceManagement;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	[CodeProperty("Code")]
	[DescriptionProperty(Schema.CDH_Description)]
	public class ClientDeviceHeader : AutoDmgDeviceHeader
	{
		public ClientDeviceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CDH_Status = ClientDeviceHeaderLookups.Statuses.Active;
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("03e67845-4b7d-4ce6-8da6-f09393982d8a", "Device Header");

		#region Properties

		public ZString Code => CDH_IsTemplate ? CDH_ModelID : CDH_Identifier;

		public ZBool IsNotTemplate => !CDH_IsTemplate;

		[ReadOnly(true)]
		public override ZString CDH_ClientParentID
		{
			get => base.CDH_ClientParentID;
			set => base.CDH_ClientParentID = value;
		}

		[ReadOnly(true)]
		[List("Lookups.ClientParentTypes")]
		public override ZString CDH_ClientParentType
		{
			get => base.CDH_ClientParentType;
			set => base.CDH_ClientParentType = value;
		}

		[ResourceStringData("ClientDeviceHeader|ClientParentTypeDescription", Caption = "Customer Attached Type", ShortCaption = "Attached To", FullDescription = "Specifies what the customer has attached this device to.")]
		public ZString ClientParentTypeDescription
			=> Lookups.ClientParentTypes.GetDescriptionFromCode(CDH_ClientParentType);

		public new ClientDeviceHeaderValidation Validation => (ClientDeviceHeaderValidation)base.Validation;

		protected override DmgDeviceHeaderValidation GetNewValidation() => new ClientDeviceHeaderValidation(this);

		public new ClientDeviceHeaderLookups Lookups => (ClientDeviceHeaderLookups)base.Lookups;

		protected override DmgDeviceHeaderLookups GetNewLookups() => new ClientDeviceHeaderLookups(this);

		[ReadOnlyMember(nameof(CDH_Identifier_ReadOnly))]
		public override ZString CDH_Identifier
		{
			get => base.CDH_Identifier;
			set => base.CDH_Identifier = value;
		}

		protected bool CDH_Identifier_ReadOnly => CDH_IsTemplate || IsInDatabase || CDH_Identifier_UsingFountain;
		protected bool CDH_Identifier_UsingFountain { get; set; }

		[ReadOnly(true)]
		public override ZBool CDH_IsTemplate
		{
			get => base.CDH_IsTemplate;
			set => base.CDH_IsTemplate = value;
		}

		[ReadOnlyMember(DmgDeviceHeaderSchema.Constants.CDH_IsTemplate)]
		[List("Lookups.EnterpriseCodes")]
		public override ZString CDH_EnterpriseCode
		{
			get => base.CDH_EnterpriseCode;
			set
			{
				if (base.CDH_EnterpriseCode != value)
				{
					base.CDH_EnterpriseCode = value;
					CDH_ServerCode = ZString.Empty;
				}
			}
		}

		public override ZPropertyInfo CDH_EnterpriseCodeInfo => GetZPropertyInfo(nameof(CDH_EnterpriseCode));

		[ReadOnlyMember(nameof(CDH_ServerCode_ReadOnly))]
		[List("Lookups.ServerCodes")]
		public override ZString CDH_ServerCode
		{
			get => base.CDH_ServerCode;
			set => base.CDH_ServerCode = value;
		}

		public override ZPropertyInfo CDH_ServerCodeInfo => GetZPropertyInfo(nameof(CDH_ServerCode));

		public bool CDH_ServerCode_ReadOnly => CDH_IsTemplate || CDH_EnterpriseCode.IsEmpty || (!CDH_EnterpriseCode.IsEmpty && CDH_EnterpriseCodeInfo.HasErrors());

		[ResourceStringData("ClientDeviceHeader|StatusDescription", Caption = "Status", FullDescription = "The status of the device.")]
		public ZString StatusDescription => Lookups.StatusTypes.GetDescriptionFromCode(CDH_Status);

		[List("Lookups.StatusTypes")]
		public override ZString CDH_Status
		{
			get => base.CDH_Status;
			set => base.CDH_Status = value;
		}

		[List("Lookups.Organisations")]
		public ZGuid LicenceOrgPK => Licence?.LicEnterprise.LE_OH ?? ZGuid.Empty;

		public ZPropertyInfo LicenceOrgPKInfo => GetZPropertyInfo(nameof(LicenceOrgPK));

		[ResourceStringData("ClientDeviceHeader|LicenceEnterpriseCode", Caption = "License Code", ShortCaption = "License", FullDescription = "The license code of the customer system that this device is assigned to.")]
		public ZString LicenceEnterpriseCode => CDH_EnterpriseCode;

		public ZPropertyInfo LicenceEnterpriseCodeInfo => GetZPropertyInfo(nameof(LicenceEnterpriseCode));

		[ReadOnlyMember(nameof(IsNotTemplate))]
		public override ZString CDH_ModelID
		{
			get => base.CDH_ModelID;
			set => base.CDH_ModelID = value;
		}

		[ReadOnlyMember(nameof(IsNotTemplate))]
		public override ZString CDH_Description
		{
			get => base.CDH_Description;
			set => base.CDH_Description = value;
		}

		[List("Lookups.DeviceKinds")]
		[ReadOnlyMember(nameof(CDH_DeviceKind_ReadOnly))]
		public override ZString CDH_DeviceKind
		{
			get => base.CDH_DeviceKind;
			set => base.CDH_DeviceKind = value;
		}

		public ZBool CDH_DeviceKind_ReadOnly => IsNotTemplate && IsInDatabase && CDH_DeviceKind != ClientDeviceHeaderLookups.Kinds.Unknown && !CDH_DeviceKindInfo.HasChanges;

		[ReadOnlyMember(nameof(CDH_DeviceIdentifier_ReadOnly))]
		public override ZString CDH_DeviceIdentifier
		{
			get => base.CDH_DeviceIdentifier;
			set => base.CDH_DeviceIdentifier = value;
		}

		public ZBool CDH_DeviceIdentifier_ReadOnly => IsNotTemplate && IsInDatabase && !CDH_DeviceIdentifier.IsEmpty && !CDH_DeviceIdentifierInfo.HasChanges;

		[ReadOnly(true)]
		public ZBool RimRegistered => Factory.LoadTop1<ClientTelRimRegistration>(
			new ZQuery(
				new ZQuery(ClientTelRimRegistrationSchema.TRR_EnrolmentId, CDH_Identifier),
				JoinCondition.And,
				new ZQuery(ClientTelRimRegistrationSchema.TRR_EndTime, null))) != null;

		#endregion

		#region Related Objects

		[ChildEditable]
		public ClientDeviceComponentCollection Components
		{
			get
			{
				if (components == null)
				{
					components = new ClientDeviceComponentCollection(this);
					RegisterEditableChildObject(components);
				}

				return components;
			}
		}

		ClientDeviceComponentCollection components;

		[ChildEditable]
		public ClientTelRimRegistrationCollection RimRegistrations
		{
			get
			{
				if (rimRegistrations == null)
				{
					rimRegistrations = new ClientTelRimRegistrationCollection(this);
					RegisterEditableChildObject(rimRegistrations);
				}

				return rimRegistrations;
			}
		}

		ClientTelRimRegistrationCollection rimRegistrations;

		public LicenceDatabase Licence
		{
			get
			{
				return GetLicenseFromCodes(Factory, CDH_ServerCode, CDH_EnterpriseCode);
			}
		}

		static LicenceDatabase GetLicenseFromCodes(BusinessObjectFactory factory, ZString serverCode, ZString enterpriseCode)
		{
			if (serverCode.IsEmpty || enterpriseCode.IsEmpty)
			{
				return null;
			}

			var licenceQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
			licenceQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);

			var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
			enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);

			licenceQuery.AddSubQuery(enterpriseSubQuery, JoinCondition.And);

			var licence = factory.LoadTop1<LicenceDatabase>(licenceQuery);

			return licence;
		}

		public override void Delete()
		{
			base.Delete();
			Components.DeleteAll();
		}

		#endregion

		#region Saving

		internal void EnsureHasValidIdentifier()
		{
			if (!CDH_IsTemplate && CDH_Identifier.IsEmpty)
			{
				CDH_Identifier_UsingFountain = true;
				CDH_Identifier = ClientNumberFountainRegistration.GetInstance().TelematicsDeviceNumber.GetNextFormatted(Factory);
			}
		}

		void ClearCachedClientInfo()
		{
			if (CDH_ServerCodeInfo.HasChanges || CDH_EnterpriseCodeInfo.HasChanges)
			{
				CDH_ClientParentIDInfo.ClearValue();
				CDH_ClientParentTypeInfo.ClearValue();
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			EnsureHasValidIdentifier();
			ClearCachedClientInfo();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				CDH_IdentifierInfo.RefreshBinding();
			}
		}

		#endregion

		#region Clone

		public void PopulateDeviceFromModel(ClientDeviceHeader newDevice)
		{
			if (!CDH_IsTemplate)
			{
				throw new InvalidOperationException("You can only clone devices that are device models/templates.");
			}

			newDevice.CopyPersistentValuesFrom(this);
			newDevice.CDH_IsTemplate = false;
			foreach (var component in Components)
			{
				var newComponent = newDevice.Components.AddNew();
				newComponent.CopyPersistentValuesFrom(component, new BusinessObjectCloneArgs(new[] { DmgDeviceComponentSchema.CDC_CDH_Device.Name }));

				foreach (var identification in component.Identifiers)
				{
					var newIdentifier = newComponent.Identifiers.AddNew();
					newIdentifier.CopyPersistentValuesFrom(identification, new BusinessObjectCloneArgs(new[] { DmgDeviceComponentIdentificationSchema.CDD_CDC_Component.Name }));
				}
			}
		}

		#endregion
	}
}

