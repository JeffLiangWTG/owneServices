using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TCPGAHeader))]
	sealed class TCPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<TCPGAHeader>
	{
		public void TestLPCODefaulter()
		{
			var header = Factory.New<TCPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			Assert(!lpcoDefaulter.ShouldDefaultLPCOFields);
			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			header.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VCC;
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
		}

		public void TestAvailableLPCOFields()
		{
			AssertEquals(4, TCPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_RN_NKAuthorizationCountry", TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKAuthorizationCountry));
			Assert("CLP_DIFRefNumberOrLocation", TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_RefNo", TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
		}

		public void TestSupportsNotes()
		{
			var bo = (TCPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestImporterDeclarationState()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.CA_TCInd = "Y";
			var pgaHeader = invoiceLine.TCPGAHeader;

			AssertEquals(string.Empty, pgaHeader.CA_ImporterDeclarationCode);

			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_ImportReasonCode = TCIntendedUseCodes.Codes.TC01;
			pgaHeader.CA_ProductType = TCProductCategories.Codes.TC04;

			pgaHeader.CA_ProductClass = TCProductCategories.Codes.TC01;
			invoiceLine.JI_Tariff = "4011100011";
			invoiceLine.JI_CountryOfOrigin = "US";
			pgaHeader.IsUSImporterDeclared = true;
			AssertEquals(TCComplicanceStatements.Codes.TC02, pgaHeader.CA_ImporterDeclarationCode);

			pgaHeader.IsZZImporterDeclared = true;
			AssertEquals(TCComplicanceStatements.Codes.TC01, pgaHeader.CA_ImporterDeclarationCode);

			invoiceLine.JI_CountryOfOrigin = "CN";
			pgaHeader.IsZZImporterDeclared = true;
			AssertEquals(TCComplicanceStatements.Codes.TC01, pgaHeader.CA_ImporterDeclarationCode);

			pgaHeader.CA_ProductClass = TCProductCategories.Codes.TC03;
			invoiceLine.JI_Tariff = "4012201011";
			pgaHeader.IsZZImporterDeclared = true;
			AssertEquals(TCComplicanceStatements.Codes.TC07, pgaHeader.CA_ImporterDeclarationCode);

			invoiceLine.JI_CountryOfOrigin = "US";
			pgaHeader.IsUSImporterDeclared = true;
			AssertEquals(TCComplicanceStatements.Codes.TC08, pgaHeader.CA_ImporterDeclarationCode);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_TCInd = "Y";
			var pgaHeader2 = invoiceLine2.TCPGAHeader;
			pgaHeader2.CA_VPRProgramInd = "Y";
			pgaHeader2.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFS;
			pgaHeader2.IsVPRImporterDeclared = true;
			AssertEquals(TCComplicanceStatements.Codes.TC06, pgaHeader2.CA_ImporterDeclarationCode);

			pgaHeader2.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFC;
			pgaHeader2.IsVPRImporterDeclared = true;
			AssertEquals(TCComplicanceStatements.Codes.TC06, pgaHeader2.CA_ImporterDeclarationCode);

			pgaHeader2.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VVP;
			pgaHeader2.IsVPRImporterDeclared = true;
			AssertEquals(TCComplicanceStatements.Codes.TC04, pgaHeader2.CA_ImporterDeclarationCode);
		}

		public void TestSetDefaultImporterDeclarationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.CA_TCInd = "Y";
			invoiceLine.JI_Tariff = "4011100011";
			var pgaHeader = invoiceLine.TCPGAHeader;

			AssertEquals(string.Empty, pgaHeader.CA_ImporterDeclarationCode);

			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_ImportReasonCode = TCIntendedUseCodes.Codes.TC01;
			pgaHeader.CA_ProductType = TCProductCategories.Codes.TC04;
			pgaHeader.CA_ProductClass = TCProductCategories.Codes.TC01;

			AssertEquals(TCComplicanceStatements.Codes.TC01, pgaHeader.CA_ImporterDeclarationCode);

			invoiceLine.JI_CountryOfOrigin = "US";
			pgaHeader.CA_ProductClass = TCProductCategories.Codes.TC02;
			AssertEquals(TCComplicanceStatements.Codes.TC02, pgaHeader.CA_ImporterDeclarationCode);
		}

		public void TestManufacturerLetterAttached()
		{
			var pgaHeader = GetNewBusinessObjectForDeleteTest(Factory) as TCPGAHeader;
			Assert(!pgaHeader.ManufacturerLetterAttached);

			pgaHeader.CA_CriteriaConformance = TCComplicanceStatements.Codes.TC05;
			Assert(pgaHeader.ManufacturerLetterAttached);

			pgaHeader.CA_CriteriaConformance = TCComplicanceStatements.Codes.TC03;
			Assert(!pgaHeader.ManufacturerLetterAttached);

			pgaHeader.ManufacturerLetterAttached = true;

			AssertEquals(TCComplicanceStatements.Codes.TC05, pgaHeader.CA_CriteriaConformance);
			AssertEquals(1, pgaHeader.LPCOViews.Count);
			AssertEquals(LPCODocumentTypeQualifier.Codes._4003, pgaHeader.LPCOViews[0].CLP_Type);
		}

		public void TestStatementLabelAttached()
		{
			Assert(!header.StatementLabelAttached);

			header.CA_CriteriaConformance = TCComplicanceStatements.Codes.TC03;
			Assert(header.StatementLabelAttached);

			header.CA_CriteriaConformance = TCComplicanceStatements.Codes.TC05;
			Assert(!header.StatementLabelAttached);

			header.StatementLabelAttached = true;

			AssertEquals(TCComplicanceStatements.Codes.TC03, header.CA_CriteriaConformance);
		}

		public void TestAddDefaultLPCOs()
		{
			var pgaHeader = Factory.New<TCPGAHeader>();
			pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VCC;
			AssertContainsExactElementsInAnyOrder(new[] { "4002", "4004" }, pgaHeader.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));
		}

		public void TestImporterDeclarationOnProduct()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "LON01";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CCA_AirsCode = "A001";

			pivot.CCA_TCIndicator = YesNoList.Codes.Yes;
			var pgaHeader = pivot.TCPGAHeader;
			pivot.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;

			pgaHeader.CA_ImportReasonCode = TCIntendedUseCodes.Codes.TC01;
			pgaHeader.CA_ProductType = TCProductCategories.Codes.TC04;
			pgaHeader.CA_ProductClass = TCProductCategories.Codes.TC01;

			Assert(!pgaHeader.USImporterDeclarationVisibility);
			Assert(!pgaHeader.ZZImporterDeclarationVisibility);
			Assert(!pgaHeader.IsNewOrRetreadedTiresNeedDeclaraion);
			Assert(!pgaHeader.IsUsedTiresNeedDeclaraion);
		}

		public void TestImporterDeclarationOnCusClassification()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = ClassificationType.IMP;
			classification.CCA_TCIndicator = YesNoList.Codes.Yes;
			var pgaHeader = classification.TCPGAHeader;
			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;

			pgaHeader.CA_ImportReasonCode = TCIntendedUseCodes.Codes.TC01;
			pgaHeader.CA_ProductType = TCProductCategories.Codes.TC04;
			pgaHeader.CA_ProductClass = TCProductCategories.Codes.TC01;

			Assert(!pgaHeader.USImporterDeclarationVisibility);
			Assert(!pgaHeader.ZZImporterDeclarationVisibility);
			Assert(!pgaHeader.IsNewOrRetreadedTiresNeedDeclaraion);
			Assert(!pgaHeader.IsUsedTiresNeedDeclaraion);
		}

		#region Purge Values

		public void TestPurgeCA_ProductClass()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR,
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ProductClass, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ProductType()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ProductType, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ProductSize()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ProductSize, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ImportReasonCode()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ImportReasonCode, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ImporterDeclaration()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ImporterDeclarationCode, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ChassisManufacturerName()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ChassisManufacturerName, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ChassisMake()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ChassisMake, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ChassisModel()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ChassisModel, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ChassisYear()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ChassisYear, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_AssemblerName()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_AssemblerName, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_CriteriaConformance()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_CriteriaConformance, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_VehicleCondition()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_VehicleCondition, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ODOReading()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ODOReading, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_TitleStatus()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_TitleStatus, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_VehicleStatus()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_VehicleStatus, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_SubProgram()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.VPR
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_SubProgram, (ZString)"A", availabePrograms);
		}

		public void TestPurgeCA_ManufactureYear()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ManufactureYear, (ZString)"1", availabePrograms);
		}

		public void TestPurgeCA_ManufactureMonth()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertPurgeValues(AutoTCPGAHeader.Schema.CA_ManufactureMonth, (ZString)"1", availabePrograms);
		}

		public void TestPurgeLPCOs()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			var pgaHeader = Factory.New<TCPGAHeader>();
			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;

			foreach (var subProgram in availabePrograms)
			{
				foreach (var subProgramChangeTo in VPRSubProgramList.GetAllCodes().Where(x => x != subProgram))
				{
					pgaHeader.CA_SubProgram = subProgram;
					pgaHeader.LPCOViews.RemoveAndDeleteAll();
					var lpco = pgaHeader.LPCOViews.AddNew();
					lpco.CLP_RefNo = "1111";

					AssertEquals(1, pgaHeader.LPCOViews.Count);

					pgaHeader.CA_SubProgram = subProgramChangeTo;
					if (availabePrograms.Contains(subProgramChangeTo))
					{
						Assert($"Should NOT clear LPCOs when change SubProgram from {subProgram} to {subProgramChangeTo}", pgaHeader.LPCOViews.Count > 0);
					}
					else
					{
						AssertEquals($"Clear LPCOs when change SubProgram from {subProgram} to {subProgramChangeTo}", 0, pgaHeader.LPCOViews.Count);
					}
				}
			}
		}

		void AssertPurgeValues(ZString propertyName, IZType modifiedValue, string[] availablePrograms)
		{
			var pgaHeader = Factory.New<TCPGAHeader>();
			var propertyInfo = pgaHeader.FindPropertyInfo(propertyName);

			CombineAssertions(() =>
			{
				AssertNotNull(propertyInfo);
				AssertPurgeTPR(pgaHeader, propertyInfo, modifiedValue, availablePrograms);
				AssertPurgeVPR(pgaHeader, propertyInfo, modifiedValue, availablePrograms);
			});
		}

		void AssertPurgeTPR(TCPGAHeader pgaHeader, ZPropertyInfo propertyInfo, IZType modifiedValue, string[] availablePrograms)
		{
			if (availablePrograms.Contains(TCPGADepartmentCodes.Codes.TPR))
			{
				pgaHeader.CA_VPRProgramInd = YesNoList.Codes.No;
				pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
				propertyInfo.Value = modifiedValue;
				pgaHeader.CA_TPRProgramInd = YesNoList.Codes.No;
				AssertEquals($"Clear {propertyInfo.Name}", modifiedValue.Default, propertyInfo.Value);

				var availableSubPrograms = availablePrograms.Where(x => VPRSubProgramList.ContainsCode(x));
				pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;

				foreach (var subProgram in VPRSubProgramList.GetAllCodes())
				{
					pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
					propertyInfo.Value = modifiedValue.Default;
					pgaHeader.CA_SubProgram = subProgram;
					propertyInfo.Value = modifiedValue;
					pgaHeader.CA_TPRProgramInd = YesNoList.Codes.No;

					if (availableSubPrograms.Contains(subProgram))
					{
						AssertEquals($"Should NOT clear {propertyInfo.Name} as {propertyInfo.Name} is used for {subProgram} and TPR", modifiedValue, propertyInfo.Value);
					}
					else
					{
						AssertEquals($"Clear {propertyInfo.Name}", modifiedValue.Default, propertyInfo.Value);
					}
				}
			}
		}

		void AssertPurgeVPR(TCPGAHeader pgaHeader, ZPropertyInfo propertyInfo, IZType modifiedValue, string[] availablePrograms)
		{
			var availableSubPrograms = availablePrograms.Where(x => VPRSubProgramList.ContainsCode(x));
			pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_TPRProgramInd = YesNoList.Codes.No;
			foreach (var subProgram in availableSubPrograms)
			{
				foreach (var subProgramChangeTo in VPRSubProgramList.GetAllCodes().Where(x => x != subProgram))
				{
					pgaHeader.CA_SubProgram = subProgram;
					propertyInfo.Value = modifiedValue;
					pgaHeader.CA_SubProgram = subProgramChangeTo;
					if (propertyInfo.Name == AutoTCPGAHeader.Schema.CA_ImporterDeclarationCode && pgaHeader.CA_VPRProgramInd == YesNoList.Codes.Yes)
					{
						switch (pgaHeader.CA_SubProgram)
						{
							case TCPGAVehicleProgramCodes.Codes.VFS:
							case TCPGAVehicleProgramCodes.Codes.VFC:
								AssertEquals($"Should NOT clear {propertyInfo.Name} as {propertyInfo.Name} is used for {subProgram} and {subProgramChangeTo}", TCComplicanceStatements.Codes.TC06, propertyInfo.Value);
								continue;
							case TCPGAVehicleProgramCodes.Codes.VVP:
								AssertEquals($"Should NOT clear {propertyInfo.Name} as {propertyInfo.Name} is used for {subProgram} and {subProgramChangeTo}", TCComplicanceStatements.Codes.TC04, propertyInfo.Value);
								continue;
						}
					}

					if (availableSubPrograms.Contains(subProgramChangeTo))
					{
						AssertEquals($"Should NOT clear {propertyInfo.Name} as {propertyInfo.Name} is used for {subProgram} and {subProgramChangeTo}", modifiedValue, propertyInfo.Value);
					}
					else
					{
						AssertEquals($"Clear {propertyInfo.Name} when change sub-program from {subProgram} to {subProgramChangeTo}", modifiedValue.Default, propertyInfo.Value);
					}
				}

				pgaHeader.CA_SubProgram = subProgram;
				propertyInfo.Value = modifiedValue;
				pgaHeader.CA_SubProgram = string.Empty;
				AssertEquals($"Clear {propertyInfo.Name} when change sub-program from {subProgram} to empty", modifiedValue.Default, propertyInfo.Value);
			}

			if (availablePrograms.Contains(TCPGADepartmentCodes.Codes.VPR))
			{
				pgaHeader.CA_TPRProgramInd = YesNoList.Codes.No;
				pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
				propertyInfo.Value = modifiedValue;
				pgaHeader.CA_VPRProgramInd = YesNoList.Codes.No;
				AssertEquals($"Clear {propertyInfo.Name}", modifiedValue.Default, propertyInfo.Value);
			}

			if (availablePrograms.Contains(TCPGADepartmentCodes.Codes.TPR))
			{
				pgaHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
				pgaHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
				foreach (var subProgram in availableSubPrograms)
				{
					foreach (var subProgramChangeTo in VPRSubProgramList.GetAllCodes().Where(x => x != subProgram))
					{
						pgaHeader.CA_SubProgram = subProgram;
						propertyInfo.Value = modifiedValue;
						pgaHeader.CA_SubProgram = subProgramChangeTo;
						if (propertyInfo.Name == AutoTCPGAHeader.Schema.CA_ImporterDeclarationCode && pgaHeader.CA_VPRProgramInd == YesNoList.Codes.Yes)
						{
							switch (pgaHeader.CA_SubProgram)
							{
								case TCPGAVehicleProgramCodes.Codes.VFS:
								case TCPGAVehicleProgramCodes.Codes.VFC:
									AssertEquals($"NOT clear {propertyInfo.Name} as {propertyInfo.Name} is used for {subProgram} and TPR", TCComplicanceStatements.Codes.TC06, propertyInfo.Value);
									continue;
								case TCPGAVehicleProgramCodes.Codes.VVP:
									AssertEquals($"NOT clear {propertyInfo.Name} as {propertyInfo.Name} is used for {subProgram} and TPR", TCComplicanceStatements.Codes.TC04, propertyInfo.Value);
									continue;
							}
						}

						AssertEquals($"Not clear {propertyInfo.Name} as {propertyInfo.Name} is used for {subProgram} and TPR", modifiedValue, propertyInfo.Value);
					}
				}
			}
		}

		CodeDescriptionPairList VPRSubProgramList => Factory.GetCachedValue<TCPGAVehicleProgramCodes>();

		#endregion

		public void TestLPCOViewCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "4006", "4006 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.TC);
			newFactory.Save();

			AssertEquals("LpcoViews on TC", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "4006";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on TC", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "4006";
			AssertEquals("LpcoViews on TC", 3, header.LPCOViews.Count);
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNULL()
		{
			var pgaHeader = Factory.New<TCPGAHeader>();
			AssertNoExceptionThrown(() =>
			{
				_ = pgaHeader.OA_Manufacturer;
				pgaHeader.OA_Manufacturer = ZGuid.Empty;
				_ = pgaHeader.OA_ManufacturerInfo.SupportsMaxLength;
				_ = pgaHeader.OA_Manufacturer_ZAddress;
				_ = pgaHeader.RN_NKCountryOfOrigin;
				pgaHeader.RN_NKCountryOfOrigin = ZString.Empty;
				_ = pgaHeader.RN_NKCountryOfOriginInfo.SupportsMaxLength;
				_ = pgaHeader.JI_BrandName;
				pgaHeader.JI_BrandName = ZString.Empty;
				_ = pgaHeader.JI_BrandNameInfo.SupportsMaxLength;
				_ = pgaHeader.JI_Model;
				pgaHeader.JI_Model = ZString.Empty;
				_ = pgaHeader.JI_ModelInfo.SupportsMaxLength;
			});
		}

		public void TestBrandNameAndModelMaxLength()
		{
			AssertEquals("JI_BrandName max length", invoiceLine.JI_BrandNameInfo.MaxLength, header.JI_BrandNameInfo.MaxLength);
			AssertEquals("JI_Model max length", invoiceLine.JI_ModelInfo.MaxLength, header.JI_ModelInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			return invoiceLine.TCPGAHeader;
		}

		protected override IEnumerable<TCPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			yield return invoiceLine.TCPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_TCIndicator = Customs.Business.YesNoList.Codes.Yes;
			yield return pivot.TCPGAHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			header = invoiceLine.TCPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header;
		}
		TCPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
