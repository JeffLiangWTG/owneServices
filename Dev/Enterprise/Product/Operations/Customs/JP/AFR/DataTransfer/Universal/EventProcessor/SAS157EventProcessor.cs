using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS157EventProcessor : AFREventProcessor
	{
		public SAS157EventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory) : base(eventDataObject, logger, factory)
		{
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
			var billNumber = eventDataObject.Context.MBOLNumber.GetValueOrDefault();
			if (!billNumber.IsEmpty)
			{
				var bill = header?.Bills.FirstOrDefault(x => x.JPB_BillNumber == billNumber);
				if (bill == null)
				{
					logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Bill '{0}' could not be found on AFR Job '{1}'.", billNumber, header?.JPH_JobReference));
				}
				else
				{
					if (bill.JPB_ReleaseStatus == AFRBillCustomsStatusList.Codes.NL2)
					{
						bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
						logger.Log(Integration.LogType.Information,
							string.Format(CultureInfo.InvariantCulture,
								"The Bill Customs Status of AFR Job '{2}' changes from '{0}' to '{1}'.", AFRBillCustomsStatusList.Codes.NL2, AFRBillCustomsStatusList.Codes.Registered, header.JPH_JobReference));
					}
				}
			}
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.NotificationnofHouseBillRegistrationStatus;
		}

		protected override bool IsSuccessResponse => true;
	}
}
