using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	internal class BusinessObjectCache
	{
		public void Add(BusinessObject bizO, Type preTypeDecidedBusinessObjectType)
		{
			if (ReferenceEquals(bizO, null))
			{
				throw new ArgumentNullException(nameof(bizO));
			}
			if (bizO.PK.IsEmpty)
			{
				throw new ArgumentException("BizO has empty PK: " + preTypeDecidedBusinessObjectType.FullName);
			}

			BusinessObject existingBusinessObject = null;
			var exists = HashByPK.TryGetValue(bizO.PK.ToGuid(), out existingBusinessObject);

			if (exists && !ReferenceEquals(existingBusinessObject, null)) // Business Object PK is in hash and only has one bizO
			{
				if (existingBusinessObject != bizO) // Multiple objects around a row for the first time
				{
					var multiDictionary = new Dictionary<Type, BusinessObject>(2);

					multiDictionary[existingBusinessObject.GetType()] = existingBusinessObject;
					multiDictionary[bizO.GetType()] = bizO;
					MultiObjectHashByPK[bizO.PK.ToGuid()] = multiDictionary;
					HashByPK[bizO.PK.ToGuid()] = null;
				}
				else
				{
					throw new ArgumentException("Trying to add BizO with same type and PK twice to cache: " + preTypeDecidedBusinessObjectType.FullName);
				}
			}
			else if (exists && ReferenceEquals(existingBusinessObject, null)) // Multiple objects around a row again
			{
				MultiObjectHashByPK[bizO.PK.ToGuid()][bizO.GetType()] = bizO;
			}
			else // First time the PK has been seen
			{
				HashByPK[bizO.PK.ToGuid()] = bizO;
			}

			if (bizO.Row != null)
			{
				BusinessObjectsForARow elementsForRow;
				if (!HashByRow.TryGetValue(bizO.Row, out elementsForRow))
				{
					elementsForRow = new BusinessObjectsForARow(bizO);
					HashByRow[bizO.Row] = elementsForRow;
					AllBizOs.Add(bizO);
				}
				else
				{
					var bizOType = bizO.GetType();
					if (elementsForRow[bizOType] == null)
					{
						// This is to get some more info for Issue 01266545
						if (elementsForRow.FirstBizO.GetType().Name == "DocumentNote" && bizOType.Name == "QuotedBookingStmNote")
						{
							ErrorReporter.ReportOnce("DocumentNote_Same_With_QuotedBookingStmNote", FormattableString.Invariant($"{nameof(BusinessObjectsForARow)} has 2 different types: DocumentNote & QuotedBookingStmNote.\r\nRow data: {string.Join(";", bizO.Row.ItemArray)}")); // Developer Info
						}
						else if (elementsForRow.FirstBizO.GetType().Name == "JobComInvoiceHeader" && bizOType.Name == "BaseJobComInvoiceHeader")
						{
							ErrorReporter.ReportOnce("JobComInvoiceHeader_Same_With_BaseJobComInvoiceHeader", FormattableString.Invariant($"{nameof(BusinessObjectsForARow)} has 2 different types: JobComInvoiceHeader & BaseJobComInvoiceHeader.\r\nRow data: {string.Join(";", bizO.Row.ItemArray)}")); // Developer Info
						}
						else if (elementsForRow.FirstBizO.GetType().Name == "ForwardingShipmentStmNote" && bizOType.Name == "OrderUpdateHistoryStmNote")
						{
							ErrorReporter.ReportOnce("ForwardingShipmentStmNote_Same_With_OrderUpdateHistoryStmNote", FormattableString.Invariant($"{nameof(BusinessObjectsForARow)} has 2 different types: ForwardingShipmentStmNote & OrderUpdateHistoryStmNote.\r\nRow data: {string.Join(";", bizO.Row.ItemArray)}")); // Developer Info
						}

						elementsForRow.Add(bizO);
						AllBizOs.Add(bizO);
					}
				}
			}
			else
			{
				AllBizOs.Add(bizO);
			}

			cachedAllBizosArray = null;
		}

		public void Add(BusinessObject bizO)
		{
			Add(bizO, bizO.GetType());
		}

		public void Remove(BusinessObject bizO)
		{
			BusinessObjectsForARow bizObjsForARow = this[bizO.Row];
			if (bizObjsForARow != null)
			{
				bizObjsForARow.HandleDeleted(bizO);
			}
		}

		public void ClearDeletedElements()
		{
			var hasRemovedElements = false;

			for (int index = AllBizOs.Count - 1; index >= 0; index--)
			{
				var bizo = AllBizOs[index];
				if (bizo.IsDeleted)
				{
					if (bizo.Row != null)
					{
						if (bizo.Row.RowState != DataRowState.Detached)
						{
							continue; // Ignore new deleted but uncommitted rows
						}

						HashByRow.Remove(bizo.Row);
					}

					AllBizOs.RemoveAt(index);

					var guid = bizo.PK.ToGuid();
					HashByPK.Remove(guid);
					MultiObjectHashByPK.Remove(guid);

					hasRemovedElements = true;
				}
			}

			if (hasRemovedElements)
			{
				cachedAllBizosArray = null;
			}
		}

		public BusinessObject Fetch(Type businessObjectType, ZGuid pK)
		{
			BusinessObject result = null;
			if (!pK.IsEmpty && pK.IsValid)
			{
				if (HashByPK.TryGetValue(pK.ToGuid(), out result))
				{
					if (!ReferenceEquals(result, null))
					{
						var resultType = result.GetType();

						if (resultType != businessObjectType && (!IDontMindLoadingASubclassInsteadAttribute.HasAttribute(businessObjectType) || !resultType.IsSubclassOf(businessObjectType))
#if DEBUG
							&& (!(result is Moq.IMocked) || resultType.BaseType != businessObjectType)
#endif
						)
						{
							result = null;
						}
					}
					else
					{
						MultiObjectHashByPK[pK.ToGuid()].TryGetValue(businessObjectType, out result);
					}
				}
			}
			if (!ReferenceEquals(result, null) && result.IsDeleted)
			{
				result = null;
			}
			return result;
		}

		public BusinessObjectsForARow this[DataRow row]
		{
			get
			{
				BusinessObjectsForARow result;
				HashByRow.TryGetValue(row, out result);
				return result;
			}
		}

		public IReadOnlyList<BusinessObject> AllBusinessObjects
		{
			get { return cachedAllBizosArray ?? (cachedAllBizosArray = AllBizOs.ToList().AsReadOnly()); }
		}
		IReadOnlyList<BusinessObject> cachedAllBizosArray;

		internal IReadOnlyList<BusinessObject> AllBusinessObjectsUnsafeForQuickAccess
		{
			get { return cachedAllBizosArray ?? AllBizOs.AsReadOnly(); }
		}

		public BusinessObject[] GetBusinessObjectsForPK(Guid pK)
		{
			BusinessObject[] result;
			BusinessObject businessObject = null;
			if (HashByPK.TryGetValue(pK, out businessObject))
			{
				if (!ReferenceEquals(businessObject, null))
				{
					result = new[] { businessObject };
				}
				else
				{
					result = MultiObjectHashByPK[pK].Values.ToArray();
				}
			}
			else
			{
				result = Array.Empty<BusinessObject>();
			}
			return result;
		}

		public int NumberOfBusinessObjects
		{
			get { return AllBizOs.Count; }
		}

		readonly List<BusinessObject> AllBizOs = new List<BusinessObject>();
		readonly Dictionary<DataRow, BusinessObjectsForARow> HashByRow = new Dictionary<DataRow, BusinessObjectsForARow>();
		readonly Dictionary<Guid, BusinessObject> HashByPK = new Dictionary<Guid, BusinessObject>();
		readonly Dictionary<Guid, Dictionary<Type, BusinessObject>> MultiObjectHashByPK = new Dictionary<Guid, Dictionary<Type, BusinessObject>>();
	}
}
