using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocStaffCollection : DocumentWrapperCollection
	{
		public DocStaffCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocStaff this[int index]
		{
			get
			{
				return (DocStaff)base[index];
			}
		}
	}
}

