using System.Collections.Generic;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.Business
{
	public class BufferDTO : IBuffer
	{
		#region IBuffer Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int SizeInMinutes { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IBuffer.OffsetInMinutes
		{
			get { return 0; }
		}

		string IBuffer.Name
		{
			get { return string.Empty; }
		}

		BufferType IBuffer.Type
		{
			get { return BufferType.Unknown; }
		}

		IReadOnlyCollection<IBufferedItem> IBuffer.RelatedBufferedItems
		{
			get { return new List<IBufferedItem>(); }
		}

		#endregion

		#region For Test
#if DEBUG

		public override bool Equals(object obj)
		{
			var other = obj as IBuffer;
			return other != null && other.SizeInMinutes == SizeInMinutes;
		}

		public override int GetHashCode()
		{
			return SizeInMinutes.GetHashCode();
		}

#endif
		#endregion
	}
}
