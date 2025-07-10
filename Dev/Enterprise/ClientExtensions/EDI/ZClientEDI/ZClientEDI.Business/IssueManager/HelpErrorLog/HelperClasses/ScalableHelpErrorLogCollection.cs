using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public interface IHelpErrorLogCollection
	{
		EdiHelpErrorLog Find(ZString key);
		EdiHelpErrorLog AddNew(ZString key);
		void Save();
	}

	/// <summary>
	/// Implementation of IHelpErrorLogCollection that only loads records as needed from the database.
	/// Used as an alternative to HelpErrorLogCollection which loads the entire set of HelpErrorLog records
	/// and doesn't scale well with large numbers of records.
	/// </summary>
	public class ScalableHelpErrorLogCollection : IHelpErrorLogCollection
	{
		public ScalableHelpErrorLogCollection(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public EdiHelpErrorLog Find(ZString key)
		{
			EdiHelpErrorLog log;
			if (!previousLogs.TryGetValue(key, out log))
			{
				ZInt hash = HelpErrorLogKey.GetKeyHashCode(key);
				var query = new ZDBOnlyQuery(typeof(EdiHelpErrorLog));
				var keyQuery = new ZDBOnlySubQuery(typeof(HelpErrorLogKey), HelpErrorLogKeySchema.HK_HE);
				keyQuery.AddToFilter(HelpErrorLogKeySchema.HK_HashCode, hash);
				keyQuery.AddToFilter(HelpErrorLogKeySchema.HK_Key, key);
				query.AddSubQuery(keyQuery, JoinCondition.And);
				log = factory.LoadTop1<EdiHelpErrorLog>(query);
				if (log != null)
				{
					previousLogs.Add(key, log);
				}
			}

			return log;
		}

		public EdiHelpErrorLog AddNew(ZString key)
		{
			EdiHelpErrorLog log = factory.New<EdiHelpErrorLog>();
			previousLogs.Add(key, log);
			return log;
		}

		public void Save()
		{
			factory.Save();
		}

		readonly BusinessObjectFactory factory;
		readonly Dictionary<string, EdiHelpErrorLog> previousLogs = new Dictionary<string, EdiHelpErrorLog>();
	}
}

