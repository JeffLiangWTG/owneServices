using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOneOffPackLine : DocBaseWrapper
	{
		DocOneOffPackLine(RateOneOffPackLine oneOffPackLine, BusinessObjectFactory factoryToWrap)
			: base(oneOffPackLine, factoryToWrap)
		{
		}

		public static DocOneOffPackLine New(RateOneOffPackLine oneOffPackLine, BusinessObjectFactory factoryToWrap)
		{
			return oneOffPackLine != null ? new DocOneOffPackLine(oneOffPackLine, factoryToWrap) : null;
		}

		RateOneOffPackLine OneOffPackLine
		{
			get { return (RateOneOffPackLine)WrappedObject; }
		}

		public override string ToString()
		{
			return PackLineCode;
		}

		#region Properties

		public ZInt PackLineCount
		{
			get { return OneOffPackLine.TPL_PackLineCount; }
		}

		public ZString PackLineCode
		{
			get { return OneOffPackLine.RefContainer != null ? OneOffPackLine.RefContainer.RC_Code : ZString.Empty; }
		}

		#region Loose Cargo

		public ZString Height
		{
			get { return OneOffPackLine.TPL_Height.ToStringTrimZeros() + " " + OneOffPackLine.TPL_DimensionUQ; }
		}

		public ZString Width
		{
			get { return OneOffPackLine.TPL_Width.ToStringTrimZeros() + " " + OneOffPackLine.TPL_DimensionUQ; }
		}

		public ZString Length
		{
			get { return OneOffPackLine.TPL_Length.ToStringTrimZeros() + " " + OneOffPackLine.TPL_DimensionUQ; }
		}

		#endregion

		#endregion
	}
}
