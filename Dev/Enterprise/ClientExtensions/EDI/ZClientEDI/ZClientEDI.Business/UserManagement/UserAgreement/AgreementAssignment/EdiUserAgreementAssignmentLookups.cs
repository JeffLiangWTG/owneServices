//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiUserAgreementAssignmentLookups
//
//    This class should be used for overriding collections in AutoEdiUserAgreementAssignmentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAssignmentLookups : AutoEdiUserAgreementAssignmentLookups
	{
		public EdiUserAgreementAssignmentLookups(AutoEdiUserAgreementAssignment parent) : base(parent)
		{
			this.parent = (EdiUserAgreementAssignment)parent;
		}

		readonly EdiUserAgreementAssignment parent;

		public ReadOnlyCodeDescriptionPairList Variants => EdiUserAgreementLookups.GetVariantList(Factory, parent.EAE_AgreementType);

		public CodeDescriptionPairList AgreementTypes => EdiUserAgreementTypesMapper.GetParentAvailableAgreementTypeList(parent.EAE_ParentTableCode);

		public CodeDescriptionPairList ParentTypes => new CodeDescriptionPairList
		{
			new CodeDescriptionPair(LicenceEnterpriseSchema.Constants.Prefix, EnterpriseLicenceParentDescription),
			new CodeDescriptionPair(LicenceDatabaseSchema.Constants.Prefix, DatabaseParentDescription)
		};

		public static MultilingualString EnterpriseLicenceParentDescription = ResString.GetMultilingualString("d3ff39cd-0f0b-4cfb-8e45-d3419ec9c8ee", "Enterprise License");
		public static MultilingualString DatabaseParentDescription = ResString.GetMultilingualString("0972c8ab-38ba-47ce-b6c4-f8048f03e754", "Database");

		public BusinessObjectCollection<LicenceDatabase> LicenceDatabaseParents
		{
			get
			{
				if (parent.EnterpriseParent == null)
				{
					var collection = new LicenceDatabaseNonDependentCollection(Factory);
					collection.Load();
					return collection;
				}

				return Factory.GetCachedValue("LicenceDatabaseParents" + parent.EAE_AgreementType + parent.EnterpriseParent.PK, () =>
				{
					var additionalFilter = new ZQuery();
					if (parent.EAE_AgreementType.EqualsIgnoringCase(EdiUserAgreementTypes.Codes.CargoWiseNext))
					{
						additionalFilter.AddToFilter(LicenceDatabaseSchema.LD_Product, ProductTypes.CargoWiseNextAgreementCompatibleProducts);
					}

					var collectionWithParent = new LicenceDatabaseCollection(parent.EnterpriseParent, additionalFilter);
					collectionWithParent.Load();
					return collectionWithParent;
				});
			}
		}

		public bool IsPermittedParentTableCode(string parentTableCode)
		{
			return ParentTypes.ContainsCode(parentTableCode);
		}
	}
}

