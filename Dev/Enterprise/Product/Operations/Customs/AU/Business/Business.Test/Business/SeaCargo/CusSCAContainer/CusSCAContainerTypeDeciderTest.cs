using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAContainerTypeDeciderTest : SeaCargoTypeDeciderTest
	{
		protected override TypeDecider GetTypeDecider()
		{
			return new CusSCAContainerTypeDecider();
		}

		protected override Type GetCMRType()
		{
			return typeof(CusSCAContainer);
		}

		protected override DataRow GetDataRowForCMRObject()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			DataTable cusSCAContainerTable = ((INeedDataSet)Factory).Data.Tables[CusSCAContainer.Schema.TableName];
			if (cusSCAContainerTable == null)
			{
				cusSCAContainerTable = ((INeedDataSet)Factory).Data.Tables.Add(CusSCAContainer.Schema.TableName);
				DataColumn fK = new DataColumn(CusSCAContainerSchema.CN_CB.Name, CusSCAContainerSchema.CN_CB.DotNetType);
				cusSCAContainerTable.Columns.Add(fK);
			}
			DataRow result = cusSCAContainerTable.NewRow();
			result[CusSCAContainerSchema.CN_CB.Name] = oceanBill.PK.ToGuid();
			return result;
		}
	}
}
