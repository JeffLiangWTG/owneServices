using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _200000Line : RecordLine
	{
		public new class Constants : RecordLine.Constants
		{
			public static class TrackingNumber
			{
				public const int Length = 11;
				public const int Position = 50;
			}

			public static class ShipmentType
			{
				public const int Length = 1;
				public const int Position = 72;
			}

			public static class ShipmentWeightUnit
			{
				public const int Length = 3;
				public const int Position = 81;
			}

			public static class DeclaredValue
			{
				public const int Length = 9;
				public const int Position = 88;
			}

			public static class CurrencyCodeForDeclaredValue
			{
				public const int Length = 3;
				public const int Position = 97;
			}

			public static class Discount
			{
				public const int Length = 9;
				public const int Position = 106;
			}

			public static class InsuranceCharges
			{
				public const int Length = 9;
				public const int Position = 141;
			}

			public static class FreightCharges
			{
				public const int Length = 9;
				public const int Position = 153;
			}

			public static class CurrencyCodeForInvoiceTotal
			{
				public const int Length = 3;
				public const int Position = 175;
			}

			public static class ThirdPartyIndicator
			{
				public const int Length = 1;
				public const int Position = 179;
			}

			public static class ConsolidatedClearanceFlag
			{
				public const int Length = 1;
				public const int Position = 181;
			}

			public static class ServiceLevel
			{
				public const int Length = 1;
				public const int Position = 219;
			}

			public static class FreightCollectFlag
			{
				public const int Length = 1;
				public const int Position = 228;
			}

			public static class OtherCharges
			{
				public const int Length = 11;
				public const int Position = 241;
			}

			public static class DateShippedDDMMMYYYY
			{
				public const int Length = 9;
				public const int Position = 283;
			}

			public static class ExpandedInvoiceTotal
			{
				public const int Length = 11;
				public const int Position = 307;
			}

			public static class BillingTerms
			{
				public const int Length = 3;
				public const int Position = 318;
			}

			public static class DimensionalWeight
			{
				public const int Length = 5;
				public const int Position = 331;
			}

			public static class DimensionalWeightUnit
			{
				public const int Length = 3;
				public const int Position = 336;
			}
		}

		public _200000Line(ZString value)
			: base(value)
		{
		}

		public ZString TrackingNumber
		{
			get { return Value.SubstringSafe(Constants.TrackingNumber.Position, Constants.TrackingNumber.Length).Trim(); }
		}

		public ZString ServiceLevel => Value.SubstringSafe(Constants.ServiceLevel.Position, Constants.ServiceLevel.Length).Trim().ToUpper();

		public ZDateTime ShippedOnBoardDate
		{
			get { return ToZDateTime(Value.SubstringSafe(Constants.DateShippedDDMMMYYYY.Position, Constants.DateShippedDDMMMYYYY.Length).Trim()); }
		}

		public string IncoTerm
		{
			get { return "FOB"; }
		}

		public string GoodsDescription
		{
			get
			{
				string result = "";

				if (ShipmentType != ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments)
				{
					result = "Documents Only";
				}

				return result;
			}
		}

		public string ShipmentWeightUnit
		{
			get
			{
				string result = Value.SubstringSafe(Constants.ShipmentWeightUnit.Position, Constants.ShipmentWeightUnit.Length).Trim();
				return result == "LBS" ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;
			}
		}

		public ZDecimal DeclaredValue
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.DeclaredValue.Position, Constants.DeclaredValue.Length)) / 100; }
		}

		public ZString CurrencyCodeForDeclaredValue
		{
			get
			{
				var result = Value.SubstringSafe(Constants.CurrencyCodeForDeclaredValue.Position, Constants.CurrencyCodeForDeclaredValue.Length);
				return BadCurrencyCode.ContainsKey(result) ? (ZString)BadCurrencyCode[result] : result;
			}
		}

		public string ShipmentType
		{
			get
			{
				string result = "";

				string shipmentType = Value.SubstringSafe(Constants.ShipmentType.Position, Constants.ShipmentType.Length).Trim();
				switch (shipmentType)
				{
					case "D":
						result = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
						break;
					case "N":
						result = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
						break;
					case "L":
						result = ShipmentTypeCodeDescriptionPairList.Codes.Letter;
						break;
				}

				return result;
			}
		}

		public ZDecimal ExpandedInvoiceTotal
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.ExpandedInvoiceTotal.Position, Constants.ExpandedInvoiceTotal.Length)) / 100; }
		}

		public ZDecimal Insurance
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.InsuranceCharges.Position, Constants.InsuranceCharges.Length)) / 100; }
		}

		public ZDecimal Freight
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.FreightCharges.Position, Constants.FreightCharges.Length)) / 100; }
		}

		public ZDecimal OtherCharges
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.OtherCharges.Position, Constants.OtherCharges.Length)) / 100; }
		}

		public ZDecimal Discount
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.Discount.Position, Constants.Discount.Length)) / 100; }
		}

		public ZDecimal GoodsValue
		{
			get { return ExpandedInvoiceTotal - Insurance - Freight - OtherCharges + Discount; }
		}

		public string CurrencyCodeForInvoiceTotal
		{
			get
			{
				string result = Value.SubstringSafe(Constants.CurrencyCodeForInvoiceTotal.Position, Constants.CurrencyCodeForInvoiceTotal.Length).Trim();
				return BadCurrencyCode.ContainsKey(result) ? BadCurrencyCode[result] : result;
			}
		}

		public string ThirdPartyIndicator
		{
			get { return Value.SubstringSafe(Constants.ThirdPartyIndicator.Position, Constants.ThirdPartyIndicator.Length); }
		}

		public bool FreightCollect
		{
			get { return ToZBool(Value.SubstringSafe(Constants.FreightCollectFlag.Position, Constants.FreightCollectFlag.Length)); }
		}

		public string ConsolidatedClearanceFlag
		{
			get { return Value.SubstringSafe(Constants.ConsolidatedClearanceFlag.Position, Constants.ConsolidatedClearanceFlag.Length); }
		}

		public bool IsGCCLead
		{
			get { return IsConsolidated && (ConsolidatedClearanceFlag == "L" || IsVirtual); }
		}

		public bool IsGCCChild
		{
			get { return IsConsolidated && ConsolidatedClearanceFlag == "H"; }
		}

		public bool IsConsolidated
		{
			get { return DutyType == DutyTypeCodeDescriptionPairList.Codes.GCC; }
		}

		public bool IsVirtual
		{
			get { return ConsolidatedClearanceFlag == "V"; }
		}

		public ZString BillingTerms
		{
			get { return Value.SubstringSafe(Constants.BillingTerms.Position, Constants.BillingTerms.Length).Trim(); }
		}

		public ZDecimal DimensionalWeight
		{
			get
			{
				ZDecimal result = 0m;
				ZDecimal weightValue = ToZDecimal(Value.SubstringSafe(Constants.DimensionalWeight.Position, Constants.DimensionalWeight.Length));
				if (DimensionalWeightUnit == Core.Constants.Weight.Pounds)
				{
					result = Core.Constants.Weight.Convert(weightValue, Core.Constants.Weight.Pounds, Core.Constants.Weight.Kilograms);
				}
				else
				{
					result = weightValue / 10m;
				}

				return result;
			}
		}

		string DimensionalWeightUnit
		{
			get
			{
				string result = Value.SubstringSafe(Constants.DimensionalWeightUnit.Position, Constants.DimensionalWeightUnit.Length).Trim();
				return result == "LBS" ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;
			}
		}

		static IReadOnlyDictionary<string, string> BadCurrencyCode => badCurrencyCode.Value;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Readonly and ThreadSafe")]
		static Lazy<IReadOnlyDictionary<string, string>> badCurrencyCode => new Lazy<IReadOnlyDictionary<string, string>>(() =>
			new Dictionary<string, string>
				{
					{ "UKL", "GBP" },
					{ "AUS", "AUD" },
					{ "RMB", "CNY" }
				});
	}
}
