using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class SplitHouse : SplitConsignment, Integration.Customs.GB.CCSUK.ISplitHouse
	{
		public SplitHouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusHAWB HAWB
		{
			get { return Factory.Load<CusHAWB>(CG_CS); }
			set { CG_CS = value.PK; }
		}

		[ReadOnlyMember(nameof(AgentBadgeReadOnly))]
		[MaxLength(3)]
		[List(nameof(HAWB) + "." + nameof(CusHAWB.MAWB) + "." + nameof(CusHAWB.MAWB.Lookups) + "." + nameof(CusMAWBLookups.AgentsList))]
		public new ZString AgentBadge
		{
			get { return base.AgentBadge; }
			set { base.AgentBadge = value; }
		}

		protected override ICcsukCusAwb AwbCore
		{
			get { return HAWB; }
			set { HAWB = (CusHAWB)value; }
		}

		protected override ICuscar GetCuscarWrapperCore()
		{
			return new CuscarWrapperFromSplitHouse(this);
		}

		protected override CusUnderbondCollection<InterAirportRemoval> IARsCore
		{
			get { return HAWB.IARs; }
		}

		protected override CusUnderbondCollection<InterShedRemoval> ISRsCore
		{
			get { return HAWB.ISRs; }
		}

		protected override CusUnderbondCollection<TranshipmentRemoval> TSRsCore
		{
			get { return HAWB.TSRs; }
		}

		protected override CusUnderbondCollection<Fallback> FBKsCore
		{
			get { return HAWB.FBKs; }
		}

		protected override ControllerID ModuleControllerIdCore
		{
			get { return ControllerIDs.Customs.GB.CcsukSplitHouseController; }
		}

		protected override ZString ConsignmentParentName
		{
			get { return "House"; }
		}

		protected override bool IsParentLinkedToForwardingJob
		{
			get { return HAWB.IsLinkedToJobShipment; }
		}

		protected override void SetNewChiefDeclarationProperties(JobDeclaration declaration)
		{
			declaration.JE_DateOfArrival = HAWB.MAWB.CM_ArrivalDate;
			declaration.JE_EntrySubStyle = HAWB.IsPrearrival ?
				GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullDeclarationGoodsNotArrived :
				GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived;
			declaration.JE_VoyageFlightNo = HAWB.MAWB.CM_FlightNo;
			declaration.JE_HouseBill = HAWB.CS_HAWB;
			HAWB.SynchroniseToDeclaration(declaration, true, false);
		}

		protected override ZString ChiefMasterUCRReferenceSuffixCore
		{
			get { return ReferenceNumber.KeepAlphanumericCharacters(); }
		}

		protected override CusAddInfoCollection<CommunityHandlingCode> CommunityHandlingCodesCore
		{
			get
			{
				var collection = new CusAddInfoCollection<CommunityHandlingCode>(HAWB, new ZQuery(CusAddInfoSchema.B7_AddInfoData, SQLComparisonOperator.Contains, string.Format("{0}={1}", CcsukCusAddInfoSchema.C4_SplitReferenceToWhichThisPertains.Name.Substring(3), SplitReference)));
				collection.Load();
				return collection;
			}
		}
	}
}
