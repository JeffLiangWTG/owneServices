using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AJ;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5AJDataProvider
	{
		public IGOVCBR5AJMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR5AJMessageData();

			result.NoticeNumber = response.Declaration.Id.Value;
			result.DeclarantID = response.Declaration.Submitter.Id.Value;
			result.ImporterID = response.Declaration.Importer.Id.Value;
			result.ImporterType = response.Declaration.Importer.Id.SchemeAgencyId.ToString();
			result.DeclarationNumberType = response.Declaration.TransactionNatureCode.Value;
			result.DutyTaxFreeType = response.Declaration.DutyTaxFee.TypeCode.Value;
			result.ApplicationNumber = response.Declaration.AdditionalDocument?.Id?.Value ?? ZString.Empty;
			result.CarrierID = response.Declaration.Carrier?.Id?.Value ?? ZString.Empty;
			result.AgentID = response.Declaration.Agent?.Id?.Value ?? ZString.Empty;

			if (response.Declaration.GoodsShipment.Count > 0)
			{
				var dutyTaxFee = response.Declaration.GoodsShipment[0].DutyTaxFee.FirstOrDefault(x => x.TypeCode.Value == MiscDisbursementChargeCodeList.Codes._1);
				result.PaymentNumber = dutyTaxFee?.Payment?.ReferenceId?.Value ?? ZString.Empty;
			}

			var goodsShipments = new List<GoodsShipment5AJ>();
			foreach (var goodsShipment in response.Declaration.GoodsShipment)
			{
				var goodsShipment5AJ = new GoodsShipment5AJ();
				goodsShipment5AJ.DeclarationNumber = goodsShipment.AdditionalDocument.Id.Value;
				foreach (var tax in goodsShipment.DutyTaxFee)
				{
					if (tax.TypeCode.Value == MiscDisbursementChargeCodeList.Codes._1)
					{
						goodsShipment5AJ.TemporaryOpeningFee = tax.Payment.PaymentAmount.Value;
					}
					else if (tax.TypeCode.Value == MiscDisbursementChargeCodeList.Codes._2)
					{
						goodsShipment5AJ.InspectionFee = tax.Payment?.PaymentAmount?.Value ?? ZInt.Zero;
					}
					else if (tax.TypeCode.Value == MiscDisbursementChargeCodeList.Codes._3)
					{
						goodsShipment5AJ.OtherFee = tax.Payment?.PaymentAmount?.Value ?? ZInt.Zero;
					}
				}
				goodsShipments.Add(goodsShipment5AJ);
			}
			result.GoodsShipments = goodsShipments.ToArray();

			return result;
		}
	}
}
