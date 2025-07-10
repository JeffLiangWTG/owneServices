using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public static class NationalAdditionalCodeHelper
	{
		public static NationalAdditionalCode LoadWithOrder<TParent>(TParent parent, ZShort order)
			where TParent : BusinessObject, ISupplementaryCodeSupporter
		{
			var loader = new NationalAdditionalCode.Loader(parent.Factory);
			return loader.Load(parent, order);
		}

		public static NationalAdditionalCode LoadOrCreate<TParent>(ZString value, TParent parent, ZShort order, ZPropertyInfo targetPropertyInfo, NationalAdditionalCode bizObj)
			where TParent : BusinessObject
		{
			var loader = new NationalAdditionalCode.Loader(parent.Factory);
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
					oldValue = bizObj.CY_Code;
				}

				bizObj.CY_Code = value;
			}
			else if (bizObj != null)
			{
				oldValue = bizObj.CY_Code;
				bizObj.SuspendValidation();
				bizObj.CY_Code = ZString.Empty;
				bizObj.Delete();
				(parent as JobComInvoiceLine)?.Validation.ValidateJI_Tariff();
			}

			targetPropertyInfo.RefreshBinding(oldValue);

			return bizObj;
		}
	}
}
