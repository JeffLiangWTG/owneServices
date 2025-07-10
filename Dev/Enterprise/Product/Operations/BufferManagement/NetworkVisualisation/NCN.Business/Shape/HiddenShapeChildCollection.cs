using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	abstract class HiddenShapeChildCollection<TChild> : ImpObservableCollection<TChild>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "I don't have time to fix this right now.")]
		protected HiddenShapeChildCollection(IShapeNetworkEntity owner)
		{
			Argument.NotNull(owner, "owner");
			this.ownerShape = owner;

			Build();
		}

		readonly IShapeNetworkEntity ownerShape;

		protected IShapeNetworkEntity OwnerShape
		{
			get { return ownerShape; }
		}

		protected ProcessJobHeader JobHeader
		{
			get { return ownerShape.ProcessHeader as ProcessJobHeader; }
		}

		protected override void OnReloading(AllSelectedEntities reloader)
		{
			base.OnReloading(reloader);
			Build();
		}

		void Build()
		{
			if (!ownerShape.Shape.IsDeleted && JobHeader != null)
			{
				AddRange(GetHiddenElements());
			}
		}

		protected abstract IEnumerable<TChild> GetHiddenElements();
	}
}
