using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class EUR1BoxItemsBuilder
	{
		public EUR1BoxItemsBuilder(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			InvoiceLines = Argument.NotNull(invoiceLines, nameof(invoiceLines));
		}

		protected IEnumerable<JobComInvoiceLine> InvoiceLines { get; }

		public ZString ItemsInfoBox8 { get; private set; }
		public ZString GrossMassVolumeBox9 { get; private set; }
		public ZString InvoicesBox10 { get; private set; }

		public void Build()
		{
			var box8Builder = new ZStringBuilder();
			var box9Builder = new ZStringBuilder();
			var box10Builder = new ZStringBuilder();

			BuildCore(box8Builder, box9Builder, box10Builder);

			box8Builder.AppendIfBuilderIsNotEmpty(Box8EndOfSection);

			ItemsInfoBox8 = box8Builder.ToStringWithNewLineBetweenAppends();
			GrossMassVolumeBox9 = box9Builder.ToStringWithNewLineBetweenAppends();
			InvoicesBox10 = box10Builder.ToStringWithNewLineBetweenAppends();
		}

		protected virtual void BuildCore(ZStringBuilder box8Builder, ZStringBuilder box9Builder, ZStringBuilder box10Builder)
		{
			int lineNumber = 1;
			var linegroups = InvoiceLines.GroupBy(x => x.InvoiceNumber);

			foreach (var lineGroup in linegroups.OrderBy(x => x.Key))
			{
				if (ShouldShowInvoiceNumber)
				{
					box10Builder.Append(lineGroup.Key);
				}

				foreach (JobComInvoiceLine line in lineGroup.OrderBy(x => x.JI_LineNo))
				{
					box8Builder.Append(GetItemsInfoBox8Row(line, lineNumber));
					box9Builder.Append(GetGrossMassRow(line));

					lineNumber++;
				}
			}
		}

		#region Implementation

		ZString GetItemsInfoBox8Row(JobComInvoiceLine line, int lineNumber)
		{
			var packageAndMarksAndNumbersInfo = GetPackageAndMarksAndNumberInfo(line);

			var sb = new ZStringBuilder(FormattableString.Invariant($"{lineNumber};"));
			sb.AppendIfNotEmpty(packageAndMarksAndNumbersInfo.MarksAndNumbers);
			sb.AppendIfNotEmpty(packageAndMarksAndNumbersInfo.Package);

			if (ShouldShowDescription)
			{
				sb.AppendIfNotEmpty(line.JI_Description.ToUpperInvariant());
			}

			return sb.ToStringWithDelimiterBetweenAppends(" ");
		}

		protected virtual PackageAndMarksAndNumbersInfo GetPackageAndMarksAndNumberInfo(JobComInvoiceLine line) => new PackageAndMarksAndNumbersInfo(line);

		ZString GetGrossMassRow(JobComInvoiceLine line) => FormattableString.Invariant($"{line.JI_Weight.ToStringTrimZeros()} {line.JI_WeightUQ}");

		#endregion

		const string Box8EndOfSection = "-------------------------------------------------------------------------------------------------------";

		protected virtual bool ShouldShowDescription => true;

		protected virtual bool ShouldShowInvoiceNumber => true;
	}
}
