using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.DataTransfer
{
	public class NctsHeaderDataContextManager : ShipmentDataContextManager<NctsHeader>
	{
		public ZString CountryCode;

		public override ZString DataContextKey
		{
			get { return ParentBO.BH_JobReference; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.NctsHeader; }
		}

		public override string DefaultOutputDirectory
		{
			get { return ""; }
		}

		public override bool ManagesEvents => true;

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) =>
			matchingValues.Key.IsEmpty ? ZQuery.NoResultQuery : new(CusInBondHeaderSchema.BH_JobReference, matchingValues.Key);

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return null;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var countryCode = GetCountryCode(ParentBO);
			return ObjectFactory.GetCountrySpecificOrDefault<NctsEventParentFinder>(countryCode, factory, this, logger);
		}

		IUniversalNCTSDataObjectProvider GetUniversalCustomsDataObjectProvider(string countryCode)
		{
			var objectHandle = string.IsNullOrEmpty(countryCode) ? null : (ObjectHandle)ObjectFactory.Get<Hashtable>("UniversalNCTSDataObjectProviders")[countryCode];
			return (IUniversalNCTSDataObjectProvider)objectHandle?.GetObject();
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var countryCode = GetCountryCode(ParentBO);
			var provider = GetUniversalCustomsDataObjectProvider(countryCode);
			var applicationCode = universalShipment.MessagingApplicationCode.GetCodeAsUpperCase() == CusInBondApplicationCodeList.Codes.NCTS5 ? CusInBondApplicationCodeList.Codes.NCTS5 : CusInBondApplicationCodeList.Codes.NCTS4;
			var reader = provider?.GetNewNctsHeaderDataObjectReader(universalShipment, logger, factory, applicationCode);
			if (reader == null)
			{
				if (applicationCode == CusInBondApplicationCodeList.Codes.NCTS5)
				{
					reader = new Phase5.NctsDepartureMovementHeaderDataObjectReader(universalShipment, logger, factory);
				}
				else
				{
					reader = new Phase4.NctsHeaderDataObjectReader(universalShipment, logger, factory);
				}
			}

			return reader;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return GetShipmentDataObjectWriter(writeManager, ParentBO);
		}

		ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager, NctsHeader header)
		{
			if (header == null)
			{
				header = writeManager.Action != null ? writeManager.Action.ParentBO as NctsHeader : null;
			}
#if DEBUG
			if (header == null)
			{
				throw new System.InvalidOperationException("Unable to get Ncts from DataContextManager.ParentBO or IDataWritingManager.Action.ParentBO to retrieve country-specific writer.");
			}
#endif
			var result = GetNctsHeaderDataObjectWriter(writeManager, header);
			if (result == null)
			{
				if (header?.IsPhase5 ?? false)
				{
					switch (header.BH_HeaderType.ToUpperInvariant())
					{
						case NctsMovementType.Codes.Departure:
							result = new Phase5.NctsDepartureMovementHeaderDataObjectWriter(writeManager);
							break;
						case NctsMovementType.Codes.Arrival:
							result = new Phase5.NctsArrivalMovementHeaderDataObjectWriter(writeManager);
							break;
						default:
							ErrorReporter.ReportOnce("No NCTS Phase 5 UXML Writer Support");
							break;
					}
				}
				else
				{
					result = new Phase4.NctsHeaderDataObjectWriter(writeManager);
				}
			}
			return result;
		}

		ITopLevelDataObjectWriter GetNctsHeaderDataObjectWriter(IDataWritingManager writeManager, NctsHeader header)
		{
			ITopLevelDataObjectWriter result = null;
			if (header != null)
			{
				result = GetUniversalCustomsDataObjectProvider(GetCountryCode(header))?.GetNewNctsHeaderDataObjectWriter(writeManager, header.BH_ApplicationCode);
			}
			return result;
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override bool TryGetMatchingDataTarget(Shipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = null;
			return base.TryGetMatchingDataTarget(universalShipment, out dataTarget);
		}

		protected ZString GetCountryCode(NctsHeader header) => Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode); //(header.Branch?.Company ?? GlbCompany.CurrentCompany).GC_RN_NKCountryCode
	}
}
