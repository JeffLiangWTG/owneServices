using System;
using System.Data;

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEProcessQueueTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;

			switch (row[ProcessQueueSchema.Constants.P4_ParentTableCode].ToString())
			{
				case CusHAWBSchema.Constants.Prefix:
					if (!String.IsNullOrEmpty(row[ProcessQueueSchema.Constants.P4_QueueName].ToString()) &&
						row[ProcessQueueSchema.Constants.P4_Status].ToString() != ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment)
					{
						result = typeof(UPECalloutQueue);
					}
					else
					{
						result = typeof(UPECargoReportQueue);
					}
					break;

				case JobDeclarationSchema.Constants.Prefix:
					result = typeof(UPEDeclarationQueue);
					break;
			}

			return result;
		}
	}
}
