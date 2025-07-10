using System.Text;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BranchCollectionRegistryDataType))]
	sealed class BranchCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BranchCollectionRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "BranchControlRegistryItemEditor"; }
		}

		protected override BranchCollectionRegistryDataType GetNewDataType()
		{
			return new BranchCollectionRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var master = new BranchProxyMaster();
			var branchProxy = new BranchProxy { ProxyPK = EnvProxy.Instance.CurrentBranch.PK };
			master.Items.Add(branchProxy);

			byte[] byteArrayValue = Encoding.Unicode.GetBytes(string.Format("<?xml version=\"1.0\" encoding=\"utf-16\"?><BranchProxyMaster><ArrayOfBranchProxy xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><BranchProxy><ProxyPK>{0}</ProxyPK></BranchProxy></ArrayOfBranchProxy></BranchProxyMaster>", branchProxy.ProxyPK));

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(master, byteArrayValue)
			};
		}
	}
}
