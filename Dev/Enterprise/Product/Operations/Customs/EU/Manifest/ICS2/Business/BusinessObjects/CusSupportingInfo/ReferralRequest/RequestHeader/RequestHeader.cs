using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RequestHeader :
		EUMemberStateCommunication,
		Integration.Customs.ICusSupportingInfoTypeSupporter,
		ICusStorageDocPivotTypeSupporter
	{
		public RequestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("6A70FE3C-3E1D-4D87-983E-CD720A787E98", "Request Header");

		public new RequestHeaderValidation Validation => (RequestHeaderValidation)base.Validation;
		protected override EUMemberStateCommunicationValidation GetNewValidation() => new RequestHeaderValidation(this);

		public new RequestHeaderLookups Lookups => (RequestHeaderLookups)base.Lookups;
		protected override EUMemberStateCommunicationLookups GetNewLookups() => new RequestHeaderLookups(this);

		new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public AsycudaBill RelatedHouseBill => Parent.Bills.Cast<AsycudaBill>().FirstOrDefault(b => b.ABL_BillNumber == base.EUS_HouseBillNumber);

		#region Properties

		[ResourceStringData("72370E2B-3DBB-4091-925B-9F80922BA5B1", Caption = "Identifier")]
		public override ZString EUS_Identifier { get => base.EUS_Identifier; set => base.EUS_Identifier = value; }

		[List(nameof(Lookups) + "." + nameof(RequestHeaderLookups.RequestTypeList))]
		[ResourceStringData("52D7A0BD-1869-48A2-82E2-E4298FCF1005", Caption = "Type")]
		public override ZString EUS_Type { get => base.EUS_Type; set => base.EUS_Type = value; }

		[ResourceStringData("9B9B6C18-7149-4B1D-9476-3AE4CB13B51B", Caption = "Text")]
		public ZString RequestTypeDescription => Lookups.RequestTypeList.GetDescriptionFromCode(EUS_Type);

		[ResourceStringData("67DDFC63-BBAE-406F-A02F-08816B6AE3BA", Caption = "Message Element")]
		public override ZString EUS_MessageElement { get => base.EUS_MessageElement; set => base.EUS_MessageElement = value; }

		[List(nameof(Lookups) + "." + nameof(RequestHeaderLookups.HouseBillList))]
		[ResourceStringData("F66E632D-290E-4117-9C09-A86C2C6930A9", Caption = "House Bill Number")]
		public override ZString EUS_HouseBillNumber { get => base.EUS_HouseBillNumber; set => base.EUS_HouseBillNumber = value; }

		[ResourceStringData("470F608C-C6D2-436E-89EB-65AF43E55D3C", Caption = "HRCM Screening Method")]
		public override ZString EUS_ScreeningMethod { get => base.EUS_ScreeningMethod; set => base.EUS_ScreeningMethod = value; }

		[List(nameof(Lookups) + "." + nameof(RequestHeaderLookups.TransportDocumentTypeList))]
		[ResourceStringData("CB0E6491-6A72-4F50-A642-0B58F9CEF68A", Caption = "Transport Document Type")]
		public override ZString EUS_TransportDocumentType { get => base.EUS_TransportDocumentType; set => base.EUS_TransportDocumentType = value; }

		[ResourceStringData("D4BEEBD1-2B32-477C-BC65-102411B36B3E", Caption = "Responsible Member State")]
		[ReadOnly(true)]
		public override ZString EUS_MemberState { get => base.EUS_MemberState; set => base.EUS_MemberState = value; }

		[ResourceStringData("8207106F-24E3-43CE-96AF-D2B0FEF4FD21", Caption = "Include HRCM Details")]
		public override ZBool EUS_IncludeScreeningDetails { get => base.EUS_IncludeScreeningDetails; set => base.EUS_IncludeScreeningDetails = value; }

		[ReadOnly(true)]
		[ResourceStringData("C02A2270-619A-40FD-9DD8-592B364C2B0E", Caption = "Status")]
		public override ZString EUS_Status { get => base.EUS_Status; set => base.EUS_Status = value; }

		public ZBool IsAwaiting => base.EUS_Status == MessageStatusCodeList.Codes.Awaiting;

		public ZBool IsSubmited => base.EUS_Status == MessageStatusCodeList.Codes.Sent;

		#endregion

		#region Request Informations

		[ChildEditable(true)]
		public RequestInformationCollection RequestInformations
		{
			get
			{
				if (requestInformations == null)
				{
					requestInformations = new RequestInformationCollection(this);
					requestInformations.Load();
					RegisterEditableChildObject(requestInformations);
				}

				return requestInformations;
			}
		}

		RequestInformationCollection requestInformations;

		#endregion

		#region Request Responses

		[ChildEditable(true)]
		public RequestResponseCollection RequestResponses
		{
			get
			{
				if (requestResponses == null)
				{
					requestResponses = new RequestResponseCollection(this);
					requestResponses.Load();
					RegisterEditableChildObject(requestResponses);
				}

				return requestResponses;
			}
		}

		RequestResponseCollection requestResponses;

		#endregion

		#region Supporting Documents

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(this);
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}

		SupportingDocumentCollection supportingDocuments;

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region ICusStorageDocPivotTypeSupporter

		Type ICusStorageDocPivotTypeSupporter.CusStorageDocPivotType => typeof(CusStorageDocPivot);

		IEnumerable<IStorageDocsBaseCollection> ICusStorageDocPivotTypeSupporter.EDocCollections => EDocsHelper.GetEDocCollections(Parent);

		void ICusStorageDocPivotTypeSupporter.ReloadCollection()
		{
			if (IsEDocPivotCollectionLoaded)
			{
				Attachments.Reload(true);
			}
		}

		bool IsEDocPivotCollectionLoaded => attachments?.IsLoaded ?? false;

		[ChildEditable(true)]
		public CusStorageDocPivotCollection Attachments
		{
			get
			{
				if (attachments == null)
				{
					attachments = new CusStorageDocPivotCollection(this);
					attachments.Load();
					RegisterEditableChildObject(attachments);
				}

				return attachments;
			}
		}

		CusStorageDocPivotCollection attachments;

		#endregion

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type>()
			{
				{ CusSupportingInfoTypeList.Codes.RequestInformation, typeof(RequestInformation) },
				{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
				{ CusSupportingInfoTypeList.Codes.RequestResponse, typeof(RequestResponse) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion
	}
}
