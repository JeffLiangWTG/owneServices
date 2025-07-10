using System.Collections;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public class SysMergeProductXmlDataTransferExporter : SysMergeXmlDataTransferExporter
	{
		public SysMergeProductXmlDataTransferExporter()
			: base(new SysMergeProductValueObjectDataAdapter())
		{
		}

		protected override bool AreSelectedElementsOkToExport(IList selectedElements)
		{
			var result = true;

			if (selectedElements.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("0f49e82f-a8f3-463b-aed4-6064c578ae4b", "Select one or more items to create the XML file for."));
				result = false;
			}

			return result;
		}

		protected override void PromptUserAndExportCore(IList selectedElements)
		{
			if (selectedElements.Count < 1)
			{
				Globals.Message.ShowError(Res.GetString("CDD22266-4A92-45B7-9EBF-1266A593A28D", "There is nothing to export."));
				return;
			}

			IList products;
			if (selectedElements[0] is OrgSupplierPart)
			{
				products = selectedElements;
			}
			else
			{
				factoryProvider = new DataTransferBusinessObjectFactoryProvider();
				factoryProvider.Current.RefreshEnabled = false;
				products = GetProductsRelatedToSelectedOrganisations(selectedElements, factoryProvider);
			}
			base.PromptUserAndExportCore(products);
		}

		protected override IList GetElementsToExport(ZQuery exportQuery)
		{
			factoryProvider = new DataTransferBusinessObjectFactoryProvider();
			factoryProvider.Current.RefreshEnabled = false;
			IList products = GetProductsRelatedToSelectedOrganisations(exportQuery, factoryProvider);
			return products;
		}

		BusinessObjectFactoryProvider factoryProvider;

		protected override ITransactionManager BeginTransactionWithManager()
		{
			return new TransactionManager(this, ((IDbConnected)factoryProvider.Current).Connection.BeginTransactionWithManager());
		}

		BusinessObjectListReader GetProductsRelatedToSelectedOrganisations(IList selectedElements, BusinessObjectFactoryProvider factoryProvider)
		{
			var orgPks = GetSelectedPks(selectedElements);
			var partsToIncludeSubquery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP, false);
			partsToIncludeSubquery.AddToFilter(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.Equal, orgPks);

			var partsQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			partsQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
			partsQuery.AddSubQuery(partsToIncludeSubquery, JoinCondition.And);

			return
				new BusinessObjectListReader(factoryProvider, partsQuery, typeof(OrgSupplierPart))
				{
					BatchSize = 1000,
					SaveBeforeLoadNextEnabled = ShoulSaveFactoriesAfterExport
				};
		}

		BusinessObjectListReader GetProductsRelatedToSelectedOrganisations(ZQuery exportQuery, BusinessObjectFactoryProvider factoryProvider)
		{
			var allOrganozationSubquery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			allOrganozationSubquery.AddToFilter(exportQuery);
			var partsToIncludeSubquery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP, false);
			partsToIncludeSubquery.AddSubQuery(OrgPartRelationSchema.OU_OH, OrgHeaderSchema.PK, allOrganozationSubquery, JoinCondition.And);

			var partsQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			partsQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
			partsQuery.AddSubQuery(partsToIncludeSubquery, JoinCondition.And);

			return
				new BusinessObjectListReader(factoryProvider, partsQuery, typeof(OrgSupplierPart))
				{
					BatchSize = 1000,
					SaveBeforeLoadNextEnabled = ShoulSaveFactoriesAfterExport
				};
		}

		IList<ZGuid> GetSelectedPks(IList selectedElements)
		{
			IList<ZGuid> pks = new List<ZGuid>(selectedElements.Count);

			foreach (BusinessObject bizo in selectedElements)
			{
				pks.Add(bizo.PK);
			}

			return pks;
		}

		class TransactionManager : AggregateTransactionManager<SysMergeProductXmlDataTransferExporter>
		{
			public TransactionManager(SysMergeProductXmlDataTransferExporter owner, ITransactionManager innerTransactionManager) : base(owner, innerTransactionManager)
			{
			}

			protected override void Commit()
			{
				if (owner.ShoulSaveFactoriesAfterExport)
				{
					owner.factoryProvider.Current.Save();
				}
				base.Commit();
				owner.factoryProvider = null;
			}

			protected override void Rollback()
			{
				base.Rollback();
				owner.factoryProvider = null;
			}
		}
	}
}
