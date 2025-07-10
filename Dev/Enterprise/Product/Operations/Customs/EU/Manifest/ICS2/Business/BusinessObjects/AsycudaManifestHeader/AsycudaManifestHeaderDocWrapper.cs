using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaManifestHeaderDocWrapper : ASYCUDA.Business.AsycudaManifestHeaderDocWrapper
	{
		public AsycudaManifestHeaderDocWrapper(ASYCUDA.Business.AsycudaManifestHeader manifestHeader)
			: base(manifestHeader)
		{
		}

		public new AsycudaManifestHeader Manifest => (AsycudaManifestHeader)base.Manifest;

		public AsycudaBillCollection Bills => Manifest.Bills;

		public Image CompanyLogo => SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);

		public ZString ETD => Manifest.AMA_E_DEP.ToBestReadableDateString();

		public ZString RegistrationDate => Manifest.RegistrationDate.ToString();

		public BusinessObjectCollectionWrapper<AsycudaBillDocWrapper> AsycudaBillDocWrappers
		{
			get
			{
				var asycudaBillDocWrappers = new List<AsycudaBillDocWrapper>();

				foreach (AsycudaBill bill in Bills)
				{
					asycudaBillDocWrappers.Add(new AsycudaBillDocWrapper(bill));
				}

				return new BusinessObjectCollectionWrapper<AsycudaBillDocWrapper>(asycudaBillDocWrappers);
			}
		}
	}
}
