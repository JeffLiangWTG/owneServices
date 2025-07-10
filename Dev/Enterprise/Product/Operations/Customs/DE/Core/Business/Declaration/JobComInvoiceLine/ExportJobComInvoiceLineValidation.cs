using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateUNDGs();
			}
		}

		public void ValidateUNDGs()
		{
			var parent = Parent;
			var declaration = parent.Declaration;
			var allowedNumberofUNDGs = !declaration.IsUCC6 || declaration.IsTransitionPeriodAES30 ? 1 : 99;
			if (parent.UNDGs.Count > allowedNumberofUNDGs)
			{
				parent.AddRowMessageError(Res.GetString("395042A0-A3F7-4D17-8FE4-F3A32248B178", "A maximum of {0} dangerous goods information can be entered.", allowedNumberofUNDGs));
			}
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
		}

		protected override void CheckJI_CustomsSecondQuantity_Mandatory()
		{
			var parent = Parent;
			if (instruction.Style5thDigitIs0() && !parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JI_CustomsSecondQuantityInfo, JI_CustomsSecondQuantityMandatoryMessageError);
			}
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();
			var entryInstruction = Parent.EntryInstruction;
			var procedureWithConcession = Parent.JI_Procedure;
			var declaration = Parent.Declaration;
			var targetInfo = Parent.JI_ProcedureInfo;

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);

			if (!procedureWithConcession.IsEmpty && entryInstruction != null)
			{
				CheckJI_ProcedureConcession(targetInfo, declaration, entryInstruction);
			}

			if (Parent.PreviousProcedureMaster.CSI_Procedure.IsEmpty && PreviousDocumentValidationHelper.PreviousProcedureIsRequiredForInvoiceLine(Parent))
			{
				targetInfo.AddWarning(Res.GetString("E7365736-5608-4628-982D-0BC4353D313E", "You have not entered a Previous Procedure."));
			}

			if (declaration != null)
			{
				CheckJI_ProcedureWithDestination(targetInfo, declaration);

				if (declaration.JE_EntryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory)
				{
					var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Parent.Factory, Parent.Declaration.JE_RL_NKFinalDestination.Left(2), Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.I0812, ZDate.Today);
					if (cusCode == null)
					{
						var previousProcedureCode = procedureWithConcession.SubstringSafe(2, 2);
						if (previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._02 ||
							previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._41 ||
							previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._51 ||
							previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._71 ||
							previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._91)
						{
							targetInfo.AddMessageError(Res.GetString("09AF69B8-1B2B-42D1-88FC-D37F09F344D7", "CPC – The Previous Procedure Code is not allowed for Entry Style of Type 'CO' and the selected Country/Region of Destination"));
						}
					}
				}
			}
		}

		void CheckJI_ProcedureWithDestination(ZPropertyInfo targetInfo, JobDeclaration declaration)
		{
			var concession = Parent.Concession;
			var destinationCountry = declaration.JE_RL_NKFinalDestination.Left(2);
			var concessionlist1 = new ZString[] { Concessions.F61, Concessions.F62, Concessions.F64 };
			var concessionlist2 = new ZString[] { Concessions.F61, Concessions.F62, Concessions._6F0 };
			if (new ZString[] { CountryCodes.CountryCodeQQ, CountryCodes.CountryCodeQR, CountryCodes.CountryCodeQS }.Contains(destinationCountry) && !concessionlist1.Contains(concession))
			{
				targetInfo.AddMessageError(Res.GetString("0f0e8d96-4e53-49f7-9135-ad77e1a5ed44", "Destination Country/Region is only allowed with CPC – Concessions F61, F62, F64."));
			}
			else if (new ZString[] { CountryCodes.CountryCodeQU, CountryCodes.CountryCodeQV, CountryCodes.CountryCodeQP }.Contains(destinationCountry) && !concessionlist2.Contains(concession))
			{
				targetInfo.AddMessageError(Res.GetString("637f4c28-3d2e-46c9-bdab-4926359d43a8", "Destination Country/Region is only allowed with CPC – Concessions F61, F62, 6F0."));
			}
		}

		void CheckJI_ProcedureConcession(ZPropertyInfo targetInfo, JobDeclaration declaration, CusEntryInstruction entryInstruction)
		{
			var concession = Parent.Concession;
			if (!concession.IsEmpty)
			{
				if (concession == CustomsProcedureCodeList.Export.Concession._F61 ||
					concession == CustomsProcedureCodeList.Export.Concession._F75 ||
					concession == CustomsProcedureCodeList.Export.Concession._6F0)
				{
					if (declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.EntryInstruction == entryInstruction && line.Concession != concession))
					{
						targetInfo.AddMessageError(Res.GetString("46A3CB25-AB6D-4CE8-8087-BA5CB1A53134", "For CPC - Concession F61, 6F0 or F75 all Lines assigned to this Entry Instruction must have the same CPC-Concession."));
					}
				}

				if (concession == CustomsProcedureCodeList.Export.Concession._F75 && declaration.JE_EntryStyle != EntryStyleListExport.Codes.ExportToSpecialTerritory)
				{
					targetInfo.AddMessageError(Res.GetString("0F378D5A-1A8F-448B-9721-E91332BAA3C0", "The Concession code is only allowed for Entry Style of Type ‘CO’."));
				}
			}
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			if (Parent.JI_Weight.IsEmpty)
			{
				var invoiceHeader = Parent.InvoiceHeader;
				if (invoiceHeader != null && invoiceHeader.JZ_Weight.IsEmpty && new ZString[] { "71", "78" }.Contains(Parent.JI_Procedure.SubstringSafe(2, 2)))
				{
					Parent.JI_WeightInfo.AddMessageError(Res.GetString("B190D3F9-54AF-4728-9E57-24EA97C58210", "Gross Weight must be greater than 0."));
				}
			}

			if (Parent.JI_Weight != ZDecimal.Zero && Parent.JI_Weight < Parent.JI_CustomsQuantity)
			{
				Parent.JI_WeightInfo.AddMessageError(Res.GetString("0117E9E9-1A6A-4C70-96BB-47C8F7DCDA23", "[35] Gross Weight must be greater than [38] Net Mass Measure."));
			}
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();

			if (Parent.JI_CustomsQuantity == ZDecimal.Zero && Parent.JI_Weight > ZDecimal.Zero)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("720F3027-5D64-41C8-81F5-1F9E88D4A646", "If [38] Net Mass Measure is 0, [35] GWT must also be 0."));
			}

			ValidateJI_Weight();
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			if (!Parent.Declaration?.Declarant?.Header.HasEUEoriRegNo() ?? ZBool.True)
			{
				var tariff = Parent.JI_Tariff;
				if (!tariff.IsEmpty && Parent.EntryInstruction.Constellation1stDigitIs0())
				{
					var list = Universal.RefCusCodeListTypes.GetCachedList(Parent.Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityCodesForMineralOilsAndGases, ZDateTime.Today);
					if (list.ContainsCode(tariff))
					{
						Parent.JI_TariffInfo.AddMessageError(Res.GetString("5168F312-0164-4C83-AF0A-22DF3CA2A3CA", "The entered combination of Tariff and Party Constellation requires the Declarant to have a Registration Number of Type 'EOR' stored in Registration Numbers/Codes"));
					}
				}
			}

			if (Parent.JI_Tariff.StartsWith("98"))
			{
				if (!Parent.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == SupportingDocumentTypes._9DEE))
				{
					Parent.JI_TariffInfo.AddMessageError(Res.GetString("7223F2E4-1BF9-4DFD-A8F8-F53CD3D11691", "A Supporting Document of Type '{0}' is required for this Tariff.", SupportingDocumentTypes._9DEE));
				}
			}
		}

		protected override void CheckJI_OA_ConsigneeAddress()
		{
			base.CheckJI_OA_ConsigneeAddress();

			var parent = Parent;
			var declaration = parent.Declaration;
			if (declaration != null)
			{
				var importer = declaration.Importer;
				var targetInfo = parent.JI_OA_ConsigneeAddressInfo;
				if (importer != null)
				{
					var consignee = parent.JI_OA_ConsigneeAddress_ZAddress.OrgHeader;
					if (consignee != null && importer.PK == consignee.PK)
					{
						targetInfo.AddMessageError(Res.GetString("97ddaab0-2705-483c-b2e0-a80d6db3aa19", "The Consignee must not be equal to the Importer on the Declaration."));
					}
				}

				if (parent.InvoiceHeader.JZ_OA_ConsigneeAddress.IsEmpty)
				{
					var instruction = parent.EntryInstruction;
					if (instruction.Style5thDigitIs0() || instruction.Constellation3rdDigitIs0() || instruction.Constellation4thDigitIs0())
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
					}
				}
			}
		}

		protected override void CheckJI_RN_NKCountryOfExport()
		{
			base.CheckJI_RN_NKCountryOfExport();

			ListValidation.MessageErrorIfInvalidCode(Parent.JI_RN_NKCountryOfExportInfo);
		}

		protected override void CheckJI_CountryOfOriginMandatoryValidation()
		{
			var parent = Parent;
			if (parent.EntryInstruction.Style5thDigitIs0())
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JI_CountryOfOriginInfo);
			}
		}

		protected override void CheckJI_StateOrRegionOfOrigin()
		{
			base.CheckJI_StateOrRegionOfOrigin();

			var targetInfo = Parent.JI_StateOrRegionOfOriginInfo;

			if (instruction != null && !instruction.Style4thDigitIs4() &&
				(Parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.Germany ||
				 !Parent.JI_CountryOfOrigin.IsEmpty && instruction.Style5thDigitIs0()))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			base.CheckJI_PreviousEntryLineNumber();
			var line = Parent;
			if (line.IsOutOfWarehouseWarehousing && !line.JI_PreviousEntryNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(line.JI_PreviousEntryLineNumberInfo, line.JI_PreviousEntryLineNumberBondedWarehouseCaption.Caption);
			}
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();
			var line = Parent;
			if (line.IsOutOfWarehouseWarehousing && !line.JI_BondedWhsQuantity.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(line.JI_PreviousEntryNumberInfo, line.JI_PreviousEntryNumberBondedWarehouseCaption.FullDescription);
			}
		}

		protected override void CheckJI_BondedWHSOrderLineNumber()
		{
			var parent = Parent;
			if (!parent.JI_BondedWHSOrderNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_BondedWHSOrderLineNumberInfo);
			}
		}

		protected override void CheckJI_BondedWhsUnitQty()
		{
			var parent = Parent;
			var info = parent.JI_BondedWhsUnitQtyInfo;
			ListValidation.MessageErrorIfInvalidCode(info);
			if (!parent.JI_BondedWhsQuantity.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		protected override void CheckJI_BondedWhsQuantity()
		{
			var parent = Parent;
			var info = parent.JI_BondedWhsQuantityInfo;

			if (parent.Factory.IsIntegerRequiredUnitOfQuantity(parent.JI_BondedWhsUnitQty) && !parent.JI_BondedWhsQuantity.IsInteger)
			{
				info.AddMessageError(Res.GetString("8B52432A-F300-4A96-9156-30DDE484B4BC", "Unit of quantity 'NAR', 'NARB', 'NCL' or 'NPR' requires an Integer value."));
			}
		}

		protected override bool HasValidPackagePivots
		{
			get
			{
				var parent = Parent;
				var result = false;

				if (result = parent.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Any(y => y.IsLinked))
				{
					if (parent.JI_IsMainPack)
					{
						result = parent.PackagesPivot.OfType<InvoiceLinePackagePivot>().Any(p => p.CHC_NumberOfPacks > 0);
					}
					else
					{
						result = parent.PackagesPivot.OfType<InvoiceLinePackagePivot>().Any(p => p.CHC_NumberOfPacks >= 0);
					}
				}
				return result;
			}
		}
	}
}
