#if DEBUG

using CargoWise.Application;
using Enterprise.Accounting.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.Business.Testing
{
	public static class TestExtentions
	{
		public static string Serialize<T>(this T universalTransaction, IUniversalXmlSchema schema = null)
			where T : IDataObject
		{
			var testHelper = ObjectFactory.Get<IAccountingDataTransferTestHelper>();
			return testHelper.SerializeDataObject(universalTransaction, schema);
		}
	}
}

#endif
