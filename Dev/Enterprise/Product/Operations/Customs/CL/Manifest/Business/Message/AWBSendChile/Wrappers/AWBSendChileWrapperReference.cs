using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class AWBSendChileWrapperReference : IDocReferences
	{
		internal AWBSendChileWrapperReference(AsycudaBill bill, string referenceType, AsycudaArrivalLine arrivalDetail)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.referenceType = referenceType;
			header = this.bill.Header;
			this.arrivalDetail = arrivalDetail;
		}
		readonly AsycudaBill bill;
		readonly string referenceType;
		readonly AsycudaManifestHeader header;
		readonly AsycudaArrivalLine arrivalDetail;

		string IDocReferences.ReferenceType => referenceType;

		string IDocReferences.DocumentType => referenceType == WrappersConstants.ReferenceType.Ref ? WrappersConstants.DocumentType.Mftoa : WrappersConstants.DocumentType.Ga;

		string IDocReferences.Number
		{
			get
			{
				ZString number;
				if (referenceType == WrappersConstants.ReferenceType.Ref)
				{
					if (arrivalDetail == null)
					{
						number = header.RegistrationNumber;
					}
					else
					{
						number = arrivalDetail.ArrivalHeader.ATH_Reference;
					}
				}
				else
				{
					number = bill.ABL_BillNumber;
				}
				return number;
			}
		}

		string IDocReferences.Date
		{
			get
			{
				ZString date;
				if (referenceType == WrappersConstants.ReferenceType.Ref)
				{
					if (arrivalDetail == null)
					{
						date = header.RegistrationDate.ToString(WrappersConstants.DateFormatShort);
					}
					else
					{
						date = arrivalDetail.ArrivalHeader.ATH_ReferenceIssueDate.ToString(WrappersConstants.DateFormatShort);
					}
				}
				else
				{
					date = bill.ABL_BillIssueDate.ToString(WrappersConstants.DateFormatShort);
				}
				return date;
			}
		}
	}
}
