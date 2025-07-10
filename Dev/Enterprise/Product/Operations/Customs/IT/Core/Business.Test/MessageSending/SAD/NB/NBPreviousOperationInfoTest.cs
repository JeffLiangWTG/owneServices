using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NBPreviousOperationInfoTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var paDocument1 = new Mock<IMergedPreviousDocument>();
		paDocument1.Setup(m => m.Register).Returns("A3");
		paDocument1.Setup(m => m.ReferenceNumber).Returns("1A");
		paDocument1.Setup(m => m.Date).Returns(new ZDate(2020, 01, 01));
		paDocument1.Setup(m => m.Series).Returns("X");
		paDocument1.Setup(m => m.CustomsOffice).Returns("IT137100");
		paDocument1.Setup(m => m.ItemNumber).Returns(1);
		paDocument1.Setup(m => m.PackageQuantity).Returns(100);
		paDocument1.Setup(m => m.GrossMass).Returns(100m);
		paDocument1.Setup(m => m.Mrn).Returns("MRN");

		var rpDocument = new Mock<IMergedPreviousDocument>();
		rpDocument.Setup(m => m.Register).Returns("2");
		rpDocument.Setup(m => m.ReferenceNumber).Returns("2B");
		rpDocument.Setup(m => m.Date).Returns(new ZDate(2020, 01, 01));
		rpDocument.Setup(m => m.Series).Returns("X");
		rpDocument.Setup(m => m.CustomsOffice).Returns("IT137100");
		rpDocument.Setup(m => m.ItemNumber).Returns(0);
		rpDocument.Setup(m => m.Tariff).Returns("9503001110");
		rpDocument.Setup(m => m.NetMass).Returns(99m);
		rpDocument.Setup(m => m.SupplementaryQuantity).Returns(2m);

		var dynamicWrappableObject = new Mock<INBWrappableBusinessObject>();
		dynamicWrappableObject.Setup(m => m.LineNumber).Returns(0);
		dynamicWrappableObject.Setup(m => m.IsExport).Returns(false);
		dynamicWrappableObject.Setup(m => m.IsImport).Returns(true);

		AssertExceptionThrown<ArgumentNullException>(() => new NBPreviousOperationInfo(null));
		AssertNoExceptionThrown(() => new NBPreviousOperationInfo(utility.GetGroupedPreviousDocument((x) => x = rpDocument, (y) => y = paDocument1)));
	}

	public void TestNBPreviousOperationInfo()
	{
		var groupedPreviousDocument = utility.GetGroupedPreviousDocument((x) =>
		{
			x.Setup(m => m.Register).Returns("2");
			x.Setup(m => m.ReferenceNumber).Returns("2");
			x.Setup(m => m.Date).Returns(new ZDate(2020, 01, 01));
			x.Setup(m => m.Series).Returns("X");
			x.Setup(m => m.CustomsOffice).Returns("IT137100");
			x.Setup(m => m.ItemNumber).Returns(It.IsAny<ZInt>());
			x.Setup(m => m.Tariff).Returns("9503001110");
			x.Setup(m => m.NetMass).Returns(99m);
			x.Setup(m => m.SupplementaryQuantity).Returns(2m);
			x.Setup(m => m.Mrn).Returns("MRN");
			x.Setup(m => m.ReferenceNumberCin).Returns("B");
		}, (y) =>
		{
			y.Setup(m => m.Register).Returns("A3");
			y.Setup(m => m.ReferenceNumber).Returns("1");
			y.Setup(m => m.Date).Returns(new ZDate(2020, 01, 01));
			y.Setup(m => m.Series).Returns("X");
			y.Setup(m => m.CustomsOffice).Returns("IT137100");
			y.Setup(m => m.ItemNumber).Returns(1);
			y.Setup(m => m.PackageQuantity).Returns(100);
			y.Setup(m => m.GrossMass).Returns(100m);
			y.Setup(m => m.ReferenceNumberCin).Returns("A");
			y.Setup(m => m.Mrn).Returns("MRN");
		});

		var wrapper = new NBPreviousOperationInfo(groupedPreviousDocument);

		CombineAssertions("PreviousAllibrament", () =>
		{
			var previousAllibrament = wrapper.PreviousAllibrament;
			AssertEquals("137100", previousAllibrament.CustomsOffice);
			AssertEquals(new ZDate(2020, 01, 01), previousAllibrament.Date);
			AssertEquals(1, previousAllibrament.ItemNumber);
			AssertEquals("A", previousAllibrament.ReferenceCIN);
			AssertEquals("1", previousAllibrament.ReferenceNumber);
			AssertEquals("A3", previousAllibrament.Register);
			AssertEquals("X", previousAllibrament.Series);
			AssertEquals("MRN", wrapper.MRN);
		});

		CombineAssertions("PreviousProcedure", () =>
		{
			var previousProcedure = wrapper.PreviousProcedure;
			AssertEquals("137100", previousProcedure.CustomsOffice);
			AssertEquals(new ZDate(2020, 01, 01), previousProcedure.Date);
			AssertEquals(null, previousProcedure.ItemNumber);
			AssertEquals("B", previousProcedure.ReferenceCIN);
			AssertEquals("2", previousProcedure.ReferenceNumber);
			AssertEquals("2", previousProcedure.Register);
			AssertEquals("X", previousProcedure.Series);
		});

		CombineAssertions("Other fields", () =>
		{
			AssertEquals("9503001110", wrapper.CombinedNomenclature);
			AssertEquals(100m, wrapper.GrossMass);
			AssertEquals(99m, wrapper.NetMass);
			AssertEquals(100, wrapper.NumberOfPackages);
			AssertEquals(2m, wrapper.SupplementaryUnit);
		});
	}

	public void TestPreviousAllibrament_ForManualA3()
	{
		var groupedPreviousDocument = utility.GetGroupedPreviousDocument(null, (y) =>
		{
			y.Setup(m => m.Register).Returns("A3");
			y.Setup(m => m.ReferenceNumber).Returns("123456");
			y.Setup(m => m.Date).Returns(new ZDate(2020, 01, 01));
			y.Setup(m => m.Series).Returns("M");
			y.Setup(m => m.CustomsOffice).Returns("IT137100");
			y.Setup(m => m.ItemNumber).Returns(1);
			y.Setup(m => m.ReferenceNumberCin).Returns("");
			y.Setup(m => m.Mrn).Returns("MRN");
		});

		var wrapper = new NBPreviousOperationInfo(groupedPreviousDocument);

		CombineAssertions("PreviousAllibrament", () =>
		{
			var previousAllibrament = wrapper.PreviousAllibrament;
			AssertEquals("137100", previousAllibrament.CustomsOffice);
			AssertEquals(new ZDate(2020, 01, 01), previousAllibrament.Date);
			AssertEquals(1, previousAllibrament.ItemNumber);
			AssertEquals("", previousAllibrament.ReferenceCIN);
			AssertEquals("123456", previousAllibrament.ReferenceNumber);
			AssertEquals("A3", previousAllibrament.Register);
			AssertEquals("M", previousAllibrament.Series);
			AssertEquals("MRN", wrapper.MRN);
		});
	}

	public void TestNetMass()
	{
		var groupedPreviousDocument = utility.GetGroupedPreviousDocument((x) =>
		{
			x.Setup(m => m.NetMass).Returns(100);
		}, null);

		var wrapper = new NBPreviousOperationInfo(groupedPreviousDocument);

		AssertEquals("NetMass", 100m, wrapper.NetMass);
	}

	public void TestGrossMass()
	{
		var groupedPreviousDocument = utility.GetGroupedPreviousDocument(null, (x) =>
		{
			x.Setup(m => m.GrossMass).Returns(101.1236);
		});

		var wrapper = new NBPreviousOperationInfo(groupedPreviousDocument);

		AssertEquals("GrossMass", 101.1236m, wrapper.GrossMass);
	}

	public void TestNullQuantitiesIfZero()
	{
		var groupedPreviousDocument = utility.GetGroupedPreviousDocument((x) =>
		{
			x.Setup(m => m.NetMass).Returns(93.5m);
			x.Setup(m => m.SupplementaryQuantity).Returns(4);
		}, null);

		var wrapper = new NBPreviousOperationInfo(groupedPreviousDocument);

		CombineAssertions("Check NetMass and SupplementaryUnit with values.", () =>
		{
			AssertEquals(nameof(wrapper.NetMass), 93.5m, wrapper.NetMass);
			AssertEquals(nameof(wrapper.SupplementaryUnit), 4m, wrapper.SupplementaryUnit);
		});

		groupedPreviousDocument = utility.GetGroupedPreviousDocument((x) =>
		{
			x.Setup(m => m.NetMass).Returns(0);
			x.Setup(m => m.SupplementaryQuantity).Returns(0);
		}, null);

		wrapper = new NBPreviousOperationInfo(groupedPreviousDocument);

		CombineAssertions("Check NetMass and SupplementaryUnit with 0.", () =>
		{
			AssertNull(nameof(wrapper.NetMass), wrapper.NetMass);
			AssertNull(nameof(wrapper.SupplementaryUnit), wrapper.SupplementaryUnit);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		utility = new ITDocM2TestUtility(Factory);
	}

	ITDocM2TestUtility utility;

	class ITDocM2TestUtility
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
