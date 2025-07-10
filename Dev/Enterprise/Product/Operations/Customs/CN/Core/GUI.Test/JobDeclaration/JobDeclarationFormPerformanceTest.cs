using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.GUI.Testing
{
	abstract class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		#region Prepare Test Data
		protected override void DecorateDeclaration(BaseJobDeclaration baseDeclaration)
		{
			var declaration = (JobDeclaration)baseDeclaration;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_CNPortOfOrigin = "CHN000";
			declaration.JE_CNPortOfDestination = "AUS000";
			declaration.JE_CNLastPortBeforeEntry = "USA000";
			for (var i = 0; i < 6; i++)
			{
				DecorateEntryInstruction(declaration.CustomsEntryInstructions.AddNew(), i);
			}
		}

		void DecorateEntryInstruction(Business.CusEntryInstruction instruction, int index)
		{
			instruction.CEI_Style = "0110"; // RefCusProcedure
			instruction.CEI_Description = index.ToString();
			instruction.BillOfLading = "BL111";
			instruction.CEI_LevyType = "101";
			instruction.CustomsMessageRemarks = "Remarks";
			instruction.CEI_CIQRequires = true;
			for (var i = 0; i < 6; i++)
			{
				instruction.EnterpriseQualifications.AddNew();
			}

			for (var i = 0; i < 6; i++)
			{
				instruction.OperationMatters.AddNew();
			}

			for (var i = 0; i < 6; i++)
			{
				instruction.OtherPackages.AddNew();
			}

			for (var i = 0; i < 6; i++)
			{
				instruction.SpecialBusinessIdentifiers.AddNew();
			}
		}

		protected override void DecorateInvoiceHeader(BaseJobComInvoiceHeader baseInvoice)
		{
			var invoice = (JobComInvoiceHeader)baseInvoice;
			for (var i = 0; i < 6; i++)
			{
				invoice.ContractNumbers.AddNew();
			}
		}

		protected override void DecorateInvoiceLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var index = invoiceLine.JI_LineNo;
			invoiceLine.JI_CEI = invoiceLine.Declaration.CustomsEntryInstructions[index % 3 * 2].PK;
			invoiceLine.JI_Tariff = TariffCodes[index % 6]; // RefCusAttribute, RefCusRate, RefCusApplicability, RefCusTariffUOM
			invoiceLine.JI_CIQTariff = TariffCodes[index % 6] + "999"; // RefCusTariff, RefCusTariffRelationship
																	   //invoiceLine.CIQIngredient = "Ingredient";
			invoiceLine.JI_NameOfGoods = "NameOfGoods";
			invoiceLine.XC_GoodsSpecModel = "1|2|3|4|5";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_StateOrRegionOfOrigin = "AB";
			invoiceLine.JI_RN_NKCountryOfExport = "CN";
			invoiceLine.JI_DestinationDistrict = index.ToString();
			invoiceLine.JI_DestinationRegion = index.ToString();
			invoiceLine.JI_OriginDistrict = index.ToString();
			invoiceLine.JI_OriginRegion = index.ToString();
			invoiceLine.JI_CIQOriginState = index.ToString();
			invoiceLine.JI_PrimaryPreference = "MFN";
			invoiceLine.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "2008", "c", "IMO").First().PK;
			for (var i = 0; i < 6; i++)
			{
				invoiceLine.CusSupportingDocuments.AddNew().CSI_Code = i.ToString(); // RefCusCondition, RefCusConditionValue
				invoiceLine.CusSupportingDocuments.AddNew().CSI_Code = (i + 10).ToString(); // RefCusCondition, RefCusConditionValue
			}

			for (var i = 0; i < 6; i++)
			{
				invoiceLine.CIQProductQualifications.AddNew();
			}

			for (var i = 0; i < 6; i++)
			{
				invoiceLine.VINDataCollection.AddNew();
			}

			for (var i = 0; i < 6; i++)
			{
				invoiceLine.ProductionBatch.AddNew();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var factoryForRefData = new BusinessObjectFactory();
			var minDateTime = ZDateTime.MinSmallDateTimeValue;
			var maxDateTime = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(factoryForRefData);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China, "China");
			factoryForRefData.Save();
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00422", "品牌类型");
			helper.CreateAdditionalElement("00069", "出口享惠情况");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("00010", "包装规格");
			helper.CreateAdditionalElement("00009", "GTIN");
			helper.CreateAdditionalElement("00005", "CAS");
			helper.CreateAdditionalElement("99999", "其他");
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "DTY");
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(factoryForRefData, "DTY", dtyRateType.PK);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.China, "CTRL", "CNDOC", "Customs Required Documents");
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.China, "DOC");
			var mfnPreference = helper.CreatePreferenceForCountry("MFN", "MFN", Core.Constants.CountryCodes.China);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "STANDARD", minDateTime, maxDateTime);
			helper.AddCountry(tradeGroup, "CA", minDateTime.Date, maxDateTime.Date);
			helper.CreateNewOrGetExistingCusCodeType("CNDOC", "CN Doc");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNDOC", "1", "1", minDateTime, maxDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNDOC", "2", "2", minDateTime, maxDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNDOC", "3", "3", minDateTime, maxDateTime);
			factoryForRefData.Save();
			foreach (var tariffCode in TariffCodes)
			{
				var tariff = helper.LoadCustomsTariff(factoryForRefData, tariffCode);
				if (tariff == null)
				{
					tariff = helper.CreateCustomsTariff(tariffCode, "00000", "00422", "00069", "00352", "00010", "00009", "00005", "99999");
					factoryForRefData.Save();
					helper.CreateTariffUOM(tariff, "CU1", "001");
					helper.CreateCIQTariff(tariffCode + "999");
					var mfnRate = helper.CreateRate(tariff, dtyRateCode.PK, minDateTime, maxDateTime, rateFormula: "0.2 * VFD", preferencePk: mfnPreference.PK);
					helper.CreateCusApplicability(mfnRate, tradeGroup, minDateTime, maxDateTime);
					helper.CreateSingleValueConditionForTariff(tariff.PK, true, true, conditionType.PK, "1", conditionValueType.PK, "1");
					helper.CreateSingleValueConditionForTariff(tariff.PK, true, true, conditionType.PK, "2", conditionValueType.PK, "2");
					helper.CreateSingleValueConditionForTariff(tariff.PK, true, true, conditionType.PK, "3", conditionValueType.PK, "3");
				}
			}

			factoryForRefData.Save();
			_ = GlbCompany.CurrentCompany.OrgProxy;
		}

		string[] TariffCodes => tariffCodes ?? (tariffCodes = new[] { "9001100001", "2928000035", "2934999034", "2935900013", "8482103000", "3918909000" });
		string[] tariffCodes;
		#endregion
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);
		Dictionary<string, int> CNBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ TariffAttributeViewSchema.Constants.TableName, 6 },
			{ TariffViewSchema.Constants.TableName, 6 },
		};
		Dictionary<string, int> CNBaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 8 },
			{ GlbBranchSchema.Constants.TableName, 5 },
			{ OrgHeaderSchema.Constants.TableName, 6 },
			{ RateViewSchema.Constants.TableName, 7 },
			{ RefCusConditionSchema.Constants.TableName, 7 },
			{ RefCusConditionValueSchema.Constants.TableName, 6 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 10 },
			{ RefCusConditionLanguageSchema.Constants.TableName, 6 },
			{ TariffViewSchema.Constants.TableName, 7 },
			{ TariffAttributeViewSchema.Constants.TableName, 7 },
		};
		Dictionary<string, int> CNBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ RateViewSchema.Constants.TableName, 7 },
			{ RefCusConditionSchema.Constants.TableName, 7 },
			{ RefCusConditionValueSchema.Constants.TableName, 6 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 8 },
			{ RefCusConditionLanguageSchema.Constants.TableName, 6 },
			{ TariffViewSchema.Constants.TableName, 7 }
		};
		Dictionary<string, int> CNBaseFormMergeExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> CNBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ VATApplicabilityViewSchema.Constants.TableName, 6 },
			{ TariffViewSchema.Constants.TableName, 7 }
		};
		Dictionary<string, int> CNBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ RefPacksSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 10 },
			{ StmNoteSchema.Constants.TableName, 11 },
			{ StmDocDataOverrideSchema.Constants.TableName, 10 },
			{ RefCusConditionSchema.Constants.TableName, 7 },
			{ RefCusConditionValueSchema.Constants.TableName, 18 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 12 }
		};
		Dictionary<string, int> CNBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ RefPacksSchema.Constants.TableName, 6 },
			{ RefCusConditionSchema.Constants.TableName, 7 },
			{ RefCusConditionValueSchema.Constants.TableName, 18 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 12 }
		};
		Dictionary<string, int> CNBaseDeleteExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 22 },
			{ TariffAttributeViewSchema.Constants.TableName, 6 },
			{ TariffViewSchema.Constants.TableName, 6 },
		};
		protected virtual Dictionary<string, int> CNLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CNValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CNLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CNFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CNUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CNUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CNUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CNDeleteExpectedHits => new Dictionary<string, int>();
		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(CNBaseLoadEditableChildObjectsExpectedHits, CNLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(CNBaseValidateAllExpectedHits, CNValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(CNBaseLightFormValidationAndSaveExpectedHits, CNLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(CNBaseFormMergeExpectedHits, CNFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(CNBaseUniversalXMLExportExpectedHits, CNUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(CNBaseUniversalXMLImportUpdateExpectedHits, CNUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(CNBaseUniversalXMLAddExpectedHits, CNUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(CNBaseDeleteExpectedHits, CNDeleteExpectedHits);
	}
}
