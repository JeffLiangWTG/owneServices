using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public class JobDeclarationCustomsOfficeRequirementHelper : EU.Business.JobDeclarationCustomsOfficeRequirementHelper
	{
		public JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override CustomsOfficeRequirement GetMainOffice()
		{
			return Factory.GetCachedValue("DE.JobDeclarationCustomsOfficeRequirementHelper.MainOffice" + Declaration.JE_MessageType, () =>
			{
				if (Declaration.IsImport)
				{
					return new CustomsOfficeRequirement(ZString.Empty, true, true);
				}
				else if (Declaration.IsExport)
				{
					return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, true, true);
				}
				return base.GetMainOffice();
			});
		}

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			return Factory.GetCachedValue("DE.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements" + Declaration.JE_MessageType, () =>
			{
				if (Declaration.IsImport)
				{
					return new List<CustomsOfficeRequirement>
					{
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, false, false)
					};
				}
				if (Declaration.IsExport)
				{
					return new List<CustomsOfficeRequirement>
					{
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, false, false)
						{
							OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland },
						},
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice, false, false)
						{
							OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport }
						},
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, false, false),
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, false, false, true)
						{
							OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport }
						}
					};
				}
				return base.GetOtherRequirements();
			});
		}
	}
}
