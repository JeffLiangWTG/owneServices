using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class PGAInvoiceLineValidator
	{
		public PGAInvoiceLineValidator(JobComInvoiceLine parent)
		{
			invoiceLine = Argument.NotNull(parent, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		#region IsJI_ModelRequired

		public bool IsJI_ModelRequired
		{
			get
			{
				return PGATC_IsVPR || IsDFOPGAHeaderRequiredModel || IsECCCPGAHeaderRequiredModel || IsHCPGAHeaderRequiredModel || PGANRCan_IsEEFRequired;
			}
		}

		bool IsDFOPGAHeaderRequiredModel
		{
			get
			{
				var dfoPGAHeader = DFOPGAHeader;
				return dfoPGAHeader != null &&
					(dfoPGAHeader.CA_ABIProgramInd == YesNoList.Codes.Yes && dfoPGAHeader.CA_HasGeneticModification
					|| dfoPGAHeader.CA_AISProgramInd == YesNoList.Codes.Yes
					|| dfoPGAHeader.CA_TTPProgramInd == YesNoList.Codes.Yes);
			}
		}

		bool IsECCCPGAHeaderRequiredModel
		{
			get
			{
				var ecccPGAHeader = ECCCPGAHeader;
				return ecccPGAHeader != null
					&& (ecccPGAHeader.CA_WENProgramInd == YesNoList.Codes.Yes && ecccPGAHeader.CA_ComplianceDeclaration
					|| ecccPGAHeader.IsVehicleDetailsRequiredForXE01
					|| ecccPGAHeader.IsXE04Process);
			}
		}

		bool IsHCPGAHeaderRequiredModel
		{
			get
			{
				var hCPGAHeader = HCPGAHeader;
				return hCPGAHeader != null && hCPGAHeader.CA_REDProgramInd == YesNoList.Codes.Yes;
			}
		}

		#endregion

		#region IsTradeNameRequired

		public bool IsTradeNameRequired
		{
			get
			{
				return IsDFOPGAHeaderRequiredTradeName || PGAHC_IsPesticides || PGANRCan_IsEEFRequired;
			}
		}

		bool IsDFOPGAHeaderRequiredTradeName
		{
			get
			{
				var dfoPGAHeader = DFOPGAHeader;
				return dfoPGAHeader != null && dfoPGAHeader.CA_ABIProgramInd == YesNoList.Codes.Yes && dfoPGAHeader.CA_HasGeneticModification;
			}
		}

		#endregion

		#region IsAuthorizedPartyRequired

		public bool IsAuthorizedPartyRequired
		{
			get => PGANRCan_IsEXPRequired;
		}

		#endregion

		#region IsJI_BrandNameRequired

		public bool IsJI_BrandNameRequired
		{
			get
			{
				return IsTCPGAHeaderRequiredBrandName || IsHCPGAHeaderRequiredBrandName || PGAECCC_IsVehicleDetailsRequiredForXE01OrIsXE04 || PGANRCan_IsEEFRequired;
			}
		}

		bool IsTCPGAHeaderRequiredBrandName
		{
			get
			{
				var tcPGAHeader = TCPGAHeader;
				return tcPGAHeader != null
					&& (tcPGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes
						|| tcPGAHeader.IsVPR
						&& tcPGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.PIG);
			}
		}

		bool IsHCPGAHeaderRequiredBrandName
		{
			get
			{
				var hcPGAHeader = HCPGAHeader;
				return hcPGAHeader != null
					&& (hcPGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.MDE)
						|| hcPGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.NHP)
						|| hcPGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.VET));
			}
		}

		#endregion

		#region IsJI_NetWeightAndUQRequiredByCNSC

		public bool IsJI_NetWeightAndUQRequiredByCNSC
		{
			get
			{
				var cnscPGAHeader = CNSCPGAHeader;
				return cnscPGAHeader != null
					&& (cnscPGAHeader.CA_Category == CNSCCategories.Codes.RD
						|| cnscPGAHeader.CA_Category == CNSCCategories.Codes.NS
						|| cnscPGAHeader.CA_Category == CNSCCategories.Codes.CNS);
			}
		}

		#endregion

		#region IsJI_NetWeightAndUQRequiredByGAC

		public bool IsJI_NetWeightAndUQRequiredByGAC
		{
			get
			{
				var gacPGAHeader = GACPGAHeader;
				var unitQty = invoiceLine.JI_CustomsUnitQty;
				return gacPGAHeader != null
						&& unitQty != UnitOfMeasureListForDLM.Codes.Kilogram
						&& unitQty != UnitOfMeasureListForDLM.Codes.MetricTonne
						&& gacPGAHeader.LPCOViews.Cast<LPCOView>().Any(x => x.CLP_Type == LPCODocumentTypeQualifier.Codes._2006);
			}
		}

		#endregion

		#region IsJI_InvoiceQuantityRequired

		public bool IsJI_InvoiceQuantityRequired
		{
			get { return GACPGAHeader.IsProgramEnabled(GACPGADepartmentCodes.Codes.ALL); }
		}

		#endregion

		#region IsWMIOfManufacturerRequired

		public bool IsWMIOfManufacturerRequired
		{
			get
			{
				var tcHeader = TCPGAHeader;
				return tcHeader != null && tcHeader.CA_VPRProgramInd == YesNoList.Codes.Yes && tcHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.PIL;
			}
		}

		#endregion

		#region IsJI_OA_ManufacturerAddressRequired

		public bool IsJI_OA_ManufacturerAddressRequired => IsJI_OA_ManufacturerAddressRequiredForECCC
														|| IsJI_OA_ManufacturerAddressRequiredForTC
														|| IsJI_OA_ManufacturerAddressRequiredForDFO;

		#endregion

		#region IsJI_OA_ManufacturerAddressPhoneAndEmailRequired

		public bool IsJI_OA_ManufacturerAddressPhoneAndEmailRequired => IsJI_OA_ManufacturerAddressPhoneAndEmailRequiredForECCC || IsJI_OA_ManufacturerAddressPhoneAndEmailRequiredForHC;

		#endregion

		#region IsAddressPhoneOrEmailRecommended

		public bool IsAddressPhoneOrEmailRecommended
		{
			get
			{
				var hcHeader = HCPGAHeader;
				return hcHeader != null && hcHeader.CA_CPRProgramInd == YesNoList.Codes.Yes;
			}
		}

		#endregion

		#region IsJI_OA_ManufacturerAddressPhoneAndEmailRequiredForHC

		bool IsJI_OA_ManufacturerAddressPhoneAndEmailRequiredForHC
		{
			get
			{
				return PGAHC_IsPesticides;
			}
		}

		#endregion

		#region IsJI_OA_ManufacturerAddressPhoneAndEmailRequiredForECCC

		bool IsJI_OA_ManufacturerAddressPhoneAndEmailRequiredForECCC
		{
			get
			{
				var ecccHeader = ECCCPGAHeader;
				return ecccHeader != null && ecccHeader.CA_WENProgramInd == YesNoList.Codes.Yes;
			}
		}

		#endregion

		#region IsJI_OA_ManufacturerAddressRequiredForECCC

		bool IsJI_OA_ManufacturerAddressRequiredForECCC
		{
			get
			{
				var ecccHeader = ECCCPGAHeader;
				return ecccHeader != null && (ecccHeader.CA_WRMProgramInd == YesNoList.Codes.Yes || ecccHeader.VehicleGroupBoxVisible);
			}
		}

		#endregion

		#region IsJI_OA_ManufacturerAddressRequiredForTC

		bool IsJI_OA_ManufacturerAddressRequiredForTC
		{
			get
			{
				var tcHeader = TCPGAHeader;
				return tcHeader != null
					&& (tcHeader.CA_VPRProgramInd == YesNoList.Codes.Yes && tcHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.PIL
						|| tcHeader.CA_TPRProgramInd == YesNoList.Codes.Yes);
			}
		}

		#endregion

		#region IsJI_OA_ManufacturerAddressRequiredForDFO

		bool IsJI_OA_ManufacturerAddressRequiredForDFO
		{
			get
			{
				var dfoHeader = DFOPGAHeader;
				return dfoHeader != null && dfoHeader.IsProgramEnabled(DFOPGADepartmentCodes.Codes.ABI) && dfoHeader.CA_HasGeneticModification;
			}
		}

		#endregion

		#region IsCA_ModelYearRequired

		public bool IsCA_ModelYearRequired => IsCA_ModelYearRequiredForTC || IsCA_ModelYearRequiredForECCC;

		#endregion

		#region IsCA_ModelYearRequiredForTC

		bool IsCA_ModelYearRequiredForTC
		{
			get
			{
				return PGATC_IsVPR && TCPGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.VUV;
			}
		}

		#endregion

		#region IsCA_ModelYearRequiredForECCC

		bool IsCA_ModelYearRequiredForECCC => PGAECCC_IsVehicleDetailsRequiredForXE01OrIsXE04;

		#endregion

		#region IsValidCA_ModelYearRequired

		public bool IsValidCA_ModelYearRequired => PGATC_IsVPR || IsValidCA_ModelYearRequiredForECC;

		bool IsValidCA_ModelYearRequiredForECC => ECCCPGAHeader != null;

		#endregion

		#region IsCA_VINNumberRequired

		public bool IsCA_VINNumberRequired
		{
			get
			{
				return PGATC_IsVPR || IsECCCPGAHeaderRequiredVINNumber;
			}
		}

		bool IsECCCPGAHeaderRequiredVINNumber
		{
			get
			{
				var ecccHeader = ECCCPGAHeader;
				return ecccHeader != null && ecccHeader.IsVehicleDetailsRequiredForXE01;
			}
		}

		#endregion

		#region PGA Headers

		NRCanPGAHeader NRCanHeader
		{
			get => invoiceLine.NRCanPGAHeader;
		}

		TCPGAHeader TCPGAHeader
		{
			get => invoiceLine.TCPGAHeader;
		}

		HCPGAHeader HCPGAHeader
		{
			get => invoiceLine.HCPGAHeader;
		}

		DFOPGAHeader DFOPGAHeader
		{
			get => invoiceLine.DFOPGAHeader;
		}

		ECCCPGAHeader ECCCPGAHeader
		{
			get => invoiceLine.ECCCPGAHeader;
		}

		CNSCPGAHeader CNSCPGAHeader
		{
			get => invoiceLine.CNSCPGAHeader;
		}

		GACPGAHeader GACPGAHeader
		{
			get => invoiceLine.GACPGAHeader;
		}

		#endregion

		#region Common

		public bool PGANRCan_IsEEFRequired
		{
			get
			{
				var nrCanHeader = NRCanHeader;
				return nrCanHeader != null && nrCanHeader.CA_EEFProgramInd == YesNoList.Codes.Yes && nrCanHeader.CA_Category != NRCanIntendedUseCodes.Codes.NR01;
			}
		}

		public bool PGANRCan_IsExplosives
		{
			get
			{
				var nrCanHeader = NRCanHeader;
				return nrCanHeader != null && nrCanHeader.CA_EXPProgramInd == YesNoList.Codes.Yes;
			}
		}

		public bool PGANRCan_IsEXPRequired
		{
			get
			{
				var nrCanHeader = NRCanHeader;
				return nrCanHeader != null && nrCanHeader.CA_EXPProgramInd == YesNoList.Codes.Yes && !nrCanHeader.CA_AuthorizedParty.IsEmpty;
			}
		}

		public bool PGATC_IsVPR
		{
			get
			{
				var tcHeader = TCPGAHeader;
				return tcHeader != null && tcHeader.IsVPR && tcHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.PIG;
			}
		}

		public bool PGAECCC_IsVehicleDetailsRequiredForXE01OrIsXE04
		{
			get
			{
				var ecccHeader = ECCCPGAHeader;
				return ecccHeader != null && (ecccHeader.IsXE04Process || ecccHeader.IsVehicleDetailsRequiredForXE01);
			}
		}

		public bool PGAHC_IsPesticides
		{
			get
			{
				var hcHeader = HCPGAHeader;
				return hcHeader != null && hcHeader.CA_PESProgramInd == YesNoList.Codes.Yes;
			}
		}

		#endregion
	}
}
