using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocCurrency : DocBaseWrapper, ICurrency
	{
		DocCurrency(RefCurrency refCurrency, BusinessObjectFactory factoryForWrapper)
			: base(refCurrency, factoryForWrapper)
		{
		}

		public static DocCurrency New(BusinessObjectFactory factory, ICurrency currency)
		{
			if (currency != null)
			{
				var refCurrency = factory.Load<RefCurrency>(currency.PK);
				return New(refCurrency, factory);
			}

			return null;
		}

		public static DocCurrency New(RefCurrency refCurrency, BusinessObjectFactory factoryForWrapper)
		{
			if (refCurrency == null)
			{
				return null;
			}
			else
			{
				return factoryForWrapper.GetCachedValue(refCurrency.PK.ToStringKey(), delegate
				{ return new DocCurrency(refCurrency, factoryForWrapper); }, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		public static DocCurrency New(ZString currencyCode, BusinessObjectFactory factoryForWrapper)
		{
			return New(factoryForWrapper.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode), factoryForWrapper);
		}

		public ZString Code
		{
			get { return RefCurrency.RX_Code; }
		}

		public ZString Desc
		{
			get { return RefCurrency.RX_DescMultilingual; }
		}

		public ZBool IsActive
		{
			get { return RefCurrency.RX_IsActive; }
		}

		public ZBool IsSystem
		{
			get { return RefCurrency.RX_IsSystem; }
		}

		public ZString SubUnitName
		{
			get { return RefCurrency.RX_SubUnitNameMultilingual; }
		}

		public ZInt SubUnitRatio
		{
			get { return RefCurrency.RX_SubUnitRatio; }
		}

		public ZString Symbol
		{
			get { return RefCurrency.RX_Symbol; }
		}

		public ZString UnitName
		{
			get { return RefCurrency.RX_UnitNameMultilingual; }
		}

		public ZInt Decimals
		{
			get { return RefCurrency.Decimals; }
		}

		public override string ToString()
		{
			return Code;
		}

		public ZString FormatMoney(ZDecimal value)
		{
			return Symbol + value.ToString(Decimals) + " " + Code;
		}

		#region Implementation

		RefCurrency RefCurrency
		{
			get { return (RefCurrency)WrappedObject; }
		}

		#endregion

		#region ICurrency Members

		string ICurrency.Code
		{
			get { return Code; }
		}

		Guid ICurrency.PK
		{
			get { return RefCurrency.PK.ToGuid(); }
		}

		//public int Decimals
		//{
		//    get { return 2; }
		//}

		int ICurrency.Decimals
		{
			get { return Decimals; }
		}

		#endregion

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}

		#endregion
	}
}
