using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public abstract class JobDeclarationValueSetStrategy : EU.Business.Declaration.JobDeclarationValueSetStrategy
	{
		public JobDeclarationValueSetStrategy(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
			Declaration.JE_DeltaModeInfo.ValueChanged -= JE_DeltaModeInfo_ValueChanged;
			Declaration.JE_DeltaModeInfo.ValueChanged += JE_DeltaModeInfo_ValueChanged;
		}

		void JE_DeltaModeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateCEI_SubStyle();
			DefaultJE_CustomsGuaranteeNumber();
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_OH_Supplier:
				case JobDeclaration.Schema.JE_OH_Importer:
					Declaration.PopulateInvoiceCharges();
					OnClientOrDeclarantChanged();
					PopulateGroupCharges();
					break;
				case JobDeclaration.Schema.JE_MessageType:
					DefaultTHI();
					OnClientOrDeclarantChanged();
					Declaration.SetRegionOrTerritoryOfDestinationDefaultValue();
					break;
				case JobDeclaration.Schema.JE_OA_DeclarantAddress:
					OnClientOrDeclarantChanged();
					break;
				case JobDeclaration.Schema.JE_LocationOfGoods:
					DefaultJE_SubLocationOfGoods();
					DefaultJE_CustomsOffice();
					break;
				case JobDeclaration.Schema.JE_CustomsProfile:
					DefaultJE_DeltaMode();
					DefaultOfficeOfDeclaration();
					Declaration.VATDeferStrategy.OnOrganisationChanged();
					DefaultJE_LocationOfGoods();
					break;
				case JobDeclaration.Schema.JE_DateOfArrival:
					OnDateOfArrivalChanged();
					break;
				case JobDeclaration.Schema.JE_ExportDate:
					OnExportDateChanged();
					break;
				case JobDeclaration.Schema.JE_RL_NKPortOfArrival:
				case JobDeclaration.Schema.JE_RL_NKPortOfLoading:
				case JobDeclaration.Schema.JE_CustomsOffice:
				case JobDeclaration.Schema.JE_ContainerMode:
					DefaultTHI();
					break;
				case JobDeclaration.Schema.JE_ApplicationCode:
					DefaultJE_DeclarationLanguage();
					DefaultJE_CustomsProfile();
					DefaultZG_AgreedCodePlace();
					DefaultJE_CustomsGuaranteeNumber();
					break;
				case JobDeclaration.Schema.JE_VATCANACode:
					DefaultAdditionalInfoInDeclaration();
					break;
				case JobDeclaration.Schema.JE_IsHighValueOvrd:
					UpdateDV1Details();
					break;
			}
		}

		protected override void VATDeferTypeChanged()
		{
			base.VATDeferTypeChanged();
			var declaration = Declaration;
			if (declaration.IsImport)
			{
				var invoiceLines = declaration.InvoiceLines;
				if (invoiceLines.Any(x => ((JobComInvoiceLine)x).RequiresVATNumberDocument))
				{
					declaration.ManageVATSupportingDocuments();
				}
			}
		}

		void UpdateDV1Details()
		{
			var declaration = Declaration;
			if (declaration.ZG_IsHighValueOvrd)
			{
				if (declaration.DV1Details.Count == 0)
				{
					declaration.DV1Details.AddNew();
				}
			}
			else
			{
				if (declaration.DV1Details.Count > 0)
				{
					declaration.DV1Details.RemoveAndDeleteAll();
				}
			}
		}

		void DefaultAdditionalInfoInDeclaration()
		{
			var declaration = Declaration;
			var vAtCanaCode = declaration.JE_VATCANACode;
			var listToRemove = new List<AdditionalInfo>();

			var specialMentionCode = declaration.GetVatCanaSpecialMention(vAtCanaCode);
			if (!specialMentionCode.IsEmpty)
			{
				var additionnalInfoWithSM = declaration.AdditionalInfos?.Cast<AdditionalInfo>().FirstOrDefault(x => string.Compare(x.CSI_Code, specialMentionCode, StringComparison.OrdinalIgnoreCase) == 0);
				if (additionnalInfoWithSM == null)
				{
					var additionalInfo = declaration.AdditionalInfos.AddNew();
					additionalInfo.CSI_Code = specialMentionCode.Left(additionalInfo.CSI_CodeInfo.MaxLength);
				}

				listToRemove = declaration.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_Code != specialMentionCode && declaration.Lookups.VatProcedureSpecialMentionList.Contains(x.CSI_Code)).ToList();
			}
			else
			{
				listToRemove = declaration.AdditionalInfos.Cast<AdditionalInfo>().Where(x => declaration.Lookups.VatProcedureSpecialMentionList.Contains(x.CSI_Code)).ToList();
			}

			if (listToRemove != null && listToRemove.Count > 0)
			{
				foreach (var code in listToRemove)
				{
					if (code != null)
					{
						RemoveFromCusSupportingCollection(declaration.AdditionalInfos, code.CSI_Code);
					}
				}
			}
		}

		void RemoveFromCusSupportingCollection<T>(CusSupportingInfoCollection<T> cusSupportingCollection, ZString codeToRemove) where T : CusSupportingInfo
		{
			var cusSupportingInfosToRemove = cusSupportingCollection.Cast<T>().Where(x => x.CSI_Code == codeToRemove).ToList();
			cusSupportingInfosToRemove.ForEach(cusSupportingCollection.RemoveAndDelete);
		}

		void DefaultTHI()
		{
			Declaration.ChargePaymentOrDestinationID = UniversalReferenceDataHelper.GetChargePaymentOrDestinationID(Declaration.Factory, Declaration);
		}

		void PopulateGroupCharges()
		{
			Declaration.JobComInvoiceGroupHeaders.Cast<JobComInvoiceGroupHeader>().ForEach(x => x.PopulateCharges());
		}

		protected abstract void DefaultZG_AgreedCodePlace();

		protected abstract void DefaultJE_DeltaMode();

		protected abstract void DefaultJE_DeclarationLanguage();

		void OnClientOrDeclarantChanged()
		{
			DefaultJE_CustomsProfile();
			DefaultJE_CustomsGuaranteeNumber();
		}

		public void DefaultJE_CustomsGuaranteeNumber() => DefaultJE_CustomsGuaranteeNumberCore();

		protected virtual void DefaultJE_CustomsGuaranteeNumberCore()
		{
			var declaration = Declaration;
			var guaranteeList = declaration.Lookups.CODCustomsGuarantees;

			var guaranteeSourceList = GetGuaranteeSourcePriorityList();
			var guaranteeSource = guaranteeSourceList.FirstOrDefault(source => guaranteeList.Any(x => x.CPH_OH_PermitHolder == source));
			var guarantee = guaranteeList.FirstOrDefault(x => x.CPH_OH_PermitHolder == guaranteeSource);

			Declaration.JE_CustomsGuaranteeNumber = guarantee?.CPH_Number ?? ZString.Empty;
		}

		protected abstract List<ZGuid> GetGuaranteeSourcePriorityList();

		protected abstract void DefaultJE_CustomsProfile();

		void DefaultJE_LocationOfGoods()
		{
			var declaration = Declaration;
			var auls = declaration.JE_CustomsProfileAuthorizedLocations;
			declaration.JE_LocationOfGoods = auls.Count() == 1 ? auls.Single().SubstringSafe(0, JobDeclaration.Schema.JE_LocationOfGoodsMaxLength) : ZString.Empty;
		}

		void DefaultJE_SubLocationOfGoods()
		{
			var locs = Declaration.JE_LocationOfGoodsRelatedCusAuthorisation?.GetAuthorisationRuleValueWithCode(CusAuthorisationRuleTypeList.Codes.SUB) ?? Enumerable.Empty<ZString>();
			Declaration.JE_SubLocationOfGoods = locs.Count() == 1 ? locs.Single().SubstringSafe(0, JobDeclaration.Schema.JE_SubLocationOfGoodsMaxLength) : ZString.Empty;
		}

		void DefaultJE_CustomsOffice()
		{
			Declaration.JE_CustomsOffice = Declaration.JE_LocationOfGoodsRelatedCusAuthorisation
			?.GetAuthorisationRuleValueWithCode(CusAuthorisationRuleTypeList.Codes.OFC)
			.FirstOrDefault()
			.SubstringSafe(0, JobDeclaration.Schema.JE_CustomsOfficeMaxLength) ?? ZString.Empty;
		}

		void DefaultOfficeOfDeclaration()
		{
			Declaration.OfficeOfDeclaration = Declaration.JE_CustomsProfileRelatedAccount?.CZ_Issuer ?? ZString.Empty;
		}

		void OnDateOfArrivalChanged()
		{
			if (Declaration.IsImport)
			{
				UpdateCEI_SubStyle();
			}
		}

		void OnExportDateChanged()
		{
			if (Declaration.IsExport)
			{
				UpdateCEI_SubStyle();
			}
		}

		void UpdateCEI_SubStyle()
		{
			var dateInterestedIn = Declaration.IsImport ? Declaration.JE_DateOfArrival : Declaration.JE_ExportDate;
			foreach (CusEntryInstruction cei in Declaration.CustomsEntryInstructions)
			{
				var entryHeader = cei.EntryHeader;
				if (entryHeader?.EntryNumber.IsEmpty ?? true)
				{
					SetDefaultOrSwapSubStyleIfNecessarily(cei, dateInterestedIn);
				}
			}
		}

		protected abstract void SetDefaultOrSwapSubStyleIfNecessarily(CusEntryInstruction entryInstruction, ZDateTime dateInterestedIn);

		protected override void DefaultSupplierChanged()
		{
			base.DefaultSupplierChanged();
			Declaration.VATDeferStrategy.OnPaymentMethodChanged();
		}
	}
}
