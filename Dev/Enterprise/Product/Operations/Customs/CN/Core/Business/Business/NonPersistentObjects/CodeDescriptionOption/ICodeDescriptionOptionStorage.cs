using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface ICodeDescriptionOptionStorage
	{
		bool ContainsCode(string code);
		IEnumerable<ZString> AllCodes { get; }
		BusinessObject FindByCode(ZString code);
		void AddNew(ZString code);
		void RemoveAndDelete(BusinessObject businessObject);
		ICodeDescriptionPairList GetAllOptions();
		ZPropertyInfo SelectedOptionsAsStringPropertyInfo { get; }
		void ValidateSeletedOption(ZString code, bool selected, ZPropertyInfo propertyInfo, IEnumerable<ZString> selectedCodes);
		IValidationModeProvider ValidationModeProvider { get; }
	}

	public interface IGroupedCodeDescriptionPairList
	{
		IEnumerable<string> GetMutuallyExclusiveCodes(string code);
		bool AutoUnselectExclusiveCodes { get; }
	}

	public static class CodeDescriptionOptionStorageExtension
	{
		public static ZString GetSelectedOptionCodeAsString(this ICodeDescriptionOptionStorage storage, string separator = null) =>
			storage.GetSelectedOptionCodes().JoinAsString(separator);

		public static IEnumerable<string> GetSelectedOptionCodes(this ICodeDescriptionOptionStorage storage) =>
			storage.GetAllOptions().Cast<ICodeDescription>().Where(x => storage.ContainsCode(x.Code)).Select(x => x.Code);

		public static ZString GetSelectedOptionDescAsString(this ICodeDescriptionOptionStorage storage, string separator = null)
		{
			return storage.GetAllOptions().Cast<ICodeDescription>().Where(x => storage.ContainsCode(x.Code)).Select(x => x.Description).JoinAsString(separator);
		}

		public static void ValidateSelectedOptionAsString(this ICodeDescriptionOptionStorage storage)
		{
			var targetInfo = storage.SelectedOptionsAsStringPropertyInfo;
			if (targetInfo != null)
			{
				foreach (ICodeDescription pair in storage.GetAllOptions())
				{
					var code = pair.Code;
					var selected = storage.ContainsCode(code);

					storage.ValidateMutuallyExclusiveCodes(code, selected, targetInfo, storage.AllCodes);
					storage.ValidateSeletedOption(code, selected, targetInfo, storage.AllCodes);
				}
			}
		}

		public static void ValidateMutuallyExclusiveCodes(this ICodeDescriptionOptionStorage storage, string code, bool selected, ZPropertyInfo targetInfo, IEnumerable<ZString> selectedCodes)
		{
			if (selected)
			{
				var allOptions = storage.GetAllOptions();
				var mutuallyExclusiveCodes = (allOptions as IGroupedCodeDescriptionPairList)?.GetMutuallyExclusiveCodes(code);
				if (mutuallyExclusiveCodes != null)
				{
					foreach (var exclusiveCode in mutuallyExclusiveCodes.Where(c => selectedCodes.Contains(c)))
					{
						var exclusiveCodes = new[] { code, exclusiveCode }.StableSort(StringComparer.OrdinalIgnoreCase);
						targetInfo.AddNotification(Res.GetString("A0F89A74-1613-4E47-BFE3-62B7A341A956", "'{0}' and '{1}' cannot be selected at the same time.", allOptions.GetDescriptionFromCode(exclusiveCodes[0]), allOptions.GetDescriptionFromCode(exclusiveCodes[1])), storage.ValidationModeProvider);
					}
				}
			}
		}
	}
}
