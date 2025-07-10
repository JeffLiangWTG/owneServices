using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class AddInfoDataObjectReaderForHC : AddInfoDataObjectReader<HCPGAHeader>
	{
		public AddInfoDataObjectReaderForHC(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, HCPGAHeaderAddInfoSchema.Instance)
		{
		}

		protected override IEnumerable<UniversalAddInfo> AddAddionalAddInfo(IEnumerable<UniversalAddInfo> addInfos, IEnumerable<UniversalAddInfo> list)
		{
			var intendedUseCode = list.GetZStringValue(Constants.AddInfoKeys.HCPGAHeader.IntendedUseCode);
			var category = list.GetZStringValue(Constants.AddInfoKeys.HCPGAHeader.Category);
			if (intendedUseCode.HasValue || category.HasValue)
			{
				return AddFallbackValuesIfNeeded(addInfos, intendedUseCode, category);
			}
			else
			{
				return addInfos;
			}
		}

		IEnumerable<UniversalAddInfo> AddFallbackValuesIfNeeded(IEnumerable<UniversalAddInfo> addInfos, ZString? oldIntendedUseCode, ZString? oldCategory)
		{
			var categories = new List<string>(new[] { HCPGAHeader.Schema.CA_CategoryAPI.Substring(3), HCPGAHeader.Schema.CA_CategoryBBC.Substring(3),
				HCPGAHeader.Schema.CA_CategoryCPR.Substring(3), HCPGAHeader.Schema.CA_CategoryCTO.Substring(3),
				HCPGAHeader.Schema.CA_CategoryDSE.Substring(3), HCPGAHeader.Schema.CA_CategoryHDR.Substring(3),
				HCPGAHeader.Schema.CA_CategoryMDE.Substring(3), HCPGAHeader.Schema.CA_CategoryNHP.Substring(3),
				HCPGAHeader.Schema.CA_CategoryOCS.Substring(3), HCPGAHeader.Schema.CA_CategoryPES.Substring(3),
				HCPGAHeader.Schema.CA_CategoryRED.Substring(3), HCPGAHeader.Schema.CA_CategoryVET.Substring(3) });
			var intendedUseCodes = new List<string>(new[] { HCPGAHeader.Schema.CA_IntendedUseCodeAPI.Substring(3), HCPGAHeader.Schema.CA_IntendedUseCodeBBC.Substring(3),
				HCPGAHeader.Schema.CA_IntendedUseCodeCPR.Substring(3), HCPGAHeader.Schema.CA_IntendedUseCodeCTO.Substring(3),
				HCPGAHeader.Schema.CA_IntendedUseCodeDSE.Substring(3), HCPGAHeader.Schema.CA_IntendedUseCodeHDR.Substring(3),
				HCPGAHeader.Schema.CA_IntendedUseCodeMDE.Substring(3), HCPGAHeader.Schema.CA_IntendedUseCodeNHP.Substring(3),
				HCPGAHeader.Schema.CA_IntendedUseCodeOCS.Substring(3), HCPGAHeader.Schema.CA_IntendedUseCodePES.Substring(3),
				HCPGAHeader.Schema.CA_IntendedUseCodeVET.Substring(3) });
			var programCodes = new List<string>();
			var programIndKey = HCPGAHeader.Schema.CA_APIProgramInd.Substring(6);
			foreach (var addInfo in addInfos)
			{
				if (addInfo.Key.HasValue)
				{
					var key = addInfo.Key.Value;
					if (key.EndsWith(programIndKey, StringComparison.OrdinalIgnoreCase))
					{
						if (addInfo.Value.GetValueOrDefault().EqualsIgnoringCase(YesNoList.Codes.Yes))
						{
							var programCode = key.Left(key.Length - programIndKey.Length);
							if (!programCodes.Contains(programCode))
							{
								programCodes.Add(programCode);
							}
						}
					}
					else
					{
						intendedUseCodes.Remove(key);
						categories.Remove(key);
					}
				}

				yield return addInfo;
			}
			if (oldCategory.HasValue)
			{
				foreach (var programCode in programCodes)
				{
					var category = Constants.AddInfoKeys.HCPGAHeader.Category + programCode;
					if (categories.Remove(category))
					{
						yield return new UniversalAddInfo() { Key = category, Value = oldCategory };
					}
				}
			}
			if (oldIntendedUseCode.HasValue)
			{
				foreach (var programCode in programCodes)
				{
					var intendedUseCode = Constants.AddInfoKeys.HCPGAHeader.IntendedUseCode + programCode;
					if (intendedUseCodes.Remove(intendedUseCode))
					{
						yield return new UniversalAddInfo() { Key = intendedUseCode, Value = oldIntendedUseCode };
					}
				}
			}
		}
	}
}
