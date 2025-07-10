using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage
{
	public class CusTempStorageJobHeaderDataObjectWriter : TopLevelDataObjectWriter<CusTempStorageJobHeader, UniversalShipment>
	{
		public CusTempStorageJobHeaderDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.TemporaryStorage;

		protected override void PopulateDataObject(CusTempStorageJobHeader sourceBO, UniversalShipment dataObject)
		{
			dataObject.CustomsOffice = new CodeDescriptionPair10Char
			{
				Code = sourceBO.SJH_CustomsOffice,
				Description = sourceBO.SJH_CustomsOfficeDescription
			};
			dataObject.TransportMode = new CodeDescriptionPair
			{
				Code = sourceBO.SJH_TransportMode,
				Description = sourceBO.Lookups.TransportModeList.GetDescriptionFromCode(sourceBO.SJH_TransportMode)
			};
			dataObject.AddOrgAddress(writeManager, sourceBO.Presenter, Constants.AddressType.Declarant);
			dataObject.AddOrgAddress(writeManager, sourceBO.Customer, Constants.AddressType.SendersLocalClient);

			PopulateDates(sourceBO, dataObject);
			PopulateAdditionalReferences(sourceBO, dataObject);

			PopulateCountrySpecificData(sourceBO, dataObject);
		}

		void PopulateAdditionalReferences(CusTempStorageJobHeader sourceBO, UniversalShipment dataObject)
		{
			var countryCode = sourceBO.CountryCode;
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
					Type = GetRegistrationEntryType(),
					ContextInformation = countryCode,
					ReferenceNumber = sourceBO.SJH_ReferenceNumber
				},
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = sourceBO.SJH_PreviousReferenceType,
						Description = Constants.AdditionalReference.EntryType.Descriptions.PreviousReferenceNumber
					},
					ContextInformation = countryCode,
					ReferenceNumber = sourceBO.SJH_PreviousReferenceNumber
				}
			});
		}

		protected virtual EntryType GetRegistrationEntryType()
		{
			return new EntryType
			{
				Code = Constants.AdditionalReference.EntryType.Codes.Registration,
				Description = Constants.AdditionalReference.EntryType.Descriptions.Registration
			};
		}

		static void PopulateDates(CusTempStorageJobHeader sourceBO, UniversalShipment dataObject)
		{
			dataObject.SetDateCollection(() => new List<Date>
			{
				{ DateType.EntryDate, ZBool.True, sourceBO.SJH_PresentationDate }
			});
		}

		protected virtual void PopulateCountrySpecificData(CusTempStorageJobHeader sourceBO, UniversalShipment dataObject)
		{
		}
	}
}
