using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocGroup))]
	public class DocGroupTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocGroup.New(Groups, Factory)
			};
		}

		AccGroups Groups;
		protected override void SetUp()
		{
			Groups = Factory.New<AccGroups>();
			base.SetUp();
		}
	}
}
