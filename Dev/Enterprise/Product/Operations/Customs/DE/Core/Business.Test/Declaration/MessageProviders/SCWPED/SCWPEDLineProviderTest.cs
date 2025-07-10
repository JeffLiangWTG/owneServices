using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWPEDLineProvider))]
	class SCWPEDLineProviderTest : MonthlyClosingDecLineProviderAbstractTest<SCWPEDLineProvider, ISCWRECHeader, ISCWRECLine>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCWPEDLineProvider(null, isModificationMessage: false));
		}

		public void TestRequestedPreferentialTreatment_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(l => l.RequestedPreferentialTreatment).Returns((ZString)"200");
			AssertEquals("200", Provider.RequestedPreferentialTreatment);
		}

		public void TestRequestedPreferentialTreatment_Snapshot()
		{
			isModificationMessage = false;
			snapshot.PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment { RequestedPreferentialTreatment = "200" };
			AssertEquals("200", Provider.RequestedPreferentialTreatment);
		}

		public void TestRequestedPreferentialTreatment_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertNullOrEmpty(Provider.RequestedPreferentialTreatment);
		}

		public void TestInwardMovementAmount_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(l => l.InwardMovementAmount).Returns(
				Mock.Of<IAmount>(m =>
					m.Quantity == 1.2m &&
					m.MeasurementUnit == "KGM" &&
					m.Qualifier == "A")
			);
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 1.2m, Provider.InwardMovementAmount.Quantity);
				AssertEquals("Unit", "KGM", Provider.InwardMovementAmount.MeasurementUnit);
				AssertEquals("Qualifier", "A", Provider.InwardMovementAmount.Qualifier);
			});
		}

		public void TestInwardMovementAmount_Snapshot()
		{
			isModificationMessage = false;
			snapshot.InwardMovementAmount = new Amount()
			{
				Quantity = 1.2m,
				MeasurementUnit = "KGM",
				Qualifier = "A"
			};
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 1.2m, Provider.InwardMovementAmount.Quantity);
				AssertEquals("Unit", "KGM", Provider.InwardMovementAmount.MeasurementUnit);
				AssertEquals("Qualifier", "A", Provider.InwardMovementAmount.Qualifier);
			});
		}

		public void TestInwardMovementAmount_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertNull(Provider.InwardMovementAmount);
		}

		public void TestForeignTradeImportEarlyClearanceFlag_Provider()
		{
			isModificationMessage = true;
			Mock.Get(headerProvider).Setup(h => h.ForeignTradeImportEarlyClearanceFlag).Returns("Y");
			AssertEquals("Y", Provider.ForeignTradeImportEarlyClearanceFlag);
		}

		public void TestForeignTradeImportEarlyClearanceFlag_Snapshot()
		{
			isModificationMessage = false;
			snapshot.ForeignTradeFlag = "X";
			AssertEquals("X", Provider.ForeignTradeImportEarlyClearanceFlag);
		}

		public void TestForeignTradeImportEarlyClearanceFlag_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertNull(Provider.ForeignTradeImportEarlyClearanceFlag);
		}

		protected override SCWPEDLineProvider GetProvider() => new SCWPEDLineProviderForTest(reconEntryLine, isModificationMessage, headerProvider, lineProvider, snapshot);

		class SCWPEDLineProviderForTest : SCWPEDLineProvider
		{
			public SCWPEDLineProviderForTest(CusReconEntryLine reconEntryLine, bool isModificationMessage, ISCWRECHeader headerProvider, ISCWRECLine lineProvider, DEMonthlyClosingEntryLineSnapshot snapshot) : base(reconEntryLine, isModificationMessage)
			{
				this.snapshot = snapshot;
				this.lineProvider = lineProvider;
				this.headerProvider = headerProvider;
			}

			protected override IImportDecHeader GetHeaderProvider(CusEntryHeader entryHeader) => headerProvider;

			protected override IImportDecLine GetLineProvider(CusEntryLine entryLine) => lineProvider;

			protected override DEMonthlyClosingEntryLineSnapshot LoadCurrentSnapshot() => snapshot;

			readonly IImportDecHeader headerProvider;
			readonly IImportDecLine lineProvider;
			readonly DEMonthlyClosingEntryLineSnapshot snapshot;
		}
	}
}
