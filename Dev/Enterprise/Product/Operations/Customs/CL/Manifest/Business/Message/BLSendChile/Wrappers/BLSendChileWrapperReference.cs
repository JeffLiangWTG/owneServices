using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperReference : IReference
	{
		internal BLSendChileWrapperReference(AsycudaBill bill, string referenceType)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			header = this.bill.Header;
			this.referenceType = referenceType;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;
		readonly string referenceType;

		string IReference.ReferenceType => referenceType;

		string IReference.DocumentType => referenceType == WrappersConstants.ReferenceType.Ref ? WrappersConstants.DocumentType.Mfto : WrappersConstants.DocumentType.Bl;

		string IReference.Number
		{
			get
			{
				ZString number;
				if (referenceType == WrappersConstants.ReferenceType.Ref)
				{
					ZQuery query = new ZQuery(CusEntryNumSchema.CE_ParentTable, AsycudaManifestHeader.Schema.TableName);
					query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
					query.AddToFilter(CusEntryNumSchema.CE_ParentID, header.PK);
					number = header.Factory.LoadTop1<CusEntryNumber>(query)?.CE_EntryNum ?? ZString.Empty;
				}
				else
				{
					number = bill.ABL_BillNumber;
				}
				return number;
			}
		}

		string IReference.Date
		{
			get
			{
				ZString date;
				if (referenceType == WrappersConstants.ReferenceType.Ref)
				{
					ZQuery query = new ZQuery(CusEntryNumSchema.CE_ParentTable, AsycudaManifestHeader.Schema.TableName);
					query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
					query.AddToFilter(CusEntryNumSchema.CE_ParentID, header.PK);
					date = header.Factory.LoadTop1<CusEntryNumber>(query)?.CE_IssueDate.ToString(WrappersConstants.DateFormatShort) ?? ZString.Empty;
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
