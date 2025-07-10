using Enterprise.MailManager;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Module.Testing
{
	[TestedType(typeof(EDIWorkTaskMailItemFilterBusinessObject))]
	public class EDIWorkTaskMailItemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			EDIWorkTaskMailItemFilterBusinessObject filterBizo = new EDIWorkTaskMailItemFilterBusinessObject();
			AssertEquals(MailStatus.Unprocessed, ((ModuleTextFilter)filterBizo["Status"]).Property);
			AssertEquals(MailDirection.Receive, ((ModuleTextFilter)filterBizo["Direction"]).Property);
		}

		public void TestStatusList()
		{
			EDIWorkTaskMailItemFilterBusinessObject filterBizo = new EDIWorkTaskMailItemFilterBusinessObject();
			AssertEquals(3, filterBizo.MI_StatusList.Count);
			AssertEquals("", filterBizo.MI_StatusList[0].Code);
			AssertEquals(MailStatus.Processed, filterBizo.MI_StatusList[1].Code);
			AssertEquals(MailStatus.Unprocessed, filterBizo.MI_StatusList[2].Code);
		}

		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIWorkTaskMailItemFilterBusinessObject();
		}
		#endregion
	}
}
