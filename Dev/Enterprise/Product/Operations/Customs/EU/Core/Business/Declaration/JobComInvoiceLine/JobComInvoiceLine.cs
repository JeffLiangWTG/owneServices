using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public partial class JobComInvoiceLine : AutoJobComInvoiceLine
		, ICanBeImportOrExport
		, Integration.Customs.EU.IJobComInvoiceLine
		, ICusSupportingInfoTypeSupporter
		, ICusReferenceTypeSupporter
		, IUltimateDistributee
		, ICusFiscalReferenceProviderWithValidationDecider
		, IAdditionalInfosProviderWithValidationDecider
		, ISupportingDocumentsProviderWithValidationDecider
		, IPreviousDocumentsProviderWithValidationDecider
		, ITaxAndDocsProvider
		, IDocAddresses
		, ISupplementaryCodeSupporter
		, ICusCodeDataTypeSupporter
		, IAdditionalProcedureParent
		, IAdditionalLineTariffDetailParent
		, IAddInfoWithSyncPropertySupporter
		, ICusAuthorizationUsageMaster
		, ISupportMultipleResourceStringData
		, IEffectiveValueManagerSupporter
		, INationalAdditionalCodeSupporter
		, IUcc6ValueProvider
		, ICusAuthorizationUsageProviderWithValidationDecider
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobComInvoiceLine.Schema
		{
			public const string JI_SupplementaryCode1 = "JI_SupplementaryCode1";
			public const string JI_SupplementaryCode2 = "JI_SupplementaryCode2";
			public const string JI_NationalAdditionalCode1 = "JI_NationalAdditionalCode1";
			public const string JI_NationalAdditionalCode2 = "JI_NationalAdditionalCode2";
			public const string RelatedIndicator = "RelatedIndicator";
			public const string RelatedIndicator2 = "RelatedIndicator2";
			public const string RelatedIndicator3 = "RelatedIndicator3";
			public const string RelatedIndicator4 = "RelatedIndicator4";
			public const string JI_FormattedProcedure = "JI_FormattedProcedure";
			public const string EntryReferenceNumber = "EntryReferenceNumber";
			public const string EntryInstructionDescription = "EntryInstructionDescription";
			public const string JI_TaxOrFeeDetail = "JI_TaxOrFeeDetail";
			public const string JI_Calc_RequestedProcedure = "JI_Calc_RequestedProcedure";
			public const string JI_PreviousProcedure = "JI_PreviousProcedure";
		}

		public new static readonly JobComInvoiceLineTypeDecider TypeDecider = new JobComInvoiceLineTypeDecider();
		// Need this here, even though it does the same as base, to make the plumbing work. Framework looks for a member called "TypeDecider" directly on the initial-load type (ie EU) and not in a base class.

		public AddInfoJobComInvoiceLineLookups GetAddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine addInfoJobDeclaration) => GetAddInfoJobComInvoiceLineLookupsCore(addInfoJobDeclaration);

		protected virtual AddInfoJobComInvoiceLineLookups GetAddInfoJobComInvoiceLineLookupsCore(AddInfoJobComInvoiceLine addInfo) => new AddInfoJobComInvoiceLineLookups(addInfo);

		public new CustomsQuantityConverter CustomsQuantityConverter => (CustomsQuantityConverter)base.CustomsQuantityConverter;

		protected override BaseCustomsQuantityConverter GetCustomsQuantityConverter()
			=> new CustomsQuantityConverter(this, (ZPropertyInfoDecimal)JI_CustomsQuantityInfo, (ZPropertyInfoString)JI_CustomsUnitQtyInfo);

		public new CustomsQuantityConverter CustomsQuantity2Converter => (CustomsQuantityConverter)base.CustomsQuantity2Converter;

		protected override BaseCustomsQuantityConverter GetCustomsQuantity2Converter()
			=> new CustomsQuantityConverter(this, (ZPropertyInfoDecimal)JI_CustomsSecondQuantityInfo, (ZPropertyInfoString)JI_CustomsSecondUnitQtyInfo);

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
		{
			return new UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>((invoiceLine) => new RateView[] { invoiceLine.UniversalDutyRate }, Enumerable.Empty<ZPropertyInfo>(), RateCalcUnitOfMeasureAggregator.IsConvertableFrom);
		}

		protected override void SetTariffEtcDataFromProductsPivotCore(BaseCusClassPartPivot pivot)
		{
			base.SetTariffEtcDataFromProductsPivotCore(pivot);
			var euPivot = (CusClassPartPivot)pivot;

			var tariffNum = euPivot.CI_TariffNum;
			if (!tariffNum.IsEmpty)
			{
				JI_Tariff = UniversalTariffType == CusTariffTypes.ExportTariff
					? EU.Business.TariffFormatter.GetExportTariffNumber(tariffNum)
					: tariffNum;
			}

			var countryOfOrigin = euPivot.CI_RN_NKCountryOfOrigin;
			if (!countryOfOrigin.IsEmpty)
			{
				JI_CountryOfOrigin = countryOfOrigin;
			}

			if (!euPivot.IsClassificationBoth)
			{
				var cpc = euPivot.CI_CPC;
				if (!cpc.IsEmpty)
				{
					JI_Procedure = cpc;
				}

				var procedureCodesAsString = euPivot.AdditionalProcedureCodesAsString;
				if (!procedureCodesAsString.IsEmpty && IsAdditionalProcedureCodesApplicable)
				{
					AdditionalProcedureCodes.AsString = procedureCodesAsString;
				}

				var supplement1 = euPivot.CI_Supplement1;
				if (!supplement1.IsEmpty)
				{
					JI_SupplementaryCode1 = supplement1;
				}

				var supplement2 = euPivot.CI_Supplement2;
				if (!supplement2.IsEmpty)
				{
					JI_SupplementaryCode2 = supplement2;
				}

				if (euPivot.AdditionalSupplementaryCodes.Count > 0)
				{
					AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					euPivot.AdditionalSupplementaryCodes.CloneElementsTo(AdditionalSupplementaryCodes);
				}

				var thirdQty = euPivot.CI_ThirdQty;
				if (!thirdQty.IsEmpty)
				{
					JI_CustomsThirdQuantity = thirdQty;
				}

				var preferenceCode = euPivot.PreferenceCode;
				if (!preferenceCode.IsEmpty)
				{
					JI_PrimaryPreference = preferenceCode;
				}

				var concessionOrder = euPivot.CI_ConcessionOrder;
				if (!concessionOrder.IsEmpty)
				{
					JI_ConcessionOrder = concessionOrder;
				}

				SetBox44DocsFromOrgSupplierPart(SupportingDocuments, euPivot.SupportingDocuments);
				SetBox44DocsFromOrgSupplierPart(PreviousDocuments, euPivot.PreviousDocuments);
				SetBox47TaxFromOrgSupplierPart(Taxes, euPivot.Taxes);
				if (Declaration?.Configuration.UCCAdditionalInfosSupport(Declaration) ?? false)
				{
					SetBox44DocsFromOrgSupplierPart(AdditionalInfos, euPivot.AdditionalInfos);
				}
				else
				{
					SetBox44DocsFromOrgSupplierPart(AdditionalInfos, euPivot.AdditionalInfos, AdditionalInfoSubTypeList.Codes.TransportDocument, AdditionalInfoSubTypeList.Codes.AdditionalReference);
				}
			}
		}

		protected override ZString GetPartPivotTypeCore()
		{
			var result = ClassificationType.Both;
			if (IsImport)
			{
				result = ClassificationType.IMP;
			}
			else if (IsExport)
			{
				result = ClassificationType.EXP;
			}
			return result;
		}

		void SetBox44DocsFromOrgSupplierPart<T>(CusSupportingInfoCollection<T> targetCusAddInfos, CusSupportingInfoCollection<T> sourceCusSupportingInfos, params ZString[] excludeSubTypes)
			where T : ImportExportAwareSupportingInfo
		{
			foreach (T src in sourceCusSupportingInfos)
			{
				if (!excludeSubTypes.Contains(src.CSI_SubType))
				{
					var maybeExisting = (from T candidate in targetCusAddInfos where candidate.KeyToDeterimeUniqueness == src.KeyToDeterimeUniqueness select candidate).FirstOrDefault();
					if (maybeExisting == null)
					{
						targetCusAddInfos.Add(src.Clone());
					}
					else
					{
						var args = new BusinessObjectCloneArgs(new[] { CusSupportingInfoSchema.Constants.CSI_Type, CusSupportingInfoSchema.Constants.CSI_ParentID, CusSupportingInfoSchema.Constants.CSI_ParentTableCode });
						maybeExisting.CopyPersistentValuesFrom(src, args);
						maybeExisting.RefreshBinding();
					}
				}
			}
		}

		void SetBox47TaxFromOrgSupplierPart(JobComInvoiceLineTaxCollection targetTaxes, CusAddInfoCollection<Tax_CusAddInfoOnlyForPIVOT> sourceCusAddInfoTaxes)
		{
			foreach (CusAddInfo<Tax_CusAddInfoOnlyForPIVOT> src in sourceCusAddInfoTaxes)
			{
				var maybeExisting = (from JobComInvoiceLineTax candidate in targetTaxes where candidate.JLT_Type == src.Data.G4_Type select candidate).FirstOrDefault()
					?? targetTaxes.AddNew();
				foreach (ZPropertyInfo sourceZPI in src.Data.ZPropertyInfoHash)
				{
					if (!sourceZPI.Value.IsEmpty && maybeExisting.ZPropertyInfoHash.ContainsKey(sourceZPI.Name))
					{
						maybeExisting[sourceZPI.Name] = sourceZPI.Value;
					}
				}
				maybeExisting.RefreshBinding();
			}
		}

		protected override Customs.Business.ClassificationDetailsUpdater GetNewClassificationDetailsUpdater() => new ClassificationDetailsUpdater(this);

		protected override Customs.Business.ProcedureRegimeDecider GetNewProcedureRegimeDecider() => new ProcedureRegimeDecider();

		protected override void SynchroniseFromForwardingOrderLineCore(Freight.Forwarding.Orders.Business.OrderLine specificOrderLine)
		{
			base.SynchroniseFromForwardingOrderLineCore(specificOrderLine);
			new InvoiceLineFromOrderLineSynchroniser().UpdateInvoiceLineFromOrderLine(specificOrderLine, this);
		}

		protected override ZString DutyAmountsAsStringCore => CusEntryLine == null ? base.DutyAmountsAsStringCore : GetDutyLinesAndFormatWithThisRunner(CusEntryLine.Fees, fee => fee.CF_ChargeType + ":" + fee.CF_ChargeAmount.ToString("#.00", CultureInfo.CurrentCulture));

		internal static ZString GetDutyLinesAndFormatWithThisRunner(ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> fees, FormatOneDutyLineRunnerFee runner)
		{
			var groupedFees = fees.Cast<CusEntryLineFee>().GroupBy(x => x.CF_ChargeType).Where(group => group.Count() == 1).Select(group => new
			{
				ChargeType = group.Key,
				Fees = group.ToList()
			});

			var feeLines = new List<ZString>();
			foreach (var fee in (from CusEntryLineFee f in groupedFees.SelectMany(x => x.Fees) where f.CF_ChargeType != UniversalReferenceConstants.RefCusRateCodes.Vat select f))
			{
				feeLines.Add(runner(fee));
			}
			var array = feeLines.ToArray();
			Array.Sort(array);
			return ZString.Join(System.Environment.NewLine, array);
		}

		internal delegate ZString FormatOneDutyLineRunnerFee(CusEntryLineFee fee);

		public void SetSupervisingOfficeFromDeclarant()
		{
			JI_OA_SupervisingOffice = Declaration.GetSpoffFromDeclarant();
		}

		public ZString LanguageForTariffDescription => LanguageForTariffDescriptionCore();
		protected virtual ZString LanguageForTariffDescriptionCore() => ZString.Empty;

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			if (Declaration != null)
			{
				IJobComInvoiceLineValueCalculator calculator = Declaration.GetJobComInvoiceLineCalculator(this);
				if (calculator != null)
				{
					return calculator.GetTariffDescription(tariffCode);
				}
			}
			return ZString.Empty;
		}

		protected override bool IsContainerisedModeCore(string containerMode)
		{
			return base.IsContainerisedModeCore(containerMode) || containerMode == Core.Constants.ContainerModes.ULD;
		}

		[MaxLength(16)]
		[ResourceStringData("9D2642D6-BA5C-4CB1-8D70-93CE7DEE3454", Caption = "[33] Tariff", ShortCaption = "Tariff")]
		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					base.JI_Tariff = value;
				}
			}
		}

		public override ZString CustomsUQ => UniversalTariff?.GetSpecificUOM(Constants.UnitOfMeasureTypes.StatisticalUOMType) ?? ZString.Empty;

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			return UseUniversalTariff
							&& UniversalTariff != null
							&& UniversalTariff.UnitsOfMeasure
									.Where(uom => uom.ZZ8_Type == Constants.UnitOfMeasureTypes.StatisticalUOMType || uom.ZZ8_Type == Constants.UnitOfMeasureTypes.AdditionalUOMType || uom.ZZ8_Type == Constants.UnitOfMeasureTypes.CustomsUOM3Type)
									.Any(uom => uom.CusTradeGroup?.TradeGroupCountries.Any(country => country.ZZB_RN_NKTradeGroupCountryCode == JI_CountryOfOrigin) ?? true);
		}

		public override bool CanConvertFromNetWeightToCustomsUnit(ZString customsUnit) => JI_NetWeight > ZDecimal.Zero && Core.Constants.Weight.ContainsCode(JI_NetWeightUQ) && Core.Constants.Weight.ContainsCode(CustomsQuantityConverter.GetEffectiveWeightUnit(customsUnit));

		public override void CalculateFromNetWeightToCustomsQty()
		{
			if (!IsCalculateFromNetWeightToCustomsQtySuspended)
			{
				base.CalculateFromNetWeightToCustomsQty();
			}
		}

		bool IsCalculateFromNetWeightToCustomsQtySuspended => calculateFromNetWeightToCustomsQtySuspenderIndex > 0;
		int calculateFromNetWeightToCustomsQtySuspenderIndex;

		public IDisposable SuspendCalculateFromNetWeightToCustomsQty()
		{
			return new DisposableAction(
				createAction: () => calculateFromNetWeightToCustomsQtySuspenderIndex++,
				disposeAction: () => calculateFromNetWeightToCustomsQtySuspenderIndex--);
		}

		protected override ZWeight GetCustomsWeight() => new ZWeight(JI_CustomsQuantity, CustomsQuantityConverter.GetEffectiveWeightUnit(JI_CustomsUnitQty));

		public override ZGuid JI_CL
		{
			get => base.JI_CL;
			set
			{
				var oldValue = JI_CL;
				base.JI_CL = value;
				if (!IsCopying && oldValue != JI_CL)
				{
					Declaration?.MarkFeesAsNeedingValidation();
				}
			}
		}

		public override ZGuid JI_JZ
		{
			get => base.JI_JZ;
			set
			{
				var oldValue = JI_JZ;
				base.JI_JZ = value;
				if (!IsCopying && oldValue != JI_JZ)
				{
					Declaration?.MarkFeesAsNeedingValidation();
				}
			}
		}

		protected virtual bool SetSecondQuantityFromEdiTariffsOwnRecord => true;

		public ZString JI_Calc_RequestedProcedure => JI_Procedure.Left(2).TrimEnd();

		protected override ZString GetProcedureCodeCore() => JI_Procedure;  // Full 7 chars

		protected override bool AllowMyBrotherInvoiceLinesToBeOfMixedCategoriesCore => false;

		protected override (GetValueDelegate<RefCusProcedure> GetCusProcedureFunc, Func<string> GetKeyFunc) GetCusProcedureCore(ZString procedureCode)
		{
			string KeyFunc() =>
				$"EUCusProcedure_{GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure)}_{Declaration?.JE_MessageType}_{procedureCode}_{JI_Calc_PreviousProcedure}";

			RefCusProcedure ProcedureFunc()
			{
				var declaration = Declaration;
				if (declaration != null)
				{
					var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, declaration.JE_MessageType);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode.Left(2));
					query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, JI_Calc_PreviousProcedure);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, procedureCode.PadRight(7).Right(3));
					return Factory.LoadTop1<RefCusProcedure>(query);
				}

				return null;
			}

			return (ProcedureFunc, KeyFunc);
		}

		void SetPackagePivotToInvoiceQuantityIfNoPivotExists()
		{
			var (shouldCreateCusPackagePivot, package) = ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageType();

			if (shouldCreateCusPackagePivot && JI_InvoiceQuantity <= Int32.MaxValue)
			{
				var chc = ToggleLinkageWithPackage(package, true);
				chc.NumberOfPacks = JI_InvoiceQuantity.ToZInt();
			}
		}

		public (ZBool shouldCreate, BasePackage package) ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageType() => ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageTypeCore();

		protected virtual (ZBool shouldCreate, BasePackage package) ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageTypeCore()
		{
			(ZBool shouldCreate, BasePackage package) result = (ZBool.False, null);
			if (!JI_InvoiceQuantity.IsEmpty && !JI_InvoiceUQ.IsEmpty)
			{
				var converter = new InvoiceLineFromOrderLineSynchroniser();
				var mappedCodeTuple = converter.GetTwoCharacterUnitType(JI_InvoiceUQ);
				var packageRowsOfMatchingType = Declaration?.Packages.OfType<BasePackage>().Where(cw => mappedCodeTuple.IsExactMatch && cw.CW_PackType == mappedCodeTuple.MappedCode);
				if (packageRowsOfMatchingType?.Take(2).Count() == 1)
				{
					result = (ZBool.True, packageRowsOfMatchingType.First());
				}
			}

			return result;
		}

		public override ZDecimal JI_InvoiceQuantity
		{
			get => base.JI_InvoiceQuantity;
			set
			{
				base.JI_InvoiceQuantity = value;
				SetPackagePivotToInvoiceQuantityIfNoPivotExists();
			}
		}

		public override ZString JI_InvoiceUQ
		{
			get => base.JI_InvoiceUQ;
			set
			{
				base.JI_InvoiceUQ = value;
				SetPackagePivotToInvoiceQuantityIfNoPivotExists();
			}
		}

		/// <summary>
		/// Customs Procedure Code (CPC, Box 37).
		/// </summary>
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CPCList))]
		[ResourceStringData("0ab5762f-b1d1-4832-9e8a-df48c336cc68", Caption = "[37] Procedure Code", MediumCaption = "[37] CPC", ShortCaption = "CPC")]
		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				var hasChanged = value != JI_Procedure;
				base.JI_Procedure = value;
				MarkAsNeedingValidation();
				fSellerDocAddress?.MarkAsNeedingValidation();
				fBuyerDocAddress?.MarkAsNeedingValidation();
				var declaration = Declaration;

				if (declaration != null && EntryInstruction is CusEntryInstruction entryInstruction)
				{
					if (IsIntoOrOutOfRegimeProcedure)
					{
						if (declaration.WarehouseDocAddress is JobDocAddress warehouseDocAddress && warehouseDocAddress.E2_OA_Address.IsValid)
						{
							entryInstruction.CEI_OA_Warehouse = warehouseDocAddress.E2_OA_Address;
						}
					}
					else if (!declaration.InvoiceLines.OfType<JobComInvoiceLine>().Except(new[] { this }).Any(i => i.IsIntoOrOutOfRegimeProcedure))
					{
						entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
					}
				}

				if (!IsCopying)
				{
					if (declaration != null)
					{
						declaration.MarkAsNeedingValidation();  // Because JobDeclaration.IsFSD looks at JobComInvoiceLine.JI_Procedure. If they touch, then they must have linked validations.
					}

					if (hasChanged)
					{
						OnCusProcedureChanged();
					}
				}
			}
		}

		[ResourceStringData("a36e4d7c-a322-4231-afa0-326b79f3aab1", Caption = "[38] Customs Qty")]
		[ResourceStringData("9e83de0d-8a2b-4336-aa44-1de0ea03c82c", Caption = "Net Weight in KG", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZDecimal JI_CustomsQuantity
		{
			get => base.JI_CustomsQuantity;
			set => base.JI_CustomsQuantity = value;
		}

		[ResourceStringData("7833ff64-9bdd-463a-9baf-74470df8ad9b", Caption = "[34] Goods Origin", MediumCaption = "[34] Origin", ShortCaption = "Origin", FullDescription = "Country/Region of Origin of the goods being moved.")]
		[ResourceStringData("89C9D3B8-B800-4D34-BD4B-D110BFE0134B", Caption = "Country/Region of Origin", MediumCaption = "Ctry./Rgn. of Origin", ShortCaption = "Ctry./Rgn. of Orig.", FullDescription = "Country/Region of Origin of the goods being moved.", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EB3D2088-95CD-45BF-85D3-F46B8DBC8E55", Caption = "Country of Origin", ShortCaption = "Origin", FullDescription = "[16 08 001 000] Country of origin", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString JI_CountryOfOrigin
		{
			get => base.JI_CountryOfOrigin;
			set => base.JI_CountryOfOrigin = value;
		}

		[ResourceStringData("CC1459DE-0618-467C-BCF7-F9205FD37AB4", Caption = "Country/Region of Export", MediumCaption = "Ctry./Rgn. of Export", ShortCaption = "Ctry./Rgn. of Exp.", FullDescription = "Country/Region of Export of the goods being moved.", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString JI_RN_NKCountryOfExport { get => base.JI_RN_NKCountryOfExport; set => base.JI_RN_NKCountryOfExport = value; }

		[ResourceStringData("4e061fc9-c9b7-465e-863d-eb924c57b47f", Caption = "[42] Price", FullDescription = "Line price for line item.")]
		public override ZDecimal JI_LinePrice
		{
			get => base.JI_LinePrice;
			set => base.JI_LinePrice = value;
		}

		[ResourceStringData("f96e4fcb-3a28-4a5f-851e-f65ff26ec3ca", Caption = "[41] Supp. Qty")]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get => base.JI_CustomsSecondQuantity;
			set => base.JI_CustomsSecondQuantity = value;
		}

		[ResourceStringData("496528d5-568c-4962-91ca-7bda3ce88769", Caption = "[44] Third Qty", MediumCaption = "Third Qty", ShortCaption = "3rd Qty")]
		public override ZDecimal JI_CustomsThirdQuantity
		{
			get => base.JI_CustomsThirdQuantity;
			set => base.JI_CustomsThirdQuantity = value;
		}

		[ResourceStringData("ea723a05-e9db-456a-a9b4-5d789310bc1e", Caption = "[35] Gross Weight", MediumCaption = "[35] GWT", ShortCaption = "GWT")]
		public override ZDecimal JI_Weight
		{
			get => base.JI_Weight;
			set
			{
				var hasChanged = value != JI_Weight;
				base.JI_Weight = value;
				if (!IsCopying && hasChanged)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JI_WeightUQ
		{
			get => base.JI_WeightUQ;
			set
			{
				var hasChanged = value != JI_WeightUQ;
				base.JI_WeightUQ = value;
				if (!IsCopying && hasChanged)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(9)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CPCList))]
		[ResourceStringData("3F8E2EC5-28EB-4145-B8DB-9F99C0BAF6ED", Caption = "[37] Procedure Code", MediumCaption = "[37] CPC", ShortCaption = "CPC")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine|EXPUCC6|JI_FormattedProcedure", Caption = "Procedure", FullDescription = "[11 09 000 000] Procedure", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public virtual ZString JI_FormattedProcedure
		{
			get => JI_Procedure;
			set => JI_Procedure = value;
		}

		public ZPropertyInfo JI_FormattedProcedureInfo => GetWrappedZPropertyInfo(Schema.JI_FormattedProcedure, x => JI_ProcedureInfo);

		public virtual string ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage => Declaration?.IsUCC6AndIsExport ?? false ? Res.GetString("{83D2BA89-98CC-4FD2-BD72-6A667CE99CCF}", "To select additional procedure codes the procedure/CPC must be filled in.") : Res.GetString("C7775945-C3AD-4983-B019-EA40F5C624B9", "To select additional procedure codes (Box 37.2) the CPC must be filled in");

		public ZPropertyInfo EntryReferenceNumberInfo => GetZPropertyInfo(Schema.EntryReferenceNumber);

		public ZString EntryReferenceNumber => EntryReferenceNumberCore;

		protected virtual ZString EntryReferenceNumberCore => CusEntryLine?.Header?.CH_BGMReference ?? Res.GetString("0E6D1E53-61EF-4B7C-83F5-B6675A98A125", "Not Merged");

		[ResourceStringData("764612F6-02FD-4D4F-B3C4-310AE58C1269", Caption = "Entry Instr. Desc.", FullDescription = "Entry Instruction Description")]
		public ZString EntryInstructionDescription => EntryInstruction?.CEI_Description ?? ZString.Empty;

		public override bool ShouldWipeNKTaxType => AdditionalProcedureWithCalculateVATIsFalse != null || base.ShouldWipeNKTaxType;

		public AdditionalProcedureCode AdditionalProcedureWithCalculateVATIsFalse => AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().FirstOrDefault(x => (!Lookups.CPCList.FirstOrDefault(y => y.FullCodeCurrentPlusPreviousPlusConcession == x.CY_Code)?.ZZ6_CalculateVAT) ?? false);

		public override ZString ProcedureIndicatesVATNotApply => (!CusProcedure?.ZZ6_CalculateVAT ?? false) ? JI_Procedure : (AdditionalProcedureWithCalculateVATIsFalse?.CY_Code ?? ZString.Empty);

		public virtual ZInt MaxNumberOfAdditionalProcedureCode => 0;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public AdditionalProcedureCodeCollection AdditionalProcedureCodes
		{
			get
			{
				if (additionalProcedureCodes == null)
				{
					additionalProcedureCodes = GetAdditionalProcedureCodeCollection();
					RegisterEditableChildObject(additionalProcedureCodes);
					additionalProcedureCodes.Load();
					additionalProcedureCodes.CountChanged += AdditionalProcedureCodes_CountChanged;
					AdditionalProcedureCodesAsStringInfo.RefreshBinding();
				}

				return additionalProcedureCodes;
			}
		}
		AdditionalProcedureCodeCollection additionalProcedureCodes;

		protected virtual AdditionalProcedureCodeCollection GetAdditionalProcedureCodeCollection() => new AdditionalProcedureCodeCollection(this);

		void AdditionalProcedureCodes_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateAdditionalProcedureCodesAsString();
			}
			AdditionalProcedureCodesAsStringInfo.RefreshBinding();
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine|AdditionalProcedureCodesAsString", Caption = "Additional Procedure Codes", MediumCaption = "Add. Procedure Codes", ShortCaption = "Add. CPCs")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine|EXPUCC6|AdditionalProcedureCodesAsString", Caption = "Additional Procedures", MediumCaption = "Add. Procedures", FullDescription = "[11 10 000 000] Additional Procedures", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public virtual ZString AdditionalProcedureCodesAsString => AdditionalProcedureCodes.AsString;

		public ZPropertyInfo AdditionalProcedureCodesAsStringInfo => GetZPropertyInfo(nameof(AdditionalProcedureCodesAsString));

		public ZBool IsAdditionalProcedureCodesApplicable => MaxNumberOfAdditionalProcedureCode > 0;

		[ChildEditable(true)]
		public ICusFiscalReferenceCollection<CusFiscalReference> FiscalReferences
		{
			get
			{
				if (cusFiscalReferences == null)
				{
					cusFiscalReferences = GetNewFiscalReferenceCollection();
					RegisterEditableChildObject(cusFiscalReferences);
					cusFiscalReferences.Load();
				}
				return cusFiscalReferences;
			}
		}
		ICusFiscalReferenceCollection<CusFiscalReference> cusFiscalReferences;

		protected virtual ICusFiscalReferenceCollection<CusFiscalReference> GetNewFiscalReferenceCollection() => new CusFiscalReferenceCollection<CusFiscalReference>(this);

		ICusFiscalReferenceValidationDecider ICusFiscalReferenceProviderWithValidationDecider.ValidationDecider => Factory.GetValue(ref cusFiscalReferenceValidationDeciderCached,
			() => InvoiceHeader?.JobDeclaration?.Configuration?.InvoiceLineConfiguration?.GetCusFiscalReferenceValidationDecider(this));
		CachedProperty<ICusFiscalReferenceValidationDecider> cusFiscalReferenceValidationDeciderCached;

		ICusAuthorizationUsageValidationDecider ICusAuthorizationUsageProviderWithValidationDecider.ValidationDecider => Factory.GetValue(ref cusAuthorizationUsageValidationDeciderCached,
			() => InvoiceHeader?.JobDeclaration?.Configuration?.InvoiceLineConfiguration?.GetCusAuthorizationUsageValidationDecider(this));
		CachedProperty<ICusAuthorizationUsageValidationDecider> cusAuthorizationUsageValidationDeciderCached;

		[ChildEditable]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences
		{
			get
			{
				if (cusSupplyChainActorReferences == null)
				{
					cusSupplyChainActorReferences = GetNewCusSupplyChainActorReferenceCollection();
					RegisterEditableChildObject(cusSupplyChainActorReferences);
					cusSupplyChainActorReferences.Load();
				}

				return cusSupplyChainActorReferences;
			}
		}
		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> cusSupplyChainActorReferences;

		protected virtual ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> GetNewCusSupplyChainActorReferenceCollection() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnlyMember(nameof(JI_CustomsThirdUnitQty_ReadOnly))]
		public override ZString JI_CustomsThirdUnitQty
		{
			get => base.JI_CustomsThirdUnitQty;
			set => base.JI_CustomsThirdUnitQty = value;
		}

		protected virtual ZBool JI_CustomsThirdUnitQty_ReadOnly => false;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnlyMember(nameof(JI_CustomsFourthUnitQty_ReadOnly))]
		public override ZString JI_CustomsFourthUnitQty
		{
			get => base.JI_CustomsFourthUnitQty;
			set => base.JI_CustomsFourthUnitQty = value;
		}

		protected virtual ZBool JI_CustomsFourthUnitQty_ReadOnly => false;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnlyMember(nameof(JI_CustomsFifthUnitQty_ReadOnly))]
		public override ZString JI_CustomsFifthUnitQty
		{
			get => base.JI_CustomsFifthUnitQty;
			set => base.JI_CustomsFifthUnitQty = value;
		}

		protected virtual ZBool JI_CustomsFifthUnitQty_ReadOnly => false;

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			return JI_CustomsUnitQty.IsEmpty;
		}

		#region Exposed AddInfo Fields

		/// <summary>
		/// Customs Preference Code (Box 36, codes dictated by Customs for Imports only).
		/// </summary>
		[MaxLength("MaxLengthOfPrimaryPreference")]
		[ResourceStringData("a8009eba-77e4-42e2-ab91-22baf6060c81", Caption = "Preference", ShortCaption = "Pref. Code")]
		[ResourceStringData("EA637077-7B6C-4982-8D14-0E3EC4D63866", Caption = "Preference", FullDescription = "[14 11 001 000] Preference", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString JI_PrimaryPreference
		{
			get => base.JI_PrimaryPreference;
			set
			{
				var hasChanges = base.JI_PrimaryPreference != value;
				base.JI_PrimaryPreference = value;
				if (hasChanges)
				{
					Validation.ValidateJI_CountryOfOrigin();
					AddInfoValidation.ValidateZG_CountryOfSupply();
				}
			}
		}

		protected virtual int MaxLengthOfPrimaryPreference => 3;

		ZString ITaxAndDocsProvider.CountryCode => InvoiceHeader?.JobDeclaration?.Country?.Code ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		ZString ITaxAndDocsProvider.PreferenceCode
		{
			set => JI_PrimaryPreference = value;
		}

		ZString ITaxAndDocsProvider.SupplementaryCode
		{
			set => JI_SupplementaryCode1 = value;
		}

		ZString ITaxAndDocsProvider.Tariff => JI_Tariff;

		ZString ITaxAndDocsProvider.CountryOfOriginCode => JI_CountryOfOrigin;

		BusinessObjectFactory ITaxAndDocsProvider.Factory => Factory;

		#region SupplementaryQty and UnitQty (Box41)

		/// <summary>
		/// Customs Supplementary Qty (Box 41)
		/// </summary>
		[List(nameof(Lookups) + "+" + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnlyMember(nameof(JI_CustomsSecondUnitQty_ReadOnly))]
		public override ZString JI_CustomsSecondUnitQty
		{
			get => base.JI_CustomsSecondUnitQty;
			set => base.JI_CustomsSecondUnitQty = value;
		}

		protected virtual ZBool JI_CustomsSecondUnitQty_ReadOnly => false;

		#endregion

		#region ZG Properties

		[ResourceStringData("EU.JobComInvoiceLine.ZG_CountryOfDestination", Caption = "[UCC 5/8] Destination", ShortCaption = "[UCC 5/8] Dest.")]
		[ResourceStringData("4681CA53-0A7D-4CBD-BD2C-F281D01D3F20", Caption = "Country of Destination", MediumCaption = "Destination Country", ShortCaption = "Destination", FullDescription = "Country of Destination of the goods being moved.", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[UniversalCopyAddInfoPropertyMapping(AutoEUAddInfo.Schema.ZG_CountryOfDestination)]
		public override ZString ZG_CountryOfDestination
		{
			get => base.ZG_CountryOfDestination;
			set => base.ZG_CountryOfDestination = value;
		}

		[ResourceStringData("201aa4a9-0242-44ec-9a55-5b47328e923c", Caption = "Country of Supply")]
		[ResourceStringData("45F66710-79CC-41A2-80B4-E01605C19DC5", Caption = "Pref. Origin", ShortCaption = "Pref. Ctry.", FullDescription = "[16 09 001 000] Country of preferential origin", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString ZG_CountryOfSupply
		{
			get => base.ZG_CountryOfSupply;
			set
			{
				base.ZG_CountryOfSupply = value;

				var instruction = EntryInstruction;
				if (instruction != null)
				{
					instruction.Validation.ValidateInvoiceLinesCountryOfSupply();
				}
			}
		}

		/// <summary>
		/// A place for the user to set his own overridden stat value, if the tickbox allows.  Without the tickbox, just returns the calculated stat value.
		/// </summary>
		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ZG_StatisticalValueIsReadOnly))]
		public override ZDecimal ZG_StatisticalValue
		{
			get => ZG_StatisticalValueManualOverride ? base.ZG_StatisticalValue : JI_Calc_StatisticalValue;
			set => base.ZG_StatisticalValue = value;
		}
		protected bool ZG_StatisticalValueIsReadOnly => !ZG_StatisticalValueManualOverride;

		[ResourceStringData("{25D9CF53-5DB6-4132-8DAB-F6892280A703}", Caption = "Manual Ovr.")]
		public override ZBool ZG_StatisticalValueManualOverride
		{
			get => base.ZG_StatisticalValueManualOverride;
			set
			{
				var oldValue = base.ZG_StatisticalValueManualOverride;
				base.ZG_StatisticalValueManualOverride = value;
				if (value != oldValue)
				{
					ZG_StatisticalValue = value ? JI_Calc_StatisticalValue : ZDecimal.Zero;
				}
			}
		}

		[ResourceStringData("887ae04f-1948-4734-9193-c76b188fd497", Caption = "Restrictions as to the disposal or use of the goods by the buyer in accordance with Article 70(3)(a) of the Code")]
		public override ZString ZG_RelatedIndicator2
		{
			get => base.ZG_RelatedIndicator2;
			set
			{
				base.ZG_RelatedIndicator2 = value;
				InvoiceHeader?.ZG_RelatedIndicator2Info.RefreshBinding();
			}
		}

		[ResourceStringData("887ae04f-1948-4734-9193-c76b188fd497", Caption = "Restrictions as to the disposal or use of the goods by the buyer in accordance with Article 70(3)(a) of the Code")]
		public virtual ZBool RelatedIndicator2
		{
			get => ZG_RelatedIndicator2.EqualsIgnoringCase(ValuationIndicatorCodeList.Codes.Yes);
			set => ZG_RelatedIndicator2 = value ? ValuationIndicatorCodeList.Codes.Yes : ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		}
		public ZPropertyInfo RelatedIndicator2Info => GetWrappedZPropertyInfo(Schema.RelatedIndicator2, x => ZG_RelatedIndicator2Info);
		public bool RelatedIndicator2_ReadOnly => !RelatedIndicator2 && (InvoiceHeader?.RelatedIndicator2 ?? false);

		[ResourceStringData("3a948477-28d5-4e4b-aef5-5dd113640148", Caption = "Sale or price is subject to some condition or consideration in accordance with Article 70(3)(b) of the Code")]
		public override ZString ZG_RelatedIndicator3
		{
			get => base.ZG_RelatedIndicator3;
			set
			{
				base.ZG_RelatedIndicator3 = value;
				InvoiceHeader?.ZG_RelatedIndicator3Info.RefreshBinding();
			}
		}

		[ResourceStringData("3a948477-28d5-4e4b-aef5-5dd113640148", Caption = "Sale or price is subject to some condition or consideration in accordance with Article 70(3)(b) of the Code")]
		public virtual ZBool RelatedIndicator3
		{
			get => ZG_RelatedIndicator3.EqualsIgnoringCase(ValuationIndicatorCodeList.Codes.Yes);
			set => ZG_RelatedIndicator3 = value ? ValuationIndicatorCodeList.Codes.Yes : ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		}
		public ZPropertyInfo RelatedIndicator3Info => GetWrappedZPropertyInfo(Schema.RelatedIndicator3, x => ZG_RelatedIndicator3Info);
		public bool RelatedIndicator3_ReadOnly => !RelatedIndicator3 && (InvoiceHeader?.RelatedIndicator3 ?? false);

		[ResourceStringData("bf366b8d-732b-4924-876e-0a8b22197a76", Caption = "The sale is subject to an arrangement under which part of the proceeds of any subsequent resale, disposal or use accrues directly or indirectly to the seller")]
		public override ZString ZG_RelatedIndicator4
		{
			get => base.ZG_RelatedIndicator4;
			set
			{
				base.ZG_RelatedIndicator4 = value;
				InvoiceHeader?.ZG_RelatedIndicator4Info.RefreshBinding();
			}
		}

		[ResourceStringData("bf366b8d-732b-4924-876e-0a8b22197a76", Caption = "The sale is subject to an arrangement under which part of the proceeds of any subsequent resale, disposal or use accrues directly or indirectly to the seller")]
		public virtual ZBool RelatedIndicator4
		{
			get => ZG_RelatedIndicator4.EqualsIgnoringCase(ValuationIndicatorCodeList.Codes.Yes);
			set => ZG_RelatedIndicator4 = value ? ValuationIndicatorCodeList.Codes.Yes : ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		}
		public ZPropertyInfo RelatedIndicator4Info => GetWrappedZPropertyInfo(Schema.RelatedIndicator4, x => ZG_RelatedIndicator4Info);
		public bool RelatedIndicator4_ReadOnly => !RelatedIndicator4 && (InvoiceHeader?.RelatedIndicator4 ?? false);

		[ResourceStringData("9A4EE47B-192F-4877-B5A4-7751A1B12BDA", Caption = "CUS Code")]
		public override ZString ZG_CusNumber
		{
			get => base.ZG_CusNumber;
			set => base.ZG_CusNumber = value;
		}

		[ResourceStringData("EU.JobComInvoiceLine.ZG_CountryOfDispatch", Caption = "[UCC 5/8] Dispatch")]
		[UniversalCopyAddInfoPropertyMapping(AutoEUAddInfo.Schema.ZG_CountryOfDispatch)]
		public override ZString ZG_CountryOfDispatch
		{
			get => base.ZG_CountryOfDispatch;
			set => base.ZG_CountryOfDispatch = value;
		}

		#endregion

		public void ClearFecDSTChallenge()
		{
			// The hidden fields used to store info about received challenges
			ZG_FecChallengeDST = false;
		}

		public void ClearFecDSTTickbox()
		{
			// The fields shown to the user and ticked when they confirm a fec
			ZG_FecDST = false;
		}

		public void ClearFecDSTChallengeAndTickbox()
		{
			ClearFecDSTChallenge();
			ClearFecDSTTickbox();
		}

		#region Supervising Office

		public ZGuid JI_OA_SupervisingOffice
		{
			get => SupervisingOfficeDocAddress.E2_OA_Address;
			set => SupervisingOfficeDocAddress.E2_OA_Address = value;
		}

		public OrgAddress SupervisingOffice => Factory.Load<OrgAddress>(JI_OA_SupervisingOffice);

		public JobDocAddress SupervisingOfficeDocAddress
		{
			get
			{
				if (fSupervisingOfficeDocAddress == null || fSupervisingOfficeDocAddress.IsDeleted)
				{
					if (fSupervisingOfficeDocAddress != null)
					{
						foreach (ZPropertyInfo propertyInfo in fSupervisingOfficeDocAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= new EventHandler(SupervisingOfficeDocAddress_ValueChanged);
						}
					}

					fSupervisingOfficeDocAddress = ((IDocAddresses)this).DocAddresses.FindOrCreateWithRequirement(SupervisingOfficeDocAddressRequirement);

					foreach (ZPropertyInfo propertyInfo in fSupervisingOfficeDocAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += new EventHandler(SupervisingOfficeDocAddress_ValueChanged);
					}
				}
				return fSupervisingOfficeDocAddress;
			}
		}
		JobDocAddress fSupervisingOfficeDocAddress;

		protected virtual void SupervisingOfficeDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddressRequirement SupervisingOfficeDocAddressRequirement
		{
			get
			{
				if (fSupervisingOfficeDocAddressRequirement == null)
				{
					fSupervisingOfficeDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CustomsSupervisingOffice, ContactType.Administration);
					DocAddressManager.AddRequirement(fSupervisingOfficeDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fSupervisingOfficeDocAddressRequirement, DocAddressType.CustomsSupervisingOffice);
				return fSupervisingOfficeDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fSupervisingOfficeDocAddressRequirement;

		protected virtual void DecorateDocAddressRequirement(JobDocAddressRequirement requirement, DocAddressType addressType)
		{
		}

		#region IDocAddresses Implementation

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Environment.Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => SupervisingOfficeDocAddressRequirement;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => new OrgHeaderCollection(Factory);

		[ChildEditable(true)]
		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[] { DocAddressType.CustomsSupervisingOffice };

		ZString IDocAddresses.HumanReadableName => HumanReadableNameCore;

		#endregion

		#region DocAddressManager

		JobDocAddressManager DocAddressManager => fDocAddressManager ?? (fDocAddressManager = new JobDocAddressManager());
		JobDocAddressManager fDocAddressManager;

		#endregion
		#endregion

		/// <summary>
		/// In INVOICE currency
		/// </summary>
		protected override ZDecimal GetJI_Calc_CIF()
		{
			if (InvoiceHeader is JobComInvoiceHeader invoiceHeader && Declaration is JobDeclaration declaration)
			{
				var customsValueInInvoiceCurrency = CurrencyConverter.ConvertExact(new Money(JI_CustomsValue, invoiceHeader.LocalCurrency), invoiceHeader.Invoice_Currency);
				return ((EuCustomsValuationCalculator)ValuationCalculator).GetEuCifAmount(
					invoiceHeader.Invoice_Currency,
					customsValueInInvoiceCurrency.Amount,
					declaration.IncoTermAndChargeFactory);
			}
			else
			{
				return ZDecimal.Zero;
			}
		}

		/// <summary>
		/// Always in customs/local currency
		/// </summary>
		public override ZDecimal JI_CustomsValue
		{
			get
			{
				if (IsImport)
				{
					// BP: Customs Value = Line Price + Sum of Dutiable Additions – Sum of non - dutiable deductions
					decimal result = 0m;
					if (InvoiceHeader != null)
					{
						result = ItemPriceInLocalCurrency + ValuationCalculator.GetAmountToAddToITOTForDutiable(LocalCurrency);
					}
					return result;
				}
				else
				{
					return JI_Calc_FOB_InLocalCurrency;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Override exists for comment")]
		protected override ZDecimal GetJI_Calc_FOB()
		{
			// DJC asserts that FOB is simply the item price add or deduct some charges to bring us back to point FOB on the INCO chart.  It does not depend on whether a charge is dutiable.
			// Yet base uses item price + dutiable charges = FOB.  This is wrong and BP confirms it. This override is just to add this comment -  do not rely on FOB!
			return base.GetJI_Calc_FOB();
		}

		// BP: Statistical Value = Line Price + Sum of Statistical Additions – Sum of non-Statistical deductions
		public ZDecimal JI_Calc_StatisticalValue
		{
			get
			{
				decimal result = 0m;
				if (InvoiceHeader != null)
				{
					result = ItemPriceInLocalCurrency + ValuationCalculator.GetAmountToAddToITOTForStatistical(LocalCurrency);
				}
				return result;
			}
		}

		public ZDecimal JI_Calc_StatisticalBasisExcludingSTACharge
		{
			get
			{
				decimal result = JI_Calc_StatisticalValue;
				if (result != 0m && Charges != null)
				{
					result -= Charges.Cast<InvoiceLineCharge>().Where(x => x.J7_ChargeType == ChargeTypeList.Codes.StatisticalValue).Sum(x => x.J7_Amount);
				}
				return result;
			}
		}

		protected override ICustomsValuationCalculator GetValuationCalculatorCore() => new EuCustomsValuationCalculator(this);

		[ResourceStringData("f3923782-9687-46b8-9227-5f4e73d7ca9f", ShortCaption = "Quota", MediumCaption = "[39] Quota", Caption = "[39] Quota Order Number", FullDescription = "Quota. Box 39. The Quota Order Number of the quota against which a claim for relief from Customs Duty is to be applied.")]
		public override ZString JI_ConcessionOrder
		{
			get => base.JI_ConcessionOrder;
			set
			{
				var oldValue = JI_ConcessionOrder;
				base.JI_ConcessionOrder = value;
				if (!IsCopying && JI_ConcessionOrder != oldValue)
				{
					if (JI_ConcessionOrder.IsEmpty)
					{
						ZG_SecondQuota = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("EU.JobComInvoiceLine.ZG_TransNature", Caption = "[24] Tran. Nature")]
		public override ZString ZG_TransNature
		{
			get => base.ZG_TransNature;
			set => base.ZG_TransNature = value;
		}

		[ResourceStringData("21060405-5A93-47D4-9CF4-88F9B4CC72D8", Caption = "2nd Quota")]
		[ReadOnlyMember(nameof(ZG_SecondQuota_ReadOnly))]
		public override ZString ZG_SecondQuota
		{
			get => base.ZG_SecondQuota;
			set => base.ZG_SecondQuota = value;
		}

		protected bool ZG_SecondQuota_ReadOnly => IsImport && JI_ConcessionOrder.IsEmpty;

		#region SupplementaryCodes

		#region JI_SupplementaryCode1

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalCodesList))]
		[MaxLength(SupplementaryCode.Schema.CY_CodeMaxLength)]
		[ResourceStringData("CA13E7DA-F883-4F62-B7FC-F2F05E0FCC65", Caption = "Sup. Code 1", FullDescription = "Supplementary Code 1")]
		[ResourceStringData("1C6C2693-8D68-4345-BAB2-B1ACFCBA3F59", ShortCaption = "Add. Code 1", Caption = "Additional Code 1", FullDescription = "[18 09 059 000] TARIC Additional Code 1", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public virtual ZString JI_SupplementaryCode1
		{
			get => (SupplementaryCode1 != null) ? SupplementaryCode1.CY_Code : ZString.Empty;
			set
			{
				SupplementaryCodeHelper.SupplementaryCodeSetter(this, JI_SupplementaryCode1Info, value, SupplementaryCode1, 1);
				ValidateTariffOnSupplementaryCodeChange(value);
			}
		}

		public ZPropertyInfo JI_SupplementaryCode1Info
			=> SupplementaryCode1 != null ? GetWrappedZPropertyInfo(nameof(JI_SupplementaryCode1), x => SupplementaryCode1.CY_CodeInfo) : GetZPropertyInfo(nameof(JI_SupplementaryCode1));

		SupplementaryCode SupplementaryCode1
		{
			get
			{
				if (supplementaryCode1 == null || supplementaryCode1.IsDeleted)
				{
					var loader = new BaseSupplementaryCode.Loader(Factory);
					supplementaryCode1 = loader.Load<SupplementaryCode, JobComInvoiceLine>(this, 1);
					if (supplementaryCode1 != null)
					{
						RegisterEditableChildObject(supplementaryCode1);
					}
				}
				return supplementaryCode1;
			}
		}
		SupplementaryCode supplementaryCode1;

		#endregion

		#region JI_SupplementaryCode2

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalCodesList))]
		[MaxLength(SupplementaryCode.Schema.CY_CodeMaxLength)]
		[ResourceStringData("BC36408C-5E0C-4483-8D1D-F856987B6876", Caption = "Sup. Code 2", FullDescription = "Supplementary Code 2")]
		[ResourceStringData("20357E9E-5691-4D7E-961B-EFF05DB1FC0C", ShortCaption = "Add. Code 2", Caption = "Additional Code 2", FullDescription = "[18 09 059 000] TARIC Additional Code 2", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public virtual ZString JI_SupplementaryCode2
		{
			get => (SupplementaryCode2 != null) ? SupplementaryCode2.CY_Code : ZString.Empty;
			set
			{
				SupplementaryCodeHelper.SupplementaryCodeSetter(this, JI_SupplementaryCode2Info, value, SupplementaryCode2, 2);
				ValidateTariffOnSupplementaryCodeChange(value);
			}
		}

		public ZPropertyInfo JI_SupplementaryCode2Info
			=> SupplementaryCode2 != null ? GetWrappedZPropertyInfo(nameof(JI_SupplementaryCode2), x => SupplementaryCode2.CY_CodeInfo) : GetZPropertyInfo(nameof(JI_SupplementaryCode2));

		SupplementaryCode SupplementaryCode2
		{
			get
			{
				if (supplementaryCode2 == null || supplementaryCode2.IsDeleted)
				{
					var loader = new BaseSupplementaryCode.Loader(Factory);
					supplementaryCode2 = loader.Load<SupplementaryCode, JobComInvoiceLine>(this, 2);
					if (supplementaryCode2 != null)
					{
						RegisterEditableChildObject(supplementaryCode2);
					}
				}
				return supplementaryCode2;
			}
		}
		SupplementaryCode supplementaryCode2;

		void ValidateTariffOnSupplementaryCodeChange(ZString supplementaryCode)
		{
			if (!supplementaryCode.IsEmpty)
			{
				return;
			}

			Validation.ValidateJI_Tariff();
		}

		#endregion

		#region JI_AdditionalSupplements

		[ReadOnlyMember(nameof(JI_AdditionalSupplements_ReadOnly))]

		[ResourceStringData("21060405-5A93-47D4-9CF4-88F9B4CC72D8a5650b75-08f9-46cc-9e57-c1b4d57fc69c", Caption = "Additional Supplementary Codes", MediumCaption = "Additional Sup. Codes", ShortCaption = "Add. Sup. Codes")]
		[ResourceStringData("2B1C35E8-F0CC-47EB-A7F6-5F0D5A55B545", Caption = "Additional Codes", FullDescription = "[18 09 059 000] TARIC Additional Codes", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public virtual ZString JI_AdditionalSupplements => AdditionalSupplementaryCodes.AsString;

		public ZPropertyInfo JI_AdditionalSupplementsInfo => GetZPropertyInfo(nameof(JI_AdditionalSupplements));

		public bool JI_AdditionalSupplements_ReadOnly => false;

		#endregion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NationalAdditionalCodeList))]
		[ResourceStringData("B7E02D3C-4B37-4DB2-AAE0-0BF621965A49", ShortCaption = "Nat. Code 1", MediumCaption = "National Code 1", Caption = "National Additional Code 1")]
		[ResourceStringData("C9C6A0BC-F4A4-4667-83AD-03FCF019874C", ShortCaption = "Nat. Code 1", MediumCaption = "National Code 1", Caption = "National Additional Code 1", FullDescription = "[18 09 060 000] National Additional Code 1", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[MaxLength(4)]
		public virtual ZString JI_NationalAdditionalCode1
		{
			get => NationalAdditionalCode1?.CY_Code ?? ZString.Empty;
			set
			{
				CheckMaximumLength(JI_NationalAdditionalCode1Info, value);
				NationalAdditionalCodeHelper.LoadOrCreate(value, this, 1, JI_NationalAdditionalCode1Info, NationalAdditionalCode1);
			}
		}

		public ZPropertyInfo JI_NationalAdditionalCode1Info => NationalAdditionalCode1 is null ? GetZPropertyInfo(nameof(JI_NationalAdditionalCode1)) : GetWrappedZPropertyInfo(nameof(JI_NationalAdditionalCode1), x => NationalAdditionalCode1.CY_CodeInfo);

		NationalAdditionalCode NationalAdditionalCode1
		{
			get
			{
				if (nationalAdditionalCode1 == null || nationalAdditionalCode1.IsDeleted)
				{
					var loader = new NationalAdditionalCode.Loader(Factory);
					nationalAdditionalCode1 = loader.Load(this, 1);
					if (nationalAdditionalCode1 != null)
					{
						RegisterEditableChildObject(nationalAdditionalCode1);
					}
				}
				return nationalAdditionalCode1;
			}
		}
		NationalAdditionalCode nationalAdditionalCode1;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NationalAdditionalCodeList))]
		[ResourceStringData("4DB31F11-2DCA-4FD2-BAB7-D8C9D011862C", ShortCaption = "Nat. Code 2", MediumCaption = "National Code 2", Caption = "National Additional Code 2")]
		[ResourceStringData("C92DC420-5123-4FA1-819C-F978D68575CB", ShortCaption = "Nat. Code 2", MediumCaption = "National Code 2", Caption = "National Additional Code 2", FullDescription = "[18 09 060 000] National Additional Code 2", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[MaxLength(4)]
		public virtual ZString JI_NationalAdditionalCode2
		{
			get => NationalAdditionalCode2?.CY_Code ?? ZString.Empty;
			set
			{
				CheckMaximumLength(JI_NationalAdditionalCode2Info, value);
				NationalAdditionalCodeHelper.LoadOrCreate(value, this, 2, JI_NationalAdditionalCode2Info, NationalAdditionalCode2);
			}
		}

		public ZPropertyInfo JI_NationalAdditionalCode2Info => NationalAdditionalCode2 is null ? GetZPropertyInfo(nameof(JI_NationalAdditionalCode2)) : GetWrappedZPropertyInfo(nameof(JI_NationalAdditionalCode2), x => NationalAdditionalCode2.CY_CodeInfo);

		NationalAdditionalCode NationalAdditionalCode2
		{
			get
			{
				if (nationalAdditionalCode2 == null || nationalAdditionalCode2.IsDeleted)
				{
					var loader = new NationalAdditionalCode.Loader(Factory);
					nationalAdditionalCode2 = loader.Load(this, 2);
					if (nationalAdditionalCode2 != null)
					{
						RegisterEditableChildObject(nationalAdditionalCode2);
					}
				}
				return nationalAdditionalCode2;
			}
		}
		NationalAdditionalCode nationalAdditionalCode2;

		[ResourceStringData("53533443-8630-410F-98D7-6E425C62E3AD", ShortCaption = "Nat. Add. Codes", MediumCaption = "National Add. Codes", Caption = "National Additional Codes")]
		[ResourceStringData("A1BDCBB3-8E21-482C-8ADA-D5678A20C868", ShortCaption = "Nat. Add. Codes", MediumCaption = "National Add. Codes", Caption = "National Additional Codes", FullDescription = "[18 09 060 000] National Additional Codes", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public virtual ZString JI_NationalAdditionalCodes => NationalAdditionalCodes.AsString;

		public ZPropertyInfo JI_NationalAdditionalCodesInfo => GetZPropertyInfo(nameof(JI_NationalAdditionalCodes));

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public NationalAdditionalCodeCollection NationalAdditionalCodes
		{
			get
			{
				if (nationalAdditionalCode == null)
				{
					nationalAdditionalCode = NationalAdditionalCodeCollection.New(JI_NationalAdditionalCodesInfo);
					RegisterEditableChildObject(nationalAdditionalCode);
				}

				return nationalAdditionalCode;
			}
		}
		NationalAdditionalCodeCollection nationalAdditionalCode;

		public IEnumerable<NationalAdditionalCode> NationalCodes => NationalAdditionalCodes.Cast<NationalAdditionalCode>().Union(new[] { NationalAdditionalCode1, NationalAdditionalCode2 }).Where(x => x != null);

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public SupplementaryCodeCollection AdditionalSupplementaryCodes
		{
			get
			{
				if (additionalSupplementaryCodes == null)
				{
					additionalSupplementaryCodes = SupplementaryCodeCollection.New(JI_AdditionalSupplementsInfo);
					RegisterEditableChildObject(additionalSupplementaryCodes);
				}

				return additionalSupplementaryCodes;
			}
		}
		SupplementaryCodeCollection additionalSupplementaryCodes;

		ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => AdditionalSupplementaryCodes;

		public IEnumerable<SupplementaryCode> SupplementaryCodes => AdditionalSupplementaryCodes.Cast<SupplementaryCode>().Union(new[] { SupplementaryCode1, SupplementaryCode2 }).Where(x => x != null);

		IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes => SupplementaryCodes;

		public ZString SupplementaryCodesFieldType => UseUniversalTariff ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text);

		TariffView ICusCodeDataWithOrderSupporter.Tariff => UniversalTariff;
		IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => AllApplicableRatesSelectionCriteria;
		CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => Lookups.CachedListOfAdditionalCodeDescriptions;
		ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => SupplementaryCodesFieldType;
		ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => SupplementaryCodeCaptionCore;

		protected virtual ResourceStringData SupplementaryCodeCaptionCore => Res.GetData("EAE4C751-E74C-45D9-803A-28006F0446C0", "Supplementary Code");

		void ICusCodeDataWithOrderSupporter.OnCodesChanged()
		{
			if (UseUniversalTariff)
			{
				ExecuteCustomsUnitDefaultingStrategy();
			}
		}

		public ZString GetCountryCodeForCodeProvider() => GetCountryCodeForSupplementaryCodeProviderCore();
		protected virtual ZString GetCountryCodeForSupplementaryCodeProviderCore() => GetCountryCodeForSupplementaryCodeHelper(Declaration, InvoiceHeader);

		public static string GetCountryCodeForSupplementaryCodeHelper(JobDeclaration declaration, JobComInvoiceHeader invoiceHeader)
			=> declaration?.Country?.RN_Code ?? invoiceHeader?.InvoiceCountry?.RN_Code ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public ZString GetCountryCodeFromAdditionalCode(ZString additionalCode) => GetCountryCodeFromEffectiveVATApplicabilities(additionalCode) ?? GetCountryCodeFromConditions(additionalCode) ?? GetCountryCodeFromRates(additionalCode) ?? ZString.Empty;

		ZString? GetCountryCodeFromEffectiveVATApplicabilities(ZString additionalCode) => GetEffectiveVATApplicabilities().FirstOrDefault(x => x.ZX5_AdditionalCode == additionalCode)?.ZX5_ZZZ_NKDataGrouping;

		ZString? GetCountryCodeFromConditions(ZString additionalCode) => UniversalTariff?.FilteredConditions.FirstOrDefault(x => x.FilteredApplicabilities.Any(y => y.ZZT_AdditionalCode == additionalCode))?.ZX1_ZZZ_NKDataGrouping;

		ZString? GetCountryCodeFromRates(ZString additionalCode) => UniversalTariff?.FilteredRates.FirstOrDefault(x => x.FilteredRateApplicabilities.Any(y => y.ZZT_AdditionalCode == additionalCode))?.ZZ2_ZZZ_NKDataGrouping;

		#endregion

		[MaxLength(1)]
		[ResourceStringData("0bf4061c-dbfb-4b43-9184-bed09d576515", Caption = "[43] Valuation Method", MediumCaption = "Valuation Method", ShortCaption = "Val. Method")]
		public override ZString JI_ValuationCode
		{
			get => base.JI_ValuationCode;
			set
			{
				bool hasChanged = base.JI_ValuationCode != value;
				base.JI_ValuationCode = value;
				if (hasChanged && InvoiceHeader?.JobDeclaration != null)
				{
					var jobComInvLineStrat = (JobComInvoiceLineValueSetStrategy)GetValueSetStrategy();
					if (jobComInvLineStrat != null)
					{
						jobComInvLineStrat.HandleSettingOfNewValuationMethod();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ValuationIndicators))]
		[ResourceStringData("e1216886-4644-481f-a960-770919e9dc80", Caption = "Party relationship, whether there is price influence or not")]
		public override ZString JI_RelatedIndicator
		{
			get => base.JI_RelatedIndicator;
			set
			{
				base.JI_RelatedIndicator = value;
				InvoiceHeader?.RelatedIndicatorInfo.RefreshBinding();
			}
		}

		[ResourceStringData("e1216886-4644-481f-a960-770919e9dc80", Caption = "Party relationship, whether there is price influence or not")]
		public virtual ZBool RelatedIndicator
		{
			get => JI_RelatedIndicator.EqualsIgnoringCase(ValuationIndicatorCodeList.Codes.Yes);
			set => JI_RelatedIndicator = value ? ValuationIndicatorCodeList.Codes.Yes : ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		}
		public ZPropertyInfo RelatedIndicatorInfo => GetWrappedZPropertyInfo(Schema.RelatedIndicator, x => JI_RelatedIndicatorInfo);
		public bool RelatedIndicator_ReadOnly => !RelatedIndicator && (InvoiceHeader?.RelatedIndicator ?? false);

		[ResourceStringData("EU.JobComInvoiceLine.ZG_CommercialReference", Caption = "Commercial Reference")]
		public override ZString ZG_CommercialReference
		{
			get => base.ZG_CommercialReference;
			set => base.ZG_CommercialReference = value;
		}

		[ResourceStringData("EU.JobComInvoiceLine.ZG_RegionOfDestination", Caption = "Region of Destination", MediumCaption = "Reg. of Dest.", ShortCaption = "Reg./Dest.")]
		[ResourceStringData("EU.JobComInvoiceLine.ZG_RegionOfDestination|UCC6IMP", Caption = "Region of Destination", ShortCaption = "Dest. Region", FullDescription = "[16 04 001 000] Region of Destination", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString ZG_RegionOfDestination
		{
			get => base.ZG_RegionOfDestination;
			set => base.ZG_RegionOfDestination = value;
		}
		#endregion

		#region MultiLineAddInfo Collections
		#region public SupportingDocumentCollection SupportingDocuments
		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments => supportingDocuments ?? (supportingDocuments = GetSupportingDocuments());
		SupportingDocumentCollection supportingDocuments;

		SupportingDocumentCollection GetSupportingDocuments()
		{
			var result = CreateNewSupportingDocumentCollection();

			if (MaxSupportingDocuments != -1)
			{
				result.EnableMaxCountValidationWithMessageError(MaxSupportingDocuments, warnAtHalfway: false, SupportingDocumentsValidationMessage, GetSupportingDocumentsMaxCountReduction);
			}

			result.Load();
			RegisterEditableChildObject(result);

			return result;
		}
		public virtual int MaxSupportingDocuments => -1;
		public virtual ZString SupportingDocumentsValidationMessage { get; }
		public virtual Func<int> GetSupportingDocumentsMaxCountReduction => () => 0;

		protected virtual SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		public virtual List<SupportingDocument> EffectiveSupportingDocuments()
		{
			var result = new List<SupportingDocument>();

			// Each Supporting Document can either be header only, header or line, or line only.
			// header or line are all currently sent at the line level so get added together for the merge key
			foreach (SupportingDocument sd in SupportingDocuments)
			{
				if (sd.IsLine)
				{
					result.Add(sd);
				}
			}

			return result;
		}
		#endregion

		#region public AdditionalInfoCollection AdditionalInfos
		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos => fAdditionalInfos ?? (fAdditionalInfos = GetAdditionalInfos());
		AdditionalInfoCollection fAdditionalInfos;

		AdditionalInfoCollection GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		public List<AdditionalInfo> EffectiveAdditionalInfos() => EffectiveAdditionalInfosCore();

		protected virtual List<AdditionalInfo> EffectiveAdditionalInfosCore()
		{
			var result = new List<AdditionalInfo>();
			result.AddRange(AdditionalInfos.Cast<AdditionalInfo>());

			// Each Additional Info can either be header only, header or line, or line only.
			// header or line are all currently sent at the line level so get added together for the merge key
			var invoice = this.InvoiceHeader;
			if (invoice != null)
			{
				foreach (AdditionalInfo ai in invoice.AdditionalInfos)
				{
					if (ai.IsLine)
					{
						result.Add(ai);
					}
				}
			}
			return result;
		}

		#endregion

		#region public PreviousDocumentCollection PreviousDocuments
		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments => fPreviousDocuments ?? (fPreviousDocuments = GetPreviousDocuments());
		PreviousDocumentCollection fPreviousDocuments;

		PreviousDocumentCollection GetPreviousDocuments()
		{
			var result = CreateNewPreviousDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		public virtual ZBool IsPreviousDocsRelevant => ZBool.True;

		public ZBool ShouldCheckMissingPreviousDocuments => ShouldCheckMissingPreviousDocumentsCore;
		protected virtual ZBool ShouldCheckMissingPreviousDocumentsCore => ZBool.True;

		#endregion

		ZDateTime ITaxAndDocsProvider.DateOfValuation => Declaration?.DateOfValuation ?? ZDateTime.Now;
		BusinessObjectCollection ITaxAndDocsProvider.Taxes => Taxes;

		#region public CusAddInfoCollection<Tax> Taxes
		[ChildEditable(true)]
		public JobComInvoiceLineTaxCollection Taxes => taxes ?? (taxes = GetTaxes());
		JobComInvoiceLineTaxCollection taxes;

		JobComInvoiceLineTaxCollection GetTaxes()
		{
			var result = CreateTaxCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual JobComInvoiceLineTaxCollection CreateTaxCollection() => new JobComInvoiceLineTaxCollection(this);

		#endregion
		#endregion

		#region ICanBeImportOrExport Members

		string ICanBeImportOrExport.Level => UniversalReferenceConstants.RefCusCodeListLevelType.Item;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
			if (CusEntryLine != null)
			{
				foreach (BaseJobComInvoiceLine line in CusEntryLine.InvoiceLines)
				{
					line.Validation.ValidateJI_Tariff();
				}
			}
			else
			{
				Validation.ValidateJI_Tariff();
			}
		}

		string ICanBeImportOrExport.TrueCountryCode => CountryCode;
		string ICanBeImportOrExport.DataGroupingCode => GetDefaultDataGroupingCode();

		#endregion

		public override ZGuid JI_CC
		{
			get => base.JI_CC;
			set
			{
				bool hasChanges = base.JI_CC != value;
				base.JI_CC = value;
				if (hasChanges && !IsCopying)
				{
					if (Classification != null)
					{
						base.JI_Procedure = Classification.CC_ProcedureCode;
						JI_SupplementaryCode1 = Classification.CC_EcSupplement1;
						JI_SupplementaryCode2 = Classification.CC_EcSupplement2;
						AdditionalSupplementaryCodes.RemoveAndDeleteAll();
						Classification.AdditionalSupplementaryCodes.CloneElementsTo(AdditionalSupplementaryCodes);
					}
				}
			}
		}

		[DecimalPlaces(3)]
		public ZDecimal TotalBondedWhsQuantity
		{
			get => InvoiceHeader?.JobDeclaration?.FilteredInvoiceLines.Sum(x => x.JI_BondedWhsQuantity) ?? ZDecimal.Zero;
		}

		[DecimalPlaces(6)]
		public ZDecimal TotalCustomsQuantity
		{
			get => InvoiceHeader?.JobDeclaration?.FilteredInvoiceLines.Sum(x => x.JI_CustomsQuantity) ?? ZDecimal.Zero;
		}

		[DecimalPlaces(3)]
		public ZDecimal TotalWeight
		{
			get => InvoiceHeader?.JobDeclaration?.FilteredInvoiceLines.Sum(x => x.JI_Weight) ?? ZDecimal.Zero;
		}

		public ZDecimal TotalLinePrice
		{
			get => InvoiceHeader?.JobDeclaration?.FilteredInvoiceLines.Sum(x => x.JI_LinePrice) ?? ZDecimal.Zero;
		}

		#region UpdateDetailsOnPartChange

		protected override void ASNRefereshDataCountrySpecific(IEnumerable<ZString> refreshOptions, BaseCusClassPartPivot pivot)
		{
			base.ASNRefereshDataCountrySpecific(refreshOptions, pivot);
			if (refreshOptions.Contains(DefaultOptions.Codes.CountryOfOrigin))
			{
				JI_CountryOfOrigin = pivot.CI_RN_NKCountryOfOrigin;
			}
		}

		#endregion

		public new CusEntryLine CusEntryLine => (CusEntryLine)base.CusEntryLine;

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new JobComInvoiceLine Clone() => (JobComInvoiceLine)base.Clone();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new FetchStrategies.JobComInvoiceLineFetchStrategy(this);

		protected override IValueSetStrategy GetValueSetStrategy() => (Declaration != null) ? Declaration.GetJobComInvoiceLineValueSetStrategy(this) : new JobComInvoiceLineValueSetStrategy();

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Taxes.RemoveAndDeleteAll();
				FiscalReferences.RemoveAndDeleteAll();
				CusAuthorizationUsages.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
			=> EntryInstruction?.EntryHeader is CusEntryHeader entryHeader
				&& entryHeader.LockNumberOfEntryLines
				&& !RowErrors.Contains(Validation.GetNotAllowCreateNewEntryLineErrorMessage(entryHeader))
			? ResString.GetMultilingualString("CE112853-73C2-46A7-81E9-C70FFC7CE781", "The selected Invoice Line has been already declared. Entry lines cannot be deleted from a declared or canceled Entry.")
			: base.GetWarningBeforeBeingDeleted();

		public override void OnLoaded()
		{
			base.OnLoaded();
			taxOrFeeDetail = null;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JI_CEI = Declaration?.CustomsEntryInstructions.FirstOrDefault()?.PK ?? ZGuid.Empty;
			JI_CustomsUnitQty = "KGM";
			JI_TaxOrFeeDetail = ZGuid.Empty;
		}

		public void SetFecChallengeFlag(string propertyInfoName, bool newValueToWhichFecChallengeFlagWillBeChanged)
		{
			this[propertyInfoName] = newValueToWhichFecChallengeFlagWillBeChanged;
		}

		public EffectiveValueManager EffectiveValueManager => effectiveValueManager ?? (effectiveValueManager = new EffectiveValueManager());
		EffectiveValueManager effectiveValueManager;

		protected T GetValueOrReturnDefault<T>(BusinessObject bizObj, string fieldName) where T : IZType
		{
			return bizObj == null ? default(T) : (T)bizObj[fieldName];
		}

		protected T GetInvoiceEffectiveValueToReturnIfNeeded<T>(T baseValue, string fieldNameInInvoiceLine, string fieldNameInInvoiceHeader) where T : IZType
		{
			return EffectiveValueManager.GetEffectiveValueToReturn(baseValue, fieldNameInInvoiceLine, () => GetValueOrReturnDefault<T>(InvoiceHeader, fieldNameInInvoiceHeader));
		}

		protected T GetEntryInstructionEffectiveValueToReturnIfNeeded<T>(T baseValue, string fieldNameInInvoiceLine, string fieldNameInInstruction) where T : IZType
		{
			return EffectiveValueManager.GetEffectiveValueToReturn(baseValue, fieldNameInInvoiceLine, () => GetValueOrReturnDefault<T>(EntryInstruction, fieldNameInInstruction));
		}

		protected T GetDeclarationEffectiveValueToReturnIfNeeded<T>(T baseValue, string fieldNameInInvoiceLine, string fieldNameInDeclaration) where T : IZType
		{
			return EffectiveValueManager.GetEffectiveValueToReturn(baseValue, fieldNameInInvoiceLine, () => GetValueOrReturnDefault<T>(Declaration, fieldNameInDeclaration));
		}

		protected T GetEffectiveValueToSetCompareToInvoice<T>(T valuePassed, string fieldNameInInvoiceHeader) where T : IZType
		{
			T result = valuePassed;

			if (!result.IsDefault && !string.IsNullOrEmpty(fieldNameInInvoiceHeader) && InvoiceHeader is JobComInvoiceHeader invoice && invoice[fieldNameInInvoiceHeader].Equals(valuePassed))
			{
				result = (T)result.Default;
			}

			return result;
		}

		protected T GetEffectiveValueToSetCompareToEntryInstruction<T>(T valuePassed, string fieldNameInInstruction) where T : IZType
		{
			T result = valuePassed;

			if (!result.IsDefault && !string.IsNullOrEmpty(fieldNameInInstruction) && EntryInstruction is CusEntryInstruction instruction && instruction[fieldNameInInstruction].Equals(valuePassed))
			{
				result = (T)result.Default;
			}

			return result;
		}

		protected T GetEffectiveValueToSetCompareToDeclaration<T>(T valuePassed, string fieldNameInJobDeclaration) where T : IZType
		{
			T result = valuePassed;

			if (!result.IsDefault && !string.IsNullOrEmpty(fieldNameInJobDeclaration) && Declaration is JobDeclaration declaration && declaration[fieldNameInJobDeclaration].Equals(valuePassed))
			{
				result = (T)result.Default;
			}

			return result;
		}

		#region IAdditionalProcedureParent Members

		#region ICanBeImportOrExport Members

		CodeDescriptionPairList IAdditionalProcedureParent.AdditionalProcedureCodeList => AdditionalProcedureCodeListCore();

		protected virtual CodeDescriptionPairList AdditionalProcedureCodeListCore()
		{
			var dataGroupingCode = GetDefaultDataGroupingCode();
			CodeDescriptionPairList additionalProcedureCodeList;

			if (JI_Procedure.IsEmpty)
			{
				additionalProcedureCodeList = GetCachedAdditionalProcedureCodeList(
					"JI_Procedure_EMPTY",
					cdpl => Lookups.CPCList.ForEach(procedure => AddAdditionalProcedureCodeDescription(cdpl, dataGroupingCode, procedure))
				);
			}
			else if (CusProcedure == null)
			{
				additionalProcedureCodeList = GetCachedAdditionalProcedureCodeList("CusProcedure_NULL", cdpl => { });
			}
			else
			{
				var currentPlusPreviousOfMainProcedure = CusProcedure.ZZ6_ProcedureCode + CusProcedure.ZZ6_PreviousProcedureCode;

				additionalProcedureCodeList = GetCachedAdditionalProcedureCodeList(
					GetKey(currentPlusPreviousOfMainProcedure, Lookups.CPCList.CompleteFilter.GetHashKey().ToString(), JI_Procedure),
					cdpl =>
					{
						foreach (var additionalProcedure in Lookups.CPCList.Where(x =>
							x.FullCodeCurrentPlusPreviousPlusConcession != JI_Procedure &&
							x.FullCodeCurrentPlusPreviousPlusConcession.StartsWith(currentPlusPreviousOfMainProcedure, StringComparison.OrdinalIgnoreCase)))
						{
							AddAdditionalProcedureCodeDescription(cdpl, dataGroupingCode, additionalProcedure);
						}
					});
			}

			return additionalProcedureCodeList;
		}

		protected void AddAdditionalProcedureCodeDescription(CodeDescriptionPairList targetList, ZString dataGroupingCode, RefCusProcedure procedure)
		{
			var concessionDescription = RefCusProcedure.GetConcessionDescription(Factory, dataGroupingCode, procedure.ZZ6_Concession);
			targetList.AddPair(((ICodeDescription)procedure).Code, concessionDescription);
		}

		protected string GetKey(params string[] keys) => string.Format(Culture.Invariant, string.Join("_", keys));

		protected CodeDescriptionPairList GetCachedAdditionalProcedureCodeList(string key, Action<CodeDescriptionPairList> initialiseCache)
		{
			return Factory.GetCachedValue(key, () =>
			{
				var result = new CodeDescriptionPairList();
				initialiseCache(result);
				result.Sort();
				return result;
			});
		}

		AdditionalProcedureCodeCollection IAdditionalProcedureParent.AdditionalProcedureCodes => AdditionalProcedureCodes;

		BusinessObject IAdditionalProcedureParent.BusinessObject => this;

		ZString IAdditionalProcedureParent.MainProcedure => JI_Procedure;

		ZString IAdditionalProcedureParent.MainProcedurePrefix
		{
			get
			{
				var cusProcedure = CusProcedure;
				return !JI_Procedure.IsEmpty && cusProcedure != null ? (ZString)(cusProcedure.ZZ6_ProcedureCode + cusProcedure.ZZ6_PreviousProcedureCode) : ZString.Empty;
			}
		}

		ZPropertyInfo IAdditionalProcedureParent.AdditionalProcedureCodesAsStringInfo => AdditionalProcedureCodesAsStringInfo;

		int IAdditionalProcedureParent.MaxNumberOfAdditionalProcedureCode => MaxNumberOfAdditionalProcedureCode;

		#endregion

		ZBool ICanBeImportOrExport.IsExport => IsExport;

		ZBool ICanBeImportOrExport.IsImport => IsImport;

		#endregion

		#region IUltimateDistributee Members

		ZBool IUltimateDistributee.IsCapableOfCalculatingOwnGstVatRate => true;

		ZDecimal IUltimateDistributee.CalculateOwnGstVatRate()
		{
			ZDecimal result = 0m;
			foreach (JobComInvoiceLineTax tax in Taxes)
			{
				if (tax.JLT_Type == UniversalReferenceConstants.RefCusRateCodes.Vat)
				{
					result = tax.JLT_Calc_CalculatedPercentage / 100m;
					break;
				}
			}
			return result;
		}

		#endregion

		#region ICusSupportingInfoTypeSupporter Members

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo));
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument));
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument));
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			foreach (var fetchStrategy in GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return fetchStrategy;
			}
		}

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes() => GetCusCodeDataTypes();

		protected virtual Dictionary<ZString, Type> GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>()
			{
				{ CusCodeDataTypeList.Codes.SupplementaryCode, typeof(SupplementaryCode) },
				{ CusCodeDataTypeList.Codes.AdditionalProcedureCode, typeof(AdditionalProcedureCode) },
				{ CusCodeDataTypeList.Codes.NationalAdditionalCode, typeof(NationalAdditionalCode) }
			};
		}

		#endregion

		#region ICusReferenceTypeSupporter Members

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, SupplyChainActorType },
			{ CusReferenceTypeList.Codes.FiscalReference, FiscalReferenceType },
		};

		protected virtual Type SupplyChainActorType => typeof(CusSupplyChainActorReference);
		protected virtual Type FiscalReferenceType => typeof(CusFiscalReference);

		#endregion

		#region ICusLinkPackageSupporter

		protected override ZBool IsSupportEmptyPackType(BasePackage package)
		{
			var pack = package as Package;
			return pack != null && pack.IsEmptyPackTypeAllowed;
		}

		#endregion

		public override ZGuid JI_CEI
		{
			get => base.JI_CEI;
			set
			{
				var oldValue = JI_CEI;
				base.JI_CEI = value;
				if (oldValue != JI_CEI)
				{
					Declaration?.MarkAsNeedingValidation();
					InvoiceHeader?.MarkAsNeedingValidation();
					MarkAsNeedingValidation();
					fSellerDocAddress?.MarkAsNeedingValidation();
					fBuyerDocAddress?.MarkAsNeedingValidation();
				}
			}
		}

		public bool ShouldKeepNotAllowCreateNewEntryLineError { get; set; }

		[BusinessObjectTestExclude]
		public new ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails => (ICusLineTariffDetailCollection<CusLineTariffDetail>)base.CusLineTariffDetails;

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection<CusLineTariffDetail>(this);

		public override ZString UniversalTariffType
		{
			get
			{
				var result = base.UniversalTariffType;
				if (IsImport)
				{
					result = CusTariffTypes.ImportTariff;
				}
				else if (IsExport)
				{
					result = CusTariffTypes.ExportTariff;
				}
				return result;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TaxOrFeeCodeList))]
		[ResourceStringData("76a6cf37-02d6-4323-aaf2-94872383b707", Caption = "VAT")]
		[ReadOnlyMember(nameof(JI_ZZF_NKTaxTypeReadOnly))]
		public override ZString JI_ZZF_NKTaxType
		{
			get => base.JI_ZZF_NKTaxType;
			set
			{
				if (!(Configuration?.UseMultipleVatFields ?? false))
				{
					var oldValue = JI_ZZF_NKTaxType;
					base.JI_ZZF_NKTaxType = value;
					if (oldValue != JI_ZZF_NKTaxType)
					{
						SetSupplementaryCodeFromVatApplicabilitiesWhenUsingSingleVatField();
					}
				}
				else
				{
					base.JI_ZZF_NKTaxType = value;
				}
			}
		}

		protected virtual bool JI_ZZF_NKTaxTypeReadOnly => false;

		[RelatedBusinessObjectTestExclude("RelatedBusinessObject TaxOrFeeDetailEntity is a NonPersistentBusinessObject")]
		[RelatedBusinessObject(nameof(JI_TaxOrFeeDetailEntity))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TaxOrFeeDetailEntities))]
		[ResourceStringData("76a6cf37-02d6-4323-aaf2-94872383b707", Caption = "VAT")]
		public ZGuid JI_TaxOrFeeDetail
		{
			get
			{
				if (!taxOrFeeDetail.HasValue)
				{
					var entitiesList = Lookups.TaxOrFeeDetailEntities;
					var foundEntity = entitiesList.FirstOrDefault(entity => entity.VATCode == JI_ZZF_NKTaxType && SupplementaryCodes.Any(code => code.CY_Code == entity.AdditionalCode)) ?? entitiesList.FirstOrDefault(entity => entity.VATCode == JI_ZZF_NKTaxType && entity.AdditionalCode.IsEmpty);
					taxOrFeeDetail = foundEntity?.PK ?? ZGuid.Empty;
				}
				return taxOrFeeDetail.Value;
			}
			set
			{
				var oldValue = taxOrFeeDetail;
				if (value != oldValue)
				{
					taxOrFeeDetail = value;
					JI_TaxOrFeeDetailInfo.RefreshBinding(oldValue);
					if (Configuration?.UseMultipleVatFields ?? false)
					{
						SetTaxOrFeeAndSupplementaryCodeFromJI_TaxOrFeeDetailEntityWhenUsingMultipleVatFields();
					}
				}
			}
		}
		ZGuid? taxOrFeeDetail;

		public ZPropertyInfo JI_TaxOrFeeDetailInfo => GetZPropertyInfo(nameof(JI_TaxOrFeeDetail));

		public TaxOrFeeDetailEntity JI_TaxOrFeeDetailEntity
		{
			get
			{
				return Factory.GetCachedValue($"{Lookups.GetVATCacheKey()}-{JI_TaxOrFeeDetail}", () =>
				{
					return Lookups.TaxOrFeeDetailEntities.FirstOrDefault(x => x.PK == JI_TaxOrFeeDetail);
				});
			}
		}

		protected override void WipeNKTaxTypeCore()
		{
			if (ShouldWipeNKTaxType)
			{
				JI_TaxOrFeeDetail = ZGuid.Empty;
				JI_ZZF_NKTaxType = ZString.Empty;
			}
		}

		[ResourceStringData("F88559A4-1EE6-48C2-AF19-AB9AC541C5F4", Caption = "Buyer")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.BuyerList))]
		public JobDocAddress BuyerDocAddress
		{
			get
			{
				if (fBuyerDocAddress == null || fBuyerDocAddress.IsDeleted)
				{
					fBuyerDocAddress = ((IDocAddresses)this).DocAddresses.FindOrCreateWithRequirement(BuyerDocAddressRequirement);
					fBuyerDocAddress.AdditionalValidation = GetBuyerJobDocAddressAdditionalValidation(fBuyerDocAddress);
				}
				return fBuyerDocAddress;
			}
		}
		JobDocAddress fBuyerDocAddress;

		protected virtual ZValidation GetBuyerJobDocAddressAdditionalValidation(JobDocAddress buyerJobDocAddress) => new InvoiceLineTraderJobDocAddressValidation(buyerJobDocAddress);

		public JobDocAddressRequirement BuyerDocAddressRequirement
		{
			get
			{
				if (fBuyerDocAddressRequirement == null)
				{
					fBuyerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.BuyingParty, ContactType.Administration);
					DocAddressManager.AddRequirement(fBuyerDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fBuyerDocAddressRequirement, DocAddressType.BuyingParty);
				return fBuyerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fBuyerDocAddressRequirement;

		[ResourceStringData("C62E0F43-5392-4EAD-8D1E-2169302B17F9", Caption = "Seller")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SellerList))]
		public JobDocAddress SellerDocAddress
		{
			get
			{
				if (fSellerDocAddress == null || fSellerDocAddress.IsDeleted)
				{
					fSellerDocAddress = ((IDocAddresses)this).DocAddresses.FindOrCreateWithRequirement(SellerDocAddressRequirement);
					fSellerDocAddress.AdditionalValidation = GetSellerJobDocAddressAdditionalValidation(fSellerDocAddress);
				}
				return fSellerDocAddress;
			}
		}
		JobDocAddress fSellerDocAddress;

		protected virtual ZValidation GetSellerJobDocAddressAdditionalValidation(JobDocAddress sellerJobDocAddress) => new InvoiceLineTraderJobDocAddressValidation(sellerJobDocAddress);

		public JobDocAddressRequirement SellerDocAddressRequirement
		{
			get
			{
				if (fSellerDocAddressRequirement == null)
				{
					fSellerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.SellingParty, ContactType.Administration);
					DocAddressManager.AddRequirement(fSellerDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fSellerDocAddressRequirement, DocAddressType.SellingParty);
				return fSellerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fSellerDocAddressRequirement;

		protected void SetTaxOrFeeAndSupplementaryCodeFromJI_TaxOrFeeDetailEntityWhenUsingMultipleVatFields()
		{
			var taxType = JI_TaxOrFeeDetailEntity?.VATCode ?? ZString.Empty;
			var additionalCode = JI_TaxOrFeeDetailEntity?.AdditionalCode ?? ZString.Empty;

			JI_ZZF_NKTaxType = taxType;
			SetVATAdditionalCode(additionalCode);
		}

		protected virtual void SetSupplementaryCodeFromVatApplicabilitiesWhenUsingSingleVatField()
		{
			var vatApplicability = GetEffectiveVATApplicabilities().FirstOrDefault(x => x.ZX5_ZZF_NKTaxOrFeeCode == JI_ZZF_NKTaxType);

			if (vatApplicability != null)
			{
				SetVATAdditionalCode(vatApplicability.ZX5_AdditionalCode);
			}
		}

		void SetVATAdditionalCode(ZString additionalCode)
		{
			ClearExistingVATAdditionalCodeIfAny();
			if (!additionalCode.IsEmpty)
			{
				InsertAdditionalCodeIntoSupplementaryCodes(additionalCode);
			}
		}

		void InsertAdditionalCodeIntoSupplementaryCodes(ZString additionalCode)
		{
			if (JI_SupplementaryCode1.IsEmpty)
			{
				JI_SupplementaryCode1 = additionalCode;
			}
			else if (JI_SupplementaryCode2.IsEmpty)
			{
				JI_SupplementaryCode2 = additionalCode;
			}
			else
			{
				AdditionalSupplementaryCodes.AddNew(additionalCode);
			}
		}

		void ClearExistingVATAdditionalCodeIfAny()
		{
			var vatAdditionalCodes = GetEffectiveVATApplicabilities().Select(x => x.ZX5_AdditionalCode).Where(x => !x.IsEmpty).ToHashSet();

			if (vatAdditionalCodes.Contains(JI_SupplementaryCode1))
			{
				JI_SupplementaryCode1 = ZString.Empty;
			}

			if (vatAdditionalCodes.Contains(JI_SupplementaryCode2))
			{
				JI_SupplementaryCode2 = ZString.Empty;
			}

			AdditionalSupplementaryCodes.Where(x => vatAdditionalCodes.Contains(x.CY_Code)).ToList().ForEach(t => t.Delete());
		}

		public IEnumerable<VATApplicabilityView> GetEffectiveVATApplicabilities() => UniversalTariff?.GetEffectiveVATApplicabilities(EffectiveAssessmentDate) ?? Enumerable.Empty<VATApplicabilityView>();

		public IZZRateSelectionCriteria ADDRateSelectionCriteria => Factory.GetValue(ref addRateSelectionCriteriaCached, GetADDRateSelectionCriteriaCore);
		CachedProperty<IZZRateSelectionCriteria> addRateSelectionCriteriaCached;

		protected virtual IZZRateSelectionCriteria GetADDRateSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, RefCusRateTypes.AntiDumpingDuty, ZString.Empty);

		public IZZRateSelectionCriteria CVDRateSelectionCriteria => Factory.GetValue(ref cvdRateSelectionCriteriaCached, GetCVDRateSelectionCriteriaCore);
		CachedProperty<IZZRateSelectionCriteria> cvdRateSelectionCriteriaCached;

		protected virtual IZZRateSelectionCriteria GetCVDRateSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, RefCusRateTypes.CountervailingDuty, ZString.Empty);

		public override ConditionChecker.EvaluateConditionValue EvaluateConditionValue => (conditionType, valueType, inputValue) =>
		{
			var result = ZBool.False;
			switch (valueType)
			{
				case Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument:
				case Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber:
					result = DocumentsIncludingInherited.Any(x => x.CSI_Code == inputValue);
					break;
			}
			return result;
		};

		IEnumerable<CusSupportingInfo> DocumentsIncludingInherited
		{
			get
			{
				if (Declaration?.IsUCC6 ?? false)
				{
					return SupportingDocumentsIncludingInherited.Cast<CusSupportingInfo>().Union(AdditionalDocumentsIncludingInherited.Cast<CusSupportingInfo>());
				}
				else
				{
					return SupportingDocumentsIncludingInherited.Cast<CusSupportingInfo>();
				}
			}
		}

		protected virtual IEnumerable<AdditionalInfo> AdditionalDocumentsIncludingInherited => AdditionalInfos.Cast<AdditionalInfo>()
																							.Union(InvoiceHeader?.AdditionalInfos.Cast<AdditionalInfo>() ?? Enumerable.Empty<AdditionalInfo>())
																							.Union(Declaration?.AdditionalInfos.Cast<AdditionalInfo>() ?? Enumerable.Empty<AdditionalInfo>())
																							.Where(p => p.IsAnAdditionalReference);

		IEnumerable<SupportingDocument> SupportingDocumentsIncludingInherited => SupportingDocuments.Cast<SupportingDocument>()
																							.Union(InvoiceHeader?.SupportingDocuments.Cast<SupportingDocument>() ?? Enumerable.Empty<SupportingDocument>())
																							.Union(Declaration?.SupportingDocuments.Cast<SupportingDocument>() ?? Enumerable.Empty<SupportingDocument>())
																							.Union(EntryInstruction?.SupportingDocuments.Cast<SupportingDocument>() ?? Enumerable.Empty<SupportingDocument>());

		protected override IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, ZString.Empty, ZString.Empty);

		protected override IZZRateSelectionCriteria GetDutyRateSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, Constants.RateTypes.Duty, ZString.Empty);

		public IZZRateSelectionCriteria AntiDumpingRateSelectionCriteria => Factory.GetValue(ref antiDumpingRateSelectionCriteriaCached, GetAntiDumpingRateSelectionCriteriaCore);
		CachedProperty<IZZRateSelectionCriteria> antiDumpingRateSelectionCriteriaCached;

		protected virtual IZZRateSelectionCriteria GetAntiDumpingRateSelectionCriteriaCore() => new RateSelectionCriteriaNoPrimaryPreference<JobComInvoiceLine>(this, RefCusRateTypes.AntiDumpingDuty, ZString.Empty);

		public IZZRateSelectionCriteria CountervailingRateSelectionCriteria => Factory.GetValue(ref countervailingRateSelectionCriteriaCached, GetCountervailingRateSelectionCriteriaCore);
		CachedProperty<IZZRateSelectionCriteria> countervailingRateSelectionCriteriaCached;

		protected virtual IZZRateSelectionCriteria GetCountervailingRateSelectionCriteriaCore() => new RateSelectionCriteriaNoPrimaryPreference<JobComInvoiceLine>(this, RefCusRateTypes.CountervailingDuty, ZString.Empty);

		public IEnumerable<IZZRateSelectionCriteria> NationalRateSelectionCriteria => Factory.GetValue(ref nationalRateSelectionCriteriaCached, GetNationalRateSelectionCriteriaCore);
		CachedProperty<IEnumerable<IZZRateSelectionCriteria>> nationalRateSelectionCriteriaCached;

		protected virtual IEnumerable<IZZRateSelectionCriteria> GetNationalRateSelectionCriteriaCore() => Enumerable.Empty<IZZRateSelectionCriteria>();

		public ZString EffectiveCountryOfDestination => ZG_CountryOfDestination.FallbackTo(Declaration?.JE_GoodsDestination ?? ZString.Empty);

		public new class RateSelectionCriteria<T> : BaseJobComInvoiceLine.RateSelectionCriteria<T> where T : JobComInvoiceLine
		{
			public RateSelectionCriteria(T invoiceLine, ZString rateType, ZString rateCode)
				: base(invoiceLine, rateType, rateCode)
			{
			}

			protected override ISet<ZString> GetAdditionalCodes(T invoiceLine) => invoiceLine.SupplementaryCodes.Where(x => x != null).Select(x => x.CY_Code).ToHashSet();

			protected override ZString GetTradeGroupCountry(T invoiceLine) => invoiceLine.IsExport ? invoiceLine.EffectiveCountryOfDestination : base.GetTradeGroupCountry(invoiceLine);
		}

		#region ZZSelectionCriteria - Unshelved

		protected override IZZConditionSelectionCriteria[] GetConditionSelectionCriterias() => new[] { new ZZConditionSelectionCriteria<JobComInvoiceLine>(this) };

		protected override bool UseUniversalConditionCheck => true;

		internal bool GetUniversalConditionCheck() => UseUniversalConditionCheck;

		public new class ZZConditionSelectionCriteria<T> : BaseJobComInvoiceLine.ZZConditionSelectionCriteria<T> where T : JobComInvoiceLine
		{
			public ZZConditionSelectionCriteria(T invoiceLine)
				: base(invoiceLine)
			{
			}

			protected override ISet<ZString> GetAdditionalCodes(T invoiceLine) => invoiceLine.SupplementaryCodes.Where(x => x != null).Select(x => x.CY_Code).ToHashSet();

			protected override ZString GetTradeGroupCountry(T invoiceLine) => invoiceLine.IsExport ? invoiceLine.EffectiveCountryOfDestination : base.GetTradeGroupCountry(invoiceLine);
		}

		#endregion

		protected override IVATSelectionCriteria GetVATSelectionCriteriaCore() => new EUVATSelectionCriteria<JobComInvoiceLine>(this);

		public class EUVATSelectionCriteria<T> : ZZVATSelectionCriteria<T> where T : JobComInvoiceLine
		{
			public EUVATSelectionCriteria(T invoiceLine) : base(invoiceLine)
			{
			}

			protected override ISet<ZString> GetAdditionalCodes(T invoiceLine) => invoiceLine.SupplementaryCodes.Where(x => x != null).Select(x => x.CY_Code).ToHashSet();
		}

		public class RateSelectionCriteriaNoPrimaryPreference<T> : RateSelectionCriteria<T>
			where T : JobComInvoiceLine
		{
			public RateSelectionCriteriaNoPrimaryPreference(T invoiceLine, ZString rateType, ZString rateCode)
				: base(invoiceLine, rateType, rateCode)
			{
			}

			protected override ZString GetPrimaryPreference(T invoiceLine) => ZString.Empty;
		}

		[ResourceStringData("EU.JobComInvoiceLine.JI_PreviousEntryNumber", Caption = "Prev. Entry No.")]
		public override ZString JI_PreviousEntryNumber
		{
			get => base.JI_PreviousEntryNumber;
			set => base.JI_PreviousEntryNumber = value;
		}

		[ResourceStringData("EU.JobComInvoiceLine.JI_PreviousEntryLineNumber", Caption = "Prev. Entry Line No.", ShortCaption = "Prev. Line No.")]
		public override ZShort JI_PreviousEntryLineNumber
		{
			get => base.JI_PreviousEntryLineNumber;
			set => base.JI_PreviousEntryLineNumber = value;
		}

		protected override ZBool IsPreviousEntryNumberVisibleCore
		{
			get
			{
				if (SupportsBondedWarehousing)
				{
					return CusProcedure?.IsOutOfRegime() ?? false;
				}
				return false;
			}
		}

		[ResourceStringData("EU.JobComInvoiceLine.JI_BondedWhsQuantity", Caption = "Bonded Whs. Qty")]
		public override ZDecimal JI_BondedWhsQuantity
		{
			get => base.JI_BondedWhsQuantity;
			set => base.JI_BondedWhsQuantity = value;
		}

		[ResourceStringData("EU.JobComInvoiceLine.JI_BondedWhsUnitQty", Caption = "Bonded Whs. UQ.")]
		public override ZString JI_BondedWhsUnitQty
		{
			get => base.JI_BondedWhsUnitQty;
			set => base.JI_BondedWhsUnitQty = value;
		}

		protected override ZBool IsBondedWhsQuantityVisibleCore
		{
			get
			{
				if (SupportInvoiceLineComponents)
				{
					return true;
				}

				if (SupportsBondedWarehousing)
				{
					var procedure = CusProcedure;
					return procedure != null && (procedure.IsIntoVATWarehouse() || procedure.IsIntoRegime() || procedure.IsOutOfRegime());
				}
				return false;
			}
		}

		public void OnCusProcedureChanged()
		{
			var shallClearBondedWhsQuantity = !IsBondedWhsQuantityVisibleCore;

			if (shallClearBondedWhsQuantity)
			{
				if (!JI_BondedWhsQuantity.IsEmpty)
				{
					JI_BondedWhsQuantityInfo.ClearValue();
				}
				if (!JI_BondedWhsUnitQty.IsEmpty)
				{
					JI_BondedWhsUnitQtyInfo.ClearValue();
				}
			}
			else if (IsBondedWhsQuantityVisible)
			{
				if (JI_BondedWhsQuantity.IsEmpty)
				{
					JI_BondedWhsQuantity = JI_InvoiceQuantity;
				}
				if (JI_BondedWhsUnitQty.IsEmpty)
				{
					JI_BondedWhsUnitQty = JI_InvoiceUQ;
				}
			}
		}

		SchemaGuidColumn ICusAuthorizationUsageMaster.FKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;

		[ChildEditable]
		public ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine> CusAuthorizationUsages
		{
			get
			{
				if (cusAuthorizationUsages == null)
				{
					cusAuthorizationUsages = GetCusAuthorizationUsages();
					cusAuthorizationUsages.Load();
					cusAuthorizationUsages.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(cusAuthorizationUsages);
				}
				return cusAuthorizationUsages;
			}
		}

		ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine> cusAuthorizationUsages;

		protected virtual ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>(this, Factory);

		InvoiceLineConfiguration Configuration => Declaration?.Configuration.InvoiceLineConfiguration;

		IAddInfoWithSyncProperty IAddInfoWithSyncPropertySupporter.AddInfo => AddInfo;

		public void EmptyTaxTypeIfNotImport()
		{
			if (!IsImport)
			{
				JI_ZZF_NKTaxType = ZString.Empty;
			}
		}

		#region InvoiceLinePayments

		[ResourceStringData("6D73EC7C-39EC-45A6-85D6-346EC2870477", ShortCaption = "Payment Code", Caption = "Payment Code")]
		public override ZString ZG_CommercialPaymentCode { get => base.ZG_CommercialPaymentCode; set => base.ZG_CommercialPaymentCode = value; }

		[ResourceStringData("F078DD5B-53F4-4C75-B8EF-CAAB06820E3B", ShortCaption = "Amount", Caption = "Amount")]
		public override ZDecimal ZG_CommercialPaymentAmount { get => base.ZG_CommercialPaymentAmount; set => base.ZG_CommercialPaymentAmount = value; }

		[ResourceStringData("0AF947C4-6AD6-429E-B5DE-0FFD90F32971", ShortCaption = "Payment Reference", Caption = "Payment Reference")]
		public override ZString ZG_CommercialPaymentNumber { get => base.ZG_CommercialPaymentNumber; set => base.ZG_CommercialPaymentNumber = value; }

		[ResourceStringData("7A4A4A77-963D-48CD-A5D7-BEC26254F8A6", ShortCaption = "Payment Ref.Date", Caption = "Payment Ref.Date")]
		public override ZDateTime ZG_CommercialPaymentDate { get => base.ZG_CommercialPaymentDate; set => base.ZG_CommercialPaymentDate = value; }

		#endregion

		public virtual ZBool IsValuationCodeAvailable => ZBool.False;

		public IGuidedDecisionMakingSource GetGuidedDecisionMakingSingleInvoiceLineSource() => GetGuidedDecisionMakingSingleInvoiceLineSourceCore();

		protected virtual IGuidedDecisionMakingSource GetGuidedDecisionMakingSingleInvoiceLineSourceCore() => new GuidedDecisionMakingSingleInvoiceLineSource(this);

		public IGuidedDecisionMakingTarget GetGuidedDecisionMakingSingleInvoiceLineTarget() => GetGuidedDecisionMakingSingleInvoiceLineTargetCore();

		protected virtual IGuidedDecisionMakingTarget GetGuidedDecisionMakingSingleInvoiceLineTargetCore() => new GuidedDecisionMakingSingleInvoiceLineTarget(this);

		public GuidedDecisionMakingBasic GetGuidedDecisionMakingBasic()
		{
			var source = GetGuidedDecisionMakingSingleInvoiceLineSource();
			return GetGuidedDecisionMakingBasicCore(source);
		}

		public IGuidedDecisionMakingSource GetGuidedDecisionMakingMultiInvoiceLinesSource() => GetGuidedDecisionMakingMultiInvoiceLinesSourceCore();

		protected virtual IGuidedDecisionMakingSource GetGuidedDecisionMakingMultiInvoiceLinesSourceCore() => new GuidedDecisionMakingMultiInvoiceLinesSource(this);

		public IGuidedDecisionMakingTarget GetGuidedDecisionMakingMultiInvoiceLinesTarget(List<JobComInvoiceLine> invoiceLines) => GetGuidedDecisionMakingMultiInvoiceLinesTargetCore(invoiceLines);

		protected virtual IGuidedDecisionMakingTarget GetGuidedDecisionMakingMultiInvoiceLinesTargetCore(List<JobComInvoiceLine> invoiceLines) => new GuidedDecisionMakingMultiInvoiceLinesTarget(invoiceLines);

		public GuidedDecisionMakingBasic GetGuidedDecisionMakingBasicForMultiLine()
		{
			var source = GetGuidedDecisionMakingMultiInvoiceLinesSource();
			return GetGuidedDecisionMakingBasicCore(source);
		}

		protected virtual GuidedDecisionMakingBasic GetGuidedDecisionMakingBasicCore(IGuidedDecisionMakingSource source) => new GuidedDecisionMakingBasic(source, Factory);

		protected override ZDecimal ComponentPrice
		{
			get
			{
				ZDecimal result = JI_LinePrice;

				if (Declaration != null && Configuration.InflateItemPriceByValuationMarkup(Declaration))
				{
					result = JI_LinePrice * (1 + (JI_ValuationMarkup / 100));
				}

				return result;
			}
		}

		public IReadOnlyList<string> MultipleKeysToUse => InvoiceHeader?.MultipleKeysToUse ?? Array.Empty<string>();

		public IEntryNumberFormatterForNctsAndDeclarationIntegration GetEntryNumberFormatter() => GetEntryNumberFormatterCore();
		protected virtual IEntryNumberFormatterForNctsAndDeclarationIntegration GetEntryNumberFormatterCore() => new EntryNumberFormatterForNctsAndDeclarationIntegration();

		PreviousDocumentCollection IPreviousDocumentsProvider.PreviousDocuments => PreviousDocuments;

		public void SetFirst2CharactersOfJI_Procedure(ZString characters)
		{
			string newProcedure;
			var code = characters.PadRight(2);
			if (JI_Procedure.Length > 2)
			{
				var otherCharacters = JI_Procedure.Substring(2);
				newProcedure = code + otherCharacters;
			}
			else
			{
				newProcedure = code;
			}

			if (JI_Procedure != newProcedure)
			{
				JI_Procedure = newProcedure;
			}
		}

		internal bool ShouldNotHaveTraders()
		{
			var procedureCode = JI_Calc_RequestedProcedure;
			var concession = JI_Calc_Concession;
			var additionalDeclarationType = EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

			return procedureCode == Core.Constants.Customs.Universal.RefCusProcedure.Codes._51
				|| procedureCode == Core.Constants.Customs.Universal.RefCusProcedure.Codes._53
				|| procedureCode == Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration
				|| concession == Core.Constants.Customs.Universal.RefCusProcedure.Concession.F15
				|| additionalDeclarationType == EntrySubStyleList.Codes.SimplifiedDeclaration
				|| additionalDeclarationType == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
		}

		#region PreviousDocuments

		IPreviousDocumentValidationDecider IPreviousDocumentsProviderWithValidationDecider.ValidationDecider => InvoiceHeader?.JobDeclaration?.Configuration?.InvoiceLineConfiguration?.GetPreviousDocumentValidationDecider(this);

		#endregion

		#region SupportingDocuments

		ISupportingDocumentCollection<SupportingDocument> ISupportingDocumentsProvider.SupportingDocuments => SupportingDocuments;

		ISupportingDocumentValidationDecider ISupportingDocumentsProviderWithValidationDecider.ValidationDecider => InvoiceHeader?.JobDeclaration?.Configuration?.InvoiceLineConfiguration?.GetSupportingDocumentValidationDecider(this);

		#endregion

		#region AdditionalInfos

		IAdditionalInfoCollection<AdditionalInfo> IAdditionalInfosProvider.AdditionalInfos => AdditionalInfos;

		IAdditionalInfoValidationDecider IAdditionalInfosProviderWithValidationDecider.ValidationDecider => InvoiceHeader?.JobDeclaration?.Configuration?.InvoiceLineConfiguration?.GetAdditionalInfoValidationDecider(this);

		#endregion

		#region IUcc6ValueProvider

		bool IUcc6ValueProvider.IsUCC6 => Declaration?.IsUCC6 ?? false;

		#endregion

		protected override IRefCountry CountryOfOriginFallbackCore => JI_CountryOfOrigin.IsEmpty ? null : new DataTransferCountryInfo(JI_CountryOfOrigin, ((CodeDescriptionPairList)Lookups.CountryOfOrigins).GetDescriptionFromCode(JI_CountryOfOrigin));

		protected override IRefCountry CountryOfExportFallbackCore => JI_RN_NKCountryOfExport.IsEmpty ? null : new DataTransferCountryInfo(JI_RN_NKCountryOfExport, Lookups.CountryOfExports.FirstOrDefault(x => x.RN_Code == JI_RN_NKCountryOfExport)?.RN_Desc ?? null);

		protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) => new InvoiceLinePackageValidation(linkPackage, this);
	}
}
