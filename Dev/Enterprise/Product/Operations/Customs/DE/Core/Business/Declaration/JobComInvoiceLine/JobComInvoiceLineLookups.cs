using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public override CodeDescriptionPairList BondedWhsUnitQtyList => CustomsUQList;

		protected override bool CustomsUQListIncludeParentDataGrouping => false;

		public override RefCusProcedureCollection CPCList
		{
			get
			{
				var result = base.CPCList;

				var parent = Parent;
				var declaration = parent.Declaration;
				var instruction = parent.EntryInstruction;
				if (parent.IsImport)
				{
					var procedureCode = instruction?.CEI_Procedure ?? ZString.Empty;
					if (!procedureCode.IsEmpty)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusProcedureFilters.CPC, "Property", procedureCode, false));
					}
				}
				else if (IsExport)
				{
					if (instruction != null)
					{
						result = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentTypeAndGroup(Factory, GetDefaultDataGroupingCode(), declaration.JE_MessageType, GetDateOfValuation(), GroupPatterns);
					}
					if (declaration.JE_EntryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusProcedureFilters.ProcedureCode, "Property", (ZString)CustomsProcedureCodeList.Export.ProcedureCode._10, false));
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusProcedureFilters.ProcedureCode, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.StartsWith, false));
					}
				}
				return result;
			}
		}

		public new CodeDescriptionPairList CustomsEntryInstructions => Parent.Declaration?.CustomsEntryInstructionProvider?.SortedEntryInstructionList;

		public RefCountryCollection ExportCountryList
		{
			get
			{
				var result = new RefCountryCollection(Factory);
				if (IsExport)
				{
					var query = new ZQuery(RefCountrySchema.RN_EconomicGrouping, EconomicGroupList.Codes.EuropeanUnion);
					result.AdditionalFilter = query;
				}
				return result;
			}
		}

		public CodeDescriptionPairList StateOrRegionOfOriginList
		{
			get
			{
				var countryOfOriginIsDE = Parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.Germany;
				return Factory.GetCachedValue("DEJobComInvoiceLineLookups.StateOrRegionOfOriginList|" + countryOfOriginIsDE, () =>
				{
					var result = new CodeDescriptionPairList();
					if (countryOfOriginIsDE)
					{
						result = new OriginFederalStateList();
						result.RemoveCode(Business.OriginFederalStateList.Codes.Ursprungsausland);
					}
					else
					{
						result.AddPair(Business.OriginFederalStateList.Codes.Ursprungsausland, Business.OriginFederalStateList.Descriptions.Ursprungsausland);
					}
					return result;
				});
			}
		}

		protected override ZString CountryOfOriginForPrimaryPreferenceList => Parent.EffectiveCountryOfOrigin;

		bool IsExport => Parent.Declaration?.IsExport ?? false;

		protected IEnumerable<string> GroupPatterns
		{
			get
			{
				var instruction = Parent.EntryInstruction;
				var subStyle = instruction.CEI_SubStyle;
				var style = instruction.CEI_Style;
				yield return subStyle + style;
				yield return subStyle + style.SubstringSafe(0, 2) + "****";
				yield return subStyle + style.SubstringSafe(0, 3) + "*" + style.SubstringSafe(4, 2);
				yield return "**" + style.SubstringSafe(0, 1) + "*****";
				yield return "**" + style.SubstringSafe(0, 2) + "****";
			}
		}
	}
}
