
namespace Enterprise.BusinessObjectGenerator
{
	public class AutoBusinessObjectLivesInZArchitecture : AutoBusinessObject
	{
		public AutoBusinessObjectLivesInZArchitecture(BusinessObjectInfo info)
			: base(info)
		{
		}

		protected override string CodeForLookupsProperty
		{
			get { return null; }
		}
	}
}