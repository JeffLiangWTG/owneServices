using System;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	[UseSnapshotProtection]
	public class CASSHOTFileFormatTest : TestCase
	{
		public void TestConstants()
		{
			Array positions = Enum.GetValues(typeof(CASSHOTFileFormat.AWMTransactionRecord.Position));
			foreach (CASSHOTFileFormat.AWMTransactionRecord.Position position in positions)
			{
				foreach (CASSHOTFileFormat.AWMTransactionRecord.Position nextPosition in positions)
				{
					if (position < nextPosition)
					{
						Assert(string.Format("AWMRecord {0} field overlap {1}.", position, nextPosition),
							(int)position + (int)Enum.Parse(typeof(CASSHOTFileFormat.AWMTransactionRecord.Length), position.ToString()) <= (int)nextPosition);
					}
				}
			}

			positions = Enum.GetValues(typeof(CASSHOTFileFormat.DCR_DCR_CCO_CCRTransactionRecord.Position));
			foreach (CASSHOTFileFormat.DCR_DCR_CCO_CCRTransactionRecord.Position position in positions)
			{
				foreach (CASSHOTFileFormat.DCR_DCR_CCO_CCRTransactionRecord.Position nextPosition in positions)
				{
					if (position < nextPosition)
					{
						Assert(string.Format("DCR_DCR_CCO_CCRRecord {0} field overlap {1}.", position, nextPosition),
							(int)position + (int)Enum.Parse(typeof(CASSHOTFileFormat.DCR_DCR_CCO_CCRTransactionRecord.Length), position.ToString()) <= (int)nextPosition);
					}
				}
			}

			positions = Enum.GetValues(typeof(CASSHOTFileFormat.HeaderRecord.Position));
			foreach (CASSHOTFileFormat.HeaderRecord.Position position in positions)
			{
				foreach (CASSHOTFileFormat.HeaderRecord.Position nextPosition in positions)
				{
					if (position < nextPosition)
					{
						Assert(string.Format("HeaderRecord {0} field overlap {1}.", position, nextPosition),
							(int)position + (int)Enum.Parse(typeof(CASSHOTFileFormat.HeaderRecord.Length), position.ToString()) <= (int)nextPosition);
					}
				}
			}

			positions = Enum.GetValues(typeof(CASSHOTFileFormat.Header2Record.Position));
			foreach (CASSHOTFileFormat.Header2Record.Position position in positions)
			{
				foreach (CASSHOTFileFormat.Header2Record.Position nextPosition in positions)
				{
					if (position < nextPosition)
					{
						Assert(string.Format("Header2Record {0} field overlap {1}.", position, nextPosition),
							(int)position + (int)Enum.Parse(typeof(CASSHOTFileFormat.Header2Record.Length), position.ToString()) <= (int)nextPosition);
					}
				}
			}

			positions = Enum.GetValues(typeof(CASSHOTFileFormat.IBI_IBO_IBRTransactionRecord.Position));
			foreach (CASSHOTFileFormat.IBI_IBO_IBRTransactionRecord.Position position in positions)
			{
				foreach (CASSHOTFileFormat.IBI_IBO_IBRTransactionRecord.Position nextPosition in positions)
				{
					if (position < nextPosition)
					{
						Assert(string.Format("IBI_IBO_IBRTransactionRecord {0} field overlap {1}.", position, nextPosition),
							(int)position + (int)Enum.Parse(typeof(CASSHOTFileFormat.IBI_IBO_IBRTransactionRecord.Length), position.ToString()) <= (int)nextPosition);
					}
				}
			}
		}

		public void TestFileExtensionForImport()
		{
			AssertEquals(FileExtensionType.ClientSpecific, new CASSHOTFileFormat().FileExtensionForImport);
			AssertEquals("hot", new CASSHOTFileFormat().GetClientSpecificFileExtension_ForTestOnly());
		}

		public void TestConvertToRow()
		{
			CASSHOTFileFormat dataFormat = new CASSHOTFileFormat();
			CASSHOTFileDataRow result;
			string rawDataRow;

			rawDataRow = "GS123456PARFRBOMING724AF  000007N02BGA00300KUSD200000605019990101199906301234512";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertNull("Not Supported Record Type", result);

			rawDataRow = "AAADE   08080108083108091001EUR 2347006";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("080801", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodStart));
			AssertEquals("080831", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodEnd));
			AssertEquals("080910", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDateOfBilling));
			AssertEquals("", result.GetField(CASSHOTFileHeaderRow.Schema.BillingCurrency));

			rawDataRow = "TTT DEDE     0001945000000000000480001993";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertNull("Not Supported Record Type", result);

			rawDataRow = "AWM N16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320900013204000000100000180";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("AWM", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("160", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("62144154", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("23470068510", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("080814", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("DUS", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("HKG", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0011900", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("EUR", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000346290", result.GetField(CASSHOTFileExportLineRow.Schema.WeightCharge));
			AssertEquals("000000003000", result.GetField(CASSHOTFileExportLineRow.Schema.ValuationCharge));
			AssertEquals("000000180880", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000002000", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000001000", result.GetField(CASSHOTFileExportLineRow.Schema.Commission));
			AssertEquals("000000132090", result.GetField(CASSHOTFileExportLineRow.Schema.Discount));
			AssertEquals("N", result.GetField(CASSHOTFileExportLineRow.Schema.VATIndicator));
			AssertEquals("00132040", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAirline));
			AssertEquals("00000100", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAgent));

			rawDataRow = "AWM Y16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320900013204000000100000180     -";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("-000000132090", result.GetField(CASSHOTFileExportLineRow.Schema.Discount));
			AssertEquals("Y", result.GetField(CASSHOTFileExportLineRow.Schema.VATIndicator));
			AssertEquals("00132040", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAirline));
			AssertEquals("00000100", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAgent));

			rawDataRow = "DCO N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618000000000000000000000300000000000000000000000400 K0002150MEX";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("DCO", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("172", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("67828073", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("23470068510", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("080607", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("LEJ", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("MEX", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0002150", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("EUR", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000047300", result.GetField(CASSHOTFileExportLineRow.Schema.WeightCharge));
			AssertEquals("0", result.GetField(CASSHOTFileExportLineRow.Schema.ValuationCharge));
			AssertEquals("000000021618", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000000200", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000000300", result.GetField(CASSHOTFileExportLineRow.Schema.Commission));
			AssertEquals(" 000000000400", result.GetField(CASSHOTFileExportLineRow.Schema.Discount));
			AssertEquals("N", result.GetField(CASSHOTFileExportLineRow.Schema.VATIndicator));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAirline));

			rawDataRow = "DCO Y17267828073  LEJ23470068510808640EUR00000.00000080607C000000047300P000000000100C000000000000P000000000200C000000021618000000043210000000000300000000000000000000000400-K0002150MEX";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("DCO", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("172", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("67828073", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("23470068510", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("080607", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("LEJ", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("MEX", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0002150", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("EUR", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("0", result.GetField(CASSHOTFileExportLineRow.Schema.WeightCharge));
			AssertEquals("000000000100", result.GetField(CASSHOTFileExportLineRow.Schema.ValuationCharge));
			AssertEquals("0", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("0", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000000300", result.GetField(CASSHOTFileExportLineRow.Schema.Commission));
			AssertEquals("-000000000400", result.GetField(CASSHOTFileExportLineRow.Schema.Discount));
			AssertEquals("Y", result.GetField(CASSHOTFileExportLineRow.Schema.VATIndicator));
			AssertEquals("000000043210", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAirline));

			rawDataRow = "DCR N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000000000000000000000000000000000000000000000400 K0002150MEX";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("DCR", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("172", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("67828073", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("23470068510", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("080607", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("LEJ", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("MEX", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0002150", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("EUR", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000047300", result.GetField(CASSHOTFileExportLineRow.Schema.WeightCharge));
			AssertEquals("0", result.GetField(CASSHOTFileExportLineRow.Schema.ValuationCharge));
			AssertEquals("000000054668", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.Commission));
			AssertEquals(" 000000000400", result.GetField(CASSHOTFileExportLineRow.Schema.Discount));
			AssertEquals("N", result.GetField(CASSHOTFileExportLineRow.Schema.VATIndicator));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAirline));

			rawDataRow = "CCO N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618000000000000000000000300000000000000000000000400 K0002150MEX";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("CCO", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("172", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("67828073", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("23470068510", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("080607", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("LEJ", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("MEX", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0002150", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("EUR", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000047300", result.GetField(CASSHOTFileExportLineRow.Schema.WeightCharge));
			AssertEquals("0", result.GetField(CASSHOTFileExportLineRow.Schema.ValuationCharge));
			AssertEquals("000000021618", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000000200", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000000300", result.GetField(CASSHOTFileExportLineRow.Schema.Commission));
			AssertEquals(" 000000000400", result.GetField(CASSHOTFileExportLineRow.Schema.Discount));
			AssertEquals("N", result.GetField(CASSHOTFileExportLineRow.Schema.VATIndicator));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAirline));

			rawDataRow = "CCR Y17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668100200300004000000000000000000001004000000000400 K0002150MEX";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("CCR", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("172", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("67828073", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("23470068510", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("080607", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("LEJ", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("MEX", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0002150", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("EUR", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000047300", result.GetField(CASSHOTFileExportLineRow.Schema.WeightCharge));
			AssertEquals("0", result.GetField(CASSHOTFileExportLineRow.Schema.ValuationCharge));
			AssertEquals("000000054668", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.Commission));
			AssertEquals(" 000000000400", result.GetField(CASSHOTFileExportLineRow.Schema.Discount));
			AssertEquals("Y", result.GetField(CASSHOTFileExportLineRow.Schema.VATIndicator));
			AssertEquals("100200300004", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAirline));
			AssertEquals("000000001004", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAgent));
		}

		public void TestConvertToRow_ECR_IfRejectedClaimLinesIsNotExpected()
		{
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", false, CASSBilling.IsRejectedClaimLinesExpected);

			CASSHOTFileFormat dataFormat = new CASSHOTFileFormat();
			CASSHOTFileDataRow result;
			string rawDataRow;

			rawDataRow = "ECR N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000000000000000000000000000000000000000000000400 K0002150MEX";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertNull("ECR record is not supported.", result);
		}

		public void TestConvertToRow_ECR_IfRejectedClaimLinesIsExpected()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", true, CASSBilling.IsRejectedClaimLinesExpected);

			CASSHOTFileFormat dataFormat = new CASSHOTFileFormat();
			CASSHOTFileDataRow result;
			string rawDataRow;

			rawDataRow = "ECR N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000000000000000000000000000000000000000000000400 K0002150MEX";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("ECR", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("172", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("67828073", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("23470068510", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("080607", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("LEJ", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("MEX", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0002150", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("EUR", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000047300", result.GetField(CASSHOTFileExportLineRow.Schema.WeightCharge));
			AssertEquals("0", result.GetField(CASSHOTFileExportLineRow.Schema.ValuationCharge));
			AssertEquals("000000054668", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.Commission));
			AssertEquals(" 000000000400", result.GetField(CASSHOTFileExportLineRow.Schema.Discount));
			AssertEquals("N", result.GetField(CASSHOTFileExportLineRow.Schema.VATIndicator));
			AssertEquals("000000000000", result.GetField(CASSHOTFileExportLineRow.Schema.VATDueAirline));
		}

		public void TestConvertToRow_Import()
		{
			CASSHOTFileFormat dataFormat = new CASSHOTFileFormat();
			CASSHOTFileDataRow result;
			string rawDataRow;

			rawDataRow = "AA2JP054000004  10080110081510082301JPY";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("100801", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodStart));
			AssertEquals("100815", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodEnd));
			AssertEquals("100823", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDateOfBilling));
			AssertEquals("JPY", result.GetField(CASSHOTFileHeaderRow.Schema.BillingCurrency));

			rawDataRow = "AA3JP054000004  10080110081510082301JPY";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("100801", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodStart));
			AssertEquals("100815", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodEnd));
			AssertEquals("100823", result.GetField(CASSHOTFileHeaderRow.Schema.HeaderDateOfBilling));
			AssertEquals("JPY", result.GetField(CASSHOTFileHeaderRow.Schema.BillingCurrency));

			rawDataRow = "IBI        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("IBI", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("006", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("70158756", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("JP054000004", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("100804", result.GetField(CASSHOTFileLineRow.Schema.DateOfArrival));
			AssertEquals("100805", result.GetField(CASSHOTFileLineRow.Schema.DateOfDelivery));
			AssertEquals("MGA", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("NRT", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0001400", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("USD", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000041560", result.GetField(CASSHOTFileImportLineRow.Schema.WeightCharges));
			AssertEquals("000000500000", result.GetField(CASSHOTFileImportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000230000", result.GetField(CASSHOTFileImportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000003000", result.GetField(CASSHOTFileImportLineRow.Schema.FeeAmount));
			AssertEquals("Y", result.GetField(CASSHOTFileImportLineRow.Schema.FeeCharged));
			AssertEquals("000000000077", result.GetField(CASSHOTFileImportLineRow.Schema.HandlingCharges));
			AssertEquals("440000000044", result.GetField(CASSHOTFileImportLineRow.Schema.StorageCharges));
			AssertEquals("120000000021", result.GetField(CASSHOTFileImportLineRow.Schema.OtherCharge1Amount));
			AssertEquals("670000000067", result.GetField(CASSHOTFileImportLineRow.Schema.OtherCharge2Amount));
			AssertEquals("140000000041", result.GetField(CASSHOTFileImportLineRow.Schema.MiscellaneousChargesAmount));

			rawDataRow = "IBO        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("IBO", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("006", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("70158756", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("JP054000004", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("100804", result.GetField(CASSHOTFileLineRow.Schema.DateOfArrival));
			AssertEquals("100805", result.GetField(CASSHOTFileLineRow.Schema.DateOfDelivery));
			AssertEquals("MGA", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("NRT", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0001400", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("USD", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000041560", result.GetField(CASSHOTFileImportLineRow.Schema.WeightCharges));
			AssertEquals("000000500000", result.GetField(CASSHOTFileImportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000230000", result.GetField(CASSHOTFileImportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000003000", result.GetField(CASSHOTFileImportLineRow.Schema.FeeAmount));
			AssertEquals("Y", result.GetField(CASSHOTFileImportLineRow.Schema.FeeCharged));
			AssertEquals("000000000077", result.GetField(CASSHOTFileImportLineRow.Schema.HandlingCharges));
			AssertEquals("440000000044", result.GetField(CASSHOTFileImportLineRow.Schema.StorageCharges));
			AssertEquals("120000000021", result.GetField(CASSHOTFileImportLineRow.Schema.OtherCharge1Amount));
			AssertEquals("670000000067", result.GetField(CASSHOTFileImportLineRow.Schema.OtherCharge2Amount));
			AssertEquals("140000000041", result.GetField(CASSHOTFileImportLineRow.Schema.MiscellaneousChargesAmount));

			rawDataRow = "IBR        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000";
			result = (CASSHOTFileDataRow)dataFormat.ConvertToRow(rawDataRow);
			AssertEquals("IBR", result.GetField(CASSHOTFileDataRow.Schema.RecordType));
			AssertEquals("006", result.GetField(CASSHOTFileLineRow.Schema.AirlinePrefix));
			AssertEquals("70158756", result.GetField(CASSHOTFileLineRow.Schema.AWBSerialNumber));
			AssertEquals("JP054000004", result.GetField(CASSHOTFileLineRow.Schema.AgentCode));
			AssertEquals("", result.GetField(CASSHOTFileLineRow.Schema.DateAWBExecution));
			AssertEquals("100804", result.GetField(CASSHOTFileLineRow.Schema.DateOfArrival));
			AssertEquals("100805", result.GetField(CASSHOTFileLineRow.Schema.DateOfDelivery));
			AssertEquals("MGA", result.GetField(CASSHOTFileLineRow.Schema.Origin));
			AssertEquals("NRT", result.GetField(CASSHOTFileLineRow.Schema.Destination));
			AssertEquals("0001400", result.GetField(CASSHOTFileLineRow.Schema.Weight));
			AssertEquals("K", result.GetField(CASSHOTFileLineRow.Schema.WeightIndicator));
			AssertEquals("USD", result.GetField(CASSHOTFileLineRow.Schema.CurrencyCode));
			AssertEquals("000000041560", result.GetField(CASSHOTFileImportLineRow.Schema.WeightCharges));
			AssertEquals("000000500000", result.GetField(CASSHOTFileImportLineRow.Schema.ChargesDueAgent));
			AssertEquals("000000230000", result.GetField(CASSHOTFileImportLineRow.Schema.ChargesDueCarrier));
			AssertEquals("000000003000", result.GetField(CASSHOTFileImportLineRow.Schema.FeeAmount));
			AssertEquals("Y", result.GetField(CASSHOTFileImportLineRow.Schema.FeeCharged));
			AssertEquals("000000000077", result.GetField(CASSHOTFileImportLineRow.Schema.HandlingCharges));
			AssertEquals("440000000044", result.GetField(CASSHOTFileImportLineRow.Schema.StorageCharges));
			AssertEquals("120000000021", result.GetField(CASSHOTFileImportLineRow.Schema.OtherCharge1Amount));
			AssertEquals("670000000067", result.GetField(CASSHOTFileImportLineRow.Schema.OtherCharge2Amount));
			AssertEquals("140000000041", result.GetField(CASSHOTFileImportLineRow.Schema.MiscellaneousChargesAmount));
		}
	}
}
