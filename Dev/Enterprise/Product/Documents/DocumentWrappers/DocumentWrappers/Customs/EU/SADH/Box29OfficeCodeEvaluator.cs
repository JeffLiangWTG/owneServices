using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public interface IBox29OfficeCodeEvaluator
	{
		ZString Evaluate();
	}

	class Box29OfficeCodeEvaluator : IBox29OfficeCodeEvaluator
	{
		public Box29OfficeCodeEvaluator(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		}

		readonly CusEntryHeader entryHeader;

		public ZString Evaluate()
		{
			var officeCode = entryHeader.Declaration.IsImport ? entryHeader.OfficeOfEntry : entryHeader.OfficeOfExit;

			if (!officeCode.IsEmpty)
			{
				var countryCode = officeCode.Left(2);
				var officeDescription = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(entryHeader.Factory, officeCode, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today)?.ZZD_Description ?? ZString.Empty;
				return FormattableString.Invariant($"{officeCode} {officeDescription}").Trim();
			}

			return ZString.Empty;
		}
	}
}
