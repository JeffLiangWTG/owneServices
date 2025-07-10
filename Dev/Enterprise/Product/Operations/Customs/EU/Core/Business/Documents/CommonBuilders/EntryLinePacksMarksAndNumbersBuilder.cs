using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Documents
{
	public class EntryLinePacksMarksAndNumbersBuilder
	{
		public EntryLinePacksMarksAndNumbersBuilder(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly CusEntryLine entryLine;

		public ZString Build()
		{
			ZStringBuilder packingDetails = new ZStringBuilder();
			var allPacksInAllLines = entryLine.PackagingDetails;
			Dictionary<string, int> uniquePacks = new Dictionary<string, int>();
			foreach (Biz.InvoiceLinePackagePivot pivot in allPacksInAllLines)
			{
				string key = pivot.Package.CW_PackType + "~" + pivot.Package.CW_MarksAndNos;
				if (uniquePacks.ContainsKey(key))
				{
					uniquePacks[key] += pivot.CHC_NumberOfPacks;
				}
				else
				{
					uniquePacks[key] = pivot.CHC_NumberOfPacks;
				}
			}
			foreach (string key in uniquePacks.Keys)
			{
				string pc = key.Split('~')[0];
				string pm = key.Split('~')[1];
				string pn = uniquePacks[key].ToString(CultureInfo.InvariantCulture);

				packingDetails.AppendIfNotEmpty(FormatPackageDetailFields(pm, pn, pc));
			}

			return packingDetails.ToStringWithDelimiterBetweenAppends(PackagesSeparator);
		}

		protected virtual string PackagesSeparator => SemicolonAndSpace;

		protected virtual string FormatPackageDetailFields(string mark, string number, string type) => FormattableString.Invariant($"{mark}, {number} {type}");

		const string SemicolonAndSpace = "; ";
	}
}
