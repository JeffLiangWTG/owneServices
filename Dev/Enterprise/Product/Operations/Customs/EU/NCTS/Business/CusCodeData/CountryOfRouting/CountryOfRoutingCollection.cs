using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CountryOfRoutingCollection<T> : CusCodeDataCollection<T> where T : CountryOfRouting
	{
		public CountryOfRoutingCollection(NctsHeader master)
			: base(master, CusCodeDataTypeList.Codes.CountryOfRouting)
		{
			MaxCountValidationWithMessageErrorEnable(99, Res.GetString("F90328D4-FD67-4FD2-8CBB-82287FB2D7EA", "You may enter a maximum of 99 Countries."));
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			Master.CountryOfRoutingLineNumberGenerator.RecalculateWhenAdded((T)child);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			Master.CountryOfRoutingLineNumberGenerator.ReCalculateAll();
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			var master = Master;
			if (master.IsPhase5Departure)
			{
				var movement = master.MovementHeader;
				movement.MarkAsNeedingValidation();
				movement.Validation.ValidateBM_TypeOfSecurity();
			}
		}

		new NctsHeader Master => (NctsHeader)base.Master;
	}
}
