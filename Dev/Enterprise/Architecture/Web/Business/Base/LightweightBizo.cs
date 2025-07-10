using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business
{
	internal class LightweightBizo
	{
		public LightweightBizo(BusinessObject heavy)
		{
			PK = heavy.PK;
			RowItems = ((INeedRow)heavy).Row.ItemArray;
		}
		protected readonly object[] RowItems;
		public ZGuid PK { get; }

		protected BusinessObject GetHeavy(Type type, BusinessObjectFactory factory)
		{
			var result = factory.GetBizOsForPK(PK.ToGuid()).Where(x => type.IsAssignableFrom(x.GetType())).FirstOrDefault();
			if (result == null)
			{
				IBusinessObjectFactoryInternals internals = factory;
				var cloneRow = internals.RowFactory.New(BusinessObjectFactory.GetViewNameFromType(type));
				cloneRow.ItemArray = RowItems;
				cloneRow.Table.Rows.Add(cloneRow);
				cloneRow.AcceptChanges();
				result = factory.CreateBusinessObject(cloneRow, type, factory);
			}
			return result;
		}
	}

	/// <summary>
	/// LightweightBizo for an application which has serialized thread access at a higher level,
	/// such as a web application where IIS locks the session.
	/// It only supports one thread at a time accessing it.
	/// Don't use in an application which may have multiple threads concurrently accessing it.
	/// The returned value from GetHeavy may be the value loaded on another thread.
	/// </summary>
	internal class LightweightBizo<T> : LightweightBizo where T : BusinessObject
	{
		public LightweightBizo(T heavy) : base(heavy)
		{
			weakRef = new WeakReference<T>(heavy);
		}
		readonly WeakReference<T> weakRef;

		/// <summary>
		/// Return the full business object
		/// </summary>
		/// <param name="provider">provides a factory if needed</param>
		/// <returns></returns>
		public T GetHeavy(IFactoryProvider provider)
		{
			var result = TryGetHeavy();
			if (result == null)
			{
				result = (T)base.GetHeavy(typeof(T), provider.Factory);
				weakRef.SetTarget(result);
			}

			return result;
		}

		/// <summary>
		/// Return the full business object
		/// </summary>
		/// <param name="factory">factory to use if needed</param>
		/// <returns></returns>
		public T GetHeavy(BusinessObjectFactory factory)
		{
			var result = TryGetHeavy();
			if (result == null)
			{
				result = (T)base.GetHeavy(typeof(T), factory);
				weakRef.SetTarget(result);
			}

			return result;
		}

		public T TryGetHeavy()
		{
			if (!weakRef.TryGetTarget(out var result))
			{
				result = null;
			}

			return result;
		}

#if DEBUG
		public virtual void DiscardAnyFactoryReferenceForTest()
		{
			weakRef.SetTarget(null);
		}
#endif
	}
}
