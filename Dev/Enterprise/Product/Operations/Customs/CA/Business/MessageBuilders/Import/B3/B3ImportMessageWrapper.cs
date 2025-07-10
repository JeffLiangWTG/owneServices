using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class B3ImportMessageWrapper : IB3HeaderWithScheduledMessageSupport
	{
		public B3ImportMessageWrapper(CusEntryHeader entryHeader, bool suppressPageLineSetting = false)
		{
			CanSendDeclarationChecker.EntryNotNullAndAttachedToDeclaration(entryHeader);
			this.entryHeader = entryHeader;
			this.suppressPageLineSetting = suppressPageLineSetting;
			declaration = entryHeader.Declaration;
			this.entryHeader.IsB3GrossWeightSet = true;
			declaration.SetB3SubHeaderNumbers();
		}

		public void PopulateEntrySubmittedDateIfRequired(ZDateTime? scheduledTime)
		{
			this.entryHeader.PopulateEntrySubmittedDateIfRequired(scheduledTime);
		}

		#region IB3Header Members

		public ZString BatchNumber
		{
			get { return EDIMessage.UniqueBatchNumberPlaceHolder; }
		}

		public ZString B3TypeCode
		{
			get { return declaration.JE_MessageSubType; }
		}

		public ZString PaymentCode
		{
			get
			{
				if (declaration.IsImporterDirectPayment)
				{
					return "I";
				}
				else if (declaration.IsGSTDirectPayment)
				{
					return "G";
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString CBSAOffice
		{
			get { return declaration.JE_CustomsOffice.TrimStart('0'); }
		}

		public ZString PortOfUnlading
		{
			get { return declaration.CA_UnladingOffice.TrimStart('0'); }
		}

		public ZString WarehouseNumber
		{
			get { return declaration.IsWarehouseEntry ? IB3HeaderHelper.GetCustomsRegNo(declaration.WarehouseDocAddress.Organisation, OrgCusCode.CodeTypes.WarehouseControlledPremisesID) : ZString.Empty; }
		}

		public ZString TransactionNumber
		{
			get { return declaration.TransactionNumber.UniqueIdentifier; }
		}

		public ZString BusinessNumber
		{
			get
			{
				var importer = declaration.ImporterOfRecordAddress.HasRealOrganisation ? declaration.ImporterOfRecordAddress.Organisation : declaration.Importer;
				return IB3HeaderHelper.GetBusinessNumber(importer);
			}
		}

		public ZString GSTNumber
		{
			get
			{
				var importer = declaration.ImporterOfRecordAddress.HasRealOrganisation ? declaration.ImporterOfRecordAddress.Organisation : declaration.Importer;
				return IB3HeaderHelper.GetGSTNumber(importer);
			}
		}

		public ZString TransportMode => TransportTypeList.GetTransportModeNumber(declaration.JE_TransportMode);

		public ZString CarrierCodeAtImportation
		{
			get { return declaration.JE_CarrierCode; }
		}

		public IEnumerable<IB3BRelease> B3BInputReleases
		{
			get
			{
				if (b3BInputReleases == null)
				{
					var result = (from CargoControlNumber ccn in declaration.CargoControlNumbers where !ccn.CY_CargoControlNumber.IsEmpty select new B3BRelease(ccn.CY_CargoControlNumber, ccn.CY_DateOfRelease)).ToArray();

					if (!declaration.JE_EntryAuthorisationDate.IsEmpty)
					{
						if (!result.Any())
						{
							result = new[] { new B3BRelease() };
						}

						result[0].DateOfRelease = declaration.JE_EntryAuthorisationDate;
					}

					b3BInputReleases = result.ToArray();
				}
				return b3BInputReleases;
			}
		}
		IB3BRelease[] b3BInputReleases;

		public ZDecimal TotalValueForDuty
		{
			get { return entryHeader.CustomsValue; }
		}

		public IEnumerable<IB3SubHeader> PositiveB3SubHeaders
		{
			get
			{
				if (b3SubHeaders == null)
				{
					b3SubHeaders = this.GetB3SubHeaders();
				}
				return b3SubHeaders;
			}
		}
		IEnumerable<B3SubHeader> b3SubHeaders;

		public IEnumerable<IClassificationLine1> PositiveClassificationLines
		{
			get
			{
				if (positiveClassificationLines == null)
				{
					if (!suppressPageLineSetting)
					{
						SetPageRelativeLineNumbers();
					}

					suppressPageLineSetting = true;
					positiveClassificationLines = entryHeader.ClassificationLines.OrderBy(x => x.B3LineNumber).ToArray();
				}

				entryHeader.IsB3GrossWeightSet = true;
				return positiveClassificationLines;
			}
		}
		IClassificationLine1[] positiveClassificationLines;

		IEnumerable<IB3SubHeader> IB3Header.NegativeB3SubHeaders => Enumerable.Empty<IB3SubHeader>();

		IEnumerable<IClassificationLine1> IB3Header.NegativeClassificationLines => Enumerable.Empty<IClassificationLine1>();

		public bool IsCalculationsDone
		{
			get
			{
				if (!_isCalculationsDone.HasValue)
				{
					_isCalculationsDone = !PositiveClassificationLines.Any() || PositiveClassificationLines.All(line => line.HasGSTDetails);
				}
				return _isCalculationsDone.Value;
			}
		}
		bool? _isCalculationsDone;

		bool IB3Header.SumPosAndNeg => false;

		void IB3Header.RefreshCachedValues()
		{
			b3BInputReleases = null;
			b3SubHeaders = null;
			positiveClassificationLines = null;
			positiveTotalAmounts = null;
			negativeTotalAmounts = null;
			_isCalculationsDone = null;
		}

		public ZString B3Comments
		{
			get { return declaration.B3Comments; }
		}

		public IDocAddress Importer
		{
			get
			{
				return declaration.ImporterOfRecordAddress.HasRealAddress ? declaration.ImporterOfRecordAddress : AdjustmentDocHelper.AddressForImporter(declaration.Importer);
			}
		}

		public ZString AccountSecurityCode
		{
			get { return declaration.TransactionNumber.AccountSecurityCode; }
		}

		public ITotalAmounts PositiveTotalAmounts
		{
			get { return positiveTotalAmounts ?? (positiveTotalAmounts = entryHeader.PositiveTotalAmounts); }
		}

		ITotalAmounts positiveTotalAmounts;

		public ITotalAmounts NegativeTotalAmounts
		{
			get { return negativeTotalAmounts ?? (negativeTotalAmounts = entryHeader.NegativeTotalAmounts); }
		}

		ITotalAmounts negativeTotalAmounts;

		#endregion

		#region ICAEDIFACTMessageAttachee Members

		public BusinessObjectFactory Factory
		{
			get { return declaration.Factory; }
		}

		public void AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			entryHeader.Messages.Add(message);
		}

		public Enterprise.Messaging.Business.EDIMessageCollection Messages
		{
			get { return entryHeader.Messages; }
		}

		public ZString MessageStatus
		{
			get { return entryHeader.CH_Status; }
			set { entryHeader.CH_Status = value; }
		}

		public ZString JobStatus
		{
			get { return entryHeader.CH_EntryStatus; }
			set { entryHeader.CH_EntryStatus = value; }
		}

		public bool HasChanges
		{
			get { return declaration.HasChanges; }
		}

		public ZString JobIdentification
		{
			get { return declaration.JE_DeclarationReference; }
		}

		public BusinessObject TopLevelBusinessObject
		{
			get { return declaration; }
		}

		public ZDateTime ReleaseDate
		{
			get { return declaration.JE_EntryAuthorisationDate; }
			set { declaration.JE_EntryAuthorisationDate = value; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return declaration.IsCancelled; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return declaration.RefreshValidationBeforeSendMessage; }
		}

		ZString IB3Header.MessageType => declaration.JE_MessageType;

		#endregion

		#region Implementation

		void SetPageRelativeLineNumbers()
		{
			if (declaration.IsOtherWarehouseEntry)
			{
				return;
			}

			foreach (JobComInvoiceHeader header in declaration.Invoices)
			{
				var dictionary = new Dictionary<int, List<CusEntryLine>>();
				int lineNumber = 0;
				var invoiceLines = header.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.CA_PageNumber).ThenBy(x => x.JI_LineNo);
				foreach (JobComInvoiceLine line in invoiceLines)
				{
					if (!dictionary.ContainsKey(line.CA_PageNumber))
					{
						dictionary[line.CA_PageNumber] = new List<CusEntryLine>();
						lineNumber = 1;
					}

					var b3EntryLine = line.B3EntryLine;
					if (b3EntryLine != null && !dictionary[line.CA_PageNumber].Contains(b3EntryLine))
					{
						dictionary[line.CA_PageNumber].Add(b3EntryLine);
					}
					line.CA_PageRelativeLineNumber = lineNumber;
					line.InvoiceCrossReferencePageLineNumber = lineNumber++;
				}

				var lines = from pair in dictionary
							where pair.Value.Count == 1
							from JobComInvoiceLine line in pair.Value[0].InvoiceLines
							where line != null && line.CA_PageNumber == pair.Key
							select line;

				foreach (var line in lines)
				{
					line.InvoiceCrossReferencePageLineNumber = ZInt.Zero;
				}
			}
		}

		#region B3SubHeader

		public class B3SubHeader : IB3SubHeader
		{
			internal B3SubHeader(ZInt subHeaderNumber, IEnumerable<JobComInvoiceLine> lines, IB3Header b3Header)
			{
				this.b3SubHeaderNumber = subHeaderNumber;
				this.lines = lines;
				this.invoiceHeader = lines.First().InvoiceHeader;
				this.b3Header = b3Header;
			}

			#region IB3SubHeader Members

			readonly ZInt b3SubHeaderNumber;
			readonly JobComInvoiceHeader invoiceHeader;
			readonly IEnumerable<JobComInvoiceLine> lines;
			readonly IB3Header b3Header;

			public JobComInvoiceHeader InvoiceHeader
			{
				get { return invoiceHeader; }
			}

			public IEnumerable<JobComInvoiceLine> Lines
			{
				get { return lines; }
			}

			ZInt IB3SubHeader.B3SubHeaderNumber
			{
				get { return b3SubHeaderNumber; }
			}

			ZString IB3SubHeader.InvoiceNumber
			{
				get
				{
					if (!_invoiceNumber.HasValue)
					{
						var numbers = from JobComInvoiceLine line in this.lines
									  orderby line.InvoiceHeader.JZ_InvoiceNumber
									  group line by line.InvoiceHeader.JZ_InvoiceNumber into lines
									  select lines.Key;
						_invoiceNumber = ZString.Join(",", numbers.ToArray());
					}
					return _invoiceNumber.Value;
				}
			}
			ZString? _invoiceNumber;

			ZDecimal IB3SubHeader.FreightCharges
			{
				get
				{
					var result = this.lines.Sum(line => line.FreightCharges);
					if (result == ZDecimal.Zero && this.b3SubHeaderNumber == 1 && (this.invoiceHeader?.IsUSCountryOfExport ?? false))
					{
						result = this.invoiceHeader?.JobDeclaration?.CalculatedFreightAmount ?? ZDecimal.Zero;
					}
					return result > 0 && result < 1 ? 1 : result;
				}
			}

			IDocAddress IB3SubHeader.Vendor
			{
				get
				{
					return ((IEDIInvoiceOGD)this.invoiceHeader).Vendor;
				}
			}

			IDocAddress IB3SubHeader.Exporter
			{
				get { return ((IEDIInvoiceOGD)this.invoiceHeader).Exporter; }
			}

			ZDateTime IB3SubHeader.DateOfDirectShipment
			{
				get { return this.invoiceHeader.EffectiveValuationDate; }
			}

			ZString IB3SubHeader.CountryOfOrigin
			{
				get { return this.lines.First().EffectiveCountryAndStateOfOrigin; }
			}

			ZString IB3SubHeader.PlaceOfExport
			{
				get { return this.invoiceHeader.CA_TradeZone.IsEmpty ? ((IEDIInvoiceOGD)this.invoiceHeader).CommonCountryOfExport : this.invoiceHeader.CA_TradeZone; }
			}

			ZString IB3SubHeader.USPortOfExit
			{
				get { return this.invoiceHeader.CA_USPortOfExit; }
			}

			ZString IB3SubHeader.TariffTreatmentCode
			{
				get { return this.lines.First().EffectiveTreatmentCode; }
			}

			ZString IB3SubHeader.TimeLimitUnit
			{
				get { return this.invoiceHeader.CA_TimeLimitCode; }
			}

			ZInt IB3SubHeader.B3TimeLimits
			{
				get { return this.invoiceHeader.CA_TimeLimit; }
			}

			ZString IB3SubHeader.CurrencyCode
			{
				get { return this.lines.First().JI_RX_NKLinePriceCurr; }
			}

			ZDecimal IB3SubHeader.ExchangeRate
			{
				get { return this.invoiceHeader.EffectiveExchangeRateForInvoiceCurr; }
			}

			IB3Header IB3SubHeader.B3Header
			{
				get { return this.b3Header; }
			}

			#endregion

			VendorStateAndZipStruct IB3SubHeader.VendorStateAndZip
			{
				get { return this.invoiceHeader.VendorStateAndZip; }
			}

			public ZString TradeZone
			{
				get
				{
					return this.invoiceHeader.CA_TradeZone;
				}
			}
		}

		#endregion

		readonly CusEntryHeader entryHeader;
		bool suppressPageLineSetting;
		readonly JobDeclaration declaration;

		#endregion

		#region IB3HeaderWithScheduledMessageSupport Members

		public void CancelAndDeactivateScheduledB3Message()
		{
			this.entryHeader.CancelAndDeactivateDeferredB3Message();
		}

		#endregion
	}
}
