using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TGE.Business.CSSInterface.Testing
{
	internal abstract class CSSMapperTest<T> : TestCaseWithFactory where T : CSSMapper, new()
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
			FlatFileDataRowCollection results = Mapper.Map(bizObj);
			CombineAssertions(delegate
			{
				AssertEquals("Total rows", 1, results.Count);
				CSSDataRow dataRow = results[0] as CSSDataRow;
				AssertCommonContents(dataRow);
				AssertSpecificContents(dataRow, bizObj.GetType());
			});
		}

		void AssertCommonContents(CSSDataRow dataRow)
		{
			AssertEquals("SequenceNumber", "00000089", dataRow.SequenceNumber);
			AssertEquals("FileCreateDate", true, dataRow.FileCreateDate.IsValid);
			AssertEquals("FileCreateTime", true, dataRow.FileCreateTime.IsValid);
		}

		T Mapper
		{
			get
			{
				if (mapper == null)
				{
					mapper = new T();
					mapper.FileSequenceNumber = "00000089";
				}

				return mapper;
			}
		}

		T mapper;
		protected TGETestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TGETestHelper(Factory));
			}
		}

		TGETestHelper testHelper;
		protected abstract IList<BusinessObject> GetPopulatedBizos();
		protected abstract void AssertSpecificContents(CSSDataRow dataRow, Type bizoSource);
	}
}
