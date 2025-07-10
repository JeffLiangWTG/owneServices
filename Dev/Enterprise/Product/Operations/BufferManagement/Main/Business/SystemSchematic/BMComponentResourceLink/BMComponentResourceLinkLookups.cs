namespace Enterprise.BufferManagement.Business
{
	public class BMComponentResourceLinkLookups : AutoBMComponentResourceLinkLookups
	{
		public BMComponentResourceLinkLookups(AutoBMComponentResourceLink parent)
			: base(parent)
		{
		}

		public BMComponentCollection Components
		{
			get { return Factory.GetCachedValue("BMComponentResourceLinkLookups.Components", () => new BMComponentCollection(Factory)); }
		}
	}
}
