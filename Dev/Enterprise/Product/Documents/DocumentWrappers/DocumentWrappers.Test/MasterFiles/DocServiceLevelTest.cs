using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocServiceLevel))]
	public class DocServiceLevelTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			return new DocumentWrapper[]
				{
					DocServiceLevel.New(serviceLevel, Factory)
			};
		}

		public void TestCode()
		{
			AssertEquals("", ServiceLevelWrapper.Code);
			ServiceLevel.RS_Code = "BLH";
			AssertEquals("BLH", ServiceLevelWrapper.Code);
		}

		public void TestDescription()
		{
			AssertEquals("", ServiceLevelWrapper.Description);
			ServiceLevel.RS_Description = "TEST SERV LEVEL DESC";
			AssertEquals("TEST SERV LEVEL DESC", ServiceLevelWrapper.Description);
		}

		public void TestIsActive()
		{
			Assert(ServiceLevelWrapper.IsActive);
			ServiceLevel.RS_IsActive = ZBool.False;
			AssertEquals(ZBool.False, ServiceLevelWrapper.IsActive);
		}

		public void TestIsDoorToDoor()
		{
			AssertEquals(ZBool.False, ServiceLevelWrapper.IsDoorToDoor);
			ServiceLevel.RS_IsDoorToDoor = ZBool.True;
			Assert(ServiceLevelWrapper.IsDoorToDoor);
		}

		RefServiceLevel ServiceLevel;
		DocServiceLevel ServiceLevelWrapper;
		protected override void SetUp()
		{
			ServiceLevel = Factory.New<RefServiceLevel>();
			ServiceLevelWrapper = DocServiceLevel.New(ServiceLevel, Factory);
			base.SetUp();
		}
	}
}
