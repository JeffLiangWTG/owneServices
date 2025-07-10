using System;
using System.Reflection;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public abstract class BusinessObjectWrapper : NonPersistentBusinessObject, ILinkable
	{
		protected BusinessObjectWrapper(BusinessObject businessObjectToWrap) : base(businessObjectToWrap.Factory)
		{
			WrappedBusinessObject = businessObjectToWrap;
			manager.Add(this);
		}

		public static BusinessObjectWrapper Load(Type wrapperType, BusinessObject businessObjectToWrap)
		{
			BusinessObjectWrapper result = null;
			BusinessObjectWrapperManager manager = businessObjectToWrap.Factory.WrapperManager;

			foreach (BusinessObjectWrapper wrapper in manager.GetWrappers(businessObjectToWrap))
			{
				if (wrapper.GetType() == wrapperType)
				{
					result = wrapper;
					break;
				}
			}
			if (result == null)
			{
				result = (BusinessObjectWrapper)Activator.CreateInstance(wrapperType, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { businessObjectToWrap }, null);
			}
			return result;
		}

		public override void Delete()
		{
			base.Delete();
			WrappedBusinessObject.Factory.WrapperManager.Remove(this);
		}

		public readonly BusinessObject WrappedBusinessObject;

		BusinessObjectWrapperManager manager
		{
			get { return WrappedBusinessObject.Factory.WrapperManager; }
		}

		#region ILinkable Members

		ZGuid ILinkable.LinkPK
		{
			get { return WrappedBusinessObject.PK; }
		}

		string ILinkable.LinkTableName
		{
			get { return WrappedBusinessObject.TableName; }
		}

		string ILinkable.LinkTablePrefix
		{
			get { return WrappedBusinessObject.TablePrefix; }
		}

		bool ILinkable.LinkIsInDatabase
		{
			get { return WrappedBusinessObject.IsInDatabase; }
		}

		#endregion
	}
}
