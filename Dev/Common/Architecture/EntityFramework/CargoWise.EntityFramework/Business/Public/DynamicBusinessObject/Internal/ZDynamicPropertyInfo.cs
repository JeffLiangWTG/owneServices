namespace CargoWise.EntityFramework
{
	public sealed class ZDynamicPropertyInfo : ZPropertyInfo
	{
		internal ZDynamicPropertyInfo(BusinessObject bizObj, string name) : base(bizObj, name, null)
		{
		}

		public override bool SupportsMaxLength
		{
			get { return false; }
		}
	}
}
