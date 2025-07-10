namespace Enterprise.BusinessObjectGenerator
{
	class AutoAddInfoBusinessObjectLookups : AutoBusinessObjectLookups
	{
		public AutoAddInfoBusinessObjectLookups(BusinessObjectInfo info, AutoPropertyList autoProperties)
			: base(info, autoProperties)
		{
		}

		protected override string InheritsFrom => Info.BaseLookupsClassName;

		new AddInfoBusinessObjectInfo Info => (AddInfoBusinessObjectInfo)base.Info;
	}
}
