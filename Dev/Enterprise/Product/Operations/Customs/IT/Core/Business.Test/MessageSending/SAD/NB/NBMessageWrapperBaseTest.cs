using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class NBMessageWrapperBaseTest : TestCaseWithFactory
{
	public abstract void TestAnnualProgressiveNumber();

	public void TestDataBlocks()
	{
		wrapper = GetNBMessageSendingObject(dynamicWrappableObject.Object);

		var dataBlocks = wrapper.DataBlocks;
		AssertEquals("DataBlocks count", 2, dataBlocks.Count());
		AssertType<NBPreviousOperationInfo>("DataBlocks item type", dataBlocks.ElementAt(0));
	}

	public abstract void TestHeader();

	protected override void SetUp()
	{
		base.SetUp();

		utility = new ITDocM2TestUtility(Factory);

		var groupedPreviousDocument1 = utility.GetGroupedPreviousDocument(null, (x) =>
		{
			x.Setup(m => m.PackageQuantity).Returns(100);
			x.Setup(m => m.GrossMass).Returns(1000.12m);
			x.Setup(m => m.SupplementaryQuantity).Returns(10.12m);
			x.Setup(m => m.NetMass).Returns(10.12m);
		});

		var groupedPreviousDocument2 = utility.GetGroupedPreviousDocument((x) =>
		{
			x.Setup(m => m.PackageQuantity).Returns(19);
			x.Setup(m => m.GrossMass).Returns(19);
			x.Setup(m => m.SupplementaryQuantity).Returns(19);
			x.Setup(m => m.NetMass).Returns(29);
		}, null);

		dynamicWrappableObject = new Mock<INBWrappableBusinessObject>();
		dynamicWrappableObject.Setup(m => m.IsExport).Returns(false);
		dynamicWrappableObject.Setup(m => m.IsImport).Returns(true);
		dynamicWrappableObject.Setup(m => m.NBGroupedPreviousDocuments).Returns(new[] { groupedPreviousDocument1, groupedPreviousDocument2 });

		wrapper = GetNBMessageSendingObject(dynamicWrappableObject.Object);
	}

	protected abstract NBMessageWrapperBase GetNBMessageSendingObject(INBWrappableBusinessObject nbObject);

	protected ITDocM2TestUtility utility;
	protected Mock<INBWrappableBusinessObject> dynamicWrappableObject;
	protected NBMessageWrapperBase wrapper;

	protected class ITDocM2TestUtility
	{
		public ITDocM2TestUtility(BusinessObjectFactory factory)
		{
			this.factory = factory;
			mergedPreviousDocumentsProviderMock = new Mock<IMergedPreviousDocumentsProvider>();
			mergedPreviousDocumentsProviderMock.Setup(m => m.LineNumber).Returns(1);
			mergedPreviousDocumentsProviderMock.Setup(m => m.NBStatus).Returns("");
		}
		readonly BusinessObjectFactory factory;
		readonly Mock<IMergedPreviousDocumentsProvider> mergedPreviousDocumentsProviderMock;

		public GroupedPreviousDocument GetGroupedPreviousDocument(Action<Mock<IMergedPreviousDocument>> setupRpDocument, Action<Mock<IMergedPreviousDocument>> setupPaDocument)
		{
			IMergedPreviousDocument previousProcedureDocument = null;
			if (setupRpDocument != null)
			{
				var rpMergerdPreviousDocumentMock = new Mock<IMergedPreviousDocument>();
				setupRpDocument(rpMergerdPreviousDocumentMock);
				previousProcedureDocument = rpMergerdPreviousDocumentMock.Object;
			}

			IMergedPreviousDocument summaryDocument = null;
			if (setupPaDocument != null)
			{
				var paMergerdPreviousDocumentMock = new Mock<IMergedPreviousDocument>();
				setupPaDocument(paMergerdPreviousDocumentMock);
				summaryDocument = paMergerdPreviousDocumentMock.Object;
			}

			return new GroupedPreviousDocument(mergedPreviousDocumentsProviderMock.Object, summaryDocument, previousProcedureDocument, factory);
		}
	}
}
