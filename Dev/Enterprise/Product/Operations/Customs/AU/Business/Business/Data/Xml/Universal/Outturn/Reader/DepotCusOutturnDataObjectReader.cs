using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class DepotCusOutturnDataObjectReader : DataTransfer.Universal.Outturn.CusOutturnDataObjectReader<DepotCusOutturn>
	{
		protected new readonly CusOutturnHeader outturnheader;

		public DepotCusOutturnDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusOutturnHeader outturnheader)
			: base(dataObject, logger, factory, outturnheader)
		{
			this.outturnheader = Argument.NotNull(outturnheader, nameof(outturnheader));
		}

		protected override DepotCusOutturn GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			DepotCusOutturn result = null;

			var container = dataObject.ContainerCollection != null ? dataObject.ContainerCollection.FirstOrDefault() : null;
			var cargoType = container?.ContainerType.GetCodeAsUpperCase() ?? ZString.Empty;
			var containerNumber = container?.ContainerNumber.GetValueOrDefault() ?? ZString.Empty;

			var additionBill = dataObject.AdditionalBillCollection != null ?
				dataObject.AdditionalBillCollection.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House) : null;
			var houseBill = additionBill?.BillNumber.GetValueOrDefault() ?? ZString.Empty;
			var masterBill = additionBill?.ParentBillNumber.GetValueOrDefault() ?? ZString.Empty;

			result = outturnheader.Outturns.Cast<DepotCusOutturn>().FirstOrDefault(x => x.C5_CargoType == cargoType && x.C5_ContainerNumber == containerNumber && x.C5_HouseBill == houseBill
						&& x.C5_MasterBill == masterBill);

			return result;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(DepotCusOutturn outturn)
		{
			// need to check for message status; don't allow update if the existing targetBO is readonly.
			var reason = new ZStringBuilder();
			reason.Append(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(outturn));

			if (outturn != null && outturn.C5_MessageStatus == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
			{
				reason.Append(Res.GetString("A9B9FFCC-06C7-4930-86B0-7BEFC7C98442", "Can't update read-only Sea Cargo Outturn with cargo type '{0}' and container '{1}' and house bill '{2}' and master bill '{3}'",
					outturn.C5_CargoType, outturn.C5_ContainerNumber, outturn.C5_HouseBill, outturn.C5_MasterBill));
			}

			return reason.ToStringWithDelimiterBetweenAppends(" ");
		}
	}
}
