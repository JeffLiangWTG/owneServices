using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSJobDeclaration : EU.EMCS.Business.EMCSJobDeclaration
		, Integration.Customs.IEEMCS.IEMCSJobDeclaration
		, ICusContainerTypeSupporter
		, IMessageAttachee
	{
		public EMCSJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(CusEntryNumber.Schema.CE_EntryLineReferenceMaxLength)]
		public ZString SequenceNumber
		{
			get => LoadCusEntryNumber(false)?.CE_EntryLineReference ?? ZString.Empty;
			set
			{
				var oldValue = SequenceNumber;
				CheckMaximumLength(SequenceNumberInfo, value);
				LoadCusEntryNumber(true).CE_EntryLineReference = value;
				SequenceNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SequenceNumberInfo => GetZPropertyInfo(nameof(SequenceNumber));

		[ResourceStringData("IE.EMCSJobDeclaration.JE_CustomsProfile", Caption = "Certificate Identifier")]
		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.CertificateIdentifierList))]
		[ReadOnlyMember(nameof(CertificateIDReadOnly))]
		public override ZString JE_CustomsProfile { get => base.JE_CustomsProfile; set => base.JE_CustomsProfile = value; }

		/// <summary>
		/// Certificate Identifier field should not be editable if any messages exist against the declaration.
		/// If the field is empty however, we will allow entry to cater for existing declarations that have messages but no value in this field yet and to allow users data entry ability to fix resulting stop error.
		/// </summary>
		protected bool CertificateIDReadOnly => !JE_CustomsProfile.IsEmpty && Messages.Count > 0;

		public EMCSGlbCompanyCredential CertificateIdentifier => EMCSGlbExternalPasswordCollection.Cast<EMCSGlbCompanyCredential>().FirstOrDefault(x => x.GP_MailBoxID == JE_CustomsProfile);

		public EMCSGlbCompanyCredentialCollection EMCSGlbExternalPasswordCollection
		{
			get
			{
				if (emcsGlbCompanyCredentialCollection == null)
				{
					emcsGlbCompanyCredentialCollection = new EMCSGlbCompanyCredentialCollection(Company);
					emcsGlbCompanyCredentialCollection.Load();
				}

				return emcsGlbCompanyCredentialCollection;
			}
		}
		EMCSGlbCompanyCredentialCollection emcsGlbCompanyCredentialCollection;

		public new EMCSJobDeclarationLookups Lookups => (EMCSJobDeclarationLookups)base.Lookups;

		protected override JobDeclarationLookups GetNewLookups() => new EMCSJobDeclarationLookups(this);

		public new EMCSJobDeclarationValidation Validation => (EMCSJobDeclarationValidation)base.Validation;

		protected override JobDeclarationValidation GetNewValidation() => new EMCSJobDeclarationValidation(this);

		public new EU.EMCS.Business.OfficeCodeCollection<OfficeCode> CustomsOffices => (EU.EMCS.Business.OfficeCodeCollection<OfficeCode>)base.CustomsOffices;

		protected override EU.EMCS.Business.OfficeCodeCollection GetCustomsOffices() => new EU.EMCS.Business.OfficeCodeCollection<OfficeCode>(this);

		protected override Type OfficeCodeType => typeof(OfficeCode);

		public new EMCSAddInfoJobDeclaration AddInfo => (EMCSAddInfoJobDeclaration)base.AddInfo;

		public override EU.EMCS.Business.EMCSAddInfoJobDeclaration GetNewAddInfo() => new EMCSAddInfoJobDeclaration(this);

		protected override EU.EMCS.Business.EMCSPackageCollection CreateNewEMCSPackagesCollection() => new EMCSPackageCollection(this);

		protected override EU.EMCS.Business.IEMCSDocumentCollection<EU.EMCS.Business.EMCSDocument> CreateNewEMCSDocumentCollection() => new EU.EMCS.Business.EMCSDocumentCollection<EMCSDocument>(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.Certificate] = typeof(EMCSDocument);
			return result;
		}

		protected override EU.EMCS.Business.EMCSJobDeclarationMessageSendingConfiguration GetNewMessageSendingConfiguration() => new EMCSJobDeclarationMessageSendingConfiguration();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (EMCSGlbExternalPasswordCollection.Count == 1)
			{
				JE_CustomsProfile = EMCSGlbExternalPasswordCollection[0].GP_MailBoxID.Left(Schema.JE_CustomsProfileMaxLength);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && onSavedActions != null)
			{
				for (var i = 0; i < onSavedActions.Count; i++)
				{
					onSavedActions[i](this);
				}
			}
		}

		public void OnSuccessfulSaveDo(Action<EMCSJobDeclaration> action)
		{
			if (onSavedActions == null)
			{
				onSavedActions = new List<Action<EMCSJobDeclaration>>();
			}
			onSavedActions.Add(action);
		}

		List<Action<EMCSJobDeclaration>> onSavedActions;

		#region IMessageAttachee Members
		GlbBranch IMessageAttachee.Branch => Branch;
		GlbStaff IMessageAttachee.CustomsAgent => CusAgent;
		IRelatedJob IMessageAttachee.RelatedJob => this;
		ZString IMessageAttachee.LogicalStatus { get => JE_MessageStatus; set => JE_MessageStatus = value; }
		ZString IMessageAttachee.EntryStatus { get => JE_EntryStatus; set => JE_EntryStatus = value; }
		ZString IMessageAttachee.MovementReferenceNumber => EADNumber;
		IEnumerable<Enterprise.Messaging.Business.EDIMessage> IMessageAttachee.Messages => Messages.Cast<EDIMessage>();
		#endregion

		protected override DocumentSupporter CreateNewDocumentSupporter() => new EMCSJobDeclarationDocumentSupporter(this);
	}
}
