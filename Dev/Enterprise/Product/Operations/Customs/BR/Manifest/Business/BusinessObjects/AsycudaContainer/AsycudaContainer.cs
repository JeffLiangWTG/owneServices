using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.BRManifest.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ManifestBase.AutoAsycudaContainer.Schema
		{
			public const string TareWeight = "TareWeight";
		}

		[ResourceStringData("BR.Manifest.Business.AsycudaContainer.TareWeight", Caption = "Tare Weight")]
		public ZDecimal TareWeight => ContainerType?.RC_TareWeight ?? ZDecimal.Zero;
	}
}
