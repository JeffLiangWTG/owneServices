using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class DatesPlacesProvider : IDatesPlaces
	{
		readonly RefundApplicationMessageSendingAction sendingAction;
		readonly CusEntryInstruction instruction;

		public DatesPlacesProvider(RefundApplicationMessageSendingAction sendingAction, CusEntryInstruction instruction)
		{
			this.sendingAction = sendingAction;
			this.instruction = instruction;
		}

		public DateTime Date => ZDate.Today.ToDateTime();

		public string OfficeOfDebt => sendingAction.OfficeOfDebt;

		public string OfficeOfResponsibility => sendingAction.OfficeOfResponsibility;

		public IRF415GoodsLocation LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, () => new RF415GoodsLocationProvider(instruction.GoodsLocation));
		CachedValue<IRF415GoodsLocation> locationOfGoods;
	}
}
