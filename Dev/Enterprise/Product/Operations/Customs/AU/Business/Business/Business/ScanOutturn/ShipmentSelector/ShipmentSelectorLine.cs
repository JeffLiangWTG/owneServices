using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ShipmentSelectorLine : AutoShipmentSelectorLine
	{
		protected ShipmentSelectorLine()
		{
			this.houseBills = new List<IScanHouseBillProvider>();
		}
		protected readonly List<IScanHouseBillProvider> houseBills;

		public IEnumerable<IScanHouseBillProvider> HouseBills
		{
			get { return houseBills; }
		}

		public virtual void AddStandardHouseBill(IScanHouseBillProvider houseBill)
		{
			houseBills.Add(houseBill);
		}

		public OutturnStatus OutturnStatus { get; set; }
		public UnderbondStatus UnderbondStatus { get; set; }

		public override ZString OutturnStatusText
		{
			get { return OutturnStatus.ToString(); }
		}

		public override ZString UnderbondStatusText
		{
			get { return UnderbondStatus.ToString(); }
		}

		protected override int Type_MaxLength { get { return CommonShipment.Schema.JS_ShipmentTypeMaxLength; } }
	}
}
