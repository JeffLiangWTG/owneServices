using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CommonAnnexH7SendMessageWrapper : H7CommonSendMessageWrapper, ICommonAnnexMessageDataProvider
	{
		public CommonAnnexH7SendMessageWrapper(AsycudaBill bill, ICertificateProvider certificate, IeDoc eDoc, AdditionalInfoSendingObject additionalInfoSendingObject, UploadDocumentsSendingAction sendingObject) : base(bill, certificate)
		{
			this.eDoc = Argument.NotNull(eDoc, nameof(eDoc));
			this.additionalInfoSendingObject = Argument.NotNull(additionalInfoSendingObject, nameof(additionalInfoSendingObject));
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		const string H7OperationCode = "02";
		const string ReqDispatchTagName = "SolicitudDespacho";

		readonly IeDoc eDoc;
		readonly AdditionalInfoSendingObject additionalInfoSendingObject;
		readonly UploadDocumentsSendingAction sendingObject;

		public ZString Operation => H7OperationCode;

		public ZString Reference
		{
			get
			{
				if (string.IsNullOrEmpty(reference))
				{
					var cusEntryNumQuery = new ZQuery();
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaBillSchema.Constants.TableName);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, Bill.PK);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryLineReference, "H7");
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Spain);

					var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(cusEntryNumQuery);

					reference = cusEntryNumber.CE_EntryNum;
				}

				return reference;
			}
		}
		ZString reference;

		public ZString RequestDispatchTagName => ReqDispatchTagName;

		public ZString DispatchRequest => sendingObject.ClearanceRequested ? DispatchRequestBool.Yes : DispatchRequestBool.No;

		public IAnnexDocCommon Document => document ??= new CommonAnnexDocWrapper(eDoc, eDoc.FileName, additionalInfoSendingObject);
		IAnnexDocCommon document;

		public ZString AdministrationCode
		{
			get
			{
				if(!string.IsNullOrEmpty(Bill.Header.AMA_CustomsOffice))
				{
					return IsATCAdmin() ? MessageProcessorConstants.CustomsAdministration.ATC : MessageProcessorConstants.CustomsAdministration.AEAT;
				}

				return null;
			}
		}

		bool IsATCAdmin()
		{
			return Bill.Header.AMA_CustomsOffice.StartsWith("ES0035")
				|| Bill.Header.AMA_CustomsOffice.StartsWith("ES0038")
				|| Bill.Header.AMA_CustomsOffice.StartsWith("ESD35")
				|| Bill.Header.AMA_CustomsOffice.StartsWith("ESD38");
		}

		public static class DispatchRequestBool
		{
			public const string Yes = "S";
			public const string No = "N";
		}
	}
}
