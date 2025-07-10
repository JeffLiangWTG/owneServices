using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Declaration.Wizard.CDSFSD;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSFinalSupplementaryDeclarationHelper : AutoCDSFinalSupplementaryDeclarationHelper
	{
		public CDSFinalSupplementaryDeclarationHelper(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		[List(nameof(Lookups) + "." + nameof(CDSFinalSupplementaryDeclarationHelperLookups.AuthorisationTypeList))]
		public override ZString AuthorisationType { get => base.AuthorisationType; set => base.AuthorisationType = value; }

		#region Declarant Organisation and Address

		[RelatedBusinessObject("DeclarantOrgAddress")]
		[List(nameof(Lookups) + "." + nameof(CDSFinalSupplementaryDeclarationHelperLookups.DeclarantList))]
		public override ZGuid DeclarantAddressPK { get => base.DeclarantAddressPK; set => base.DeclarantAddressPK = value; }

		public virtual OrgAddress DeclarantOrgAddress => Factory.Load<OrgAddress>(DeclarantAddressPK);

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress DeclarantAddressPK_ZAddress => declarantZAddress ?? (declarantZAddress = GetNewDeclarantZAddress());
		ZAddress declarantZAddress;

		ZAddress GetNewDeclarantZAddress() => new ZAddress(DeclarantAddressPKInfo);

		public virtual bool IsDeclarantAddressRequired => true;

		#endregion

		#region Authorisation Holder Organisation and Address

		[RelatedBusinessObject("AuthorisationHolderAddress")]
		[List(nameof(Lookups) + "." + nameof(CDSFinalSupplementaryDeclarationHelperLookups.AuthorisationHolderList))]
		public override ZGuid AuthorisationHolderAddressPK { get => base.AuthorisationHolderAddressPK; set => base.AuthorisationHolderAddressPK = value; }

		public virtual OrgAddress AuthorisationHolderAddress => Factory.Load<OrgAddress>(AuthorisationHolderAddressPK);

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress AuthorisationHolderAddressPK_ZAddress => authorisationHolderZAddress ?? (authorisationHolderZAddress = GetNewAuthorisationHolderZAddress());
		ZAddress authorisationHolderZAddress;

		ZAddress GetNewAuthorisationHolderZAddress() => new ZAddress(AuthorisationHolderAddressPKInfo);

		public virtual bool IAuthorisationHolderAddressRequired => true;

		#endregion

		#region Importer Organisation and Address

		[RelatedBusinessObject("ImporterHeader")]
		[List(nameof(Lookups) + "." + nameof(CDSFinalSupplementaryDeclarationHelperLookups.ImporterList))]
		public override ZGuid ImporterPK { get => base.ImporterPK; set => base.ImporterPK = value; }

		public virtual OrgHeader ImporterHeader => Factory.Load<OrgHeader>(ImporterPK);

		public OrgHeader ImporterPK_OrgHeader => importerOrg ?? (importerOrg = GetNewImporterOrgHeader());
		OrgHeader importerOrg;

		OrgHeader GetNewImporterOrgHeader() => Factory.New<OrgHeader>();

		public virtual bool IImporterAddressRequired => true;

		#endregion

		public CDSFinalSupplementaryDeclarationHelperLateChildCollection CDSFinalSupplementaryDeclarationHelperLateChildCollection => cdsFinalSupplementaryDeclarationHelperLateChildCollection ?? (cdsFinalSupplementaryDeclarationHelperLateChildCollection = new CDSFinalSupplementaryDeclarationHelperLateChildCollection(this));
		CDSFinalSupplementaryDeclarationHelperLateChildCollection cdsFinalSupplementaryDeclarationHelperLateChildCollection;

		#region Lookups

		public CDSFinalSupplementaryDeclarationHelperLookups Lookups
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
		CDSFinalSupplementaryDeclarationHelperLookups fLookups;

		protected virtual CDSFinalSupplementaryDeclarationHelperLookups GetNewLookups() => new CDSFinalSupplementaryDeclarationHelperLookups(declaration);

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			var today = ZDate.Today;
			DueDate = new ZDate(today.Year, today.Month, 1);
			StartOfPeriod = DueDate.AddMonths(-1);
			DeclarantAddressPK = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			AuthorisationHolderAddressPK = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			AuthorisationType = CDSAuthorisationHeaderTypeList.Codes.SimplifiedDeclaration;
		}
	}
}
