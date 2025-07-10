using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business
{
	public abstract class DependentBizOAutoCompleteHelper : AutoCompleteHelper
	{
		public DependentBizOAutoCompleteHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Additional Parameters

		public override string SerializeAdditionalParamsToString()
		{
			return ParentPK.ToString();
		}

		public override void RestoreAdditionalParamsFromSerializedString(string serializedParamsString)
		{
			ParentPK = new ZGuid(serializedParamsString);
		}

		public ZGuid ParentPK
		{
			get
			{
				return parentPK;
			}
			set
			{
				parentPK = value;
			}
		}
		ZGuid parentPK = ZGuid.Empty;

		#endregion
	}
}
