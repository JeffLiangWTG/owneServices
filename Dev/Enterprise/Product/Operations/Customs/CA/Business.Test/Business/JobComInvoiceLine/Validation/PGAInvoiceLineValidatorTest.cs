using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PGAInvoiceLineValidatorTest : BusinessObjectValidationTestCase
	{
		public void TestIsJI_OA_ManufacturerAddressPhoneAndEmailRequired()
		{
			hCHeader.CA_PESProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsJI_OA_ManufacturerAddressPhoneAndEmailRequired);
			hCHeader.CA_PESProgramInd = YesNoList.Codes.No;
			AssertEquals(false, pgaValidator.IsJI_OA_ManufacturerAddressPhoneAndEmailRequired);
			eCCCHeader.CA_WENProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsJI_OA_ManufacturerAddressPhoneAndEmailRequired);
			eCCCHeader.CA_WENProgramInd = YesNoList.Codes.No;
			AssertEquals(false, pgaValidator.IsJI_OA_ManufacturerAddressPhoneAndEmailRequired);
		}

		public void TestIsAddressPhoneOrEmailRecommended()
		{
			hCHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsAddressPhoneOrEmailRecommended);
			hCHeader.CA_CPRProgramInd = YesNoList.Codes.No;
			AssertEquals(false, pgaValidator.IsAddressPhoneOrEmailRecommended);
		}

		public void TestProperties()
		{
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.No;
			AssertEquals(false, pgaValidator.PGANRCan_IsEEFRequired);
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			nRCanHeader.CA_Category = NRCanIntendedUseCodes.Codes.NR02;
			AssertEquals(true, pgaValidator.PGANRCan_IsEEFRequired);

			AssertEquals(true, pgaValidator.PGANRCan_IsExplosives);
			AssertEquals(false, pgaValidator.PGANRCan_IsEXPRequired);

			nRCanHeader.CA_AuthorizedParty = LPCOHolderPartyTypeCodes.Codes.Importer;
			AssertEquals(true, pgaValidator.PGANRCan_IsEXPRequired);

			AssertEquals(true, pgaValidator.IsJI_ModelRequired);
			AssertEquals(true, pgaValidator.IsTradeNameRequired);
			AssertEquals(true, pgaValidator.IsAuthorizedPartyRequired);
			AssertEquals(true, pgaValidator.IsJI_BrandNameRequired);

			AssertEquals(false, pgaValidator.PGATC_IsVPR);
			AssertEquals(false, pgaValidator.PGAECCC_IsVehicleDetailsRequiredForXE01OrIsXE04);
			AssertEquals(false, pgaValidator.PGAHC_IsPesticides);

			tCHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			tCHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VVP;
			AssertEquals(true, pgaValidator.PGATC_IsVPR);

			eCCCHeader.CA_ProcessCode = ProcessCodes.Codes.XE04;
			AssertEquals(true, pgaValidator.PGAECCC_IsVehicleDetailsRequiredForXE01OrIsXE04);

			eCCCHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			eCCCHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eCCCHeader.CA_Incomplete = true;
			eCCCHeader.CA_VehicleClass = "EC05";
			AssertEquals(true, pgaValidator.PGAECCC_IsVehicleDetailsRequiredForXE01OrIsXE04);

			hCHeader.CA_PESProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.PGAHC_IsPesticides);
		}

		public void TestIsJI_ModelRequired()
		{
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			nRCanHeader.CA_Category = NRCanIntendedUseCodes.Codes.NR01;
			AssertEquals(false, pgaValidator.IsJI_ModelRequired);

			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			nRCanHeader.CA_Category = NRCanIntendedUseCodes.Codes.NR02;
			AssertEquals(true, pgaValidator.IsJI_ModelRequired);

			invoiceLine.CA_NRCanInd = YesNoList.Codes.No;
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			hCHeader.CA_REDProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsJI_ModelRequired);

			invoiceLine.CA_HCInd = YesNoList.Codes.No;
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_WENProgramInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_ComplianceDeclaration = true;
			AssertEquals(true, pgaValidator.IsJI_ModelRequired);

			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.No;
			hCHeader.CA_REDProgramInd = YesNoList.Codes.No;
			invoiceLine.ECCCPGAHeader.CA_WENProgramInd = YesNoList.Codes.No;
			invoiceLine.ECCCPGAHeader.CA_ComplianceDeclaration = false;

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			invoiceLine.ECCCPGAHeader.CA_Incomplete = true;
			invoiceLine.ECCCPGAHeader.CA_VehicleClass = "EC05";
			AssertEquals(true, pgaValidator.IsJI_ModelRequired);

			invoiceLine.ECCCPGAHeader.CA_VehicleClass = "";
			AssertEquals(false, pgaValidator.IsJI_ModelRequired);
		}

		public void TestIsCA_ModelYearRequired_ForECCC()
		{
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			invoiceLine.ECCCPGAHeader.CA_Incomplete = true;
			invoiceLine.ECCCPGAHeader.CA_VehicleClass = "EC05";
			AssertEquals(true, pgaValidator.IsCA_ModelYearRequired);

			invoiceLine.ECCCPGAHeader.CA_VehicleClass = "";
			AssertEquals(false, pgaValidator.IsCA_ModelYearRequired);
		}

		public void TestIsCA_VINNumberRequired_ForECCC()
		{
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			invoiceLine.ECCCPGAHeader.CA_Incomplete = true;
			invoiceLine.ECCCPGAHeader.CA_VehicleClass = "EC05";
			AssertEquals(true, pgaValidator.IsCA_VINNumberRequired);

			invoiceLine.ECCCPGAHeader.CA_VehicleClass = "";
			AssertEquals(false, pgaValidator.IsCA_VINNumberRequired);
		}

		public void TestIsTradeNameRequired()
		{
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsTradeNameRequired);
			nRCanHeader.CA_Category = NRCanIntendedUseCodes.Codes.NR02;
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.No;
			AssertEquals(false, pgaValidator.IsTradeNameRequired);
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsTradeNameRequired);

			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.No;
			nRCanHeader.CA_EXPProgramInd = YesNoList.Codes.Yes;
			AssertEquals(false, pgaValidator.IsTradeNameRequired);
			nRCanHeader.CA_AuthorizedParty = LPCOHolderPartyTypeCodes.Codes.Importer;

			invoiceLine.CA_NRCanInd = YesNoList.Codes.No;
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			invoiceLine.DFOPGAHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			invoiceLine.DFOPGAHeader.CA_HasGeneticModification = true;
			AssertEquals(true, pgaValidator.IsTradeNameRequired);

			invoiceLine.CA_DFOInd = YesNoList.Codes.No;
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_PESProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsTradeNameRequired);
		}

		public void TestIsAuthorizedPartyRequired()
		{
			nRCanHeader.CA_EXPProgramInd = YesNoList.Codes.Yes;
			AssertEquals(false, pgaValidator.IsAuthorizedPartyRequired);
			nRCanHeader.CA_AuthorizedParty = LPCOHolderPartyTypeCodes.Codes.Importer;
			AssertEquals(true, pgaValidator.IsAuthorizedPartyRequired);
		}

		public void TestIsJI_BrandNameRequired()
		{
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			nRCanHeader.CA_Category = NRCanIntendedUseCodes.Codes.NR01;
			AssertEquals(false, pgaValidator.IsJI_BrandNameRequired);

			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			nRCanHeader.CA_Category = NRCanIntendedUseCodes.Codes.NR02;
			AssertEquals(true, pgaValidator.IsJI_BrandNameRequired);

			invoiceLine.CA_NRCanInd = YesNoList.Codes.No;
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			tCHeader.CA_VPRProgramInd = "Y";
			tCHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFS;
			AssertEquals(true, pgaValidator.IsJI_BrandNameRequired);

			invoiceLine.CA_TCInd = YesNoList.Codes.No;
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			hCHeader.CA_MDEProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsJI_BrandNameRequired);

			hCHeader.CA_MDEProgramInd = YesNoList.Codes.No;
			hCHeader.CA_NHPProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsJI_BrandNameRequired);

			hCHeader.CA_NHPProgramInd = YesNoList.Codes.No;
			hCHeader.CA_VETProgramInd = YesNoList.Codes.Yes;
			AssertEquals(true, pgaValidator.IsJI_BrandNameRequired);

			tCHeader.CA_TPRProgramInd = YesNoList.Codes.No;
			tCHeader.CA_VPRProgramInd = YesNoList.Codes.No;
			hCHeader.CA_MDEProgramInd = YesNoList.Codes.No;
			hCHeader.CA_NHPProgramInd = YesNoList.Codes.No;
			hCHeader.CA_VETProgramInd = YesNoList.Codes.No;
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.No;

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			invoiceLine.ECCCPGAHeader.CA_Incomplete = true;
			invoiceLine.ECCCPGAHeader.CA_VehicleClass = "EC05";
			AssertEquals(true, pgaValidator.IsJI_BrandNameRequired);

			invoiceLine.ECCCPGAHeader.CA_VehicleClass = "";
			AssertEquals(false, pgaValidator.IsJI_BrandNameRequired);
		}

		public void TestIsJI_NetWeightAndUQRequiredByCNSC()
		{
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;

			invoiceLine.CNSCPGAHeader.CA_Category = CNSCCategories.Codes.RD;
			AssertEquals(true, pgaValidator.IsJI_NetWeightAndUQRequiredByCNSC);

			invoiceLine.CNSCPGAHeader.CA_Category = CNSCCategories.Codes.NS;
			AssertEquals(true, pgaValidator.IsJI_NetWeightAndUQRequiredByCNSC);

			invoiceLine.CNSCPGAHeader.CA_Category = CNSCCategories.Codes.CNS;
			AssertEquals(true, pgaValidator.IsJI_NetWeightAndUQRequiredByCNSC);

			invoiceLine.CNSCPGAHeader.CA_Category = CNSCCategories.Codes.NE;
			AssertEquals(false, pgaValidator.IsJI_NetWeightAndUQRequiredByCNSC);
		}

		public void TestIsJI_NetWeightAndUQRequiredByGAC()
		{
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var lpco = invoiceLine.GACPGAHeader.LPCOViews.AddNew();
			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;
			AssertEquals(true, pgaValidator.IsJI_NetWeightAndUQRequiredByGAC);

			invoiceLine.JI_CustomsUnitQty = UnitOfMeasureListForDLM.Codes.Kilogram;
			AssertEquals(false, pgaValidator.IsJI_NetWeightAndUQRequiredByGAC);

			invoiceLine.JI_CustomsUnitQty = UnitOfMeasureListForDLM.Codes.MetricTonne;
			AssertEquals(false, pgaValidator.IsJI_NetWeightAndUQRequiredByGAC);

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._3003;
			AssertEquals(false, pgaValidator.IsJI_NetWeightAndUQRequiredByGAC);
		}

		public void TestIsWMIOfManufacturerRequired()
		{
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			tCHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			Assert(!pgaValidator.IsWMIOfManufacturerRequired);

			tCHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.PIL;
			Assert(pgaValidator.IsWMIOfManufacturerRequired);

			tCHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VCC;
			Assert(!pgaValidator.IsWMIOfManufacturerRequired);
		}

		public void TestIsJI_InvoiceQuantityRequired()
		{
			gACHeader.CA_AllProgramInd = YesNoList.Codes.No;
			Assert(!pgaValidator.IsJI_InvoiceQuantityRequired);
			gACHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			Assert(pgaValidator.IsJI_InvoiceQuantityRequired);
		}

		public void TestIsJI_OA_ManufacturerAddressRequired_ForECC()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Null Pga header", false, pgaValidator.IsJI_OA_ManufacturerAddressRequired);
				invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
				eCCCHeader.CA_WRMProgramInd = YesNoList.Codes.No;
				AssertEquals("CA_WRMProgramInd No", false, pgaValidator.IsJI_OA_ManufacturerAddressRequired);
				eCCCHeader.CA_WRMProgramInd = YesNoList.Codes.Yes;
				AssertEquals("CA_WRMProgramInd Yes", true, pgaValidator.IsJI_OA_ManufacturerAddressRequired);
				eCCCHeader.CA_WRMProgramInd = YesNoList.Codes.No;
				eCCCHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
				AssertEquals("Process Code XE01", true, pgaValidator.IsJI_OA_ManufacturerAddressRequired);
				eCCCHeader.CA_ProcessCode = ProcessCodes.Codes.XE02;
				AssertEquals("Process Code XE02", false, pgaValidator.IsJI_OA_ManufacturerAddressRequired);
				eCCCHeader.CA_ProcessCode = ProcessCodes.Codes.XE04;
				AssertEquals("Process Code XE04", true, pgaValidator.IsJI_OA_ManufacturerAddressRequired);
			});
		}

		public void TestIsJI_OA_ManufacturerAddressRequired()
		{
			invoiceLine.CA_ECCCInd = YesNoList.Codes.No;
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			dFOHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			dFOHeader.CA_HasGeneticModification = true;
			Assert(pgaValidator.IsJI_OA_ManufacturerAddressRequired);

			invoiceLine.CA_DFOInd = YesNoList.Codes.No;
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;

			tCHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			Assert(pgaValidator.IsJI_OA_ManufacturerAddressRequired);

			tCHeader.CA_TPRProgramInd = YesNoList.Codes.No;
			tCHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			tCHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.PIL;

			Assert(pgaValidator.IsJI_OA_ManufacturerAddressRequired);

			invoiceLine.CA_TCInd = YesNoList.Codes.No;
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			hCHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;
			Assert(pgaValidator.IsJI_OA_ManufacturerAddressRequired);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "111";
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "Brand";
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;
			invoiceLine.NRCanPGAHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;
			invoiceLine.NRCanPGAHeader.CA_EXPProgramInd = YesNoList.Codes.Yes;
			invoiceLine.NRCanPGAHeader.CA_RDAProgramInd = YesNoList.Codes.Yes;

			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			nRCanHeader = invoiceLine.NRCanPGAHeader;
			dFOHeader = invoiceLine.DFOPGAHeader;
			hCHeader = invoiceLine.HCPGAHeader;
			gACHeader = invoiceLine.GACPGAHeader;
			tCHeader = invoiceLine.TCPGAHeader;
			eCCCHeader = invoiceLine.ECCCPGAHeader;
			pgaValidator = new PGAInvoiceLineValidator(invoiceLine);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		NRCanPGAHeader nRCanHeader;
		DFOPGAHeader dFOHeader;
		GACPGAHeader gACHeader;
		HCPGAHeader hCHeader;
		TCPGAHeader tCHeader;
		ECCCPGAHeader eCCCHeader;
		PGAInvoiceLineValidator pgaValidator;
	}
}
