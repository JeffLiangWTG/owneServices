using System;
using ResString = Enterprise.Security.Core.ResString;

namespace Enterprise.Security
{
	public sealed class NoneSecurityCheckpoint : SecurityCheckpoint
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Security checkpoint code should not be translated")]
		public const string NoneCode = "None";

		internal NoneSecurityCheckpoint()
			: base(NoneCode, ResString.GetMultilingualString("cb2fbdc9-97ed-45a0-83ca-7c6376e3a45f", "None"), null, null, false)
		{
		}

		public override bool IsAllowed
		{
			get { return true; }
		}

		public override bool Visible
		{
			get { return false; }
		}

		public override void AddChild(SecurityCheckpoint child)
		{
			throw new NotSupportedException("AddChild() is not supported by NoneSecurityCheckpoint.");
		}

		public override void ShowError()
		{
			throw new NotSupportedException("ShowError() is not supported by NoneSecurityCheckpoint.");
		}
	}
}
