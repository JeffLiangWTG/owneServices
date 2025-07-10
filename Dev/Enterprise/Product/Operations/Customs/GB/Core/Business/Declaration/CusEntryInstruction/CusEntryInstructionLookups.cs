using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using Univ = Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		public new CusEntryInstruction Parent
		{
			get { return (CusEntryInstruction)base.Parent; }
		}

		protected override CodeDescriptionPairList GetEntrySubstyleList(EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport parent)
		{
			var result = new CodeDescriptionPairList();
			var declaration = Parent?.JobDeclaration;
			if (declaration != null)
			{
				var ceiStyle = Parent.CEI_Style;
				if (!ceiStyle.IsEmpty)
				{
					if (declaration.IsImport)
					{
						result = declaration.Factory.GetCachedValue(
							"CusEntryInstruction.CEI_StyleImports" + ceiStyle,
							() => GetImportSubStyleFromDeclarationType(ceiStyle));
					}
					else if (declaration.IsExport)
					{
						result = declaration.Factory.GetCachedValue(
							"CusEntryInstruction.CEI_StyleExports" + ceiStyle,
							() => GetExportSubStyleFromDeclarationType(ceiStyle));
					}
				}

				if (result.Count == 0)
				{
					result = Univ.RefCusCodeListTypes.GetCachedList(declaration.Factory, declaration.GetDefaultDataGroupingCode(), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, ZDateTime.Today, null, string.Empty, false);
				}
			}
			return result;
		}

		CodeDescriptionPairList GetImportSubStyleFromDeclarationType(ZString declarationType)
		{
			var result = new CodeDescriptionPairList();
			var list = new EntrySubStyleCodeList(Factory);
			list.Load();

			switch (declarationType)
			{
				case ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse:
				case ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission:
				case ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing:
				case ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories:
				case ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration:
					result.AddPair(EntrySubStyleCodeList.Codes.A, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.A));
					result.AddPair(EntrySubStyleCodeList.Codes.D, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.D));
					result.AddPair(EntrySubStyleCodeList.Codes.Y, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.Y));
					result.AddPair(EntrySubStyleCodeList.Codes.Z, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.Z));
					break;

				case ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing:
					result.AddPair(EntrySubStyleCodeList.Codes.A, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.A));
					result.AddPair(EntrySubStyleCodeList.Codes.D, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.D));
					break;
				case ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration:
					result.AddPair(EntrySubStyleCodeList.Codes.A, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.A));
					result.AddPair(EntrySubStyleCodeList.Codes.D, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.D));
					break;

				case ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration:
					result.AddPair(EntrySubStyleCodeList.Codes.C, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.C));
					result.AddPair(EntrySubStyleCodeList.Codes.B, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.B));
					result.AddPair(EntrySubStyleCodeList.Codes.F, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.F));
					result.AddPair(EntrySubStyleCodeList.Codes.E, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.E));
					break;

				case ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration:
					result.AddPair(EntrySubStyleCodeList.Codes.Q, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.Q));
					break;

				case ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I:
				case ImportDeclarationTypeList.Codes.ImportClearanceRequestC21N:
					result.AddPair(EntrySubStyleCodeList.Codes.J, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.J));
					result.AddPair(EntrySubStyleCodeList.Codes.K, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.K));
					break;

				case ImportDeclarationTypeList.Codes.BulkImportReducedDataSet:
					result.AddPair(EntrySubStyleCodeList.Codes.J, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.J));
					result.AddPair(EntrySubStyleCodeList.Codes.K, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.K));
					break;
			}
			return result;
		}

		CodeDescriptionPairList GetExportSubStyleFromDeclarationType(ZString declarationType)
		{
			var result = new CodeDescriptionPairList();
			var list = new EntrySubStyleCodeList(Factory);
			list.Load();

			switch (declarationType)
			{
				case ExportDeclarationTypeList.Codes.DeclarationForExport:
				case ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods:
					result.AddPair(EntrySubStyleCodeList.Codes.A, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.A));
					result.AddPair(EntrySubStyleCodeList.Codes.D, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.D));
					result.AddPair(EntrySubStyleCodeList.Codes.Y, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.Y));
					result.AddPair(EntrySubStyleCodeList.Codes.Z, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.Z));
					break;

				case ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing:
					result.AddPair(EntrySubStyleCodeList.Codes.A, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.A));
					result.AddPair(EntrySubStyleCodeList.Codes.D, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.D));
					break;

				case ExportDeclarationTypeList.Codes.SimplifiedDeclarationForExport:
					result.AddPair(EntrySubStyleCodeList.Codes.B, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.B));
					result.AddPair(EntrySubStyleCodeList.Codes.C, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.C));
					result.AddPair(EntrySubStyleCodeList.Codes.E, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.E));
					result.AddPair(EntrySubStyleCodeList.Codes.F, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.F));
					break;

				case ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E:
				case ExportDeclarationTypeList.Codes.ExportClearanceRequestC21EEIDRNOP:
					result.AddPair(EntrySubStyleCodeList.Codes.J, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.J));
					result.AddPair(EntrySubStyleCodeList.Codes.K, list.GetDescriptionFromCode(EntrySubStyleCodeList.Codes.K));
					break;
			}
			return result;
		}

		protected override CodeDescriptionPairList GetDefinedDeclarationTypeList(EU.Business.Declaration.JobDeclaration declaration)
		{
			CodeDescriptionPairList descriptions;
			if (declaration.IsImport)
			{
				descriptions = new ImportDeclarationTypeList();
			}
			else if (declaration.IsExport)
			{
				descriptions = new ExportDeclarationTypeList();
			}
			else
			{
				descriptions = base.GetDefinedDeclarationTypeList(declaration);
			}
			return descriptions;
		}
	}
}
