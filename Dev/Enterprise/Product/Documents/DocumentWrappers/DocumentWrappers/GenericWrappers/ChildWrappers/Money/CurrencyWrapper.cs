using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CodeAndDescription")]
	public class CurrencyWrapper : GenericWrapper
	{
		public CurrencyWrapper(ICurrency currency, BusinessObjectFactory factory)
			: base(null, factory)
		{
			if (currency != null)
			{
				fCode = currency.Code;
				fCurrencyPK = currency.PK;
				fDecimals = currency.Decimals;
			}
			else
			{
				fCode = "";
				fCurrencyPK = ZGuid.Empty;
				fDecimals = 2;
			}
		}

		public ZString Code => fCode;

		public ZString Description => CurrencyBO == null ? fCode : CurrencyBO.RX_DescMultilingual;

		public ZString RX_Symbol => CurrencyBO == null ? fCode : CurrencyBO.RX_Symbol;

		public ZString CodeAndDescription => GetCombinedValue(Code, Description);

		public ZInt DecimalPlaces => fDecimals;

		#region Implementation
		readonly ZString fCode;
		readonly ZGuid fCurrencyPK;
		readonly ZInt fDecimals;

		RefCurrency CurrencyBO
		{
			get
			{
				if (fCurrencyBO == null && !fCurrencyPK.IsEmpty)
				{
					fCurrencyBO = Factory.Load<RefCurrency>(fCurrencyPK);
				}
				return fCurrencyBO;
			}
		}
		RefCurrency fCurrencyBO;
		#endregion
	}
}
