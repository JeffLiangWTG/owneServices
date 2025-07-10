
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public struct CMRContainerModeChecker
	{
		public CMRContainerModeChecker(ZString containerMode)
		{
			this.ContainerMode = containerMode;
		}

		public bool IsFCL
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.FullContainerLoad; }
		}

		public bool IsFCX
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills; }
		}

		public bool IsBulk
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.Bulk; }
		}

		public bool IsBreakBulk
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.BreakBulk; }
		}

		public bool IsLCL
		{
			get { return ContainerMode == CMRImportCargoTypes.Codes.LessThanContainerLoad; }
		}

		public bool IsValid
		{
			get { return IsFCL || IsFCX || IsLCL || IsBulk || IsBreakBulk; }
		}

		public bool IsEmpty
		{
			get { return ContainerMode.IsEmpty; }
		}

		public ZString ContainerMode;

		public override string ToString()
		{
			return ContainerMode;
		}
	}
}
