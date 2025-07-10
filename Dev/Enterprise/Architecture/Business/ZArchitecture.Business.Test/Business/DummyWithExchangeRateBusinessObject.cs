using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class DummyWithExchangeRateBusinessObject : DummyEnterpriseBusinessObject
	{
		public DummyWithExchangeRateBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Exchange Rate

		public DummyZExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new DummyZExchangeRate(this, ExchangeRateType.Buy, Z0_AnotherDecimalInfo, (ZPropertyInfoString)Z0_CodeInfo);
				}

				return fExchangeRate;
			}
		}

		DummyZExchangeRate fExchangeRate;

		#endregion

		public override ZString Z0_Code
		{
			get { return base.Z0_Code; }
			set
			{
				base.Z0_Code = value;
				Z0_AnotherDate = new ZDateTime(2000, 12, 12);
			}
		}

		protected override DummyBizoValidation GetNewValidation()
		{
			return new DummyWithExchangeRateBusinessObjectValidation(this);
		}
	}
}
