using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSJobDeclaration : EU.EMCS.Business.EMCSJobDeclaration
		, Integration.Customs.DEEMCS.IEMCSJobDeclaration
		, ICusContainerTypeSupporter
	{
		public EMCSJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString ZG_DeferredSubmission
		{
			get => base.ZG_DeferredSubmission;
			set
			{
				base.ZG_DeferredSubmission = value;
				if (value == EmcsDeferredSubmissionList.Codes.JaZusammengefasstesEVd)
				{
					SetConsolidatedDocumentDefaultValues();
				}
			}
		}

		[ReadOnlyMember(nameof(IsConsolidatedDocument))]
		public override ZInt JourneyTimeNumericPart
		{
			get => base.JourneyTimeNumericPart;
			set => base.JourneyTimeNumericPart = value;
		}

		[ReadOnlyMember(nameof(IsConsolidatedDocument))]
		public override ZString JourneyTimeFormatPart
		{
			get => base.JourneyTimeFormatPart;
			set => base.JourneyTimeFormatPart = value;
		}

		[MaxLength(nameof(ZG_GuarantorTypeMaxLength))]
		public override ZString ZG_GuarantorType
		{
			get => base.ZG_GuarantorType;
			set => base.ZG_GuarantorType = value;
		}

		protected int ZG_GuarantorTypeMaxLength => IsConsignor ? 1 : 4;

		public new EMCSOfficeCodeCollection CustomsOffices => (EMCSOfficeCodeCollection)base.CustomsOffices;

		protected override OfficeCodeCollection GetCustomsOffices() => new EMCSOfficeCodeCollection(this);

		protected override Type OfficeCodeType => typeof(EMCSOfficeCode);

		public new EMCSAddInfoJobDeclarationLookups AddInfoLookups => (EMCSAddInfoJobDeclarationLookups)base.AddInfoLookups;

		public new EMCSAddInfoJobDeclaration AddInfo => (EMCSAddInfoJobDeclaration)base.AddInfo;

		public override EU.EMCS.Business.EMCSAddInfoJobDeclaration GetNewAddInfo() => new EMCSAddInfoJobDeclaration(this);

		public new EMCSJobDeclarationValidation Validation => (EMCSJobDeclarationValidation)base.Validation;

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

		protected override JobDeclarationValidation GetNewValidation() => new EMCSJobDeclarationValidation(this);

		public new EMCSJobDeclarationLookups Lookups => (EMCSJobDeclarationLookups)base.Lookups;

		protected override JobDeclarationLookups GetNewLookups() => new EMCSJobDeclarationLookups(this);

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new EMCSJobDeclarationJobDocAddressValidation(addressToValidate, this);

		public new ImportSADNumberCollection ImportSADNumbers => (ImportSADNumberCollection)base.ImportSADNumbers;

		protected override EU.EMCS.Business.ImportSADNumberCollection CreateNewImportSADNumberCollection() => new ImportSADNumberCollection(this);

		protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.ImporterDocumentaryAddressChanged(sender, e);
			MarkAsNeedingValidation();
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.ImportSad] = typeof(ImportSADNumber);
			return result;
		}

		Type ICusContainerTypeSupporter.GetCusContainerType() => typeof(EMCSCusContainer);

		[ChildEditable(true)]
		public new EMCSCusContainerCollection CusContainers => (EMCSCusContainerCollection)base.CusContainers;

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new EMCSCusContainerCollection(this);

		protected override EU.EMCS.Business.EMCSPackageCollection CreateNewEMCSPackagesCollection() => new EMCSPackageCollection(this);

		protected override CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new EMCSCustomsOfficeRequirementHelper(this);

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

		void SetConsolidatedDocumentDefaultValues()
		{
			UpdateJourneyTime(45, JourneyTimeUnitList.Codes.Days);
			ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignor;
		}

		ZBool IsConsolidatedDocument => this.IsConsolidatedDocument();
	}
}
