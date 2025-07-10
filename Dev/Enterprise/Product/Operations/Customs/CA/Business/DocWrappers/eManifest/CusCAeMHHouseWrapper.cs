using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	class CusCAeMHHouseWrapper : NonPersistentBusinessObject, IDocumentWrapper, IVisualizerNoteSupporter, ISourceIdentifierProvider
	{
		public CusCAeMHHouseWrapper(CusCAeMHHouse house)
			: base(house.Factory)
		{
			this.house = house;
			this.lookups = house.Lookups;
		}
		readonly CusCAeMHHouse house;
		readonly CusCAeMHHouseLookups lookups;

		public BusinessObjectCollectionWrapper<CusCAeMHItemWrapper> Items
		{
			get
			{
				if (fItems == null)
				{
					var items = new List<CusCAeMHItemWrapper>();
					ZShort rowIndex = 1;
					foreach (var item in house.Items)
					{
						items.Add(new CusCAeMHItemWrapper(item, rowIndex));
						rowIndex++;
					}
					fItems = new BusinessObjectCollectionWrapper<CusCAeMHItemWrapper>(items);
				}
				return fItems;
			}
		}

		BusinessObjectCollectionWrapper<CusCAeMHItemWrapper> fItems;

		public ZString CargoControlNumber => house.BW_HouseCCN;
		public ZString ReleasePort => CodeDescriptionExtension.GetCodeDescriptionWithSpace(house.BW_CBSAReleasePort, lookups.ReleasePorts);
		public ZString Sub_Location => CodeDescriptionExtension.GetCodeDescriptionWithSpace(house.BW_CBSAReleaseSubLocation, lookups.ReleaseSubLocations);

		public ZString MovementType => house.BW_MovementTypeDescription;

		IEnumerable<IHouseBillContainer> Containers => ((IACIHouseBillProvider)house).Containers;

		public ZString ContainerIdentifier1 => string.Join(",", Containers.Select(x => x.ContainerNumber));
		public ZString SealNumber1 => string.Join(",", Containers.Select(x => x.Seals.FirstOrDefault()));
		public ZString UCR => house.BW_UCR;
		public ZString AmendmentReason => CodeDescriptionExtension.GetCodeDescriptionWithSpace(house.BW_AmendReasonCode, lookups.AmendmentCodes);

		public ZBool ConsolidatedIndicator => house.BW_IsMasterHouse;

		public ZDecimal Weight => house.BW_Weight;
		public ZString WeightUQName => house.BW_WeightUQDescription;

		public ZDecimal Volume => house.BW_Volume;
		public ZString VolumeUQName => house.BW_VolumeUQDesciption;

		public ZString Business2Business => house.BW_B2BComments;

		public ZString DGContactName => ((IACIHouseBillProvider)house).UNDGContact?.OC_ContactName ?? string.Empty;
		public ZString DGPhoneNumber => ((IACIHouseBillProvider)house).UNDGContact?.OC_Phone ?? string.Empty;

		public ZString DGSpecialInstructions => house.BW_DGSpecialInstructions;

		public ZString SpecialInstructions => house.BW_HandlingInstructions;

		public IJobDocAddress Shipper => ((IACIHouseBillProvider)house).Shipper;
		public IJobDocAddress Consignee => ((IACIHouseBillProvider)house).Consignee;

		IEnumerable<IJobDocAddress> DeliveryAddresses => ((IACIHouseBillProvider)house).DeliveryAddresses;
		IEnumerable<IJobDocAddress> NotifyParties => ((IACIHouseBillProvider)house).NotifyParties;

		public ZString FormattedDeliveryAddresses => string.Join("\r\n", DeliveryAddresses.Select(x => x.E2_CompanyName + " " + x.E2_Address1));
		public ZString FormattedNotifyParties => string.Join("\r\n", NotifyParties.Select(x => x.E2_CompanyName + " " + x.E2_Address1));

		public JobDocAddress PlaceOfConsolidation => house.PlaceOfConsolidation;

		public JobDocAddress Consolidator => house.Consolidator;

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => house.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => CusCAeMHHouseSchema.Constants.Prefix;

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => this.house.PK;
	}
}
