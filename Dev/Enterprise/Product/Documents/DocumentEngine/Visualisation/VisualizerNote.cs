using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Visualisation
{
	public sealed class VisualizerNote : AutoStmDocDataOverride
	{
		public VisualizerNote(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
		}

		VisualiserDataSet dataSource;
		public VisualiserDataSet DataSource
		{
			get
			{
				if (dataSource == null)
				{
					dataSource = new VisualiserDataSet();

					if (DD_DocumentData.Length > 0)
					{
						var xml = DD_DocumentData.ToUTF8();
						dataSource.DeSerialise(xml);
					}
				}

				return dataSource;
			}
		}

		public void ResetDataSource()
		{
			dataSource = null;
		}

		public override ZBlob DD_DocumentData
		{
			get { return base.DD_DocumentData; }
			set
			{
				ResetDataSource();
				base.DD_DocumentData = value;
			}
		}

		protected override void OnFactorySaving()
		{
			if (!IsInDatabase && DD_DocumentData.IsEmpty)
			{
				this.Delete();
			}
		}

		#region UniqueIndexFailureHandler

		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return fUniqueIndexFailureHandler ?? (fUniqueIndexFailureHandler = new VisualizerNoteNumberFountainUniqueIndexFailureHandler(this)); }
		}

		class VisualizerNoteNumberFountainUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			readonly VisualizerNote visualizerNoteRecord;

			public VisualizerNoteNumberFountainUniqueIndexFailureHandler(VisualizerNote visualizerNoteRecord)
			{
				this.visualizerNoteRecord = visualizerNoteRecord;
			}

			#region IUniqueIndexFailureHandler Members

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var query = new ZDBOnlyQuery(typeof(VisualizerNote));
				query.AddToFilter(StmDocDataOverrideSchema.DD_ParentID, visualizerNoteRecord.DD_ParentID);
				query.AddToFilter(StmDocDataOverrideSchema.DD_SU, visualizerNoteRecord.DD_SU);
				query.AddToFilter(StmDocDataOverrideSchema.DD_ParentRelatedID, visualizerNoteRecord.DD_ParentRelatedID.IsValid ? visualizerNoteRecord.DD_ParentRelatedID : null);
				query.AddToFilter(StmDocDataOverrideSchema.PK, SQLComparisonOperator.NotEqual, visualizerNoteRecord.PK);
				var noteInDatabase = visualizerNoteRecord.Factory.LoadTop1<VisualizerNote>(query);
				if (noteInDatabase != null && noteInDatabase.PK != visualizerNoteRecord.PK)
				{
					if (visualizerNoteRecord.HasChanges)
					{
						notifier.ReportInformation(Res.GetString("7c0e0437-83af-49e5-b10f-c60f5d9aa44f",
						"While you were working, the Doc Data for this record was modified. The system will now need to merge this information. Press OK to have this information loaded and then try saving again."),
						Res.GetString("e2de1860-970d-47f6-b237-fd31492773f2", "Doc Data Reload Required"));

						noteInDatabase.DD_DocumentData = visualizerNoteRecord.DD_DocumentData;
						noteInDatabase.DD_ParentTableCode = visualizerNoteRecord.DD_ParentTableCode;
					}
					visualizerNoteRecord.Delete();
				}
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return StmDocDataOverrideSchema.Constants.Indexes.NR_UX__DD_ParentID_DD_SU_DD_ParentRelatedID;
				}
			}

			#endregion
		}

		#endregion

		internal static VisualizerNote Get(BusinessObjectFactory factory, IStmMenuItem parentMenuItem, IStmMenuItem curMenuItem, IVisualizerNoteSupporter supporter)
		{
			ReportErrorForInvalidMenuItemAndSupporterConfiguration(parentMenuItem, curMenuItem, supporter);

			if (parentMenuItem == null
				|| (parentMenuItem != null && !parentMenuItem.SU_SupportsVisualisation)
				|| supporter == null
				|| (supporter != null && (supporter.PK.IsEmpty || string.IsNullOrEmpty(supporter.TableCode))))
			{
				return null;
			}

			var result = Load(factory, parentMenuItem, supporter);

			if (result == null || result.IsDeleted)
			{
				result = LoadFromMenuItemForVisualisationData(factory, parentMenuItem, supporter);
			}

			if (result == null || result.IsDeleted)
			{
				result = New(factory, parentMenuItem, supporter);
			}
			return result;
		}

		static void ReportErrorForInvalidMenuItemAndSupporterConfiguration(IStmMenuItem parentMenuItem, IStmMenuItem curMenuItem, IVisualizerNoteSupporter supporter)
		{
			if (parentMenuItem != null
				&& parentMenuItem.SU_IsSystemDefined
				&& parentMenuItem.SU_SupportsVisualisation
				&& curMenuItem != null
				&& curMenuItem.SU_IsSystemDefined
				&& curMenuItem.SU_SupportsVisualisation
				&& supporter != null
				&& (supporter.PK.IsEmpty || string.IsNullOrEmpty(supporter.TableCode)))
			{
				var visualizerNoteSupporter = supporter as VisualizerNoteSupporter;
				var bizoTypeFullName = visualizerNoteSupporter != null ? visualizerNoteSupporter.BusinessObjectToLogAgainst.GetType().FullName : supporter.GetType().FullName;
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, @"Bizo '{0}' doesn't provide a persistent PK/Table Code to store StmDocDataOverride", bizoTypeFullName), string.Format(CultureInfo.InvariantCulture, @"The document '{0}' supports modifications (SU_SupportsVisualisation = 1) but it's using '{1}' to store them and that business object does not provide a persistent PK and Table Code.
If your business object in non-persistent, make sure it implements IVisualizerNoteSupporter properly.",
					parentMenuItem.DocumentId, bizoTypeFullName));
			}
		}

		static VisualizerNote LoadFromMenuItemForVisualisationData(BusinessObjectFactory factory, IStmMenuItem menuItem, IVisualizerNoteSupporter supporter)
		{
			var documentSupportable = supporter as IDocumentSupportable;
			if (documentSupportable == null)
			{
				var visualizerNoteSupporter = supporter as VisualizerNoteSupporter;
				if (visualizerNoteSupporter != null)
				{
					documentSupportable = visualizerNoteSupporter.GetDocumentSupportable();
				}
			}
			var documentSupporter = documentSupportable != null ? documentSupportable.DocumentSupporter : null;
			var menuItemForVisualisationData = documentSupporter != null ? documentSupportable.DocumentSupporter.GetMenuItemForVisualisationData(menuItem) : null;

			if (menuItemForVisualisationData != null)
			{
				return Load(factory, menuItemForVisualisationData, supporter);
			}

			return null;
		}

		static VisualizerNote Load(BusinessObjectFactory factory, IStmMenuItem menuItem, IVisualizerNoteSupporter supporter)
		{
			var query = new ZQuery(StmDocDataOverrideSchema.DD_ParentID, supporter.PK);
			if (supporter.ChildBusinessObjectPK.IsEmpty)
			{
				query.AddToFilter(StmDocDataOverrideSchema.DD_ParentRelatedID, null);
			}
			else
			{
				query.AddToFilter(StmDocDataOverrideSchema.DD_ParentRelatedID, supporter.ChildBusinessObjectPK);
			}
			query.AddToFilter(StmDocDataOverrideSchema.DD_SU, menuItem.PK);

			return factory.LoadTop1<VisualizerNote>(query);
		}

		static VisualizerNote New(BusinessObjectFactory factory, IStmMenuItem menuItem, IVisualizerNoteSupporter supporter)
		{
			var result = factory.New<VisualizerNote>();
			result.DD_ParentID = supporter.PK;
			result.DD_ParentRelatedID = supporter.ChildBusinessObjectPK;
			result.DD_SU = menuItem.PK;
			result.DD_ParentTableCode = supporter.TableCode;
			return result;
		}

		public class VisualizerNoteSupporter : IVisualizerNoteSupporter
		{
			public VisualizerNoteSupporter(BusinessObject businessObjectToLogAgainst)
			{
				BusinessObjectToLogAgainst = businessObjectToLogAgainst;
			}

			public VisualizerNoteSupporter(BusinessObject businessObjectToLogAgainst, BusinessObject childBusinessObjectToLogAgainst)
				: this(businessObjectToLogAgainst)
			{
				this.childBusinessObjectToLogAgainst = childBusinessObjectToLogAgainst;
			}

			internal readonly BusinessObject BusinessObjectToLogAgainst;
			readonly BusinessObject childBusinessObjectToLogAgainst;

			#region IVisualizerNoteSupporter Members

			ZGuid IVisualizerNoteSupporter.PK => BusinessObjectToLogAgainst.PK;

			ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => childBusinessObjectToLogAgainst == null ? ZGuid.Empty : childBusinessObjectToLogAgainst.PK;

			string IVisualizerNoteSupporter.TableCode => BusinessObjectToLogAgainst.TablePrefix;

			#endregion

			internal IDocumentSupportable GetDocumentSupportable()
			{
				return BusinessObjectToLogAgainst as IDocumentSupportable;
			}
		}
	}
}
