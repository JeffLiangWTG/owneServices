using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceRounding : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PriceRounding(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZDecimal PriceBreak
		{
			get { return priceBreak; }
			set { SetNonPersistentPropertyValue(PriceBreakInfo, ref priceBreak, value); }
		}
		ZDecimal priceBreak;

		public ZPropertyInfo PriceBreakInfo
		{
			get { return GetZPropertyInfo(nameof(PriceBreak)); }
		}

		public ZDecimal RoundingScale
		{
			get { return roundingScale; }
			set
			{
				SetNonPersistentPropertyValue(RoundingScaleInfo, ref roundingScale, value);
				if (!IsValidationSuspended)
				{
					ValidateRoundingScale();
				}
			}
		}
		ZDecimal roundingScale;

		public ZPropertyInfo RoundingScaleInfo
		{
			get { return GetZPropertyInfo(nameof(RoundingScale)); }
		}

		public static decimal Round(IEnumerable<PriceRounding> roundingList, decimal price)
		{
			var rounding = FindRounding(roundingList, price);
			if (rounding != null)
			{
				decimal absPrice = Utilities.Round(Math.Abs(price) / rounding.RoundingScale, 0) * rounding.RoundingScale;
				price = (price < 0) ? -absPrice : absPrice;
			}
			return price;
		}

		static PriceRounding FindRounding(IEnumerable<PriceRounding> roundingList, decimal price)
		{
			PriceRounding matchingRounding = null;
			decimal absPrice = Math.Abs(price);

			foreach (var rounding in roundingList)
			{
				if ((rounding.PriceBreak == 0m || absPrice < rounding.PriceBreak)
					&& (matchingRounding == null
						|| (matchingRounding.PriceBreak > rounding.PriceBreak && rounding.PriceBreak != 0)))
				{
					matchingRounding = rounding;
				}
			}

			return matchingRounding;
		}

		public static IEnumerable<PriceRounding> DecodePriceRoundingParams(string encodedText)
		{
			var result = new List<PriceRounding>();
			if (!string.IsNullOrEmpty(encodedText))
			{
				var pairs = encodedText.Split(',');
				foreach (var pair in pairs)
				{
					var keyAndValue = pair.Split('=');
					if (keyAndValue.Length == 2
						&& decimal.TryParse(keyAndValue[0].Trim(), out var priceBreak)
						&& decimal.TryParse(keyAndValue[1].Trim(), out var roundingScale)
						&& roundingScale > 0
						&& priceBreak >= 0
						&& !result.Any(x => x.PriceBreak == priceBreak))
					{
						result.Add(new PriceRounding(null) { PriceBreak = priceBreak, RoundingScale = roundingScale });
					}
					else
					{
						result.Clear();
						break;
					}
				}
			}

			if (result.Count == 0)
			{
				throw new ArgumentException("Expected comma separated list of unique {price break}={rounding scale}, where {price break} >= 0 and {rounding scale} > 0");
			}

			return result;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRoundingScale();
		}

		void ValidateRoundingScale()
		{
			RoundingScaleInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotZero(RoundingScaleInfo, "Rounding Scale");
		}
	}
}

