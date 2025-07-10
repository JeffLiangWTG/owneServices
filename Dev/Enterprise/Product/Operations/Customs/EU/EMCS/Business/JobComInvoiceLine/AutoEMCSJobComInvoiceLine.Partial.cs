namespace Enterprise.Customs.EU.EMCS.Business
{
	public abstract partial class AutoEMCSJobComInvoiceLine
	{
		protected EMCSAddInfoJobComInvoiceLine AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new EMCSAddInfoJobComInvoiceLine(JI_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		EMCSAddInfoJobComInvoiceLine fAddInfo;
	}
}
