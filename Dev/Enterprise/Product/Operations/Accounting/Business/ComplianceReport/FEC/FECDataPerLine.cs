using System;
using System.Data;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.FEC
{
	internal class FECDataPerLine
	{
		internal FECDataPerLine(IDataRecord rawData)
		{
			// Performance note: This constructor is very slow. If overall performance is too bad, then consider using IDataRecord.GetValues instead.
			#region SuppressResourceStringsCheckRegion

			Ledger = DataReaderGetZString(rawData, "AH_Ledger");
			TransactionType = DataReaderGetZString(rawData, "AH_TransactionType");

			OrganisationCode = DataReaderGetZString(rawData, "CompAuxNum");
			OrganisationName = DataReaderGetZString(rawData, "CompAuxLib");

			PieceDate = DataReaderGetZDateTime(rawData, "PieceDate");
			PieceRef = DataReaderGetZString(rawData, "PieceRef");
			InternalReference = DataReaderGetZString(rawData, "InternalReference");

			EcritureLib = DataReaderGetZString(rawData, "EcritureLib");
			PostDate = DataReaderGetZDateTime(rawData, "EcritureDate");
			ValidDate = DataReaderGetZDateTime(rawData, "ValidDate");

			NetAmountLocal = DataReaderGetZDecimal(rawData, "NetAmountLocal");
			TaxAmountLocal = DataReaderGetZDecimal(rawData, "TaxAmountLocal");
			ReportSubCode = DataReaderGetZString(rawData, "ACQ_ReportSubCode");

			LineType = DataReaderGetZString(rawData, "LineType");
			VatBasis = DataReaderGetZString(rawData, "VATBasis");
			InputVatRecoverable = DataReaderGetZDecimal(rawData, "InputVatRecoverable");
			AccountPK = DataReaderGetGuid(rawData, "AccountPK");

			IDevise = DataReaderGetZString(rawData, "IDevise");
			GrossAmountForeign = DataReaderGetZDecimal(rawData, "GrossAmountForeign");
			ExchangeRateForeign = DataReaderGetZDecimal(rawData, "ExchangeRate");
			SequenceNumber = DataReaderGetSmallInt(rawData, "AL_Sequence");

			EcritureLet = DataReaderGetZString(rawData, "EcritureLet");
			DateLet = DataReaderGetZDateTime(rawData, "DateLet");

			#endregion
		}

		internal FECDataPerLine(ZString ledger, ZString transactionType, ZDateTime postDate, ZGuid accountPK, ZString organisationCode, ZString organisationName, ZString pieceRef,
			ZDateTime pieceDate, ZString ecritureLib, ZDecimal netAmountLocal, ZDecimal taxAmountLocal, ZDateTime validDate, ZString iDevise, ZDecimal grossAmountForeign,
			ZDecimal exchangeRateForeign, ZString reportSubCode, ZString lineType, ZString vatBasis, ZDecimal inputVatRecoverable, ZInt sequenceNumber, ZString internalReference)
		{
			Ledger = ledger;
			TransactionType = transactionType;
			PostDate = postDate;
			AccountPK = accountPK;
			OrganisationCode = organisationCode;
			OrganisationName = organisationName;
			PieceRef = pieceRef;
			PieceDate = pieceDate;
			EcritureLib = ecritureLib;
			NetAmountLocal = netAmountLocal;
			TaxAmountLocal = taxAmountLocal;
			ValidDate = validDate;
			IDevise = iDevise;
			GrossAmountForeign = grossAmountForeign;
			ExchangeRateForeign = exchangeRateForeign;
			ReportSubCode = reportSubCode;
			LineType = lineType;
			VatBasis = vatBasis;
			InputVatRecoverable = inputVatRecoverable;
			SequenceNumber = sequenceNumber;
			InternalReference = internalReference;
		}

		internal ZString Ledger { get; }
		internal ZString TransactionType { get; }
		internal ZDateTime PostDate { get; }            // EcritureDate
		internal ZGuid AccountPK { get; }               // fill CompteNum + CompteLib
		internal ZString OrganisationCode { get; }      // CompAuxNum
		internal ZString OrganisationName { get; }      // CompAuxLib

		internal ZString PieceRef { get; }
		internal ZDateTime PieceDate { get; }
		internal ZString EcritureLib { get; }
		internal ZDecimal NetAmountLocal { get; }       // fill Debit + Credit
		internal ZDecimal TaxAmountLocal { get; }
		internal ZString EcritureLet { get; }
		internal ZDateTime DateLet { get; }
		internal ZDateTime ValidDate { get; }
		internal ZString IDevise { get; }
		internal ZDecimal GrossAmountForeign { get; }	// MontantDevise
		internal ZDecimal ExchangeRateForeign { get; }

		internal ZString ReportSubCode { get; }
		internal ZString LineType { get; }
		internal ZString VatBasis { get; }
		internal ZDecimal InputVatRecoverable { get; }
		internal ZInt SequenceNumber { get; }
		internal ZString InternalReference { get; }

		// It is very unlikely that a column name doesn't exist in the DataRecord because we provide fixed column names only.
		ZString DataReaderGetZString(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? ZString.Empty : (ZString)(string)record[columnName];
		ZInt DataReaderGetSmallInt(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? ZInt.Zero : (ZInt)(short)record[columnName];
		ZDecimal DataReaderGetZDecimal(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? ZDecimal.Zero : (ZDecimal)(decimal)record[columnName];
		ZDateTime DataReaderGetZDateTime(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? ZDateTime.Empty : (DateTime)record[columnName];
		Guid DataReaderGetGuid(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? Guid.Empty : (Guid)record[columnName];
	}
}
