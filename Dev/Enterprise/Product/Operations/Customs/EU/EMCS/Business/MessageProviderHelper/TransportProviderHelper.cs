namespace Enterprise.Customs.EU.EMCS.Business
{
	public class TransportProviderHelper
	{
		public TransportProviderHelper(EMCSCusContainer container)
		{
			this.container = container;
		}
		readonly EMCSCusContainer container;

		public string UnitCode => container.ZG_UnitCode;

		public string IdentityOfUnit => container.CO_ContainerNumber;

		public string CommercialSealIdentification => container.CO_Seal;
	}
}
