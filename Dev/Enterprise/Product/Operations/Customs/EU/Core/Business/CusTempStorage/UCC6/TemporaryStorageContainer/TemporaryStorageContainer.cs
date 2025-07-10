using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[CodeProperty(Schema.ACN_ContainerNumber), DescriptionProperty("Description")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class TemporaryStorageContainer : AsycudaContainer, ICusSealSequenceNumberGeneratorProvider, Integration.Customs.EU.ITemporaryStorageContainer, ICusSealCollectionSupporter, ICusSealTypeSupporter
	{
		public TemporaryStorageContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

		protected virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return ReadOnly || (Header?.IsNoEditAllowedCustomsStatus() ?? false);
		}

		public ZString Description => ZString.Format((NoResString)"{0} {1}PK {2}", ACN_ContainerNumber, ACN_NumberOfPackages, ACN_CommodityCode);

		public new TemporaryStorageContainerLookups Lookups => (TemporaryStorageContainerLookups)base.Lookups;

		protected override AsycudaContainerLookups GetNewLookups() => new TemporaryStorageContainerLookups(this);

		protected override AsycudaContainerValidation GetNewValidation()
		{
			return new TemporaryStorageContainerValidation(this);
		}

		public new TemporaryStorageContainerValidation Validation
		{
			get { return (TemporaryStorageContainerValidation)GetNewValidation(); }
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
					AdditionalSeals.RefreshBinding();
				}
			}
		}

		[ChildEditable]
		public CusSealCollection AdditionalSeals
		{
			get
			{
				if (additionalSeals == null)
				{
					additionalSeals = GetCusSealCollection();
					RegisterEditableChildObject(additionalSeals);
				}
				return additionalSeals;
			}
		}
		CusSealCollection additionalSeals;

		protected virtual CusSealCollection GetCusSealCollection() => new CusSealCollection(this);

		Type ICusSealTypeSupporter.CusSealType => typeof(Declaration.CusSeal);

		public override void Delete()
		{
			AdditionalSeals.DeleteAll();
			base.Delete();
		}

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageContainerLookups.EmptyFullIndicatorList))]
		public override ZString ACN_EmptyFullIndicator
		{
			get => base.ACN_EmptyFullIndicator;
			set => base.ACN_EmptyFullIndicator = value;
		}

		ShortSequenceNumberGenerator sealsSequenceNumberGeneratorCache;
		internal ShortSequenceNumberGenerator SealsSequenceNumberGenerator => sealsSequenceNumberGeneratorCache ?? (sealsSequenceNumberGeneratorCache = new ShortSequenceNumberGenerator(() => AdditionalSeals));

		ShortSequenceNumberGenerator ICusSealSequenceNumberGeneratorProvider.SequenceNumberGenerator => SealsSequenceNumberGenerator;

		public ZBool AllowNewCusSeal => !ACN_Seal1.IsEmpty && !ACN_Seal2.IsEmpty && !ACN_Seal3.IsEmpty;

		protected override bool SupportsCloneCore() => true;
	}
}
