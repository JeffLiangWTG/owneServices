using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	public partial class DecTypeList
	{
		public static CodeDescriptionPairList GetDecTypeList(BusinessObjectFactory factory, bool isImport = false, bool isExport = false)
		{
			if (isImport && isExport)
			{
				throw new DeveloperNotificationException("IsImport and isExport cannot be true in the same time.");
			}

			var bthActivated = CNCustomsDataRegistry.Instance.CNBTHFunctionActive.Value;

			var cacheKey = string.Format(CultureInfo.InvariantCulture,
				"CNJobDeclarationDecTypeList{0}{1}{2}{3}",
				!(isImport || isExport) ? "_COMMON" : string.Empty,
				isImport ? "_IMP" : string.Empty,
				isExport ? "_EXP" : string.Empty,
				bthActivated ? "_BTHActivated" : string.Empty
			);

			return factory.GetCachedValue(cacheKey, () => GetDecTypeList(bthActivated, isImport, isExport));
		}

		public static CodeDescriptionPairList GetDecTypeList(JobDeclaration jobDeclaration)
		{
			return GetDecTypeList(jobDeclaration.Factory, jobDeclaration.IsImport, jobDeclaration.IsExport);
		}

		public static CodeDescriptionPairList GetDecTypeList(CusEntryInstruction cusEntryInstruction)
		{
			return GetDecTypeList(cusEntryInstruction.Factory, cusEntryInstruction.WillGenerateEnteringEntry, cusEntryInstruction.WillGenerateExitingEntry);
		}

		static CodeDescriptionPairList GetDecTypeList(bool bthActivated, bool isImport, bool isExport)
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"Const localized texts no need to translate.");

			if (!(isImport || isExport))
			{
				result.AddRange(new DecTypeList());
				if (!bthActivated)
				{
					result.RemoveCode(Codes.Both);
				}
			}
			else
			{
				var cusPrefix = isImport ? (NoResString)"进口" : isExport ? (NoResString)"出口" : string.Empty;
				result.AddPair(Codes.CustomsEntry, cusPrefix + Descriptions.CustomsEntry);

				var recPrefix = isImport ? (NoResString)"进境" : isExport ? (NoResString)"出境" : string.Empty;
				result.AddPair(Codes.RecordListing, recPrefix + Descriptions.RecordListing);

				if (bthActivated)
				{
					var bthDesc = isImport ? (NoResString)"进口报关单+出境备案清单" : (NoResString)"进境备案清单+出口报关单";
					result.AddPair(Codes.Both, bthDesc);
				}
			}

			return result;
		}
	}
}
