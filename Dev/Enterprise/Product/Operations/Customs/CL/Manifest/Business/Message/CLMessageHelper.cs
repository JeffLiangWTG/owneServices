using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public static class CLMessageHelper
	{
		internal static List<Tuple<string, string>> GetResponseInformation(ZString refNumber, ZString status)
		{
			var valueList = new List<Tuple<string, string>>();
			valueList.Add(new Tuple<string, string>((NoResString)"Ref. Number", refNumber));
			valueList.Add(new Tuple<string, string>((NoResString)"Status", status));
			return valueList;
		}

		internal static EDIMessage GetEDIMessageRequest(ZString messageRequestNumber, BusinessObjectFactory factory)
		{
			EDIMessage result = null;
			if (!messageRequestNumber.IsEmpty)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CLCustoms);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageRequestNumber);
				const string indexName = "NR_RX__EM_MessageNum"; // there is no existing constant for this index name
				query.TableIndexHints.Add(new TableIndexHint(indexName));
				result = factory.Load<EDIMessage>(query).OrderBy(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		internal static AWBRequestMessage GetMASObjectOriginal(AsycudaBill bill)
		{
			var lastOriginalMessage = FindLastOriginalMessage(bill);
			if (lastOriginalMessage != null)
			{
				return MessageBuilderHelper.GetAWBRequest(lastOriginalMessage.EM_MessageText);
			}
			return null;
		}

		static CLMessage FindLastOriginalMessage(AsycudaBill bill)
		{
			return bill.Messages.Cast<CLMessage>()
				.Where(x => x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit
				&& x.EM_ApplicationCode == ApplicationCodeList.Codes.CLCustoms
				&& x.EM_Status == EDIMessage.Status.Sent
				&& x.EM_MessageType == MessageTypes.Codes.CHE).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
		}

		internal static AsycudaArrivalLine GetAsycudaArrivalLine(AsycudaBill bill)
		{
			var header = bill.Header;
			var list = new List<AsycudaArrivalLine>();
			foreach (var arrivalHeaders in header.ArrivalHeaders)
			{
				var arrivalLines = arrivalHeaders.ArrivalDetails.Cast<AsycudaArrivalLine>().Where(x => x.ATL_ABL_AsycudaBill == bill.PK && x.ArrivalHeader != null).ToList();
				list.AddRange(arrivalLines);
			}

			list = list.OrderByDescending(x => x.ArrivalHeader.ATH_ArrivalSequence).ToList();
			return list.Count > 0 ? list[0] : null;
		}

		#region Wrappers Unit Code Calculator

		internal static string VolumeUnitCodeCalculator(ZString uQ)
		{
			string res;
			switch (uQ)
			{
				case Core.Constants.Volume.CubicCentimeters:
					res = WrappersConstants.VolumeUnitCode.Cmq;
					break;
				case Core.Constants.Volume.CubicFeet:
					res = WrappersConstants.VolumeUnitCode.Ftq;
					break;
				case Core.Constants.Volume.CubicInches:
					res = WrappersConstants.VolumeUnitCode.Inq;
					break;
				case Core.Constants.Volume.Litre:
					res = WrappersConstants.VolumeUnitCode.Ltr;
					break;
				default:
					res = WrappersConstants.VolumeUnitCode.Mtq;
					break;
			}
			return res;
		}

		internal static string WeightUnitCodeCalculator(ZString uQ)
		{
			string res;
			switch (uQ)
			{
				case Core.Constants.Weight.Grams:
					res = WrappersConstants.WeightUnitCode.Grm;
					break;
				case Core.Constants.Weight.Hectograms:
					res = WrappersConstants.WeightUnitCode.Hgm;
					break;
				case Core.Constants.Weight.Kilotonnes:
					res = WrappersConstants.WeightUnitCode.Ktn;
					break;
				case Core.Constants.Weight.Ounces:
					res = WrappersConstants.WeightUnitCode.Onz;
					break;
				case Core.Constants.Weight.Pounds:
					res = WrappersConstants.WeightUnitCode.Lbr;
					break;
				case Core.Constants.Weight.Tonnes:
					res = WrappersConstants.WeightUnitCode.Tne;
					break;
				default:
					res = WrappersConstants.WeightUnitCode.Kgm;
					break;
			}
			return res;
		}

		#endregion

		#region Wrapper Convertions

		internal static ZString VolumeConvertion(ZString uQ, ZDecimal volume)
		{
			ZString res;
			switch (uQ)
			{
				case Core.Constants.Volume.CubicCentimeters:
				case Core.Constants.Volume.CubicFeet:
				case Core.Constants.Volume.CubicInches:
				case Core.Constants.Volume.CubicMetres:
				case Core.Constants.Volume.Litre:
					res = volume.ToString("0.00");
					break;
				default:
					res = (Core.Constants.Volume.ConvertSafe(volume, uQ, Core.Constants.Volume.CubicMetres)).ToString("0.00");
					break;
			}
			return res;
		}

		internal static ZString WeightConvertion(ZString uQ, ZDecimal weight)
		{
			ZString res;
			switch (uQ)
			{
				case Core.Constants.Weight.Grams:
				case Core.Constants.Weight.Hectograms:
				case Core.Constants.Weight.Kilograms:
				case Core.Constants.Weight.Kilotonnes:
				case Core.Constants.Weight.Ounces:
				case Core.Constants.Weight.Pounds:
				case Core.Constants.Weight.Tonnes:
					res = weight.ToString("0.000");
					break;
				default:
					res = (Core.Constants.Weight.ConvertSafe(weight, uQ, Core.Constants.Weight.Kilograms)).ToString("0.000");
					break;
			}
			return res;
		}

		#endregion
	}
}
