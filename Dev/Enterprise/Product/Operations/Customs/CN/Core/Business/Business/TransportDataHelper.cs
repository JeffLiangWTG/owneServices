using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class TransportDataHelper
	{
		public TransportDataHelper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		internal IValidationModeProvider ValidationModeProvider => declaration;

		#region Transport Codes

		public bool IsSea => IsCrossBorder && declaration.JE_TransportMode == Core.Constants.TransportModes.Sea;
		public bool IsRail => IsCrossBorder && declaration.JE_TransportMode == Core.Constants.TransportModes.Rail;
		public bool IsRoad => IsCrossBorder && declaration.JE_TransportMode == Core.Constants.TransportModes.Road;
		public bool IsAir => IsCrossBorder && declaration.JE_TransportMode == Core.Constants.TransportModes.Air;
		public bool IsMail => IsCrossBorder && declaration.JE_TransportMode == Core.Constants.TransportModes.Mail;
		public bool IsPipeline => IsCrossBorder && declaration.JE_TransportMode == Core.Constants.TransportModes.FixedTransportInstallations;
		public bool IsPassengerCarried => IsCrossBorder && declaration.JE_TransportMode == Core.Constants.TransportModes.PassengerHandCarried;
		public bool IsCrossBorder => !IsLoadingInChina || !IsDischargeInChina;
		public bool IsLoadingInChina => IsPortInChina(declaration.JE_RL_NKPortOfLoading);
		public bool IsDischargeInChina => IsPortInChina(declaration.JE_RL_NKPortOfArrival);

		bool IsPortInChina(ZString port)
		{
			return port.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region Bill Of Lading

		public bool ShouldBillOfLadingKeepEmpty => !IsCrossBorder || IsPipeline || IsPassengerCarried;

		public void DefaultBillOfLadingOnEntryInstructions()
		{
			declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(UpdateBillOfLoading);
		}

		public void UpdateBillOfLoading(CusEntryInstruction cusEntryInstruction)
		{
			ZString billOfLoading = ZString.Empty;
			if (!ShouldBillOfLadingKeepEmpty)
			{
				if (IsAir)
				{
					var houseBill = declaration.IsImport ? declaration.JE_HouseBill : declaration.JE_HouseBill.Right(8);
					billOfLoading = declaration.JE_MasterBill + (houseBill.IsEmpty ? string.Empty : "_" + houseBill);
				}
				else if (IsMail || IsRail)
				{
					billOfLoading = declaration.JE_MasterBill;
				}
			}
			cusEntryInstruction.BillOfLading = billOfLoading.Left(cusEntryInstruction.BillOfLadingInfo.MaxLength);
		}

		public void ValidateBillOfLadingOnEntryInstructions(ZPropertyInfo targetInfo)
		{
			if (!ShouldBillOfLadingKeepEmpty)
			{
				targetInfo.AddNotificationIfNotEntered(ValidationModeProvider);

				if (!targetInfo.Value.IsEmpty)
				{
					ZString formatMsg = ZString.Empty;
					ZString formatReg = ZString.Empty;

					if (IsAir)
					{
						if (declaration.IsImport)
						{
							formatReg = RegexPatterns.BLNumberformatAirImp;
							formatMsg = bLNumberFormatMessageAirImp;
						}
						else if (declaration.IsExport)
						{
							formatReg = RegexPatterns.BLNumberFormatAirExp;
							formatMsg = bLNumberFormatMessageAirExp;
						}
					}
					else if (IsSea)
					{
						formatReg = RegexPatterns.BLNumberFormatSea;
						formatMsg = bLNumberFormatMessageSea;
					}
					else if (IsMail)
					{
						formatReg = RegexPatterns.BLNumberFormatMail;
						formatMsg = bLNumberFormatMessageMail;
					}

					if (!formatReg.IsEmpty && !Regex.IsMatch((ZString)targetInfo.Value, formatReg))
					{
						targetInfo.AddNotification(formatMsg, ValidationModeProvider);
					}
				}
			}
			else
			{
				targetInfo.AddNotificationIfIsEntered(ValidationModeProvider);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regex Pattern")]
		static class RegexPatterns
		{
			internal const string BLNumberformatAirImp = @"^\d{11}(_[a-zA-Z0-9]+)?$";

			internal const string BLNumberFormatAirExp = @"^\d{11}(_[a-zA-Z0-9]{1,8})?$";

			internal const string BLNumberFormatSea = @"^[a-zA-Z0-9][a-zA-Z0-9*]+[a-zA-Z0-9]$";

			internal const string BLNumberFormatMail = @"^[a-zA-Z0-9]+$";
		}

		readonly ZString bLNumberFormatMessageAirImp = ResString.GetMultilingualString("B2ADEEAA-63E4-4557-AFC9-EFFF6B5BFE97", "B/L No. should be 11 digits followed by \"_\" plus alphanumeric characters, or 11 digits.");
		readonly ZString bLNumberFormatMessageAirExp = ResString.GetMultilingualString("339801B5-4DD3-4F5D-B904-6E6147403913", "B/L No. should be 11 digits followed by \"_\" plus no more than 8 alphanumeric characters, or 11 digits.");
		readonly ZString bLNumberFormatMessageSea = ResString.GetMultilingualString("428ACB64-78C1-4FD0-8049-71C452AA87B3", "B/L No. should consist of alphanumeric characters and asterisk(*), and may not begin or end with asterisk.");
		readonly ZString bLNumberFormatMessageMail = ResString.GetMultilingualString("19F0FBA7-6D98-40B4-9379-47ADEAEB0A62", "B/L No. should be alphanumeric characters");

		public ZString PopulateBillNumber(CusEntryHeader entryHeader)
		{
			ZString result = ZString.Empty;

			if (declaration.IsCustomsTransit)
			{
				if (declaration.IsImport)
				{
					if (declaration.IsInlandRoadTransport)
					{
						result = declaration.JE_VesselInland;
					}

					if (result.IsEmpty && (declaration.IsDirectTransition || declaration.IsTransshipment) && (declaration.IsSea || declaration.IsRail || declaration.IsAir))
					{
						result = GetBillOfLading(entryHeader);
					}
				}
				else if (declaration.IsExport)
				{
					if (declaration.IsDeclaringInAdvance && declaration.IsInlandRoadTransport)
					{
						result = declaration.JE_VesselInland;
					}

					if (result.IsEmpty && declaration.IsTransshipment && declaration.IsSea)
					{
						result = GetBillOfLading(entryHeader);
					}
				}
			}
			else
			{
				result = GetBillOfLading(entryHeader);
			}

			return result;

			ZString GetBillOfLading(CusEntryHeader cusEntryHeader)
			{
				return cusEntryHeader.EntryInstruction?.BillOfLading ?? ZString.Empty;
			}
		}

		#endregion

		#region Vessel

		public ZString PopulateVesselName()
		{
			var result = ZString.Empty;

			if (declaration.IsCustomsTransit)
			{
				if (declaration.IsImport)
				{
					if (declaration.IsTransshipment)
					{
						if (declaration.IsSea || declaration.IsRail)
						{
							result = declaration.JE_VesselName;
						}
						else if (declaration.IsAir)
						{
							result = "@";
						}
						else
						{
							result = GetReferenceNumber();
						}
					}
					else
					{
						result = GetReferenceNumber();
					}
				}
				else if (declaration.IsExport)
				{
					if (declaration.IsTransshipment && declaration.IsSea)
					{
						if (declaration.IsInlandWaterwayTransport)
						{
							result = declaration.JE_VesselInland;
						}
						else if (declaration.IsInlandRoadTransport || declaration.IsInlandRailTransport)
						{
							result = declaration.JE_CustomsOffice + declaration.JE_VesselInland;
						}
					}
					else if ((declaration.IsSea || declaration.IsAir || declaration.IsRail) && declaration.CustomsEntryInstructions.Count > 1)
					{
						result = "@";
					}
					else
					{
						result = GetReferenceNumber();
					}
				}
			}
			else
			{
				if (IsSea || IsRail)
				{
					result = declaration.JE_VesselName;
				}
				else if (IsMail)
				{
					result = declaration.JE_MasterBill;
				}
				else if (IsPipeline)
				{
					result = VesselPipeline;
				}
				else if (IsPassengerCarried)
				{
					result = PassengerCarried;
				}
			}

			return result;

			ZString GetReferenceNumber()
			{
				var referenceNumber = declaration.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceNumberTypes.Codes.GoodsCarriedListNo)?.CE_EntryNum ?? ZString.Empty;
				if (referenceNumber.IsEmpty)
				{
					referenceNumber = declaration.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceNumberTypes.Codes.DTDPreNumber)?.CE_EntryNum ?? ZString.Empty;
				}
				if (!referenceNumber.IsEmpty)
				{
					referenceNumber = "@" + referenceNumber;
				}

				return referenceNumber;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant content string")]
		public const string VesselPipeline = "管道";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant content string")]
		public const string PassengerCarried = "旅客携带";

		#endregion

		#region Voyage

		public ZString PopulateVoyage(CusEntryHeader entryHeader)
		{
			var result = ZString.Empty;

			if (declaration.IsCustomsTransit)
			{
				if (declaration.IsImport)
				{
					if (declaration.IsTransshipment && declaration.IsSea)
					{
						result = "@" + declaration.JE_VoyageFlightNo;
					}
					else if (declaration.IsRail)
					{
						result = "@" + declaration.JE_DateOfArrival.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
					}
				}
				else if (declaration.IsExport)
				{
					if (declaration.IsTransshipment && declaration.IsSea)
					{
						if (declaration.IsInlandWaterwayTransport)
						{
							result = declaration.JE_VoyageInland;
						}
						else if (declaration.IsInlandRoadTransport || declaration.IsInlandRailTransport)
						{
							result = declaration.JE_ExportDate.ToString("yyMMdd", CultureInfo.InvariantCulture);
						}
					}
				}
			}
			else
			{
				if (IsSea)
				{
					result = declaration.JE_VoyageFlightNo;
				}
				else if (IsMail || IsRail)
				{
					result = entryHeader.ImportOrExportDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				}
				else if (IsRoad)
				{
					result = declaration.JE_MasterBill;
				}
			}

			return result;
		}

		#endregion
	}
}
