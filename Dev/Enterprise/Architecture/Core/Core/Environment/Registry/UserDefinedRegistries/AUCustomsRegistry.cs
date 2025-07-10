using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class AUCustomsRegistry
	{
		public AUCustomsRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		public string LocalCustomsBranchIdentifier
		{
			get { return (string)RawRegistry.LocalCustomsBranchIdentifier.Value; }
#if DEBUG
			set { RawRegistry.LocalCustomsBranchIdentifier.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public void SetLocalCustomsBranchIdentifierForBranch(Guid branch, string value)
		{
			RawRegistry.LocalCustomsBranchIdentifier.SetValue(Guid.Empty, branch, Guid.Empty, value);
		}

		public string GetLocalCustomsBranchIdentifierForBranch(Guid branch)
		{
			return RawRegistry.LocalCustomsBranchIdentifier.GetValueWithoutFallback(Guid.Empty, branch, Guid.Empty) as string;
		}

		public string PreLodgementLicenceCode
		{
			get { return RawRegistry.PreLodgementLicenceCode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PreLodgementLicenceCode.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public string AgentsReferenceDefaulting
		{
			get { return RawRegistry.AgentsReferenceDefaulting.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AgentsReferenceDefaulting.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#region CMR

		public DateTime LastDateCertificatesChecked
		{
			get { return (DateTime)RawRegistry.LastDateCertificatesChecked.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.LastDateCertificatesChecked.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public Guid CarrierMovementAdviceGroup
		{
			get { return new Guid(RawRegistry.AUCCarrierMovementAdviceGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty).ToString()); }
			set { RawRegistry.AUCCarrierMovementAdviceGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public Guid GetCarrierMovementAdviceGroupForBranchOrCurrentCompany(Guid branch)
		{
			return new Guid(RawRegistry.AUCCarrierMovementAdviceGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, branch, Guid.Empty).ToString());
		}

		public void SetCarrierMovementAdviceGroupForBranch(Guid branch, Guid value)
		{
			RawRegistry.AUCCarrierMovementAdviceGroup.SetValue(Guid.Empty, branch, Guid.Empty, value);
		}

		public void DeleteCarrierMovementAdviceGroupForBranch(Guid branch)
		{
			((IRegistryItemInternals)RawRegistry.AUCCarrierMovementAdviceGroup).DeleteValue(Guid.Empty, branch, Guid.Empty);
		}

		public bool AlertCarrierMovementLoad
		{
			get { return (bool)RawRegistry.AUCAlertCarrierMovementLoadStatus.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCAlertCarrierMovementLoadStatus.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string HVLVSpecialReporterNumber
		{
			get { return (string)RawRegistry.AUCHVLVSpecialReporterNumber.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCHVLVSpecialReporterNumber.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string RemailSpecialReporterNumber
		{
			get { return (string)RawRegistry.AUCRemailSpecialReporterNumber.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.AUCRemailSpecialReporterNumber.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region Air Cargo

		public string AirCargoSendErrors
		{
			get { return (string)RawRegistry.AirCargoSendErrors.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AirCargoSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid AirCargoSendErrorsToGroup
		{
			get { return (Guid)RawRegistry.AirCargoSendErrorsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AirCargoSendErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string AirCargoSendAcknowledgements
		{
			get { return (string)RawRegistry.AirCargoSendAcknowledgements.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AirCargoSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid AirCargoSendAcknowledgementsToGroup
		{
			get { return (Guid)RawRegistry.AirCargoSendAcknowledgementsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public string AirCargoSendImpediments
		{
			get { return (string)RawRegistry.AirCargoSendImpediments.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AirCargoSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid AirCargoSendImpedimentsToGroup
		{
			get { return (Guid)RawRegistry.AirCargoSendImpedimentsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList AirCargoShipmentType
		{
			get { return RawRegistry.AirCargoShipmentType.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList AirCargoCommercialStatus
		{
			get { return RawRegistry.AirCargoCommercialStatus.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public string HVLVAirCargoSendErrors
		{
			get { return (string)RawRegistry.HVLVAirCargoSendErrors.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.HVLVAirCargoSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string HVLVAirCargoSendAcknowledgements
		{
			get { return (string)RawRegistry.HVLVAirCargoSendAcknowledgements.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.HVLVAirCargoSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string HVLVAirCargoSendImpediments
		{
			get { return (string)RawRegistry.HVLVAirCargoSendImpediments.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.HVLVAirCargoSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Edifice

		public string EdificeSendErrors
		{
			get { return (string)RawRegistry.EdificeSendErrors.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.EdificeSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid EdificeSendErrorsToGroup
		{
			get { return (Guid)RawRegistry.EdificeSendErrorsToGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.EdificeSendErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid EdificeSendErrorsToGroupForBranch(Guid companyPK, Guid branchPK)
		{
			return (Guid)RawRegistry.EdificeSendErrorsToGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public string EdificeSendAcknowledgements
		{
			get { return (string)RawRegistry.EdificeSendAcknowledgements.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.EdificeSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid EdificeSendAcknowledgementsToGroup
		{
			get { return (Guid)RawRegistry.EdificeSendAcknowledgementsToGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.EdificeSendAcknowledgementsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid EdificeSendAcknowledgementsToGroupForBranch(Guid companyPK, Guid branchPK)
		{
			return (Guid)RawRegistry.EdificeSendAcknowledgementsToGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public string EdificeSendImpediments
		{
			get { return (string)RawRegistry.EdificeSendImpediments.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.EdificeSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid EdificeSendImpedimentsToGroup
		{
			get { return (Guid)RawRegistry.EdificeSendImpedimentsToGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.EdificeSendImpedimentsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid EdificeSendImpedimentsToGroupForBranch(Guid companyPK, Guid branchPK)
		{
			return (Guid)RawRegistry.EdificeSendImpedimentsToGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		#endregion

		#region Export Declaration

		public string ExportDeclarationSendErrors
		{
			get { return (string)RawRegistry.ExportDeclarationSendErrors.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ExportDeclarationSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid ExportDeclarationSendErrorsToGroup
		{
			get { return (Guid)RawRegistry.ExportDeclarationSendErrorsToGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ExportDeclarationSendErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid ExportDeclarationSendErrorsToGroupForBranch(Guid companyPK, Guid branchPK)
		{
			return (Guid)RawRegistry.ExportDeclarationSendErrorsToGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public string ExportDeclarationSendAcknowledgements
		{
			get { return (string)RawRegistry.ExportDeclarationSendAcknowledgements.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ExportDeclarationSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid ExportDeclarationSendAcknowledgementsToGroup
		{
			get { return (Guid)RawRegistry.ExportDeclarationSendAcknowledgementsToGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ExportDeclarationSendAcknowledgementsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid ExportDeclarationSendAcknowledgementsToGroupForBranch(Guid companyPK, Guid branchPK)
		{
			return (Guid)RawRegistry.ExportDeclarationSendAcknowledgementsToGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public string ExportDeclarationSendImpediments
		{
			get { return (string)RawRegistry.ExportDeclarationSendImpediments.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ExportDeclarationSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid ExportDeclarationSendImpedimentsToGroup
		{
			get { return (Guid)RawRegistry.ExportDeclarationSendImpedimentsToGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ExportDeclarationSendImpedimentsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public Guid ExportDeclarationSendImpedimentsToGroupForBranch(Guid companyPK, Guid branchPK)
		{
			return (Guid)RawRegistry.ExportDeclarationSendImpedimentsToGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public bool NEXDOCSDisableQRPView
		{
			get => (bool)RawRegistry.NEXDOCSDisableQRPView.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#endregion

		#region ExportManifest

		public string ExportManifestSendErrors
		{
			get { return (string)RawRegistry.ExportManifestSendErrors.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExportManifestSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}
		public Guid ExportManifestSendErrorsToGroup
		{
			get { return (Guid)RawRegistry.ExportManifestSendErrorsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExportManifestSendErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string ExportManifestSendAcknowledgements
		{
			get { return (string)RawRegistry.ExportManifestSendAcknowledgements.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExportManifestSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}
		public Guid ExportManifestSendAcknowledgementsToGroup
		{
			get { return (Guid)RawRegistry.ExportManifestSendAcknowledgementsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExportManifestSendAcknowledgementsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string ExportManifestSendImpediments
		{
			get { return (string)RawRegistry.ExportManifestSendImpediments.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExportManifestSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}
		public Guid ExportManifestSendImpedimentsToGroup
		{
			get { return (Guid)RawRegistry.ExportManifestSendImpedimentsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.ExportManifestSendImpedimentsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region Sea Cargo

		public string SeaCargoSendErrors
		{
			get { return (string)RawRegistry.SeaCargoSendErrors.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SeaCargoSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid SeaCargoSendErrorsToGroup
		{
			get { return (Guid)RawRegistry.SeaCargoSendErrorsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SeaCargoSendErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string SeaCargoSendAcknowledgements
		{
			get { return (string)RawRegistry.SeaCargoSendAcknowledgements.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SeaCargoSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid SeaCargoSendAcknowledgementsToGroup
		{
			get { return (Guid)RawRegistry.SeaCargoSendAcknowledgementsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SeaCargoSendAcknowledgementsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string SeaCargoSendImpediments
		{
			get { return (string)RawRegistry.SeaCargoSendImpediments.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SeaCargoSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid SeaCargoSendImpedimentsToGroup
		{
			get { return (Guid)RawRegistry.SeaCargoSendImpedimentsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SeaCargoSendImpedimentsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public ReadOnlyCodeDescriptionPairList SeaCargoCommercialStatus
		{
			get { return RawRegistry.SeaCargoCommercialStatus.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public string HVLVSeaCargoSendErrors
		{
			get { return (string)RawRegistry.HVLVSeaCargoSendErrors.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.HVLVSeaCargoSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string HVLVSeaCargoSendAcknowledgements
		{
			get { return (string)RawRegistry.HVLVSeaCargoSendAcknowledgements.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.HVLVSeaCargoSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string HVLVSeaCargoSendImpediments
		{
			get { return (string)RawRegistry.HVLVSeaCargoSendImpediments.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.HVLVSeaCargoSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Underbond

		public string UnderbondSendErrors
		{
			get { return (string)RawRegistry.UnderbondSendErrors.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UnderbondSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid UnderbondSendErrorsToGroup
		{
			get { return (Guid)RawRegistry.UnderbondSendErrorsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UnderbondSendErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string UnderbondSendAcknowledgements
		{
			get { return (string)RawRegistry.UnderbondSendAcknowledgements.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UnderbondSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid UnderbondSendAcknowledgementsToGroup
		{
			get { return (Guid)RawRegistry.UnderbondSendAcknowledgementsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UnderbondSendAcknowledgementsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string UnderbondSendImpediments
		{
			get { return (string)RawRegistry.UnderbondSendImpediments.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UnderbondSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid UnderbondSendImpedimentsToGroup
		{
			get { return (Guid)RawRegistry.UnderbondSendImpedimentsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UnderbondSendImpedimentsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region CargoStatus

		public string CargoStatusSendErrors
		{
			get { return (string)RawRegistry.CargoStatusSendErrors.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CargoStatusSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid CargoStatusSendErrorsToGroup
		{
			get { return (Guid)RawRegistry.CargoStatusSendErrorsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CargoStatusSendErrorsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string CargoStatusSendAcknowledgements
		{
			get { return (string)RawRegistry.CargoStatusSendAcknowledgements.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CargoStatusSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid CargoStatusSendAcknowledgementsToGroup
		{
			get { return (Guid)RawRegistry.CargoStatusSendAcknowledgementsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CargoStatusSendAcknowledgementsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string CargoStatusSendImpediments
		{
			get { return (string)RawRegistry.CargoStatusSendImpediments.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CargoStatusSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		public Guid CargoStatusSendImpedimentsToGroup
		{
			get { return (Guid)RawRegistry.CargoStatusSendImpedimentsToGroup.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.CargoStatusSendImpedimentsToGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		readonly RawDataRegistry RawRegistry;
	}
}
