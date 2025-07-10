using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZRecordAttacher : IFindBox
	{
		public ZRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			this.DestinationCollection = destinationCollection;
			this.findBoxList = findBoxList;
			this.originalFindBoxList = findBoxList;
			this.moduleID = moduleID;
		}

		protected readonly IBusinessObjectCollection DestinationCollection;
		readonly IBusinessObjectCollection originalFindBoxList;
		readonly ModuleIdentifier moduleID;

		#region Show

		public void Show(IZForm formToShowModalTo)
		{
			ShowCore(formToShowModalTo);
		}

		protected virtual void ShowCore(IZForm formToShowModalTo)
		{
			#region Testing Only
#if DEBUG
			BaseShowCoreCallCountForTesting++;
			if (DisableShowingGUIForTesting)
			{
				return;
			}
#endif
			#endregion

			findBoxList = originalFindBoxList;

			SetCollectionFilter();
			try
			{
				var module = ZModuleFactory.Instance.Create(moduleID) as ZFilterModule ?? throw new Exception("Unable to find Module for findbox. Check the ModuleID points to a FilterGridModule and that it is valid for the current country.");

				module.DisallowedPKList = AlreadySelectedPKs;
				module.OverrideModuleDecisionProvider(ModuleDecisionProvider);

				// module will be disposed by the popup when it closes
				var popup = new EmbeddedModulePopup(module);
				popup.Selected += new EmbeddedModulePopup.SelectedEventHandler(Popup_Selected);
				popup.ShowModal(this, (Form)formToShowModalTo);

				lastShownAttachPopup = popup;
			}
			finally
			{
				RevertCollectionFilter();
			}
		}

		EmbeddedModulePopup lastShownAttachPopup;
		IBusinessObjectCollection findBoxList;

		public string NameOfAnElementInDestinationCollection = Res.GetString("ee3d3958-64dc-4cef-8af6-e3c0073949d4", "record");

		#region Filter

		void SetCollectionFilter()
		{
			var activeCollection = findBoxList as IActiveBusinessObjectCollection;
			var legacyCollection = findBoxList as BusinessObjectCollection;

			if (activeCollection != null)
			{
				originalAdditionalQuery = activeCollection.AdditionalFilter;
				var clonedQuery = originalAdditionalQuery.ShallowClone();
				clonedQuery.AddToFilter(RemoveAlreadyAddedElementsFilter);
				activeCollection.AdditionalFilter = clonedQuery;
			}
			else if (legacyCollection != null)
			{
				var legacyCollectionAdditionalFilter = ((ILegacyBusinessObjectCollectionInternals)legacyCollection).AdditionalFilter;
				var clonedQuery = legacyCollectionAdditionalFilter.ShallowClone();
				clonedQuery.AddToFilter(RemoveAlreadyAddedElementsFilter);
				((ILegacyBusinessObjectCollectionInternals)legacyCollection).SetOverriddenAdditionalFilter(clonedQuery);
			}
		}

		void RevertCollectionFilter()
		{
			var activeCollection = findBoxList as IActiveBusinessObjectCollection;
			var legacyCollection = findBoxList as BusinessObjectCollection;

			if (activeCollection != null)
			{
				activeCollection.AdditionalFilter = originalAdditionalQuery;
			}
			else if (legacyCollection != null)
			{
				((ILegacyBusinessObjectCollectionInternals)legacyCollection).SetOverriddenAdditionalFilter(null);
			}
		}

		ZQuery originalAdditionalQuery;

		ZGuid[] AlreadySelectedPKs
		{
			get
			{
				var result = new List<ZGuid>();
				if (DestinationCollection != null)
				{
					foreach (BusinessObject bizObj in DestinationCollection)
					{
						result.Add(GetSelectedPKFromDestinationCollection(bizObj));
					}
				}

				return result.ToArray();
			}
		}

		protected virtual ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
		{
			return bizObj.PK;
		}

		ZQuery RemoveAlreadyAddedElementsFilter
		{
			get
			{
				var query = new ZQuery();
				var typeOfElements = findBoxList.TypeOfElements;
				//LocationCollection has type ICollection and uses specialized loading logic.
				if (typeOfElements.IsSubclassOf(typeof(BusinessObject)))
				{
					var tableName = BusinessObjectFactory.GetTableNameFromType(typeOfElements);
					SchemaGuidColumn pkSchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(tableName);
					query.AddToFilter(JoinCondition.And, pkSchemaColumn, SQLComparisonOperator.NotEqual, AlreadySelectedPKs);
				}
				return query;
			}
		}

		#endregion

		#endregion

		#region Module Decision Provider

		public IModuleDecisionProvider ModuleDecisionProvider
		{
			get
			{
				if (fModuleDecisionProvider == null)
				{
					fModuleDecisionProvider = new ButtonGridModuleDecisionProvider(this);
				}
				return fModuleDecisionProvider;
			}
			set { fModuleDecisionProvider = value; }
		}
		IModuleDecisionProvider fModuleDecisionProvider;

		#endregion

		#region Popup

		void Popup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects == null)
			{
				throw new Exception("Unexpected null in popup!");
			}
			else
			{
				var list = new List<BusinessObject>();
				var notAttachedCount = 0;

				if (DestinationCollection?.Factory != null)
				{
					foreach (var obj in e.SelectedBusinessObjects)
					{
						DestinationCollection.Factory.AddFetchHint(obj.PKSchemaColumn, obj.PK);
					}
				}

				foreach (var bizObj in e.SelectedBusinessObjects)
				{
					if (!Attach(bizObj, list))
					{
						notAttachedCount++;
					}
				}

				if (DestinationCollection != null)
				{
					if (OnAttach != null)
					{
						OnAttach(this, new ModuleButtonGridOnAttachEventArgs(list.ToArray()));
					}

					if (CheckAttaching(list))
					{
						using (GetCellNotificationSuspender())
						{
							AttachItemsCore(DestinationCollection, list);
							OnAttached();
						}
					}
				}

				if (notAttachedCount > 0)
				{
					var message = Res.GetString("034773b3-5302-406c-b79c-c59ea1829efb", "Unable to attach {0} of {1} {2}(s) {3}", notAttachedCount, e.SelectedBusinessObjects.Length, NameOfAnElementInDestinationCollection, UnableToAttachText);
					var caption = Res.GetString("b112b07e-10b7-4dbf-89c9-75b8b57c717a", "Unable to Attach {0}...", NameOfAnElementInDestinationCollection);
					Globals.Message.ShowWarning(message, caption);
				}
			}
		}

		protected virtual void AttachItemsCore(IBusinessObjectCollection destinationCollection, IEnumerable<BusinessObject> list)
		{
			destinationCollection.AddRange(list);
		}

		IDisposable GetCellNotificationSuspender()
		{
			if (lastShownAttachPopup != null)
			{
				var form = lastShownAttachPopup.Owner;
				if (form != null)
				{
					return new CellNotificationSuspender(form);
				}
			}
			return new DisposableObject();
		}

		protected virtual bool CheckAttaching(List<BusinessObject> businessObjectsToAttach)
		{
			return businessObjectsToAttach.Count > 0;
		}

		internal event EventHandler<ModuleButtonGridOnAttachEventArgs> OnAttach;

		protected virtual void OnAttached()
		{
		}

		#endregion

		#region Attach

		public string UnableToAttachText = Res.GetString("fc0bb777-8a81-4875-af10-d3542614c35a", "as they are no longer in the database.");

		bool Attach(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return AttachCore(bizO, listToBulkAdd);
		}

		protected virtual bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var pk = bizO.PK;
			var result = DestinationCollection.FindByPK(pk) != null;
			if (!result)
			{
				var destinationType = DestinationCollection.GetTypeOfElementsFromPK(pk);
				//LocationCollection has type ICollection and uses specialized loading logic.
				if (!destinationType.IsSubclassOf(typeof(BusinessObject)))
				{
					destinationType = bizO.GetType();
				}
				var loaded = DestinationCollection.Factory.Load(destinationType, pk);
				if (loaded != null && !loaded.IsDeleted)
				{
					listToBulkAdd.Add(loaded);
					result = true;
				}
			}
			return result;
		}

		#endregion

		#region Testing Only
#if DEBUG

		public int BaseShowCoreCallCountForTesting;
		public bool DisableShowingGUIForTesting;

		public EmbeddedModulePopup LastShownAttachPopupForTesting
		{
			get { return lastShownAttachPopup; }
		}

		public int DestinationCollectionCount
		{
			get { return DestinationCollection.Count; }
		}

#endif
		#endregion

		#region IFindBox Members

		string IFindBox.Code
		{
			get { return ""; }
			set { }
		}

		string IFindBox.Description
		{
			get { return fFindBoxDescription; }
			set { fFindBoxDescription = value; }
		}
		string fFindBoxDescription = "";

		IFindBoxListProvider IFindBox.ListProvider
		{
			get
			{
				var listProvider = findBoxList as IFindBoxListProvider;
				if (findBoxList != null && listProvider == null)
				{
					ErrorReporter.ReportOnce("ZRecordAttacher.ListProviderIsNullWhileFindBoxListIsNotNull", findBoxList.GetType().FullName);
				}
				return listProvider;
			}
		}

		IFindBoxPopup IFindBox.PopupForm
		{
			get { return lastShownAttachPopup; }
		}

		#endregion
	}
}
