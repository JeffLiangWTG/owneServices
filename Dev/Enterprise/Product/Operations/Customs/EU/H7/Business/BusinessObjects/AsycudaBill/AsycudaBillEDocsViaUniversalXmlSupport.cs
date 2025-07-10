using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public ZString ExampleCodeFormat => new ZString("H7D0000001|Bill00001");

		public ZString ExpectedCodeFormat => new ZString("ManifestHeaderJobReference|BillNumber");

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			var codeParts = code.Split('|');
			if (codeParts.Length == 2)
			{
				var asycudaBillQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				asycudaBillQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, codeParts[1]);

				var manifestHeaderQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
				manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_JobReference, codeParts[0]);

				asycudaBillQuery.AddSubQuery(AsycudaBillSchema.ABL_AMA, manifestHeaderQuery, JoinCondition.And);

				return factory.LoadTop1<AsycudaBill>(asycudaBillQuery);
			}

			return null;
		}
	}
}
