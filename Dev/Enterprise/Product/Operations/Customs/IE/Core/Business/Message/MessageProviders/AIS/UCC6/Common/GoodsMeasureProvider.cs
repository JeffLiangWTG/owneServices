using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class GoodsMeasureProvider : IGoodsMeasure
	{
		public GoodsMeasureProvider(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
		}

		readonly CusEntryLine entryLine;

		public decimal GrossMass => entryLine.EffectiveGrossWeight.InKilogramsSafe;

		public decimal NetMass => entryLine.EffectiveNetWeight.InKilogramsSafe;

		public decimal SupplementaryUnits => entryLine.SupplementaryQuantity;
	}
}
