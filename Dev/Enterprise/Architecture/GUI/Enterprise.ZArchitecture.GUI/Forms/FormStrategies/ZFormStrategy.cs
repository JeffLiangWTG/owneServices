using System;
using System.Collections.Generic;
using System.Windows.Forms;

using CargoWise.Common;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	#region Overriding Interfaces

	public interface IDoDisplayModeBrowseOverride
	{
		void DoDisplayModeBrowse();
	}

	public interface IDoDisplayModeNewOverride
	{
		void DoDisplayModeNew();
	}

	public interface IDoDisplayModeNewSavedOverride
	{
		void DoDisplayModeNewSaved();
	}

	public interface IDoDisplayModeEditOverride
	{
		void DoDisplayModeEdit();
	}

	public interface IDoDisplayModeDeleteOverride
	{
		void DoDisplayModeDelete();
	}

	public interface IDoDisplayModeReadOnlyOverride
	{
		void DoDisplayModeReadOnly();
	}

	#endregion

	public static class ZFormStrategy
	{
		#region Constructors

		static ZFormStrategy()
		{
		}

		#endregion

		#region Created Forms Strategy

		static List<Type> FormsThatCanBeCreatedDuringDbTransaction
		{
			get
			{
				if (formsThatCanBeCreatedDuringDbTransaction == null)
				{
					formsThatCanBeCreatedDuringDbTransaction = new List<Type>();
					formsThatCanBeCreatedDuringDbTransaction.Add(typeof(ExceptionReportingForm));
				}
				return formsThatCanBeCreatedDuringDbTransaction;
			}
		}
		[ThreadStatic]
		static List<Type> formsThatCanBeCreatedDuringDbTransaction;

		public static void AddFormTypeThatCanBeCreatedDuringDbTransaction(Type formType)
		{
			if (!FormsThatCanBeCreatedDuringDbTransaction.Contains(formType))
			{
				FormsThatCanBeCreatedDuringDbTransaction.Add(formType);
			}
		}

		public static bool CanFormBeCreatedDuringDbTransaction(Type type)
		{
			return IsNewFormInTransactionWarningSuppressed || FormsThatCanBeCreatedDuringDbTransaction.Contains(type);
		}

		public static IDisposable SuppresseNewFormInTransactionWarning()
		{
			lock (isNewFormInTransactionWarningSuppressedLock)
			{ newFormInTransactionWarningSuppressCounter++; }
			return new DisposableAction(() =>
			{
				lock (isNewFormInTransactionWarningSuppressedLock)
				{
					if (IsNewFormInTransactionWarningSuppressed)
					{
						newFormInTransactionWarningSuppressCounter--;
					}
				}
			});
		}

		static int newFormInTransactionWarningSuppressCounter;
		static bool IsNewFormInTransactionWarningSuppressed => newFormInTransactionWarningSuppressCounter > 0;
		static readonly object isNewFormInTransactionWarningSuppressedLock = new object();

		#endregion

		public static void AddAdornments(ZForm form)
		{
			EnterpriseFormLookStrategy.AddAdornments(form);
			ZFormStatusBarStrategy.AddAdornments(form);
			ZFormMenuStrategy.AddAdornments(form);
			ZFormMenuStrategy.AddValidateMenuItem(form);
		}

		#region Do Display Mode

		public static void DoDisplayModeNew(Form form)
		{
			var displayModeNewOverride = form as IDoDisplayModeNewOverride;
			if (displayModeNewOverride != null && !isDoDisplayModeNewOverrideCalled)
			{
				isDoDisplayModeNewOverrideCalled = true;
				displayModeNewOverride.DoDisplayModeNew();
				isDoDisplayModeNewOverrideCalled = false;
			}
			else
			{
				ZFormPostingButtonsStrategy.DoDisplayModeNew(form);
				ZFormMenuStrategy.DoDisplayModeNew(form);
			}
		}
		[ThreadStatic]
		static bool isDoDisplayModeNewOverrideCalled;

		public static void DoDisplayModeNewSaved(Form form)
		{
			var displayModeNewSavedOverride = form as IDoDisplayModeNewSavedOverride;
			if (displayModeNewSavedOverride != null && !isDoDisplayModeNewSavedOverrideCalled)
			{
				isDoDisplayModeNewSavedOverrideCalled = true;
				displayModeNewSavedOverride.DoDisplayModeNewSaved();
				isDoDisplayModeNewSavedOverrideCalled = false;
			}
			else
			{
				ZFormPostingButtonsStrategy.DoDisplayModeNewSaved(form);
				ZFormMenuStrategy.DoDisplayModeNewSaved(form);
			}
		}
		[ThreadStatic]
		static bool isDoDisplayModeNewSavedOverrideCalled;

		public static void DoDisplayModeBrowse(Form form)
		{
			var displayModeBrowseOverride = form as IDoDisplayModeBrowseOverride;
			if (displayModeBrowseOverride != null && !isDoDisplayModeBrowseOverrideCalled)
			{
				isDoDisplayModeBrowseOverrideCalled = true;
				displayModeBrowseOverride.DoDisplayModeBrowse();
				isDoDisplayModeBrowseOverrideCalled = false;
			}
			else
			{
				ZFormPostingButtonsStrategy.DoDisplayModeBrowse(form);
				ZFormMenuStrategy.DoDisplayModeBrowse(form);
			}
		}
		[ThreadStatic]
		static bool isDoDisplayModeBrowseOverrideCalled;

		public static void DoDisplayModeEdit(Form form)
		{
			var displayModeEditOverride = form as IDoDisplayModeEditOverride;
			if (displayModeEditOverride != null && !isDoDisplayModeEditOverrideCalled)
			{
				isDoDisplayModeEditOverrideCalled = true;
				displayModeEditOverride.DoDisplayModeEdit();
				isDoDisplayModeEditOverrideCalled = false;
			}
			else
			{
				ZFormPostingButtonsStrategy.DoDisplayModeEdit(form);
				ZFormMenuStrategy.DoDisplayModeEdit(form);
			}
		}
		[ThreadStatic]
		static bool isDoDisplayModeEditOverrideCalled;

		public static void DoDisplayModeDelete(Form form)
		{
			var displayModeDeleteOverride = form as IDoDisplayModeDeleteOverride;
			if (displayModeDeleteOverride != null && !isDoDisplayModeDeleteOverrideCalled)
			{
				isDoDisplayModeDeleteOverrideCalled = true;
				displayModeDeleteOverride.DoDisplayModeDelete();
				isDoDisplayModeDeleteOverrideCalled = false;
			}
			else
			{
				ZFormPostingButtonsStrategy.DoDisplayModeDelete(form);
				ZFormMenuStrategy.DoDisplayModeDelete(form);
			}
		}
		[ThreadStatic]
		static bool isDoDisplayModeDeleteOverrideCalled;

		public static void DoDisplayModeReadOnly(Form form)
		{
			var displayModeReadOnlyOverride = form as IDoDisplayModeReadOnlyOverride;
			if (displayModeReadOnlyOverride != null && !isDoDisplayModeReadOnlyOverrideCalled)
			{
				isDoDisplayModeReadOnlyOverrideCalled = true;
				displayModeReadOnlyOverride.DoDisplayModeReadOnly();
				isDoDisplayModeReadOnlyOverrideCalled = false;
			}
			else
			{
				ZFormPostingButtonsStrategy.DoDisplayModeReadOnly(form);
				ZFormMenuStrategy.DoDisplayModeReadOnly(form);
			}
		}
		[ThreadStatic]
		static bool isDoDisplayModeReadOnlyOverrideCalled;

		#endregion
	}

	public class ZFormStrategyMessageHelper : INeedToShowMessage
	{
		public IDisposable SuppressNewFormInTransactionWarning()
		{
			return ZFormStrategy.SuppresseNewFormInTransactionWarning();
		}

		public bool CanFormBeCreatedDuringDbTransaction(Type type)
		{
			return ZFormStrategy.CanFormBeCreatedDuringDbTransaction(type);
		}
	}
}
