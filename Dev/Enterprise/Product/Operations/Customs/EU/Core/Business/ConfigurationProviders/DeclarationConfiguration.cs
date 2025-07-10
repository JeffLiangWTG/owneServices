using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business
{
	public class DeclarationConfiguration
	{
		public static DeclarationConfiguration GetConfiguration(BusinessObjectFactory factory, string countryOrGrouping)
		{
			return factory.GetCachedValue(FormattableString.Invariant($"DeclarationConfiguration_{countryOrGrouping}"), () =>
			{
				object supporter = null;
				var builders = ObjectFactory.Get<Hashtable>("DeclarationConfiguration");
				if (!string.IsNullOrEmpty(countryOrGrouping))
				{
					var objectHandle = (ObjectHandle)builders[countryOrGrouping];
					supporter = objectHandle?.GetObject();
				}
				if (supporter == null)
				{
					var objectHandle = (ObjectHandle)builders[Core.Constants.CountryCodes.EuropeanUnion];
					supporter = objectHandle.GetObject();
				}
				return (DeclarationConfiguration)supporter;
			});
		}

		public ZBool UCCAdditionalInfosSupport(BusinessObject businessObject) => UCCAdditionalInfosSupportCore(businessObject);
		protected virtual ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => false;

		public ZBool MiscAdditionalInfosSupport(BusinessObject businessObject) => MiscAdditionalInfosSupportCore(businessObject);
		protected virtual ZBool MiscAdditionalInfosSupportCore(BusinessObject businessObject) => true;

		public ZBool MiscSupportingDocumentsSupport(BusinessObject businessObject) => MiscSupportingDocumentsSupportCore(businessObject);
		protected virtual ZBool MiscSupportingDocumentsSupportCore(BusinessObject businessObject) => true;

		public ZBool MiscPreviousDocumentsSupport(BusinessObject businessObject) => MiscPreviousDocumentsSupportCore(businessObject);
		protected virtual ZBool MiscPreviousDocumentsSupportCore(BusinessObject businessObject) => true;

		public ZBool MiscGuaranteesSupport(BusinessObject businessObject) => MiscGuaranteesSupportCore(businessObject);
		protected virtual ZBool MiscGuaranteesSupportCore(BusinessObject businessObject) => false;

		public ZBool UseUniversalFeeCalculation(BusinessObject businessObject) => UseUniversalFeeCalculationCore(businessObject);
		protected virtual ZBool UseUniversalFeeCalculationCore(BusinessObject businessObject) => true;

		public ZBool DV1DetailsSupport(BusinessObject businessObject) => DV1DetailsSupportCore(businessObject);
		protected virtual ZBool DV1DetailsSupportCore(BusinessObject businessObject) => false;

		public ZBool LockNumberOfEntryLinesForRegisteredEntry => LockNumberOfEntryLinesForRegisteredEntryCore;
		protected virtual ZBool LockNumberOfEntryLinesForRegisteredEntryCore => false;

		/// <summary>
		/// Indicates that the system uses EUCDM (EU Customs Data Model Version 5)
		/// </summary>
		/// <param name="businessObject"></param>
		/// <returns></returns>
		public ZBool IsUCC5(BusinessObject businessObject) => IsUCC5Core(businessObject);
		protected virtual ZBool IsUCC5Core(BusinessObject businessObject) => false;

		/// <summary>
		/// Indicates that the system uses EUCDM (EU Customs Data Model Version 6)
		/// </summary>
		/// <param name="businessObject"></param>
		/// <returns></returns>
		public ZBool IsUCC6(BusinessObject businessObject) => IsUCC6Core(businessObject);
		protected virtual ZBool IsUCC6Core(BusinessObject businessObject) => false;

		public ZBool IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice => IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore;

		protected virtual ZBool IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore => false;

		public ZBool UseEoriForFiscalReference => UseEoriForFiscalReferenceCore;
		protected virtual ZBool UseEoriForFiscalReferenceCore => false;

		public ZBool UseEucdmSupportingDocumentGoodsShipment => UseEucdmSupportingDocumentGoodsShipmentCore;
		protected virtual ZBool UseEucdmSupportingDocumentGoodsShipmentCore => true;

		public ZBool UseEucdmSupportingDocumentGoodsShipmentAndItem => UseEucdmSupportingDocumentGoodsShipmentAndItemCore;
		protected virtual ZBool UseEucdmSupportingDocumentGoodsShipmentAndItemCore => false;

		public EntryHeaderConfiguration EntryHeaderConfiguration => entryHeaderConfiguration ?? (entryHeaderConfiguration = GetNewEntryHeaderConfiguration());
		EntryHeaderConfiguration entryHeaderConfiguration;

		protected virtual EntryHeaderConfiguration GetNewEntryHeaderConfiguration() => new EntryHeaderConfiguration();

		public EntryLineConfiguration EntryLineConfiguration => entryLineConfiguration ?? (entryLineConfiguration = GetNewEntryLineConfiguration());
		EntryLineConfiguration entryLineConfiguration;

		protected virtual EntryLineConfiguration GetNewEntryLineConfiguration() => new EntryLineConfiguration();

		public InvoiceHeaderConfiguration InvoiceHeaderConfiguration => invoiceHeaderConfiguration ?? (invoiceHeaderConfiguration = GetNewInvoiceHeaderConfiguration());
		InvoiceHeaderConfiguration invoiceHeaderConfiguration;

		protected virtual InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

		public InvoiceLineConfiguration InvoiceLineConfiguration => invoiceLineConfiguration ?? (invoiceLineConfiguration = GetNewInvoiceLineConfiguration());
		InvoiceLineConfiguration invoiceLineConfiguration;

		protected virtual InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

		public InstructionConfiguration InstructionConfiguration => instructionConfiguration ?? (instructionConfiguration = GetNewInstructionConfiguration());
		InstructionConfiguration instructionConfiguration;

		protected virtual InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

		public ZBool IsTransitionPeriodAES30(BusinessObject businessObject) => IsTransitionPeriodAES30Core(businessObject);
		protected virtual ZBool IsTransitionPeriodAES30Core(BusinessObject businessObject)
		{
			return businessObject is JobDeclaration declaration
				&& declaration.IsExport
				&& FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, options: FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN);
		}

		public ZBool UseIDDDocument(BusinessObject businessObject) => UseIDDDocumentCore(businessObject);
		protected virtual ZBool UseIDDDocumentCore(BusinessObject businessObject) => false;

		public IDeclarationValidationDecider GetValidationDecider(BusinessObject businessObject) => GetValidationDeciderCore(businessObject);

		protected virtual IDeclarationValidationDecider GetValidationDeciderCore(BusinessObject businessObject)
			=> businessObject switch {
				JobDeclaration { IsUCC6AndIsImport: true } => new UCC6ImportDeclarationValidationDecider(),
				JobDeclaration { IsUCC6AndIsExport: true } => new UCC6ExportDeclarationValidationDecider(),
				_ => null
			};

		public IPackageValidationDecider GetPackageValidationDecider(BusinessObject businessObject) => GetPackageValidationDeciderCore(businessObject);

		protected virtual IPackageValidationDecider GetPackageValidationDeciderCore(BusinessObject businessObject)
			=> businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsImport
			? UCC6ImportPackageValidationDecider
			: null;

		protected virtual IPackageValidationDecider UCC6ImportPackageValidationDecider => new UCC6ImportPackageValidationDecider();

		public IAdditionalInfoValidationDecider GetAdditionalInfoValidationDecider(JobDeclaration declaration) => GetAdditionalInfoValidationDeciderCore(declaration);
		protected virtual IAdditionalInfoValidationDecider GetAdditionalInfoValidationDeciderCore(JobDeclaration declaration) => (declaration as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? GetUCC6ImportAdditionalInfoValidationDecider : null;
		protected virtual IAdditionalInfoValidationDecider GetUCC6ImportAdditionalInfoValidationDecider => new UCC6ImportAdditionalInfoValidationDecider();

		public bool ShouldCheckLegalByDeclarantType => ShouldCheckLegalByDeclarantTypeCore;

		protected virtual bool ShouldCheckLegalByDeclarantTypeCore => false;

		public ZBool IsPopulateAuthorisationsForOfficeOfPresentationEnabled(JobDeclaration declaration) => IsPopulateAuthorisationsForOfficeOfPresentationEnabledCore(declaration);
		protected virtual ZBool IsPopulateAuthorisationsForOfficeOfPresentationEnabledCore(JobDeclaration declaration) => false;

		public ICustomsOfficeValidationDecider GetCustomsOfficeValidationDecider(BusinessObject businessObject) => GetCustomsOfficeValidationDeciderCore(businessObject);
		protected virtual ICustomsOfficeValidationDecider GetCustomsOfficeValidationDeciderCore(BusinessObject businessObject) =>
			businessObject is JobDeclaration { IsUCC6: true } declaration
				? GetUCC6CustomsOfficeValidationDecider(declaration)
				: null;
		protected virtual ICustomsOfficeValidationDecider GetUCC6CustomsOfficeValidationDecider(JobDeclaration declaration) => new UCC6CustomsOfficeValidationDecider();

		public IInvoiceLinePackageValidationDecider GetInvoiceLinePackageValidationDecider() => GetInvoiceLinePackageValidationDeciderCore();
		protected virtual IInvoiceLinePackageValidationDecider GetInvoiceLinePackageValidationDeciderCore() => new InvoiceLinePackageValidationDecider();

		public IPreviousDocumentValidationDecider GetPreviousDocumentValidationDecider(JobDeclaration declaration) => GetPreviousDocumentValidationDeciderCore(declaration);
		protected virtual IPreviousDocumentValidationDecider GetPreviousDocumentValidationDeciderCore(JobDeclaration declaration) => (declaration as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportPreviousDocumentValidationDecider : null;
		protected virtual IPreviousDocumentValidationDecider UCC6ImportPreviousDocumentValidationDecider => new UCC6ImportPreviousDocumentValidationDecider();
	}
}
