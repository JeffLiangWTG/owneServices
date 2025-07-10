using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class CommonPreviousDocument : EU.NCTS.Business.CommonPreviousDocument
	{
		public CommonPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(nameof(CSI_ReferenceNumber_MaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[MaxLength(nameof(CSI_ReferenceNumber2_MaxLength))]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		public int CSI_ReferenceNumber_MaxLength =>
			IsPhase5Departure
				? IsInPhase5TransitionPeriod ? 35 : 70
				: AutoCusSupportingInfo.Schema.CSI_ReferenceNumberMaxLength;

		public int CSI_ReferenceNumber2_MaxLength =>
			IsPhase5Departure
				? IsInPhase5TransitionPeriod ? 26 : 35
				: AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2MaxLength;

		public override bool IsPhase5Departure => Header?.IsPhase5Departure ?? false;

		public bool IsPhase5Arrival => Header?.IsPhase5Arrival ?? false;

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = base.CSI_Code;
				base.CSI_Code = value;

				if (!IsCopying
					&& oldValue != value
					&& (value == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830 || oldValue == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830)
					&& Parent is NctsBill bill
					&& bill.IsPhase5Departure)
				{
					CreateOrRemoveAdditionalDocumentInfForN830(bill);
				}
			}
		}

		public override void Delete()
		{
			var bill = Parent as NctsBill;
			var code = CSI_Code;
			base.Delete();
			if (bill != null && bill.IsPhase5Departure && code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830)
			{
				CreateOrRemoveAdditionalDocumentInfForN830(bill);
			}
		}

		static void CreateOrRemoveAdditionalDocumentInfForN830(NctsBill bill)
		{
			var n830Exists = bill.PreviousDocuments
				.Any(d => d.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830);
			var infExists = bill.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Any(d => d.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && d.CSI_Code == AdditionalInfoCodes._20300);

			if (n830Exists && !infExists)
			{
				var infAddInfo = bill.AdditionalDocuments.AddNew();
				infAddInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				infAddInfo.CSI_Code = AdditionalInfoCodes._20300;
			}
			else if (infExists && !n830Exists)
			{
				bill.AdditionalDocuments.Where(d => d.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && d.CSI_Code == AdditionalInfoCodes._20300).DeleteAll();
			}
		}

		protected override CusSupportingInfoValidation GetNewValidation() => IsPhase5Arrival ? new CusSupportingInfoDisabledValidation(this) : base.GetNewValidation();
	}
}
