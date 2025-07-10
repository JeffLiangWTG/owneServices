using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Business.Wizards.EIDR
{
	public class EidrWizard : AutoEidrWizard
	{
		public EidrWizard(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		[List(nameof(Lookups) + "." + nameof(EidrWizardLookups.CPCList))]
		public override ZString CPC { get => base.CPC; set => base.CPC = value; }

		[List(nameof(Lookups) + "." + nameof(EidrWizardLookups.PackageTypeList))]
		public override ZString TypeOfPackages { get => base.TypeOfPackages; set => base.TypeOfPackages = value; }

		[List(nameof(Lookups) + "." + nameof(EidrWizardLookups.TransportTypeList))]
		public override ZString TransportMode { get => base.TransportMode; set => base.TransportMode = value; }

		[List(nameof(Lookups) + "." + nameof(EidrWizardLookups.CountryList))]
		public override ZString CountryOfOrigin { get => base.CountryOfOrigin; set => base.CountryOfOrigin = value; }

		[List(nameof(Lookups) + "." + nameof(EidrWizardLookups.EIDRTypeList))]
		public override ZString EIDRType { get => base.EIDRType; set => base.EIDRType = value; }

		#region Warehouse Organisation and Address

		[RelatedBusinessObject("WarehouseAddressList")]
		[List(nameof(Lookups) + "." + nameof(EidrWizardLookups.WarehouseList))]
		public override ZGuid WarehouseOrgAddressPk { get => base.WarehouseOrgAddressPk; set => base.WarehouseOrgAddressPk = value; }

		public virtual OrgAddress WarehouseAddressList => Factory.Load<OrgAddress>(WarehouseOrgAddressPk);

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress WarehouseOrgAddressPk_ZAddress => warehouseOrgZAddress ?? (warehouseOrgZAddress = GetNewWarehouseOrgZAddress());
		ZAddress warehouseOrgZAddress;

		ZAddress GetNewWarehouseOrgZAddress() => new ZAddress(WarehouseOrgAddressPkInfo);

		public OrgAddress WarehouseOrgAddressList => Factory.Load<OrgAddress>(base.WarehouseOrgAddressPk);

		public virtual bool IsWarehouseAddressRequired => true;

		#endregion

		#region Declarant Organisation and Address

		[RelatedBusinessObject("DeclarantAddressList")]
		[List(nameof(Lookups) + "." + nameof(EidrWizardLookups.DeclarantList))]
		public override ZGuid DeclarantOrgAddressPk { get => base.DeclarantOrgAddressPk; set => base.DeclarantOrgAddressPk = value; }

		public virtual OrgAddress DeclarantAddressList => Factory.Load<OrgAddress>(DeclarantOrgAddressPk);

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress DeclarantOrgAddressPk_ZAddress => declarantOrgZAddress ?? (declarantOrgZAddress = GetNewDeclarantOrgZAddress());
		ZAddress declarantOrgZAddress;

		ZAddress GetNewDeclarantOrgZAddress() => new ZAddress(DeclarantOrgAddressPkInfo);

		public OrgAddress DeclarantOrgAddress => Factory.Load<OrgAddress>(base.DeclarantOrgAddressPk);

		public virtual bool IsDeclarantAddressRequired => true;

		#endregion

		#region Lookups

		public EidrWizardLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}
		EidrWizardLookups fLookups;

		protected virtual EidrWizardLookups GetNewLookups() => new EidrWizardLookups(declaration);

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPC = "4000000";
			DateOfImport = ZDate.Today;
			DeclarantOrgAddressPk = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			SupplementaryDeclarationDueDate = DateOfImport.AddMonths(6);
		}
	}
}
