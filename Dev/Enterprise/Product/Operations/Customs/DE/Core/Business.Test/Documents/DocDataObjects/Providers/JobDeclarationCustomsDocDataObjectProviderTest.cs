using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class JobDeclarationCustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetDocDataObject()
		{
			CombineAssertions(() =>
			{
				var dataObject = GetNewDataObject(DataContext.CMRWayBill);
				AssertNotNull("When data context is CMRWayBill, a valid object is expected", dataObject);
				AssertType<CMRConsignmentNoteDocDataObject>($"{nameof(dataObject)} type", dataObject);

				dataObject = GetNewDataObject("WHATEVER");
				AssertNull("When data context is not CMRWayBill, no valid object is expected", dataObject);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;

		#region Implementation

		object GetNewDataObject(string dataContext)
		{
			var docDataObjectParameters = new Mock<IDocDataObjectParameters>();
			var logProvider = new Mock<IStmALogProvider>();
			docDataObjectParameters.Setup(x => x.LogProvider).Returns(logProvider.Object);

			return new JobDeclarationCustomsDocDataObjectProvider().GetDocDataObject(declaration, dataContext, docDataObjectParameters.Object);
		}

		#endregion
	}
}
