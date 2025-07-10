using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaContainerValidation : ManifestBase.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent) : base(parent)
		{
		}

		protected new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		protected override void CheckACN_ContainerNumber()
		{
			base.CheckACN_ContainerNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_ContainerNumberInfo);
			ContainerNumberValidation.WarnIfInvalid(Parent.ACN_ContainerNumberInfo);
			if (CheckContainerLinkToPack)
			{
				var foundPack = Parent?.Header?.Bills.Cast<AsycudaBill>().SelectMany(b => b.Packs.Cast<AsycudaPack>()).Any(p => p.ContainerPK == Parent.PK) ?? false;
				if (!foundPack)
				{
					Parent?.ACN_ContainerNumberInfo.AddWarning(Res.GetString("5d7c7574-1296-40a2-a42a-b466f80bf2d4", "This container does not appear on any pack lines."));
				}
			}
		}

		protected virtual bool CheckContainerLinkToPack => true;

		protected override void CheckACN_Seal1()
		{
			base.CheckACN_Seal1();
			if (Parent.ACN_Seal1.IsEmpty && (!Parent.ACN_Seal2.IsEmpty || !Parent.ACN_Seal3.IsEmpty))
			{
				Parent.ACN_Seal1Info.AddMessageError(Res.GetString("40d98e7c-42bc-4562-bdb7-d0ca280d107a", "Please complete seal 1 before seal 2 or 3 are entered"));
			}

			ZZValidationHelper.CheckIsMandatoryFor(Parent.ACN_Seal1Info, Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.Seal);
		}

		protected override void CheckACN_SealingPartyType()
		{
			base.CheckACN_SealingPartyType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ACN_SealingPartyTypeInfo);
		}

		protected override void CheckACN_SealType1()
		{
			base.CheckACN_SealType1();
			ListValidation.ErrorIfInvalidCode(Parent.ACN_SealType1Info);
		}

		protected override void CheckACN_EmptyFullIndicator()
		{
			base.CheckACN_EmptyFullIndicator();
			var info = Parent.ACN_EmptyFullIndicatorInfo;
			ListValidation.MessageErrorIfInvalidCode(info);
			MandatoryValidation.MessageErrorIfNotEntered(info);
			var header = Parent.Header;
			if (header != null)
			{
				if (header.AMA_IsBuyersConsolidation && Parent.ACN_EmptyFullIndicator != EmptyFullIndicatorList.Codes.FullContainerLoad)
				{
					info.AddMessageError(Res.GetString("7b42d32c-1918-455c-904c-cac445c5f06e", "When Buyers Consolidation is selected only 'Full Container Load' is allowed"));
				}

				if (Parent.ACN_EmptyFullIndicator != EmptyFullIndicatorList.Codes.EmptyContainer)
				{
					ZZValidationHelper.CheckIsMandatoryWhenAttributeMatches(info,
						Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.ContainerEmptyFullIndicator,
						Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.MandatoryForManifestType,
						header.AMA_ManifestType);
				}
			}
		}

		protected override void CheckACN_RC_ContainerType()
		{
			base.CheckACN_RC_ContainerType();
			ListValidation.ErrorIfInvalidPK(Parent.ACN_RC_ContainerTypeInfo);
			MandatoryValidationOfACN_RC_ContainerTypeCore();
		}

		protected virtual void MandatoryValidationOfACN_RC_ContainerTypeCore()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_RC_ContainerTypeInfo);
		}

		protected override void CheckACN_GoodsWeightUQ()
		{
			base.CheckACN_GoodsWeightUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ACN_GoodsWeightUQInfo);
		}

		protected override void CheckACN_Seal2()
		{
			base.CheckACN_Seal2();

			if (Parent.ACN_Seal2.IsEmpty)
			{
				if (!Parent.ACN_Seal3.IsEmpty)
				{
					Parent.ACN_Seal2Info.AddMessageError(Res.GetString("e7ea4880-6310-471a-bb0d-829e73439c41", "Please complete seal 2 before seal 3 is entered"));
				}
			}
			else if (Parent.ACN_Seal1.IsEmpty)
			{
				Parent.ACN_Seal2Info.AddMessageError(Res.GetString("23b2d3e6-67fd-467a-a7a0-f1a86fe43b3b", "Please complete seal 1 before seal 2 is entered"));
			}
		}

		protected override void CheckACN_SealingPartyType2()
		{
			base.CheckACN_SealingPartyType2();
			ListValidation.MessageErrorIfInvalidCode(Parent.ACN_SealingPartyType2Info);
		}

		protected override void CheckACN_SealType2()
		{
			base.CheckACN_SealType2();
			ListValidation.ErrorIfInvalidCode(Parent.ACN_SealType2Info);
		}

		protected override void CheckACN_Seal3()
		{
			base.CheckACN_Seal3();
			if (!Parent.ACN_Seal3.IsEmpty && (Parent.ACN_Seal1.IsEmpty || Parent.ACN_Seal2.IsEmpty))
			{
				Parent.ACN_Seal3Info.AddMessageError(Res.GetString("81044342-3a55-4206-9ae1-3a3b89dda551", "Please complete seal 1 and 2 before seal 3 is entered"));
			}
		}

		protected override void CheckACN_SealingPartyType3()
		{
			base.CheckACN_SealingPartyType3();
			ListValidation.MessageErrorIfInvalidCode(Parent.ACN_SealingPartyType3Info);
		}

		protected override void CheckACN_SealType3()
		{
			base.CheckACN_SealType3();
			ListValidation.ErrorIfInvalidCode(Parent.ACN_SealType3Info);
		}

		protected override void CheckACN_Seal1UnloadingState()
		{
			base.CheckACN_Seal1UnloadingState();
			ListValidation.ErrorIfInvalidCode(Parent.ACN_Seal1UnloadingStateInfo);
		}

		protected override void CheckACN_Seal2UnloadingState()
		{
			base.CheckACN_Seal2UnloadingState();
			ListValidation.ErrorIfInvalidCode(Parent.ACN_Seal2UnloadingStateInfo);
		}

		protected override void CheckACN_Seal3UnloadingState()
		{
			base.CheckACN_Seal3UnloadingState();
			ListValidation.ErrorIfInvalidCode(Parent.ACN_Seal3UnloadingStateInfo);
		}

		ZZDatabaseValidationHelper ZZValidationHelper => zzValidationHelper ??= Parent.Header?.ZZValidationHelper ?? ZZDatabaseValidationHelper.GetDefaultValidationHelper(Parent.Factory);
		ZZDatabaseValidationHelper zzValidationHelper;
	}
}
