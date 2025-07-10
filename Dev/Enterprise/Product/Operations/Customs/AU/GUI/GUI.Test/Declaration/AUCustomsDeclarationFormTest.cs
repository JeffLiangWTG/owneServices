using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	#region Form Tests

	public abstract class AUCustomsDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashingCore();
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.CusContainers.AddNew();
			declaration.Bills.AddNew();
			return declaration;
		}

		protected override BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
		{
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			var tariff = Tariffs[(index + (invoiceIndex * 10)) % Tariffs.Length];
			invoiceLine.JI_Tariff = tariff.SC_TariffClassificationNumber + tariff.SC_StatisticalClassificationCode;
			invoiceLine.AddInfo.ZA_PST = "GEN";
			invoiceLine.AddInfo.ZA_WAR = index.ToString().PadLeft(5, 'A');
			for (var i = 1; i < 7; i++)
			{
				var permit = invoiceLine.ICSPermits.AddNew();
				permit.CY_Data = $"{invoiceIndex}_{index}_PER{i}";
			}
			return invoiceLine;
		}

		CMRStatisticalClassificationPeriodSnapshot[] Tariffs => tariffs ?? (tariffs = Factory.Load<CMRStatisticalClassificationPeriodSnapshot>(new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_TariffClassificationNumber, TariffsToLoad)));
		CMRStatisticalClassificationPeriodSnapshot[] tariffs;

		internal static ZString[] TariffsToLoad
		{
			get
			{
				return new ZString[]
				{
					"01011000", "02022000", "03019900", "04063000", "05079000", "06049100", "07061000", "08030000", "09102000", "10082000",
					"11042200", "12076000", "13021400", "14030000", "15111000", "16022000", "17023000", "18032000", "19042010", "20029000",
					"21033000", "22030072", "23050000", "24039100", "25061000", "26050000", "27022000", "28030000", "29053100", "30059010",
					"31053000", "32081000", "33029000", "34039190", "35030090", "36050000", "37025300", "38070000", "39049000", "40052000",
					"41069100", "42031000", "43031000", "44113900", "45020000", "46019900", "47072000", "48022049", "49019990", "50050000",
					"51022000", "52041100", "53039000", "54025900", "55062000", "56021000", "57022000", "58063910", "59090010", "60054300",
					"61034100", "62033100", "63029190", "64029190", "65061000", "66031000", "67049000", "68069010", "69131000", "70049000",
					"71031000", "72045000", "73052000", "74112200", "75089000", "76072000", "76169900", "78042000", "79040000", "80040000",
					"81029400", "82056000", "83091000", "84129090", "85064000", "86079100", "87039020", "88023000", "89061090", "90066100",
					"91141000", "92079000", "93039000", "94039000", "95049010", "96151100", "97019000", "97060000", "99995025", "9999703"
				};
			}
		}
	}

	#endregion

	#region Form Performance Tests

	class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new ZAUCustomsDeclarationForm((JobDeclaration)bizO);

		Dictionary<string, int> AUBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 }
		};

		Dictionary<string, int> AUBaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ GlbBranchSchema.Constants.TableName, 5 },
			{ OrgCusCodeSchema.Constants.TableName, 8 }
		};

		Dictionary<string, int> AUBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ OrgCusCodeSchema.Constants.TableName, 8 }
		};

		Dictionary<string, int> AUBaseFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 }
		};

		Dictionary<string, int> AUBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 7 },
			{ QuarantineExDocHeaderSchema.Constants.TableName, 6 }
		};

		Dictionary<string, int> AUBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 7 },
			{ StmNoteSchema.Constants.TableName, 5 },
			{ QuarantineExDocHeaderSchema.Constants.TableName, 6 },
			{ OrgAddressCapabilitySchema.Constants.TableName, 7 }
		};

		Dictionary<string, int> AUBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 7 },
			{ OrgAddressCapabilitySchema.Constants.TableName, 7 }
		};

		Dictionary<string, int> AUBaseDeleteExpectedHits => new Dictionary<string, int>
		{
			{ QuarantineExDocHeaderSchema.Constants.TableName, 6 }
		};

		protected virtual Dictionary<string, int> AULoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> AUValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> AULightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> AUFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> AUUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> AUUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> AUUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> AUDeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(AUBaseLoadEditableChildObjectsExpectedHits, AULoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(AUBaseValidateAllExpectedHits, AUValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(AUBaseLightFormValidationAndSaveExpectedHits, AULightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(AUBaseFormMergeExpectedHits, AUFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(AUBaseUniversalXMLExportExpectedHits, AUUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(AUBaseUniversalXMLImportUpdateExpectedHits, AUUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(AUBaseUniversalXMLAddExpectedHits, AUUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(AUBaseDeleteExpectedHits, AUDeleteExpectedHits);
	}

	sealed class JobDeclarationFormPerformanceTest_WhenCancelled_PerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override bool DeclarationIsCancelled => true;

		protected override Dictionary<string, int> AUDeleteExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 8 }
		};
	}

	sealed class JobDeclarationFormPerformanceTest_Import_PerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.Import;

		protected override Dictionary<string, int> AUValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 8 }
		};

		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ OrgCusCodeSchema.Constants.TableName, 8 },
			{ OrgHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> AUFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ CusCodeDataSchema.Constants.TableName, 60 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 6 },
			{ StmALogSchema.Constants.TableName, 7 }
		};

		protected override Dictionary<string, int> AUUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
		};

		protected override Dictionary<string, int> AULoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 8 }
		};
	}

	sealed class JobDeclarationFormPerformanceTest_Export_PerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.Export;

		protected override Dictionary<string, int> AUFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 6 }
		};
	}

	sealed class JobDeclarationFormPerformanceTest_Drawback_PerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.Drawback;

		protected override Dictionary<string, int> AUUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ JobHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> AUUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ OrgPartRelationSchema.Constants.TableName, 19 },
			{ StmNoteSchema.Constants.TableName, 22 }
		};

		protected override Dictionary<string, int> AUUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ OrgPartRelationSchema.Constants.TableName, 19 },
			{ StmNoteSchema.Constants.TableName, 18 }
		};
	}

	sealed class JobDeclarationFormPerformanceTest_ExWarehouse_PerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;

		protected override Dictionary<string, int> AUValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ GlbBranchSchema.Constants.TableName, 5 },
			{ OrgCusCodeSchema.Constants.TableName, 8 }
		};

		protected override Dictionary<string, int> AULightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 8 },
			{ OrgHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> AUUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ JobHeaderSchema.Constants.TableName, 5 },
		};
	}

	sealed class JobDeclarationFormPerformanceTest_Miscellaneous_PerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;

		protected override Dictionary<string, int> AUUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ JobHeaderSchema.Constants.TableName, 5 }
		};
	}

	sealed class JobDeclarationFormPerformanceTest_WarehousedByExternalAgent_PerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.WarehousedByExternalAgent;

		protected override Dictionary<string, int> AUValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 8 }
		};

		protected override Dictionary<string, int> AULightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 8 },
			{ OrgHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> AUFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ CusCodeDataSchema.Constants.TableName, 60 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 6 },
			{ StmALogSchema.Constants.TableName, 7 }
		};

		protected override Dictionary<string, int> AUUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
		};
	}

	sealed class JobDeclarationFormPerformanceTest_Quarantine_PerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => AUJobMessageTypeList.Codes.Quarantine;

		protected override Dictionary<string, int> AULoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ QuarantineExDocEstablishmentAndTimeSchema.Constants.TableName, 60 }
		};

		protected override Dictionary<string, int> AUValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ QuarantineExDocEstablishmentAndTimeSchema.Constants.TableName, 60 },
			{ QuarantineExDocHeaderSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> AULightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ QuarantineExDocEstablishmentAndTimeSchema.Constants.TableName, 60 },
			{ QuarantineExDocHeaderSchema.Constants.TableName, 6 },
			{ QuarantineExDocLineSchema.Constants.TableName, 60 }
		};

		protected override Dictionary<string, int> AUUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 7 },
			{ CusCodeDataSchema.Constants.TableName, 13 },
			{ CusEntryNumSchema.Constants.TableName, 14 },
			{ CusSupportingInfoSchema.Constants.TableName, 6 },
			{ JobDocAddressSchema.Constants.TableName, 7 },
			{ QuarantineExDocEstablishmentAndTimeSchema.Constants.TableName, 60 },
			{ QuarantineExDocShipsCompartmentSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> AUUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ CusCodeDataSchema.Constants.TableName, 62 },
			{ EDIMessageSchema.Constants.TableName, 8 },
			{ QuarantineExDocEstablishmentAndTimeSchema.Constants.TableName, 60 },
			{ QuarantineExDocHeaderSchema.Constants.TableName, 12 },
			{ QuarantineExDocShipsCompartmentSchema.Constants.TableName, 6 },
			{ StmDocDataOverrideSchema.Constants.TableName, 126 },
			{ StmNoteSchema.Constants.TableName, 65 }
		};

		protected override Dictionary<string, int> AUUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ QuarantineExDocHeaderSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> AUDeleteExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 7 },
			{ CusCodeDataSchema.Constants.TableName, 8 },
			{ CusSupportingInfoSchema.Constants.TableName, 6 },
			{ GenAddOnColumnSchema.Constants.TableName, 6 },
			{ QuarantineExDocEstablishmentAndTimeSchema.Constants.TableName, 60 },
			{ QuarantineExDocShipsCompartmentSchema.Constants.TableName, 6 },
			{ StmDocDataOverrideSchema.Constants.TableName, 18 },
			{ StmNoteSchema.Constants.TableName, 9 },
			{ CusEntryNumSchema.Constants.TableName, 12 }
		};
	}

	#endregion
}
