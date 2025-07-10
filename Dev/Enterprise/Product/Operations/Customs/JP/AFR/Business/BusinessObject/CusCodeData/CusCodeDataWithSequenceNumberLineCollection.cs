using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class CusCodeDataWithSequenceNumberLineCollection<T> : ActiveBusinessObjectCollection<T>
		where T : CusCodeData, ISequenceNumberLine
	{
		public CusCodeDataWithSequenceNumberLineCollection(JPAFRBills master, ZString type)
			: base(master.Factory, master, new ZQuery(CusCodeDataSchema.CY_Type, type), CusCodeDataSchema.CY_ParentID)
		{
		}

		public T[] GetNonEmptyInSortOrder()
		{
			var notificationForwardingParties = new List<T>(Find(new FindPredicate(data => !data.IsDeleted && !data.CY_Data.IsEmpty)));
			notificationForwardingParties.Sort(new System.Comparison<T>((x, y) =>
				{
					var result = 0;
					if (x != null || y != null)
					{
						if (x != null && y == null)
						{
							result = -1;
						}
						else if (x == null && y != null)
						{
							result = 1;
						}
						else
						{
							result = x.CY_Order.CompareTo(y.CY_Order);
							result = result == 0 ? x.CY_Data.CompareTo(y.CY_Data) : result;
							result = result == 0 ? x.PK.CompareTo(y.PK) : result;
						}
					}
					return result;
				}));
			return notificationForwardingParties.ToArray();
		}

		public T AddNewIfNotExist(ZString value)
		{
			var result = this.FirstOrDefault(x => x.CY_Data == value);
			if (result == null)
			{
				result = AddNew();
				result.CY_Data = value;
			}
			return result;
		}

		protected override void OnLoadedIntoCollectionCore(T loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.Parent = Relationship.Master;
		}

		protected override void SetRelationshipDefaultsForElementCore(T newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.Parent = Relationship.Master;
		}

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.CY_Order = (ZShort)Count + 1;
		}
	}
}
