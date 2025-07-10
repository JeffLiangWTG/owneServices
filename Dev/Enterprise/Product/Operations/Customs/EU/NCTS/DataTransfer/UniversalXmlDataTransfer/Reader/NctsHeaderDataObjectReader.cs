using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public class NctsHeaderDataObjectReader : NctsHeaderCommonDataObjectReader
	{
		public NctsHeaderDataObjectReader(UniversalShipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(headerDataObject, logger, factory)
		{
		}

		protected override ZString ApplicationCode => CusInBondApplicationCodeList.Codes.NCTS4;

		protected sealed override void PopulateBusinessObjectCore(NctsHeader header, IColumnIndexer headerRow)
		{
			var headerPK = headerRow.GetValue(CusInBondHeaderSchema.PK);

			header.BH_HeaderType = dataObject?.MessageSubType?.Code?.ToString();
			var entryNumberDataObject = dataObject.EntryNumberCollection?.Find(x => x.Type.Code.Equals(CusEntryNumberTypes.Standard.MovementReferenceNumber));
			if (entryNumberDataObject != null)
			{
				CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode).CE_EntryNum = entryNumberDataObject.Number.ToString();
			}
			FillCarrier(header, headerRow);
			FillCustomsReferences(header);
			new AddInfoGroupCollectionDataObjectReader(logger, helper).ReadIntoDataRows(headerPK, CusInBondHeaderSchema.Constants.Prefix, header.IsInDatabase, dataObject);
			FillDates(header);
			FillGuarantees(header);
			FillCountryData(header);
			new NctsMoveHeaderDataObjectReader(dataObject.CommercialInfo, dataObject, logger, helper, header).ReadIntoBusinessObject();
		}

		protected virtual void FillCountryData(NctsHeader header)
		{
		}

		void FillGuarantees(NctsHeader header)
		{
			if (dataObject.GuaranteeCollection != null)
			{
				var guaranteesNotAlreadyInList = dataObject.GuaranteeCollection
					.Where(x => !header.Guarantees.Cast<CommonGuarantee>().Any(y => y.PW_BondType + y.PW_BondNumber == x.BondType?.GetNullableCodeAsUpperCase() + x.BondNumber.GetValueOrDefault())).ToList();

				foreach (var guarantee in guaranteesNotAlreadyInList)
				{
					var guaranteeBO = header.Guarantees.AddNew();
					guaranteeBO.PW_ActivityCode = guarantee.ActivityCode?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_BondType = guarantee.BondType?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_BondFiledPort = guarantee.BondFiledPort?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_BondNumber = guarantee.BondNumber.GetValueOrDefault();
					guaranteeBO.PW_BondNumber2 = guarantee.BondNumber2.GetValueOrDefault();
					guaranteeBO.PW_SuretyCode = guarantee.SuretyCode.GetValueOrDefault();
					guaranteeBO.PW_Password = guarantee.AccessCode.GetValueOrDefault();
					guaranteeBO.PW_BondAmount = guarantee.BondAmount.GetValueOrDefault();
					guaranteeBO.PW_HolderIdentification = guarantee.HolderIdentification.GetValueOrDefault();
					guaranteeBO.PW_RX_NKCurrency = guarantee.BondCurrency?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_RN_NKCountryOfIssue = guarantee.CountryOfIssue?.GetNullableCodeAsUpperCase() ?? ZString.Empty;
					guaranteeBO.PW_ValidityLimitation = guarantee.ValidityLimitation.GetValueOrDefault();
				}
			}
		}

		void FillDates(NctsHeader header)
		{
			if (dataObject.DateCollection != null)
			{
				if (dataObject.DateCollection.Find(x => x.Type == DateType.Arrival) != null)
				{
					var arrivalMoveHeader = header.ArrivalMovementHeader;
					if (arrivalMoveHeader != null)
					{
						var arrivalMoveHeaderRow = GetColumnIndexer(arrivalMoveHeader);
						FillDates(arrivalMoveHeaderRow, dataObject.DateCollection, ZBool.False,
							new DateTypeSchemaColumnMap(CusInBondMoveHeaderSchema.BM_EntryDate, new[] { DateType.Arrival }));
					}
				}
				if (dataObject.DateCollection.Find(x => x.Type == DateType.Departure) != null)
				{
					var moveHeader = header.MovementHeader;
					if (moveHeader != null)
					{
						var moveHeaderRow = GetColumnIndexer(moveHeader);
						FillDates(moveHeaderRow, dataObject.DateCollection, ZBool.False,
							new DateTypeSchemaColumnMap(CusInBondMoveHeaderSchema.BM_EntryDate, new[] { DateType.Departure }));
					}
				}
				if (dataObject.DateCollection.Find(x => x.Type == DateType.Unpack) != null)
				{
					if (header.UnloadingRemark != null)
					{
						var unloadingRemarkRow = GetColumnIndexer(header.UnloadingRemark);
						FillDates(unloadingRemarkRow, dataObject.DateCollection, ZBool.False,
							new DateTypeSchemaColumnMap(UnloadingRemarkAddInfoSchema.G9_UnloadingDate, new[] { DateType.Unpack }));
					}
				}
			}
		}

		void FillCarrier(IDocAddresses docAddresses, IColumnIndexer headerRow)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var supportedAddressTypes = docAddresses?.SupportedAddressTypes;
				if (supportedAddressTypes != null && supportedAddressTypes.Count > 0)
				{
					var organizationAddress = dataObject.OrganizationAddressCollection.Find(x => x.AddressType.ToString() == nameof(DocAddressType.Carrier));
					if (organizationAddress != null)
					{
						var orgAddress = new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatched();
						if (orgAddress != null)
						{
							SetValue(headerRow, CusInBondHeaderSchema.BH_OH_Carrier, orgAddress.OA_OH);
						}
					}
				}
			}
		}

		void FillCustomsReferences(NctsHeader header)
		{
			if (CustomsReferenceDictionary.TryGetValue(DataObjectWriterConstants.Header.AddInfo.CustomReferenceType, out var customReferenceTypes))
			{
				if (customReferenceTypes.Count > 0 && header.MovementHeader is NctsDepartureMovementHeader moveHeader)
				{
					var moveHeaderRow = GetColumnIndexer(moveHeader);
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_AdditionalText, customReferenceTypes[0].Reference);
				}
			}
			FillOfficeCodes(header);
		}
	}
}
