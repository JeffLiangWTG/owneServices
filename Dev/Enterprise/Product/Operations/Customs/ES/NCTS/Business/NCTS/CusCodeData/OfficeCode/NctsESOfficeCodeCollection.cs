using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsESOfficeCodeCollection : NctsEuOfficeCodeCollection
	{
		public NctsESOfficeCodeCollection(NctsHeader master) : base(master) { }

		public NctsESOfficeCodeCollection(NctsDepartureMovementHeader master) : base(master) { }

		public new NctsESOfficeCode this[int index] => (NctsESOfficeCode)Elements[index];

		public new NctsESOfficeCode AddNew() => (NctsESOfficeCode)base.AddNew();

		protected new NctsESOfficeCode AddNew(Type type) => (NctsESOfficeCode)base.AddNew(type);

		protected override BusinessObject AddNewCore() => AddNew(typeof(NctsESOfficeCode));
	}
}
