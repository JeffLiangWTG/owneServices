using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.JP.Business.DeclarationCargoTypeList;
using static Enterprise.Customs.JP.Business.JPExportDeclarationTypeList;
using static Enterprise.Customs.JP.Business.JPImportDeclarationTypeList;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Business
{
	public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(AutoCusEntryInstruction parent) : base(parent) { }

		public CodeDescriptionPairList ValueTypeList => Factory.GetCachedValue<ValueTypeList>();

		#region TradeTypeFirstCharList

		public CodeDescriptionPairList TradeTypeFirstCharList
		{
			get
			{
				var transportMode = JobDeclaration.JE_TransportMode;
				var declarationCargoType = CusEntryInstruction.CEI_DeclarationCargoType;
				var isImport = JobDeclaration.IsImport;

				return Factory.GetCachedValue($"transportMode:{transportMode}, declarationCargoType:{declarationCargoType}, isImport:{isImport}", () =>
				{
					var res = new TradeTypeFirstChar();

					if (transportMode != Enterprise.Core.Constants.TransportModes.Sea)
					{
						res.RemoveCode(TradeTypeFirstChar.Codes.E);
					}

					if (transportMode != Enterprise.Core.Constants.TransportModes.Air)
					{
						res.RemoveCode(TradeTypeFirstChar.Codes.A);
					}

					if (!string.IsNullOrEmpty(declarationCargoType) && !new MailedCargoList().ContainsCode(declarationCargoType))
					{
						res.RemoveCode(TradeTypeFirstChar.Codes.B);
					}

					if (!isImport)
					{
						res.RemoveCode(TradeTypeFirstChar.Codes.C);
					}

					return res;
				});
			}
		}

		#endregion

		#region TradeTypeSecondCharList

		public CodeDescriptionPairList TradeTypeSecondCharList
		{
			get
			{
				var res = new CodeDescriptionPairList();

				var declarationType = CusEntryInstruction.CEI_Style;
				var isExport = JobDeclaration.IsExport;
				var isImport = JobDeclaration.IsImport;
				var cei_BeforePermitApplicationReason = CusEntryInstruction.CEI_BeforePermitApplicationReason;

				return Factory.GetCachedValue($"declarationType:{declarationType}, isExport:{isExport}, isImport:{isImport}, cei_BeforePermitApplicationReason:{cei_BeforePermitApplicationReason}", () =>
				{
					if (isExport)
					{
						if (declarationType == JPExportDeclarationTypeList.Codes.R)
						{
							res.AddPair(TradeTypeSecondChar.Codes.B, TradeTypeSecondChar.Descriptions.B);
							res.AddPair(TradeTypeSecondChar.Codes.C, TradeTypeSecondChar.Descriptions.C);
							res.AddPair(TradeTypeSecondChar.Codes.D, TradeTypeSecondChar.Descriptions.D);
							res.AddPair(TradeTypeSecondChar.Codes.I, TradeTypeSecondChar.Descriptions.I);
						}
						else
						{
							res.AddPairIfNotExist(TradeTypeSecondChar.Codes.A, TradeTypeSecondChar.Descriptions.A);
						}
					}
					else if (isImport)
					{
						switch (declarationType)
						{
							case JPImportDeclarationTypeList.Codes.C:
							case JPImportDeclarationTypeList.Codes.F:
							case JPImportDeclarationTypeList.Codes.H:
							case JPImportDeclarationTypeList.Codes.N:
							case JPImportDeclarationTypeList.Codes.J:
							case JPImportDeclarationTypeList.Codes.P:
								res.AddPair(TradeTypeSecondChar.Codes.A, TradeTypeSecondChar.Descriptions.A);
								break;
							case JPImportDeclarationTypeList.Codes.S:
								res.AddPair(TradeTypeSecondChar.Codes.B, TradeTypeSecondChar.Descriptions.B);
								break;
							case JPImportDeclarationTypeList.Codes.M:
								res.AddPair(TradeTypeSecondChar.Codes.C, TradeTypeSecondChar.Descriptions.C);
								break;
							case JPImportDeclarationTypeList.Codes.A:
								res.AddPair(TradeTypeSecondChar.Codes.I, TradeTypeSecondChar.Descriptions.I);
								break;
							case JPImportDeclarationTypeList.Codes.K:
							case JPImportDeclarationTypeList.Codes.D:
							case JPImportDeclarationTypeList.Codes.R:
								res.AddPair(TradeTypeSecondChar.Codes.E, TradeTypeSecondChar.Descriptions.E);
								break;
							case JPImportDeclarationTypeList.Codes.U:
							case JPImportDeclarationTypeList.Codes.L:
								res.AddPair(TradeTypeSecondChar.Codes.F, TradeTypeSecondChar.Descriptions.F);
								break;
							case JPImportDeclarationTypeList.Codes.B:
							case JPImportDeclarationTypeList.Codes.E:
								res.AddPair(TradeTypeSecondChar.Codes.J, TradeTypeSecondChar.Descriptions.I);
								break;
						}

						if (!string.IsNullOrEmpty(cei_BeforePermitApplicationReason))
						{
							res.AddPair(TradeTypeSecondChar.Codes.D, TradeTypeSecondChar.Descriptions.D);
							res.AddPair(TradeTypeSecondChar.Codes.G, TradeTypeSecondChar.Descriptions.I);
						}

						res.AddPair(TradeTypeSecondChar.Codes.H, TradeTypeSecondChar.Descriptions.H);
					}

					res.Sort();
					return res;
				});
			}
		}

		#endregion

		#region TradeTypeThirdCharList

		public CodeDescriptionPairList TradeTypeThirdCharList
		{
			get
			{
				var commercialValueType = CusEntryInstruction.CEI_CommercialValueType;
				var isImport = JobDeclaration.IsImport;

				return Factory.GetCachedValue($"commercialValueType:{commercialValueType}, isImport:{isImport}", () =>
				{
					var res = new TradeTypeThirdChar();

					if (!string.IsNullOrEmpty(commercialValueType) && commercialValueType != CommercialValueTypeList.Codes.ImportApprovalHasCommercialOrCombination)
					{
						res.RemoveCode(TradeTypeThirdChar.Codes.E);
					}

					if (!(isImport && (string.IsNullOrEmpty(commercialValueType) || commercialValueType == CommercialValueTypeList.Codes.ImportApprovalNoCommercialValue)))
					{
						res.RemoveCode(TradeTypeThirdChar.Codes.G);
					}

					return res;
				});
			}
		}

		#endregion

		public ImportTradeControlOrdinanceArticle3CodeList ImportTradeControlOrdinanceArticle3CodeList => Factory.GetCachedValue<ImportTradeControlOrdinanceArticle3CodeList>();

		public IDACertificateIdList IDACertificateIdList => Factory.GetCachedValue<IDACertificateIdList>();

		public CodeDescriptionPairList ApprovalCertificateCategoryList => Factory.GetCachedValue<ApprovalCertificateCategoryCodeList>();

		public CodeDescriptionPairList BeforePermitApplicationReasonList => Factory.GetCachedValue<BeforePermitApplicationReasonList>();

		public CodeDescriptionPairList GrossWeightUnitList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public CodeDescriptionPairList CommercialValueTypes => Factory.GetCachedValue<CommercialValueTypeList>();

		public CodeDescriptionPairList ContentInspectionResultList => Factory.GetCachedValue<ContentInspectionResultList>();

		public CodeDescriptionPairList CustomsOfficeDepartmentForSpecialDeclarationsList
		{
			get
			{
				var customsOfficeForSpecialDeclarations = CusEntryInstruction.CEI_CustomsOfficeForSpecialDeclarations;
				var today = ZDateTime.Today;

				return customsOfficeForSpecialDeclarations.Length < 2
					? new CodeDescriptionPairList()
					: Factory.GetCachedValue($"JPCustomsOfficeDepartment-{today}-{customsOfficeForSpecialDeclarations}", () =>
					{
						var filter = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.StartsWith, customsOfficeForSpecialDeclarations.SubstringSafe(0, 2));
						var departmentCodes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCustomsOfficeDepartment, today, filter).OrderBy(x => x.ZZD_Code);

						var result = new CodeDescriptionPairList();
						departmentCodes.ForEach(x => result.AddPairIfNotExist(x.ZZD_Code.SubstringSafe(2, 2), x.ZZD_Description));

						return result;
					});
			}
		}

		public CodeDescriptionPairList CustomsOfficeList
		{
			get
			{
				var today = ZDateTime.Today;

				return Factory.GetCachedValue($"JPCustomsOfficeList-{today}", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, today));
					return result;
				});
			}
		}

		public IBusinessObjectCollection BondedLocationList => JPRefCusCodeListTypes.GetJapanBondedAreaCodes(Factory);

		public IBusinessObjectCollection SpecialCargoCodeList => JPRefCusCodeListTypes.GetSpecialCargoCodes(Factory);

		public CodeDescriptionPairList VolumeUnitList => CustomsUnitOfMeasureList.GetVolumeUnitList(Factory);

		public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();

		public CodeDescriptionPairList PreInspectedCargoTypeList => Factory.GetCachedValue<PreInspectedCargoTypeList>();

		public CodeDescriptionPairList CargoTypeList => Factory.GetCachedValue<CargoTypeList>();

		public CodeDescriptionPairList BillNumberTypeList => Factory.GetCachedValue<BillNumberTypeList>();

		public CodeDescriptionPairList CDB01CargoTypeList => Factory.GetCachedValue<CDB01CargoTypeList>();

		public ICollection MoveInDestinationCodeList => MoveInDestinationLookups.GetRefCusCodeLisCollection(Factory, CusEntryInstruction, false);

		public CodeDescriptionPairList RCRActionList => Factory.GetCachedValue<RCRActionList>();

		public ZZRefCusCodeListCombinedCollection ViaList => GetRefCusCodeLisCollection();

		ZZRefCusCodeListCombinedCollection GetRefCusCodeLisCollection()
		{
			var today = ZDateTime.Today;
			var jobDeclaration = CusEntryInstruction.JobDeclaration;
			var customsOfficeFirstChar = jobDeclaration.JE_CustomsOffice.SubstringSafe(0, 1);
			var transportMode = jobDeclaration.JE_TransportMode;

			var result = Factory.GetCachedValue($"JP.CusEntryInstructionLookups.GetRefCusCodeLisCollection-{today.ToShortDateString()}-{customsOfficeFirstChar}", () =>
			{
				var refCusCodeListCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, today);

				refCusCodeListCollection.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
				filterName: Universal.Constants.ZZRefCusCodeListFilters.Code,
				propertyName: "Property",
				value: customsOfficeFirstChar,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.StartsWith));

				refCusCodeListCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode), true));
				refCusCodeListCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(Core.Constants.CountryCodes.Japan), true));

				return refCusCodeListCollection;
			});

			result.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
				filterName: Universal.Constants.ZZRefCusCodeListFilters.TransportMode,
				propertyName: "Property",
				value: transportMode,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact));

			return result;
		}

		public CodeDescriptionPairList ReceiptModeList => Factory.GetCachedValue<ReceiptModeList>();

		public RefUNLOCOCollection FinalDestinations
		{
			get
			{
				var result = new RefUNLOCOCollection(Factory, FinalDestinationPortFilter());
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", CusEntryInstruction.FinalDestination));
				return result;
			}
		}

		ZQuery FinalDestinationPortFilter()
		{
			var result = new ZQuery();
			if (JobDeclaration.IsImport)
			{
				var origin = JobDeclaration.JE_RL_NKOrigin;
				if (!origin.IsEmpty)
				{
					result = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, origin.Left(2));
				}
			}
			else if (JobDeclaration.IsExport)
			{
				result.AddToFilter(new ZQuery().AddToFilter(JoinCondition.And, RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, JobDeclaration.CountryCode));
			}

			return result;
		}

		public CodeDescriptionPairList DeclarationCargoTypes
		{
			get
			{
				var cusEntryInstruction = CusEntryInstruction;
				var jobDeclaration = JobDeclaration;
				var messageType = jobDeclaration?.JE_MessageType ?? ZString.Empty;
				var transportMode = jobDeclaration?.JE_TransportMode ?? ZString.Empty;
				var style = cusEntryInstruction.CEI_Style;
				return Factory.GetCachedValue($"DeclarationCargoTypes-{ZDateTime.Today}-{messageType}-{transportMode}-{style}", () =>
				{
					var result = new CodeDescriptionPairList();
					if (messageType == JobMessageTypeList.Codes.Import)
					{
						result = GetImportDeclarationCargoTypeList(result, transportMode, style);
					}
					else if (messageType == JobMessageTypeList.Codes.Export)
					{
						result = GetExportDeclarationCargoTypeList(result, transportMode, style);
					}
					RemoveDeclarationCargoTypeIfNeeded(result, messageType, style);
					result.Sort();
					return result;
				});
			}
		}

		CodeDescriptionPairList GetImportDeclarationCargoTypeList(CodeDescriptionPairList list, ZString transportMode, ZString style)
		{
			if (transportMode == Enterprise.Core.Constants.TransportModes.Air)
			{
				list = new ImportAirCargoList();
			}
			else if (transportMode == Enterprise.Core.Constants.TransportModes.Sea)
			{
				list = new ImportSeaCargoList();
			}
			if (style == JPImportDeclarationTypeList.Codes.C)
			{
				list.AddPairIfNotExist(DeclarationCargoTypeList.Codes.X, DeclarationCargoTypeList.Descriptions.X);
			}
			return list;
		}

		CodeDescriptionPairList GetExportDeclarationCargoTypeList(CodeDescriptionPairList list, ZString transportMode, ZString style)
		{
			if (transportMode == Enterprise.Core.Constants.TransportModes.Air)
			{
				switch (style)
				{
					case JPExportDeclarationTypeList.Codes.G:
						list.AddPairIfNotExist(ExportAirCargoList.Codes.S, ExportAirCargoList.Descriptions.S);
						list.AddPairIfNotExist(ExportAirCargoList.Codes.B, ExportAirCargoList.Descriptions.B);
						break;
					case JPExportDeclarationTypeList.Codes.T:
						list.AddPairIfNotExist(ExportAirCargoList.Codes.S, ExportAirCargoList.Descriptions.S);
						list.AddPairIfNotExist(ExportAirCargoList.Codes.B, ExportAirCargoList.Descriptions.B);
						list.AddPairIfNotExist(ExportAirCargoList.Codes.K, ExportAirCargoList.Descriptions.K);
						break;
					default:
						list = new ExportAirCargoList();
						break;
				}
			}
			else if (transportMode == Enterprise.Core.Constants.TransportModes.Sea)
			{
				list = new ExportSeaCargoList();
			}
			return list;
		}

		void RemoveDeclarationCargoTypeIfNeeded(CodeDescriptionPairList list, ZString messageType, ZString style)
		{
			if (messageType == JobMessageTypeList.Codes.Export && (style == JPExportDeclarationTypeList.Codes.N ||
				style == JPExportDeclarationTypeList.Codes.M ||
				style == JPExportDeclarationTypeList.Codes.T ||
				style == JPExportDeclarationTypeList.Codes.G))
			{
				list.RemoveCode(MailedCargoList.Codes.E);
				list.RemoveCode(MailedCargoList.Codes.H);
				list.RemoveCode(MailedCargoList.Codes.M);
				list.RemoveCode(MailedCargoList.Codes.U);
			}
		}

		public override CodeDescriptionPairList EntrySubStyleList => Factory.GetCachedValue<JPAdditionalDeclarationTypeList>();

		public override CodeDescriptionPairList StyleList
		{
			get
			{
				var result = base.StyleList;
				var cusEntryInstruction = CusEntryInstruction;
				if (cusEntryInstruction.IsImport)
				{
					result = Factory.GetCachedValue<JPImportDeclarationTypeList>();
				}
				else if (cusEntryInstruction.IsExport)
				{
					var invoiceLines = cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
					if (invoiceLines.Any(l => l.JI_NACCSCode == ExportNACCSCodeList.Codes.T))
					{
						result = Factory.GetCachedValue<ReturnedGoodsDeclarationTypeList>();
					}
					else if (invoiceLines.Any(l => l.OtherLaws.Cast<CusOtherLawReferenceForInvoiceLines>().Any(o => o.IsRoadTranportVehicleLaw)))
					{
						result = Factory.GetCachedValue<BaseExportDeclarationTypeList>();
					}
					else
					{
						result = Factory.GetCachedValue<JPExportDeclarationTypeList>();
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList DeclarationConditionList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var instruction = CusEntryInstruction;
				var declarationType = instruction.CEI_Style.ToString();
				if (instruction.IsImport)
				{
					var isNormalDeclarationType = NormalDeclarationTypes.Contains(declarationType);
					if (isNormalDeclarationType)
					{
						result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.T, ImportDeclarationConditionList.Descriptions.T);
						result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.H, ImportDeclarationConditionList.Descriptions.H);
						result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.Empty, ImportDeclarationConditionList.Descriptions.Empty);
						result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.K, ImportDeclarationConditionList.Descriptions.K);
					}

					if (!new MailedCargoList().GetAllCodes().Contains(instruction.CEI_DeclarationCargoType.ToString()) && !instruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.JI_BondedDate.IsEmpty))
					{
						var isSea = instruction.IsSea;

						if (isNormalDeclarationType)
						{
							result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.Z, ImportDeclarationConditionList.Descriptions.Z);
							result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.U, ImportDeclarationConditionList.Descriptions.U);
							result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.S, ImportDeclarationConditionList.Descriptions.S);

							if (isSea && declarationType != JPImportDeclarationTypeList.Codes.Y)
							{
								result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.I, ImportDeclarationConditionList.Descriptions.I);
							}
						}

						if (isSea && declarationType == JPImportDeclarationTypeList.Codes.G)
						{
							result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.I, ImportDeclarationConditionList.Descriptions.I);
						}

						if (AutomaticStartDeclarationTypes.Contains(declarationType))
						{
							result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.J, ImportDeclarationConditionList.Descriptions.J);
						}
					}

					if (OpeningAndNormalRegDeclarationTypes.Contains(declarationType))
					{
						result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.Empty, ImportDeclarationConditionList.Descriptions.Empty);
						result.AddPairIfNotExist(ImportDeclarationConditionList.Codes.K, ImportDeclarationConditionList.Descriptions.K);
					}
				}
				else if (instruction.IsExport)
				{
					if (declarationType == JPExportDeclarationTypeList.Codes.E || declarationType == JPExportDeclarationTypeList.Codes.R)
					{
						result = Factory.GetCachedValue<ExportDeclarationConditionListWhenDeclarationTypeIsEorR>();
					}
					else
					{
						result = Factory.GetCachedValue<ExportDeclarationConditionListWhenDeclarationTypeIsNotEorR>();
					}
				}

				return result;
			}
		}

		public CodeDescriptionPairList CustomsVolumeUnitList => Factory.GetCachedValue<CustomsVolumeUnitList>();

		public CodeDescriptionPairList CustomsWeightUnitConditionList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var instruction = CusEntryInstruction;

				if (instruction.IsImport && instruction.IsAir)
				{
					result.AddPairIfNotExist(CustomsWeightUnitList.Codes.Kilograms, CustomsWeightUnitList.Descriptions.Kilograms);
					result.AddPairIfNotExist(CustomsWeightUnitList.Codes.Pound, CustomsWeightUnitList.Descriptions.Pound);
				}
				else if (instruction.IsExport && instruction.IsAir)
				{
					result.AddPairIfNotExist(CustomsWeightUnitList.Codes.Kilograms, CustomsWeightUnitList.Descriptions.Kilograms);
				}
				else
				{
					result.AddPairIfNotExist(CustomsWeightUnitList.Codes.Kilograms, CustomsWeightUnitList.Descriptions.Kilograms);
					result.AddPairIfNotExist(CustomsWeightUnitList.Codes.Tonnes, CustomsWeightUnitList.Descriptions.Tonnes);
					result.AddPairIfNotExist(CustomsWeightUnitList.Codes.Pound, CustomsWeightUnitList.Descriptions.Pound);
				}

				return result;
			}
		}

		ZString[] NormalDeclarationTypes => new ZString[]
		{
			JPImportDeclarationTypeList.Codes.C,
			JPImportDeclarationTypeList.Codes.F,
			JPImportDeclarationTypeList.Codes.Y,
			JPImportDeclarationTypeList.Codes.H,
			JPImportDeclarationTypeList.Codes.N,
			JPImportDeclarationTypeList.Codes.J,
			JPImportDeclarationTypeList.Codes.P,
			JPImportDeclarationTypeList.Codes.S,
			JPImportDeclarationTypeList.Codes.M,
			JPImportDeclarationTypeList.Codes.A
		};

		IEnumerable<string> AutomaticStartDeclarationTypes => new TakeoverAndSpecialCaseDeclarationTypeList().GetAllCodes().Concat(new TakeoverDeclarationTypeList().GetAllCodes());

		IEnumerable<string> OpeningAndNormalRegDeclarationTypes => new BondedImportDeclarationTypeList().GetAllCodes().Append(JPImportDeclarationTypeList.Codes.G).Append(JPImportDeclarationTypeList.Codes.R);

		CusEntryInstruction CusEntryInstruction => (CusEntryInstruction)Parent;

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
	}
}
