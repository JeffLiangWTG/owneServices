using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public sealed class AdditionalProcedureCodeCollection : EU.Business.AdditionalProcedureCodeCollection
	{
		public AdditionalProcedureCodeCollection(IAdditionalProcedureParent master) : base(master)
		{
		}

		public new AdditionalProcedureCode this[int i] => (AdditionalProcedureCode)base[i];

		public new AdditionalProcedureCode AddNew() => (AdditionalProcedureCode)base.AddNew();
	}
}
