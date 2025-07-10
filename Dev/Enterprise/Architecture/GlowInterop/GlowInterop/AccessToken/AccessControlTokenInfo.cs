using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GlowInterop
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not (yet) needed.")]
	public struct AccessTokenInfo
	{
		public AccessTokenInfo(string scope, Guid parentId, string parentTableCode)
		{
			Argument.NotNull(scope, nameof(scope));
			Argument.NotNull(parentTableCode, nameof(parentTableCode));

			Scope = scope;
			ParentId = parentId;
			ParentTableCode = parentTableCode;
		}

		public string Scope { get; }
		public Guid ParentId { get; }
		public string ParentTableCode { get; }

		public override bool Equals(object obj)
		{
			if (!(obj is AccessTokenInfo))
			{
				return false;
			}

			return Equals((AccessTokenInfo)obj);
		}

		public override int GetHashCode()
		{
			return Scope.GetHashCode() + ParentId.GetHashCode() + ParentTableCode.GetHashCode();
		}

		public static bool operator ==(AccessTokenInfo tokenInfo1, AccessTokenInfo tokenInfo2)
		{
			return tokenInfo1.Equals(tokenInfo2);
		}

		public static bool operator !=(AccessTokenInfo tokenInfo1, AccessTokenInfo tokenInfo2)
		{
			return !tokenInfo1.Equals(tokenInfo2);
		}
	}
}
