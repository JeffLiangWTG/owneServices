using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOneOffContainer : DocBaseWrapper
	{
		DocOneOffContainer(RateOneOffContainers oneOffContainer, BusinessObjectFactory factoryToWrap)
			: base(oneOffContainer, factoryToWrap)
		{
		}

		public static DocOneOffContainer New(RateOneOffContainers oneOffContainer, BusinessObjectFactory factoryToWrap)
		{
			return oneOffContainer != null ? new DocOneOffContainer(oneOffContainer, factoryToWrap) : null;
		}

		RateOneOffContainers OneOffContainer
		{
			get { return (RateOneOffContainers)WrappedObject; }
		}

		public override string ToString()
		{
			return Code;
		}

		#region Properties

		public ZInt Count
		{
			get { return OneOffContainer.TC_ContainerCount; }
		}

		#region Container Info

		public ZString Code
		{
			get { return OneOffContainer.Container != null ? OneOffContainer.Container.RC_Code : ZString.Empty; }
		}

		#endregion

		#region Loose Cargo (It is obsolete and replaced by DocOneOffPackLine. Keep it as is for backward compatibility )

		public ZString Height => ZString.Empty;

		public ZString Width => ZString.Empty;

		public ZString Length => ZString.Empty;

		#endregion

		#endregion
	}
}
