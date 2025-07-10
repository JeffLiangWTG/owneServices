using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class NonPersistentPackagePivot : AutoNonPersistentPackagePivot
	{
		public NonPersistentPackagePivot(EMCSJobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory)
		{
			this.invoiceLine = invoiceLine;
		}
		protected readonly EMCSJobComInvoiceLine invoiceLine;

		public EMCSPackage Package
		{
			get
			{
				if (package == null || package.IsDeleted)
				{
					return null;
				}
				else
				{
					return package;
				}
			}
			set
			{
				package = value;
				base.IsForInvoiceLine = HasPivot;
			}
		}
		EMCSPackage package;

		[ResourceStringData("NonPersistentPackagePivot.IsForInvoiceLine", Caption = "Is for invoice Line?")]
		public override ZBool IsForInvoiceLine
		{
			get { return HasPivot; }
			set
			{
				base.IsForInvoiceLine = value;

				if (Package != null)
				{
					if (base.IsForInvoiceLine)
					{
						AddIsForInvoiceLinePivot();
					}
					else
					{
						DeleteIsForInvoiceLinePivot();
					}
				}
				IsForInvoiceLineInfo.RefreshBinding();
			}
		}

		public override ZPropertyInfo IsForInvoiceLineInfo
		{
			get { return GetZPropertyInfo(nameof(IsForInvoiceLine)); }
		}

		ZBool HasPivot
		{
			get { return IsForInvoiceLinePivot != null; }
		}

		void AddIsForInvoiceLinePivot()
		{
			var pivot = IsForInvoiceLinePivot;
			if (invoiceLine != null && pivot == null)
			{
				var genPivot = Factory.New<GenPivot>();
				genPivot.XX_RelationType = "EMC";
				genPivot.XX_Relation1ID = invoiceLine.PK;
				genPivot.XX_Relation2ID = package.PK;
				genPivot.XX_Relation1TableCode = JobComInvoiceLineSchema.Constants.Prefix;
				genPivot.XX_Relation2TableCode = CusInvPackSchema.Constants.Prefix;
			}
		}

		void DeleteIsForInvoiceLinePivot()
		{
			IsForInvoiceLinePivot?.Delete();
		}

		GenPivot IsForInvoiceLinePivot
		{
			get
			{
				GenPivot pivot = null;
				if (invoiceLine != null && package != null)
				{
					var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, "EMC");
					pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, invoiceLine.PK);
					pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, package.PK);
					pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceLineSchema.Constants.Prefix);
					pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusInvPackSchema.Constants.Prefix);
					pivot = invoiceLine.Factory.LoadTop1<GenPivot>(pivotQuery);
				}
				return pivot;
			}
		}

		[ResourceStringData("NonPersistentPackagePivot.UnitCount", Caption = "Number Of Packages")]
		public override ZLong UnitCount => Package?.B5_UnitCount ?? ZLong.Zero;

		[ResourceStringData("NonPersistentPackagePivot.UnitType", Caption = "UQ")]
		public override ZString UnitType => Package?.B5_UnitType ?? ZString.Empty;

		[ResourceStringData("NonPersistentPackagePivot.MarksAndNumbers", Caption = "Shipping Marks")]
		public override ZString MarksAndNumbers => Package?.B5_MarksAndNumbers ?? ZString.Empty;

		[ResourceStringData("NonPersistentPackagePivot.SealNumber", Caption = "Seal Number")]
		public override ZString SealNumber => Package?.B5_SealNumber ?? ZString.Empty;

		[ResourceStringData("NonPersistentPackagePivot.SealComment", Caption = "Seal Comment")]
		public override ZString SealComment => Package?.B5_SealComment ?? ZString.Empty;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				DeleteIsForInvoiceLinePivot();
			}
			base.Delete();
		}
	}
}
