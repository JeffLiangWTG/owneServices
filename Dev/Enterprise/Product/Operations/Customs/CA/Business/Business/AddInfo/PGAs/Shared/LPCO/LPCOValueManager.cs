using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	sealed class LPCOValueManager
	{
		public LPCOValueManager(Lazy<LPCOViewCollection> lazyCollection, ZString lpcoType, Func<LPCOView> lpcoGetter, string determinantProperty = AutoCusCALPCO.Schema.CLP_RefNo)
		{
			this.lpcoType = lpcoType;
			this.lpcoGetter = Argument.NotNull(lpcoGetter, nameof(lpcoGetter));
			this.lazyCollection = Argument.NotNull(lazyCollection, nameof(lazyCollection));
			this.determinantProperty = Argument.NotNullOrEmpty(determinantProperty, nameof(determinantProperty));
		}

		readonly ZString lpcoType;
		readonly Func<LPCOView> lpcoGetter;
		readonly Lazy<LPCOViewCollection> lazyCollection;
		readonly string determinantProperty;

		public T GetValueSafe<T>(string propertyName) where T : IZType
		{
			var lpco = CurrentLPCO;
			return lpco == null || lpco.IsDeleted ? default(T) : (T)lpco[propertyName];
		}

		public void SetValueSafe<T>(string propertyName, T value) where T : IZType
		{
			var lpco = CurrentLPCO;
			var isDeterminantProperty = propertyName == determinantProperty;

			if (lpco == null || lpco.IsDeleted)
			{
				if (isDeterminantProperty && value.IsValid && !value.IsEmpty)
				{
					var collection = lazyCollection.Value;

					if (collection != null)
					{
						lpco = collection.AddNew();
						lpco.CLP_Type = lpcoType;
					}
				}
			}

			if (lpco != null && !lpco.IsDeleted)
			{
				if (isDeterminantProperty && value.IsEmpty)
				{
					var collection = lazyCollection.Value;

					if (collection != null && collection.Contains(lpco))
					{
						lazyCollection.Value?.RemoveAndDelete(lpco);
					}
					else
					{
						lpco.Delete();
					}
				}
				else
				{
					lpco[propertyName] = value;
				}
			}
		}

		public LPCOView CurrentLPCO => lpcoGetter();
	}
}
