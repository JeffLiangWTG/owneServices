using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public sealed class ITransactionLineTaxDateTest : TestCase
	{
		public void TestInterfaceFields()
		{
			//Properties in ITransactionLineTaxDate will be cached to improve performance.
			//If you add a new property to this interface, please call InvoicingLineTaxDateCacheProvider.StaleCache() in new property setter, to clear cached values.
			//And then add the new property name into validPropertyNames list.
			var validPropertyNames = new HashSet<string>()
			{
				"AL_AC",
				"ChargeCode",
				"TaxRate",
				"AL_TaxDate"
			};
			var propertyNames = typeof(ITransactionLineTaxDate).GetProperties().Select(x => x.Name);
			foreach (var propertyName in propertyNames)
			{
				AssertEquals(true, validPropertyNames.Contains(propertyName));
			}
		}
	}
}
