using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.Business.LandedCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public partial class JobDeclaration : ILookupsResetter, IConsolMessagingProvider, IMessageAttachee
	{
		protected override Customs.Business.MergeManager GetMergeManager() => ApplicationExtender.GetMergeManager(this);

		#region JobDeclaration
		protected override IValueSetStrategy GetValueSetStrategy()
		{
			Type type = ApplicationExtender.GetValueStrategy(this).GetType();
			return (IValueSetStrategy)Activator.CreateInstance(type, this);
		}

		protected override void DecorateDocAddressRequirement(JobDocAddressRequirement requirement, DocAddressType addressType)
		{
			base.DecorateDocAddressRequirement(requirement, addressType);

			switch (addressType)
			{
				case DocAddressType.SupplierDocumentaryAddress:
					requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
						+=
						new JobDocAddressRequirement.ValidationDelegate(Validation.ValidateSupplierDocumentaryAddress);  // gives us a Gems validation (which is a Chief validation (which is a GB.Biz validation (which is an EU.Biz validation)))
					break;

				case DocAddressType.ImporterDocumentaryAddress:
					requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
						+=
						new JobDocAddressRequirement.ValidationDelegate(Validation.ValidateImporterDocumentaryAddress); // gives us a Gems validation (which is a Chief validation (which is a GB.Biz validation (which is an EU.Biz validation)))
					break;

				case DocAddressType.CustomsWarehouseAddress:
					requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
						+=
						delegate
						{
							Validation.ValidateWarehouseDocAddressForeignKey();  // goes straight to a GB.Biz validation (which is an EU.biz validation)
						};
					break;

				case DocAddressType.CustomsSupervisingOffice:
					requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
						+=
						delegate  // Make sure to delegate the getting of the ApplicationExtender until the validation is invoked, and not at the point of initialising the DocASddress, otherwise we'll wire up one type of validation based on thie initial application code and not change it when the application code changes. 
						{
							//Validation.ValidateSupervisingOfficeDocAddress();  // goes straight to a GB.Biz validation (which is an EU.biz validation)
							ApplicationExtender?.GetNewJobDeclarationValidation(this).ValidateSupervisingOfficeDocAddress();  // goes straight to a GB.Biz validation (which is an EU.biz validation)
						};
					break;
			}
		}

		public void ResetApplicationExtender()
		{
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.ResetCustomsUnitDefaultingStrategy());
			applicationExtender = null;
		}

		public override ZString JE_ApplicationCode
		{
			get => base.JE_ApplicationCode;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_ApplicationCode))
				{
					var hasChanged = base.JE_ApplicationCode != value;
					if (hasChanged)
					{
						MarkAsNeedingValidationAndResetLookupIncludingChildren();
						applicationExtender = null;
					}
					base.JE_ApplicationCode = value;
					if (hasChanged)
					{
						InvoiceLines.Cast<JobComInvoiceLine>().ForEach(invoiceLine =>
						{
							invoiceLine.ResetCustomsUnitDefaultingStrategy();
							invoiceLine.ExecuteCustomsUnitDefaultingStrategy();
						});
						if (OnApplicationCodeChanged != null)
						{
							OnApplicationCodeChanged.Invoke(this, null);
						}
						NeedToGetNewIncoTermAndChargeFactory = true;

						foreach (var invoice in Invoices)
						{
							invoice.NeedToGetNewIncoTermAndChargeFactory = true;
						}

						var newMop = ApplicationExtender.GetJobComInvoiceLineDefaultMethodOfPayment(this);
						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							if (invoiceLine.ZG_MethodOfPayment.IsEmpty)
							{
								invoiceLine.ZG_MethodOfPayment = newMop;
							}
						}
					}
				}
			}
		}

		public event EventHandler<EventArgs> OnApplicationCodeChanged;

		void MarkAsNeedingValidationAndResetLookupIncludingChildren()
		{
			LoadChildEditableObjects();
			MarkAsNeedingValidationAndResetLookupIncludingChildren(this);
		}
		void MarkAsNeedingValidationAndResetLookupIncludingChildren(BusinessObject bizObj)
		{
			if (bizObj != null)
			{
				(bizObj as ILookupsResetter)?.Reset();
				bizObj.MarkAsNeedingValidation();
				foreach (var child in ((IBusiness)bizObj).Children)
				{
					if (child is BusinessObject childBizObj)
					{
						MarkAsNeedingValidationAndResetLookupIncludingChildren(childBizObj);
					}
					else if (child is IBusinessObjectCollection collection && collection != null)
					{
						foreach (BusinessObject collectionBizObj in collection)
						{
							MarkAsNeedingValidationAndResetLookupIncludingChildren(collectionBizObj);
						}
					}
				}
			}
		}

		ApplicationExtender applicationExtender;
		public ApplicationExtender ApplicationExtender => applicationExtender ?? (applicationExtender = ApplicationExtender.New(JE_ApplicationCode));

		public UCCHelper UCCHelper => ApplicationExtender?.UCCHelper;

		#region Lookups

		void ILookupsResetter.Reset()
		{
			fLookups = null;
		}

		protected override bool IsLookupsCachedInBase => false; // We need a mechanism to ensure all classes using the ApplicationExtender for Lookups have IsLookupsCachedInBase = false. Also, we probably want to cache locally in the ApplicationExtender for performance, and again have a mechanism to ensure we do it.

		Customs.Business.JobDeclarationLookups fLookups;
		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			return fLookups ??= new JobDeclarationLookups(this);
		}

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		#endregion

		#region Validation

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			return ApplicationExtender?.GetNewJobDeclarationValidation(this);
		}

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		#endregion

		protected override DocumentEngineCore.DocumentSupport.DocumentSupporter CreateNewDocumentSupporter()
			=> new JobDeclarationDocumentSupporter(this);

		protected override bool SupportIATALoadPortDefaulting => false;
		#endregion

		#region AddInfoJobDeclaration
		protected override IValueSetStrategy GetAddInfoJobComInvoiceLineValueSetStrategyCore(Eu.AddInfoJobComInvoiceLine addInfoJobComInvoiceLine)
			=> applicationExtender?.GetAddInfoJobComInvoiceLineValueSetStrategy(addInfoJobComInvoiceLine) ?? new AddInfoJobComInvoiceLineValueSetStrategy(addInfoJobComInvoiceLine);

		protected override IValueSetStrategy GetSupportingDocumentValueSetStrategyCore(Eu.MultiLineAddInfos.SupportingDocument supportingDocument)
			=> new SupportingDocumentValueSetStrategy((SupportingDocument)supportingDocument);
		#endregion

		#region JobComInvoiceLine

		protected override IValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(Eu.JobComInvoiceLine invoiceLine)
			=> ApplicationExtender?.GetJobComInvoiceLineValueSetStrategy(invoiceLine as JobComInvoiceLine);

		protected override Eu.JobComInvoiceLineLookups GetJobComInvoiceLineLookupsCore(Eu.JobComInvoiceLine invoiceLine)
			=> ApplicationExtender?.GetJobComInvoiceLineLookups((JobComInvoiceLine)invoiceLine);

		protected override Eu.JobComInvoiceLineValidation GetJobComInvoiceLineValidationCore(Eu.JobComInvoiceLine invoiceLine)
			=> ApplicationExtender?.GetJobComInvoiceLineValidation((JobComInvoiceLine)invoiceLine);

		protected override Eu.JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(Eu.JobComInvoiceHeader invoiceHeader)
			=> ApplicationExtender.GetJobComInvoiceHeaderValidation(invoiceHeader);

		protected override EUAddInfoValidation GetAddInfoJobComInvoiceLineValidationCore(Eu.AddInfoJobComInvoiceLine addInfo)
			=> ApplicationExtender?.GetAddInfoInvoiceLineValidation(addInfo);
		#endregion

		#region CusEntryHeader
		protected override Eu.CusEntryHeaderValidation GetCusEntryHeaderValidationCore(Eu.CusEntryHeader entryHeader) => ApplicationExtender.GetCusEntryHeaderValidation(entryHeader);

		protected override Eu.CusEntryHeaderDocumentSupporter GetCusEntryHeaderDocumentSupporterCore(Eu.CusEntryHeader entryHeader)
			=> new CusEntryHeaderDocumentSupporter((CusEntryHeader)entryHeader);
		#endregion

		protected override IStatusChecker GetStatusCheckerCore()
		{
			Type type = ObjectFactory.GetType<Integration.Customs.GB.GBChief.IStatusChecker>();
			return (IStatusChecker)Activator.CreateInstance(type);
		}

		protected override Eu.EntryFeePaymentPartyUnderstander GetEntryFeePaymentPartyUnderstanderCore(Eu.CusEntryHeader header) => new EntryFeePaymentPartyUnderstander(this, header as CusEntryHeader);

		protected override MessageChangedStatusDeterminerToDictateWhetherSavingAllowed GetMessageChangedStatusDeterminerForDictatingWhetherSavingAllowed()
			=> new GbMessageChangedStatusDeterminerToDictateWhetherSavingAllowed(this);

		protected override LandedCostingHelper GetLandedCostingHelperCore() => new GbLandedCostingHelper();

		public void CalculateVATAdjustmentForBox68() => new Box68VatValueAdjustmentCalculator().CalculateVATAdjustmentBox68(this);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection()
			=> new Eu.InvoiceLineViewCollection<JobComInvoiceLine>(this);

		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection()
			=> new InvoiceHeaderActiveCollection(this);

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override Eu.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override Eu.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		public override Eu.EntryCreationStrategy CreateEntryCreationStrategy() => ApplicationExtender?.CreateC88CreationStrategy(this) ?? new C88CreationStrategy(this);

		protected override string GetIApportionInvoiceHolderCountryContextCore()
			=> ApplicationExtender?.GetIApportionInvoiceHolderCountryContext(this) ?? base.GetIApportionInvoiceHolderCountryContextCore();

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public ZString CustomsProfile => JE_CustomsProfile;

		public ZString MessageType => JE_MessageType;

		public ZString MasterBill => JE_MasterBill;

		public ZString HouseSplitReference => ZG_HouseSplitReference;

		public ZString HouseBill => JE_HouseBill;

		public ZString LocationOfGoods => JE_LocationOfGoods;

		public ZString JobReference => JE_DeclarationReference;

		public ZBool FindGen51Statement => ZBool.True;

		public bool ShouldDefaultRRS01 => JE_MasterUCR.IsEmpty && IsGoodsNotArrivedSubStyle && (IsIFD || IsEFD || IsESP);

		public InvoiceLineCompleteCollection GetInvoiceLines() => InvoiceLines;

		public ZString SubLocationOfGoods => JE_SubLocationOfGoods;

		#region IMessageAttachee

		string IMessageAttachee.DataGroupingCode => throw new NotImplementedException();

		ZString IMessageAttachee.MovementReferenceNumber => throw new NotImplementedException();

		ZString IMessageAttachee.LocalReferenceNumber => throw new NotImplementedException();

		void IMessageAttachee.UpdateStatusIfNotEmpty(ZString status) { }

		ZString IMessageAttachee.Gateway => ZG_Gateway;

		#endregion

		protected override Eu.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);
	}
}
