using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
namespace Enterprise.Customs.CA.Business
{
	class RepairLineSynchroniser : BusinessObjectSynchroniser
	{
		public RepairLineSynchroniser(JobComInvoiceLine destination, JobComInvoiceLine source)
			: base(destination, source)
		{
		}

		public new JobComInvoiceLine Source
		{
			get { return (JobComInvoiceLine)base.Source; }
		}

		public new JobComInvoiceLine Destination
		{
			get { return (JobComInvoiceLine)base.Destination; }
		}

		protected override void ForceSynchroniseCore()
		{
			base.ForceSynchroniseCore();
			if (DetectEnabled)
			{
				if (Destination.JI_Description != Source.JI_Description)
				{
					SyncChangesDetected = true;
					return;
				}
			}
			else
			{
				if (Destination.IsRemissionRepairLine)
				{
					Destination.JI_Description = Destination.GetCalculationMethodDescription(Destination.CA_CalculationMethod) + " -";
				}
			}
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!SyncChangesDetected)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_PageNumberInfo, Source.CA_PageNumberInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_JZInfo, Source.JI_JZInfo));
				if (Destination.CA_CalculationMethod == CalculationMethods.Codes.SoftwareRemission)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.JI_TariffInfo, Source.JI_TariffInfo, true));
				}
				else
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.JI_TariffInfo, Source.JI_TariffInfo));
				}
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_99TariffCodeInfo, Source.CA_99TariffCodeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_TreatmentCodeInfo, Source.CA_TreatmentCodeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_ADJCodeInfo, Source.CA_ADJCodeInfo, true));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_InvoiceUQInfo, Source.JI_InvoiceUQInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_InvoiceQuantityInfo, Source.JI_InvoiceQuantityInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_CustomsUnitQtyInfo, Source.JI_CustomsUnitQtyInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_CustomsSecondUnitQtyInfo, Source.JI_CustomsSecondUnitQtyInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_CustomsThirdUnitQtyInfo, Source.JI_CustomsThirdUnitQtyInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_CountryOfOriginInfo, Source.JI_CountryOfOriginInfo, true));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_StateOrRegionOfOriginInfo, Source.JI_StateOrRegionOfOriginInfo, true));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_RequirementIDInfo, Source.CA_RequirementIDInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_RequirementVerInfo, Source.CA_RequirementVerInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_AirsCodeInfo, Source.CA_AirsCodeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_DestinationProvinceInfo, Source.CA_DestinationProvinceInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_RN_NKCFIAOriginInfo, Source.CA_RN_NKCFIAOriginInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_CFIAUSStateOfOriginInfo, Source.CA_CFIAUSStateOfOriginInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_EndUseInfo, Source.CA_EndUseInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_MiscIDInfo, Source.CA_MiscIDInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_ImportReasonCodeInfo, Source.CA_ImportReasonCodeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_ModelInfo, Source.CA_ModelInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_ModelNumberInfo, Source.CA_ModelNumberInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_BrandNameInfo, Source.JI_BrandNameInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_TypeSizeInfo, Source.CA_TypeSizeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_TIINInfo, Source.CA_TIINInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_CompliantCompletionInfo, Source.CA_CompliantCompletionInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_CompliantImportDateInfo, Source.CA_CompliantImportDateInfo));

				if (Destination.IsRemissionRepairLine)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.CA_ValueForDutyCodeInfo,
						delegate
						{
							var vfd = ZString.Empty;
							if (ValueForDutyCodes.IsRelatedFirms(Source.CA_ValueForDutyCode))
							{
								vfd = ValueForDutyCodes.Codes.RelatedFirmsResidualMethodValue;
							}
							else if (ValueForDutyCodes.IsUnrelatedFirms(Source.CA_ValueForDutyCode))
							{
								vfd = ValueForDutyCodes.Codes.UnrelatedFirmsResidualMethodValue;
							}
							return vfd;
						}, () => new ZPropertyInfo[] { Source.CA_ValueForDutyCodeInfo }));
				}

				if (Destination.CA_CalculationMethod != CalculationMethods.Codes.WarrantyRepairsRemission)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.JI_CustomsQuantityInfo, Source.JI_CustomsQuantityInfo));
					Synchronisers.Add(new FieldSynchroniser(Destination.JI_CustomsSecondQuantityInfo, Source.JI_CustomsSecondQuantityInfo));
					Synchronisers.Add(new FieldSynchroniser(Destination.JI_CustomsThirdQuantityInfo, Source.JI_CustomsThirdQuantityInfo));
					Synchronisers.Add(new FieldSynchroniser(Destination.CA_ADJValueInfo, Source.CA_ADJValueInfo, true));
				}

				sittCertificationNumbersCollectionSyncroniser = new CusCodeDataCollectionSynchroniser<SITTCertificationNumber>(Source, Destination, Source.SITTCertificationNumbers, Destination.SITTCertificationNumbers);
				sittCertificationNumbersCollectionSyncroniser.SetEnabled(IsEnabled, DetectEnabled);
				sittCertificationNumbersCollectionSyncroniser.Synchronise();
				if (DetectEnabled && sittCertificationNumbersCollectionSyncroniser.SyncChangesDetected)
				{
					SyncChangesDetected = true;
					return;
				}
				cfiaRegistrationNumbersCollectionSyncroniser = new CusCodeDataCollectionSynchroniser<CFIARegistrationNumber>(Source, Destination, Source.CFIARegistrationNumbers, Destination.CFIARegistrationNumbers);
				cfiaRegistrationNumbersCollectionSyncroniser.SetEnabled(IsEnabled, DetectEnabled);
				cfiaRegistrationNumbersCollectionSyncroniser.Synchronise();
				if (DetectEnabled && cfiaRegistrationNumbersCollectionSyncroniser.SyncChangesDetected)
				{
					SyncChangesDetected = true;
					return;
				}
				SetDestinationCollectionsReadOnlyStatus();

				var declaration = Source.Declaration;
				if (declaration != null && declaration.IsLVS)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.CA_RN_NKExportInfo, Source.CA_RN_NKExportInfo));
					Synchronisers.Add(new FieldSynchroniser(Destination.CA_USStateOfExportInfo, Source.CA_USStateOfExportInfo));
				}
			}
		}
		CusCodeDataCollectionSynchroniser<SITTCertificationNumber> sittCertificationNumbersCollectionSyncroniser;
		CusCodeDataCollectionSynchroniser<CFIARegistrationNumber> cfiaRegistrationNumbersCollectionSyncroniser;

		protected override void OnEnabledChanged()
		{
			base.OnEnabledChanged();
			SetDestinationCollectionsReadOnlyStatus();
		}

		void SetDestinationCollectionsReadOnlyStatus()
		{
			Destination.SITTCertificationNumbers.SetReadOnlyIncludingChildren(IsEnabled);
			Destination.CFIARegistrationNumbers.SetReadOnlyIncludingChildren(IsEnabled);
			if (sittCertificationNumbersCollectionSyncroniser != null)
			{
				sittCertificationNumbersCollectionSyncroniser.SetEnabled(IsEnabled, DetectEnabled);
			}

			if (cfiaRegistrationNumbersCollectionSyncroniser != null)
			{
				cfiaRegistrationNumbersCollectionSyncroniser.SetEnabled(IsEnabled, DetectEnabled);
			}
		}
	}
}
