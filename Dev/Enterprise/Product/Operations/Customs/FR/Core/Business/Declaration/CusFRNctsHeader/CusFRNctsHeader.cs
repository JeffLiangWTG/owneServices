using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	[DependentBusinessObject(typeof(NctsHeader), "PK")]
	public class CusFRNctsHeader : AutoCusFRNctsHeader
	{
		public CusFRNctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public NctsHeader Header => Factory.Load<NctsHeader>(CFN_BH);

		protected override CusFRNctsHeaderValidation GetNewValidation() => Header?.IsPhase5 ?? false ? new CusFRNctsHeaderPhase5Validation(this) : new CusFRNctsHeaderPhase4Validation(this);

		[List(nameof(Header) + "." + nameof(NctsHeader.Lookups) + "." + nameof(NctsHeaderLookups.DetailedStatusCodeList))]
		public override ZString CFN_DetailedDepartureStatusCode
		{
			get => base.CFN_DetailedDepartureStatusCode;
			set
			{
				var oldValue = CFN_DetailedDepartureStatusCode;
				base.CFN_DetailedDepartureStatusCode = value;
				if (!value.IsEmpty && oldValue != CFN_DetailedDepartureStatusCode)
				{
					Header?.Logs?.AddNew(Events.StatusChange, value);
				}
			}
		}

		[List(nameof(Header) + "." + nameof(NctsHeader.Lookups) + "." + nameof(NctsHeaderLookups.DetailedStatusCodeList))]
		public override ZString CFN_DetailedArrivalStatusCode
		{
			get => base.CFN_DetailedArrivalStatusCode;
			set
			{
				var oldValue = CFN_DetailedArrivalStatusCode;
				base.CFN_DetailedArrivalStatusCode = value;
				if (!value.IsEmpty && oldValue != CFN_DetailedArrivalStatusCode)
				{
					Header?.Logs?.AddNew(Events.StatusChange, value);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusFRNctsHeaderLookups.NatureOfSealsList))]
		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration.CFN_NatureOfSeals", Caption = "Seals Nature")]
		public override ZString CFN_NatureOfSeals { get => base.CFN_NatureOfSeals; set => base.CFN_NatureOfSeals = value; }

		public override ZBool CFN_IsPrelodgedMovement
		{
			get => base.CFN_IsPrelodgedMovement;
			set
			{
				var hasChanged = CFN_IsPrelodgedMovement != value;
				base.CFN_IsPrelodgedMovement = value;

				var header = Header;
				var customsOffices = header.IsPhase5 ? header.MovementHeader.CustomsOffices : header.CustomsOffices;
				foreach (var office in customsOffices)
				{
					office.MarkAsNeedingValidation();
				}
				if (!IsCopying && hasChanged)
				{
					Header?.MovementHeader?.PreLodgedForAgreedLocationOfGoodsCodeInfo.RefreshBinding();
				}
			}
		}

		public override ZBool CFN_IsQueryAvailableOnPaper
		{
			get => base.CFN_IsQueryAvailableOnPaper;
			set
			{
				var hasChanged = CFN_IsQueryAvailableOnPaper != value;
				base.CFN_IsQueryAvailableOnPaper = value;
				if (!IsCopying && hasChanged && !CFN_IsQueryAvailableOnPaper)
				{
					CFN_IsTC11DeliveredByCustoms = false;
				}
			}
		}

		[ReadOnlyMember(nameof(CFN_IsTC11DeliveredByCustoms_ReadOnly))]
		public override ZBool CFN_IsTC11DeliveredByCustoms
		{
			get => base.CFN_IsTC11DeliveredByCustoms;
			set => base.CFN_IsTC11DeliveredByCustoms = value;
		}

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CFN_NatureOfSeals = "2";
		}

		#endregion

		ZBool CFN_IsTC11DeliveredByCustoms_ReadOnly => !CFN_IsQueryAvailableOnPaper;
	}
}
