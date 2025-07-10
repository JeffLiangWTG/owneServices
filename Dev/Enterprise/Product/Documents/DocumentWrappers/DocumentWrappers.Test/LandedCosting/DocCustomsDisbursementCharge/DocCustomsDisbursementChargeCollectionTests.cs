using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCustomsDisbursementChargeCollection))]
	sealed class DocCustomsDisbursementChargeCollectionTests : DocumentWrapperCollectionTest<DocCustomsDisbursementChargeCollection>
	{
		protected sealed override object GetNewObjectToWrap()
		{
			return null;
		}

		protected sealed override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			GenericWrapper wrapper = GetNewWrapperToAddToTheCollection();
			collection.Add(wrapper);
			return wrapper;
		}

		GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var mock = new Mock<ICustomsChargeLCItemSetting>();
			mock.Setup(m => m.CostType).Returns(new ZString("TDT"));
			mock.Setup(m => m.DocumentCustomLabelCode).Returns(new ZString("DutyAmount"));
			mock.Setup(m => m.Description).Returns(new ZString("Total Duty"));
			CustomsDisbursementCharge customsDisbursementCharge = new CustomsDisbursementCharge(mock.Object, 100m);
			return DocCustomsDisbursementCharge.New(customsDisbursementCharge, Factory);
		}

		protected override DocCustomsDisbursementChargeCollection GetNewDocumentWrapperCollection()
		{
			var mock = new Mock<ICustomsChargeLCItemSetting>();
			mock.Setup(m => m.CostType).Returns(new ZString("TDT"));
			mock.Setup(m => m.DocumentCustomLabelCode).Returns(new ZString("DutyAmount"));
			mock.Setup(m => m.Description).Returns(new ZString("Total Duty"));
			DocCustomsDisbursementChargeCollection collection = new DocCustomsDisbursementChargeCollection(Factory);
			collection.Add(DocCustomsDisbursementCharge.New(new CustomsDisbursementCharge(mock.Object, 180m), Factory));
			return collection;
		}

		public override void TestTypedStringIndexer()
		{
			var mock1 = new Mock<ICustomsChargeLCItemSetting>();
			mock1.Setup(m => m.CostType).Returns(new ZString("TDT"));
			mock1.Setup(m => m.DocumentCustomLabelCode).Returns(new ZString("DutyAmount"));
			mock1.Setup(m => m.Description).Returns(new ZString("Total Duty"));

			var mock2 = new Mock<ICustomsChargeLCItemSetting>();
			mock2.Setup(m => m.CostType).Returns(new ZString("OTH"));
			mock2.Setup(m => m.DocumentCustomLabelCode).Returns(new ZString("FlatOrOtherDutyAmount"));
			mock2.Setup(m => m.Description).Returns(new ZString("OTH Duty"));

			DocCustomsDisbursementChargeCollection collection = new DocCustomsDisbursementChargeCollection(Factory);
			collection.Add(DocCustomsDisbursementCharge.New(new CustomsDisbursementCharge(mock1.Object, 180m), Factory));
			collection.Add(DocCustomsDisbursementCharge.New(new CustomsDisbursementCharge(mock2.Object, 270m), Factory));

			AssertEquals(180m, collection["TDT"].Amount);
			AssertEquals(270m, collection["OTH"].Amount);
		}
	}
}
