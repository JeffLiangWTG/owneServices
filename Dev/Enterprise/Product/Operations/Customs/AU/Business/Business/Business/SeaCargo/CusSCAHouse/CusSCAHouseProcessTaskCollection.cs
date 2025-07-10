namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseProcessTaskCollection : Customs.Business.CusSCAHouseProcessTaskCollection
	{
		public CusSCAHouseProcessTaskCollection(CusSCAHouse parent) : base(parent)
		{
		}

		public new CusSCAHouse Parent
		{
			get { return (CusSCAHouse)base.Parent; }
		}

		public new CusSCAHouseProcessTask this[int index]
		{
			get { return (CusSCAHouseProcessTask)Elements[index]; }
		}

		public new CusSCAHouseProcessTask AddNew()
		{
			return (CusSCAHouseProcessTask)base.AddNew();
		}
	}
}
