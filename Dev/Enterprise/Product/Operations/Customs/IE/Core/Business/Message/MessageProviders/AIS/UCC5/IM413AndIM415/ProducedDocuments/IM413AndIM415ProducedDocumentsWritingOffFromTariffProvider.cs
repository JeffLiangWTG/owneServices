using System;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider : IGoodsShipmentItemTypeProducedDocumentsWritingOff
	{
		public static IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider New(CusLineTariffDetail tariff)
		{
			IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider result = null;
			if (tariff != null)
			{
				result = new IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider(tariff);
			}
			return result;
		}

		IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider(CusLineTariffDetail tariff)
		{
			this.tariff = tariff;
		}
		readonly CusLineTariffDetail tariff;

		public string Id => null;

		public string Type => null;

		public string IssuingAuthorityNameSubmitter => null;

		public string IssuingAuthorityNameRoleCode => null;

		public DateTime? DateOfValidity => null;

		public string MeasurementUnit => tariff.BZ_UQ1;

		public decimal Quantity => tariff.BZ_Qty1;

		public string Currency => null;

		public decimal Amount => 0;
	}
}
