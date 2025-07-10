using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class IShipmentDataProxyHelper
	{
		public IShipmentDataProxyHelper(ICustomFieldProvider businessObject)
		{
			BusinessObject = businessObject;
		}
		readonly ICustomFieldProvider BusinessObject;

		public RefCurrencyCurrencyConverter GetCurrencyConverter(BusinessObjectFactory factory)
		{
			if (currencyConverter == null)
			{
				currencyConverter = new RefCurrencyCurrencyConverter(factory) { DateForRate = ZDateTime.Today, RateType = ExchangeRateType.Customs };
			}
			return currencyConverter;
		}
		RefCurrencyCurrencyConverter currencyConverter;

		#region CustomCharge
		public ZDecimal CustomCharge1
		{
			get
			{
				if (!customCharge1.HasValue)
				{
					customCharge1 = GetCustomCharge(CustomCode1, charge1);
				}
				return customCharge1.Value;
			}
		}
		ZDecimal? customCharge1;

		public ZDecimal CustomCharge2
		{
			get
			{
				if (!customCharge2.HasValue)
				{
					customCharge2 = GetCustomCharge(CustomCode2, charge2);
				}
				return customCharge2.Value;
			}
		}
		ZDecimal? customCharge2;

		public ZDecimal CustomCharge3
		{
			get
			{
				if (!customCharge3.HasValue)
				{
					customCharge3 = GetCustomCharge(CustomCode3, charge3);
				}
				return customCharge3.Value;
			}
		}
		ZDecimal? customCharge3;

		public ZString CustomCode1
		{
			get
			{
				if (!customCode1.HasValue)
				{
					customCode1 = GetCustomCode(code1);
				}
				return customCode1.Value;
			}
		}
		ZString? customCode1;

		public ZString CustomCode2
		{
			get
			{
				if (!customCode2.HasValue)
				{
					customCode2 = GetCustomCode(code2);
				}
				return customCode2.Value;
			}
		}
		ZString? customCode2;

		public ZString CustomCode3
		{
			get
			{
				if (!customCode3.HasValue)
				{
					customCode3 = GetCustomCode(code3);
				}
				return customCode3.Value;
			}
		}
		ZString? customCode3;

		const string code1 = "Code 1";
		const string code2 = "Code 2";
		const string code3 = "Code 3";
		const string charge1 = "Charge 1";
		const string charge2 = "Charge 2";
		const string charge3 = "Charge 3";

		public bool IsCustomChargeShouldbeUploaded => CustomCharge1 > ZDecimal.Zero || CustomCharge2 > ZDecimal.Zero || CustomCharge3 > ZDecimal.Zero;

		ZString GetCustomCode(ZString code)
		{
			return GetCustomData<ZString>(code, AddOnColumnDataType.Codes.String);
		}

		ZDecimal GetCustomCharge(ZString code, ZString charge)
		{
			var result = ZDecimal.Zero;
			if (!code.IsEmpty)
			{
				result = GetCustomData<ZDecimal>(charge, AddOnColumnDataType.Codes.Decimal);
			}
			return result;
		}

		T GetCustomData<T>(ZString code, string type)
			where T : IZType
		{
			T result = default(T);
			var customBO = CustomBusinessObject;
			var (codeData, codePropertyName) = customBO.GetPropertyByDescription(code, type);
			if (codeData != null)
			{
				result = (T)customBO[codePropertyName];
			}
			return result;
		}

		CustomBusinessObject CustomBusinessObject => customBusinessObject ?? (customBusinessObject = BusinessObject.GetCustomBusinessObject());
		CustomBusinessObject customBusinessObject;
		#endregion
	}
}
