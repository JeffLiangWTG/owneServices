using System;

namespace CargoWise.PdfiumWrapper.Native
{
	class TextPageHandle : RelatedSafeHandle
	{
		public TextPageHandle(IntPtr value)
			: base(IntPtr.Zero, true)
		{
			if (value == IntPtr.Zero)
			{
				throw new ArgumentException("Invalid text page handle");
			}

			SetHandle(value);
		}

		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle()
		{
			base.ReleaseHandle();
			if (handle != IntPtr.Zero)
			{
				UnsafeNativeMethods.FPDFText_ClosePage(handle);
				SetHandle(IntPtr.Zero);
			}

			return true;
		}
	}
}
