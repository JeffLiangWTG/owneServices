using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Base
{
	sealed internal class DataWriterTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestSaveRecordToEnteprise_NullLogger()
		{
			var writer = new TestDataWriter(Factory);
			writer.SaveRecordToEnterprise(false, null);
		}

		#region TestDataWriter

		sealed internal class TestDataWriter : DataWriter
		{
			public TestDataWriter(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override internal ZString GetAnyReasonRecordShouldBeExcluded()
			{
				return ZString.Empty;
			}

			protected override BusinessObject GetExistingBusinessObject()
			{
				return null;
			}

			protected override BusinessObject GetNewBusinessObject()
			{
				return null;
			}

			public override ZString RecordDescription
			{
				get { return ZString.Empty; }
			}

			protected override void UpdateEnterpriseValues(BusinessObject businessObjectToUpdate)
			{
			}
		}

		#endregion
	}
}
