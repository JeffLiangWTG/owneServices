using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using GBCodeList = Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Chief.Declaration
{
	public class ChiefCusEntryInstructionLookups : CusEntryInstructionLookups
	{
		public ChiefCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		protected override CodeDescriptionPairList GetEntrySubstyleList(EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport parent)
		{
			var declaration = Parent?.JobDeclaration;
			if (declaration != null)
			{
				if (!declaration.JE_DeclarationType.IsEmpty)
				{
					return Factory.GetCachedValue(
						"ChiefCusEntryInstructionLookups.JE_EntrySubStyle_" + declaration.JE_DeclarationType,
						() => GetSubStyleFromDeclarationType(declaration.JE_DeclarationType));
				}
				else
				{
					if (parent.IsImport)
					{
						return Factory.GetCachedValue<GBCodeList.EntrySubStyleListImport>();
					}
					if (parent.IsExport)
					{
						return Factory.GetCachedValue<GBCodeList.EntrySubStyleListExport>();
					}
				}
			}
			return new CodeDescriptionPairList();
		}

		CodeDescriptionPairList GetSubStyleFromDeclarationType(ZString declarationType)
		{
			var result = new CodeDescriptionPairList();
			switch (declarationType)
			{
				// Import....
				case ImportSADDeclarationTypeList.Codes.ImportFullDeclaration:
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.NormalFullDeclarationGoodsNotArrived, GBCodeList.EntrySubStyleListImport.Descriptions.NormalFullDeclarationGoodsNotArrived);   // D
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived, GBCodeList.EntrySubStyleListImport.Descriptions.NormalFullAndWrdDeclarationGoodsArrived);  // A
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.SdpSfdGoodsArrived, GBCodeList.EntrySubStyleListImport.Descriptions.SdpSfdGoodsArrived);   // C
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived, GBCodeList.EntrySubStyleListImport.Descriptions.SdpSfdGoodsNotArrived);   // F
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.TransitSfdGoodsArrived, GBCodeList.EntrySubStyleListImport.Descriptions.TransitSfdGoodsArrived);   // G
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.TransitSfdGoodsNotArrived, GBCodeList.EntrySubStyleListImport.Descriptions.TransitSfdGoodsNotArrived);   // H
					break;

				case ImportSADDeclarationTypeList.Codes.ImportFullWarehouse:
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived, GBCodeList.EntrySubStyleListImport.Descriptions.NormalFullAndWrdDeclarationGoodsArrived);  // A 
					break;

				case ImportSADDeclarationTypeList.Codes.ImportClearanceRequest:
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.C21GoodsArrived, GBCodeList.EntrySubStyleListImport.Descriptions.C21GoodsArrived);  // J
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.C21GoodsNotArrived, GBCodeList.EntrySubStyleListImport.Descriptions.C21GoodsNotArrived);   // K
					break;

				case ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration:  // SDI = ISD
				case ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse:
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.LcpEidrSupplementaryDeclarationImportOrWarehouseRemovalSdiSdw, GBCodeList.EntrySubStyleListImport.Descriptions.LcpEidrSupplementaryDeclarationImportOrWarehouseRemovalSdiSdw);  // Z
					result.AddPair(GBCodeList.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration, GBCodeList.EntrySubStyleListImport.Descriptions.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration);  // Y
					break;

				// Exports......

				case ExportSADDeclarationTypeList.Codes.ExportFullDeclaration:
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD, GBCodeList.EntrySubStyleListExport.Descriptions.FullDeclarationGoodsNotArrived_IEFD);  // D
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD, GBCodeList.EntrySubStyleListExport.Descriptions.FullDeclarationGoodsArrived_IEFD);  // A
					break;

				case ExportSADDeclarationTypeList.Codes.ExportClearanceRequest:
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.C21_GoodsArrived_IECR, GBCodeList.EntrySubStyleListExport.Descriptions.C21_GoodsArrived_IECR);  // J
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.C21_GoodsNotArrived_IECR, GBCodeList.EntrySubStyleListExport.Descriptions.C21_GoodsNotArrived_IECR);  // K
					break;

				case ExportSADDeclarationTypeList.Codes.ExportLCPPreShipment:  // ELP
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP, GBCodeList.EntrySubStyleListExport.Descriptions.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP);  //F
					break;

				case ExportSADDeclarationTypeList.Codes.ExportSupplementaryDeclaration:  //	ESD
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.LCP_SupplementaryDeclaration_IESD, GBCodeList.EntrySubStyleListExport.Descriptions.LCP_SupplementaryDeclaration_IESD); //Z
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.SDP_SupplementaryDeclaration_IESD, GBCodeList.EntrySubStyleListExport.Descriptions.SDP_SupplementaryDeclaration_IESD); //Y
					break;

				case ExportSADDeclarationTypeList.Codes.ExportSDPPreShipment:    // ESP
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.SDP_PSA_GoodsArrived_IESP, GBCodeList.EntrySubStyleListExport.Descriptions.SDP_PSA_GoodsArrived_IESP); //C
					result.AddPair(GBCodeList.EntrySubStyleListExport.Codes.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP, GBCodeList.EntrySubStyleListExport.Descriptions.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP); //F
					break;
			}
			return result;
		}

		protected override CodeDescriptionPairList GetDefinedDeclarationTypeList(EU.Business.Declaration.JobDeclaration declaration)
		{
			CodeDescriptionPairList descriptions;
			if (declaration.IsImport)
			{
				descriptions = new ImportSADDeclarationTypeList();
			}
			else if (declaration.IsExport)
			{
				descriptions = new ExportSADDeclarationTypeList();
			}
			else
			{
				descriptions = base.GetDefinedDeclarationTypeList(declaration);
			}
			return descriptions;
		}
	}
}
