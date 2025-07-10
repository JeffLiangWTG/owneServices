using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GuaranteeForEntryInstructionLookups : CommonGuaranteeLookups
	{
		public GuaranteeForEntryInstructionLookups(GuaranteeForEntryInstruction guarantee) : base(guarantee)
		{
		}

		public CodeDescriptionPairList LiabilityApplicablePercentageList => Factory.GetCachedValue<LiabilityApplicablePercentageCodeList>();

		protected override IReadOnlyList<ZString> GuaranteeTypeFilter
		{
			get
			{
				IReadOnlyList<ZString> result;
				if (entryInstruction?.JobDeclaration is JobDeclaration declaration && declaration.IsUCC6)
				{
					if (declaration.IsImport)
					{
						result = new ZString[] { EUGuaranteeTypeList.Codes.COD, EUGuaranteeTypeList.Codes.IMP };
					}
					else if (declaration.IsExport)
					{
						result = new ZString[] { EUGuaranteeTypeList.Codes.COD };
					}
					else
					{
						result = base.GuaranteeTypeFilter;
					}
				}
				else
				{
					result = base.GuaranteeTypeFilter;
				}

				return result;
			}
		}

		protected override IReadOnlyList<ZString> GuaranteeReferencesTypeFilter
		{
			get
			{
				if (entryInstruction?.JobDeclaration is JobDeclaration declaration && declaration.IsUCC6)
				{
					return (declaration.IsImport || declaration.IsExport) ? new ZString[] { entryInstruction.CEI_Style } : base.GuaranteeReferencesTypeFilter;
				}
				else
				{
					return base.GuaranteeReferencesTypeFilter;
				}
			}
		}

		CusEntryInstruction entryInstruction => Parent.Parent as CusEntryInstruction;
	}
}
