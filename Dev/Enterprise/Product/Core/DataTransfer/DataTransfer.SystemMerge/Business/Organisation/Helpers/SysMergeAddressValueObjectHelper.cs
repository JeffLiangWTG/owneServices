using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeAddressValueObjectHelper
	{
		public SysMergeAddressValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public void ImportFromValueObjectCollection(Xsd.SysMergeOrgAddressCollection xsdAddressCollection, OrgHeaderForDataTransfer organisation, IValueObjectImportContext context)
		{
			if (organisation.Factory != context.Factory)
			{
				throw new InvalidOperationException("Factory for the Organisation should be the same as the Context's Factory");
			}

			if (xsdAddressCollection.IsSpecified)
			{
				for (int i = 0; i < xsdAddressCollection.Count; i++)
				{
					Xsd.SysMergeOrgAddress xsdAddress = xsdAddressCollection[i];
					CreateFromValueObject(organisation, xsdAddress, context);
				}
			}
			else
			{
				context.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, Res.GetString("2c537d0e-8344-4298-ba6f-4d785579ece5", "Address on organization {0}", ErrorContext)));
			}
		}

		protected OrgAddress CreateFromValueObject(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrgAddress xsdAddress, IValueObjectImportContext context)
		{
			OrgAddress newAddress = organisation.Factory.NewWithPrimaryKey<OrgAddress>(new Guid(xsdAddress.PK));
			(newAddress as ISupportDataImporting).IsImportingData = true;
			ImportAddressCapabilities(newAddress, xsdAddress);

			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_CompanyNameOverrideInfo, xsdAddress.CompanyNameOverride);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_Address1Info, xsdAddress.AddressLine1);

			if (xsdAddress.AddressLine1.IsEmpty)
			{
				context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("89555a8b-955a-403c-8545-9e2f991d301a", "Address Line 1 is required on Organization {0}", organisation.OH_Code)));
			}

			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_CodeInfo, xsdAddress.AddressCode);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_IsActiveInfo, xsdAddress.IsActive.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_LanguageInfo, xsdAddress.Language);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_Address2Info, xsdAddress.AddressLine2);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_AdditionalAddressInformationInfo, xsdAddress.AdditionalAddressInformation);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_CityInfo, xsdAddress.CityOrSuburb);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_StateInfo, xsdAddress.StateOrProvince);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_PostCodeInfo, xsdAddress.PostCode);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_RL_NKRelatedPortCodeInfo, xsdAddress.RL_NKRelatedPortCode);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_RN_NKCountryCodeInfo, xsdAddress.Country);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_PhoneInfo, xsdAddress.Phone);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_FaxInfo, xsdAddress.Fax);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_MobileInfo, xsdAddress.Mobile);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_EmailInfo, xsdAddress.Email);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_PickupFromTimeOnlyInfo, xsdAddress.PickupFromTimeOnly);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_PickupToTimeOnlyInfo, xsdAddress.PickupToTimeOnly);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_DeliverFromTimeOnlyInfo, xsdAddress.DeliverFromTimeOnly);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_DeliverToTimeOnlyInfo, xsdAddress.DeliverToTimeOnly);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_DoNotAttendFromInfo, xsdAddress.DoNotAttendFrom);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_DoNotAttendToInfo, xsdAddress.DoNotAttendTo);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_ForkLiftInfo, xsdAddress.ForkLift.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_PalletJackInfo, xsdAddress.PalletJack.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_ContainerHandlingInfo, xsdAddress.ContainerHandling);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_AccessPointInfo, xsdAddress.AccessPoint);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_LabourRequiredInfo, xsdAddress.LabourRequired);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_CommunicationRequiredInfo, xsdAddress.CommunicationRequired);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_Dock_HeightInfo, xsdAddress.DockHeight);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_OtherWarehouseFacilitiesInfo, xsdAddress.OtherWareHouseFacilities);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_LoadingUnloadingConstraintsInfo, xsdAddress.LoadingUnloadingConstraints);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_FCLEquipmentNeededInfo, xsdAddress.FCLEquipementNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_LCLEquipmentNeededInfo, xsdAddress.LCLEquipementNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_AIREquipmentNeededInfo, xsdAddress.AIREquipementNeeded);
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_LatitudeInfo, xsdAddress.Latitude.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newAddress.OA_LongitudeInfo, xsdAddress.Longitude.ToString());

			newAddress.OA_OH = organisation.PK;
			return newAddress;
		}

		void ImportAddressCapabilities(OrgAddress newAddress, Xsd.SysMergeOrgAddress xsdAddress)
		{
			foreach (Xsd.SysMergeOrgAddressCapability xsdCapability in xsdAddress.OrgAddressCapabilities)
			{
				OrgAddressCapability capability = newAddress.Factory.New<OrgAddressCapability>();

				if (xsdCapability.AddressTypeSpecified)
				{
					capability.PZ_AddressType = xsdCapability.AddressType;
				}

				if (xsdCapability.IsMainAddressSpecified)
				{
					capability.PZ_IsMainAddress = xsdCapability.IsMainAddress;
				}

				capability.PZ_OA = newAddress.PK;
			}
		}

		#endregion

		#region Export

		public void ExportToValueObjectCollection(OrgHeaderForDataTransfer org, Xsd.SysMergeOrgAddressCollection xsdAddressCollection, INotifications notifications)
		{
			ZQuery query = new ZQuery(OrgAddressSchema.OA_OH, org.PK);
			OrgAddress[] addresses = org.Factory.Load<OrgAddress>(query);

			foreach (OrgAddress address in addresses)
			{
				if (!address.OA_Address1.IsEmpty)
				{
					Xsd.SysMergeOrgAddress xsdAddress = ExportToValueObject(address, notifications);
					xsdAddressCollection.Add(xsdAddress);
				}
			}
		}

		public Xsd.SysMergeOrgAddress ExportToValueObject(OrgAddress address, INotifications notifications)
		{
			Xsd.SysMergeOrgAddress result = new Xsd.SysMergeOrgAddress();

			ExportAddressCapabilities(result, address, notifications);

			if (!address.PK.IsEmpty)
			{
				result.PK = address.PK.ToString();
			}

			if (address.OA_IsActive)
			{
				result.IsActive = address.OA_IsActive;
				result.IsActiveSpecified = true;
			}

			if (!address.OA_Code.IsEmpty)
			{
				result.AddressCode = address.OA_Code;
			}

			if (!address.OA_Language.IsEmpty)
			{
				result.Language = address.OA_Language;
			}

			if (!address.OA_CompanyNameOverride.IsEmpty)
			{
				result.CompanyNameOverride = address.OA_CompanyNameOverrideTruncated;
			}

			if (!address.OA_AdditionalAddressInformation.IsEmpty)
			{
				result.AdditionalAddressInformation = address.OA_AdditionalAddressInformation;
			}

			if (!address.OA_Address1.IsEmpty)
			{
				result.AddressLine1 = address.OA_Address1;
			}

			if (!address.OA_Address2.IsEmpty)
			{
				result.AddressLine2 = address.OA_Address2;
			}

			if (!address.OA_RN_NKCountryCode.IsEmpty)
			{
				result.Country = address.OA_RN_NKCountryCode;
			}

			if (!address.OA_City.IsEmpty)
			{
				result.CityOrSuburb = address.OA_City;
			}

			if (!address.OA_State.IsEmpty)
			{
				result.StateOrProvince = address.OA_State;
			}

			if (!address.OA_PostCode.IsEmpty)
			{
				result.PostCode = address.OA_PostCode;
			}

			if (!address.OA_Phone.IsEmpty)
			{
				result.Phone = address.OA_Phone;
			}

			if (!address.OA_Fax.IsEmpty)
			{
				result.Fax = address.OA_Fax;
			}

			if (!address.OA_Mobile.IsEmpty)
			{
				result.Mobile = address.OA_Mobile;
			}

			if (!address.OA_Email.IsEmpty)
			{
				result.Email = address.OA_Email;
			}

			if (!address.OA_PickupFromTimeOnly.IsEmpty)
			{
				result.PickupFromTimeOnly = address.OA_PickupFromTimeOnly;
			}

			if (!address.OA_PickupToTimeOnly.IsEmpty)
			{
				result.PickupToTimeOnly = address.OA_PickupToTimeOnly;
			}

			if (!address.OA_DeliverFromTimeOnly.IsEmpty)
			{
				result.DeliverFromTimeOnly = address.OA_DeliverFromTimeOnly;
			}

			if (!address.OA_DeliverToTimeOnly.IsEmpty)
			{
				result.DeliverToTimeOnly = address.OA_DeliverToTimeOnly;
			}

			if (!address.OA_DoNotAttendFrom.IsEmpty)
			{
				result.DoNotAttendFrom = address.OA_DoNotAttendFrom;
			}

			if (!address.OA_DoNotAttendTo.IsEmpty)
			{
				result.DoNotAttendTo = address.OA_DoNotAttendTo;
			}

			if (!address.OA_DockLeveler.IsEmpty)
			{
				result.DockLeveler = address.OA_DockLeveler;
			}

			if (!address.OA_ForkLift.IsEmpty)
			{
				result.ForkLift = address.OA_ForkLift;
			}

			if (!address.OA_PalletJack.IsEmpty)
			{
				result.PalletJack = address.OA_PalletJack;
			}

			if (!address.OA_ContainerHandling.IsEmpty)
			{
				result.ContainerHandling = address.OA_ContainerHandling;
			}

			if (!address.OA_AccessPoint.IsEmpty)
			{
				result.AccessPoint = address.OA_AccessPoint;
			}

			if (!address.OA_LabourRequired.IsEmpty)
			{
				result.LabourRequired = address.OA_LabourRequired;
			}

			if (!address.OA_CommunicationRequired.IsEmpty)
			{
				result.CommunicationRequired = address.OA_CommunicationRequired;
			}

			if (!address.OA_Dock_Height.IsEmpty)
			{
				result.DockHeight = address.OA_Dock_Height;
			}

			if (!address.OA_OtherWarehouseFacilities.IsEmpty)
			{
				result.OtherWareHouseFacilities = address.OA_OtherWarehouseFacilities;
			}

			if (!address.OA_LoadingUnloadingConstraints.IsEmpty)
			{
				result.LoadingUnloadingConstraints = address.OA_LoadingUnloadingConstraints;
			}

			if (!address.OA_FCLEquipmentNeeded.IsEmpty)
			{
				result.FCLEquipementNeeded = address.OA_FCLEquipmentNeeded;
			}

			if (!address.OA_LCLEquipmentNeeded.IsEmpty)
			{
				result.LCLEquipementNeeded = address.OA_LCLEquipmentNeeded;
			}

			if (!address.OA_AIREquipmentNeeded.IsEmpty)
			{
				result.AIREquipementNeeded = address.OA_AIREquipmentNeeded;
			}

			if (!address.OA_RL_NKRelatedPortCode.IsEmpty)
			{
				result.RL_NKRelatedPortCode = address.OA_RL_NKRelatedPortCode;
			}

			if (!address.OA_Latitude.IsEmpty)
			{
				result.Latitude = address.OA_Latitude;
			}

			if (!address.OA_Longitude.IsEmpty)
			{
				result.Longitude = address.OA_Longitude;
			}

			if (!address.OA_OH.IsEmpty)
			{
				result.OrgHeaderPK = address.OA_OH.ToString();
			}

			return result;
		}

		void ExportAddressCapabilities(Xsd.SysMergeOrgAddress xsdAddress, OrgAddress address, INotifications notifications)
		{
			ZQuery query = new ZQuery(OrgAddressCapabilitySchema.PZ_OA, address.PK);
			OrgAddressCapability[] capabilities = address.Factory.Load<OrgAddressCapability>(query);

			if (capabilities.Length > 0)
			{
				xsdAddress.OrgAddressCapabilities = new Xsd.SysMergeOrgAddressCapabilityCollection();

				foreach (OrgAddressCapability capability in capabilities)
				{
					Xsd.SysMergeOrgAddressCapability xsdCapability = xsdAddress.OrgAddressCapabilities.AddNew();
					xsdCapability.AddressType = capability.PZ_AddressType;
					xsdCapability.IsMainAddress = capability.PZ_IsMainAddress;

					// Set Specified flag for boolean/numeric fields so they get serialised to XML
					// Boolean
					xsdCapability.IsMainAddressSpecified = true;
				}
			}
		}

		#endregion
	}
}
