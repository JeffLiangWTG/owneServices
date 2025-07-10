using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class ZQueryProviderCodeDescriptionListBase : ICodeDescriptionPairList, IList
	{
		public ZQueryProviderCodeDescription this[int index]
		{
			get { return (ZQueryProviderCodeDescription)List[index]; }
		}

		public ZQueryProviderCodeDescription GetElementFromCode(ZString code)
		{
			foreach (ZQueryProviderCodeDescription next in this)
			{
				if (next.Code == code)
				{
					return next;
				}
			}

			return null;
		}

		public bool ContainsCode(object code)
		{
			foreach (ICodeDescription pair in this)
			{
				if (String.Compare(pair.Code.Trim(), code.ToString().Trim(), true) == 0)
				{
					return true;
				}
			}

			return false;
		}

		public void RemoveCode(string code)
		{
			for (int i = 0; i < this.Count; i++)
			{
				ICodeDescription pair = this[i];
				if (String.Compare(pair.Code.Trim(), code.Trim(), true) == 0)
				{
					((IList)this).RemoveAt(i);
					return;
				}
			}
		}

		public string GetDescriptionFromCode(string code)
		{
			ZQueryProviderCodeDescription element = GetElementFromCode(code);
			return (element != null) ? element.Description : "";
		}

		public abstract void AddEmptySelection();

		public void Add(ZQueryProviderCodeDescription pair)
		{
			if (Count == 0)
			{
				fQueryProviderCountOnEachPair = pair.QueryProviders.Length;
			}
			else if (pair.QueryProviders.Length != fQueryProviderCountOnEachPair)
			{
				throw new ZException("Number of " + nameof(IQueryProvider) + "S not consistent with previous elements added");
			}

			List.Add(pair);
		}

		#region Adding items that take a variable number of	query providers (typically this is 1 or 2 only)

		protected void AddInternal(ZString code, MultilingualString description, SQLComparisonOperator @operator, params IQueryProvider[] queryProviders)
		{
			Add(new ZQueryProviderCodeDescription(code, description, @operator, queryProviders));
		}

		protected void AddInternal(ZString code, MultilingualString description, SQLComparisonOperator @operator, params SchemaColumn[] propertyNames)
		{
			IQueryProvider[] providers = GetQueryProvidersForPropertyNames(propertyNames);
			Add(new ZQueryProviderCodeDescription(code, description, @operator, providers));
		}

		protected void AddInternal(ZString code, MultilingualString description, SQLComparisonOperator @operator, params AddToQueryDelegate[] delegates)
		{
			IQueryProvider[] providers = GetQueryProvidersForDelegates(delegates);
			Add(new ZQueryProviderCodeDescription(code, description, @operator, providers));
		}

		#endregion

		#region AddQueryProviderComposition / AddQueryProviderCompositionForAll

		/// <summary>
		/// Add's a composition of items to a query provider list. For example, "Most Common".
		/// </summary>
		/// <param name="code">The text to use - "Most Common"</param>
		/// <param name="description">Description - "Most Common"</param>
		/// <param name="indexToInsertAt">Position to Insert at</param>
		/// <param name="queryCodesInvolvedInComposition">The query providers that are included in this composition</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public void AddQueryProviderComposition(ZString code, MultilingualString description, int indexToInsertAt, params string[] queryCodesInvolvedInComposition)
		{
			ZQueryProviderCodeDescription[] pairs = GetPairsFromCodes(queryCodesInvolvedInComposition);
			int queryProviderCountOnEachElement = pairs[0].QueryProviders.Length;
			ArrayList compositions = new ArrayList();

			for (int i = 0; i < queryProviderCountOnEachElement; i++)
			{
				IQueryProvider[] providers = new IQueryProvider[pairs.Length];
				SQLComparisonOperator[] operators = new SQLComparisonOperator[pairs.Length];

				for (int pairIndex = 0; pairIndex < pairs.Length; pairIndex++)
				{
					ZQueryProviderCodeDescription pair = pairs[pairIndex];

					providers[pairIndex] = pair.QueryProviders[i];
					operators[pairIndex] = pair.Operator;
				}

				ZCompositeQueryProvider composition = new ZCompositeQueryProvider(operators, providers);
				compositions.Add(composition);
			}

			IQueryProvider[] compositionsArray = (IQueryProvider[])compositions.ToArray(typeof(IQueryProvider));
			ZQueryProviderCodeDescription newPair = new ZQueryProviderCodeDescription(code, description, compositionsArray);
			((IList)this).Insert(indexToInsertAt, newPair);

			// Append identifying character to included query providers
			ZString compositionIdentification = ZString.Empty;

			if (code != "All")
			{
				compositionIdentification = " (" + description + ")";
			}

			foreach (ZQueryProviderCodeDescription queryCode in pairs)
			{
				for (int i = 0; i < Count; i++)
				{
					if (queryCode == this[i])
					{
						ZQueryProviderCodeDescription existing = ((ZQueryProviderCodeDescription)((IList)this)[i]);
						((IList)this)[i] = new ZQueryProviderCodeDescription(existing.Code, (NoResString)(existing.Description + compositionIdentification), existing.Operator, ImmutableArrayExtensions.ToArray(existing.QueryProviders));
						break;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public void AddQueryProviderCompositionForAll(int indexToInsertAt)
		{
			ArrayList codesToInclude = new ArrayList();

			foreach (ZQueryProviderCodeDescription nextPair in this)
			{
				codesToInclude.Add(nextPair.Code);
			}

			AddQueryProviderComposition("All", ResString.GetMultilingualString("003c9a3c-256c-48de-8b3b-f4e7145163d6", "All"), indexToInsertAt, (string[])codesToInclude.ToArray(typeof(string)));
		}

		#endregion

		#region IList Members

		bool IList.IsReadOnly
		{
			get { return List.IsReadOnly; }
		}

		object IList.this[int index]
		{
			get { return List[index]; }
			set { List[index] = (ICodeDescription)value; }
		}

		void IList.RemoveAt(int index)
		{
			List.RemoveAt(index);
		}

		void IList.Insert(int index, object value)
		{
			List.Insert(index, value);
		}

		void IList.Remove(object value)
		{
			List.Remove((ICodeDescription)value);
		}

		bool IList.Contains(object value)
		{
			return ((IList)List).Contains(value);
		}

		void IList.Clear()
		{
			List.Clear();
		}

		int IList.IndexOf(object value)
		{
			return List.IndexOf((ICodeDescription)value);
		}

		int IList.Add(object value)
		{
			return List.Add((ICodeDescription)value);
		}

		bool IList.IsFixedSize
		{
			get { return List.IsFixedSize; }
		}

		#endregion

		#region ICollection Members

		bool ICollection.IsSynchronized
		{
			get { return List.IsSynchronized; }
		}

		public int Count
		{
			get { return List.Count; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			List.CopyTo(array, index);
		}

		object ICollection.SyncRoot
		{
			get { return List.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion

		#region Implementation

		protected int fQueryProviderCountOnEachPair;
		protected readonly CodeDescriptionPairList List = new CodeDescriptionPairList();

		protected IQueryProvider[] GetQueryProvidersForPropertyNames(SchemaColumn[] columns)
		{
			IQueryProvider[] providers = new ZComparisonQueryProvider[columns.Length];

			for (int i = 0; i < columns.Length; i++)
			{
				providers[i] = new ZComparisonQueryProvider(columns[i]);
			}

			return providers;
		}

		protected IQueryProvider[] GetQueryProvidersForDelegates(AddToQueryDelegate[] delegates)
		{
			IQueryProvider[] providers = new ZDelegateQueryProvider[delegates.Length];

			for (int i = 0; i < delegates.Length; i++)
			{
				providers[i] = new ZDelegateQueryProvider(delegates[i]);
			}

			return providers;
		}

		protected ZQueryProviderCodeDescription[] GetPairsFromCodes(string[] codes)
		{
			ArrayList result = new ArrayList();

			foreach (string code in codes)
			{
				ZQueryProviderCodeDescription pair = GetElementFromCode(code)
					?? throw new ZException(
						"Could not find " + nameof(ZQueryProviderCodeDescription) + " from code '" + code +
						"'. Maybe you havn't added the element with this code yet?");

				result.Add(pair);
			}

			return (ZQueryProviderCodeDescription[])result.ToArray(typeof(ZQueryProviderCodeDescription));
		}

		#endregion
	}
}
