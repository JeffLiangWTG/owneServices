using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public abstract class CcsukOperationalActionSupporter : OperationalActionSupporter
	{
		public override string SingularElementNoun
		{
			get { return ElementNamePrefix + "Air Waybill"; }
		}

		public override string PluralElementNoun
		{
			get { return ElementNamePrefix + "Air Waybills"; }
		}

		protected virtual string ElementNamePrefix
		{
			get { return ""; }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(Enterprise.Services.OperationalActions.Support.ActionMethodProviderIDs.GbCcsuk);
		}

		protected override bool SupportsBulkUpdatesCore
		{
			get { return true; }
		}
	}
}
