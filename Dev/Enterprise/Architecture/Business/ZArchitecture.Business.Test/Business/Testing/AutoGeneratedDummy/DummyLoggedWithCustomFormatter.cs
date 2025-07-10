using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	class DummyLoggedWithCustomFormatter : DummyLogged, IDataVersionLoggingSupported
	{
		public DummyLoggedWithCustomFormatter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => new CustomFormatter();

		class CustomFormatter : DataVersionLogValueFormatter
		{
			protected internal override string GetDataColumnName(ZPropertyInfo property)
			{
				return base.GetDataColumnName(property) + " NOT!";
			}
		}
	}
}
