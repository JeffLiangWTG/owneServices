using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class ClassComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var imoClass = wrapper?.IMOClass ?? ZString.Empty;

			if (!imoClass.IsEmpty)
			{
				string result = $"class {wrapper?.IMOClass}";

				if (!wrapper.SubLabel1.IsEmpty)
				{
					result += !wrapper.SubLabel2.IsEmpty
						? " (" + wrapper.SubLabel1 + ", " + wrapper.SubLabel2 + ")"
						: " (" + wrapper.SubLabel1 + ")";
				}

				return result;
			}

			return ZString.Empty;
		}
	}
}
