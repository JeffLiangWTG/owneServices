using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatGroup : AutoCusIntrastatGroup
		, IClusterKeyMaster
		, Integration.Customs.EU.ICusIntrastatGroup
	{
		public CusIntrastatGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusIntrastatGroup.Schema
		{
			public const string ReporterCode = nameof(CusIntrastatGroup.ReporterCode);
		}

		[ResourceStringData("6babd2b0-6e01-49a0-9ff8-926e24f7d059", Caption = "Flow")]
		public override ZString CIG_Flow { get => base.CIG_Flow; set => base.CIG_Flow = value; }

		[ResourceStringData("95964638-c5c3-4fa5-8579-4bc781019511", Caption = "Group Number")]
		public override ZString CIG_GroupNumber { get => base.CIG_GroupNumber; set => base.CIG_GroupNumber = value; }

		[ResourceStringData("4649a38b-62e0-416c-8f89-297f9819fc69", Caption = "Reporter")]
		public ZString ReporterCode => Reporter?.OH_Code ?? ZString.Empty;

		[ResourceStringData("e2444b6d-8c12-4806-a15f-80373c39c272", Caption = "Reporting Period")]
		public override ZString CIG_Period { get => base.CIG_Period; set => base.CIG_Period = value; }

		[ResourceStringData("ab18206e-c919-496c-a146-d3b26edf53e0", Caption = "Status")]
		public override ZString CIG_Status { get => base.CIG_Status; set => base.CIG_Status = value; }

		protected override ZString HumanReadableNameCore => Res.GetString("2096c8fa-e9e6-4194-b67e-d5a7c8857306", "Intrastat - Report");

		[LightValidationTestExempt]
		public override ZInt CIG_ClusterKey
		{
			get => base.CIG_ClusterKey;
			set
			{
				if (CIG_ClusterKey != value)
				{
					this.CheckCanSetMasterClusterKey();
					base.CIG_ClusterKey = value;
					if (!IsCopying)
					{
						CusIntrastatMergedLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ChildEditable(true)]
		public ICusIntrastatMergedLineCollection<CusIntrastatMergedLine> CusIntrastatMergedLines
		{
			get
			{
				if (cusIntrastatMergedLines == null)
				{
					cusIntrastatMergedLines = CreateNewCusIntrastatMergedLineCollection();
					RegisterEditableChildObject(cusIntrastatMergedLines);
				}
				return cusIntrastatMergedLines;
			}
		}
		ICusIntrastatMergedLineCollection<CusIntrastatMergedLine> cusIntrastatMergedLines;

		protected virtual ICusIntrastatMergedLineCollection<CusIntrastatMergedLine> CreateNewCusIntrastatMergedLineCollection() => new CusIntrastatMergedLineCollection<CusIntrastatMergedLine>(this);

		#region IClusterKeyMaster

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CIG_ClusterKeyInfo;

		#endregion
	}
}
