using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseTypeDeciderTest : SeaCargoTypeDeciderTest
	{
		protected override TypeDecider GetTypeDecider()
		{
			return new CusSCAHouseTypeDecider();
		}

		protected override Type GetCMRType()
		{
			return typeof(CusSCAHouse);
		}

		protected override DataRow GetDataRowForCMRObject()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			DataTable cusSCAHouseTable = ((INeedDataSet)Factory).Data.Tables[CusSCAHouse.Schema.TableName];
			if (cusSCAHouseTable == null)
			{
				cusSCAHouseTable = ((INeedDataSet)Factory).Data.Tables.Add(CusSCAHouse.Schema.TableName);
				DataColumn fK = new DataColumn(CusSCAHouseSchema.CA_CB.Name, CusSCAHouseSchema.CA_CB.DotNetType);
				cusSCAHouseTable.Columns.Add(fK);
			}
			DataRow result = cusSCAHouseTable.NewRow();
			result[CusSCAHouseSchema.CA_CB.Name] = oceanBill.PK.ToGuid();
			return result;
		}
	}
}
