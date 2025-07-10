using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeader))]
	public class CusTempStorageJobHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCorrelationID()
		{
			var jobHeader1 = CusTempStorageJobHeader.New(Factory);
			jobHeader1.FillWithValidTestData();

			var jobHeader2 = CusTempStorageJobHeader.New(Factory);
			jobHeader2.FillWithValidTestData();

			Factory.Save();

			Assert(jobHeader1.CorrelationID != jobHeader2.CorrelationID);
			AssertType<ZString>(jobHeader1.CorrelationID);
			AssertEquals("CusTempStorageJobHeader transaction ID should start from 0000000001", "0000000001", header.CorrelationID);
			AssertEquals("CusTempStorageJobHeader transaction ID can be auto-increased to 0000000002", "0000000002", jobHeader1.CorrelationID);
			AssertEquals("CusTempStorageJobHeader transaction ID can be auto-increased to 0000000003", "0000000003", jobHeader2.CorrelationID);
		}

		public void TestCorrelationIDPrefix()
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory);
			AssertEquals(ZString.Empty, jobHeader.CorrelationIDPrefix);
		}

		public void TestAllocateDDTNumber()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var authorisation = Factory.New<CusAuthorisationHeader>();
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			authorisation.CPH_Number = "TST_ATH_001";
			authorisation.CPH_OH_PermitHolder = customer.PK;
			authorisation.CPH_StartDate = ZDate.Today.AddMonths(-1);
			authorisation.CPH_EndDate = ZDate.Today.AddMonths(1);
			var rule = authorisation.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.USE;
			rule.CPR_ValueFrom = FRConstants.TemporaryStorage.AppCodeIST;

			var storageHeader = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			storageHeader.SJH_OH_Customer = customer.PK;

			var action = new AnonymousMethod(() => storageHeader.AllocateDDTNumber());

			storageHeader.SJH_CustomsProfile = "A001";
			AssertExceptionThrown<ZCannotSaveException>("Invalid customs profile.", "Please select one valid customs profile.", action);

			storageHeader.SJH_CustomsProfile = "TST_ATH_001";
			AssertExceptionThrown<ZCannotSaveException>("No DDT number range.", "There is no valid DDT number ranges defined.\r\nPlease go to Authorization > Number Ranges to create one.", action);

			var stmNums = authorisation.CustomsNumberProvider.CustomsNumbers.AddNew();
			stmNums.SN_Type = CusAuthorisationHeaderCustomsNumberRangeTypeList.Codes.TemporaryStorageInstallationDdtNumberFrance;
			stmNums.SN_FountainName = "JAC";
			stmNums.SN_MinimumValue = 1;
			stmNums.SN_Count = 100;
			stmNums.SN_Value = 12;
			Factory.Save();
			AssertNoExceptionThrown("DDT number allocated.", action);
			AssertEquals("JAC000012", storageHeader.DDTNumber);

			var storageHeader2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			storageHeader2.SJH_OH_Customer = customer.PK;
			storageHeader2.DDTNumber = "JAC000013";
			Factory.Save();

			AssertNoExceptionThrown("Allocate a new DDT number.", action);
			AssertEquals("Should Skip JAC000013 because it is used by another IST job.", "JAC000014", storageHeader.DDTNumber);
		}

		public void TestComplementaryJobIST()
		{
			var frc = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeFRC);
			frc.SJH_JobReference = "FRJ_FRC";

			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.SJH_JobReference = "FRJ_IST2";
			var cusEntryNum = CusEntryNumber.New(ist2, CusEntryNumberTypes.France.DDT, Core.Constants.CountryCodes.France);
			cusEntryNum.CE_EntryNum = "DDT_SAMPLE";

			var storageHeader = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			storageHeader.SJH_PreviousReferenceType = FRConstants.TemporaryStorage.AppCodeFRC;
			storageHeader.SJH_PreviousReferenceNumber = "FRJ_FRC";
			var complementaryIST = storageHeader.PreviousISTHeader;
			AssertNull("Cannot find complementary IST because the previous reference type is not IST", complementaryIST);

			storageHeader.SJH_PreviousReferenceType = FRConstants.TemporaryStorage.AppCodeIST;
			storageHeader.SJH_PreviousReferenceNumber = "FRJ_IST1";
			complementaryIST = storageHeader.PreviousISTHeader;
			AssertSame("Find complementary IST from the job reference.", ist1, complementaryIST);

			storageHeader.SJH_PreviousReferenceType = FRConstants.TemporaryStorage.AppCodeIST;
			storageHeader.SJH_PreviousReferenceNumber = "DDT_SAMPLE";
			complementaryIST = storageHeader.PreviousISTHeader;
			AssertSame("Find complementary IST from the DDT number", ist2, complementaryIST);
		}

		public void TestPackageCount()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			var line1 = storageHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 12;
			var line2 = storageHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 33;
			AssertEquals(45, ((IGuaranteeJobParent)storageHeader).PackageCount);
		}

		public void TestAmountToBeGuaranteedInDeclarationCurrency()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			var line1 = storageHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			var item1a = line1.CusTempStorageLineItems.AddNew();
			item1a.TSI_GuaranteedValue = 12m;
			var item1b = line1.CusTempStorageLineItems.AddNew();
			item1b.TSI_GuaranteedValue = 23m;
			var line2 = storageHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			var item2a = line2.CusTempStorageLineItems.AddNew();
			item2a.TSI_GuaranteedValue = 34m;
			AssertEquals(69m, ((IGuaranteeJobParent)storageHeader).AmountToBeGuaranteedInDeclarationCurrency);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CusTempStorageJobHeader.New(Factory);
		}

		public void TestReadOnly()
		{
			var header = CusTempStorageJobHeader.New(Factory);
			AssertEquals(false, header.ReadOnly);

			header.Logs.AddNew(Events.InStore);
			AssertEquals(false, header.ReadOnly);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("SJH_AppCode should be 'IST'", FRConstants.TemporaryStorage.AppCodeIST, header.SJH_AppCode);
		}

		public void TestLookups()
		{
			AssertType<CusTempStorageJobHeaderLookups>(header.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusTempStorageJobHeaderValidation>(header.Validation);
		}

		public void TestApplicationCode()
		{
			Assert(header.IsIST);

			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeFRC;
			Assert(header.IsFRC);

			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeLAD;
			Assert(header.IsLADT);
		}

		public void TestDocumentSupporter()
		{
			AssertType<CusTempStorageJobHeaderDocumentSupporter>(header.DocumentSupporter);
		}

		public void TestSJH_CPH_GuaranteeCustomAttribute()
		{
			var typeToCheck = header.GetType();
			AssertHasCustomAttribute<ListAttribute>(typeToCheck, CusTempStorageJobHeader.Schema.SJH_CPH_Guarantee, false, attrib => attrib.ListDataSourceMember == "Lookups.GuaranteeList");
		}

		public void TestTemporaryStorageHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			TemporaryStorageHeaderTest(declaration, Core.Constants.GenPivotTypes.CusStorageHeaderDeclaration, typeof(JobDeclaration), JobDeclarationSchema.Constants.Prefix);

			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			TemporaryStorageHeaderTest(shipment, Core.Constants.GenPivotTypes.CusStorageHeaderShipment, typeof(ForwardingShipment), "JS");

			var deltaT = Factory.New<NctsHeader>();
			deltaT.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			TemporaryStorageHeaderTest(deltaT, Core.Constants.GenPivotTypes.CusStorageHeaderNctsHeader, typeof(NctsHeader), "BH");
		}

		void TemporaryStorageHeaderTest(BusinessObject bizO, ZString relationType, Type typeToAssert, ZString tablePRefix)
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SetRelatedBusinessObject(bizO, relationType);
			Factory.Save();

			var reloadedStorageHeader = new BusinessObjectFactory().Load<CusTempStorageJobHeader>(header.PK);
			AssertNotNull(reloadedStorageHeader);
			AssertNotNull(reloadedStorageHeader.RelatedBusinessObject);
			AssertEquals(typeToAssert, reloadedStorageHeader.RelatedBusinessObject.GetType());

			var temporaryStorageHeaderPivot = reloadedStorageHeader.FindTemporaryStorageHeaderPivot();
			AssertEquals(bizO.PK, temporaryStorageHeaderPivot.XX_Relation2ID);
			AssertEquals(tablePRefix, temporaryStorageHeaderPivot.XX_Relation2TableCode);
			AssertEquals(relationType, temporaryStorageHeaderPivot.XX_RelationType);
		}

		public void TestInitialisefromNPBO()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TransportMode = "ROA",
				CustomerPK = new ZGuid("11111111-1111-1111-1111-111111111111"),
				PlaceOfLoading = "POL",
				DepartureDate = new ZDateTime(2021, 09, 29),
				ArrivalDate = new ZDateTime(2021, 09, 30),
				CustomsOfficeOfDestination = "FR111",
				CustomsOfficeOfEntry = "TR222",
				PresentationDate = new ZDateTime(2021, 10, 01),
				PreviousEntryType = "PET",
				PreviousEntryNumber = "333",
				PresenterPK = new ZGuid("22222222-2222-2222-2222-222222222222"),
				BorderTransportInfo = "BTI",
				Vessel = "Vessel",
				GuaranteePK = new ZGuid("33333333-3333-3333-3333-333333333333"),
				CustomsProfile = "11111111",
				TransportMeansDescription = "TMD"
			};

			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				OwnerReferenceType = "OR1",
				OwnerReferenceNumber = "2222222",
				LocationOfGoods = "FR222",
				GrossWeight = 10m,
				GrossWeightUQ = "KG",
				PackageQty = 20,
				GoodsDescription = "Description1",
				PackageType = "PK",
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});

			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				OwnerReferenceType = "OR2",
				OwnerReferenceNumber = "3333333",
				LocationOfGoods = "FR333",
				GrossWeight = 20m,
				GrossWeightUQ = "KG",
				PackageQty = 30,
				GoodsDescription = "Description2",
				PackageType = "1A",
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});

			npbo.TemporaryStorageLines[0].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				CommodityCode = "TT",
				OriginCountry = "FR",
				NetMass = 10m,
				NetMassUQ = "KG",
				GoodsValue = 20m,
				GuaranteedValue = 30m,
				Currency = "EUR"
			});

			npbo.TemporaryStorageLines[1].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				CommodityCode = "RR",
				OriginCountry = "US",
				NetMass = 20m,
				NetMassUQ = "KG",
				GoodsValue = 30m,
				GuaranteedValue = 40m,
				Currency = "EUR"
			});

			npbo.TemporaryStorageContainers.Add(new TemporaryStorageWrapperContainer
			{
				ContainerNumber = "CNT001",
				ContainerType = "TYP001"
			});
			npbo.TemporaryStorageContainers.Add(new TemporaryStorageWrapperContainer
			{
				ContainerNumber = "CNT002",
				ContainerType = "TYP002"
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			storageHeader.InitialisefromNPBO(npbo);

			AssertEquals("ROA", storageHeader.SJH_TransportMode);
			AssertEquals(new ZGuid("11111111-1111-1111-1111-111111111111"), storageHeader.SJH_OH_Customer);
			AssertEquals("POL", storageHeader.SJH_RL_NKLoading);
			AssertEquals(new ZDateTime(2021, 09, 29), storageHeader.SJH_DepartureDate);
			AssertEquals(new ZDateTime(2021, 09, 30), storageHeader.SJH_ArrivalDate);
			AssertEquals("FR111", storageHeader.SJH_CustomsOffice);
			AssertEquals("TR222", storageHeader.SJH_CustomsOfficeOfEntryIntoEU);
			AssertEquals(new ZDateTime(2021, 10, 01), storageHeader.SJH_PresentationDate);
			AssertEquals("PET", storageHeader.SJH_PreviousReferenceType);
			AssertEquals("333", storageHeader.SJH_PreviousReferenceNumber);
			AssertEquals(new ZGuid("22222222-2222-2222-2222-222222222222"), storageHeader.SJH_OA_Presenter);
			AssertEquals("TMD", storageHeader.SJH_TransportMeansDescription);
			AssertEquals("Vessel", storageHeader.SJH_TransportRegNo);
			AssertEquals(2, storageHeader.SJH_ContainerCount);
			AssertEquals(new ZGuid("33333333-3333-3333-3333-333333333333"), storageHeader.SJH_CPH_Guarantee);
			AssertEquals("11111111", storageHeader.SJH_CustomsProfile);

			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageContainers.Count);
			AssertEquals("CNT001", storageHeader.CusTempStorageDec.CusTempStorageContainers[0].CY_Data);
			AssertEquals("TYP001", storageHeader.CusTempStorageDec.CusTempStorageContainers[0].CY_Code);
			AssertEquals("CNT002", storageHeader.CusTempStorageDec.CusTempStorageContainers[1].CY_Data);
			AssertEquals("TYP002", storageHeader.CusTempStorageDec.CusTempStorageContainers[1].CY_Code);

			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals("OR1", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_OwnerReferenceType);
			AssertEquals("2222222", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_OwnerReferenceNumber);
			AssertEquals("FR222", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_LocationOfGoods);
			AssertEquals(10m, storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_GrossWeight);
			AssertEquals("KG", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_GrossWeightUQ);
			AssertEquals(20, storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_PackageQty);
			AssertEquals("Description1", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_GoodsDescription);
			AssertEquals("PK", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_PackageType);

			AssertEquals("OR2", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_OwnerReferenceType);
			AssertEquals("3333333", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_OwnerReferenceNumber);
			AssertEquals("FR333", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_LocationOfGoods);
			AssertEquals(20m, storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GrossWeight);
			AssertEquals("KG", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GrossWeightUQ);
			AssertEquals(30, storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_PackageQty);
			AssertEquals("Description2", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GoodsDescription);
			AssertEquals("1A", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_PackageType);

			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems.Count);
			AssertEquals("TT", storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_CommodityCode);
			AssertEquals("FR", storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_GoodsOrigin);
			AssertEquals(10m, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_NetWeight);
			AssertEquals("KG", storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_NetWeightUQ);
			AssertEquals(20m, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_GoodsValue);
			AssertEquals(30m, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_GuaranteedValue);
			AssertEquals("EUR", storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_RX_NKCurrency);

			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems.Count);
			AssertEquals("RR", storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_CommodityCode);
			AssertEquals("US", storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_GoodsOrigin);
			AssertEquals(20m, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_NetWeight);
			AssertEquals("KG", storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_NetWeightUQ);
			AssertEquals(30m, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_GoodsValue);
			AssertEquals(40m, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_GuaranteedValue);
			AssertEquals("EUR", storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_RX_NKCurrency);
		}

		public void TestInitialisefromNPBOSJH_PreviousReferenceType()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				PreviousEntryType = "PET"
			};
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals("PET", storageHeader.SJH_PreviousReferenceType);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals("PET", storageHeader.SJH_PreviousReferenceType);
		}

		public void TestInitialisefromNPBOSJH_PreviousReferenceNumber()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				PreviousEntryNumber = "PEN"
			};
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals("PEN", storageHeader.SJH_PreviousReferenceNumber);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals("PEN", storageHeader.SJH_PreviousReferenceNumber);
		}

		public void TestInitialisefromNPBOSJH_CustomsProfile()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				CustomsProfile = "CP"
			};
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals("CP", storageHeader.SJH_CustomsProfile);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals("CP", storageHeader.SJH_CustomsProfile);
		}

		public void TestInitialisefromNPBOSJH_OA_Presenter()
		{
			var presenterGuid = ZGuid.NewZGuid();
			var npbo = new TemporaryStorageWrapperHeader
			{
				PresenterPK = presenterGuid
			};
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(presenterGuid, storageHeader.SJH_OA_Presenter);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(presenterGuid, storageHeader.SJH_OA_Presenter);
		}

		public void TestInitialisefromNPBOSJH_TransportMeansDescription()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TransportMeansDescription = "TMD"
			};
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals("TMD", storageHeader.SJH_TransportMeansDescription);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals("TMD", storageHeader.SJH_TransportMeansDescription);
		}

		public void TestInitialisefromNPBOSJH_ContainerCount()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				ContainerCount = 2
			};
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.SJH_ContainerCount);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.SJH_ContainerCount);
		}

		public void TestInitialisefromNPBOSJH_CPH_Guarantee()
		{
			var guaranteeGuid = ZGuid.NewZGuid();
			var npbo = new TemporaryStorageWrapperHeader
			{
				GuaranteePK = guaranteeGuid
			};
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(guaranteeGuid, storageHeader.SJH_CPH_Guarantee);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(guaranteeGuid, storageHeader.SJH_CPH_Guarantee);
		}

		public void TestInitialisefromNPBOTSL_OwnerReferenceType()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				OwnerReferenceType = "RT1"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				OwnerReferenceType = "RT2"
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals("RT1", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_OwnerReferenceType);
			AssertEquals("RT2", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_OwnerReferenceType);
		}

		public void TestInitialisefromNPBOTSL_OwnerReferenceNumber()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				OwnerReferenceNumber = "ORN1"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				OwnerReferenceNumber = "ORN2"
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals("ORN1", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_OwnerReferenceNumber);
			AssertEquals("ORN2", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_OwnerReferenceNumber);
		}

		public void TestInitialisefromNPBOTSL_LocationOfGoods()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				LocationOfGoods = "LOG1"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				LocationOfGoods = "LOG2"
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals("LOG1", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_LocationOfGoods);
			AssertEquals("LOG2", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_LocationOfGoods);
		}

		public void TestInitialisefromNPBOTSL_GrossWeight()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				GrossWeight = 1.1d
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				GrossWeight = 2.2d
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertZDecimalEquals("", 1.1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_GrossWeight);
			AssertZDecimalEquals("", 2.2, storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GrossWeight);
		}

		public void TestInitialisefromNPBOTSL_GrossWeightUQ()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				GrossWeightUQ = "KG"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				GrossWeightUQ = "MG"
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals("KG", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_GrossWeightUQ);
			AssertEquals("MG", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GrossWeightUQ);
		}

		public void TestInitialisefromNPBOTSL_PackageQty()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				PackageQty = 11
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				PackageQty = 22
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals(11, storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_PackageQty);
			AssertEquals(22, storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_PackageQty);
		}

		public void TestInitialisefromNPBOTSL_PackageType()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				PackageType = "AA"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				PackageType = "BB"
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals("AA", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_PackageType);
			AssertEquals("BB", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_PackageType);
		}

		public void TestInitialisefromNPBOTSL_GoodsDescription()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				GoodsDescription = "ABCD"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				GoodsDescription = "WXYZ"
			});
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals("ABCD", storageHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_GoodsDescription);
			AssertEquals("WXYZ", storageHeader.CusTempStorageDec.CusTempStorageLines[1].TSL_GoodsDescription);
		}

		public void TestInitialisefromNPBOTSI_CommodityCode()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[0].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				CommodityCode = "111.111.111"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[1].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				CommodityCode = "222.222.222"
			});

			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems.Count);
			AssertEquals("111.111.111", storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_CommodityCode);
			AssertEquals("222.222.222", storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_CommodityCode);
		}

		public void TestInitialisefromNPBOTSI_NetWeight()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[0].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				NetMass = 1.1d
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[1].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				NetMass = 2.2d
			});

			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems.Count);
			AssertZDecimalEquals("", 1.1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_NetWeight);
			AssertZDecimalEquals("", 2.2, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_NetWeight);
		}

		public void TestInitialisefromNPBOTSI_NetWeightUQ()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[0].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				NetMassUQ = "KG"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[1].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				NetMassUQ = "MG"
			});

			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems.Count);
			AssertEquals("KG", storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_NetWeightUQ);
			AssertEquals("MG", storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_NetWeightUQ);
		}

		public void TestInitialisefromNPBOTSI_GoodsValue()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[0].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				GoodsValue = 1.1d
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[1].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				GoodsValue = 2.2d
			});

			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems.Count);
			AssertZDecimalEquals("", 1.1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_GoodsValue);
			AssertZDecimalEquals("", 2.2, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_GoodsValue);
		}

		public void TestInitialisefromNPBOTSI_GuaranteedValue()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[0].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				GuaranteedValue = 1.1d
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[1].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				GuaranteedValue = 2.2d
			});

			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems.Count);
			AssertZDecimalEquals("", 1.1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_GuaranteedValue);
			AssertZDecimalEquals("", 2.2, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_GuaranteedValue);
		}

		public void TestInitialisefromNPBOTSI_RX_NKCurrency()
		{
			var npbo = new TemporaryStorageWrapperHeader
			{
				TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory)
			};
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[0].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				Currency = "EUR"
			});
			npbo.TemporaryStorageLines.Add(new TemporaryStorageWrapperLine
			{
				TemporaryStorageFurtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(Factory)
			});
			npbo.TemporaryStorageLines[1].TemporaryStorageFurtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				Currency = "GBP"
			});

			var storageHeader = CusTempStorageJobHeader.New(Factory);
			AssertNotEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.CusTempStorageLines.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems.Count);
			AssertEquals(1, storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems.Count);
			AssertEquals("EUR", storageHeader.CusTempStorageDec.CusTempStorageLines[0].CusTempStorageLineItems[0].TSI_RX_NKCurrency);
			AssertEquals("GBP", storageHeader.CusTempStorageDec.CusTempStorageLines[1].CusTempStorageLineItems[0].TSI_RX_NKCurrency);
		}

		public void TestInitialisefromNPBOShouldImportSupportingDocuments()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var supportingDoc1 = declaration.SupportingDocuments.AddNew();
			supportingDoc1.CSI_Code = "CSI01";
			supportingDoc1.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument;
			supportingDoc1.CSI_DateOfIssue = ZDateTime.Today;
			supportingDoc1.CSI_RN_NKCountryCode = "CN";
			supportingDoc1.CSI_SubType = "SUBTP";
			supportingDoc1.CSI_Status = "STA";
			supportingDoc1.CSI_Quantity = 100;
			supportingDoc1.CSI_UnitOfQuantity = "UNT1";
			supportingDoc1.CSI_Quantity2 = 111;
			supportingDoc1.CSI_UnitOfQuantity2 = "UNT2";
			supportingDoc1.CSI_Procedure = "PRO";
			supportingDoc1.CSI_CustomsOffice = "CO";
			supportingDoc1.CSI_LineNo = 1;
			supportingDoc1.CSI_Tariff = "Tariff";
			supportingDoc1.CSI_DateOfExpiry = ZDateTime.Today;
			supportingDoc1.CSI_Quantity3 = 222;
			supportingDoc1.CSI_UnitOfQuantity3 = "UNT3";
			supportingDoc1.CSI_Value = 444;
			supportingDoc1.CSI_RX_NKCurrency = "CUR";
			supportingDoc1.CSI_ReferenceNumber = "REF01";
			supportingDoc1.CSI_ReferenceNumber2 = "REF02";
			supportingDoc1.CSI_Description = "Desc";
			supportingDoc1.CSI_AdditionalDescription = "AddDesc";
			supportingDoc1.CSI_IssuerType = "ITP";
			supportingDoc1.CSI_ItemNumber = 11;
			declaration.SupportingDocuments.AddNew().CSI_Code = "CSI02";

			var npbo = TemporaryStorageWrapperHeader.NewFromDeclaration(declaration);
			storageHeader.InitialisefromNPBO(npbo);
			AssertEquals(2, storageHeader.CusTempStorageDec.SupportingDocuments.Count);
			AssertNotEquals(supportingDoc1.PK, storageHeader.CusTempStorageDec.SupportingDocuments[0].PK);
			AssertEquals(storageHeader.CusTempStorageDec.PK, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_ParentID);
			AssertEquals(storageHeader.CusTempStorageDec.TablePrefix, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_ParentTableCode);
			AssertEquals("CSI01", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Code);
			AssertEquals("SUP", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Type);
			AssertEquals(ZDateTime.Today, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_DateOfIssue);
			AssertEquals("CN", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_RN_NKCountryCode);
			AssertEquals("SUBTP", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_SubType);
			AssertEquals("STA", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Status);
			AssertEquals(100m, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Quantity);
			AssertEquals("UNT1", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_UnitOfQuantity);
			AssertEquals(111m, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Quantity2);
			AssertEquals("UNT2", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_UnitOfQuantity2);
			AssertEquals("PRO", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Procedure);
			AssertEquals("CO", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_CustomsOffice);
			AssertEquals(1, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_LineNo);
			AssertEquals("Tariff", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Tariff);
			AssertEquals(ZDateTime.Today, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_DateOfExpiry);
			AssertEquals(222m, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Quantity3);
			AssertEquals("UNT3", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_UnitOfQuantity3);
			AssertEquals(444m, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Value);
			AssertEquals("CUR", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_RX_NKCurrency);
			AssertEquals("REF01", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_ReferenceNumber);
			AssertEquals("REF02", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_ReferenceNumber2);
			AssertEquals("Desc", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_Description);
			AssertEquals("AddDesc", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_AdditionalDescription);
			AssertEquals("ITP", storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_IssuerType);
			AssertEquals(11, storageHeader.CusTempStorageDec.SupportingDocuments[0].CSI_ItemNumber);
			AssertEquals("CSI02", storageHeader.CusTempStorageDec.SupportingDocuments[1].CSI_Code);
		}

		public void TestSJH_JobReference()
		{
			var header1 = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			AssertNotNullOrEmpty(header1.SJH_JobReference);

			var header2 = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			AssertNotNullOrEmpty(header2.SJH_JobReference);

			AssertNotEquals(header2.SJH_JobReference, header1.SJH_JobReference);
		}

		public void TestDDTNumber()
		{
			var storageHeader1 = CusTempStorageJobHeader.New(Factory);
			var storageHeader2 = CusTempStorageJobHeader.New(Factory);
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			storageHeader1.SJH_OH_Customer = orgHeader1.PK;
			storageHeader2.SJH_OH_Customer = orgHeader2.PK;
			Assert(storageHeader1.DDTNumber == storageHeader2.DDTNumber);
			AssertType<ZString>(storageHeader1.DDTNumber);
			storageHeader1.DDTNumber = "YKK";
			AssertEquals("YKK", storageHeader1.DDTNumber);
			Factory.Save();
			var cehReloaded = new BusinessObjectFactory().Load<CusTempStorageJobHeader>(storageHeader1.PK);
			AssertEquals("YKK", cehReloaded.DDTNumber);
			AssertEquals(35, storageHeader1.DDTNumberInfo.MaxLength);
		}

		public void TestDDTEntryNumber()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			var ddtEntryNumber = storageHeader.DDTEntryNumber;
			AssertSame("DDTEntryNumber should be cached.", ddtEntryNumber, storageHeader.DDTEntryNumber);
			AssertEquals(CusEntryNumber.Categories.CustomsPermitClearanceNumber, ddtEntryNumber.CE_Category);
			AssertEquals(CusEntryNumberTypes.France.DDT, ddtEntryNumber.CE_EntryType);
			AssertEquals(Core.Constants.CountryCodes.France, ddtEntryNumber.CE_RN_NKCountryCode);
		}

		public void TestFetchFetchStrategy()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			var fetchStrategy = storageHeader.FetchStrategy;
			AssertNotNull("Has FetchStrategy", fetchStrategy);
			AssertType("Has FetchStrategy", typeof(CusTempStorageJobHeaderFetchStrategy), fetchStrategy);
		}

		public void TestDDTNumber_ShouldStoreInEntryNumber()
		{
			var storageHeader = CusTempStorageJobHeader.New(Factory);
			storageHeader.DDTNumber = "DDT001";
			AssertEquals("DDT001", storageHeader.DDTEntryNumber.CE_EntryNum);

			storageHeader.DDTEntryNumber.CE_EntryNum = "DDT004";
			AssertEquals("DDT004", storageHeader.DDTNumber);
		}

		#region SJH_TempStorageEndDateUtc
		[TestDate(2020, 08, 04, 14, 17, 33)]
		public void TestSJH_TempStorageEndDateUtc()
		{
			var typeToCheck = header.GetType();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeLAD;

			CombineAssertions("End Date calculated value depends on SJH_AppCode. If LADT, it also depends on Customer authorization type or is fixed to now + 90 days in any other cases.", () =>
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "TESTLAD";
				CreateAuthorisation(orgHeader.PK, "110001", "2");
				CreateAuthorisation(orgHeader.PK, "210001", ZString.Empty);
				CreateAuthorisation(orgHeader.PK, "310001", "6");
				CreateAuthorisation(orgHeader.PK, "410001", "7");
				Factory.Save();

				AssertTempStorageEndDateUtcDefaultValueFrommSTO(ZGuid.Empty, "TST", "110001", ZDateTime.Now.AddDays(90));
				AssertTempStorageEndDateUtcDefaultValueFrommSTO(ZGuid.Empty, "LAD", "110001", ZDateTime.Now.AddDays(3));
				AssertTempStorageEndDateUtcDefaultValueFrommSTO(orgHeader.PK, "LAD", "110001", ZDateTime.Now.AddDays(2));
				AssertTempStorageEndDateUtcDefaultValueFrommSTO(orgHeader.PK, "LAD", "210001", ZDateTime.Now.AddDays(3));
				AssertTempStorageEndDateUtcDefaultValueFrommSTO(orgHeader.PK, "LAD", "310001", ZDateTime.Now.AddDays(6));
				AssertTempStorageEndDateUtcDefaultValueFrommSTO(orgHeader.PK, "LAD", "410001", ZDateTime.Now.AddDays(6));
			});
		}

		void AssertTempStorageEndDateUtcDefaultValueFrommSTO(ZGuid orgHeaderPk, ZString appCode, ZString customsProfile, ZDateTime expect)
		{
			header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = appCode;
			header.SJH_OH_Customer = orgHeaderPk;
			header.SJH_CustomsProfile = customsProfile;
			header.SetTempStorageEndDateUtcDefaultValue();
			AssertEquals(expect, header.SJH_TempStorageEndDateUtc);
		}

		void CreateAuthorisation(ZGuid orgHeaderPK, ZString number, ZString daysForSTO)
		{
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_OH_PermitHolder = orgHeaderPK;
			header.CPH_Number = number;
			header.CPH_Type = "TST";
			var rule = header.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "LAD";
			rule = header.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "IST";
			if (!daysForSTO.IsEmpty)
			{
				rule = header.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = "STO";
				rule.CPR_ValueFrom = daysForSTO;
			}
		}

		public void TestTempStorageEndDateUtcReadOnly()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestEND";
			header.SJH_OH_Customer = orgHeader.PK;
			header.SJH_TempStorageEndDateUtc = ZDateTime.Empty;
			var info = header.SJH_TempStorageEndDateUtcInfo;
			AssertEquals(ZBool.False, info.ReadOnly);
			Factory.Save();
			AssertEquals(ZBool.False, info.ReadOnly);

			header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_OH_Customer = orgHeader.PK;
			info = header.SJH_TempStorageEndDateUtcInfo;
			header.SJH_TempStorageEndDateUtc = ZDateTime.Today;
			AssertEquals(ZBool.False, info.ReadOnly);
			Factory.Save();
			AssertEquals(ZBool.True, info.ReadOnly);
		}

		public void TestShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty()
		{
			header.SJH_TempStorageEndDateUtc = ZDateTime.Empty;
			AssertEquals(ZBool.False, header.ShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty);
			header.SJH_TempStorageEndDateUtc = ZDateTime.Now;
			AssertEquals(ZBool.True, header.ShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty);
		}
		#endregion

		#region SJH_CustomsProfile
		public void TestSJH_CustomsProfile()
		{
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTLAD2";
			header.SJH_OH_Customer = orgHeader2.PK;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTLAD";
			CreateAuthorisation(orgHeader.PK, "110001", "");
			Factory.Save();
			AssertCustomsProfileDefaultValue(orgHeader.PK, "LAD", "110001");
			AssertCustomsProfileDefaultValue(orgHeader.PK, "IST", "110001");

			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTLAD3";
			CreateAuthorisation(orgHeader.PK, "220001", "");
			CreateAuthorisation(orgHeader.PK, "220002", "");
			Factory.Save();
			AssertCustomsProfileDefaultValue(orgHeader.PK, "IST", ZString.Empty);
			AssertCustomsProfileDefaultValue(orgHeader.PK, "IST", ZString.Empty);
		}

		void AssertCustomsProfileDefaultValue(ZGuid orgHeaderPk, ZString appCode, ZString expect)
		{
			header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = appCode;
			header.SJH_OH_Customer = orgHeaderPk;
			AssertEquals(expect, header.SJH_CustomsProfile);
		}
		#endregion

		public void TestRegisterHeader()
		{
			var regheader1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regheader1.SRH_InternalReference = "FRJ00480021";
			var regheader2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regheader2.SRH_Reference = "DDT123456";
			Factory.Save();

			var header = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			header.SJH_JobReference = "FRJ00480021";
			AssertEquals("Load Register Header by JobReference", regheader1.PK, header.RegisterHeader.PK);
			header.SJH_JobReference = "";
			header.DDTNumber = "DDT123456";
			AssertEquals("Load Register Header by DDTNumber", regheader2.PK, header.RegisterHeader.PK);
		}

		[ExpectNoExceptions]
		public void TestGoodsDescription_NewLinesFromDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line = declaration.InvoiceLines.AddNew();
			line.JI_Description = new ZString('A', line.JI_DescriptionInfo.MaxLength);

			var npbo = TemporaryStorageWrapperHeader.NewFromDeclaration(declaration);
			AssertEquals(line.JI_Description.SubstringSafe(0, TemporaryStorageWrapperLine.Schema.GoodsDescriptionMaxLength), npbo.TemporaryStorageLines[0].GoodsDescription);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
		}
		CusTempStorageJobHeader header;

		#endregion
	}
}
