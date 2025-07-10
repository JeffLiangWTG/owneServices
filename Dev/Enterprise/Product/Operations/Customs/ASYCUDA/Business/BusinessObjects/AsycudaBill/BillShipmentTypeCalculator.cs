using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.ASYCUDA.Business
{
	class BillShipmentTypeCalculator
	{
		public BillShipmentTypeCalculator(AsycudaBill bill)
		{
			this.bill = bill;
		}

		public string ShipmentTypeExplanationForValidation
		{
			get
			{
				var sb = new ZStringBuilder();
				var stl = bill.Factory.GetCachedValue<ShipmentTypeList>();
				var country = bill.CountryCode;
				if (!country.IsEmpty)
				{
					var billType = CalculateShipmentTypeFor(country);
					sb.AppendFormat("{0}: {1}/{2}", country, billType, stl.GetDescriptionFromCode(billType));
				}
				return sb.ToStringWithNewLineBetweenAppends();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]  // "It is safe to suppress a warning from this rule if the complexity cannot easily be reduced and the method is easy to understand, test, and maintain".  Yep. 
		public ZString CalculateShipmentTypeFor(string countryCode)
		{
			var header = bill.Header;
			var manifestLoad = header?.AMA_RL_NKPortOfLoading.Left(2) ?? ZString.Empty;
			var manifestDisc = header?.AMA_RL_NKPortOfDischarge.Left(2) ?? ZString.Empty;
			var billOrigin = bill.ABL_RL_NKOrigin.Left(2);
			var billDest = bill.ABL_RL_NKFinalDestination.Left(2);

			var isManifestLoadAsy = manifestLoad.IsSupportedCountries(bill.Factory);
			var isManifestDiscAsy = manifestDisc.IsSupportedCountries(bill.Factory);
			var isBillOriginAsy = billOrigin.IsSupportedCountries(bill.Factory);
			var isBillDestAsy = billDest.IsSupportedCountries(bill.Factory);

			// 16 mathematical combinations, not all will be correct for business. Group into fours. 

			if (isManifestLoadAsy && isManifestDiscAsy)  // group a
			{
				if (isBillOriginAsy && isBillDestAsy) // scenario 1
				{
					if (manifestLoad == countryCode)
					{
						return ShipmentTypeList.Codes.Export22;
					}

					if (manifestDisc == countryCode)
					{
						return ShipmentTypeList.Codes.Import23;
					}
				}
				else if (isBillOriginAsy && !isBillDestAsy) // scenario 2
				{
					if (manifestLoad == countryCode)
					{
						return ShipmentTypeList.Codes.Export22;
					}

					if (manifestDisc == countryCode)
					{
						return ShipmentTypeList.Codes.Transhipment28;
					}
				}
				else if (!isBillOriginAsy && isBillDestAsy) // scenario 3
				{
					if (manifestLoad == countryCode)
					{
						return ShipmentTypeList.Codes.Transhipment28;
					}

					if (manifestDisc == countryCode)
					{
						return ShipmentTypeList.Codes.Import23;
					}
				}
				else if (!isBillOriginAsy && !isBillDestAsy) // scenario 4
				{
					return ShipmentTypeList.Codes.Transhipment28;
				}
			}
			else if (isManifestLoadAsy && !isManifestDiscAsy)  // group b
			{
				if (isBillOriginAsy && isBillDestAsy) // scenario 5
				{
					if (manifestLoad == countryCode)
					{
						return ShipmentTypeList.Codes.Export22;
					}
					//discharge is not realistic says BP
				}
				else if (isBillOriginAsy && !isBillDestAsy) // scenario 6
				{
					if (manifestLoad == countryCode)
					{
						return ShipmentTypeList.Codes.Export22;
					}
					//if (manifestDisc == countryCode && IsRoutedThrough(countryCode)) return ShipmentTypeList.Codes.Transit24; // IMPOSSIBLE
				}
				else if (!isBillOriginAsy && isBillDestAsy) // scenario 7
				{
					if (manifestLoad == countryCode)
					{
						return ShipmentTypeList.Codes.Transhipment28;
					}
					// discharge not realistic says BP
				}
				else if (!isBillOriginAsy && !isBillDestAsy) // scenario 8
				{
					if (manifestLoad == countryCode)
					{
						return ShipmentTypeList.Codes.Transhipment28;
					}
				}
			}
			else if (!isManifestLoadAsy && isManifestDiscAsy)  // group c
			{
				if (isBillOriginAsy && isBillDestAsy) // scenario 9
				{
					//load is not realistic says BP
					if (manifestDisc == countryCode)
					{
						return ShipmentTypeList.Codes.Import23;
					}
				}
				else if (isBillOriginAsy && !isBillDestAsy) // scenario 10
				{
					//load is not realistic says BP					
					if (manifestDisc == countryCode)
					{
						return ShipmentTypeList.Codes.Transhipment28;
					}
				}
				else if (!isBillOriginAsy && isBillDestAsy) // scenario 11
				{
					//if (manifestLoad == countryCode) return ShipmentTypeList.Codes.Transit24; // IMPOSSIBLE
					if (billDest == countryCode)
					{
						return ShipmentTypeList.Codes.Import23;
					}

					if (manifestDisc == countryCode && billDest != countryCode)
					{
						return ShipmentTypeList.Codes.Transhipment28;
					}
				}
				else if (!isBillOriginAsy && !isBillDestAsy) // scenario 12
				{
					// if (manifestLoad == countryCode) return ShipmentTypeList.Codes.Transit24; //IMPOSSIBLE
					if (manifestDisc == countryCode)
					{
						return ShipmentTypeList.Codes.Transhipment28;
					}
				}
			}
			else if (!isManifestLoadAsy && !isManifestDiscAsy)  // group d
			{
				return ShipmentTypeList.Codes.Transit24;  // Scenarios 13-16
			}

			return "";  // messaging will insert ABL_ShipmentType here
		}

		readonly AsycudaBill bill;
	}
}
