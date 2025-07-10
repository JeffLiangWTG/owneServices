using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	using ReleaseAndOrderLinePair = KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>;

	public interface IPackingSlipWrapperCollection<T>
		where T : IPackingSlipWrapper
	{
		void Add(T packingSlipWrapper);
		IEnumerable<T> Wrappers { get; }
	}

	public interface IPackingSlipWrapper
	{
		ZString LineNo { get; }
		ZString PositionAfterSorting { get; set; }

		void AddParentToRollUp(WhsPickableDocketLine parent);
		bool ContainsRolledUpParent(WhsPickableDocketLine parent);
	}

	public abstract class PackingSlipLineWrapperHelper
	{
		protected PackingSlipLineWrapperHelper()
		{
		}

		public static ZString GetPropertyToSortBy(WhsPickableDocket packingDocket)
		{
			switch (GetPackingSlipOrderByCode(packingDocket))
			{
				case WhsPackingSlipOrderByList.Codes.ProductCode:
					return "ProductCode";
				case WhsPackingSlipOrderByList.Codes.ProductDescription:
					return "ProductDescription";
				case WhsPackingSlipOrderByList.Codes.LineNo:
					return "LineNo";

				default:
					return ZString.Empty;
			}
		}

		static ZString GetPackingSlipOrderByCode(WhsPickableDocket packingDocket)
		{
			ZString result;

			var client = packingDocket != null ? packingDocket.Client : null;
			var miscServ = client != null ? client.MiscServ : null;
			if (miscServ != null)
			{
				result = miscServ.OM_WhsPackingSlipOrderBy == "DEF" ? (ZString)WarehouseDataRegistry.Instance.PackingSlipOrderBy.Value : miscServ.OM_WhsPackingSlipOrderBy;
			}
			else
			{
				result = ZString.Empty;
			}

			return result;
		}
	}

	public abstract class PackingSlipLineWrapperCollectionHelper<TWrapper, TCollection> : PackingSlipLineWrapperHelper
		where TWrapper : IPackingSlipWrapper
		where TCollection : IPackingSlipWrapperCollection<TWrapper>
	{
		protected PackingSlipLineWrapperCollectionHelper()
		{
		}

		public TCollection GetPackingLines(WhsPickableDocket packingDocket)
		{
			TCollection packingLines;

			var tempFactory = new BusinessObjectFactory();
			var temp = new List<ReleaseAndOrderLinePair>();

			WhsOrder order = null;
			if (packingDocket != null)
			{
				order = packingDocket as WhsOrder;
				var docketLines = order != null ? order.ParentLines : packingDocket.AllLines;
				foreach (WhsPickableDocketLine orderLine in docketLines)
				{
					if (orderLine.ReleaseLines.Count == 0)
					{
						var emptyReleaseLine = GetEmptyReleaseLineInTempFactory(orderLine, tempFactory);
						if (emptyReleaseLine.HasValue)
						{
							temp.Add(emptyReleaseLine.Value);
						}
					}
					else
					{
						temp.AddRange(orderLine.ReleaseLines.Select(r => new ReleaseAndOrderLinePair(r, orderLine)));
					}
				}
			}

			if (order != null)
			{
				packingLines = GetRolledUpReleaseLines(temp, tempFactory);

				var propertyToSortBy = GetPropertyToSortBy(packingDocket);
				Sort(packingLines.Wrappers, packingDocket.LinesToPickForBinding, propertyToSortBy);
			}
			else
			{
				packingLines = GetNewPackingSlipWrapperCollection(temp, tempFactory);
				SortForWorkOrder(packingLines.Wrappers);
			}

			return packingLines;
		}

		static ReleaseAndOrderLinePair? GetEmptyReleaseLineInTempFactory(WhsPickableDocketLine orderline, BusinessObjectFactory tempFactory)
		{
			ReleaseAndOrderLinePair? result = null;
			var lineInOtherFactory = tempFactory.Load<WhsPickableDocketLine>(orderline.PK);
			if (lineInOtherFactory != null)
			{
				using (lineInOtherFactory.ReleaseLines.SuspendSettingDefaults())
				{
					result = new ReleaseAndOrderLinePair(lineInOtherFactory.ReleaseLines.AddNew(), lineInOtherFactory);
				}
			}
			return result;
		}

		protected abstract TCollection GetNewPackingSlipWrapperCollection(IEnumerable<ReleaseAndOrderLinePair> releaseLines, BusinessObjectFactory factory);

		#region GetRolledUpReleaseLines

		TCollection GetRolledUpReleaseLines(IEnumerable<ReleaseAndOrderLinePair> releaseAndOrderLinePairs, BusinessObjectFactory factory)
		{
			var result = GetNewPackingSlipWrapperCollection(Enumerable.Empty<ReleaseAndOrderLinePair>(), factory);
			var docWhsPackingSlipLinesDictionary = new Dictionary<string, TWrapper>();

			foreach (var pair in releaseAndOrderLinePairs)
			{
				TWrapper docPackingSlipLine;

				var releaseLine = pair.Key;
				var orderLine = pair.Value;
				var key = GetDictionaryKey(releaseLine);
				if (docWhsPackingSlipLinesDictionary.TryGetValue(key, out docPackingSlipLine))
				{
					docPackingSlipLine.AddParentToRollUp(orderLine);
				}
				else
				{
					docPackingSlipLine = GetNewPackingSlipWrapper(releaseLine, orderLine, factory);
					result.Add(docPackingSlipLine);
					docWhsPackingSlipLinesDictionary.Add(key, docPackingSlipLine);
				}
			}

			return result;
		}

		protected abstract TWrapper GetNewPackingSlipWrapper(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, BusinessObjectFactory factory);

		static string GetDictionaryKey(WhsReleaseLine releaseLine)
		{
			string result;
			const string separator = "^";

			var part = releaseLine.SupplierPart;
			var client = releaseLine.Client;
			if (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(part))
			{
				result = part.PK.ToString();

				result += releaseLine.OrderedPartAttribute1 + separator;
				result += releaseLine.OrderedPartAttribute2 + separator;
				result += releaseLine.OrderedPartAttribute3 + separator;
				result += releaseLine.OrderedSerialNumber + separator;
				result += releaseLine.OrderedExpiryDate + separator;
				result += releaseLine.OrderedPackingDate + separator;
			}
			else
			{
				result = releaseLine.PK.ToString(); // do not roll up at all.
			}

			return result;
		}

		#endregion

		#region Sorting

		/// <summary>
		/// This is internal purely because existing Tests use this method.
		/// </summary>
		internal static void Sort(IEnumerable<TWrapper> packingSlipWrappers, WhsPickableDocketLineCollection expectedLinesOrder, ZString propertyToSortBy)
		{
			var tempCollection = new DocWhsOrderLineCollection(expectedLinesOrder, expectedLinesOrder.Factory);

			if (!propertyToSortBy.IsEmpty)
			{
				tempCollection.Sort(propertyToSortBy);
			}

			for (int i = 0; i < tempCollection.Count; i++)
			{
				var docketLine = (WhsPickableDocketLine)tempCollection[i].WrappedObject;
				foreach (var line in packingSlipWrappers)
				{
					if (line.PositionAfterSorting.IsEmpty && line.ContainsRolledUpParent(docketLine))
					{
						line.PositionAfterSorting = (i + 1).ToString("00000", Culture.Invariant);
					}
				}
			}
		}

		/// <summary>
		/// This is internal purely because existing Tests use this method.
		/// </summary>
		internal static void SortForWorkOrder(IEnumerable<TWrapper> packingSlipWrappers)
		{
			foreach (var line in packingSlipWrappers)
			{
				line.PositionAfterSorting = line.LineNo;
			}
		}

		#endregion
	}
}
