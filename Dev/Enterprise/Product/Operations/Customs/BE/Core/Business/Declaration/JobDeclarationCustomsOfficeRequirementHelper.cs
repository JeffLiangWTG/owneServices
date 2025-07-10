using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public class JobDeclarationCustomsOfficeRequirementHelper : EU.Business.JobDeclarationCustomsOfficeRequirementHelper
{
	public JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override IEnumerable<string> ValidateCore()
	{
		foreach (var error in base.ValidateCore())
		{
			yield return error;
		}

		var declaration = Declaration;
		if (declaration.Configuration.IsUCC6(declaration))
		{
			var customsOffice = declaration.JE_CustomsOffice;
			if (!customsOffice.IsEmpty && !customsOffice.StartsWith(Core.Constants.CountryCodes.Belgium))
			{
				yield return Res.GetString("87DCB114-5CC2-4BF0-863E-CE800179F816", "You have not entered a customs office that begins with {0}.", Core.Constants.CountryCodes.Belgium);
			}
		}
	}

	protected override CustomsOfficeRequirement GetMainOffice()
	{
		var declaration = Declaration;
		var isUcc6 = declaration.Configuration.IsUCC6(declaration);
		return Factory.GetCachedValue("BE.JobDeclarationCustomsOfficeRequirementHelper.MainOffice|" + declaration.JE_MessageType + "|" + (isUcc6 ? "UCC6" : (NoResString)"no UCC6"), () => declaration.IsImport // Cache Key
			? GetImportMainOfficeRequirement()
			: declaration.IsExport
				? new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, true, true)
				: base.GetMainOffice());

		CustomsOfficeRequirement GetImportMainOfficeRequirement() => isUcc6
			? new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.AuthorityControlCode, true, true, Res.GetString("3FEE0C32-FDEB-43A8-AFE4-24DB926270BA", "Supervising Customs Office"))
			: new CustomsOfficeRequirement(ZString.Empty, true, true);
	}

	protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
	{
		var declaration = Declaration;
		var isUcc6 = declaration.Configuration.IsUCC6(declaration);
		return Factory.GetCachedValue("BE.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements|" + declaration.JE_MessageType + "|" + (isUcc6 ? "UCC6" : (NoResString)"no UCC6"), () => declaration.IsImport // Cache Key
			? GetImportOtherRequirements()
			: declaration.IsExport
				? GetExportOtherRequirements()
				: base.GetOtherRequirements());

		IEnumerable<CustomsOfficeRequirement> GetImportOtherRequirements()
		{
			var customsOfficeRequirements = new List<CustomsOfficeRequirement>
			{
				new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, false, false, false)
				{
					OfficeRolesForLookup = new ZString[]
					{
						EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent,
						EuOfficeCodesTypes.Codes.ReleaseForFreeCirculation,
						EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented,
						EuOfficeCodesTypes.Codes.AuthorityControlCode
					},
				},
				new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfDispatch, false, false, false)
				{
					OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfDispatch }
				}
			};

			if (!isUcc6)
			{
				customsOfficeRequirements.Add(new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, false, false));
			}

			return customsOfficeRequirements;
		}

		IEnumerable<CustomsOfficeRequirement> GetExportOtherRequirements() => new List<CustomsOfficeRequirement>
		{
			new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, false, false)
			{
				OfficeRolesForLookup = new ZString[]
				{
					EuOfficeCodesTypes.Codes.OfficeOfExit,
					EuOfficeCodesTypes.Codes.OfficeOfExitInland
				},
			},
			new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice, false, false)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport }
			},
			new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, false, false),
			new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, false, false, true)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport }
			},
			new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfLodgement, false, false)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport }
			},
			new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.SupervisingOffice, false, false)
		};
	}
}
