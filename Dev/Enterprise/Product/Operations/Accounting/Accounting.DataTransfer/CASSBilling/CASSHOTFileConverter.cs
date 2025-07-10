using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSHOTFileConverter : FlatFileConverter
	{
		public CASSHOTFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			NotificationManager notifier = new NotificationManager(Notification);
			var cassCostHeader = valueObject as CASSCostHeader;
			if (cassCostHeader != null)
			{
				IDisposable suspendListChanged = null;
				try
				{
					using (cassCostHeader.GetValidationSuspender())
					{
						bool hasHeaderRecordAlreadyRead = false;
						bool hasHeader2Or3RecordBeenRead = false;
						bool hasHeaderErrorBeenReported = false;
						bool? isImportHeader = null;
						foreach (CASSHOTFileDataRow dataRow in fileLines)
						{
							if (IsValidRecordType(isImportHeader, dataRow.RecordType))
							{
								if (!cassCostHeader.IsInitialized)
								{
									if (ImportHeaderRecordTypes.Contains(dataRow.RecordType) || ImportLineRecordTypes.Contains(dataRow.RecordType))
									{
										cassCostHeader.InitializeAsImportCASS();
										isImportHeader = true;
									}
									else
									{
										cassCostHeader.InitializeAsExportCASS();
										isImportHeader = false;
									}
									suspendListChanged = cassCostHeader.Lines.SuspendListChanged();
								}

								if (ImportHeaderRecordTypes.Contains(dataRow.RecordType) || ExportHeaderRecordTypes.Contains(dataRow.RecordType))
								{
									if (hasHeaderRecordAlreadyRead || hasHeader2Or3RecordBeenRead)
									{
										notifier.AddWarningToNotifications(Res.GetString("b6dd17e3-7161-406e-8679-2288a3524e03", "More than one header records in the file."));
									}
									else
									{
										var headerRow = (CASSHOTFileHeaderRow)dataRow;

										cassCostHeader.DatePeriodStart = headerRow.HeaderDatePeriodStart;
										cassCostHeader.DatePeriodEnd = headerRow.HeaderDatePeriodEnd;
										cassCostHeader.DateOfBilling = headerRow.HeaderDateOfBilling;
										cassCostHeader.BillingCurrency = headerRow.BillingCurrency;
										cassCostHeader.RecordType = headerRow.RecordType;

										hasHeaderRecordAlreadyRead = ExportHeaderRecordTypes.Contains(dataRow.RecordType);
										hasHeader2Or3RecordBeenRead = ImportHeaderRecordTypes.Contains(dataRow.RecordType);
									}
								}
								else if (ImportLineRecordTypes.Contains(dataRow.RecordType) || ExportLineRecordTypes.Contains(dataRow.RecordType))
								{
									var lineRow = (CASSHOTFileLineRow)dataRow;

									var isImportLine = ImportLineRecordTypes.Contains(dataRow.RecordType);

									if (isImportLine && !hasHeader2Or3RecordBeenRead && !hasHeaderErrorBeenReported)
									{
										notifier.AddErrorToNotifications(Res.GetString("c48ef677-999e-4bf0-a11e-c038a5fd3b11", "A header record with AA2 or AA3 Record ID is expected before the import records with IBI, IBR or IBO Record ID."));
										hasHeaderErrorBeenReported = true;
									}

									if (!isImportLine && !hasHeaderRecordAlreadyRead && !hasHeaderErrorBeenReported)
									{
										notifier.AddWarningToNotifications(Res.GetString("82f41734-62de-4c64-ba36-faeb443e330f", "A header record with AAA Record ID is expected before the export records with AWM, CCO, CCR, DCO, DCR or ECR Record ID."));
									}

									var cassLineType = CASSCostLineType.Default;
									if (AdjustmentRecordTypes.Contains(dataRow.RecordType))
									{
										cassLineType = CASSCostLineType.Adjustment;
									}
									else if (dataRow.RecordType == CASSHOTFileFormat.RecortID.ECR)
									{
										cassLineType = CASSCostLineType.Rejected;
									}
									else if (dataRow.RecordType == CASSHOTFileFormat.RecortID.AWM)
									{
										cassLineType = CASSCostLineType.Billing;
									}

									var cassCostLine = isImportHeader.Value ? new CASSCostImportLine(Factory, cassLineType) : (CASSCostLine)new CASSCostExportLine(Factory, cassLineType);
									cassCostHeader.Lines.Add(cassCostLine);

									using (cassCostLine.GetValidationSuspender())
									{
										cassCostLine.RecordType = dataRow.RecordType;
										cassCostLine.AirlinePrefix = lineRow.AirlinePrefix;
										cassCostLine.AWBSerialNumber = lineRow.AWBSerialNumber;
										cassCostLine.AgentCode = lineRow.AgentCode;
										cassCostLine.DateAWBExecution = lineRow.DateAWBExecution;
										cassCostLine.DateOfArrival = lineRow.DateOfArrival;
										cassCostLine.DateOfDelivery = lineRow.DateOfDelivery;
										cassCostLine.Origin = lineRow.Origin;
										cassCostLine.Destination = lineRow.Destination;
										cassCostLine.WeightUnit = lineRow.WeightUnit;
										cassCostLine.Weight = lineRow.Weight;
										cassCostLine.CurrencyCode = lineRow.Currency;

										var decimals = cassCostLine.CurrencyISODecimalPlaces;
										var unitRatio = cassCostLine.CurrencyISOSubUnitRatio;
										if (isImportHeader.Value)
										{
											var cassImportCostLine = cassCostLine as CASSCostImportLine;
											var importLineRow = lineRow as CASSHOTFileImportLineRow;
											cassImportCostLine.WeightCharges = Utilities.Round(importLineRow.WeightCharges / unitRatio, decimals);
											cassImportCostLine.ChargesDueAgentCC = Utilities.Round(importLineRow.ChargesDueAgentCC / unitRatio, decimals);
											cassImportCostLine.ChargesDueCarrierCC = Utilities.Round(importLineRow.ChargesDueCarrierCC / unitRatio, decimals);
											cassImportCostLine.FeeCharged = (importLineRow.FeeCharged == "Y");
											cassImportCostLine.FeeAmount = Utilities.Round(importLineRow.FeeAmount / unitRatio, decimals);
											cassImportCostLine.HandlingCharges = Utilities.Round(importLineRow.HandlingCharges / unitRatio, decimals);
											cassImportCostLine.StorageCharges = Utilities.Round(importLineRow.StorageCharges / unitRatio, decimals);
											cassImportCostLine.OtherCharge1Amount = Utilities.Round(importLineRow.OtherCharge1Amount / unitRatio, decimals);
											cassImportCostLine.OtherCharge2Amount = Utilities.Round(importLineRow.OtherCharge2Amount / unitRatio, decimals);
											cassImportCostLine.MiscellaneousChargesAmount = Utilities.Round(importLineRow.MiscellaneousChargesAmount / unitRatio, decimals);
										}
										else
										{
											var cassExportCostLine = cassCostLine as CASSCostExportLine;
											var exportLineRow = lineRow as CASSHOTFileExportLineRow;
											cassExportCostLine.CCADCMNumber = exportLineRow.CCADCMNumber;
											cassExportCostLine.WeightChargePP = Utilities.Round(exportLineRow.WeightChargePP / unitRatio, decimals);
											cassExportCostLine.ValuationChargePP = Utilities.Round(exportLineRow.ValuationChargePP / unitRatio, decimals);
											cassExportCostLine.ChargesDueCarrierPP = Utilities.Round(exportLineRow.ChargesDueCarrierPP / unitRatio, decimals);
											cassExportCostLine.ChargesDueAgentCC = Utilities.Round(exportLineRow.ChargesDueAgentCC / unitRatio, decimals);
											cassExportCostLine.Commission = Utilities.Round(exportLineRow.Commission / unitRatio, decimals);
											cassExportCostLine.Discount = Utilities.Round(exportLineRow.Discount / unitRatio, decimals);
											cassExportCostLine.VATDueAirline = Utilities.Round(exportLineRow.VATDueAirline / unitRatio, decimals);
											cassExportCostLine.VATDueAgent = Utilities.Round(exportLineRow.VATDueAgent / unitRatio, decimals);
										}

										cassCostLine.VATIndicator = lineRow.VATIndicator;
										cassCostLine.CostAmountRowFileValue = cassCostLine.CASSCost;
										cassCostLine.VATAmountRowFileValue = cassCostLine.VATAmount;
									}
								}
							}
							else
							{
								var expectedRecordTypes = new List<ZString>();
								if (isImportHeader.HasValue && isImportHeader.Value)
								{
									expectedRecordTypes.AddRange(ImportHeaderRecordTypes);
									expectedRecordTypes.AddRange(ImportLineRecordTypes);
								}
								else if (isImportHeader.HasValue && !isImportHeader.Value)
								{
									expectedRecordTypes.AddRange(ExportHeaderRecordTypes);
									expectedRecordTypes.AddRange(ExportLineRecordTypes);
								}
								else
								{
									expectedRecordTypes.AddRange(ExportHeaderRecordTypes);
									expectedRecordTypes.AddRange(ExportLineRecordTypes);
									expectedRecordTypes.AddRange(ImportHeaderRecordTypes);
									expectedRecordTypes.AddRange(ImportLineRecordTypes);
								}

								notifier.AddErrorToNotifications(Res.GetString("9bb3c618-f517-4a47-8eed-2f840befce1e", "Invalid Record Type: {0}. Expected Record Types are {1}", dataRow.RecordType, string.Join(", ", expectedRecordTypes.ToArray())));
							}
						}

						cassCostHeader.UpdateOriginalAmountFields();
					}
				}
				finally
				{
					suspendListChanged?.Dispose();
				}
			}
		}

		protected override void CheckArguments(IValueObject valueObject, IFlatFileFormat flatFileFormat)
		{
			base.CheckArguments(valueObject, flatFileFormat);
			if (!(valueObject is CASSCostHeader))
			{
				throw new ArgumentException("Not supported type of ValueObject", nameof(valueObject));
			}
			if (!(flatFileFormat is CASSHOTFileFormat))
			{
				throw new ArgumentException("Not supported format type", nameof(flatFileFormat));
			}
		}

		bool IsValidRecordType(bool? isImportHeader, ZString recordType)
		{
			bool result = true;
			if (isImportHeader.HasValue && isImportHeader.Value)
			{
				result = ImportHeaderRecordTypes.Contains(recordType) || ImportLineRecordTypes.Contains(recordType);
			}
			else if (isImportHeader.HasValue && !isImportHeader.Value)
			{
				result = ExportHeaderRecordTypes.Contains(recordType) || ExportLineRecordTypes.Contains(recordType);
			}
			else
			{
				result = ImportHeaderRecordTypes.Contains(recordType)
							|| ImportLineRecordTypes.Contains(recordType)
							|| ExportHeaderRecordTypes.Contains(recordType)
							|| ExportLineRecordTypes.Contains(recordType);
			}
			return result;
		}

		ZString[] ImportHeaderRecordTypes
		{
			get
			{
				return new ZString[] { CASSHOTFileFormat.RecortID.Header2, CASSHOTFileFormat.RecortID.Header3 };
			}
		}

		ZString[] ExportHeaderRecordTypes
		{
			get
			{
				return new ZString[] { CASSHOTFileFormat.RecortID.Header };
			}
		}

		ZString[] ImportLineRecordTypes
		{
			get
			{
				return new ZString[] { CASSHOTFileFormat.RecortID.IBI, CASSHOTFileFormat.RecortID.IBO, CASSHOTFileFormat.RecortID.IBR };
			}
		}

		ZString[] ExportLineRecordTypes
		{
			get
			{
				return new ZString[] { CASSHOTFileFormat.RecortID.AWM, CASSHOTFileFormat.RecortID.CCO, CASSHOTFileFormat.RecortID.CCR,
										CASSHOTFileFormat.RecortID.DCO, CASSHOTFileFormat.RecortID.DCR, CASSHOTFileFormat.RecortID.ECR };
			}
		}

		ZString[] AdjustmentRecordTypes
		{
			get
			{
				return new ZString[] { CASSHOTFileFormat.RecortID.CCO, CASSHOTFileFormat.RecortID.DCO, CASSHOTFileFormat.RecortID.IBO };
			}
		}
	}
}
