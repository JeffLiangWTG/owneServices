using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobDeclarationCustomsOfficeRequirementHelper : EU.Business.JobDeclarationCustomsOfficeRequirementHelper
	{
		public JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override string GetCacheKeyCombination()
		{
			return string.Join(",", base.GetCacheKeyCombination(), Declaration.JE_TransportMode);
		}

		protected override CustomsOfficeRequirement GetMainOffice()
		{
			return Factory.GetCachedValue($"FR.JobDeclarationCustomsOfficeRequirementHelper.MainOffice{Declaration.JE_MessageType}{(Declaration.IsUCC6 ? @"UCC6" : "")}", () =>
			{
				var result = base.GetMainOffice();
				if (Declaration.IsImport || Declaration.IsExport)
				{
					result = Declaration.IsUCC6AndIsImport
						? new CustomsOfficeRequirement(ZString.Empty, true, true, Res.GetString("50740288-9f24-4a19-9de9-185aef2c6b50", "Office of Presentation"))
						: new CustomsOfficeRequirement(ZString.Empty, true, true, Res.GetString("8432ff98-d455-4e88-a8dc-222740b1bf6b", "Office of Lodgement"));
				}
				return result;
			});
		}

		protected override IEnumerable<string> ValidateCore()
		{
			var errors = new List<string>();
			var customsOfficesCAU = (Declaration as JobDeclaration).CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep);
			if (FRCustomsDataRegistry.DeltaGFallbackIsActive)
			{
				if (customsOfficesCAU == null)
				{
					errors.Add(Res.GetString("65852F29-B798-48D5-B027-3E9E608CD971", "An office of type CAU is needed"));
				}
				else
				{
					var office = ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Declaration.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, customsOfficesCAU.CY_Date).Where(x => x.ZZD_Code == customsOfficesCAU.CY_Data).FirstOrDefault();
					if (office == null || (!office.HasAttribute(RefCusCodeListAttributeTypes.Codes.EMAIL)))
					{
						errors.Add(Res.GetString("B15DDF9A-3439-4EE1-973F-7B74D7445178", "The office of type CAU must have an email address"));
					}
				}
			}
			errors.AddRange(base.ValidateCore());
			return errors;
		}

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			var styleCodes = new List<ZString> { DeltaIEImportDeclarationTypeList.Codes.H2, DeltaIEImportDeclarationTypeList.Codes.H3, DeltaIEImportDeclarationTypeList.Codes.H4 };
			var isCei_StyleH2H3H4 = Declaration.CustomsEntryInstructions.Any(e => styleCodes.Contains(e.CEI_Style));

			var key = $"FR.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements{Declaration.JE_MessageType}{Declaration.JE_TransportMode}{(Declaration.IsUCC6 ? @"UCC6" : "")}{(isCei_StyleH2H3H4 ? "H2_H3_H4" : "")}";
			return Factory.GetCachedValue(key, () =>
			{
				if (Declaration.IsImport)
				{
					var result = new List<CustomsOfficeRequirement>();
					if (Declaration.IsUCC6)
					{
						var cauRequirement = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true, false, Res.GetString("3f4806b5-cdad-4eeb-b142-717d07d8d20e", "Supervising Customs Office"));
						result.Add(cauRequirement);

						if (isCei_StyleH2H3H4)
						{
							var disRequirement = new CustomsOfficeRequirement(FrOfficeCodesTypes.Codes.OfficeOfDischarge, true, false, Res.GetString("23338276-F1CC-4AB4-92E6-949408E50E09", "Office of Discharge"))
							{
								MaxOfficeCountLimit = 99
							};
							result.Add(disRequirement);
						}
					}
					else
					{
						var cauRequirement = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true, false, Res.GetString("4bd2c491-3ba8-4c4b-90f3-ae59e4337799", "Office of Declaration"));
						var entRequirement = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, false, true, Res.GetString("4913a6ed-2ae3-44f2-b099-cf9ec98560a4", "Office of Entry"));
						var desRequirement = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfDestination, false, false, Res.GetString("8aa23b93-eeb1-4a27-bdf4-e26e7ba1746b", "Office of Clearance"));
						result.AddRange(new[] { cauRequirement, entRequirement, desRequirement });
					}
					return result;
				}
				if (Declaration.IsExport)
				{
					return new List<CustomsOfficeRequirement>
					{
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true, false, Res.GetString("4bd2c491-3ba8-4c4b-90f3-ae59e4337799", "Office of Declaration")),
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, true, false),
						new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfDeparture, false, false, Res.GetString("ce88aacf-05c3-4fac-8fc7-d912df73257a", "Office of Clearance"))
					};
				}
				return base.GetOtherRequirements();
			});
		}
	}
}
