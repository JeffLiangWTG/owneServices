using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusAddInfoChildrenCollection
	{
		static readonly ConcurrentDictionary<Type, string> TypeCodesByType = new ConcurrentDictionary<Type, string>();

		readonly BusinessObject parent;
		readonly Dictionary<string, Type> typeCodes = new Dictionary<string, Type>();

		/// <summary>
		/// <p>This field contains loaded CusAddInfo objects.</p>
		/// <p>It contains neither objects that were not loaded yet, nor keys for headers that do not exist.</p>
		/// </summary>
		readonly Dictionary<string, CusAddInfo> children = new Dictionary<string, CusAddInfo>();

		public CusAddInfoChildrenCollection(BusinessObject parent)
		{
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent), string.Format(CultureInfo.InvariantCulture, "Parent of {0} cannot be null.", nameof(CusAddInfoChildrenCollection)));
		}

		/// <summary>
		/// Registers type of a child.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public void Register<T>() where T : CusAddInfo
		{
			Type type = typeof(T);
			string typeCode = GetTypeCode(type);
			typeCodes.Add(typeCode, type);
		}

		/// <summary>
		/// Loads CusAddInfo object from database if it was not loaded previously.
		/// If CusAddInfo object does not exist in database, then new CusAddInfo object is created.
		/// </summary>
		public T Load<T>() where T : CusAddInfo
		{
			return (T)Load(typeof(T), true);
		}

		/// <summary>
		/// Loads CusAddInfo object from database if it was not loaded previously.
		/// If CusAddInfo object does not exist in database and <paramref name="createIfNone"/> is set to true, then new CusAddInfo object is created.
		/// </summary>
		CusAddInfo Load(Type childType, bool createIfNone)
		{
			string typeCode = GetTypeCode(childType);

			if (!typeCodes.ContainsKey(typeCode))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type code '{0}' is not registered for this collection.", typeCode));
			}

			CusAddInfo child;

			if (children.TryGetValue(typeCode, out child))
			{
				if (child.IsDeleted)
				{
					// It is possible for a child to be deleted by another instance of CusAddInfoChildrenCollection.
					// In that case we have to try to load another child of same type from database.
					children.Remove(typeCode);
				}
				else
				{
					return child;
				}
			}

			var query = new ZQuery(CusAddInfoSchema.B7_Type, typeCode);
			query.AddToFilter(CusAddInfoSchema.B7_ParentID, parent.PK);
			// The idea to skip searching for children in database, when parent is not saved to database, was taken from DependentBusinessObjectCollection.
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;

			child = (CusAddInfo)parent.Factory.LoadTop1(childType, query);

			if (child == null && createIfNone)
			{
				child = (CusAddInfo)parent.Factory.New(childType);
				child.B7_ParentID = parent.PK;
				child.B7_ParentTableCode = parent.TablePrefix;
			}

			if (child != null)
			{
				children.Add(typeCode, child);
				parent.RegisterEditableChildObject(child);
			}

			return child;
		}

		/// <summary>
		/// Deletes child CusAddInfo of the given type if it exists; otherwise, does nothing.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public void Delete<T>() where T : CusAddInfo
		{
			Delete(typeof(T));
		}

		/// <summary>
		/// Deletes child CusAddInfo of the given type if it exists; otherwise, does nothing.
		/// </summary>
		void Delete(Type childType)
		{
			string typeCode = GetTypeCode(childType);
			if (!typeCodes.ContainsKey(typeCode))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type code '{0}' is not registered for this collection.", typeCode));
			}

			CusAddInfo child = Load(childType, false);
			if (child != null)
			{
				if (!child.IsDeleted)
				{
					child.Delete();
				}
				children.Remove(typeCode);
				parent.UnRegisterEditableChildObject(child);
			}
		}

		/// <summary>
		/// Calls FetchForDelete for each existing child.
		/// </summary>
		public void FetchForDelete()
		{
			foreach (Type childType in typeCodes.Values)
			{
				CusAddInfo child = Load(childType, false);
				child?.FetchStrategy.FetchForDelete();
			}
		}

		public void FetchForValidate()
		{
			foreach (Type childType in typeCodes.Values)
			{
				CusAddInfo child = Load(childType, false);
				child?.FetchStrategy.FetchForValidate();
			}
		}

		string GetTypeCode(Type type)
		{
			return TypeCodesByType.GetOrAdd(type, CusAddInfoTypeAttribute.GetTypeCodeFromAttribute);
		}
	}
}
