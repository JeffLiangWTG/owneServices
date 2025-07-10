using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckJE_MessageTypeIsEnteredOrValid()
		{
			// no need to check as we already checked that it's import
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
			CheckRuleBR8F0009();
			CheckRuleBR8F0010();
			CheckRuleBR8063_NoAmend();
		}

		void CheckRuleBR8063_NoAmend()
		{
			CheckNoAmendingOnEntryStatus(Parent.OriginalCustomsOffice, Parent.JE_CustomsOfficeInfo);
		}

		void CheckRuleBR8F0010()
		{
			var parent = Parent;
			if (parent.SupervisingCustomsOffice.IsEmpty
				&& parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(
					invoiceLine => IsRuleBR8F0010Valid(invoiceLine.EntryInstruction?.CEI_Style ?? ZString.Empty, invoiceLine.JI_Calc_RequestedProcedure)
					&& invoiceLine.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsAnAdditionalInformation && info.CSI_Code == Constants.AdditionalInformationCodes._00100)))
			{
				parent.JE_CustomsOfficeInfo.AddMessageError(Res.GetString("293E980B-FE8D-497F-90A8-A49CE1203401", "[BR8F0010] Please enter a Supervising Customs Office (SVO) in the below grid."));
			}

			bool IsRuleBR8F0010Valid(ZString declarationType, ZString procedure)
			{
				return (declarationType.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H1) && procedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._44)
					|| (declarationType.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H3) && procedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._53)
					|| (declarationType.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H4) && procedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._51);
			}
		}

		void CheckRuleBR8F0009()
		{
			var parent = Parent;
			if (!parent.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfDischarge))
			{
				if (parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine =>
				{
					var style = invoiceLine.EntryInstruction?.CEI_Style ?? ZString.Empty;
					var procedure = invoiceLine.JI_Calc_RequestedProcedure;
					return ((style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H1) && procedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._44) ||
							(style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H3) && procedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._53) ||
							(style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H4) && procedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._51))
							&& invoiceLine.AdditionalInfos.ContainsAdditionalInfoOfCode(AdditionalInfoSubTypeList.Codes.AdditionalInformation, new ZString[] { Constants.AdditionalInformationCodes._00100 });
				}))
				{
					parent.JE_CustomsOfficeInfo.AddMessageError(Res.GetString("BB57F26C-FCD3-4AA6-8FD7-3AAB0FB51D61", "[BR8F0009] Please enter a Customs Office of Discharge (DSC) in the below grid."));
				}
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			if (IsEntryStyle_h1h2h3h4h5())
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKFinalDestinationInfo);
			}
		}

		protected override void CheckJE_GoodsDestination()
		{
			base.CheckJE_GoodsDestination();
			if (IsEntryStyle_h1h2h3h4h5())
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsDestinationInfo, (NoResString)"Country of Destination");
			}
		}
		bool IsEntryStyle_h1h2h3h4h5()
		{
			return Parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x =>
			{
				return x.CEI_Style == ImportDeclarationTypeList.Codes.H1
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H2
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H3
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H4
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H5;
			});
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			ValidateBR2318_TransportMode(Parent.JE_TransportModeInfo);
		}

		void ValidateBR2318_TransportMode(ZPropertyInfo info)
		{
			var declaration = Parent;

			if (!declaration.IsSea && declaration.IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference)
			{
				info.AddMessageError(Res.GetString("90511680-E015-494B-9B3D-2C7117F64A66", "[BR2318] If '1D95' is present in Additional reference and Add. Dec. Type is not Y, then Transport Mode should be Sea (1) and Inland Type of ID should be IMO/ENI number."));
			}
		}

		protected override void CheckJE_TransportModeMandatory()
		{
			var parent = Parent;
			if (!parent.IsExWarehouse && !parent.IsNonTransportDeclarationType && !parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().All(instruction => instruction.IsH2))
			{
				var prefix = "[CD7041] ";
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportModeInfo, string.Empty, prefix);
			}
		}

		protected override void CheckJE_TransportModeInland()
		{
			base.CheckJE_TransportModeInland();
			var parent = Parent;
			if (parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x =>
			{
				var declarationType = x.CEI_Style;
				return declarationType == ImportDeclarationTypeList.Codes.H1 || declarationType == ImportDeclarationTypeList.Codes.H3 || declarationType == ImportDeclarationTypeList.Codes.H4;
			}))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportModeInlandInfo);
			}
		}

		protected override void CheckJE_TransportMeans()
		{
			base.CheckJE_TransportMeans();
			ValidateBR2318_TransportMeans();
			ValidateMandatory_TransportMeans();
		}

		void ValidateMandatory_TransportMeans()
		{
			var parent = Parent;
			if (!parent.JE_TransportModeInland.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportMeansInfo);
			}
		}

		void ValidateBR2318_TransportMeans()
		{
			var declaration = Parent;

			if (declaration.IsSea && declaration.JE_TransportMeans != TransportMeansList.Codes.ImoShipIdentificationNumber && declaration.JE_TransportMeans != TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode && declaration.IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference)
			{
				declaration.JE_TransportMeansInfo.AddMessageError(Res.GetString("26E67CE1-2EC0-49A1-873D-5E387C967FFE", "[BR2318] If '1D95' is present in Additional reference, Add. Dec. Type is not Y and Transport Mode is Sea (1) then Inland Type of ID should be IMO/ENI number (10 or 80)."));
			}
		}

		protected override void CheckJE_EntryStyle()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_EntryStyleInfo);
			ValidateJE_EntryStyle_BR1011();
			ValidateJE_EntryStyle_BR1118();
			CheckNoAmendingOnEntryStatus(Parent.OriginalDeclarationType, Parent.JE_EntryStyleInfo);
		}

		void ValidateJE_EntryStyle_BR1011()
		{
			var parent = Parent;
			if (parent.JE_EntryStyle.IsEmpty && parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => ImportDeclarationTypeList.DeclarationTypeListBR1011.Contains(x.CEI_Style)))
			{
				parent.JE_EntryStyleInfo.AddMessageError(Res.GetString("81589AE2-88F4-462A-95EA-8CED3FE47039", "[BR1011] Declaration Type is required when Declaration is H1, H2, H3, H4, H5, H6, or I1"));
			}
		}

		void ValidateJE_EntryStyle_BR1118()
		{
			var parent = Parent;
			if (!parent.JE_EntryStyle.EqualsIgnoringCase(EntryStyleListImport.Codes.ImportFromSpecialTerritory) && JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(parent.InvoiceLines.Cast<JobComInvoiceLine>()))
			{
				parent.JE_EntryStyleInfo.AddMessageError(Res.GetString("18CDA2B2-C716-4E45-8AFE-96A3BD2543EC", "[BR1118] Declaration Type must be CO when at least one invoice line has Additional Procedure Code F15"));
			}
		}

		protected override void CheckJE_OH_DutyPayer()
		{
			base.CheckJE_OH_DutyPayer();

			var parent = Parent;
			if (parent.CustomsEntryInstructions.Any(x => ImportDeclarationTypeList.DeclarationTypeListBR2074.Contains(x.CEI_Style)) &&
				parent.DutyPayer == null)
			{
				parent.JE_OH_DutyPayerInfo.AddMessageError(Res.GetString("7387FBCB-B31C-4793-93AD-67CDE71ECDDC", "[BR2074] Duty Payer is required when Declaration is H1, H5, H6 or I1."));
			}

			if (parent.DutyPayer != null && parent.DutyPayer.GetEORI().IsEmpty)
			{
				parent.JE_OH_DutyPayerInfo.AddMessageError(Res.GetString("A6ADBEF0-A23D-4626-989F-AC84FE316903", "An IE EORI number is required but missing for the entered organization. Press F3 and navigate to the Details > Config > Registration Numbers/Codes tab to add an IE EORI number for the entered organization."));
			}
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();
			if (Parent.JE_GoodsOrigin == Core.Constants.CountryCodes.Ireland)
			{
				Parent.JE_GoodsOriginInfo.AddMessageError(Res.GetString("D53D706C-A512-4DDB-B6DD-58365E2F9EC5", "[BR0514] Country of Dispatch of the goods cannot be IE when Message Type is IMP."));
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();
			var parent = Parent;
			if (parent.CustomsEntryInstructions.Any(x =>
			{
				var declarationType = x.CEI_Style;
				return declarationType == ImportDeclarationTypeList.Codes.H1
					|| declarationType == ImportDeclarationTypeList.Codes.H3
					|| declarationType == ImportDeclarationTypeList.Codes.H4
					|| declarationType == ImportDeclarationTypeList.Codes.H5
					|| declarationType == ImportDeclarationTypeList.Codes.I1;
			}))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_PaymentMethodInfo);
			}
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();
			if (Parent.JE_OA_DeclarantAddress.IsValid)
			{
				var targetInfo = Parent.JE_OA_DeclarantAddressInfo;
				CheckNoAmendingOnEntryStatus(Parent.OriginalDeclarant, targetInfo);
			}
		}

		protected override void CheckJE_OA_Representative()
		{
			base.CheckJE_OA_Representative();
			var targetInfo = Parent.JE_OA_RepresentativeInfo;
			if (Parent.JE_OA_Representative.IsValid)
			{
				CheckNoAmendingOnEntryStatus(Parent.OriginalRepresentative, targetInfo);
			}
		}
	}
}
