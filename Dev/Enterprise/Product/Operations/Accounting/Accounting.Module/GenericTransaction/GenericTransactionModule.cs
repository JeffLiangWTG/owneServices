using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.GenericTransaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class GenericTransactionModule : ZFilterGridModule
	{
		public GenericTransactionModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GenericTransaction; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result = null;

			if (selectedBusinessObject != null)
			{
				if (selectedBusinessObject is TransactionHeader)
				{
					if (selectedBusinessObject is GLJournal)
					{
						result = new GLJournalController();
					}
					else if (selectedBusinessObject is JCJournalHeader)
					{
						result = new JCJournalController();
					}
					else
					{
						result = AccountingControllerCreator.GetNewController((TransactionHeader)selectedBusinessObject);
					}
				}
				else if (selectedBusinessObject is BaseWIPAccrual)
				{
					if (((BaseWIPAccrual)selectedBusinessObject).AL_LineType == TransactionLineTypes.WIP)
					{
						result = new WIPController();
					}
					else
					{
						result = new AccrualController();
					}
				}
			}

			return result;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GenericTransactionFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GenericTransactionCollection(Factory);
		}

		protected override FilteredGridLoader CreateSearchManager()
			=> new GenericTransactionModuleGridLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

		class GenericTransactionModuleGridLoader : FilteredGridLoader
		{
			public GenericTransactionModuleGridLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
				: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
			{
			}

			public override BusinessObjectReader CreateReader(ZQuery exportQuery)
			{
				var collection = new GenericTransactionCollection(GetNewFactory(), false);
				collection.Load(exportQuery);

				return new CollectionWrapperBusinessObjectReader(collection);
			}
		}

		protected override BusinessObject GetFirstBizOInList()
		{
			var collection = new GenericTransactionCollection(Factory, false);
			collection.LoadTop1(ExportQuery);
			return collection.Cast<BusinessObject>().FirstOrDefault();
		}

		protected override bool ShouldLoadTop1WhenGridEmpty
		{
			get
			{
				return false;
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GenericTransactionFilterBusinessObject();
		}

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;

			if (selectedBusinessObject != null)
			{
				GenericTransaction gT = selectedBusinessObject as GenericTransaction;
				if (gT != null)
				{
					BusinessObject bizObj = Factory.Load(gT.VT_IsHeader ? typeof(TransactionHeader) : typeof(BaseWIPAccrual), gT.VT_FK);
					result = base.ShowViewForm(bizObj);
				}
			}

			return result;
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var filterHelper = new GenericTransactionFilterHelper(query, true);
			var bizos = GenericTransactionCollection.LoadUsingFilterHelper(factory, filterHelper);
			return PerformSearchResult.Success(factory, filterHelper.Filter, bizos, true);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TransactionSearch; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}
	}
}
