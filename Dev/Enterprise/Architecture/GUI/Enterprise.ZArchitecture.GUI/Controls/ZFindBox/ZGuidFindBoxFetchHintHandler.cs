using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZGuidFindBoxFetchHintHandler : ZCodeFindBoxFetchHintHandler
	{
		public ZGuidFindBoxFetchHintHandler(IBindToList control, object dataSource)
			: base(control, dataSource)
		{
		}

		protected override void AddFetchHint(BusinessObject businessObject)
		{
			var type = GetListType(businessObject);
			if (type != null && type.IsSubclassOf(typeof(BusinessObject)) && businessObject.Factory != null)
			{
				businessObject.Factory.AddFetchHint(type, (ZGuid)businessObject[BindingMember]);
			}
		}
	}
}
