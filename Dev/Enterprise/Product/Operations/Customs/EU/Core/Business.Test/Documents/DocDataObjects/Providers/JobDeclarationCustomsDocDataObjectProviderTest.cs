using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class JobDeclarationCustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetDocDataObject()
		{
			CombineAssertions(() =>
			{
				var dataObject = GetNewDataObject(DataContext.CMRWayBill);
				NUnit.Framework.Assert.That(dataObject, NUnit.Framework.Is.Not.EqualTo(default(object)), "When data context is CMRWayBill, a valid object is expected - should not be [null]");
				NUnit.Framework.Assert.That(dataObject, NUnit.Framework.Is.TypeOf<CMRConsignmentNoteDocDataObject>(), $"{nameof(dataObject)} type");

				dataObject = GetNewDataObject("WHATEVER");
				NUnit.Framework.Assert.That(dataObject, NUnit.Framework.Is.EqualTo(default(object)), "When data context is not CMRWayBill, no valid object is expected - should be [null]");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			docDataObjectParameters = new Mock<IDocDataObjectParameters>();
			logProvider = new Mock<IStmALogProvider>();
			docDataObjectParameters.Setup(x => x.LogProvider).Returns(logProvider.Object);
		}

		Mock<IDocDataObjectParameters> docDataObjectParameters;
		Mock<IStmALogProvider> logProvider;
		JobDeclaration declaration;

		#region Implementation

		object GetNewDataObject(string dataContext)
		{
			return new JobDeclarationCustomsDocDataObjectProvider().GetDocDataObject(declaration, dataContext, docDataObjectParameters.Object);
		}

		#endregion
	}
}
