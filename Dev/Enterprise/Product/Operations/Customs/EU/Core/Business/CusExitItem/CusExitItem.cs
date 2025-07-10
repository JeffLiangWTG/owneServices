using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.Business
{
	[DependentBusinessObject(typeof(CusExitDetail), "CusExitItems")]
	public class CusExitItem : AutoCusExitItem, Integration.Customs.EU.ICusExitItem
	{
		public CusExitItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusExitItemTypeDecider TypeDecider = new CusExitItemTypeDecider();

		public CusExitDetail CusExitDetail => Factory.Load<CusExitDetail>(CXI_CED);

		public CusExitControlHeader Header => CusExitDetail.Header;

		[ResourceStringData("99498C41-F3F5-4C1E-B7EB-B0F3C7EB0505", Caption = "Line No")]
		public override ZShort CXI_LineNumber { get => base.CXI_LineNumber; set => base.CXI_LineNumber = value; }

		[ResourceStringData("7B5CA8EF-536E-471D-A9E3-E9FCF7F73292", Caption = "Gross Mass")]
		public override ZDecimal CXI_GrossMass { get => base.CXI_GrossMass; set => base.CXI_GrossMass = value; }

		[ResourceStringData("96D4528B-4CCD-447E-B161-0DB25633C44A", Caption = "Gross Mass UQ")]
		[List(nameof(Lookups) + "." + nameof(CusExitItemLookups.WeightUQList))]
		public override ZString CXI_GrossMassUQ { get => base.CXI_GrossMassUQ; set => base.CXI_GrossMassUQ = value; }

		[ResourceStringData("614F7396-1171-4289-AC3B-099D12A00CAB", Caption = "Net Mass")]
		public override ZDecimal CXI_NetMass { get => base.CXI_NetMass; set => base.CXI_NetMass = value; }

		[ResourceStringData("B59B36EA-7B2A-4D9F-B085-80A69377DBD2", Caption = "Net Mass UQ")]
		[List(nameof(Lookups) + "." + nameof(CusExitItemLookups.WeightUQList))]
		public override ZString CXI_NetMassUQ { get => base.CXI_NetMassUQ; set => base.CXI_NetMassUQ = value; }

		[ResourceStringData("52A15E6A-25A3-4949-A2F7-9E7759769EAC", Caption = "Status")]
		[List(nameof(Lookups) + "." + nameof(CusExitItemLookups.StatusList))]
		public override ZString CXI_Status { get => base.CXI_Status; set => base.CXI_Status = value; }

		[ChildEditable(true)]
		public virtual CusExitItemPackageCollection Packages
		{
			get
			{
				if (packages == null)
				{
					packages = CreateNewPackagesCollection();
					packages.Load();
					RegisterEditableChildObject(packages);
				}

				return packages;
			}
		}
		CusExitItemPackageCollection packages;

		protected virtual CusExitItemPackageCollection CreateNewPackagesCollection() => new CusExitItemPackageCollection(this);

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			CXI_Status = ExitItemStatusList.Codes.UNK;
			CXI_LineNumber = 1;
		}
#endif
	}
}
