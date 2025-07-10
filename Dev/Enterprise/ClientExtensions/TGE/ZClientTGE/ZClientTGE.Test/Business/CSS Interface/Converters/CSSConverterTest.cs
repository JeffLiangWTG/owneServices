using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TGE.Business.CSSInterface.Testing
{
	internal abstract class CSSConverterTest<T> : TestCaseWithFactory where T : CSSConverter
	{
		public void TestMapping()
		{
			foreach (BusinessObject bizObj in GetPopulatedBizos())
			{
				TestMappingCore(bizObj);
			}
		}

		void TestMappingCore(BusinessObject bizObj)
		{
			FlatFileDataRowCollection results = Converter.MapExport(bizObj);
			CombineAssertions(delegate
			{
				AssertEquals("Converter is error free.", false, Converter.HasErrors);
				AssertEquals("Total rows", 1, results.Count);
			});
		}

		T Converter
		{
			get
			{
				if (converter == null)
				{
					converter = NewCSSConverter();
					converter.FileSequenceNumber = "00000089";
				}

				return converter;
			}
		}

		T converter;
		protected abstract T NewCSSConverter();
		protected TGETestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TGETestHelper(Factory));
			}
		}

		TGETestHelper testHelper;
		protected abstract IList<BusinessObject> GetPopulatedBizos();
	}
}
