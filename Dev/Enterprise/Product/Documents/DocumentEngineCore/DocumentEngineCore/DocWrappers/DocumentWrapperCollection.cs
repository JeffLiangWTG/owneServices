using System;
using System.Collections;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public abstract class DocumentWrapperCollection<T> : DocumentWrapperCollection where T : DocumentWrapper
	{
		protected DocumentWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DocumentWrapperCollection(IEnumerable objectsToWrap, BusinessObjectFactory factory)
			: base(objectsToWrap, factory)
		{
		}

		public new T this[string index]
		{
			get { return (T)base[index]; }
		}

		public new T this[int index]
		{
			get { return (T)base[index]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}
	}

	public abstract class DocumentWrapperCollection : NonPersistentBusinessObjectCollection<DocumentWrapper>, IBODocDataProviderCollection
	{
		protected DocumentWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DocumentWrapperCollection(IEnumerable objectsToWrap, BusinessObjectFactory factory)
			: base(factory)
		{
			Load(objectsToWrap);
		}

		public void Load(IEnumerable objectsToWrap)
		{
			foreach (object objectToWrap in objectsToWrap)
			{
				Add(WrapObject(objectToWrap));
			}
		}

		protected internal virtual DocumentWrapper WrapObject(object objectToWrap)
		{
			try
			{
				return (DocumentWrapper)TypeOfElements.InvokeMember("New", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { objectToWrap, Factory });
			}
			catch (MissingMethodException)
			{
				ErrorReporter.ReportOnce("DocumentWrapperCollection.WrapObject-MissingMethodException",
					"Incident CS00142497 / Issue 00602923 / Assign to M.K - " +
					"objectToWrap type: " + (objectToWrap != null ? objectToWrap.GetType().FullName : "null") +
					", Factory: " + (Factory != null ? Factory.NameForDebugging : "null"));

				throw;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Document wrappers do not support creating brand new objects. They only wrap existing objects. Use Load(IEnumerable objectsToWrap) instead.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public DocumentWrapper this[string index]
		{
			get { return (DocumentWrapper)GetRow(index); }
		}

		public bool ContainsWrappedObject(object wrappedObjectToFind)
		{
			foreach (DocumentWrapper wrapper in this)
			{
				if (wrapper.WrappedObject == wrappedObjectToFind)
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsWrappedObject(ZGuid wrappedBusinessObjectPKToFind)
		{
			foreach (DocumentWrapper wrapper in this)
			{
				BusinessObject wrappedBusinessObject = wrapper.WrappedObject as BusinessObject;
				if (wrappedBusinessObject != null && wrappedBusinessObject.PK == wrappedBusinessObjectPKToFind)
				{
					return true;
				}
			}
			return false;
		}

		#region IBODocDataProviderCollection Members

		protected IBODocDataProviderCollectionHelper Helper
		{
			get { return helper ?? (helper = (IBODocDataProviderCollectionHelper)Activator.CreateInstance(ObjectFactory.GetType<IBODocDataProviderCollectionHelper>(), new object[] { this })); }
		}
		IBODocDataProviderCollectionHelper helper;

		IBODocDataProvider IBODocDataProviderCollection.this[string index]
		{
			get { return BODocDataProvider.Get(this[index]); }
		}

		IBODocDataProvider IBODocDataProviderCollection.this[int index]
		{
			get { return BODocDataProvider.Get(this[index]); }
		}

		int IBODocDataProviderCollection.Count
		{
			get { return Count; }
		}

		ZString IBODocDataProviderCollection.Format(ZString formatString, ZString delimiter, ZString filterString, ZString groupByParameters, ZInt maxItems)
		{
			return Helper.Format(formatString, delimiter, filterString, groupByParameters, maxItems);
		}

		object IBODocDataProviderCollection.Total(ZString fieldName, ZString decimalPlaces, ZString filter)
		{
			return Count > 0 ? GetTotal(fieldName, decimalPlaces, filter) : ZString.Empty;
		}

		protected virtual object GetTotal(ZString fieldName, ZString decimalPlaces, ZString filter)
		{
			return Helper.Total(fieldName, decimalPlaces, filter);
		}

		protected virtual IBODocDataProvider GetRow(ZString index)
		{
			return Helper[index];
		}

		public BusinessObject Find(ZString match)
		{
			return Helper.Find(match);
		}

		#endregion
	}
}
