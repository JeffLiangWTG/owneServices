using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	[AllowPublicConstructor, AllowNoStaticNew]
	public class DocBillOfLadingBodySection : DocumentWrapper, IObsoleteValidation
	{
		public DocBillOfLadingBodySection(BusinessObjectFactory factory)
			: base(null, factory)
		{
		}

		public override string ToString()
		{
			return GetType().ToString();
		}

		public ZBool IsEmpty
		{
			get
			{
				return (
					this.MarksAndNumbers.IsEmpty &&
					this.Packages.IsEmpty &&
					this.GoodsDescription.IsEmpty &&
					this.Weight.IsEmpty &&
					this.Volume.IsEmpty &&
					this.ContainerNumbers.IsEmpty &&
					this.ContainerSeals.IsEmpty &&
					this.ContainerTypes.IsEmpty &&
					this.ContainerWeights.IsEmpty &&
					this.ContainerVolumes.IsEmpty &&
					this.ContainerPackages.IsEmpty &&
					this.ContainerModes.IsEmpty
					);
			}
		}

		#region Properties

		public ZString MarksAndNumbers { get; set; }
		public ZString GoodsDescription { get; set; }
		public ZString Packages { get; set; }
		public ZString Weight { get; set; }
		public ZString Volume { get; set; }
		public ZString ContainerNumbers { get; set; }
		public ZString ContainerSeals { get; set; }
		public ZString ContainerTypes { get; set; }
		public ZString ContainerWeights { get; set; }
		public ZString ContainerVolumes { get; set; }
		public ZString ContainerPackages { get; set; }
		public ZString ContainerModes { get; set; }

		#endregion
	}
}
