using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ExportJobDeclarationValidation : JobDeclarationValidation
	{
		public ExportJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();
			if (CompanyOrgProxy != null && !Parent.IsG7ExportDeclaration)
			{
				if (CompanyOrgProxy.MainAddress.OA_State.IsEmpty)
				{
					Parent.JE_MessageTypeInfo.AddMessageError(CompanyOrgProxyStateIsRequired);
				}

				if (IsPhoneEmpty(CompanyOrgProxy.MainAddress.OA_Phone) && (CompanyOrgProxy.CountryCode == Core.Constants.CountryCodes.Canada || CompanyOrgProxy.CountryCode == Core.Constants.CountryCodes.UnitedStates))
				{
					Parent.JE_MessageTypeInfo.AddMessageError(CompanyOrgProxyPhoneIsRequired);
				}
			}
		}
		internal static string CompanyOrgProxyStateIsRequired
		{
			get { return Res.GetString("30e179e7-cf10-4c38-a893-eb07f4318c5e", "A province/state is required for messaging to Customs. Please setup a province/state for the current company organization proxy (Maintain -> User Admin -> Companies -> Find current company -> Organization Proxy -> Press F3 -> State)."); }
		}

		internal static string CompanyOrgProxyPhoneIsRequired
		{
			get { return Res.GetString("d137167c-6fa5-49da-99da-7b3469504b6b", "A telephone number is required for messaging to Customs. Please setup a telephone number for the current company organization proxy (Maintain -> User Admin -> Companies -> Find current company -> Organization Proxy -> Press F3 -> Phone)."); }
		}

		protected override void CheckJE_TotalWeightUnit()
		{
			base.CheckJE_TotalWeightUnit();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalWeightUnitInfo, Res.GetString("ab29260d-ea40-4fb7-af5a-c96939455dc1", "Total Weight Unit"));
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo, Res.GetString("72761171-a71d-411a-8225-5f89fe41bc0f", "Vessel; a Vessel is required when the transport mode is sea"));
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_RL_NKFinalDestinationInfo, Parent.Lookups.FinalDestinations);
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExportDateInfo);
		}

		protected override void CheckJE_TotalNoOfPacks()
		{
			base.CheckJE_TotalNoOfPacks();
			MandatoryValidation.MessageErrorIfIsZero(Parent.JE_TotalNoOfPacksInfo, Res.GetString("72b35d46-a7d1-4f81-bae9-61a105d01679", "No. Of Packages"));
		}

		protected override void CheckJE_TotalNoOfPacksPackType()
		{
			base.CheckJE_TotalNoOfPacksPackType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TotalNoOfPacksPackTypeInfo, Parent.Lookups.JE_TotalNoOfPacksPackType_List, Res.GetString("8949d409-74de-4b46-8865-2a9883e5d230", "Packages Type"));
		}

		protected override void CheckJE_OA_SupplierAddress()
		{
			base.CheckJE_OA_SupplierAddress();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_SupplierAddressInfo, Res.GetString("22704FEC-23F2-43E8-87BA-6B315358E226", "Exporter"));
			if (!Parent.JE_OA_SupplierAddress.IsEmpty)
			{
				CheckHasExporterAuthorizationID();
				CheckExporterHasBusinessNumber();

				var exporterAddress = Parent.SupplierAddress;
				if (exporterAddress != null)
				{
					if (Parent.IsG7ExportDeclaration)
					{
						CAAddressValidator.Validate(exporterAddress, Parent.JE_OA_SupplierAddressInfo);
					}
					else
					{
						CAAddressValidator.PostCodeValidation(exporterAddress.Postcode, Parent.JE_OA_SupplierAddressInfo, exporterAddress.OA_RN_NKCountryCode);
						CAAddressValidator.StateValidation(exporterAddress.StateCode, Parent.JE_OA_SupplierAddressInfo, exporterAddress.OA_RN_NKCountryCode);
					}
					OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.JE_OA_SupplierAddressInfo, exporterAddress);
				}
			}
		}

		void CheckExporterHasBusinessNumber()
		{
			OrgHeader exporter = Parent.Supplier;
			if (exporter != null)
			{
				ZString businessNumber = exporter.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForExport, Core.Constants.CountryCodes.Canada);
				if (businessNumber.IsEmpty)
				{
					businessNumber = exporter.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada);
					if (businessNumber.IsEmpty)
					{
						Parent.JE_OA_SupplierAddressInfo.AddMessageError(ExporterBusinessNumberIsRequired);
					}
					else
					{
						ZString error = CanadianCustomsCodeValidator.GetBusinessNumberForImportExportError(businessNumber);
						if (!error.IsEmpty)
						{
							Parent.JE_OA_SupplierAddressInfo.AddMessageError(error);
						}
					}
				}
				else
				{
					ZString error = CanadianCustomsCodeValidator.GetBusinessNumberForExportError(businessNumber);
					if (!error.IsEmpty)
					{
						Parent.JE_OA_SupplierAddressInfo.AddMessageError(error);
					}
				}
			}
		}
		internal static string ExporterBusinessNumberIsRequired
		{
			get { return Res.GetString("747d32d6-4a31-48d5-aaa6-cd84ba7551a3", "The exporter does not have a business number for import/export ({0}) or Export({1}) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes to add the business number.", OrgCusCode.CACodeTypes.BusinessNumberForImportExport, OrgCusCode.CACodeTypes.BusinessNumberForExport); }
		}

		void CheckHasExporterAuthorizationID()
		{
			OrgHeader exporter = Parent.Supplier;
			if (exporter != null)
			{
				ZString authorizationID = exporter.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.AuthorizationID);
				if (authorizationID.IsEmpty)
				{
					Parent.JE_OA_SupplierAddressInfo.AddMessageError(ExporterAuthorizationIDIsRequired);
				}
				else
				{
					ZString error = CanadianCustomsCodeValidator.GetAuthorizationIDError(authorizationID);
					if (!error.IsEmpty)
					{
						Parent.JE_OA_SupplierAddressInfo.AddMessageError(error);
					}
				}
			}
		}
		internal static string ExporterAuthorizationIDIsRequired
		{
			get { return Res.GetString("256a9f10-1660-4b55-b90c-4c78cf214d4a", "This exporter does not have an Authorization ID ({0}) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.", OrgCusCode.CACodeTypes.AuthorizationID); }
		}

		protected override void CheckJE_RL_NKPortOfFirstArrival()
		{
			// this port is not applicable to export
		}

		protected override void CheckJE_OH_Forwarder()
		{
			base.CheckJE_OH_Forwarder();
			if (!Parent.IsG7ExportDeclaration)
			{
				CheckServiceProviderAuthorizationID();
				CheckServiceProviderPhone();
				CheckServiceProviderPostCode();
			}
		}

		void CheckServiceProviderPostCode()
		{
			OrgHeader serviceProvider = Parent.Forwarder;
			if (serviceProvider != null)
			{
				ZString postCode = serviceProvider.MainAddress.OA_PostCode.Replace(" ", "");
				if (serviceProvider.CountryCode == Core.Constants.CountryCodes.Canada && !Regex.IsMatch(postCode, @"^[a-z][0-9][a-z][0-9][a-z][0-9]$", RegexOptions.IgnoreCase))
				{
					Parent.JE_OH_ForwarderInfo.AddMessageError(CanadianServiceProviderPostCodeFormat);
				}
			}
		}
		internal static string CanadianServiceProviderPostCodeFormat
		{
			get { return Res.GetString("6890cd6a-d3ea-41d3-91e1-8fa6d43940b4", "This service provider's postal code is invalid. A Canadian postal code should be in the following format: A9A9A9, where A is a letter and 9 is a digit."); }
		}

		void CheckServiceProviderPhone()
		{
			OrgHeader serviceProvider = Parent.Forwarder;
			if (serviceProvider != null && IsPhoneEmpty(serviceProvider.MainAddress.OA_Phone))
			{
				Parent.JE_OH_ForwarderInfo.AddMessageError(ServiceProviderPhoneIsRequired);
			}
		}
		internal static string ServiceProviderPhoneIsRequired
		{
			get { return Res.GetString("6c6bf998-074e-48f1-8a67-7d5597467581", "A telephone number is required by Customs when a service provider is used. Please press F3 in the field and a telephone number."); }
		}

		void CheckServiceProviderAuthorizationID()
		{
			OrgHeader serviceProvider = Parent.Forwarder;
			ZString authorizationID = ZString.Empty;
			if (serviceProvider == null)
			{
				if (!Parent.IsCompanyOrgProxyTheExporter)
				{
					Parent.JE_OH_ForwarderInfo.AddMessageError(ServiceProviderIsRequired);
				}
			}
			else
			{
				authorizationID = serviceProvider.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.AuthorizationID);
				if (!authorizationID.IsEmpty || !Parent.IsCompanyOrgProxyTheExporter)
				{
					ZString error = CanadianCustomsCodeValidator.GetServiceProviderAuthorizationIDError(authorizationID);
					if (!error.IsEmpty)
					{
						Parent.JE_OH_ForwarderInfo.AddMessageError(error);
					}
				}
			}
		}
		internal static string ServiceProviderIsRequired
		{
			get { return Res.GetString("b452fcde-5899-4c84-b114-86407e098056", "A service provider is required when the exporter is not your company."); }
		}

		void CheckImporterAddress()
		{
			var importersAddress = Parent.ImporterAddress;
			if (importersAddress != null)
			{
				if (Parent.IsG7ExportDeclaration)
				{
					CAAddressValidator.ValidateMandatory(importersAddress, Parent.JE_OA_ImporterAddressInfo, Res.GetString("9cbe82ef-b7d6-4ea7-9fd1-0827fbfc8066", "Consignee"));
				}
				OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.JE_OA_ImporterAddressInfo, importersAddress);
				OrganisationValidation.ValidateCity(Parent.JE_OA_ImporterAddressInfo, importersAddress);
				OrganisationValidation.ValidateCountry(Parent.JE_OA_ImporterAddressInfo, importersAddress);
			}
		}

		protected override void CheckJE_OA_ImporterAddress()
		{
			base.CheckJE_OA_ImporterAddress();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_ImporterAddressInfo, Res.GetString("9cbe82ef-b7d6-4ea7-9fd1-0827fbfc8066", "Consignee"));
			CheckImporterAddress();
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (Parent.IsSea && Parent.IsContainerised && Parent.CusContainers.Count < 1)
			{
				Parent.JE_TransportModeInfo.AddMessageError(ContainerIsRequiredForSea);
			}
			ValidateJE_ContainerMode();
		}
		internal static string ContainerIsRequiredForSea
		{
			get { return Res.GetString("4f3a48e9-4fdb-47eb-825f-060f5e2a5dc5", "At least one container must be specified if transport mode is containerized marine."); }
		}

		protected override void CheckJE_ContainerMode()
		{
			base.CheckJE_ContainerMode();
			ValidateJE_TransportMode();
		}

		OrgHeader CompanyOrgProxy
		{
			get { return GlbCompany.CurrentCompany.OrgProxy == null ? null : Parent.Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK); }
		}

		bool IsPhoneEmpty(ZString phone)
		{
			return phone.Replace(" ", "").IsEmpty;
		}
	}
}
