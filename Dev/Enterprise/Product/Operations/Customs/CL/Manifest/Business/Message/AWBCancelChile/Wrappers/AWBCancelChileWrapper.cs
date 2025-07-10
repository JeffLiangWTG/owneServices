using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AWBCancelChileWrapper : IAWBCancelRequest
	{
		public AWBCancelChileWrapper(AsycudaBill bill, ZString reason)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			header = this.bill.Header;
			ObservationDescription = reason;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;

		string IAWBCancelRequest.DocumentID => bill.CustomsEntryNumber;

		string IAWBCancelRequest.ReferenceNumber => bill.ABL_BillNumber;

		string IAWBCancelRequest.PartialCorrelative
		{
			get
			{
				var arrivalDetail = CLMessageHelper.GetAsycudaArrivalLine(bill);
				var awbCancelRequestIsPartial = arrivalDetail == null || arrivalDetail.ATL_Quantity != 0;
				var arrivalHeader = arrivalDetail?.ArrivalHeader;

				return awbCancelRequestIsPartial && arrivalHeader != null ? arrivalHeader.ATH_ArrivalSequence.ToString() : string.Empty;
			}
		}

		string IAWBCancelRequest.ReferenceDocument
		{
			get
			{
				var query = new ZQuery(CusEntryNumSchema.CE_ParentTable, AsycudaManifestHeader.Schema.TableName);
				query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
				query.AddToFilter(CusEntryNumSchema.CE_ParentID, header.PK);
				return header.Factory.LoadTop1<CusEntryNumber>(query)?.CE_EntryNum ?? ZString.Empty;
			}
		}

		string IAWBCancelRequest.ParticipationIDValue => GlbCompany.CurrentCompany.GC_BusinessRegNo;

		string IAWBCancelRequest.DateValue => header.AMA_MasterBillIssueDate.ToString(WrappersConstants.DateFormatShort);

		public string ObservationDescription { get; }
	}
}
