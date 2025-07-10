using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		#region Variant
		readonly static ImmutableArray<string> AvailableSubStyles_A_D = new[] { EU.Business.EntrySubStyleList.Codes.NormalDeclaration, EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA }.ToImmutableArray();
		readonly static ImmutableArray<string> AvailableSubStyles_C = new[] { EU.Business.EntrySubStyleList.Codes.SimplifiedDeclaration }.ToImmutableArray();
		readonly static ImmutableArray<string> AvailableSubStyles_C_F = new[] { EU.Business.EntrySubStyleList.Codes.SimplifiedDeclaration, EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC }.ToImmutableArray();
		readonly static ImmutableArray<string> AvailableSubStyles_Y_Z = new[] { EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic }.ToImmutableArray();
		readonly static ImmutableArray<string> AvailableSubStyles_A_B_D_E = new[] { EU.Business.EntrySubStyleList.Codes.NormalDeclaration, EU.Business.EntrySubStyleList.Codes.IncompleteDeclaration, EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB }.ToImmutableArray();

		[ThreadSafe]
		readonly ImmutableDictionary<string, ImmutableArray<string>> validImportStyleSubStyleCombinations = new Dictionary<string, ImmutableArray<string>>
		{
			{ ImportDeclarationTypeList.Codes.AAV, AvailableSubStyles_C },
			{ ImportDeclarationTypeList.Codes.AZ,  AvailableSubStyles_C },
			{ ImportDeclarationTypeList.Codes.AZL, AvailableSubStyles_C },
			{ ImportDeclarationTypeList.Codes.BA,  AvailableSubStyles_Y_Z },
			{ ImportDeclarationTypeList.Codes.EAV, AvailableSubStyles_A_D },
			{ ImportDeclarationTypeList.Codes.EGN, AvailableSubStyles_Y_Z },
			{ ImportDeclarationTypeList.Codes.EGZ, AvailableSubStyles_Y_Z },
			{ ImportDeclarationTypeList.Codes.EZA, AvailableSubStyles_A_B_D_E },
			{ ImportDeclarationTypeList.Codes.EZL, AvailableSubStyles_A_D },
			{ ImportDeclarationTypeList.Codes.VAV, AvailableSubStyles_C_F },
			{ ImportDeclarationTypeList.Codes.VZA, AvailableSubStyles_C_F },
			{ ImportDeclarationTypeList.Codes.VZL, AvailableSubStyles_C_F },
		}.ToImmutableDictionary();

		public override CodeDescriptionPairList EntrySubStyleList => VariantList;

		public CodeDescriptionPairList VariantList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				var declaration = Parent.JobDeclaration;
				if (declaration != null)
				{
					if (declaration.IsImport)
					{
						list = Factory.GetCachedValue(string.Join("|", "DE.CusEntryInstructionsLookups.ImportVariantList", Parent.CEI_Style), () =>
						{
							if (validImportStyleSubStyleCombinations.TryGetValue(Parent.CEI_Style, out ImmutableArray<string> validSubStyles))
							{
								var impSubStyleList = Factory.GetCachedValue<ImportSubStyleList>();
								foreach (var subStyle in validSubStyles)
								{
									list.Add(new CodeDescriptionPair(subStyle, impSubStyleList.GetDescriptionFromCode(subStyle)));
								}
							}
							return list;
						});
					}
					else if (declaration.IsExport)
					{
						list = Factory.GetCachedValue<ExportDeclarationTypeTimeList>();
					}
				}
				return list;
			}
		}

		#endregion

		public CodeDescriptionPairList EarlyClearanceFlags => Factory.GetCachedValue<EarlyClearanceFlagsList>();

		protected override CodeDescriptionPairList DeclarationTypeListCore
		{
			get
			{
				var parent = Parent;
				var declaration = parent.JobDeclaration;
				if (declaration?.IsExport ?? false)
				{
					var typeTime = parent.CEI_SubStyle;
					return base.Factory.GetCachedValue("DE.CusEntryInstructionLookups.DeclarationTypeListCore_" + typeTime,
						() => ExportDeclarationTypeProcedureList.GetTypeProcedureList(typeTime));
				}
				else if ((declaration?.JE_MessageType ?? ZString.Empty) == Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment)
				{
					return Factory.GetCachedValue<WarehouseAdjustmentDeclarationTypeList>();
				}
				else
				{
					var cashKeyWithInwardProcessingRegistry = string.Join("_", GetDeclarationTypeListCacheKey(declaration), CustomsDataRegistry.Instance.EnableInwardProcessing.Value);
					return base.Factory.GetCachedValue(cashKeyWithInwardProcessingRegistry, () =>
					{
						if (!CustomsDataRegistry.Instance.EnableInwardProcessing.Value)
						{
							var result = new CodeDescriptionPairList();
							result.AddRange(base.DeclarationTypeListCore);
							result.RemoveCode(ImportDeclarationTypeList.Codes.AVABR);

							return result;
						}

						return base.DeclarationTypeListCore;
					});
				}
			}
		}

		protected override string GetDeclarationTypeListCacheKey(EU.Business.Declaration.JobDeclaration declaration)
		{
			var result = base.GetDeclarationTypeListCacheKey(declaration);
			if (declaration != null)
			{
				result = string.Join("_", result, declaration.JE_EntryStyle);
			}
			return result;
		}

		protected override CodeDescriptionPairList GetDefinedDeclarationTypeList(EU.Business.Declaration.JobDeclaration declaration)
		{
			var result = base.GetDefinedDeclarationTypeList(declaration);
			if (declaration?.IsImport ?? false)
			{
				result = new ImportDeclarationTypeList();
				if (declaration.JE_EntryStyle == EntryStyleListImport.Codes.ImportFromSpecialTerritory)
				{
					var list = new CodeDescriptionPairList();
					foreach (ICodeDescription pair in result)
					{
						if (ImportDeclarationTypeList.IsForImportFromSpecialTerritoryEntryStyle(pair.Code))
						{
							list.Add(pair);
						}
					}
					result = list;
				}
			}
			return result;
		}

		public CodeDescriptionPairList SimplifiedGrantAuthorizationList => Factory.GetCachedValue<SimplifiedGrantAuthorizationList>();

		public CodeDescriptionPairList CriteriaTypeList => Factory.GetCachedValue<CriteriaTypeList>();

		public OrgHeaderCollection MainAccountingOrganisations => fMainAccountingOrganisations ?? (fMainAccountingOrganisations = new OrgHeaderCollection(Factory));

		OrgHeaderCollection fMainAccountingOrganisations;

		public CodeDescriptionPairList CPCList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var jobDeclaration = Parent.JobDeclaration;
				if (jobDeclaration != null && jobDeclaration.IsImport)
				{
					var style = Parent.CEI_Style;
					result = GetCachedCpcList(style) ?? GetCachedCpcListUsingRefData(jobDeclaration, style);
				}
				return result;
			}
		}

		CodeDescriptionPairList GetCachedCpcList(ZString style)
		{
			return Factory.GetCachedValue("DE.CusEntryInstructionLookups.CPCList|" + style, () =>
			{
				if (style == string.Empty)
				{
					return new ImportMainProcedureCodeList();
				}

				var list = new CodeDescriptionPairList();
				switch (style)
				{
					case ImportDeclarationTypeList.Codes.AAV:
						list.AddPair(ImportMainProcedureCodeList.Codes._51, ImportMainProcedureCodeList.Descriptions._51);
						break;
					case ImportDeclarationTypeList.Codes.AZ:
						list.AddPair(ImportMainProcedureCodeList.Codes._01, ImportMainProcedureCodeList.Descriptions._01);
						list.AddPair(ImportMainProcedureCodeList.Codes._40, ImportMainProcedureCodeList.Descriptions._40);
						list.AddPair(ImportMainProcedureCodeList.Codes._42, ImportMainProcedureCodeList.Descriptions._42);
						list.AddPair(ImportMainProcedureCodeList.Codes._45, ImportMainProcedureCodeList.Descriptions._45);
						list.AddPair(ImportMainProcedureCodeList.Codes._49, ImportMainProcedureCodeList.Descriptions._49);
						list.AddPair(ImportMainProcedureCodeList.Codes._61, ImportMainProcedureCodeList.Descriptions._61);
						list.AddPair(ImportMainProcedureCodeList.Codes._63, ImportMainProcedureCodeList.Descriptions._63);
						break;
					case ImportDeclarationTypeList.Codes.AZL:
						list.AddPair(ImportMainProcedureCodeList.Codes._71, ImportMainProcedureCodeList.Descriptions._71);
						break;
					case ImportDeclarationTypeList.Codes.EAV:
						list.AddPair(ImportMainProcedureCodeList.Codes._51, ImportMainProcedureCodeList.Descriptions._51);
						break;
					case ImportDeclarationTypeList.Codes.EZA:
						list.AddPair(ImportMainProcedureCodeList.Codes._01, ImportMainProcedureCodeList.Descriptions._01);
						list.AddPair(ImportMainProcedureCodeList.Codes._40, ImportMainProcedureCodeList.Descriptions._40);
						list.AddPair(ImportMainProcedureCodeList.Codes._42, ImportMainProcedureCodeList.Descriptions._42);
						list.AddPair(ImportMainProcedureCodeList.Codes._45, ImportMainProcedureCodeList.Descriptions._45);
						list.AddPair(ImportMainProcedureCodeList.Codes._49, ImportMainProcedureCodeList.Descriptions._49);
						list.AddPair(ImportMainProcedureCodeList.Codes._61, ImportMainProcedureCodeList.Descriptions._61);
						list.AddPair(ImportMainProcedureCodeList.Codes._63, ImportMainProcedureCodeList.Descriptions._63);
						break;
					case ImportDeclarationTypeList.Codes.EZL:
						list.AddPair(ImportMainProcedureCodeList.Codes._71, ImportMainProcedureCodeList.Descriptions._71);
						break;
					case ImportDeclarationTypeList.Codes.LUZ:
						list.AddPair(ImportMainProcedureCodeList.Codes._71, ImportMainProcedureCodeList.Descriptions._71);
						break;
					case ImportDeclarationTypeList.Codes.VAV:
						list.AddPair(ImportMainProcedureCodeList.Codes._51, ImportMainProcedureCodeList.Descriptions._51);
						break;
					case ImportDeclarationTypeList.Codes.VZA:
						list.AddPair(ImportMainProcedureCodeList.Codes._01, ImportMainProcedureCodeList.Descriptions._01);
						list.AddPair(ImportMainProcedureCodeList.Codes._40, ImportMainProcedureCodeList.Descriptions._40);
						list.AddPair(ImportMainProcedureCodeList.Codes._42, ImportMainProcedureCodeList.Descriptions._42);
						list.AddPair(ImportMainProcedureCodeList.Codes._45, ImportMainProcedureCodeList.Descriptions._45);
						list.AddPair(ImportMainProcedureCodeList.Codes._49, ImportMainProcedureCodeList.Descriptions._49);
						list.AddPair(ImportMainProcedureCodeList.Codes._61, ImportMainProcedureCodeList.Descriptions._61);
						list.AddPair(ImportMainProcedureCodeList.Codes._63, ImportMainProcedureCodeList.Descriptions._63);
						break;
					case ImportDeclarationTypeList.Codes.VZL:
						list.AddPair(ImportMainProcedureCodeList.Codes._71, ImportMainProcedureCodeList.Descriptions._71);
						break;
					case ImportDeclarationTypeList.Codes.AVABR:
						list.AddPair(ImportMainProcedureCodeList.Codes._40, ImportMainProcedureCodeList.Descriptions._40);
						break;
					case ImportDeclarationTypeList.Codes.BA:
					case ImportDeclarationTypeList.Codes.EGN:
					case ImportDeclarationTypeList.Codes.EGZ:
						break;
					default:
						return null;
				}

				return list;
			});
		}

		CodeDescriptionPairList GetCachedCpcListUsingRefData(JobDeclaration jobDeclaration, ZString style)
		{
			var dataGroupingCode = jobDeclaration.GetDefaultDataGroupingCode();
			var dateOfValuation = jobDeclaration.DateOfValuation;
			var messageType = jobDeclaration.JE_MessageType;
			return Factory.GetCachedValue(string.Join("|", "DE.CusEntryInstructionLookups.CPCList",
				string.Join("_", dataGroupingCode, dateOfValuation, style, messageType)), () =>
			{
				var list = new CodeDescriptionPairList();
				new RefCusProcedureCollection(Factory, dataGroupingCode, dateOfValuation, style, messageType).ForEach(x => list.AddPairIfNotExist(x.ZZ6_ProcedureCode, x.ZZ6_Description));
				return list;
			});
		}

		public CodeDescriptionPairList InwardProcessingAuthorizationNumberList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				var entryInstruction = Parent;
				var jobDeclaration = entryInstruction.JobDeclaration;
				if (jobDeclaration != null && jobDeclaration.IsImport)
				{
					var ceiStyle = entryInstruction.CEI_Style;
					var transactionDate = ceiStyle == ImportDeclarationTypeList.Codes.AAV ? entryInstruction.CEI_LocalClearanceDate.Date : ZDate.Today;
					var primaryAuthorizationHolder = jobDeclaration.Declarant?.OA_OH ?? ZGuid.Empty;
					var secondaryAuthorizationHolder = ZGuid.Empty;

					if (ceiStyle == ImportDeclarationTypeList.Codes.AAV || ceiStyle == ImportDeclarationTypeList.Codes.VAV)
					{
						if (jobDeclaration.JE_DeclarantType == RepresentationTypeList.Codes._2Direct)
						{
							secondaryAuthorizationHolder = jobDeclaration.Representative?.OA_OH ?? ZGuid.Empty;
						}
						else if (jobDeclaration.JE_DeclarantType == RepresentationTypeList.Codes._3Indirect)
						{
							secondaryAuthorizationHolder = jobDeclaration.BuyingAgentAddress?.OA_OH ?? ZGuid.Empty;
						}
					}

					list = CusAuthorizationHelper.GetAuthorizationNumbersForPrimaryAndSecondaryHolders(Factory, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, transactionDate, primaryAuthorizationHolder, secondaryAuthorizationHolder);
				}
				return list;
			}
		}
	}
}
