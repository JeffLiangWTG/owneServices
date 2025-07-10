using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusSupplyChainActorReferenceProvider
	{
		public static CusSupplyChainActorReferenceProvider GetByDataGroupingCode(ZString dataGroupingCode, string parentTableName = "")
		{
			CusSupplyChainActorReferenceProvider result = null;
			switch (parentTableName)
			{
				case AsycudaBillSchema.Constants.Prefix:
				case AsycudaPackedItemSchema.Constants.Prefix:
					result = new CusTempSupplyChainActorReferenceProvider(dataGroupingCode);
					break;
			}

			if (result == null && !dataGroupingCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("CusSupplyChainActorReferenceProviders");
				var objectHandle = (ObjectHandle)types[dataGroupingCode.ToString()];
				result = (CusSupplyChainActorReferenceProvider)objectHandle?.GetObject(dataGroupingCode);
			}

			return result ?? new CusSupplyChainActorReferenceProvider(dataGroupingCode);
		}

		protected CusSupplyChainActorReferenceProvider(ZString dataGroupingCode)
		{
			DataGroupingCode = dataGroupingCode;
		}

		public CusSupplyChainActorReferenceValidation GetNewValidation(CusSupplyChainActorReference reference) => GetNewValidationCore(reference);

		protected virtual CusSupplyChainActorReferenceValidation GetNewValidationCore(CusSupplyChainActorReference reference) => new CusSupplyChainActorReferenceValidation(reference);

		public ZString GetReferenceFromOwner(OrgHeader ownerOrg) => GetReferenceFromOwnerCore(ownerOrg);

		protected virtual ZString GetReferenceFromOwnerCore(OrgHeader ownerOrg)
		{
			var reference = ownerOrg?.GetEoriDetails() ?? ZString.Empty;
			if (reference.IsEmpty)
			{
				reference = ownerOrg.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);
			}
			return reference;
		}

		public ZString DataGroupingCode { get; }

		public ZString OverwrittenReferenceColumnCaption => OverwrittenReferenceColumnCaptionCore;

		protected virtual ZString OverwrittenReferenceColumnCaptionCore => ZString.Empty;

		public ZBool ReferenceColumnCasingToUpper => ReferenceColumnCasingToUpperCore;

		protected virtual ZBool ReferenceColumnCasingToUpperCore => ZBool.False;

		public IReadOnlyList<string> ColumnNamesInSortOrder => ColumnNamesInSortOrderCore;

		protected virtual IReadOnlyList<string> ColumnNamesInSortOrderCore => System.Array.Empty<string>();
	}
}
