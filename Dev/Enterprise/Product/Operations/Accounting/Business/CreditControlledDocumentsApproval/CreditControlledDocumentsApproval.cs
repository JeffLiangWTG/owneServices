using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.CreditControl;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using static CargoWise.EventReference.Constants;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business
{
	[CodeProperty(Schema.XP_RequestID)]
	[DescriptionProperty(Schema.XP_ReasonDescription)]
	[UniversalDataContext(DataContextType.CreditControlledApprovalRequest)]
	public class CreditControlledDocumentsApproval : GenApprovalRequest, IDocManagerSupport, ICreditControlledDocumentsApproval, IJobNumber, IAccountingNumberFountainDataSource, IEDocsParsingSupport
	{
		BusinessObject parentBizObj;
		CreditControlledDocumentsApprovalData approvalData;
		bool shouldEmailBeSent;

		public CreditControlledDocumentsApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			XP_GB_RequestingBranch = GlbBranch.CurrentBranch.PK;
			XP_SubSystem = Core.Constants.GenApprovalRequestSubSystem.Accounting;
			XP_ApprovalType = Core.Constants.GenApprovalRequestApprovalType.ARCreditControlledDocuments;
			XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Requested;
		}

		public void Initialize(BusinessObject parentBusinessObject)
		{
			Initialize(parentBusinessObject, ZGuid.Empty, Array.Empty<int>());
		}

		public void Initialize(BusinessObject parentBusinessObject, ZGuid menuItemPK)
		{
			Initialize(parentBusinessObject, menuItemPK, Array.Empty<int>());
		}

		public void Initialize(BusinessObject parentBO, ZGuid menuItemPK, int[] authorizationLevel, string requestReasonDescription = null)
		{
			documents = null;
			Argument.NotNull(parentBO, "parentBusinessObject");
			XP_ParentID = parentBO.PK;
			XP_ParentTableCode = parentBO.TablePrefix;
			XP_PrivledgeRequired = authorizationLevel != null ? string.Join(", ", authorizationLevel).TrimEnd() : string.Empty;
			XP_ReasonDescription = new ZString(requestReasonDescription).Left(Schema.XP_ReasonDescriptionMaxLength);
			parentBizObj = parentBO;
			ApprovalData.MenuItemPK = menuItemPK;
			if (AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues.Value != ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Default.Code)
			{
				ApprovalData.OrganizationsForCreditCheckWithAmountsCollection = GetOrganizationsForCreditCheckWithAmounts();
			}
		}

		OrganizationsForCreditCheckWithAmountsCollection GetOrganizationsForCreditCheckWithAmounts()
		{
			var orgWithAmountsCollection = new OrganizationsForCreditCheckWithAmountsCollection();
			if (Job != null)
			{
				foreach (var org in OrganisationsForCreditChecks)
				{
					var orgWithAmounts = new OrganizationsForCreditCheckWithAmounts();
					orgWithAmounts.OrgPK = org.PK;

					var postedAmount = 0M;
					var unpostedAmount = 0M;
					foreach (Charge charge in Job.Charges.Where(x => x.JR_OH_SellAccount == org.PK))
					{
						if (charge.IsRevenuePosted)
						{
							postedAmount += charge.TotalLocalRevenueAmount;
						}
						else
						{
							unpostedAmount += charge.TotalLocalRevenueAmount;
						}
					}

					orgWithAmounts.PostedAmount = postedAmount;
					orgWithAmounts.UnpostedAmount = unpostedAmount;

					orgWithAmountsCollection.Add(orgWithAmounts);
				}
			}

			return orgWithAmountsCollection;
		}

		internal AccountingNumberFountainWrapper CreditControlApprovalNumberFountain => AccountingNumberFountainWrapperFactory.Instance.CreditControlApproval;

		public CreditStatusBizObjForDocumentApproval CreditStatusBizObj => creditStatusBizObj ?? (creditStatusBizObj = new CreditStatusBizObjForDocumentApproval(Factory, OrganisationsInBreach.Cast<OrganisationInBreach>().Select(x => x.OrganisationPK)));
		CreditStatusBizObjForDocumentApproval creditStatusBizObj;

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CreditControlledDocumentsApproval) { UseBusinessEntityFactoryAsInternal = true });
		DocManagerInfo docManagerInfo;

		public StorageMain StorageMain => storageMain ?? (storageMain = (StorageMain)DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(this, Core.Constants.DocManagerCodes.CreditControlledDocumentsApproval));
		StorageMain storageMain;

		public void SetupEDocsFactoryToBeSavedWithApprovalFactory()
		{
			var docFactory = (BusinessObjectFactory)DocManagerInfo.MasterFactory;
			Factory.ChildFactories.Add(docFactory);
		}

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region ApprovalData

		CreditControlledDocumentsApprovalData ApprovalData
		{
			get
			{
				if (approvalData == null)
				{
					ReadApprovalData();
				}
				return approvalData;
			}
		}

#if DEBUG
		internal CreditControlledDocumentsApprovalData ApprovalData_ForTestOnly => ApprovalData;
#endif
		#endregion

		#region JobNumber

		[ResourceStringData("CreditControlledDocumentsApproval|JobNumber", Caption = "Job Number")]
		public ZString JobNumber
		{
			get
			{
				if (jobNumber.IsEmpty)
				{
					jobNumber = Job != null ? Job.JH_JobNum : ZString.Empty;

					if (jobNumber.IsEmpty)
					{
						jobNumber = ParentBusinessObject != null ? GetJobNumber(ParentBusinessObject) : ZString.Empty;
					}
				}
				return jobNumber;
			}
		}
		ZString jobNumber;

		public Job Job => Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, SQLComparisonOperator.Equal, XP_ParentID).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));

		internal static ZString GetJobNumber(BusinessObject bizo)
		{
			var bizoAsIJobNum = bizo as IJobNumber;
			return bizoAsIJobNum != null ? bizoAsIJobNum.JobNumber : CodePropertyAttribute.CodeFromBusinessObject(bizo).ToString();
		}

		string IJobNumber.JobNumber => JobNumber;

		#endregion

		#region Department

		public ZString Department
		{
			get
			{
				if (department.IsEmpty)
				{
					var job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, SQLComparisonOperator.Equal, XP_ParentID).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
					department = (job != null && job.Department != null) ? job.Department.GE_Code : ZString.Empty;
				}
				return department;
			}
		}
		ZString department;

		#endregion

		#region INCO Term

		public ZString IncoTermCode
		{
			get
			{
				if (incoTermCode.IsEmpty && ParentBusinessObject != null)
				{
					incoTermCode = (ParentBusinessObject as IJobInvoicingPlugIn)?.GetINCOTermCode() ?? ZString.Empty;
				}
				return incoTermCode;
			}
		}
		ZString incoTermCode;
		public ZString IncoTermDescription
		{
			get
			{
				if (incoTermDescription.IsEmpty && ParentBusinessObject != null)
				{
					incoTermDescription = (ParentBusinessObject as IJobInvoicingPlugIn)?.GetINCOTermDescription(Factory) ?? ZString.Empty;
				}
				return incoTermDescription;
			}
		}
		ZString incoTermDescription;

		#endregion

		#region Queries

		public bool RequestAlreadyMade
		{
			get
			{
				var filter = GetBaseFilter();
				filter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);
				var existingRequests = Factory.Load<CreditControlledDocumentsApproval>(filter);
				var result = DoAnyOfTheseRequestsCoverThisMenuItem(existingRequests);

				if (!result)
				{
					CancelApprovalRequestsForSameDocumentWithIgnoringPrivilegeAndAmountIfApplicable();
				}

				return result;
			}
		}

		void CancelApprovalRequestsForSameDocumentWithIgnoringPrivilegeAndAmountIfApplicable()
		{
			var filter = GetBaseFilter(checkPrivilege: false);
			filter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);
			var existingRequests = Factory.Load<CreditControlledDocumentsApproval>(filter);
			var coveringApprovalRequests = GetCoveringApprovalRequests(existingRequests, ignoreAmountsIfApplicable: true);
			coveringApprovalRequests.ForEach(x =>
			{
				x.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Cancelled;
				x.XP_GS_NKApprovingUser1 = ZString.Empty;
				x.XP_ApprovalDate = ZDateTime.Empty;
			});
		}

		public CreditControlledDocumentsApproval GetLastRequestIfWasRejected()
		{
			if (RequestAlreadyApproved)
			{
				return null;
			}

			var filter = GetBaseFilter();
			filter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);
			var existingRequests = Factory.Load<CreditControlledDocumentsApproval>(filter);
			var lastRequestForSameDocument = GetCoveringApprovalRequests(existingRequests)
															.OrderByDescending(request => request.XP_SystemCreateTimeUtc)
															.FirstOrDefault();

			if (lastRequestForSameDocument != null && lastRequestForSameDocument.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Rejected)
			{
				return lastRequestForSameDocument;
			}

			return null;
		}

		IEnumerable<CreditControlledDocumentsApproval> GetCoveringApprovalRequests(CreditControlledDocumentsApproval[] existingRequests, bool ignoreAmountsIfApplicable = false)
		{
			return existingRequests.Where(request => (request.ApprovalData.ApproveAllDocuments || request.ApprovalData.MenuItemPK == ApprovalData.MenuItemPK)
					&& (ignoreAmountsIfApplicable
						|| AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues.Value == ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Default.Code
						|| Job == null
						|| ApprovalData.OrganizationsForCreditCheckWithAmountsCollection == null
						|| ApprovalData.OrganizationsForCreditCheckWithAmountsCollection.IsSubsetOf(
							request.ApprovalData.OrganizationsForCreditCheckWithAmountsCollection,
							AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues.Value == ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Increased.Code
							)
						)
					);
		}

		public bool RequestAlreadyApproved
		{
			get
			{
				var filter = GetBaseFilter();
				filter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.Equal, Core.Constants.GenApprovalRequestApprovalStatus.Approved);

				var approvedRequests = Factory.Load<CreditControlledDocumentsApproval>(filter);
				return DoAnyOfTheseRequestsCoverThisMenuItem(approvedRequests);
			}
		}

		bool DoAnyOfTheseRequestsCoverThisMenuItem(IEnumerable<CreditControlledDocumentsApproval> requests)
		{
			if (requests.Contains(null))
			{
				requests = requests.Where(r => r != null);
			}
			return GetCoveringApprovalRequests(requests.ToArray()).Any();
		}

		ZQuery GetBaseFilter(bool checkPrivilege = true)
		{
			var filter = new ZQuery(GenApprovalRequestSchema.PK, SQLComparisonOperator.NotEqual, PK);
			filter.AddToFilter(GenApprovalRequestSchema.XP_ParentID, XP_ParentID);
			filter.AddToFilter(GenApprovalRequestSchema.XP_ParentTableCode, XP_ParentTableCode);
			filter.AddToFilter(GenApprovalRequestSchema.XP_GB_RequestingBranch, GlbCompany.CurrentCompany.Branches.GetPKs());
			filter.AddToFilter(GenApprovalRequestSchema.XP_SubSystem, Core.Constants.GenApprovalRequestSubSystem.Accounting);
			filter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalType, Core.Constants.GenApprovalRequestApprovalType.ARCreditControlledDocuments);
			if (checkPrivilege)
			{
				filter.AddToFilter(GenApprovalRequestSchema.XP_PrivledgeRequired, XP_PrivledgeRequired.Split(',').Select(x => x.Trim()).ToArray());
			}

			return filter;
		}

		#endregion

		#region ParentBusinessObject

		public BusinessObject ParentBusinessObject
		{
			get
			{
				if (parentBizObj == null)
				{
					parentBizObj = Factory.Load(XP_ParentTableCode, XP_ParentID);
				}
				return parentBizObj;
			}
		}

		#endregion

		#region Documents

		public CreditControlledDocumentsApprovalDocumentCollection Documents
		{
			get
			{
				if (documents == null)
				{
					documents = new CreditControlledDocumentsApprovalDocumentCollection(Factory);

					if (ApprovalData.MenuItemPK != ZGuid.Empty)
					{
						var menuItem = Factory.Load<StmMenuItem>(ApprovalData.MenuItemPK);
						if (menuItem != null)
						{
							documents.Add(new CreditControlledDocumentsApprovalDocument(string.Format(CultureInfo.CurrentCulture, "{0}/{1}", menuItem.SU_MenuPathMultilingual, menuItem.SU_MenuNameMultilingual)));
						}
					}
				}
				return documents;
			}
		}
		CreditControlledDocumentsApprovalDocumentCollection documents;

		#endregion

		public ZString RejectionReason
		{
			get { return ApprovalData.RejectionReason; }
			set { ApprovalData.RejectionReason = value; }
		}

		#region Organisations In Breach

		public OrganisationInBreachCollection OrganisationsInBreach
		{
			get
			{
				if (organisationsInBreach == null)
				{
					organisationsInBreach = new OrganisationInBreachCollection();
					organisationsInBreach.PopulateOrganisations(ParentBusinessObject);
				}
				return organisationsInBreach;
			}
		}
		OrganisationInBreachCollection organisationsInBreach;

		#endregion

		#region Organisations for Credit Check

		OrgHeader[] OrganisationsForCreditChecks
		{
			get
			{
				if (organisationsForCreditChecks == null)
				{
					if (ParentBusinessObject is ICreditControlledDocumentDelivery documentDelivery)
					{
						organisationsForCreditChecks = documentDelivery.OrganisationsForCreditChecks.Where(x => x != null).Distinct();
					}
				}

				return organisationsForCreditChecks.ToArray();
			}
		}

		IEnumerable<OrgHeader> organisationsForCreditChecks;
		#endregion

		#region ControllerID

		public ControllerID ControllerID
		{
			get => GetControllerIDFromControllerProvider() ?? GetControllerIDFromInvoicingPlugin() ?? GetControllerIDFromViewGenericJob();
		}

		ControllerID GetControllerIDFromInvoicingPlugin()
		{
			ControllerID controllerID = null;
			var invoicingPlugin = ParentBusinessObject as IJobInvoicingPlugIn;
			if (invoicingPlugin != null)
			{
				controllerID = invoicingPlugin.InvoicingSupporter.ConsumerType.ControllerID;
			}
			return controllerID;
		}

		ControllerID GetControllerIDFromControllerProvider()
		{
			ControllerID controllerID = null;
			var controllerIDProvider = ParentBusinessObject as IControllerIDProvider;
			if (controllerIDProvider != null)
			{
				controllerID = controllerIDProvider.ControllerID;
			}
			return controllerID;
		}

		ControllerID GetControllerIDFromViewGenericJob()
		{
			ControllerID controllerID = null;
			var genericJob = Factory.LoadGenericJob(XP_ParentID, XP_ParentTableCode);
			if (genericJob != null)
			{
				controllerID = genericJob.GetConsumerController();
			}
			return controllerID;
		}

		#endregion

		#region Properties

		[ReadOnly(true)]
		public override ZString XP_PrivledgeRequired
		{
			get { return base.XP_PrivledgeRequired; }
			set { base.XP_PrivledgeRequired = value; }
		}

		[ReadOnly(true)]
		public override ZGuid XP_GB_RequestingBranch
		{
			get { return base.XP_GB_RequestingBranch; }
			set { base.XP_GB_RequestingBranch = value; }
		}

		[ReadOnly(true)]
		public override ZGuid XP_ParentID
		{
			get { return base.XP_ParentID; }
			set { base.XP_ParentID = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_ParentTableCode
		{
			get { return base.XP_ParentTableCode; }
			set { base.XP_ParentTableCode = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_ApprovalType
		{
			get { return base.XP_ApprovalType; }
			set { base.XP_ApprovalType = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_SubSystem
		{
			get { return base.XP_SubSystem; }
			set { base.XP_SubSystem = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime XP_ApprovalDate
		{
			get { return base.XP_ApprovalDate; }
			set { base.XP_ApprovalDate = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_ApprovalStatus
		{
			get { return base.XP_ApprovalStatus; }
			set { base.XP_ApprovalStatus = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser1
		{
			get { return base.XP_GS_NKApprovingUser1; }
			set { base.XP_GS_NKApprovingUser1 = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser2
		{
			get { return base.XP_GS_NKApprovingUser2; }
			set { base.XP_GS_NKApprovingUser2 = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser3
		{
			get { return base.XP_GS_NKApprovingUser3; }
			set { base.XP_GS_NKApprovingUser3 = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime XP_SystemCreateTimeUtc
		{
			get { return base.XP_SystemCreateTimeUtc; }
			set { base.XP_SystemCreateTimeUtc = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_SystemCreateUser
		{
			get { return base.XP_SystemCreateUser; }
			set { base.XP_SystemCreateUser = value; }
		}

		[ReadOnly(true)]
		[ResourceStringData("IsApprovedForAllDocuments", Caption = "Is Approved For All Documents")]
		public ZBool IsApprovedForAllDocuments
		{
			get
			{
				return XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Approved && ApprovalData.ApproveAllDocuments;
			}
		}

		#endregion

		public void SetStatus(ZString status, bool approveAllDocuments)
		{
			XP_ApprovalStatus = status;
			ApprovalData.ApproveAllDocuments = approveAllDocuments;
		}

		#region Implementation

		ZString GetEventReference()
		{
			ZString result = ZString.Empty;
			if (!IsInDatabase && XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested)
			{
				result = AddTypeToEventReference(Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			}
			else if (IsInDatabase && XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Cancelled && (ZString)XP_ApprovalStatusInfo.OriginalValue != Core.Constants.GenApprovalRequestApprovalStatus.Cancelled)
			{
				result = AddTypeToEventReference(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);
			}

			if (!result.IsEmpty)
			{
				result += AppendReferenceToEventReference(XP_RequestID);
				if (!XP_ReasonDescription.IsEmpty)
				{
					result += AppendReasonToEventReference(XP_ReasonDescription);
				}
			}

			return result;
		}

		public static ZString AddTypeToEventReference(string eventType)
		{
			return FormattableString.Invariant($"{EventReferenceParameters.Codes.Type}={eventType}");
		}

		public static ZString AppendReferenceToEventReference(string referenceNumber)
		{
			return FormattableString.Invariant($"{StmALog.ReferenceDelimiter}{EventReferenceParameters.Codes.ReferenceNumber}={referenceNumber}");
		}

		public static ZString AppendReasonToEventReference(string reason)
		{
			return FormattableString.Invariant($"{StmALog.ReferenceDelimiter}{EventReferenceParameters.Codes.Reason}={reason}");
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			WriteApprovalData();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase)
			{
				XP_RequestID = CreditControlApprovalNumberFountain.Generate(this);
			}

			if (IsApprovalRequestRequestedOrCancelled && AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.Value == AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code)
			{
				Logs.AddNew(Events.CreditApprovalRequested, GetEventReference());
				Job?.Logs.AddNew(Events.CreditApprovalRequested, GetEventReference());
			}

			shouldEmailBeSent =
				(!IsInDatabase && XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested) ||
				(
					(ZString)XP_ApprovalStatusInfo.OriginalValue == Core.Constants.GenApprovalRequestApprovalStatus.Requested &&
					(XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Approved ||
					XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Rejected)
				);
		}

		#region IAccountingNumberFountainDataSource members

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		ZDateTime IAccountingNumberFountainDataSource.PostDate => ZDateTime.Today;

		GlbBranch IAccountingNumberFountainDataSource.Branch => GlbBranch.CurrentBranch;

		GlbDepartment IAccountingNumberFountainDataSource.Department => GlbDepartment.CurrentDepartment;

		#endregion

		bool IsApprovalRequestRequestedOrCancelled =>
			(!IsInDatabase && XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested)
				|| (XP_ApprovalStatusInfo.HasChanges && XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && shouldEmailBeSent)
			{
				shouldEmailBeSent = false;
				new CreditControlledDocumentsApprovalEmailDef(this).Send();
			}
		}

		void ReadApprovalData()
		{
			if (XP_ApprovalRequestData != ZBlob.Empty)
			{
				var serializer = ZXmlSerializer.New(typeof(CreditControlledDocumentsApprovalData));
				using (var stream = new MemoryStream(XP_ApprovalRequestData))
				using (var reader = new XmlTextReader(stream))
				{
					try
					{
						approvalData = (CreditControlledDocumentsApprovalData)serializer.Deserialize(reader);
					}
					catch (InvalidOperationException ex)
					{
						ErrorReporter.ReportOnce("CreditControlledDocumentsApproval.ReadApprovalData.serializer.Deserialize",
							FormattableString.Invariant($"Exception={ex.ToString()}\r\nXP_ApprovalRequestData={Convert.ToBase64String(XP_ApprovalRequestData)}"));
					}
				}
			}
			if (approvalData == null)
			{
				approvalData = new CreditControlledDocumentsApprovalData { ApproveAllDocuments = false, MenuItemPK = ZGuid.Empty };
			}
		}

		void WriteApprovalData()
		{
			var serializer = ZXmlSerializer.New(typeof(CreditControlledDocumentsApprovalData));
			using (var stream = new MemoryStream())
			using (var writer = new XmlTextWriter(stream, Encoding.Unicode))
			{
				serializer.Serialize(writer, ApprovalData);
				writer.Flush();
				XP_ApprovalRequestData = stream.ToArray();
			}
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			XP_ParentID = ZGuid.NewZGuid();
		}
#endif
	}
}
