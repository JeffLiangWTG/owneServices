using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.ARManifest.IAsycudaManifestHeader, IMessageAttachee
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const int RegistrationNumberMaxLength = 16;
		}

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Argentina;
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override bool RegistrationDetails_ReadOnly => !IsSea;
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var oldValue = AMA_TransportMode;
				base.AMA_TransportMode = value;
				if (oldValue != AMA_TransportMode)
				{
					Containers.MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(Schema.RegistrationNumberMaxLength)]
		public override ZString RegistrationNumber
		{
			get => base.RegistrationNumber;
			set
			{
				var oldValue = RegistrationNumber;
				CheckMaximumLength(RegistrationNumberInfo, value);
				base.RegistrationNumber = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRegistrationNumber();
					Validation.ValidateRegistrationDate();
				}
				RegistrationNumberInfo.RefreshBinding(oldValue);
			}
		}

		#region IManifestMessageAttachee

		ZGuid IMessageAttachee.GlobalBranchPK => AMA_GB;
		IBusinessObjectCollection IMessageAttachee.Messages => Messages;
		ZString IMessageAttachee.TableName => AsycudaManifestHeaderSchema.Constants.TableName;
		ZString IMessageAttachee.JobReference => AMA_JobReference;

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = ARManifestTypes.Codes.MAN;
		}
#endif
	}
}
