using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// Generic Warehouse Docket Line Wrapper
	/// </summary>
	/// <remarks>
	/// THIS CLASS IS A STANDARD INTERFACE FOR DOCBUILDER DOCUMENTS ONLY AND COMMON TO ALL WAREHOUSE DOCKETS DOWN TO WHSORDER.
	/// </remarks>
	public abstract class WarehouseDocketLineWrapperCollection<T> : WarehouseGenericWrapperCollection<WarehouseDocketLineWrapper> where T : WarehouseDocketLineWrapper
	{
		#region Constructors

		public WarehouseDocketLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseDocketLineWrapperCollection(BusinessObjectCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		#endregion

		/// <summary>
		/// Totals Amount of the Extended Line Price
		/// </summary>
		/// <remarks>The implementation of currency across the collection is a bit fluid - it is effectively a text box and is up to the end user to ensure it "makes sense".
		/// If only one unique currency exists at the line level then the currency code is return along with the amount.</remarks>
		public MoneyWrapper TotalExtendedLinePrice
		{
			get
			{
				if (totalExtendedLinePrice == null)
				{
					var totalAmount = ZDecimal.Zero;
					var uniqueCurrencies = new UniqueList<ICurrency>();
					foreach (T line in this)
					{
						if (line != null && line.ExtendedLinePrice != null)
						{
							totalAmount += line.ExtendedLinePriceForTotal;
							uniqueCurrencies.Add(line.ExtendedLinePrice.AmountAsMoney.Currency);
						}
					}

					if (uniqueCurrencies.Count == 1)
					{
						totalExtendedLinePrice = new MoneyWrapper(new Money(totalAmount, uniqueCurrencies[0]), Factory);
					}
					else
					{
						totalExtendedLinePrice = new MoneyWrapper(new Money(totalAmount, null), Factory);
					}
				}
				return totalExtendedLinePrice;
			}
		}
		MoneyWrapper totalExtendedLinePrice;
	}
}

