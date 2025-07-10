using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class JobDeclarationRatingAdapter<T> : BaseJobDeclarationRatingAdapter<T>, IAutoRatingCustomsInfo
		where T : JobDeclaration
	{
		public JobDeclarationRatingAdapter(T parent)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}

		readonly T parent;

		#region Autorating

		public override void OnAutoRated(IEnumerable<IAutoRatedCharge> charges)
		{
			// Override InvoicingLineDescription
			if (Parent.IsLVX && Parent.Invoices.Count > 0)
			{
				foreach (var charge in charges)
				{
					var reference = Res.GetString("8D97CF37-4CA8-40B8-AA37-75E3237DA07C", "LVS ID : {0}", Parent.Invoices[0].JZ_InvoiceNumber);
					var newDescription = FormattableString.Invariant($"{charge.InvoiceLineDescription} - {reference}");
					charge.InvoiceLineDescription = newDescription;
				}
			}
		}

		protected override AutoRatingStatusInfo GetStatusInformationCore()
		{
			var existingStatus = base.GetStatusInformationCore();
			if (!existingStatus.CanExecute || !existingStatus.Message.IsEmpty)
			{
				return existingStatus;
			}

			var canExecute = true;
			var message = ZString.Empty;

			if (parent.IsB2Adjustments || parent.IsB3X)
			{
				if (!parent.IsMergeDone && parent.Invoices.Count > 0 && parent.FilteredInvoiceLines.Count > 0)
				{
					parent.DoMerge();
				}

				if (!parent.CustomsEntryHeaders.Any())
				{
					canExecute = false;
					message = Res.GetString("D4D2BE76-DE6B-4CA7-978F-81D148A911A3", "Merge has not occurred.\r\nPlease ensure that an Invoice Header and Invoice lines have been entered.\r\nThen save the job and try again.");
				}
			}

			if (parent.IsConsolidatedLVS)
			{
				foreach (var invoice in parent.Invoices)
				{
					var declaration = (JobDeclaration)invoice.JobDeclaration;
					if (declaration.IsLVX)
					{
						canExecute = false;
						message = Res.GetString("5ccdbbce-e2ff-4e5f-b5a6-3702c58cf104", "Cannot Auto-Rate because there is one, or more, Courier LVS Declaration jobs attached to this Consolidated LVS Declaration.");
						break;
					}
				}
			}
			if (canExecute && parent.IsImport)
			{
				if (parent.B3EntryHeader == null)
				{
					canExecute = false;
					message = Res.GetString("31f9dfe7-88f5-4691-9c45-47ba3f6ac78e", "Merge has not occurred.\r\nPlease ensure that an Invoice Header and Invoice lines have been entered.\r\nThen save the job and try again.");
				}
				else if (new B3ImportStatusCalculator().IsAwaitingReply(parent.JE_MessageStatus) || new EDIReleaseImportStatusCalculator().IsAwaitingReply(parent.JE_MessageStatus))
				{
					message = Res.GetString("b977f97f-7f9d-4d30-a9ce-cefe11d43e15", "The Declaration is waiting for a response.\r\n\r\nThe customs disbursement information presented on the invoice should not be treated as final until a response is received.\r\n\r\nWe suggest that rating and posting of this amount is postponed until a response has been received.\r\n\r\nDo you wish to continue AutoRating?");
				}
			}
			return new AutoRatingStatusInfo(canExecute, message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void AddAdditionalRatingMeasurements(RateableMeasureSet measurements)
		{
			base.AddAdditionalRatingMeasurements(measurements);

			var totalDeclaredPGALines = 0;
			var additionalMeasureTypeCounts = new Dictionary<MeasureType, int>
			{
				{ MeasureType.CFIALine, 0 },
				{ MeasureType.NRCANLine, 0 },
				{ MeasureType.SITTLine, 0 },
				{ MeasureType.TCLine, 0 },
				{ MeasureType.OtherPGALine, 0 },
				{ MeasureType.HCLine, 0 },
				{ MeasureType.PHACLine, 0 },
				{ MeasureType.ECCCLine, 0 },
				{ MeasureType.DFOLine, 0 },
				{ MeasureType.CNSCLine, 0 },
				{ MeasureType.GACLine, 0 },
				{ MeasureType.PGALines, 0 },
			};

			if (parent.IsIID)
			{
				foreach (JobComInvoiceLine invoiceLine in parent.InvoiceLines)
				{
					AddAdditionalRatingMeasurementsForPGA(MeasureType.CFIALine, () => invoiceLine.CFIAPGAHeader != null && invoiceLine.CA_CFIAInd == YesNoList.Codes.Yes);
					AddAdditionalRatingMeasurementsForPGA(MeasureType.NRCANLine, () => invoiceLine.NRCanPGAHeader != null && invoiceLine.CA_NRCanInd == YesNoList.Codes.Yes);
					AddAdditionalRatingMeasurementsForPGA(MeasureType.TCLine, () => invoiceLine.TCPGAHeader != null && invoiceLine.CA_TCInd == YesNoList.Codes.Yes);
					AddAdditionalRatingMeasurementsForPGA(MeasureType.HCLine, () => invoiceLine.HCPGAHeader != null && invoiceLine.CA_HCInd == YesNoList.Codes.Yes);
					AddAdditionalRatingMeasurementsForPGA(MeasureType.PHACLine, () => invoiceLine.PHACPGAHeader != null && invoiceLine.CA_PHACInd == YesNoList.Codes.Yes);
					AddAdditionalRatingMeasurementsForPGA(MeasureType.ECCCLine, () => invoiceLine.ECCCPGAHeader != null && invoiceLine.CA_ECCCInd == YesNoList.Codes.Yes);
					AddAdditionalRatingMeasurementsForPGA(MeasureType.DFOLine, () => invoiceLine.DFOPGAHeader != null && invoiceLine.CA_DFOInd == YesNoList.Codes.Yes);
					AddAdditionalRatingMeasurementsForPGA(MeasureType.CNSCLine, () => invoiceLine.CNSCPGAHeader != null && invoiceLine.CA_CNSCInd == YesNoList.Codes.Yes);
					AddAdditionalRatingMeasurementsForPGA(MeasureType.GACLine, () => invoiceLine.GACPGAHeader != null && invoiceLine.CA_GACInd == YesNoList.Codes.Yes);
				}
			}
			else if (parent.IsImport && !parent.IsLVS)
			{
				foreach (JobComInvoiceLine invoiceLine in parent.InvoiceLines)
				{
					AddAdditionalRatingMeasurementsForPGA(MeasureType.CFIALine, () => parent.CA_OGDCFIA && !invoiceLine.CA_AirsCode.IsEmpty);

					if (!invoiceLine.CA_ImportReasonCode.IsEmpty)
					{
						var isHSCodeContainsOGDBilled = false;
						AddAdditionalRatingMeasurementsForPGA(MeasureType.NRCANLine, () => parent.CA_OGDNR && invoiceLine.IsHSCodeContainsOGDNRCan, () => isHSCodeContainsOGDBilled = true);
						AddAdditionalRatingMeasurementsForPGA(MeasureType.TCLine, () => parent.CA_OGDTC && invoiceLine.IsHSCodeContainsOGDTC, () => isHSCodeContainsOGDBilled = true);

						var isOGDBilled = false;
						AddAdditionalRatingMeasurementsForPGA(MeasureType.SITTLine, () => !isHSCodeContainsOGDBilled && !isOGDBilled && parent.CA_OGDIC, () => isOGDBilled = true);
						AddAdditionalRatingMeasurementsForPGA(MeasureType.NRCANLine, () => !isHSCodeContainsOGDBilled && !isOGDBilled && parent.CA_OGDNR, () => isOGDBilled = true);
						AddAdditionalRatingMeasurementsForPGA(MeasureType.TCLine, () => !isHSCodeContainsOGDBilled && !isOGDBilled && parent.CA_OGDTC, () => isOGDBilled = true);
					}
				}
			}

			additionalMeasureTypeCounts[MeasureType.PGALines] = totalDeclaredPGALines;

			additionalMeasureTypeCounts
				.Where(x => x.Value > 0)
				.ForEach(x => measurements.SetQuantity(x.Key, x.Value, string.Empty));

			void AddAdditionalRatingMeasurementsForPGA(MeasureType measureType, Func<ZBool> shouldAddCountFunc, Action addExtraPGAMeasurementsAction = null)
			{
				if (shouldAddCountFunc != null && shouldAddCountFunc())
				{
					additionalMeasureTypeCounts[measureType]++;
					totalDeclaredPGALines++;

					if (addExtraPGAMeasurementsAction != null)
					{
						addExtraPGAMeasurementsAction();
					}
				}
			}
		}

		public override ILocation Destination
		{
			get
			{
				return parent.InvoicingSupporter.Destination;
			}
		}

		#endregion

		protected override ZDecimal ShipmentCountCore
		{
			get
			{
				return parent.IsLVX || parent.IsB2Adjustments || parent.IsB3X ? new ZDecimal(1m) : base.ShipmentCountCore;
			}
		}

		public override FreightMode FreightMode
		{
			get
			{
				return parent.IsLVX || parent.IsB2Adjustments || parent.IsB3X ? FreightMode.ROA : base.FreightMode;
			}
		}

		public override Directions JobDirection
		{
			get
			{
				return parent.IsLVX || parent.IsB2Adjustments || parent.IsB3X ? Directions.Import : base.JobDirection;
			}
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get
			{
				return parent.IsB2Adjustments || parent.IsB3X ? new CADeclarationJobDatesProvider(parent) : base.JobDatesProvider;
			}
		}

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get
			{
				var result = new EntryInfoCollection();
				var header = parent.B3EntryHeader;
				if (header != null)
				{
					result.AddNew(header.MergedLines.Count, header.MergedLines.InvoiceLineCount, header.CustomsValue);
				}
				return result;
			}
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get
			{
				return parent.InvoiceLines.Cast<JobComInvoiceLine>().Select(l => l.CA_B3SubHeaderNumber).Distinct().Count();
			}
		}
	}
}
