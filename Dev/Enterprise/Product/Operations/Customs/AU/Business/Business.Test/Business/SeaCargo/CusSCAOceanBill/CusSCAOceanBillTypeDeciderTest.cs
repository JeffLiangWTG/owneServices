using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillTypeDeciderTest : SeaCargoTypeDeciderTest
	{
		public void TestGetTypeForLoad()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var oceanBill2 = factory2.Load<CusSCAOceanBill>(oceanBill.PK);
			AssertEquals("Failed to return Forced Legacy Mode", GetCMRType(), oceanBill2.GetType());

			oceanBill.CB_ApplicationCode = "";
			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			var oceanBill3 = factory2.Load<CusSCAOceanBill>(oceanBill.PK);
			AssertEquals("Failed to return Forced Legacy Mode", GetCMRType(), oceanBill3.GetType());
		}

		protected override TypeDecider GetTypeDecider()
		{
			return new CusSCAOceanBillTypeDecider();
		}

		protected override Type GetCMRType()
		{
			return typeof(CusSCAOceanBill);
		}

		protected override DataRow GetDataRowForCMRObject()
		{
			DataTable cusSCAOceanBillTable = ((INeedDataSet)Factory).Data.Tables[CusSCAOceanBill.Schema.TableName];
			if (cusSCAOceanBillTable == null)
			{
				cusSCAOceanBillTable = ((INeedDataSet)Factory).Data.Tables.Add(CusSCAOceanBill.Schema.TableName);
				DataColumn applicationCodeColumn = new DataColumn(CusSCAOceanBillSchema.CB_ApplicationCode.Name, CusSCAOceanBillSchema.CB_ApplicationCode.DotNetType);
				cusSCAOceanBillTable.Columns.Add(applicationCodeColumn);
			}
			DataRow result = cusSCAOceanBillTable.NewRow();
			result[CusSCAOceanBillSchema.CB_ApplicationCode.Name] = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			return result;
		}
	}
}
