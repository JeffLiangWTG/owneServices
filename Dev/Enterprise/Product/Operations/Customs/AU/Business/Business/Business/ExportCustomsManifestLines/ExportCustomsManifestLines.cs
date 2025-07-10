using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[DependentBusinessObject(typeof(ExportCustomsManifestHeader), "Lines")]
	public class ExportCustomsManifestLines :
		Customs.Business.ExportCustomsManifestLines,
		ICMRMessageRespondee,
		IStatusNeedsRecalculationProvider,
		IJobInvoicingPlugIn,
		IDocAddresses,
		Integration.Customs.AU.IExportCustomsManifestLines
	{
		public ExportCustomsManifestLines(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CTORECStatusCalculator = new CTORECStatusCalculator(this);
			CTOREMStatusCalculator = new CTOREMStatusCalculator(this);
		}

		#region Static Loader

		public static ExportCustomsManifestLines Load(BusinessObjectFactory factory, ZString reference)
		{
			ZQuery query = new ZQuery(ExportCustomsManifestLinesSchema.EL_UserReferenceNum, reference);
			return factory.LoadTop1<ExportCustomsManifestLines>(query);
		}

		#endregion

		#region Linetype Bools

		public bool IsCANLine
		{
			get
			{
				return EL_TypeOfCAN == CANType.CustomsAuthorityNumber.Code;
			}
		}

		public bool IsCCANLine
		{
			get
			{
				return EL_TypeOfCAN == CANType.ContingencyCustomsAuthorityNumber.Code;
			}
		}

		public bool IsExemptLine
		{
			get
			{
				return new CMRExportExemptionCodesList().ContainsCode(EL_TypeOfCAN);
			}
		}

		public bool IsPersonalEffectsOrLowValue
		{
			get
			{
				return IsExemptLine && (EL_TypeOfCAN == CMRExportExemptionCodes.EXLV.Code || EL_TypeOfCAN == CMRExportExemptionCodes.EXPE.Code);
			}
		}

		public bool IsCTO
		{
			get
			{
				return Header != null && Header.ED_ManifestType == AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			}
		}

		#endregion

		#region Related Business Objects

		#region Header

		public ExportCustomsManifestHeader Header
		{
			get
			{
				return Factory.Load<ExportCustomsManifestHeader>(EL_ED);
			}
		}

		#endregion

		#region DocAddresses

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}
				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		#endregion

		#region ConsignorDocumenteryAddress

		public JobDocAddress ConsignorDocumentaryAddress
		{
			get { return CachedGetAddress(ref consignorDocumentaryAddress, ConsignorDocAddressRequirement); }
		}

		JobDocAddressRequirement ConsignorDocAddressRequirement
		{
			get
			{
				if (consignorDocAddressRequirement == null)
				{
					consignorDocAddressRequirement = new JobDocAddressRequirement(
						DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
				}

				return consignorDocAddressRequirement;
			}
		}

		JobDocAddress consignorDocumentaryAddress;
		JobDocAddressRequirement consignorDocAddressRequirement;

		#endregion

		#region ConsigneeDocumenteryAddress

		public JobDocAddress ConsigneeDocumentaryAddress
		{
			get { return CachedGetAddress(ref consigneeDocumentaryAddress, ConsigneeDocAddressRequirement); }
		}

		JobDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get
			{
				if (consigneeDocAddressRequirement == null)
				{
					consigneeDocAddressRequirement = new JobDocAddressRequirement(
						DocAddressType.ConsigneeDocumentaryAddress, ContactType.Consignee);
				}

				return consigneeDocAddressRequirement;
			}
		}

		JobDocAddress consigneeDocumentaryAddress;
		JobDocAddressRequirement consigneeDocAddressRequirement;

		#endregion

		#endregion

		#region Properties

		#region Owner Proxy

		public override ZGuid EL_OH_Owner
		{
			get
			{
				return base.EL_OH_Owner;
			}
			set
			{
				base.EL_OH_Owner = value;

				if (Owner != null)
				{
					var customsClientID = Owner.GetCustomsClientID();
					EL_GoodsOwner = Owner.OH_FullName.Left(EL_GoodsOwnerInfo.MaxLength);
					EL_GoodsOwnerPartyID = !customsClientID.IsEmpty ? customsClientID : Owner.TaxRegistrationNumber.SubstringSafe(2);
				}
			}
		}

		[ReadOnlyMember(nameof(OwnerExists))]
		public override ZString EL_GoodsOwner
		{
			get { return base.EL_GoodsOwner; }
			set { base.EL_GoodsOwner = value; }
		}

		[ReadOnlyMember(nameof(OwnerExists))]
		public override ZString EL_GoodsOwnerPartyID
		{
			get { return base.EL_GoodsOwnerPartyID; }
			set { base.EL_GoodsOwnerPartyID = value; }
		}

		protected bool OwnerExists
		{
			get { return Owner != null; }
		}

		#endregion

		#endregion

		#region Overridden Properties

		protected override Customs.Business.ExportCustomsManifestLinesValidation GetNewValidation()
		{
			return new ExportCustomsManifestLinesValidation(this);
		}

		protected override Customs.Business.ExportCustomsManifestLinesLookups GetNewLookups()
		{
			return new ExportCustomsManifestLinesLookups(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
		}

		public override ZString EL_AirWayBill
		{
			get
			{
				return base.EL_AirWayBill;
			}
			set
			{
				var oldValue = base.EL_AirWayBill;

				if (oldValue != value && string.IsNullOrEmpty(EL_CAN))
				{
					UpdateEL_CANWithTranshipmentNum(value);
				}

				base.EL_AirWayBill = value;
			}
		}

		public override ZShort EL_NumberOfContainers
		{
			get { return base.EL_NumberOfContainers; }
			set
			{
				bool different = base.EL_NumberOfContainers != value;
				base.EL_NumberOfContainers = value;
				if (different && !IsCopying)
				{
					if (Header != null)
					{
						Header.MarkAsNeedingValidation();
					}
					Validation.ValidateEL_NumberOfPackages();
				}
				if (Header != null)
				{
					Header.CalculateTotalContainersFromLines();
				}
			}
		}

		public override ZInt EL_NumberOfPackages
		{
			get { return base.EL_NumberOfPackages; }
			set
			{
				bool different = base.EL_NumberOfPackages != value;
				base.EL_NumberOfPackages = value;
				if (different && !IsCopying)
				{
					if (Header != null)
					{
						Header.MarkAsNeedingValidation();
					}
					Validation.ValidateEL_NumberOfContainers();
				}
				if (Header != null)
				{
					Header.CalculateTotalPackagesFromLines();
				}
			}
		}

		public override ZString EL_TypeOfCAN
		{
			get { return base.EL_TypeOfCAN; }
			set
			{
				bool isDifferent = EL_TypeOfCAN != value;
				base.EL_TypeOfCAN = value;
				if (isDifferent)
				{
					if (IsExemptLine)
					{
						EL_CAN = ZString.Empty;
					}
				}
			}
		}

		[DecimalPlaces(3)]
		[MeasureUnit(Schema.EL_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal EL_Volume
		{
			get { return base.EL_Volume; }
			set { base.EL_Volume = value; }
		}

		[List(nameof(EL_Volume_List))]
		public override ZString EL_VolumeUQ
		{
			get { return base.EL_VolumeUQ; }
			set { base.EL_VolumeUQ = value; }
		}

		[DecimalPlaces(3)]
		[MeasureUnit(Schema.EL_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal EL_Weight
		{
			get { return base.EL_Weight; }
			set { base.EL_Weight = value; }
		}

		[List(nameof(EL_Weight_List))]
		public override ZString EL_WeightUQ
		{
			get { return base.EL_WeightUQ; }
			set { base.EL_WeightUQ = value; }
		}

		#endregion

		#region Statuses

		public CusEntryNumStatus CTORECStatus
		{
			get
			{
				if (fCTORECStatus == null)
				{
					fCTORECStatus = new CusEntryNumStatus(this, new CMRBaseStatuses(), CMRBaseStatuses.Codes.NotSent, CusEntryNumber.EntryType.CTORECStatus, Core.Constants.CountryCodes.Australia);
				}
				return fCTORECStatus;
			}
		}
		CusEntryNumStatus fCTORECStatus;

		public CusEntryNumStatus CTOREMStatus
		{
			get
			{
				if (fCTOREMStatus == null)
				{
					fCTOREMStatus = new CusEntryNumStatus(this, new CMRBaseStatuses(), CMRBaseStatuses.Codes.NotSent, CusEntryNumber.EntryType.CTOREMStatus, Core.Constants.CountryCodes.Australia);
				}
				return fCTOREMStatus;
			}
		}
		CusEntryNumStatus fCTOREMStatus;

		public readonly CTORECStatusCalculator CTORECStatusCalculator;
		public readonly CTOREMStatusCalculator CTOREMStatusCalculator;

		#endregion

		#region BindingLists

		public ConsignorCollection Consignor_List
		{
			get { return new ConsignorCollection(Factory); }
		}

		public ConsigneeCollection Consignee_List
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public CodeDescriptionPairList EL_Weight_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList EL_Volume_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region Implementation

		JobDocAddress CachedGetAddress(ref JobDocAddress field, JobDocAddressRequirement requirement)
		{
			if (field == null || field.IsDeleted)
			{
				UnRegisterListChangedCalledRefreshBinding(field);
				field = DocAddresses.FindOrCreateWithRequirement(requirement);
				RegisterListChangedCalledRefreshBinding(field);
			}
			return field;
		}

		void UpdateEL_CANWithTranshipmentNum(string airWayBillNum)
		{
			if (!string.IsNullOrEmpty(airWayBillNum))
			{
				var query = new ZDBOnlyQuery(typeof(CusHAWB));
				query.AddToFilter(CusHAWBSchema.CS_HAWB, airWayBillNum);

				var subQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				var mawbQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
				mawbQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, CusMAWBBase.Loader.CMRApplicationCodes);
				mawbQuery.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, false);
				subQuery.AddSubQuery(CusHAWBSchema.CS_CM, mawbQuery, JoinCondition.Or);

				var hawbQuery = new ZQuery(CusHAWBSchema.CS_CM, null);
				hawbQuery.AddToFilter(CusHAWBSchema.CS_ApplicationCode, CusMAWBBase.Loader.CMRApplicationCodes);
				subQuery.AddToFilter(hawbQuery, JoinCondition.Or);
				query.AddToFilter(subQuery);

				var hawb = Factory.LoadTop1<CusHAWB>(query);

				if (hawb != null && !string.IsNullOrEmpty(hawb.CS_TranshipmentEntryNum))
				{
					EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
					EL_CAN = hawb.CS_TranshipmentEntryNum.Left(ExportCustomsManifestLines.Schema.EL_CANMaxLength);
				}
			}
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		bool IStatusNeedsRecalculationProvider.StatusNeedsRecalculation
		{
			get
			{
				bool result = false;
				if (!this.IsDeleted)
				{
					result = (messages != null && Messages.HasChanges);
				}
				return result;
			}
		}

		#endregion

		#region ICMRMessageRespondee Members

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					RegisterEditableChildObject(messages);
					messages.Load();
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		ZString ICMRMessageRespondee.Details
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();

				if (!EL_UserReferenceNum.IsEmpty)
				{
					builder.Append("Reference: " + EL_UserReferenceNum);
				}

				if (!EL_CAN.IsEmpty)
				{
					builder.Append("CAN: " + EL_CAN);
				}

				if (Header.IsAir && !EL_AirWayBill.IsEmpty)
				{
					builder.Append("AirWay Bill: " + EL_AirWayBill);
				}

				if (!EL_RN_NKCountryOfDestination.IsEmpty)
				{
					builder.Append("Destination Country/Region: " + EL_RN_NKCountryOfDestination);
				}

				if (Header.IsSea && !EL_NumberOfContainers.IsEmpty)
				{
					builder.Append("Number of Containers: " + EL_NumberOfContainers);
				}

				if (!EL_NumberOfPackages.IsEmpty)
				{
					builder.Append("Number of Packages: " + EL_NumberOfPackages);
				}

				if (!EL_GoodsDescription.IsEmpty)
				{
					builder.Append("Goods Description: " + EL_GoodsDescription);
				}

				if (!EL_GoodsOwner.IsEmpty)
				{
					builder.Append("Goods Owner: " + EL_GoodsOwner);
				}

				if (!EL_GoodsOwnerPartyID.IsEmpty)
				{
					builder.Append("Goods Owner Party ID: " + EL_GoodsOwnerPartyID);
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		ZString ICMRMessageRespondee.ShortDescription
		{
			get { return EL_CAN.IsEmpty ? ZString.Empty : (ZString)("CAN: " + EL_CAN); }
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return EL_UserReferenceNum; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateEL_UserRefNumberIfNeeded();
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		ExportCustomsManifestLinesInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new ExportCustomsManifestLinesInvoicingSupporter(this)); }
		}

		#endregion

		#region IDocAddresses Members

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.ConsignorDocumentaryAddress,
					DocAddressType.ConsigneeDocumentaryAddress
				};
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ConsignorDocumentaryAddress:
					return ConsignorDocAddressRequirement;
				case DocAddressType.ConsigneeDocumentaryAddress:
					return ConsigneeDocAddressRequirement;
				default:
					return null;
			}
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion
	}
}
