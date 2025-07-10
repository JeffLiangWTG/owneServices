using System.Collections;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.Business
{
	public class ControllerIDsList : AddressValidationList<ControllerInfo>
	{
		public ControllerIDsList()
		{
			BuildList();
		}

		protected override IEnumerable GetList()
		{
			return new ControllerList().All;
		}

		protected override string GetItemToString(ControllerInfo item)
		{
			return item.ID.Name;
		}
	}
}
