using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Integration;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskAction : IComplianceRiskAction
	{
		public ComplianceRiskAction(Action synchronizeAndSaveIfNeededAction, Action<List<ZString>> showMessageIfNeededAction, Action updateVisibilityIfNeededAction, ZString jobComplianceStatus)
		{
			SynchronizeAndSaveIfNeededAction = synchronizeAndSaveIfNeededAction;
			ShowMessageIfNeededAction = showMessageIfNeededAction;
			UpdateVisibilityIfNeededAction = updateVisibilityIfNeededAction;
			JobComplianceStatus = new List<ZString> { jobComplianceStatus };
		}

		public ComplianceRiskAction(Action synchronizeAndSaveIfNeededAction, Action<List<ZString>> showMessageIfNeededAction, Action updateVisibilityIfNeededAction, List<ZString> jobComplianceStatus)
		{
			SynchronizeAndSaveIfNeededAction = synchronizeAndSaveIfNeededAction;
			ShowMessageIfNeededAction = showMessageIfNeededAction;
			UpdateVisibilityIfNeededAction = updateVisibilityIfNeededAction;
			JobComplianceStatus = jobComplianceStatus;
		}

		public void SynchronizeAndSaveIfNeeded()
		{
			SynchronizeAndSaveIfNeededAction?.Invoke();
			SynchronizeAndSaveIfNeededAction = null;
		}

		public void ShowMessageIfNeeded()
		{
			ShowMessageIfNeededAction?.Invoke(JobComplianceStatus);
			ShowMessageIfNeededAction = null;
		}

		/// <summary>
		/// Refresh the user interface (UI) visibility when compliance risk status is Potential Risk.
		/// </summary>
		public void UpdateVisibilityIfNeeded()
		{
			UpdateVisibilityIfNeededAction?.Invoke();
		}

		Action SynchronizeAndSaveIfNeededAction { get; set; }

		Action<List<ZString>> ShowMessageIfNeededAction { get; set; }

		Action UpdateVisibilityIfNeededAction { get; set; }

		List<ZString> JobComplianceStatus { get; }
	}
}
