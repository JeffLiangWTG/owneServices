//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHCPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoHCPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class HCPGAHeaderAddInfoValidation : AutoHCPGAHeaderAddInfoValidation
	{
		public HCPGAHeaderAddInfoValidation(AutoHCPGAHeaderAddInfo parent) : base(parent)
		{
		}

		HCPGAHeader PGAHeader => Parent.Parent as HCPGAHeader;

		protected override void CheckCA_IntendedUseCodeAPI()
		{
			base.CheckCA_IntendedUseCodeAPI();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.API))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeAPIInfo, Parent.Lookups.IntendedUseCodesAPI);
			}
		}
		protected override void CheckCA_IntendedUseCodeBBC()
		{
			base.CheckCA_IntendedUseCodeBBC();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.BBC))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeBBCInfo, Parent.Lookups.IntendedUseCodesBBC);
			}
		}
		protected override void CheckCA_IntendedUseCodeCTO()
		{
			base.CheckCA_IntendedUseCodeCTO();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.CTO))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeCTOInfo, Parent.Lookups.IntendedUseCodesCTO);
			}
		}
		protected override void CheckCA_IntendedUseCodeCPR()
		{
			base.CheckCA_IntendedUseCodeCPR();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.CPR))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeCPRInfo, Parent.Lookups.IntendedUseCodesCPR);
			}
		}
		protected override void CheckCA_IntendedUseCodeDSE()
		{
			base.CheckCA_IntendedUseCodeDSE();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.DSE))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeDSEInfo, Parent.Lookups.IntendedUseCodesDSE);
			}
		}
		protected override void CheckCA_IntendedUseCodeHDR()
		{
			base.CheckCA_IntendedUseCodeHDR();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.HDR))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeHDRInfo, Parent.Lookups.IntendedUseCodesHDR);
			}
		}
		protected override void CheckCA_IntendedUseCodeNHP()
		{
			base.CheckCA_IntendedUseCodeNHP();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.NHP))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeNHPInfo, Parent.Lookups.IntendedUseCodesNHP);
			}
		}
		protected override void CheckCA_IntendedUseCodeOCS()
		{
			base.CheckCA_IntendedUseCodeOCS();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.OCS))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeOCSInfo, Parent.Lookups.IntendedUseCodesOCS);
			}
		}
		protected override void CheckCA_IntendedUseCodeMDE()
		{
			base.CheckCA_IntendedUseCodeMDE();

			if (PGAHeader.CA_MDE_LEX && Parent.CA_IntendedUseCodeMDE != HCIntendedUseCode.Codes.HC01)
			{
				Parent.CA_IntendedUseCodeMDEInfo.AddMessageError(Res.GetString("a11f1671-2526-4eb9-acfb-9bf565158119", "If 'Medical Device Establishment License Exemption' is indicated, the Intended Use Code must equal HC01: Human Therapeutic Use."));
			}
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.MDE))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeMDEInfo, Parent.Lookups.IntendedUseCodesMDE);
			}
		}
		protected override void CheckCA_IntendedUseCodePES()
		{
			base.CheckCA_IntendedUseCodePES();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.PES))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodePESInfo, Parent.Lookups.IntendedUseCodesPES);
			}
		}
		protected override void CheckCA_IntendedUseCodeVET()
		{
			base.CheckCA_IntendedUseCodeVET();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.VET))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_IntendedUseCodeVETInfo, Parent.Lookups.IntendedUseCodesVET);
			}
		}

		protected override void CheckCA_CategoryAPI()
		{
			base.CheckCA_CategoryAPI();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.API))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryAPIInfo, Parent.Lookups.CategoryCodesAPI);
			}
		}

		protected override void CheckCA_CategoryBBC()
		{
			base.CheckCA_CategoryBBC();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.BBC))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryBBCInfo, Parent.Lookups.CategoryCodesBBC);
			}
		}
		protected override void CheckCA_CategoryCTO()
		{
			base.CheckCA_CategoryCTO();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.CTO))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryCTOInfo, Parent.Lookups.CategoryCodesCTO);
			}
		}
		protected override void CheckCA_CategoryCPR()
		{
			base.CheckCA_CategoryCPR();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.CPR))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryCPRInfo, Parent.Lookups.CategoryCodesCPR);
			}
		}
		protected override void CheckCA_CategoryDSE()
		{
			base.CheckCA_CategoryDSE();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.DSE))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryDSEInfo, Parent.Lookups.CategoryCodesDSE);
			}
		}
		protected override void CheckCA_CategoryHDR()
		{
			base.CheckCA_CategoryHDR();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.HDR))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryHDRInfo, Parent.Lookups.CategoryCodesHDR);
			}
		}
		protected override void CheckCA_CategoryNHP()
		{
			base.CheckCA_CategoryNHP();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.NHP))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryNHPInfo, Parent.Lookups.CategoryCodesNHP);
			}
		}
		protected override void CheckCA_CategoryOCS()
		{
			base.CheckCA_CategoryOCS();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.OCS))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryOCSInfo, Parent.Lookups.CategoryCodesOCS);
			}
		}
		protected override void CheckCA_CategoryMDE()
		{
			base.CheckCA_CategoryMDE();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.MDE))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryMDEInfo, Parent.Lookups.CategoryCodesMDE);
			}
		}
		protected override void CheckCA_CategoryPES()
		{
			base.CheckCA_CategoryPES();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.PES))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryPESInfo, Parent.Lookups.CategoryCodesPES);
			}
		}
		protected override void CheckCA_CategoryRED()
		{
			base.CheckCA_CategoryRED();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.RED))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryREDInfo, Parent.Lookups.CategoryCodesRED);
			}
		}
		protected override void CheckCA_CategoryVET()
		{
			base.CheckCA_CategoryVET();
			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.VET))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CategoryVETInfo, Parent.Lookups.CategoryCodesVET);
			}
		}

		protected override void CheckCA_BatchLotNumber()
		{
			base.CheckCA_BatchLotNumber();

			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.API)
				|| PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.HDR)
				|| PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.MDE)
				|| PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.NHP)
				|| PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.VET))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_BatchLotNumberInfo);
			}
		}

		protected override void CheckCA_ComplianceStatement()
		{
			base.CheckCA_ComplianceStatement();

			if (!PGAHeader.CA_ComplianceStatement && PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.DSE) && PGAHeader.CA_IntendedUseCodeDSE == HCIntendedUseCode.Codes.HC01)
			{
				PGAHeader.CA_ComplianceStatementInfo.AddMessageError(Res.GetString("d163434c-ac02-4e43-bceb-7c46a56dbc86", "Certify should be ticked."));
			}
		}
	}
}
