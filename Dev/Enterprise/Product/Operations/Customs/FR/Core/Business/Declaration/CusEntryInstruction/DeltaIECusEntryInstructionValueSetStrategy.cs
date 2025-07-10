using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIECusEntryInstructionValueSetStrategy : CusEntryInstructionValueSetStrategy
	{
		public DeltaIECusEntryInstructionValueSetStrategy(CusEntryInstruction entryInstruction) : base(entryInstruction)
		{
		}

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case CusEntryInstruction.Schema.CEI_Procedure:
					DefaultCEI_Style();
					break;
			}
		}

		protected void DefaultCEI_Style()
		{
			if (EntryInstruction.CEI_Style != DeltaIEImportDeclarationTypeList.Codes.I1)
			{
				EntryInstruction.CEI_Style = GetDefaultEntryStyleFromProcedure();
			}
		}

		public ZString GetDefaultEntryStyleFromProcedure()
		{
			var declaration = (JobDeclaration)EntryInstruction.JobDeclaration;
			var regionOrTerritoryOfDestination = declaration?.JE_RegionOrTerritoryOfDestination ?? ZString.Empty;
			var result = EntryInstruction.CEI_Style;

			switch (EntryInstruction.CEI_Procedure)
			{
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._40:
					if ((declaration?.JE_TransportModeInland ?? ZString.Empty) == TransportTypeList.Codes.Mail)
					{
						result = DeltaIEImportDeclarationTypeList.Codes.H6;
					}
					else if (regionOrTerritoryOfDestination != FRDomesticOverseasTerritories.Codes.CONTI && regionOrTerritoryOfDestination != FRDomesticOverseasTerritories.Codes.CORSE)
					{
						result = DeltaIEImportDeclarationTypeList.Codes.H5;
					}
					else
					{
						result = DeltaIEImportDeclarationTypeList.Codes.H1;
					}
					break;
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._01:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._07:
					if ((declaration?.JE_TransportModeInland ?? ZString.Empty) == TransportTypeList.Codes.Mail)
					{
						result = DeltaIEImportDeclarationTypeList.Codes.H6;
					}
					else
					{
						result = DeltaIEImportDeclarationTypeList.Codes.H1;
					}
					break;
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._42:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._61:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._63:
					if (regionOrTerritoryOfDestination != FRDomesticOverseasTerritories.Codes.CONTI && regionOrTerritoryOfDestination != FRDomesticOverseasTerritories.Codes.CORSE)
					{
						result = DeltaIEImportDeclarationTypeList.Codes.H5;
					}
					else
					{
						result = DeltaIEImportDeclarationTypeList.Codes.H1;
					}
					break;
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._95:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._96:
					result = DeltaIEImportDeclarationTypeList.Codes.H5;
					break;
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._51:
					result = DeltaIEImportDeclarationTypeList.Codes.H4;
					break;
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._53:
					result = DeltaIEImportDeclarationTypeList.Codes.H3;
					break;
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration:
					result = DeltaIEImportDeclarationTypeList.Codes.H2;
					break;
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._43:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._44:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._45:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._46:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._48:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._68:
					result = DeltaIEImportDeclarationTypeList.Codes.H1;
					break;
			}

			return result;
		}
	}
}
