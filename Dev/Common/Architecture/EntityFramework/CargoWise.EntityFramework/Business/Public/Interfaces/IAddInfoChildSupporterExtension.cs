using System.Linq;

namespace CargoWise.EntityFramework
{
	public static class IAddInfoChildSupporterExtension
	{
		public static T LoadOrCreateAddInfoChild<T>(this IAddInfoChildSupporter supporter, ref T addInfoChild)
			where T : BusinessObject
		{
			var factory = supporter?.Factory;
			if (factory != null && (addInfoChild == null || addInfoChild.IsDeleted) && !supporter.IsDeleted)
			{
				if (addInfoChild != null)
				{
					supporter.UnRegisterEditableChildObject(addInfoChild);
					supporter.UnRegisterListChangedCalledRefreshBinding(addInfoChild);
				}
				var foreignKeyColumn = supporter.ChildForeignKeyColumn;
				if (foreignKeyColumn == null)
				{
					addInfoChild = null;
				}
				else
				{
					var supporterPK = supporter.PK;
					var query = new ZQuery(foreignKeyColumn, supporterPK);
					query.FetchOnlyFromLocalCache = !supporter.IsInDatabase;
					var addInfoChildType = typeof(T);
					if (supporter is IAddInfoChildOverrideTypeSupporter overrideTypeSupporter)
					{
						addInfoChildType = overrideTypeSupporter.AddInfoChildType;
					}
					addInfoChild = (T)factory.Load(addInfoChildType, query).OrderBy(x => x.PK).FirstOrDefault();
					if (addInfoChild == null)
					{
						addInfoChild = (T)factory.New(addInfoChildType);
						using (addInfoChild.SuspendSettingHasChanges())
						using (addInfoChild.GetValidationSuspender())
						{
							addInfoChild[foreignKeyColumn] = supporterPK;
						}
					}

					supporter.RegisterEditableChildObject(addInfoChild);
					supporter.RegisterListChangedCalledRefreshBinding(addInfoChild);
				}
			}
			return addInfoChild;
		}
	}
}
