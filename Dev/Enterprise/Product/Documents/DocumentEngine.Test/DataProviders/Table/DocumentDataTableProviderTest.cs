using System;
using System.Data;
using Enterprise.DocumentEngine.RuntimeOptions;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class DocumentDataTableProviderTest : TestCase
	{
		public DocumentDataTableProviderTest() : base()
		{ }

		class PlainOldPKGuidDataProvider : DocumentDataTableProvider
		{
			protected override DataTable GetDataTable(Guid pK)
			{
				return new DataTable(this.ToString() + pK.ToString());
			}

			internal new DataTable GetDataTable(CollectionOfIFilter filters) => base.GetDataTable(filters);
		}

		public void TestPlainOldPKGuid()
		{
			PlainOldPKGuidDataProvider p = new PlainOldPKGuidDataProvider();
			Guid g = Guid.NewGuid();
			CollectionOfIFilter f = new CollectionOfIFilter();
			f.Add(new PrimaryKeyFilter("", g));

			AssertEquals("GetDataTable", p.ToString() + g.ToString(), p.GetDataTable(f).ToString());
		}

		[ExpectException(typeof(Exception))]
		public void TestPlainOldPKGuidMissingPKFilter()
		{
			PlainOldPKGuidDataProvider p = new PlainOldPKGuidDataProvider();
			p.GetDataTable(new CollectionOfIFilter());
		}
	}
}
