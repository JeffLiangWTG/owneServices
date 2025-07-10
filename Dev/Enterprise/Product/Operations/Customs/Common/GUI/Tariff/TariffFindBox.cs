using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Common.GUI
{
	public class TariffFindBox : ZCodeFindBox
	{
		readonly System.ComponentModel.Container components;

		public TariffFindBox()
		{
		}

		protected virtual IFindBoxPopup GetNewNonBorderWisePopupForm()
		{
			return base.GetNewPopupForm();
		}

		#region GetNewPopupForm

		protected sealed override IFindBoxPopup GetNewPopupForm()
		{
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper((BusinessObject)CurrentItem, DataPropertyName, GetNewNonBorderWisePopupForm);
		}

		#endregion

		#region IDataBoundControl

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (CurrentItem != null)
			{
				AdditionalDataForBorderWise.DataSourceHasIHaveAdditionalDataForBorderWiseImplemented((BusinessObject)CurrentItem);
			}
		}

		#endregion

		#region List

		[SuppressWeaklyTypedCollectionMessage]
		public override IList List
		{
			set
			{
				base.List = value;
				if (List != null && !listHasBeenChecked)
				{
					listHasBeenChecked = true;

					if (CurrentItem is IHaveAdditionalDataForBorderWise)
					{
						Type expectedBusinessObjectTypeList = ((IHaveAdditionalDataForBorderWise)CurrentItem).ExpectedBusinessObjectTypeForList;

						if (!expectedBusinessObjectTypeList.IsAssignableFrom(((IBusinessObjectCollection)List).TypeOfElements))
						{
							ZString errorMessage = GetFullName(this) + (NoResString)": " + DeveloperErrorBindToListNotSetProperly + expectedBusinessObjectTypeList.ToString() + (NoResString)" - Collection found was " + (List == null ? (NoResString)"null" : List.GetType().ToString());
							ErrorReporter.ReportOnce(errorMessage, errorMessage);
						}
					}
				}
			}
		}

		bool listHasBeenChecked;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer error message")]
		public const string DeveloperErrorBindToListNotSetProperly = "This Control's BindToList needs to be bound to a collection containing ";

		string GetFullName(System.Windows.Forms.Control control)
		{
			string result = "";
			if (control.Parent != null)
			{
				result = GetFullName(control.Parent) + ".";
			}

			result += control.Name;
			return result;
		}
		#endregion

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		internal IFindBoxPopup GetNewPopupFormForTest() => GetNewPopupForm();
	}
}
