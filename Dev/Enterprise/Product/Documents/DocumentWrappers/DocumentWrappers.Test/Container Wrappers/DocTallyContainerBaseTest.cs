using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	[TestedType(typeof(DocTallyContainer))]
	sealed class DocTallyContainerBaseTest : DocPackUnpackContainerRegoBaseTest
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocTallyContainer.New(Tally, Factory),
				DocTallyContainer.New(Factory, Tally.PK)
			};
		}

		#region Implementation

		TallyContainer Tally;

		protected override void SetUp()
		{
			Tally = Factory.New<TallyContainer>();

			base.SetUp();
		}

		#endregion
	}
}
