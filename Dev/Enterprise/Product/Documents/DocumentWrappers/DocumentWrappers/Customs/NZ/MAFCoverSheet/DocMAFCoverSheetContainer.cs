using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class DocMAFCoverSheetContainer : DocBaseWrapper
	{
		public DocMAFCoverSheetContainer(NZDocsMAFCSContainer container, BusinessObjectFactory factory, int containerReference)
			: base(container, factory)
		{
			if (container != null)
			{
				containerNumber = container.D2_ContainerNumber;
				isFCL = container.D2_IsFCL;
				isLCL = container.D2_IsLCL;
			}
			this.containerReference = containerReference;
		}
		readonly ZString containerNumber;
		readonly ZBool isFCL;
		readonly ZBool isLCL;

		public ZString ContainerNumber
		{
			get { return containerNumber; }
		}

		public ZBool IsFCL
		{
			get { return isFCL; }
		}

		public ZBool IsLCL
		{
			get { return isLCL; }
		}

		public ZInt ContainerReference
		{
			get { return containerReference; }
			set { containerReference = value; }
		}
		ZInt containerReference;
	}
}
