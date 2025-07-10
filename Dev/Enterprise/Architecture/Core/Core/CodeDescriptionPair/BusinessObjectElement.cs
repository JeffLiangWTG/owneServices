using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Core
{
	public class BusinessObjectElement : CodeElement
	{
		public BusinessObjectElement(BusinessObject bizObject, string code, string description)
			: base(bizObject.PK, code, description)
		{
			BizObject = bizObject;
		}

		public readonly object BizObject;
	}
}
