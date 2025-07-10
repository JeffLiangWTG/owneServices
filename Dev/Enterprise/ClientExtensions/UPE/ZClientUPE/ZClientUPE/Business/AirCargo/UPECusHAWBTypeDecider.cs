using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	class UPECusHAWBTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => typeof(UPECusHAWB);

		public override Type GetTypeForNew() => typeof(UPECusHAWB);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var cusHAWBPk = new ZGuid(row[CusHAWB.Schema.PK]);
			var processQueue = factory.LoadTop1<ProcessQueue>(new ZQuery(ProcessQueueSchema.P4_ParentID, cusHAWBPk));
			if (processQueue != null && !processQueue.P4_QueueName.IsEmpty && processQueue.P4_Status != ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment)
			{
				return typeof(Callout);
			}
			else
			{
				return typeof(UPECusHAWB);
			}
		}
	}
}
