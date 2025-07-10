using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CargoWise.PdfiumWrapper.Native
{
	internal abstract class RelatedSafeHandle : SafeHandle
	{
		internal RelatedSafeHandle Parent { get; set; }
		internal List<SafeHandle> Children { get; }

		protected RelatedSafeHandle(IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle)
		{
			Children = new List<SafeHandle>();
		}

		protected override bool ReleaseHandle()
		{
			foreach (var handle in Children.ToArray())
			{
				if (!handle.IsClosed && !handle.IsInvalid)
				{
					handle.Close();
				}
			}

			if (Parent?.Children.Contains(this) == true)
			{
				Parent.Children.Remove(this);
			}

			return true;
		}
	}
}
