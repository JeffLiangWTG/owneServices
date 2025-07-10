using System;

namespace CargoWise.PdfiumWrapper.Native
{
	class SearchHandle : RelatedSafeHandle
	{
		public SearchHandle(IntPtr value)
			: base(IntPtr.Zero, true)
		{
			if (value == IntPtr.Zero)
			{
				throw new ArgumentException("Invalid search handle");
			}

			SetHandle(value);
		}

		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle()
		{
			base.ReleaseHandle();
			if (handle != IntPtr.Zero)
			{
				UnsafeNativeMethods.FPDFText_FindClose(handle);
				SetHandle(IntPtr.Zero);
			}

			return true;
		}
	}
}
