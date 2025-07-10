using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class COLSLodgementMessageBuilder : COLSMessageBuilder<COLSLodgementMessage>
	{
		public COLSLodgementMessageBuilder(QuarantineColsHeader colsHeader, ZString additionalComment)
			: base(colsHeader)
		{
			this.additionalComment = additionalComment;
		}
		readonly ZString additionalComment;

		const int UnpackAddressMaxLength = 50;
		const int ContactNameMaxLength = 40;
		const int EmailAddressMaxLength = 40;
		const int PhoneNumberMaxLength = 10;

		protected override ZString MessageType => AUCOLSMessageTypeList.Codes.AddNewLodgement;

		protected override BusinessObject MessageParent => colsHeader;

		protected override COLSLodgementMessage GetMessageData()
		{
			var declaration = colsHeader.JobDeclaration;
			var responsibleParty = colsHeader.ResponsibleParty;
			var deliveryOrUnpack = colsHeader.DeliveryOrUnpack;
			var phoneNum = GetLocalFormattedPhoneNumber(responsibleParty.E2_Phone_Formatted.IsEmpty ? responsibleParty.E2_Mobile_Formatted : responsibleParty.E2_Phone_Formatted);
			var stringBuilder = new ZStringBuilder();
			stringBuilder.AppendIfNotEmpty(deliveryOrUnpack.Address1);
			stringBuilder.AppendIfNotEmpty(deliveryOrUnpack.Address2);
			stringBuilder.AppendIfNotEmpty(deliveryOrUnpack.City);
			stringBuilder.AppendIfNotEmpty(deliveryOrUnpack.Postcode);
			stringBuilder.AppendIfNotEmpty(deliveryOrUnpack.State);
			var unpackAddress = new ZString(stringBuilder.ToStringWithDelimiterBetweenAppends(","));
			var branchPK = declaration?.Branch?.PK.ToGuid() ?? Guid.Empty;
			var branchID = Env.Registry.AUCustoms.GetLocalCustomsBranchIdentifierForBranch(branchPK) ?? string.Empty;
			var directionRequests = new List<COLSDirectionRequests>();
			foreach (QuarantineColsDirection direction in colsHeader.Directions)
			{
				if (!direction.QCD_Direction.IsEmpty)
				{
					var directionAddress = direction.AAAddress;
					var containerNumber = direction.Container?.CO_ContainerNumber ?? ZString.Empty;
					var entryLineNumber = direction.CusEntryLine?.UniqueKey ?? ZString.Empty;
					directionRequests.Add(new COLSDirectionRequests
					{
						direction = direction.QCD_Direction,
						directionLineContainer = containerNumber.IsEmpty ? entryLineNumber : containerNumber,
						treatmentType = direction.QCD_TreatmentType,
						location = directionAddress.Address1,
						aaname = directionAddress.E2_CompanyName,
						aanumber = directionAddress.E2_GovRegNum
					});
				}
			}

			var lodgementMessageData = new COLSLodgementMessage()
			{
				entryNumber = colsHeader.IMPNumber,
				branchId = branchID.ToUpper(),
				biconReference = colsHeader.QCH_BiosecurityImportConditionURL,
				importPermitNumber = colsHeader.QCH_ImportPermitNumber,
				contactName = responsibleParty.E2_Contact.Left(ContactNameMaxLength),
				phoneNumber = phoneNum.Left(PhoneNumberMaxLength),
				email = responsibleParty.E2_Email.Left(EmailAddressMaxLength),
				thirdPartyInd = !colsHeader.QCH_AlsoNotifyEmail.IsEmpty,
				thirdPartyEmail = colsHeader.QCH_AlsoNotifyEmail,
				aaRefNum = colsHeader.QCH_ApprovedArrangementRefNum,
				lateLodgementReason = colsHeader.QCH_LateLodgementReason,
				lateLodgementDetails = colsHeader.QCH_LateLodgementDetails,
				directionRequests = directionRequests,
				deliveryClassification = colsHeader.QCH_DeliveryClassification,
				unpackAddress = unpackAddress.Left(UnpackAddressMaxLength),
				additionalComment = additionalComment,
				generalDeclaration = "True"
			};
			return lodgementMessageData;
		}
	}
}
