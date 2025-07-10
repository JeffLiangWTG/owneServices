using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public interface IAsycudaContainerCollection<out TContainer, out TManifestHeader> : IDependentBusinessObjectCollection, IBusinessObjectCollection<TContainer>, IBusinessObjectCollection
		where TContainer : AsycudaContainer
		where TManifestHeader : AsycudaManifestHeader
	{
		new TContainer this[int i] { get; }
		new TManifestHeader Master { get; }
		IDisposable SuspendSettingHasChanges();
		new void Remove(BusinessObject container);
		new TContainer AddNew();
		TContainer AddNew(Type bizOType);
	}

	public class AsycudaContainerCollection<TContainer, TManifestHeader> : DependentBusinessObjectCollection<TContainer, TManifestHeader>, IAsycudaContainerCollection<TContainer, TManifestHeader>
		where TContainer : AsycudaContainer
		where TManifestHeader : AsycudaManifestHeader
	{
		public AsycudaContainerCollection(TManifestHeader master)
			: base(master)
		{ }

		public AsycudaContainerCollection(TManifestHeader master, ZQuery additionalQuery)
			: base(master, additionalQuery)
		{ }

		public IEnumerator<TContainer> GetEnumerator() => Elements.Cast<TContainer>().GetEnumerator();

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => Master.GetContainerType();
		protected override string FkColumnName => AsycudaContainer.Schema.ACN_AMA_Manifest;
	}
}
