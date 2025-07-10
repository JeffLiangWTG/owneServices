using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.DataTransfer.Test
{
	[TestedType(typeof(DocManagerDataContextManager))]
	public class DocManagerDataContextManagerTest : DataContextManagerTestCase<DocManagerDataContextManager, BusinessObject>
	{
		protected override BusinessObject GetNewBusinessObjectForTesting()
		{
			return new DocManagerWrapper(new BusinessObjectFactory());
		}

		protected override void TestAttributeIsOnBusinessObjectCore()
		{
			var attribute = typeof(DocManagerWrapper).GetAttribute<UniversalDataContextAttribute>();
			AssertNotNull("Must have UniversalDataContextAttribute applied.", attribute);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("Workflow requires IJobNumber on the Top Level BusinessObject type so the (*JobNumber*) macro works on FileNames and in the Email Subject from EDI Communications Modes.",
				typeof(IJobNumber).IsAssignableFrom(typeof(DocManagerWrapper)));
		}
	}
}
