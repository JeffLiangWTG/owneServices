using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.MessageManagers;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS
{
	[UniversalCopyWithExtendedEntities(IgnoreAllElementsExceptSpecificallyMarked = true)]
	[CodeProperty("DisplayReference")]
	[DescriptionProperty("DisplayReference")]
	public class CDSDISQueryMessage : CDSEDIMessage
	{
		public CDSDISQueryMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			MessageNumberStrategy = GetMessageNumberStrategy();
		}

		public ZString ResponseInterpretation => GetResponseInterpretation();

		public GlbExternalPassword_GB GlbExternalPassword
		{
			get
			{
				var glbExternalPassword_GB = Factory.LoadTop1<GlbExternalPassword_GB>(new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.CDS)
																		.AddToFilter(GlbExternalPasswordSchema.GP_GC, GlbCompany.CurrentCompany.PK)
																		.AddToFilter(GlbExternalPasswordSchema.GP_UserID, EM_MessageOwner));
				return glbExternalPassword_GB;
			}
		}

		#region Properties for Universal Copy
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString EM_ApplicationCode { get => base.EM_ApplicationCode; set => base.EM_ApplicationCode = value; }

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString EM_ReceiveTransmit { get => base.EM_ReceiveTransmit; set => base.EM_ReceiveTransmit = value; }

		[ResourceStringData("CDSDISQueryMessage.EM_ApplicationReference", Caption = "Reference")]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString EM_ApplicationReference { get => base.EM_ApplicationReference; set => base.EM_ApplicationReference = value; }

		[List(nameof(Lookups) + "." + nameof(CDSDISQueryMessageLookups.ProfileList))]
		[ResourceStringData("CDSDISQueryMessage.EM_MessageOwner", ShortCaption = "Profile", MediumCaption = "Messaging Profile", Caption = "CDS Messaging Profile")]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString EM_MessageOwner { get => base.EM_MessageOwner; set => base.EM_MessageOwner = value; }

		[List(nameof(Lookups) + "." + nameof(CDSDISQueryMessageLookups.DeclarationCategories))]
		[ResourceStringData("CDSDISQueryMessage.DeclarationCategory", ShortCaption = "Category", Caption = "Declaration Category")]
		public ZString DeclarationCategory
		{
			get => declarationCategory;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationCategoryInfo, ref declarationCategory, value))
				{
					GenerateApplicationReference();
					EM_ApplicationReferenceInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					(Validation as CDSDISQueryMessageValidation).ValidateDeclarationCategory();
				}
				DeclarationCategoryInfo.RefreshBinding();
			}
		}
		protected bool DeclarationCategory_ReadOnly => ReadOnly;
		ZString declarationCategory;
		public ZPropertyInfo DeclarationCategoryInfo => GetZPropertyInfo(nameof(DeclarationCategory));

		[ResourceStringData("CDSDISQueryMessage.DateFrom", Caption = "Date From")]
		public virtual ZDate DateFrom
		{
			get => dateFrom;
			set
			{
				if (SetNonPersistentPropertyValue(DateFromInfo, ref dateFrom, value))
				{
					GenerateApplicationReference();
					EM_ApplicationReferenceInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					(Validation as CDSDISQueryMessageValidation).ValidateDateFrom();
				}
				DateFromInfo.RefreshBinding();
			}
		}
		protected bool DateFrom_ReadOnly => ReadOnly;
		ZDate dateFrom;
		public virtual ZPropertyInfo DateFromInfo => GetZPropertyInfo(nameof(DateFrom));

		[ResourceStringData("CDSDISQueryMessage.DateTo", Caption = "Date To")]
		public virtual ZDate DateTo
		{
			get => dateTo;
			set
			{
				if (SetNonPersistentPropertyValue(DateToInfo, ref dateTo, value))
				{
					GenerateApplicationReference();
					EM_ApplicationReferenceInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					(Validation as CDSDISQueryMessageValidation).ValidateDateTo();
				}
				DateToInfo.RefreshBinding();
			}
		}
		protected bool DateTo_ReadOnly => ReadOnly;
		ZDate dateTo;
		public virtual ZPropertyInfo DateToInfo => GetZPropertyInfo(nameof(DateTo));

		[ResourceStringData("CDSDISQueryMessage.DeclarationStatus", Caption = "Declaration Status")]
		[List(nameof(Lookups) + "." + nameof(CDSDISQueryMessageLookups.DeclarationStatuses))]
		public virtual ZString DeclarationStatus
		{
			get => declarationStatus;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationStatusInfo, ref declarationStatus, value))
				{
					GenerateApplicationReference();
					EM_ApplicationReferenceInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					(Validation as CDSDISQueryMessageValidation).ValidateDeclarationStatus();
				}
				DeclarationStatusInfo.RefreshBinding();
			}
		}
		protected bool DeclarationStatus_ReadOnly => ReadOnly;
		ZString declarationStatus;
		public virtual ZPropertyInfo DeclarationStatusInfo => GetZPropertyInfo(nameof(DeclarationStatus));

		[ResourceStringData("CDSDISQueryMessage.PageNumber", Caption = "Page Number")]
		public virtual ZInt PageNumber
		{
			get => pageNumber;
			set
			{
				if (SetNonPersistentPropertyValue(PageNumberInfo, ref pageNumber, value))
				{
					GenerateApplicationReference();
					EM_ApplicationReferenceInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					(Validation as CDSDISQueryMessageValidation).ValidatePageNumber();
				}
				PageNumberInfo.RefreshBinding();
			}
		}
		protected bool PageNumber_ReadOnly => ReadOnly;
		ZInt pageNumber;
		public virtual ZPropertyInfo PageNumberInfo => GetZPropertyInfo(nameof(PageNumber));
		#endregion

		#region Overrides
		public new CDSDISQueryMessageLookups Lookups => (CDSDISQueryMessageLookups)base.Lookups;
		protected override EDIMessageLookups GetNewLookups() => new CDSDISQueryMessageLookups(this);
		protected override EDIMessageValidation GetNewValidation() => new CDSDISQueryMessageValidation(this);

		public override bool ReadOnly
		{
			get => base.ReadOnly || IsInDatabase;
			set => base.ReadOnly = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_ApplicationCode = ApplicationCodes.GbCDSDISQuery;
			EM_ReceiveTransmit = Direction.Transmit;

			SetDefaultProfile();
			SetDefaultQueryStringContext();
		}

		protected override string GetMessageReferenceNumber()
		{
			if (MessageNumberStrategy == null)
			{
				MessageNumberStrategy = GetMessageNumberStrategy();
			}
			return MessageNumberStrategy.GetMessageReferenceNumber();
		}

		public override void OnSaving()
		{
			if (!IsInDatabase && !EM_MessageOwner.IsEmpty && !EM_ApplicationReference.IsEmpty && EM_MessageText.IsEmpty)
			{
				GenerateMessageText();
				GenerateApplicationReference();
			}

			base.OnSaving();
		}
		#endregion

		#region Implementation
		void SetDefaultProfile()
		{
			var com = Company;
			var profiles = Lookups.ProfileList;
			if (profiles.Count == 1)
			{
				EM_MessageOwner = profiles[0].Code;
			}
		}

		void SetDefaultQueryStringContext()
		{
			DeclarationCategory = Lookups.DeclarationCategories[CDSDISQueryHelper.Constants.DeclarationCategories.ALL].Code;
			DateFrom = ZDate.Today.AddDays(-7);
			DateTo = ZDate.Today;
			DeclarationStatus = Lookups.DeclarationStatuses[CDSDISQueryHelper.Constants.DeclarationStatuses.Uncleared].Code;
			PageNumber = 1;
		}

		string GetResponseInterpretation()
		{
			var response = ZString.Empty;

			if (IsInDatabase)
			{
				if (EM_Status == EDIMessageStatusList.Codes.Acknowledged)
				{
					response = ResponseMessage?.EM_MessageInterpretation ?? responseNotAcknowledgedMessage;
				}
				else if (EM_Status == EDIMessageStatusList.Codes.Sent)
				{
					response = ResponseMessage?.EM_MessageInterpretation ?? responseNotFoundMessage;
				}
				else if (EM_Status == EDIMessageStatusList.Codes.Failed)
				{
					response = ResponseMessage?.EM_MessageInterpretation ?? requestFailedMessage;
				}
				else
				{
					response = requestNotSentMessage;
				}
			}

			return response;
		}

		CDSDeclarationInfoResponseEDIMessage ResponseMessage => responseMessage ?? (responseMessage = Factory.LoadTop1<CDSDeclarationInfoResponseEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK).AddToFilter(EDIMessageSchema.EM_LinkTable, EDIMessage.Schema.TableName)));
		CDSDeclarationInfoResponseEDIMessage responseMessage;
		readonly string requestNotSentMessage = "<html><body style='font-family: arial;'><p>The request has not been sent.</p></body></html>";
		readonly string responseNotFoundMessage = "<html><body style='font-family: arial;'><p>No response message was found for this request.</p></body></html>";
		readonly string responseNotAcknowledgedMessage = "<html><body style='font-family: arial;'><p>Query has been acknowledged, but no response has been received.</p></body></html>";
		readonly string requestFailedMessage = "<html><body style='font-family: arial;'><p>The request was not delivered successfully, please refer to the Logs tab for more information, in particular the detail of any event with code MRJ.</p></body></html>";

		void GenerateMessageText()
		{
			EM_MessageText = new CDSQuerySendingManager(new CDSQueryListSendingObject(this)).GetUniversalEventContent();
		}

		void GenerateApplicationReference()
		{
			EM_ApplicationReference = CDSDISQueryHelper.ComposeQueryString(this).SubstringSafe(0, Schema.EM_ApplicationReferenceMaxLength);
		}

		public UniversalEvent GetUniversalEvent() => new CDSQuerySendingManager(new CDSQueryListSendingObject(this)).GetUniversalEvent();

		public ZString DisplayReference => System.FormattableString.Invariant($"DQ:{EM_MessageOwner} #{EM_MessageNum}");

		IMessageNumberStrategy GetMessageNumberStrategy() => new GbMessageNumberStrategy(Factory, ApplicationCodeList.Codes.GbCDSDISQuery);
		#endregion
	}
}
