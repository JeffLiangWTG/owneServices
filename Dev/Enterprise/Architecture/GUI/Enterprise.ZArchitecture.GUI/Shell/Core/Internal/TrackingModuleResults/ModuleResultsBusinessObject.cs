using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules
{
	public sealed class ModuleResultsBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string NumberOfResults = "NumberOfResults";
			public const string CurrentRecordNumberAllowingInvalid = "CurrentRecordNumberAllowingInvalid";
		}

		#endregion

		public ModuleResultsBusinessObject(ZPKCollection pKList)
		{
			SetPKList(pKList);
			pKList.ListChanged += new EventHandler(PKList_ListChanged);
		}

		#region Properties

		#region NumberOfResults

		public ZInt NumberOfResults
		{
			get { return PKList.Count; }
		}

		public ZPropertyInfo NumberOfResultsInfo
		{
			get { return GetZPropertyInfo(Schema.NumberOfResults); }
		}

		#endregion

		#region CurrentRecordNumber

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message for developers only")]
		const string InvalidRecordErrorMessage = "Cannot set current record number to an invalid value";

		ZInt PerformOnCurrentRecordNumberChanging(int originalIndex, bool throwIfInvalid, out bool wasCancelled)
		{
			if (throwIfInvalid && !IsValidRecordNumber(originalIndex))
			{
				throw new NotSupportedException(InvalidRecordErrorMessage);
			}

			var nextItem = PKList.ElementAtOrDefault(originalIndex - 1);
			wasCancelled = OnCurrentRecordNumberChanging().Cancel;
			var postEventIndex = nextItem.PK.IsValid ? PKList.IndexOf(nextItem) + 1 : -1;

			var itemWasDeleted = postEventIndex < 1;

			if (throwIfInvalid && !wasCancelled && (itemWasDeleted || !IsValidRecordNumber(postEventIndex)))
			{
				throw new NotSupportedException(InvalidRecordErrorMessage);
			}

			return itemWasDeleted ? originalIndex : postEventIndex;
		}

		void SetRecordNumber(ZInt value, bool throwIfInvalid)
		{
			bool wasCancelled;
			var newValue = PerformOnCurrentRecordNumberChanging(value, throwIfInvalid, out wasCancelled);
			var oldCurrentRecordNumberAllowingInvalid = fCurrentRecordNumberAllowingInvalid;

			if (!wasCancelled || !IsValidRecordNumber(value))
			{
				fCurrentRecordNumberAllowingInvalid = wasCancelled ? value : newValue;
				if (!CurrentRecordNumberAllowingInvalidInfo.BizObj.IsValidationSuspended)
				{
					ValidateCurrentRecordNumberAllowingInvalid();
				}
			}

			if (!wasCancelled && IsValidRecordNumber(newValue))
			{
				var oldValue = fRecordNumber;
				var oldCurrentPK = fCurrentPK;
				try
				{
					fRecordNumber = newValue;
					fCurrentPK = PKList[newValue - 1].PK;

					OnCurrentRecordNumberChanged();
				}
				catch
				{
					fRecordNumber = oldValue;
					fCurrentPK = oldCurrentPK;
					fCurrentRecordNumberAllowingInvalid = oldCurrentRecordNumberAllowingInvalid;
					throw;
				}
			}

			CurrentRecordNumberAllowingInvalidInfo.RefreshBinding();
		}

		ZInt fRecordNumber;
		public ZInt CurrentRecordNumber
		{
			get { return fRecordNumber; }
			set
			{
				SetRecordNumber(value, true);
			}
		}

		#region CurrentRecordNumberAllowingInvalid

		public ZInt CurrentRecordNumberAllowingInvalid
		{
			get
			{
				return fCurrentRecordNumberAllowingInvalid;
			}
			set
			{
				SetRecordNumber(value, false);
			}
		}
		ZInt fCurrentRecordNumberAllowingInvalid;

		public ZPropertyInfo CurrentRecordNumberAllowingInvalidInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentRecordNumberAllowingInvalid); }
		}

		void ValidateCurrentRecordNumberAllowingInvalid()
		{
			CurrentRecordNumberAllowingInvalidInfo.ClearAllNotifications();
			if (!IsValidRecordNumber(CurrentRecordNumberAllowingInvalid))
			{
				var message = NumberOfResults > 1 ?
					Res.GetString("aa278af7-b84d-48be-8766-7a6c3c82621d", "Record number must be between 1 and {0}.", NumberOfResults) :
					Res.GetString("fcd08f35-6ca0-4fe4-9de4-8a9c3707584b", "There is currently only one record in the search results.");

				CurrentRecordNumberAllowingInvalidInfo.AddError(message);
			}
		}

		#endregion

		bool IsValidRecordNumber(ZInt number)
		{
			return number > 0 && number <= NumberOfResults;
		}

		#region CurrentRecordNumberChanging / Changed Events

		public delegate void NumberChangingEvent(CurrentRecordNumberChangingEventArgs args);
		public event NumberChangingEvent CurrentRecordNumberChanging;
		public event EventHandler CurrentRecordNumberChanged;

		public class CurrentRecordNumberChangingEventArgs : EventArgs
		{
			public CurrentRecordNumberChangingEventArgs(ZPKCollection pKList)
			{
				this.PKList = pKList;
			}

			public readonly ZPKCollection PKList;
			public ZBool Cancel;
		}

		CurrentRecordNumberChangingEventArgs OnCurrentRecordNumberChanging()
		{
			var args = new CurrentRecordNumberChangingEventArgs(PKList);

			if (CurrentRecordNumberChanging != null)
			{
				CurrentRecordNumberChanging(args);
			}

			return args;
		}

		void OnCurrentRecordNumberChanged()
		{
			if (CurrentRecordNumberChanged != null)
			{
				CurrentRecordNumberChanged(this, null);
			}
		}

		#endregion

		#endregion

		#endregion

		#region PK Functionality

		public ZPKCollection PKList
		{
			get { return fPKList; }
		}

		void SetPKList(ZPKCollection list)
		{
			fPKList = list;
			OnPKListChanged();
		}

		ZPKCollection fPKList;

		public event EventHandler PKListChanged;

		void OnPKListChanged()
		{
			if (PKListChanged != null)
			{
				PKListChanged(this, null);
			}
		}

		void PKList_ListChanged(object sender, EventArgs e)
		{
			OnPKListChanged();
			fCurrentRecordNumberAllowingInvalid = fPKList.IndexOf(fCurrentPK) + 1;
			fRecordNumber = fCurrentRecordNumberAllowingInvalid;
			RefreshBinding();
		}

		public ZGuid CurrentPK
		{
			get { return fCurrentPK; }
			set
			{
				fCurrentPK = value;
				var newIndex = fPKList.IndexOf(fCurrentPK) + 1;
				if (IsValidRecordNumber(newIndex))
				{
					CurrentRecordNumber = newIndex;
				}
			}
		}
		ZGuid fCurrentPK;

		public bool IsCurrentPKInResults
		{
			get { return fPKList.IndexOf(CurrentPK) != -1; }
		}

		#endregion

		#region Move Next/Previous

		public ZBool CanMoveNext
		{
			get { return IsValidRecordNumber(CurrentRecordNumber) && IsValidRecordNumber(CurrentRecordNumber + 1); }
		}

		public void MoveNext()
		{
			if (CanMoveNext)
			{
				CurrentRecordNumberAllowingInvalid++;
			}
			else
			{
				throw new NotSupportedException("Can't move next when on the last PK");
			}
		}

		public ZBool CanMovePrevious
		{
			get { return IsValidRecordNumber(CurrentRecordNumber) && IsValidRecordNumber(CurrentRecordNumber - 1); }
		}

		public void MovePrevious()
		{
			if (CanMovePrevious)
			{
				CurrentRecordNumberAllowingInvalid--;
			}
			else
			{
				throw new NotSupportedException("Can't move before the first PK");
			}
		}

		#endregion
	}
}
