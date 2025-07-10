using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.BR.Module.JobDeclarationFilterBusinessObject;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class LicenseFilterStripBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		public override ZQuery Filter => base.Filter.AddToFilter(JobDeclarationSchema.JE_MessageType, BRJobMessageTypeList.Codes.ImportLicense);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = base.GetModuleFiltersCore();

			foreach (var filter in GetFilterToRemove())
			{
				collection.RemoveFilter(collection[filter]);
			}

			collection[Customs.Module.DeclarationFilterConstants.NumberFilterTypes.EntryNumber].MultilingualDescription = LicenseNumberText;
			collection[Customs.Module.DeclarationFilterConstants.EntryStatusText].MultilingualDescription = LicenseStatusText;

			return collection;
		}

		internal static MultilingualString LicenseNumberText = ResString.GetMultilingualString("LicenseFilterStripBusinessObject|EntryNumber", "License #");
		internal static MultilingualString LicenseStatusText = ResString.GetMultilingualString("LicenseFilterStripBusinessObject|EntryStatusText", "License Status");

		IEnumerable<ZString> GetFilterToRemove()
		{
			yield return Customs.GUI.InvoiceLineFilterConstants.ContainerNumber;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.HouseBill;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.MasterBill;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.PaymentNumber;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.PaymentAmount;
			yield return Customs.Module.DeclarationFilterConstants.DateFilterTypes.DateOfExport;
			yield return Customs.Module.DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival;
			yield return Customs.Module.DeclarationFilterConstants.DateFilterTypes.ETAOfDischarge;
			yield return Customs.Module.DeclarationFilterConstants.DateFilterTypes.ETDOfLoading;
			yield return Customs.Module.DeclarationFilterConstants.PortFilterTypes.LoadDischarge;
			yield return Customs.Module.DeclarationFilterConstants.PortFilterTypes.PortOfFirstArrival;
			yield return Customs.Module.DeclarationFilterConstants.PortFilterTypes.OriginDestination;
			yield return Customs.Module.DeclarationFilterConstants.FlightVoyageVessel;
			yield return Customs.Module.DeclarationFilterConstants.ServiceLevel;
			yield return Customs.Module.DeclarationFilterConstants.ServiceType;
			yield return Customs.Module.DeclarationFilterConstants.ShipmentSubType;
			yield return Customs.Module.DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder;
			yield return Customs.Module.DeclarationFilterConstants.OrgFilterTypes.PickupTransportCompany;
			yield return Customs.Module.DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator;
			yield return Customs.Module.DeclarationFilterConstants.OrgFilterTypes.DeliveryTransportCompany;
			yield return Customs.Module.DeclarationFilterConstants.RelatedTransportBookings;
			yield return Customs.Module.DeclarationFilterConstants.RelatedContainers;
			yield return Customs.Module.DeclarationFilterConstants.NumberFilterTypes.ContainerModeCustoms;
			yield return Customs.Module.DeclarationFilterConstants.ModeFilterTypes.DeliveryDropMode;
			yield return Customs.Module.DeclarationFilterConstants.ModeFilterTypes.PickupDropMode;
			yield return Customs.Module.DeclarationFilterConstants.ShipmentType;
		}

		public new LicenseFilterLookups Lookups
		{
			get { return (LicenseFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new LicenseFilterLookups(this);
		}

		protected override void AddModeFilters(ModuleFilterCollection filters)
		{
			base.AddModeFilters(filters);
			AddExchangeHedgeTypeFilter(filters);
			AddManufacturerIndicatorFilter(filters);
			AddDrawbackModalityFilter(filters);
		}

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);
			AddJobDeclarationLinkedImpLicenseFilter(filters);
			AddImpDeclarationEntryLinkedImpLicenseFilter(filters);
			AddDrawbackCANumber(filters);
		}

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationFilters(filters);
			AddManufacturerFilter(filters);
		}

		protected override void AddAdditionalDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DeclarationFilterConstants.DispatchExpiryDate, GetDispatchExpiryDateQuery).MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|DispatchExpiryDate", DeclarationFilterConstants.DispatchExpiryDate);
			filters.AddDateFilter(DeclarationFilterConstants.ShipmentExpiryDate, GetValidityILShipmentDateQuery).MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|ShipmentExpiryDate", DeclarationFilterConstants.ShipmentExpiryDate);
		}

		void AddJobDeclarationLinkedImpLicenseFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.JobDeclarationLinkedImpLicense, GetJobDeclarationLinkedImpLicense);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|JobDeclarationLinkedImpLicense", DeclarationFilterConstants.JobDeclarationLinkedImpLicense);
			filter.MaxLength = JobDeclarationSchema.JE_DeclarationReference.MaxLength;
			filter.SubGroup = LinkedImpLicenseSubGroup;
			filter.SupportsBlankComparisonOperators = false;
		}

		void AddImpDeclarationEntryLinkedImpLicenseFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.ImpDeclarationEntryLinkedImpLicense, GetImpDeclarationEntryLinkedImpLicense);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|ImpDeclarationEntryLinkedImpLicense", DeclarationFilterConstants.ImpDeclarationEntryLinkedImpLicense);
			filter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			filter.SubGroup = LinkedImpLicenseSubGroup;
			filter.SupportsBlankComparisonOperators = false;
		}

		void AddExchangeHedgeTypeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.ExchangeHedgeType, GetExchangeHedgeTypeQuery, Lookups.ExchangeHedgeTypeList);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|ExchangeHedgeType", DeclarationFilterConstants.ExchangeHedgeType);
			filter.MaxLength = JobComInvoiceHeader.Schema.ExchangeHedgeTypeMaxLength;
			filter.SubGroup = InvoiceHeaderCusSupportingInfoSubGroup;
		}

		void AddManufacturerIndicatorFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.ManufacturerIndicator, GetManufacturerIndicatorQuery, Lookups.ManufacturerIndicatorList);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|ManufacturerIndicator", DeclarationFilterConstants.ManufacturerIndicator);
			filter.MaxLength = BRJobComInvoiceLineSchema.JI_ManufacturerIndicator.MaxLength;
		}

		void AddManufacturerFilter(ModuleFilterCollection filters)
		{
			var manufacturerFilter = new ModuleGuidFilterForOrg(DeclarationFilterConstants.Manufacturer, ModuleIDs.Organisation, GetManufacturerQuery, Lookups.ManufacturerList);
			manufacturerFilter.Category = FilterCategories.Organisations;
			manufacturerFilter.MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|Manufacturer", DeclarationFilterConstants.Manufacturer);
			filters.AddFilter(manufacturerFilter);
		}

		void AddDrawbackModalityFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.DrawbackModality, GetDrawbackModalityQuery, Lookups.DrawbackModalityList);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|DrawbackModality", DeclarationFilterConstants.DrawbackModality);
			filter.SubGroup = InvoiceLineCusSupportingInfoSubGroup;
		}

		void AddDrawbackCANumber(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.DrawbackCANumber, GetDrawbackCANumberQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("LicenseFilters|DrawbackCANumber", DeclarationFilterConstants.DrawbackCANumber);
			filter.MaxLength = CusSupportingInfoSchema.CSI_ReferenceNumber.MaxLength;
			filter.SubGroup = InvoiceLineCusSupportingInfoSubGroup;
		}

		ZQuery GetJobDeclarationLinkedImpLicense(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_DeclarationReference, comparisonOperator, value);
		}

		ZQuery GetImpDeclarationEntryLinkedImpLicense(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);

			var cusEntryHeaderQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryHeader), JobDeclarationSchema.JE_ClusterKey, CusEntryHeaderSchema.CH_ClusterKey);
			cusEntryHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

			declarationQuery.AddSubQuery(cusEntryHeaderQuery, JoinCondition.And);

			return declarationQuery;
		}

		ZQuery GetExchangeHedgeTypeQuery(ZString value)
		{
			return new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.ExchangeHedge)
				.AddToFilter(CusSupportingInfoSchema.CSI_Code, value);
		}

		ZQuery GetManufacturerIndicatorQuery(ZString value)
		{
			return InvoiceLineModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, ModelViewConstants.BRJobComInvoiceLine.ClusterKey, ModelViewConstants.BRJobComInvoiceLine.Name, ModelViewConstants.BRJobComInvoiceLine.ManufacturerIndicator, SQLComparisonOperator.Equal, value);
		}

		ZQuery GetManufacturerQuery(ZGuid value)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			var orgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH, OrgHeaderSchema.PK);
			orgHeaderQuery.AddToFilter(OrgHeaderSchema.PK, value);

			var docAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address, OrgAddressSchema.PK);
			docAddressQuery.AddSubQuery(orgHeaderQuery, JoinCondition.And);

			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobComInvoiceLineSchema.Constants.Prefix);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.Manufacturer);
			jobDocAddressQuery.AddSubQuery(docAddressQuery, JoinCondition.And);

			var invoiceLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobDeclarationSchema.JE_ClusterKey, JobComInvoiceLineSchema.JI_ClusterKey);
			invoiceLineQuery.AddSubQuery(jobDocAddressQuery, JoinCondition.And);

			declarationQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);

			return declarationQuery;
		}

		ZQuery GetDrawbackModalityQuery(ZString value)
		{
			return new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.Drawback)
				.AddToFilter(CusSupportingInfoSchema.CSI_Code, value);
		}

		ZQuery GetDrawbackCANumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.Drawback)
				.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, comparisonOperator, value);
		}

		ZQuery GetDispatchExpiryDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
			=> EntryHeaderModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.JE_ClusterKey, ModelViewConstants.BRCusEntryHeader.ClusterKey, ModelViewConstants.BRCusEntryHeader.Name, ModelViewConstants.BRCusEntryHeader.ValidityILDispatchDate, comparisonOperator, startDate, endDate);

		ZQuery GetValidityILShipmentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
			=> EntryHeaderModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.JE_ClusterKey, ModelViewConstants.BRCusEntryHeader.ClusterKey, ModelViewConstants.BRCusEntryHeader.Name, ModelViewConstants.BRCusEntryHeader.ValidityILShipmentDate, comparisonOperator, startDate, endDate);

		#region Model View

		ModelViewColumnQueryHelper<JobComInvoiceLine> InvoiceLineModelViewColumnHelper => invoiceLineModelViewColumnHelper ?? (invoiceLineModelViewColumnHelper = new ModelViewColumnQueryHelper<JobComInvoiceLine>());
		ModelViewColumnQueryHelper<JobComInvoiceLine> invoiceLineModelViewColumnHelper;

		ModelViewColumnQueryHelper<Business.CusEntryHeader> EntryHeaderModelViewColumnHelper => entryHeadermodelViewColumnHelper ?? (entryHeadermodelViewColumnHelper = new ModelViewColumnQueryHelper<Business.CusEntryHeader>());
		ModelViewColumnQueryHelper<Business.CusEntryHeader> entryHeadermodelViewColumnHelper;

		#endregion

		#region Sub Group

		LinkedImpLicenseFilterSubGroup LinkedImpLicenseSubGroup => linkedImpLicenseSubGroup ?? (linkedImpLicenseSubGroup = new LinkedImpLicenseFilterSubGroup());
		LinkedImpLicenseFilterSubGroup linkedImpLicenseSubGroup;

		class LinkedImpLicenseFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var entryInstructionQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryInstructionSchema.CEI_JE);

				var impDeclarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), GenPivotSchema.XX_Relation1ID);
				impDeclarationQuery.AddToFilter(filter);

				var genPivotQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID, CusEntryInstructionSchema.PK);
				genPivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot);
				genPivotQuery.AddSubQuery(impDeclarationQuery, JoinCondition.And);

				entryInstructionQuery.AddSubQuery(genPivotQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(entryInstructionQuery, JoinCondition.And);

				return declarationQuery;
			}
		}

		InvoiceLineCusSupportingInfoFilterSubGroup InvoiceLineCusSupportingInfoSubGroup => invoiceLineCusSupportingInfoSubGroup ?? (invoiceLineCusSupportingInfoSubGroup = new InvoiceLineCusSupportingInfoFilterSubGroup());
		InvoiceLineCusSupportingInfoFilterSubGroup invoiceLineCusSupportingInfoSubGroup;

		InvoiceHeaderCusSupportingInfoFilterSubGroup InvoiceHeaderCusSupportingInfoSubGroup => invoiceHeaderCusSupportingInfoSubGroup ?? (invoiceHeaderCusSupportingInfoSubGroup = new InvoiceHeaderCusSupportingInfoFilterSubGroup());
		InvoiceHeaderCusSupportingInfoFilterSubGroup invoiceHeaderCusSupportingInfoSubGroup;

		#endregion
	}
}
