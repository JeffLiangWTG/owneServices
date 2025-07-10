using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AsycudaManifestHeader : EU.H7.Business.AsycudaManifestHeader,
		Integration.Customs.IEH7.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string DefaultApplicationCode => SubmitTypeList.Codes.V1;

		public bool IsLV1 => AMA_ApplicationCode == SubmitTypeList.Codes.V1;

		public bool IsLV2 => AMA_ApplicationCode == SubmitTypeList.Codes.V2;

		protected override ZString GetDataGroupingCore()
		{
			if (AMA_ApplicationCode == ApplicationCodeTypeList.Codes.EuH7V1)
			{
				return Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUCC6V1;
			}
			else
			{
				return base.GetDataGroupingCore();
			}
		}

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		public new ValidationConfiguration ValidationConfiguration => (ValidationConfiguration)base.ValidationConfiguration;

		protected override EU.H7.Business.ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		protected override ZString GetDefaultCountryCode() => CountryCodes.Ireland;

		#region Properties

		[ResourceStringData("0a25803d-c442-4c26-9d0c-023dadd2bb43", Caption = "Rep. Status")]
		public override ZString AMA_AgentType
		{
			get { return base.AMA_AgentType; }
			set { base.AMA_AgentType = value; }
		}

		[ResourceStringData("85943115-21f0-4b36-a6bb-87d577a558e7", Caption = "Payment Method")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MethodOfPaymentList))]
		public override ZString AMA_PaymentMethod
		{
			get => base.AMA_PaymentMethod;
			set => base.AMA_PaymentMethod = value;
		}

		[ResourceStringData("2d2f40ad-d45d-4d55-bd54-764f1079c9f7", Caption = "Account Number")]
		public override ZString AMA_PaymentAccountNumber
		{
			get => base.AMA_PaymentAccountNumber;
			set => base.AMA_PaymentAccountNumber = value;
		}

		[ResourceStringData("c38eabf9-aa5b-4486-9c2e-3465446c6229", Caption = "Submit Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.SubmitTypeList))]
		[BusinessObjectTestExclude]
		public override ZString AMA_ApplicationCode
		{
			get => base.AMA_ApplicationCode;
			set
			{
				if (Lookups.SubmitTypeList.GetAllCodes().Contains(value.ToString()))
				{
					base.AMA_ApplicationCode = value;
				}

				foreach (var bill in Bills)
				{
					bill.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		public new EU.H7.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (EU.H7.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

		protected override EU.H7.Business.IAsycudaBillCollection<EU.H7.Business.AsycudaBill, EU.H7.Business.AsycudaManifestHeader> CreateNewEUH7AsycudaBillCollection() => new EU.H7.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		public new EU.H7.Business.ISupportingDocumentCollection<SupportingDocument> SupportingDocuments => (EU.H7.Business.ISupportingDocumentCollection<SupportingDocument>)base.SupportingDocuments;

		protected override EU.H7.Business.ISupportingDocumentCollection<EU.H7.Business.SupportingDocument> CreateNewSupportingDocumentCollection() => new EU.H7.Business.SupportingDocumentCollection<SupportingDocument>(this);

		public new EU.H7.Business.IPreviousDocumentCollection<PreviousDocument> PreviousDocuments => (EU.H7.Business.IPreviousDocumentCollection<PreviousDocument>)base.PreviousDocuments;

		protected override EU.H7.Business.IPreviousDocumentCollection<EU.H7.Business.PreviousDocument> CreateNewPreviousDocumentCollection() => new EU.H7.Business.PreviousDocumentCollection<PreviousDocument>(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		public new H7ApplicationBusinessProvider ApplicationBusinessProvider => (H7ApplicationBusinessProvider)base.ApplicationBusinessProvider;

		public CurrencyConverter CurrencyConverter
		{
			get
			{
				if (currencyConverter == null)
				{
					currencyConverter = CurrencyConverter.New(Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, true);
				}
				return currencyConverter;
			}
		}

		CurrencyConverter currencyConverter;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ApplicationCode = "LV1";
			AMA_JobReference = "IE123456";
		}
#endif
	}
}
