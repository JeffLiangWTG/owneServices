using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public sealed class AsycudaContainer
		: ASYCUDA.Business.AsycudaContainer,
		Integration.Customs.ASYCUDA.ILManifest.IAsycudaContainer,
		ICusSealCollectionSupporter,
		ICusSealSequenceNumberGeneratorProvider
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString ACN_Seal1
		{
			get => base.ACN_Seal1;
			set
			{
				var oldValue = base.ACN_Seal1;
				base.ACN_Seal1 = value;
				if (!IsCopying && oldValue != value)
				{
					AdditionalSeals.RefreshBinding();
				}
			}
		}

		public override ZString ACN_Seal2
		{
			get => base.ACN_Seal2;
			set
			{
				var oldValue = base.ACN_Seal2;
				base.ACN_Seal2 = value;
				if (!IsCopying && oldValue != value)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateACN_SealType2();
						Validation.ValidateACN_SealingPartyType2();
						Validation.ValidateACN_Seal2UnloadingState();
					}

					AdditionalSeals.RefreshBinding();
				}
			}
		}

		public override ZString ACN_Seal3
		{
			get => base.ACN_Seal3;
			set
			{
				var oldValue = base.ACN_Seal3;
				base.ACN_Seal3 = value;
				if (!IsCopying && oldValue != value)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateACN_SealType3();
						Validation.ValidateACN_SealingPartyType3();
						Validation.ValidateACN_Seal3UnloadingState();
					}

					AdditionalSeals.RefreshBinding();
				}
			}
		}

		[ResourceStringData("75539674-AC01-40B0-95CE-6D7ED94874B5", Caption = "Seal 1 Party", ShortCaption = "Seal 1 Party")]
		public override ZString ACN_SealingPartyType { get => base.ACN_SealingPartyType; set => base.ACN_SealingPartyType = value; }

		[ResourceStringData("BCFB011A-C9CD-41B4-9E65-0AA1489B786F", Caption = "Seal 2 Party", ShortCaption = "Seal 2 Party")]
		public override ZString ACN_SealingPartyType2 { get => base.ACN_SealingPartyType2; set => base.ACN_SealingPartyType2 = value; }

		[ResourceStringData("D28E7E42-4071-4293-95A6-3D6657C01EED", Caption = "Seal 3 Party", ShortCaption = "Seal 3 Party")]
		public override ZString ACN_SealingPartyType3 { get => base.ACN_SealingPartyType3; set => base.ACN_SealingPartyType3 = value; }

		[ChildEditable]
		public CusSealCollection AdditionalSeals
		{
			get
			{
				if (additionalSeals == null)
				{
					additionalSeals = new CusSealCollection(this);
					RegisterEditableChildObject(additionalSeals);
				}

				return additionalSeals;
			}
		}
		CusSealCollection additionalSeals;

		public ZBool AllowNewCusSeal => !ACN_Seal1.IsEmpty && !ACN_Seal2.IsEmpty && !ACN_Seal3.IsEmpty;

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.UnloadedStates))]
		public override ZString ACN_Seal1UnloadingState { get => base.ACN_Seal1UnloadingState; set => base.ACN_Seal1UnloadingState = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.UnloadedStates))]
		public override ZString ACN_Seal2UnloadingState { get => base.ACN_Seal2UnloadingState; set => base.ACN_Seal2UnloadingState = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.UnloadedStates))]
		public override ZString ACN_Seal3UnloadingState { get => base.ACN_Seal3UnloadingState; set => base.ACN_Seal3UnloadingState = value; }

		public override void Delete()
		{
			AdditionalSeals.DeleteAll();
			base.Delete();
		}

		public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;
		protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);

		public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;
		protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

		internal ShortSequenceNumberGenerator SealsSequenceNumberGenerator => sealsSequenceNumberGeneratorCache ?? (sealsSequenceNumberGeneratorCache = new ShortSequenceNumberGenerator(() => AdditionalSeals));
		ShortSequenceNumberGenerator sealsSequenceNumberGeneratorCache;

		ShortSequenceNumberGenerator ICusSealSequenceNumberGeneratorProvider.SequenceNumberGenerator => SealsSequenceNumberGenerator;
	}
}
