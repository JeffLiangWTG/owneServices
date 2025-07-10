using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public static class ReceptacleHelper
	{
		public static Receptacle LoadOrCreate<TParent>(ZString value, TParent parent, ZShort order, ZPropertyInfo targetPropertyInfo, Receptacle bizObj)
			where TParent : BusinessObject
		{
			var loader = new Receptacle.Loader(parent.Factory);
			var oldValue = ZString.Empty;
			if (!value.IsEmpty)
			{
				if (bizObj == null)
				{
					bizObj = loader.LoadOrCreate(parent, order);
					parent.RegisterEditableChildObject(bizObj);
				}
				else
				{
					oldValue = bizObj.CY_Data;
				}

				bizObj.CY_Data = value;
			}
			else if (bizObj != null)
			{
				oldValue = bizObj.CY_Data;
				bizObj.SuspendValidation();
				bizObj.CY_Data = ZString.Empty;
				bizObj.Delete();
			}

			targetPropertyInfo.RefreshBinding(oldValue);

			return bizObj;
		}
	}
}
