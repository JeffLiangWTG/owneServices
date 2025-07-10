using System;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestHeaderCreator
	{
		public CusMiscRequestHeader Create(ExtendedHoursRequestHeader extendedHoursRequestHeader)
		{
			var header = extendedHoursRequestHeader.Factory.New<CusMiscRequestHeader>();

			header.CMR_CustomsOffice = extendedHoursRequestHeader.CustomsOffice + extendedHoursRequestHeader.Department;
			header.CMR_MessageType = extendedHoursRequestHeader.MessageType;
			header.CMR_GB = extendedHoursRequestHeader.BranchPK;
			header.CMR_GS_NKBroker = GlbStaff.CurrentUser.GS_Code;
			header.CMR_RequestDate = ZDateTime.Now;
			header.CreateCusEntryNumber();
			header.CusEntryNumber.CE_IssueDate = extendedHoursRequestHeader.StartDate;
			header.CusEntryNumber.CE_ExpiryDate = extendedHoursRequestHeader.EndDate;
			header.CMR_RequestDetails = CreateHeaderRemarks(extendedHoursRequestHeader);

			foreach (ExtendedHoursRequestLine extendedHoursRequestLine in extendedHoursRequestHeader.ExtendedHoursRequestLines)
			{
				var line = header.RequestLines.AddNew();

				line.CML_EntryNumber = extendedHoursRequestLine.ReferenceNumber;
				line.CML_EntryType = extendedHoursRequestLine.ReferenceNumberType;
				line.CML_Remarks = CreateLineRemarks(extendedHoursRequestHeader.IsExport, extendedHoursRequestLine);
			}

			return header;
		}

		public CusMiscRequestHeader Create(FinalPriceReportByDateExtensionHeader finalPriceReportByDateExtensionHeader)
		{
			var header = finalPriceReportByDateExtensionHeader.Factory.New<CusMiscRequestHeader>();

			header.CMR_CustomsOffice = finalPriceReportByDateExtensionHeader.CustomsOffice;
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			header.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			header.CMR_GB = finalPriceReportByDateExtensionHeader.GB_Branch;
			header.CMR_GS_NKBroker = GlbStaff.CurrentUser.GS_Code;
			header.CMR_RequestDate = ZDateTime.Now;

			foreach (FinalPriceReportByDateExtensionLine finalPriceReportByDateExtensionLine in finalPriceReportByDateExtensionHeader.FinalPriceReportByDateExtensionLines)
			{
				var line = header.RequestLines.AddNew();
				line.CML_EntryNumber = finalPriceReportByDateExtensionLine.ImportDeclarationNumber;
				line.CML_EntryType = SharedJobMessageTypeList.Codes.Import;
				line.CML_Remarks = CreateLineRemarks(finalPriceReportByDateExtensionLine);
			}

			return header;
		}

		public EDIMessage CreateEdiMessage<T>(CusMiscRequestHeader cusMiscRequestHeader, T nonBusinessObject)
		{
			EDIMessage message = null;

			try
			{
				message = cusMiscRequestHeader.Messages.AddNew();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message.EM_Status = EDIMessageStatusList.Codes.Queued;

				SetEM_MessageTextOrDataSource(message, nonBusinessObject);

				cusMiscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			}
			catch (Exception)
			{
				message.Delete();
				return null;
			}

			return message;
		}

		protected virtual void SetEM_MessageTextOrDataSource<T>(EDIMessage message, T nonBusinessObject)
		{
			if (nonBusinessObject.GetType() == typeof(ExtendedHoursRequestHeader))
			{
				var extendedHoursRequestHeader = nonBusinessObject as ExtendedHoursRequestHeader;
				if (extendedHoursRequestHeader.IsExport)
				{
					message.EM_MessageType = ElectronicDocumentTypeList.Codes._5AC;
					var export5AC = new Export5ACCreator().Create(extendedHoursRequestHeader);
					var stream = new GOVCBR5ACMessageBuilder(export5AC).MessageContent;
					message.SetEM_MessageTextOrDataSource(stream);
				}
				else
				{
					message.EM_MessageType = ElectronicDocumentTypeList.Codes._5GW;
					var import5GW = new Import5GWCreator().Create(extendedHoursRequestHeader);
					var stream = new GOVCBR5GWMessageBuilder(import5GW).MessageContent;
					message.SetEM_MessageTextOrDataSource(stream);
				}
			}
			else
			{
				var finalPriceReportByDateExtensionHeader = nonBusinessObject as FinalPriceReportByDateExtensionHeader;
				message.EM_MessageType = ElectronicDocumentTypeList.Codes._5SG;

				var import5SG = new Import5SGHeaderCreator().Create(finalPriceReportByDateExtensionHeader);
				var stream = new GOVCBR5SGMessageBuilder(import5SG).MessageContent;
				message.SetEM_MessageTextOrDataSource(stream);
			}
		}

		#region SuppressResourceStringsCheckRegion
		internal const string EmptyDate = "날짜를 입력하지 않았습니다.";

		public string CreateHeaderRemarks(ExtendedHoursRequestHeader header)
		{
			ZStringBuilder remark = new ZStringBuilder();
			remark.Append("임시개청 시작일시 : " + (header.StartDate == ZDateTime.Empty ? EmptyDate : header.StartDate.ToString(DateFormatType.DateTimeKoreanNoSecond)));
			remark.Append("임시개청 종료일시 : " + (header.EndDate == ZDateTime.Empty ? EmptyDate : header.EndDate.ToString(DateFormatType.DateTimeKoreanNoSecond)));
			remark.Append("임시개청 사유 : " + header.Reason);
			return remark.ToStringWithNewLineBetweenAppends();
		}

		public string CreateLineRemarks(bool isExport, ExtendedHoursRequestLine extendedHoursRequestLine)
		{
			ZStringBuilder remark = new ZStringBuilder();

			ZStringBuilder commonRemark = new ZStringBuilder();
			commonRemark.Append("총포장개수 : " + string.Format("{0:#,##0}", extendedHoursRequestLine.PackageCount));
			commonRemark.Append("총중량 : " + string.Format("{0:#,##0.000}", extendedHoursRequestLine.TotalWeight) + "(" + extendedHoursRequestLine.UQ + ")");

			if (isExport)
			{
				remark.Append("신고가격(USD) : " + string.Format("{0:#,##0}", extendedHoursRequestLine.CustomsValue));
				remark.Append(commonRemark);
				remark.Append("수출화주 : " + extendedHoursRequestLine.SupplierName);
			}
			else
			{
				remark.Append("과세가격(USD) : " + string.Format("{0:#,##0}", extendedHoursRequestLine.CustomsValue));
				remark.Append(commonRemark);
				remark.Append("품명 : " + extendedHoursRequestLine.HSDescription);
				remark.Append("납세의무자상호 : " + extendedHoursRequestLine.PayerCompanyName);
				remark.Append("(예정)장치장소 : " + extendedHoursRequestLine.BondedAreaCode);
			}

			return remark.ToStringWithNewLineBetweenAppends();
		}

		public string CreateLineRemarks(FinalPriceReportByDateExtensionLine finalPriceReportByDateExtensionLine)
		{
			ZString extensionDate = finalPriceReportByDateExtensionLine.ExtensionDate == ZDateTime.Empty ? EmptyDate : finalPriceReportByDateExtensionLine.ExtensionDate.ToString(DateFormatType.DateKorean);
			return "(" + extensionDate + ") " + finalPriceReportByDateExtensionLine.ApplicationReason;
		}
		#endregion
	}
}
