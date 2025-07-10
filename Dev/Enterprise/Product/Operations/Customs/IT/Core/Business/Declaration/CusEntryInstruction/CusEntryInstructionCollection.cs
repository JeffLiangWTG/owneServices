using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryInstructionCollection : EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>, IDisposable
{
	public CusEntryInstructionCollection(JobDeclaration parentBO) : base(parentBO)
	{
		refresher = ObjectFactory.Get<IEntryInstructionDPOAuthorizationRefresher>(nameof(IEntryInstructionDPOAuthorizationRefresher), this);
		refresher.HookEvents();
	}

	readonly IEntryInstructionDPOAuthorizationRefresher refresher;
	bool disposed;

	public new JobDeclaration Master => (JobDeclaration)base.Master;

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var entryInstruction = (CusEntryInstruction)child;
		Master?.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent(entryInstruction);
		ResetOrDefaultParticipantType(entryInstruction);
	}

	public void ResetOrDefaultParticipantType()
	{
		foreach (CusEntryInstruction entryInstruction in this)
		{
			ResetOrDefaultParticipantType(entryInstruction);
		}
	}

	#region Implementation

	void ResetOrDefaultParticipantType(CusEntryInstruction entryInstruction)
	{
		if (!(Master?.IsExport ?? false))
		{
			entryInstruction.ZG_ParticipantType = ZString.Empty;
		}
		else if (entryInstruction.ZG_ParticipantType.IsEmpty)
		{
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.StandardOneSupplierOneImporter;
		}
	}

	void IDisposable.Dispose()
	{
		if (!disposed)
		{
			refresher?.UnhookEvents();
			disposed = true;
		}
	}

	#endregion
}
