using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	class CusCAeMHMasterDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, IVisualizerNoteSupporter, ISourceIdentifierProvider
	{
		internal CusCAeMHMasterDocumentWrapper(CusCAeMHMaster master)
			: base(master.Factory)
		{
			this.master = master;
			carrier = new Lazy<ZZRefCarrierCombined>(() =>
					new ZZRefCarrierCombined.Loader(Factory).LoadFromCode(Core.Constants.CountryCodes.Canada, master.BP_CBSACarrierCode));
		}
		readonly CusCAeMHMaster master;
		readonly Lazy<ZZRefCarrierCombined> carrier;

		public ACIForwarderCloseMessage LatestACIAcceptedMessage
		{
			get
			{
				if (latestACIAcceptedMessage == null)
				{
					var afcMessage = master.MessagesForDisplay?.
						Where(x => x is ACIForwarderCloseMessage && x.EM_ReceiveTransmit == EDIMessage.Direction.Receive && x.EM_Status == EDIMessageStatusList.Codes.Received).CollectMaxBy(m => m.EM_MessageDateTime).FirstOrDefault();
					if (afcMessage != null)
					{
						latestACIAcceptedMessage = afcMessage as ACIForwarderCloseMessage;
					}
				}
				return latestACIAcceptedMessage;
			}
		}
		ACIForwarderCloseMessage latestACIAcceptedMessage;

		public ZString ReferenceNumber => LatestACIAcceptedMessage?.CargoControlNumber ?? string.Empty;

		public ZString PreviousCCN => master.BP_MasterHouseCCN;

		public BusinessObjectCollectionWrapper<HouseBill> HouseBills
		{
			get
			{
				return houseBills ?? (houseBills = new BusinessObjectCollectionWrapper<HouseBill>(GetHouseBills()));
			}
		}
		BusinessObjectCollectionWrapper<HouseBill> houseBills;

		IEnumerable<HouseBill> GetHouseBills()
		{
			foreach (var houseBill in master.HouseBills)
			{
				yield return new HouseBill() { HouseCCN = houseBill.BW_HouseCCN };
			}
		}

		public ZZRefCarrierCombined Carrier => carrier.Value;

		public ZString CarrierName => Carrier?.ZZ4_Description ?? "";

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => master.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => CusCAeMHMasterSchema.Constants.Prefix;

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => this.master.PK;
	}

	class HouseBill : NonPersistentBusinessObject
	{
		public ZString HouseCCN { get; internal set; }
	}
}
