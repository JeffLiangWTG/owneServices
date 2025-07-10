using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(EU.Business.Declaration.CusEntryInstruction parent) : base(parent)
		{
		}

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNotAllowDeleteAllEntryLines();
			ValidateIncludeRoutingSecurityData();
			CheckAEOSuportingDocuments();
		}

		protected override void CheckGoodsLocationDescription()
		{
			base.CheckGoodsLocationDescription();
			var parent = Parent;
			var autorizationNumber = parent.GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty;
			var declarationLocation = parent.JobDeclaration?.JE_LocationOfGoods ?? ZString.Empty;
			if (declarationLocation.StartsWith("ES") && !autorizationNumber.IsEmpty && autorizationNumber != declarationLocation && (parent.EntryHeader?.MovementReferenceNumber ?? ZString.Empty).IsEmpty)
			{
				parent.GoodsLocationDescriptionInfo.AddWarning(Res.GetString("D7DB3110-F48C-4400-93F1-F10B48535DCF", "Entry Location does not match with Declaration/Shipment Details/[30] Goods Location."));
			}
		}

		protected void ValidateIncludeRoutingSecurityData()
		{
			ValidateCalculatedProperty(Parent.IncludeRoutingSecurityDataInfo);
		}

		protected void CheckIncludeRoutingSecurityData()
		{
			var isExsAndSpecificCircunstancesIndicatoNoTB = Parent.IsEXS && Parent.JobDeclaration.ZG_SpecificCircumstanceIndicator != SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies;
			if (isExsAndSpecificCircunstancesIndicatoNoTB && !Parent.IncludeRoutingSecurityData)
			{
				Parent.IncludeRoutingSecurityDataInfo.AddWarning(Res.GetString("1A655D1F-BF09-4291-A990-3C024D100E2C", "Itinerary is mandatory if specific circumstance indicator has not 'B' value."));
			}
			if (isExsAndSpecificCircunstancesIndicatoNoTB && Parent.IncludeRoutingSecurityData && !Parent.JobDeclaration.Transports.Any())
			{
				Parent.IncludeRoutingSecurityDataInfo.AddWarning(Res.GetString("436BE2B4-F3FD-4363-8563-115631AAB5EB", "If there is no value in Routing tab, only dispatch and destination country will be included as declaration Itinerary."));
			}
			if (IsSecurityFlagMandatoryForExportUCC6AndEntryStyleIsEXAndDeclarationSecurityIsTicked() && !Parent.IncludeRoutingSecurityData)
			{
				Parent.IncludeRoutingSecurityDataInfo.AddMessageError(Res.GetString("3DF51805-6A57-4EAE-9578-D7C5AB8F2225", "If security value is 2, countries of routing of consignment must be sent.\r\n\r\nPlease tick this field and add the countries of routing in Routing tab. If no rows are added in Routing tab, only country of origin and destination will be submitted as countries of routing."));
			}
		}

		ZBool IsSecurityFlagMandatoryForExportUCC6AndEntryStyleIsEXAndDeclarationSecurityIsTicked()
		{
			var declaration = Parent.JobDeclaration;
			var entryHeader = Parent.EntryHeader;
			if (entryHeader != null)
			{
				return entryHeader.IsExportUCC6 && declaration.JE_EntryStyle == EntryStyleListExport.Codes.ExportNormal && declaration.ZG_IsSecurityDeclaration;
			}
			return false;
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			if ((Parent.JobDeclaration?.IsImport ?? false) && Parent.IsH2 && Parent.JobDeclaration.JE_EntryStyle == EU.Business.EntryStyleListImport.Codes.ImportFromEFTAMember)
			{
				Parent.CEI_StyleInfo.AddMessageError(Res.GetString("ECC197A4-4A68-4D3F-8E56-607E70DBD4A4", "Value EU is not valid for H2 declarations, only values IM or CO may be used in Declaration/Shipment Type/[1a] Entry Style."));
			}
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_SubStyleInfo);
		}

		protected override void CheckCEI_OA_Warehouse()
		{
			base.CheckCEI_OA_Warehouse();
			if (!Parent.CEI_OA_Warehouse.IsEmpty && Parent.FromWarehouseCode.IsEmpty)
			{
				Parent.CEI_OA_WarehouseInfo.AddMessageError(EmptyControlledPremisesMessageError);
			}
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();
			if (!Parent.CEI_OA_Warehouse2.IsEmpty && Parent.ToWarehouseCode.IsEmpty)
			{
				Parent.CEI_OA_Warehouse2Info.AddMessageError(EmptyControlledPremisesMessageError);
			}
		}

		public void ValidateNotAllowDeleteAllEntryLines()
		{
			SetNotAllowDeleteEntryLineErrorForPDI();
		}

		public void AddNotAllowDeleteEntryLinesErrorForPDIOnMerged()
		{
			SetNotAllowDeleteEntryLineErrorForPDI();
		}

		void SetNotAllowDeleteEntryLineErrorForPDI()
		{
			var entryHeader = Parent.EntryHeader;
			if (entryHeader != null)
			{
				Parent.ClearRowNotificationsContaining(NotAllowDeleteEntryLineErrorMessageForPDI(entryHeader));
				if (entryHeader.CH_EntryStatus == EntryStatusCodes.IncompletePreDeclaration && entryHeader.MergedLines.Cast<CusEntryLine>().All(x => x.InvoiceLines.Count == 0))
				{
					Parent.AddRowError(NotAllowDeleteEntryLineErrorMessageForPDI(entryHeader));
				}
			}
		}

		void CheckAEOSuportingDocuments()
		{
			if ((Parent.IsT2L || Parent.IsT2C) && HasAnySupDocForCode(AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo) && HasAnySupDocForCode(AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo))
			{
				Parent.AddRowMessageError(Res.GetString("BE2499E3-7B6A-435A-BB0F-9C276D2D0F2F", "Documents Y024 and Y025 must not be declared together in the same entry for T2L declarations."));
			}

			bool HasAnySupDocForCode(string code) => EntryInstructionHasSupportingDocument(code)
											|| InvoiceHeaderHasSupportingDocument(code)
											|| InvoiceLineHasSupportingDocument(code)
											|| JobDeclarationHasSupportingDocument(code);

			bool EntryInstructionHasSupportingDocument(string code) => Parent.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == code);
			bool InvoiceHeaderHasSupportingDocument(string code) => Parent.Invoices?.Cast<JobComInvoiceHeader>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()).Any(x => x.CSI_Code == code) ?? false;
			bool InvoiceLineHasSupportingDocument(string code) => Parent.InvoiceLines?.Cast<JobComInvoiceLine>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()).Any(x => x.CSI_Code == code) ?? false;
			bool JobDeclarationHasSupportingDocument(string code) => Parent.JobDeclaration?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == code) ?? false;
		}

		ZString EmptyControlledPremisesMessageError => Res.GetString("AE2C763C-BFF4-46D7-B8F9-B29E50F50610", "The selected organization Address does not have a Customs Controlled Premises Code");
		ZString NotAllowDeleteEntryLineErrorMessageForPDI(CusEntryHeader entryHeader) => Res.GetString("D0705564-5D0F-490A-902F-32D4637E8F45", "PDI accepted ({0}) : Cannot remove all Entry lines from this entry. If you need to do so, please cancel that entry by sending a cancellation to Customs. Please, cancel and reopen the declaration. Data will revert to the last save.", entryHeader?.CH_BGMReference ?? ZString.Empty);
	}
}
