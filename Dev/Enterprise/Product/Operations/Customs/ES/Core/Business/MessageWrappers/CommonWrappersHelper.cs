using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public static class CommonWrappersHelper
{
	public static ZBool WarehouseTypeListContainsCode(ZString code)
	{
		var codesList = new ZString[]
		{
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2,
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP,
			CusAuthorizationHeaderTypeList.Codes.TemporaryStorage,
			ESCusAuthorisationHeaderTypeList.Codes.InAPrivateOtherThanCustomsWarehouse,
			ESCusAuthorisationHeaderTypeList.Codes.InAPublicOtherThanCustomsWarehouseTypeI,
			ESCusAuthorisationHeaderTypeList.Codes.InAPublicOtherThanCustomsWarehouseTypeIi,
			ESCusAuthorisationHeaderTypeList.Codes.InAPublicRefWarehouseOnlyCanaryIslandAdministration,
		};

		return codesList.Contains(code);
	}

	public static IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> GetDocumentSequenceNumberWrapperList(List<CusSupportingInfo> documents, bool shouldSendReferenceNumber = true, bool shouldSendDescription = false)
	{
		var docs = new List<CommonDocumentSequenceNumberWrapper>();
		ZShort seqNum = 1;
		foreach (var doc in documents)
		{
			var reference = shouldSendReferenceNumber ? doc.CSI_ReferenceNumber : shouldSendDescription ? doc.CSI_Description : ZString.Empty;
			docs.Add(new CommonDocumentSequenceNumberWrapper(doc.CSI_Code, reference, seqNum));
			seqNum++;
		}
		return docs.AsReadOnly();
	}

	public static IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(List<CusSupportingInfo> documents, bool shouldSendReferenceNumber = true, bool shouldSendDescription = false)
	{
		var docs = new List<CommonDocumentSequenceNumberWrapper>();
		var orderedDocs = documents.OrderBy(doc => doc.CSI_LineNo);
		foreach (var doc in orderedDocs)
		{
			var reference = shouldSendReferenceNumber ? doc.CSI_ReferenceNumber : shouldSendDescription ? doc.CSI_Description : ZString.Empty;
			docs.Add(new CommonDocumentSequenceNumberWrapper(doc.CSI_Code, reference, doc.CSI_LineNo));
		}
		return docs.AsReadOnly();
	}

	public static ZString GetIncotermPlaceCode(JobComInvoiceHeader invoiceHeader, JobDeclaration declaration)
	{
		return !invoiceHeader.ZG_AgreedPlaceCode.IsEmpty ? invoiceHeader.ZG_AgreedPlaceCode.Length > 2 ? invoiceHeader.ZG_AgreedPlaceCode : ZString.Empty
						: !declaration.ZG_AgreedPlaceCode.IsEmpty ? declaration.ZG_AgreedPlaceCode.Length > 2 ? declaration.ZG_AgreedPlaceCode : ZString.Empty
						: ZString.Empty;
	}

	public static ZString GetIncotermPlaceCodeCountry(JobComInvoiceHeader invoiceHeader, JobDeclaration declaration)
	{
		return !invoiceHeader.ZG_AgreedPlaceCode.IsEmpty ? invoiceHeader.ZG_AgreedPlaceCode.Length == 2 ? invoiceHeader.ZG_AgreedPlaceCode : ZString.Empty
						: !declaration.ZG_AgreedPlaceCode.IsEmpty ? declaration.ZG_AgreedPlaceCode.Length == 2 ? declaration.ZG_AgreedPlaceCode : ZString.Empty
						: ZString.Empty;
	}
}
