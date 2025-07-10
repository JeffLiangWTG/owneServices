using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs.CA.DIF;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.Business
{
	[UniversalDataContext(DataContextType.CADIFDocument)]
	public class DIFDocument : AutoDIFDocument, IDISDocumentBase, IDIFDocument, IStmALogParent
	{
		public DIFDocument(DIFHostWrapper hostWrapper)
			: base(hostWrapper.Factory)
		{
			HostWrapper = Argument.NotNull(hostWrapper, nameof(hostWrapper));
			defaultValues = HostWrapper.DefaultValues;
		}

		public static class Constants
		{
			public const string XmlNamespace = "http://www.cargowise.com/Schemas/DIFDocument";
		}
		public readonly DIFHostWrapper HostWrapper;
		readonly ICADIFDefaultValues defaultValues;

		[List(nameof(Lookups) + "." + nameof(DIFDocumentLookups.BusinessNumbers))]
		[ResourceStringData("DIFDocument|BusinessNumber", Caption = "Document Owner/Stake Holder")]
		public override ZString BusinessNumber { get => base.BusinessNumber; set => base.BusinessNumber = value; }

		[List(nameof(Lookups) + "." + nameof(DIFDocumentLookups.PGAs))]
		[ResourceStringData("DIFDocument|PGA", Caption = "PGA")]
		public override ZString PGA { get => base.PGA; set => base.PGA = value; }

		[List(nameof(Lookups) + "." + nameof(DIFDocumentLookups.DocumentTypes))]
		[ResourceStringData("DIFDocument|DocumentType", Caption = "Document Type")]
		public override ZString DocumentType { get => base.DocumentType; set => base.DocumentType = value; }

		[ResourceStringData("DIFDocument|DocumentDescription", Caption = "Description")]
		public override ZString DocumentDescription { get => base.DocumentDescription; set => base.DocumentDescription = value; }

		[List(nameof(Lookups) + "." + nameof(DIFDocumentLookups.EDocsList))]
		[ReadOnlyMember(nameof(EDocsDocumentPK_ReadOnly))]
		[ResourceStringData("DIFDocument|EDocsDocumentPK", Caption = "eDocs")]
		public override ZGuid EDocsDocumentPK
		{
			get { return base.EDocsDocumentPK; }
			set
			{
				base.EDocsDocumentPK = value;
				var eDoc = EDoc;
				if (eDoc != null)
				{
					if (RequiredDocumentPK.IsEmpty)
					{
						var requiredDoc = RequiredDocuments.Cast<JobRequiredDocument>().FirstOrDefault(x => x.EQ_DocType == eDoc.DocType);
						if (requiredDoc != null)
						{
							RequiredDocumentPK = requiredDoc.PK;
						}
					}
					if (DocumentDescription.IsEmpty)
					{
						DocumentDescription = eDoc.Description.Left(Schema.DocumentDescriptionMaxLength);
					}
				}
			}
		}

		bool EDocsDocumentPK_ReadOnly
		{
			get { return !CanDelete; }
		}

		public override bool CanDelete
		{
			get { return !StatusList.IsDocumentIDSentToCustoms(Status) && !StatusList.IsWaitingForResponse(Status); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return StatusList.IsWaitingForResponse(Status) ? ResString.GetMultilingualString("5F4AAAA7-E78C-4135-AE80-3613B2E8FCA6", "This document has been submitted to Customs. Delete is not allowed when a response from Customs is outstanding.") :
					StatusList.IsDocumentIDSentToCustoms(Status) ? ResString.GetMultilingualString("D430A9E4-F9B5-4CF8-96C2-F921E268CADE", "This document has been accepted by Customs. Delete is not allowed once a document is accepted.") : (NoResString)string.Empty;
			}
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			var result = base.GetWarningBeforeBeingDeleted();

			if (result.IsEmpty)
			{
				if (!URN.IsEmpty)
				{
					result = ResString.GetMultilingualString("71330B0C-2FB7-49E7-9224-64A89ED7435A", "URN is already assigned. If you proceed, this URN will be discarded. Are you sure you wish to continue?");
				}
			}
			return result;
		}

		[ResourceStringData("DIFDocument|Comment", Caption = "Comment")]
		public override ZString Comment { get => base.Comment; set => base.Comment = value; }

		[ResourceStringData("DIFDocument|URN", Caption = "URN")]
		public ZString URN => RequiredDocumentAddInfo?.EX_ReferenceNumber ?? ZString.Empty;

		public ZPropertyInfo URNInfo
		{
			get { return GetZPropertyInfo(nameof(URN)); }
		}

		[ReadOnlyMember(nameof(DocumentNumber_ReadOnly))]
		[ResourceStringData("DIFDocument|DocumentNumber", Caption = "Ref/Permit/License No")]
		public override ZString DocumentNumber
		{
			get
			{
				var documentNumber = defaultValues?.DocumentNumber;
				return !documentNumber.GetValueOrDefault().IsEmpty ? documentNumber.Value : base.DocumentNumber;
			}
			set => base.DocumentNumber = value;
		}
		public ZBool DocumentNumber_ReadOnly => !defaultValues?.DocumentNumber.IsEmpty ?? false;

		[ReadOnlyMember(nameof(EffectiveDate_ReadOnly))]
		[ResourceStringData("DIFDocument|EffectiveDate", Caption = "Effective Date")]
		public override ZDateTime EffectiveDate
		{
			get
			{
				var effectiveDate = defaultValues?.EffectiveDate;
				return effectiveDate.GetValueOrDefault().IsValid ? effectiveDate.Value : base.EffectiveDate;
			}
			set => base.EffectiveDate = value;
		}
		public ZBool EffectiveDate_ReadOnly => defaultValues?.EffectiveDate.IsValid ?? false;

		[ReadOnlyMember(nameof(ExpiryDate_ReadOnly))]
		[ResourceStringData("DIFDocument|ExpiryDate", Caption = "Expiry Date")]
		public override ZDateTime ExpiryDate
		{
			get
			{
				var expiryDate = defaultValues?.ExpiryDate;
				return expiryDate.GetValueOrDefault().IsValid ? expiryDate.Value : base.ExpiryDate;
			}
			set => base.ExpiryDate = value;
		}
		public ZBool ExpiryDate_ReadOnly => defaultValues?.ExpiryDate.IsValid ?? false;

		[MaxLength(3)]
		[ReadOnlyMember(nameof(Status_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(DIFDocumentLookups.StatusList))]
		public ZString Status
		{
			get { return status; }
			set
			{
				SetNonPersistentPropertyValue(StatusInfo, ref status, value);
			}
		}
		ZString status;

		bool Status_ReadOnly => true;

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(nameof(Status)); }
		}

		public ZString StatusDescription
		{
			get { return Status.IsEmpty ? "Not Sent" : Factory.GetCachedValue<StatusList>().GetDescriptionFromCode(Status); }
		}

		[List(nameof(Lookups) + "." + nameof(DIFDocumentLookups.RequiredDocuments))]
		[ReadOnlyMember(nameof(RequiredDocumentPK_ReadOnly))]
		[ResourceStringData("DIFDocument|RequiredDocumentPK", Caption = "Tracking Document")]
		public ZGuid RequiredDocumentPK
		{
			get { return requiredDocumentPK; }
			set
			{
				SetNonPersistentPropertyValue(RequiredDocumentPKInfo, ref requiredDocumentPK, value);
				Validation.ValidateRequiredDocumentPK();
				if (RequiredDocumentAddInfo != null)
				{
					RequiredDocumentAddInfo = null;
				}
			}
		}
		ZGuid requiredDocumentPK;
		bool RequiredDocumentPK_ReadOnly
		{
			get { return !CanDelete || !URN.IsEmpty; }
		}
		public ZPropertyInfo RequiredDocumentPKInfo
		{
			get { return GetZPropertyInfo(nameof(RequiredDocumentPK)); }
		}

		public JobRequiredDocument RequiredDocument
		{
			get { return Factory.Load<JobRequiredDocument>(RequiredDocumentPK); }
		}

		public IBusinessObjectCollection RequiredDocuments
		{
			get { return Lookups.RequiredDocuments; }
		}

		public JobRequiredDocumentAddInfo RequiredDocumentAddInfo
		{
			get
			{
				return requiredDocumentAddInfo;
			}
			set
			{
				if (requiredDocumentAddInfo != null)
				{
					requiredDocumentAddInfo.EX_ReferenceNumberInfo.ValueChanged -= UpdateCLP_DIFRefNumberOrLocationOfLPCO;
				}
				requiredDocumentAddInfo = value;
				if (LPCO != null && requiredDocumentAddInfo != null)
				{
					requiredDocumentAddInfo.EX_ReferenceNumberInfo.ValueChanged += UpdateCLP_DIFRefNumberOrLocationOfLPCO;
				}
			}
		}
		JobRequiredDocumentAddInfo requiredDocumentAddInfo;

		ZString IDISDocumentBase.Serialize(string xmlNamespace)
		{
			return BusinessObjectXmlSerializer.Serialize(this, xmlNamespace);
		}

		void IDISDocumentBase.Deserialize(string xml, string xmlNamespace)
		{
			BusinessObjectXmlSerializer.Deserialize(this, xml, xmlNamespace);
		}

		public void Initialize()
		{
			if (RequiredDocumentAddInfo != null && !RequiredDocumentAddInfo.EX_AddInfo.IsEmpty)
			{
				this.Deserialize(RequiredDocumentAddInfo.EX_AddInfo, DIFDocument.Constants.XmlNamespace);
			}
		}

		public DIFDocumentLookups Lookups
		{
			get { return new DIFDocumentLookups(this); }
		}

		public IeDoc EDoc
		{
			get { return EDocsDocumentPK.IsValid ? HostWrapper.GetEDoc(EDocsDocumentPK) : null; }
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
					RegisterEditableChildObject(messages);
					messages.SetReadOnlyIncludingChildren(true);
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		public ZString MessageSendingError
		{
			get
			{
				var sb = new ZStringBuilder();
				var eDoc = EDoc;
				if (eDoc != null && DIFDocumentValidation.HasInvalidCharacters(eDoc.FileName))
				{
					sb.Append(DIFDocumentValidation.FileNameHasInvalidCharacters);
				}

				return sb.ToStringWithNewLineBetweenAppends();
			}
		}

		ZGuid IDIFDocument.StakeHolderOrg
		{
			get
			{
				return HostWrapper.DISHost?.BusinessNumberHolder?.PK ?? ZGuid.Empty;
			}
		}

		public CusCALPCO LPCO
		{
			get;
			set;
		}

		public void SetDefaultFromLPCO(CusCALPCO lpco)
		{
			if (lpco != null)
			{
				LPCO = lpco;
				DocumentType = lpco.CLP_Type;
				DocumentNumber = lpco.CLP_RefNo;
				EffectiveDate = lpco.CLP_StartDate;
				ExpiryDate = lpco.CLP_EndDate;
				PGA = (lpco.Parent as IPGAHeader)?.GovAgencyIDCode ?? ZString.Empty;
			}
		}

		void UpdateCLP_DIFRefNumberOrLocationOfLPCO(object sender, EventArgs e)
		{
			if (LPCO != null && RequiredDocumentAddInfo != null)
			{
				LPCO.CLP_DIFRefNumberOrLocation = RequiredDocumentAddInfo.EX_ReferenceNumber;
			}
		}

		public MessageSendingAction MessageSendingAction
		{
			get
			{
				if (messageSendingAction == null)
				{
					messageSendingAction = new MessageSendingAction(this);
					messageSendingAction.Send = this.status.IsEmpty;
				}
				return messageSendingAction;
			}
		}
		MessageSendingAction messageSendingAction;

		#region IStmALogParent

		IStmALogParent stmALogParent => RequiredDocumentAddInfo;

		bool IStmALogParent.IsDeleted => stmALogParent?.IsDeleted ?? false;
		ZGuid IStmALogParent.LogsParentPK => stmALogParent?.LogsParentPK ?? ZGuid.Empty;
		string IStmALogParent.LogsParentTableName => stmALogParent?.LogsParentTableName ?? string.Empty;
		Logs IStmALogProvider.Logs => stmALogParent?.Logs;
		BusinessObjectFactory IStmALogProvider.LogsFactory => stmALogParent?.LogsFactory;
		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => stmALogParent?.BusinessObjectsWithRelatedEvents;
		bool IStmALogParent.DeferFiringWorkflow => stmALogParent?.DeferFiringWorkflow ?? false;

		void IStmALogParent.ProcessLog(IStmALog log)
		{
			if (stmALogParent != null)
			{
				stmALogParent.ProcessLog(log);
			}
		}
		#endregion
	}
}
