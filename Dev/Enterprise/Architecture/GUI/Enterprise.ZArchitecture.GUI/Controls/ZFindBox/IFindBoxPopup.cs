using System;
using System.Windows.Forms;

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IFindBoxWithMultipleSelect : IFindBox
	{
		bool AllowModuleMultiSelect { get; }
	}

	public interface IFindBox
	{
		string Code { get; set; }
		string Description { get; set; }
		IFindBoxListProvider ListProvider { get; }
		IFindBoxPopup PopupForm { get; }
	}

	public enum SilentSelectResult
	{
		None,
		FoundOne,
		FoundNothing,
		MultipleResults,
	}

	public interface IFindBoxPopup : IDisposable
	{
		void ShowModal(IFindBox findBox, Form parentForm);
		SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup);
		void SelectRowByPK(ZGuid pK);
		event EventHandler Closed;
	}

	public interface ICustomizableFindBoxPopup
	{
		string CodeForPopup { get; }
		string PropertyNameForPopup { get; }
	}

	public interface IFindBoxPopupWithAdvancedCodeStore
	{
		string CodeFromPopup { get; set; }
	}

	public interface IFindBoxPopupWithBindToForDescription
	{
		string BindToForDescription { get; set; }
	}

	public static class IFindBoxExtensions
	{
		public static void SetCodeDescription(this IFindBox findBox, BusinessObject bizo)
		{
			if (findBox != null && bizo != null)
			{
				if (bizo is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord && templateRecordProvider.TemplateRecord is BusinessObject templateBizo)
				{
					bizo = templateBizo;
				}

				var codeDescription = findBox.ListProvider?.GetCustomCodeDescription(bizo) ?? bizo;
				findBox.Code = codeDescription.Code;

				if (findBox is not IFindBoxPopupWithBindToForDescription findBoxPopupWithBindToForDescription || string.IsNullOrEmpty(findBoxPopupWithBindToForDescription.BindToForDescription))
				{
					findBox.Description = codeDescription.Description;
				}

				if (findBox is IFindBoxPopupWithAdvancedCodeStore findBoxWithAdvancedCodeStore)
				{
					findBoxWithAdvancedCodeStore.CodeFromPopup = ((ICodeDescription)bizo).Code;
				}
			}
		}
	}
}
