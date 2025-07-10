using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataConverters.CustomsFiles;
using Enterprise.DataConverters.Testing.Base;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework.TestHelper;

namespace Enterprise.DataConverters.Testing.DataWriters
{
	internal abstract class ClassificationWriterTestBase : DataWriterTestCase
	{
		public override void TestAllFieldsInRecordAreImportedProperly()
		{
			var classification = (ClassificationWriter)GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(classification);

			classification.SaveRecordToEnterprise(false, Logger);

			var filter = new ZQuery(CusClassificationSchema.CC_LookupCode, classification.LookupCode);
			var newRecord = (BaseCusClassification)Factory.LoadTop1(GetExpectedBusinessObjectType(), filter);
			AssertNotNull("Classification should exist", newRecord);
			AssertEquals(classification.LookupCode, newRecord.CC_LookupCode);
			AssertEquals(classification.Description, newRecord.CC_Description);
			AssertEquals(classification.ClassificationType, newRecord.CC_ClassificationType);

			AssertCountrySpecificData(classification, newRecord);
		}

		protected override void FillInRecordWithInvalidDetails(DataWriter writer)
		{
			((ClassificationWriter)writer).LookupCode = ZString.Empty;
		}

		protected Type GetExpectedBusinessObjectType() =>
			TestedTypeHelper.GetTestedType(GetType());

		protected abstract void AssertCountrySpecificData(ClassificationWriter classification, BaseCusClassification bizO);
	}
}
