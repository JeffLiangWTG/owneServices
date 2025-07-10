using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business
{
	public class JobDeclarationCustomsOfficeRequirementHelper : EU.Business.JobDeclarationCustomsOfficeRequirementHelper
	{
		public JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			return Factory.GetCachedValue("ES.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements_" + Declaration.JE_MessageType + "_" + Declaration.IsUCC6, () =>
			{
				if (Declaration.IsImport)
				{
					var result = new List<CustomsOfficeRequirement>
					{
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, false, false),
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, false, false)
					};

					if (Declaration.IsUCC6)
					{
						result.Add(new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.SupervisingCustomsOffice, false, false));
					}

					return result;
				}

				if (Declaration.IsExport)
				{
					if (!Declaration.IsUCC6)
					{
						return new List<CustomsOfficeRequirement>
						{
							new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, false, false),
							new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, false, false)
						};
					}
					else
					{
						return new List<CustomsOfficeRequirement>
						{
							new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, false, false)
							{
								OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland },
							},
							new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, false, false),
							new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, false, false)
						};
					}
				}
				return base.GetOtherRequirements();
			});
		}
	}
}
