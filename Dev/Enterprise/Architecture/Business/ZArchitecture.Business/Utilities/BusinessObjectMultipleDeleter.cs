using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Business.Res;
using ResString = Enterprise.ZArchitecture.Business.ResString;

namespace Enterprise.ZArchitecture
{
	public enum BusinessObjectMultipleDeleterAction
	{
		Delete,
		Activate,
		Deactivate
	}
	public class BusinessObjectMultipleDeleter : ICanDelete
	{
		#region Constructor

		public BusinessObjectMultipleDeleter(BusinessObject[] selectedBusinessObjects)
		{
			SelectedBusinessObjects = selectedBusinessObjects;
		}

		readonly BusinessObject[] SelectedBusinessObjects;
		#endregion

		#region Processing

		public string Process(BusinessObjectMultipleDeleterAction action)
		{
			string result = null;
			var messages = new List<string>();
			switch (action)
			{
				case BusinessObjectMultipleDeleterAction.Delete:
					messages = Delete();
					break;
				case BusinessObjectMultipleDeleterAction.Activate:
					messages = ActivateOrDeactivate(true);
					break;
				case BusinessObjectMultipleDeleterAction.Deactivate:
					messages = ActivateOrDeactivate(false);
					break;
			}

			if (messages.Count > 0)
			{
				result = string.Join("\r\n", messages.ToArray());
			}
			return result;
		}

		List<string> ActivateOrDeactivate(bool activate)
		{
			var messages = new List<string>();

			//first filter out actuallyCancellable and otherwise make messages

			var actuallyCancellable = new List<BusinessObject>();

			foreach (var obj in Cancelable)
			{
				string message = CheckCanCancel(obj);
				if (!string.IsNullOrEmpty(message))
				{
					messages.Add(message);
				}
				else
				{
					message = "";
					//watch out for the 'single negative' here - if is cancelled == activate, then we want to FLIP the state, so this is correct
					if ((obj as ICancellable).IsCancelled == activate)
					{
						actuallyCancellable.Add(obj);
					}
					else
					{
						message = activate
							? Res.GetString("c82af5df-55ce-42f2-9a21-fa3101e7a5d0", "{0} is already active.", obj.HumanReadableName)
							: Res.GetString("d2ddacd3-47e2-46f3-9621-a1bd55fca9de", "{0} is already inactive.", obj.HumanReadableName);
					}

					if (!string.IsNullOrEmpty(message))
					{
						messages.Add(message);
					}
				}
			}

			//first try mass activate/deactivate in second factory
			try
			{
				var secondFactory = new BusinessObjectFactory();
				foreach (var obj in actuallyCancellable)
				{
					//at this point we know we can and do want to cancel it
					var objInOtherFactory = secondFactory.ImportFromAnotherFactory(obj);
					(objInOtherFactory as ICancellable).IsCancelled = !activate;
				}

				secondFactory.Save();

				return messages;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}

			//else fall back to one-by-one with latest DB information
			foreach (BusinessObject obj in actuallyCancellable)
			{
				string error = null;
				var objInOtherFactory = new BusinessObjectFactory().Load(obj.GetType(), obj.PK);
				try
				{
					(objInOtherFactory as ICancellable).IsCancelled = !activate;
					objInOtherFactory.Factory.Save();
				}
				catch (ZSaveException ex) when (!ex.IsCriticalException())
				{
					error = ex.FriendlyMessage;
					if (string.IsNullOrEmpty(error))
					{
						error = ex.Message;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					error = ex.Message;
				}
				if (!string.IsNullOrEmpty(error) && !messages.Contains(error))
				{
					messages.Add(error);
				}
			}

			return messages;
		}

		List<string> Delete()
		{
			var messages = new List<string>();

			foreach (var obj in CannotBeDeleted)
			{
				messages.Add(Res.GetString("3a4bad9f-7a5a-419b-a7aa-b77aed4e800d", "Object cannot be deleted. Reason: {0}", obj.ReasonForNotAbleToDelete));
			}

			//first try mass delete in second factory
			try
			{
				var secondFactory = new BusinessObjectFactory();
				foreach (var obj in CanBeDeleted)
				{
					var objInOtherFactory = secondFactory.ImportFromAnotherFactory(obj);
					objInOtherFactory.Delete();
				}

				secondFactory.Save();

				return messages;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}

			//else fall back to one-by-one with latest DB information
			foreach (var obj in CanBeDeleted)
			{
				var objInOtherFactory = new BusinessObjectFactory().Load(obj.GetType(), obj.PK);
				string error = null;
				try
				{
					objInOtherFactory.Delete();
					objInOtherFactory.Factory.Save();
				}
				catch (ZSaveException ex) when (!ex.IsCriticalException())
				{
					error = ex.FriendlyMessage;
					if (string.IsNullOrEmpty(error))
					{
						error = ex.Message;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					error = ex.Message;
				}
				if (!string.IsNullOrEmpty(error) && !messages.Contains(error))
				{
					messages.Add(error);
				}
			}

			return messages;
		}

		#region ICanDelete Members

		public MultilingualString GetWarningBeforeBeingDeleted()
		{
			return (NoResString)string.Empty;
		}

		public bool CanDelete
		{
			get { return Cancelable.Count > 0 || CanBeDeleted.Count > 0; }
		}

		public MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;
				if (!CanDelete)
				{
					result = ResString.GetMultilingualString("c9c657d3-f8fe-4866-b634-df05f1943887", "None of selected objects can be deleted or canceled.");
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Helper methods

		// !((!a & b) | (a & c)) = (a | !b) & (!a | !c)
		string CheckCanCancel(BusinessObject obj)
		{
			string result = string.Empty;
			var objInSeparateFactory = new BusinessObjectFactory().Load(obj.GetType(), obj.PK);
			var cancellable = objInSeparateFactory as ICancellable;

			if (cancellable != null && PreventDeleteAttribute.IsTrue(objInSeparateFactory.GetType()))
			{
				if ((cancellable.IsCancelled || !string.IsNullOrEmpty(cancellable.CanCancel())) &&
					(!cancellable.IsCancelled || !string.IsNullOrEmpty(cancellable.CanReactivate())))
				{
					result = !cancellable.IsCancelled ? cancellable.CanCancel() : cancellable.CanReactivate();
				}
			}
			return result;
		}

		#region Properties

		List<BusinessObject> cancelable;
		List<BusinessObject> Cancelable
		{
			get
			{
				if (cancelable == null)
				{
					cancelable = GetCancelable();
				}
				return cancelable;
			}
		}

		List<BusinessObject> GetCancelable()
		{
			var cancellable = new List<BusinessObject>();
			foreach (var obj in SelectedBusinessObjects)
			{
				var cancellableObj = obj as ICancellable;
				if (cancellableObj != null && PreventDeleteAttribute.IsTrue(obj.GetType()))
				{
					cancellable.Add(obj);
				}
			}
			return cancellable;
		}

		List<BusinessObject> canBeDeleted;
		List<BusinessObject> CanBeDeleted
		{
			get
			{
				if (canBeDeleted == null)
				{
					canBeDeleted = GetDeletableOrUndeletable(true);
				}
				return canBeDeleted;
			}
		}

		List<BusinessObject> cannotBeDeleted;
		List<BusinessObject> CannotBeDeleted
		{
			get
			{
				if (cannotBeDeleted == null)
				{
					cannotBeDeleted = GetDeletableOrUndeletable(false);
				}
				return cannotBeDeleted;
			}
		}

		List<BusinessObject> GetDeletableOrUndeletable(bool deletable)
		{
			var result = new List<BusinessObject>();
			foreach (var obj in SelectedBusinessObjects)
			{
				var cancellableObj = obj as ICancellable;
				if ((cancellableObj == null || !PreventDeleteAttribute.IsTrue(obj.GetType())) && obj.CanDelete == deletable)
				{
					result.Add(obj);
				}
			}
			return result;
		}

		#endregion

		#region Confirmation message

		public string GetConfirmationMessage(BusinessObjectMultipleDeleterAction action, bool listBizObj = true)
		{
			var confirmationMessage = string.Empty;
			if (action == BusinessObjectMultipleDeleterAction.Delete || listBizObj)
			{
				switch (action)
				{
					case BusinessObjectMultipleDeleterAction.Delete:
						var delete = Res.GetString("14ced34c-ef70-4ce0-865a-aadf213b63fb", "delete");
						confirmationMessage = GetMessage(Res.GetString("4929387b-dd38-4bb3-8c89-aa4c7b96a9e5", "The selected records will be deleted:"), delete, CanBeDeleted);
						confirmationMessage += GetMessage(Res.GetString("f310f1b3-2333-42e6-ab9c-6b58f82264aa", "The selected records cannot be deleted:"), delete, CannotBeDeleted);
						break;
					case BusinessObjectMultipleDeleterAction.Activate:
						confirmationMessage = Res.GetString("bd20c87f-9139-4f69-a1dc-5334609ea03f", "If some objects are already active, no action will be performed on them.") + "\r\n";
						confirmationMessage += GetMessage(Res.GetString("7ff4b70d-715e-40c8-a49f-070b65b87a77", "These objects will be activated:"), Res.GetString("e39ac61b-b123-4abb-8636-b1b3c95280dc", "activate"), Cancelable);
						break;
					case BusinessObjectMultipleDeleterAction.Deactivate:
						confirmationMessage = Res.GetString("0d9ea58d-d909-4d18-8c8e-1acc87c51742", "If some objects are already inactive, no action will be performed on them.") + "\r\n";
						confirmationMessage += GetMessage(Res.GetString("a09f7dab-5e45-446a-b1d0-69d005faa970", "These objects will be deactivated:"), Res.GetString("1ebb5f49-b80a-4130-ad54-c9b642b26cfd", "deactivate"), Cancelable);
						break;
					default:
						throw new NotSupportedException(string.Format("Action {0} is not supported", action));
				}
			}
			else
			{
				var actionLabel = string.Empty;
				var message = string.Empty;
				switch (action)
				{
					case BusinessObjectMultipleDeleterAction.Activate:
						actionLabel = Res.GetString("e39ac61b-b123-4abb-8636-b1b3c95280dc", "activate");
						message = Res.GetString("bd20c87f-9139-4f69-a1dc-5334609ea03f", "If some objects are already active, no action will be performed on them.");
						break;
					case BusinessObjectMultipleDeleterAction.Deactivate:
						actionLabel = Res.GetString("1ebb5f49-b80a-4130-ad54-c9b642b26cfd", "deactivate");
						message = Res.GetString("0d9ea58d-d909-4d18-8c8e-1acc87c51742", "If some objects are already inactive, no action will be performed on them.");
						break;
					default:
						throw new NotSupportedException(string.Format("Action {0} is not supported", action));
				}
				confirmationMessage = Res.GetString("c19c4ba8-9feb-4a25-9726-4729bc07a9c7", "Would you like to {0} the selected items? {1}", actionLabel, message);
			}
			return confirmationMessage.Trim();
		}

		ZString GetMessage(ZString message, ZString action, List<BusinessObject> list)
		{
			var result = string.Empty;
			if (list.Count > 0)
			{
				if (list.Count < MaxToUseHumanReadableName)
				{
					result += message + "\r\n\n" + GetHumanReadableNamesEachForOneLine(list) + "\r\n";
				}
				else
				{
					result += Res.GetString("f2850b62-3b74-44be-8335-53ed56855a05", "You are about to {0} {1} objects.", action, list.Count) + "\r\n";
				}
			}
			return result;
		}

		ZString GetHumanReadableNamesEachForOneLine(List<BusinessObject> list)
		{
			return ZString.Join("\r\n", Array.ConvertAll(list.ToArray(), x => x.HumanReadableName));
		}

		const int MaxToUseHumanReadableName = 20;

		#endregion

		#endregion

		#region ICanDelete Members

		public void OnCannotDelete()
		{
			OnCannotDeleteCore();
		}

		protected virtual void OnCannotDeleteCore()
		{
		}

		#endregion
	}
}
