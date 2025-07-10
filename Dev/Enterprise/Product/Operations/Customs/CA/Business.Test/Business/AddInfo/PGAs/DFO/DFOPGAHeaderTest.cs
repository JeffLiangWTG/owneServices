using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
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
	[TestedType(typeof(DFOPGAHeader))]
	sealed class DFOPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DFOPGAHeader>
	{
		public void TestLPCODefaulter()
		{
			var header = Factory.New<DFOPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			Assert(!lpcoDefaulter.ShouldDefaultLPCOFields);
			header.CA_TTPProgramInd = YesNoList.Codes.Yes;
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
		}

		public void TestAvailableLPCOFields()
		{
			AssertEquals(3, DFOPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_DIFRefNumberOrLocation", DFOPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_RefNo", DFOPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", DFOPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
		}

		public void TestCA_CommonNameCode()
		{
			var header = (DFOPGAHeader)GetNewBusinessObject();
			var line = header.InvoiceLine;
			line.JI_Model = string.Empty;

			header.CA_TTPProgramInd = YesNoList.Codes.No;
			header.CA_CommonNameCode = DFOCommonNameCodes.Codes.FO21;

			AssertEquals(ZString.Empty, line.JI_Model);

			header.CA_TTPProgramInd = YesNoList.Codes.Yes;
			header.CA_CommonNameCode = DFOCommonNameCodes.Codes.FO23;

			var desc = header.AddInfoLookups.CommonNameCodes.GetDescriptionFromCode(header.CA_CommonNameCode);
			Assert(!line.JI_Model.IsEmpty);
			AssertEquals(desc, line.JI_Model);
		}

		public void TestCA_SpeciesCode()
		{
			var header = (DFOPGAHeader)GetNewBusinessObject();

			header.CA_AISProgramInd = YesNoList.Codes.No;
			header.CA_SpeciesCode = DFOScientificNames.Codes.FO11;
			header.CA_GenusOrSpecies = ZString.Empty;

			AssertEquals(ZString.Empty, header.CA_GenusOrSpecies);

			header.CA_AISProgramInd = YesNoList.Codes.Yes;
			header.CA_SpeciesCode = DFOScientificNames.Codes.FO16;

			var desc = header.AddInfoLookups.ScientificNames.GetDescriptionFromCode(header.CA_SpeciesCode);
			Assert(!header.CA_GenusOrSpecies.IsEmpty);
			AssertEquals(desc, header.CA_GenusOrSpecies);
		}

		public void TestCA_GenusOrSpecies_ReadOnly()
		{
			var header = (DFOPGAHeader)GetNewBusinessObject();

			header.CA_AISProgramInd = YesNoList.Codes.No;
			header.CA_SpeciesCode = ZString.Empty;

			AssertEquals(false, header.CA_GenusOrSpeciesInfo.ReadOnly);

			header.CA_AISProgramInd = YesNoList.Codes.Yes;
			AssertEquals(false, header.CA_GenusOrSpeciesInfo.ReadOnly);

			header.CA_SpeciesCode = DFOScientificNames.Codes.FO11;
			AssertEquals(true, header.CA_GenusOrSpeciesInfo.ReadOnly);

			header.CA_SpeciesCode = "XXX0";
			AssertEquals(false, header.CA_GenusOrSpeciesInfo.ReadOnly);
		}

		public void TestSupportsNotes()
		{
			var bo = (DFOPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestLPCOViewCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "6008", "6008 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.DFO);
			newFactory.Save();

			AssertEquals("LpcoViews on DFO", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "6008";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on DFO", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "6008";
			AssertEquals("LpcoViews on DFO", 3, header.LPCOViews.Count);
		}

		public void TestNSNNumberReadOnly()
		{
			var pgaHeader = (DFOPGAHeader)GetNewBusinessObject();
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			var line = header.InvoiceLine;
			line.JI_Model = string.Empty;

			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_AISProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_TTPProgramInd = YesNoList.Codes.Yes;

			var lpcoType = "6000";
			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.CLP_Type = lpcoType;
			lpco.CLP_RefNo = "A0000";
			AssertEquals(false, pgaHeader.NSNNumberInfo.ReadOnly);

			lpco.ReadOnly = true;
			AssertEquals(true, pgaHeader.NSNNumberInfo.ReadOnly);
		}

		public void TestDeleteNSNNumber()
		{
			var pgaHeader = (DFOPGAHeader)GetNewBusinessObject();
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			var line = header.InvoiceLine;
			line.JI_Model = string.Empty;

			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_AISProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_TTPProgramInd = YesNoList.Codes.Yes;

			var lpcoType = "6000";
			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.CLP_Type = lpcoType;
			lpco.CLP_RefNo = "A0000";

			Factory.Save();

			AssertEquals("LPCOViews on DFO", 1, pgaHeader.LPCOViews.Count);
			AssertEquals("LPCOs on DFO", 1, pgaHeader.LPCOViews.Count);
			AssertEquals(false, lpco.IsDeleted);

			pgaHeader.NSNNumber = "123";
			pgaHeader.CA_GeneOrNucleotideSequence = "Test";
			pgaHeader.NSNNumber = ZString.Empty;
			AssertEquals("LPCOViews on DFO", 0, pgaHeader.LPCOViews.Count);
			AssertEquals("LPCOs on DFO", 0, pgaHeader.LPCOViews.Count);
			AssertEquals(true, lpco.IsDeleted);
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNULL()
		{
			AssertNoExceptionThrown(() =>
			{
				var pgaHeader = Factory.New<DFOPGAHeader>();
				_ = pgaHeader.OA_Manufacturer;
				pgaHeader.OA_Manufacturer = ZGuid.Empty;
				_ = pgaHeader.OA_ManufacturerInfo.SupportsMaxLength;
				_ = pgaHeader.OA_Manufacturer_ZAddress;
				_ = pgaHeader.RN_NKCountryOfOrigin;
				pgaHeader.RN_NKCountryOfOrigin = ZString.Empty;
				_ = pgaHeader.RN_NKCountryOfOriginInfo.SupportsMaxLength;
			});
		}

		#region Purge Values

		public void TestPurgeValues()
		{
			var header = (DFOPGAHeader)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertPurgeValue(header, header.CA_CategoryInfo, new ZString("TEST"), true, false, false);
				AssertPurgeValue(header, header.CA_CountInfo, new ZInt(100), true, true, true);
				AssertPurgeValue(header, header.CA_GeneOrNucleotideSequenceInfo, new ZString("TEST"), true, false, false);
				AssertPurgeValue(header, header.CA_GenusOrSpeciesInfo, new ZString("TEST"), true, true, true);
				AssertPurgeValue(header, header.CA_HasGeneticModificationInfo, ZBool.True, true, false, false);
				AssertPurgeValue(header, header.CA_GeneticModificationDescriptionInfo, new ZString("TEST"), true, false, false);

				AssertPurgeValue(header, header.CA_IUAInfo, ZBool.True, true, false, true);
				AssertPurgeValue(header, header.CA_IUEInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_IUEAInfo, ZBool.True, true, false, true);
				AssertPurgeValue(header, header.CA_IUFInfo, ZBool.True, true, false, true);
				AssertPurgeValue(header, header.CA_IUOInfo, ZBool.True, true, false, true);
				AssertPurgeValue(header, header.CA_IUOTHInfo, ZBool.True, true, false, true);
				AssertPurgeValue(header, header.CA_IURADInfo, ZBool.True, true, false, true);
				AssertPurgeValue(header, header.CA_IUSCPInfo, ZBool.True, true, false, true);
				AssertPurgeValue(header, header.CA_IUAISInfo, ZBool.True, false, true, false);
				AssertPurgeValue(header, header.CA_IUSInfo, ZBool.True, false, true, false);

				AssertPurgeValue(header, header.CA_LifeStageAdultInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_LifeStageEmbryoInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_LifeStageJuvenileInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_LifeStagePropagateInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_LifeStageDeadInfo, ZBool.True, false, true, true);
				AssertPurgeValue(header, header.CA_LifeStageLiveInfo, ZBool.True, false, true, false);

				AssertPurgeValue(header, header.CA_SexFemaleInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_SexHermaphroditeInfo, ZBool.True, true, false, true);
				AssertPurgeValue(header, header.CA_SexMaleInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_SexOtherInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_SexSterileInfo, ZBool.True, true, true, true);
				AssertPurgeValue(header, header.CA_SexUnknownInfo, ZBool.True, false, true, true);

				AssertPurgeValue(header, header.CA_TSNInfo, new ZString("TEST"), true, true, true);
				AssertPurgeValue(header, header.CA_DirectionInfo, new ZString("TEST"), false, true, false);
				AssertPurgeValue(header, header.CA_EvisceratedInfo, ZBool.True, false, true, true);

				AssertPurgeValue(header, header.CA_OA_ProcessorInfo, ZGuid.BrettsGuid, false, true, true);
				AssertPurgeValue(header, header.CA_OA_HarvestingPartyInfo, ZGuid.BrettsGuid, true, true, true);
				AssertPurgeValue(header, header.CA_SpeciesCodeInfo, new ZString("FO01"), false, true, false);
				AssertPurgeValue(header, header.CA_CommissionInfo, new ZString("TEST"), false, false, true);
				AssertPurgeValue(header, header.CA_CommonNameCodeInfo, new ZString("TEST"), false, false, true);
			});
		}

		public void TestPurgeValues_LPCOs()
		{
			var header = (DFOPGAHeader)GetNewBusinessObject();
			header.LPCOViews.RemoveAndDeleteAll();

			var line = header.InvoiceLine;
			line.JI_Model = string.Empty;

			header.CA_ABIProgramInd = YesNoList.Codes.Yes;
			header.CA_AISProgramInd = YesNoList.Codes.Yes;
			header.CA_TTPProgramInd = YesNoList.Codes.Yes;

			var lpco = header.LPCOViews.AddNew();

			header.CA_ABIProgramInd = YesNoList.Codes.No;

			AssertEquals("LPCOs is used on these three programs.", 1, header.LPCOViews.Count);
			AssertEquals("Should not delete as AIS and TTP are enable.", false, lpco.IsDeleted);

			header.CA_AISProgramInd = YesNoList.Codes.No;

			AssertEquals("LPCOs is used on these three programs.", 1, header.LPCOViews.Count);
			AssertEquals("Should not delete as TTP is enable.", false, lpco.IsDeleted);

			header.CA_TTPProgramInd = YesNoList.Codes.No;

			AssertEquals("LPCOs is used on these three programs.", 0, header.LPCOViews.Count);
			AssertEquals("Should delete as ABI, AIS and TTP are all disable.", true, lpco.IsDeleted);
		}

		void AssertPurgeValue(DFOPGAHeader header, ZPropertyInfo info, IZType value, bool useAbi, bool useAis, bool useTtp)
		{
			header.CA_ABIProgramInd = YesNoList.Codes.Yes;
			header.CA_AISProgramInd = YesNoList.Codes.Yes;
			header.CA_TTPProgramInd = YesNoList.Codes.Yes;

			info.Value = value;
			AssertEquals("Set Value", value, info.Value);

			var message = $"{info.Name} - ABI:{useAbi} AIS:{useAis} TTP:{useTtp}";

			if (useAbi && !useAis && !useTtp)
			{
				header.CA_AISProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_TTPProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_ABIProgramInd = YesNoList.Codes.No;
				AssertEquals(message, info.DefaultValue, info.Value);
			}

			if (!useAbi && useAis && !useTtp)
			{
				header.CA_ABIProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_TTPProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_AISProgramInd = YesNoList.Codes.No;
				AssertEquals(message, info.DefaultValue, info.Value);
			}

			if (!useAbi && !useAis && useTtp)
			{
				header.CA_ABIProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_AISProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_TTPProgramInd = YesNoList.Codes.No;
				AssertEquals(message, info.DefaultValue, info.Value);
			}

			if (useAbi && useAis && !useTtp)
			{
				header.CA_TTPProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_ABIProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_AISProgramInd = YesNoList.Codes.No;
				AssertEquals(message, info.DefaultValue, info.Value);
			}

			if (useAbi && !useAis && useTtp)
			{
				header.CA_AISProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_ABIProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_TTPProgramInd = YesNoList.Codes.No;
				AssertEquals(message, info.DefaultValue, info.Value);
			}

			if (!useAbi && useAis && useTtp)
			{
				header.CA_ABIProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_AISProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_TTPProgramInd = YesNoList.Codes.No;
				AssertEquals(message, info.DefaultValue, info.Value);
			}

			if (useAbi && useAis && useTtp)
			{
				header.CA_TTPProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_AISProgramInd = YesNoList.Codes.No;
				AssertEquals(message, value, info.Value);

				header.CA_ABIProgramInd = YesNoList.Codes.No;
				AssertEquals(message, info.DefaultValue, info.Value);
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = "Y";
			header = invoiceLine.DFOPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header;
		}
		DFOPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override IEnumerable<DFOPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = "Y";
			yield return invoiceLine.DFOPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_DFOIndicator = Customs.Business.YesNoList.Codes.Yes;
			yield return pivot.DFOPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = "Y";
			return invoiceLine.DFOPGAHeader;
		}
	}
}
