using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ESGuarantee : EU.Business.Declaration.GuaranteeForDeclaration
	{
		public ESGuarantee(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override CusBondDetailLookups GetNewLookups() => new ESGuaranteeLookups(this);
		public new ESGuaranteeLookups Lookups => (ESGuaranteeLookups)base.Lookups;

		protected override CusBondDetailValidation GetNewValidation() => new ESGuaranteeValidation(this);
		public new ESGuaranteeValidation Validation => (ESGuaranteeValidation)base.Validation;

		public override ZString PW_BondNumber
		{
			get => base.PW_BondNumber;
			set
			{
				ZString oldValue = PW_BondNumber;
				base.PW_BondNumber = value;
				if (!value.IsEmpty && oldValue != value)
				{
					SetBondFiledPortWhenNeeded();
				}
			}
		}

		protected override void OnEntryInstructionIDChanged()
		{
			SetBondFiledPortWhenNeeded();
		}

		bool IsEntryInstructionH2 => EntryInstruction != null && EntryInstruction.CEI_Style == IMPDeclarationTypeList.Codes.H2;

		protected override Func<OrgHeader, ZString> AlternativeDelegateToFindHolderIdentification => Lookups.GetEoriOrNif;

		void SetBondFiledPortWhenNeeded()
		{
			if (!PW_BondNumber.IsEmpty && IsEntryInstructionH2 && PW_BondFiledPort.IsEmpty)
			{
				var guarantee = (CusGuaranteeHeader)Lookups.ReferenceNumbers.FirstOrDefault(g => g.CPH_Number == PW_BondNumber && g.CPH_IsActive == true && (g.CPH_StartDate <= ZDateTime.Today) && (g.CPH_EndDate >= ZDateTime.Today || g.CPH_EndDate == ZDateTime.Empty));
				if (guarantee != null)
				{
					PW_BondFiledPort = guarantee.DefaultPW_BondFiledPortForGuarantee;
				}
			}
		}
	}
}
