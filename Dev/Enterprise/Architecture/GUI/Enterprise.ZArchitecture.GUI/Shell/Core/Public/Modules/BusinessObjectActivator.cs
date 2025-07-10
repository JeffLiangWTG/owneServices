using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules
{
	public class BusinessObjectActivator
	{
		public BusinessObjectActivator()
		{
		}

		protected BusinessObjectFactory Factory { get; set; }

		public void Activate(BusinessObject[] selectedObjects, ISecurityCheckpoint checkpoint)
		{
			ActivateDeactivate(true, selectedObjects, checkpoint);
		}

		public void Deactivate(BusinessObject[] selectedObjects, ISecurityCheckpoint checkpoint)
		{
			ActivateDeactivate(false, selectedObjects, checkpoint);
		}

		void ActivateDeactivate(bool activate, BusinessObject[] selectedObjects, ISecurityCheckpoint checkpoint)
		{
			Argument.NotNull(checkpoint, "checkpoint"); // parameter name
			Argument.NotNull<Array>(selectedObjects, "selectedObjects"); // parameter name
			var resultToShow = string.Empty;

			if (selectedObjects.Length > 0)
			{
				Factory = selectedObjects[0].Factory;
				if (checkpoint.IsAllowed)
				{
					var question = Res.GetString("c0a3903b-4623-49fe-a63b-b63e6b26bd47", "You are about to {0} {1} elements.",
						(activate ? Res.GetString("1D6E2D21-87FF-4FA1-8C2B-EEAD3D6C9322", "activate") : Res.GetString("2CE71997-58AD-4109-8B5F-1C2442E47504", "deactivate")),
						selectedObjects.Length.ToString());
					var caption = Res.GetString("f75005a0-5e1d-4a5e-9a60-11b9e479c8b7", "Activate / De-Activate");
					var confirmation = Res.GetString("a0d2cbfa-8f18-4317-bcd1-e16cd9621bd3", "yes");

					if (Globals.Message.ShowConfirmation(question, caption, confirmation, MessageBoxIcon.Question, MessageBoxButtons.OKCancel) == DialogResult.OK)
					{
						var activationprocessResult = ActivateDeactivateCore(activate, selectedObjects);
						resultToShow = SaveIfApplicable(activationprocessResult);
					}
				}
				else
				{
					resultToShow = Res.GetString("d60436f9-bde5-4df7-9eca-14dabdb47e72", "You are not allowed to Activate/Deactivate in this module. Please contact your system administrator.");
				}
			}
			else
			{
				resultToShow = Res.GetString("628bfdac-1bdf-474e-a815-7825af1dec6d", "Please select at least one object");
			}

			if (!string.IsNullOrEmpty(resultToShow))
			{
				Globals.Message.Show(resultToShow);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		string SaveIfApplicable(ActivationResult activationprocessResult)
		{
			var result = string.Empty;
			var finalCheckResult = ProvideFinalActivateDeactivateCheckBeforeSaving(activationprocessResult);
			if (string.IsNullOrEmpty(finalCheckResult))
			{
				result = activationprocessResult.Errors;
				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					result = "Could not save changes. Error: " + ex.Message;
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			else
			{
				result = finalCheckResult;
				CancelChanges(activationprocessResult.ProcessedObjects);
			}
			return result;
		}

		void CancelChanges(BusinessObject[] businessObjects)
		{
			Array.ForEach(businessObjects, bizO => bizO.CancelChanges());
		}

		protected virtual string ProvideFinalActivateDeactivateCheckBeforeSaving(ActivationResult activationProcessResult)
		{
			return null;
		}

		protected class ActivationResult
		{
			public ActivationResult(string errors, BusinessObject[] processedObjects, bool activation)
			{
				Errors = errors;
				ProcessedObjects = processedObjects;
				Activation = activation;
			}

			public string Errors { get; private set; }
			public BusinessObject[] ProcessedObjects { get; private set; }
			public bool Activation { get; private set; }
		}

		ActivationResult ActivateDeactivateCore(bool activate, BusinessObject[] selectedObjects)
		{
			var errors = string.Empty;
			var processed = new List<BusinessObject>();
			foreach (var bizO in selectedObjects)
			{
				var cancellable = bizO as ICancellable;
				if (cancellable != null)
				{
					var actionMessage = activate ? cancellable.CanReactivate() : cancellable.CanCancel();
					if (string.IsNullOrEmpty(actionMessage))
					{
						var requiresAdditionalAction = RequiresAdditionalAction(activate, bizO);
						if (activate == cancellable.IsCancelled || requiresAdditionalAction)
						{
							if (requiresAdditionalAction)
							{
								AdditionalActivateDeactivateAction(activate, bizO);
							}

							cancellable.IsCancelled = !activate;
							processed.Add(bizO);
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(errors))
						{
							errors += System.Environment.NewLine;
						}
						errors += actionMessage;
					}
				}
			}
			return new ActivationResult(errors, processed.ToArray(), activate);
		}

		protected virtual void AdditionalActivateDeactivateAction(bool activate, BusinessObject bizO)
		{
		}

		protected virtual bool RequiresAdditionalAction(bool activate, BusinessObject bizO)
		{
			return false;
		}
	}
}
