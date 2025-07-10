using System;
using System.Diagnostics;
using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	sealed class DocumentPackId : IEquatable<DocumentPackId>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ZGuid commandPK;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ZGuid supporterPK;

		public DocumentPackId(ZGuid commandPK)
			: this(commandPK, ZGuid.Empty)
		{
		}

		public DocumentPackId(ZGuid commandPK, ZGuid supporterPK)
		{
			this.commandPK = commandPK;
			this.supporterPK = supporterPK;
		}

		public ZGuid CommandPK
		{
			get { return commandPK; }
		}

		public ZGuid SupporterPK
		{
			get { return supporterPK; }
		}

		public bool Equals(DocumentPackId other)
		{
			return (other != null) && (CommandPK == other.CommandPK) && (SupporterPK == other.SupporterPK);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as DocumentPackId);
		}

		public override int GetHashCode()
		{
			return CommandPK.GetHashCode() ^ SupporterPK.GetHashCode();
		}
	}
}
