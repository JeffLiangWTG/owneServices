using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.NCTS.DataTransfer
{
	public abstract class NctsHeaderCommonDataObjectReader : ShipmentDataObjectReader<NctsHeader>
	{
		protected NctsHeaderCommonDataObjectReader(UniversalShipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(headerDataObject, logger, factory)
		{
			helper = new UniversalDataObjectReaderHelper(factory, Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), headerDataObject.GetSourceCountryCode(), headerDataObject.GetDataProviderForCodeMapping());
		}

		protected readonly UniversalDataObjectReaderHelper helper;

		public override DataContextType DataContextType => DataContextType.NctsHeader;

		protected override IMatchingBusinessEntityFinder<NctsHeader> GetCombinedReferenceMatcher() => null;

		protected abstract ZString ApplicationCode { get; }

		protected override NctsHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			NctsHeader result = null;
			var entryNumberDataObject = EntryNumberDataObject;
			if (entryNumberDataObject != null)
			{
				var query = new ZDBOnlyQuery(typeof(NctsHeader));
				query.AddToFilter(CusInBondHeaderSchema.BH_IsActive, true);
				query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, ApplicationCode);
				AddAdditionalFilterForExistingMatching(query);
				var branchQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				branchQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumberDataObject.Number);
				branchQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				query.AddSubQuery(branchQuery, JoinCondition.And);
				var currentBranchesQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK, CusInBondHeaderSchema.BH_GB);
				currentBranchesQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				query.AddSubQuery(currentBranchesQuery, JoinCondition.And);
				query.OrderBy = CusInBondHeaderSchema.Constants.BH_SystemCreateTimeUtc + OrderByClause.Descending;
				result = helper.LoadTop1<NctsHeader>(query);
			}
			return result;
		}

		protected virtual void AddAdditionalFilterForExistingMatching(ZDBOnlyQuery query) { }

		protected UniversalDataBuss.DataObjects.Universal.EntryNumber EntryNumberDataObject => CachedValueHelper.GetValue(ref entryNumberDataObjectCached, () => dataObject.EntryNumberCollection?.Find(x => x.Type.GetCodeAsUpperCase() == CusEntryNumberTypes.Standard.MovementReferenceNumber));
		CachedValue<UniversalDataBuss.DataObjects.Universal.EntryNumber> entryNumberDataObjectCached;

		protected override NctsHeader GetNewBusinessObject()
		{
			var result = (NctsHeader)factory.New(new NctsHeaderTypeDecider().GetTypeForCountryCode(helper.TargetCountryCode));
			result.BH_ApplicationCode = ApplicationCode;
			return result;
		}

		protected sealed override void PopulateBusinessObject(NctsHeader header)
		{
			if (IsUpdateAllowed(header))
			{
				var headerRow = GetColumnIndexer(header);
				FillBranch(headerRow);
				var entryNumberDataObject = EntryNumberDataObject;
				if (entryNumberDataObject != null)
				{
					CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode).CE_EntryNum = entryNumberDataObject.Number.GetValueOrDefault();
				}
				FillJobDocAddresses(header);
				FillNoteCollection(header);
				PopulateBusinessObjectCore(header, headerRow);
			}
		}

		protected virtual void PopulateBusinessObjectCore(NctsHeader header, IColumnIndexer headerRow)
		{
		}

		protected Dictionary<ZString, List<CustomsReference>> CustomsReferenceDictionary
		{
			get
			{
				if (customsReferenceDictionary == null)
				{
					customsReferenceDictionary = new Dictionary<ZString, List<CustomsReference>>();
					var customsReferenceCollection = GetCustomsReferenceCollection();
					if (customsReferenceCollection != null)
					{
						foreach (var customsReference in customsReferenceCollection)
						{
							var type = customsReference.Type.GetCodeAsUpperCase();
							if (!type.IsEmpty)
							{
								var list = customsReferenceDictionary.GetOrAdd(type, () => new List<CustomsReference>());
								list.Add(customsReference);
							}
						}
					}
				}
				return customsReferenceDictionary;
			}
		}
		Dictionary<ZString, List<CustomsReference>> customsReferenceDictionary;

		protected virtual List<CustomsReference> GetCustomsReferenceCollection() => dataObject.CustomsReferenceCollection;

		protected void FillOfficeCodes(NctsHeader header)
		{
			if (CustomsReferenceDictionary.TryGetValue(EU.Business.CusCodeDataTypeList.Codes.OfficeCode, out var officeCodeDataObjects))
			{
				var customsOffices = header.IsPhase5 ? header.MovementHeader.CustomsOffices : header.CustomsOffices;
				var existingOfficeCodes = customsOffices.Cast<NctsEuOfficeCode>().GroupBy(x => x.CY_Code).ToDictionary(x => x.Key, y => y.ToList());
				foreach (var officeCodeDataObject in officeCodeDataObjects)
				{
					var code = officeCodeDataObject.SubType.GetCodeAsUpperCase();
					NctsEuOfficeCode officeCode = null;
					if (existingOfficeCodes.TryGetValue(code, out var officeCodes))
					{
						officeCode = officeCodes[0];
						if (officeCodes.Count == 1)
						{
							existingOfficeCodes.Remove(code);
						}
					}
					else
					{
						officeCode = customsOffices.AddNew();
					}
					var officeCodeRow = GetColumnIndexer(officeCode);

					SetValue(officeCodeRow, CusCodeDataSchema.CY_Code, code);
					SetValue(officeCodeRow, CusCodeDataSchema.CY_Data, officeCodeDataObject.Reference);
					SetValue(officeCodeRow, CusCodeDataSchema.CY_IsOverridden, officeCodeDataObject.IsOverridden);
					SetValue(officeCodeRow, CusCodeDataSchema.CY_Order, officeCodeDataObject.Order);
					SetValue(officeCodeRow, CusCodeDataSchema.CY_Date, officeCodeDataObject.DateCollection?.Find(x => x.Type == DateType.DateAtOffice)?.Value);
				}
			}
		}

		bool IsUpdateAllowed(NctsHeader header)
		{
			if (header.IsInDatabase && header.IsMessagingActive)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("05C9863A-AB6C-4E04-86EC-39D13FC6C34F", "{0} data will not be updated as there is active messaging.", header.HumanReadableName));
				return false;
			}
			return true;
		}

		void FillNoteCollection(NctsHeader header)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, header).ReadIntoCollection();
			}
		}

		void FillJobDocAddresses(IDocAddresses docAddresses)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var supportedAddressTypes = docAddresses?.SupportedAddressTypes;
				if (supportedAddressTypes != null && supportedAddressTypes.Count > 0)
				{
					foreach (var orgAddressDataObject in dataObject.OrganizationAddressCollection)
					{
						var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
						DocAddressType docAddressType;
						if (Enum.TryParse(addressType, false, out docAddressType) && supportedAddressTypes.Contains(docAddressType))
						{
							OrganisationDataObjectReader.MatchedOrNew(docAddresses, orgAddressDataObject, logger, factory, null);
						}
					}
				}
			}
		}

		void FillBranch(IColumnIndexer headerRow)
		{
			ZGuid branchPK = dataObject.GetBranchPK(factory.BOFactory);
			if (branchPK.IsValid)
			{
				SetValue(headerRow, CusInBondHeaderSchema.BH_GB, branchPK);
			}
		}
	}
}
