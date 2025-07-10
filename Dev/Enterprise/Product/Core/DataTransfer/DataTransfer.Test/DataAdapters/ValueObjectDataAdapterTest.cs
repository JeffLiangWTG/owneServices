using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	public abstract class ValueObjectDataAdapterTest<TBusinessObject, TValueObject>
			: ValueObjectDataAdapterResourceAgnosticTest<TBusinessObject, TValueObject>
				where TBusinessObject : BusinessObject
				where TValueObject : IValueObject
	{
		protected abstract BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample();
		protected abstract BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample();
		protected abstract BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample();
		protected abstract BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects();

		protected virtual void TestExportToValueObject(BusinessObjectAndExpectedOutputFileName sample) =>
			base.TestExportToValueObject(sample);

		protected virtual void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample) =>
			base.TestExportToAndImportFromAndExportToValueObject(sample);

		protected virtual void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample,
			TBusinessObject bizObjToImportTo, TValueObject exportedValueObject, string exportedValueObjectXml,
			string bizObjToImportToDescription) =>
			base.AssertImportFromThenExportToProducesSameXml(sample, bizObjToImportTo, exportedValueObject, exportedValueObjectXml, bizObjToImportToDescription);

		protected sealed override BusinessObjectSampleAndExpectedOutput GetEmptyBusinessObjectSampleAndExpectedOutput() =>
			GetEmptyBizObjSample();

		protected sealed override BusinessObjectSampleAndExpectedOutput GetPopulatedBusinessObjectWithEmptyFieldsSampleAndExpectedOutput() =>
			GetPopulatedBizObjWithEmptyFieldsSample();

		protected sealed override BusinessObjectSampleAndExpectedOutput GetFullyPopulatedBusinessObjectSampleAndExpectedOutput() =>
			GetFullyPopulatedBizObjSample();

		protected sealed override BusinessObjectSampleAndExpectedOutput[] GetMiscBusinessObjectSamplesAndExpectedOutputs() =>
			GetMiscSampleBusinessObjects()?.Cast<BusinessObjectSampleAndExpectedOutput>().ToArray();

		protected sealed override void TestExportToValueObject(BusinessObjectSampleAndExpectedOutput sample) =>
			TestExportToValueObject((BusinessObjectAndExpectedOutputFileName)sample);

		protected sealed override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectSampleAndExpectedOutput sample) =>
			TestExportToAndImportFromAndExportToValueObject((BusinessObjectAndExpectedOutputFileName)sample);

		protected sealed override void AssertImportFromThenExportToProducesSameXml(BusinessObjectSampleAndExpectedOutput sample,
			TBusinessObject bizObjToImportTo, TValueObject exportedValueObject, string exportedValueObjectXml,
			string bizObjToImportToDescription) =>
				AssertImportFromThenExportToProducesSameXml((BusinessObjectAndExpectedOutputFileName)sample, bizObjToImportTo, exportedValueObject, exportedValueObjectXml, bizObjToImportToDescription);
	}
}
