//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTCPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoTCPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class TCPGAHeaderAddInfoValidation : AutoTCPGAHeaderAddInfoValidation
	{
		public TCPGAHeaderAddInfoValidation(AutoTCPGAHeaderAddInfo parent) : base(parent)
		{
		}

		TCPGAHeader PGAHeader => (TCPGAHeader)Parent.Parent;

		protected override void CheckCA_ProductClass()
		{
			base.CheckCA_ProductClass();

			if (PGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes
				|| PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes
					&& PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.PIL
						|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
						|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
						|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
						|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
						|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR
						|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_ProductClassInfo, Parent.Lookups.ProductClassList);
			}
		}

		protected override void CheckCA_ProductType()
		{
			base.CheckCA_ProductType();

			if (PGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_ProductTypeInfo, Parent.Lookups.ProductTypeList);
			}
		}

		protected override void CheckCA_ProductSize()
		{
			base.CheckCA_ProductSize();

			if (PGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_ProductSizeInfo, Parent.Lookups.ProductSizeList);
			}
		}

		protected override void CheckCA_ImportReasonCode()
		{
			base.CheckCA_ImportReasonCode();
			if (PGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_ImportReasonCodeInfo, Parent.Lookups.ImportReasonCodeList);
			}
		}

		protected override void CheckCA_VehicleStatus()
		{
			base.CheckCA_VehicleStatus();
			if (PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes && PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_VehicleStatusInfo, Parent.Lookups.VehicleStatusList);
			}
		}

		protected override void CheckCA_VehicleCondition()
		{
			base.CheckCA_VehicleCondition();

			if (PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes
				&& PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
					|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
					|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC
					|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
					|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_VehicleConditionInfo, Parent.Lookups.VehicleConditionList);
			}
		}

		protected override void CheckCA_ODOReading()
		{
			base.CheckCA_ODOReading();

			if (PGAHeader.IsVPR
				&& (PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS || PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC)
				&& PGAHeader.ParentCountryOfOrigin == Core.Constants.CountryCodes.Mexico)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_ODOReadingInfo);
			}
		}

		protected override void CheckCA_ChassisYear()
		{
			base.CheckCA_ChassisYear();

			if (PGAHeader.IsVPR
				&& PGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.VVP
				&& PGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.PIG)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_ChassisYearInfo, Parent.Lookups.ChassisYearList);
			}
		}

		protected override void CheckCA_ManufactureYear()
		{
			base.CheckCA_ManufactureYear();

			if (PGAHeader.IsVPR && PGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.PIG)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_ManufactureYearInfo, Parent.Lookups.ManufactureYearList);
			}
		}

		protected override void CheckCA_ManufactureMonth()
		{
			base.CheckCA_ManufactureMonth();

			if (PGAHeader.IsVPR && PGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.PIG)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_ManufactureMonthInfo, Parent.Lookups.ManufactureMonthList);
			}
		}

		protected override void CheckCA_CriteriaConformance()
		{
			base.CheckCA_CriteriaConformance();
			if (PGAHeader.IsVPR
				&& PGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.PIG
				&& PGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.VUV
				&& PGAHeader.CA_SubProgram != TCPGAVehicleProgramCodes.Codes.VVP)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CriteriaConformanceInfo, Parent.Lookups.CriteriaConformanceList);
			}
		}

		protected override void CheckCA_TitleStatus()
		{
			base.CheckCA_TitleStatus();

			if (PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes
				&& PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS || PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VVP)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_TitleStatusInfo, Parent.Lookups.TitleStatusList);
			}
		}

		protected override void CheckCA_SubProgram()
		{
			base.CheckCA_SubProgram();
			if (PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_SubProgramInfo, Parent.Lookups.SubProgramCodesList);
			}
		}
	}
}
