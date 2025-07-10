using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsFrOfficeCodeCollection : NctsEuOfficeCodeCollection
	{
		public NctsFrOfficeCodeCollection(NctsHeader master)
			: base(master)
		{
		}

		public NctsFrOfficeCodeCollection(NctsCommonMovementHeader master)
			: base(master)
		{
		}

		public new NctsFrOfficeCode this[int index] => (NctsFrOfficeCode)Elements[index];

		public new NctsFrOfficeCode AddNew()
		{
			return (NctsFrOfficeCode)base.AddNew();
		}

		protected new NctsFrOfficeCode AddNew(Type type)
		{
			return (NctsFrOfficeCode)base.AddNew(type);
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNew(typeof(NctsFrOfficeCode));
		}
	}
}
