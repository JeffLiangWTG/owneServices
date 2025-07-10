using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MessageValidation : ExternalMessageValidation
	{
		public const string NonDutiableGSTApplicableWarning =
			@"You have entered this amount as non-dutiable, GST applicable. Any non-dutiable, but GST-applicable charge is treated as non-dutiable freight (OFT).
This amount will be sent as additional OFT and it will be non-dutiable in the customs value (FOB) calculation.
The CIF should include this figure as GST is calculated on the difference between the FOB and CIF, namely the T&I amount.";

		public MessageValidation(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		bool IsNewCMRCodeAndEqualToOldCode(ZPropertyInfo airCargoInfo, object freightData)
		{
			return ((((ZString)airCargoInfo.Value).ToUpper().Trim() == CMRMethodsOfPayment.Codes.Collect &&
				((ZString)freightData).ToUpper().Trim() == LegacyPaymentTypes.Collect)
				||
				(((ZString)airCargoInfo.Value).ToUpper().Trim() == CMRMethodsOfPayment.Codes.PrepaidOnly
				&& ((ZString)freightData).ToUpper().Trim() == LegacyPaymentTypes.PrePaid));
		}

		class LegacyPaymentTypes
		{
			public const string PrePaid = "PPD";
			public const string Collect = "CCX";
			public const string Other = "OTH";
		}

		/// <summary>
		/// Assumes that AirCargoInfo.PropertyType == FreightInfo.PropertyType
		/// </summary>
		/// <param name="airCargoInfo"></param>
		/// <param name="FreightInfo"></param>
		public void ValidateAirCargoDataDifferentFromFreight(ZPropertyInfo airCargoInfo, object freightData)
		{
			bool isDifferent = false;

			if (airCargoInfo.PropertyType == typeof(ZString))
			{
				if (airCargoInfo.Name == CusHAWB.Schema.CS_RX_NKGoodsCurrency)
				{
					ZGuid freightGuid = (ZGuid)freightData;
					var testFreightInfoCurr = Factory.Load<RefCurrency>(freightGuid);
					if (testFreightInfoCurr != null)
					{
						isDifferent = ((ZString)airCargoInfo.Value).ToUpper().Trim() != testFreightInfoCurr.RX_Code.ToUpper();
					}
					else
					{
						isDifferent = (ZString)airCargoInfo.Value != "";
					}
				}
				else if (airCargoInfo.Name == CusHAWBBase.Schema.CS_FreightPrepaidCollect)
				{
					isDifferent = !IsNewCMRCodeAndEqualToOldCode(airCargoInfo, freightData) && ((ZString)airCargoInfo.Value).ToUpper().Trim() != ((ZString)freightData).ToUpper().Trim();
				}
				else
				{
					isDifferent = ((ZString)airCargoInfo.Value).ToUpper().Trim() != ((ZString)freightData).ToUpper().Trim();
				}
			}
			else if (airCargoInfo.PropertyType == typeof(ZDateTime))
			{
				isDifferent = ((ZDateTime)airCargoInfo.Value).ToString("yyMMdd") != ((ZDateTime)freightData).ToString("yyMMdd");
			}
			else if (airCargoInfo.PropertyType == typeof(ZDecimal))
			{
				isDifferent = (ZDecimal)airCargoInfo.Value != (ZDecimal)freightData;
			}
			else if (airCargoInfo.PropertyType == typeof(ZInt) || airCargoInfo.PropertyType == typeof(ZShort))
			{
				int airCargoValue = int.Parse(airCargoInfo.Value.ToString());
				int freightValue = int.Parse(freightData.ToString());
				isDifferent = airCargoValue != freightValue;
			}
			else if (airCargoInfo.PropertyType == typeof(ZGuid))
			{
				isDifferent = (ZGuid)airCargoInfo.Value != (ZGuid)freightData;
			}

			if (isDifferent)
			{
				string airCargoData = airCargoInfo.Value.IsEmpty ? "Empty" : airCargoInfo.Value.ToString();
				string freightData2 = string.IsNullOrEmpty(freightData.ToString()) ? "empty" : freightData.ToString();
				airCargoInfo.AddWarning(airCargoData + " is different from the freight job data, " + freightData2 + ". Click 'Refresh AirCargo Data' menu if you want to synchronise data from freight.");
			}
		}

		public ZString ValidatePortType(ZString naturalKey, bool expectedAirPort, bool expectedSeaPort)
		{
			RefUNLOCO port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, naturalKey);
			return ValidatePortTypeCore(port, expectedAirPort, expectedSeaPort);
		}

		public ZString ValidatePortType(ZGuid key, bool expectedAirPort, bool expectedSeaPort)
		{
			RefUNLOCO port = Factory.Load<RefUNLOCO>(key);
			return ValidatePortTypeCore(port, expectedAirPort, expectedSeaPort);
		}

		ZString ValidatePortTypeCore(RefUNLOCO port, bool expectAirPort, bool expectSeaPort)
		{
			ZString result = "";
			if (port != null)
			{
				if (expectAirPort && !port.RL_HasAirport)
				{
					result = "Database indicates that " + port.Code + " doesn't have an airport.";
				}
				if (expectSeaPort && !port.RL_HasSeaport)
				{
					result = "Database indicates that " + port.Code + " doesn't have an seaport.";
				}
			}
			return result;
		}
	}
}
