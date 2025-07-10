using System.Linq;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms;

namespace Enterprise.DbUpgrader.Transformations
{
	public class TransformationDirector : ITransformationIndexProvider
	{
		public TransformationDirector(IUpgradeManager manager, DbConnection auditConnection = null, DbConnection edwConnection = null)
		{
			Manager = manager;
			AuditConnection = auditConnection;
			EdwConnection = edwConnection;
		}

		public void OnlinePreUpgradeRun()
		{
			Run(TransformationSection.OnlinePreUpgrade);
		}

		public void OnlineBiPreUpgradeRun()
		{
			RunBiTransformations(TransformationSection.OnlinePreUpgrade);
		}

		public void OfflinePreUpgradeRun()
		{
			if (HasSchemaChangeTransformations)
			{
				CreateDataCopyDb();
			}

			Run(TransformationSection.OfflinePreUpgrade);
			RunBiTransformations(TransformationSection.OfflinePreUpgrade);
		}

		public void OfflinePostUpgradeRun()
		{
			Run(TransformationSection.OfflinePostUpgrade);
			RunBiTransformations(TransformationSection.OfflinePostUpgrade);
		}

		internal ITriggerTransformation[] GetTriggerTransformations() => Transformations.OfType<ITriggerTransformation>().ToArray();

		void Run(TransformationSection transformationSection)
		{
			Manager.ActivateSubtaskProgress(Transformations.Length);

			foreach (var transformation in Transformations)
			{
				if (transformation is not BiDataTransformation)
				{
					using (new SlowTransformReporter(transformation.UserDescription, transformationSection))
					{
						transformation.Run(transformationSection, CancellationToken.None);
					}
				}
			}
		}

		void RunBiTransformations(TransformationSection transformationSection)
		{
			if (AuditConnection != null)
			{
				using (((ICurrentDbControl)AuditConnection).UseDatabase(Db.AuditDatabaseName))
				{
					var auditTransformations = Transformations.OfType<AuditDataTransformation>().ToList();
					foreach (var transformation in auditTransformations)
					{
						transformation.RunTransformWithSlowTransformReporter(transformationSection, AuditConnection, CancellationToken.None);
					}
				}
			}

			if (EdwConnection != null)
			{
				using (((ICurrentDbControl)EdwConnection).UseDatabase(Db.EdwDatabaseName))
				{
					var edwTransformations = Transformations.OfType<EdwDataTransformation>().ToList();
					foreach (var transformation in edwTransformations)
					{
						transformation.RunTransformWithSlowTransformReporter(transformationSection, EdwConnection, CancellationToken.None);
					}
				}
			}
		}

		TransformationIndexProvider _indexProvider;
		public TransformationIndexProvider IndexProvider
		{
			get
			{
				if (_indexProvider == null)
				{
					_indexProvider = new TransformationIndexProvider(new DescriptionOnlyTransformation(nameof(TransformationDirector)));

					foreach (var transform in Transformations.OfType<ITransformationIndexProvider>())
					{
						_indexProvider.Add(transform.IndexProvider);
					}
				}

				return _indexProvider;
			}
		}

		#region Implementation

		readonly IUpgradeManager Manager;
		readonly DbConnection AuditConnection;
		readonly DbConnection EdwConnection;

		DataTransformation[] _Transformations;
		protected
			#if DEBUG
			virtual
			#endif
			DataTransformation[] Transformations
		{
			get
			{
				if (_Transformations == null)
				{
					var mapper = new Mapper(Manager);
					_Transformations = mapper.GetOrderedTransformations();
				}

				return _Transformations;
			}
		}
		protected bool HasSchemaChangeTransformations => new Mapper(Manager).GetOrderedTransformations().OfType<SchemaChangeDataTransformation>().Any();

#region Create And Drop DataCopy Database

		public static readonly string DataCopyDb = TransformationHelper.DataCopyDbName;

		protected void CreateDataCopyDb()
		{
			using (var conn = Db.NewAdminConnection())
			{
				DataCopyDbCreator.CreateDropExisting(conn);
			}
		}

		protected IDbCreator DataCopyDbCreator
		{
			get
			{
				if (fDataCopyDbCreator == null)
				{
					fDataCopyDbCreator = new EmptyDbCreator(DataCopyDb);
				}

				return fDataCopyDbCreator;
			}
		}

		IDbCreator fDataCopyDbCreator;

#endregion // Create And Drop DataCopy Database

#endregion // Implementation
	}
}
