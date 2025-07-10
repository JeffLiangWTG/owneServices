using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class ATRBoxItemsWrapper : IATRBoxItems
	{
		public ATRBoxItemsWrapper(IEnumerable<Customs.Business.BaseJobComInvoiceLine> invoiceLines)
		{
			this.invoiceLines = Argument.NotNull(invoiceLines, nameof(invoiceLines));
		}
		protected readonly IEnumerable<Customs.Business.BaseJobComInvoiceLine> invoiceLines;

		public ZString ItemsInfoBox9 { get; private set; }
		public ZString MarksNumberBox10 { get; private set; }
		public ZString GrossWeightBox11 { get; private set; }

		public void Build()
		{
			var box9Builder = new ZStringBuilder();
			var box10Builder = new ZStringBuilder();
			var box11Builder = new ZStringBuilder();

			BuildCore(box9Builder, box10Builder, box11Builder);

			ItemsInfoBox9 = box9Builder.ToStringWithNewLineBetweenAppends();
			MarksNumberBox10 = box10Builder.ToStringWithNewLineBetweenAppends();
			GrossWeightBox11 = box11Builder.ToStringWithNewLineBetweenAppends();
		}

		protected virtual void BuildCore(ZStringBuilder box9Builder, ZStringBuilder box10Builder, ZStringBuilder box11Builder)
		{
			var lineNumber = 1;
			var linegroups = invoiceLines.GroupBy(x => x.InvoiceNumber);
			var volumeLine = ZString.Empty;

			foreach (var lineGroup in linegroups.OrderBy(x => x.Key))
			{
				foreach (JobComInvoiceLine line in lineGroup.OrderBy(x => x.JI_LineNo))
				{
					box9Builder.Append(GetItemsInfoBox9Row(lineNumber));
					box10Builder.Append(GetItemsInfoBox10Row(line));
					box11Builder.Append(GetGrossMassRow(line));
					volumeLine = GetVolume(line);
					if (!volumeLine.IsEmpty)
					{
						box11Builder.Append(volumeLine);
					}

					lineNumber++;
				}
			}
		}

		#region Implementation

		ZString GetItemsInfoBox9Row(int lineNumber)
		{
			var sb = new ZStringBuilder(FormattableString.Invariant($"{lineNumber}"));

			return sb.ToStringWithDelimiterBetweenAppends(" ");
		}

		protected virtual ZString GetItemsInfoBox10Row(JobComInvoiceLine line)
		{
			var packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(line, ignoreBulkPackage: true);

			var sb = new ZStringBuilder();
			sb.AppendIfNotEmpty(packageAndMarksAndNumbersInfo.MarksAndNumbers);
			sb.AppendIfNotEmpty(packageAndMarksAndNumbersInfo.Package);
			if (packageAndMarksAndNumbersInfo.ContainsBulkPackages && !TransportDetail.Voyage.IsEmpty)
			{
				sb.AppendIfNotEmpty(FormattableString.Invariant($"{TransportDetail.Voyage};"));
			}

			sb.AppendIfNotEmpty(line.JI_Description.ToUpperInvariant());

			return sb.ToStringWithDelimiterBetweenAppends(" ");
		}

		ITransportDetail TransportDetail => transportDetail ?? (transportDetail = new TransportDetailWrapper((JobDeclaration)invoiceLines.First().Declaration));
		ITransportDetail transportDetail;

		protected virtual ZString GetGrossMassRow(JobComInvoiceLine line)
		{
			var weight = line.CusEntryLine.EffectiveGrossWeight;

			var weightBuilder = new ZStringBuilder();
			if (weight.Amount > 0)
			{
				weightBuilder.Append(weight.ToString());
			}
			return weightBuilder.ToStringWithDelimiterBetweenAppends(" ");
		}

		protected virtual ZString GetVolume(JobComInvoiceLine line)
		{
			var volume = line.CusEntryLine.EffectiveVolume;

			var volumeBuilder = new ZStringBuilder();
			if (volume.Amount > 0)
			{
				volumeBuilder.Append(volume.ToString());
			}
			return volumeBuilder.ToStringWithDelimiterBetweenAppends("");
		}

		#endregion

	}
}
