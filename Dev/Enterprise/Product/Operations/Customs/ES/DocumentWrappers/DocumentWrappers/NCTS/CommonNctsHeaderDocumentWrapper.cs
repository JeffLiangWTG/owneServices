using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using ESNctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS
{
	public static class CommonNctsHeaderDocumentWrapper
	{
		public static ZString GetNumberFormatSpain(IZType number, bool isInPhase5TransitionPeriod)
		{
			return number != null ? GetStringNumberFormatSpain(number, isInPhase5TransitionPeriod) : ZString.Empty;
		}

		static ZString GetStringNumberFormatSpain(IZType number, bool isInPhase5TransitionPeriod)
		{
			var esNumberFormat = CultureInfo.GetCultureInfo("es-ES").NumberFormat;
			var sep = esNumberFormat.NumberDecimalSeparator;
			var sep2 = esNumberFormat.NumberGroupSeparator;
			var strFormatted = ZString.Empty;
			switch (number)
			{
				case ZDecimal decimalNumber:
					strFormatted = decimalNumber.ToString(isInPhase5TransitionPeriod ? "N3" : "N6", esNumberFormat);
					break;
				case ZInt intNumber:
					strFormatted = intNumber.ToString("N0", esNumberFormat);
					break;
				case ZLong longNumber:
					strFormatted = longNumber.ToString("N0", esNumberFormat);
					break;
				case ZShort shortNumber:
					strFormatted = shortNumber.ToString("N0", esNumberFormat);
					break;
			}
			return strFormatted.Contains(sep) ? strFormatted.TrimEnd(sep.ToCharArray()) : strFormatted;
		}

		public static ZString GetDateFormatSpain(ZDateTime? date)
		{
			return date?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) ?? ZString.Empty;
		}

		public static ZString GetBox35GrossMass(ZDecimal grossMass, bool isInPhase5TransitionPeriod) => GetNumberFormatSpain(grossMass, isInPhase5TransitionPeriod);

		public static ZString GetTotalBox35GrossMass(NctsHeader nctsHeader)
		{
			ZDecimal totalGrossMass = nctsHeader.MovementHeader?.BM_GrossWeight ?? ZDecimal.Zero;
			return GetNumberFormatSpain(totalGrossMass, nctsHeader.IsInPhase5TransitionPeriod);
		}

		public static ZString GetBoxCOfficeOfDepartureData(NctsHeader nctsHeader)
		{
			var departureCustomsOffice = NctsEuOfficeCode.Load<NctsEuOfficeCode>(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			var arrivalDate = nctsHeader.MovementReferenceEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;

			var result = ZString.Empty;
			if (departureCustomsOffice != null)
			{
				result += departureCustomsOffice.CY_OfficeDescription + "\n" + departureCustomsOffice.CY_Data;
			}

			if (!arrivalDate.IsEmpty)
			{
				result += "\n" + GetDateFormatSpain(arrivalDate);
			}
			return result;
		}

		public static ZString GetBoxDResult(NctsHeader nctsHeader)
		{
			var clearanceCriteria = ((ESNctsHeader)nctsHeader).ClearanceCriteria;
			var isNormalOrSimplifiedProcesure = clearanceCriteria.Equals(ClearanceCriteriaCodeList.Codes.NormalProcedure) || clearanceCriteria.Equals(ClearanceCriteriaCodeList.Codes.SimplifiedProcedure);

			return isNormalOrSimplifiedProcesure ? ((ESNctsHeader)nctsHeader).ClearanceCriteriaDescription.Replace("[", "").Replace("]", "") : ZString.Empty;
		}

		public static ZString GetBoxDTimeLimitDate(NctsHeader nctsHeader)
		{
			return GetDateFormatSpain(((ESNctsHeader)nctsHeader).ArrivalLimit);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public static ZString GetBoxDClearance(NctsHeader nctsHeader)
		{
			var result = "----------------------------------------------------------------------------------------------------\nAutentificación";

			var csvClearance = ((ESNctsHeader)nctsHeader).ClearanceReferenceNumber;
			if (!csvClearance.IsEmpty)
			{
				result += ": " + csvClearance;
			}
			return result;
		}
	}
}
