using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CargoWise.PdfiumWrapper.Native
{
	class DocumentHandle : RelatedSafeHandle
	{
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "This is for objects that need to be referenced from native code and can't be GC'd")]
		[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "This is for objects that need to be referenced from native code")]
		readonly object[] additionalThingsWeWantAStrongReferenceFor;
		readonly Stream assosciatedStream;

		public DocumentHandle(IntPtr value, Stream assosciatedStream = null, params object[] additionalThingsWeWantAStrongReferenceFor)
			: base(IntPtr.Zero, true)
		{
			if (value == IntPtr.Zero)
			{
				throw new ArgumentException("Invalid document handle");
			}

			this.assosciatedStream = assosciatedStream;
			this.additionalThingsWeWantAStrongReferenceFor = additionalThingsWeWantAStrongReferenceFor;
			SetHandle(value);
		}

		public override bool IsInvalid
			=> handle == IntPtr.Zero;

		protected override bool ReleaseHandle()
		{
			if (handle != IntPtr.Zero)
			{
				base.ReleaseHandle();
				assosciatedStream?.Dispose();
				UnsafeNativeMethods.FPDF_CloseDocument(handle);
				SetHandle(IntPtr.Zero);
			}

			return true;
		}
	}
}
