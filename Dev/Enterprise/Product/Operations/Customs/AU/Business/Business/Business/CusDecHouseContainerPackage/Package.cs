using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class Package : Customs.Business.BasePackage, Integration.Customs.AU.IPackage
	{
		public Package(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : Customs.Business.BasePackage.Schema
		{
			public const string CW_CargoStatus = "CW_CargoStatus";
		}
		#endregion

		#region CW_CargoStatus
		public virtual ZString CW_CargoStatus
		{
			get
			{
				return PackingGroup != null ? PackingGroup.AbbreviatedCargoStatusDescription : ZString.Empty;
			}
		}

		public ZPropertyInfo CW_CargoStatusInfo
		{
			get { return GetZPropertyInfo(Schema.CW_CargoStatus); }
		}
		#endregion

		[BusinessObjectTestExclude]
		public override ZString CW_PackType
		{
			get
			{
				var result = base.CW_PackType;
				if (result.IsEmpty && Bill != null)
				{
					result = Bill.CU_PackType;
				}

				if (result.IsEmpty)
				{
					result = Declaration != null && !Declaration.JE_TotalNoOfPacksPackType.IsEmpty ? Declaration.JE_TotalNoOfPacksPackType : (ZString)Core.Constants.PkgUnit.Package;
				}

				return result;
			}

			set { base.CW_PackType = value; }
		}

		public override ZInt CW_InBondPackQty
		{
			get => base.CW_InBondPackQty;
			set
			{
				var hasChange = CW_InBondPackQty != value;
				base.CW_InBondPackQty = value;
				if (hasChange && !IsCopying && PackingGroup != null)
				{
					PackingGroup.Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZInt CW_PackQty
		{
			get => base.CW_PackQty;
			set
			{
				var hasChange = CW_PackQty != value;
				base.CW_PackQty = value;
				if (hasChange && !IsCopying && PackingGroup != null)
				{
					PackingGroup.Packages.MarkAsNeedingValidation();
				}
			}
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new PackingGroup PackingGroup
		{
			get { return (PackingGroup)base.PackingGroup; }
		}

		protected override bool ShouldDeleteIfPackQtyIsEmpty
		{
			get { return false; }
		}

		protected override Customs.Business.CusDecHouseContainerPackLookups GetNewLookups()
		{
			return new PackageLookups(this);
		}

		public new PackageLookups Lookups
		{
			get { return (PackageLookups)base.Lookups; }
		}

		public new PackageValidation Validation
		{
			get { return (PackageValidation)base.Validation; }
		}

		protected override Customs.Business.CusDecHouseContainerPackValidation GetNewValidation()
		{
			return new PackageValidation(this);
		}

		public override void Delete()
		{
			PackingGroup packingGroup = PackingGroup;

			base.Delete();

			if (!IsSynchronising)
			{
				if (packingGroup != null && !packingGroup.Packages.HasAtLeastOneOtherElementBesidesThisOne(this))
				{
					packingGroup.Delete();
				}
			}
		}
	}
}
