using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoCusEntryHeader : EU.Business.Declaration.AddInfoCusEntryHeader
	{
		public AddInfoCusEntryHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
			IsUpdateRelatedPropertyInfoDisabled = true;
		}

		public new CusEntryHeader Parent => (CusEntryHeader)base.Parent;
	}
}
