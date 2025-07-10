using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	class DummyWithAccExchangeRateBusinessObject : DummyBusinessObject
	{
		public DummyWithAccExchangeRateBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Exchange Rate

		public ZAccExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Buy, Z0_AnotherDecimalInfo, (ZPropertyInfoString)Z0_CodeInfo, null);
				}

				return fExchangeRate;
			}
		}

		protected ZAccExchangeRate fExchangeRate;

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
	}
}
