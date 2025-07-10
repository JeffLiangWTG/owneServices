using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.AWB.Testing
{
	[TestedType(typeof(JASConsolExportAWBRateLineCollection))]
	public class JASConsolExportAWBRateLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASConsolExportAWBHeader awbHeader = Factory.New<JASConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;
			return new JASConsolExportAWBRateLineCollection(awbHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JASConsolExportAWBRateLine>();
		}
	}
}
