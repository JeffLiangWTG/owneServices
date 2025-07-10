using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.ZQueryHelpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Business
{
	static class VisualizerDocumentDataExtensions
	{
		#region Load/Create VisualizerDocumentData

		public static VisualizerDocumentData LoadOrCreateDocumentData(this BusinessObject parent, string name)
		{
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNullOrEmpty(name, nameof(name));

			var documentData = Load(parent, name) ?? Create(parent, name);

			return documentData;
		}

		static VisualizerDocumentData Load(BusinessObject parent, string name)
		{
			var query = JobDocumentDataZQueryFilters.GetIndexedQuery(parent.PK, parent.TablePrefix, name);

			var documentData = parent.Factory.LoadTop1<VisualizerDocumentData>(query);

			if (documentData != null)
			{
				documentData.Parent = parent;
			}

			return documentData;
		}

		static VisualizerDocumentData Create(BusinessObject parent, string name)
		{
			var documentData = parent.Factory.New<VisualizerDocumentData>();

			using (documentData.SuspendSettingHasChanges())
			{
				documentData.Parent = parent;
				documentData.JDD_Name = name;
			}

			return documentData;
		}

		public static IEnumerable<VisualizerDocumentData> LoadDocumentData(this BusinessObject parent)
		{
			if (parent == null)
			{
				return Enumerable.Empty<VisualizerDocumentData>();
			}

			var query = new ZQuery(JobDocumentDataSchema.JDD_ParentID, parent.PK);
			query.AddToFilter(JobDocumentDataSchema.JDD_ParentTableCode, parent.TablePrefix);
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;

			return parent.Factory.Load<VisualizerDocumentData>(query);
		}

		public static VisualizerDocumentData LoadDocumentData(this BusinessObject parent, string dataStoreName)
		{
			if (parent == null || string.IsNullOrWhiteSpace(dataStoreName))
			{
				return null;
			}

			var query = new ZQuery(JobDocumentDataSchema.JDD_ParentID, parent.PK);
			query.AddToFilter(JobDocumentDataSchema.JDD_ParentTableCode, parent.TablePrefix);
			query.AddToFilter(JobDocumentDataSchema.JDD_Name, dataStoreName);
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;

			return parent.Factory.LoadTop1<VisualizerDocumentData>(query);
		}

		#endregion

		#region Logs

		public static IEnumerable<IDialog> GetDialogs(this IVisualizerDocumentData documentData, string documentName, bool orderByLocalTime)
		{
			var logParent = documentData as IStmALogParent;

			if (logParent == null)
			{
				return Enumerable.Empty<IDialog>();
			}

			return logParent.GetDialogs(documentName, orderByLocalTime);
		}

		#endregion

		#region GetSystemLastEditUserName

		public static string GetSystemLastEditUserName(this IVisualizerDocumentData documentData)
		{
			Argument.NotNull(documentData, nameof(documentData));

			var factory = (documentData as BusinessObject)?.Factory;
			var audit = documentData as IAuditDetails;

			var user = factory != null && audit != null
				? factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, audit.SystemLastEditUser)
				: null;

			return user != null
				? user.GS_FullName
				: ZString.Empty;
		}

		#endregion
	}
}