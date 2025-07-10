using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.ESConstants;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;
using ESCusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using EUCusContainer = Enterprise.Customs.EU.Business.Declaration.CusContainer;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	sealed class ESDocSADHExportTest : ESDocSADHTest
	{
		public void TestLines()
		{
			var wrapper = GetNewDocumentWrapper() as ESDocSADHExport;
			AssertEquals(typeof(ESDocSADHLineCollectionExport), wrapper.Lines.GetType());
		}

		public void TestPages()
		{
			var wrapper = GetNewDocumentWrapper() as ESDocSADHExport;
			AssertEquals(typeof(ESDocSADHPageCollectionExport), wrapper.Pages.GetType());
		}

		public void TestBox1bSubStyle()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);

				var entryHeader = declaration.CustomsEntryHeaders[0];

				var wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Expected filled Box1bSubStyle A when entry status not PDA and subStyle declared A", "A", wrapper.Box1bSubStyle);

				entryHeader.CH_EntryStatus = "PDA";
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Expected filled Box1bSubStyle D when entry status PDA and subStyle declared A", "D", wrapper.Box1bSubStyle);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Expected filled Box1bSubStyle E when entry status PDA and subStyle declared B", "E", wrapper.Box1bSubStyle);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Expected filled Box1bSubStyle F when entry status PDA and subStyle declared C", "F", wrapper.Box1bSubStyle);

				entryHeader.CH_EntryStatus = "CLR";
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Expected filled Box1bSubStyle C when entry status not PDA and subStyle declared C", "C", wrapper.Box1bSubStyle);
			});
		}

		public void TestBox18IdentityOfTransportAtDeparture_TransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			declaration.ZG_Box18TransportID = "RAIL 1234";
			declaration.JE_TransportIDInland = "Inland Transport";
			declaration.JE_Trailer1RegNo = "Wagon/Trailer1 Num";

			CombineAssertions(() =>
			{
				var wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When no transport mode declared, declaration.ZG_Box18TransportID", "RAIL 1234", wrapper.Box18IdentityOfTransportAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is ROA, declaration.JE_TransportIDInland when declared", "Inland Transport", wrapper.Box18IdentityOfTransportAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Rail;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is RAI, declaration.ZG_Box18TransportID when declared", "RAIL 1234", wrapper.Box18IdentityOfTransportAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
				declaration.JE_TransportIDInland = ZString.Empty;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is ROA, declaration.JE_Trailer1RegNo when JE_TransportIDInland is not declared", "Wagon/Trailer1 Num", wrapper.Box18IdentityOfTransportAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is SEA, declaration.ZG_Box18TransportID", "RAIL 1234", wrapper.Box18IdentityOfTransportAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Rail;
				declaration.ZG_Box18TransportID = ZString.Empty;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is RAI, declaration.JE_Trailer1RegNo when ZG_Box18TransportID is not declared", "Wagon/Trailer1 Num", wrapper.Box18IdentityOfTransportAtDeparture);
			});
		}

		public void TestBox18TransportNationalityAtDeparture_TransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			declaration.ZG_Box18TransportID = "RAIL 1234";
			declaration.ZG_Box18TransportNationality = "ES";

			declaration.JE_TransportIDInland = "Inland Transport";
			declaration.JE_RN_NKTransportNationalityInland = "FR";

			declaration.JE_Trailer1RegNo = "Wagon/Trailer1 Num";
			declaration.JE_RN_NKTrailer1Nationality = "IT";

			CombineAssertions(() =>
			{
				var wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When no transport mode declared, declaration.ZG_Box18TransportNationality", "ES", wrapper.Box18TransportNationalityAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is ROA, declaration.JE_RN_NKTransportNationalityInland when JE_TransportIDInland declared", "FR", wrapper.Box18TransportNationalityAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Rail;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is RAI, declaration.ZG_Box18TransportID when declared", "ES", wrapper.Box18TransportNationalityAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
				declaration.JE_TransportIDInland = ZString.Empty;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is ROA, declaration.JE_RN_NKTrailer1Nationality when JE_TransportIDInland is not declared", "IT", wrapper.Box18TransportNationalityAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is SEA, declaration.ZG_Box18TransportID", "ES", wrapper.Box18TransportNationalityAtDeparture);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Rail;
				declaration.ZG_Box18TransportID = ZString.Empty;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("When transport mode declared is RAI, declaration.JE_RN_NKTrailer1Nationality when ZG_Box18TransportID is not declared", "IT", wrapper.Box18TransportNationalityAtDeparture);
			});
		}

		public void TestBox20AgreedPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box20AgreedPlace empty when all incoterm places are empty and ZG_UCC6Version = 0", "", wrapper.Box20AgreedPlace);

				declaration.JE_ShipmentIncoTermPlace = "SYDNEY";
				AssertEquals("Box20AgreedPlace with declaration incoterm place when header one is empty and ZG_UCC6Version = 0", "SYDNEY", wrapper.Box20AgreedPlace);

				invoiceHeader.JZ_IncoTermPlace = "MADRID";
				AssertEquals("Box20AgreedPlace with header incoterm place when not empty and ZG_UCC6Version = 0", "MADRID", wrapper.Box20AgreedPlace);

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				AssertEquals("Box20AgreedPlace empty when all incoterm places and place codes are empty and ZG_UCC6Version = 0", "", wrapper.Box20AgreedPlace);

				declaration.JE_ShipmentIncoTermPlace = "SYDNEY";
				AssertEquals("Box20AgreedPlace with declaration incoterm place when header one is empty, incoterm place codes are empty and ZG_UCC6Version = 1", "SYDNEY", wrapper.Box20AgreedPlace);

				invoiceHeader.JZ_IncoTermPlace = "MADRID";
				AssertEquals("Box20AgreedPlace with header incoterm place when not empty, incoterm place codes are empty and ZG_UCC6Version = 1", "MADRID", wrapper.Box20AgreedPlace);

				declaration.ZG_AgreedPlaceCode = "AUSYD";
				AssertEquals("Box20AgreedPlace with declaration incoterm place code when header one is empty, length > 2 and ZG_UCC6Version = 1", "AUSYD", wrapper.Box20AgreedPlace);

				invoiceHeader.ZG_AgreedPlaceCode = "ESMAD";
				AssertEquals("Box20AgreedPlace with header incoterm place code when not empty, length > 2 and ZG_UCC6Version = 1", "ESMAD", wrapper.Box20AgreedPlace);

				invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
				invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
				declaration.ZG_AgreedPlaceCode = "AU";
				declaration.JE_ShipmentIncoTermPlace = "SYDNEY";
				AssertEquals("Box20AgreedPlace with declaration incoterm place when header one is empty, incoterm place codes length <= 2 and ZG_UCC6Version = 1", "SYDNEY", wrapper.Box20AgreedPlace);

				invoiceHeader.ZG_AgreedPlaceCode = "ES";
				invoiceHeader.JZ_IncoTermPlace = "MADRID";
				AssertEquals("Box20AgreedPlace with header incoterm place when not empty, incoterm place codes length <= 2 and ZG_UCC6Version = 1", "MADRID", wrapper.Box20AgreedPlace);
			});
		}

		public void TestBox20AgreedPlaceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				declaration.ZG_AgreedPlaceCode = "3";
				invoiceHeader.ZG_AgreedPlaceCode = "ES";
				AssertEquals("Box20AgreedPlaceCode gets code from declaration's agreed place when ZG_UCC6Version = 0", "3", wrapper.Box20AgreedPlaceCode);

				entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				AssertEquals("Box20AgreedPlaceCode gets code from invoice header's incoterm place when it's length is 2 and ZG_UCC6Version = 1", "ES", wrapper.Box20AgreedPlaceCode);

				invoiceHeader.ZG_AgreedPlaceCode = "ESMAD";
				AssertEquals("Box20AgreedPlaceCode gets code from invoice header's incoterm place when it's length is not 2 and ZG_UCC6Version = 1", ZString.Empty, wrapper.Box20AgreedPlaceCode);
			});
		}

		public void TestBoxDReleaseDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.CH_EntryReleaseDate = new ZDateTime(2020, 03, 12);

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDReleaseDate);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is notempty because there is MRN", "Levante: 12-03-2020", wrapper.BoxDReleaseDate);
			});
		}

		public void TestBoxDAcceptanceDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.MovementReferenceNumberIssueDate = new ZDateTime(2020, 03, 12);

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDAcceptanceDate);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is notempty because there is MRN", "Admitido: 12-03-2020", wrapper.BoxDAcceptanceDate);
			});
		}

		public void TestBoxDContainerSealsAffixedOnlyContainersWithSeals_ShouldShowText()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;

			EUCusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";
			container1.CO_Seal = "SEAL1";

			EUCusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_Seal = "SEAL2";
			container2.CO_ContainerNumber = "2";

			EUCusContainer container0 = declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "0";

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("1").IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("2").IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("0").IsForInvoiceLine = true;

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDContainerSealsAffixed);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is not empty because there is MRN", "Con precinto: 2; SEAL1; SEAL2", wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestBoxDContainerSealsAffixedOnlyContainersWithSecondSeals_ShouldShowText()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;

			EUCusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";
			container1.CO_SecondSeal = "SEAL1";

			EUCusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_SecondSeal = "SEAL2";
			container2.CO_ContainerNumber = "2";

			EUCusContainer container0 = declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "0";

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("1").IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("2").IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("0").IsForInvoiceLine = true;

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDContainerSealsAffixed);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is not empty because there is MRN", "Con precinto: 2; SEAL1; SEAL2", wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestBoxDContainerSealsAffixedOnlyContainersWithAdditionalSeals_ShouldShowText()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;

			EUCusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "SEAL1";

			EUCusContainer container2 = declaration.CusContainers.AddNew();
			container2.AdditionalSeals.AddNew().BK_SealNumber = "SEAL2";
			container2.CO_ContainerNumber = "2";

			EUCusContainer container0 = declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "0";

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("1").IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("2").IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("0").IsForInvoiceLine = true;

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDContainerSealsAffixed);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is not empty because there is MRN", "Con precinto: 2; SEAL1; SEAL2", wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestBoxDContainerSealsAffixedOnlyEquipmentsWithSeals_ShouldShowText()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "1";
			equipment1.Seals.AddNew().BK_SealNumber = "SEAL1";
			var pack1 = packingGroups.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.Seals.AddNew().BK_SealNumber = "SEAL2";
			equipment2.CEQ_IdentificationNumber = "2";
			var pack2 = packingGroups.Packages.AddNew();
			pack2.CW_PackQty = 1;
			pack2.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;

			var equipment0 = declaration.Equipments.AddNew();
			equipment0.CEQ_IdentificationNumber = "0";
			var pack0 = packingGroups.Packages.AddNew();
			pack0.CW_PackQty = 1;
			pack0.CW_ContainerNoOrEquipmentNo = equipment0.CEQ_IdentificationNumber;

			invoiceLine.PackagesPivot.AddPivotFor(pack1);
			invoiceLine.PackagesPivot.AddPivotFor(pack2);
			invoiceLine.PackagesPivot.AddPivotFor(pack0);

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDContainerSealsAffixed);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is not empty because there is MRN", "Con precinto: 2; SEAL1; SEAL2", wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestBoxDContainerSealsAffixed_ContainersAndEquipmentSeals()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = "A";
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = "B";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction2.PK;

			var entryHeader1 = (ESCusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			entryHeader1.MovementReferenceNumber = "MRN-TEST";
			var entryLine1 = entryHeader1.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryHeader2 = (ESCusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryHeader2.MovementReferenceNumber = "MRN-TEST";
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine2.PK;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CTN1";
			container1.CO_Seal = "S01";
			container1.CO_SecondSeal = "S21";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "A01";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "A02";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CTN2";
			container2.CO_Seal = "S02";
			container2.CO_SecondSeal = "S22";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "A03";

			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CTN3";
			container3.CO_Seal = "S03";
			container3.CO_SecondSeal = "S23";
			container3.AdditionalSeals.AddNew().BK_SealNumber = "A04";

			var container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CTN4";
			container4.CO_Seal = ZString.Empty;
			container4.CO_SecondSeal = ZString.Empty;
			container4.AdditionalSeals.AddNew().BK_SealNumber = "";

			invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN1").IsForInvoiceLine = true;
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN2").IsForInvoiceLine = true;
			invoiceLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN3").IsForInvoiceLine = true;
			invoiceLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN4").IsForInvoiceLine = true;

			var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "EQUIP1";
			equipment1.Seals.AddNew().BK_SealNumber = "E01";
			var pack1 = packingGroups.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "EQUIP2";
			equipment2.Seals.AddNew().BK_SealNumber = "E02";
			equipment2.Seals.AddNew().BK_SealNumber = "E03";
			var pack2 = packingGroups.Packages.AddNew();
			pack2.CW_PackQty = 1;
			pack2.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;

			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "EQUIP3";
			equipment3.Seals.AddNew().BK_SealNumber = "";
			var pack3 = packingGroups.Packages.AddNew();
			pack3.CW_PackQty = 1;
			pack3.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			invoiceLine1.PackagesPivot.AddPivotFor(pack1);
			invoiceLine2.PackagesPivot.AddPivotFor(pack2);
			invoiceLine2.PackagesPivot.AddPivotFor(pack3);

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader1, Factory);
				AssertEquals("Expected 8 SealCodes in entry header 1", "Con precinto: 8; S01; S02; S21; S22; A01; A02; A03; E01", wrapper.BoxDContainerSealsAffixed);
				wrapper = ESDocSADHExport.New(entryHeader2, Factory);
				AssertEquals("Expected 5 SealCodes in entry header 2", "Con precinto: 5; S03; S23; A04; E02; E03", wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestBoxDContainerSealsAffixedUCC6_InputInContainers_NoPrint()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;

			EUCusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";
			container1.CO_Seal = "SEAL1";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "SEAL4";

			EUCusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_Seal = "SEAL2";
			container2.CO_SecondSeal = "SEAL3";
			container2.CO_ContainerNumber = "2";

			EUCusContainer container0 = declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "0";

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("1").IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("2").IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("0").IsForInvoiceLine = true;

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDContainerSealsAffixed);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is not empty because there is MRN", ZString.Empty, wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestBoxDContainerSealsAffixedUCC6_InputInContainersAndEquipmentSeals_OnlyPrintEquipmentSeals()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = "A";
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = "B";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction2.PK;

			var entryHeader1 = (ESCusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			entryHeader1.MovementReferenceNumber = "MRN-TEST";
			entryHeader1.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			var entryLine1 = entryHeader1.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryHeader2 = (ESCusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryHeader2.MovementReferenceNumber = "MRN-TEST";
			entryHeader2.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine2.PK;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CTN1";
			container1.CO_Seal = "S01";
			container1.CO_SecondSeal = "S21";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "A01";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "A02";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CTN2";
			container2.CO_Seal = "S02";
			container2.CO_SecondSeal = "S22";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "A03";

			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CTN3";
			container3.CO_Seal = "S03";
			container3.CO_SecondSeal = "S23";
			container3.AdditionalSeals.AddNew().BK_SealNumber = "A04";

			var container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CTN4";
			container4.CO_Seal = ZString.Empty;
			container4.CO_SecondSeal = ZString.Empty;
			container4.AdditionalSeals.AddNew().BK_SealNumber = "";

			invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN1").IsForInvoiceLine = true;
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN2").IsForInvoiceLine = true;
			invoiceLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN3").IsForInvoiceLine = true;
			invoiceLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN4").IsForInvoiceLine = true;

			var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "EQUIP1";
			equipment1.Seals.AddNew().BK_SealNumber = "E01";
			var pack1 = packingGroups.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "EQUIP2";
			equipment2.Seals.AddNew().BK_SealNumber = "E02";
			equipment2.Seals.AddNew().BK_SealNumber = "E03";
			var pack2 = packingGroups.Packages.AddNew();
			pack2.CW_PackQty = 1;
			pack2.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;

			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "EQUIP3";
			equipment3.Seals.AddNew().BK_SealNumber = "";
			var pack3 = packingGroups.Packages.AddNew();
			pack3.CW_PackQty = 1;
			pack3.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			invoiceLine1.PackagesPivot.AddPivotFor(pack1);
			invoiceLine2.PackagesPivot.AddPivotFor(pack2);
			invoiceLine2.PackagesPivot.AddPivotFor(pack3);

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader1, Factory);
				AssertEquals("Expected 1 SealCodes in entry header 1", "E01", wrapper.BoxDContainerSealsAffixed);
				wrapper = ESDocSADHExport.New(entryHeader2, Factory);
				AssertEquals("Expected 2 SealCodes in entry header 2", "E02; E03", wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestBoxDContainerSealsAffixedUCC6_InputInEquipmentSeals_PrintEquipmentSeals()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = "A";
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = "B";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction2.PK;

			var entryHeader1 = (ESCusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			entryHeader1.MovementReferenceNumber = "MRN-TEST";
			entryHeader1.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			var entryLine1 = entryHeader1.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryHeader2 = (ESCusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryHeader2.MovementReferenceNumber = "MRN-TEST";
			entryHeader2.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine2.PK;

			var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "EQUIP1";
			equipment1.Seals.AddNew().BK_SealNumber = "E01";
			var pack1 = packingGroups.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "EQUIP2";
			equipment2.Seals.AddNew().BK_SealNumber = "E02";
			equipment2.Seals.AddNew().BK_SealNumber = "E03";
			var pack2 = packingGroups.Packages.AddNew();
			pack2.CW_PackQty = 1;
			pack2.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;

			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "EQUIP3";
			equipment3.Seals.AddNew().BK_SealNumber = "";
			var pack3 = packingGroups.Packages.AddNew();
			pack3.CW_PackQty = 1;
			pack3.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			invoiceLine1.PackagesPivot.AddPivotFor(pack1);
			invoiceLine2.PackagesPivot.AddPivotFor(pack2);
			invoiceLine2.PackagesPivot.AddPivotFor(pack3);

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader1, Factory);
				AssertEquals("Expected 1 SealCodes in entry header 1", "E01", wrapper.BoxDContainerSealsAffixed);
				wrapper = ESDocSADHExport.New(entryHeader2, Factory);
				AssertEquals("Expected 2 SealCodes in entry header 2", "E02; E03", wrapper.BoxDContainerSealsAffixed);
			});
		}

		public void TestBoxDClearance()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.SetCSVClearanceNum("AAAAAAAAAAA");

			CombineAssertions(() =>
			{
				var wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDClearance);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = ESDocSADHExport.New(entryHeader, Factory);
				AssertEquals("Box D is notempty because there is MRN", "C.S.V.: AAAAAAAAAAA", wrapper.BoxDClearance);
			});
		}

		protected override DocSADH GetNewDocSADH(ESCusEntryHeader entryHeader)
		{
			return ESDocSADHExport.New(entryHeader, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return ESDocSADHExport.New(entryHeader, Factory);
		}

		protected override ZString ExpectedBox20AgreedPlaceCode => "3";

		protected override ZString ExpectedBox20AgreedPlaceCode2 => ZString.Empty;

		protected override ZString GetDeclarationType => MessageTypeList.Codes.Export;
	}
}
