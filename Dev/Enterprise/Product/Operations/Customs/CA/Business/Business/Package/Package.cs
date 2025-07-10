using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class Package : BasePackage, Integration.Customs.CA.IPackage
	{
		public Package(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZGuid CW_CW_Parent
		{
			get => base.CW_CW_Parent;
			set
			{
				base.CW_CW_Parent = value;
				Declaration.Packages.MarkAsNeedingValidation();
			}
		}

		public override CodeDescriptionPairList PackTypeList
		{
			get
			{
				var declaration = Declaration as JobDeclaration;
				if (declaration != null && declaration.IsIID)
				{
					return UNPackTypeList;
				}
				else
				{
					return Factory.GetCachedValue<ACROSSPackageTypes>();
				}
			}
		}

		public CodeDescriptionPairList UNPackTypeList
		{
			get
			{
				return Enterprise.Customs.Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, Enterprise.Customs.CA.Business.UniversalReferenceConstants.UNPackTypeStartDate);
			}
		}

		public new PackageValidation Validation
		{
			get { return (PackageValidation)base.Validation; }
		}

		protected override CusDecHouseContainerPackValidation GetNewValidation()
		{
			return new PackageValidation(this);
		}

		protected override ZString FreightPackageTypeCore
		{
			get { return ACROSSPackageTypes.ToFreightPackageType(CW_PackType); }
		}

		protected override void MarkAsNeedingValidationCore()
		{
			base.MarkAsNeedingValidationCore();
			Validation.ValidateAtLeastOneInvoiceOrInvoiceLineLinked();
		}
	}
}
