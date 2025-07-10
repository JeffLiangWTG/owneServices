using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules
{
	public interface ISecurityCheckpoint
	{
		void AddChild(ISecurityCheckpoint child);
		ISecurityCheckpoint FindChild(string childCode);
		IEnumerable<ISecurityCheckpoint> ChildCheckPoints { get; }
		void ClearIsAllowedCache();
#if DEBUG
		void ClearOverriddenSecurityValue();
#endif
		string Code { get; }
		Guid ItemGuid { get; }
		CheckpointLookupKey LookupKey { get; }
		string Country
		{
			get;
#if DEBUG
			set;
#endif
		}
		string DefaultSecurityOverrideMessage { get; }
		MultilingualString DisplayText { get; }
		void SetDisplayText(MultilingualString displayText);
		MultilingualString DisplayTextPathToSecurityRight { get; }
		MultilingualString ErrorMessageForNotAllowed { get; }
		MultilingualString HumanReadableName { get; }
		bool IsAllowed
		{
			get;
#if DEBUG
			set;
#endif
		}
		bool IsAllowedForAllBranches
		{
			get;
#if DEBUG
			set;
#endif
		}
		bool IsAncestorOf(ISecurityCheckpoint checkPoint);
		bool Visible { get; }
		ISecurityCheckpoint Parent { get; }
		void ShowError();
	}
}
