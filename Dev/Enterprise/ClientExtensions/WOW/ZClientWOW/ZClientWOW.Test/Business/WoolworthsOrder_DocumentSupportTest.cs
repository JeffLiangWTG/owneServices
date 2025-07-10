using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrder))]
	public class WoolworthsOrder_DocumentSupportTest : WowDocumentSupportTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			WoolworthsOrder result = Factory.NewWithValidTestData<WoolworthsOrder>();
			BusinessObject lcHeader = (BusinessObject)Factory.New<Enterprise.Integration.LandedCosting.ILandedCostHeader>();
			lcHeader[LandedCostHeaderSchema.LT_ParentID] = result.PK;
			lcHeader[LandedCostHeaderSchema.LT_ParentTableCode] = JobOrderHeaderSchema.Constants.Prefix;
			Factory.Save();
			return result;
		}
	}
}
