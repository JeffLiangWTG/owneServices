using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocOrgSupplierPart))]
	public class DocOrgSupplierPartTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrgSupplierPart.New(Part, Factory)
			};
		}

		OrgSupplierPart Part;
		protected override void SetUp()
		{
			Part = Factory.New<OrgSupplierPart>();
			base.SetUp();
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocOrgSupplierPart.New(Factory.New<OrgSupplierPart>(), Factory);
		}
	}
}
