using System;

namespace CargoWise.PdfiumWrapper.Native
{
	class PageHandle : RelatedSafeHandle
	{
		public PageHandle(IntPtr value)
			: base(IntPtr.Zero, true)
		{
			if (value == IntPtr.Zero)
			{
				throw new ArgumentException("Invalid page handle");
			}

			SetHandle(value);
		}

		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle()
		{
			base.ReleaseHandle();
			if (handle != IntPtr.Zero)
			{
				UnsafeNativeMethods.FPDF_ClosePage(handle);
				SetHandle(IntPtr.Zero);
			}

			return true;
		}
	}
}
