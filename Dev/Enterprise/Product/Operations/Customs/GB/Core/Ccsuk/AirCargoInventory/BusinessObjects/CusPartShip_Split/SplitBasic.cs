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
	public class SplitBasic : SplitConsignment, Integration.Customs.GB.CCSUK.ISplitBasic
	{
		public SplitBasic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusMAWB Basic
		{
			get { return Factory.Load<CusMAWB>(CG_CM_LinkToPartMaster); }
			set { CG_CM_LinkToPartMaster = value.PK; }
		}

		[ReadOnlyMember(nameof(AgentBadgeReadOnly))]
		[MaxLength(3)]
		[List(nameof(Basic) + "." + nameof(SplitBasic.Basic.Lookups) + "." + nameof(CusMAWBLookups.AgentsList))]
		public new ZString AgentBadge
		{
			get { return base.AgentBadge; }
			set { base.AgentBadge = value; }
		}

		protected override ICcsukCusAwb AwbCore
		{
			get { return Basic; }
			set { Basic = (CusMAWB)value; }
		}

		protected override ICuscar GetCuscarWrapperCore()
		{
			return new CuscarWrapperFromSplitBasic(this);
		}

		protected override CusUnderbondCollection<InterAirportRemoval> IARsCore
		{
			get { return BasicsHawbHelper.IARs; }
		}

		protected override CusUnderbondCollection<InterShedRemoval> ISRsCore
		{
			get { return BasicsHawbHelper.ISRs; }
		}

		protected override CusUnderbondCollection<TranshipmentRemoval> TSRsCore
		{
			get { return BasicsHawbHelper.TSRs; }
		}

		protected override CusUnderbondCollection<Fallback> FBKsCore
		{
			get { return BasicsHawbHelper.FBKs; }
		}

		CusHAWB BasicsHawbHelper
		{
			get { return Basic != null ? Basic.MasterLevelHouseHelper : null; }
		}

		protected override ControllerID ModuleControllerIdCore
		{
			get { return ControllerIDs.Customs.GB.CcsukSplitBasicController; }
		}

		protected override ZString ConsignmentParentName
		{
			get { return "Basic"; }
		}

		protected override bool IsParentLinkedToForwardingJob
		{
			get { return Basic.IsLinkedToJobConsol; }
		}

		protected override void SetNewChiefDeclarationProperties(JobDeclaration declaration)
		{
			declaration.JE_DateOfArrival = Basic.CM_ArrivalDate;
			declaration.JE_EntrySubStyle = Basic.IsPrearrival ?
				GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullDeclarationGoodsNotArrived :
				GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived;
			declaration.JE_VoyageFlightNo = Basic.CM_FlightNo;
			Basic.MasterLevelHouseHelper.SynchroniseToDeclaration(declaration, true, false);
		}

		protected override ZString ChiefMasterUCRReferenceSuffixCore
		{
			get { return Basic.ReferenceNumber.KeepAlphanumericCharacters() + "        " + SplitReference; }
		}

		protected override CusAddInfoCollection<CommunityHandlingCode> CommunityHandlingCodesCore
		{
			get
			{
				var collection = new CusAddInfoCollection<CommunityHandlingCode>(Basic.MasterLevelHouseHelper, new ZQuery(CusAddInfoSchema.B7_AddInfoData, SQLComparisonOperator.Contains, string.Format("{0}={1}", CcsukCusAddInfoSchema.C4_SplitReferenceToWhichThisPertains.Name.Substring(3), SplitReference)));
				collection.Load();
				return collection;
			}
		}
	}
}
