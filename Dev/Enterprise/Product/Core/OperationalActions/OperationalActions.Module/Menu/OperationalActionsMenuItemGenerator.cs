using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Services.OperationalActions.Module
{
	public sealed class OperationalActionsMenuItemGenerator
	{
		public OperationalActionsMenuItemGenerator(BusinessObjectFactory factory, IEnumerable<BusinessObject> businessObjects, OperationalActionContext context)
			: this(factory, new BusinessObjectSelector(businessObjects), context)
		{
		}

		internal OperationalActionsMenuItemGenerator(BusinessObjectFactory factory, ITargetRecordSelection selection, OperationalActionContext context)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (selection == null)
			{
				throw new ArgumentNullException(nameof(selection));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			this.factory = factory;
			this.selection = selection;
			this.context = context;
		}

		#region DoAction

		void DoAction(OperationalAction action, string caption)
		{
			if (action.IsDeleted)
			{
				var message = Res.GetString("FCD0A8BB-BBE6-45DD-97C6-C226A9F44D57", "The Operational Action '{0}' could not be found, it has been deleted by another user.", caption);
				Globals.Message.ShowWarning(message);
				return;
			}
			var runSecurityCheckpoint = Env.Security.FindOrCreateOperationalActionsRunSpecificCheckpoint(action.MenuNameMultilingual, context.Supporter.RunSecurityCheckpoint);
			if (runSecurityCheckpoint.IsAllowed)
			{
				var runnerFactory = new BusinessObjectFactory();
				runnerFactory.NameForDebugging = "Operational Actions Runner Factory";

				var actionInRunnerFactory = runnerFactory.Load<OperationalAction>(action.PK);
				if (actionInRunnerFactory != null)
				{
					actionInRunnerFactory.Context = action.Context;

					var runner = new OperationalActionRunner(actionInRunnerFactory, context.Supporter.RootType, selection);
					if (CheckRequiredSecurityCheckpoints(runner))
					{
						var hasNonUIActions = action.MethodDescriptors.Cast<OperationalActionMethodDescriptor>().Any(descriptor => descriptor.Method != null && descriptor.Method.RunWithoutUI);
						if (hasNonUIActions)
						{
							var logger = new NoUILogger();
							runner.Run(logger, runnerFactory, OperationalActionMethodUIMode.NonUIOnly);
							if  (logger.HighestErrorLevelEncountered == OperationalActionLogErrorLevel.Error)
							{
								Globals.Message.ShowError(string.Join(System.Environment.NewLine, logger.Messages));
							}
							else if (logger.HighestErrorLevelEncountered == OperationalActionLogErrorLevel.Warning)
							{
								Globals.Message.ShowWarning(string.Join(System.Environment.NewLine, logger.Messages));
							}
							if (logger.HighestErrorLevelEncountered != OperationalActionLogErrorLevel.Error)
							{
								runnerFactory.Save();
							}
						}

						if (!hasNonUIActions ||
							action.MethodDescriptors.Cast<OperationalActionMethodDescriptor>().Any(descriptor => descriptor.Method != null && !descriptor.Method.RunWithoutUI) ||
							action.FieldDescriptors.Count > 0 ||
							action.DocumentPivots.Count > 0)
						{
							ShowOrNotifyUser(runner);
						}
					}
				}
				else
				{
					var message = Res.GetString("FCD0A8BB-BBE6-45DD-97C6-C226A9F44D57", "The Operational Action '{0}' could not be found, it has been deleted by another user.", caption);
					Globals.Message.ShowWarning(message);
				}
			}
			else
			{
				runSecurityCheckpoint.ShowError();
			}
		}

		void ShowOrNotifyUser(OperationalActionRunner runner)
		{
			if (runner.SelectedGridRowCount > 0)
			{
				if (selection.ExclusionReasons.Count > 0)
				{
					Globals.Message.Show(Res.GetString("8d568da7-a3df-4f6a-80ac-11f1e233f293",
						"Some objects were excluded from the Operational Action for the following reason(s):\r\n{0}",
						selection.ExclusionReasons.Aggregate((x, y) => { return x + "\r\n" + y; })));
				}
				OperationalActionRunnerForm.Show(runner);
			}
			else
			{
				if (selection.ExclusionReasons.Count > 0)
				{
					Globals.Message.Show(Res.GetString("73b7987c-1756-4146-aa11-daf149af5e36",
						"All objects were excluded from the Operational Action for the following reason(s):\r\n{0}",
						selection.ExclusionReasons.Aggregate((x, y) => { return x + "\r\n" + y; })));
				}
				else
				{
					OperationalActionRunnerForm.Show(runner);
				}
			}
		}

		internal static IEnumerable<ZController> GetControllersRecursivelyFromType(Type type)
		{
			var result = ZControllerFactory.Instance.GetControllersForType(type);
			if (!result.Any() && type.BaseType != null)
			{
				result = GetControllersRecursivelyFromType(type.BaseType);
			}
			return result;
		}

		void CheckForEditPermission(OperationalActionRunner runner, HashSet<string> deniedCheckpoints)
		{
			if (runner.Fields.Count > 0)
			{
				List<ZController> controllers = new List<ZController>();

				/*
				 * 1) Gather all conceivable controllers. (Though if the controller we got from the module selection works, we don't need to look for any others.)
				 * 2) for each PK, check if ANY securitycheckpoint is valid for it. If we get 1+ granted, we allow it. If we get 1+ denied, the whole process fails.
				 * 3) If we get even one PK that fails, return the list of all security rights that could be granted. If no PKs fail, don't.
				 * */

				if (selection is ModuleSelection modselection)
				{
					controllers.Add(((ZFilterGridModule)modselection.Module).GetNewController());
				}

				//Only gather other controllers if we don't get an edit checkpoint from our module selection controller.
				var checkCheckpoint = controllers.Any() ?
					GetCheckPointForEditSafe(controllers[0],
						selection.GetSelectedRecords().PrimaryKeys.Any() ?
						factory.Load(context.Supporter.RootType, selection.GetSelectedRecords().PrimaryKeys[0])
						: null)
					: null;

				if (checkCheckpoint == null)
				{
					controllers.AddRange(GetControllersRecursivelyFromType(context.Supporter.RootType));
				}

				var doDeny = false;

				foreach (var pk in selection.GetSelectedRecords().PrimaryKeys)
				{
					var wasDenied = false;
					var wasGranted = false;
					foreach (var controller in controllers)
					{
						var bizo = factory.Load(context.Supporter.RootType, pk);
						var checkpointForEdit = GetCheckPointForEditSafe(controller, bizo);
						if (checkpointForEdit != null)
						{
							if (!checkpointForEdit.IsAllowed)
							{
								wasDenied = true;
								deniedCheckpoints.Add(checkpointForEdit.DisplayTextPathToSecurityRight);
							}
							else
							{
								wasGranted = true;
								break;
							}
						}
					}
					if (wasDenied && !wasGranted)
					{
						doDeny = true;
					}
				}

				if (!doDeny)
				{
					deniedCheckpoints.Clear();
				}
			}
		}

		SecurityCheckpoint GetCheckPointForEditSafe(ZController controller, BusinessObject bizo)
		{
			try
			{
				return controller.GetCheckPointForEdit(bizo);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		bool CheckRequiredSecurityCheckpoints(OperationalActionRunner runner)
		{
			var deniedCheckpoints = new HashSet<string>();

			CheckForEditPermission(runner, deniedCheckpoints);

			foreach (SecurityCheckpoint checkpoint in runner.ExtractUniqueSecurityCheckpoints())
			{
				if (!checkpoint.IsAllowed)
				{
					deniedCheckpoints.Add(checkpoint.DisplayTextPathToSecurityRight);
				}
			}

			if (deniedCheckpoints.Count == 0)
			{
				return true;
			}
			else
			{
				var builder = new StringBuilder();
				builder.Append(Res.GetString(
					"OpperationalActionsRunner|Security|Header",
					"You are missing 1 or more security rights required to run this action.\r\n" +
					"If you believe you should be able to run this action then you should ask your administrator to grant you the following security rights."));
				builder.Append("\n\n");

				var listDeniedCheckpoints = new List<string>(deniedCheckpoints);
				listDeniedCheckpoints.Sort(StringComparer.OrdinalIgnoreCase);

				foreach (string checkpoint in listDeniedCheckpoints)
				{
					builder.AppendFormat("\t{0}\n", checkpoint);
				}

				string caption = Res.GetString("OperationalActionsRunner|Security|Caption", "Security Check Failed");
				Globals.Message.ShowError(builder.ToString(), caption);

				return false;
			}
		}

		#region NoUILogger

		class NoUILogger : IOperationalActionLog
		{
			public OperationalActionLogErrorLevel HighestErrorLevelEncountered
			{
				get { return highestErrorLevel; }
			}
			OperationalActionLogErrorLevel highestErrorLevel = OperationalActionLogErrorLevel.Informational;

			public void Notify(OperationalActionLogErrorLevel errorLevel, string text)
			{
				if (errorLevel > highestErrorLevel)
				{
					highestErrorLevel = errorLevel;
				}

				if (errorLevel >= OperationalActionLogErrorLevel.Warning)
				{
					if (messages == null)
					{
						messages = new List<string>();
					}
					messages.Add(text);
				}
			}

			public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
			{
				Notify(errorLevel, string.Format(format, args));
			}

			public IEnumerable<string> Messages
			{
				get
				{
					if (messages == null)
					{
						return Enumerable.Empty<string>();
					}
					return messages;
				}
			}
			List<string> messages;

			#region IOperationalActionLog - not implemented members

			public void BumpMasterProgress() { }
			public void BumpSectionProgress() { }
			public void SetMasterProgressMax(int max) { }
			public void SetSectionProgressMax(int max) { }

			#endregion
		}

		#endregion

		#endregion

		#region CreateActionMenuItem

		MenuItem CreateActionMenuItem(OperationalAction action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var caption = action.SU_MenuNameMultilingual;
			return new ZMenuItem(caption, delegate { DoAction(action, caption); });
		}

		#endregion

		#region IMenuItemGenerator Members

		public MenuItem[] Generate(bool forRoot)
		{
			OperationalAction[] actions = LoadApplicableActions(forRoot);
			var result = new List<MenuItem>(actions.Length + 2);

			if (actions.Length == 0 && !forRoot)
			{
				result.Add(new ZMenuItem(ResString.GetMultilingualString("fb8ef7e3-3cd6-44e6-8102-0d611696ab29", "No Operational Actions Found.")));
			}
			else
			{
				Array.Sort(actions, new ActionComparer());
				foreach (OperationalAction action in actions)
				{
					action.Context = context;
					var paths = action.SU_MenuPathMultilingual.ToString().Split(separator, StringSplitOptions.RemoveEmptyEntries);
					TracePath(result, paths, 0).Add(CreateActionMenuItem(action));
				}
			}

			if (!forRoot)
			{
				var maker = new OperationalActionsCustomisationMenuMaker(context);
				result.AddRange(maker.Make());
			}
			return result.ToArray();
		}

		readonly char[] separator = new char[] { '/', '\\' };

		OperationalAction[] LoadApplicableActions(bool forRoot)
		{
			var actions = LoadApplicableActions(context, factory);

			return Array.FindAll(actions, (a) =>
			{
				try
				{
					return (a.SU_MenuPathMultilingual.ToString().IndexOfAny(separator) == 0) == forRoot && FilterParser.Parse(a.SU_FilterList).Evaluate(EnvironmentFilterProvider.Instance);
				}
				catch (ParseException)
				{
					return false;
				}
			});
		}

		internal static OperationalAction[] LoadApplicableActions(OperationalActionContext context, BusinessObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory() { NameForDebugging = "OperationalActionsMenuItemGenerator_LoadApplicableActions" };
			}
			return factory.Load<OperationalAction>(new OperationalActionZQuery(context.Supporter.BusinessContext, Environment.Env.CurrentUser.Initials));
		}

		IList TracePath(IList children, string[] path, int index)
		{
			if (index >= path.Length)
			{
				return children;
			}
			else if (string.IsNullOrEmpty(path[index]))
			{
				return TracePath(children, path, index + 1);
			}
			else
			{
				MenuItem child = FindSubMenu(children, path[index]);

				if (child == null)
				{
					child = new ZMenuItem(path[index]);
					children.Add(child);
				}

				return TracePath(child.MenuItems, path, index + 1);
			}
		}

		MenuItem FindSubMenu(IEnumerable options, string name)
		{
			foreach (MenuItem item in options)
			{
				if (item.Text == name && item.IsParent)
				{
					return item;
				}
			}
			return null;
		}

		#endregion

		readonly OperationalActionContext context;
		readonly BusinessObjectFactory factory;
		readonly ITargetRecordSelection selection;
	}
}
