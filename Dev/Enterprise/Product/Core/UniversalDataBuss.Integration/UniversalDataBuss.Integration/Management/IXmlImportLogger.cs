using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public delegate string GetDataContextKey();

	public interface IXmlImportLogger : ISimpleLogger
	{
		void LogBoth(LogType type, string message);

		void LogErrorToServiceTaskOnly(string message);

		void FireDataImportedToBusinessObject(BusinessObject targetBO);

		bool IsUpdatingConsol { get; set; }

		bool HasIgnoredModule { get; set; }

		void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey);

		IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

		ITopLevelDataObject TopLevelDataObject { get; }
		IDataContextDataObject TopLevelDataContext { get; }
		bool OrgMatchingDisabled { get; }
	}

	public class DummyLogger : IXmlImportLogger
	{
		public void Log(LogType type, string message)
		{
		}

		public void LogBoth(LogType type, string message)
		{
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
		}

		public bool IsUpdatingConsol { get; set; }

		public bool HasIgnoredModule { get; set; }

		public bool OrgMatchingDisabled => OrgMatchingDisabledCore;

		protected virtual bool OrgMatchingDisabledCore => false;

		public void LogTopLevelDataContextKey(GetDataContextKey dataContextKey)
		{
		}

		public void FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
		}

		public ITopLevelDataObject TopLevelDataObject { get; set; }

		public IDataContextDataObject TopLevelDataContext
		{
			get { return TopLevelDataObject == null ? null : TopLevelDataObject.DataContext; }
		}

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

		#region ISimpleLogResult Members

		public IEnumerable<ISimpleLog> Logs { get { return null; } }

		#endregion
	}
}
