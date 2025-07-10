namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSCusContainerCollection : EU.EMCS.Business.EMCSCusContainerCollection
	{
		public EMCSCusContainerCollection(EMCSJobDeclaration master)
			: base(master)
		{
		}

		public new EMCSCusContainer this[int index] => (EMCSCusContainer)Elements[index];

		public new EMCSCusContainer AddNew() => (EMCSCusContainer)base.AddNew();
	}
}

