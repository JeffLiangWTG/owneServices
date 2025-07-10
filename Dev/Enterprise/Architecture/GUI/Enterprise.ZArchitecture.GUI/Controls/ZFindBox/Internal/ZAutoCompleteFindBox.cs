using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public abstract class ZAutoCompleteFindBox : ZFindBoxUserControl, IFindBoxWithMultipleSelect
	{
		protected ZAutoCompleteFindBox()
		{
		}

		#region Auto Complete

		protected override bool AutoCompleteText(bool explicitAutoComplete)
		{
			var previousCode = CodeBox.Text;
			try
			{
				var result = AutoCompleteTextCore(CodeBox.Text, explicitAutoComplete, CodeBox.SelectionStart);
				//if user entered a literal =, don't change selection and return false. else, return true
				if (result != previousCode.Insert(CodeBox.SelectionStart, "="))
				{
					CodeBox.Text = result;
					if (CodeBox.Text.StartsWith(previousCode, StringComparison.Ordinal))
					{
						CodeBox.SelectionStart = Math.Min(CodeBox.Text.Length, previousCode.Length);
						CodeBox.SelectionLength = Math.Max(0, CodeBox.Text.Length - previousCode.Length);
					}
					else
					{
						CodeBox.SelectAll();
					}
					return true;
				}
			}
			catch (ArgumentOutOfRangeException ex)
			{
				var errorMessage = ex.Message;
				errorMessage += $@"
CodeBox's Text Length: {CodeBox.TextLength}
CodeBox's Selection Start: {CodeBox.SelectionStart}
Control: {CodeBox.GetDataMember()}";
				ErrorReporter.ReportOnce(errorMessage, ex);
				throw;
			}

			return false;
		}

		protected virtual string AutoCompleteTextCore(string text, bool explicitAutoComplete, int cursor)
		{
			return IFindBox.ListProvider.NearestMatch(text, explicitAutoComplete, cursor).Item1;
		}

		protected override bool AutoCompleteOnCommit
		{
			get { return ListProvider.AutoCompleteOnCommit; }
		}

		#endregion

		#region Implementation

		protected virtual IFindBox IFindBox
		{
			get { return this; }
		}
#if DEBUG
		internal
#endif
		protected virtual string Code
		{
			get { return CodeBox.Text; }
			set { CodeBox.Text = value; }
		}

		protected virtual string CodeForFinding => IFindBox.Code;

		protected virtual string Description
		{
			get { return ""; }
			set { }
		}

		protected virtual IFindBoxListProvider ListProvider
		{
			get { throw new OdysseyException("Please Override ListProvider in your custom FindBox."); }
		}

#if DEBUG
		internal
#endif
		protected abstract IFindBoxPopup PopupForm { get; }

		#region IFindBox Members

		string IFindBox.Code
		{
			get { return Code; }
			set { Code = value; }
		}

		string IFindBox.Description
		{
			get { return Description; }
			set { Description = value; }
		}

		IFindBoxListProvider IFindBox.ListProvider
		{
			get { return this.ListProvider; }
		}

		IFindBoxPopup IFindBox.PopupForm
		{
			get { return PopupForm; }
		}

		#endregion

		#region  IFindBoxWithMultipleSelect Members
		bool IFindBoxWithMultipleSelect.AllowModuleMultiSelect => AllowModuleMultiSelect;
		#endregion

		#endregion
	}
}
