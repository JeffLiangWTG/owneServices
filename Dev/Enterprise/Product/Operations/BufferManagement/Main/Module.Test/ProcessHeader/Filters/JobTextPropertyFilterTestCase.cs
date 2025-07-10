using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(JobTextPropertyFilter))]
	class JobTextPropertyFilterTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestSerializeXml()
		{
			var filter1 = new JobTextPropertyFilter(new JobTextPropertyQuery((op, a, b, c) => new ZQuery()), new GetList(() => System.Array.Empty<object>()));
			filter1.WorkflowTypeCode = "WKI";
			filter1.JobPropertyName = "WKI_Test";
			filter1.Property = "SomeValue";

			var filter2 = new JobTextPropertyFilter(new JobTextPropertyQuery((op, a, b, c) => new ZQuery()), new GetList(() => System.Array.Empty<object>()));

			BMSTestHelper.SerialiseAndDeSerialise(filter1, filter2);

			AssertEquals("WKI", filter2.WorkflowTypeCode);
			AssertEquals("WKI_Test", filter2.JobPropertyName); // not a schema column name
			AssertEquals("SomeValue", filter2.Property);
		}

		public void TestValidation_Property_InvalidSchema()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("WKI", "Foo", "Bar");
			var filter1 = new JobTextPropertyFilter(new JobTextPropertyQuery((op, a, b, c) => new ZQuery()), new GetList(() => list));
			filter1.WorkflowTypeCode = "WKI";
			filter1.JobPropertyName = "XA9G_Test"; // XA9G isn't a prefix for a real table in our schema.
			filter1.Property = "SomeValue";
			filter1.Validation.ValidateAll();

			AssertNoErrors(filter1.PropertyInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobTextPropertyFilter(new JobTextPropertyQuery((op, a, b, c) => new ZQuery()), new GetList(() => System.Array.Empty<object>()));
		}
	}
}
