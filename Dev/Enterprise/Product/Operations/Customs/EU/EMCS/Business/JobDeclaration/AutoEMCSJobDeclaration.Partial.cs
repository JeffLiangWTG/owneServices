namespace Enterprise.Customs.EU.EMCS.Business
{
	public abstract partial class AutoEMCSJobDeclaration
	{
		protected EMCSAddInfoJobDeclaration AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = GetNewAddInfo();
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		EMCSAddInfoJobDeclaration fAddInfo;

		public virtual EMCSAddInfoJobDeclaration GetNewAddInfo() => new EMCSAddInfoJobDeclaration((EMCSJobDeclaration)this);
	}
}
